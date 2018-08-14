using MongoDB.Bson.Serialization.Attributes;

namespace FaaS.Models
{
    class DeviceModel
    {
        [BsonElement("name")]
        public string Name { get; set; }
    }
}
