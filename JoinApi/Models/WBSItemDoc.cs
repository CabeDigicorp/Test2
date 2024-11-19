
using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class WBSItemDoc : ModelData.Model.WBSItem, IOggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
        public Guid ParentId { get => ProgettoId; }
        public OrderPosition Position { get; set; } = new OrderPosition(0, 1);
    }
}
