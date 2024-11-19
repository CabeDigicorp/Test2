
using ProtoBuf;


namespace ModelData.Model
{
    [ProtoContract]
    public class AllegatiItem : Entity
    {
        [ProtoMember(1)]
        public string FileName { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class AllegatiItemType : EntityType
    {

    }


    [ProtoContract]
    public class AllegatiItemCollection : EntityCollection
    {
    }




}