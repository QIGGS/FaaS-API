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

    public static class AddLog
    {
        [FunctionName("AddLog")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info($"AddLog was triggered!");
            SettingsModel.Init();

            var settingsPath = Path.Combine(context.FunctionAppDirectory, "settings.json");
            var settingsModel = BsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(settingsPath));

            var logModel = BsonSerializer.Deserialize<List<LogModel>>(await req.Content.ReadAsStringAsync());

            var dbClient = new MongoClient(settingsModel.MongoConnectionString);
            var database = dbClient.GetDatabase(settingsModel.MongoDatabase);

            IMongoCollection<LogModel> logCollection;

            try
            {
                await database.CreateCollectionAsync(settingsModel.MongoLogCollection, new CreateCollectionOptions
                {
                    Capped = settingsModel.CollectionCapped,
                    MaxSize = settingsModel.CollectionMaxSize,
                    MaxDocuments = settingsModel.CollectionMaxDocuments,
                });
            }
            catch (Exception)
            {
                log.Error("logs Collection already exists -> skip creation");
            }
            finally
            {
                logCollection = database.GetCollection<LogModel>(settingsModel.MongoLogCollection);
            }


            int dCount = logModel.Count;

            if (dCount == 0)
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    error = $"Imported {dCount} {settingsModel.MongoLogCollection}"
                });


            logModel.ForEach(async logs => await logCollection.InsertOneAsync(logs));

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                success = $"Imported {dCount} {settingsModel.MongoLogCollection}"
            });


        }

    }
}




