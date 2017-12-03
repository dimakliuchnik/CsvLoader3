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
    public class LoaderModel
    {
        public string FileName { get; set; }
        public string Header { get; set; }

        public DataTable Files { get; set; }

        public DataTable Data { get; set; }
    }
}