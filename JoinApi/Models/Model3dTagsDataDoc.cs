using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class Model3dTagsDataDoc : ModelData.Model.Model3dTagsData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
