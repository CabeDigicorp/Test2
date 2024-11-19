using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class GanttDataDoc : ModelData.Model.GanttData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
