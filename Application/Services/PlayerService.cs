using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PlayerService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<PlayerDto> CreatePlayerAsync(PlayerDto playerDto)
        {
            if (playerDto == null)
                throw new ArgumentNullException(nameof(playerDto));

            var player = _mapper.Map<Player>(playerDto);
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Now that we have the player ID, update the skills with it
            foreach (var skill in player.PlayerSkills)
            {
                skill.PlayerId = player.Id;
            }
            await _context.SaveChangesAsync();

            return _mapper.Map<PlayerDto>(player);
        }
        public async Task<PlayerDto> GetPlayerByIdAsync(int id)
        {
            var player = await _context.Players
                .Include(p => p.PlayerSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return null;

            return _mapper.Map<PlayerDto>(player);
        }
        public async Task<IEnumerable<PlayerDto>> GetAllPlayersAsync()
        {
            var players = await _context.Players
                .Include(p => p.PlayerSkills)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlayerDto>>(players);
        }
        public async Task<bool> UpdatePlayerAsync(int id, PlayerDto playerDto)
        {
            var player = await _context.Players
                .Include(p => p.PlayerSkills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null)
                return false;

            // Remove existing skills
            _context.PlayerSkills.RemoveRange(player.PlayerSkills);

            // Map updated data from DTO to entity
            _mapper.Map(playerDto, player);

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
            var players = await _context.Players
                .Include(p => p.PlayerSkills)
                .Where(p => p.Position == position.ToLower()) // ensure position matching is case-insensitive
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlayerDto>>(players);
        }
    }
}
