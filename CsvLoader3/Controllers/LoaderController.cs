using CsvLoader3.Models;
using LumenWorks.Framework.IO.Csv;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CsvLoader3.Controllers
{
    public class LoaderController : Controller
    {
        private readonly MongoDbHelper _mongoDbHelper = new MongoDbHelper();
        private const string Files = "Files";

        // GET: Loader
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(HttpPostedFileBase upload)
        {
            LoaderModel loaderModel = new LoaderModel();
           
            if (ModelState.IsValid)
            {
                if (upload.ContentLength > 0)
                {
                    if (upload.FileName.EndsWith(".csv"))
                    {
                        LoadCsvAndFillLoaderModel(upload, loaderModel);

                        IMongoDatabase database = _mongoDbHelper.CreateConnection();

                        await _mongoDbHelper.SaveDataTableToCollection(database, loaderModel.Data, upload.FileName);


                        await PutNewFileObjectToDb(loaderModel, database);

                         LoadDbFileObjects(loaderModel, database);

                        return View(loaderModel);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            return View();
        }

        private static void LoadCsvAndFillLoaderModel(HttpPostedFileBase upload, LoaderModel loaderModel)
        {
            Stream stream = upload.InputStream;
            loaderModel.Data = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(stream), true))
            {
                loaderModel.Data.Load(csvReader);
            }

            loaderModel.Header = "";
            foreach (var column in loaderModel.Data.Columns)
            {
                loaderModel.Header = string.Concat(loaderModel.Header, "\n", column);
            }
            loaderModel.Header = loaderModel.Header.Trim();
            loaderModel.FileName = upload.FileName;
        }

        private static void LoadDbFileObjects(LoaderModel loaderModel, IMongoDatabase database)
        {
            var collection = database.GetCollection<BsonDocument>(Files);
            var cursor = collection.Find(new BsonDocument()).ToCursor();
            loaderModel.Files = new DataTable();
            loaderModel.Files.Columns.Add("Id", typeof(string));
            loaderModel.Files.Columns.Add("Name", typeof(string));
            loaderModel.Files.Columns.Add("Header", typeof(string));
            foreach (var document in cursor.ToEnumerable())
            {
                var documentElements = document.Elements.ToDictionary(x => x.Name, y => y.Value);
                var values = documentElements.Select(_ => (object) _.Value).ToArray();
                loaderModel.Files.Rows.Add(values);
            }
        }

        private async Task PutNewFileObjectToDb(LoaderModel loaderModel, IMongoDatabase database)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();
            obj.Add("Name", loaderModel.FileName);
            obj.Add("Header", loaderModel.Header);
            await _mongoDbHelper.SaveObjectToCollection(database, obj, Files);
        }
    }
}