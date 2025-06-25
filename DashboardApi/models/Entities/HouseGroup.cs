using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class HouseGroup
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("Organization")]
    public OrganizationRef Organization { get; set; }

    [BsonIgnore]
    public string OrganizationId => Organization?.ID;

    [BsonElement("Houses")]
    public List<string> Houses { get; set; }

    [BsonElement("Name")]
    public Name Name { get; set; }

}

public class OrganizationRef
{
    [BsonElement("ID")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }
}

public class Name
{
    [BsonElement("Kk")]
    public string? Kk { get; set; }

    [BsonElement("Ru")]
    public string Ru { get; set; }

    [BsonElement("En")]
    public string? En { get; set; }
}
