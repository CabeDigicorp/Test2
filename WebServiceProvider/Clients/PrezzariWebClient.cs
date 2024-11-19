using Commons;
using Model.JoinService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace WebServiceClient.Clients
{

    public class PrezzariWebClient
    {

        //static string _baseAddress = "http://joinservice.digicorp.it/";
        static string _baseAddress = "https://localhost:44328/";
        //static string _baseAddress = ServerAddress.ApiRegister;

        static string ApiPath { get => "/api/prezzari"; }
        static readonly HttpClient _httpClient = null;

        public static event EventHandler ProgressNotify;

        public static string ErrorMessage { get; set; } = string.Empty;

        static PrezzariWebClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ServerAddress.ApiRegister);
        }

        public static async Task<List<PrezzarioInfo>> GetPrezzariInfoAsync()
        {
            ErrorMessage = string.Empty;

            //string path = string.Format("api/prezzari/");
            string responseString = string.Empty;
            List<PrezzarioInfo> info = new List<PrezzarioInfo>();


            _httpClient.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(ApiPath).ConfigureAwait(false);
                //response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

                if (response.IsSuccessStatusCode)
                {
                    Stream stream = await response.Content.ReadAsStreamAsync();

                    if (response.IsSuccessStatusCode)
                        info = DeserializeJsonFromStream<List<PrezzarioInfo>>(stream);

                    //var content = await StreamToStringAsync(stream);

                    //responseString = await response.Content.ReadAsStringAsync();
                    //JsonSerializer.JsonDeserialize<PrezzarioInfo>(responseString, out info, infoType.GetType());
                }
            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
                ErrorMessage = exc.Message;
            }




            return info;
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new Newtonsoft.Json.JsonTextReader(sr))
            {
                var js = new Newtonsoft.Json.JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }


        //public static async Task<string> DownloadFile2(string fullFileName)
        //{
        //    string errorMsg = string.Empty;
        //    var fileInfo = new FileInfo(fullFileName);

        //    string filename = Path.GetFileNameWithoutExtension(fullFileName);

        //    //string localPath = string.Format("{0}\\{1}", targetFolder, filename);

        //    string path = string.Format("{0}{1}", ApiPath, filename);
        //    // validation
        //    //_logger.LogInformation($"Downloading File with GUID=[{guid}].");

        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(_baseAddress);
        //        client.DefaultRequestHeaders.Accept.Clear();

        //        try
        //        {


        //            //var response = await client.GetAsync($"{_url}/api/files?guid={guid}");
        //            HttpResponseMessage response = await client.GetAsync(path).ConfigureAwait(false);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                response.EnsureSuccessStatusCode();
        //                Stream stream = await response.Content.ReadAsStreamAsync();

        //                FileStream fs = File.Create(fullFileName);
        //                stream.Seek(0, SeekOrigin.Begin);
        //                stream.CopyTo(fs);

        //            }
        //            else
        //            {
        //                errorMsg = response.StatusCode.ToString();
        //            }

        //        }
        //        catch (Exception exc)
        //        {
        //            errorMsg = exc.Message;
        //        }
        //        //    response.EnsureSuccessStatusCode();
        //        //await using var ms = await response.Content.ReadAsStreamAsync();
        //        //await using var fs = File.Create(fileInfo.FullName);
        //        //ms.Seek(0, SeekOrigin.Begin);
        //        //ms.CopyTo(fs);
        //    }




        //    //_logger.LogInformation($"File saved as [{fileInfo.Name}].");
        //    return errorMsg;
        //}

        public static async Task<string> DownloadFile(string fullFileName)
        {
            string errorMsg = string.Empty;
            var fileInfo = new FileInfo(fullFileName);

            string filename = Path.GetFileNameWithoutExtension(fullFileName);

            //string localPath = string.Format("{0}\\{1}", targetFolder, filename);

            string path = string.Format("{0}/{1}", ApiPath, filename);
            // validation
            //_logger.LogInformation($"Downloading File with GUID=[{guid}].");


            _httpClient.DefaultRequestHeaders.Accept.Clear();

            try
            {


                //var response = await client.GetAsync($"{_url}/api/files?guid={guid}");
                HttpResponseMessage response = await _httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    int totalReads = 0;

                    response.EnsureSuccessStatusCode();
                    //Stream stream = await response.Content.ReadAsStreamAsync();
                    //FileStream fs = File.Create(fullFileName);
                    //stream.Seek(0, SeekOrigin.Begin);
                    //stream.CopyTo(fs);

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(fullFileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var totalRead = 0L;
                        //var totalReads = 0L;
                        var buffer = new byte[8192];
                        var isMoreToRead = true;

                        do
                        {
                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);

                                totalRead += read;
                                totalReads += 1;
                                    

                                if (totalReads % 900 == 0)//2000
                                {
                                    OnProgressNotify(new PrezzariWebClientProgressNotifyEventArgs() {FileName = filename, DownloadedBytes = totalRead });

                                    Debug.WriteLine(string.Format("total bytes downloaded so far: {0:n0}", totalRead));
                                    //Console.WriteLine(string.Format("total bytes downloaded so far: {0:n0}", totalRead));
                                }
                            }
                        }
                        while (isMoreToRead);
                    }
                }
                else
                {
                    errorMsg = response.StatusCode.ToString();
                    ErrorMessage = errorMsg;
                }

            }
            catch (Exception exc)
            {
                errorMsg = exc.Message;
                ErrorMessage = errorMsg;
            }
            //    response.EnsureSuccessStatusCode();
            //await using var ms = await response.Content.ReadAsStreamAsync();
            //await using var fs = File.Create(fileInfo.FullName);
            //ms.Seek(0, SeekOrigin.Begin);
            //ms.CopyTo(fs);




            //_logger.LogInformation($"File saved as [{fileInfo.Name}].");
            return errorMsg;
        }


        protected static void OnProgressNotify(EventArgs e)
        {
            ProgressNotify?.Invoke("PrezzariWebClient", e);
            //ProgressNotify = null;//scopo: per far in modo che l'evento parta una volta sola
        }


    }

    public class PrezzariWebClientProgressNotifyEventArgs : EventArgs
    {
        public string FileName = string.Empty;
        public long DownloadedBytes { get; set; } = 0;
    }
}
