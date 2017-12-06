using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvLoader3.Controllers
{
    public class Utils
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<string> FileParts { get; set; }

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
            bool rslt = false;
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = fileName.Substring(0, fileName.IndexOf(partToken));
            string trailingTokens = fileName.Substring(fileName.IndexOf(partToken) + partToken.Length);
            int fileIndex = 0;
            int fileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out fileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out fileCount);
            // get a list of all file parts in the temp folder
            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
            string[] filesList = Directory.GetFiles(Path.GetDirectoryName(fileName), Searchpattern);
            //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
            // only proceed if we have received all the file chunks
            if (filesList.Count() == fileCount)
            {
                // use a singleton to stop overlapping processes
                if (!MergeFileManager.Instance.InUse(baseFileName))
                {
                    MergeFileManager.Instance.AddFile(baseFileName);
                    if (File.Exists(baseFileName))
                        File.Delete(baseFileName);
                    // add each file located to a list so we can get them into
                    // the correct order for rebuilding the file
                    List<SortedFile> mergeList = new List<SortedFile>();
                    foreach (string file in filesList)
                    {
                        SortedFile sFile = new SortedFile();
                        sFile.FileName = file;
                        baseFileName = file.Substring(0, file.IndexOf(partToken));
                        trailingTokens = file.Substring(file.IndexOf(partToken) + partToken.Length);
                        int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out fileIndex);
                        sFile.FileOrder = fileIndex;
                        mergeList.Add(sFile);
                    }
                    // sort by the file-part number to ensure we merge back in the correct order
                    var mergeOrder = mergeList.OrderBy(s => s.FileOrder).ToList();
                    using (FileStream FS = new FileStream(baseFileName, FileMode.Create))
                    {
                        // merge each file chunk back into one contiguous file stream
                        foreach (var chunk in mergeOrder)
                        {
                            try
                            {
                                using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                                {
                                    fileChunk.CopyTo(FS);
                                    System.IO.File.Delete(chunk.FileName);
                                }
                            }
                            catch (IOException ex)
                            {
                                // handle
                            }
                        }
                    }
                    rslt = true;
                    // unlock the file from singleton
                    MergeFileManager.Instance.RemoveFile(baseFileName);
                }
            }
            return rslt;
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