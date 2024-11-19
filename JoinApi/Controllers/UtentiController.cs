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
using JoinApi.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Cors;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("corsPolicy")]
    [Authorize(Roles = RuoliAuth0.REGISTERED)]
    public class UtentiController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        private readonly UserManager<UtenteDoc> _userManager;
        private readonly JwtSettings _jwtSettings;

        public UtentiController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, UserManager<UtenteDoc> userManager, IOptions<JwtSettings> jwtSettingsOptions, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettingsOptions.Value;
            _logger = logger;
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> PostUtente([FromBody] UtenteDto dto)
        {
            _logger.
                Information("Accesso controller UtentiController, richiesta PostUtente.");

            UtenteDoc utente = await _userManager.FindByEmailAsync(dto.Email);
            if (utente != null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta PostUtente. User already exists.");

                return Conflict(ErrorDtoBuilder.New.Add("Reason", "UserAlreadyExists").Build());
            }

            utente = new UtenteDoc
            {
                Email = dto.Email!.ToLower(),
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = dto.Email!.ToLower(),
                Nome = dto.Nome,
                Cognome = dto.Cognome,
                PrivacyConsent = dto.PrivacyConsent,
                Disabled = dto.Disabled
            };

            var createResult = await _userManager.CreateAsync(utente); //, model.Password);
            if (!createResult.Succeeded)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta PostUtente. Internal server error.");

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            //var roles = new List<string>() { RuoliAuth0.REGISTERED };
            //roles.Add(RuoliAuth0.REGISTERED);

            //var addToRoleResult = await _userManager.AddToRolesAsync(utente, roles);
            //if (!addToRoleResult.Succeeded)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            CurrentUser!.ForceUpdate = utente.Email == CurrentUserEmail;

            _logger.
                Information("Accesso controller UtentiController, richiesta PostUtente.");

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<UtenteDto>(utente));
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult PutUtente(Guid id, UtenteDto dto)
		{
            _logger.
                Information("Accesso controller UtentiController, richiesta PutUtente.");

            if (dto.Id != id)
			{
                _logger.
                   Warning("Uscita controller UtentiController, richiesta PutUtente. Id not equal.");

                return BadRequest("id not equal");
			}

            var utente = _mongoDbService.UtentiCollection.Find(u => u.Id == id).FirstOrDefault();
            if (utente == null)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta PutUtente. Utente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (!utente.Disabled && dto.Disabled && !AuthorizeRemoveUser(utente))
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta PutUtente. Errore - tentata disabilitazione dell'unico utente di un gruppo amministrativo.");

                return StatusCode(StatusCodes.Status406NotAcceptable, "Errore - tentata disabilitazione dell'unico utente di un gruppo amministrativo");
            }

            var update = Builders<UtenteDoc>.Update.Set(u => u.Nome, dto.Nome)
                                                   .Set(u => u.Cognome, dto.Cognome)
                                                   .Set(u => u.Email, dto.Email)
                                                   .Set(u => u.PrivacyConsent, dto.PrivacyConsent)
                                                   .Set(u => u.Disabled, dto.Disabled);
                                                   //.Set(u => u.Roles, dto.RolesList.Select(r => r.Id).ToList())
                                                   
			           

			UpdateResult? result;

			try
			{
				result = _mongoDbService.UtentiCollection.UpdateOne(u => u.Id == id, update);
			}
			catch (MongoWriteException e)
			{
				if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
				{
					return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
				}
                _logger.
                    Error($"Uscita controller UtentiController, richiesta PutUtente. Dettaglio eccezione {e}.");

                throw;
			}

            CurrentUser!.ForceUpdate = utente.Email == CurrentUserEmail;

            if (!result.IsAcknowledged)
			{
                _logger.
                    Warning("Uscita controller UtentiController, richiesta PutUtente. Team update error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "team update error");
			}

			if (result.MatchedCount == 0)
			{
                _logger.
                    Warning("Uscita controller UtentiController, richiesta PutUtente. Group not found.");

                return NotFound("group not found");
			}

            _logger.
                Information("Accesso controller UtentiController, richiesta PutUtente.");

            return Ok();
		}


		[HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-utente-by-email/{email}")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult<UtenteDto>> GetUtenteByEmail(string email)
        {
            _logger.
                Information("Accesso controller UtentiController, richiesta GetUtenteByEmail.");

            email = email.ToLower();
            var foundUser = await _mongoDbService.UtentiCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (foundUser == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta GetUtenteByEmail. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == CurrentUser.JoinInfo.Id).FirstOrDefaultAsync();
            var update = Builders<UtenteDoc>.Update.Push(u => u.FoundUsersIds, foundUser.Id);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == utente.Id, update);

            var utenteInfoDto = _mapper.Map<UtenteDto>(foundUser);
            _logger.
                Information("Accesso controller UtentiController, richiesta GetUtenteByEmail.");

            return Ok(utenteInfoDto);
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-utente-attuale")]
        [Authorize]
        public async Task<ActionResult<UtenteDto>> GetUtenteLoggedIn()
        {
            _logger.
                Information("Accesso controller UtentiController, richiesta GetUtenteLoggedIn.");

            if (CurrentUser == null)
            {
                _logger.
                    Information("Accesso controller UtentiController, richiesta GetUtenteLoggedIn. Completata correttamente.");
                return Ok(null);
            }


            string? email = CurrentUserEmail;

            if (!string.IsNullOrWhiteSpace(email))
            {
                //var utente = await _mongoDbService.UtentiCollection.Find(u => u.Email == email).FirstAsync();
                //if (utente != null)
                //{
                //    var dto = _mapper.Map<UtenteInfoDto>(utente);
                //    foreach (Guid rid in utente.Roles)
                //    {
                //        var ruolo = await _mongoDbService.RuoliCollection.Find(r => r.Id == rid).FirstAsync();
                //        dto.RolesList.Add(new RuoloInfoDto() { Id = ruolo.Id, Name = ruolo.Name });
                //    }
                //    return Ok(dto);
                //}

                var dto = new UtenteDto();

                var uAuth0 = CurrentUser?.Auth0Info;
                var uJoin = CurrentUser?.JoinInfo;
                
                if (uAuth0 != null)
                {
                    dto.Email = uAuth0.Email;
                    dto.Auth0Roles = new List<string>();
                    foreach (string? role in CurrentUser!.Auth0Roles)
                    {
                        if (!string.IsNullOrWhiteSpace(role)) dto.Auth0Roles.Add(role);
                    }

                    if (uJoin != null)
                    {
                        dto.Id = uJoin.Id;
                        dto.Cognome = uJoin.Cognome;
                        dto.Nome = uJoin.Nome;
                        dto.PrivacyConsent = uJoin.PrivacyConsent;
                        dto.Disabled = uJoin.Disabled;
                        dto.DomainManagerInfo = uJoin.DomainManagerInfo;
                    }
                    else
                    {
                        dto.Id = Guid.Empty;
                        dto.Cognome = uAuth0.LastName ;
                        dto.Nome = uAuth0.FirstName;
                        dto.PrivacyConsent = false;
                        dto.Disabled = false;
                    }

                    _logger.
                        Information("Accesso controller UtentiController, richiesta GetUtenteLoggedIn. Completata correttamente.");

                    return Ok(dto);
                }
                else
                {
                    _logger.
                        Error("Accesso controller UtentiController, richiesta GetUtenteLoggedIn. JOIN Login error: user does not exist.");
                    return Problem("JOIN Login error: user does not exist");
                }
            }
            else
            {
                //TODO
                _logger.
                    Error("Accesso controller UtentiController, richiesta GetUtenteLoggedIn. JOIN Login error: user has no associated email.");
                return Problem("JOIN Login error: user has no associated email");
            }
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-found-users")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> GetFoundUsers()
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetFoundUsers.");

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == CurrentUser.JoinInfo.Id).FirstAsync();
            var foundUsers = await _mongoDbService.UtentiCollection.Find(u => utente.FoundUsersIds.Contains(u.Id)).ToListAsync();

            _logger.
               Information("Accesso controller UtentiController, richiesta GetFoundUsers. Completata correttamente.");

            return Ok(_mapper.Map<IEnumerable<UtenteDto>>(foundUsers));
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("assign-utente-team")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> AssignUtenteTeam(AssignUtenteTeamDto dto)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta AssignUtenteTeam.");

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteTeam. Errore - Utente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (utente.TeamsIds.Contains(dto.TeamId))
            {
                //l'utente fa già parte del team
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteTeam. Already reported.");

                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var team = await _mongoDbService.TeamsCollection.Find(t => t.Id.Equals(dto.TeamId)).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteTeam. Errore - Team non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Team non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(team.ClienteId))
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteTeam. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.TeamsIds.Add(team.Id);
            var update = Builders<UtenteDoc>.Update.Set(u => u.TeamsIds, utente.TeamsIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteTeam. Errore inserimento utente in team.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore inserimento utente in team");
            }

            _logger.
               Information("Accesso controller UtentiController, richiesta AssignUtenteTeam. Completata correttamente.");

            return Ok();
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("remove-utente-team")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveUtenteTeam(AssignUtenteTeamDto dto)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta RemoveUtenteTeam.");

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteTeam. Errore - Utente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (!utente.TeamsIds.Contains(dto.TeamId))
            {
                //l'utente non fa parte del team
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteTeam. Already reported.");

                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var team = await _mongoDbService.TeamsCollection.Find(t => t.Id.Equals(dto.TeamId)).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteTeam. Errore - team non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - team non trovato");
            }

            if (!AuthorizeRemoveUser(utente, team))
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteTeam. Errore - tentata rimozione dell'unico utente da un gruppo amministrativo.");

                return StatusCode(StatusCodes.Status406NotAcceptable, "Errore - tentata rimozione dell'unico utente da un gruppo amministrativo");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(team.ClienteId))
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteTeam. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.TeamsIds.Remove(team.Id);
            var update = Builders<UtenteDoc>.Update.Set(u => u.TeamsIds, utente.TeamsIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteTeam. Errore rimozione utente da team.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione utente da team");
            }

            _logger.
               Information("Accesso controller UtentiController, richiesta RemoveUtenteTeam. Completata correttamente.");

            return Ok();
        }


        private bool AuthorizeRemoveUser(UtenteDoc utente, TeamDoc? team = null)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta AuthorizeRemoveUser.");

            if (utente == null) return false;
            if (team != null && !team.IsAdmin) return true;
            
            if (team == null)
            {
                // disabilito l'utente su tutti i team
                // devo verificare che ogni team amministrativo a cui l'utente appartiene abbia almeno un altro utente

                var adminTeams = _mongoDbService.TeamsCollection.Find(t => utente.TeamsIds.Contains(t.Id) && t.IsAdmin).ToList();
                if (adminTeams != null && adminTeams.Count > 0)
                {
                    foreach (var adminTeam in adminTeams)
                    {
                        var utentiAdmin = _mongoDbService.UtentiCollection.Find(u => u.Id != utente.Id && u.TeamsIds.Contains(adminTeam.Id)).CountDocuments();
                        if (utentiAdmin == 0) return false;
                    }
                }
                _logger.
                    Information("Accesso controller UtentiController, richiesta AuthorizeRemoveUser. Completata correttamente (true).");

                return true;
            }
            else
            {
                // rimuovo l'utente dal team
                // devo verificare che il team abbia almeno un altro utente

                var utentiAdmin = _mongoDbService.UtentiCollection.Find(u => u.Id != utente.Id && u.TeamsIds.Contains(team.Id)).CountDocuments();
                if (utentiAdmin > 0) return true;

                var adminTeams = _mongoDbService.TeamsCollection.Find(t => t.Id != team.Id && t.IsAdmin && t.ClienteId == team.ClienteId).ToList();
                if (adminTeams.Count > 0)
                {
                    foreach (var adminTeam in adminTeams)
                    {
                        utentiAdmin = _mongoDbService.UtentiCollection.Find(u => u.TeamsIds.Contains(adminTeam.Id)).CountDocuments();
                        if (utentiAdmin > 0) return true;
                    }
                }

            }

            _logger.
               Information("Accesso controller UtentiController, richiesta AuthorizeRemoveUser. Completata correttamente (false).");

            return false;
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("assign-utente-gruppo")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> AssignUtenteGruppi(AssignUtenteGruppoDto dto)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta AssignUtenteGruppi.");

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteGruppi. Errore - Utente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (utente.GruppiIds.Contains(dto.GruppoId))
            {
                //l'utente fa già parte del gruppo
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteGruppi. Already reported.");

                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id.Equals(dto.GruppoId)).FirstOrDefaultAsync();
            if (gruppo == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteGruppi. Gruppo non trovato.");

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
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteGruppi. Errore - Opera, settore o cliente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera, settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteGruppi. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.GruppiIds.Add(gruppo.Id);
            var update = Builders<UtenteDoc>.Update.Set(u => u.GruppiIds, utente.GruppiIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta AssignUtenteGruppi. Errore inserimento utente in gruppo.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore inserimento utente in gruppo");
            }

            _logger.
               Information("Accesso controller UtentiController, richiesta AssignUtenteGruppi. Completata correttamente.");

            return Ok();
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("remove-utente-gruppo")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveUtenteGruppi(AssignUtenteGruppoDto dto)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta RemoveUtenteGruppi.");

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteGruppi. Errore - Utente non trovato.");
                
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (!utente.GruppiIds.Contains(dto.GruppoId))
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteGruppi. Already reported.");

                //l'utente non fa parte del gruppo
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id.Equals(dto.GruppoId)).FirstOrDefaultAsync();
            if (gruppo == null)
            {
                _logger.
                    Warning("Uscita controller UtentiController, richiesta RemoveUtenteGruppi. Errore - Gruppo non trovato.");

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
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteGruppi. Errore - Opera, settore o cliente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera, settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteGruppi. Errore - Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.GruppiIds.Remove(gruppo.Id);
            var update = Builders<UtenteDoc>.Update.Set(u => u.GruppiIds, utente.GruppiIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteGruppi. Errore rimozione utente da gruppo.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione utente da gruppo");
            }

            _logger.
               Information("Accesso controller UtentiController, richiesta RemoveUtenteGruppi. Completata correttamente.");

            return Ok();
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        [Route("remove-utente-opera")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveUtenteOpera(RemoveUtenteOperaDto dto)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta RemoveUtenteOpera.");

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteOpera. Errore - Utente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == dto.OperaId).FirstOrDefaultAsync();
            if (opera == null)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteOpera. Errore - Opera non trovata.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera non trovata");
            }

            var gruppi = _mongoDbService.GruppiUtentiCollection.Find(g => g.OperaId.Equals(dto.OperaId) && utente.GruppiIds.Contains(g.Id)).ToList();
            if (gruppi == null || gruppi.Count() == 0)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteOpera. Already reported.");

                //l'utente non fa parte di alcun gruppo dell'opera
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var clienteId = (from s in _mongoDbService.SettoriCollection.AsQueryable(null)
                             where s.Id == opera.SettoreId
                             select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteOpera. Errore - settore o cliente non trovato.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteOpera. Permessi mancanti.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.GruppiIds.RemoveAll(id => gruppi.Select(g => g.Id).Contains(id));
            var update = Builders<UtenteDoc>.Update.Set(u => u.GruppiIds, utente.GruppiIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                _logger.
                   Warning("Uscita controller UtentiController, richiesta RemoveUtenteOpera. Errore rimozione utente da gruppi.");

                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione utente da gruppi");
            }

            _logger.
               Information("Accesso controller UtentiController, richiesta RemoveUtenteOpera. Completata correttamente.");

            return Ok();
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-gruppo-opera-utenti-ids")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> GetGruppoUtentiIds(Guid operaId, Guid gruppoId)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetGruppoUtentiIds.");

            //TODO verifica che opera sia effettivamente del cliente che effettua questa richiesta
            //var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id == gruppoId && g.OperaId == operaId).FirstAsync();
            var utenti = await _mongoDbService.UtentiCollection.Find(u => u.GruppiIds.Contains(gruppoId)).ToListAsync();

            _logger.
               Information("Accesso controller UtentiController, richiesta GetGruppoUtentiIds. Completata correttamente.");

            return Ok(utenti.Select(u => u.Id));

        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta Logout.");

            CurrentUserLogout();

            _logger.
               Information("Uscita controller UtentiController, richiesta Logout. Completata correttamente.");

            return Ok();
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-gruppo")]
        public ActionResult GetUtentiByGruppo(Guid gruppoId)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetUtentiByGruppo.");

            var utenti = _mongoDbService.UtentiCollection.Find(u => u.GruppiIds.Contains(gruppoId)).ToList();

            _logger.
               Information("Uscita controller UtentiController, richiesta GetUtentiByGruppo. Completata correttamente.");

            return Ok(_mapper.Map<List<UtenteDto>>(utenti));

        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-opera")]
        public ActionResult GetUtentiByOpera(Guid operaId)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetUtentiByOpera.");

            //List<Guid> gruppi = (from g in _mongoDbService.GruppiUtentiCollection.AsQueryable()
            //                     where g.OperaId == operaId
            //                     select g.Id).ToList();
            //
            //List<UtenteDoc> utenti = new List<UtenteDoc>();
            //foreach (var u in _mongoDbService.UtentiCollection.AsQueryable())
            //{
            //    foreach (var g in u.GruppiIds.AsQueryable())
            //    {
            //        if (gruppi.Contains(g) && !utenti.Contains(u))
            //        {
            //            utenti.Add(u);
            //            break;
            //        }
            //    }
            //}

            var utenti = (from u in _mongoDbService.UtentiCollection.AsQueryable(null)
                         from g in _mongoDbService.GruppiUtentiCollection.AsQueryable(null)
                         where g.OperaId == operaId
                               && u.GruppiIds.Contains(g.Id)
                         select u).Distinct().ToList();

            _logger.
               Information("Uscita controller UtentiController, richiesta GetUtentiByOpera. Completata correttamente.");

            return Ok(_mapper.Map<List<UtenteDto>>(utenti));

        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente")]
        public ActionResult GetUtentiByCliente(Guid clienteId, bool? disabled = null)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetUtentiByCliente.");

            var cliente = _mongoDbService.ClientiCollection.Find(c => c.Id == clienteId).FirstOrDefault();

            var utenti = (from u in _mongoDbService.UtentiCollection.AsQueryable()
                         from d in cliente.DominiAssociati
                         where (disabled == null || u.Disabled == disabled)
                            && u.Email != null && u.Email.ToLowerInvariant().EndsWith("@" + d.ToLowerInvariant())
                         orderby d, u.Email
                         select u).Distinct().ToList();

            _logger.
               Information("Uscita controller UtentiController, richiesta GetUtentiByCliente. Completata correttamente.");

            return Ok(_mapper.Map<List<UtenteDto>>(utenti));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-team")]
        public ActionResult GetUtentiByTeam(Guid teamId, bool? disabled = null)
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetUtentiByTeam.");

            var utenti = _mongoDbService.UtentiCollection.Find(u => u.TeamsIds.Contains(teamId) && (disabled == null || u.Disabled == disabled)).ToList();

            _logger.
               Information("Uscita controller UtentiController, richiesta GetUtentiByTeam. Completata correttamente.");

            return Ok(_mapper.Map<List<UtenteDto>>(utenti));
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-utenti-noti")]
        public ActionResult GetUtentiNoti()
        {
            _logger.
               Information("Accesso controller UtentiController, richiesta GetUtentiNoti.");

            List<UtenteInfoWithClienteDto> result = new List<UtenteInfoWithClienteDto>();

            foreach (ClienteDoc cliente in CurrentUser.JoinInfo.Clienti)
            {
                List<UtenteDoc> utenti = (from t in _mongoDbService.TeamsCollection.AsQueryable(null)
                                          from u in _mongoDbService.UtentiCollection.AsQueryable(null)
                                          where t.ClienteId == cliente.Id && u.TeamsIds.Contains(t.Id)
                                          select u).Distinct().ToList();
                foreach (var u in utenti)
                {
                    UtenteInfoWithClienteDto dto = _mapper.Map<UtenteInfoWithClienteDto>(u);
                    dto.Cliente = cliente.Info;

                    result.Add(dto);
                }

            }

            var utente = _mongoDbService.UtentiCollection.Find(u => u.Id == CurrentUser.JoinInfo.Id).First();
            var foundUsers = _mongoDbService.UtentiCollection.Find(u => utente.FoundUsersIds.Contains(u.Id) && !(result.Select(r => r.Id).Contains(u.Id))).ToList();
            foreach (var u in foundUsers)
            {
                UtenteInfoWithClienteDto dto = _mapper.Map<UtenteInfoWithClienteDto>(u);
                dto.Cliente = "[Altri utenti conosciuti]";
                
                result.Add(dto);
            }

            _logger.
               Information("Uscita controller UtentiController, richiesta GetUtentiNoti. Completata correttamente.");

            return Ok(result);

        }

        [HttpDelete]
        [EnableCors("corsPolicy")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<IActionResult> DeleteUtente(Guid? id, string? email)
        {
            _logger.
               Information("Accesso ed uscita controller UtentiController, richiesta DeleteUtente. Not implemented");

            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}
