using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class ProgettoDoc : IOggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descrizione { get; set; }
        public Guid OperaId { get; set; }
        public Guid ParentId { get => OperaId; }
        public int ContentVersion { get; set; }
        public DateTime? ContentCreationDate { get; set; }
        public DateTime? ContentLastWriteDate { get; set; }
        public long ContentSize { get; set; }
    }
}
