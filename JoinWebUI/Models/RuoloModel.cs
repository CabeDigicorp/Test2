namespace JoinWebUI.Models
{
	public class RuoloModel : IEquatable<RuoloModel>
	{
		public Guid Id { get; set; }
        public string IdString { get { return Id.ToString(); } }
        public string? Name { get; set; }

        public bool Inheritable { get; set; } = true;

        public List<ModelData.Utilities.Azioni> Azioni { get; set; }

        public bool Equals(RuoloModel? other)
        {
			return this.Id.Equals(other?.Id);
        }
    }

    public enum RuoloValues
    {
        //NotAssigned = 0,
        Inherited = 1,
        Inheritable = 2,
        NotInheritable = 3
    }
}
