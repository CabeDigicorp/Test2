using System.ComponentModel;
using System.Globalization;

namespace ModelData.Dto
{
    public class OperaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public List<Guid> TagIds { get; set; } = new List<Guid>();
        public Guid SettoreId { get; set; }
        //public string SettoreNome { get; set; } = string.Empty;
        public Guid ClienteId { get; set; }
        //public string ClienteInfo { get; set; } = string.Empty;
    }
}

