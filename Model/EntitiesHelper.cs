using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.RichEdit;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using MasterDetailModel;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class EntitiesHelper
    {
        IDataService _dataService { get; set; }

        public EntitiesHelper(IDataService dataService)
        {
            _dataService = dataService;
        }

        public Entity GetDataServiceEntityById(string entityTypeKey, Guid id)
        {
            IEnumerable<Entity> ents;

            Dictionary<string, EntityType> entTypes = _dataService.GetEntityTypes();
            if (!entTypes.ContainsKey(entityTypeKey))
                return null;
            
            if (entTypes[entityTypeKey].IsTreeMaster)
            {
                ents = _dataService.GetTreeEntitiesDeepById(entityTypeKey, new List<Guid>() { id });
            }
            else
            {
                ents = _dataService.GetEntitiesById(entityTypeKey, new List<Guid>() { id });
            }

            //if (ents.Any() && ents.First() != null)
            //{
            //    Entity ent = ents.First();
            //    return ent;
            //}


            if (ents.Any())
            {
                Entity ent = ents.Last();
                return ent;
            }

            return null;
        }

        public EntityAttributo GetSourceEntityAttributoOld(Entity entity, string codiceAttributo)
        {
            if (entity == null)
                return null;

            Entity ent = entity;

            if (!ent.Attributi.ContainsKey(codiceAttributo))
                return null;

            EntityAttributo entAtt = ent.Attributi[codiceAttributo];
            Attributo att = entAtt.Attributo;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
            {
                List<EntityMasterInfo> varsEntInfo = _dataService.GetFilteredEntities(BuiltInCodes.EntityType.Variabili, null, null, null, out _);
                Entity varsEnt = _dataService.GetEntityById(BuiltInCodes.EntityType.Variabili, varsEntInfo[0].Id);
                if (varsEnt != null)
                {
                    ValoreAttributoVariabili valAttVar = att.ValoreAttributo as ValoreAttributoVariabili;
                    if (valAttVar != null)
                    {
                        if (varsEnt.Attributi.ContainsKey(valAttVar.CodiceAttributo))
                            return varsEnt.Attributi[valAttVar.CodiceAttributo];
                    }
                }
                return null;
            }

            while (att is AttributoRiferimento)//per andare in profondità riferimento di riferimento
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;
                if (IsAttributoRiferimentoGuidCollection(attRif))
                    return null;
                
                ValoreGuid valGuid = ent.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuid;
                if (valGuid == null)
                    return null;

                ent = GetDataServiceEntityById(attRif.ReferenceEntityTypeKey, valGuid.V);

                if (ent == null)
                    return null;
                //if (ent == null)
                //    break;

                if (ent.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                {
                    entAtt = ent.Attributi[attRif.ReferenceCodiceAttributo];
                }
                else
                    return null;

                att = entAtt.Attributo;
            }

            return entAtt;

        }


        public Entity GetSourceEntity(Entity entity, string codiceAttributo, out Attributo sourceEntityAttributo)
        {
            sourceEntityAttributo = null;

            if (entity == null)
                return null;

            Entity ent = entity;

            string entTypeKey = ent.EntityTypeCodice;

            EntityType entType = _dataService.GetEntityType(entTypeKey);
            if (entType == null)
                return null;

            Attributo att = null;

            if (!entType.Attributi.TryGetValue(codiceAttributo, out att))
                return null;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
            {
                List<EntityMasterInfo> varsEntInfo = _dataService.GetFilteredEntities(BuiltInCodes.EntityType.Variabili, null, null, null, out _);
                Entity varsEnt = _dataService.GetEntityById(BuiltInCodes.EntityType.Variabili, varsEntInfo[0].Id);
                if (varsEnt != null)
                {
                    ValoreAttributoVariabili valAttVar = att.ValoreAttributo as ValoreAttributoVariabili;
                    if (valAttVar != null)
                    {
                        if (varsEnt.Attributi.ContainsKey(valAttVar.CodiceAttributo))
                        {
                            EntityType varEntType = _dataService.GetEntityType(BuiltInCodes.EntityType.Variabili);
                            varEntType.Attributi.TryGetValue(valAttVar.CodiceAttributo, out sourceEntityAttributo);
                            return varsEnt;
                        }
                    }
                }
                return null;
            }

            EntityType entTypeRef = entType;

            while (att is AttributoRiferimento)//per andare in profondità riferimento di riferimento
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;
                if (IsAttributoRiferimentoGuidCollection(attRif))
                {
                    //nel caso sia questo tipo di attributo il sorgente restituito sarà AttributoRiferimento altrimenti dovrei ritornare più di un'entità
                    break;
                    //return null;
                }

                if (!ent.Attributi.ContainsKey(attRif.ReferenceCodiceGuid))
                    return null;

                ValoreGuid valGuid = ent.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuid;
                if (valGuid == null)
                    return null;

                ent = GetDataServiceEntityById(attRif.ReferenceEntityTypeKey, valGuid.V);

                if (ent == null)
                    return null;

                entTypeRef = _dataService.GetEntityType(attRif.ReferenceEntityTypeKey);

                if (!entTypeRef.Attributi.TryGetValue(attRif.ReferenceCodiceAttributo, out att))
                    return null;
            }

            sourceEntityAttributo = att;
            //entTypeRef.Attributi.TryGetValue(att.Codice, out sourceEntityAttributo);

            return ent;

        }

        public EntityAttributo GetSourceEntityAttributo(Entity entity, string codiceAttributo)
        {
            Attributo sourceEntityAttributo = null;
            Entity sourceEntity = GetSourceEntity(entity, codiceAttributo, out sourceEntityAttributo);

            if (sourceEntity == null)
                return null;

            EntityAttributo sourceEntAtt = null;
            sourceEntity.Attributi.TryGetValue(sourceEntityAttributo.Codice, out sourceEntAtt);

            return sourceEntAtt;
        }


        public Valore GetValoreAttributo(Entity entity, string codiceAttributo, bool deep, bool brief)
        {
            if (entity == null)
                return null;

            Valore val = null;

            if (codiceAttributo == "QuantitaTotale" && entity.EntityTypeCodice == BuiltInCodes.EntityType.Computo)
            {
                int p = 0;
            }

            Attributo sourceEntityAttributo = null;
            Entity sourceEnt = GetSourceEntity(entity, codiceAttributo, out sourceEntityAttributo);

            if (sourceEnt == null)
                return null;

            if (IsAttributoRiferimentoGuidCollection(sourceEntityAttributo))
            {
                Valore valRes = CalculateAttributoRiferimentoGuidCollectionValore(sourceEnt, sourceEntityAttributo.Codice);
                val = valRes;
            }
            else
            {
                val = sourceEnt.GetValoreAttributo(sourceEntityAttributo.Codice, deep, brief);
            }

            return val;

        }


        //public Valore GetValoreAttributo_old(Entity entity, string codiceAttributo, bool deep, bool brief)
        //{
        //    if (entity == null)
        //        return null;

        //    Valore val = null;

        //    if (codiceAttributo == "116" && entity.EntityTypeCodice == BuiltInCodes.EntityType.Computo)
        //    {
        //        int p = 0;
        //    }

        //    EntityAttributo sourceEntAtt = GetSourceEntityAttributo(entity, codiceAttributo);

        //    if (sourceEntAtt == null)
        //    {
        //        if (IsAttributoRiferimentoGuidCollection(entity.EntityType.GetKey(), codiceAttributo))
        //        {
        //            Valore valRes = CalculateAttributoRiferimentoGuidCollectionValore(entity, codiceAttributo);
        //            val = valRes;
        //        }

        //    }
        //    else
        //    {
        //        Entity sourceEnt = sourceEntAtt.Entity;
        //        Attributo sourceAtt = sourceEntAtt.Attributo;

        //        if (sourceEnt == null)
        //            return null;

        //        if (IsAttributoRiferimentoGuidCollection(sourceAtt))
        //        {
        //            Valore valRes = CalculateAttributoRiferimentoGuidCollectionValore(sourceEnt, sourceEntAtt.AttributoCodice);
        //            val = valRes;
        //        }
        //        else
        //        {
        //            val = sourceEnt.GetValoreAttributo(sourceAtt.Codice, deep, brief);
        //        }

        //    }


        //    //Rem by Ale 18/04/2023
        //    //if (!entity.Attributi.ContainsKey(codiceAttributo))
        //    //    return null;
        //    //


        //    //rem by ale 13/11/2023
        //    //if (IsAttributoRiferimentoGuidCollection(entity.EntityType.GetKey(), codiceAttributo))
        //    //{
        //    //    Valore  valRes = CalculateAttributoRiferimentoGuidCollectionValore(entity, codiceAttributo);
        //    //    val = valRes;
        //    //}
        //    //else
        //    //{

        //    //    EntityAttributo entAtt = GetSourceEntityAttributo(entity, codiceAttributo);
        //    //    if (entAtt == null)
        //    //        return null;


        //    //    Entity ent = entAtt.Entity;
        //    //    Attributo sourceAtt = entAtt.Attributo;

        //    //    if (ent == null)
        //    //        return null;

        //    //    val = ent.GetValoreAttributo(sourceAtt.Codice, deep, brief);

        //    //    if (sourceAtt.ValoreAttributo != null)
        //    //        sourceAtt.ValoreAttributo.UpdatePlainText(val);

        //    //}



        //    return val;

        //}




        /// <summary>
        /// Ritorna l'attributo sorgente (non di riferimento) a cui si riferisce.
        /// </summary>
        /// <param name="attributoRiferimento"></param>
        /// <returns></returns>
        public Attributo GetSourceAttributo(Attributo attributo)
        {
            Attributo att = attributo;
            while (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;

                Dictionary<string, EntityType> entTypes = _dataService.GetEntityTypes();
                if (!entTypes.ContainsKey(attRif.ReferenceEntityTypeKey))
                {
                    return null;
                }

                EntityType entType = entTypes[attRif.ReferenceEntityTypeKey];

                if (!entType.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                {
                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), string.Format("Source attributo non trovato:{0} - {1} - {2}", attRif.ReferenceEntityTypeKey, attRif.ReferenceCodiceGuid, attRif.ReferenceCodiceAttributo));
                    return null;
                }

                att = entType.Attributi[attRif.ReferenceCodiceAttributo];
            }
            return att;
        }

        public Attributo GetSourceAttributo(string entTypeKey, string codiceAttributo)
        {
            Dictionary<string, EntityType> entTypes = _dataService.GetEntityTypes();

            EntityType entType = null;
            if (!entTypes.TryGetValue(entTypeKey, out entType))
                return null;

            Attributo att = null;
            if (!entType.Attributi.TryGetValue(codiceAttributo, out att))
                return null;

            return GetSourceAttributo(att);
        }


        /// <summary>
        /// Filtro treeInfo rispetto a ids mantenendo la struttura
        /// </summary>
        /// <param name="treeInfo"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<TreeEntityMasterInfo> TreeFilterById(List<TreeEntityMasterInfo> treeInfo, IEnumerable<Guid> ids)
        {
            List<TreeEntityMasterInfo> result = new List<TreeEntityMasterInfo>();

            Dictionary<Guid, TreeEntityMasterInfo> parentsId = treeInfo.ToDictionary(item => item.Id, item => item);
            HashSet<Guid> filterIds = new HashSet<Guid>(ids);

            HashSet<Guid> added = new HashSet<Guid>();

            foreach (TreeEntityMasterInfo item in treeInfo)
            {
                if (!filterIds.Contains(item.Id))
                    continue;

                if (!added.Contains(item.Id))
                {
                    result.Add(item);
                    added.Add(item.Id);
                }

                int index = result.Count-1;
                Guid parentId = item.ParentId;
                while (parentId != Guid.Empty)
                {
                    TreeEntityMasterInfo itemParentId = parentsId[parentId];

                    if (!added.Contains(itemParentId.Id))
                    {
                        result.Insert(index, itemParentId);
                        added.Add(itemParentId.Id);
                    }

                    parentId = itemParentId.ParentId;
                }
            }


            return result;
        }

        public DivisioneItemType GetDivisioneTypeById(Guid id)
        {
            IEnumerable<EntityType> entTypes = _dataService.GetEntityTypes().Values.Where(item => item is DivisioneItemType);
            DivisioneItemType divType = entTypes.FirstOrDefault(item => (item as DivisioneItemType).DivisioneId == id) as DivisioneItemType;
            return divType;
        }

        public DivisioneItemType GetDivisioneTypeByCodice(string codice)
        {
            IEnumerable<EntityType> entTypes = _dataService.GetEntityTypes().Values.Where(item => item is DivisioneItemType);
            DivisioneItemType divType = entTypes.FirstOrDefault(item => item.Codice == codice) as DivisioneItemType;
            return divType;
        }


        public static string GetNewCodiceAttributo(HashSet<string> codici)
        {
            string candidate = null;
            int i = -1;
            while (i < 1000)
            {
                i++;
                candidate = i.ToString();
                if (!codici.Contains(candidate))
                    break;
            }

            codici.Add(candidate);
            return candidate;

        }

        public string GetNewCodiceDivisione()
        {
            HashSet<string> codici = new HashSet<string>(_dataService.GetEntityTypes().Select(item => item.Value.Codice));

            string candidate = null;
            int i = -1;
            while (i < 1000)
            {
                i++;
                candidate = i.ToString();
                if (!codici.Contains(candidate))
                    break;
            }

            return candidate;

        }

        public static bool IsCodiceBuiltIn(string codice)
        {
            if (codice == null || !codice.Any())
                return false;

            return !int.TryParse(codice, out _);
        }

        public void UpdateRtfByStiliItems(ref string rtf)
        {

            if (_dataService.IsReadOnly)
                return;

            List<EntityMasterInfo> entsInfo = _dataService.GetFilteredEntities(BuiltInCodes.EntityType.Stili, null, null, null, out _);
            List<Entity> ents = _dataService.GetEntitiesById(BuiltInCodes.EntityType.Stili, entsInfo.Select(item => item.Id));

            Valore val = null;
            Dictionary<string, StyleInfo> styles = new Dictionary<string, StyleInfo>();
            foreach (Entity ent in ents)
            {
                var styleInfo = new StyleInfo();

                val = ent.GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, false);
                if (val != null)
                    styleInfo.Key = val.PlainText;

                //val = ent.GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, false);
                //if (val != null)
                //    styleInfo.DisplayName = val.PlainText;

                val = ent.GetValoreAttributo(BuiltInCodes.Attributo.Carattere, false, false);
                if (val != null)
                    styleInfo.FontFamilyName = val.PlainText;

                val = ent.GetValoreAttributo(BuiltInCodes.Attributo.DimensioneCarattere, false, false);
                if (val != null)
                {
                    double res = 10;
                    if (Double.TryParse(val.PlainText, out res))
                        styleInfo.FontSize = res;
                }

                ValoreBooleano valBool = ent.GetValoreAttributo(BuiltInCodes.Attributo.Grassetto, false, false) as ValoreBooleano;
                if (valBool != null)
                    styleInfo.IsBold = valBool.V.Value;

                valBool = ent.GetValoreAttributo(BuiltInCodes.Attributo.Italic, false, false) as ValoreBooleano;
                if (valBool != null)
                    styleInfo.IsItalic = valBool.V.Value;

                valBool = ent.GetValoreAttributo(BuiltInCodes.Attributo.Sottolineato, false, false) as ValoreBooleano;
                if (valBool != null)
                    styleInfo.IsUnderline = valBool.V.Value;

                valBool = ent.GetValoreAttributo(BuiltInCodes.Attributo.Barrato, false, false) as ValoreBooleano;
                if (val != null)
                    styleInfo.IsStrikethrough = valBool.V.Value;

                ValoreColore valCol = ent.GetValoreAttributo(BuiltInCodes.Attributo.ColoreCarattere, false, false) as ValoreColore;
                if (valCol != null)
                    styleInfo.ForeColorHex = valCol.Hexadecimal;

                //paragraph
                valCol = ent.GetValoreAttributo(BuiltInCodes.Attributo.ColoreSfondo, false, false) as ValoreColore;
                if (valCol != null)
                    styleInfo.BackColorHex = valCol.Hexadecimal;

                ValoreElenco valElenco = ent.GetValoreAttributo(BuiltInCodes.Attributo.Allineamento, false, false) as ValoreElenco;
                if (valElenco != null)
                    styleInfo.TextAlignment = (TextAlignmentEnum) valElenco.ValoreAttributoElencoId;


                if (!styles.ContainsKey(styleInfo.Key))
                    styles.Add(styleInfo.Key, styleInfo);
            }

            //RtfDataService rtfDataService = new RtfDataService(_dataService);

            ValoreHelper.RtfUpdateStyles(ref rtf, styles.Values.ToList()/*, rtfDataService*/);


        }

        public List<string> GetValorePlainTextForLevel(Entity entity, string codiceAttributo, bool deep, bool brief)
        {
            

            List<string> result = new List<string>();

            EntityAttributo entAtt = GetSourceEntityAttributo(entity, codiceAttributo);
            if (entAtt == null)
                return null;

            Entity ent = entAtt.Entity;
            Attributo sourceAtt = entAtt.Attributo;
            string codAttSource = sourceAtt.Codice;

            if (ent == null)
                return null;

            //string result = string.Empty;

            if (ent.EntityType.IsTreeMaster)
            {

                TreeEntity entIter = ent as TreeEntity;
                while (entIter != null)
                {
                    if (!entIter.Attributi.ContainsKey(codAttSource))
                        break;

                    if (entIter.Attributi[codAttSource].Valore is ValoreTestoRtf)
                    {
                        ValoreTestoRtf valRtf = entIter.Attributi[codAttSource].Valore as ValoreTestoRtf;
                        string plainText;

                        //brief rtf
                        if (brief)
                            plainText = valRtf.BriefPlainText;
                        else
                            plainText = valRtf.PlainText;

                        //if (result != null && result.Any())
                        //    result = string.Format("{0}{1}{2}", plainText, levelSeparator, result);
                        //else
                        //    result = plainText;

                        if (result.Any())
                            result.Insert(0, plainText);
                        else
                            result.Add(plainText);

                    }
                    else
                    {
                        //Valore val = entIter.Attributi[codAttSource].Valore;
                        Valore val = GetValoreAttributo(entIter, codAttSource, false, false);

                        //if (sourceAtt.ValoreAttributo != null)
                        //    sourceAtt.ValoreAttributo.UpdatePlainText(val);

                        
                        string plainText = (val == null)? string.Empty : val.ToPlainText();

                        //if (result != null && result.Any())
                        //    result = string.Format("{0}{1}{2}", plainText, levelSeparator, result);
                        //else
                        //    result = plainText;
                        if (result.Any())
                            result.Insert(0, plainText);
                        else
                            result.Add(plainText);

                    }

                    if (deep)
                        entIter = entIter.Parent;
                    else
                        break;
                }
            }
            else if (entAtt.Valore != null)
            {
                Valore val = GetValoreAttributo(entity, codiceAttributo, false, false);

                //if (sourceAtt.ValoreAttributo != null)
                //    sourceAtt.ValoreAttributo.UpdatePlainText(val/*entAtt.Valore*/);

                //result = entAtt.Valore.PlainText;
                //result.Add(entAtt.Valore.PlainText);
                result.Add(val.PlainText);

            }

            return result;
        }

        public string GetValorePlainText(Entity entity, string codiceAttributo, bool deep, bool brief)
        {
            string levelSeparator = "\\";

            List<string> res = GetValorePlainTextForLevel(entity, codiceAttributo, deep, brief);

            if (res == null)
                return null;

            string str = string.Join(levelSeparator, res);
            return str;
        }


        public string GetRtfPreview(string rtf, RtfEntityDataService rtfDataService)
        {
            UpdateRtfByStiliItems(ref rtf);

            return ValoreHelper.RtfMailMerge(rtf, rtfDataService);
        }


        /// <summary>
        /// metodo ricorsivo
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <returns></returns>
        public List<string> GetDependentEntityTypesKey(string entityTypeKey)
        {
            Dictionary<string, EntityType> entTypesDict = _dataService.GetEntityTypes();

            if (entityTypeKey == null || !entTypesDict.ContainsKey(entityTypeKey))
                return new List<string>();

            //raccolgo le entityType da aggiornare
            HashSet<string> entTypesDependent = new HashSet<string>();

            foreach(EntityType entType in entTypesDict.Values)
            {
                string entTypeKey = entType.GetKey();
                if (entTypeKey == entityTypeKey)
                    continue;

                IEnumerable<Attributo> attsGuid = entType.Attributi.Values.Where(item => item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection);
                foreach (Attributo attGuid in attsGuid)
                {
                    if (attGuid.GuidReferenceEntityTypeKey == entityTypeKey)
                    {
                        entTypesDependent.Add(entTypeKey);
                        entTypesDependent.UnionWith(GetDependentEntityTypesKey(entTypeKey));
                        break;
                    }
                }

                if (entityTypeKey == BuiltInCodes.EntityType.Variabili)
                {
                    IEnumerable<Attributo> attsVar = entType.Attributi.Values.Where(item => item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile);
                    if (attsVar.Any())
                        entTypesDependent.Add(entTypeKey);
                }

            }

            List<string> entTypes = new List<string>();
            foreach (string entTypeKey in GetEntityTypeDependencyOrder())
            {
                if (entTypesDependent.Contains(entTypeKey))
                {
                    entTypes.Add(entTypeKey);
                }
            }

            return entTypes;

        }

        public List<string> GetEntityTypeDependencyOrder()
        {
            List<string> list = new List<string>();
            
            list.Add(BuiltInCodes.EntityType.Variabili); 
            list.Add(BuiltInCodes.EntityType.Allegati);
            list.Add(BuiltInCodes.EntityType.Tag);

            //divisioni
            var entTypes = _dataService.GetEntityTypes();
            foreach (EntityType divType in entTypes.Values.Where(item => item.DependencyEnum == EntityTypeDependencyEnum.Divisione))
                list.Add(divType.GetKey());

            
            list.Add(BuiltInCodes.EntityType.Stili);
            list.Add(BuiltInCodes.EntityType.Capitoli);
            list.Add(BuiltInCodes.EntityType.Contatti);
            list.Add(BuiltInCodes.EntityType.ElencoAttivita);
            list.Add(BuiltInCodes.EntityType.Calendari);
            list.Add(BuiltInCodes.EntityType.Prezzario);
            list.Add(BuiltInCodes.EntityType.Elementi);
            list.Add(BuiltInCodes.EntityType.Computo);
            list.Add(BuiltInCodes.EntityType.WBS);
            list.Add(BuiltInCodes.EntityType.InfoProgetto);
            list.Add(BuiltInCodes.EntityType.Report);
            list.Add(BuiltInCodes.EntityType.Documenti);

            return list;

    }

        public Valore GetValoreAttributo(string entityTypeKey, string codiceAttributo, bool deep, bool brief)
        {
            if (entityTypeKey == BuiltInCodes.EntityType.InfoProgetto || entityTypeKey == BuiltInCodes.EntityType.Variabili)
            {
                EntityMasterInfo info = _dataService.GetFilteredEntities(entityTypeKey, null, null, null, out _).FirstOrDefault();
                Entity entInfo = _dataService.GetEntityById(entityTypeKey, info.Id);

                return GetValoreAttributo(entInfo, codiceAttributo, deep, brief);
            }

            return null;
        }

        /// <summary>
        /// Aggiorno i valori di default degli attributi di tipo Numero sostituendo nella formula eventuali funzioni att{Nome vecchio} con il att{Nome nuovo}
        /// </summary>
        /// <param name="entityTypeNew"></param>
        /// <param name="etichetteModificate"></param>
        public static void ReplaceAttributiEtichette(EntityType entityTypeNew, Dictionary<string, string> etichetteModificate)
        {


            //Sostituzione Nome vecchio con nuovo nel valore di default di tipo numero
            foreach (string oldEtichetta in etichetteModificate.Keys)
            {
                string newEtichetta = etichetteModificate[oldEtichetta];
                foreach (Attributo att in entityTypeNew.Attributi.Values)
                {

                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                    {
                        ValoreReale vReale = att.ValoreDefault as ValoreReale;
                        if (vReale != null)
                        {
                            string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
                            string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

                            if (vReale.HasValue())
                                vReale.V = vReale.V.Replace(oldFunction, newFunction);
                        }
                    }
                    else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    {
                        ValoreContabilita vCont = att.ValoreDefault as ValoreContabilita;
                        if (vCont != null)
                        {
                            string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
                            string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

                            if (vCont.HasValue())
                                vCont.V = vCont.V.Replace(oldFunction, newFunction);
                        }
                    }
                }
            }

        }


        /// <summary>
        /// Aggiorno i valori degli attributi di un'entità di tipo Numero sostituendo nella formula eventuali funzioni att{Nome vecchio} con il att{Nome nuovo}
        /// </summary>
        /// <param name="entityTypeNew"></param>
        /// <param name="etichetteModificate"></param>
        /// <param name="entAtt"></param>
        public static void ReplaceEntityAttributiEtichette(EntityType entityTypeNew, Dictionary<string, string> etichetteModificate, EntityAttributo entAtt)
        {

            foreach (string oldEtichetta in etichetteModificate.Keys)
            {
                if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                {
                    ValoreReale vReale = entAtt.Valore as ValoreReale;
                    if (vReale != null)
                    {
                        string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
                        string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

                        if (vReale.V != null)
                            vReale.V = vReale.V.Replace(oldFunction, newFunction);
                    }
                }
                else if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                {
                    ValoreContabilita vCont = entAtt.Valore as ValoreContabilita;
                    if (vCont != null)
                    {
                        string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
                        string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

                        if (vCont.V != null)
                            vCont.V = vCont.V.Replace(oldFunction, newFunction);

                    }
                }
            }
        }

        /// <summary>
        /// Funzione che cerca di stabilire se dopo una modifica a EntityType sono da aggiornare tutte le entità
        /// </summary>
        /// <param name="entityTypeOld"></param>
        /// <param name="entityTypeNew"></param>
        /// <param name="etichetteModificate"></param>
        /// <returns></returns>
        public static bool AreEntitiesToUpdate(EntityType entityTypeOld, EntityType entityTypeNew, Dictionary<string, string> etichetteModificate)
        {
            /////////////////////////////////////////////////////////////////////
            ///Cerco di capire se le entità sono da aggiornare

            //etichette modificate
            if (etichetteModificate.Any())
                return true;


            //attributi aggiunti
            List<string> attsToAdd2 = entityTypeNew.Attributi.Select(att => att.Key).Where(k => !entityTypeOld.Attributi.Keys.Contains(k)).ToList();
            if (attsToAdd2.Any())
                return true;

            //attributi rimossi
            List<string> attsToRemove2 = entityTypeOld.Attributi.Select(att => att.Key)
            .Where(k => !entityTypeNew.Attributi.ContainsKey(k))
            .ToList();

            if (attsToRemove2.Any())
                return true;

            //attributi modificati nel valore di IsValoreLockedByDefault
            List<string> attsChanged = entityTypeNew.Attributi.Select(att => att.Key).Where(k => entityTypeNew.Attributi[k].IsValoreLockedByDefault != entityTypeOld.Attributi[k].IsValoreLockedByDefault).ToList();
            if (attsChanged.Any())
                return true;


            //attributi locked che hanno cambiato il valore di default
            List<string> attsDefaultChanged = entityTypeNew.Attributi.Select(att => att.Key).Where(k =>
            {
                if (entityTypeNew.Attributi[k].IsValoreLockedByDefault == true)
                {
                    if (!entityTypeNew.Attributi[k].ValoreDefault.Equals(entityTypeOld.Attributi[k].ValoreDefault))
                        return true;
                }
                return false;
            }).ToList();
            if (attsDefaultChanged.Any())
                return true;

            //attributi di cui è stata modificata la definizione
            List<string> attsDefChanged = entityTypeNew.Attributi.Values.Where(item => item.DefinizioneAttributoCodice != entityTypeOld.Attributi[item.Codice].DefinizioneAttributoCodice)
                                                                        .Select(item => item.Codice).ToList();
            if (attsDefChanged.Any())
                return true;

            //Attributi la cui modifica di ValoreAttributo implica il ricalcolo di tutte le entità
            IEnumerable<string> attsVarChanged = entityTypeNew.Attributi.Values.Where(item =>
            {
                //if (item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile ||
                //    item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                //{

                    if (entityTypeOld.Attributi.ContainsKey(item.Codice))
                    {
                        ValoreAttributo valAttNew = item.ValoreAttributo;
                        ValoreAttributo valAttOld = entityTypeOld.Attributi[item.Codice].ValoreAttributo;

                        if (valAttNew != null && !valAttNew.Equals(valAttOld))
                            return true;
                    }
                //}
                return false;
            }).Select(item => item.Codice);
            if (attsVarChanged.Any())
                return true;

            //Attributi di cui è stato modificato il formato
            List<string> attsFormatChanged = entityTypeNew.Attributi.Values.Where(item => item.ValoreFormat != entityTypeOld.Attributi[item.Codice].ValoreFormat)
                                                            .Select(item => item.Codice).ToList();
            if (attsFormatChanged.Any())
                return true;



            return false;
        }

        public EntityType GetEntityType(string entityTypeKey)
        {
            EntityType entType = _dataService.GetEntityType(entityTypeKey);
            return entType;
        }

        public EntityType GetTreeEntityType(Entity entity)
        {
            if (entity == null)
                return null;

            TreeEntity treeEnt = entity as TreeEntity;

            if (treeEnt != null && treeEnt.IsParent)
                return treeEnt.ParentEntityType;

            return entity.EntityType;

        }

        public bool IsAttributoRiferimentoGuidCollection(string entityTypeKey, string codiceAttributo)
        {

            EntityType entType = _dataService.GetEntityType(entityTypeKey);

            if (entType == null)
                return false;

            if (!entType.Attributi.ContainsKey(codiceAttributo))
                return false;

            Attributo att = entType.Attributi[codiceAttributo];

            return IsAttributoRiferimentoGuidCollection(att);
        }

        public bool IsAttributoRiferimentoGuidCollection(Attributo att)
        {
            AttributoRiferimento attRif = att as AttributoRiferimento;
            if (attRif == null)
                return false;

            EntityType entType = _dataService.GetEntityType(attRif.EntityTypeKey);

            if (entType.Attributi.ContainsKey(attRif.ReferenceCodiceGuid))
            {
                Attributo attGuid = entType.Attributi[attRif.ReferenceCodiceGuid];
                if (attGuid.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    return true;
            }
            return false;
        }

        //private Valore GetAttributoRiferimentoGuidCollectionValore(Entity entity, string codiceAttributo)
        //{
        //    Valore valore = null;

        //    ProjectService projectService = _dataService as ProjectService;
        //    if (projectService == null)
        //    {
        //        ClientDataService clientDataService = _dataService as ClientDataService;
        //        projectService = clientDataService.Service as ProjectService;
        //    }

        //    if (projectService == null)
        //        return valore;

        //    string entTypeKey = entity.EntityType.GetKey();

        //    Attributo att = null;
        //    entity.EntityType.Attributi.TryGetValue(codiceAttributo, out att);
        //    if (att == null)
        //        return null;

        //    valore = projectService.Calculator.GetCalculatedValue(entTypeKey, entity.EntityId, att.Etichetta);

        //    return valore;
        //}



        private Valore CalculateAttributoRiferimentoGuidCollectionValore(Entity entity, string codiceAttributo)
        {


            ////per non far ricalcolare molte volte il riferimento al GuidCollection
            //Valore val = GetAttributoRiferimentoGuidCollectionValore(entity, codiceAttributo);

            //if (val != null)
            //    return val;



            Valore valore = null;

            ProjectService projectService = _dataService as ProjectService;
            if (projectService == null)
            {
                ClientDataService clientDataService = _dataService as ClientDataService;
                projectService = clientDataService.Service as ProjectService;
            }

            if (projectService == null)
                return valore;

            AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(_dataService);

            Valore result = null;
            string entityTypeKey = entity.EntityType.GetKey();   
            


            EntityType entType = _dataService.GetEntityType(entityTypeKey);
            if (!entType.Attributi.ContainsKey(codiceAttributo))
                return null;

            AttributoRiferimento attRif = entType.Attributi[codiceAttributo] as AttributoRiferimento;
            if (attRif == null)
                return null;

            Attributo sourceAtt = GetSourceAttributo(attRif);

            ValoreAttributoRiferimentoGuidCollection valAtt = attRif.ValoreAttributo as ValoreAttributoRiferimentoGuidCollection;
            if (valAtt == null)
                return null;

            ValoreGuidCollection valGuidColl = GetValoreAttributo(entity, attRif.ReferenceCodiceGuid, false, false) as ValoreGuidCollection;
            if (valGuidColl == null)
                return null;

            //per non far ricalcolare molte volte il riferimento al GuidCollection
            Valore val = projectService.Calculator.GetRiferimentoGuidCollectionsCalculatedValore(valGuidColl.Filter, attRif);
            if (val != null)
                return val;

            IEnumerable<Guid> ids = valGuidColl.GetEntitiesId();
            List<Entity> ents = _dataService.GetEntitiesById(attRif.ReferenceEntityTypeKey, ids);


            List<Valore> vals = new List<Valore>();
            foreach (Entity ent in ents)
            {
                Valore v = GetValoreAttributo(ent, attRif.ReferenceCodiceAttributo, false, false);
                vals.Add(v);
            }

            int? significantDigitsCount = null;
            var valAttReale = sourceAtt.ValoreAttributo as ValoreAttributoReale;
            if (valAttReale != null && valAttReale.UseSignificantDigitsByFormat)
            {
                NumberFormat nf = NumericFormatHelper.DecomposeFormat(sourceAtt.ValoreFormat);
                if (nf.DecimalDigitCount >= 0)
                {
                    significantDigitsCount = nf.DecimalDigitCount;
                }
            }

            if (vals.Any())
            {
                int p = 0;
            }

            string format = attFormatHelper.GetValorePaddedFormat(sourceAtt);
            result = ValoreCalculator.ExecuteOperation(sourceAtt.DefinizioneAttributoCodice, vals, valAtt.Operation, significantDigitsCount);


            projectService.Calculator.SetRiferimentoGuidCollectionsCalculatedValore(valGuidColl.Filter, attRif, result);

            return result;

        }

        public bool IsAttributoDeep(string entityTypeKey, string codiceAttributo)
        {
            EntityType entType = _dataService.GetEntityType(entityTypeKey);
            TreeEntityType treeEntType = entType as TreeEntityType;
            if (treeEntType != null)
            {
                HashSet<string> parentAttsCode = new HashSet<string>(treeEntType.GetParentAttributi());
                if (parentAttsCode.Contains(codiceAttributo))
                    return true;
            }
            return false;

        }

        /// <summary>
        /// Metodo ricorsivo per istanziare una lista di model3dbjectsKey dato un elemento o gruppo di elementi
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="model3DObjectsKey"></param>
        public void GetModel3dObjectsByElement(Entity elem, List<Model3dObjectKey> model3DObjectsKey)
        {
            if (!(elem is ElementiItem))
                return;

            if (model3DObjectsKey == null)
                return;

            Valore valGlobalId = elem.GetValoreAttributo(BuiltInCodes.Attributo.GlobalId, false, false);
            string globalId = (valGlobalId == null) ? String.Empty : valGlobalId.PlainText;

            Valore valFileIfc = elem.GetValoreAttributo(BuiltInCodes.Attributo.ProjectGlobalId, false, false);
            string projectGlobalId = (valFileIfc == null) ? String.Empty : valFileIfc.PlainText;

            if (!string.IsNullOrEmpty(globalId) && !string.IsNullOrEmpty(projectGlobalId))
            {
                Model3dObjectKey m3dObjKey = new Model3dObjectKey();
                m3dObjKey.GlobalId = valGlobalId.PlainText;
                m3dObjKey.ProjectGlobalId = valFileIfc.PlainText;

                model3DObjectsKey.Add(m3dObjKey);
            }
            else
            {
                //non è un elemento ifc

                Valore valIfcClass = elem.GetValoreAttributo(BuiltInCodes.Attributo.IfcClass, false, false);
                if (valIfcClass != null && valIfcClass.PlainText == Model3dClassEnum.IfcGroup.ToString())
                {

                    EntityType groupDivType = _dataService.GetEntityTypes().Values.FirstOrDefault(item =>
                    {
                        if (item is DivisioneItemType)
                        {
                            if ((item as DivisioneItemType).Model3dClassName == Model3dClassEnum.IfcGroup)
                                return true;
                        }
                        return false;
                    });

                    if (groupDivType != null)//esiste la divisione IfcGroup
                    {

                        EntityType elType = _dataService.GetEntityType(BuiltInCodes.EntityType.Elementi);
                        Attributo attIfcGroup = null;
                        elType.Attributi.TryGetValue(BuiltInCodes.Attributo.IfcGroup, out attIfcGroup);

                        if (attIfcGroup != null && attIfcGroup.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                        {
                            //esiste almeno un attributo multiriferimento degli elementi con riferimento alla divisione IfcGroup

                            Valore valGroups = elem.GetValoreAttributo(BuiltInCodes.Attributo.IfcGroup, false, false);

                            var valGroupsColl = valGroups as ValoreGuidCollection;

                            if (valGroupsColl != null)
                            {
                                IEnumerable<Guid> groupsId = valGroupsColl.GetEntitiesId();

                                if (groupsId.Any())
                                {

                                    FilterData filter = new FilterData();
                                    filter.Items.Add(new AttributoFilterData()
                                    {
                                        EntityTypeKey = BuiltInCodes.EntityType.Elementi,
                                        FilterType = FilterTypeEnum.Result,
                                        CodiceAttributo = BuiltInCodes.Attributo.IfcGroup,
                                        CheckedValori = new HashSet<string>(groupsId.Select(item => item.ToString())),
                                        IsFiltroAttivato = true,
                                    });
                                    List<Guid> entitiesFound = new List<Guid>();//entità appartenenti ad almeno un gruppo di groupsId
                                    _dataService.GetFilteredEntities(BuiltInCodes.EntityType.Elementi, filter, null, null, out entitiesFound);

                                    entitiesFound.Remove(elem.EntityId);


                                    List<Entity> ents = _dataService.GetEntitiesById(BuiltInCodes.EntityType.Elementi, entitiesFound);

                                    ents.ForEach(item => GetModel3dObjectsByElement(item, model3DObjectsKey));
                                }
                            }
                        }

                    }
                }
            }
        }

        public bool IsAttributoRiferimento(Attributo attributo, string entityTypeKey = null)
        {
            if (attributo is AttributoRiferimento)
            {
                AttributoRiferimento attRif = attributo as AttributoRiferimento;

                if (entityTypeKey != null)
                {
                    if (attRif.ReferenceEntityTypeKey == entityTypeKey)
                        return true;
                }
                else
                    return true;

            }
            return false;
        }

        public IEnumerable<Guid> GetEntitiesIdReferencedByAttributo(EntityAttributo entAtt)
        {
            HashSet<Guid> entitiesId = new HashSet<Guid>();
            
            if (entAtt == null)
                return entitiesId;

            if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {
                Valore valGuids = GetValoreAttributo(entAtt.Entity, entAtt.AttributoCodice, false, false);
                if (valGuids is ValoreGuidCollection)
                    entitiesId.UnionWith((valGuids as ValoreGuidCollection).GetEntitiesId());
                    //entitiesId.AddRange((valGuids as ValoreGuidCollection).GetEntitiesId());
                if (valGuids is ValoreGuid)
                    entitiesId.Add((valGuids as ValoreGuid).V);

                return entitiesId;
            }
            else if (entAtt.Attributo is AttributoRiferimento)
            {
                AttributoRiferimento attRif = entAtt.Attributo as AttributoRiferimento;
                EntityAttributo refEntAtt = GetSourceEntityAttributo(entAtt.Entity, attRif.ReferenceCodiceGuid);

                return GetEntitiesIdReferencedByAttributo(refEntAtt);
            }

            return entitiesId;
        }

        public string GetAttributoFilterTextDescription(AttributoFilterData attFilterData)
        {

            string str = string.Empty;

            if (attFilterData == null)
                return string.Empty;

            EntityType entType = _dataService.GetEntityTypes().Values.FirstOrDefault(item => item.Codice == attFilterData.EntityTypeKey);
            if (entType == null)
                return string.Empty;

            Attributo att = null;
            if (!entType.Attributi.TryGetValue(attFilterData.CodiceAttributo, out att))
                return string.Empty;



            if (attFilterData.FilterType == FilterTypeEnum.Conditions)
            {
                str = LocalizationProvider.GetString("_Condizione");
            }
            else
            {
                Attributo sourceAtt = GetSourceAttributo(att);
                if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    IEnumerable<Guid> ids = attFilterData.CheckedValori.Select(item => new Guid(item));
                    List<Entity> ents = _dataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, ids);

                    str = string.Join("\n", ents.Select(item => item.ToUserIdentity(UserIdentityMode.SingleLine)));
                }
                else
                {
                    str = string.Join("\n", attFilterData.CheckedValori);

                }
            }

            return str;
        }

        public Attributo GetAttributo(Entity ent, string codiceAtt)
        {
            EntityType entType = _dataService.GetEntityType(ent.EntityTypeCodice);
            TreeEntity treeEnt = ent as TreeEntity;
            if (treeEnt != null &&  treeEnt.IsParent)
            {
                TreeEntityType treeEntType = entType as TreeEntityType;
                if (treeEntType != null)
                {
                    TreeEntityType parentEntType = treeEntType.AssociedType as TreeEntityType;
                    if (parentEntType != null && parentEntType.Attributi.ContainsKey(codiceAtt))
                        return parentEntType.Attributi[codiceAtt];
                }
            }
            else if (entType.Attributi.ContainsKey(codiceAtt))
                return entType.Attributi[codiceAtt];

            return null;
        }


        public TreeEntityType GetEntityTypeParent(string entTypeKey)
        {
            EntityType entType = _dataService.GetEntityType(entTypeKey);
            TreeEntityType treeEntType = entType as TreeEntityType;
            if (treeEntType != null)
            {
                TreeEntityType parentEntType = treeEntType.AssociedType as TreeEntityType;
                return parentEntType;
            }
            return null;
        }


        public Dictionary<string, Attributo> GetAttributi(Entity ent)
        {
            if (ent == null) return new Dictionary<string, Attributo>();

            Dictionary<string, Attributo> atts = new Dictionary<string, Attributo>();

            EntityType entType = _dataService.GetEntityType(ent.EntityTypeCodice);
            if (entType != null)
            {
                TreeEntity treeEnt = ent as TreeEntity;
                if (treeEnt != null && treeEnt.IsParent)
                {
                    TreeEntityType treeEntType = entType as TreeEntityType;
                    if (treeEntType != null)
                    {
                        TreeEntityType parentEntType = treeEntType.AssociedType as TreeEntityType;
                        if (parentEntType != null)
                            atts = parentEntType.Attributi;
                    }
                }
                else
                    atts = entType.Attributi;
            }

            return atts;
        }

        


    }
}
