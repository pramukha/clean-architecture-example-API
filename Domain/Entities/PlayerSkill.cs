using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class PlayerSkill
    {
        public int Id { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Skill { get; set; }

        [Required]
        [Range(0, 100)]
        public int Value { get; set; }

        public Player Player { get; set; }
    }
}
