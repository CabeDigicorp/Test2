using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class Model3dUserRotoTranslationDoc : ModelData.Model.Model3dUserRotoTranslation
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
