using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class Model3dFiltersDataDoc : ModelData.Model.Model3dFiltersData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
