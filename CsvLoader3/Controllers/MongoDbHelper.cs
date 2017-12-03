using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CsvLoader3.Controllers
{
    public class MongoDbHelper
    {
        public IMongoDatabase CreateConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("test");
            var isMongoLive = database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
            if (isMongoLive)
            {
                return database;
            }
            return null;
        }

        public async Task SaveDataTableToCollection(IMongoDatabase database, DataTable dt, string name)
        {
            var collection = database.GetCollection<BsonDocument>(name);

            List<BsonDocument> batch = new List<BsonDocument>();
            foreach (DataRow dr in dt.Rows)
            {
                var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);
                batch.Add(new BsonDocument(dictionary));
            }

            await collection.InsertManyAsync(batch.AsEnumerable());
        }

        public async Task SaveObjectToCollection(IMongoDatabase database, Dictionary<string, object> obj, string name)
        {
            var collection = database.GetCollection<BsonDocument>(name);
            await collection.InsertOneAsync(new BsonDocument(obj));
        }
    }
}