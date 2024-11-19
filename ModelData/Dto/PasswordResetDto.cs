using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class PasswordResetDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public Guid? UserId { get; set; }
        [Required]
        public string? PasswordResetToken { get; set; }
        [DataType(DataType.Password)]
        [MaxLength(255)]
        [Required]
        public string? NewPassword { get; set; }
    }
}
