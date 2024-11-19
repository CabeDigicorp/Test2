using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class PasswordForgottenDto
    {
        [MaxLength(255)]
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
    }
}
