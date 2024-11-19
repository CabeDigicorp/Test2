using System;
using System.ComponentModel;

namespace ModelData.Dto
{
    public class FoglioDiCalcoloDto : ICloneable
    {
        public FoglioDiCalcoloDto Clone() => new FoglioDiCalcoloDto
        {
            ProgettoId = this.ProgettoId,
            FoglioDiCalcoloBase = this.FoglioDiCalcoloBase,
            Codice = this.Codice,
            Descrizione = this.Descrizione
        };

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public Guid ProgettoId { get; set; }
        public string? Codice { get; set; } = string.Empty;
        public string? Descrizione { get; set; } = string.Empty;
        public byte[]? FoglioDiCalcoloBase { get; set; }
    }
}
