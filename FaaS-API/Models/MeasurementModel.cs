using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FaaS.Models
{


    class MeasurementModel
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

        [BsonElement("start-date")]
        public long? StartDate { get; set; }

        [BsonElement("end-date")]
        public long? EndDate { get; set; }

        [BsonElement("last-modified")]
        public DateTime LastModified { get { return DateTime.Now; } }
    }
}

