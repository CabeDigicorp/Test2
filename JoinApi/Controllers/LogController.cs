using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ModelData.Dto;
using JoinApi.Service;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using ModelData.Model;
using JoinApi.Utilities;
using MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;
using System.Reflection;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public LogController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [Route("post-log")]
        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<bool>> PostLog([FromBody] LogDto LogData)
        {
            try
            {
                //_logger.ForContext("LogDto", JsonSerializer.Serialize(LogData)).
                //    Information("Accesso controller LogController, richiesta PostLog.");

                var logToWrite = _logger
                    .ForContext("ClientId", LogData.Id)
                    .ForContext("ClientMessageType", LogData.Type)
                    .ForContext("ClientAssemblyName", LogData.AssemblyName)
                    .ForContext("ClientAssemblyVersion", LogData.AssemblyVersion)
                    .ForContext("ClientEnvironmentName", LogData.EnvironmentName)
                    .ForContext("ClientUser", LogData.User)
                    .ForContext("ClientRequestPath", LogData.RequestPath)
                    .ForContext("ClientRequestFunction", LogData.RequestFunction)
                    .ForContext("ClientRequestLinePath", LogData.RequestLine);

                switch (LogData.Type)
                {
                    case ModelData.Utilities.LogType.Verbose:
                        logToWrite.Verbose(LogData.Message ?? "");
                        break;
                    case ModelData.Utilities.LogType.Information:
                        logToWrite.Information(LogData.Message ?? "");
                        break;
                    case ModelData.Utilities.LogType.Warning:
                        logToWrite.Warning(LogData.Message ?? "");
                        break;
                    case ModelData.Utilities.LogType.Error:
                        logToWrite.Error(LogData.Message ?? "");
                        break;
                    case ModelData.Utilities.LogType.Fatal:
                        logToWrite.Fatal(LogData.Message ?? "");
                        break;
                    default:
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller LogController, richiesta PostLog. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }
    }
}