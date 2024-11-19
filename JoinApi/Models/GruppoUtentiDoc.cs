using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("gruppiUtenti")]
    public class GruppoUtentiDoc : ISoggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Nome { get; set; }

        public Guid OperaId { get; set; }

        //public List<Guid> UtentiIds { get; set; } = new List<Guid>();
        
        //public List<Guid> TeamIds { get; set; } = new List<Guid>();
    }
}
