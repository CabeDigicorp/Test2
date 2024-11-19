using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    public interface ISoggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }

        public string? Nome { get; set; }

    }
}
