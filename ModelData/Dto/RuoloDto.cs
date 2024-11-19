namespace ModelData.Dto
{
    public class RuoloDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public bool Inheritable { get; set; } = true;

        public List<string> Azioni { get; set; }

    }
}
