using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class TagRenameFormModel
    {
        [Required]
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Nome { get; set; } = string.Empty;
    }
}
