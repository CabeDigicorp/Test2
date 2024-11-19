using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class ClienteDto
    {
        public Guid Id { get; set; }
        public string? CodiceCliente { get; set; }
        public string? Nome { get; set; }

        public List<string>? DominiAssociati { get; set; }

        public string? ChiaveLicenza { get; set; }
        public List<string>? ArchivioLicenze { get; set; }
    }
}
