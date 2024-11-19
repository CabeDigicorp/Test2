using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class TagDoc
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public Guid ClienteId { get; set; }
    }
}
