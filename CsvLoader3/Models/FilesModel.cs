using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace CsvLoader3.Models
{
    [MongoCollectionName("Files")]
    public class FilesModel
    {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("Name")]
        public string FileName { get; set; }

        [BsonElement("Header")]
        public string Header { get; set; }
    }
}