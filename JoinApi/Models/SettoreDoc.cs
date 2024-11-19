using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("settori")]
    public class SettoreDoc : IOggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Nome { get; set; }

        public Guid ClienteId { get; set; }
        public Guid ParentId { get => ClienteId; }

    }
}
