using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Conventions;

namespace FaaS.Models
{
    class SettingsModel
    {
        public string MongoConnectionString { get; set; }
        public string MongoDatabase { get; set; }
        public string MongoDeviceCollection { get; set; }
        public string MongoLogCollection { get; set; }
        public string MongoMeasurementCollection { get; set; }

        public int ExpireAfterDays { get; set; }
        public int ExpireAfterHours { get; set; }
        public int ExpireAfterMinutes { get; set; }
        public int ExpireAfterSeconds { get; set; }

        public bool CollectionCapped { get; set; }
        public int CollectionMaxSize { get; set; }
        public int CollectionMaxDocuments { get; set; }

        //https://github.com/Azure/azure-functions-host/issues/586
        public static void Init()
        {
            JsonWriterSettings.Defaults.Indent = true;
            JsonWriterSettings.Defaults.OutputMode = JsonOutputMode.Strict;

            ConventionRegistry.Register(
            "Ignore null and extra values",
            new ConventionPack
            {
                new IgnoreIfDefaultConvention(true),
                new IgnoreExtraElementsConvention(true)
            },
            t => true);
        }
    }
}
