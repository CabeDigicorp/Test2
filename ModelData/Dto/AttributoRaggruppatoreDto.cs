
using ModelData.Model;

namespace ModelData.Dto
{
    public class AttributoRaggruppatoreDto : IDisposable, ICloneable
    {
        public Guid ProgettoId { get; set; }
        public Guid AttributoId { get; set; }
        public int? SequenceNumber { get; set; }
        public bool IsAllowMasterGrouping { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSelected { get; set; }
        public string? Codice { get; set; }
        public string? CodiceGruppo { get; set; }
        public string? NomeGruppo { get; set; }
        public string? Antenati { get; set; }
        public string? DefinizionAttributoCodice { get; set; }
        public string? ValoreAttributo { get; set; }
        public string? ValoreEtichetta { get; set; }
        public string? Descrizione { get; set; }
        public string? Tooltip { get; set; }
        public string? FiltroCondizione { get; set; }
        public List<Guid>? EntitiesId { get; set; } = new List<Guid>();
        public List<string>? ValoriUnivociOrdered { get; set; } = new List<string>();
        public List<AttributoRaggruppatoreDto>? SottoGruppo { get; set; } = new List<AttributoRaggruppatoreDto>();

        public AttributoRaggruppatoreDto Clone() => new AttributoRaggruppatoreDto
        {
            ProgettoId = this.ProgettoId,
            AttributoId = this.AttributoId,
            Codice = this.Codice,
            CodiceGruppo = this.CodiceGruppo,
            NomeGruppo = this.NomeGruppo,
            Antenati = this.Antenati,
            DefinizionAttributoCodice = this.DefinizionAttributoCodice,
            ValoreAttributo = this.ValoreAttributo,
            ValoreEtichetta = this.ValoreEtichetta,
            Descrizione = this.Descrizione,
            Tooltip = this.Tooltip,
            SequenceNumber = this.SequenceNumber,
            IsAllowMasterGrouping = this.IsAllowMasterGrouping,
            IsVisible = this.IsVisible,
            IsSelected = this.IsSelected,
            FiltroCondizione = this.FiltroCondizione,
            EntitiesId = new List<Guid>(this.EntitiesId ?? new List<Guid>()),
            ValoriUnivociOrdered = new List<string>(this.ValoriUnivociOrdered ?? new List<string>()),
            SottoGruppo = new List<AttributoRaggruppatoreDto>(this.SottoGruppo ?? new List<AttributoRaggruppatoreDto>())
        };

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AttributoRaggruppatoreDto()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EntitiesId?.Clear();
                ValoriUnivociOrdered?.Clear();

                if (SottoGruppo != null)
                {
                    foreach (var item in SottoGruppo)
                    {
                        (item as IDisposable)?.Dispose();
                    }
                    SottoGruppo.Clear();
                }
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
