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
using static ModelData.Model.BuiltInCodes;
using System.Collections.Generic;
using ModelData.Utilities;
//using Amazon.Auth.AccessControlPolicy;
using System.Text.Json;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class ComputoController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public ComputoController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Richiesta computo complessivo, senza filtri ne lista di id (richiesto inizialmente, allo stato 0)
        /// </summary>
        /// <param name="ProgettoId"></param>
        /// <returns></returns>
        [Route("get-computo-progetto-summary/{ProgettoId}")]
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<ComputoDto>> GetComputoSummary(Guid ProgettoId)
        {
            try
            {
                _logger.ForContext("Guid", ProgettoId).
                    Information("Accesso controller ComputoController, richiesta GetComputoSummary.");
                EntityHelper? _entHelper = await EntityHelper.Create(_mongoDbService, ProgettoId);

                EntitiesSummary? entitiesSummary = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Computo, null!);
                var entityType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.Computo);

                if (
                    ((entitiesSummary != null && !entitiesSummary.Attributi.Any()) || (entitiesSummary == null))
                    || ((entityType != null && !entityType.Attributi.Any()) || (entityType == null))
                    )
                {
                    _logger.ForContext("Guid", ProgettoId).
                        Warning("Uscita controller ComputoController, richiesta GetComputoSummary. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }
                else
                {
                    // Raccolta proprietà IsAttributoDeep (attesa sincrona)
                    Dictionary<string, bool> risultatiCache = new Dictionary<string, bool>();
                    var risultatiIsAttributoDeep = await Task.WhenAll(entitiesSummary.Attributi
                        .Select(async attributo =>
                        {
                            string sourceCodice = attributo.Value.SourceCodice;
                            string sourceEntityTypeKey = attributo.Value.SourceEntityTypeKey;
                            string codice = attributo.Value.Codice;
                            string key = $"{sourceEntityTypeKey}:{sourceCodice}";
                            bool cachedResult = risultatiCache.TryGetValue(key, out bool resultFromCache) ? resultFromCache : (risultatiCache[key] = await _entHelper.IsAttributoDeep(sourceEntityTypeKey, sourceCodice));
                            return new { Codice = codice, CodiceAttributo = sourceCodice, IsAttributoDeepResult = cachedResult };
                        })
                    );

                    // Filtraggio risultati basati su IsAttributoDeep
                    Dictionary<string, bool>? risultatiIsAttributoDeepDictionary = risultatiIsAttributoDeep
                        .ToDictionary(resultToDictionary => resultToDictionary.Codice, result => result.IsAttributoDeepResult);

                    // Estrazione valori e creazione collezione dei ComputoDto (attesa sincrona)
                    var joinedData = await Task.WhenAll(
                        entitiesSummary.Attributi.Select(async attrInfo =>
                        {
                            var entityId = entitiesSummary.EntitiesId.FirstOrDefault();
                            var attrType = entityType.Attributi.FirstOrDefault(a => a.Value.Codice == attrInfo.Value.Codice);

                            if (attrType.Value == null)
                                return null;

                            string codiceAttributo = attrInfo.Value.Codice;
                            bool isAttributoDeep = risultatiIsAttributoDeepDictionary.ContainsKey(codiceAttributo)
                                ? risultatiIsAttributoDeepDictionary[codiceAttributo]
                                : false;

                            string? valore = isAttributoDeep
                                ? string.Join(@"~!@#$%^&*()_+", attrInfo.Value.Valore)
                                : attrInfo.Value.Valore.FirstOrDefault();

                            // Chiamata asincrona a GetValoriUnivoci per popolare gli attributi foglia
                            var valoriUnivociOrdered = await _entHelper.GetValoriUnivoci(
                                BuiltInCodes.EntityType.Computo,
                                (entitiesSummary.EntitiesId == null || !entitiesSummary.EntitiesId.Any()) ? null : entitiesSummary.EntitiesId,
                                attrInfo.Value.Codice?.ToString() ?? ""
                            );

                            return new ComputoDto
                            {
                                ProgettoId = ProgettoId,
                                EntityId = entityId,
                                EntitiesId = entitiesSummary.EntitiesId,
                                Etichetta = attrInfo.Value.Etichetta,
                                Descrizione = string.Join(@"~!@#$%^&*()_+", attrInfo.Value.ValoreUtente),
                                NomeGruppo = attrType.Value.GroupName,
                                Codice = attrType.Value.Codice,
                                SourceCodice = attrInfo.Value.SourceCodice,
                                ValoreAttributo = valore,
                                ValoreEtichetta = attrInfo.Value.Etichetta,
                                ValoreFormula = attrInfo.Value.ValoreFormula,
                                ValoreDescrizione = attrInfo.Value.ValoreDescrizione,
                                DefinizioneAttributoCodice = attrInfo.Value.DefinizioneAttributoCodice,
                                GuidReferenceEntityTypeKey = attrInfo.Value.GuidReferenceEntityTypeKey,
                                IsMultiValore = attrInfo.Value.IsMultiValore,
                                ValoriUnivociOrdered = valoriUnivociOrdered.ToList(), // Assegnazione del risultato asincrono
                                IsMultiValoreFormula = attrInfo.Value.IsMultiValoreFormula,
                                IsMultiValoreDescrizione = attrInfo.Value.IsMultiValoreDescrizione,
                                EntitiesIdNum = entitiesSummary.EntitiesId?.Count().ToString(),
                                IsVisible = (!string.IsNullOrEmpty(valore) || !string.IsNullOrEmpty(string.Join(@"~!@#$%^&*()_+", attrInfo.Value.ValoreUtente))),
                                IsAllowMasterGrouping = attrType.Value.AllowMasterGrouping
                            };
                        })
                    );

                    // Converti il risultato in una lista, escludendo eventuali valori nulli
                    var resultData = joinedData.Where(dto => dto != null).ToList();
                    _logger/*.ForContext("ResultData", JsonSerializer.Serialize(resultData))*/.
                        Information($"Uscita controller ComputoController, richiesta GetComputoSummary. Completata correttamente con {resultData.Count()} elementi.");

                    return Ok(resultData);
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta GetComputoSummary. Dettaglio eccezione: {ex}");
                return BadRequest(ex);
                throw;
            }
            finally
            {
            }
        }


        /// <summary>
        /// Richiesta computo parziale, ottenuto dato filtri, filtri aggregati e raggruppatori (in POST poichè si necessita di ricevere dati dal client). 
        /// </summary>
        /// <param name="filtriMultipliInput"></param>
        /// <returns></returns>
        [Route("post-valori-da-attributi")]
        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<List<ComputoDto>?>> PostValoriDaAttributi([FromBody] AttributoFiltroMultiploDto filtriMultipliInput)
        {
            try
            {
                _logger.ForContext("FiltriRaggruppatoriInput", JsonSerializer.Serialize(filtriMultipliInput)).
                    Information("Accesso controller ComputoController, richiesta PostValoriDaAttributi.");

                Guid ProgettoId = filtriMultipliInput != null ? filtriMultipliInput.ProgettoId : Guid.Empty;

                if (ProgettoId == Guid.Empty)
                {
                    _logger.ForContext("Guid", ProgettoId).
                                Warning("Uscita controller PostValoriDaAttributi, richiesta PostValoriDaAttributi. Parametri non validi.");
                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "ProjectNotFound").Build());
                }

                if (filtriMultipliInput == null)
                {
                    _logger.ForContext("Guid", ProgettoId).
                                Warning("Uscita controller PostValoriDaAttributi, richiesta PostValoriDaAttributi. Struttura dati vuota.");
                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "ProjectNotFound").Build());
                }

                EntityHelper? _entHelper = await EntityHelper.Create(_mongoDbService, ProgettoId);

                // costruzione filtri multipli
                AttributiFilter attFilter = (ProcessFiltroDto(filtriMultipliInput?.AttributiFiltri) as AttributiFilter) ?? new();
                ProcessRaggruppatoreDto((filtriMultipliInput.AttributiRaggruppatori as List<AttributoRaggruppatoreDto> ?? new()), attFilter);
                await ProcessAggregatoDto((filtriMultipliInput.AttributiAggregati as List<AttributoAggregatoDto> ?? new()), attFilter, _entHelper);

                EntitiesSummary? entitiesSummary = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Computo, null!, attFilter.Groups.Any() ? attFilter : null, null);
                var entityType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.Computo);

                if (
                    ((entitiesSummary != null && !entitiesSummary.Attributi.Any()) || (entitiesSummary == null))
                    || ((entityType != null && !entityType.Attributi.Any()) || (entityType == null))
                    )
                {
                    List<ComputoDto>? computoData = new();
                    _logger./*ForContext("ComputoData", JsonSerializer.Serialize(computoData)).*/
                        Warning("Uscita controller ComputoController, richiesta PostValoriDaAttributi. Risultato operazione completata ma vuoto.");

                    return Ok(computoData);
                }
                else
                {
                    Dictionary<string, bool> risultatiCache = new Dictionary<string, bool>();
                    var risultatiIsAttributoDeep = await Task.WhenAll(entitiesSummary.Attributi
                        .Select(async attributo =>
                        {
                            string sourceCodice = attributo.Value.SourceCodice;
                            string sourceEntityTypeKey = attributo.Value.SourceEntityTypeKey;
                            string codice = attributo.Value.Codice;
                            string key = $"{sourceEntityTypeKey}:{sourceCodice}";
                            bool cachedResult = risultatiCache.TryGetValue(key, out bool resultFromCache) ? resultFromCache : (risultatiCache[key] = await _entHelper.IsAttributoDeep(sourceEntityTypeKey, sourceCodice));
                            return new { Codice = codice, CodiceAttributo = sourceCodice, IsAttributoDeepResult = cachedResult };
                        })
                    );
                    Dictionary<string, bool>? risultatiIsAttributoDeepDictionary = risultatiIsAttributoDeep
                        .ToDictionary(resultToDictionary => resultToDictionary.Codice, result => result.IsAttributoDeepResult);

                    var joinedData = entitiesSummary.Attributi
                        .Join(entityType.Attributi,
                              attrInfo => attrInfo.Value.Codice,
                              attrType => attrType.Value.Codice,
                              (attrInfo, attrType) => new
                              {
                                  AttrInfo = attrInfo,
                                  AttrType = attrType
                              })
                        .Select((x, index) => new ComputoDto
                        {
                            ProgettoId = ProgettoId,
                            EntityId = entitiesSummary.EntitiesId.FirstOrDefault(),
                            Etichetta = x.AttrInfo.Value.Etichetta,
                            Descrizione = string.Join(@"~!@#$%^&*()_+", x.AttrInfo.Value.ValoreUtente),
                            NomeGruppo = x.AttrType.Value.GroupName,
                            Codice = x.AttrType.Value.Codice,
                            SourceCodice = x.AttrInfo.Value.SourceCodice,
                            ValoreAttributo = risultatiIsAttributoDeepDictionary.ContainsKey(x.AttrInfo.Value.Codice) ?
                                        (risultatiIsAttributoDeepDictionary[x.AttrInfo.Value.Codice] ?
                                            string.Join(@"~!@#$%^&*()_+", x.AttrInfo.Value.Valore) :
                                            x.AttrInfo.Value.Valore.FirstOrDefault()) :
                                        x.AttrInfo.Value.Valore.FirstOrDefault(),
                            ValoreEtichetta = x.AttrInfo.Value.Etichetta,
                            ValoreFormula = x.AttrInfo.Value.ValoreFormula,
                            ValoreDescrizione = x.AttrInfo.Value.ValoreDescrizione,
                            DefinizioneAttributoCodice = x.AttrInfo.Value.DefinizioneAttributoCodice,
                            GuidReferenceEntityTypeKey = x.AttrInfo.Value.GuidReferenceEntityTypeKey,
                            IsMultiValore = x.AttrInfo.Value.IsMultiValore,
                            IsMultiValoreFormula = x.AttrInfo.Value.IsMultiValoreFormula,
                            IsMultiValoreDescrizione = x.AttrInfo.Value.IsMultiValoreDescrizione,
                            EntitiesId = index == 0 ? entitiesSummary.EntitiesId.ToList() : new List<Guid>(),
                            EntitiesIdNum = entitiesSummary.EntitiesId.Count().ToString(),
                            IsVisible = (!string.IsNullOrEmpty(risultatiIsAttributoDeepDictionary.ContainsKey(x.AttrInfo.Value.Codice) ?
                                        (risultatiIsAttributoDeepDictionary[x.AttrInfo.Value.Codice] ?
                                            string.Join(@"~!@#$%^&*()_+", x.AttrInfo.Value.Valore) :
                                            x.AttrInfo.Value.Valore.FirstOrDefault()) :
                                        x.AttrInfo.Value.Valore.FirstOrDefault()) || !string.IsNullOrEmpty(string.Join(@"~!@#$%^&*()_+", x.AttrInfo.Value.ValoreUtente))),
                            IsAllowMasterGrouping = x.AttrType.Value.AllowMasterGrouping
                        });
                    List<ComputoDto>? computoData = joinedData.ToList();
                    _logger./*ForContext("ComputoData", JsonSerializer.Serialize(joinedData)).*/
                        Information($"Uscita controller ComputoController, richiesta PostValoriDaAttributi. Risultato operazione completata correttamente con {joinedData.Count()} elementi.");

                    return Ok(computoData);
                }

            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta PostValoriDaAttributi. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }


        /// <summary>
        /// Richiesta valori univoci da popolare in una struttura dati utilizzata in reference mode. 
        /// </summary>
        /// <param name="gruppiData"></param>
        /// <returns></returns>
        [Route("post-computo-valori-univoci")]
        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<List<AttributoRaggruppatoreDto>?>> PostComputoValoriUnivoci([FromBody] List<AttributoRaggruppatoreDto>? gruppiData)
        {
            try
            {
                _logger.ForContext("GruppiData", JsonSerializer.Serialize(gruppiData)).
                    Information("Accesso controller ComputoController, richiesta PostComputoValoriUnivoci.");

                if (gruppiData != null && gruppiData.Any())
                {
                    Guid ProgettoId = gruppiData.FirstOrDefault()?.ProgettoId ?? Guid.Empty;
                    if (ProgettoId == Guid.Empty)
                    {
                        _logger.ForContext("Guid", ProgettoId).
                            Warning("Uscita controller ComputoController, richiesta PostComputoValoriUnivoci. Not found.");

                        return NotFound(ErrorDtoBuilder.New.Add("Reason", "ProjectNotFound").Build());
                    }

                    EntityHelper? _entHelper = await EntityHelper.Create(_mongoDbService, ProgettoId);

                    foreach (AttributoRaggruppatoreDto item in gruppiData.Where(x => x != null))
                    {
                        var entityValori = await _entHelper.GetValoriUnivoci(BuiltInCodes.EntityType.Computo, (item.EntitiesId == null ? null : item.EntitiesId), item.Codice?.ToString() ?? "");
                        item.ValoriUnivociOrdered = entityValori.ToList<string>();
                    }

                    if (gruppiData.Any())
                    {
                        _logger.ForContext("GruppiData", JsonSerializer.Serialize(gruppiData)).
                            Information($"Uscita controller ComputoController, richiesta PostComputoValoriUnivoci. Completata correttamente con {gruppiData.Count()} elementi.");

                        return Ok(gruppiData);
                    }
                    else
                    {
                        _logger.ForContext("Guid", ProgettoId).
                            Warning("Uscita controller ComputoController, richiesta PostComputoValoriUnivoci. Not found.");

                        return BadRequest($"Non sono stati rilevati valori validi per Progetto {ProgettoId}.");
                    }
                }
                else
                {
                    _logger.
                            Error("Uscita controller ComputoController, richiesta PostComputoValoriUnivoci. Errore nell'estrazione dell'ID di progetto.");

                    return BadRequest($"Errore nell'estrazione dell'ID di progetto, uscita in corso.");
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta PostComputoValoriUnivoci. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }


        /// <summary>
        /// Chiamata REST GET per ottenere tutti gli attributi utilizzabili per filtrare un computo.
        /// </summary>
        /// <param name="ProgettoId"></param>
        /// <returns></returns>
        [Route("get-computo-attributi-per-filtro/{ProgettoId}")]
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<List<AttributoFiltroDto>?>> GetComputoAttributiPerFiltro(Guid ProgettoId)
        {
            try
            {
                _logger.ForContext("Guid", ProgettoId).
                    Information("Accesso controller ComputoController, richiesta GetComputoAttributiPerFiltro.");

                //EntityHelper? _entHelper = new EntityHelper(_mongoDbService, ProgettoId);
                EntityHelper? _entHelper = await EntityHelper.Create(_mongoDbService, ProgettoId);

                var entityType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.Computo);

                int sequenceNumber = 0;

                var lstAttributi = entityType?.Attributi.Values.ToList();

                var tasksAttributo = lstAttributi?.Select(async attributo =>
                {
                    var sourceAttributo = await _entHelper.GetSourceAttributo(attributo.EntityTypeKey, attributo.Codice);
                    return new
                    {
                        Attributo = attributo,
                        SourceAttributo = sourceAttributo
                    };
                }).ToList();

                if (tasksAttributo != null && tasksAttributo.Any())
                {
                    var attributiWithSource = await Task.WhenAll(tasksAttributo);
                    var filteredAttributi = attributiWithSource
                        .Where(x => AttributiConfig.IsAttributoFilterable(x.SourceAttributo?.DefinizioneAttributoCodice))
                        .Select(x => new AttributoFiltroDto
                        {
                            ProgettoId = ProgettoId,
                            Codice = x.Attributo.Codice,
                            CodiceGruppo = x.Attributo.GroupName,
                            DefinizionAttributoCodice = x.SourceAttributo?.DefinizioneAttributoCodice,
                            ValoreAttributo = null,
                            Descrizione = x.Attributo.Etichetta,
                            Tooltip = x.Attributo.Etichetta,
                            SequenceNumber = sequenceNumber++,
                            IsVisible = x.Attributo.IsVisible,
                            IsSelected = false,
                            EntityTypeKey = x.Attributo.EntityTypeKey,
                            GuidReferenceEntityTypeKey = x.Attributo.GuidReferenceEntityTypeKey,
                            ValoreFormat = x.Attributo.ValoreFormat,
                        }).ToList();

                    if (filteredAttributi.Any())
                    {
                        var tasksFiltro = filteredAttributi.Select(async attributoFiltro =>
                        {
                            attributoFiltro.ValoriUnivoci = (await _entHelper.GetValoriUnivoci(BuiltInCodes.EntityType.Computo, null, attributoFiltro.Codice)).ToList<string>();
                        }).ToList();

                        if (tasksFiltro != null && tasksFiltro.Any())
                        {
                            await Task.WhenAll(tasksFiltro);
                        }

                        _logger.ForContext("AttributiFiltrati", JsonSerializer.Serialize(filteredAttributi)).
                            Information($"Uscita controller ComputoController, richiesta GetComputoAttributiPerFiltro. Richesta completata correttamente con {filteredAttributi.Count()} elementi.");

                        return Ok(filteredAttributi);
                    }
                }
                _logger.
                    Warning($"Uscita controller ComputoController, richiesta GetComputoAttributiPerFiltro. Nessun filtro raccolto.");

                return BadRequest("No filter collected!");
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta GetComputoAttributiPerFiltro. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }


        /// <summary>
        /// Funzione per gestire i casi in cui le stringhe siano composte e distinte da caratteri separatori specifici
        /// </summary>
        /// <param name="valore"></param>
        /// <returns></returns>
        public string EstraiParteSuccessivaASimbolo(string valore)
        {
            if (string.IsNullOrEmpty(valore))
            {
                return valore; // Ritorna la stringa intera se è null o vuota.
            }

            int index = valore.LastIndexOf(EntityHelper.TreePathSep);
            if (index >= 0 && index < valore.Length - 1)
            {
                return valore.Substring(index + 1); // Estrae la parte successiva al simbolo.
            }

            return valore; // Ritorna la stringa intera se non c'è o se è l'ultimo carattere.
        }

        /// <summary>
        /// Chiamata REST GET per pulire lo memoria in cui sono memorizzati i dei dati remoti
        /// </summary>
        /// <param name="ProgettoId"></param>
        /// <returns></returns>
        [Route("get-computo-clear/{ProgettoId}")]
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<bool>> GetComputoClear(Guid ProgettoId)
        {
            try
            {
                _logger.ForContext("Guid", ProgettoId).
                    Information("Accesso controller ComputoController, richiesta GetComputoClear.");

                if (ProgettoId == Guid.Empty)
                {
                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "ProjectNotFound").Build());
                }

                EntityHelper.Clear(ProgettoId);

                _logger.ForContext("Guid", ProgettoId).
                    Information("Uscita controller ComputoController, richiesta GetComputoClear. Completata correttamente.");

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta GetComputoClear. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }

        /// <summary>
        /// Chiamata REST GET che, dato la lista degli ID, sia possibile ottenere la lista dei codici ifcProjectGlobalId e ifcElemGlobalId per evidenziarli nel modello 3d
        /// </summary>
        /// <param name="ProgettoId"></param>
        /// <returns></returns>
        [Route("post-ifc-ids/{ProgettoId}")]
        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<List<GlobalIdPair>>> PostIfcIds([FromRoute] Guid ProgettoId, [FromBody] List<Guid>? lstGuids)
        {
            try
            {
                // var progettoId = HttpContext.Request.RouteValues["ProgettoId"]?.ToString();

                _logger.ForContext("ListGuid", lstGuids).
                    Information("Accesso controller ComputoController, richiesta GetIfcIds.");

                if (lstGuids == null)
                {
                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "List is null").Build());
                }

                List<GlobalIdPair> lstIfcIds = new List<GlobalIdPair>();

                EntityHelper? _entHelper = await EntityHelper.Create(_mongoDbService, ProgettoId);

                List<Guid>? elemsId = await _entHelper.GetEntitiesReferred(BuiltInCodes.EntityType.Elementi, BuiltInCodes.EntityType.Computo, lstGuids);

                foreach (var elemId in elemsId)
                {

                    EntitySummary? entSum = await _entHelper.GetEntitySummary(BuiltInCodes.EntityType.Elementi, elemId, new HashSet<string>() { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId });

                    if (entSum == null)
                        continue;

                    GlobalIdPair globalIdPair = new GlobalIdPair(
                        entSum.Attributi[BuiltInCodes.Attributo.ProjectGlobalId].Valore.FirstOrDefault() ?? "",
                        entSum.Attributi[BuiltInCodes.Attributo.GlobalId].Valore.FirstOrDefault() ?? ""
                    );

                    lstIfcIds.Add(globalIdPair);
                }

                List<GlobalIdPair> distinctGlobalIdPairs = lstIfcIds.Distinct().ToList();
                return Ok(distinctGlobalIdPairs);
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta GetIfcIds. Dettaglio eccezione: {ex}");

                return BadRequest(ex);
                throw;
            }
        }


        /// <summary>
        /// Funzione per organizzare i filtri in base alla loro struttura ad albero e creare un oggetto con cui filtrare il computo
        /// </summary>
        /// <param name="filtroData"></param>
        /// <returns></returns>
        private IAttributiFilter ProcessFiltroDto(AttributoFiltroCompositoDto filtroData)
        {
            try
            {
                if (filtroData == null)
                    return new AttributiFilter();

                // Se il filtro non ha figli e ha un AttributoFiltro, crea un singolo AttributoValoreCondition
                if (filtroData.Children == null || filtroData.Children.Count == 0)
                {
                    if (filtroData.AttributoFiltro != null)
                    {
                        var singleFilter = new AttributoValoreCondition
                        {
                            Codice = filtroData.AttributoFiltro.Codice ?? string.Empty,
                            Valore = (filtroData.Valore ?? string.Empty),
                            Condition = filtroData.Condizione
                        };

                        return singleFilter;
                    }
                }

                // Crea un nuovo filtro per il gruppo
                var filterFactory = new AttributiFilter
                {
                    Operator = filtroData.OperatoreLogico
                };

                foreach (var child in filtroData.Children ?? new List<AttributoFiltroCompositoDto>())
                {
                    filterFactory.Groups.Add(ProcessFiltroDto(child));
                }
                return filterFactory;
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta ProcessComputoFiltroDto. Dettaglio eccezione: {ex}");

                throw;
            }
        }

        /// <summary>
        /// Funzione per organizzare i raggruppatori in base alla loro struttura ad albero ed integrarlo all'oggetto utile al filtraggio del computo
        /// </summary>
        /// <param name="attributiGrouperDtos"></param>
        /// <param name="attFilter">filtro già creato, a cui concatenare i raggruppatori</param>
        private void ProcessRaggruppatoreDto(List<AttributoRaggruppatoreDto> filtriRaggruppatoriInput, AttributiFilter attFilter)
        {
            try
            {
                foreach (var item in filtriRaggruppatoriInput ?? new List<AttributoRaggruppatoreDto>()
    .Where(x => x != null && x.Codice != null))
                {
                    if (Enum.TryParse(item.FiltroCondizione?.ToString(), out ValoreConditionEnum sCondition) &&
                        item.Codice != null && item.ValoreAttributo != null)
                    {
                        attFilter.Groups.Add(new AttributoValoreCondition()
                        {
                            Codice = item.Codice == null ? string.Empty : item.Codice,
                            Valore = /*EstraiParteSuccessivaASimbolo(item.ValoreAttributo),*/ (item.ValoreAttributo ?? string.Empty),
                            Condition = sCondition,
                        });
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta ProcessRaggruppatoreDto. Dettaglio eccezione: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Funzione per organizzare i raggruppatori in base alla loro struttura ad albero ed integrarlo all'oggetto utile a filtrare gli elementi passati dal modello 3d
        /// </summary>
        /// <param name="filtriAggregatoInput"></param>
        /// <param name="attFilter"></param>
        private async Task ProcessAggregatoDto(List<AttributoAggregatoDto> filtriAggregatoInput, AttributiFilter attFilter, EntityHelper _entHelper)
        {
            try
            {

                if (filtriAggregatoInput != null && filtriAggregatoInput.Any())
                {
                    AttributiFilter attInnerFilterIfc = new AttributiFilter();
                    attInnerFilterIfc.Operator = ValoreConditionsGroupOperator.Or;

                    foreach (var ifcIds in filtriAggregatoInput ?? new())
                    {
                        AttributiFilter localFilter = new AttributiFilter();
                        localFilter.Operator = ValoreConditionsGroupOperator.And;

                        localFilter.Groups.Add(new AttributoValoreCondition()
                        {
                            Codice = BuiltInCodes.Attributo.ProjectGlobalId,
                            Valore = ifcIds.IfcProjectGlobalId,
                            Condition = ValoreConditionEnum.Equal,
                        });

                        localFilter.Groups.Add(new AttributoValoreCondition()
                        {
                            Codice = BuiltInCodes.Attributo.GlobalId,
                            Valore = ifcIds.IfcElemGlobalId,
                            Condition = ValoreConditionEnum.Equal,
                        });

                        attInnerFilterIfc.Groups.Add(localFilter);
                    }

                    var elemsSum = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Elementi, null, attInnerFilterIfc, new HashSet<string>());

                    AttributiFilter attFilterComputoElemsId = new AttributiFilter();
                    attFilterComputoElemsId.Operator = ValoreConditionsGroupOperator.Or;

                    foreach (var elemId in elemsSum.EntitiesId)
                    {
                        attFilterComputoElemsId.Groups.Add(new AttributoValoreCondition()
                        {
                            Codice = BuiltInCodes.Attributo.ElementiItem_Guid,
                            Valore = elemId.ToString(),
                            Condition = ValoreConditionEnum.Equal,
                        });
                    }

                    // concatenazione con quelli eventualmente pre-esistenti
                    attFilter.Operator = ValoreConditionsGroupOperator.And;
                    attFilter.Groups.Add(attFilterComputoElemsId);
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller ComputoController, richiesta ProcessAggregatoDto. Dettaglio eccezione: {ex}");
                throw;
            }
        }
    }
}