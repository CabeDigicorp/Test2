using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
	public class ConfirmEmailDto
	{

        [Required]
        public Guid? UtenteId { get; set; }

        [Required]
        public string? Token { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(255)]
        public string? Email { get; set; }
    }
}
