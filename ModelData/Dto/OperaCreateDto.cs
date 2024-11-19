using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class OperaCreateDto
    {
        [Required]
        public Guid? SettoreId { get; set; }

        [Required]
        [MaxLength(255)]
        
        public string Nome { get; set; } = string.Empty;
        [MaxLength(255)]
        
        public string Descrizione { get; set; } = string.Empty;
        
        public List<Guid> TagIds { get; set; } = new List<Guid>();

    }
}
