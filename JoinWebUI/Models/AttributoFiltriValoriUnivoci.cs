namespace JoinWebUI.Models
{
    public class AttributoFiltriValoriUnivoci : IDisposable, ICloneable
    {
        ~AttributoFiltriValoriUnivoci()
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
        public string? Antenati { get; set; }
        public string? ValoreAttributo { get; set; }
        public string? ValoreEtichetta { get; set; }

        private string? _valore;
        private Guid _attributoId;


        public AttributoFiltriValoriUnivoci Clone() => new AttributoFiltriValoriUnivoci
        {
            ProgettoId = this.ProgettoId,
            AttributoId = this.AttributoId,
            Codice = this.Codice,
            Antenati = this.Antenati,
            CodiceGruppo = this.CodiceGruppo,
            ValoreAttributo = this.ValoreAttributo,
            ValoreEtichetta = this.ValoreEtichetta,
        };

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
