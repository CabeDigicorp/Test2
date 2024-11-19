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
using ModelData.Utilities;
using ModelData.Model;
using ModelData.Dto.Error;
using JoinApi.Utilities;
using MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Cors;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class Ruoli : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public Ruoli(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RuoloDto>>> GetRuoli()
        {
            _logger.
                Information("Accesso controller Ruoli, richiesta GetRuoli.");

            var ruoli = _mongoDbService.RuoliCollection.AsQueryable().ToList();

            _logger.
                Information("Uscita controller Ruoli, richiesta GetRuoli. Completata correttamente.");
            return Ok(_mapper.Map<List<RuoloDto>>(ruoli));
        }
    }
}
