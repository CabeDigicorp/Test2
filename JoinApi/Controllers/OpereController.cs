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
using System.Linq;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;
//using Amazon.SecurityToken.Model;
using Serilog;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class OpereController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        //private UtenteDoc _utente;
        //private UtenteDoc Utente { 
        //    get
        //    {
        //        if(_utente == null)
        //        {
        //            _utente = _mongoDbService.UtentiCollection.Find(u => u.UserName == User.Identity!.Name).FirstOrDefault();
        //        }
        //        return _utente;
        //    }
        //}

        //private IEnumerable<ClienteDoc> _clienti;
        //private IEnumerable<ClienteDoc> Clienti { 
        //    get
        //    {
        //        if(_clienti == null)
        //        {
        //            //DEBUG
        //            //var utente = Utente;
        //            //_clienti = _mongoDbService.ClientiCollection.Find(c => utente.ClientiIds.Contains(c.Id)).ToList();
        //            _clienti = _mongoDbService.ClientiCollection.Find(c => true).ToList();
        //        }
        //        return _clienti;
        //    }
        //}
        //private IEnumerable<Guid> ClientiIds { 
        //    get
        //    {
        //        //DEBUG
        //        //return Utente.ClientiIds;
        //        List<Guid> res = new List<Guid>();
        //        res.Add(new Guid("e6f6011c-9aea-4103-b8e9-3041556bed8e"));
        //        return res;
        //    }
        //}

        public OpereController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger)
            : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<IEnumerable<OperaDto>>> GetOpere()
        {
            try
            {
                _logger.
                    Information("Accesso controller OpereController, richiesta GetOpere.");

                if (!IsInRole(RuoliAuth0.REGISTERED))
                {
                    _logger.
                        Warning("Uscita controller OpereController, richiesta GetOpere. Completata correttamente.");

                    return Forbid();
                }

                var opere = _mongoDbService.OpereCollection.AsQueryable().ToList();


                IList<OperaDto> opereDtos = new List<OperaDto>();
                foreach (var opera in opere)
                {
                    if (CheckPermissions(opera, null))
                    {
                        var operaDto = _mapper.Map<OperaDto>(opera);

                        Guid clienteId = GetClienteId(opera.SettoreId);

                        operaDto.ClienteId = clienteId;
                        opereDtos.Add(operaDto);
                    }
                }

                _logger.ForContext("OperaData", JsonSerializer.Serialize(opereDtos)).
                    Information("Uscita controller OpereController, richiesta GetOpere. Completata correttamente.");

                return Ok(opereDtos);
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta GetOpere. Dettaglio eccezione: {ex}");
                throw;
            }
        }

        [HttpGet("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<OperaDto>> GetOpera(Guid id)
        {
            try
            {
                _logger.ForContext("Guid", id).
                    Information("Accesso controller OpereController, richiesta GetOpera.");

                var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == id).FirstOrDefaultAsync();

                if (opera == null)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta GetOpera. RecordNotFound.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }

                var operaInfoDto = _mapper.Map<OperaDto>(opera);

                if (CheckPermissions(opera, Azioni.DefaultAction))
                {
                    Guid clienteId = GetClienteId(opera.SettoreId);
                    operaInfoDto.ClienteId = clienteId;
                }

                _logger.ForContext("Guid", id).ForContext("OperaInfo", JsonSerializer.Serialize(operaInfoDto)).
                       Information("Uscita controller OpereController, richiesta GetOpera. Completata correttamente.");

                return Ok(operaInfoDto);
            }
            catch (Exception ex)
            {
                _logger.Error($"Uscita controller OpereController, richiesta GetOpera. Dettaglio eccezione: {ex}");

                throw;
            }
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> PutOpera(Guid id, OperaUpdateDto operaUpdateDto)
        {
            try
            {
                _logger.ForContext("Guid", id).ForContext("OperaUpdateDto", JsonSerializer.Serialize(operaUpdateDto)).
                    Information("Accesso controller OpereController, richiesta PutOpera.");

                if (id != operaUpdateDto.Id)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta PutOpera. Bad request.");

                    return BadRequest();
                }

                //var clientiIds = CurrentUserClientiIds;

                //var opera = (from o in _mongoDbService.OpereCollection.AsQueryable()
                //             join c in clientiIds on o.ClienteId equals c
                //             where o.Id == id
                //             select o).FirstOrDefault();
                var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == id).FirstOrDefaultAsync();

                if (opera == null)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta PutOpera. Record not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }

                // controlla che i tag id forniti siano tutti di tag reali e appartenenti al cliente dell'opera
                if (_mongoDbService.TagsCollection.CountDocuments(t => operaUpdateDto.TagIds.Contains(t.Id) && GetClienteId(opera.SettoreId) == t.ClienteId) < operaUpdateDto.TagIds.Count())
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta PutOpera. Bad request.");

                    return BadRequest();
                }

                var update = Builders<OperaDoc>.Update
                    .Set(p => p.Nome, operaUpdateDto.Nome)
                    .Set(p => p.Descrizione, operaUpdateDto.Descrizione)
                    .Set(p => p.TagIds, operaUpdateDto.TagIds);

                UpdateResult? result;
                try
                {
                    result = await _mongoDbService.OpereCollection.UpdateOneAsync(o => o.Id == id, update);
                }
                catch (MongoWriteException e)
                {
                    if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    {
                        _logger.
                            Error($"Uscita controller OpereController, richiesta PutOpera. DuplicateKey, dettaglio eccezione: {e}");
                        return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                    }
                    throw;
                }

                if (!result.IsAcknowledged)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta PutOpera. Db update error.");

                    return StatusCode(StatusCodes.Status500InternalServerError, "db update error");
                }

                if (result.MatchedCount == 0)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta PutOpera. RecordNotFound.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }

                _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta PutOpera. No content returned.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta PutOpera. Dettaglio eccezione: {ex}");

                throw;
            }
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> PostOpera([FromBody] OperaCreateDto operaCreateDto)
        {
            try
            {
                _logger.ForContext("OperaCreateDto", JsonSerializer.Serialize(operaCreateDto)).
                    Information("Accesso controller OpereController, richiesta PostOpera.");

                if (operaCreateDto == null)
                {
                    _logger.
                        Warning("Uscita controller OpereController, richiesta PostOpera. Bad request.");

                    return BadRequest();
                }

                var clienteId = GetClienteId(operaCreateDto.SettoreId);

                if (!CurrentUserClientiIds.Contains(clienteId))
                {
                    _logger.
                        Warning("Uscita controller OpereController, richiesta PostOpera. Forbid.");

                    return Forbid();
                }

                var tags = _mongoDbService.TagsCollection.Find(t => operaCreateDto.TagIds.Contains(t.Id)).ToList();
                if (tags.Any(t => t.ClienteId != clienteId))
                {
                    _logger.
                        Warning("Uscita controller OpereController, richiesta PostOpera. Bad request.");

                    return BadRequest();
                }

                var opera = _mapper.Map<OperaDoc>(operaCreateDto);

                try
                {
                    await _mongoDbService.OpereCollection.InsertOneAsync(opera);
                }
                catch (MongoWriteException e)
                {
                    if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    {
                        _logger.
                            Warning("Uscita controller OpereController, richiesta PostOpera. Duplicate key.");

                        return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                    }
                    throw;
                }

                _logger.
                    Information("Uscita controller OpereController, richiesta PostOpera. Completata correttamente.");

                return CreatedAtAction("GetOpera", new { id = opera.Id }, _mapper.Map<OperaDto>(opera));
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta GetOpere. Dettaglio eccezione: {ex}");

                throw;
            }
        }

        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> DeleteOpera(Guid id)
        {
            //var deleteProgettiResult = await _mongoDbService.ProgettiCollection.DeleteManyAsync(p => p.OperaId == id && p.CodiceCliente == GetClaimCodiceCliente());

            //if (!deleteProgettiResult.IsAcknowledged)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            //var deleteOperaResult = await _mongoDbService.OpereCollection.DeleteOneAsync(o => o.Id == id && o.CodiceCliente == GetClaimCodiceCliente());

            //if (!deleteProgettiResult.IsAcknowledged)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            //if (deleteOperaResult.DeletedCount == 0)
            //{
            //    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            //}

            try
            {
                _logger.ForContext("Guid", id).
                    Information("Accesso controller OpereController, richiesta DeleteOpera.");

                long deleteOperaCount = await _mongoDbService.DeleteOpere(new Guid[] { id });
                if (deleteOperaCount == 0)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta DeleteOpera. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }
                else if (deleteOperaCount < 0)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller OpereController, richiesta DeleteOpera. Internal Server Error.");

                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta DeleteOpera. Dettaglio eccezione: {ex}");

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.ForContext("Guid", id).
                Information("Uscita controller OpereController, richiesta DeleteOpera. Completato correttamente.");

            return Ok();
        }

        [HttpPut("assign-tag/{operaId}/{tagId}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> AssignTag(Guid operaId, Guid tagId)
        {
            try
            {
                _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).
                    Information("Accesso controller OpereController, richiesta AssignTag.");

                var clientiIds = CurrentUserClientiIds;

                //var tagCount = (from t in _mongoDbService.TagsCollection.AsQueryable()
                //             join c in clientiIds on t.ClienteId equals c
                //             select t).Distinct().Count();

                if (_mongoDbService.TagsCollection.CountDocuments(t => clientiIds.Contains(t.ClienteId)) == 0)
                {
                    _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).
                        Warning("Uscita controller OpereController, richiesta AssignTag. Record Not Found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Tag").Build());
                };
                var update = Builders<OperaDoc>.Update.Push(o => o.TagIds, tagId);

                var updateResult = _mongoDbService.OpereCollection.UpdateOne(o => o.Id == operaId, update);
                //var updateResult = _mongoDbService.OpereCollection.UpdateOne(o => o.Id == operaId, update);
                if (updateResult.MatchedCount == 0)
                {
                    _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).
                        Warning("Uscita controller OpereController, richiesta AssignTag. Record not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta AssignTag. Dettaglio eccezione: {ex}");

                throw;
            }
            
        }

        [HttpPut("unassign-tag/{operaId}/{tagId}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> UnassignTag(Guid operaId, Guid tagId)
        {
            try
            {
                _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).Information("Accesso controller OpereController, richiesta UnassignTag.");

                var clientiIds = CurrentUserClientiIds;

                //var tagCount = (from t in _mongoDbService.TagsCollection.AsQueryable()
                //                 join c in clientiIds on t.ClienteId equals c
                //                 select t).Distinct().Count();

                if (_mongoDbService.TagsCollection.CountDocuments(t => clientiIds.Contains(t.ClienteId)) == 0)
                {
                    _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).
                        Warning("Uscita controller OpereController, richiesta UnassignTag. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Tag").Build());
                };
                var update = Builders<OperaDoc>.Update.PullFilter(o => o.TagIds, i => i == tagId);

                var updateResult = _mongoDbService.OpereCollection.UpdateOne(o => o.Id == operaId, update);
                //var updateResult = _mongoDbService.OpereCollection.UpdateOne(o => o.Id == operaId, update);
                if (updateResult.MatchedCount == 0)
                {
                    _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).
                        Warning("Uscita controller OpereController, richiesta UnassignTag. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Add("EntityType", "Opera").Build());
                }
                _logger.ForContext("Guid", operaId).ForContext("Tag", tagId).
                    Information("Uscita controller OpereController, richiesta UnassignTag. Completata correttamente.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Accesso controller OpereController, richiesta UnassignTag. Dettaglio eccezione: {ex}");

                throw;
            }
        }


        private Guid GetClienteId(Guid? settoreId)
        {
            try
            {
                _logger.ForContext("Guid", settoreId).
                    Information("Accesso controller OpereController, richiesta GetClienteId.");

                SettoreDoc? settore = _mongoDbService.SettoriCollection.Find(s => s.Id == settoreId).FirstOrDefault();
                _logger.ForContext("Guid", settoreId).ForContext("Settore", JsonSerializer.Serialize(settore)).
                    Information("Uscita controller OpereController, richiesta GetClienteId.");

                return settore?.ClienteId ?? Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta GetClienteId. Dettaglio eccezione: {ex}");

                throw;
            }
        }

        
        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-settore")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperaDto>>> GetOpereBySettore(Guid settoreId)
        {
            try
            {
                _logger.ForContext("Guid", settoreId).
                    Information("Accesso controller OpereController, richiesta GetOpereBySettore.");

                var opere = _mongoDbService.OpereCollection.Find(o => o.SettoreId == settoreId);

                _logger.ForContext("Guid", settoreId).ForContext("Opere", JsonSerializer.Serialize(opere)).
                    Information("Uscita controller OpereController, richiesta GetOpereBySettore.");

                return Ok(_mapper.Map<List<OperaDto>>(opere.ToList()));
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Accesso controller OpereController, richiesta GetOpereBySettore. Dettaglio eccezione: {ex}");

                throw;
            }
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OperaDto>>> GetOpereByCliente(Guid clienteId)
        {
            try
            {
                _logger.ForContext("Guid", clienteId).
                    Information("Accesso controller OpereController, richiesta GetOpereByCliente.");

                var opere = (from o in _mongoDbService.OpereCollection.AsQueryable(null)
                             join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                             where s.ClienteId == clienteId
                             select o).Distinct();

                _logger.ForContext("Guid", clienteId).ForContext("Opere", JsonSerializer.Serialize(opere)).
                    Information("Uscita controller OpereController, richiesta GetOpereByCliente.");

                return Ok(_mapper.Map<List<OperaDto>>(opere.ToList()));
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller OpereController, richiesta GetOpereByCliente. Dettaglio eccezione: {ex}");

                throw;
            }
        }
    }
}
