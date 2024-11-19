
using ModelData.Model;
using System.Linq;

namespace ModelData.Dto
{
    public class AttributoFiltroDto : IDisposable, ICloneable
    {
        ~AttributoFiltroDto()
        {
            Dispose(false);
        }

        public Guid ProgettoId { get; set; }
        public Guid AttributoId
        {
            get { if (Guid.Empty == _attributoId) { _attributoId = Guid.NewGuid(); } return _attributoId; }
            set { if (Guid.Empty == value) { _attributoId = Guid.NewGuid(); } else { _attributoId = value; } }
        }
        public string? Codice { get; set; }
        public string? CodiceGruppo { get; set; }
        public string? DefinizionAttributoCodice { get; set; }
        public string? Antenati { get; set; }
        public string? ValoreAttributo { get; set; }
        public string? Descrizione { get; set; }
        public string? Tooltip { get; set; }
        public int? SequenceNumber { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }
        public string? EntityTypeKey { get; set; }
        public string? GuidReferenceEntityTypeKey { get; set; }
        public string? ValoreFormat { get; set; }
        public List<string>? ValoriUnivoci { get; set; } = new List<string>();

        private string? _valore;
        private Guid _attributoId;
        private List<string>? _valoriUnivoci;

        public AttributoFiltroDto Clone() => new AttributoFiltroDto
        {
            ProgettoId = this.ProgettoId,
            AttributoId = this.AttributoId,
            Codice = this.Codice,
            CodiceGruppo = this.CodiceGruppo,
            DefinizionAttributoCodice = this.DefinizionAttributoCodice,
            ValoreAttributo = this.ValoreAttributo,
            Descrizione = this.Descrizione,
            Tooltip = this.Tooltip,
            SequenceNumber = this.SequenceNumber,
            IsVisible = this.IsVisible,
            IsSelected = this.IsSelected,
            EntityTypeKey = this.EntityTypeKey,
            GuidReferenceEntityTypeKey = this.GuidReferenceEntityTypeKey,
            ValoreFormat = this.ValoreFormat,
            ValoriUnivoci = new List<string>(this.ValoriUnivoci ?? new List<string>()),
        };

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ValoriUnivoci?.Clear();
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
