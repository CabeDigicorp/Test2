using Commons;
using MasterDetailModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _3DModelExchange;
using System.Globalization;
using System.Diagnostics;
using Commons;
using System.IO;
using ProtoBuf;
using CommonResources;
using DevExpress.Xpf.Core.Native;
using Model.Calculators;


namespace Model
{


    public class ProjectService : ProjectServiceBase, IDataService
    {

        AttributoFormatHelper AttributoFormatHelper { get; set; }
        //EntitiesHelper EntitiesHelper { get; set; }

        public bool IsReadOnly => throw new NotImplementedException();

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        //protected StampeProjectService StampeProjectService { get; set; }

        public void Init(Project project/*, IRepository rep*/)
        {
            ServerFactory = new Factory(this);

            

            _epCalculatorFunction = new EPCalculatorFunction(this);
            _cmpCalculatorFunction = new CmpCalculatorFunction(this);
            _elmCalculatorFunction = new ElmCalculatorFunction(this);
            _ifcCalculatorFunction = new ValoreM3dCalculatorFunction(this, Model3dCalculatorFunction.Names.Ifc);
            _rvtCalculatorFunction = new ValoreM3dCalculatorFunction(this, Model3dCalculatorFunction.Names.Rvt);
            _cntCalculatorFunction = new CntCalculatorFunction(this);
            _infCalculatorFunction = new InfCalculatorFunction(this);
            _divCalculatorFunction = new DivCalculatorFunction(this);
            _eatCalculatorFunction = new EAtCalculatorFunction(this);
            _wbsCalculatorFunction = new WBSCalculatorFunction(this);
            _varCalculatorFunction = new VarCalculatorFunction(this);
            _capCalculatorFunction = new CapCalculatorFunction(this);

            Calculator = new ValoreCalculator(this);
            Calculator.ElmCalculatorFunction = _elmCalculatorFunction;
            Calculator.CmpCalculatorFunction = _cmpCalculatorFunction;
            Calculator.EPCalculatorFunction = _epCalculatorFunction;
            Calculator.IfcCalculatorFunction = _ifcCalculatorFunction;
            Calculator.RvtCalculatorFunction = _rvtCalculatorFunction;
            Calculator.CntCalculatorFunction = _cntCalculatorFunction;
            Calculator.InfCalculatorFunction = _infCalculatorFunction;
            Calculator.DivCalculatorFunction = _divCalculatorFunction;
            Calculator.EAtCalculatorFunction = _eatCalculatorFunction;
            Calculator.WBSCalculatorFunction = _wbsCalculatorFunction;

            InitProject(project);
            
            Model3dValuesData model3dValuesData = GetModel3dValuesData();
            _ifcCalculatorFunction.SetValues(model3dValuesData.Values.Where(item => item.Model3dType == Model3dType.Ifc));
            _rvtCalculatorFunction.SetValues(model3dValuesData.Values.Where(item => item.Model3dType == Model3dType.Revit));

            AttributoFormatHelper = new AttributoFormatHelper(this);
            EntitiesHelper = new EntitiesHelper(this);

            UpdateEntitiesIndexes();

        }

        public void SetIfcService(I3DModelService model3dService)
        {
            _ifcCalculatorFunction.Model3dService = model3dService;
            _rvtCalculatorFunction.Model3dService = null;
        }
        public void SetRvtService(I3DModelService model3dService)
        {
            _rvtCalculatorFunction.Model3dService = model3dService;
            _ifcCalculatorFunction.Model3dService = null;
        }


