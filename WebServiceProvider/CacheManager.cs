using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebServiceClient.Clients;

namespace WebServiceClient
{
    public class CacheManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static string FolderPath { get; set; }

        public static long MaxBytes { get; set; } 

        public static string ApiFileRepositoryPath => @"/api/allegati/download?";

        public static async Task<string> Download(/*Guid fileId, */string url)
        {
            string pathToOpen = String.Empty;

            //file da scaricare in cache
            //if (fileId == Guid.Empty)
            //    return pathToOpen;


            string fileName = string.Empty;
            string operaId = string.Empty;


            
            string uploadGuid = string.Empty;

            Uri uri = new Uri(url);
            //if (uri.IsFile)
            //    fileName = Path.GetFileName(url);
            //else
            //{
            //    //web
            //    if (IsJoinApiSource(url))
            //    {
            //        //server web digicorp
            //        uploadGuid = HttpUtility.ParseQueryString(uri.Query).Get("uploadGuid");

            //        operaId = HttpUtility.ParseQueryString(uri.Query).Get("operaId");

            //        if (string.IsNullOrEmpty(operaId))
            //            return pathToOpen;

            //        if (string.IsNullOrEmpty(uploadGuid))
            //            return pathToOpen;
            //    }
            //    else
            //    {
            //        //altro server web
            //        fileName = Path.GetFileName(url);
            //    }

            //}

            if (!IsJoinApiSource(url))
                return pathToOpen;

            //server web digicorp
            uploadGuid = HttpUtility.ParseQueryString(uri.Query).Get("uploadGuid");

            operaId = HttpUtility.ParseQueryString(uri.Query).Get("operaId");


            if (string.IsNullOrEmpty(operaId))
                return pathToOpen;

            if (string.IsNullOrEmpty(uploadGuid))
                return pathToOpen;

            var allegatiDto = await AllegatiWebClient.GetAllegati(new Guid(operaId));
            var allegatoDto = allegatiDto.FirstOrDefault(item => item.Id.ToString() == uploadGuid);
            if (allegatoDto == null)
                return pathToOpen;


            fileName = allegatoDto.FileName;

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            //string fileIdDir = allegatoDto.UploadGuid;
            string fileIdDir = uploadGuid;

            string fullFileIdDir = string.Format("{0}{1}", FolderPath, fileIdDir);

            pathToOpen = string.Format("{0}\\{1}", fullFileIdDir, fileName);

            if (!File.Exists(pathToOpen))
            {
                //il file con nome univoco non esiste perciò occorre scaricarlo

                if (!Directory.Exists(fullFileIdDir))
                    Directory.CreateDirectory(fullFileIdDir);


                await DownloadFileAsync(url, pathToOpen);


                FileInfo downloadedFileInfo = new FileInfo(pathToOpen);

                if (downloadedFileInfo.Exists)
                    downloadedFileInfo.Attributes = FileAttributes.ReadOnly;
                else
                    return String.Empty;


                Task.Run(() => FreeBytesOver());


            }
            return pathToOpen;
        }

        public static bool IsJoinApiSource(string url)
        {
            if (!url.StartsWith("https://"))
                return false;

            Uri uri = new Uri(url);
            Uri uriBase = new Uri(string.Format("{0}{1}", ServerAddress.ApiCurrent, CacheManager.ApiFileRepositoryPath));

            var ret = uri.IsBaseOf(uriBase);

            return ret;
        }

        static async Task DownloadFileAsync(string uri
             , string outputPath)
        {
            Uri uriResult;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
            {
                //throw new InvalidOperationException("URI is invalid.");
                return;
            }


            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);


                byte[] fileBytes = await _httpClient.GetByteArrayAsync(uri);
                File.WriteAllBytes(outputPath, fileBytes);
            }
            catch
            {

            }
        }


        private static async void FreeBytesOver()
        {
            DirectoryInfo cacheDirInfo = new DirectoryInfo(FolderPath);

            List<CacheDirInfo> dirs = new List<CacheDirInfo>();

            //raccolgo i dati sulle directory
            DirectoryInfo[] dirsInfo = cacheDirInfo.GetDirectories();
            foreach (var dirInfo in dirsInfo)
            {
                IEnumerable<FileInfo> filesInfo = dirInfo.EnumerateFiles();
                long lenght = 0;
                foreach (var fileInfo in filesInfo)
                    lenght += fileInfo.Length;


                dirs.Add(new CacheDirInfo() { DirPath = dirInfo.FullName, Bytes = lenght, Date = dirInfo.LastWriteTime });
            }

            //elimino directory vecchie
            var orderedFiles = dirs.OrderByDescending(item => item.Date);
            long bytes = 0;
            foreach (var dir in orderedFiles)
            {
                bytes += dir.Bytes;

                if (bytes > dir.Bytes && bytes > MaxBytes)//se non è il primo ed è stato superato il limite
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir.DirPath);
                    foreach (var info in dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
                    {
                        info.Attributes = FileAttributes.Normal;
                    }
                    dirInfo.Delete(true);
                }
            }
        }

        class CacheDirInfo
        {
            public string DirPath { get; set; }
            public DateTime Date { get; set; }
            public long Bytes { get; set; }
        }

        public static void Clear()
        {
            if (!Directory.Exists(FolderPath))
                return;

            DirectoryInfo cacheDirInfo = new DirectoryInfo(FolderPath);

            DirectoryInfo[] dirsInfo = cacheDirInfo.GetDirectories();
            foreach (var dirInfo in dirsInfo)
            {
                foreach (var info in dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }
                dirInfo.Delete(true);
            }
        }

        public static Task<long> GetCurrentBytesAsync()
        {
            Task<long> res = Task.Run (() =>
            {
                return GetCurrentBytes();
            });

            return res;
        }

        public static long GetCurrentBytes()
        {
            long bytes = 0;

            if (!Directory.Exists(FolderPath))
                return bytes;

            DirectoryInfo cacheDirInfo = new DirectoryInfo(FolderPath);

            //raccolgo i dati sulle directory
            DirectoryInfo[] dirsInfo = cacheDirInfo.GetDirectories();
            foreach (var dirInfo in dirsInfo)
            {
                IEnumerable<FileInfo> filesInfo = dirInfo.EnumerateFiles();
                foreach (var fileInfo in filesInfo)
                    bytes += fileInfo.Length;
            }

            return bytes;
        }
    }
}
