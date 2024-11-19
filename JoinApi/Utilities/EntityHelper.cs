
using JoinApi.Models;
using JoinApi.Service;
using ModelData.Model;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Text.Json;
using NuGet.Packaging;
//using Amazon.Runtime.Internal.Transform;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections;
using System.Collections.Generic;




namespace JoinApi.Utilities
{

    /// <summary>
    /// classe con i dati (cache) dei progetti 
    /// </summary>
    public static class ProgettiEntities
    {
        //key: progettoId
        public static ConcurrentDictionary<Guid, ProgettoEntities> Progetti = new ConcurrentDictionary<Guid, ProgettoEntities>();
    }

    public class ProgettoEntities
    {  
        public Guid ProgettoId { get; private set; } = Guid.Empty;
        public Dictionary<string, ProgettoTypeEntities> Entities { get; set; } = new Dictionary<string, ProgettoTypeEntities>();
        public Dictionary<string, List<Guid>> GuidCollectionsCalculated = new Dictionary<string, List<Guid>>();
        public DateTime LastAccess { get; set; } = DateTime.MinValue;

        public ProgettoEntities(Guid progettoId)
        {
            ProgettoId = progettoId;
        }
    }


    /// <summary>
    /// Classe per ottenere gli attributi da visualizzare per ogni tipo di entità
    /// </summary>
    /// 
    public class EntityHelper
    {
        readonly MongoDbService _mongoDBService;
        public Guid _progettoId { get; private set; } = Guid.Empty;

        /// <summary>
        /// key: entityTypeKey
        /// </summary>
        Dictionary<string, ProgettoTypeEntities> Entities { get; set; } = new Dictionary<string, ProgettoTypeEntities>();
        Dictionary<string, List<Guid>> GuidCollectionsCalculated = new Dictionary<string, List<Guid>>();


        static readonly string _formatSpecialChars = "[0#.,]";
        static readonly string DefaultFormat = "#,##0.00";
        public static readonly string ValoreNullAsText = "[     Ø     ]";//"Ø null"
        public static readonly string ValoreAsItem = "_AsItem";
        static readonly string SumSymbol = "\u2211";//Simbolo di sommatoria
        public static readonly string TreePathSep = "¦";//separatore per percorso ad albero
        static int FreeMemoryMinutes { get => 30; }


        private static volatile SemaphoreSlim _semaphoreLoadFromDb = new SemaphoreSlim(1, 1);
        private static volatile SemaphoreSlim _semaphoreCalculateFilter = new SemaphoreSlim(1, 1);//per fare in modo che non ricalcoli il filtro di ValoreGuidCollection prima che abbia finito di leggerlo 


        private EntityHelper(MongoDbService mongoDBService, Guid progettoId)
        {
            _mongoDBService = mongoDBService;
            _progettoId = progettoId;
        }

        public static async Task<EntityHelper> Create(MongoDbService mongoDBService, Guid progettoId)
        {
            EntityHelper entsHelper = new EntityHelper(mongoDBService, progettoId);

            try
            {
                await _semaphoreLoadFromDb.WaitAsync();
                await entsHelper.LoadFromDb(progettoId);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _semaphoreLoadFromDb.Release();
            }

            return entsHelper;
        }

