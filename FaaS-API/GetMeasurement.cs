using FaaS.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace FaaS
{
    public static class GetMeasurement
    {
        [FunctionName("GetMeasurement")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info($"GetLog was triggered!");
            SettingsModel.Init();

            var settingsPath = Path.Combine(context.FunctionAppDirectory, "settings.json");
            var settingsModel = BsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));

            var measurementModelGet = BsonSerializer.Deserialize<MeasurementModelGet>(await req.Content.ReadAsStringAsync());

            var dbClient = new MongoClient(settingsModel.MongoConnectionString);
            var database = dbClient.GetDatabase(settingsModel.MongoDatabase);
            var measurementCollection = database.GetCollection<BsonDocument>(settingsModel.MongoMeasurementCollection);

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Gte("timestamp", measurementModelGet.StartDate) & filterBuilder.Lte("timestamp", measurementModelGet.EndDate) & filterBuilder.Eq("name", measurementModelGet.Name);

            var result = new List<BsonDocument>();

            await measurementCollection.Find(filter).ForEachAsync(dLog =>
            {
                dLog["_id"] = dLog["_id"].ToString();
                dLog["last-modified"] = dLog["last-modified"].ToString();
                result.Add(dLog);
            });

            if (result.Count != 0)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(result.ToJson(), Encoding.UTF8, JsonMediaTypeFormatter.DefaultMediaType.ToString())
                };
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, new
            {
                error = $"No logs"
            });
        }
    }
}