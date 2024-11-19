using ModelData.Dto;
using Syncfusion.Blazor.PivotView;
using ModelData.Model;

namespace JoinWebUI.Models.Filtri
{
    /// <summary>
    /// Gli operatori logici possono essere radici, nodi intermedi o foglie (se non hanno parametri, e quindi inutili).
    /// </summary>
    public class FilterLogicalOperation : IFilterLogicalComponent, IDisposable, ICloneable
    {
        public Guid Id { get; set; }                                     // id univoco
        public ValoreConditionsGroupOperator Operator { get; set; }      // tipo operatori
        public List<IFilterLogicalComponent> Parameters { get; set; }    // lista parametri (composita)

        /// <summary>
        /// Costrutture dove passo il tipo di operatore logico
        /// </summary>
        /// <param name="op"></param>
        public FilterLogicalOperation(ValoreConditionsGroupOperator op)
        {
            Id = Guid.NewGuid();
            Operator = op;
            Parameters = new List<IFilterLogicalComponent>();
        }

        /// <summary>
        /// Aggiungi un operatore
        /// </summary>
        /// <param name="component"></param>
        public void AddComponent(IFilterLogicalComponent component)
        {
            Parameters.Add(component);
        }

        /// <summary>
        /// Rimuovi un operatore
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(IFilterLogicalComponent component)
        {
            Parameters.Remove(component);
        }

        /// <summary>
        /// Eventuale funzione per valutare la formula qui contenuta
        /// </summary>
        /// <returns></returns>
        public string Evaluate()
        {
            var evaluatedComponents = Parameters.Select(c => c.Evaluate());
            return $"({string.Join($" {Operator} ", evaluatedComponents)})";
        }

        /// <summary>
        /// Funzione che enumera gli elementi della formula in modo da creare una sequenza leggibile
        /// </summary>
        /// <param name="component"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<(IFilterLogicalComponent Component, int Level)> FlattenComponents(IFilterLogicalComponent component, int level = 0)
        {
            var result = new List<(IFilterLogicalComponent, int)> { (component, level) };

            if (component is FilterLogicalOperation logicalOperation)
            {
                foreach (var subComponent in logicalOperation.Parameters)
                {
                    result.AddRange(FlattenComponents(subComponent, level + 1));
                }
            }

            return result;
        }

        public IFilterLogicalComponent Clone()
        {
            // Crea una copia del FilterLogicalOperation
            var clonedOperation = new FilterLogicalOperation(this.Operator)
            {
                Id = this.Id, // Mantiene lo stesso Id o può essere cambiato a seconda delle necessità
                Operator = this.Operator,
                // Clona ricorsivamente i componenti figli
                Parameters = this.Parameters.Select(param => (param.Clone())).ToList()
            };

            return clonedOperation;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Logica supplementare per la pulizia
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
