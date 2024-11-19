using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("opere")]
    public class OperaDoc : IOggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descrizione { get; set; }
        public Guid SettoreId { get; set; }
        public Guid ParentId { get => SettoreId; }
        public List<Guid> TagIds { get; set; } = new List<Guid>();
    }
}
