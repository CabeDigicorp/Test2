using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class PasswordChangeDto
    {
        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Required]
        public string? CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Required]
        public string? NewPassword { get; set; }
    }
}
