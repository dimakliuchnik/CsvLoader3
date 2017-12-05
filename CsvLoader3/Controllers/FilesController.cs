using System;
using CsvLoader3.Models;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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




        //// generic file post method - use in MVC or WebAPI
        //[HttpPost]
        //public HttpResponseMessage Upload()
        //{
        //    foreach (string file in Request.Files)
        //    {
        //        var FileDataContent = Request.Files[file];
        //        if (FileDataContent != null && FileDataContent.ContentLength > 0)
        //        {
        //            // take the input stream, and save it to a temp folder using the original file.part name posted
        //            var stream = FileDataContent.InputStream;
        //            var fileName = Path.GetFileName(FileDataContent.FileName);
        //            var UploadPath = Server.MapPath("~/App_Data/uploads");
        //            Directory.CreateDirectory(UploadPath);
        //            string path = Path.Combine(UploadPath, fileName);
        //            try
        //            {
        //                if (System.IO.File.Exists(path))
        //                    System.IO.File.Delete(path);
        //                using (var fileStream = System.IO.File.Create(path))
        //                {
        //                    stream.CopyTo(fileStream);
        //                }
        //                // Once the file part is saved, see if we have enough to merge it
        //                Utils UT = new Utils();
        //                UT.MergeFile(path);
        //            }
        //            catch (IOException ex)
        //            {
        //                // handle
        //            }
        //        }
        //    }
        //    return new HttpResponseMessage()
        //    {
        //        StatusCode = System.Net.HttpStatusCode.OK,
        //        Content = new StringContent("File uploaded.")
        //    };
        //}


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile()
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
                        UT.MergeFile(path);
                    }
                    catch (IOException ex)
                    {
                        // handle
                    }
                }
            }
            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("File uploaded.")
            };












            //FilesModel filesModel = new FilesModel();

            //if (ModelState.IsValid)
            //{
            //    if (upload != null && upload.ContentLength > 0)
            //    {
            //        if (upload.FileName.EndsWith(".csv"))
            //        {
            //            string path = Path.Combine(Server.MapPath("~/Images"),
            //                Path.GetFileName(upload.FileName));
            //            upload.SaveAs(path);
            //            ViewBag.Message = "File uploaded successfully";

            //            LoadCsvAndFillLoaderModel(upload, filesModel);
            //            _filesRepository.Create(filesModel);

            //            List<FilesModel> models = _filesRepository.GetAllObjectsList();

            //            return View(models);
            //        }
            //        else
            //        {
            //            ModelState.AddModelError("File", "This file format is not supported");
            //            return View();
            //        }
            //    }
            //    else
            //    {
            //        ModelState.AddModelError("File", "Please Upload Your file");
            //        List<FilesModel> models = _filesRepository.GetAllObjectsList();
            //        return View(models);
            //    }
            //}
            //return View();
        }

        private static void LoadCsvAndFillLoaderModel(HttpPostedFileBase upload, FilesModel filesModel)
        {
            Stream stream = upload.InputStream;
            using (var csvReader = new CsvReader(new StreamReader(stream), true))
            {
                var headers = csvReader.GetFieldHeaders();
                filesModel.Header = "";
                foreach (var column in headers)
                {
                    filesModel.Header = string.Concat(filesModel.Header, "\n", column);
                }
                filesModel.Header = filesModel.Header.Trim();
                filesModel.FileName = upload.FileName;
            }
        }
    }



    class Utils
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<String> FileParts { get; set; }

        public Utils()
        {
            FileParts = new List<string>();
        }

        /// <summary>
        /// original name + ".part_N.X" (N = file part number, X = total files)
        /// Objective = enumerate files in folder, look for all matching parts of split file. If found, merge and return true.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool MergeFile(string FileName)
        {
            bool rslt = false;
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = FileName.Substring(0, FileName.IndexOf(partToken));
            string trailingTokens = FileName.Substring(FileName.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;
            int FileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out FileCount);
            // get a list of all file parts in the temp folder
            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
            string[] FilesList = Directory.GetFiles(Path.GetDirectoryName(FileName), Searchpattern);
            //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
            // only proceed if we have received all the file chunks
            if (FilesList.Count() == FileCount)
            {
                // use a singleton to stop overlapping processes
                if (!MergeFileManager.Instance.InUse(baseFileName))
                {
                    MergeFileManager.Instance.AddFile(baseFileName);
                    if (File.Exists(baseFileName))
                        File.Delete(baseFileName);
                    // add each file located to a list so we can get them into 
                    // the correct order for rebuilding the file
                    List<SortedFile> MergeList = new List<SortedFile>();
                    foreach (string File in FilesList)
                    {
                        SortedFile sFile = new SortedFile();
                        sFile.FileName = File;
                        baseFileName = File.Substring(0, File.IndexOf(partToken));
                        trailingTokens = File.Substring(File.IndexOf(partToken) + partToken.Length);
                        int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
                        sFile.FileOrder = FileIndex;
                        MergeList.Add(sFile);
                    }
                    // sort by the file-part number to ensure we merge back in the correct order
                    var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();
                    using (FileStream FS = new FileStream(baseFileName, FileMode.Create))
                    {
                        // merge each file chunk back into one contiguous file stream
                        foreach (var chunk in MergeOrder)
                        {
                            try
                            {
                                using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                                {
                                    fileChunk.CopyTo(FS);
                                }
                            }
                            catch (IOException ex)
                            {
                                // handle                                
                            }
                        }
                    }
                    foreach (var chunk in MergeOrder)
                    {
                       // System.IO.File.Delete(chunk.FileName);
                    }
                    rslt = true;
                    // unlock the file from singleton
                    MergeFileManager.Instance.RemoveFile(baseFileName);
                }
            }
            return rslt;
        }


    }

    public struct SortedFile
    {
        public int FileOrder { get; set; }
        public String FileName { get; set; }
    }

    public class MergeFileManager
    {
        private static MergeFileManager instance;
        private List<string> MergeFileList;

        private MergeFileManager()
        {
            try
            {
                MergeFileList = new List<string>();
            }
            catch { }
        }

        public static MergeFileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MergeFileManager();
                return instance;
            }
        }

        public void AddFile(string BaseFileName)
        {
            MergeFileList.Add(BaseFileName);
        }

        public bool InUse(string BaseFileName)
        {
            return MergeFileList.Contains(BaseFileName);
        }

        public bool RemoveFile(string BaseFileName)
        {
            return MergeFileList.Remove(BaseFileName);
        }
    }
}