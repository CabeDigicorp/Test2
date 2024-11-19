using System.ComponentModel;

namespace ModelData.Dto
{
    public class InfoProgettoDto
    {
        public Guid ProgettoId { get; set; }
        public string? Codice { get; set; } = string.Empty;
        public string? Etichetta { get; set; } = string.Empty;
        public string? Descrizione { get; set; } = string.Empty;
        public string? GroupName { get; set; } = string.Empty;
        public string? SourceCodice { get; set; } = string.Empty;
        public string? ValoreUtente { get; set; } = string.Empty;
        public string? Valore { get; set; } = string.Empty;
        public string? ValoreFormula { get; set; } = string.Empty;
        public string? ValoreDescrizione { get; set; } = string.Empty;
        public string? DefinizioneAttributoCodice { get; set; } = string.Empty;
        public string? GuidReferenceEntityTypeKey { get; set; } = string.Empty;
        public string? EntitiesIdNum { get; set; } = string.Empty;
        public bool IsMultiValore { get; set; } = false;
        public bool IsMultiValoreFormula { get; set; } = false;
        public bool IsMultiValoreDescrizione { get; set; } = false;
        public bool IsVisible { get; set; } = false;
        public bool IsAllowMasterGrouping { get; set; } = false;
    }
}
