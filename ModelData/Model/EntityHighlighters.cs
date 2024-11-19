using ProtoBuf;

namespace ModelData.Model
{

    /// <summary>
    /// Evidenziatori di entità
    /// </summary>
    [ProtoContract]
    public class EntityHighlighters
    {
        [ProtoMember(1)]
        public string EntityTypeKey { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string CodiceAttributo { get; set; } = string.Empty;

        [ProtoMember(3)]
        public Dictionary<string, ValoreHighlighter> Highlighters { get; set; } = new Dictionary<string, ValoreHighlighter>();

    }

    [ProtoContract]
    public class ValoreHighlighter
    {
        [ProtoMember(1)]
        public string Valore { get; set; } = string.Empty;

        [ProtoMember(2)]
        public ValoreColore Colore { get; set; } = null;
    }
}
