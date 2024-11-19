using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class WBSItem : TreeEntity
    {

    }

    [ProtoContract]
    public class WBSItemType : TreeEntityType
    {
    }


    [ProtoContract]
    public class WBSItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    public class WBSItemParentType : TreeEntityType
    {

    }

}