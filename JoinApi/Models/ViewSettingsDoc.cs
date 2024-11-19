using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class ViewSettingsDoc : ModelData.Model.ViewSettings
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
