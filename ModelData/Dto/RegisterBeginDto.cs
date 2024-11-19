using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class RegisterBeginDto
    {
        [MaxLength(1000)]
        public string? ChiaveLicenza { get; set; }
        [Required]
        public bool RegisterAsGuest { get; set; } = false;
    }
}
