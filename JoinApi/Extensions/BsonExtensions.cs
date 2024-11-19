using MongoDB.Bson;

namespace JoinApi.Extensions
{
    public static class BsonExtensions
    {
        public static BsonBinaryData ToBsonBinaryData(this Guid guid)
        {
            return new BsonBinaryData(guid, GuidRepresentation.Standard);
        }

    }
}
