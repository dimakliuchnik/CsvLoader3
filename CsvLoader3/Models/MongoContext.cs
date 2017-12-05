using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MongoDB.Driver;

namespace CsvLoader3.Models
{
    public class MongoContext : System.Data.Entity.DbContext
    {
        public MongoContext()
            : this("MongoDb")
        {
        }

        public MongoContext(string connectionName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;

            IMongoClient client = new MongoClient(connectionString);
            MongoDatabase = client.GetDatabase("test");
        }

        public IMongoDatabase MongoDatabase { get; }
    }
}