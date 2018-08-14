using FaaS.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaaS
{
    public static class RemoveDevice
    {
        [FunctionName("RemoveDevice")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info($"RemoveDevice was triggered!");
            SettingsModel.Init();

            var settingsPath = Path.Combine(context.FunctionAppDirectory, "settings.json");
            var settingsModel = BsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));

            var deviceModel = BsonSerializer.Deserialize<DeviceModel>(await req.Content.ReadAsStringAsync());

            var dbClient = new MongoClient(settingsModel.MongoConnectionString);
            var database = dbClient.GetDatabase(settingsModel.MongoDatabase);
            var deviceCollection = database.GetCollection<DeviceModel>(settingsModel.MongoDeviceCollection);

            var deleteResult = await deviceCollection.DeleteOneAsync(
                       Builders<DeviceModel>.Filter.Eq("_id", deviceModel.Name));

            if (deleteResult.DeletedCount != 0)
            {
                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    success = deleteResult
                });
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, new
            {
                error = deleteResult
            });
        }
    }

}
