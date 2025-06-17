using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{    /// <summary>
    /// Represents a request to generate a team with specific positions and required skills
    /// </summary>
    public class TeamRequestDto
    {
        /// <summary>
        /// Dictionary of positions and their required skills.
        /// Key: Position name (e.g., "Forward", "Midfielder")
        /// Value: List of required skills for that position
        /// </summary>        /// <example>
        /// {
        ///     "Forward": ["Pace", "Shooting"],
        ///     "Midfielder": ["Passing", "Vision"],
        ///     "Defender": ["Tackling", "Strength"],
        ///     "Goalkeeper": ["Reflexes", "Positioning"]
        /// }
        /// </example>
        [Required]
        public Dictionary<string, List<string>> RequiredPositions { get; set; } = new Dictionary<string, List<string>>();
    }
}
