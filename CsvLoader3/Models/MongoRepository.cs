using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;

namespace CsvLoader3.Models
{
    public abstract class MongoRepository<T> : IRepository<T>
    {
        protected abstract IMongoCollection<T> Collection { get; }

        public virtual void Create(T item)
        {
            Collection.InsertOne(item);
        }

        public void Dispose()
        {
        }

        public virtual List<T> GetAllObjectsList()
        {
            return  Collection.Find(Builders<T>.Filter.Empty).ToList();
        }
    }
}