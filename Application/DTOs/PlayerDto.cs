using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Represents a player's skill with a name and value
    /// </summary>
    // public class SkillDto
    // {
    //     /// <summary>
    //     /// Name of the skill (e.g., "Pace", "Shooting", "Passing")
    //     /// </summary>
    //     /// <example>Pace</example>
    //     [Required]
    //     public string Name { get; set; }

    //     /// <summary>
    //     /// Skill value ranging from 0 to 100
    //     /// </summary>
    //     /// <example>85</example>
    //     [Range(0, 100)]
    //     public int Value { get; set; }
    // }

    /// <summary>
    /// Represents a player with their details and skills
    /// </summary>
    public class PlayerDto
    {        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of the player
        /// </summary>
        /// <example>John Doe</example>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Player's position on the team
        /// </summary>
        /// <example>Forward</example>
        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        /// <summary>
        /// List of player's skills
        /// </summary>
        [Required]
        public List<PlayerSkillDto> PlayerSkills { get; set; } = new List<PlayerSkillDto>();
        // public object SkillLevel { get; internal set; }
    }
}
