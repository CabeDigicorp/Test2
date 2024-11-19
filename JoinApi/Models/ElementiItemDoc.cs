using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class ElementiItemDoc :ModelData.Model.ElementiItem
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
        public OrderPosition Position { get; set; } = new OrderPosition(0, 1);
    }
}
