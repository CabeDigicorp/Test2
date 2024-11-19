using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class RegisterOtherDto
    {
        [MaxLength(255)]
        [Required(ErrorMessage = "Nome is required")]
        public string? Nome { get; set; }
        [Required(ErrorMessage = "Cognome is required")]
        [MaxLength(255)]
        public string? Cognome { get; set; }
        [EmailAddress]
        [MaxLength(255)]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
        [EmailAddress]
        [MaxLength(255)]
        [Compare("Email", ErrorMessage = "Email and ConfirmEmail must match")]
        [Required(ErrorMessage = "ConfirmEmail is required")]
        public string? ConfirmEmail { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Compare("Password", ErrorMessage = "Password and ConfirmPassword must match")]
        [Required(ErrorMessage = "ConfirmPassword is required")]
        public string? ConfirmPassword { get; set; }
    }
}
