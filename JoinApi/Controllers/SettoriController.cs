using Microsoft.AspNetCore.Mvc;
using JoinApi.Models;
using AutoMapper;
using ModelData.Dto;
using JoinApi.Service;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using ModelData.Utilities;
using ModelData.Dto.Error;
using JoinApi.Utilities;
using Microsoft.AspNetCore.Cors;
using Serilog;
using System.Text.Json;
using Humanizer;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class SettoriController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public SettoriController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        [EnableCors("corsPolicy")]
        [Authorize]
        public async Task<IActionResult> GetSettore(Guid id)
        {
            _logger.ForContext("Guid", id).
                    Information("Accesso controller SettoriController, richiesta GetSettore.");

            var settore = await _mongoDbService.SettoriCollection.Find(s => s.Id == id).FirstOrDefaultAsync();

            _logger.ForContext("Guid", id).ForContext("Settore", JsonSerializer.Serialize(settore)).
                    Information("Uscita controller SettoriController, richiesta GetSettore.");

            return Ok(_mapper.Map<SettoreDto>(settore));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SettoreDto>>> GetSettoriCliente(Guid ClienteId)
        {
            _logger.ForContext("Guid", ClienteId).
                    Information("Accesso controller SettoriController, richiesta GetSettoriCliente.");

            var settori = await _mongoDbService.SettoriCollection.FindAsync(s => s.ClienteId == ClienteId);

            _logger.ForContext("Guid", ClienteId).ForContext("Settore", JsonSerializer.Serialize(settori)).
                    Information("Uscita controller SettoriController, richiesta GetSettoriCliente.");

            return Ok(_mapper.Map<List<SettoreDto>>(settori.ToList()));
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public ActionResult PostSettore([FromBody] SettoreCreateDto dto)
        {
            _logger.ForContext("Settore", JsonSerializer.Serialize(dto)).
                    Information("Accesso controller SettoriController, richiesta PostSettore.");

            var settore = _mapper.Map<SettoreDoc>(dto);
            try
            {
                _mongoDbService.SettoriCollection.InsertOne(settore);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Error($"Uscita controller SettoriController, richiesta PostSettore. Dettaglio eccezione {e}.");
                throw;
            }

            _logger.
                Information($"Uscita controller SettoriController, richiesta PostSettore. Completata correttamente.");

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<SettoreDto>(settore));
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult PutSettore(Guid id, SettoreUpdateDto dto)
        {
            _logger.ForContext("Guid", id).ForContext("Settore", JsonSerializer.Serialize(dto)).
                Information("Accesso controller SettoriController, richiesta PutSettore.");

            if (dto.Id != id)
            {
                _logger.ForContext("Guid", id).
                    Warning($"Uscita controller SettoriController, richiesta PutSettore. Bad request id not equal.");

                return BadRequest("id not equal");
            }
            var update = Builders<SettoreDoc>.Update.Set(s => s.Nome, dto.Nome);

            UpdateResult? result;

            try
            {
                result = _mongoDbService.SettoriCollection.UpdateOne(s => s.Id == id, update);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Error($"Uscita controller SettoriController, richiesta PutSettore. Dettaglio eccezione {e}.");

                throw;
            }

            if (!result.IsAcknowledged)
            {
                _logger.
                    Warning($"Uscita controller SettoriController, richiesta PutSettore. Sector update error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "sector update error");
            }

            if (result.MatchedCount == 0)
            {
                _logger.
                    Warning($"Uscita controller SettoriController, richiesta PutSettore. Sector not found.");

                return NotFound("sector not found");
            }

            _logger.
                Information($"Uscita controller SettoriController, richiesta PutSettore. Completa correttamente.");

            return Ok();
        }

        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> DeleteSettore(Guid id)
        {
            _logger.ForContext("Guid", id).
                Information("Accesso controller SettoriController, richiesta DeleteSettore.");

            var settore = await _mongoDbService.SettoriCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (settore == null)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller SettoriController, richiesta DeleteSettore. Sector not found.");

                return StatusCode(StatusCodes.Status500InternalServerError, "sector not found");
            }

            //TODO GESTIONE ORFANI

            var settoriDeleteResult = await _mongoDbService.SettoriCollection.DeleteOneAsync(s => s.Id == id);

            if (!settoriDeleteResult.IsAcknowledged)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller SettoriController, richiesta DeleteSettore. Sector delete error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "sector delete error");
            }

            _logger.
                Information($"Uscita controller SettoriController, richiesta DeleteSettore. Completa correttamente.");

            return Ok();
        }
    }
}
