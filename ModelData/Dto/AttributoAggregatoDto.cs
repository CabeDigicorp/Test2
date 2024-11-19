using ModelData.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using ModelData.Dto;

namespace ModelData.Dto
{
    public class AttributoAggregatoDto : IDisposable, ICloneable
    {
        ~AttributoAggregatoDto()
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
        public string? IfcProjectGlobalId { get; set; }
        public string? IfcElemGlobalId { get; set; }
        public string? Tooltip { get; set; }
        public int? SequenceNumber { get; set; }
        public string? FiltroCondizione { get; set; }

        private Guid _attributoId;

        public AttributoAggregatoDto Clone() => new AttributoAggregatoDto
        {
            ProgettoId = this.ProgettoId,
            AttributoId = this.AttributoId,
            Codice = this.Codice,
            IfcProjectGlobalId = this.IfcProjectGlobalId,
            IfcElemGlobalId = this.IfcElemGlobalId,
            Tooltip = this.Tooltip,
            SequenceNumber = this.SequenceNumber,
            FiltroCondizione = this.FiltroCondizione,
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
