using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class Model3dValuesDataDoc : ModelData.Model.Model3dValuesData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
