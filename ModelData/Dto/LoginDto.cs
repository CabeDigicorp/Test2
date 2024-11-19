using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class LoginDto
    {
        [MaxLength(255)]
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "RememberMe is required")]
        public bool RememberMe { get; set; }
    }
}