        #region IDataService

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityTypeNew"></param>
        /// <param name="removeInvalidRiferimenti">Rimuove i riferimenti non più validi in altri entityType</param>
        /// <returns></returns>
        public bool SetEntityType(EntityType entityTypeNew, bool removeInvalidRiferimenti, bool forceUpdateEntities = false)
        {
            try
            {

                if (SetEntityTypeInternal(entityTypeNew, forceUpdateEntities))
                {
                    if (removeInvalidRiferimenti)
                    {
                        //aggiorno anche le altre EntityType
                        //List<string> entityTypesKey = EntityTypes.Keys.ToList();
                        List<string> entityTypesKey = EntitiesHelper.GetDependentEntityTypesKey(entityTypeNew.GetKey());

                        foreach (string entityTypeKey in entityTypesKey)
                        {
                            EntityType entType = EntityTypes[entityTypeKey];

                            bool anyRemoved = RemoveInvalidRiferimenti(entType);
                            
                            if (anyRemoved)
                                SetEntityTypeInternal(entType, true);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
            return true;
        }

        /// <summary>
        /// Ritorna le entità passando gli Id
        /// </summary>
        /// <param name="entTypeCode"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<TreeEntity> GetTreeEntitiesById(string entTypeCode, IEnumerable<Guid> ids, bool compactToMoveFormat)
        {

            TreeEntityCollection collection = new TreeEntityCollection();
            collection.TreeEntities = new List<TreeEntity>();
            foreach (Guid id in ids)
            {
                if (EntitiesByGuid[entTypeCode].ContainsKey(id))
                {
                    TreeEntity treeEnt = EntitiesByGuid[entTypeCode][id] as TreeEntity;
                    treeEnt.IsParent = IsTreeEntityParent(treeEnt);
                    collection.TreeEntities.Add(treeEnt);
                }
            }

            if (compactToMoveFormat)
            {
                TreeEntityCollection clone = collection.Clone();
                clone.ResolveAllReferences(Project.EntityTypes);
                //clone.CompactToMove();
                CompactToMove(clone);
                return clone.TreeEntities;
            }

            return collection.TreeEntities;
        }  

        /// <summary>
        /// Applica il filtro, l'ordinamento e la ricerca e ritorna gli id delle entità filtrate e ordinate
        /// </summary>
        /// <param name="entTypeKey"></param>
        /// <param name="filter">Filtro applicato</param>
        /// <param name="sort">Ordine applicato</param>
        /// <param name="valoriUnivociAttributi">Valori univoci per gli attributi nel filtro/ricerca</param>
        /// <param name="entitiesFound">Id di entità trovate (sottoinsieme di entitiesFoundWithPaths)</param>
        /// <param name="entitiesFoundWithAnchestor">Id di entità trovate con i relativi avi (sottoinsieme dei valori filtrati)</param>
        /// <returns>Valori filtrati</returns>
        public List<EntityMasterInfo> GetFilteredEntities(string entTypeKey, FilterData filter, SortData sort, GroupData group, out List<Guid> entitiesFound)
        {
            //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<ProjectService>();

            HashSet<Entity> entitiesFilteredHashSet = new HashSet<Entity>(GetEntities(entTypeKey));
            entitiesFilteredHashSet.RemoveWhere(item => item.Deleted);

            //foreach (Entity ent in entitiesFilteredHashSet)
            //    CalcolaEntityValues(ent);

            HashSet<Entity> entitiesFilteredSortedHashSet = ApplyFilter(entTypeKey, entitiesFilteredHashSet, filter, true);

            ApplySort(ref entitiesFilteredSortedHashSet, sort);

            entitiesFound = ApplySearch(entTypeKey, entitiesFilteredSortedHashSet, filter);

            List<Guid> entitiesReturned = entitiesFilteredSortedHashSet.Select(item => item.EntityId).ToList();

            //if (group != null)
            //{
            //    //Imposta i dati del raggruppamento corrente nel progetto perchè possa essere salvato nel progetto
            //    Project.ViewSettings.EntityTypes[entTypeKey].Groups = group.Items.Select(item => new AttributoGroupData() { CodiceAttributo = item.CodiceAttributo }).ToList();
            //}

            return entitiesFilteredSortedHashSet.Select(item => new EntityMasterInfo()
            {
                Id = item.EntityId,
                ComparerKey = item.GetComparerKey(),

                GroupKeys = group == null ? new List<string>()
                : group.Items.Select(attGr =>
                {
                    string plainText = string.Empty;

                    bool deep = false;
                    Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(group.EntityTypeKey, attGr.CodiceAttributo);
                    if (sourceAtt?.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                        deep = valAttTesto.UseDeepValore;


                    Valore val = null;
                    val = EntitiesHelper.GetValoreAttributo(item, attGr.CodiceAttributo, deep, false);

                    if (val != null)
                        plainText = val.ToPlainText();

                    return plainText;

                }).ToList(),

            }).ToList();
        }
        public List<EntityAttributo> GetAttributiValoriComuni(string entityTypeCode, List<Guid> entitiesId)
        {
            List<EntityAttributo> entityAttributi = new List<EntityAttributo>();
            List<Entity> entities = GetEntities(entityTypeCode).Where(item => entitiesId.Contains(item.EntityId)).ToList();

            var entAtts = EntitiesHelper.GetAttributi(entities[0]);
            //int attributiCount = entities[0].Attributi.Count;

            //foreach (string codiceAtt in entities[0].Attributi.Keys)
            foreach (string codiceAtt in entAtts.Keys)
            {
                //EntityAttributo attValoriComuniClone = entities[0].Attributi[codiceAtt].Clone();
                EntityAttributo attValoriComuniClone = new EntityAttributo(entities[0], entAtts[codiceAtt]);

                bool allNull = true;

                Valore val = GetValoreAttributo(entities[0], codiceAtt);
                if (val != null)
                {
                    attValoriComuniClone.Valore = val.Clone();
                    allNull = false;
                }
                else
                {
                    attValoriComuniClone.Valore = new ValoreTesto();
                }

                foreach (Entity ent in entities)
                {
                    val = GetValoreAttributo(ent, codiceAtt);
                    if (val != null)
                        allNull = false;

                    attValoriComuniClone.Valore.Intersect(val);
                }

                if (allNull)
                    attValoriComuniClone.Valore = new ValoreTesto();

                entityAttributi.Add(attValoriComuniClone);
            }

            return entityAttributi;
        }
        public ModelActionResponse CommitAction(ModelAction action)
        {
            //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<ProjectService>();
            //log.Trace("CommitAction: ActionName:{0} - AttributoCode:{1} - EntitiesCount:{2}", action.ActionName.ToString(), action.AttributoCode, action.EntitiesId.Count);


            HashSet<Guid> entitiesChangedId = new HashSet<Guid>();
            //Impostazione dei valori
            ModelActionResponse mar = CommitActionInternal(action, entitiesChangedId);

            HashSet<Guid> entitiesChangedIdByCalc = new HashSet<Guid>();

            Calculator.ResetExpressions();

            EntitiesError error = new EntitiesError();

            EntityCalcOptions options = new EntityCalcOptions(){ CalcolaAttributiResults = true, ResetCalulatedValues = false };

            if (action.ActionName == ActionName.MULTI_AND_CALCOLA)
            {
                //EntityCalcOptions options = new EntityCalcOptions(true, true);
                //entitiesChangedIdByCalc = CalcolaEntities(action.EntityTypeKey, options, entitiesChangedId.ToList(), error);
                options.ResetCalulatedValues = true;

                EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);
                entityAttributiUpdater.CalcolaEntityValuesAndDependentEntityTypes(action.EntityTypeKey, entitiesChangedId, options, error);
            }
            else if (action.ActionName == ActionName.MULTI_NODEPENDENTS)
            {
                EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);
                entityAttributiUpdater.CalcolaEntitiesValues(action.EntityTypeKey, entitiesChangedId, options, error);
            }
            else
            {
                //Ricalcolo iterando sulle entità modificate
                switch (action.ActionName)
                {

                    case ActionName.ATTRIBUTO_VALORE_MODIFY:
                    case ActionName.TREEENTITY_ADD_PARENT:
                    case ActionName.TREEENTITY_ADD_CHILD:
                    case ActionName.TREEENTITY_INSERT:
                    case ActionName.TREEENTITY_ADD:
                    case ActionName.ENTITY_INSERT:
                    case ActionName.ENTITY_ADD:
                    case ActionName.MULTI:
                    case ActionName.ENTITY_DELETE:
                    case ActionName.TREEENTITY_DELETE:
                    case ActionName.ENTITY_MOVE:
                    case ActionName.TREEENTITY_MOVE:
                    case ActionName.TREEENTITY_MOVE_CHILD_OF:
                    case ActionName.ENTITIES_PASTE:
                    case ActionName.TREEENTITIES_PASTE:
                    case ActionName.ATTRIBUTO_VALORECOLLECTION_REMOVE:
                    case ActionName.ATTRIBUTO_VALORECOLLECTION_REPLACE:
                        {
                            switch (action.ActionName)
                            {
                                case ActionName.TREEENTITY_ADD_PARENT:
                                case ActionName.TREEENTITY_ADD_CHILD:
                                case ActionName.TREEENTITY_INSERT:
                                case ActionName.TREEENTITY_ADD:
                                case ActionName.ENTITY_INSERT:
                                case ActionName.ENTITY_ADD:
                                    entitiesChangedId.Add(mar.NewId);
                                    break;

                            }

                            EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);

                            switch (action.ActionName)
                            {
                                case ActionName.ENTITY_DELETE:
                                case ActionName.TREEENTITY_DELETE:
                                    entitiesChangedId.UnionWith(action.EntitiesId);
                                    break;

                            }

                            entityAttributiUpdater.CalcolaEntityValuesAndDependentEntityTypes(action.EntityTypeKey, entitiesChangedId, options, error);

                            if (entitiesChangedIdByCalc != null)
                                entitiesChangedIdByCalc.UnionWith(entityAttributiUpdater.GetEntitiesChanged());


                            break;
                        }

                    case ActionName.ATTRIBUTO_VALORECOLLECTION_ADD:
                        {
                            break;
                        }

                    case ActionName.ATTRIBUTO_VALORE_REPLACEINTEXT:
                        {

                            break;
                        }

                }
            }

            //Calculator.ResetExpressions();


            mar.ChangedEntitiesId = new HashSet<Guid> (entitiesChangedId.Union(entitiesChangedIdByCalc));
            if (error.ActionErrorType != ActionErrorType.NOTHING)
            {
                mar.ActionResponse = ActionResponse.OK;
                mar.EntitiesError = error;
            }

            UpdateEntitiesErrors(error);

            return mar;
        }
        //public Dictionary<string, HashSet<SuggestionPackage>> GetSuggestions()
        //{
        //    return SuggestionsDictionary;

        //}
        public async Task<List<string>> GetValoriUnivociAsync(string entityTypeKey, List<Guid> entitiesId, string codiceAttributo, int takeResults, string textSearched)
        {
            return await Task.Run(() =>
            {
                return GetValoriUnivoci(entityTypeKey, entitiesId, codiceAttributo, takeResults, textSearched);
            });
        }
        /// <summary>
        /// Applica il filtro, l'ordinamento e la ricerca e ritorna gli id delle entità filtrate e ordinate
        /// </summary>
        /// <param name="entTypeCode"></param>
        /// <param name="filter">Filtro applicato</param>
        /// <param name="sort">Ordine applicato</param>
        /// <param name="valoriUnivociAttributi">Valori univoci per gli attributi nel filtro/ricerca</param>
        /// <param name="entitiesFound">Id di entità trovate</param>
        /// <returns>Valori filtrati</returns>
        public List<TreeEntityMasterInfo> GetFilteredTreeEntities(string entTypeCode, FilterData filter, SortData sort, out List<Guid> entitiesFound)
        {
            

            HashSet<Entity> entitiesFilteredSortedHashSet = ApplyTreeFilter(entTypeCode, filter);

            ApplyTreeSort(ref entitiesFilteredSortedHashSet, sort);

            entitiesFound = ApplySearch(entTypeCode, entitiesFilteredSortedHashSet, filter);


            List<TreeEntityMasterInfo> entitiesReturned = CreateTreeEntityMasterInfo(entitiesFilteredSortedHashSet);

            return entitiesReturned;
        }

        private static List<TreeEntityMasterInfo> CreateTreeEntityMasterInfo(HashSet<Entity> entitiesFilteredSortedHashSet)
        {
            //List<TreeEntityMasterInfo> entitiesReturned = entitiesFilteredSortedHashSet.Select(item => new TreeEntityMasterInfo() { Id = item.Id, /*Depth = (item as TreeEntity).Depth,*/ ParentId = (item as TreeEntity).Parent != null ? (item as TreeEntity).Parent.Id : Guid.Empty }).ToList();


            List<TreeEntityMasterInfo> entitiesReturned = new List<TreeEntityMasterInfo>();
            Guid entId = Guid.Empty;
            Stack<Guid> parentsId = new Stack<Guid>();
            int depth = 0;
            foreach (TreeEntity treeEnt in entitiesFilteredSortedHashSet)
            {
                if (treeEnt.Depth > depth)
                    parentsId.Push(entId);

                for (int d= depth; d> treeEnt.Depth; d--)
                    parentsId.Pop();

                TreeEntityMasterInfo treeEntityMasterInfo = new TreeEntityMasterInfo();
                treeEntityMasterInfo.Id = treeEnt.EntityId;
                if (parentsId.Any())
                    treeEntityMasterInfo.ParentId = parentsId.Peek();

                treeEntityMasterInfo.ComparerKey = treeEnt.GetComparerKey();
                entitiesReturned.Add(treeEntityMasterInfo);

                depth = treeEnt.Depth;
                entId = treeEnt.EntityId;
            }
            return entitiesReturned;
        }

        public Entity NewEntity(string codice)
        {
            return ServerFactory.NewEntity(codice);
        }
        //public Dictionary<string, Guid> CreateKey(string entityTypeKey, string separator, List<string> attributiCodice)
        //{
        //    List<Entity> ents = GetEntities(entityTypeKey);

        //    Dictionary<string, Guid> keys = new Dictionary<string, Guid>();
        //    ents.ForEach(item =>
        //    {
        //        IEnumerable<string> attsVal = attributiCodice.Select(attCode => item.Attributi[attCode].Valore.ToPlainText());
        //        string key = string.Join(separator, attsVal);
        //        if (!keys.ContainsKey(key))
        //            keys.Add(key, item.Id);
        //    });

        //    return keys;
        //}

        public ViewSettings GetViewSettings()
        {
            return Project.ViewSettings;
        }

        public void SetViewSettings(ViewSettings viewSettings)
        {
            Project.ViewSettings = viewSettings;
        }

        //public bool GetGroupRecordsData(GroupData groupData)
        //{
        //    return GetGroupRecordsData2(groupData);
        //}



        public bool GetGroupRecordsData(GroupData groupData)
        {
            List<string> groupKeys = new List<string>(groupData.GroupRecords.Select(item => item.Key));
            string entityTypeCode = groupData.EntityTypeKey;

            string sumSymbol = "\u2211";//Simbolo di sommatoria

            foreach (string groupKey in groupKeys)
            {
                if (groupData.GroupRecords[groupKey] == null)
                    continue;

                GroupRecordData grd = groupData.GroupRecords[groupKey];

                string[] keys = groupData.SplitGroupKey(groupKey);

                IEnumerable<Entity> ents = GetEntitiesById(groupData.EntityTypeKey, groupData.GroupRecords[groupKey].ChildsId);

                EntityType entType = EntityTypes[entityTypeCode];

                //Evidenziatori e resolve reference
                string highlighterColorName = null;

                foreach (Entity ent in ents)
                {
                    ent.ResolveReferences(EntityTypes);
                    
                    if (highlighterColorName == null)
                        highlighterColorName = ent.HighlighterColorName;
                    else if (highlighterColorName != ent.HighlighterColorName)
                        highlighterColorName = MyColorsEnum.Transparent.ToString();
                    else
                        highlighterColorName = ent.HighlighterColorName;
                }

                grd.HighlighterColorName = highlighterColorName;


                ////////////////////////////////////////////////////////////////////////////////////////////
                /// gestone Attributi -> entities
                foreach (Attributo att in entType.Attributi.Values)
                {
                    string codiceAttributo = att.Codice;

                    if (!grd.Attributi.ContainsKey(codiceAttributo))
                        continue;

                    Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(entType.Attributi[codiceAttributo]);
                    if (sourceAtt == null)
                        continue;

                    Valore valDefault = sourceAtt.ValoreDefault;

                    ValoreOperationType groupOperation = entType.Attributi[codiceAttributo].GroupOperation;

                    if (codiceAttributo == BuiltInCodes.Attributo.QuantitaTotale)
                    {
                        ValoreReale valRealeDefault = valDefault as ValoreReale;

                        double somma = 0;
                        string formatSomma = null;
                        bool SommaErr = false;
                        foreach (Entity ent in ents)
                        {

                            Valore val = EntitiesHelper.GetValoreAttributo(ent, codiceAttributo, false, false);

                            string format = AttributoFormatHelper.GetValorePaddedFormat(ent, codiceAttributo);
                            if (formatSomma == null)
                                formatSomma = format;
                            else if (formatSomma != format)
                                formatSomma = string.Empty;

                            if (val is ValoreReale)
                            {
                                ValoreReale valReale = val as ValoreReale;
                                if (valReale.RealResult != null && !double.IsNaN(valReale.RealResult.Value))
                                    somma += (double)valReale.RealResult;
                                else
                                    SommaErr = true;
                            }

                        }

                        //Sommo i valori solo se i formati sono tutti uguali
                        if (!SommaErr && formatSomma != null && formatSomma.Any())
                        {
                            string fullFormat = string.Format("{0}= {1}", sumSymbol, formatSomma);
                            string strSomma = string.Format(fullFormat, somma);

                            if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                            {
                                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                                groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, strSomma);
                            }
                            else
                                groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = strSomma;
                        }

                    }
                    else if (groupOperation == ValoreOperationType.Sum && valDefault is ValoreReale) //sommo
                    {
                        //ValoreReale valRealeDefault = entType.Attributi[codiceAttributo].ValoreDefault as ValoreReale;
                        ValoreReale valRealeDefault = valDefault as ValoreReale;

                        double somma = 0;
                        bool SommaErr = false;
                        foreach (Entity ent in ents)
                        {
                            Valore val = EntitiesHelper.GetValoreAttributo(ent, codiceAttributo, false, false);

                            if (val is ValoreReale)
                            {
                                ValoreReale valReale = val as ValoreReale;
                                if (valReale.RealResult != null)
                                    somma += (double)valReale.RealResult;
                                else
                                    SommaErr = true;
                            }

                        }

                        if (!SommaErr)
                        {
                            string format = AttributoFormatHelper.GetValorePaddedFormat(entType.Attributi[codiceAttributo]);
                            string fullFormat = string.Format("{0}= {1}", sumSymbol, format);

                            string strSomma = string.Format(fullFormat, somma);

                            if (groupData.GroupRecords.ContainsKey(groupKey))
                            {

                                if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                                {
                                    MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                                    groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, strSomma);
                                }
                                else
                                    groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = strSomma;
                            }
                        }
                        else
                        {

                            string format = AttributoFormatHelper.GetValorePaddedFormat(entType.Attributi[codiceAttributo]);
                            string fullFormat = string.Format("*{0}= {1}", sumSymbol, format);
                            string strSomma = string.Format(fullFormat, somma);

                            if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                            {
                                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                                groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, strSomma);
                            }
                            else
                                groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = strSomma;

                        }

                    }
                    else if (groupOperation == ValoreOperationType.Sum && valDefault is ValoreContabilita)//sommo
                    {
                        ValoreContabilita valContDefault = valDefault as ValoreContabilita;

                        decimal somma = 0;
                        bool SommaErr = false;
                        foreach (Entity ent in ents)
                        {
                            Valore val = EntitiesHelper.GetValoreAttributo(ent, codiceAttributo, false, false);

                            if (val is ValoreContabilita)
                            {
                                ValoreContabilita valCont = val as ValoreContabilita;
                                if (valCont.RealResult != null)
                                    somma += (decimal)valCont.RealResult;
                                else
                                    SommaErr = true;
                            }

                        }

                        if (!SommaErr)
                        {
                            string format = AttributoFormatHelper.GetValorePaddedFormat(entType.Attributi[codiceAttributo]);
                            string fullFormat = string.Format("{0}= {1}", sumSymbol, format);

                            string strSomma = string.Format(fullFormat, somma);

                            if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                            {
                                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                                groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, strSomma);
                            }
                            else
                                groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = strSomma;
                        }
                        else
                        {

                            string format = AttributoFormatHelper.GetValorePaddedFormat(entType.Attributi[codiceAttributo]);
                            string fullFormat = string.Format("*{0}= {1}", sumSymbol, format);
                            string strSomma = string.Format(fullFormat, somma);

                            if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                            {
                                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                                groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, strSomma);
                            }
                            else
                                groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = strSomma;

                        }
                    }
                    else if (groupOperation == ValoreOperationType.Equivalent)//riporto il valore se sono tutti uguali
                    {

                        Valore valComune = null;
                        bool equals = true;
                        string res = string.Empty;


                        foreach (Entity ent in ents)
                        {
                            Valore val = null;

                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                                val = new ValoreTesto() { V = EntitiesHelper.GetValorePlainText(ent, codiceAttributo, true, true) };
                            else
                            {
                                bool deep = false;
                                if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                                    deep = valAttTesto.UseDeepValore;

                                val = EntitiesHelper.GetValoreAttributo(ent, codiceAttributo, deep, false);
                            }

                            if (valComune == null)
                            {
                                valComune = val;
                                continue;
                            }
                            else if (!valComune.ResultEquals(val))
                            {
                                equals = false;
                                break;
                            }
                        }

                        if (equals && valComune != null)
                        {
                            res = valComune.ToPlainText();
                        }


                        if (res != null && res.Any())
                        {

                            if (valComune is ValoreReale)
                            {
                                ValoreReale valReale = valComune as ValoreReale;
                                string format = AttributoFormatHelper.GetValorePaddedFormat(entType.Attributi[codiceAttributo]);
                                res = string.Format(format, valReale.RealResult);
                            }
                            if (valComune is ValoreContabilita)
                            {
                                ValoreContabilita valCont = valComune as ValoreContabilita;
                                string format = AttributoFormatHelper.GetValorePaddedFormat(entType.Attributi[codiceAttributo]);
                                res = string.Format(format, valCont.RealResult);
                            }
                        }

                        //se l'attributo che si sta calcolando è uno di quelli per cui si sta raggruppando ed è già stato raggruppato nel padre non calcolo e non lo visualizzo nella griglia
                        AttributoGroupData attGroupData = groupData.Items.FirstOrDefault(item => item.CodiceAttributo == codiceAttributo);
                        if (attGroupData == null || groupData.Items.IndexOf(attGroupData) > keys.Count() - 2)
                        {
                            if (groupData.GroupRecords.ContainsKey(groupKey))
                            {

                                if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                                {
                                    MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                                    groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, res);
                                }
                                else
                                    groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = res;
                            }
                        }
                    }
                    else //riporto nella voce raggruppata solo il valore raggruppato (Nothing)
                    {
                        string res = "";


                        AttributoGroupData groupAtt = groupData.Items.FirstOrDefault(item => item.CodiceAttributo == codiceAttributo);
                        if (groupAtt != null && groupKey != "TableSummary")
                        {
                            int groupAttIndex = groupData.Items.IndexOf(groupAtt);

                            if (groupAttIndex == keys.Count() - 1)
                            {
                                res = keys.Last();
                            }
                        }
                        if (!groupData.GroupRecords[groupKey].Attributi.ContainsKey(codiceAttributo))
                        {
                            MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("{0} - {1}", groupKey, codiceAttributo));
                            groupData.GroupRecords[groupKey].Attributi.Add(codiceAttributo, res);
                        }
                        else
                            groupData.GroupRecords[groupKey].Attributi[codiceAttributo] = res;

                    }
                }




            }
            return true;
        }

        public bool SetModel3dUserViews(Model3dUserViewList userViews)
        {
            Project.Model3dUserViews = userViews;
            return true;
        }

        public Model3dUserViewList GetModel3dUserViews()
        {
            return Project.Model3dUserViews;
        }

        public bool RemoveDivisione(Guid id)
        {

            //Rimuovo dalla lista dei tipi
            string[] keys = Project.EntityTypes.Where(item => item.Key.Contains(id.ToString())).Select(item => item.Key).ToArray();

            bool allowRemove = true;

            if (keys.Count() != 2)
                return false;

            EntityType divItemTypeClone = null;

            if (Project.EntityTypes[keys[0]] is DivisioneItemType)
            {

                DivisioneItemType divItemType = Project.EntityTypes[keys[0]] as DivisioneItemType;
                if (divItemType.IsBuiltIn)
                    allowRemove = false;

                divItemTypeClone = divItemType.Clone();
            }
            else if (Project.EntityTypes[keys[0]] is DivisioneItemParentType)
            {
                DivisioneItemParentType divItemParentType = Project.EntityTypes[keys[0]] as DivisioneItemParentType;
                DivisioneItemType divItemType = Project.EntityTypes[keys[1]] as DivisioneItemType;

                if (divItemType.IsBuiltIn)
                    allowRemove = false;

                divItemTypeClone = divItemType.Clone();
            }

            

            if (!allowRemove)
                return false;

            var entTypesOld = GetEntityTypes();

            //Rimuovo dalla lista dei tipi
            foreach (string key in keys)
            {
                Project.EntityTypes.Remove(key);
            }

            //Rimuovo dalle divisioni
            if (Project.DivisioniItems.ContainsKey(id))
                Project.DivisioniItems.Remove(id);

            //Rimuovo tutti gli attributi di tipo Guid e riferimento che fanno riferimento a questa divisione
            List<string> entityTypesKey = Project.EntityTypes.Keys.ToList();
            foreach (string entTypeKey in entityTypesKey)
            {
                EntityType entTypeClone = Project.EntityTypes[entTypeKey].Clone();

                bool anyRemoved = RemoveInvalidRiferimenti(entTypeClone);
                anyRemoved = anyRemoved? anyRemoved : entTypeClone.UpdateAttributi(GetEntityTypes(), divItemTypeClone, true);
                if (anyRemoved)
                    SetEntityTypeInternal(entTypeClone, true);
            }

            return true;
        }

        public bool RenameDivisione(Guid id, string newName, string codice = null)
        {
            if (!Project.DivisioniItems.ContainsKey(id))
                return false;

            DivisioneItemType divTypeOld = null;
            string[] keys = Project.EntityTypes.Where(item => item.Key.Contains(id.ToString())).Select(item => item.Key).ToArray();
            foreach (string key in keys)
            {
                EntityType entType = Project.EntityTypes[key];
                if (entType is DivisioneItemType)
                    divTypeOld = entType.Clone() as DivisioneItemType;

                entType.Name = newName;
                if (codice != null)
                    entType.Codice = codice;
            }

            //Aggiorno gli attributi delle altre sezioni (per esempio sezione Report, attributo Sezione)
            List<string> entTypesKeyToUpdate = EntitiesHelper.GetDependentEntityTypesKey(divTypeOld.GetKey());
            foreach (string entTypeKey in entTypesKeyToUpdate)
            {
                EntityType entType = Project.EntityTypes[entTypeKey];
                entType.UpdateAttributi(GetEntityTypes(), divTypeOld, false);
            }

            return true;
        }

        public bool SortDivisioni(Dictionary<Guid, int> divTypesIdPos)
        {
            var divTypes = GetEntityTypes().Values.Where(item => item is DivisioneItemType);
            foreach (DivisioneItemType divType in divTypes)
            {
                if (divTypesIdPos.ContainsKey(divType.DivisioneId))
                    divType.Position = divTypesIdPos[divType.DivisioneId];
            }

            return true;
        }

        public Model3dFiltersData GetModel3dFiltersData()
        {
            return Project.Model3dFiltersData;
        }

        public bool SetModel3dFiltersData(Model3dFiltersData filtersData)
        {
            Project.Model3dFiltersData = filtersData;
            return true;
        }

        public Model3dTagsData GetModel3dTagsData()
        {
            return Project.Model3dTagsData;
        }

        public bool SetModel3dTagsData(Model3dTagsData tagsData)
        {
            Project.Model3dTagsData = tagsData;
            return true;
        }

        public Model3dPreferencesData GetModel3dPreferencesData()
        {
            return Project.Model3dPreferencesData;
        }

        public bool SetModel3dPreferencesData(Model3dPreferencesData preferencesData)
        {
            Project.Model3dPreferencesData = preferencesData;
            return true;
        }

        public Model3dFilesInfo GetModel3dFilesInfo()
        {
            return Project.Model3dFilesInfo;
        }

        public bool SetModel3dFilesInfo(Model3dFilesInfo filesInfo)
        {
            Project.Model3dFilesInfo = filesInfo;
            return true;
        }

        public Model3dValuesData GetModel3dValuesData()
        {
            return Project.Model3dValuesData;
        }

        public bool SetModel3dValuesData(Model3dValuesData model3dValuesData)
        {
            Project.Model3dValuesData = model3dValuesData;
            return true;
        }

        public bool PrepareBeforeSave()
        {
            if (_ifcCalculatorFunction != null)
            {
                //Model3dValuesData model3dValuesData = _ifcCalculatorFunction.GetValues();
                //SetModel3dValuesData(model3dValuesData);
                UpdateModel3dValues(Model3dType.Ifc);
            }

            if (_rvtCalculatorFunction != null)
            {
                //Model3dValuesData model3dValuesData = _rvtCalculatorFunction.GetValues();
                //SetModel3dValuesData(model3dValuesData);
                UpdateModel3dValues(Model3dType.Revit);
            }

            return true;
        }

        public bool ClearValuesFromModel3d(Model3dType model3dType)
        {

            if (model3dType == Model3dType.Unknown)
            {
                _ifcCalculatorFunction.Clear();
                _rvtCalculatorFunction.Clear();

                Model3dValuesData model3dValuesData = new Model3dValuesData();
                SetModel3dValuesData(model3dValuesData);
            }
            else if (_ifcCalculatorFunction != null && model3dType == Model3dType.Ifc)
            {
                //elimina i valori da calculatorfuntion lato server
                _ifcCalculatorFunction.Clear();
                
                //elimina i valori da project
                Model3dValuesData model3dValuesData = GetModel3dValuesData();
                model3dValuesData.Values.RemoveAll(item => item.Model3dType == Model3dType.Ifc);
                SetModel3dValuesData(model3dValuesData);
                

            }
            else if (_rvtCalculatorFunction != null && model3dType == Model3dType.Revit)
            {
                //elimina i valori da calculatorfuntion lato server
                _rvtCalculatorFunction.Clear();

                //elimina i valori da project
                Model3dValuesData model3dValuesData = GetModel3dValuesData();
                model3dValuesData.Values.RemoveAll(item => item.Model3dType == Model3dType.Revit);
                SetModel3dValuesData(model3dValuesData);

            }

            return true;
        }
        
        public bool UpdateValuesFromModel3d(Model3dType model3dType, bool alsoByService)
        {
            if (model3dType == Model3dType.Unknown)
            {
                if (alsoByService)
                {
                    //aggiorno i valori del calcolatore
                    _ifcCalculatorFunction.UpdateValuesFromModel3d();
                    _rvtCalculatorFunction.UpdateValuesFromModel3d();
                }
                UpdateModel3dValues(Model3dType.Unknown);

            }
            else if (_ifcCalculatorFunction != null && model3dType == Model3dType.Ifc)
            {
                if (alsoByService)
                {
                    //aggiorno i valori del calcolatore
                    _ifcCalculatorFunction.UpdateValuesFromModel3d();
                }

                //aggiorno i valori nel Project
                //Model3dValuesData ifcValuesData = _ifcCalculatorFunction.GetValues();
                //SetModel3dValuesData(ifcValuesData);
                UpdateModel3dValues(Model3dType.Ifc);

            }
            else if (_rvtCalculatorFunction != null && model3dType == Model3dType.Revit)
            {
                if (alsoByService)
                {
                    //aggiorno i valori del calcolatore
                    _rvtCalculatorFunction.UpdateValuesFromModel3d();
                }

                //aggiorno i valori nel Project
                UpdateModel3dValues(Model3dType.Revit);
            }

            return true;
        }

        private void UpdateModel3dValues(Model3dType model3dType)
        {
            Model3dValuesData model3dValuesData = GetModel3dValuesData();

            IEnumerable<Model3dValueData> newValues = null;
            if (model3dType == Model3dType.Ifc)
            {
                newValues = _ifcCalculatorFunction.GetValues();
                model3dValuesData.Values.RemoveAll(item => (item.Model3dType == Model3dType.Ifc));
                model3dValuesData.Values.AddRange(newValues);
            }
            else if (model3dType == Model3dType.Revit)
            {
                newValues = _rvtCalculatorFunction.GetValues();
                model3dValuesData.Values.RemoveAll(item => (item.Model3dType == Model3dType.Revit));
                model3dValuesData.Values.AddRange(newValues);
            }
            else if (model3dType == Model3dType.Unknown)
            {
                model3dValuesData.Values.Clear();

                newValues = _ifcCalculatorFunction.GetValues();
                model3dValuesData.Values.AddRange(newValues);

                newValues = _rvtCalculatorFunction.GetValues();
                model3dValuesData.Values.AddRange(newValues);
            }

            SetModel3dValuesData(model3dValuesData);
        }

        public bool CalculateModel3dValues(Model3dValues model3dValues)
        {
            

            //if (_ifcCalculatorFunction == null)
            //    return false;

            foreach (Model3dValue m3dVal in model3dValues.Values)
            {
                string value = null;

                if (m3dVal.Model3DType == Model3dType.Ifc)
                    _ifcCalculatorFunction?.Calculate(m3dVal.ProjectGlobalId, m3dVal.GlobalId, m3dVal.ClassName, m3dVal.ItemPath, m3dVal.ValuePath, out value);
                else if (m3dVal.Model3DType == Model3dType.Revit)
                    _rvtCalculatorFunction?.Calculate(m3dVal.ProjectGlobalId, m3dVal.GlobalId, m3dVal.ClassName, m3dVal.ItemPath, m3dVal.ValuePath, out value);
                else
                {
                    //error
                }


                m3dVal.Value = value;
            }
            return true;
        }

        Dictionary<string, DefinizioneAttributo> IDataService.GetDefinizioniAttributo()
        {
            return GetDefinizioniAttributo();
        }

        Dictionary<string, EntityType> IDataService.GetEntityTypes()
        {
            return GetEntityTypes();
        }

        EntityType IDataService.GetEntityType(string entTypeKey)
        {
            Dictionary<string, EntityType> entTypes = GetEntityTypes();
            if (entTypes.ContainsKey(entTypeKey))
                return entTypes[entTypeKey];

            return null;
        }

        IEnumerable<TreeEntity> IDataService.GetTreeEntitiesDeepById(string entTypeCode, IEnumerable<Guid> ids)
        {
            return GetTreeEntitiesDeepById(entTypeCode, ids);
        }

        Guid IDataService.AddDivisione(string codice, string name, Model3dClassEnum model3dClassName)
        {
            return AddDivisione(codice, name, model3dClassName);
        }

        public HashSet<Guid> ApplyFilter(string entTypeCode, HashSet<Guid> entitiesToFilter, FilterData filter)
        {
            List<Entity> ents = GetEntitiesById(entTypeCode, entitiesToFilter);
            HashSet<Entity> filteredEnts = ApplyFilter(entTypeCode, new HashSet<Entity>(ents), filter, false);

            return new HashSet<Guid>(filteredEnts.Select(item => item.EntityId));
        }

        //IDataStampeService IDataService.GetStampeService()
        //{
        //    return StampeProjectService;
        //}

