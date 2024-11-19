using ModelData.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using ModelData.Dto;

namespace ModelData.Dto
{
    public class AttributoFiltroCompositoDto : IDisposable, ICloneable
    {
        ~AttributoFiltroCompositoDto()
        {
            Dispose(false);
        }

        public Guid ProgettoId { get; set; }
        public ValoreConditionsGroupOperator OperatoreLogico { get; set; } = ValoreConditionsGroupOperator.And; // Operatore logico se presenti delle composizioni figlie (Children), altrimenti è inutile
        public List<AttributoFiltroCompositoDto>? Children { get; set; } = new List<AttributoFiltroCompositoDto>();  // Eventuali sotto operatori annidati
        public AttributoFiltroDto? AttributoFiltro { get; set; }  // Attributo selezionato per il filtro (es. quantità)
        public ValoreConditionEnum Condizione { get; set; }  // Condizione applicata (minore di, maggiore, etc.)
        public string? Valore { get { return _valore; } set { if (AttributoFiltro != null) { AttributoFiltro.ValoreAttributo = value; } _valore = value; } }  // ValoreAttributo applicato alla condizione (es. 100)

        private string? _valore = string.Empty;
        public AttributoFiltroCompositoDto Clone() => new AttributoFiltroCompositoDto
        {
            ProgettoId = this.ProgettoId,
            OperatoreLogico = this.OperatoreLogico,
            Children = this.Children?.Select(c => c.Clone()).ToList() ?? new List<AttributoFiltroCompositoDto>(),
            AttributoFiltro = AttributoFiltro != null ? this.AttributoFiltro?.Clone() : null,
            Condizione = this.Condizione,
            Valore = this.Valore,
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
                Children?.Clear();
                AttributoFiltro?.Dispose();
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
