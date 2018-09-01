using MongoDB.Bson.Serialization.Attributes;

namespace FaaS.Models
{
    class LogModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("timestamp")]
        public long TimeStamp { get; set; }

        [BsonElement("value")]
        public string Value { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("sensor-name")]
        public string SensorName { get; set; }
    }
}

