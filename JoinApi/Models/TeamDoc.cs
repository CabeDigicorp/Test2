using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("teams")]
    public class TeamDoc : ISoggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsLicensed { get; set; } = false;
        public Guid ClienteId { get; set; }

        public List<Guid> GruppiIds { get; set; } = new List<Guid>();
    }
}
