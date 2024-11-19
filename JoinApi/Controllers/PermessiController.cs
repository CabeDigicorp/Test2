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
using System.Security.Policy;
using Auth0.ManagementApi.Models;
using System.Data;
using MongoDbGenericRepository;
using NiL.JS.Core;
using System.Collections.Immutable;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class PermessiController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public PermessiController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }


        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("get-by-cliente")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PermessoDto>>> GetPermessiCliente(Guid clienteId)
        {
            _logger.ForContext("Guid", clienteId).
                    Information("Accesso controller PermessiController, richiesta GetPermessiCliente.");

            var pCliente = _mongoDbService.PermessiCollection.Find(p => p.OggettoId == clienteId).ToList();
            //pCliente.ForEach(x => { x.OggettoTipo = TipiOggettoPermessi.Cliente; });

            var pSettori = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                            join se in _mongoDbService.SettoriCollection.AsQueryable(null) on pe.OggettoId equals se.Id
                            where se.ClienteId == clienteId
                            select pe).ToList();
            //pSettori.ForEach(x => { x.OggettoTipo = TipiOggettoPermessi.Settore; });

            var pOpere = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                          join op in _mongoDbService.OpereCollection.AsQueryable(null) on pe.OggettoId equals op.Id
                          join se in _mongoDbService.SettoriCollection.AsQueryable(null) on op.SettoreId equals se.Id
                          where se.ClienteId == clienteId
                          select pe).ToList();
            //pOpere.ForEach(x => { x.OggettoTipo = TipiOggettoPermessi.Opera; });

            var pProgetti = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                             join pr in _mongoDbService.ProgettiCollection.AsQueryable(null) on pe.OggettoId equals pr.Id
                             join op in _mongoDbService.OpereCollection.AsQueryable(null) on pr.OperaId equals op.Id
                             join se in _mongoDbService.SettoriCollection.AsQueryable(null) on op.SettoreId equals se.Id
                             where se.ClienteId == clienteId
                             select pe).ToList();
            //pProgetti.ForEach(x => { x.OggettoTipo = TipiOggettoPermessi.Progetto; });

            //TODO Verificare con Matteo...
            var pComputi = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                            join co in _mongoDbService.ComputoItemsCollection.AsQueryable(null) on pe.OggettoId equals co.Id
                            join pr in _mongoDbService.ProgettiCollection.AsQueryable(null) on co.ProgettoId equals pr.Id
                            join op in _mongoDbService.OpereCollection.AsQueryable(null) on pr.OperaId equals op.Id
                            join se in _mongoDbService.SettoriCollection.AsQueryable(null) on op.SettoreId equals se.Id
                            where se.ClienteId == clienteId
                            select pe).ToList();
            //pComputi.ForEach(x => { x.OggettoTipo = TipiOggettoPermessi.Computo; });

            var permessi = pCliente.Union(pSettori).Union(pOpere).Union(pProgetti).Distinct().ToList();


            _logger.ForContext("Guid", clienteId).
                    Information("Accesso controller PermessiController, richiesta GetPermessiCliente.");

            return Ok(_mapper.Map<List<PermessoDto>>(permessi));
        }


        [HttpPost]
        [EnableCors("corsPolicy")]
        public ActionResult PostPermesso([FromBody] PermessoDto dto)
        {
            _logger.
                Information("Accesso controller PermessiController, richiesta PostPermesso.");

            var permesso = _mapper.Map<PermessoDoc>(dto);
            try
            {
                _mongoDbService.PermessiCollection.InsertOne(permesso);

                var ruoliVisibile = _mongoDbService.RuoliCollection.Find(x => true).ToList();
                var ruoloVisibile = ruoliVisibile.Find(x => !x.Inheritable && x.Azioni.Contains(Azioni.Visibile));

                if (ruoloVisibile != null)
                {
                    TipiOggettoPermessi oggettoTipo = Enum.Parse<TipiOggettoPermessi>(permesso.OggettoTipo);
                    Guid? oggettoId = permesso.OggettoId;
                    Guid soggettoId = permesso.SoggettoId;

                    do
                    {

                        //cerco padre
                        switch (oggettoTipo)
                        {
                            case TipiOggettoPermessi.Computo:
                                oggettoId = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                                             from co in _mongoDbService.ComputoItemsCollection.AsQueryable(null)
                                             where pe.OggettoId == co.Id
                                             select co.ProgettoId).FirstOrDefault();
                                oggettoTipo = TipiOggettoPermessi.Progetto;
                                break;
                            case TipiOggettoPermessi.Progetto:
                                oggettoId = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                                             from pr in _mongoDbService.ProgettiCollection.AsQueryable(null)
                                             where pe.OggettoId == pr.Id
                                             select pr.OperaId).FirstOrDefault();
                                oggettoTipo = TipiOggettoPermessi.Opera;
                                break;
                            case TipiOggettoPermessi.Opera:
                                oggettoId = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                                             from op in _mongoDbService.OpereCollection.AsQueryable(null)
                                             where pe.OggettoId == op.Id
                                             select op.SettoreId).FirstOrDefault();
                                oggettoTipo = TipiOggettoPermessi.Settore;
                                break;
                            case TipiOggettoPermessi.Settore:
                                oggettoId = (from pe in _mongoDbService.PermessiCollection.AsQueryable(null)
                                             from se in _mongoDbService.SettoriCollection.AsQueryable(null)
                                             where pe.OggettoId == se.Id
                                             select se.ClienteId).FirstOrDefault();
                                oggettoTipo = TipiOggettoPermessi.Cliente;
                                break;
                            default:
                                oggettoId = null;
                                break;
                        }

                        if (oggettoId.HasValue)
                        {
                            PermessoDoc? permessoVisibile = _mongoDbService.PermessiCollection.Find(p => p.OggettoId == oggettoId && p.SoggettoId == p.SoggettoId).FirstOrDefault();
                            if (permessoVisibile == null)
                            {
                                permessoVisibile = new PermessoDoc()
                                {
                                    Id = new Guid(),
                                    SoggettoId = soggettoId,
                                    OggettoTipo = Enum.GetName<TipiOggettoPermessi>(oggettoTipo)!,
                                    OggettoId = oggettoId.Value,
                                    RuoliIds = [ruoloVisibile.Id]
                                };

                                _mongoDbService.PermessiCollection.InsertOne(permessoVisibile);
                            }
                            else if (permessoVisibile.RuoliIds == null || permessoVisibile.RuoliIds.Count == 0)
                            {
                                permessoVisibile.RuoliIds = [ruoloVisibile.Id];
                                PutPermesso(permessoVisibile.Id, _mapper.Map<PermessoDto>(permessoVisibile));
                            }
                            //else if (!permessoVisibile.RuoliIds.Contains(ruoloVisibile.Id))
                            //{
                            //    permessoVisibile.RuoliIds.Add(ruoloVisibile.Id);
                            //    PutPermesso(permessoVisibile.Id, _mapper.Map<PermessoDto>(permessoVisibile));
                            //}
                        }
                        
                    } while (oggettoId.HasValue);
                }
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.
                    Error($"Uscita controller PermessiController, richiesta PostPermesso. Dettaglio eccezione {e}.");
                throw;
            }

            _logger.
               Information("Accesso controller PermessiController, richiesta PostPermesso.");

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<PermessoDto>(permesso));
        }


        [HttpPut("{id}")]
        [EnableCors("corsPolicy")]
        public ActionResult PutPermesso(Guid id, PermessoDto dto)
        {
            _logger.ForContext("Guid", id).
                    Information("Accesso controller PermessiController, richiesta PutPermesso.");

            if (dto.Id != id)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller PermessiController, richiesta PermessiController. Id not equal.");
                return BadRequest("id not equal");
            }
            var update = Builders<PermessoDoc>.Update.Set(p => p.SoggettoId, dto.SoggettoId)
                                                     .Set(p => p.OggettoId, dto.OggettoId)
                                                     .Set(p => p.RuoliIds, dto.RuoliIds);

            UpdateResult? result;

            try
            {
                result = _mongoDbService.PermessiCollection.UpdateOne(p => p.Id == id, update);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }
                _logger.ForContext("Guid", id).
                    Error($"Uscita controller PermessiController, richiesta PermessiController. Dettaglio eccezione {e}.");
                throw;
            }

            if (!result.IsAcknowledged)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller PermessiController, richiesta PermessiController. Sector update error.");
                return StatusCode(StatusCodes.Status500InternalServerError, "sector update error");
            }

            if (result.MatchedCount == 0)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller PermessiController, richiesta PermessiController. Permission not found.");
                return NotFound("sector not found");
            }

            _logger.ForContext("Guid", id).
                    Information("Accesso controller PermessiController, richiesta PutPermesso.");

            return Ok();
        }


        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult> DeletePermesso(Guid id)
        {
            _logger.ForContext("Guid", id).
                    Information("Accesso controller PermessiController, richiesta DeletePermesso.");

            PermessoDoc? permesso = await _mongoDbService.PermessiCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (permesso == null)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller PermessiController, richiesta DeletePermesso. Permission not found.");
                return StatusCode(StatusCodes.Status500InternalServerError, "permission not found");
            }

            var ruoliVisibile = _mongoDbService.RuoliCollection.Find(x => true).ToList();
            var ruoloVisibile = ruoliVisibile.Find(x => !x.Inheritable && x.Azioni.Contains(Azioni.Visibile));

            DeleteResult permessiDeleteResult;

            if (ruoloVisibile == null)
            {
                //TODO GESTIONE ORFANI
                permessiDeleteResult = await _mongoDbService.PermessiCollection.DeleteOneAsync(p => p.Id == id);

                if (!permessiDeleteResult.IsAcknowledged)
                {
                    _logger.ForContext("Guid", id).
                        Warning("Uscita controller PermessiController, richiesta DeletePermesso. Permission delete error.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "permission delete error");
                }
            }
            else
            {
                permesso.RuoliIds = [ruoloVisibile.Id];
                PutPermesso(permesso.Id, _mapper.Map<PermessoDto>(permesso));
            }
            

            //PermessoDoc? visibile = ruoloVisibile == null
            //                        ? null
            //                        : _mongoDbService.PermessiCollection.Find(p => p.OggettoId == permesso.OggettoId && p.SoggettoId == p.SoggettoId && p.RuoliIds.Contains(ruoloVisibile.Id)).FirstOrDefault();

            if (ruoloVisibile != null)
            {
                bool original = true;
                var soggettoId = permesso.SoggettoId;
                Guid? parentId = permesso.OggettoId;

                do
                {
                    //IOggettoPermessiBase? oggetto = null;
                    List<IOggettoPermessiBase>? children = null;
                    int childrenCount = 0;

                    switch (Enum.Parse<TipiOggettoPermessi>(permesso.OggettoTipo))
                    {
                        case TipiOggettoPermessi.Computo:
                            parentId = (from x in _mongoDbService.ComputoItemsCollection.AsQueryable(null)
                                        where x.Id == permesso.OggettoId
                                        select x).FirstOrDefault()?.ParentId;
                            break;

                        case TipiOggettoPermessi.Progetto:
                            children = _mongoDbService.ComputoItemsCollection.AsQueryable().ToList<IOggettoPermessiBase>();
                            parentId = (from x in _mongoDbService.ProgettiCollection.AsQueryable(null)
                                        where x.Id == permesso.OggettoId
                                        select x).FirstOrDefault()?.ParentId;
                            break;

                        case TipiOggettoPermessi.Opera:
                            children = _mongoDbService.ProgettiCollection.AsQueryable().ToList<IOggettoPermessiBase>();
                            parentId = (from x in _mongoDbService.OpereCollection.AsQueryable(null)
                                        where x.Id == permesso.OggettoId
                                        select x).FirstOrDefault()?.ParentId;
                            break;

                        case TipiOggettoPermessi.Settore:
                            children = _mongoDbService.OpereCollection.AsQueryable().ToList<IOggettoPermessiBase>();
                            parentId = (from x in _mongoDbService.SettoriCollection.AsQueryable(null)
                                        where x.Id == permesso.OggettoId
                                        select x).FirstOrDefault()?.ParentId;
                            break;

                        case TipiOggettoPermessi.Cliente:
                            children = _mongoDbService.SettoriCollection.AsQueryable().ToList<IOggettoPermessiBase>();
                            parentId = null;
                            break;

                        default:
                            parentId = null;
                            break;
                    }

                    if (children != null)
                    {
                        var permessiAux = _mongoDbService.PermessiCollection.AsQueryable().ToList();
                        var childrenAux = (from pe in permessiAux
                                           from ch in children
                                           where pe.OggettoId == ch.Id
                                           select new { pe, ch }).Distinct().ToList();

                        childrenCount = childrenAux.Where(x=>x.ch.ParentId.Equals(permesso.OggettoId) && x.pe.SoggettoId.Equals(soggettoId)).Count();

                    }

                    if (childrenCount == 0)
                    {
                        var removed = permesso.RuoliIds.Remove(ruoloVisibile.Id);
                        if (permesso.RuoliIds.Count == 0)
                        {
                            permessiDeleteResult = await _mongoDbService.PermessiCollection.DeleteOneAsync(p => p.Id == permesso.Id);
                            if (original && !permessiDeleteResult.IsAcknowledged)
                            {
                                _logger.ForContext("Guid", id).
                                    Warning("Uscita controller PermessiController, richiesta DeletePermesso. Permission delete error.");
                                return StatusCode(StatusCodes.Status500InternalServerError, "permission delete error");
                            }
                        }
                        else if (removed)
                        {
                            PutPermesso(permesso.Id, _mapper.Map<PermessoDto>(permesso));
                        }
                    }
                    
                    original = false;
                    permesso = null;

                    if (parentId != null)
                    {
                        permesso = (from pe in _mongoDbService.PermessiCollection.AsQueryable()
                                    where pe.SoggettoId == soggettoId && pe.OggettoId == parentId
                                    select pe).FirstOrDefault();
                    }

                } while (permesso != null);
               
            }

            _logger.ForContext("Guid", id).
                    Information("Accesso controller PermessiController, richiesta DeletePermesso.");

            return Ok();

        }

        

    }

}
