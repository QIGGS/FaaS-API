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
    public static class GetLog
    {
        [FunctionName("GetLog")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info($"GetLog was triggered!");
            SettingsModel.Init();

            var settingsPath = Path.Combine(context.FunctionAppDirectory, "settings.json");
            var settingsModel = BsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));

            var LogModelGet = BsonSerializer.Deserialize<LogModelGet>(await req.Content.ReadAsStringAsync());

            var dbClient = new MongoClient(settingsModel.MongoConnectionString);
            var database = dbClient.GetDatabase(settingsModel.MongoDatabase);
            var logCollection = database.GetCollection<BsonDocument>(settingsModel.MongoLogCollection);

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Gte("timestamp", LogModelGet.StartDate) & filterBuilder.Lte("timestamp", LogModelGet.EndDate) & filterBuilder.Eq("name", LogModelGet.Name);

            var logs = new List<BsonDocument>();


            await logCollection.Find(filter).ForEachAsync(dLog =>
            {
                dLog["_id"] = dLog["_id"].ToString();
                log.Info(dLog.ToString());
                logs.Add(dLog);
            });


            if (logs.Count != 0)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(logs.ToJson(), Encoding.UTF8, JsonMediaTypeFormatter.DefaultMediaType.ToString())
                };
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, new
            {
                error = $"No logs"
            });
        }
    }
}
