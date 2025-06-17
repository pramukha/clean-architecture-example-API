using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public List<PlayerDto> Players { get; set; } = new List<PlayerDto>();
    }
}
