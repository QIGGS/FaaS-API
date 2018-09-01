using FaaS.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaaS
{
    public static class AddMeasurement
    {
        [FunctionName("AddMeasurement")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info($"AddMeasurement was triggered!");
            SettingsModel.Init();

            var settingsPath = Path.Combine(context.FunctionAppDirectory, "settings.json");
            var settingsModel = BsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));

            var measurementModel = BsonSerializer.Deserialize<List<MeasurementModel>>(await req.Content.ReadAsStringAsync());

            var dbClient = new MongoClient(settingsModel.MongoConnectionString);
            var database = dbClient.GetDatabase(settingsModel.MongoDatabase);
            var measurementCollection = database.GetCollection<MeasurementModel>(settingsModel.MongoMeasurementCollection);

            var indexOptions = new CreateIndexOptions() { ExpireAfter = new TimeSpan(settingsModel.ExpireAfterDays, settingsModel.ExpireAfterHours, settingsModel.ExpireAfterMinutes, settingsModel.ExpireAfterSeconds) };
            var keysDefinition = new IndexKeysDefinitionBuilder<MeasurementModel>().Ascending(m => m.LastModified);
            var measurementIndexModel = new CreateIndexModel<MeasurementModel>(keysDefinition, indexOptions);

            await measurementCollection.Indexes.CreateOneAsync(measurementIndexModel);

            int dCount = measurementModel.Count;

            if (dCount == 0)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    error = $"Imported {dCount} {settingsModel.MongoMeasurementCollection}"
                });
            }

            measurementModel.ForEach(async device => await measurementCollection.InsertOneAsync(device));

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                success = $"Imported {dCount} {settingsModel.MongoMeasurementCollection}"
            });
        }
    }
}
