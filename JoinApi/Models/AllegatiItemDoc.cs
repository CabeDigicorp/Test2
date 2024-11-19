using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class AllegatiItemDoc :ModelData.Model.AllegatiItem
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;

        //Moved to parent
        //public string FileName { get; set; }
        public OrderPosition Position { get; set; } = new OrderPosition(0, 1);
    }
}
