using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
        }

        /// <summary>
       /// </summary>
         /// Processes a team selection request and returns the best possible team based on required positions and skills.
        /// <param name="request">The team request containing required positions and their skills</param>
        /// <returns>A team with the best available players for each position</returns>
        /// <response code="200">Returns the selected team</response>
        /// <response code="400">If the request is invalid or players cannot be found for positions</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TeamDto>> ProcessTeam([FromBody] TeamRequestDto request)
        {
            if (request == null)
                return BadRequest(new ErrorResponse { Error = "Request body is required" });

            try 
            {
                var team = await _teamService.ProcessTeamAsync(request);
                return Ok(team);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse { Error = "An unexpected error occurred" });
            }
        }
    
            
        
    

        

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetAllTeams()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(teams);
        }

        [HttpGet("{id}")]        public async Task<ActionResult<TeamDto>> GetTeam(int id)
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
