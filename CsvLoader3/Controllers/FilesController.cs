using CsvLoader3.Models;
using System.Collections.Generic;
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
        [HttpGet]
        public ActionResult Upload()
        {
            List<FilesModel> models = _filesRepository.GetAllObjectsList();
            return View(models);
        }

        public ActionResult UploadFile()
        {
            var files = Request.Files;
            var server = Server;
            Utils.ProcessWithFileLoading(files, server);
            return new ViewResult();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            List<FilesModel> models = _filesRepository.GetAllObjectsList();
            if (ModelState.IsValid && file != null)
            {
                if (string.IsNullOrEmpty(file.FileName))
                {
                    ModelState.AddModelError("File", "Please choose Your file.");
                    return View(models);
                }

                if (!file.FileName.EndsWith(".csv"))
                {
                    ModelState.AddModelError("File", "This file format is not supported.");
                    return View(models);
                }

                FilesModel filesModel = Utils.LoadCsvHeaderAndFillLoaderModel(file);

                if (filesModel == null)
                {
                    ModelState.AddModelError("File", "Header haven't been read correctly.");
                    return View(models);
                }

                if (Utils.CheckFileIsAlreadyLoaded(models, filesModel))
                {
                    ModelState.AddModelError("File", "This file has been already loaded.");
                    return View(models);
                }

                _filesRepository.Create(filesModel);
                models.Add(filesModel);
                ModelState.AddModelError("File", "File uploaded successfully.");
                return View(models);
            }
            models = _filesRepository.GetAllObjectsList();
            return View(models);
        }
    }
}