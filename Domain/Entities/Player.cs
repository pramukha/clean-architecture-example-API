using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Position { get; set; }

        public List<PlayerSkill> PlayerSkills { get; set; } = new List<PlayerSkill>();
        
        public List<Team> Teams { get; set; } = new List<Team>();
    }
}
