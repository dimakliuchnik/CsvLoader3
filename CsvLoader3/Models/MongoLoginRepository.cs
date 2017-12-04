using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace CsvLoader3.Models
{
    public class MongoLoginRepository : MongoRepository<LoginModel>
    {
        private readonly DbContext _context = new DbContext("MongoDB");
        private const string CollectionName = "LoginPassword";

        protected override IMongoCollection<LoginModel> Collection =>
            _context.MongoDatabase.GetCollection<LoginModel>(CollectionName);
    }
}