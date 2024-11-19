using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class Model3dUserViewListDoc : ModelData.Model.Model3dUserViewList
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
