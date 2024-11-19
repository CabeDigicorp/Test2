using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class ProgettoUpdateFormModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Nome { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Descrizione { get; set; } = string.Empty;
    }
}