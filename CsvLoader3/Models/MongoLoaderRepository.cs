using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace CsvLoader3.Models
{
    public class MongoLoaderRepository : MongoRepository<FilesModel>
    {
        private readonly DbContext _context = new DbContext("MongoDB");
        private const string CollectionName = "Files";

        protected override IMongoCollection<FilesModel> Collection =>
            _context.MongoDatabase.GetCollection<FilesModel>(CollectionName);
    }
}