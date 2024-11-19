
using ProtoBuf;

namespace ModelData.Model
{
    
    [ProtoContract]
    public class AttributoGroupData
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; }


    }

}
