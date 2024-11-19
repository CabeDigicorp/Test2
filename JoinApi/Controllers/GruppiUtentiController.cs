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
using System.Security.Policy;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class GruppiUtentiController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public GruppiUtentiController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-opera")]
        public ActionResult GetGruppiUtentiByOpera(Guid operaId)
        {
            _logger.ForContext("Guid", operaId).
                    Information("Accesso controller GruppiUtentiController, richiesta GetGruppiUtentiByOpera.");

            var gruppi = _mongoDbService.GruppiUtentiCollection.Find(g => g.OperaId == operaId).ToList();

            _logger.ForContext("Guid", operaId).
                    Information("Uscita controller GruppiUtentiController, richiesta GetGruppiUtentiByOpera. Completata correttamente.");
            return Ok(_mapper.Map<List<GruppoUtentiDto>>(gruppi));
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public ActionResult PostGruppoUtenti([FromBody] GruppoUtentiCreateDto dto)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta PostGruppoUtenti.");

            var gruppoUtenti = _mapper.Map<GruppoUtentiDoc>(dto);
            try
            {
                _mongoDbService.GruppiUtentiCollection.InsertOne(gruppoUtenti);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Error($"Uscita controller GruppiUtentiController, richiesta PostGruppoUtenti. Dettaglio eccezione {e}.");
                
                throw;
            }
            _logger.
                Information("Uscita controller GruppiUtentiController, richiesta PostGruppoUtenti. Completata correttamente");
            
            return StatusCode(StatusCodes.Status201Created, _mapper.Map<GruppoUtentiDto>(gruppoUtenti));
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult PutGruppoUtenti(Guid id, GruppoUtentiUpdateDto dto)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta PutGruppoUtenti.");

            if (dto.Id != id)
            {
                _logger.
                    Warning("Uscita controller GruppiUtentiController, richiesta PutGruppoUtenti.");

                return BadRequest("id not equal");
            }
            var update = Builders<GruppoUtentiDoc>.Update.Set(t => t.Nome, dto.Nome);

            UpdateResult? result;

            try
            {
                result = _mongoDbService.GruppiUtentiCollection.UpdateOne(t => t.Id == id, update);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta PutGruppoUtenti. Dettaglio eccezione: {e}.");
                throw;
            }

            if (!result.IsAcknowledged)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta PutGruppoUtenti. Group update error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "group update error");
            }

            if (result.MatchedCount == 0)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta PutGruppoUtenti. Group not found.");

                return NotFound("group not found");
            }

            _logger.
                Warning($"Uscita controller GruppiUtentiController, richiesta PutGruppoUtenti. Completata correttamente.");

            return Ok();
        }

        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> DeleteGruppoUtenti(Guid id)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta DeleteGruppoUtenti.");

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (gruppo == null)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == gruppo.OperaId).FirstOrDefaultAsync();
            if (opera == null)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Opera not found.");
                return StatusCode(StatusCodes.Status500InternalServerError, "opera not found");
            }

            var settore = await _mongoDbService.SettoriCollection.Find(s => s.Id == opera.SettoreId).FirstOrDefaultAsync();
            if (settore == null)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Sector not found.");
                return StatusCode(StatusCodes.Status500InternalServerError, "sector not found");
            }

            if (!CurrentUserClientiIds.Contains(settore.ClienteId))
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var gruppiDeleteResult = await _mongoDbService.GruppiUtentiCollection.DeleteOneAsync(g => g.Id == id && g.OperaId == opera.Id);

            if (!gruppiDeleteResult.IsAcknowledged)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Group delete error.");
                return StatusCode(StatusCodes.Status500InternalServerError, "group delete error");
            }

            if (gruppiDeleteResult.DeletedCount == 0)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var utentiUpdate = Builders<UtenteDoc>.Update.Pull(u => u.GruppiIds, id);

            var utentiUpdateResult = await _mongoDbService.UtentiCollection.UpdateManyAsync(_ => true, utentiUpdate);

            if (!utentiUpdateResult.IsAcknowledged)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Utenti group pull error.");
                return StatusCode(StatusCodes.Status500InternalServerError, "utenti group pull error");
            }

            var teamsUpdate = Builders<TeamDoc>.Update.Pull(t => t.GruppiIds, id);

            var teamsUpdateResult = await _mongoDbService.TeamsCollection.UpdateManyAsync(_ => true, teamsUpdate);

            if (!teamsUpdateResult.IsAcknowledged)
            {
                _logger.
                    Warning($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Teams group pull error.");
                return StatusCode(StatusCodes.Status500InternalServerError, "teams group pull error");
            }

            _logger.
                Information($"Uscita controller GruppiUtentiController, richiesta DeleteGruppoUtenti. Completata correttamente.");
           
            return Ok();
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-team-opera-gruppi-ids")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> GetTeamGruppiIds(Guid operaId, Guid teamId)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta GetTeamGruppiIds.");

            //TODO verifica che opera sia effettivamente del cliente che effettua questa richiesta
            var team = await _mongoDbService.TeamsCollection.Find(t => t.Id == teamId).FirstAsync();
            var gruppi = await _mongoDbService.GruppiUtentiCollection.Find(g => g.OperaId == operaId && team.GruppiIds.Contains(g.Id)).ToListAsync();
            _logger.
                Information($"Uscita controller GruppiUtentiController, richiesta GetTeamGruppiIds. Completata correttamente.");
            
            return Ok(gruppi.Select(g => g.Id));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-utente-opera-gruppi-ids")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> GetUtenteGruppiIds(Guid operaId, Guid utenteId)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta GetUtenteGruppiIds.");

            //TODO verifica che opera sia effettivamente del cliente che effettua questa richiesta
            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == utenteId).FirstAsync();
            var gruppi = await _mongoDbService.GruppiUtentiCollection.Find(g => g.OperaId == operaId && utente.GruppiIds.Contains(g.Id)).ToListAsync();
            _logger.
                Information($"Uscita controller GruppiUtentiController, richiesta GetUtenteGruppiIds. Completata correttamente.");
            return Ok(gruppi.Select(g => g.Id));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GruppoUtentiDto>>>GetGruppiCliente(Guid clienteId)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta GetGruppiCliente.");

            var gruppi = (from g in _mongoDbService.GruppiUtentiCollection.AsQueryable(null)
                            join o in _mongoDbService.OpereCollection.AsQueryable(null) on g.OperaId equals o.Id
                            join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                            where s.ClienteId == clienteId
                            select g).Distinct();
            _logger.
                Information($"Uscita controller GruppiUtentiController, richiesta GetGruppiCliente. Completata correttamente.");
            return Ok(_mapper.Map<List<GruppoUtentiDto>>(gruppi.ToList()));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-team")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GruppoUtentiDto>>> GetGruppiByTeam(Guid teamId)
        {
            _logger.
                Information("Accesso controller GruppiUtentiController, richiesta GetGruppiByTeam.");

            var gruppi = (from g in _mongoDbService.GruppiUtentiCollection.AsQueryable(null)
                          from t in _mongoDbService.TeamsCollection.AsQueryable(null)
                          where t.GruppiIds.Contains(g.Id)
                          select g).Distinct();
            _logger.
                Information($"Uscita controller GruppiUtentiController, richiesta GetGruppiByTeam. Completata correttamente.");
            return Ok(_mapper.Map<List<GruppoUtentiDto>>(gruppi.ToList()));
        }

    }

}
