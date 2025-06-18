using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests
{
    public class TeamServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly TeamService _teamService;
        private readonly IMapper _mapper;

        public TeamServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TeamServiceTestDb")
                .Options;

            // Configure AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _context = new ApplicationDbContext(_options);
            _teamService = new TeamService(_context, _mapper);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var players = new List<Player>
            {
                new Player
                {
                    Id = 1,
                    Name = "Test Forward",
                    Position = "forward",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "attack", Value = 90, PlayerId = 1 },
                        new PlayerSkill { Skill = "speed", Value = 85, PlayerId = 1 }
                    }
                },
                new Player
                {
                    Id = 2,
                    Name = "Test Midfielder",
                    Position = "midfielder",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "passing", Value = 88, PlayerId = 2 },
                        new PlayerSkill { Skill = "vision", Value = 92, PlayerId = 2 }
                    }
                }
            };

            _context.Players.AddRange(players);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ProcessTeam_ValidRequest_ReturnsTeam()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "attack" } },
                    { "midfielder", new List<string> { "vision" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Players.Count);
            Assert.Contains(result.Players, p => p.Position == "forward");
            Assert.Contains(result.Players, p => p.Position == "midfielder");
        }

        [Fact]
        public async Task ProcessTeam_NoPlayersForPosition_ThrowsValidationException()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "goalkeeper", new List<string> { "reflexes" } }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _teamService.ProcessTeamAsync(request));
        }

        [Fact]
        public async Task ProcessTeam_EmptyRequest_ThrowsValidationException()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _teamService.ProcessTeamAsync(request));
        }
    }
}