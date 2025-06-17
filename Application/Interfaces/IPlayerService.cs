using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerDto> CreatePlayerAsync(PlayerDto playerDto);
        Task<PlayerDto> GetPlayerByIdAsync(int id);
        Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();
        Task<bool> UpdatePlayerAsync(int id, PlayerDto playerDto);
        Task<bool> DeletePlayerAsync(int id);
        Task<IEnumerable<PlayerDto>> GetPlayersByPositionAsync(string position);
    }
}
