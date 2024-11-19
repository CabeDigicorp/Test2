using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JoinApi.Models;
using AutoMapper;
using ModelData.Dto;
using JoinApi.Service;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;


namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    public class PingController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PingController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, IHttpContextAccessor httpContextAccessor, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> GetPingReply()
        {
            _logger.Information("Accesso dal controller PingController, richiesta GetPingReply. Controllo stato connessione al datase Mongodb in corso...");
            //var hostName = _httpContextAccessor.HttpContext?.Request?.Host.Host;
            //if (hostName != null)
            //{
            //    _logger.Information($"Host che ha invocato il controller: {hostName}");
            //}
            //else
            //{
            //    _logger.Information($"Host non identificabile correttamente.");
            //}

            if (_mongoDbService == null || !_mongoDbService.ConnectionEstablished.GetValueOrDefault())
            {
                _logger.
                    Error("Uscita dal controller PingController, richiesta GetPingReply. Impossibile instaurare una connessione al database mongo, status 424.");
                return StatusCode(StatusCodes.Status424FailedDependency, "Mongo");
            } 

            try
            {
                var mongo = _mongoDbService.UtentiCollection.CountDocuments(x => true);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.
                   Error($"Uscita dal controller PingController, richiesta GetPingReply. Dettaglio eccezione {ex}.");

                return StatusCode(StatusCodes.Status424FailedDependency, "Mongo");
            }
        }
    }
}
