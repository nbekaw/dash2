using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class DictRequestStatus
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("Code")]
    public string Code { get; set; }

    [BsonElement("Name")]
    public LocalizedString Name { get; set; }

    [BsonElement("Active")]
    public bool Active { get; set; }

    [BsonElement("UseInDashboard")]
    public bool UseInDashboard { get; set; }
}

public class LocalizedString
{
    [BsonElement("Kk")]
    public string Kk { get; set; }

    [BsonElement("Ru")]
    public string Ru { get; set; }

    [BsonElement("En")]
    public string En { get; set; }
}