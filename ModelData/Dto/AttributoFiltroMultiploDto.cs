using ModelData.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using ModelData.Dto;
using System.ComponentModel;

namespace ModelData.Dto
{
    public class AttributoFiltroMultiploDto : IDisposable, INotifyPropertyChanged
    {
        private IEnumerable<AttributoRaggruppatoreDto>? _attributiRaggruppatori;
        private IEnumerable<AttributoAggregatoDto>? _attributiAggregati;
        private AttributoFiltroCompositoDto? _attributiFiltri;
        private Guid _progettoId = Guid.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        ~AttributoFiltroMultiploDto()
        {
            Dispose(false);
        }

        public IEnumerable<AttributoRaggruppatoreDto>? AttributiRaggruppatori
        {
            get => _attributiRaggruppatori;
            set
            {
                if (_attributiRaggruppatori != value)
                {
                    _attributiRaggruppatori = value;
                    OnPropertyChanged(nameof(AttributiRaggruppatori));
                }
            }
        }        
        
        public IEnumerable<AttributoAggregatoDto>? AttributiAggregati
        {
            get => _attributiAggregati;
            set
            {
                if (_attributiAggregati != value)
                {
                    _attributiAggregati = value;
                    OnPropertyChanged(nameof(AttributiAggregati));
                }
            }
        }

        public AttributoFiltroCompositoDto? AttributiFiltri
        {
            get => _attributiFiltri;
            set
            {
                if (_attributiFiltri != value)
                {
                    _attributiFiltri = value;
                    OnPropertyChanged(nameof(AttributiFiltri));
                }
            }
        }        
        
        public Guid ProgettoId
        {
            get => _progettoId;
            set
            {
                if (_progettoId != value)
                {
                    _progettoId = value;
                    OnPropertyChanged(nameof(ProgettoId));
                }
            }
        }

        public IEnumerable<AttributoFiltroDto>? ExtractAttributiFiltro(AttributoFiltroCompositoDto dto)
        {
            if (dto.AttributoFiltro != null)
            {
                yield return dto.AttributoFiltro;
            }

            if (dto.Children != null)
            {
                foreach (var child in dto.Children)
                {
                    foreach (var attributoFiltro in ExtractAttributiFiltro(child))
                    {
                        yield return attributoFiltro;
                    }
                }
            }
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
                AttributiFiltri?.Dispose();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}