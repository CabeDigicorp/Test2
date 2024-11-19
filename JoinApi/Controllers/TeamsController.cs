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

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class TeamsController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public TeamsController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-opera")]
        public ActionResult GetTeamsByOpera(Guid operaId)
        {
            _logger.ForContext("Guid", operaId).
                    Information("Accesso controller TeamsController, richiesta GetTeamsByOpera.");


            List<Guid> gruppi = (from g in _mongoDbService.GruppiUtentiCollection.AsQueryable()
                                 where g.OperaId == operaId
                                 select g.Id).ToList();

            List<TeamDoc> teams = new List<TeamDoc>();
            foreach (var t in _mongoDbService.TeamsCollection.AsQueryable())
            {
                foreach (var g in t.GruppiIds.AsQueryable())
                {
                    if (gruppi.Contains(g) && !teams.Contains(t))
                    {
                        teams.Add(t);
                        break;
                    }
                }
            }

            _logger.ForContext("Guid", operaId).
                   Information("Uscita controller TeamsController, richiesta GetTeamsByOpera. Completata correttamente.");

            return Ok(_mapper.Map<List<TeamDto>>(teams));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-gruppo")]
        public ActionResult GetTeamsByGruppo(Guid gruppoId)
        {
            _logger.ForContext("Guid", gruppoId).
                  Information("Accesso controller TeamsController, richiesta GetTeamsByGruppo.");

            var teams = _mongoDbService.TeamsCollection.Find(t => t.GruppiIds.Contains(gruppoId)).ToList();

            _logger.ForContext("Guid", gruppoId).
                 Information("Uscita controller TeamsController, richiesta GetTeamsByGruppo. Completata correttamente.");

            return Ok(_mapper.Map<List<TeamDto>>(teams));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente")]
        public ActionResult GetTeamsByCliente(Guid clienteId)
        {
            _logger.ForContext("Guid", clienteId).
                  Information("Accesso controller TeamsController, richiesta GetTeamsByCliente.");

            var teams = _mongoDbService.TeamsCollection.Find(t => t.ClienteId == clienteId).ToList();

            _logger.ForContext("Guid", clienteId).
                 Information("Uscita controller TeamsController, richiesta GetTeamsByCliente. Completata correttamente.");

            return Ok(_mapper.Map<List<TeamDto>>(teams));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente-utente")]
        public ActionResult GetTeamsByClienteUtente(Guid clienteId, Guid utenteId)
        {
            _logger.ForContext("Guid", clienteId).
                Information("Accesso controller TeamsController, richiesta GetTeamsByClienteUtente.");

            var utente = _mongoDbService.UtentiCollection.Find(u => u.Id == utenteId).FirstOrDefault();

            if (utente == null)
            {
                _logger.ForContext("Guid", clienteId).
                    Warning("Uscita controller TeamsController, richiesta GetTeamsByClienteUtente. Impossibile processare.");

                return UnprocessableEntity();
            }

            var teams = _mongoDbService.TeamsCollection.Find(t => t.ClienteId == clienteId && utente.TeamsIds.Contains(t.Id)).ToList();

            _logger.ForContext("Guid", clienteId).
                Information("Uscita controller TeamsController, richiesta GetTeamsByClienteUtente. Completata correttamente.");

            return Ok(_mapper.Map<List<TeamDto>>(teams));
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public ActionResult PostTeam([FromBody] TeamCreateDto dto)
        {
            _logger.
                Information("Accesso controller TeamsController, richiesta PostTeam.");

            var team = _mapper.Map<TeamDoc>(dto);
            try
            {
                _mongoDbService.TeamsCollection.InsertOne(team);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Error($"Uscita controller TeamsController, richiesta PostTeam. Dettaglio eccezione: {e}.");

                throw;
            }

            _logger.
                Information("Uscita controller TeamsController, richiesta PostTeam. Completata correttamente.");

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<TeamDto>(team));
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult PutTeam(Guid id, TeamUpdateDto dto)
        {
            _logger.
                Information("Accesso controller TeamsController, richiesta PutTeam.");

            if (dto.Id != id)
            {
                _logger.
                    Warning($"Uscita controller TeamsController, richiesta PostTeam. Id not equal.");

                return BadRequest("id not equal");
            }
            var update = Builders<TeamDoc>.Update.Set(t => t.Nome, dto.Nome)
                                                 .Set(t => t.IsAdmin, dto.IsAdmin)
                                                 .Set(t => t.IsLicensed, dto.IsLicensed);

            UpdateResult? result;

            try
            {
                result = _mongoDbService.TeamsCollection.UpdateOne(t => t.Id == id, update);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                   Error($"Uscita controller TeamsController, richiesta PostTeam. Dettaglio eccezione {e}.");
                throw;
            }

            if (!result.IsAcknowledged)
            {
                _logger.
                    Warning($"Uscita controller TeamsController, richiesta PostTeam. Team update error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "team update error");
            }

            if (result.MatchedCount == 0)
            {
                _logger.
                    Warning($"Uscita controller TeamsController, richiesta PostTeam. Not found.");

                return NotFound("group not found");
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult DeleteTeam(Guid id)
        {
            var team = _mongoDbService.TeamsCollection.Find(g => g.Id == id).FirstOrDefault();
            if (team == null)
            {
                _logger.
                    Warning($"Accesso controller TeamsController, richiesta DeleteTeam. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }


            if (!AuthorizeRemoveTeam(team))
            {
                _logger.
                    Warning($"Uscita controller TeamsController, richiesta DeleteTeam. Errore - tentata rimozione dell'unico gruppo amministrativo con utenti per il Cliente");

                return StatusCode(StatusCodes.Status406NotAcceptable, "Errore - tentata rimozione dell'unico gruppo amministrativo con utenti per il Cliente");
            }

            var teamsDeleteResult = _mongoDbService.TeamsCollection.DeleteOne(g => g.Id == id);

            if (!teamsDeleteResult.IsAcknowledged)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta DeleteTeam. Team delete error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "team delete error");
            }

            if (teamsDeleteResult.DeletedCount == 0)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta DeleteTeam. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var utentiUpdate = Builders<UtenteDoc>.Update.Pull(u => u.TeamsIds, id);

            var utentiUpdateResult = _mongoDbService.UtentiCollection.UpdateMany(_ => true, utentiUpdate);

            if (!utentiUpdateResult.IsAcknowledged)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta DeleteTeam. Team pull error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "team pull error");
            }

            _logger.
                Information($"Uscita controller TeamsController, richiesta DeleteTeam. Completata correttamente.");
            return Ok();
        }

        private bool AuthorizeRemoveTeam(TeamDoc team)
        {
            if (team == null) return false;

            if (!team.IsAdmin) return true;

            // sto cancellando il team, o rendendolo non amministrativo
            // devo accertarmi che ci sia almeno un altro team amministrativo con almeno un utente per il cliente 

            var otherTeams = (from t in _mongoDbService.TeamsCollection.AsQueryable(null)
                              from u in _mongoDbService.UtentiCollection.AsQueryable(null)
                              where t.Id != team.Id && t.IsAdmin && t.ClienteId == team.ClienteId && u.TeamsIds.Contains(t.Id)
                              select t).Distinct();

            return otherTeams.Count() > 0;
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("assign-team-gruppo")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> AssignTeamGruppo(AssignTeamGruppoDto dto)
        {
            _logger.
               Information($"Accesso controller TeamsController, richiesta AssignTeamGruppo.");

            var team = await _mongoDbService.TeamsCollection.Find(u => u.Id == dto.TeamId).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.
                    Warning($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Errore - Team non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Team non trovato");
            }

            if (team.GruppiIds.Contains(dto.GruppoId))
            {
                _logger.
                    Warning($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Il team non fa parte del gruppo.");

                //il team fa già parte del gruppo
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id.Equals(dto.GruppoId)).FirstOrDefaultAsync();
            if (gruppo == null)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Errore - Gruppo non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Gruppo non trovato");
            }

            var clienteId = (
                         from o in _mongoDbService.OpereCollection.AsQueryable(null)
                         join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                         where o.Id == gruppo.OperaId
                         select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Errore - Opera, settore o cliente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera, settore o cliente non trovato.");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            team.GruppiIds.Add(gruppo.Id);
            var update = Builders<TeamDoc>.Update.Set(t => t.GruppiIds, team.GruppiIds);
            var result = await _mongoDbService.TeamsCollection.UpdateOneAsync(t => t.Id == dto.TeamId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Errore inserimento team in gruppo.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore inserimento team in gruppo");
            }

            _logger.
                  Information($"Uscita controller TeamsController, richiesta AssignTeamGruppo. Completata correttamente.");

            return Ok();
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("remove-team-gruppo")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveTeamGruppo(AssignTeamGruppoDto dto)
        {
            _logger.
               Information($"Accesso controller TeamsController, richiesta RemoveTeamGruppo.");

            var team = await _mongoDbService.TeamsCollection.Find(u => u.Id == dto.TeamId).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Team non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Team non trovato");
            }

            if (!team.GruppiIds.Contains(dto.GruppoId))
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Il team no fa parte del gruppo.");

                //il team no fa parte del gruppo
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id.Equals(dto.GruppoId)).FirstOrDefaultAsync();
            if (gruppo == null)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Gruppo non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Gruppo non trovato");
            }

            var clienteId = (
                         from o in _mongoDbService.OpereCollection.AsQueryable(null)
                         join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                         where o.Id == gruppo.OperaId
                         select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Opera, settore o cliente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera, settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            team.GruppiIds.Remove(gruppo.Id);
            var update = Builders<TeamDoc>.Update.Set(t => t.GruppiIds, team.GruppiIds);
            var result = await _mongoDbService.TeamsCollection.UpdateOneAsync(t => t.Id == dto.TeamId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore rimozione team da gruppo.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione team da gruppo");
            }

            _logger.
                Information($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Completata correttamente.");

            return Ok();
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("remove-team-opera")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveTeamOpera(RemoveTeamOperaDto dto)
        {
            _logger.
                Information($"Accesso controller TeamsController, richiesta RemoveTeamOpera.");

            var team = await _mongoDbService.TeamsCollection.Find(u => u.Id == dto.TeamId).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Team non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Team non trovato");
            }

            var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == dto.OperaId).FirstOrDefaultAsync();
            if (opera == null)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Opera non trovata.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera non trovata");
            }

            var gruppi = _mongoDbService.GruppiUtentiCollection.Find(g => g.OperaId.Equals(dto.OperaId) && team.GruppiIds.Contains(g.Id)).ToList();
            if (gruppi == null || gruppi.Count() == 0)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Il team non fa parte di alcun gruppo dell'opera.");

                //il team non fa parte di alcun gruppo dell'opera
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var clienteId = (from s in _mongoDbService.SettoriCollection.AsQueryable(null)
                             where s.Id == opera.SettoreId
                             select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - settore o cliente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                _logger.
                  Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            team.GruppiIds.RemoveAll(id => gruppi.Select(g => g.Id).Contains(id));
            var update = Builders<TeamDoc>.Update.Set(t => t.GruppiIds, team.GruppiIds);
            var result = await _mongoDbService.TeamsCollection.UpdateOneAsync(t => t.Id == dto.TeamId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                   Warning($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Errore rimozione team da gruppo.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione team da gruppi");
            }

            _logger.
                   Information($"Uscita controller TeamsController, richiesta RemoveTeamGruppo. Completata correttamente.");

            return Ok();
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-gruppo-opera-teams-ids")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> GetGruppoTeamsIds(Guid operaId, Guid gruppoId)
        {
            _logger.
               Warning($"Accesso controller TeamsController, richiesta GetGruppoTeamsIds.");

            //TODO verifica che opera sia effettivamente del cliente che effettua questa richiesta
            //var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id == gruppoId && g.OperaId == operaId).FirstAsync();
            var teams = await _mongoDbService.TeamsCollection.Find(t => t.GruppiIds.Contains(gruppoId)).ToListAsync();

            _logger.
                Information($"Uscita controller TeamsController, richiesta GetGruppoTeamsIds. Completata correttamente.");

            return Ok(teams.Select(t => t.Id));
        }
    }
}
