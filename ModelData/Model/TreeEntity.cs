

using ProtoBuf;

namespace ModelData.Model
{

    [ProtoContract]
    public /*abstract*/ class TreeEntityType : EntityType
    {

    }

    [ProtoContract]
    public class TreeEntity : Entity
    {
        public string HierarchySeparator { get; } = "\\";

        /// <summary>
        /// Profondità del nodo (-1 = rootNode non visibile)
        /// </summary>
        [ProtoMember(1)]
        public int Depth { get; set; } = 0;


        [ProtoMember(2)]
        public string ParentEntityTypeCodice { get; set; }

    }

    [ProtoContract]
    public class TreeEntityCollection
    {
        [ProtoMember(1)]
        public List<TreeEntity> TreeEntities { get; set; }


    }


}