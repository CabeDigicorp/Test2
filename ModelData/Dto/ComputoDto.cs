using System;
using System.ComponentModel;

namespace ModelData.Dto
{
    public class ComputoDto : IDisposable, ICloneable
    {
        public Guid ProgettoId { get; set; }
        public Guid EntityId { get; set; }
        public bool IsMultiValore { get; set; } = false;
        public bool IsMultiValoreFormula { get; set; } = false;
        public bool IsMultiValoreDescrizione { get; set; } = false;
        public bool IsVisible { get; set; } = false;
        public bool IsAllowMasterGrouping { get; set; } = false;
        public string? Codice { get; set; } = string.Empty;
        public string? Etichetta { get; set; } = string.Empty;
        public string? Descrizione { get; set; } = string.Empty;
        public string? NomeGruppo { get; set; } = string.Empty;
        public string? SourceCodice { get; set; } = string.Empty;
        public string? ValoreUtente { get; set; } = string.Empty;
        public string? ValoreAttributo { get; set; } = string.Empty;
        public string? ValoreEtichetta { get; set; } = string.Empty;
        public string? ValoreFormula { get; set; } = string.Empty;
        public string? ValoreDescrizione { get; set; } = string.Empty;
        public string? DefinizioneAttributoCodice { get; set; } = string.Empty;
        public string? GuidReferenceEntityTypeKey { get; set; } = string.Empty;
        public string? EntitiesIdNum { get; set; } = string.Empty;
        public List<string>? ValoriUnivociOrdered { get; set; } = new List<string>();
        public List<Guid>? EntitiesId { get; set; }

        public ComputoDto Clone() => new ComputoDto
        {
            ProgettoId = this.ProgettoId,
            EntityId = this.EntityId,
            Codice = this.Codice,
            Etichetta = this.Etichetta,
            Descrizione = this.Descrizione,
            NomeGruppo = this.NomeGruppo,
            SourceCodice = this.SourceCodice,
            ValoreUtente = this.ValoreUtente,
            ValoreAttributo = this.ValoreAttributo,
            ValoreEtichetta = this.ValoreEtichetta,
            ValoreFormula = this.ValoreFormula,
            ValoreDescrizione = this.ValoreDescrizione,
            DefinizioneAttributoCodice = this.DefinizioneAttributoCodice,
            GuidReferenceEntityTypeKey = this.GuidReferenceEntityTypeKey,
            EntitiesIdNum = this.EntitiesIdNum,
            IsMultiValore = this.IsMultiValore,
            IsMultiValoreFormula = this.IsMultiValoreFormula,
            IsMultiValoreDescrizione = this.IsMultiValoreDescrizione,
            IsVisible = this.IsVisible,
            IsAllowMasterGrouping = this.IsAllowMasterGrouping,
            ValoriUnivociOrdered = new List<string>(this.ValoriUnivociOrdered ?? new List<string>()),
            EntitiesId = new List<Guid>(this.EntitiesId ?? new())
        };

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EntitiesId?.Clear();
                ValoriUnivociOrdered?.Clear();
            }
        }

        ~ComputoDto()
        {
            Dispose(false);
        }
    }
}
