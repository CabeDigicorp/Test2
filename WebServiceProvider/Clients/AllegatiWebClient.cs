using CommonResources;
using Commons;
using DevExpress.DXBinding.Native;
using DevExpress.Utils.Zip;
using ModelData.Dto;
using ModelData.Dto.Error;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using File = System.IO.File;

namespace WebServiceClient.Clients
{
    public class AllegatiWebClient
    {
        static string ApiPath { get => "/api/allegati"; }
        static readonly HttpClient _httpClient = null;




        static AllegatiWebClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ServerAddress.ApiCurrent);
        }

        public static event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        protected static void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(null, e);
        }

        public static async Task<IEnumerable<AllegatoDto>> GetAllegati(Guid operaId)
        {
            IEnumerable<AllegatoDto> allegati = null;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {
                // GET: api/Progetti?operaId=5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
                string requestString = string.Format("{0}?operaId={1}", ApiPath, operaId.ToString());
                var response = await _httpClient.GetAsync(requestString);

                if (response.IsSuccessStatusCode)
                {
                    allegati = await response.Content.ReadFromJsonAsync<IEnumerable<AllegatoDto>>();
                }

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return allegati;
        }

        //public static async Task<AddResponse> UploadFiles(Guid operaId, List<string> fullFilesName)
        //{
        //    var res = new AddResponse(false);

        //    _httpClient.DefaultRequestHeaders.Accept.Clear();
        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccountWebClient.AuthorizationToken);


            
            
            
            
            
        //    int chunkSize = 1000000;//~1MB

        //    foreach (var filePath in fullFilesName)
        //    {
        //        Guid batchGuid = Guid.NewGuid();
        //        CancellationToken cancel = default(CancellationToken);

        //        using (FileStream fileStream = File.Open(filePath, FileMode.Open))
        //        {
        //            using (var brotli = new BrotliStream(fileStream, CompressionMode.Compress))
        //            {
        //            }
        //        }


        //        using (FileStream fileStream = File.Open(filePath, FileMode.Open))
        //        {
        //                using (var compressor = new BrotliStream(fileStream, CompressionMode.Compress))
        //                {

        //                    //await fileStream.CopyToAsync(compressor, cancel);

        //                    long fileLenght = fileStream.Length;
        //                    long compressedLength = compressor.Length;
        //                    string fileName = Path.GetFileName(filePath);


        //                    int totalChunks = (int)(compressedLength / chunkSize);
        //                    if (compressedLength % chunkSize != 0)
        //                    {
        //                        totalChunks++;
        //                    }

        //                    for (int iChunk = 0; iChunk < totalChunks; iChunk++)
        //                    {
        //                        int iPerc = (iChunk * 100) / totalChunks;
        //                        OnProgressChanged(new ProgressChangedEventArgs(iPerc, string.Format("{0}... {1}", LocalizationProvider.GetString("Upload file"), fileName)));

        //                        long position = (iChunk * (long)chunkSize);
        //                        int toRead = (int)Math.Min(compressedLength - position, chunkSize);
        //                        byte[] buffer = new byte[toRead];
        //                        await compressor.ReadAsync(buffer, 0, buffer.Length);


        //                        using (var content = new MultipartFormDataContent())
        //                        {
        //                            try
        //                            {
        //                                content.Add(new StringContent(totalChunks.ToString()), "total-chunk");
        //                                content.Add(new StringContent(iChunk.ToString()), "chunk-index");
        //                                content.Add(new StringContent(operaId.ToString()), "operaId");
        //                                content.Add(new StringContent(batchGuid.ToString()), "batchGuid");
        //                                content.Add(new StringContent(chunkSize.ToString()), "chunkSize");


        //                                content.Add(new ByteArrayContent(buffer), "chunkFile", fileName);
        //                                content.Add(new ByteArrayContent(buffer), "uploadFiles", fileName);


        //                                var response = await _httpClient.PostAsync(ApiPath + "/upload", content);

        //                                if (response.IsSuccessStatusCode)
        //                                    res.Success = true;
        //                                else
        //                                    res.Success = false;

        //                                if (!res.Success)
        //                                    break;
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                res.Message = ex.Message;
        //                                res.Success = false;
        //                            }

        //                        }
        //                    }
        //            }
        //        }
        //    }

        //    OnProgressChanged(new ProgressChangedEventArgs(100, "End"));

        //    return res;
        //}

        public static async Task<AddResponse> UploadFiles(Guid operaId, List<string> fullFilesName)
        {
            var res = new AddResponse(false);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);



            //check se i file sono tutti liberi e possono essere caricati
            foreach (var filePath in fullFilesName)
            {
                bool isFileInUse = IsFileInUse(filePath);
                if (isFileInUse)
                {
                    res.Message = string.Format("{0}: {1}", LocalizationProvider.GetString("FileInUso"), filePath);
                    res.Success = false;
                    return res;
                }

            }




            int chunkSize = 1000000;//~1MB

            foreach (var filePath in fullFilesName)
            {
                Guid batchGuid = Guid.NewGuid();

                using (FileStream fileStream = File.OpenRead(filePath))
                {

                    long fileLength = fileStream.Length;
                    string fileName = Path.GetFileName(filePath);


                    int totalChunks = (int)(fileLength / chunkSize);
                    if (fileLength % chunkSize != 0)
                    {
                        totalChunks++;
                    }

                    for (int iChunk = 0; iChunk < totalChunks; iChunk++)
                    {
                        int iPerc = (iChunk * 100) / totalChunks;
                        OnProgressChanged(new ProgressChangedEventArgs(iPerc, string.Format("{0}... {1}", LocalizationProvider.GetString("Upload file"), fileName)));

                        long position = (iChunk * (long)chunkSize);
                        int toRead = (int)Math.Min(fileLength - position, chunkSize);
                        byte[] buffer = new byte[toRead];
                        await fileStream.ReadAsync(buffer, 0, buffer.Length);


                        using (var content = new MultipartFormDataContent())
                        {
                            try
                            {

                                content.Add(new StringContent(totalChunks.ToString()), "total-chunk");
                                content.Add(new StringContent(iChunk.ToString()), "chunk-index");
                                content.Add(new StringContent(operaId.ToString()), "operaId");
                                content.Add(new StringContent(batchGuid.ToString()), "batchGuid");
                                content.Add(new StringContent(chunkSize.ToString()), "chunkSize");

                                content.Add(new StringContent(Guid.Empty.ToString()), "parentGuid");
                                content.Add(new StringContent(true.ToString()), "overwrite");


                                content.Add(new ByteArrayContent(buffer), "chunkFile", fileName);
                                content.Add(new ByteArrayContent(buffer), "uploadFiles", fileName);


                                var response = await _httpClient.PostAsync(ApiPath + "/upload", content);

                                int p = 0;

                                if (response.IsSuccessStatusCode)
                                    res.Success = true;
                                else
                                {
                                    res.Success = false;
                                    
                                    if (response.Content.Headers.ContentType?.MediaType == "application/json")
                                    {
                                        ErrorDto errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
                                        string msg = string.Empty;
                                        if (errorDto.ErrorData.TryGetValue("Reason", out msg))
                                        {
                                            if (msg == "DuplicateKey")
                                            {
                                                res.Success = true;
                                                res.Message = LocalizationProvider.GetString("Chiave duplicata");
                                            }
                                        }
                                    }
                                    else if (response.Content.Headers.ContentType?.MediaType == "text/plain")
                                    {
                                        var content1 = await response.Content.ReadAsStringAsync();
                                        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), content1);
                                    }
                                    
                                }

                                if (!res.Success)
                                    break;
                            }
                            catch (Exception ex)
                            {
                                res.Message = ex.Message;
                                res.Success = false;
                            }

                        }
                    }

                    if (!res.Success)
                        break;

                }
            }

            OnProgressChanged(new ProgressChangedEventArgs(100, "End"));

            return res;
        }

        private static bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // Se il file viene aperto con successo, non è in uso
                    return false;
                }
            }
            catch (IOException ex)
            {
                // Se viene generata un'eccezione, il file è in uso
                return true;
            }
        }



        //public static async Task<AddResponse> UploadFiles_Old(Guid operaId, List<string> fullFilesName)
        //{

        //    var gr = new AddResponse(false);

        //    using var multipartFormContent = new MultipartFormDataContent();

        //    multipartFormContent.Add(new StringContent(operaId.ToString()), "operaId");
        //    multipartFormContent.Add(new StringContent("true"), "overwrite");

        //    int i_file = 0;
        //    foreach (string fullFileName in fullFilesName)
        //    {
        //        string fileName = Path.GetFileName(fullFileName);

        //        var fileStreamContent = new StreamContent(File.OpenRead(fullFileName));
        //        fileStreamContent.Headers.ContentType = null;
        //        //fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.FileContentType);
        //        multipartFormContent.Add(fileStreamContent, name: "file-" + ++i_file, fileName: fileName);
        //    }


        //    _httpClient.DefaultRequestHeaders.Accept.Clear();
        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccountWebClient.AuthorizationToken);

        //    try
        //    {

        //        var response = await _httpClient.PostAsync(ApiPath, multipartFormContent);

        //        gr.Message = response.ReasonPhrase;
        //        if (response.IsSuccessStatusCode)
        //        {
        //            gr.Success = true;
        //        }
        //        else
        //        {
        //            if (response.Content.Headers.ContentType?.MediaType == "application/json")
        //            {
        //                ErrorDto errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
        //                string msg = string.Empty;
        //                if (errorDto.ErrorData.TryGetValue("Reason", out msg))
        //                {
        //                    if (msg == "DuplicateKey")
        //                        gr.Message = LocalizationProvider.GetString("Chiave duplicata");
        //                }
        //            }
        //            else if (response.Content.Headers.ContentType?.MediaType == "text/plain")
        //            {
        //                var content = await response.Content.ReadAsStringAsync();
        //                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), content);
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        gr.Message = ex.Message;
        //    }



        //    return gr;
        //}


        //public static void CompressStream(Stream originalStream, Stream compressedStream) => CompressStreamAsync(originalStream, compressedStream).GetAwaiter().GetResult();
        //public static async Task CompressStreamAsync(Stream originalStream, Stream compressedStream, CancellationToken cancel = default(CancellationToken))
        //{
        //    using (var compressor = new BrotliStream(compressedStream, CompressionLevel.Optimal))
        //    {
        //        await originalStream.CopyToAsync(compressor, cancel);
        //    }
        //}
    }


}

