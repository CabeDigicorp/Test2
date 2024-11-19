using AutoMapper;
using Humanizer;
using Jint;
using JoinApi.Extensions;
using JoinApi.Models;
using JoinApi.Service;
using JoinApi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.JSInterop;
using ModelData.Dto;
using ModelData.Dto.Error;
using ModelData.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.ComponentModel.DataAnnotations;
using System.IO;
using static JoinApi.Service.MongoDbService;
using ICSharpCode.SharpZipLib.GZip;
using System;
using ZstdSharp;
using Microsoft.AspNetCore.Cors;
using JoinWebUI.Utilities;
using System.ComponentModel;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    public class AllegatiController : ControllerBaseWithUserInfo
    {
        private readonly thatOpenHelper _fragmentsHelper;

        private struct BatchAndName
        {
            public Guid BatchGuid { get; private set; }
            public string FileName { get; private set; }

            public BatchAndName(Guid batch, string name)
            {
                BatchGuid = batch;
                FileName = name;
            }
        }

        private class StreamWithTimeStamp
        {
            public MemoryStream Stream { get; private set; }

            public int ChunkCounter { get; private set; }
            public DateTime LastModified { get; private set; }

            public StreamWithTimeStamp(int? capacity)
            {
                Stream = capacity != null ? new MemoryStream(capacity.Value) : new MemoryStream();
                RefreshLastModified();
                ChunkCounter = 0;
            }

            public void RefreshLastModified()
            {
                LastModified = DateTime.UtcNow;
            }

            public void IncreaseChunkCounter()
            {
                ChunkCounter++;
                RefreshLastModified();
            }
        }


        private static Dictionary<BatchAndName, StreamWithTimeStamp> _uploadingFiles = new Dictionary<BatchAndName, StreamWithTimeStamp>();
        private static Mutex _syncMutex = new Mutex();
        private static System.Timers.Timer _timer = new System.Timers.Timer(60000);
        private const int VALIDITY_SECONDS = 60;
        private readonly Serilog.ILogger _logger;

        //private IJSRuntime _jsRuntime;
        //private IJSObjectReference? _jsModule;

        public AllegatiController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;

            _logger.
                Information($"Inizializzazione controller AllegatiController, costruttore.");

            if (!_timer.Enabled)
            {
                SetTimer();
            }

            _fragmentsHelper = new thatOpenHelper(_mongoDbService);
            _fragmentsHelper.PropertyChanged += OnFragmentsHelperPropertyChanged;
        }

        private async void OnFragmentsHelperPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                default:
                    break;
            }
        }

        private static void SetTimer()
        {
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnTimerExpired;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private static void OnTimerExpired(Object? source, System.Timers.ElapsedEventArgs e)
        {
            PurgeInactiveFileTransfers();
        }

        private static void PurgeInactiveFileTransfers()
        {
            Log.
                Verbose($"Controller AllegatiController, rilascio risorse per trasferimento file inattivi. Inizio procedura in corso...");

            _syncMutex.WaitOne();

            DateTime now = DateTime.UtcNow;
            
            for (int i = _uploadingFiles.Count - 1; i >= 0; i--)
            {
                if ((now - _uploadingFiles.ElementAt(i).Value.LastModified ).TotalSeconds > VALIDITY_SECONDS)
                {
                    _uploadingFiles.Remove(_uploadingFiles.ElementAt(i).Key);
                }
            }
            _uploadingFiles.TrimExcess();

            //for (int i = _downloadableFiles.Count - 1; i >= 0; i--)
            //{
            //    if ((now - _downloadableFiles.ElementAt(i).Value.TimeStamp).TotalSeconds > VALIDITY_SECONDS)
            //    {
            //        _downloadableFiles.Remove(_downloadableFiles.ElementAt(i).Key);
            //    }
            //}
            //_downloadableFiles.TrimExcess();
            
            GC.Collect();

            _syncMutex.ReleaseMutex();

            Log.
               Verbose($"Controller AllegatiController, rilascio risorse per trasferimento file inattivi. Completato correttamente.");
        }



        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> GetAllegatiInfo(Guid? operaId, Guid? progettoId)
        {
            _logger.ForContext("Guid", operaId).ForContext("ProgettId", progettoId).
                Information($"Accesso controller AllegatiController, richiesta GetAllegatiInfo.");

            ProgettoDoc? progetto = null;
            if (progettoId.HasValue && progettoId.Value != Guid.Empty)
            {
                progetto = _mongoDbService.ProgettiCollection.Find(p => p.Id == progettoId).FirstOrDefault();
                if (progetto == null)
                {
                    _logger.ForContext("Guid", operaId).ForContext("ProgettId", progettoId).
                        Warning($"Uscita controller AllegatiController, richiesta GetAllegatiInfo. Record not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Progetto").Build());
                }
                if (!operaId.HasValue || operaId.Value == Guid.Empty)
                {
                    operaId = progetto.OperaId;
                }
                if (operaId.Value != progetto.OperaId)
                {
                    _logger.ForContext("Guid", operaId).ForContext("ProgettId", progettoId).
                        Warning($"Uscita controller AllegatiController, richiesta GetAllegatiInfo. Bad request.");

                    return BadRequest("Project does not belong to Opera");
                }                
            }
            
            var opera = _mongoDbService.OpereCollection.Find(o => o.Id == operaId).FirstOrDefault();
            if (opera == null)
            {
                _logger.ForContext("Guid", operaId).ForContext("ProgettId", progettoId).
                    Warning($"Uscita controller AllegatiController, richiesta GetAllegatiInfo. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
            }

            //if (!CurrentUserClientiIds.Contains(opera.ClienteId))
            //{
            //    return Forbid();
            //}

            var filesInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["operaId"] == operaId!.Value.ToBsonBinaryData()).Select(f => new AllegatoDto
            {
                FileName = f.Filename,
                FileSize = f.Length,
                Compressed = f.Metadata.Contains("compressed") ? f.Metadata["compressed"].AsBoolean : false,
                UploadDateTime = f.UploadDateTime,
                OperaId = operaId!.Value,
                ParentId = f.Metadata.Contains("parentGuid") ? f.Metadata["parentGuid"]?.AsNullableGuid : null,
                Id = f.Metadata["uploadGuid"].AsGuid,
                //NumeroProgettiAllegato = 0,
                //NumeroProgettiModello = 0,

                ConversionState = f.Metadata.Contains("ConversionInProgress") && f.Metadata["ConversionInProgress"].AsBoolean ? AllegatoConversionState.ConversionInProgress : AllegatoConversionState.NotConverted
            }).ToList();

            if (progetto != null)
            {
                try
                {
                    List<AllegatoDto> ifcFiles = new List<AllegatoDto>();
                    List<AllegatoDto> otherFiles = new List<AllegatoDto>();
                    List<AllegatoDto> childrenFiles = new List<AllegatoDto>();

                    //considero solo i file associati al progetto.
                    ifcFiles = (from f in filesInfo.AsQueryable()
                               from m in _mongoDbService.Model3dFileCollection.AsQueryable(null)
                               from i in m.Items
                               where (m.ProgettoId == progetto.Id && f.FileName == i.FileName)
                               select f).Distinct().ToList();

                    otherFiles = (from f in filesInfo.AsQueryable()
                                  join a in _mongoDbService.AllegatiItemsCollection.AsQueryable(null) on f.FileName equals a.FileName
                                  where a.ProgettoId == progetto.Id
                                  select f).Distinct().ToList();

                    childrenFiles = (from f in filesInfo.AsQueryable()
                                     from parent in ifcFiles
                                     where f.ParentId == parent.Id
                                     select f).Distinct().ToList();

                    filesInfo = ifcFiles.Concat(otherFiles).Distinct().ToList();
                }
                catch(Exception ex)
                {
                    _logger.ForContext("Guid", operaId).ForContext("ProgettId", progettoId).
                    Error(ex.Message);
                }
                // altro sistema...
                //var allegati = from x in _mongoDbService.AllegatiItemsCollection.AsQueryable()
                //              where x.ProgettoId == progetto.Id
                //              select x.FileName;
                //var modelli = from x in _mongoDbService.Model3dFileCollection.AsQueryable()
                //              where x.ProgettoId == progetto.Id
                //              select x.FileName;
                //allFilesInfo = allFilesInfo.Where(f => allegati.Contains(f.FileName) || modelli.Contains(f.FileName));
            }

            //var filesInfo = allFilesInfo.Where(f => f.ParentId == null || f.ParentId == Guid.Empty).ToList();

            if (filesInfo != null && filesInfo.Any())
            {
                var projects = (from p in _mongoDbService.ProgettiCollection.AsQueryable()
                                where p.OperaId == operaId
                                select p.Id).ToList();

                foreach (var file in filesInfo)
                {
                    if (projects.Count > 0)
                    {
                        var allegati = _mongoDbService.AllegatiItemsCollection.Find(a => a.FileName == file.FileName && projects.Contains(a.ProgettoId)).ToList();
                        file.Progetti.AddRange(allegati.Select<AllegatiItemDoc, Guid>(x => x.ProgettoId));


                        //var modelli = _mongoDbService.Model3dFileCollection.CountDocuments(m => m.FileName == file.FileName && projects.Contains(m.ProgettoId));
                        var modelli = _mongoDbService.Model3dFileCollection.Find(m => projects.Contains(m.ProgettoId) && m.Items.Any(i => i.FileName == file.FileName)).ToList();
                        file.Progetti.AddRange(modelli.Select<Model3dFilesInfoDoc, Guid>(x => x.ProgettoId));
                        file.Progetti = file.Progetti.Distinct().ToList();
                    }

                    if (file.FileName.EndsWith(".ifc", StringComparison.OrdinalIgnoreCase))
                    {
                        if (file.ConversionState == AllegatoConversionState.NotConverted)
                        {

                            var modelConvCnt = filesInfo.Count(f => f.ParentId == file.Id && f.FileName.EndsWith(".frag", StringComparison.OrdinalIgnoreCase));
                            var modelPropCnt = filesInfo.Count(f => f.ParentId == file.Id && f.FileName.EndsWith(".prop", StringComparison.OrdinalIgnoreCase));

                            if (modelPropCnt == 1 && modelConvCnt == 1)
                            {
                                file.ConversionState = AllegatoConversionState.Converted;
                            }
                            else if (modelPropCnt != 0 || modelConvCnt != 0)
                            {
                                file.ConversionState = AllegatoConversionState.Invalid;
                            }
                        }
                    }
                    else //resetto per gli altri file...
                    {
                        file.ConversionState = AllegatoConversionState.NotAvailable;
                    }
                }
            }
           
            await Task.CompletedTask;

            _logger.ForContext("Guid", operaId).ForContext("ProgettId", progettoId).
                Information($"Uscita controller AllegatiController, richiesta GetAllegatiInfo. Completata correttamente.");

            return Ok(filesInfo);
        }

        private Guid GetUserId()
        {
            var user = _mongoDbService.UtentiCollection.Find(u => u.UserName == User.Identity!.Name).First();
            return user.Id;
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("upload")]
        public async Task<IActionResult> UploadAllegati(IList<IFormFile> chunkFile,
                                                        IList<IFormFile> uploadFiles,
                                                        [FromForm(Name = "total-chunk")] int totalChunks,
                                                        [FromForm(Name = "chunk-index")] int chunkIndex,
                                                        [FromForm(Name = "operaId")] Guid operaId,
                                                        [FromForm(Name = "batchGuid")] Guid batchGuid,
                                                        [FromForm(Name = "chunkSize")] int chunkSize,
                                                        [FromForm(Name = "parentGuid")] Guid? parentGuid = null,
                                                        [FromForm(Name = "compressed")] bool compressed = false,
                                                        [FromForm(Name = "overwrite")] bool overwrite = false)
        {

            _logger.
                Information($"Accesso controller AllegatiController, richiesta UploadAllegati.");

            var opera = _mongoDbService.OpereCollection.Find(o => o.Id == operaId).FirstOrDefault();
            if (opera == null)
            {
                _logger.
                    Warning($"Uscita controller AllegatiController, richiesta UploadAllegati. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
            }
            if (batchGuid == Guid.Empty)
            {
                _logger.
                   Warning($"Uscita controller AllegatiController, richiesta UploadAllegati. No batch guid.");

                return BadRequest(ErrorDtoBuilder.New.Add("Reason", "NoBatchGuid"));
            }           
            if (!uploadFiles.Any() && !chunkFile.Any())
            {
                _logger.
                   Warning($"Uscita controller AllegatiController, richiesta UploadAllegati. No file submitted.");

                return BadRequest(ErrorDtoBuilder.New.Add("Reason", "NoFileSubmitted"));
            }
            if (totalChunks > 0 && totalChunks <= chunkIndex)
            {
                _logger.
                   Warning($"Uscita controller AllegatiController, richiesta UploadAllegati. Wrong total sizes.");

                return BadRequest(ErrorDtoBuilder.New.Add("Reason", "WrongTotalSizes"));
            }

            long fileSize = chunkSize * totalChunks;
            if (fileSize > int.MaxValue)
            {
                _logger.
                   Warning($"Uscita controller AllegatiController, richiesta UploadAllegati. File too large.");

                return BadRequest(ErrorDtoBuilder.New.Add("Reason", "FileTooLarge"));
            }


            try
            {
                foreach (var file in uploadFiles.ToList())
                {
                    var fileInfo = _mongoDbService.GetFilesInfo(f => f.Filename == file.FileName && f.Metadata["operaId"].AsGuid == operaId).FirstOrDefault();
                    if (fileInfo != null)
                    {
                        if (overwrite)
                        {
                            _mongoDbService.DeleteFile(fileInfo.Id);
                        }
                        else
                        {
                            _logger.
                                Information($"Uscita controller AllegatiController, richiesta UploadAllegati. Completata correttamente.");
                            
                            return Ok();
                            //return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Add("EntityType", "Allegato").Add("FileName", file.Name).Build());
                        }
                    }
                }

                var metadata = new Dictionary<string, object>() { { "operaId", operaId },
                                                                  { "compressed", compressed } };

                if (parentGuid.HasValue && parentGuid.Value != Guid.Empty)
                {
                    metadata.Add("parentGuid", parentGuid.Value);
                }

                if (chunkFile.Any())
                {
                    for (int i = 0; i < chunkFile.Count; i++)
                    {
                        var chunk = chunkFile[i];
                        var key = new BatchAndName(batchGuid, chunk.FileName);

                        _syncMutex.WaitOne();

                        if (!_uploadingFiles.ContainsKey(key))
                        {
                            _uploadingFiles.Add(key, new StreamWithTimeStamp((int)fileSize));
                        }
                      
                        _uploadingFiles[key].Stream.Position = chunkSize * chunkIndex;
                        chunk.CopyTo(_uploadingFiles[key].Stream);
                        _uploadingFiles[key].IncreaseChunkCounter();                      

                        if (_uploadingFiles[key].ChunkCounter == totalChunks)
                        {
                            //It's the final chunk
                            _uploadingFiles[key].Stream.Position = 0;

                            if (compressed)
                            {
                                var result = _mongoDbService.UploadFile(_uploadingFiles[key].Stream, chunk.FileName, chunk.ContentType, metadata);
                            }
                            else
                            {
                                MemoryStream tmp = new MemoryStream();
                                GZip.Compress(_uploadingFiles[key].Stream, tmp, false);
                                tmp.Position= 0;

                                metadata["compressed"] = true;

                                var result = _mongoDbService.UploadFile(tmp, chunk.FileName, chunk.ContentType, metadata);

                                tmp.Close();
                                tmp.Dispose();
                            }

                            _uploadingFiles[key].Stream.Close();
                            _uploadingFiles[key].Stream.Dispose();
                            _uploadingFiles.Remove(key);

                        }

                        chunk = null;

                        _syncMutex.ReleaseMutex();
                    }
                }
                else
                {
                    foreach (var file in Request.Form.Files.ToList())
                    {
                        
                        var result = _mongoDbService.UploadFile(file.OpenReadStream(), file.FileName, file.ContentType, metadata);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller AllegatiController, richiesta UploadAllegati. Dettaglio eccezione {ex}.");

                Console.WriteLine("File upload error on Server: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "file upload server error: " + ex.Message);
            }

            await Task.CompletedTask;
            _logger.
                Information($"Uscita controller AllegatiController, richiesta UploadAllegati. Completata correttamente.");

            return Ok();
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("info")]
        public async Task<IActionResult> GetAllegatoInfo(Guid operaId, Guid uploadGuid)
        {
            try
            {
                _logger.ForContext("OperaGuid", operaId).ForContext("UploadGuid", uploadGuid).
                    Information($"Accesso controller AllegatiController, richiesta GetAllegatoInfo.");

                var result = _mongoDbService.GetFilesInfo(f => f.Metadata["operaId"] == operaId.ToBsonBinaryData() && f.Metadata["uploadGuid"].AsGuid == uploadGuid)
                    .Select(f => new AllegatoDto
                    {
                        Id = uploadGuid,
                        FileName = f.Filename,
                        FileSize = f.Length,
                        Compressed = f.Metadata.Contains("compressed") ? f.Metadata["compressed"].AsBoolean : false,
                        UploadDateTime = f.UploadDateTime,
                        OperaId = operaId,
                        ParentId = f.Metadata.Contains("parentGuid") ? f.Metadata["parentGuid"]?.AsNullableGuid : null,
                        Progetti = new List<Guid>(),
                        ConversionState = f.Metadata.Contains("ConversionInProgress") && f.Metadata["ConversionInProgress"].AsBoolean ? AllegatoConversionState.ConversionInProgress : AllegatoConversionState.NotConverted
                    }).FirstOrDefault();

                if (result == null)
                {
                    _logger.ForContext("OperaGuid", operaId).ForContext("UploadGuid", uploadGuid).
                        Warning($"Uscita controller AllegatiController, richiesta GetAllegatoInfo. Non trovato.");
                    return NotFound();
                }
                else
                {
                    _logger.ForContext("Guid", uploadGuid).
                        Information($"Uscita controller AllegatiController, richiesta GetAllegatoInfo. Completata correttamente.");
                    return Ok(result);
                }

                // return result == null ? NotFound() : Ok(result);

            }
            catch (Exception ex)
            {
                _logger.ForContext("OperaGuid", operaId).ForContext("UploadGuid", uploadGuid).
                    Error($"Uscita controller AllegatiController, richiesta GetAllegatoInfo. Dettaglio eccezione {ex}.");

                return BadRequest(ErrorDtoBuilder.New.Add("Reason", ex.Message).Build());
            }
            //}
            //else
            //{
            //    return NotFound(ErrorDtoBuilder.New.Add("EntityType", "Allegato").Build());
            //}
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("download")]
        public async Task<IActionResult> DownloadAllegato(Guid operaId, Guid uploadGuid, bool decompress = true)
        {
            //if (_downloadableFiles.ContainsKey(uploadGuid))
            //{
            _logger.ForContext("OperaGuid", operaId).ForContext("UploadGuid", uploadGuid).
                Information($"Accesso controller AllegatiController, richiesta DownloadAllegato.");

            try
            {
                //var response = File(_downloadableFiles[uploadGuid].Content, _downloadableFiles[uploadGuid].ContentType!, _downloadableFiles[uploadGuid].Name);
                var dbFileResult = _mongoDbService.DownloadFile(f => f.Metadata["operaId"].AsGuid == operaId && f.Metadata["uploadGuid"].AsGuid == uploadGuid);

                byte[] fileBytes;

                bool compressed = dbFileResult.Compressed ?? false;
                if (compressed && decompress)
                {
                    var tmp = new MemoryStream();
                    GZip.Decompress(dbFileResult.ContentStream!, tmp, false);
                    tmp.Position = 0;
                    fileBytes = tmp.ToArray();
                }
                else
                {
                    fileBytes = new byte[dbFileResult.ContentStream!.Length];
                    dbFileResult.ContentStream.Read(fileBytes, 0, (int)dbFileResult.ContentStream.Length);
                }

                FileContentResult response = File(fileBytes, dbFileResult.ContentType!, dbFileResult.Name);

                dbFileResult.ContentStream!.Close();
                dbFileResult.ContentStream.Dispose();
                dbFileResult = null;

                //_downloadableFiles.Remove(uploadGuid);
                //_downloadableFiles.TrimExcess();

                _logger.ForContext("OperaGuid", operaId).ForContext("UploadGuid", uploadGuid).
                    Information($"Uscita controller AllegatiController, richiesta DownloadAllegato. Completata correttamente.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.ForContext("OperaGuid", operaId).ForContext("UploadGuid", uploadGuid).
                    Error($"Uscita controller AllegatiController, richiesta DownloadAllegato. Dettaglio eccezione {ex}.");

                return BadRequest(ErrorDtoBuilder.New.Add("Reason", ex.Message).Build());
            }
            //}
            //else
            //{
            //    return NotFound(ErrorDtoBuilder.New.Add("EntityType", "Allegato").Build());
            //}
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-download-ifc-ids")]
        public async Task<IActionResult>GetDownloadIfcUrls(Guid progettoId)
        {
            _logger.ForContext("Guid", progettoId).
                Information($"Accesso controller AllegatiController, richiesta GetDownloadIfcUrls.");


            var progetto = _mongoDbService.ProgettiCollection.Find(p => p.Id == progettoId).FirstOrDefault();
            if (progetto == null)
            {
                _logger.ForContext("Guid", progettoId).
                    Warning($"Uscita controller AllegatiController, richiesta GetDownloadIfcUrls. Record not found");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Progetto").Build());
            }

            var opera = _mongoDbService.OpereCollection.Find(o => o.Id == progetto.OperaId).FirstOrDefault();
            if (opera == null)
            {
                _logger.ForContext("Guid", progettoId).
                   Warning($"Uscita controller AllegatiController, richiesta GetDownloadIfcUrls. Record not found");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
            }

            var results = new List<IfcIdsDto>();

            //var modelli = _mongoDbService.Model3dFileCollection.Find(m => m.ProgettoId == progettoId).ToList();
            var modelli = (from m in _mongoDbService.Model3dFileCollection.AsQueryable(null)
                           from i in m.Items
                           where m.ProgettoId == progettoId && i.FileName.ToLower().EndsWith(".ifc")
                           select i.FileName).Distinct();

            foreach (var m in modelli)
            {
                DownloadFileResult? ifcFileResult = null;
                DownloadFileResult? fragFileResult = null;
                DownloadFileResult? propFileResult = null;

                try
                {
                    ifcFileResult = _mongoDbService.DownloadFile(f => f.Metadata["operaId"].AsGuid == opera.Id && f.Filename.ToLower() == m.ToLower());
                    if (ifcFileResult == null || !ifcFileResult.Found)
                    {
                        _logger.ForContext("Guid", progettoId).
                            Warning($"Uscita controller AllegatiController, richiesta GetDownloadIfcUrls. Base model not found: {m}.");

                        return NotFound(ErrorDtoBuilder.New.Add("Reason", "Base model not found: " + m).Build());
                    }

                    fragFileResult = _mongoDbService.DownloadFile(f => f.Metadata["parentGuid"].AsGuid == ifcFileResult.UploadGuid && f.Filename.ToLower().EndsWith(".frag"));
                    propFileResult = _mongoDbService.DownloadFile(f => f.Metadata["parentGuid"].AsGuid == ifcFileResult.UploadGuid && f.Filename.ToLower().EndsWith(".prop"));
                    //if (propFileResult == null || !propFileResult.Found)
                    //{
                    //    propFileResult = _mongoDbService.DownloadFile(f => f.Metadata["parentGuid"].AsGuid == ifcFileResult.UploadGuid && f.Filename.EndsWith(".prop.json"));
                    //}                    

                    Guid ifcGuid = ifcFileResult.UploadGuid!.Value;
                    Guid fragGuid = (fragFileResult != null && fragFileResult.Found) ? fragFileResult.UploadGuid!.Value : Guid.Empty;
                    Guid jsonGuid = (propFileResult != null && propFileResult.Found) ? propFileResult.UploadGuid!.Value : Guid.Empty;

                    results.Add(new IfcIdsDto()
                    {
                        IfcId = ifcGuid,
                        GeometriesId = fragGuid,
                        PropertiesId = jsonGuid
                    });
                    
                }
                catch (Exception ex)
                {
                    _logger.ForContext("Guid", progettoId).
                        Error($"Uscita controller AllegatiController, richiesta GetDownloadIfcUrls. Dettaglio eccezione: {ex}.");

                    return StatusCode(StatusCodes.Status500InternalServerError, ErrorDtoBuilder.New.Add("Reason", ex.Message).Build());
                }
                finally
                {
                    ifcFileResult?.ContentStream?.Dispose();
                    ifcFileResult = null;
                    fragFileResult?.ContentStream?.Dispose();
                    fragFileResult = null;
                    propFileResult?.ContentStream?.Dispose();
                    propFileResult = null;
                }
            }

            if (results.Count == 0)
            {
                _logger.ForContext("Guid", progettoId).
                    Warning($"Uscita controller AllegatiController, richiesta GetDownloadIfcUrls. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Modello IFC").Build());
            }

            _logger.ForContext("Guid", progettoId).
                Information($"Uscita controller AllegatiController, richiesta GetDownloadIfcUrls. Completata correttamente.");

            return Ok(results);
            
     

        }


        [HttpDelete("{uploadGuid}")]
        [EnableCors("corsPolicy")]
        public IActionResult DeleteAllegato(Guid uploadGuid)
        {
            _logger.ForContext("Guid", uploadGuid).
                Information($"Accesso controller AllegatiController, richiesta DeleteAllegato.");

            var fileInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["uploadGuid"].AsGuid == uploadGuid).FirstOrDefault();
            var childrenInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["parentGuid"].AsGuid == uploadGuid).Select(f=> f.Id);

            if (fileInfo == null)
            {
                _logger.ForContext("Guid", uploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta DeleteAllegato. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Allegato").Build());  
            }

            var opera = _mongoDbService.OpereCollection.Find(o => o.Id == fileInfo.Metadata["operaId"].AsGuid).FirstOrDefault();
            if (opera == null)
            {
                _logger.ForContext("Guid", uploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta DeleteAllegato. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
            }

            //if (!CurrentUserClientiIds.Contains(opera.ClienteId))
            //{
            //    return Forbid();
            //}

            if (childrenInfo?.Count() > 0)
            {
                int deletedChildren = 0;
                foreach (var childInfo in childrenInfo)
                {
                    deletedChildren += _mongoDbService.DeleteFile(childInfo) ? 1 : 0;
                }   
                if (deletedChildren != childrenInfo.Count())
                {
                    _logger.ForContext("Guid", uploadGuid).
                        Warning($"Uscita controller AllegatiController, richiesta DeleteAllegato. File children delete server error.");

                    return StatusCode(StatusCodes.Status500InternalServerError, "file children delete server error");
                }
            }

            var deleted = _mongoDbService.DeleteFile(fileInfo.Id);

            if(deleted)
            {
                _logger.ForContext("Guid", uploadGuid).
                Information($"Uscita controller AllegatiController, richiesta DeleteAllegato. Completata correttamente.");
                return Ok();
            }
            else
            {
                _logger.ForContext("Guid", uploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta DeleteAllegato. File delete server error.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            //return deleted ? Ok() : StatusCode(StatusCodes.Status500InternalServerError, "file delete server error");
        }

        [HttpPatch]
        [EnableCors("corsPolicy")]
        [Route("convert3D")]
        public async Task<IActionResult> Convert3D(Guid sourceUploadGuid)
        {
            _logger.ForContext("Guid", sourceUploadGuid).
                Information($"Accesso controller AllegatiController, richiesta Convert3D.");

            //return StatusCode(StatusCodes.Status501NotImplemented, "not yet available");

            var sourceInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["uploadGuid"].AsGuid == sourceUploadGuid).FirstOrDefault();
            var downloadResult = _mongoDbService.DownloadFile(f => f.Metadata["uploadGuid"].AsGuid == sourceUploadGuid);

            if (sourceInfo == null || downloadResult == null)
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta Convert3D. Not found.");

                return NotFound();
            }
            if (!sourceInfo.Filename.EndsWith(".ifc", StringComparison.OrdinalIgnoreCase))
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta Convert3D. Wrong file type.");

                return BadRequest("Wrong file type");
            }


            string fileName = sourceInfo.Filename.Substring(0, sourceInfo.Filename.LastIndexOf('.'));
            byte[] srcBytes = new byte[downloadResult.ContentStream!.Length];
            downloadResult.ContentStream!.Read(srcBytes, 0, (int)downloadResult.ContentStream!.Length);

            try
            {
                var operaId = sourceInfo.Metadata["operaId"].AsGuid;

                var fileInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["parentGuid"].AsNullableGuid == sourceUploadGuid);
                foreach (GridFSFileInfo file in fileInfo)
                {
                    _mongoDbService.DeleteFile(file.Id);
                }

                await _fragmentsHelper.ConvertModel(srcBytes, fileName, sourceUploadGuid, operaId);


                GC.Collect();


            }
            catch (Exception ex)
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Error($"Uscita controller AllegatiController, richiesta Convert3D. Dettaglio eccezione {ex}.");

                return StatusCode(StatusCodes.Status500InternalServerError, "file conversion error");
            }

            await Task.CompletedTask;
            _logger.ForContext("Guid", sourceUploadGuid).
                Information($"Uscita controller AllegatiController, richiesta Convert3D. Completata correttamente.");

            return Ok();
        }

        [HttpPatch]
        [EnableCors("corsPolicy")]
        [Route("unconvert3D")]
        public async Task<IActionResult> Unconvert3D(Guid sourceUploadGuid)
        {
            _logger.ForContext("Guid", sourceUploadGuid).
                Information($"Accesso controller AllegatiController, richiesta Unconvert3D.");

            var fileInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["uploadGuid"].AsGuid == sourceUploadGuid).FirstOrDefault();
            var childrenInfo = _mongoDbService.GetFilesInfo(f => f.Metadata["parentGuid"].AsGuid == sourceUploadGuid).Select(f => f.Id);

            if (fileInfo == null)
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta Unconvert3D. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Allegato").Build());
            }

            var opera = _mongoDbService.OpereCollection.Find(o => o.Id == fileInfo.Metadata["operaId"].AsGuid).FirstOrDefault();
            if (opera == null)
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta Unconvert3D. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
            }

            //if (!CurrentUserClientiIds.Contains(opera.ClienteId))
            //{
            //    return Forbid();
            //}

            if (childrenInfo == null || childrenInfo.Count() == 0)
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta Unconvert3D. Children records not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "ChildrenRecordsNotFound").Add("EntityType", "Allegato").Build());
            }

            int deleted = 0;
            foreach (var childInfo in childrenInfo)
            {
                deleted += _mongoDbService.DeleteFile(childInfo) ? 1 : 0;
                //var filter = Builders<BsonDocument>.Filter.In("Id", childrenInfo);
                //var update = Builders<BsonDocument>.Update.Unset("parentGuid");
                //var updateRes = _mongoDbService.GridFsFilesCollection.UpdateMany(filter, update);
            }

            _logger.ForContext("Guid", sourceUploadGuid).
                Information($"Uscita controller AllegatiController, richiesta Unconvert3D. Completata correttamente.");

            if (deleted == childrenInfo.Count())
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Information($"Uscita controller AllegatiController, richiesta Unconvert3D. Completata correttamente.");

                return Ok();
            }
            else
            {
                _logger.ForContext("Guid", sourceUploadGuid).
                    Warning($"Uscita controller AllegatiController, richiesta Unconvert3D. File children delete server error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "file children delete server error");
            }
            //return deleted == childrenInfo.Count() ? Ok() : StatusCode(StatusCodes.Status500InternalServerError, "file children delete server error");
        }
    }
}
