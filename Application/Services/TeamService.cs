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

        public TeamService(ApplicationDbContext context)
        {
            _context = context;
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

            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Players = team.Players.Select(p => new PlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Position = p.Position,
                    PlayerSkills = p.PlayerSkills.Select(s => new PlayerSkillDto
                    {
                        Id = s.Id,
                        Skill = s.Skill,
                        Value = s.Value,
                        PlayerId = s.PlayerId
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            var teams = await _context.Teams
                .Include(t => t.Players)
                .ToListAsync();

            return teams.Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                Players = t.Players.Select(p => new PlayerDto
                {                    
                    Id = p.Id,
                    Name = p.Name,
                    Position = p.Position,
                    PlayerSkills = p.PlayerSkills.Select(s => new PlayerSkillDto
                    {
                        Skill = s.Skill,
                        Value = s.Value
                    }).ToList()
                }).ToList()
            });
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
        {            var availablePlayers = await _context.Players
                .Include(p => p.PlayerSkills)
                .Where(p => p.Position == position && !selectedPlayers.Contains(p))
                .ToListAsync();

            return availablePlayers
                .OrderByDescending(p => p.PlayerSkills.FirstOrDefault(s => s.Skill == requiredSkill)?.Value ?? 0)
                .FirstOrDefault();
        }

        private TeamDto MapToTeamDto(Team team)
        {
            return new TeamDto
            {
                Id = team.Id,
                Players = team.Players.Select(p => new PlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Position = p.Position,
                    PlayerSkills = p.PlayerSkills.Select(s => new PlayerSkillDto
                    {
                        Skill = s.Skill,
                        Value = s.Value
                    }).ToList()
                }).ToList()
            };
        }

    }
}
