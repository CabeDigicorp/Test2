using ModelData.Dto;
using Syncfusion.Blazor.PivotView;
using ModelData.Model;

namespace JoinWebUI.Models.Filtri
{
    /// <summary>
    /// I parametri sono sempre delle "foglie" degli operatori logici
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterParameter<T> : IFilterLogicalComponent
    {
        /// <summary>
        /// T rappresenta il tipo di parametro (un tipo di filtro, oppure un'altra operazione). Per ora omesso, utile in caso di estensioni future
        /// </summary>
        public T Value { get; set; }
        public Guid Id { get; set; }
        public ValoreConditionEnum Condition { get; set; }          // condizione

        /// <summary>
        /// Lista di filtri disponbili
        /// </summary>
        public List<T> AttributiFiltriSelezionabili { get; set; }

        /// <summary>
        /// Attributo filtro selezionato
        /// </summary>
        public T? AttributoFiltroSelezionato { get; set; }        
        
        /// <summary>
        /// Valore attributo selezionato
        /// </summary>
        public string? ValoreAttributoSelezionato { get; set; }

        public FilterParameter(T value, ValoreConditionEnum condition = ValoreConditionEnum.Equal)
        {
            try
            {
                Id = Guid.NewGuid();                            // codice identificativo
                Value = value;                                  // è il tipo di parametro
                Condition = condition;                          // operatore condizionale
                AttributiFiltriSelezionabili = new List<T>();   // es: quantità con tutti i suoi valori validi, peso con tutti i suoi valori validi, etc...
                AttributoFiltroSelezionato = default;           // es: "quantità"
                ValoreAttributoSelezionato = default;           // es: "5"
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Metodo Clone per il FilterParameter
        public IFilterLogicalComponent Clone()
        {
            // Crea una copia del FilterParameter
            return new FilterParameter<T>(this.Value, this.Condition)
            {
                Id = this.Id,
                AttributiFiltriSelezionabili = new List<T>(this.AttributiFiltriSelezionabili),  // Clona la lista dei filtri
                AttributoFiltroSelezionato = this.AttributoFiltroSelezionato,
                ValoreAttributoSelezionato = this.ValoreAttributoSelezionato
            };
        }

        public string Evaluate()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
