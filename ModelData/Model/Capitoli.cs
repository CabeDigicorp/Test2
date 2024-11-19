using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class CapitoliItem : TreeEntity
    {
    }

    [ProtoContract]
    public class CapitoliItemType : TreeEntityType
    {
 
     
    }


    [ProtoContract]
    public class CapitoliItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    public class CapitoliItemParentType : TreeEntityType
    {
      
    }

}