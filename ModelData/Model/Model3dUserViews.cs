using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class Model3dUserViewList
    {
        [ProtoMember(1)]
        public List<Model3dSingleView> lstUserViews { get; set; } = new List<Model3dSingleView>();
    }

    [ProtoContract]
    public class Model3dSingleView
    {
        [ProtoMember(1)]
        public HashSet<string> lstGlobalID { get; set; } = new HashSet<string>();

        [ProtoMember(2)]
        public byte[] byteImage { get; set; }

        [ProtoMember(3)]
        public string ViewDescriz { get; set; }
    }


}
