using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelData.Utilities;

namespace ModelData.Dto
{
    public class LicenzaAuthDto
    {
        public string LicenseKey { get; set; }

        public string CodiceCliente { get; set; }

        public string? NomeCliente { get; set; }

        public LicenzaAuthDto(string licenseKey, string codiceCliente, string? nomeCliente)
        {
            LicenseKey = licenseKey;
            CodiceCliente = codiceCliente;
            NomeCliente = nomeCliente;
        }

    }

}
