
using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class DocumentiItem : TreeEntity
    {
    }
    [ProtoContract]
    public class DocumentiItemType : TreeEntityType
    {
    }


    [ProtoContract]
    public class DocumentiItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    public class DocumentiItemParentType : TreeEntityType
    {

    }

}
