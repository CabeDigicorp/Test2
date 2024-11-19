using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class RegisterDto
    {
        [MaxLength(255)]
        public string? CodiceCliente { get; set; }

        [MaxLength(255)]
        [Required]
        public string? Nome { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Cognome { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        [Required]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Required]
        public string? Password { get; set; }

    }
}
