using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelData.Utilities;

namespace ModelData.Dto
{
    public class LicenzaDto
    {
        public string? ChiaveLicenza { get; set; } = null;

        public string CodiceCliente { get; set; } = string.Empty;

        public bool IsValid { get; set; } = true;

        public bool IsDisabled { get; set; } = false;

        public bool IsExpired { get; set; } = false;

        public short Activations { get; set; } = 0;

        public DateOnly ExpirationDate { get; set; } = new DateOnly(2022, 12, 31);

        public List<LicenseFeature> LicenseFeatures { get; set; } = new List<LicenseFeature>();

        public string AdditionalInfo { get; set; } = string.Empty;
    }

}
