
using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class Model3dTagsData
    {
        [ProtoMember(1)]
        public List<Model3dTagsDataItem> Items { get; set; } = new List<Model3dTagsDataItem>();
    }

    [ProtoContract]
    public class Model3dTagsDataItem
    {
        [ProtoMember(1)]
        public bool IsSelected { get; set; }

        [ProtoMember(2)]
        public string TagDescr { get; set; }

        [ProtoMember(3)]
        public Guid TagId { get; set; }

        [ProtoMember(4)]
        public List<Guid> lstAssociatedFilters { get; set; } = new List<Guid>();

        [ProtoMember(5)]
        public List<string> RvtAssociatedFilters { get; set; } = new List<string>();//unique id
    }



}
