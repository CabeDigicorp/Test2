using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    [BsonKnownTypes(typeof(DivisioneItemTypeDoc))]
    [BsonKnownTypes(typeof(DivisioneItemParentTypeDoc))]
    public class EntityTypeDoc : ModelData.Model.EntityType
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

    public class DivisioneItemTypeDoc : EntityTypeDoc//, ModelData.Model.DivisioneItemType
    {
        //*************ModelData.Model.DivisioneItemType
        public Guid DivisioneId { get; set; }
        public ModelData.Model.Model3dClassEnum Model3dClassName { get; set; }
        public bool IsBuiltIn { get; set; } = false;
        public int Position { get; set; } = -1;
        //*************
    }

    public class DivisioneItemParentTypeDoc : EntityTypeDoc //ModelData.Model.DivisioneItemParentType
    {
        //**************ModelData.Model.DivisioneItemParentType
        public Guid DivisioneId { get; set; }
        public ModelData.Model.Model3dClassEnum Model3dClassName { get; set; }
        public bool IsBuiltIn { get; set; } = false;
        //**************
    }
}
