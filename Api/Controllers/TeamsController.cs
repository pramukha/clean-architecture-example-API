using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/team")]
    [Produces("application/json")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }        /// <summary>
        /// </summary>
        /// Processes a team selection request and returns the best possible players based on required positions and skills.
        /// <param name="requirements">List of position requirements with skills and number of players</param>
        /// <returns>A list of best available players for each position requirement</returns>
        /// <response code="200">Returns the selected players</response>
        /// <response code="400">If the request is invalid or players cannot be found for positions</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(List<PlayerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PlayerDto>>> ProcessTeam([FromBody] List<PositionRequirementDto> requirements)
        {
            if (requirements == null || !requirements.Any()) 
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, "Request body is required"));

            try
            {
                var players = await _teamService.SelectBestTeamAsync(requirements);
                return Ok(players);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred"));
            }
        }







        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetAllTeams()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(teams);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
                return NotFound();

            return Ok(team);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTeam(int id)
        {
            var result = await _teamService.DeleteTeamAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

    }
}
