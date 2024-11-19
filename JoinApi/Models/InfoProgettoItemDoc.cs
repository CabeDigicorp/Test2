
using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class InfoProgettoItemDoc : ModelData.Model.InfoProgettoItem
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }
}
