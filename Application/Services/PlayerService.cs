using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;

        public PlayerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PlayerDto> CreatePlayerAsync(PlayerDto playerDto)
        {
            if (playerDto == null)
                throw new ArgumentNullException(nameof(playerDto));
            // Automapper
            var player = new Player
            {
                Name = playerDto.Name,
                Position = playerDto.Position.ToLower(), // ensure position is lowercase
                PlayerSkills = playerDto.PlayerSkills.Select(s => new PlayerSkill 
                { 
                    Skill = s.Skill, 
                    Value = s.Value 
                }).ToList() ?? new List<PlayerSkill>()
            };
            // Automapper
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Now that we have the player ID, update the skills with it
            foreach (var skill in player.PlayerSkills)
            {
                skill.PlayerId = player.Id;
            }
            await _context.SaveChangesAsync();

            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Position = player.Position,
                PlayerSkills = player.PlayerSkills.Select(s => new PlayerSkillDto 
                { 
                    Id = s.Id,
                    Skill = s.Skill, 
                    Value = s.Value,
                    PlayerId = s.PlayerId
                }).ToList()
            };
        }        
        public async Task<PlayerDto> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players
                .Include(p => p.PlayerSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return null;

            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Position = player.Position,
                PlayerSkills = player.PlayerSkills.Select(s => new PlayerSkillDto
                {
                    Id = s.Id,
                    Skill = s.Skill,
                    Value = s.Value,
                    PlayerId = s.PlayerId
                }).ToList()
            };
        }        public async Task<IEnumerable<PlayerDto>> GetAllPlayersAsync()
        {
            var players = await _context.Players
                .Include(p => p.PlayerSkills)
                .ToListAsync();

            return players.Select(player => new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Position = player.Position,
                PlayerSkills = player.PlayerSkills.Select(s => new PlayerSkillDto
                {
                    Id = s.Id,
                    Skill = s.Skill,
                    Value = s.Value,
                    PlayerId = s.PlayerId
                }).ToList()
            });
        }

        public async Task<bool> UpdatePlayerAsync(int id, PlayerDto playerDto)
        {
            var player = await _context.Players
                .Include(p => p.PlayerSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return false;

            // Update basic properties
            player.Name = playerDto.Name;
            player.Position = playerDto.Position.ToLower();

            // Remove existing skills
            _context.PlayerSkills.RemoveRange(player.PlayerSkills);

            // Add new skills
            player.PlayerSkills = playerDto.PlayerSkills.Select(s => new PlayerSkill
            {
                Skill = s.Skill,
                Value = s.Value,
                PlayerId = id
            }).ToList();

            await _context.SaveChangesAsync();
            return true;
        }        
        public async Task<bool> DeletePlayerAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return false;

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            return true;
        }        
        public async Task<IEnumerable<PlayerDto>> GetPlayersByPositionAsync(string position)
        {
            return await _context.Players
                .Include(p => p.PlayerSkills)
                .Where(p => p.Position == position.ToLower()) // ensure position matching is case-insensitive
                .Select(p => new PlayerDto
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
                })
                .ToListAsync();
        }
    }
}
