using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class FogliDiCalcoloDataDoc : ModelData.Model.FogliDiCalcoloData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
