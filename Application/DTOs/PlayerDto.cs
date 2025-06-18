using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{

    public class PlayerDto
    {        
        /// <summary>
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
        
    }
}
