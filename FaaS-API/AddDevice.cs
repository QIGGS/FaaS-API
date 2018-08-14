using FaaS.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaaS
{
    public static class AddDevice
    {

        [FunctionName("AddDevice")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info($"AddDevice was triggered!");
            SettingsModel.Init();

            var settingsPath = Path.Combine(context.FunctionAppDirectory, "settings.json");
            var settingsModel = BsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));

            log.Info(settingsModel.ToJson());

            var deviceModel = BsonSerializer.Deserialize<DeviceModel>(await req.Content.ReadAsStringAsync());

            var dbClient = new MongoClient(settingsModel.MongoConnectionString);
            var database = dbClient.GetDatabase(settingsModel.MongoDatabase);
            var deviceCollection = database.GetCollection<DeviceModel>(settingsModel.MongoDeviceCollection);

            var indexOptions = new CreateIndexOptions() { Unique = true };
            var keysDefinition = new IndexKeysDefinitionBuilder<DeviceModel>().Ascending(d => d.Name);
            var deviceIndexModel = new CreateIndexModel<DeviceModel>(keysDefinition, indexOptions);

            await deviceCollection.Indexes.CreateOneAsync(deviceIndexModel);

            try
            {
                await deviceCollection.InsertOneAsync(deviceModel);
            }
            catch (MongoWriteException e)
            {
                log.Error($"Duplicate name value ({deviceModel.ToJson()},{e})");

                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    error = $"Device name {deviceModel.Name} already exists!"
                });
            }

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                success = $"{deviceModel.Name} device imported"
            });
        }
    }
}

