namespace JoinWebUI.Models.Filtri
{
    public interface IFilterLogicalComponent
    {
        Guid Id { get; }
        string Evaluate();
        IFilterLogicalComponent Clone();
    }
}