        public static void Clear(Guid progettoId)
        {
            try
            {
                bool x = ProgettiEntities.Progetti.TryRemove(progettoId, out _);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task LoadFromDb(Guid progettoId)
        {
            FreeMemory();

            ProgettoEntities? stat;
            if (ProgettiEntities.Progetti.TryGetValue(progettoId, out stat))
            {
                Entities = stat.Entities;
                GuidCollectionsCalculated = stat.GuidCollectionsCalculated;
            }
            else
            {
                //long pre = GC.GetTotalMemory(true);

                await CreateAllProgettoEntities();
                stat = new ProgettoEntities(progettoId);

                stat.Entities = Entities;
                stat.GuidCollectionsCalculated = GuidCollectionsCalculated;

                ProgettiEntities.Progetti.TryAdd(progettoId, stat);

                //long post = GC.GetTotalMemory(true);
            }

            stat.LastAccess = DateTime.UtcNow;
        }

        public void FreeMemory()
        {
            //libera i dati dei progetti in memoria dopo un'ora dall'ultimo accesso
            var progettiToRemove = ProgettiEntities.Progetti.Values.Where(item => (DateTime.UtcNow - item.LastAccess).TotalMinutes > FreeMemoryMinutes).Select(item => item.ProgettoId);
            foreach (var progettoId in progettiToRemove)
                Clear(progettoId);
        }

        /// <summary>
        /// Ottiene gli attributi e valori dell'unica entità in entityType di tipo NoMaster (esempio entityType InfoProgetto)
        /// </summary>
        /// <param name="entityTypeKey">Chiave di entityType</param>
        /// <param name="attributiSelect">selezione di attributi da ottenere, null = tutti</param>
        /// <returns></returns>
        public async Task<EntitySummary?> GetEntitySummary(string entityTypeKey, HashSet<string>? attributiSelect = null)
        {
            string? g = (new Random()).Next(0, 1000).ToString();
            Log.Information("EntityHelper:GetEntitySummary start {0} {1}", entityTypeKey, g);


            Guid entityTypeId = Guid.Empty;

            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            EntityType? entType = await GetProgettoEntityType(entityTypeKey);
            if (GetMasterType(entType) == MasterType.NoMaster)
            {
                entityTypeId = Entities[entityTypeKey].GetFirstEntityId();
            }

            var res = await GetEntitySummary(entityTypeKey, entityTypeId, attributiSelect);

            Log.Information("EntityHelper:GetEntitySummary end {0} {1}", entityTypeKey, g);
            return res;
        }

        /// <summary>
        /// Ottiene gli attributi e valori dell'entità con id entityId
        /// </summary>
        /// <param name="entityTypeKey">Chiave dell'entityType</param>
        /// <param name="entityId">id dell'entità</param>
        /// <param name="attributiSelect">selezione di attributi da ottenere, null = tutti</param>
        /// <returns></returns>
        public async Task<EntitySummary?> GetEntitySummary(string entityTypeKey, Guid entityId, HashSet<string>? attributiSelect = null)
        {

            var res = await GetEntitySummaryInternal(entityTypeKey, entityId, attributiSelect);
            return res;
        }

        /// <summary>
        /// Ottiene gli attributi e valori comuni ad un insieme di entità
        /// </summary>
        /// <param name="entityTypeKey">Chiave dell'entità</param>
        /// <param name="entitiesId">ids da ottenere</param>
        /// <param name="attributiSelect">selezione degli attributi da ottenere, null = tutti</param>
        /// <param name="attributiFilter">filtro sulle entità da ottenere che risulteranno sottoinsieme di entitiesId</param>
        /// <returns></returns>
        public async Task<EntitiesSummary?> GetEntitiesSummary(string entityTypeKey, IEnumerable<Guid> entitiesId, AttributiFilter? attributiFilter = null, HashSet<string>? attributiSelect = null)
        {
            string? g = (new Random()).Next(0, 1000).ToString();

            JsonSerializer.Serialize(attributiFilter);

            Log.Information("EntityHelper:GetEntitiesSummary start {0} {1} {2}", entityTypeKey, g, JsonSerializer.Serialize(attributiFilter?.GetConditions()));

            var res = await GetEntitiesSummaryInternal(entityTypeKey, entitiesId, attributiFilter, attributiSelect);


            Log.Information("EntityHelper:GetEntitiesSummary end {0} {1} EntitiesId_Count: {2}", entityTypeKey, g, res?.EntitiesId.Count);
            return res;
        }

        /// <summary>
        /// Ottiene l'oggetto entityType con chiave entityTypeKey
        /// </summary>
        /// <param name="entityTypeKey">Chiave di entityType</param>
        /// <returns></returns>
        public async Task<EntityType?> GetEntityType(string entityTypeKey)
        {
            try
            {
                EntityType? entType = null;

                if (_mongoDBService == null)
                    return entType;

                if (_progettoId == Guid.Empty)
                    return entType;


                if (entityTypeKey.StartsWith(BuiltInCodes.EntityType.DivisioneParent))
                {
                    var divIdS = entityTypeKey.Substring(BuiltInCodes.EntityType.DivisioneParent.Length + 1, entityTypeKey.Length - (BuiltInCodes.EntityType.DivisioneParent.Length + 1));
                    Guid divId = new Guid(divIdS);
                    entType = await _mongoDBService.EntityTypesCollection.Find(entType => entType.ProgettoId == _progettoId && entType is DivisioneItemParentTypeDoc && (entType as DivisioneItemParentTypeDoc).DivisioneId == divId).FirstOrDefaultAsync();
                }
                else if (entityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                {
                    var divIdS = entityTypeKey.Substring(BuiltInCodes.EntityType.Divisione.Length + 1, entityTypeKey.Length - (BuiltInCodes.EntityType.Divisione.Length + 1));
                    Guid divId = new Guid(divIdS);
                    entType = await _mongoDBService.EntityTypesCollection.Find(entType => entType.ProgettoId == _progettoId && entType is DivisioneItemTypeDoc && (entType as DivisioneItemTypeDoc).DivisioneId == divId).FirstOrDefaultAsync();
                }
                else
                {
                    entType = await _mongoDBService.EntityTypesCollection.Find(entType => entType.ProgettoId == _progettoId && entType.Codice == entityTypeKey).FirstOrDefaultAsync();
                }
                return entType;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Ottiene i valori (non doppi) di un certo attributo di alcune entità
        /// </summary>
        /// <param name="entityTypeKey">chiave di entityType</param>
        /// <param name="entitiesId">Id delle entità da cui ricavare i valori</param>
        /// <param name="codiceAttributo">codice dell'attributo di cui avere i valori</param>
        /// <param name="req">tipo di valore richiesto</param>
        /// <returns></returns>
        public async Task<List<string>> GetValoriUnivoci_deprecated(string entityTypeKey, IEnumerable<Guid> entitiesId, string codiceAttributo, ValoreRequest req = ValoreRequest.ValoreUtente)
        {
            List<string> valoriUnivociOrdered = new List<string>();

            try
            {
                HashSet<string> valoriUnivoci = new HashSet<string>();
                Dictionary<string, int> valoriUnivociIndex = new Dictionary<string, int>();

                if (!Entities.ContainsKey(entityTypeKey))
                    await CreateProgettoEntities(entityTypeKey);

                List<Entity> ents = await GetEntities(entityTypeKey, entitiesId);


                EntityType? entType = await GetProgettoEntityType(entityTypeKey);

                if (entType == null)
                    return valoriUnivociOrdered;

                Attributo? sourceAtt = null;
                Attributo? att = null;

                if (entType.Attributi.TryGetValue(codiceAttributo, out att))
                    sourceAtt = await GetSourceAttributo(att);


                //key: sourceEntityId, value: list entityId di entityTypeKey
                Dictionary<Guid, HashSet<Guid>> sourceEntsId2 = new Dictionary<Guid, HashSet<Guid>>();
                HashSet<Guid> sourceEntsId = new HashSet<Guid>();

                foreach (Entity ent in ents)
                {
                    var entAttVal = await GetValoreAttributo(ent, codiceAttributo);
                    sourceEntsId.Add((entAttVal != null) ? entAttVal.EntityId : Guid.Empty);
                }

                var sourceEnts = await GetEntities(sourceAtt.EntityTypeKey, null);
                foreach (var sourceEnt in sourceEnts)
                {
                    var entAttVal = await GetValoreAttributo(sourceEnt, sourceAtt.Codice);
                    if (entAttVal != null)
                    {

                        var valStr = await GetValoreAsString(new EntitiesAttributoValore()
                        {
                            Valore = entAttVal.Valore,
                            CodiceAttributo = entAttVal.CodiceAttributo,
                            EntitiesId = new List<Guid> { sourceEnt.EntityId },
                            EntityTypeKey = entAttVal.EntityTypeKey,
                        }, sourceAtt, req);


                        string val = valStr.First();
                        if (sourceEntsId.Contains(sourceEnt.EntityId) && !valoriUnivoci.Contains(val))
                        {
                            valoriUnivociOrdered.Add(val);
                            valoriUnivoci.Add(val);

                        }
                    }

                }

                if (sourceEntsId.Contains(Guid.Empty) && !valoriUnivoci.Contains(ValoreNullAsText))
                    valoriUnivociOrdered.Insert(0, ValoreNullAsText);

            }
            catch(Exception ex)
            {
                // TODO: in alcuni casi ritorna "Sequence contains no elements", gestirla in futuro.
            }

            return valoriUnivociOrdered;
        }

        /// <summary>
        /// Ottiene i valori (non doppi) di un certo attributo di alcune entità
        /// </summary>
        /// <param name="entityTypeKey">chiave di entityType</param>
        /// <param name="entitiesId">Id delle entità da cui ricavare i valori</param>
        /// <param name="codiceAttributo">codice dell'attributo di cui avere i valori</param>
        /// <param name="req">tipo di valore richiesto</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetValoriUnivoci(string entityTypeKey, IEnumerable<Guid> entitiesId, string codiceAttributo, ValoreRequest req = ValoreRequest.ValoreUtente)
        {
            var valEntsGroups = await GetValoriEntitiesGroup(entityTypeKey, entitiesId, codiceAttributo, req);

            var valUnivoci = valEntsGroups.Select(item => item.Valore);

            return valUnivoci;
        }

        /// <summary>
        /// Ottiene i valori (non doppi) di un certo attributo di alcune entità e le entità di tipo entityTypeKey associate al valore
        /// </summary>
        /// <param name="entityTypeKey">chiave di entityType</param>
        /// <param name="entitiesId">Id delle entità da cui ricavare i valori</param>
        /// <param name="codiceAttributo">codice dell'attributo di cui avere i valori</param>
        /// <param name="req">tipo di valore richiesto</param>
        /// <returns></returns>
        public async Task<IEnumerable<ValoreEntitiesGroup>> GetValoriEntitiesGroup(string entityTypeKey, IEnumerable<Guid> entitiesId, string codiceAttributo, ValoreRequest req = ValoreRequest.ValoreUtente)
        {
            List<ValoreEntitiesGroup> valoriUnivociOrdered = new List<ValoreEntitiesGroup>();

            try
            {
                HashSet<string> valoriUnivoci = new HashSet<string>();
                Dictionary<string, int> valoriUnivociIndex = new Dictionary<string, int>();

                if (!Entities.ContainsKey(entityTypeKey))
                    await CreateProgettoEntities(entityTypeKey);


                //if (entitiesId != null)
                //    if (entitiesId.Count() == 0)
                //        return valoriUnivociOrdered;


                List<Entity> ents = await GetEntities(entityTypeKey, entitiesId);


                EntityType? entType = await GetProgettoEntityType(entityTypeKey);

                if (entType == null)
                    return valoriUnivociOrdered;

                Attributo? sourceAtt = null;
                Attributo? att = null;

                if (entType.Attributi.TryGetValue(codiceAttributo, out att))
                {
                    string attEtichetta = att.Etichetta;

                    if (attEtichetta == "Categoria 4 Descrizione")
                    { }

                    sourceAtt = await GetSourceAttributo(att);
                }


                //key: sourceEntityId, value: list entityId di entityTypeKey
                Dictionary<Guid, HashSet<Guid>> sourceEntsId = new Dictionary<Guid, HashSet<Guid>>();


                foreach (Entity ent in ents)
                {
                    var entAttVal = await GetValoreAttributo(ent, codiceAttributo);
                    
                    Guid sourceEntId = (entAttVal != null) ? entAttVal.EntityId : Guid.Empty;
                    if (!sourceEntsId.ContainsKey(sourceEntId))
                        sourceEntsId.Add(sourceEntId, new HashSet<Guid>());

                    sourceEntsId[sourceEntId].Add(ent.EntityId);
                }

                //oss: le prendo tutte per mantenere l'ordine
                var sourceEnts = await GetEntities(sourceAtt.EntityTypeKey, null);
                foreach (var sourceEnt in sourceEnts)
                {
                    if (!sourceEntsId.ContainsKey(sourceEnt.EntityId))
                        continue;

                    var entAttVal = await GetValoreAttributo(sourceEnt, sourceAtt.Codice);
                    if (entAttVal != null)
                    {

                        var valStr = await GetValoreAsString(new EntitiesAttributoValore()
                        {
                            Valore = entAttVal.Valore,
                            CodiceAttributo = entAttVal.CodiceAttributo,
                            EntitiesId = new List<Guid> { sourceEnt.EntityId },
                            EntityTypeKey = entAttVal.EntityTypeKey,
                        }, sourceAtt, req);

                        string? val = valStr?.FirstOrDefault();

                        if (val == null)
                            continue;


                        if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                        {
                            if (valAttTesto.UseDeepValore)
                            {
                                /////////////////////////////////////////////////////////////////////////
                                //Aggiungo i padri
                                var valoreParents = await GetValoreParents(sourceAtt, sourceEnt);

                                foreach (var valParent in valoreParents)
                                {
                                    string? strParent = GetValoreAsString(valParent);
                                    val = string.Format("{0}{1}{2}", strParent, TreePathSep, val);
                                }
                                ////////////////////////////////////////////////////////////////////
                            }
                        }



                        if (valoriUnivociIndex.ContainsKey(val))
                        {
                            valoriUnivociOrdered[valoriUnivociIndex[val]].EntitiesId.AddRange(sourceEntsId[sourceEnt.EntityId]);
                        }
                        else
                        {
                            valoriUnivociOrdered.Add(new ValoreEntitiesGroup()
                            {
                                EntityTypeKey = entityTypeKey,
                                CodiceAttributo = codiceAttributo,
                                Valore = val,
                                EntitiesId = sourceEntsId[sourceEnt.EntityId] 
                            });
                            valoriUnivociIndex.Add(val, valoriUnivociOrdered.Count - 1);
                        }

                        //}
                    }

                }

                if (sourceEntsId.ContainsKey(Guid.Empty) && !valoriUnivoci.Contains(ValoreNullAsText))
                    valoriUnivociOrdered.Insert(0, new ValoreEntitiesGroup()
                    {
                        EntityTypeKey = entityTypeKey,
                        CodiceAttributo = codiceAttributo,
                        Valore = ValoreNullAsText,
                        EntitiesId = sourceEntsId[Guid.Empty]
                    });

            }
            catch (Exception ex)
            {
                // TODO: in alcuni casi ritorna "Sequence contains no elements", gestirla in futuro.
            }

            return valoriUnivociOrdered;
        }

        /// <summary>
        /// Restituisce true se l'attributo in una struttura ad albero vale sia per le foglie che per i padri
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="codiceAttributo"></param>
        /// <returns></returns>
        public async Task<bool> IsAttributoDeep(string entityTypeKey, string codiceAttributo)
        {
            EntityType? entType = await GetProgettoEntityType(entityTypeKey);

            var masterType = GetMasterType(entType);
            if (masterType == MasterType.Tree)
            {
                EntityType parentEntType = await GetProgettoEntityTypeParent(entityTypeKey);
                if (parentEntType.Attributi.ContainsKey(codiceAttributo))
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            Entities.Clear();
            //_attributiSelect = null;
            //_attributiFilter = null;
            GuidCollectionsCalculated.Clear();
        }
        
        /// <summary>
        /// Ottiene l'attributo sorgente
        /// </summary>
        /// <param name="entityTypeKey">entityType dell'attributo di cui si chiede il sorgente</param>
        /// <param name="codiceAttributo">codice dell'attributo di cui si chiede il sorgente</param>
        /// <returns></returns>
        public async Task<Attributo?> GetSourceAttributo(string entityTypeKey, string codiceAttributo)
        {
            EntityType? entType = await GetProgettoEntityType(entityTypeKey);


            if (entType != null && entType.Attributi.ContainsKey(codiceAttributo))
            {
                return await GetSourceAttributo(entType.Attributi[codiceAttributo]);
            }
            return null;
        }

        /// <summary>
        /// Trova le entità di tipo referenceEntityTypeKey che fanno riferimento alle entità referredEntitiesId di tipo referredEntityTypeKey
        /// </summary>
        /// <param name="referredEntityTypeKey">entity type delle entità riferite</param>
        /// <param name="referenceEntityTypeKey">entity type delle entità riferimento</param>
        /// <param name="referredEntitiesId">entità riferite</param>
        /// <param name="referenceAttributiSelect">Attributi di referenceEntityTypeKey per i quali cercare il riferimento</param>
        /// <returns></returns>
        public async Task<List<Guid>> GetEntitiesReference(string referredEntityTypeKey, string referenceEntityTypeKey, IEnumerable<Guid>? referredEntitiesId = null, HashSet<string>? referenceAttributiSelect = null)
        {
            return await GetEntitiesByReference(referredEntityTypeKey, referenceEntityTypeKey, referredEntitiesId, true, referenceAttributiSelect);
        }


        /// <summary>
        /// Trova le entità di tipo referredEntityTypeKey riferite dalle entità referenceEntitiesId di tipo referenceEntityTypeKey
        /// </summary>
        /// <param name="referredEntityTypeKey">>entity type delle entità riferite</param>
        /// <param name="referenceEntityTypeKey">entity type delle entità riferimento</param>
        /// <param name="referenceEntitiesId">entità riferimento</param>
        /// <param name="referenceAttributiSelect">Attributi di referenceEntityTypeKey per i quali cercare il riferimento</param>
        /// <returns></returns>
        public async Task<List<Guid>> GetEntitiesReferred(string referredEntityTypeKey, string referenceEntityTypeKey, IEnumerable<Guid>? referenceEntitiesId = null, HashSet<string>? referenceAttributiSelect = null)
        {
            return await GetEntitiesByReference(referredEntityTypeKey, referenceEntityTypeKey, referenceEntitiesId, false, referenceAttributiSelect);
        }



        /// <summary>
        /// Ricava gli id delle entità di tipo referredEntityTypeKey che sono riferite dalle entità in entitiesId
        /// </summary>
        /// <param name="referenceEntityTypeKey">entity type riferimento</param>
        /// <param name="entitiesId"></param>
        /// <param name="referredEntityTypeKey">entity type riferito</param>
        /// <param name="referenceAttributiSelect">attributi in referenceEntityTypeKey per i quali cercare il riferimento</param>
        /// <returns></returns>
        private async Task<List<Guid>> GetEntitiesByReference(string referredEntityTypeKey, string referenceEntityTypeKey, IEnumerable<Guid>? entitiesId = null, bool isEntitiesIdReferred = false, HashSet<string>? referenceAttributiSelect = null)
        {
            List<Guid> outIds = new List<Guid>();


            var entTypeIn = await GetEntityType(referenceEntityTypeKey);

            if (entTypeIn == null)
                return outIds;

            if (referenceAttributiSelect == null)
            {
                referenceAttributiSelect = entTypeIn!.Attributi.Values
                    .Where(item => (item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection) && item.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.Elementi)
                    .Select(item => item.Codice).ToHashSet();
            }

            HashSet<Guid> referredEntitiesId = null;
            IEnumerable<Guid> referenceEntitiesId = null;

            if (isEntitiesIdReferred)
            {
                if (entitiesId == null)
                    referredEntitiesId = new HashSet<Guid>(Entities[referredEntityTypeKey].GetEntitiesId());
                else
                    referredEntitiesId = new HashSet<Guid>(entitiesId);

                referenceEntitiesId = Entities[referenceEntityTypeKey].GetEntitiesId();
            }
            else
            {
                if (entitiesId == null)
                    referenceEntitiesId = Entities[referenceEntityTypeKey].GetEntitiesId();
                else
                    referenceEntitiesId = entitiesId;
            }

            

            foreach (Guid entId in referenceEntitiesId)
            {
                Entity? entOut = Entities[referenceEntityTypeKey].GetEntity(entId);
                if (entOut == null)
                    continue;

                foreach (string codiceAtt in referenceAttributiSelect)
                {
                    var entAtt = await GetValoreAttributo(entOut, codiceAtt);
                    if (entAtt == null)
                        continue;

                    if (entAtt.Valore is ValoreGuid valGuid)
                    {
                        if (isEntitiesIdReferred)
                        {
                            if (referredEntitiesId.Contains(valGuid.V))
                                outIds.Add(entId);
                        }
                        else
                        {
                            outIds.Add(valGuid.V);
                        }
                    }
                    if (entAtt.Valore is ValoreGuidCollection valGuidColl)
                    {
                        if (isEntitiesIdReferred)
                        {
                            var referredEntId = valGuidColl.V.Select(item => (item as ValoreGuidCollectionItem)!.EntityId);
                            if (referredEntId.Any(item => referredEntitiesId.Contains(item)))
                                outIds.Add(entId);
                        }
                        else
                        {
                            var referredEntId = valGuidColl.V.Select(item => (item as ValoreGuidCollectionItem)!.EntityId);
                            outIds.AddRange(referredEntId);
                        }
                        
                    }
                }

            }
            return outIds;

        }


        private static bool IsAlessandroUlianaUser()
        {
            //string? userName = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
            //if (UserName != null && UserName.Contains("alessandro.uliana"))
                return true;

            //return false;

        }

        private async Task<EntitiesSummary?> GetEntitiesSummaryInternal(string entityTypeKey, IEnumerable<Guid> entitiesId, AttributiFilter? attributiFilter, HashSet<string>? attributiSelect)
        {

            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            EntitiesSummary entitiesSum = new EntitiesSummary();

            IEnumerable<Guid> entsId = null;

            if (entitiesId == null)
                entsId = Entities[entityTypeKey].GetEntitiesId();
            else
                entsId = entitiesId;

            List<Entity> ents = await GetEntities(entityTypeKey, entsId);

            if (!ents.Any())
                return null;

            entitiesSum.EntityTypeKey = entityTypeKey;


            //Filter operation
            Dictionary<string, AttributoValoreCondition>? attsValoreCondition = null;
            if (attributiFilter != null)
            {

                if (await CheckFilterConditions(entityTypeKey, ents, attributiFilter))
                {

                    entitiesSum.EntitiesId = attributiFilter.GetResult().ToList();
                    ents = await GetEntities(entityTypeKey, entitiesSum.EntitiesId);
                }
            }
            else
            {
                entitiesSum.EntitiesId = entsId.ToList();
            }

            //calcolo gli attributi se attributiSelect == null (tutti) o attributiSelect.Count > 0
            if (attributiSelect == null || attributiSelect.Count > 0)
            {

                //Select operation
                attsValoreCondition = null;
                if (attributiSelect != null)
                {
                    attsValoreCondition = attributiSelect.ToDictionary(item => item, item => new AttributoValoreCondition() { Codice = item });
                }

                entitiesSum.Attributi = await CreateEntitiesSummaryAttributi(entityTypeKey, ents, attributiSelect/*, false, attsValoreCondition*/);//selecting

            }
            return entitiesSum;
        }

        private async Task<List<Entity>> GetEntities(string entityTypeKey, IEnumerable<Guid> entitiesId)
        {
            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            List<Entity> ents = new List<Entity>();

            IEnumerable<Guid> entsId = entitiesId;
            if (entsId == null)
                entsId = Entities[entityTypeKey].GetEntitiesId();

            if (entsId == null)
                return ents;

            foreach (var entId in entsId)
            {
                Entity? ent = await GetEntity(entityTypeKey, entId);
                if (ent != null)
                    ents.Add(ent);
            }

            return ents;
        }

        private async Task<EntitySummary?> GetEntitySummaryInternal(string entityTypeKey, Guid entityId, HashSet<string>? attributiSelect)
        {

            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            EntitySummary entitySum = new EntitySummary();

            Entity? ent = await GetEntity(entityTypeKey, entityId);

            if (ent == null)
                return null;

            entitySum.EntityTypeKey = entityTypeKey;
            entitySum.EntityId = entityId;

            //Select operation
            Dictionary<string, AttributoValoreCondition>? attsValoreCondition = null;
            if (attributiSelect != null)
            {
                attsValoreCondition = attributiSelect.ToDictionary(item => item, item => new AttributoValoreCondition() { Codice = item });
            }

            entitySum.Attributi = await CreateEntitiesSummaryAttributi(entityTypeKey, new List<Entity> { ent }, attributiSelect/*, false, attsValoreCondition*/);

            return entitySum;

        }


        private async Task<EntityType?> GetProgettoEntityType(string entityTypeKey)
        {
            EntityType? res;
            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            res = Entities[entityTypeKey].EntityType;

            return res;
        }

        private async Task<EntityType> GetProgettoEntityTypeParent(string entityTypeKey)
        {
            EntityType? res;
            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            ProgettoTypeEntities progettoEnts = null;
            if (Entities.TryGetValue(entityTypeKey, out progettoEnts))
            {
                return progettoEnts.ParentEntityType;
            }
            return null;
        }

        private ProgettoTypeEntities GetProgettoEntities(string entityTypeKey)
        {
            return Entities[entityTypeKey];
        }


 
        private async Task CreateProgettoEntities(string entityTypeKey)
        {
            //entity type

            if (Entities.ContainsKey(entityTypeKey))
                return;



            EntityType? entType = await GetEntityType(entityTypeKey);

            var progettoEnts = new ProgettoTypeEntities()
            {
                ProgettoId = _progettoId,
                EntityType = entType,
            };

            //items list
            IEnumerable<Entity>? ents = null;
            if (entType is DivisioneItemTypeDoc)
            {
                string parentEntTypeKey = entityTypeKey.Replace(BuiltInCodes.EntityType.Divisione, BuiltInCodes.EntityType.DivisioneParent);
                progettoEnts.ParentEntityType = await GetEntityType(parentEntTypeKey);

                DivisioneItemTypeDoc divTypeDoc = entType as DivisioneItemTypeDoc;
                var divisioniItems = await _mongoDBService.DivisioniItemsCollection.Find(item => item.ProgettoId == _progettoId && item.DivisioneId == divTypeDoc.DivisioneId).SortBy(item => item.Position.Result).ToListAsync();
                ents = divisioniItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                var computoItems = await _mongoDBService.ComputoItemsCollection.Find(item => item.ProgettoId == _progettoId).ToListAsync();

                //var computoItems = await _mongoDBService.ComputoItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = computoItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
            {
                progettoEnts.ParentEntityType = await GetEntityType(BuiltInCodes.EntityType.PrezzarioParent);
                var prezzarioItems = await _mongoDBService.PrezzarioItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = prezzarioItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
            {
                var elementiItems = await _mongoDBService.ElementiItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = elementiItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
            {
                progettoEnts.ParentEntityType = await GetEntityType(BuiltInCodes.EntityType.CapitoliParent);
                var capitoliItems = await _mongoDBService.CapitoliItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = capitoliItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
            {
                var contattiItems = await _mongoDBService.ContattiItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = contattiItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.InfoProgetto)
            {
                var infoProgettoItems = await _mongoDBService.InfoProgettoItemsCollection.Find(item => item.ProgettoId == _progettoId).ToListAsync();
                ents = infoProgettoItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Variabili)
            {
                var variabiliItems = await _mongoDBService.VariabiliItemsCollection.Find(item => item.ProgettoId == _progettoId).ToListAsync();
                ents = variabiliItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.WBS)
            {
                progettoEnts.ParentEntityType = await GetEntityType(BuiltInCodes.EntityType.WBSParent);
                var wbsItems = await _mongoDBService.WBSItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = wbsItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Calendari)
            {
                var calendariItems = await _mongoDBService.CalendariItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = calendariItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Allegati)
            {
                var allegatiItems = await _mongoDBService.AllegatiItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = allegatiItems;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
            {
                var elencoAttivitaItems = await _mongoDBService.ElencoAttivitaItemsCollection.Find(item => item.ProgettoId == _progettoId).SortBy(item => item.Position.Result).ToListAsync();
                ents = elencoAttivitaItems;
            }
            else
            {
                ents = new List<Entity>();
            }


            if (ents != null)
                progettoEnts.SetEntities(ents.ToList());

            Entities.TryAdd(entityTypeKey, progettoEnts);

        }

        public async Task CreateAllProgettoEntities()
        {
            Entities.Clear();
            GuidCollectionsCalculated.Clear();

            var divTypes = await _mongoDBService.EntityTypesCollection.Find(entType => entType.ProgettoId == _progettoId && entType is DivisioneItemTypeDoc).ToListAsync();
            foreach (var divType in divTypes)
            {
                string divKey = string.Format("{0}-{1}", BuiltInCodes.EntityType.Divisione, divType.Id);
                await CreateProgettoEntities(divKey);
            }


            await CreateProgettoEntities(BuiltInCodes.EntityType.Computo);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Prezzario);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Elementi);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Capitoli);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Contatti);
            await CreateProgettoEntities(BuiltInCodes.EntityType.InfoProgetto);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Variabili);
            await CreateProgettoEntities(BuiltInCodes.EntityType.WBS);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Calendari);
            await CreateProgettoEntities(BuiltInCodes.EntityType.Allegati);
            await CreateProgettoEntities(BuiltInCodes.EntityType.ElencoAttivita);



        }

        private async Task<Entity?> GetEntity(string entityTypeKey, Guid entityId)
        {
            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            Entity? ent = Entities[entityTypeKey].GetEntity(entityId);

            return ent;
        }


        private async Task<Dictionary<string, EntitiesSummaryAttributo>> CreateEntitiesSummaryAttributi(string entityTypeKey, List<Entity> ents, HashSet<string>? attributiSelect = null/*, Dictionary<string, AttributoValoreCondition>? attsFilter = null*/)
        {

            Dictionary<string, EntitiesSummaryAttributo> entAttsSum = new Dictionary<string, EntitiesSummaryAttributo>();


            try
            {
                EntityType? entType = await GetProgettoEntityType(entityTypeKey);

                if (entType == null)
                    return entAttsSum;


                List<Attributo> orderedAtts = null;
                if (attributiSelect != null)
                    orderedAtts = entType.Attributi.Values.Where(item => attributiSelect.Contains(item.Codice)).OrderBy(item => item.DetailViewOrder).ToList();
                else
                    orderedAtts = entType.Attributi.Values.Where(item => item.IsVisible).OrderBy(item => item.DetailViewOrder).ToList();


                foreach (var att in orderedAtts)
                {
                    string attCodice = att.Codice;
                    string attEtichetta = att.Etichetta;



                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
                    {
                        //continue;
                    }

                    //AttributoValoreCondition? attFilter = null;
                    //if (attsFilter != null)
                    //{
                    //    if (!attsFilter.ContainsKey(attCodice))
                    //        continue;

                    //    attFilter = attsFilter[attCodice];
                    //}

                    Attributo? sourceAtt = await GetSourceAttributo(att);

                    if (sourceAtt == null)
                        continue;


                    if (attEtichetta == "Tag") //manodopera
                    {

                    }

                    EntitiesAttributoValore multiEntAtt = new EntitiesAttributoValore();
                    multiEntAtt.CodiceAttributo = sourceAtt.Codice;
                    multiEntAtt.EntityTypeKey = sourceAtt.EntityTypeKey;

                    //oss per non far ricalcolare i comuni riferiti alla stessa entità 
                    Dictionary<Guid, Valore?> valorePerId = new Dictionary<Guid, Valore?>();//esempio key: PrezzarioItem Id, val: "1.10.34.A" (codice) 
                    List<Valore> valoreParents = new List<Valore>();//x tree entity

                    foreach (Entity ent in ents) //esempio scorro entità del computo
                    {
                        try
                        {

                            if (multiEntAtt.IsMultiValore)
                                break;

                            EntityAttributoValore? sourceEntAtt = await GetValoreAttributo(ent, attCodice);

                            if (sourceEntAtt != null)
                            {
                                string sourceAttCodice = sourceEntAtt.CodiceAttributo;
                                Valore? sourceValore = sourceEntAtt.Valore;


                                ////filtro per valore dell'attributo
                                //if (attsFilter != null && isFiltering)
                                //{
                                //    var condition = attsFilter[attCodice];

                                //    bool check = await CheckCondition(sourceAtt, ent, sourceValore, condition);

                                //    if (!check)
                                //        continue;
                                //}

                                multiEntAtt.EntitiesId.Add(ent.EntityId);

                                if (!valorePerId.ContainsKey(sourceEntAtt.EntityId))
                                {
                                    multiEntAtt.OperationWith(sourceValore, att.GroupOperation, ents.Count);
                                    valorePerId.Add(sourceEntAtt.EntityId, sourceValore);

                                    Entity? sourceEnt = await GetEntity(sourceEntAtt.EntityTypeKey, sourceEntAtt.EntityId);

                                    //Aggiunta dei valori dei padri
                                    if (valorePerId.Count == 1)
                                        valoreParents = await GetValoreParents(sourceAtt, sourceEnt);
                                    else
                                        valoreParents.Clear();
                                }

                            }
                            else
                            {
                                ////Valore non esistente (ValoreNullAsText)
                                //if (attsFilter != null && isFiltering)
                                //{
                                //    var condition = attsFilter[attCodice];
                                //    bool check = await CheckCondition(sourceAtt, ent, null, condition);

                                //    if (!check)
                                //        continue;

                                //}

                                var valoreEmpty = new ValoreTesto() { V = ValoreNullAsText, Result = ValoreNullAsText };
                                multiEntAtt.EntitiesId.Add(ent.EntityId);
                                multiEntAtt.OperationWith(valoreEmpty, ValoreOperationType.Equivalent, ents.Count);
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    //if (!multiEntAtt.IsMultiValore && multiEntAtt.IsValidValore)
                    if (multiEntAtt.IsValidValore)
                    {

                        var entAttSum = new EntitiesSummaryAttributo();

                        entAttSum.Codice = attCodice;
                        entAttSum.SourceCodice = sourceAtt.Codice;
                        entAttSum.GuidReferenceEntityTypeKey = sourceAtt.GuidReferenceEntityTypeKey;
                        entAttSum.SourceEntityTypeKey = sourceAtt.EntityTypeKey;
                        entAttSum.Etichetta = att.Etichetta;
                        entAttSum.DefinizioneAttributoCodice = sourceAtt.DefinizioneAttributoCodice;

                        if (multiEntAtt.IsMultiValore)
                        {
                            entAttSum.IsMultiValore = true;
                        }
                        else
                        {
                            entAttSum.Valore.AddRange(await GetValoreAsString(multiEntAtt, sourceAtt, ValoreRequest.Valore));
                            foreach (var val in valoreParents)
                            {
                                var entAttParent = new EntitiesAttributoValore() { CodiceAttributo = sourceAtt.Codice, EntitiesId = multiEntAtt.EntitiesId, EntityTypeKey = multiEntAtt.EntityTypeKey, Valore = val };
                                entAttSum.Valore.InsertRange(0, await GetValoreAsString(entAttParent, sourceAtt, ValoreRequest.Valore));
                            }

                            entAttSum.ValoreUtente.AddRange(await GetValoreAsString(multiEntAtt, sourceAtt, ValoreRequest.ValoreUtente));
                            foreach (var val in valoreParents)
                            {
                                var entAttParent = new EntitiesAttributoValore() { CodiceAttributo = sourceAtt.Codice, EntitiesId = multiEntAtt.EntitiesId, EntityTypeKey = multiEntAtt.EntityTypeKey, Valore = val };
                                entAttSum.ValoreUtente.InsertRange(0, await GetValoreAsString(entAttParent, sourceAtt, ValoreRequest.ValoreUtente));
                            }
                        }

                        if (!multiEntAtt.IsMultiValoreFormula)
                            entAttSum.ValoreFormula = (await GetValoreAsString(multiEntAtt, sourceAtt, ValoreRequest.Formula)).FirstOrDefault();

                        if (!multiEntAtt.IsMultiValoreDescrizione)
                            entAttSum.ValoreDescrizione = (await GetValoreAsString(multiEntAtt, sourceAtt, ValoreRequest.Descrizione)).FirstOrDefault();

                        

                        //if (attFilter != null)
                        //    attFilter.Result = multiEntAtt.EntitiesId;

                        entAttsSum.Add(attCodice, entAttSum);

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return entAttsSum;
        }

        private async Task<bool> CheckFilterConditions(string entityTypeKey, List<Entity> ents, AttributiFilter attributiFilter)
        {

            var conditions = attributiFilter?.GetConditions();
            if (conditions == null)
                return false;

            //Azzeramento risultati
            conditions.ForEach(item => item.Result.Clear());

            foreach (var ent in ents)
            {
                foreach (AttributoValoreCondition cond in conditions)
                {
                    var attCodice = cond.Codice;

                    EntityType? entType = await GetProgettoEntityType(entityTypeKey);

                    Attributo att = null;
                    entType?.Attributi.TryGetValue(attCodice, out att);

                    if (att == null)
                        continue; 

                    Attributo? sourceAtt = await GetSourceAttributo(att);

                    if (sourceAtt == null)
                        continue;

                    EntityAttributoValore? sourceEntAtt = await GetValoreAttributo(ent, attCodice);
                    if (sourceEntAtt != null)
                    {
                        string sourceAttCodice = sourceEntAtt.CodiceAttributo;
                        Valore? sourceValore = sourceEntAtt.Valore;
                        Entity? sourceEnt = await GetEntity(sourceEntAtt.EntityTypeKey, sourceEntAtt.EntityId);

                        if (sourceEnt == null)
                            continue;

                        bool check = await CheckFilterCondition(sourceAtt, sourceEnt, sourceValore, cond);

                        if (!check) //cerco nei padri se sono in una sezione ad albero
                        {

                            List<Valore> sourceParentsVal = await GetValoreParents(sourceAtt, sourceEnt);

                            foreach (var sourceParentVal in sourceParentsVal)
                            {
                                check = await CheckFilterCondition(sourceAtt, sourceEnt, sourceParentVal, cond);
                                if (check)
                                    break;
                            }
                        }


                        if (check)
                            cond.Result.Add(ent.EntityId);

                    }
                    else
                    {
                        bool check = await CheckFilterCondition(sourceAtt, null, null, cond);
                        if (check)
                            cond.Result.Add(ent.EntityId);
                    }
                }
            }

            return true;
        }

        private async Task<bool> CheckFilterCondition(Attributo sourceAtt, Entity? ent, Valore? sourceValore, AttributoValoreCondition condition)
        {
            bool check = false;


            if (sourceValore != null && ent != null)
            {
                var entAtt = new EntitiesAttributoValore()
                {
                    CodiceAttributo = sourceAtt.Codice,
                    EntitiesId = new List<Guid>() { ent.EntityId },
                    EntityTypeKey = sourceAtt.EntityTypeKey,
                    Valore = sourceValore
                };

                if (sourceValore is ValoreTestoCollection valTestoColl)
                {
                    if (valTestoColl?.V != null && valTestoColl.V.Any())
                    {
                        if (condition.Valore != null)
                        {
                            List<string> res = await GetValoreAsString(entAtt, sourceAtt, ValoreRequest.ValoreUtente);

                            //separazione e ricerca separata delle parole
                            string[] condArray = condition.Valore.Split(" ");

                            foreach (var str in res)//OR
                            {
                                foreach (var cond in condArray)//AND
                                {
                                    check = AttributiFilter.CheckFilterConditions(cond, str, condition.Condition);
                                    if (!check)
                                        break;
                                }
                                if (check)
                                    break;
                            }
                        }

                    }
                }
                else if (sourceValore is ValoreGuidCollection valGuidColl)
                {

                    if (valGuidColl?.V != null && valGuidColl.V.Any())
                    {
                        if (condition.Valore != null)
                        {
                            List<string> res = await GetValoreAsString(entAtt, sourceAtt, ValoreRequest.ValoreUtente);

                            //separazione e ricerca separata delle parole
                            string[] condArray = condition.Valore.Split(" ");

                            foreach (var str in res)//OR
                            {
                                foreach (var cond in condArray)//AND
                                {
                                    check = AttributiFilter.CheckFilterConditions(cond, str, condition.Condition);
                                    if (!check)
                                        break;
                                }
                                if (check)
                                    break;
                            }
                        }

                    }


                }
                else if (sourceValore is ValoreReale valReale)
                {

                    double condReale = 0.0;
                    if (double.TryParse(condition.Valore/*, CultureInfo.InvariantCulture*/, out condReale))
                    {
                        if (valReale.RealResult != null)
                            check = AttributiFilter.CheckFilterConditions(condReale, valReale.RealResult.Value, condition.Condition);
                    }
                }
                else if (sourceValore is ValoreContabilita valCont)
                {

                    decimal condDec = 0;
                    if (decimal.TryParse(condition.Valore/*, CultureInfo.InvariantCulture*/, out condDec))
                    {
                        if (valCont.RealResult != null)
                            check = AttributiFilter.CheckFilterConditions(condDec, valCont.RealResult.Value, condition.Condition);
                    }
                }
                else if (sourceValore is ValoreData valData)
                {
                    DateTime condData;
                    if (DateTime.TryParse(condition.Valore, out condData))
                    {
                        if (valData.V != null)
                            check = AttributiFilter.CheckFilterConditions(condData, valData.V, condition.Condition);
                    }
                }
                else if (sourceValore is ValoreBooleano valBool)
                {
                    bool condBool;
                    if (bool.TryParse(condition.Valore, out condBool))
                    {
                        if (valBool.V != null)
                            check = AttributiFilter.CheckFilterConditions(condBool, valBool.V.Value, condition.Condition);
                    }
                }
                else if (sourceValore is ValoreTestoRtf valRtf)
                {
                    if (condition.Valore != null)
                    {
                        string testo = null;

                        if (valRtf.V != null)
                            ConvertToPlainString(valRtf.V, out testo);

                        ////separazione e ricerca separata delle parole
                        //string[] condArray = condition.Valore.Split(" ");
                        //foreach (var cond in condArray)
                        //{
                        //    check = AttributiFilter.CheckFilterConditions(cond, testo, condition.Condition);
                        //    if (check)
                        //        break;
                        //}

                        check = AttributiFilter.CheckFilterConditions(condition.Valore, testo, condition.Condition);
                    }
                }
                else if (sourceValore is ValoreElenco valElenco)
                {
                    if (condition.Valore != null)
                    {
                        List<string> res = await GetValoreAsString(entAtt, sourceAtt, ValoreRequest.ValoreUtente);
                        string? first = res.FirstOrDefault();

                        ////separazione e ricerca separata delle parole
                        //string[] condArray = condition.Valore.Split(" ");
                        //foreach (var cond in condArray)
                        //{
                        //    if (first != null && condition.Valore != null)
                        //        check = AttributiFilter.CheckFilterConditions(cond, first, condition.Condition);
                        //}

                        check = AttributiFilter.CheckFilterConditions(condition.Valore, first, condition.Condition);
                    }
                }
                else if (sourceValore is ValoreGuid valGuid)
                {
                    Guid condGuid = new Guid(condition.Valore);
                    check = AttributiFilter.CheckFilterConditions(condGuid, valGuid.V, condition.Condition);
                }
                else //Testo
                {
                    if (condition.Valore != null)
                    {
                        List<string> res = await GetValoreAsString(entAtt, sourceAtt, ValoreRequest.ValoreUtente);
                        string? val = res.FirstOrDefault();

                        ////separazione e ricerca separata delle parole
                        //string[] condArray = condition.Valore.Split(" ");
                        //foreach (var cond in condArray)
                        //{
                        //    if (first != null && condition.Valore != null)
                        //        check = AttributiFilter.CheckFilterConditions(cond, first, condition.Condition);
                        //}

                        if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                        {
                            if (valAttTesto.UseDeepValore && val != null)
                            {
                                /////////////////////////////////////////////////////////////////////////
                                //Aggiungo i padri
                                var valoreParents = await GetValoreParents(sourceAtt, ent /*sourceEnt*/);

                                foreach (var valParent in valoreParents)
                                {
                                    string? strParent = GetValoreAsString(valParent);
                                    val = string.Format("{0}{1}{2}", strParent, TreePathSep, val);
                                }
                                ////////////////////////////////////////////////////////////////////
                            }
                        }

                        check = AttributiFilter.CheckFilterConditions(condition.Valore, val, condition.Condition);

                    }
                }
            }
            else
            {

                if (condition.Condition == ValoreConditionEnum.Equal)
                {
                    if (condition.Valore == ValoreNullAsText)
                        check = true;
                }
                else if (condition.Condition == ValoreConditionEnum.Unequal)
                {
                    if (condition.Valore != ValoreNullAsText)
                        check = true;
                }

            }
            return check;
        }

        public static void ConvertToPlainString(string rtf, out string str)
        {
            str = string.Empty;

            Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree = new Net.Sgoliver.NRtfTree.Core.RtfTree();
            rtfTree.LoadRtfText(rtf);
            str = rtfTree.Text.Trim();
        }

        private async Task<List<Valore>> GetValoreParents(Attributo sourceAtt, Entity? sourceEnt)
        {
            List<Valore> valoreParents = new List<Valore>();

            if (sourceEnt is TreeEntity)
            {
                string sourceAttCodice = sourceAtt.Codice;

                ProgettoTypeEntities progettoEntities = GetProgettoEntities(sourceAtt.EntityTypeKey);
                TreeEntity? treeEnt = progettoEntities.GetParentEntity(sourceEnt.EntityId);

                if (treeEnt != null)
                {
                    EntityType? sourceParentEntityType = await GetProgettoEntityType(treeEnt.ParentEntityTypeCodice);

                    if (sourceParentEntityType != null && sourceParentEntityType.Attributi.ContainsKey(sourceAttCodice))
                    {

                        while (treeEnt != null)
                        {
                            if (treeEnt.Attributi.ContainsKey(sourceAttCodice))//se il padre ha l'attributo con lo stesso codice
                            {
                                valoreParents.Add(treeEnt.Attributi[sourceAttCodice].Valore);
                            }

                            treeEnt = progettoEntities.GetParentEntity(treeEnt.EntityId);
                        }
                    }
                }
            }

            return valoreParents;
        }

        public static string? GetValoreAsString(Valore val)
        {
            string? res = null;

            var vTesto = val as ValoreTesto;
            if (vTesto != null)
            {
                if (vTesto.Result != null)
                    res = vTesto.Result;
            }

            var vCont = val as ValoreContabilita;
            if (vCont != null)
            {
                string? str = vCont.RealResult.ToString();
                if (str != null)
                    res = str;
            }

            var vReale = val as ValoreReale;
            if (vReale != null)
            {
                string? str = vReale.RealResult.ToString();
                if (str != null)
                    res = str;
            }

            var vData = val as ValoreData;
            if (vData != null)
               if (vData.V.HasValue)
                    res = (vData.V.Value.ToLocalTime().ToString());

            var vBool = val as ValoreBooleano;
            if (vBool != null)
                res = vBool.V.ToString();

            var vElenco = val as ValoreElenco;
            if (vElenco != null)
                res = vElenco.V.ToString();

            return res;
        }

        private async Task<List<string>> GetValoreAsString(EntitiesAttributoValore sourceEntAtt, Attributo sourceAtt, ValoreRequest request)
        {
            var res = new List<string>();

            if (sourceEntAtt == null)
                return res;

            if (sourceAtt == null && request != ValoreRequest.Valore)
                return res;

            Valore? val = sourceEntAtt.Valore;

            ValoreAttributo valAtt = sourceAtt.ValoreAttributo;

            var vTesto = val as ValoreTesto;
            if (vTesto != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                    case ValoreRequest.ValoreUtente:
                        if (vTesto.Result != null)
                            res.Add(vTesto.Result);
                        break;
                    case ValoreRequest.Formula:
                        if (vTesto.V != null)
                            res.Add(vTesto.V);
                        break;

                }
                return res;
            }

            var vTestoRtf = val as ValoreTestoRtf;
            if (vTestoRtf != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        res.Add(vTestoRtf.V);
                        break;
                    case ValoreRequest.ValoreUtente:
                            break;
                    case ValoreRequest.Formula:
                        break;
                }
                return res;
            }

            var vCont = val as ValoreContabilita;
            if (vCont != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        {
                            string? str = vCont.RealResult.ToString();
                            if (str != null)
                                res.Add(str);
                            break;
                        }
                    case ValoreRequest.Formula:
                        res.Add(vCont.V);
                        break;
                    case ValoreRequest.ValoreUtente:
                        {
                            string? formato = await GetValoreFormatInternal(sourceEntAtt);
                            if (formato == null)
                                formato = GetValoreFormatInternal(sourceAtt);

                            formato = GetPaddedFormat(formato);
                            string str = string.Format(formato, vCont.RealResult);

                            if (sourceAtt.GroupOperation == ValoreOperationType.Sum)
                                str = string.Format("{0}= {1}", SumSymbol, str);

                            res.Add(str.Trim());
                            break;
                        }
                    case ValoreRequest.Descrizione:
                        res.Add(vCont.ResultDescription);
                        break;
                        
                }
                return res;
            }

            var vReale = val as ValoreReale;
            if (vReale != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        {
                            string? str = vReale.RealResult.ToString();
                            if (str != null)
                                res.Add(str);
                            break;
                        }
                    case ValoreRequest.Formula:
                        res.Add(vReale.V);
                        break;
                    case ValoreRequest.ValoreUtente:
                        {
                            string? formato = await GetValoreFormatInternal(sourceEntAtt);
                            if (formato == null)
                                formato = GetValoreFormatInternal(sourceAtt);

                            formato = GetPaddedFormat(formato);
                            string str = string.Format(formato, vReale.RealResult);

                            if (sourceAtt.GroupOperation == ValoreOperationType.Sum)
                                str = string.Format("{0}= {1}", SumSymbol, str);

                            res.Add(str.Trim());
                            break;
                        }
                    case ValoreRequest.Descrizione:
                        res.Add(vReale.ResultDescription);
                        break;
                }
                return res;
            }

            var vCollTesto = val as ValoreTestoCollection;
            if (vCollTesto != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        res.AddRange(vCollTesto.V.Select(item => String.Format("{0}\n{1}", (item as ValoreTestoCollectionItem)?.Testo1, (item as ValoreTestoCollectionItem)?.Testo2)));
                        break;
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.ValoreUtente:
                        res.AddRange(vCollTesto.V.Select(item => String.Format("{0}\n{1}", (item as ValoreTestoCollectionItem)?.Testo1, (item as ValoreTestoCollectionItem)?.Testo2)));
                        break;
                }
                return res;
            }

            var vCollGuid = val as ValoreGuidCollection;
            if (vCollGuid != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        res.AddRange(vCollGuid.V.Select(item => (item as ValoreGuidCollectionItem).EntityId.ToString()));
                        break;
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.ValoreUtente:
                        {
                            if (vCollGuid.V != null)
                            {
                                try
                                {
                                    await _semaphoreCalculateFilter.WaitAsync();

                                    //List<ValoreCollectionItem> itemsCopy = new List<ValoreCollectionItem>(vCollGuid.V);
                                    foreach (ValoreGuidCollectionItem item in vCollGuid.V)
                                    {
                                        if (item != null)
                                        {
                                            string userIdentity = await ToUserIdentity(sourceAtt.GuidReferenceEntityTypeKey, item.EntityId);
                                            res.Add(userIdentity);
                                        }
                                    }
                                }
                                catch (Exception exc)
                                {
                                    string? g = (new Random()).Next(0, 1000).ToString();
                                    Log.Error("GetValoreAsString {0} {1} {2}", sourceAtt.Codice, g, JsonSerializer.Serialize(vCollGuid));
                                }
                                finally
                                {
                                    _semaphoreCalculateFilter.Release();
                                }


                            }
                        }
                        break;
                }
                return res;
            }

            var vData = val as ValoreData;
            if (vData != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        if (vData.V.HasValue)
                            res.Add(vData.V.Value.ToLocalTime().ToString());
                        break;
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.ValoreUtente:
                        {
                            string? formato = await GetValoreFormatInternal(sourceEntAtt);
                            if (formato == null)
                                formato = GetValoreFormatInternal(sourceAtt);

                            if (vData.V.HasValue)
                                res.Add(vData.V.Value.ToLocalTime().ToString(formato));
                        }
                        break;
                }
                return res;

            }

            var vGuid = val as ValoreGuid;
            if (vGuid != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        res.Add(vGuid.V.ToString());
                        break;
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.ValoreUtente:
                        break;
                }
                return res;
            }

            var vBool = val as ValoreBooleano;
            if (vBool != null)
            {
                switch (request)
                {
                    case ValoreRequest.Valore:
                        res.Add(vBool.V.ToString());
                        break;
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.ValoreUtente:
                        res.Add(vBool.V.ToString());
                        break;
                }
                return res;
            }

            var vElenco = val as ValoreElenco;
            if (vElenco != null)
            {
                switch (request)
                {
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.Valore:
                        res.Add(vElenco.ValoreAttributoElencoId.ToString());
                        break;
                    case ValoreRequest.ValoreUtente:
                        {
                            ValoreAttributoElenco? elencoDef = sourceAtt.ValoreAttributo as ValoreAttributoElenco;
                            if (elencoDef != null)
                            {
                                string? text = elencoDef.Items.FirstOrDefault(item => item.Id == vElenco.ValoreAttributoElencoId)?.Text;
                                if (text != null)
                                    res.Add(text);
                            }
                        }
                        break;

                }
                return res;
            }

            var vCol = val as ValoreColore;
            if (vCol != null)
            {
                switch (request)
                {
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.Valore:
                        res.Add(vCol.Hexadecimal);
                        break;
                    case ValoreRequest.ValoreUtente:
                        res.Add(vCol.V);
                        break;

                }
                return res;
            }

            var vFormatoNumero = val as ValoreFormatoNumero;
            if (vFormatoNumero != null)
            {
                switch (request)
                {
                    case ValoreRequest.Formula:
                        break;
                    case ValoreRequest.Valore:
                        res.Add(vFormatoNumero.ValoreAttributoFormatoNumeroId.ToString());
                        break;
                    case ValoreRequest.ValoreUtente:
                        res.Add(vFormatoNumero.V);
                        break;

                }
                return res;
            }

            return res;
        }


        private async Task<Attributo?> GetSourceAttributo(Attributo attributo)
        {
            Attributo att = attributo;
            while (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = (AttributoRiferimento)att;

                EntityType? entType = await GetProgettoEntityType(attRif.ReferenceEntityTypeKey);

                if (entType == null)
                    return null;

                if (!entType.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                {
                    return null;
                }

                att = entType.Attributi[attRif.ReferenceCodiceAttributo];
            }

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
            {
                if (!Entities.ContainsKey(BuiltInCodes.EntityType.Variabili))
                    await CreateProgettoEntities(BuiltInCodes.EntityType.Variabili);
            }

            return att;
        }

        private async Task<EntityAttributoValore?> GetValoreAttributo(Entity entity, string codiceAttributo)
        {
            Guid sourceEntityId = entity.EntityId;

            if (entity == null)
                return null;

            if (string.IsNullOrEmpty(codiceAttributo))
                return null;

            Entity? ent = entity;

            EntityAttributo entAtt = null;


            EntityType ? entType = await GetProgettoEntityType(ent.EntityTypeCodice);
            Attributo? att = entType?.Attributi[codiceAttributo];

            if (att?.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
            {
                //VariabiliItem varItem = _mongoDBService.VariabiliItemsCollection.Find(v => v.ProgettoId == _progettoId).FirstOrDefault();

                VariabiliItem? varItem = null;
                ProgettoTypeEntities? variabiliEnts = null;
                Entities.TryGetValue(BuiltInCodes.EntityType.Variabili, out variabiliEnts);
                if (variabiliEnts != null)
                    varItem = variabiliEnts.GetEntitiesById(variabiliEnts.GetEntitiesId()).FirstOrDefault() as VariabiliItem;

                if (varItem != null)
                {
                    ValoreAttributoVariabili? valAttVar = att.ValoreAttributo as ValoreAttributoVariabili;
                    if (valAttVar != null)
                    {
                        if (varItem.Attributi.ContainsKey(valAttVar.CodiceAttributo))
                        {
                            return new EntityAttributoValore()
                            {
                                //EntityAttributo = varItem.Attributi[valAttVar.CodiceAttributo],
                                EntityId = varItem.EntityId,
                                EntityTypeKey = att.DefinizioneAttributoCodice,
                                CodiceAttributo = valAttVar.CodiceAttributo,
                                Valore = varItem.Attributi[valAttVar.CodiceAttributo].Valore,
                            };
                        }
                    }
                }

                return null;
            }
            else if (att is AttributoRiferimento)
            {

                while (att is AttributoRiferimento)//per andare in profondità riferimento di riferimento
                {
                    AttributoRiferimento attRif = (AttributoRiferimento)att;
                    if (await IsAttributoRiferimentoGuidCollection(attRif))
                    {
                        //nel caso sia questo tipo di attributo il sorgente restituito sarà AttributoRiferimento altrimenti dovrei ritornare più di un'entità

                        Valore val = await CalculateAttributoRiferimentoGuidCollectionValore(ent, attRif);
                        var ret = new EntityAttributoValore() { CodiceAttributo = attRif.Codice, Valore = val, EntityId = sourceEntityId, EntityTypeKey = att.EntityTypeKey };

                        return ret;
                    }
                    else //attributo riferimento a Guid
                    {
                        ValoreGuid valGuid = (ValoreGuid)ent.Attributi[attRif.ReferenceCodiceGuid].Valore;
                        sourceEntityId = valGuid.V;

                        ent = await GetEntity(attRif.ReferenceEntityTypeKey, valGuid.V);


                        if (ent == null)
                            return null;

                        if (ent.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                        {
                            entAtt = ent.Attributi[attRif.ReferenceCodiceAttributo];
                        }
                        //else
                        //    return null;

                        EntityType? refEntType = await GetProgettoEntityType(attRif.ReferenceEntityTypeKey);
                        att = refEntType?.Attributi[attRif.ReferenceCodiceAttributo];
                    }

                }
                if (entAtt != null)
                    return new EntityAttributoValore() { CodiceAttributo = entAtt.AttributoCodice, Valore = entAtt.Valore, EntityId = sourceEntityId, EntityTypeKey = att.EntityTypeKey };

            }
            else if (ent.Attributi.ContainsKey(codiceAttributo))//non riferimento
            {
                entAtt = ent.Attributi[codiceAttributo];

                //string? userName = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name;
                //if (userName != null && userName.Contains("_alessandro.uliana"))
                //{

                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    if (att.ValoreAttributo is ValoreAttributoGuidCollection)
                    {
                        var attGuidCollectionSettings = (ValoreAttributoGuidCollection) att.ValoreAttributo;
                        if (attGuidCollectionSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
                        {
                            CalculateFilter(att.EntityTypeKey, ent.EntityId, (ValoreGuidCollection) entAtt.Valore);
                        }
                    }
                }
                //}

                if (entAtt != null)
                    return new EntityAttributoValore() { CodiceAttributo = entAtt.AttributoCodice, Valore = entAtt.Valore, EntityId = sourceEntityId, EntityTypeKey = att.EntityTypeKey };
            }

            return null;

        }


        private async Task CalculateFilter(string entityTypeKey, Guid entityId, ValoreGuidCollection val)
        {
            FilterData filterData = val.Filter;

            string filterDataSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(filterData);

            List<Guid> entitiesId = null;
            if (GuidCollectionsCalculated.ContainsKey(filterDataSerialized))
            {
                entitiesId = GuidCollectionsCalculated[filterDataSerialized];
            }
            else
            {

                AttributiFilter attsFilter = new AttributiFilter();
                foreach (AttributoFilterData item in filterData.Items)
                {
                    if (item.FilterType == FilterTypeEnum.Conditions)
                    {
                        ValoreConditions filterConditions = item.ValoreConditions;

                        foreach (ValoreCondition valCond in filterConditions.MainGroup.Conditions)
                        {
                            attsFilter.Load(valCond);
                        }
                    }
                }

                if (attsFilter.Groups.Any())
                {
                    ////////////////////////////////
                    //Imposto i valori per i ValoreAsItem
                    var conditionsValoreAsItem = attsFilter.GetConditions().Where(x => x.Valore == ValoreAsItem);
                    foreach (var condition in conditionsValoreAsItem)
                    {
                        var entSum = await GetEntitySummary(entityTypeKey, entityId, new HashSet<string>() { condition.Codice });
                        condition.Valore = entSum.Attributi[condition.Codice].ValoreUtente.FirstOrDefault();
                    }
                    //////////////////////////////////


                    //EntityHelper entsHelper = new EntityHelper(_mongoDBService, _progettoId);
                    //filtro su tutte le entità senza restituire attributi 
                    var res = await GetEntitiesSummary(entityTypeKey, null, attsFilter, new HashSet<string>());
                    if (res != null)
                    {
                        entitiesId = res.EntitiesId;
                        GuidCollectionsCalculated.Add(filterDataSerialized, res.EntitiesId);
                    }
                }

            }

            if (entitiesId != null)
            {
                try
                {
                    await _semaphoreCalculateFilter.WaitAsync();

                    val.V.Clear();
                    entitiesId.ForEach(id => val.V.Add(new ValoreGuidCollectionItem() { EntityId = id, Id = Guid.NewGuid() }));
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    _semaphoreCalculateFilter.Release();
                }
            }
        }

        private async Task<string> ToUserIdentity(string entityTypeKey, Guid entityId)
        {

            string entIdentity = string.Empty;

            if (!Entities.ContainsKey(entityTypeKey))
                await CreateProgettoEntities(entityTypeKey);

            Entity ent = Entities[entityTypeKey].GetEntityById(entityId);
            if (ent == null)
                return string.Empty;

            string codiceAtt1 = string.Empty;
            string codiceAtt2 = string.Empty;

            EntityAttributoValore entAttVal1 = null;
            EntityAttributoValore entAttVal2 = null;

            if (entityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
            {
                codiceAtt1 = BuiltInCodes.Attributo.Nome;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Allegati)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Nome;
                codiceAtt2 = BuiltInCodes.Attributo.Link;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Nome;
                codiceAtt2 = BuiltInCodes.Attributo.Cognome;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                codiceAtt1 = BuiltInCodes.Attributo.DescrizioneQta;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Codice;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Nome;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Codice;
                codiceAtt2 = BuiltInCodes.Attributo.Nome;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Tag)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Nome;
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.WBS)
            {
                codiceAtt1 = BuiltInCodes.Attributo.Codice;
                codiceAtt2 = BuiltInCodes.Attributo.Nome;
            }
            else
            {
                codiceAtt1 = BuiltInCodes.Attributo.Codice;
            }


            string str1 = string.Empty;
            string str2 = string.Empty;
            
            entAttVal1 = await GetValoreAttributo(ent, codiceAtt1);
            entAttVal2 = await GetValoreAttributo(ent, codiceAtt2);

            if (entAttVal1 != null)
            {
                ValoreTesto val1 = entAttVal1.Valore as ValoreTesto;
                if (val1 != null && val1.V != null)
                    str1 = val1.V;
            }

            if (entAttVal2 != null)
            {
                ValoreTesto val2 = entAttVal2.Valore as ValoreTesto;
                if (val2 != null && val2.V != null)
                    str2 = val2.V;
            }

            if (!string.IsNullOrEmpty(str2))
                entIdentity = string.Join("\n", str1, str2);
            else
                entIdentity = str1;

            return entIdentity;
        }

        private async Task<bool> IsAttributoRiferimentoGuidCollection(Attributo att)
        {
            EntityType? entType = await GetProgettoEntityType(att.EntityTypeKey);

            AttributoRiferimento? attRif = att as AttributoRiferimento;
            if (attRif == null)
                return false;

            Attributo? attGuid = entType?.Attributi[attRif.ReferenceCodiceGuid];
            if (attGuid?.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                return true;

            return false;
        }

        private async Task<Valore> CalculateAttributoRiferimentoGuidCollectionValore(Entity entity, AttributoRiferimento attRif)
        {
            Valore result = null;

            if (attRif == null)
                return result;


            string entityTypeKey = attRif.EntityTypeKey;

            Attributo? sourceAtt = await GetSourceAttributo(attRif);

            EntityType? entType = await GetProgettoEntityType(entityTypeKey);

            ValoreAttributoRiferimentoGuidCollection valAtt = attRif.ValoreAttributo as ValoreAttributoRiferimentoGuidCollection;
            if (valAtt == null)
                return null;

            EntityAttributo entAtt = entity.Attributi[attRif.ReferenceCodiceGuid];
            ValoreGuidCollection valGuidColl = entAtt.Valore as ValoreGuidCollection;

            if (valGuidColl == null)
                return null;


            ItemsSelectionTypeEnum itemsSelectionType = ItemsSelectionTypeEnum.Manual;
            if (entType.Attributi[attRif.ReferenceCodiceGuid].ValoreAttributo is ValoreAttributoGuidCollection vaAttGuidColl)
                itemsSelectionType = vaAttGuidColl.ItemsSelectionType;

            valGuidColl.V.RemoveAll(item => item == null || item.Removed);
            
            IEnumerable<Guid> ids = null;
            if (itemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
            {
                try
                {
                    await CalculateFilter(attRif.ReferenceEntityTypeKey, entity.EntityId, valGuidColl);
                    ids = valGuidColl.V.Where(item => item != null).Select(item => (item as ValoreGuidCollectionItem).EntityId).ToList();
                }
                catch (Exception exc)
                {
                    string? g = (new Random()).Next(0, 1000).ToString();
                    Log.Error("CalculateAttributoRiferimentoGuidCollectionValore {0} {1} {2}", entityTypeKey, g, JsonSerializer.Serialize(valGuidColl));
                }
            }
            else if (itemsSelectionType == ItemsSelectionTypeEnum.All)
            {
                ids = Entities[attRif.ReferenceEntityTypeKey].GetEntitiesId();
            }
            else
            {
                ids = valGuidColl.V.Select(item => (item as ValoreGuidCollectionItem).EntityId).ToList();
            }

            var ents = Entities[attRif.ReferenceEntityTypeKey].GetEntitiesById(ids);

            List<Valore> vals = new List<Valore>();
            foreach (Entity ent in ents)
            {
                EntityAttributoValore? entAttVal = await GetValoreAttributo(ent, attRif.ReferenceCodiceAttributo);
                vals.Add(entAttVal?.Valore);
            }

            int? significantDigitsCount = null;
            if (sourceAtt?.ValoreAttributo is ValoreAttributoReale valAttReale && valAttReale.UseSignificantDigitsByFormat)
            {
                NumberFormat nf = DecomposeFormat(sourceAtt.ValoreFormat);
                if (nf.DecimalDigitCount >= 0)
                {
                    significantDigitsCount = nf.DecimalDigitCount;
                }
            }

            if (sourceAtt?.ValoreAttributo is ValoreAttributoContabilita valAttCont && valAttCont.UseSignificantDigitsByFormat)
            {
                NumberFormat nf = DecomposeFormat(sourceAtt.ValoreFormat);
                if (nf.DecimalDigitCount >= 0)
                {
                    significantDigitsCount = nf.DecimalDigitCount;
                }
            }


            result = ExecuteOperation(vals, valAtt.Operation, significantDigitsCount);

            return result;

        }

        private static NumberFormat DecomposeFormat(string format)
        {
            NumberFormat numberFormat = new NumberFormat();

            List<string> list = new List<string>();

            string str = format;

            if (!str.Contains("."))//separatore decimale obbligatorio nel formato
                return null;

            list = format.Split('.').ToList();

            string str1 = Regex.Replace(list[0], _formatSpecialChars, string.Empty);
            string str2 = Regex.Replace(list[1], _formatSpecialChars, string.Empty);

            string str1TrimmedEnd = str1.TrimEnd();
            string str2TrimmedStart = str2.TrimStart();

            string str1Trimmed = str1.Trim();
            string str2Trimmed = str2.Trim();

            if (str1Trimmed.Any())
            {
                numberFormat.IsSymbolAtLeft = true;
                numberFormat.SymbolText = str1Trimmed;
                numberFormat.IsSymbolSeparated = str1TrimmedEnd.Length < str1.Length;
            }
            else
            {
                numberFormat.IsSymbolAtLeft = false;
                numberFormat.SymbolText = str2Trimmed;
                numberFormat.IsSymbolSeparated = str2TrimmedStart.Length < str2.Length;
            }

            numberFormat.LeftZeroCount = list[0].Count(item => item == '0');
            numberFormat.DecimalDigitCount = list[1].Count(item => item == '0');

            if (str.Contains(",")) //separatore di gruppo
                numberFormat.UseThousandSeparator = true;
            else
                numberFormat.UseThousandSeparator = false;

            return numberFormat;
        }

        public static Valore ExecuteOperation(IEnumerable<Valore> vals, ValoreOperationType operation, int? significantDigitsCount)
        {
            Valore result = null;

            if (vals == null)
                return null;

            Valore first = vals.FirstOrDefault();
            if (first == null)
                return null;


            //reale
            if (first is ValoreReale)
            {
                double max = Double.MinValue;
                double min = Double.MaxValue;
                double sum = 0;
                double multiplication = 1;
                foreach (Valore val in vals)
                {
                    ValoreReale valReale = val as ValoreReale;
                    if (valReale == null)
                        return null;

                    if (!valReale.RealResult.HasValue)
                        return null;

                    sum += valReale.RealResult.Value;
                    multiplication *= valReale.RealResult.Value;

                    if (valReale.RealResult.Value > max)
                        max = valReale.RealResult.Value;

                    if (valReale.RealResult.Value < min)
                        min = valReale.RealResult.Value;

                }

                ValoreReale valRealeRes = new ValoreReale();
                if (operation == ValoreOperationType.Sum)
                {
                    valRealeRes.V = sum.ToString();
                    valRealeRes.RealResult = sum;
                }
                else if (operation == ValoreOperationType.Min)
                {
                    valRealeRes.V = min.ToString();
                    valRealeRes.RealResult = min;
                }
                else if (operation == ValoreOperationType.Max)
                {
                    valRealeRes.V = max.ToString();
                    valRealeRes.RealResult = max;
                }
                else if (operation == ValoreOperationType.Average)
                {
                    double average = sum / vals.Count();

                    if (significantDigitsCount.HasValue)
                    {
                        average = Math.Round(average, significantDigitsCount.Value, MidpointRounding.AwayFromZero);
                    }

                    valRealeRes.V = average.ToString();
                    valRealeRes.RealResult = average;
                }
                else if (operation == ValoreOperationType.Multiplication)
                {
                    if (significantDigitsCount.HasValue)
                    {
                        multiplication = Math.Round(multiplication, significantDigitsCount.Value, MidpointRounding.AwayFromZero);
                    }

                    valRealeRes.V = multiplication.ToString();
                    valRealeRes.RealResult = multiplication;
                }

                result = valRealeRes;
            }
            //contabilità
            if (first is ValoreContabilita)
            {
                decimal max = Decimal.MinValue;
                decimal min = Decimal.MaxValue;
                decimal sum = 0;
                double multiplication = 1;
                foreach (Valore val in vals)
                {
                    ValoreContabilita valCont = val as ValoreContabilita;
                    if (valCont == null)
                        return null;

                    if (!valCont.RealResult.HasValue)
                        return null;

                    sum += valCont.RealResult.Value;
                    multiplication *= (double)valCont.RealResult.Value;

                    if (valCont.RealResult.Value > max)
                        max = valCont.RealResult.Value;

                    if (valCont.RealResult.Value < min)
                        min = valCont.RealResult.Value;

                }

                ValoreContabilita valContRes = new ValoreContabilita();
                if (operation == ValoreOperationType.Sum)
                {
                    valContRes.V = sum.ToString();
                    valContRes.RealResult = sum;
                }
                else if (operation == ValoreOperationType.Min)
                {
                    valContRes.V = min.ToString();
                    valContRes.RealResult = min;
                }
                else if (operation == ValoreOperationType.Max)
                {
                    valContRes.V = max.ToString();
                    valContRes.RealResult = max;
                }
                else if (operation == ValoreOperationType.Average)
                {
                    decimal average = sum / vals.Count();
                    valContRes.V = average.ToString();
                    valContRes.RealResult = average;
                }
                else if (operation == ValoreOperationType.Multiplication)
                {
                    valContRes.V = multiplication.ToString();
                    valContRes.RealResult = (decimal)multiplication;
                }
                result = valContRes;
            }
            //testo
            else if (first is ValoreTesto)
            {
                string concat = string.Empty;
                string equal = string.Empty;

                foreach (Valore val in vals)
                {
                    ValoreTesto valTesto = val as ValoreTesto;
                    if (valTesto == null)
                        return null;

                    string plainText = valTesto.Result != null ? valTesto.Result : valTesto.V;

                    concat = string.Format("{0} {1}", concat, plainText);
                    if (!equal.Any())
                        equal = plainText;
                    else if (equal != plainText)
                        equal = "[Multi]";

                }
                ValoreTesto valTestoRes = new ValoreTesto();
                if (operation == ValoreOperationType.Append)
                    valTestoRes.V = concat;
                else if (operation == ValoreOperationType.Equivalent)
                    valTestoRes.V = equal;

                result = valTestoRes;
            }
            //elenco
            else if (first is ValoreElenco)
            {
                string concat = string.Empty;
                string equal = string.Empty;

                foreach (Valore val in vals)
                {
                    ValoreElenco valEl = val as ValoreElenco;
                    if (valEl == null)
                        return null;

                    string plainText = valEl.V != null ? valEl.V : "";

                    concat = string.Format("{0} {1}", concat, plainText);
                    if (!equal.Any())
                        equal = plainText;
                    else if (equal != plainText)
                        equal = "[Multi]";

                }
                ValoreTesto valTestoRes = new ValoreTesto();
                if (operation == ValoreOperationType.Append)
                    valTestoRes.V = concat;
                else if (operation == ValoreOperationType.Equivalent)
                    valTestoRes.V = equal;

                result = valTestoRes;
            }
            //booleano
            else if (first is ValoreBooleano)
            {
                ValoreBooleano firstBool = first as ValoreBooleano;
                bool? equal = firstBool.V;

                foreach (Valore val in vals)
                {
                    ValoreBooleano valBool = val as ValoreBooleano;
                    if (valBool == null)
                        return null;

                    if (equal.HasValue)
                    {
                        if (equal.Value == true && valBool.V == false)
                            equal = null;

                        if (equal.Value == false && valBool.V == true)
                            equal = null;
                    }
                }
                ValoreBooleano valBoolRes = new ValoreBooleano();
                if (operation == ValoreOperationType.Equivalent)
                    valBoolRes.V = equal;

                result = valBoolRes;
            }

            return result;
        }

        public static MasterType GetMasterType(EntityType? entType)
        {
            if (entType == null)
                return MasterType.Nothing;

            if (entType is DivisioneItemTypeDoc || entType is DivisioneItemParentTypeDoc)
                return MasterType.Tree;

            if (entType.Codice == BuiltInCodes.EntityType.Computo ||
                entType.Codice == BuiltInCodes.EntityType.Elementi)
            {
                return MasterType.Grid;
            }
            else if (entType.Codice == BuiltInCodes.EntityType.ElencoAttivita)
            {
                return MasterType.List;
            }
            else if (entType.Codice == BuiltInCodes.EntityType.Capitoli ||
                     entType.Codice == BuiltInCodes.EntityType.CapitoliParent ||
                     entType.Codice == BuiltInCodes.EntityType.Prezzario ||
                     entType.Codice == BuiltInCodes.EntityType.PrezzarioParent ||
                     entType.Codice == BuiltInCodes.EntityType.WBS ||
                     entType.Codice == BuiltInCodes.EntityType.WBSParent)
            {
                return MasterType.Tree;
            }
            else if (entType.Codice == BuiltInCodes.EntityType.Variabili ||
                     entType.Codice == BuiltInCodes.EntityType.InfoProgetto)
            {
                return MasterType.NoMaster;
            }

            return MasterType.Nothing;

        }

        public static string CreateEntityTypeKey(string entTypeCodice, Guid? divisioneId = null)
        {
            string entTypeKey = string.Empty;
            if (entTypeCodice == BuiltInCodes.EntityType.Divisione || entTypeCodice == BuiltInCodes.EntityType.DivisioneParent)
                entTypeKey = string.Format("{0}-{1}", entTypeCodice, divisioneId.ToString());
            else
                entTypeKey = entTypeCodice;

            return entTypeKey;
        }



        private string GetValoreFormatInternal(Attributo att)
        {


            if (att.ValoreFormat != null && att.ValoreFormat.Any())
                return att.ValoreFormat;

            return DefaultFormat;
        }

        private async Task<string?> GetValoreFormatInternal(EntitiesAttributoValore entAtt)
        {
            if (entAtt == null)
                return null;

            ValoreReale? valReale = entAtt?.Valore as ValoreReale;
            if (valReale != null)
            {
                //prendo dal valore
                if (valReale.Format != null && valReale.Format.Any())
                    return valReale.Format;


                string? codiceAtt = entAtt?.CodiceAttributo;

                //prendo dall'attributo riferito (unità di misura)
                if (codiceAtt == BuiltInCodes.Attributo.Quantita || codiceAtt == BuiltInCodes.Attributo.QuantitaTotale)
                {
                    string formato = String.Empty;
                    //ritorno il formato solo se tutte le entità hanno lo stesso formato per la quantità

                    foreach (Guid entId in entAtt.EntitiesId)
                    {
                        Entity? sourceEntity = await GetEntity(entAtt.EntityTypeKey, entId);
                        if (sourceEntity == null)
                            return null;

                        EntityAttributoValore? formatoQtaEntAtt = await GetValoreAttributo(sourceEntity, BuiltInCodes.Attributo.PrezzarioItem_FormatoQuantita);

                        if (formatoQtaEntAtt == null)
                            return string.Empty;


                        ValoreFormatoNumero? valFormato = formatoQtaEntAtt?.Valore as ValoreFormatoNumero;
                        if (valFormato != null)
                        {
                            if (formato == null)
                                formato = valFormato.V;
                            else if (formato != valFormato.V)
                                return null;
                        }
                    }
                }
            }


            ValoreContabilita? valCont = entAtt?.Valore as ValoreContabilita;
            if (valCont != null)
            {
                //prendo dal valore
                if (valCont.Format != null && valCont.Format.Any())
                    return valCont.Format;
            }


            return null;
        }

        private string GetPaddedFormat(string format)
        {
            return string.Format("{0}0:{1}{2}", "{", format, "}");
        }



        private class EntityAttributoValore
        {
            public Guid EntityId { get; set; }
            public string EntityTypeKey { get; set; } = string.Empty;
            public string CodiceAttributo { get; set; } = String.Empty;
            public Valore? Valore { get; set; } = null;
        }

        private class EntitiesAttributoValore
        {
            public List<Guid> EntitiesId { get; set; } = new List<Guid>();
            public string EntityTypeKey { get; set; } = string.Empty;
            public string CodiceAttributo { get; set; } = String.Empty;
            public Valore? Valore { get; set; } = null;
            public bool IsMultiValore { get; set; } = false;
            public bool IsMultiValoreFormula { get; set; } = false;
            public bool IsMultiValoreDescrizione { get; set; } = false;
            public bool IsValidValore { get; set; } = false;

            public void OperationWith(Valore? val, ValoreOperationType op, int count)
            {

                if (Valore != null && Valore.GetType() != val?.GetType())
                    return;

                if (val is ValoreReale)
                {
                    ValoreReale? v0 = Valore as ValoreReale;
                    ValoreReale? v1 = val as ValoreReale;
                    

                    if (Valore == null)
                    {
                        Valore = v0 = new ValoreReale() { RealResult = v1.RealResult, V = v1.V, ResultDescription = v1.ResultDescription };
                    }
                    else
                    {
                        if (op == ValoreOperationType.Sum)
                            v0.RealResult += v1.RealResult;
                        else if (op == ValoreOperationType.Multiplication)
                            v0.RealResult *= v1.RealResult;
                        else if (op == ValoreOperationType.Average)
                            v0.RealResult += (v1.RealResult / count);
                        else if (op == ValoreOperationType.Min)
                        {
                            if (v0.RealResult > v1.RealResult)
                                v0.RealResult = v1.RealResult;
                        }
                        else if (op == ValoreOperationType.Max)
                        {
                            if (v0.RealResult < v1.RealResult)
                                v0.RealResult = v1.RealResult;
                        }
                        else if (op == ValoreOperationType.Equivalent)
                        {
                            if (v0.RealResult != v1.RealResult)
                            {
                                v0.RealResult = null;
                                IsMultiValore = true;
                            }
                        }
                        else //Equivalent
                        {
                            if (v0.RealResult != v1.RealResult)
                            {
                                v0.RealResult = null;
                                IsMultiValore = true;
                            }
                        }

                        if (v0.V != v1.V)//formula
                        {
                            v0.V = String.Empty;
                            IsMultiValoreFormula = true;
                        }

                        if (v0.ResultDescription != v1.ResultDescription)//descrizione
                        {
                            v0.ResultDescription = String.Empty;
                            IsMultiValoreDescrizione = true;
                        }
                    }

                    if (v0.RealResult != null && !double.IsNaN(v0.RealResult.Value))
                        IsValidValore = true;

                }
                else if (val is ValoreContabilita)
                {
                    ValoreContabilita? v0 = Valore as ValoreContabilita;
                    ValoreContabilita? v1 = val as ValoreContabilita;

                    if (Valore == null)
                    {
                        Valore = v0 = new ValoreContabilita() { RealResult = v1.RealResult, V = v1.V, ResultDescription = v1.ResultDescription};
                    }
                    else
                    {
                        if (op == ValoreOperationType.Sum)
                            v0.RealResult += v1.RealResult;
                        else if (op == ValoreOperationType.Multiplication)
                            v0.RealResult *= v1.RealResult;
                        else if (op == ValoreOperationType.Average)
                            v0.RealResult += (v1.RealResult / count);
                        else if (op == ValoreOperationType.Min)
                        {
                            if (v0.RealResult > v1.RealResult)
                                v0.RealResult = v1.RealResult;
                        }
                        else if (op == ValoreOperationType.Max)
                        {
                            if (v0.RealResult < v1.RealResult)
                                v0.RealResult = v1.RealResult;
                        }
                        else if (op == ValoreOperationType.Equivalent)
                        {
                            if (v0.RealResult != v1.RealResult)
                            {
                                v0.RealResult = null;
                                IsMultiValore = true;
                            }
                        }
                        else //Equivalent
                        {
                            if (v0.RealResult != v1.RealResult)
                            {
                                v0.RealResult = null;
                                IsMultiValore = true;
                            }
                        }

                        if (v0.V != v1.V)//formula
                        {
                            v0.V = String.Empty;
                            IsMultiValoreFormula = true;
                        }

                        if (v0.ResultDescription != v1.ResultDescription)//descrizione
                        {
                            v0.ResultDescription = String.Empty;
                            IsMultiValoreDescrizione = true;
                        }
                    }

                    if (v0.RealResult != null)
                        IsValidValore = true;
                }
                else if (val is ValoreTesto)
                {
                    ValoreTesto? v0 = Valore as ValoreTesto;
                    ValoreTesto? v1 = val as ValoreTesto;

                    if (Valore == null)
                    {
                        Valore = v0 = new ValoreTesto() { Result = v1.Result, V = v1.V };
                    }
                    else
                    {
                        if (op == ValoreOperationType.Equivalent || op == ValoreOperationType.Nothing)
                        {
                            if (v0.Result != v1.Result)
                            {
                                v0.Result = null;
                                IsMultiValore = true;

                                //oss: se non lo metto valido non esce tra gli attributi raggruppabili
                                IsValidValore = true;
                            }
                        }
                        else
                        {
                            v0.Result = null;
                        }

                        if (v0.V != v1.V)
                            v0.V = String.Empty;
                    }

                    if (v0.Result != null && v0.Result != ValoreNullAsText)
                        IsValidValore = true;

                }
                else if (val is ValoreTestoRtf)
                {
                    ValoreTestoRtf? v0 = Valore as ValoreTestoRtf;
                    ValoreTestoRtf? v1 = val as ValoreTestoRtf;

                    if (Valore == null)
                    {
                        Valore = v0 = new ValoreTestoRtf() { V = v1.V };
                    }
                    else
                    {
                        if (v0.V != v1.V)
                        {
                            v0.V = null;
                            IsMultiValore = true;
                        }
                    }

                    if (v0 != null && v0.V != null)
                        IsValidValore = true;
                }
                else if (val is ValoreGuid)
                {
                    ValoreGuid? v1 = val as ValoreGuid;
                    ValoreGuid? v0 = Valore as ValoreGuid;

                    if (Valore == null)
                        Valore = new ValoreGuid() { V = v1.V };
                    else
                    {
                        if (v0.V != v1.V)
                        {
                            v0.V = Guid.Empty;
                            IsMultiValore = true;
                        }
                    }
                }
                else if (val is ValoreElenco)
                {
                    ValoreElenco? v0 = Valore as ValoreElenco;
                    ValoreElenco? v1 = val as ValoreElenco;

                    if (Valore == null)
                        Valore = v0 = new ValoreElenco() { V = v1.V, ValoreAttributoElencoId = v1.ValoreAttributoElencoId };
                    else
                    {
                        if (op == ValoreOperationType.Equivalent)
                        {
                            if (v0.ValoreAttributoElencoId != v1.ValoreAttributoElencoId)
                            {
                                v0.V = null;
                                v0.ValoreAttributoElencoId = 0;
                                IsMultiValore = true;
                            }
                        }
                        else
                        {
                            v0.V = null;
                            v0.ValoreAttributoElencoId = 0;
                        }
                    }

                    if (v0.ValoreAttributoElencoId > 0)
                        IsValidValore = true;
                }
                else if (val is ValoreBooleano)
                {
                    ValoreBooleano? v0 = Valore as ValoreBooleano;
                    ValoreBooleano? v1 = val as ValoreBooleano;

                    if (Valore == null)
                        Valore = v0 = new ValoreBooleano() { V = v1.V };
                    else
                    {
                        if (op == ValoreOperationType.Equivalent)
                        {
                            if (v0.V != v1.V)
                            {
                                v0.V = null;
                                IsMultiValore = true;
                            }
                        }
                        else
                        {
                            v0.V = null;
                        }
                    }

                    if (v0.V != null)
                        IsValidValore = true;
                }
                else if (val is ValoreData)
                {
                    ValoreData? v0 = Valore as ValoreData;
                    ValoreData? v1 = val as ValoreData;

                    if (Valore == null)
                        Valore = v0 = new ValoreData() { V = v1.V };
                    else
                    {
                        if (op == ValoreOperationType.Equivalent)
                        {
                            if (v0.V != v1.V)
                            {
                                v0.V = null;
                                IsMultiValore = true;
                            }
                        }
                        else
                        {
                            v0.V = null;
                        }
                    }

                    if (v0.V != null)
                        IsValidValore = true;
                }
                else if (val is ValoreTestoCollection)
                {
                    var v0 = Valore as ValoreTestoCollection;
                    var v1 = val as ValoreTestoCollection;

                    if (Valore == null)
                    {
                        Valore = v0 = Clone(v1) as ValoreTestoCollection;
                    }
                    else
                    {
                        if (!Equals(v0, v1))
                        {
                            v0.V = null;
                            IsMultiValore = true;
                        }
                    }

                    if (v0.V != null)
                        IsValidValore = true;

                }
                else if (val is ValoreGuidCollection)
                {
                    var v0 = Valore as ValoreGuidCollection;
                    var v1 = val as ValoreGuidCollection;

                    if (Valore == null)
                    {
                        Valore = v0 = Clone(v1) as ValoreGuidCollection;
                    }
                    else
                    {
                        if (!Equals(v0, v1))
                        {
                            v0.V = null;
                            IsMultiValore = true;
                        }
                    }

                    if (v0.V != null)
                        IsValidValore = true;
                }

            }

            static Valore? Clone(ValoreTestoCollection? v)
            {
                if (v == null)
                    return null;

                var vClone = new ValoreTestoCollection();
                if (v.V != null)
                    v.V.ForEach(item => vClone.V.Add(new ValoreTestoCollectionItem() { Testo1 = (item as ValoreTestoCollectionItem).Testo1, Testo2 = (item as ValoreTestoCollectionItem).Testo2 }));

                return vClone;
            }

            static Valore? Clone(ValoreGuidCollection? v)
            {
                if (v == null)
                    return null;

                var vClone = new ValoreGuidCollection();

                if (v.V == null)
                    return vClone;

                IEnumerable<Guid> idsOrdered = v.V.Select(item => (item as ValoreGuidCollectionItem).EntityId);
                vClone.V.AddRange(idsOrdered.Select(item => new ValoreGuidCollectionItem() { Id = new Guid(), EntityId = item }));

                return vClone;
            }

            static bool Equals(ValoreTestoCollection v0, ValoreTestoCollection v1)
            {
                if (v0.V == null && v1.V == null)
                    return true;

                if (v0.V == null)
                    return false;

                if (v1.V == null)
                    return false;

                if (v0.V.Count != v1.V.Count)
                    return false;

                var items0Ordered = v0.V.OrderBy(item => (item as ValoreTestoCollectionItem).Testo1).OrderBy(item => (item as ValoreTestoCollectionItem).Testo2).ToList();
                var items1Ordered = v1.V.OrderBy(item => (item as ValoreTestoCollectionItem).Testo1).OrderBy(item => (item as ValoreTestoCollectionItem).Testo2).ToList();

                for (int i = 0; i< v0.V.Count; i++)
                {
                    if (!Equals(items0Ordered[i], items1Ordered[i]))
                        return false;
                }

                return true;
            }

            static bool Equals(ValoreGuidCollection v0, ValoreGuidCollection v1)
            {
                if (v0.V == null && v1.V == null)
                    return true;

                if (v0.V == null)
                    return false;

                if (v1.V == null)
                    return false;

                if (v0.V.Count != v1.V.Count)
                    return false;

                var ids0Ordered = v0.V.Select(item => (item as ValoreGuidCollectionItem).EntityId).Order().ToList();
                var ids1Ordered = v1.V.Select(item => (item as ValoreGuidCollectionItem).EntityId).Order().ToList();

                for (int i = 0; i < ids0Ordered.Count; i++)
                    if (ids0Ordered[i] != ids1Ordered[i])
                        return false;

                return true;
            }

            static bool Equals(ValoreTestoCollectionItem v0, ValoreTestoCollectionItem v1)
            {
                if (v0 == null && v1 == null)
                    return true;

                if (v0 == null)
                    return false;

                if (v1 == null)
                    return false;

                if (v0.Testo1 != v1.Testo1)
                    return false;

                if (v0.Testo2 != v1.Testo2)
                    return false;

                return true;
            }

        }

        

        public enum ValoreRequest
        {
            Valore = 0,
            ValoreUtente,
            Formula,
            Descrizione,
        }

    }

    /// <summary>
    /// Classe che contiene le entità di un entityType in un progetto 
    /// </summary>
    public class ProgettoTypeEntities
    {
        public Guid ProgettoId { get; set; } = Guid.Empty;
        public EntityType? EntityType { get; set; } = null;

        public EntityType? ParentEntityType { get; set; } = null;

        private Dictionary<Guid, Entity> _entitiesById = new Dictionary<Guid, Entity>();
        private List<Entity> _entitiesByIndex = new List<Entity>();
        private Dictionary<Guid, Guid> _entitiesParent = new Dictionary<Guid, Guid>();

        public Entity? GetEntity(Guid id)
        {
            Entity? ent = null;
            _entitiesById.TryGetValue(id, out ent);
            return ent;
        }
        public Entity? GetEntity(int index)
        {
            Entity? ent = null;
            if (0 <= index && index < _entitiesByIndex.Count)
                ent = _entitiesByIndex[index];

            return ent;
        }
        public TreeEntity? GetParentEntity(Guid id)
        {
            Guid parentId = Guid.Empty;
            _entitiesParent.TryGetValue(id, out parentId);
            if (parentId != Guid.Empty)
                return GetEntity(parentId) as TreeEntity;

            return null;
        }
        public int Count { get => _entitiesByIndex.Count; }

        public IEnumerable<Guid> GetEntitiesId()
        {
            return _entitiesById.Keys;
        }

        public void SetEntities(List<Entity> ents)
        {
            _entitiesByIndex = ents;
            _entitiesById.Clear();
            _entitiesParent.Clear();

            MasterType masterType = EntityHelper.GetMasterType(EntityType);
            if (masterType == MasterType.Tree)
            {
                List<Guid> parentId = new List<Guid>();
                TreeEntity? entPrec = null;

                for (int i = 0; i < _entitiesByIndex.Count; i++)
                {
                    TreeEntity? ent = _entitiesByIndex[i] as TreeEntity;
                    if (ent == null)
                        return;

                    if (entPrec != null && ent.Depth == entPrec.Depth + 1)
                        parentId.Add(entPrec.EntityId);

                    if (ent.Depth < parentId.Count)
                        parentId = parentId.Take(ent.Depth).ToList();


                    _entitiesParent.Add(ent.EntityId, parentId.LastOrDefault());

                    _entitiesById.Add(_entitiesByIndex[i].EntityId, _entitiesByIndex[i]);

                    entPrec = ent;
                }
            }
            else
            {
                _entitiesById = _entitiesByIndex.ToDictionary(item => item.EntityId, item => item);
            }
        }

        public Guid GetFirstEntityId()
        {
            Entity? ent = _entitiesByIndex.FirstOrDefault();
            return (ent != null) ? ent.EntityId : Guid.Empty;
        }

        public IEnumerable<Entity> GetEntitiesById(IEnumerable<Guid> ids)
        {
            return ids.Select(item => _entitiesById[item]);
        }

        public Entity GetEntityById(Guid id)
        {
            Entity ent = null;
            _entitiesById.TryGetValue(id, out ent);
            return ent;
        }

    }

    public interface IAttributiFilter
    {
        IEnumerable<Guid> GetResult();
    }

    public class AttributiFilter : IAttributiFilter
    {
        public List<IAttributiFilter> Groups { get; set; } = new List<IAttributiFilter>();
        public ValoreConditionsGroupOperator Operator { get; set; } = ValoreConditionsGroupOperator.And;

        public List<AttributoValoreCondition> GetConditions()
        {
            var conds = new List<AttributoValoreCondition>();
            GetConditionsRecursive(this, conds);
            return conds;/*.ToDictionary(item => item.Codice, item => item)*/
        }


        public IEnumerable<Guid> GetResult()
        {
            var res = Enumerable.Empty<Guid>();

            if (Groups == null || !Groups.Any())
                return res;

            if (Operator == ValoreConditionsGroupOperator.And)
            {
                IEnumerable<Guid> list = new List<Guid>(Groups[0].GetResult());

                for (int i = 1; i < Groups.Count; i++)
                    list = list.Intersect(Groups[i].GetResult());

                res = list;
            }
            else if (Operator == ValoreConditionsGroupOperator.Or)
            {
                HashSet<Guid> set = new HashSet<Guid>(Groups[0].GetResult());
                for (int i = 1; i < Groups.Count; i++)
                    set.UnionWith(Groups[i].GetResult());

                res = set;
            }
            return res;
        }


        private void GetConditionsRecursive(AttributiFilter attsFilter, List<AttributoValoreCondition> conds)
        {
            foreach (var item in attsFilter.Groups)
            {
                if (item is AttributoValoreCondition)
                    conds.Add(item as AttributoValoreCondition);
                else 
                    GetConditionsRecursive(item as AttributiFilter, conds);
            }

        }

        public void Load(ValoreCondition valCond)
        {
            if (valCond is ValoreConditionsGroup)
            {
                var source = (ValoreConditionsGroup)valCond;
                

                AttributiFilter target = new AttributiFilter();
                target.Operator = source.Operator;

                foreach (var cond in source.Conditions)
                {
                    target.Load(cond);
                }
                Groups.Add(target);

            }
            else if (valCond is AttributoValoreConditionSingle)
            {
                var source = (AttributoValoreConditionSingle)valCond;

                var target = new AttributoValoreCondition();
                target.Codice = source.CodiceAttributo;

                if (source.ValoreConditionSingle != null)
                {
                    target.Condition = source.ValoreConditionSingle.Condition;
                    target.Valore = EntityHelper.GetValoreAsString(source.ValoreConditionSingle.Valore);
                  
                }

                Groups.Add(target);

            }
            else if (valCond is ValoreConditionSingle)//Serve ancora???
            {
                var source = (ValoreConditionSingle)valCond;
            }


        }

        public static bool CheckFilterConditions(string compTesto, string testo, ValoreConditionEnum condition)
        {

            if (compTesto != null && testo != null)
            {
                switch (condition)
                {
                    case ValoreConditionEnum.StartsWith:
                        return testo.StartsWith(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.EndsWith:
                        return testo.EndsWith(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Contains:
                        {
                            //separazione e ricerca separata delle parole
                            
                            bool check = false;
                            string[] condArray = compTesto.Split(" ");
                            if (condArray.Count() == 1)
                            {
                                return testo.Contains(condArray[0], StringComparison.CurrentCultureIgnoreCase);
                            }
                            else
                            {
                                foreach (var cond in condArray)
                                {
                                    check = AttributiFilter.CheckFilterConditions(cond, testo, condition);
                                    if (!check)
                                        break;
                                }
                            }
                            return check;
                            //return testo.Contains(compTesto, StringComparison.CurrentCultureIgnoreCase);
                        }
                    case ValoreConditionEnum.NotContains:
                        return !testo.Contains(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Equal:
                        return testo.Equals(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Unequal:
                        return !testo.Equals(compTesto, StringComparison.CurrentCultureIgnoreCase);

                }
            }
            else if (compTesto == null)
            {
                switch (condition)
                {
                    case ValoreConditionEnum.StartsWith:
                    case ValoreConditionEnum.EndsWith:
                    case ValoreConditionEnum.Contains:
                        return true;
                    case ValoreConditionEnum.NotContains:
                        return false;
                    case ValoreConditionEnum.Equal:
                        return testo == null;
                }
            }

            return false;
        }

        public static bool CheckFilterConditions(DateTime? singleData, DateTime? reale, ValoreConditionEnum condition)
        {
            if (singleData.HasValue && reale.HasValue)
            {


                switch (condition)
                {
                    case ValoreConditionEnum.Equal:
                        return reale == singleData;
                    case ValoreConditionEnum.GreaterOrEqualThan:
                        return reale >= singleData;
                    case ValoreConditionEnum.GreaterThan:
                        return reale > singleData;
                    case ValoreConditionEnum.LessOrEqualThan:
                        return reale <= singleData;
                    case ValoreConditionEnum.LessThan:
                        return reale < singleData;
                    case ValoreConditionEnum.Unequal:
                        return reale != singleData;
                }
            }
            else
            {
                if (singleData.HasValue || reale.HasValue)
                {
                    switch (condition)
                    {
                        case ValoreConditionEnum.Equal:
                        case ValoreConditionEnum.GreaterOrEqualThan:
                        case ValoreConditionEnum.LessOrEqualThan:
                        case ValoreConditionEnum.GreaterThan:
                        case ValoreConditionEnum.LessThan:
                            return false;
                        case ValoreConditionEnum.Unequal:
                            return true;
                    }
                }
                else //null, null
                {
                    switch (condition)
                    {
                        case ValoreConditionEnum.Equal:
                        case ValoreConditionEnum.GreaterOrEqualThan:
                        case ValoreConditionEnum.LessOrEqualThan:
                            return true;
                        case ValoreConditionEnum.GreaterThan:
                        case ValoreConditionEnum.LessThan:
                        case ValoreConditionEnum.Unequal:
                            return false;
                    }
                }
            }
            return false;
        }

        public static bool CheckFilterConditions(double singleReale, double reale, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    return reale == singleReale;
                case ValoreConditionEnum.GreaterOrEqualThan:
                    return reale >= singleReale;
                case ValoreConditionEnum.GreaterThan:
                    return reale > singleReale;
                case ValoreConditionEnum.LessOrEqualThan:
                    return reale <= singleReale;
                case ValoreConditionEnum.LessThan:
                    return reale < singleReale;
                case ValoreConditionEnum.Unequal:
                    return reale != singleReale;
            }
            return false;
        }

        public static bool CheckFilterConditions(decimal singleDec, decimal dec, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    return dec == singleDec;
                case ValoreConditionEnum.GreaterOrEqualThan:
                    return dec >= singleDec;
                case ValoreConditionEnum.GreaterThan:
                    return dec > singleDec;
                case ValoreConditionEnum.LessOrEqualThan:
                    return dec <= singleDec;
                case ValoreConditionEnum.LessThan:
                    return dec < singleDec;
                case ValoreConditionEnum.Unequal:
                    return dec != singleDec;
            }
            return false;
        }

        public static bool CheckFilterConditions(bool singleBool, bool booleano, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    return booleano == singleBool;
                case ValoreConditionEnum.Unequal:
                    return booleano != singleBool;
            }
            return false;
        }

        public static bool CheckFilterConditions(Guid singleGuid, Guid guid, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    return singleGuid == guid;
                case ValoreConditionEnum.Unequal:
                    return singleGuid != guid;
            }
            return false;
        }

    }

    public class AttributoValoreCondition : IAttributiFilter
    {
        public string Codice { get; set; } = string.Empty;
        public string? Valore { get; set; } = null;
        public ValoreConditionEnum Condition { get; set; } = ValoreConditionEnum.Equal;
        public List<Guid> Result { get; set; } = new List<Guid>();
        public IEnumerable<Guid> GetResult() { return Result; }


    }


    public class EntitySummary
    {
        public Guid EntityId { get; set; } = Guid.Empty;
        public string EntityTypeKey { get; set; } = string.Empty;
        public Dictionary<string, EntitiesSummaryAttributo> Attributi = new Dictionary<string, EntitiesSummaryAttributo>();

    }

    public class EntitiesSummary
    {
        public List<Guid> EntitiesId { get; set; } = new List<Guid>();
        public string EntityTypeKey { get; set; } = string.Empty;
        public Dictionary<string, EntitiesSummaryAttributo> Attributi = new Dictionary<string, EntitiesSummaryAttributo>();

    }


    public class EntitiesSummaryAttributo
    {
        public string Codice { get; set; } = string.Empty;
        public string SourceCodice { get; set; } = string.Empty; // TODO: da qui estraggo l'attributo che mi serve per capire se si può applicare il filtro, da cui estraggo definizioneattributo che mi permette di capire il type se è valido 
        public string SourceEntityTypeKey { get; set; } = string.Empty;
        public string Etichetta { get; set; } = string.Empty;
        public List<string> ValoreUtente { get; set; } = new List<string>();
        public List<string> Valore { get; set; } = new List<string>();
        public string? ValoreFormula { get; set; } = string.Empty;//reale, contabilità, testo
        public string? ValoreDescrizione { get; set; } = string.Empty;//reale, contabilità, testo
        public string DefinizioneAttributoCodice { get; set; } = string.Empty;
        public string GuidReferenceEntityTypeKey { get; set; } = String.Empty;
        public bool IsMultiValore { get; set; } = false;
        public bool IsMultiValoreFormula { get; set; } = false;
        public bool IsMultiValoreDescrizione { get; set; } = false;
    }

    public class ValoreEntitiesGroup
    {
        public string EntityTypeKey { get; set; } = string.Empty;
        public string CodiceAttributo { get; set; } = string.Empty;
        public string Valore { get; set; } = string.Empty;
        public HashSet<Guid> EntitiesId { get; set; } = new HashSet<Guid>();
    }

    public class NumberFormat
    {
        public bool IsSymbolAtLeft { get; set; }
        public bool IsSymbolSeparated { get; set; }
        public bool UseThousandSeparator { get; set; }
        public string SymbolText { get; set; }
        public int LeftZeroCount { get; set; }
        public int DecimalDigitCount { get; set; }

    }


    public class EntityHelper_test
    {
        public static MongoDbService MongoDbService;
        static EntityHelper _entHelper = null;
        static Guid _progettoId = Guid.Empty;


        public async static void Test()
        {

            _progettoId = new Guid("8cde5799-b671-48a2-b803-bfed64ad2304"); //esempio desktop


            //_entHelper = new EntityHelper(MongoDbService, _progettoId);
            _entHelper = await EntityHelper.Create(MongoDbService, _progettoId);


            //Test_WBS();

            //FiltroSuComputo();

            //CreateAllEntities();

            //IsAllowMasterGrouping();

            //computoItemsToIfcGlobalIds_test();

            computoItemsFromIfcGlobalIds();

        }

        static async void AllowMasterGrouping()
        {
            EntityType? entType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.Computo);

            Attributo? att = await _entHelper.GetSourceAttributo(BuiltInCodes.EntityType.Computo, "118");


        }

        static async void CreateAllEntities()
        {
            

            long pre = GC.GetTotalMemory(true);

            await _entHelper.CreateAllProgettoEntities();

            long post = GC.GetTotalMemory(true);


        }



        private static async void Test_WBS()
        {
            //bool aaa = await _entHelper.IsAttributoDeep(BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.DescrizioneRTF);
            //bool bbb = await _entHelper.IsAttributoDeep(BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Codice);
            //bool ddd = await _entHelper.IsAttributoDeep(BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.UnitaMisura);
            //bool rrr = await _entHelper.IsAttributoDeep("DivisioneItem-d145abdb-5536-457c-aaec-e142ecccbd20", BuiltInCodes.Attributo.Nome);


            EntityType? entType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.WBS);

            if (entType != null)
            {

                var entsSum = await _entHelper.GetEntitiesSummary(entType.Codice, null, null);

                foreach (var entId in entsSum.EntitiesId)
                {
                    var ccc = await _entHelper.GetEntitySummary(entType.Codice, entId);

                }
            }
        }


        private static async void Test_Computo()
        {


            EntityType entType = await _entHelper.GetEntityType(BuiltInCodes.EntityType.Computo);

            if (entType != null)
            {
                Attributo att = entType.Attributi.Values.Last();
                string codiceAtt = att.Codice;
                string etAtt = att.Codice;

                var attributiSelect = new HashSet<string>() { "QuantitaTotale" };
                var entsSum = await _entHelper.GetEntitiesSummary(entType.Codice, null, null, attributiSelect);
            }

        }

        private static async void Test_InfoProgetto()
        {
            var attributiSelect = new HashSet<string>() { "33" };
            var entsSum = await _entHelper.GetEntitySummary(BuiltInCodes.EntityType.InfoProgetto, attributiSelect);

        }

        private static async void Test_InfoProgettoxMatteo()
        {
            var entsSum = await _entHelper.GetEntitySummary(BuiltInCodes.EntityType.InfoProgetto);
        }

        private static async void FiltroSuComputo()
        {
            AttributiFilter attFilter = new AttributiFilter();
            attFilter.Operator = ValoreConditionsGroupOperator.Or;

            //attFilter.Groups.Add(new AttributoValoreCondition()
            //{
            //    Codice = "ElementiItem_IfcBuildingStorey_Name",
            //    Valore = "1 - Primo Piano",//EntityHelper.ValoreNullAsText,
            //    Condition = ValoreConditionEnum.Equal,
            //});

            //attFilter.Groups.Add(new AttributoValoreCondition()
            //{
            //    Codice = "ElementiItem_IfcBuildingStorey_Name",
            //    Valore = "2 - Secondo Piano",//EntityHelper.ValoreNullAsText,
            //    Condition = ValoreConditionEnum.Equal,
            //});

            //attFilter.Groups.Add(new AttributoValoreCondition()
            //{
            //    Codice = "Quantita",
            //    Valore = "10,2",//EntityHelper.ValoreNullAsText,
            //    Condition = ValoreConditionEnum.GreaterThan,
            //});

            //attFilter.Groups.Add(new AttributoValoreCondition()
            //{
            //    Codice = "PrezzarioItem_DescrizioneRTF",
            //    Valore = "Caldo coperture",//EntityHelper.ValoreNullAsText,
            //    Condition = ValoreConditionEnum.Contains,
            //});

            attFilter.Groups.Add(new AttributoValoreCondition()
            {
                Codice = "PrezzarioItem_Attivita",
                Valore = "Scavo sbancamento",//EntityHelper.ValoreNullAsText,
                Condition = ValoreConditionEnum.Contains,
            });

            var attributiSelect = new HashSet<string>() { "Importo" };
            var entsSum = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Computo, null, attFilter, attributiSelect);
        }

        private static async void Computo()
        {
            var entsSum = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Computo, null, null, null);
        }

        private static async void GetValoriUnivoci()
        {
            var buildingStoreys = await _entHelper.GetValoriUnivoci_deprecated(BuiltInCodes.EntityType.Computo, null, "ElementiItem_IfcBuildingStorey_Name");
        }

        private static async void computoItemsToIfcGlobalIdsst()
        {
            List<Guid> computoItemsId = null;


            List<Guid>? elemsId = await _entHelper.GetEntitiesReferred(BuiltInCodes.EntityType.Elementi, BuiltInCodes.EntityType.Computo, computoItemsId);

            foreach (var elemId in elemsId)
            {

                EntitySummary? entSum = await _entHelper.GetEntitySummary(BuiltInCodes.EntityType.Elementi, elemId, new HashSet<string>() { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId });

                if (entSum == null)
                    continue;

                string? ifcProjectGlobalId = entSum.Attributi[BuiltInCodes.Attributo.ProjectGlobalId].Valore.FirstOrDefault();
                string? ifcElemGlobalId = entSum.Attributi[BuiltInCodes.Attributo.GlobalId].Valore.FirstOrDefault();

            }
        }
        private static async void computoItemsFromIfcGlobalIds()
        {
            ///////////////////////////////////////////////////////////////
            ///Costruzione filtro
            AttributiFilter attFilterIfc = new AttributiFilter();
            attFilterIfc.Operator = ValoreConditionsGroupOperator.Or;

            //foreach(...)
            {
                string? ifcProjectGlobalId = "2PbgA5NWL6Hvsf4dsgw6PE";
                string? ifcElemGlobalId = "1x$hwXm9b7IeC9KnmrR0MM";

                AttributiFilter attFilterAnd = new AttributiFilter();
                attFilterAnd.Operator = ValoreConditionsGroupOperator.And;

                attFilterAnd.Groups.Add(new AttributoValoreCondition()
                {
                    Codice = BuiltInCodes.Attributo.ProjectGlobalId,
                    Valore = ifcProjectGlobalId,
                    Condition = ValoreConditionEnum.Equal,
                });

                attFilterAnd.Groups.Add(new AttributoValoreCondition()
                {
                    Codice = BuiltInCodes.Attributo.GlobalId,
                    Valore = ifcElemGlobalId,
                    Condition = ValoreConditionEnum.Equal,
                });

                attFilterIfc.Groups.Add(attFilterAnd);
            }
            //////////////////////////////////////////////////////////


            var elemsSum = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Elementi, null, attFilterIfc, new HashSet<string>());


            //filtro da aggiungere in AND al filtro precedente del computo
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

            var computoSum = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Computo, null, attFilterComputoElemsId, new HashSet<string>());



            //List<Guid>? computoItemsId = await _entHelper.GetEntitiesReference(BuiltInCodes.EntityType.Elementi, BuiltInCodes.EntityType.Computo, elemsSum.EntitiesId);

        }


        private static async void computoItemsToIfcGlobalIds_test() //andata e ritorno
        {
            List<Guid> computoItemsId = null;


            List<Guid>? elemsId = await _entHelper.GetEntitiesReferred(BuiltInCodes.EntityType.Elementi, BuiltInCodes.EntityType.Computo, computoItemsId);



            AttributiFilter attFilterIfc = new AttributiFilter();
            attFilterIfc.Operator = ValoreConditionsGroupOperator.Or;



            foreach (var elemId in elemsId)
            {

                EntitySummary? entSum = await _entHelper.GetEntitySummary(BuiltInCodes.EntityType.Elementi, elemId, new HashSet<string>() { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId });

                if (entSum == null)
                    continue;

                string? ifcProjectGlobalId = entSum.Attributi[BuiltInCodes.Attributo.ProjectGlobalId].Valore.FirstOrDefault();
                string? ifcElemGlobalId = entSum.Attributi[BuiltInCodes.Attributo.GlobalId].Valore.FirstOrDefault();


                ///////
                ///reverse


                AttributiFilter attFilterAnd = new AttributiFilter();
                attFilterAnd.Operator = ValoreConditionsGroupOperator.And;

                attFilterAnd.Groups.Add(new AttributoValoreCondition()
                {
                    Codice = BuiltInCodes.Attributo.ProjectGlobalId,
                    Valore = ifcProjectGlobalId,
                    Condition = ValoreConditionEnum.Equal,
                });

                attFilterAnd.Groups.Add(new AttributoValoreCondition()
                {
                    Codice = BuiltInCodes.Attributo.GlobalId,
                    Valore = ifcElemGlobalId,
                    Condition = ValoreConditionEnum.Equal,
                });

                attFilterIfc.Groups.Add(attFilterAnd);

            }

            var elemsSum = await _entHelper.GetEntitiesSummary(BuiltInCodes.EntityType.Elementi, null, attFilterIfc);

            List<Guid>? computoItemsId2 = await _entHelper.GetEntitiesReference(BuiltInCodes.EntityType.Elementi, BuiltInCodes.EntityType.Computo, elemsSum.EntitiesId);


        }




    }




}
