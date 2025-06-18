using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class TeamServiceBestPlayerSelectionTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly TeamService _teamService;
        private readonly List<Player> _testPlayers;
        private readonly IMapper _mapper;

        public TeamServiceBestPlayerSelectionTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("PlayerSelectionTestDb")
                .Options;

            // Configure AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _context = new ApplicationDbContext(_options);
            _teamService = new TeamService(_context, _mapper);

            // Create test data with controlled skill values
            _testPlayers = new List<Player>
            {
                new Player
                {
                    Id = 1,
                    Name = "Forward1",
                    Position = "forward",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "speed", Value = 90, PlayerId = 1 },
                        new PlayerSkill { Skill = "attack", Value = 85, PlayerId = 1 }
                    }
                },
                new Player
                {
                    Id = 2,
                    Name = "Forward2",
                    Position = "forward",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "speed", Value = 90, PlayerId = 2 }, // Same speed as Forward1
                        new PlayerSkill { Skill = "attack", Value = 88, PlayerId = 2 }
                    }
                },
                new Player
                {
                    Id = 3,
                    Name = "Forward3",
                    Position = "forward",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "speed", Value = 85, PlayerId = 3 },
                        new PlayerSkill { Skill = "attack", Value = 92, PlayerId = 3 }
                    }
                }
            };

            // Seed the test database
            _context.Players.AddRange(_testPlayers);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ProcessTeam_SelectsBestPlayerForSkill_WhenSingleHighestSkillExists()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "attack" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Players);
            Assert.Equal("Forward3", result.Players[0].Name); // Should select player with highest attack (92)
        }

        [Fact]
        public async Task ProcessTeam_SelectsAnyPlayerWithEqualHighestSkill_WhenMultiplePlayersHaveSameSkillValue()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "speed" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Players);
            Assert.Contains(result.Players[0].Name, new[] { "Forward1", "Forward2" }); // Either player with speed 90 is acceptable
            var selectedPlayerSpeed = result.Players[0].PlayerSkills.Find(s => s.Skill == "speed").Value;
            Assert.Equal(90, selectedPlayerSpeed);
        }

        [Fact]
        public async Task ProcessTeam_ConsidersOnlyPlayersInRequestedPosition()
        {
            // Arrange
            // Add a midfielder with high pace to ensure position is considered
            var midfielder = new Player
            {
                Id = 4,
                Name = "Midfielder1",
                Position = "midfielder",
                PlayerSkills = new List<PlayerSkill>
                {
                    new PlayerSkill { Skill = "speed", Value = 95, PlayerId = 4 } // Higher than any forward
                }
            };
            _context.Players.Add(midfielder);
            await _context.SaveChangesAsync();

            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "speed" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Players);
            Assert.Contains(result.Players[0].Name, new[] { "Forward1", "Forward2" }); // Should not select midfielder despite higher pace
            Assert.Equal("forward", result.Players[0].Position);
        }

        [Fact]
        public async Task ProcessTeam_PreventsDuplicatePlayerSelection_WhenMultipleSkillsRequiredForPosition()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "Forward", new List<string> { "Pace", "Shooting" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Players.Count);
            Assert.NotEqual(result.Players[0].Id, result.Players[1].Id); // Should be different players
        }

        [Fact]
        public async Task ProcessTeam_SelectsBalancedTeam_WhenMultiplePositionsRequired()
        {
            // Arrange
            // Add midfielders with varying skills
            var midfielders = new List<Player>
            {
                new Player
                {
                    Id = 5,
                    Name = "Midfielder1",
                    Position = "midfielder",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "passing", Value = 90, PlayerId = 5 },
                        new PlayerSkill { Skill = "vision", Value = 85, PlayerId = 5 }
                    }
                },
                new Player
                {
                    Id = 6,
                    Name = "Midfielder2",
                    Position = "midfielder",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "passing", Value = 85, PlayerId = 6 },
                        new PlayerSkill { Skill = "vision", Value = 90, PlayerId = 6 }
                    }
                }
            };

            _context.Players.AddRange(midfielders);
            await _context.SaveChangesAsync();

            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "speed", "attack" } },
                    { "midfielder", new List<string> { "passing", "vision" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Players.Count); // 2 forwards, 2 midfielders

            var selectedForwards = result.Players.Where(p => p.Position == "forward").ToList();
            var selectedMidfielders = result.Players.Where(p => p.Position == "midfielder").ToList();

            Assert.Equal(2, selectedForwards.Count);
            Assert.Equal(2, selectedMidfielders.Count);

            // Verify forwards were selected correctly
            Assert.Contains(selectedForwards, p => p.PlayerSkills.Any(s => s.Skill == "speed" && s.Value == 90));
            Assert.Contains(selectedForwards, p => p.PlayerSkills.Any(s => s.Skill == "attack" && s.Value == 92));

            // Verify midfielders were selected correctly
            Assert.Contains(selectedMidfielders, p => p.PlayerSkills.Any(s => s.Skill == "passing" && s.Value == 90));
            Assert.Contains(selectedMidfielders, p => p.PlayerSkills.Any(s => s.Skill == "vision" && s.Value == 90));
        }

        [Fact]
        public async Task ProcessTeam_HandlesEmptySkillList_ForPosition()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string>() }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _teamService.ProcessTeamAsync(request));
        }

        [Fact]
        public async Task ProcessTeam_SelectsPlayerWithHighestAverageSkillValue_WhenTied()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player
                {
                    Id = 7,
                    Name = "Defender1",
                    Position = "defender",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "tackling", Value = 90, PlayerId = 7 },
                        new PlayerSkill { Skill = "marking", Value = 90, PlayerId = 7 }, // Average 90
                        new PlayerSkill { Skill = "strength", Value = 90, PlayerId = 7 }
                    }
                },
                new Player
                {
                    Id = 8,
                    Name = "Defender2",
                    Position = "defender",
                    PlayerSkills = new List<PlayerSkill>
                    {
                        new PlayerSkill { Skill = "tackling", Value = 90, PlayerId = 8 },
                        new PlayerSkill { Skill = "marking", Value = 85, PlayerId = 8 }, // Average 87.5
                        new PlayerSkill { Skill = "strength", Value = 88, PlayerId = 8 }
                    }
                }
            };
            _context.Players.AddRange(players);
            await _context.SaveChangesAsync();

            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "defender", new List<string> { "tackling" } }
                }
            };

            // Act
            var result = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Players);
            Assert.Equal("Defender1", result.Players[0].Name); // Should select player with higher overall skill average
            Assert.Equal(90, result.Players[0].PlayerSkills.First(s => s.Skill == "tackling").Value);
        }

        [Fact]
        public async Task ProcessTeam_HandlesUnknownSkillName()
        {
            // Arrange
            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "nonexistentskill" } }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _teamService.ProcessTeamAsync(request));
            Assert.Contains("skill", exception.Message.ToLower()); // Error message should mention skill
        }

        [Fact]
        public async Task ProcessTeam_WithSpecificPositionAndSkills_SelectsBestPlayer()
        {
            // Arrange
            var midfielder = new Player
            {
                Id = 9,
                Name = "Midfielder1",
                Position = "midfielder",
                PlayerSkills = new List<PlayerSkill>
                {
                    new PlayerSkill { Skill = "speed", Value = 95, PlayerId = 9 }
                }
            };

            var forward1 = new Player
            {
                Id = 10,
                Name = "Forward1",
                Position = "forward",
                PlayerSkills = new List<PlayerSkill>
                {
                    new PlayerSkill { Skill = "speed", Value = 90, PlayerId = 10 },
                    new PlayerSkill { Skill = "attack", Value = 85, PlayerId = 10 }
                }
            };

            var forward2 = new Player
            {
                Id = 11,
                Name = "Forward2",
                Position = "forward",
                PlayerSkills = new List<PlayerSkill>
                {
                    new PlayerSkill { Skill = "speed", Value = 85, PlayerId = 11 },
                    new PlayerSkill { Skill = "attack", Value = 90, PlayerId = 11 }
                }
            };

            _context.Players.AddRange(new[] { midfielder, forward1, forward2 });
            await _context.SaveChangesAsync();

            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "attack" } }
                }
            };

            // Act
            var team = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(team);
            Assert.Single(team.Players);
            Assert.Equal("Forward2", team.Players[0].Name); // Should select forward with highest Attack skill
            Assert.Equal(90, team.Players[0].PlayerSkills.First(s => s.Skill == "attack").Value);
        }

        [Fact]
        public async Task ProcessTeam_WithMultipleSkillRequirements_SelectsPlayersWithHighestSkillSet()
        {
            // Arrange
            var player1 = new Player
            {
                Id = 12,
                Name = "Player1",
                Position = "forward",
                PlayerSkills = new List<PlayerSkill>
                {
                    new PlayerSkill { Skill = "passing", Value = 90, PlayerId = 12 },
                    new PlayerSkill { Skill = "speed", Value = 85, PlayerId = 12 }
                }
            };

            var player2 = new Player
            {
                Id = 13,
                Name = "Player2",
                Position = "forward",
                PlayerSkills = new List<PlayerSkill>
                {
                    new PlayerSkill { Skill = "passing", Value = 85, PlayerId = 13 },
                    new PlayerSkill { Skill = "speed", Value = 90, PlayerId = 13 }
                }
            };

            _context.Players.AddRange(new[] { player1, player2 });
            await _context.SaveChangesAsync();

            var request = new TeamRequestDto
            {
                RequiredPositions = new Dictionary<string, List<string>>
                {
                    { "forward", new List<string> { "passing", "speed" } }
                }
            };

            // Act
            var team = await _teamService.ProcessTeamAsync(request);

            // Assert
            Assert.NotNull(team);
            Assert.Equal(2, team.Players.Count);

            var passingPlayer = team.Players.First(p => p.PlayerSkills.Any(s => s.Skill == "passing" && s.Value == 90));
            var speedPlayer = team.Players.First(p => p.PlayerSkills.Any(s => s.Skill == "speed" && s.Value == 90));

            Assert.Equal("Player1", passingPlayer.Name);
            Assert.Equal("Player2", speedPlayer.Name);
        }
    }
}
