﻿using MongoDB.Bson.Serialization.Attributes;

namespace FaaS.Models
{
    class LogModelGet
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("startDate")]
        public long? StartDate { get; set; }

        [BsonElement("endDate")]
        public long? EndDate { get; set; }

    }
}
