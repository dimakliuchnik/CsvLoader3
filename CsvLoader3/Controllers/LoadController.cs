//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;

//namespace CsvLoader3.Controllers
//{
//    public class LoadController : Controller
//    {
//        // GET: Load
//        public ActionResult UploadFile()
//        {
//            string fileName = "";
//            var files = Request.Files;
//            var server = Server;
//            bool result;
//            fileName = Utils.ProcessWithFileLoading(files, server, out result);

//            if (result)
//            {
//                return RedirectToAction("Upload", "Files", new { fileNameParameter = fileName });
//            }
//            else
//            {

//                return Content("Hi Vithal Wadje");
//            }
//        }
//    }
//}