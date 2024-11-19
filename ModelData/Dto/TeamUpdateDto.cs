using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class TeamUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; } = string.Empty;

        //[Required]
        //public Guid ClienteId { get; set; } = Guid.Empty;

        //[Required]
        public bool IsAdmin { get; set; } = false;

        public bool IsLicensed { get; set; } = false;
    }
}
