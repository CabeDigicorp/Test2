

using ProtoBuf;
using System.Runtime.Serialization;

namespace ModelData.Model
{

    [ProtoContract]
    public class Entity
    {

        [ProtoMember(1)]
        public Guid EntityId { get; set; } = Guid.Empty;

        [ProtoMember(2)]
        public Dictionary<string, EntityAttributo> Attributi { get; set; } = new Dictionary<string, EntityAttributo>();

        [ProtoMember(4)]
        public string EntityTypeCodice { get; set; }

        [ProtoMember(5)]
        public string HighlighterColorName { get; set; } = "Transparent";

    }

    [ProtoContract]
    public class EntityCollection
    {
        [ProtoMember(1)]
        public List<Entity> Entities { get; set; }

    }


    public enum UserIdentityMode
    {
        Nothing = 0,
        Deep,
        SingleLine,
    }



}