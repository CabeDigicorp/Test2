using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class DefinizioneAttributoDoc : ModelData.Model.DefinizioneAttributo
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;

    }
}
