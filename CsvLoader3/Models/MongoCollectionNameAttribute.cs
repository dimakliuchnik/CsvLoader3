using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CsvLoader3.Models
{
    public sealed class MongoCollectionNameAttribute : Attribute
    {
        public string CollectionName { get; }

        public MongoCollectionNameAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}