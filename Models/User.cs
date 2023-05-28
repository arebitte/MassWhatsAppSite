
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Login { get; set; }

    public string Password { get; set; }

    public string WhatsAppNumber { get; set; }

    public string Key { get; set; }
}