#endregion

#region Import entities
        public bool ImportEntities(EntitiesImportStatus entitiesImportStatus)
        {
            EntitiesImport entitiesImport = new EntitiesImport();
            entitiesImport.Target = this;

            //entitiesImport.Load(entitiesImportStatus);
            entitiesImport.Run(entitiesImportStatus);

            return true;
        }
#endregion

        public List<NumericFormat> GetNumericFormats()
        {
            return Project.NumericFormats;
        }

        public void SetNumericFormats(List<NumericFormat> numericFormats)
        {
            Project.NumericFormats = numericFormats;
        }

#region Undo/Redo

        /// <summary>
        /// Viene lanciata ad ogni operazione che impatta su Project e ritorna la patch per passare da project nuovo a vecchio (undo)
        /// </summary>
        /// <returns></returns>
        public async Task<List<Patch>> GetProjectPatchsAsync(bool reset)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(_projectSerialized))
                    {
                        _projectSerialized = SerializeProject();
                        return null;
                    }
                    else
                    {

                        string projectOld = _projectSerialized;
                        string projectNew = SerializeProject();

                        diff_match_patch dmp = new diff_match_patch();
                        List<Diff> diffs = dmp.diff_main(projectNew, projectOld);

                        List<Patch> patchs = dmp.patch_make(diffs);

                        _projectSerialized = projectNew;
                        return patchs;
                    }
                }
                catch (Exception exc)
                {
                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                    return null;
                }
            });
        }

        public Project ProjectUndo(List<Patch> patchs)
        {
//#if !ALE_DB
            try
            {
                diff_match_patch dmp = new diff_match_patch();

                object[] objs = dmp.patch_apply(patchs, _projectSerialized);

                bool[] results = objs[1] as bool[];
                if (results.Any(item => item == false))
                {
                    //non bene  :-( 
                    return null;
                }

                string stringBase64 = objs[0].ToString();

                _projectSerialized = stringBase64;

                byte[] byteAfter64 = Convert.FromBase64String(stringBase64);
                MemoryStream afterStream = new MemoryStream(byteAfter64);
                //Project prj = Serializer.Deserialize<Project>(afterStream);
                Project prj = ModelSerializer.Deserialize(afterStream, out _);

                InitProject(prj);
                UpdateEntitiesIndexes();

                Project = prj;
                return prj;
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }
//#endif
            return null;
        }

