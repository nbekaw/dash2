using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class Request
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("House")]
    public HouseRef House { get; set; }

    [BsonElement("RequestStatus")]
    public RequestStatusRef RequestStatus { get; set; }

    [BsonElement("Deadline")]
    public DateTime? Deadline { get; set; }

    [BsonIgnore]
    public string HouseId => House?.ID;

    [BsonIgnore]
    public string RequestStatusId => RequestStatus?.ID;
}

public class HouseRef
{
    [BsonElement("ID")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }
}

public class RequestStatusRef
{
    [BsonElement("ID")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }
}