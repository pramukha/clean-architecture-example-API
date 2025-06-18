using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayersController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpPost]
        public async Task<ActionResult<PlayerDto>> CreatePlayer([FromBody] PlayerDto playerDto)
        {
            var player = await _playerService.CreatePlayerAsync(playerDto);
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetAllPlayers()
        {
            var players = await _playerService.GetAllPlayersAsync();
            return Ok(players);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDto>> GetPlayer(int id)
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null)
                return NotFound();

            return Ok(player);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayer(int id, PlayerDto playerDto)
        {
            var result = await _playerService.UpdatePlayerAsync(id, playerDto);
            if (!result)
                return NotFound();

            return NoContent();
        }        
        /// <summary>
        /// Deletes a player by ID. Requires authentication token.
        /// </summary>
        /// <param name="id">The ID of the player to delete</param>
        /// <returns>No content if successful, NotFound if player doesn't exist, Unauthorized if not authenticated</returns>
        /// <response code="204">Player was successfully deleted</response>
        /// <response code="401">Not authenticated or invalid token</response>
        /// <response code="404">Player with given ID was not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            // Check for the Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header with Bearer token is required");
            }

            // Extract and validate the token
            var token = authHeader.Replace("Bearer ", "");
            if (token != "SkFabTZibXE1aE14ckpQUUxHc2dnQ2RzdlFRTTM2NFE2cGI4d3RQNjZmdEFITmdBQkE=")
            {
                return Unauthorized("Invalid token");
            }

            var result = await _playerService.DeletePlayerAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("position/{position}")]
        public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayersByPosition(string position)
        {
            var players = await _playerService.GetPlayersByPositionAsync(position);
            return Ok(players);
        }
    }
}
