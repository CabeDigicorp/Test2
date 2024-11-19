using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class WBSItemsCreationDataDoc : ModelData.Model.WBSItemsCreationData
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
