using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class TagCreateFormModel
    {
        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }  = string.Empty;

        public Guid ClienteId { get; set; }
    }
}
