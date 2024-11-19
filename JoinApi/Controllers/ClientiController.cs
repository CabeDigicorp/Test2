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
using System.Collections.Generic;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Cors;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class ClientiController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public ClientiController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        [EnableCors("corsPolicy")]
        [Authorize]
        public async Task<IActionResult> GetCliente(Guid id)
        {
            _logger.ForContext("Guid", id).
                Information("Accesso controller ClientiController, richiesta GetCliente.");

            try
            {
                ClienteDoc cliente = await _mongoDbService.ClientiCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
                _logger.ForContext("Guid", id).
                    Information("Uscita controller ClientiController, richiesta GetCliente. Completata correttamente.");

                return Ok(_mapper.Map<ClienteDto>(cliente));
            }
            catch (Exception ex)
            {
                _logger.ForContext("Guid", id).
                    Error($"Uscita controller ClientiController, richiesta GetCliente. Dettaglio eccezione: {ex}.");
                throw;
            }
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-teams")]
        [Authorize]
        public async Task<IActionResult> GetClientiByTeams()
        {
            _logger.
                Information("Accesso ed uscita controller ClientiController, richiesta GetClientiByTeams.");
            return Ok(_mapper.Map<IEnumerable<ClienteDto>>(CurrentUser.JoinInfo?.Clienti));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("check-domain-free")]
        [Authorize]
        public async Task<IActionResult> CheckDomainFree(string domain)
        {
            _logger.ForContext("Domain", domain).
                Information("Accesso controller ClientiController, richiesta CheckDomainFree.");

            var domini = _mongoDbService.ClientiCollection.AsQueryable().SelectMany(c => c.DominiAssociati);

            if (!domini.Contains(domain))
            {
                _logger.ForContext("Domain", domain).
                    Information("Uscita controller ClientiController, richiesta CheckDomainFree. Completata correttamente.");
                return Ok();
            }

            _logger.ForContext("Domain", domain).
                Warning("Uscita controller ClientiController, richiesta CheckDomainFree. Duplicate Key.");
            return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> PutCliente(Guid id, ClienteDto clienteInfoDto)
        {
            _logger.ForContext("Guid", id).
                Information("Accesso controller ClientiController, richiesta PutCliente.");

            if (id != clienteInfoDto.Id || !ValidateDto(clienteInfoDto))
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller ClientiController, richiesta PutCliente. Bad request.");
                return BadRequest();
            }

            var cliente = await _mongoDbService.ClientiCollection.Find(c => c.Id == id).FirstOrDefaultAsync();

            if (cliente == null)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller ClientiController, richiesta PutCliente. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            //TODO
            /*if (!CheckPermissions(cliente, Azioni.Default))
            {
                return Forbid();
            }*/

            var update = Builders<ClienteDoc>.Update
            .Set(p => p.CodiceCliente, clienteInfoDto.CodiceCliente)
            .Set(p => p.Nome, clienteInfoDto.Nome)
            .Set(p => p.DominiAssociati, clienteInfoDto.DominiAssociati)
            .Set(p => p.ChiaveLicenza, clienteInfoDto.ChiaveLicenza ?? cliente.ChiaveLicenza)
            .Set(p => p.ArchivioLicenze, clienteInfoDto.ArchivioLicenze ?? clienteInfoDto.ArchivioLicenze);


            UpdateResult? result;
            try
            {
                result = await _mongoDbService.ClientiCollection.UpdateOneAsync(c => c.Id == id, update);
            }
            catch (MongoWriteException e)
            {
                _logger.ForContext("Guid", id).Error($"Accesso controller ClientiController.Dettaglio eccezione {e.Message}.");

                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }

                _logger.ForContext("Guid", id).
                    Error($"Uscita controller ClientiController, richiesta PutCliente. Dettaglio eccezione {e}.");
                throw;
            }

            if (!result.IsAcknowledged)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller ClientiController, richiesta PutCliente. Db update error.");
                return StatusCode(StatusCodes.Status500InternalServerError, "db update error");
            }

            if (result.MatchedCount == 0)
            {
                _logger.ForContext("Guid", id).
                   Warning("Uscita controller ClientiController, richiesta PutCliente. Record not found.");
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            _logger.ForContext("Guid", id).
                   Information("Uscita controller ClientiController, richiesta PutCliente. Completata correttamente.");
            return Ok();
        }

        private bool ValidateDto(ClienteDto? dto)
        {
            bool res = dto != null
                       && !string.IsNullOrWhiteSpace(dto.CodiceCliente)
                       && !string.IsNullOrWhiteSpace(dto.Nome);
            return res;
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-utente-admin")]
        public ActionResult<List<ClienteDto>> GetClientiByUtenteAdmin()
        {
            _logger.
                Information("Accesso controller ClientiController, richiesta GetClientiByUtenteAdmin.");

            if (CurrentUser?.JoinInfo == null)
            {
                _logger.
                    Warning("Uscita controller ClientiController, richiesta GetClientiByUtenteAdmin. Non processabile.");
                
                return UnprocessableEntity();
            }

            var clienti = (from c in _mongoDbService.ClientiCollection.AsQueryable(null)
                           where CurrentUser.JoinInfo.Teams.Where(t => t.IsAdmin).Select(t => t.ClienteId).Contains(c.Id)
                           select c).Distinct().ToList();

            _logger.
                Information("Uscita controller ClientiController, richiesta GetClientiByUtenteAdmin. Completata correttamente.");

            return Ok(_mapper.Map<List<ClienteDto>>(clienti));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-licenziati-by-utente-attuale")]
        public ActionResult<List<ClienteDto>> GetClientiLicenzeByUtenteAttuale()
        {
            _logger.
               Information("Accesso controller ClientiController, richiesta GetClientiLicenzeByUtenteAttuale.");

            if (CurrentUser?.JoinInfo == null)
            {
                _logger.
                    Warning("Uscita controller ClientiController, richiesta GetClientiLicenzeByUtenteAttuale. Non processabile.");

                return UnprocessableEntity();
            }

            var clienti = (from c in _mongoDbService.ClientiCollection.AsQueryable(null)
                           where !string.IsNullOrWhiteSpace(c.ChiaveLicenza)
                                 && CurrentUser.JoinInfo.Teams.Where(t => t.IsLicensed).Select(t => t.ClienteId).Contains(c.Id)
                           select c).Distinct().ToList();

            List<LicenzaAuthDto> result = new List<LicenzaAuthDto>();
            foreach (var c in clienti)
            {
                result.Add(new LicenzaAuthDto(c.ChiaveLicenza!, c.CodiceCliente, c.Nome));
            }

            _logger.
                Information("Uscita controller ClientiController, richiesta GetClientiLicenzeByUtenteAttuale. Completata correttamente.");

            return Ok(_mapper.Map<List<LicenzaAuthDto>>(result));
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-licenza")]
        [Authorize]
        public async Task<ActionResult<LicenzaDto>> GetLicenza(Guid clienteId)
        {
            _logger.
               Information("Accesso controller ClientiController, richiesta GetLicenza.");

            var cliente = _mongoDbService.ClientiCollection.Find(c => c.Id == clienteId).FirstOrDefault();

            if (cliente == null) return StatusCode(StatusCodes.Status412PreconditionFailed);
            
            if (string.IsNullOrWhiteSpace(cliente.ChiaveLicenza)) { return NotFound(); }

            //se sono un utente amministrativo digicorp, recupero anche la chiave
            bool getKey = CurrentUser.Auth0Roles.Contains(RuoliAuth0.DIGICORP);

            LicenzaDto decoded = JoinLicense.DecodeLicense(cliente.ChiaveLicenza!, cliente.CodiceCliente, getKey);

            _logger.
               Information("Accesso controller ClientiController, richiesta GetLicenza. Completata correttamente.");

            return Ok(decoded);
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-licenze-archivio")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LicenzaDto>>> GetArchivioLicenze(Guid clienteId)
        {
            _logger.
               Information("Accesso controller ClientiController, richiesta GetArchivioLicenze.");

            var cliente = _mongoDbService.ClientiCollection.Find(c => c.Id == clienteId).FirstOrDefault();

            if (cliente == null) { return NotFound(); }

            //se è un utente amministrativo digicorp, invio anche la chiave
            bool getKey = CurrentUser.Auth0Roles.Contains(RuoliAuth0.DIGICORP);

            List<LicenzaDto> decoded = new List<LicenzaDto>();
            if (cliente.ArchivioLicenze != null)
            {
                foreach (var l in cliente.ArchivioLicenze)
                {
                    decoded.Add(JoinLicense.DecodeLicense(l, cliente.CodiceCliente, getKey));
                }
            }

            _logger.
               Information("Accesso controller ClientiController, richiesta GetArchivioLicenze. Completata correttamente.");

            return Ok(decoded);
        }

        [HttpPatch]
        [EnableCors("corsPolicy")]
        [Route("archivia-licenza")]
        [Authorize(Roles = RuoliAuth0.DIGICORP)]
        public async Task<ActionResult> ArchiviaLicenza(Guid clienteId)
        {
            _logger.
               Information("Accesso controller ClientiController, richiesta ArchiviaLicenza.");

            var cliente = _mongoDbService.ClientiCollection.Find(c => c.Id == clienteId).FirstOrDefault();

            if (cliente == null) return StatusCode(StatusCodes.Status412PreconditionFailed);
            if (string.IsNullOrWhiteSpace(cliente.ChiaveLicenza))  return NotFound();
            
            cliente.ArchivioLicenze ??= new List<string>();
            if (!cliente.ArchivioLicenze.Contains(cliente.ChiaveLicenza)) cliente.ArchivioLicenze.Add(cliente.ChiaveLicenza);
            cliente.ChiaveLicenza = string.Empty;
            
            ActionResult putResult = await PutCliente(cliente.Id, _mapper.Map<ClienteDto>(cliente));

            _logger.
               Information("Accesso controller ClientiController, richiesta ArchiviaLicenza. Completata correttamente.");

            return putResult;

        }

        [HttpPatch]
        [EnableCors("corsPolicy")]
        [Route("inserisci-licenza")]
        [Authorize(Roles = RuoliAuth0.DIGICORP)]
        public async Task<ActionResult<LicenzaDto>> InserisciLicenza(Guid clienteId, string chiaveLicenza)
        {
            _logger.
               Information("Accesso controller ClientiController, richiesta InserisciLicenza.");

            var cliente = _mongoDbService.ClientiCollection.Find(c => c.Id == clienteId).FirstOrDefault();

            if (cliente == null) return StatusCode(StatusCodes.Status412PreconditionFailed);
            if (!string.IsNullOrWhiteSpace(cliente.ChiaveLicenza)) return Conflict(); 
            //if (!cliente.ArchivioLicenze.Contains(chiaveLicenza))  return NotFound(); 

            LicenzaDto decoded = JoinLicense.DecodeLicense(chiaveLicenza, cliente.CodiceCliente, true);

            //if (!decoded.IsValid) return StatusCode(StatusCodes.Status304NotModified);

            cliente.ChiaveLicenza = chiaveLicenza;
            if (cliente.ArchivioLicenze != null) cliente.ArchivioLicenze.Remove(chiaveLicenza);
            ActionResult putResult = await PutCliente(cliente.Id, _mapper.Map<ClienteDto>(cliente));

            if ((putResult as StatusCodeResult)?.StatusCode == Ok().StatusCode) return Ok(decoded);

            _logger.
               Information("Accesso controller ClientiController, richiesta InserisciLicenza. Completata correttamente.");

            return putResult;
        }

        [HttpPatch]
        [EnableCors("corsPolicy")]
        [Route("elimina-licenza")]
        [Authorize(Roles = RuoliAuth0.DIGICORP)]
        public async Task<ActionResult<LicenzaDto>> DeleteLicenza(Guid clienteId, string chiaveLicenza)
        {
            _logger.
              Information("Accesso controller ClientiController, richiesta DeleteLicenza.");

            var cliente = _mongoDbService.ClientiCollection.Find(c => c.Id == clienteId).FirstOrDefault();

            if (cliente == null) return StatusCode(StatusCodes.Status412PreconditionFailed);
            if (cliente.ArchivioLicenze == null || !cliente.ArchivioLicenze.Contains(chiaveLicenza)) return NotFound();

            cliente.ArchivioLicenze.Remove(chiaveLicenza);
            ActionResult putResult = await PutCliente(cliente.Id, _mapper.Map<ClienteDto>(cliente));

            _logger.
               Information("Accesso controller ClientiController, richiesta DeleteLicenza. Completata correttamente.");

            return putResult;

        }


    }
}
