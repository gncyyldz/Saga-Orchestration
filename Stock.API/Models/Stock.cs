using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Stock.API.Models
{
    public class Stock
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        [BsonElement(Order = 0)]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement(Order = 1)]
        public int ProductId { get; set; }
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement(Order = 2)]
        public int Count { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement(Order = 3)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
