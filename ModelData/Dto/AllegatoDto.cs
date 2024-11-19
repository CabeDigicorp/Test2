using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class AllegatoDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }

        public long FileSize { get; set; } = 0L;

        public bool Compressed { get; set; }

        public DateTime UploadDateTime { get; set; } = DateTime.MinValue;

        public Guid OperaId { get; set; }

        public Guid? ParentId { get; set; }

        //public int NumeroProgettiAllegato { get; set; }

        //public int NumeroProgettiModello { get; set; }

        public List<Guid> Progetti { get; set; } = new List<Guid>();

        public AllegatoConversionState ConversionState { get; set; }
    }

    public enum AllegatoConversionState
    {
        NotAvailable,
        NotConverted,
        ConversionInProgress,
        Converted,
        Invalid
    }

    public class IfcIdsDto
    {
        public Guid IfcId { get; set; }
        public Guid GeometriesId { get; set; }
        public Guid PropertiesId { get; set; }
    }
}
