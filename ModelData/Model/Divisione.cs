using ProtoBuf;
using System.Runtime.Serialization;

namespace ModelData.Model
{

    [ProtoContract]
    public class DivisioneItem : TreeEntity
    {




    }

    [ProtoContract]
    public class DivisioneItemType : TreeEntityType
    {
        [ProtoMember(1)]
        public Guid DivisioneId { get; set; }

        [ProtoMember(2)]
        public Model3dClassEnum Model3dClassName {get;set;}

        [ProtoMember(3)]
        public bool IsBuiltIn { get; set; } = false;

        [ProtoMember(4)]
        public int Position { get; set; } = -1;

    }


    [ProtoContract]
    public class DivisioneItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    public class DivisioneItemParentType : TreeEntityType
    {
        [ProtoMember(1)]
        public Guid DivisioneId { get; set; }

        //[ProtoMember(2)]
        //[DataMember]
        //public string Model3dClassName { get; set; }

        [ProtoMember(2)]
        public Model3dClassEnum Model3dClassName { get; set; }

        [ProtoMember(3)]
        public bool IsBuiltIn { get; set; } = false;


    }
}