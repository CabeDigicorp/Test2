using AutoMapper;
using JoinApi.Models;
using JoinApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelData.Utilities;
using ModelData.Dto;
using MongoDB.Driver;
using TagDoc = JoinApi.Models.TagDoc;
using ModelData.Dto.Error;
using JoinApi.Utilities;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;
//using Amazon.SecurityToken.Model;
using Humanizer;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class TagsController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        //private UtenteDoc _utente;
        //private UtenteDoc Utente
        //{
        //    get
        //    {
        //        if (_utente == null)
        //        {
        //            _utente = _mongoDbService.UtentiCollection.Find(u => u.UserName == User.Identity!.Name).FirstOrDefault();
        //        }
        //        return _utente;
        //    }
        //}
        //private IEnumerable<ClienteDoc> _clienti;
        //private IEnumerable<ClienteDoc> Clienti
        //{
        //    get
        //    {
        //        if (_clienti == null)
        //        {
        //            _clienti = _mongoDbService.ClientiCollection.Find(c => CurrentUserClientiIds.Contains(c.Id)).ToList();
        //        }
        //        return _clienti;
        //    }
        //}

        public TagsController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> GetTags(Guid? clienteId) 
        {
            _logger.ForContext("Guid", clienteId).
                Information("Accesso controller TagsController, richiesta GetTags.");

            if (!IsInRole(RuoliAuth0.REGISTERED))
            {
                _logger.ForContext("Guid", clienteId).
                    Warning("Uscita controller TagsController, richiesta GetTags. Not REGISTERED");
                return new ForbidResult();
            }

            FilterDefinition<TagDoc> filter;
            if(clienteId == null)
            {
                filter = Builders<TagDoc>.Filter.Where(t => CurrentUserClientiIds.Contains(t.ClienteId));
                
            }
            else
            {
                if (!(CurrentUserClientiIds.Contains(clienteId.Value)))
                {
                    _logger.ForContext("Guid", clienteId).
                        Warning("Uscita controller TagsController, richiesta GetTags. Forbid.");
                    return Forbid();
                }
                filter = Builders<TagDoc>.Filter.Where(t => t.ClienteId == clienteId.Value);
            }
            var tags = _mongoDbService.TagsCollection.Find(filter).ToList();

            _logger.ForContext("Guid", clienteId).ForContext("Tags", JsonSerializer.Serialize(tags)).
                    Information("Uscita controller TagsController, richiesta GetTags. Completata correttamente.");

            return Ok(_mapper.Map<List<TagDto>>(tags));
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public ActionResult PostTag([FromBody] TagDto dto) {

            _logger.ForContext("TagDto", JsonSerializer.Serialize(dto)).
                Information("Accesso controller TagsController, richiesta PostTag.");

            if (!CurrentUserClientiIds.Contains(dto.ClienteId!))
            {
                _logger.
                    Warning("Uscita controller TagsController, richiesta PostTag. Forbid.");
                return Forbid();
            }
            var tag = _mapper.Map<TagDoc>(dto);
            try
            {
                _mongoDbService.TagsCollection.InsertOne(tag);
            }
            catch (MongoWriteException e)
            {
                if(e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Error($"Uscita controller TagsController, richiesta PostTag. Dettaglio eccezione {e}.");

                throw;
            }

            _logger.ForContext(propertyName: "Tag", JsonSerializer.Serialize(tag)).
                Information("Uscita controller TagsController, richiesta PostTag. Completata correttamente.");

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<TagDto>(tag));
        }

        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult PutTag(Guid id, TagDto dto)
        {
            _logger.ForContext("Guid", id).ForContext("TagDto", JsonSerializer.Serialize(dto)).
                Information("Accesso controller TagsController, richiesta PutTag.");

            if (dto.Id != id)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller TagsController, richiesta PutTag. Id not equal.");

                return BadRequest("id not equal");
            }

            var update = Builders<TagDoc>.Update.Set(t => t.Nome, dto.Nome);

            UpdateResult? result;

            try
            {
                result = _mongoDbService.TagsCollection.UpdateOne(t => t.Id == id && CurrentUserClientiIds.Contains(t.ClienteId), update);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.ForContext("Guid", id).
                    Error($"Uscita controller TagsController, richiesta PutTag. Dettaglio eccezione {e}.");
                throw;
            }

            if (!result.IsAcknowledged)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller TagsController, richiesta PutTag. Tag update error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "tag update error");
            }
            
            if (result.MatchedCount == 0)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller TagsController, richiesta PutTag. Tag not found.");

                return NotFound("tag not found");
            }

            _logger.
                Information("Uscita controller TagsController, richiesta PutTag. Completata correttamente.");

            return Ok();
        }

        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> DeleteTag(Guid id)
        {
            _logger.ForContext("Guid", id).
                Information("Accesso controller TagsController, richiesta DeleteTag.");

            var tagsDeleteResult = await _mongoDbService.TagsCollection.DeleteOneAsync(t => t.Id == id && CurrentUserClientiIds.Contains(t.ClienteId));

            if (!tagsDeleteResult.IsAcknowledged)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller TagsController, richiesta DeleteTag. Tag delete error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "tag delete error");
            }

            if (tagsDeleteResult.DeletedCount == 0)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller TagsController, richiesta DeleteTag. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var opereUpdate = Builders<OperaDoc>.Update.Pull(o => o.TagIds, id);

            var opereUpdateResult = await _mongoDbService.OpereCollection.UpdateManyAsync(o => true, opereUpdate);

            if (!opereUpdateResult.IsAcknowledged)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller TagsController, richiesta DeleteTag. Opere tag delete error.");

                return StatusCode(StatusCodes.Status500InternalServerError, "opere tag delete error");
            }

            _logger.ForContext("Guid", id).
                Information("Uscita controller TagsController, richiesta DeleteTag. Completata correttamente.");

            return Ok();
        }
    }
}