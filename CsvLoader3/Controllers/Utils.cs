using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CsvLoader3.Models;

namespace CsvLoader3.Controllers
{
    public class Utils
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<string> FileParts { get; set; }
        const string partToken = ".part_";

        public Utils()
        {
            FileParts = new List<string>();
        }

        /// <summary>
        /// original name + ".part_N.X" (N = file part number, X = total files)
        /// Objective = enumerate files in folder, look for all matching parts of split file. If found, merge and return true.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool MergeFile(string fileName)
        {
            // parse out the different tokens from the filename according to the convention
            if (!fileName.Contains(partToken))
                return false;
            var baseFileName = fileName.Substring(0, fileName.IndexOf(partToken, StringComparison.Ordinal));
            var trailingTokens = fileName.Substring(fileName.IndexOf(partToken, StringComparison.Ordinal) + partToken.Length);
            var fileIndex = 0;
            var fileCount = 0;
            Int32.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".", StringComparison.Ordinal)), out fileIndex);
            Int32.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".", StringComparison.Ordinal) + 1), out fileCount);

            // get a list of all file parts in the temp folder
            var searchPattern = Path.GetFileName(baseFileName) + partToken + "*";
            var filesList = Directory.GetFiles(Path.GetDirectoryName(fileName), searchPattern);

            //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
            // only proceed if we have received all the file chunks
            if (filesList.Count() != fileCount || MergeFileManager.Instance.InUse(baseFileName)) return false;

            MergeFileManager.Instance.AddFile(baseFileName);
            if (File.Exists(baseFileName))
                File.Delete(baseFileName);
            // add each file located to a list so we can get them into
            // the correct order for rebuilding the file
            List<SortedFile> mergeList = new List<SortedFile>();
            foreach (var file in filesList)
            {
                SortedFile sFile = new SortedFile();
                sFile.FileName = file;
                baseFileName = file.Substring(0, file.IndexOf(partToken, StringComparison.Ordinal));
                trailingTokens = file.Substring(file.IndexOf(partToken, StringComparison.Ordinal) + partToken.Length);
                Int32.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".", StringComparison.Ordinal)), out fileIndex);
                sFile.FileOrder = fileIndex;
                mergeList.Add(sFile);
            }
            // sort by the file-part number to ensure we merge back in the correct order
            var mergeOrder = mergeList.OrderBy(s => s.FileOrder).ToList();
            using (var fs = new FileStream(baseFileName, FileMode.Create))
            {
                // merge each file chunk back into one contiguous file stream
                foreach (var chunk in mergeOrder)
                {
                    try
                    {
                        using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                        {
                            fileChunk.CopyTo(fs);
                            //File.Delete(chunk.FileName);
                        }
                    }
                    catch (IOException ex)
                    {
                        throw new IOException(ex.Message);
                    }
                }
            }

            // unlock the file from singleton
            MergeFileManager.Instance.RemoveFile(baseFileName);
            return true;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static string[] GetFileListForDeletion(string path)
        {
            var partToken = ".part_";
            var baseFileName = path.Substring(0, path.IndexOf(partToken));
            var searchPattern = Path.GetFileName(baseFileName) + partToken + "*";
            var filesList = Directory.GetFiles(Path.GetDirectoryName(path), searchPattern);
            return filesList;
        }

        public static FilesModel LoadCsvHeaderAndFillLoaderModel(HttpPostedFileBase file)
        {
            var filesModel = new FilesModel();
            var stream = file.InputStream;
            var reader = new StreamReader(stream);

            var headerLine = reader.ReadLine();
            if (headerLine == null) return null;

            var headers = headerLine.Split(',', ';');
            filesModel.Header = "";
            foreach (var column in headers)
            {
                filesModel.Header = String.Concat(filesModel.Header, "\n", column);
            }
            filesModel.Header = filesModel.Header.Trim();
            filesModel.FileName = file.FileName;

            return filesModel;
        }

        public static FilesModel LoadCsvHeaderAndFillLoaderModel(string file)
        {
            var filesModel = new FilesModel();
            var reader = new StreamReader(file);

            var headerLine = reader.ReadLine();
            if (headerLine == null) return null;

            var headers = headerLine.Split(',', ';');
            filesModel.Header = "";
            foreach (var column in headers)
            {
                filesModel.Header = String.Concat(filesModel.Header, "\n", column, ";");
            }
            filesModel.Header = filesModel.Header.Trim();
            filesModel.FileName = file;//

            return filesModel;
        }

        public static bool CheckFileIsAlreadyLoaded(List<FilesModel> models, FilesModel filesModel)
        {
            var alreadyLoaded = false;
            foreach (var model in models)
            {
                if (model.FileName != filesModel.FileName || model.Header != filesModel.Header) continue;
                alreadyLoaded = true;
                break;
            }
            return alreadyLoaded;
        }

        public static Dictionary<string, string> LoadDbLoginPasswordObjects(IEntityRepository<LoginModel> repository,EncrDecrHelper encrDecrHelper, byte[] key, byte[] iv)
        {
            var loginModels = repository.GetAllObjectsList();
            var loginPassword = new Dictionary<string, string>();
            foreach (var document in loginModels)
            {
                loginPassword.Add(encrDecrHelper.DecryptString(document.Email, key, iv),
                    encrDecrHelper.DecryptString(document.Password, key, iv));
            }
            return loginPassword;
        }

        public static string ProcessWithFileLoading(HttpFileCollectionBase files, HttpServerUtilityBase server, out bool result)
        {
            string path = "";
            result = false; 
            foreach (string file in files)
            {
                var fileDataContent = files[file];
                if (fileDataContent != null && fileDataContent.ContentLength > 0)
                {
                    // take the input stream, and save it to a temp folder using the original file.part name posted
                    var stream = fileDataContent.InputStream;
                    var fileName = Path.GetFileName(fileDataContent.FileName);
                    var uploadPath = server.MapPath("~/App_Data/uploads");
                    Directory.CreateDirectory(uploadPath);
                    path = Path.Combine(uploadPath, fileName);
                    try
                    {
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }

                        // Once the file part is saved, see if we have enough to merge it
                        var ut = new Utils();
                        result = ut.MergeFile(path);

                        //remove parts when initial file is constructed back
                        if (result)
                        {
                            var filesList = Utils.GetFileListForDeletion(path);
                            foreach (var s in filesList)
                            {
                                try
                                {
                                    System.IO.File.Delete(s);
                                }
                                catch (IOException ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            if (result)
            {
                return path.Substring(0, path.IndexOf(partToken, StringComparison.Ordinal));
            }

            return "";//.Split('/','\\').Last().Replace(".csv","");
        }
    }
    
    public struct SortedFile
    {
        public int FileOrder { get; set; }
        public string FileName { get; set; }
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