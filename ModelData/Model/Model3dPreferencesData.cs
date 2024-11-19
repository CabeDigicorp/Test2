
using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class Model3dPreferencesData
    {
        [ProtoMember(1)]
        public List<string> lstHiddenEntities { get; set; }  = new List<string>();

    }


}
