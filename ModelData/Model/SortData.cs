
using ProtoBuf;

namespace ModelData.Model
{

    [ProtoContract]
    public class AttributoSortData
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; }

        [ProtoMember(2)]
        public bool IsOrdinamentoInverso { get; set; }

    }
}
