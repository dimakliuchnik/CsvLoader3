using CsvLoader3.Models;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace CsvLoader3.Controllers
{
    public class FilesController : Controller
    {
        private readonly IEntityRepository<FilesModel> _filesRepository;

        public FilesController(IUnitOfWork unitOfWork)
        {
            _filesRepository = unitOfWork.GetRepository<FilesModel>();
        }

        // GET: Files
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UploadFile()
        {
            foreach (string file in Request.Files)
            {
                var FileDataContent = Request.Files[file];
                if (FileDataContent != null && FileDataContent.ContentLength > 0)
                {
                    // take the input stream, and save it to a temp folder using the original file.part name posted
                    var stream = FileDataContent.InputStream;
                    var fileName = Path.GetFileName(FileDataContent.FileName);
                    var UploadPath = Server.MapPath("~/App_Data/uploads");
                    Directory.CreateDirectory(UploadPath);
                    string path = Path.Combine(UploadPath, fileName);
                    try
                    {
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                        // Once the file part is saved, see if we have enough to merge it
                        Utils UT = new Utils();
                        bool result = UT.MergeFile(path);

                        if (result)
                        {
                            string partToken = ".part_";
                            string baseFileName = path.Substring(0, path.IndexOf(partToken));
                            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
                            string[] filesList = Directory.GetFiles(Path.GetDirectoryName(path), Searchpattern);
                            foreach (var s in filesList)
                            {
                                try
                                {
                                    System.IO.File.Delete(s);
                                }
                                catch (IOException e)
                                {
                                }

                            }
                            return RedirectToAction("Upload", "Files");
                        }
                    }
                    catch (IOException ex)
                    {
                        // handle
                    }
                }
            }
            return Redirect(Request.UrlReferrer.ToString());
            //return new HttpResponseMessage()
            //{
            //    StatusCode = System.Net.HttpStatusCode.OK,
            //    Content = new StringContent("File uploaded.")
            //};

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            FilesModel filesModel = new FilesModel();

            if (ModelState.IsValid)
            {
                if ( file != null && file.FileName != "")
                {
                    if (file.FileName.EndsWith(".csv"))
                    {
                        string path = Path.Combine(Server.MapPath("~/Images"),
                            Path.GetFileName(file.FileName));
                        //file.SaveAs(path);
                        ViewBag.Message = "File uploaded successfully";

                        LoadCsvAndFillLoaderModel(file, filesModel);
                        List<FilesModel> models = _filesRepository.GetAllObjectsList();

                        bool alreadyLoaded = false;
                        foreach (var model in models)
                        {
                            if (model.FileName == filesModel.FileName && model.Header == filesModel.Header)
                            {
                                alreadyLoaded = true;
                                break;

                            }
                        }
                        if (alreadyLoaded)
                        {
                            ModelState.AddModelError("File", "This file has been already loaded");
                            return View(models);
                        }

                        _filesRepository.Create(filesModel);
                        models.Add(filesModel);
                        //List<FilesModel> models = _filesRepository.GetAllObjectsList();

                        return View(models);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("File", "Please choose Your file");
                    List<FilesModel> models = _filesRepository.GetAllObjectsList();
                    return View(models);
                }
            }
            return View();
        }

        private static void LoadCsvAndFillLoaderModel(HttpPostedFileBase file, FilesModel filesModel)
        {
            Stream stream = file.InputStream;
            using (var csvReader = new CsvReader(new StreamReader(stream), true))
            {
                var headers = csvReader.GetFieldHeaders();
                filesModel.Header = "";
                foreach (var column in headers)
                {
                    filesModel.Header = string.Concat(filesModel.Header, "\n", column);
                }
                filesModel.Header = filesModel.Header.Trim();
                filesModel.FileName = file.FileName;
            }
        }
    }

   
}