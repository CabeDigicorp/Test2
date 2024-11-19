using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    public interface IOggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid ParentId { get; }


    }
}
