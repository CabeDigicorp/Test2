namespace ModelData.Model
{
    public class ValoreConditionObj : IDisposable, ICloneable
    {
        public string? Nome { get; set; }
        public ValoreConditionEnum ValoreCondition { get; set; } = ValoreConditionEnum.Equal;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public ValoreConditionObj Clone() => new ValoreConditionObj
        {
            Nome = this.Nome,
            ValoreCondition = this.ValoreCondition,
        };
    }
}
