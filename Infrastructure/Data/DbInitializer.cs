using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Players.Any())
                {
                    return;   // DB has been seeded
                }

                var players = new List<Player>
                {
                    // Forwards
                    new Player
                    {
                        Id = 1,
                        Name = "John Doe",
                        Position = "forward",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "attack", Value = 90 },
                            new PlayerSkill { Skill = "speed", Value = 85 },
                            new PlayerSkill { Skill = "dribbling", Value = 88 }
                        }
                    },
                    new Player
                    {
                        Id = 2,
                        Name = "Alex Smith",
                        Position = "forward",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "attack", Value = 95 },
                            new PlayerSkill { Skill = "speed", Value = 82 },
                            new PlayerSkill { Skill = "stamina", Value = 78 }
                        }
                    },
                    // Midfielders
                    new Player
                    {
                        Id = 3,
                        Name = "Jane Wilson",
                        Position = "midfielder",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "passing", Value = 92 },
                            new PlayerSkill { Skill = "vision", Value = 88 },
                            new PlayerSkill { Skill = "stamina", Value = 85 }
                        }
                    },
                    new Player
                    {
                        Id = 4,
                        Name = "Mike Brown",
                        Position = "midfielder",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "passing", Value = 90 },
                            new PlayerSkill { Skill = "vision", Value = 94 },
                            new PlayerSkill { Skill = "attack", Value = 75 }
                        }
                    },
                    // Defenders
                    new Player
                    {
                        Id = 5,
                        Name = "Steve Johnson",
                        Position = "defender",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "defense", Value = 92 },
                            new PlayerSkill { Skill = "strength", Value = 88 },
                            new PlayerSkill { Skill = "speed", Value = 75 }
                        }
                    },
                    new Player
                    {
                        Id = 6,
                        Name = "David Lee",
                        Position = "defender",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "defense", Value = 88 },
                            new PlayerSkill { Skill = "strength", Value = 90 },
                            new PlayerSkill { Skill = "stamina", Value = 82 }
                        }
                    },
                    // Goalkeepers
                    new Player
                    {
                        Id = 7,
                        Name = "Tim Clark",
                        Position = "goalkeeper",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "reflexes", Value = 93 },
                            new PlayerSkill { Skill = "handling", Value = 89 },
                            new PlayerSkill { Skill = "defense", Value = 85 }
                        }
                    },
                    new Player
                    {
                        Id = 8,
                        Name = "Peter White",
                        Position = "goalkeeper",
                        PlayerSkills = new List<PlayerSkill>
                        {
                            new PlayerSkill { Skill = "reflexes", Value = 90 },
                            new PlayerSkill { Skill = "handling", Value = 92 },
                            new PlayerSkill { Skill = "defense", Value = 82 }
                        }
                    },

                };

                // Add players to context
                context.Players.AddRange(players);

                // Save changes to populate player IDs
                context.SaveChanges();

                // Now that players are saved, update the PlayerId for each skill
                foreach (var player in players)
                {
                    foreach (var skill in player.PlayerSkills)
                    {
                        skill.PlayerId = player.Id;
                    }
                }

                // Save changes again to update skill relationships
                context.SaveChanges();
            }
        }
    }
}
