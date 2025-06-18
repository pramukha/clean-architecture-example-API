using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Represents a requirement for a specific position with a main skill
    /// </summary>
    public class PositionRequirementDto
    {
        /// <summary>
        /// The position name (e.g., "midfielder", "defender")
        /// </summary>
        [Required]
        public string Position { get; set; }
        
        /// <summary>
        /// The main skill required for this position
        /// </summary>
        [Required]
        public string MainSkill { get; set; }
        
        /// <summary>
        /// Number of players required for this position
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of players must be at least 1")]
        public int NumberOfPlayers { get; set; }
    }
}
