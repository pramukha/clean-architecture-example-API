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
    }
}
