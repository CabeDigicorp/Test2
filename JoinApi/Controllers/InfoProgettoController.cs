using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ModelData.Dto;
using JoinApi.Service;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using ModelData.Model;
using JoinApi.Utilities;
using MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class InfoProgettoController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public InfoProgettoController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [Route("get-info-progetto-totale/{ProgettoId}")]
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<InfoProgettoDto>> GetInfoProgetto(Guid ProgettoId)
        {
            try
            {
                _logger.ForContext("Guid", ProgettoId).
                    Information("Accesso controller InfoProgettoController, richiesta GetInfoProgetto.");

                //ProgettoDoc? progetto = await _mongoDbService.ProgettiCollection.Find(p => p.Id == ProgettoId && OpereUtenteIds.Contains(p.OperaId)).FirstOrDefaultAsync();
                //var _entHelper = new EntityHelper(_mongoDbService, ProgettoId);
                EntityHelper? _entHelper = await EntityHelper.Create(_mongoDbService, ProgettoId);

                EntitySummary? entsInfo = await _entHelper.GetEntitySummary(BuiltInCodes.EntityType.InfoProgetto);
                var entityType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.InfoProgetto);

                if (
                    ((entsInfo != null && !entsInfo.Attributi.Any()) || (entsInfo == null))
                    || ((entityType != null && !entityType.Attributi.Any()) || (entityType == null))
                    )
                {
                    _logger.ForContext("Guid", ProgettoId).
                        Warning("Uscita controller InfoProgettoController, richiesta GetInfoProgetto. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }
                else
                {
                    var joinedData = from attrInfo in entsInfo.Attributi
                                     join attrType in entityType.Attributi
                                     on attrInfo.Value.Codice equals attrType.Value.Codice
                                     select new InfoProgettoDto
                                     {
                                         ProgettoId = entsInfo.EntityId,
                                         Etichetta = attrInfo.Value.Etichetta,
                                         Descrizione = string.Join(@"~!@#$%^&*()_+", attrInfo.Value.ValoreUtente),// ValoreFormula, separatore tab per evitare l'uso di una lista
                                         GroupName = attrType.Value.GroupName,
                                         Codice = attrType.Value.Codice,
                                         SourceCodice = attrInfo.Value.SourceCodice,
                                         Valore = attrInfo.Value.Valore.FirstOrDefault(),
                                         ValoreFormula = attrInfo.Value.ValoreFormula,
                                         ValoreDescrizione = attrInfo.Value.ValoreDescrizione,
                                         DefinizioneAttributoCodice = attrInfo.Value.DefinizioneAttributoCodice,
                                         GuidReferenceEntityTypeKey = attrInfo.Value.GuidReferenceEntityTypeKey,
                                         IsMultiValore = attrInfo.Value.IsMultiValore,
                                         IsMultiValoreFormula = attrInfo.Value.IsMultiValoreFormula,
                                         IsMultiValoreDescrizione = attrInfo.Value.IsMultiValoreDescrizione,
                                         IsVisible = (!string.IsNullOrEmpty(attrInfo.Value.Valore.FirstOrDefault()) || !string.IsNullOrEmpty(string.Join(@"~!@#$%^&*()_+", attrInfo.Value.ValoreUtente))),
                                     };

                    var result = joinedData.ToList();
                    _logger/*.ForContext("InfoData", JsonSerializer.Serialize(result))*/.
                        Information($"Uscita controller InfoProgettoController, richiesta GetInfoProgetto. Completata correttamente con {result.Count()} elementi.");

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller InfoProgettoController, richiesta GetInfoProgetto. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }


        /// <summary>
        /// Chiamata REST GET per pulire lo stato dei dati remoti
        /// </summary>
        /// <param name="ProgettoId"></param>
        /// <returns></returns>
        [Route("get-info-clear/{ProgettoId}")]
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<bool>> GetInfoClear(Guid ProgettoId)
        {
            try
            {
                _logger.ForContext("Guid", ProgettoId).
                    Information("Accesso controller InfoProgettoController, richiesta GetInfoClear.");

                if (ProgettoId == Guid.Empty)
                {
                    _logger.ForContext("Guid", ProgettoId).
                       Information("Uscita controller InfoProgettoController, richiesta GetInfoClear. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "ProjectNotFound").Build());
                }

                EntityHelper.Clear(ProgettoId);

                _logger.ForContext("Guid", ProgettoId).
                      Information("Uscita controller InfoProgettoController, richiesta GetInfoClear. Completata correttamente.");
                
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller InfoProgettoController, richiesta GetInfoClear. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }
    }
}