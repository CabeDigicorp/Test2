using System.ComponentModel;

namespace JoinWebUI.Models
{
    public class ProgettoInfoFormModel
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string OperaId { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
    }
}
