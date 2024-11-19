using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("azioni")]
    public class AzioneDoc
    {
        [BsonId]
        public Guid Id { get; set; }

        public string? Nome { get; set; }

        public List<Guid> RuoliIds { get; set; } = new List<Guid>();
    }



}