#endregion Undo/Redo

        List<Guid> IDataService.GetDependentIds(string entityTypeKey, Guid id, string dependentIdsEntityTypeKey)
        {

            IEnumerable<Entity> entities = GetEntities(dependentIdsEntityTypeKey);
            IEnumerable<Entity> entsFound = entities.Where(item =>
            {
                foreach (var entAtt in item.Attributi.Values)
                {
                    if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        if (entAtt.Attributo.GuidReferenceEntityTypeKey == entityTypeKey)
                        {
                            ValoreGuid valGuid = entAtt.Valore as ValoreGuid;

                            //se id == Guid.Empty raccolgo tutti gli id di entityTypeKey utilizzati in dependentIdsEntityTypeKey
                            if (id == null && valGuid.V != Guid.Empty)
                                return true;
                            else if (valGuid.V == id)
                                return true;
                        }
                    }
                }
                return false;
            });

            return entsFound.Select(item => item.EntityId).ToList();
        }

        void IDataService.SetModel3dUserRotoTranslation(Model3dUserRotoTranslation rotoTra)
        {
            Project.Model3dUserRotoTranslation = rotoTra;
        }

        Model3dUserRotoTranslation IDataService.GetModel3dUserRotoTranslation()
        {
            return Project.Model3dUserRotoTranslation;
        }

        public GanttData GetGanttData()
        {
            return Project.Gantt;
        }

        public void SetGanttData(GanttData ganttData)
        {
            Project.Gantt = ganttData;
        }

        public async Task<bool> CreateWBSItems(WBSItemsCreationData data)
        {
            WBSEntityAttributiUpdater upd = new WBSEntityAttributiUpdater(this);
            bool ok = await upd.CreateWBSItems(data);
            if (ok)
            {
                Project.WBSItemsCreationData = data;
                return true;
            }
            return false;
        }

        public void OnProgressChanged(ProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public HashSet<Guid> CalcolaEntities(string entityTypeKey, EntityCalcOptions options, List<Guid> entitiesId = null, EntitiesError error = null)
        {
            //aggiornamento manuale tramite comando

            HashSet<Guid> calculated = null;

            OnProgressChanged(new ProgressEventArgs() { ProgressValue = 0, Message = LocalizationProvider.GetString("Ricalcolo in corso...")} );
            EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);
            //calculated = await Task.Run(() =>
            //{
            //entityAttributiUpdater.CalcolaEntities(entityTypeKey, options, entitiesId, error);
            //entityAttributiUpdater.CalcolaEntitiesValues(entityTypeKey, GetEntitiesById(entityTypeKey, entitiesId), error);


            entityAttributiUpdater.CalcolaEntityValuesAndDependentEntityTypes(entityTypeKey, entitiesId, options,error);

            OnProgressChanged(new ProgressEventArgs() { ProgressValue = 100 });

                //return entitiesId.ToHashSet<Guid>();
            //});
            calculated = entityAttributiUpdater.GetEntitiesChanged();

            return calculated;
        }

        public List<EntitiesError> GetEntitiesErrors()
        {
            return EntitiesErrors.Select(item => new EntitiesError()
            {
                ActionErrorType = item.ActionErrorType,
                EntityTypeKey = item.EntityTypeKey,
                Ids = new HashSet<Guid>(item.Ids),
            }).ToList();
        }

        void UpdateEntitiesErrors(EntitiesError entsError)
        {
            EntitiesErrors.RemoveAll(item => item.EntityTypeKey == entsError.EntityTypeKey);
            EntitiesErrors.Add(entsError);
        }

        public FogliDiCalcoloData GetFogliDiCalcoloData()
        {
            return Project.FogliDiCalcolo;
        }

        public void SetFogliDiCalcoloData(FogliDiCalcoloData fogliDiCalcoloData)
        {
            Project.FogliDiCalcolo = fogliDiCalcoloData;
        }

    }
}