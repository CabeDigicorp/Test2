using ModelData.Dto;
using System.Reflection.Metadata.Ecma335;

namespace JoinWebUI.Models
{
    public class AllegatoModel : IListBoxCardBaseModel
    {
        public Guid Id { get; set; }
        public string IdString => Id.ToString();

        public string FileName { get; private set; }

        public long FileSize { get; private set; } = 0L;

        public bool Compressed { get; private set; } = false;

        //public int NumeroProgettiAllegato { get; private set; }

        //public int NumeroProgettiModello { get; private set; }

        public List<Guid> Progetti { get; set; } = new List<Guid>();

        public AllegatoConversionState ConversionState { get; private set; }

        public Guid? ParentId { get; private set; }

        public DateTime UploadDateTime { get; private set; } = DateTime.Now;

        public string TextLine1 => FileName;
        public string TextLine2 => (FileSize / (1024 * 1024)).ToString("#0.0") + " MB";
        public bool MultiLine => false;

        
        //public bool DropdownOpen { get; set; } = false;


    }
}
