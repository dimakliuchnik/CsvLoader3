using CsvLoader3.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
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
        public ActionResult Upload(string fileNameParameter)
        {

            if (fileNameParameter != null)
            {
                List<FilesModel> temp = _filesRepository.GetAllObjectsList();

                if(!temp.Select(_ => _.FileName).Contains(fileNameParameter))
                _filesRepository.Create(Utils.LoadCsvHeaderAndFillLoaderModel(fileNameParameter));
                return RedirectToAction("Upload");
            }

            List<FilesModel> models = _filesRepository.GetAllObjectsList();
            return View(models);
        }

        
        [HttpPost]
        public ActionResult Upload()
        {
            string fileName = "";
            var files = Request.Files;
            var server = Server;
            bool result;
            fileName = Utils.ProcessWithFileLoading(files, server, out result);
            List<FilesModel> temp = _filesRepository.GetAllObjectsList();
            if (!temp.Select(_ => _.FileName).Contains(fileName) && result)
                _filesRepository.Create(Utils.LoadCsvHeaderAndFillLoaderModel(fileName));


            if (result)
            {
                return RedirectToAction("Upload", "Files", new { fileNameParameter = fileName });
            }
            else
            {
                return new EmptyResult();
                
                return Redirect(Request.UrlReferrer.ToString());
            }
        }




        //    public ActionResult UploadFile()
        //    {
        //        var files = Request.Files;
        //        var server = Server;
        //        Utils.ProcessWithFileLoading(files, server);
        //        return new ViewResult();
        //    }

        //    [HttpPost]
        //    public ActionResult Upload(HttpPostedFileBase file)
        //    {
        //        List<FilesModel> models = _filesRepository.GetAllObjectsList();
        //        if (ModelState.IsValid && file != null)
        //        {
        //            if (string.IsNullOrEmpty(file.FileName))
        //            {
        //                ModelState.AddModelError("File", "Please choose Your file.");
        //                return View(models);
        //            }

        //            if (!file.FileName.EndsWith(".csv"))
        //            {
        //                ModelState.AddModelError("File", "This file format is not supported.");
        //                return View();
        //            }

        //            ViewBag.Message = "File uploaded successfully.";

        //            FilesModel filesModel = Utils.LoadCsvHeaderAndFillLoaderModel(file);

        //            if (filesModel == null)
        //            {
        //                ModelState.AddModelError("File", "Header haven't been read correctly.");
        //                return View(models);
        //            }

        //            if (Utils.CheckFileIsAlreadyLoaded(models, filesModel))
        //            {
        //                ModelState.AddModelError("File", "This file has been already loaded.");
        //                return View(models);
        //            }

        //            _filesRepository.Create(filesModel);
        //            models.Add(filesModel);

        //            return View(models);
        //        }
        //        models = _filesRepository.GetAllObjectsList();
        //        return View(models);
        //    }
    }
}