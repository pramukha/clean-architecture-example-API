using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Represents a player's skill with a name and value
    /// </summary>
    public class PlayerSkillDto
    {
        /// <summary>
        /// Name of the skill (e.g., "speed", "strength", "stamina")
        /// </summary>
        /// <example>speed</example>
        [Required]
        public string Skill { get; set; }

        /// <summary>
        /// Skill value ranging from 0 to 100
        /// </summary>
        /// <example>85</example>
        [Range(0, 100)]
        public int Value { get; set; }
        
        // These properties will not be included in the serialized output
        public int Id { get; set; }
        public int PlayerId { get; set; }
    }

    
}
