using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class PrezzarioItem : TreeEntity
    {
       

    }

    [ProtoContract]
    public class PrezzarioItemType : TreeEntityType
    {
    }


    [ProtoContract]
    public class PrezzarioItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    public class PrezzarioItemParentType : TreeEntityType
    {
        
    }

}