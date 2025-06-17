using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Represents a player's skill with a name and value
    /// </summary>
    public class PlayerSkillDto
    {
        /// <summary>
        /// Name of the skill (e.g., "Pace", "Shooting", "Passing")
        /// </summary>
        /// <example>Pace</example>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Skill value ranging from 0 to 100
        /// </summary>
        /// <example>85</example>
        [Range(0, 100)]
        public int Value { get; set; }
        public string Skill { get; set; }
        public int PlayerId { get; set; }
    }

    
}
