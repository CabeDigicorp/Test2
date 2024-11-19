using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class Model3dPreferencesDataDoc : ModelData.Model.Model3dPreferencesData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
