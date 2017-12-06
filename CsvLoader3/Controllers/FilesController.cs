using CsvLoader3.Models;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

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
        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UploadFile()
        {
            string baseFileName = "";
            foreach (string file in Request.Files)
            {
                var fileDataContent = Request.Files[file];
                if (fileDataContent != null && fileDataContent.ContentLength > 0)
                {
                    // take the input stream, and save it to a temp folder using the original file.part name posted
                    var stream = fileDataContent.InputStream;
                    var fileName = Path.GetFileName(fileDataContent.FileName);
                    var uploadPath = Server.MapPath("~/App_Data/uploads");
                    Directory.CreateDirectory(uploadPath);
                    string path = Path.Combine(uploadPath, fileName);
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
                            baseFileName = path.Substring(0, path.IndexOf(partToken));
                            string searchPattern = Path.GetFileName(baseFileName) + partToken + "*";
                            string[] filesList = Directory.GetFiles(Path.GetDirectoryName(path), searchPattern);
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
                        }
                    }
                    catch (IOException ex)
                    {
                        // handle
                    }
                }

            }
            var parameters = new RouteValueDictionary { { "file", baseFileName } };

            return RedirectToAction("Upload", "Files", parameters);

            //return new HttpResponseMessage()
            //{
            //    StatusCode = System.Net.HttpStatusCode.OK,
            //    Content = new StringContent("File uploaded.")
            //};
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Upload(string file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file != "")
                {
                    if (file.EndsWith(".csv"))
                    {
                        string path = Path.Combine(Server.MapPath("~/Images"),
                            Path.GetFileName(file));
                        //file.SaveAs(path);
                        ViewBag.Message = "File uploaded successfully";

                        FilesModel filesModel = LoadCsvAndFillLoaderModel(file);
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

        private static FilesModel LoadCsvAndFillLoaderModel(string file)
        {
            FilesModel filesModel = new FilesModel();
            using (StreamReader sr = new StreamReader(file))
            {
                using (var csvReader = new CsvReader(sr, true))
                {
                    var headers = csvReader.GetFieldHeaders();
                    filesModel.Header = "";
                    foreach (var column in headers)
                    {
                        filesModel.Header = string.Concat(filesModel.Header, "\n", column);
                    }
                    filesModel.Header = filesModel.Header.Trim();
                    filesModel.FileName = file;
                }
            }
            return filesModel;
        }
    }
}