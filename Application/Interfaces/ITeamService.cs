using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITeamService
    {
        Task<bool> DeleteTeamAsync(int id);
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();
        Task<TeamDto> GetTeamByIdAsync(int id);
        Task<TeamDto> ProcessTeamAsync(TeamRequestDto request);
    }
}
