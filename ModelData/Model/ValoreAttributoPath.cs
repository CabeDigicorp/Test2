using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class AttributoFilter
    {
        [ProtoMember(1)]
        public string EntityTypeCodice { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string CodiceAttributo { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string Valore { get; set; } = null;
    }
}
