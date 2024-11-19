using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class TeamCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string? Nome { get; set; }

        [Required]
        public Guid ClienteId { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        [Required]
        public bool IsLicensed { get; set; }

    }
}
