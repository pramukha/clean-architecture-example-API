using System;
using System.Threading.Tasks;
using Application.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests
{
    public class PlayerServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly PlayerService _playerService;

        public PlayerServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _playerService = new PlayerService(_context);
        }

        [Fact]
        public async Task DeletePlayer_ExistingPlayer_ReturnsTrue()
        {
            // Arrange
            var player = new Player
            {                    Id = 1,
                Name = "Test Player",
                Position = "Forward"
            };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Act
            var result = await _playerService.DeletePlayerAsync(player.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Players.FindAsync(player.Id));
        }

        [Fact]
        public async Task DeletePlayer_NonExistingPlayer_ReturnsFalse()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = await _playerService.DeletePlayerAsync(nonExistingId);

            // Assert
            Assert.False(result);
        }
    }
}
