using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TeamService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TeamDto> ProcessTeamAsync(TeamRequestDto request)
        {
            ValidateRequest(request);

            var selectedPlayers = new List<Player>();
            var errors = new List<string>();

            foreach (var position in request.RequiredPositions)
            {
                foreach (var requiredSkill in position.Value)
                {
                    var player = await SelectBestPlayerForPosition(position.Key, requiredSkill, selectedPlayers);

                    if (player == null)
                    {
                        errors.Add($"No available player for position {position.Key} with skill {requiredSkill}");
                        break;
                    }

                    selectedPlayers.Add(player);
                }

                if (errors.Any())
                    break;
            }

            if (errors.Any())
                throw new ValidationException(errors.First());

            // Create a new team with selected players
            var teamName = $"Team_{DateTime.Now:yyyyMMdd_HHmmss}";
            var team = new Team { Name = teamName };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Add players to the team
            foreach (var player in selectedPlayers)
            {
                team.Players.Add(player);
            }
            await _context.SaveChangesAsync();

            return await GetTeamByIdAsync(team.Id);
        }

        public async Task<TeamDto> GetTeamByIdAsync(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Players)
                    .ThenInclude(p => p.PlayerSkills)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return null;

            return _mapper.Map<TeamDto>(team);
        }

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            var teams = await _context.Teams
                .Include(t => t.Players)
                    .ThenInclude(p => p.PlayerSkills)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return false;

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return true;
        }

        private void ValidateRequest(TeamRequestDto request)
        {
            if (request == null)
                throw new ValidationException("Request cannot be null");

            if (request.RequiredPositions == null || !request.RequiredPositions.Any())
                throw new ValidationException("Required positions must be specified");

            foreach (var position in request.RequiredPositions)
            {
                if (string.IsNullOrWhiteSpace(position.Key))
                    throw new ValidationException("Position name cannot be empty");

                if (position.Value == null || !position.Value.Any())
                    throw new ValidationException($"Skills must be specified for position {position.Key}");
            }
        }

        private async Task<Player> SelectBestPlayerForPosition(string position, string requiredSkill, List<Player> selectedPlayers)
        {
            var availablePlayers = await _context.Players
                .Include(p => p.PlayerSkills)
                .Where(p => p.Position.ToLower() == position.ToLower() && !selectedPlayers.Contains(p))
                .ToListAsync();

            return availablePlayers
                .OrderByDescending(p => p.PlayerSkills.FirstOrDefault(s => s.Skill.ToLower() == requiredSkill.ToLower())?.Value ?? 0)
                .ThenByDescending(p => p.PlayerSkills.Average(s => s.Value)) // Use average skill as tiebreaker
                .FirstOrDefault();
        }

        public async Task<List<PlayerDto>> SelectBestTeamAsync(List<PositionRequirementDto> requirements)
        {
            if (requirements == null || !requirements.Any())
                throw new ValidationException("Position requirements cannot be empty");

            ValidateRequirements(requirements);

            var selectedPlayers = new List<Player>();
            var result = new List<Player>();

            foreach (var requirement in requirements)
            {
                // Check if we have enough players for this position
                var availablePlayers = await _context.Players
                    .Include(p => p.PlayerSkills)
                    .Where(p => p.Position.ToLower() == requirement.Position.ToLower())
                    .ToListAsync();

                if (availablePlayers.Count < requirement.NumberOfPlayers)
                    throw new ValidationException($"Insufficient number of players for position: {requirement.Position}");

                // Filter out already selected players
                availablePlayers = availablePlayers
                    .Where(p => !selectedPlayers.Contains(p))
                    .ToList();

                // Select players for this position and skill
                var playersForThisRequirement = SelectBestPlayersForPositionAndSkill(
                    availablePlayers, 
                    requirement.MainSkill, 
                    requirement.NumberOfPlayers);

                // Add to our selected players list to prevent reusing them
                selectedPlayers.AddRange(playersForThisRequirement);
                result.AddRange(playersForThisRequirement);
            }

            return _mapper.Map<List<PlayerDto>>(result);
        }

        private List<Player> SelectBestPlayersForPositionAndSkill(
            List<Player> availablePlayers, 
            string requiredSkill, 
            int numberOfPlayers)
        {
            // Check if any player has the required skill
            bool hasRequiredSkill = availablePlayers.Any(p => 
                p.PlayerSkills.Any(s => s.Skill.ToLower() == requiredSkill.ToLower()));

            List<Player> selectedPlayers = new List<Player>();

            if (hasRequiredSkill)
            {
                // Sort by the required skill value
                var orderedPlayers = availablePlayers
                    .OrderByDescending(p => p.PlayerSkills
                        .FirstOrDefault(s => s.Skill.ToLower() == requiredSkill.ToLower())?.Value ?? 0)
                    .ToList();

                // Take the required number of players
                for (int i = 0; i < numberOfPlayers && i < orderedPlayers.Count; i++)
                {
                    selectedPlayers.Add(orderedPlayers[i]);
                }
            }
            else
            {
                // No player has the required skill, find players with highest skill value
                var orderedPlayers = availablePlayers
                    .OrderByDescending(p => p.PlayerSkills.Any() ? p.PlayerSkills.Max(s => s.Value) : 0)
                    .ToList();

                // Take the required number of players
                for (int i = 0; i < numberOfPlayers && i < orderedPlayers.Count; i++)
                {
                    selectedPlayers.Add(orderedPlayers[i]);
                }
            }

            return selectedPlayers;
        }

        private void ValidateRequirements(List<PositionRequirementDto> requirements)
        {
            // Check for duplicate position + skill combinations
            var positionSkillCombinations = new HashSet<string>();
            
            foreach (var requirement in requirements)
            {
                if (string.IsNullOrWhiteSpace(requirement.Position))
                    throw new ValidationException("Position cannot be empty");
                    
                if (string.IsNullOrWhiteSpace(requirement.MainSkill))
                    throw new ValidationException($"Main skill must be specified for position {requirement.Position}");
                    
                if (requirement.NumberOfPlayers <= 0)
                    throw new ValidationException($"Number of players must be positive for position {requirement.Position}");

                // Create a unique key for position + skill combination
                string key = $"{requirement.Position.ToLower()}_{requirement.MainSkill.ToLower()}";
                
                // Check if this combination already exists
                if (!positionSkillCombinations.Add(key))
                    throw new ValidationException($"Duplicate position and skill combination: {requirement.Position} with {requirement.MainSkill}");
            }
        }
    }
}
