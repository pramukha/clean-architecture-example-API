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
        /// Deletes a player by ID
        /// </summary>
        /// <param name="id">The ID of the player to delete</param>
        /// <returns>No content if successful, NotFound if player doesn't exist</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
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
