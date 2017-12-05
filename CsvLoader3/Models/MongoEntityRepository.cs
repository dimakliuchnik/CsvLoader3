using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;

namespace CsvLoader3.Models
{
    public class MongoEntityRepository<T> : IEntityRepository<T>
    {
        protected MongoContext Context;

        protected IMongoCollection<T> Collection
        {
            get
            {
                var t = typeof(T);
                var collectionName = t.GetCustomAttribute<MongoCollectionNameAttribute>();
                if (collectionName == null)
                {
                    throw new Exception("Exception");
                }

                return Context.MongoDatabase.GetCollection<T>(collectionName.CollectionName);
            }
        }

        public MongoEntityRepository(MongoContext context)
        {
            Context = context;
        }

        public virtual void Create(T item)
        {
            Collection.InsertOne(item);
        }

        public virtual List<T> GetAllObjectsList()
        {
            return  Collection.Find(Builders<T>.Filter.Empty).ToList();
        }
    }
}