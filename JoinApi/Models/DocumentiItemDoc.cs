﻿using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class DocumentiItemDoc : ModelData.Model.DocumentiItem
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ProgettoId { get; set; } = Guid.Empty;
        public OrderPosition Position { get; set; } = new OrderPosition(0, 1);
    }
}