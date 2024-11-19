
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("permessi")]
    public class PermessoDoc
    {
        [BsonId]
        public Guid Id { get; set; }

        //public string? Nome { get; set; }

        public Guid SoggettoId { get; set; }

        public string OggettoTipo { get; set; }
        public Guid OggettoId { get; set; }

        public List<Guid> RuoliIds { get; set; }


    }

}
