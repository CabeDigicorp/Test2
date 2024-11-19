
using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class VariabiliItemDoc : ModelData.Model.VariabiliItem
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }
}
