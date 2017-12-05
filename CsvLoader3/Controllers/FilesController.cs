using CsvLoader3.Models;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
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
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            FilesModel filesModel = new FilesModel();

            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    if (upload.FileName.EndsWith(".csv"))
                    {
                        string path = Path.Combine(Server.MapPath("~/Images"),
                            Path.GetFileName(upload.FileName));
                        upload.SaveAs(path);
                        ViewBag.Message = "File uploaded successfully";

                        LoadCsvAndFillLoaderModel(upload, filesModel);
                        _filesRepository.Create(filesModel);

                        List<FilesModel> models = _filesRepository.GetAllObjectsList();

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
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            return View();
        }

        private static void LoadCsvAndFillLoaderModel(HttpPostedFileBase upload, FilesModel filesModel)
        {
            Stream stream = upload.InputStream;
            DataTable data = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(stream), true))
            {
                data.Load(csvReader);
            }

            filesModel.Header = "";
            foreach (var column in data.Columns)
            {
                filesModel.Header = string.Concat(filesModel.Header, "\n", column);
            }
            filesModel.Header = filesModel.Header.Trim();
            filesModel.FileName = upload.FileName;
        }
    }
}