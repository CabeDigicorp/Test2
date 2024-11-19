using JoinWebUI.Extensions;
using JoinWebUI.Shared;
using JoinWebUI.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using ModelData.Dto.Error;
using Newtonsoft.Json;
using Syncfusion.Blazor.Inputs;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ICSharpCode.SharpZipLib.GZip;
using ModelData.Dto;
using System.Text;
//using static System.Runtime.InteropServices.JavaScript.JSType;
//using System.IO.Compression;


namespace JoinWebUI.Services
{

    public class JoinWebApiHttpClient
    {
        public HttpClient HttpClient { get; private set; }

        public JoinWebApiHttpClient(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        public string GetBaseUrl()
        {
            return HttpClient.BaseAddress.ToString();
        }
    }

    public class JoinWebApiClient
    {
        private readonly HttpClient _httpClient;

        public JoinWebApiClient(JoinWebApiHttpClient client)
        {
            _httpClient = client.HttpClient;

        }

        public string GetBaseUrl()
        {
            return _httpClient.BaseAddress.ToString();
        }

        public string BearerToken { get; private set; }

        public void SetBearerToken(string token)
        {
            try
            {
                BearerToken = token ?? string.Empty;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                //if (!_httpClient.DefaultRequestHeaders.Contains("content-type"))
                //{
                //    _httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public void ClearBearerToken()
        {
            try
            {
                BearerToken = string.Empty;
                _httpClient.CancelPendingRequests();
                _httpClient.DefaultRequestHeaders.Authorization = null;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public async Task<RequestResult<TResponse>> JsonSendAsync<TResponse>(string path, HttpMethod method, IDictionary<string, string>? query = null, object? model = null)
        {
            //SetBearerToken();

            var uri = path;

            if (query != null)
            {
                uri = QueryHelpers.AddQueryString(uri, query);
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, uri);


            if (model != null)
            {
                httpRequestMessage.Content = JsonContent.Create(model, model.GetType());
            }

            HttpResponseMessage? response;

            try
            {
                response = await _httpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult<TResponse>
                {
                    // Success is false
                };
            }

            RequestResult<TResponse> result = new RequestResult<TResponse>
            {
                ResponseStatusCode = response.StatusCode,
            };

            if (response.StatusCode.IsSuccessful())
            {
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    try
                    {
                        result.ResponseContentData = await response.Content.ReadFromJsonAsync<TResponse>();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (response.StatusCode.IsError())
            {
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    try
                    {
                        ErrorDto? errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
                        result.ResponseContentErrorData = errorDto!.ErrorData;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return result;
        }

        private async Task<RequestResult<TResponse>> JsonSendAsync<Payload, TResponse>(string path, HttpMethod method, Payload model, IDictionary<string, string>? query = null)
        {
            //SetBearerToken();

            var uri = path;

            if (query != null)
            {
                uri = QueryHelpers.AddQueryString(uri, query);
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, uri);


            if (model != null)
            {
                //httpRequestMessage.Content = JsonContent.Create(model, model.GetType());

                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"); // più performante rispetto JsonContent.Create 
            }

            HttpResponseMessage? response;

            try
            {
                response = await _httpClient.SendAsync(httpRequestMessage);
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult<TResponse>
                {
                    // Success is false
                };
            }

            RequestResult<TResponse> result = new RequestResult<TResponse>
            {
                ResponseStatusCode = response.StatusCode,
            };

            if (response.StatusCode.IsSuccessful())
            {
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    try
                    {
                        result.ResponseContentData = await response.Content.ReadFromJsonAsync<TResponse>();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                }
            }
            else if (response.StatusCode.IsError())
            {
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    try
                    {
                        ErrorDto? errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
                        result.ResponseContentErrorData = errorDto!.ErrorData;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return result;
        }

        public async Task<RequestResult<TResponse>> JsonPostAsync<TResponse>(string path, object? model)
        {
            return await JsonSendAsync<TResponse>(path, HttpMethod.Post, model: model);
        }

        public async Task<RequestResult<TResponse>> JsonPostAsync<Payload, TResponse>(string path, Payload model)
        {
            return await JsonSendAsync<Payload, TResponse>(path, HttpMethod.Post, model: model);
        }

        public async Task<RequestResult<TResponse>> JsonGetAsync<TResponse>(string path, string? id = null, IDictionary<string, string>? query = null)
        {
            return await JsonSendAsync<TResponse>(path + (id != null ? ("/" + id) : string.Empty), HttpMethod.Get, query: query);
        }

        public async Task<RequestResult<TResponse>> JsonPutAsync<TResponse>(string path, string id, object? model)
        {
            return await JsonSendAsync<TResponse>(path + (id != null ? ("/" + id) : string.Empty), HttpMethod.Put, model: model);
        }

        public async Task<RequestResult<TResponse>> JsonDeleteAsync<TResponse>(string path, string id)
        {
            return await JsonSendAsync<TResponse>(path + (id != null ? ("/" + id) : string.Empty), HttpMethod.Delete);
        }

        public async Task<RequestResult> JsonPostAsync(string path, object? model)
        {
            return await JsonPostAsync<object>(path, model);
        }

        public async Task<RequestResult> JsonGetAsync(string path, string? id = null, IDictionary<string, string>? query = null)
        {
            return await JsonGetAsync<object>(path, id, query);
        }

        public async Task<RequestResult> JsonPutAsync(string path, string id, object? model)
        {
            return await JsonPutAsync<object>(path, id, model);
        }

        public async Task<RequestResult> JsonDeleteAsync(string path, string id)
        {
            return await JsonDeleteAsync<object>(path, id);
        }

        public async Task<RequestResult> JsonSendAsync(string path, HttpMethod method, IDictionary<string, string>? query = null, object? model = null)
        {
            return await JsonSendAsync<object>(path, method, query, model);
        }

        public class RequestResult
        {
            public HttpStatusCode? ResponseStatusCode { get; set; }
            public IDictionary<string, string>? ResponseContentErrorData { get; set; }
            public bool Success
            {
                get
                {
                    return ResponseStatusCode.HasValue ? ResponseStatusCode.Value.IsSuccessful() : false;
                }
            }

            public string? GetResponseContentErrorData(string key)
            {
                if (ResponseContentErrorData == null)
                {
                    return null;
                }
                string? value;
                if (!ResponseContentErrorData.TryGetValue(key, out value))
                {
                    return null;
                }
                return value;
            }
        }

        public class RequestResult<TResponse> : RequestResult
        {
            public TResponse? ResponseContentData { get; set; }
        }

        public class RequestFileResult : RequestResult, IDisposable
        {
            public string? FileName { get; set; }
            public Byte[]? FileContentBytes { get; set; }
            public IDictionary<string, string>? ResponseMetadata { get; set; }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                FileContentBytes = null;
                ResponseMetadata?.Clear();
            }
        }

        //        public async Task<RequestResult<TResponse>> PostFilesAsync<TResponse>(string path, IEnumerable<FileHandler> files, IDictionary<string, string?>? formData = null)
        //        {
        //            //SetBearerToken();

        //            var uri = path;

        //            using var multipartFormContent = new MultipartFormDataContent();

        //            if (formData != null)
        //            {
        //                foreach (var entry in formData)
        //                {
        //                    if (entry.Value != null)
        //                    {
        //                        multipartFormContent.Add(new StringContent(entry.Value), entry.Key);
        //                    }
        //                }
        //            }

        //            int i_file = 0;
        //            foreach (var file in files)
        //            {
        //                var fileStreamContent = new StreamContent(file.FileContentStream);
        //                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.FileContentType);
        //                multipartFormContent.Add(fileStreamContent, name: "file-" + ++i_file, fileName: file.FileName);
        //            }

        //            var response = await _httpClient.PostAsync(uri, multipartFormContent);

        //            RequestResult<TResponse> result = new RequestResult<TResponse>
        //            {
        //                ResponseStatusCode = response.StatusCode,
        //            };

        //            if (response.StatusCode.IsSuccessful())
        //            {
        //                if (response.Content.Headers.ContentType?.MediaType == "application/json")
        //                {
        //                    result.ResponseContentData = await response.Content.ReadFromJsonAsync<TResponse>();
        //                }
        //            }
        //            else if (response.StatusCode.IsError())
        //            {
        //                if (response.Content.Headers.ContentType?.MediaType == "application/json")
        //                {
        //                    try
        //                    {
        //                        ErrorDto? errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
        //                        result.ResponseContentErrorData = errorDto!.ErrorData;
        //                    }
        //#if DEBUG
        //                    catch (Exception e)
        //#else
        //                    catch (Exception)
        //#endif
        //                    {
        //                    }
        //                }
        //            }

        //            return result;
        //        }

        //        public async Task<RequestResult> PostFilesAsync(string path, IEnumerable<FileHandler> files, IDictionary<string, string?>? formData = null)
        //        {
        //            return await PostFilesAsync<object>(path, files, formData);
        //        }

        //public async Task<RequestFileResult> DownloadAllegatoAsync(string path, bool decompress, IDictionary<string, string>? query)
        //{
        //    //SetBearerToken();

        //    CancellationTokenSource source = new CancellationTokenSource();
        //    CancellationToken token = source.Token;

        //    var uri = path;

        //    if (query != null)
        //    {
        //        uri = QueryHelpers.AddQueryString(uri, query);
        //    }

        //    HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

        //    HttpResponseMessage? response;

        //    try
        //    {
        //        response = await _httpClient.SendAsync(httpRequestMessage);


        //        RequestFileResult result = new RequestFileResult
        //        {
        //            ResponseStatusCode = response.StatusCode,
        //        };

        //        if (response.StatusCode.IsSuccessful())
        //        {
        //            result.FileName = response.Content.Headers.ContentDisposition?.FileName?.Trim(new char[] { '\"', '\'' });

        //            var compressed = Convert.ToBoolean(response.Headers.GetValues("compressed").FirstOrDefault());

        //            if (decompress)
        //            {
        //                var compressedBytes = await response.Content.ReadAsByteArrayAsync();
        //                MemoryStream tmp = new MemoryStream();
        //                GZip.Decompress(new MemoryStream(compressedBytes), tmp, true);
        //                result.FileContentBytes = tmp.ToArray();
        //                tmp.Dispose();
        //            }
        //            else
        //            {
        //                result.FileContentBytes = await response.Content.ReadAsByteArrayAsync();
        //            }
        //        }
        //        else if (response.StatusCode.IsError())
        //        {
        //            if (response.Content.Headers.ContentType?.MediaType == "application/json")
        //            {
        //                try
        //                {
        //                    ErrorDto? errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
        //                    result.ResponseContentErrorData = errorDto!.ErrorData;
        //                }
        //                catch (Exception)
        //                {
        //                }
        //            }
        //        }

        //        response.Content.Dispose();
        //        response.Dispose();
        //        response = null;

        //        return result;

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return new RequestFileResult
        //        {
        //            // Success is false
        //        };
        //    }
        //}

        
        public async Task<RequestFileResult> DownloadAllegatoAsync(Guid operaId, Guid uploadGuid, bool decompressOnServer, bool decompressOnClient = true)
        {
            //SetBearerToken();

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var uri = GetBaseUrl() + "allegati/info";

            IDictionary<string, string> query = new Dictionary<string, string>()
                {
                   { "operaId", operaId.ToString() },
                   { "uploadGuid", uploadGuid.ToString() },
                };

            uri = QueryHelpers.AddQueryString(uri, query);


            try
            {
                var fileInfoResponse = await JsonGetAsync<AllegatoDto>(uri);

                RequestFileResult result = new RequestFileResult
                {
                    ResponseStatusCode = fileInfoResponse.ResponseStatusCode
                };

                if (fileInfoResponse.Success && fileInfoResponse.ResponseContentData != null)
                {
                    result.FileName = fileInfoResponse.ResponseContentData.FileName.Trim(new char[] { '\"', '\'' });
                    result.ResponseMetadata = new Dictionary<string, string>()
                    {
                        { "FileSize", fileInfoResponse.ResponseContentData.FileSize.ToString() },
                        { "Compressed", fileInfoResponse.ResponseContentData.Compressed.ToString() },
                        { "UploadDateTime", fileInfoResponse.ResponseContentData.UploadDateTime.ToString() },
                        { "OperaId", fileInfoResponse.ResponseContentData.OperaId.ToString() },
                        { "ParentId", fileInfoResponse.ResponseContentData.ParentId.HasValue ? fileInfoResponse.ResponseContentData.ParentId.Value.ToString() : string.Empty },
                        //{ "NumeroProgettiAllegato", fileInfoResponse.ResponseContentData.NumeroProgettiAllegato.ToString() },
                        //{ "NumeroProgettiModello", fileInfoResponse.ResponseContentData.NumeroProgettiModello.ToString() },
                        { "ConversionState", fileInfoResponse.ResponseContentData.ConversionState.ToString() },
                    };


                    uri = GetBaseUrl() + "allegati/download";
                    query.Add("decompress", decompressOnServer.ToString());
                    uri = QueryHelpers.AddQueryString(uri, query);

                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                    HttpResponseMessage? fileContentResponse = await _httpClient.SendAsync(httpRequestMessage);

                    result.ResponseStatusCode = fileContentResponse.StatusCode;


                    if (fileContentResponse.StatusCode.IsSuccessful())
                    {
                        if (fileInfoResponse.ResponseContentData.Compressed && decompressOnClient)
                        {
                            var compressedBytes = await fileContentResponse.Content.ReadAsByteArrayAsync();
                            MemoryStream tmp = new MemoryStream();
                            GZip.Decompress(new MemoryStream(compressedBytes), tmp, true);
                            result.FileContentBytes = tmp.ToArray();
                            tmp.Dispose();
                        }
                        else
                        {
                            result.FileContentBytes = await fileContentResponse.Content.ReadAsByteArrayAsync();
                        }
                    }
                    else if (fileContentResponse.StatusCode.IsError())
                    {
                        if (fileContentResponse.Content.Headers.ContentType?.MediaType == "application/json")
                        {
                            try
                            {
                                ErrorDto? errorDto = await fileContentResponse.Content.ReadFromJsonAsync<ErrorDto>();
                                result.ResponseContentErrorData = errorDto!.ErrorData;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }

                    fileContentResponse.Content.Dispose();
                    fileContentResponse.Dispose();
                    fileContentResponse = null;

                }
                else
                {
                    result.ResponseContentErrorData = fileInfoResponse.ResponseContentErrorData;
                }

                fileInfoResponse = null;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new RequestFileResult
                {
                    // Success is false
                };
            }
        }

        public async Task<Stream> GetStreamAsync(string? requestUri)
        {
            return await _httpClient.GetStreamAsync(requestUri);
        }


        public enum CompressionOptions
        {
            DoNotCompress = 0,
            ToBeCompressed = 1,
            AlreadyCompressed = 2,

        }


        public async Task<RequestResult> UploadAllegatoAsync(string path,
                                                             string name,
                                                             string contentType,
                                                             byte[] content,
                                                             Guid operaId,
                                                             Guid? parentGuid = null,
                                                             CompressionOptions compressionOptions = CompressionOptions.DoNotCompress,
                                                             bool overwrite = false)
        {

            byte[] buffer;
            if (compressionOptions == CompressionOptions.ToBeCompressed)
            {
                MemoryStream tmp = new MemoryStream();
                GZip.Compress(new MemoryStream(content), tmp, true);
                buffer = tmp.ToArray();

                tmp.Dispose();

                compressionOptions = CompressionOptions.AlreadyCompressed;

            }
            else
            {
                buffer = content;
            }

            const int CHUNKSIZE = 1000000;
            int totalChunks = (int)Math.Ceiling((double)buffer.Length / (double)CHUNKSIZE);

            var chunkedContent = new List<byte[]>();

            for (int i = 0; i < totalChunks; i++)
            {
                int length = (int)Math.Min(CHUNKSIZE, buffer.Length - i * CHUNKSIZE);
                byte[] chunkBytes = new byte[length];

                Array.ConstrainedCopy(buffer, i* CHUNKSIZE, chunkBytes, 0, length);

                chunkedContent.Add(chunkBytes);

            }

            return await UploadAllegatoAsync(path, name, contentType, chunkedContent, CHUNKSIZE, operaId, parentGuid, compressionOptions, overwrite);

        }

        public async Task<RequestResult> UploadAllegatoAsync(string path,
                                                             string name,
                                                             string contentType,
                                                             List<byte[]> chunkedContent,
                                                             int chunkSize,
                                                             Guid operaId,
                                                             Guid? parentGuid = null,
                                                             CompressionOptions compressionOptions = CompressionOptions.DoNotCompress,
                                                             bool overwrite = false)
        {
            var uri = path;

            if (compressionOptions == CompressionOptions.ToBeCompressed)
            {
                return new RequestResult()
                {
                    ResponseStatusCode = HttpStatusCode.BadRequest,
                    ResponseContentErrorData = new Dictionary<string, string>( ){ { "Reason", "File chunks must be already compressed." } }
                };
            }

            Guid batch = Guid.NewGuid();
            //int totalChunks = (int)Math.Ceiling((double)file.FileContentStream.Length / (double)CHUNKSIZE);
            List<MultipartFormDataContent> chunks = new List<MultipartFormDataContent>();

            RequestResult result = new RequestResult { ResponseStatusCode = HttpStatusCode.InternalServerError };

            try
            {

                for (int i = 0; i < chunkedContent.Count; i++)
                {
                    var multipartFormContent = new MultipartFormDataContent();

                    //int length = (int)Math.Min(CHUNKSIZE, file.FileContentStream.Length - i * CHUNKSIZE);
                    //byte[] chunkBytes = new byte[(int)chunkedFile.FileContentStreams[i].Length];

                    StreamContent chunkStream = new StreamContent(new MemoryStream(chunkedContent[i]));
                    chunkStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    StreamContent fileStream = new StreamContent(new MemoryStream(chunkedContent[i]));
                    fileStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    if (chunkedContent.Count > 1)
                    {
                        multipartFormContent.Add(content: chunkStream, name: "chunkFile", fileName: name);
                    }
                    multipartFormContent.Add(content: fileStream, name: "uploadFiles", fileName: name);
                    multipartFormContent.Add(new StringContent(chunkedContent.Count.ToString()), "total-chunk");
                    multipartFormContent.Add(new StringContent(i.ToString()), "chunk-index");
                    multipartFormContent.Add(new StringContent(operaId.ToString()), "operaId");
                    multipartFormContent.Add(new StringContent(batch.ToString()), "batchGuid");
                    multipartFormContent.Add(new StringContent(chunkSize.ToString()), "chunkSize");
                    if (parentGuid.HasValue && parentGuid.Value != Guid.Empty)
                    {
                        multipartFormContent.Add(new StringContent(parentGuid.Value.ToString()), "parentGuid"); ;
                    }
                    multipartFormContent.Add(new StringContent((compressionOptions == CompressionOptions.AlreadyCompressed).ToString()), "compressed");
                    multipartFormContent.Add(new StringContent(overwrite.ToString()), "overwrite");

                    chunks.Add(multipartFormContent);

                }

                GC.Collect();
                               

                for (int i = 0; i < chunks.Count; i++)
                {

                    Console.WriteLine("Uploading chunk " + i + " of " + chunkedContent.Count);
                    var response = await _httpClient.PostAsync(uri, chunks[i]);


                    result = new RequestResult()
                    {
                        ResponseStatusCode = response.StatusCode,
                    };

                    if (!response.StatusCode.IsSuccessful())
                    {
                        if (response.Content.Headers.ContentType?.MediaType == "application/json")
                        {
                            try
                            {
                                ErrorDto? errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
                                result.ResponseContentErrorData = errorDto!.ErrorData;

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                        break;
                    }

                }

                chunkedContent.Clear();
                chunks.Clear();

                result = new RequestResult { ResponseStatusCode = HttpStatusCode.OK };

                GC.Collect();

            }
            catch (Exception ex)
            {
                Console.WriteLine("File upload error on Client: " + ex.Message);

            }

            return result;

        }
    }

}
