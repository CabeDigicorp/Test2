using CommonResources;
using Commons;
using DevExpress.Charts.Native;
using DevExpress.CodeParser;
using DevExpress.Data.Extensions;
using DevExpress.Mvvm.Native;
using DevExpress.Office.Utils;
using DevExpress.Utils.Serializing.Helpers;
using DevExpress.Xpf.Core.Native;
using DevExpress.XtraSpreadsheet.Import.Xls;
using FastReport;
using FastReport.Utils;
using JReport;
using MasterDetailModel;
using Microsoft.Isam.Esent.Interop;
using Model;
using Model.Calculators;
using Newtonsoft.Json;
using StampeWpf.Wizard;
using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.RightsManagement;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;

namespace StampeWpf
{
    public class ReportDataSourceGenerator
    {
        public ClientDataService DataService { get; set; }
        public JReport.Report ReportObject { get; set; }
        private List<dynamic> UnknowDatasource { get; set; }
        public List<AttributiUtilizzati> AttributiDaEstrarrePerDatasource { get; set; }
        public bool IsDataSourceEmpty { get; set; }
        private RtfEntityDataService RtfDataService;

        public List<ParenEntity> ListParentItem = new List<ParenEntity>();

        private FilterData filter;
        private EntitiesHelper entitiesHelper;
        private AttributoFormatHelper attributoFormatHelper;
        private string SezionePrimaElaborazione;

        private Dictionary<string, string> UpdateRtfEseguiti;
        private string EmptyValue = "...";

        #region DATASOURCE DI APPOGGIO

        private Dictionary<Guid, string> DictionaryDatasourceEsternoPerOrdinamentoGuid = new Dictionary<Guid, string>();

        public DataTable DataTable { get; set; }
        public List<Raggruppamento> RaggruppamentiDatasource { get; set; }
        public List<Raggruppamento> RaggruppamentiDatasourceToRemove { get; set; }
        public OrdinamentoCorpo OrdinamentoCorpo { get; set; }
        private bool IsTreeMaster { get; set; }
        public bool IsAllFieldRtfFormat { get; set; }
        private bool IsDesignerPreview { get; set; }

        private int filterOperation { get; set; }
        private IMainOperation mainOperation { get; set; }
        private string DefaultFormatRelaeContabilità;


        #endregion
        public ReportDataSourceGenerator()
        {
            ReportObject = new JReport.Report();
            UnknowDatasource = new List<dynamic>();
            UpdateRtfEseguiti = new Dictionary<string, string>();
            RaggruppamentiDatasource = new List<Raggruppamento>();
            DefaultFormatRelaeContabilità = NumericFormatHelper.DefaultFormat;
        }

        public void CreateGenericDataSource(string SezioneItem, bool isDesignerPreview, IMainOperation MainOperation, int FilterOperation = 0)
        {
            mainOperation = MainOperation;
            filterOperation = FilterOperation;
            IsDesignerPreview = isDesignerPreview;
            RtfDataService = new RtfEntityDataService(MainOperation);


            ViewSettings viewSettings = DataService.GetViewSettings();
            if (!viewSettings.EntityTypes.ContainsKey(SezioneItem))
                return;

            //ricalcolo degli items della sezione se questa prevede l'aggiornamento manuale (pulsante di aggiornamento)
            //per esempio in presenza di attributi di riferimento alla sezione medesima
            if (MainOperation.IsRecalculateItemsNeeded(SezioneItem))
            {
                EntitiesError error = new EntitiesError();
                var calcOpts = new EntityCalcOptions() { CalcolaAttributiResults = true, ResetCalulatedValues = true };   
                DataService.CalcolaEntities(SezioneItem, calcOpts, null, error);

                //MainOperation.IsRecalculateItemsNeeded(SezioneItem, false);
            }

            EntityTypeViewSettings EntityViewSettings = viewSettings.EntityTypes[SezioneItem];

            //creo un filtro uguale al filtro corrente della vista
            filter = new FilterData();
            EntityViewSettings.Filters.ForEach(item =>
            {
                AttributoFilterData attFilter = new AttributoFilterData()
                {
                    CodiceAttributo = item.CodiceAttributo,
                    CheckedValori = new HashSet<string>(item.CheckedValori),
                    EntityTypeKey = SezioneItem,
                    IsFiltroAttivato = true,
                };
                filter.Items.Add(attFilter);
            });

            entitiesHelper = new EntitiesHelper(DataService);
            attributoFormatHelper = new AttributoFormatHelper(DataService);
            SezionePrimaElaborazione = SezioneItem;

            UnknowDatasource.Clear();
            ListParentItem.Clear();
            _staticCount = 0;

            HashSet<Guid> FilteredEntitiesIds = new HashSet<Guid>();


            if (DeveloperVariables.IsNewStampa)
                GenerateDataToManage2(SezioneItem, FilteredEntitiesIds);
            else
                GenerateDataToManage(SezioneItem, FilteredEntitiesIds, null, true);


            if (ListParentItem.Count() > 0)
            {
                ListParentItem = ListParentItem.Distinct().ToList();
                var ListaRaggruppata = ListParentItem.GroupBy(x => new {AttributoGuid = x.AttributoGuid,  Attributo1 = x.Attributo1, Attributo2 = x.Attributo2, Attributo3 = x.Attributo3, Numero = x.Numero }).Select(group => group);
                ListParentItem = new List<ParenEntity>();
                foreach (var item in ListaRaggruppata)
                {
                    ListParentItem.Add(new ParenEntity() {AttributoGuid = item.Key.AttributoGuid, Attributo1 = item.Key.Attributo1, Attributo2 = item.Key.Attributo2, Attributo3 = item.Key.Attributo3, Numero = item.Key.Numero });
                }
            }
        }

        static int _staticCount = 0;
        
        /// <summary>
        /// Add by Ale 10/03/2023 per velocizzare
        /// </summary>
        /// <param name="SezioneItem"></param>
        /// <param name="filteredEntitiesIds"></param>
        private void GenerateDataToManage2(string SezioneItem, HashSet<Guid> filteredEntitiesIds)
        {

            //Esempi
            //CapitoliItemParentPrezzarioItem_Guid__CapitoliItem_Guid1      { 8438fa0c - f39e - 4f5a - b9b3 - d29540948476}
            //CapitoliItemParentPrezzarioItem_Guid__CapitoliItem_Guid2      { a9b4b34e - e1df - 453a - 9be5 - d610a1a7e1b1}
            //CapitoliItemPrezzarioItem_Guid__CapitoliItem_Guid             "e7dd1324-c030-49eb-9877-a5fd69418bd7"
            //CapitoliItemPrezzarioItem_Guid__CapitoliItem_Guid__Codice1    "1C"
            //CapitoliItemPrezzarioItem_Guid__CapitoliItem_Guid__Codice2    "1C.10"
            //CapitoliItemPrezzarioItem_Guid__CapitoliItem_Guid__Codice     "1C.10.300"

            //Destinazione        Percorso
            //CapitoliItem Parent PrezzarioItem_Guid __ CapitoliItem_Guid           1    { 8438fa0c - f39e - 4f5a - b9b3 - d29540948476}
            //CapitoliItem Parent PrezzarioItem_Guid __ CapitoliItem_Guid           2    { a9b4b34e - e1df - 453a - 9be5 - d610a1a7e1b1}
            //CapitoliItem        PrezzarioItem_Guid __ CapitoliItem_Guid                "e7dd1324-c030-49eb-9877-a5fd69418bd7"
            //CapitoliItem        PrezzarioItem_Guid __ CapitoliItem_Guid __ Codice 1    "1C"
            //CapitoliItem        PrezzarioItem_Guid __ CapitoliItem_Guid __ Codice 2    "1C.10"
            //CapitoliItem        PrezzarioItem_Guid __ CapitoliItem_Guid __ Codice      "1C.10.300"


            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[SezioneItem];

            if (EntitySelected.IsTreeMaster == false)
            {
                if (filteredEntitiesIds.Count > 0)
                {
                    Entities = DataService.GetEntitiesById(SezioneItem, filteredEntitiesIds);
                }
                else
                {
                    List<EntityMasterInfo> MasterInfo = null;
                    if (filterOperation == 0)
                    {
                        MasterInfo = DataService.GetFilteredEntities(SezioneItem, null, null, null, out entitiesFound);
                    }
                    if (filterOperation == 1)
                    {
                        entitiesFound = mainOperation.GetSelectedEntitiesId(SezioneItem);
                    }
                    if (filterOperation == 2)
                    {
                        MasterInfo = DataService.GetFilteredEntities(SezioneItem, filter, null, null, out entitiesFound);
                    }


                }
            }
            else
            {
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(SezioneItem, null, null, out entitiesFound);
                if (filteredEntitiesIds.Count > 0)
                {
                    TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, filteredEntitiesIds);
                }
                else
                {
                    if (filterOperation == 1)
                    {
                        entitiesFound = mainOperation.GetSelectedEntitiesId(SezioneItem);
                    }
                    if (filterOperation == 2)
                    {
                        TreeInfo = DataService.GetFilteredTreeEntities(SezioneItem, filter, null, out entitiesFound);
                    }
                    TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
                }

            }


            ////////////////////////////////////////////////////////
            //List<dynamic> UnknowDatasource2 = new List<dynamic>();
            UnknowDatasource = new List<dynamic>();

            Dictionary<Guid, AttributiValoreProperties> attsValue = new Dictionary<Guid, AttributiValoreProperties>();

            foreach (Guid id in entitiesFound)
            {
                System.Dynamic.ExpandoObject unknowObjectLocal = new System.Dynamic.ExpandoObject();

                if (ExtractAttributoValuesRecursive(SezioneItem, id, string.Empty, attsValue, unknowObjectLocal))
                {
                    AddProperty(unknowObjectLocal, StampeKeys.ConstGruppoPadre, null);
                    UnknowDatasource.Add(unknowObjectLocal);
                }
            }
        }

        private bool ExtractAttributoValuesRecursive(string entityTypeKey, Guid id, string codiceAttGuid, Dictionary<Guid, AttributiValoreProperties> attsValues, System.Dynamic.ExpandoObject unknowObjectLocal)
        {
            if (id == Guid.Empty)
                return false;


            Entity ent = DataService.GetEntityById(entityTypeKey, id);


            //EntityTypeEntityId entityId = new EntityTypeEntityId()
            //{ 
            //    EntityTypeKey = entityTypeKey,
            //    EntityId = id,
            //};

            if (attsValues.ContainsKey(id))
            {
                foreach (AttributiValoreProperty attValue in attsValues[id].Items)
                {
                    AddProperty(unknowObjectLocal, attValue.PropertyName, attValue.PropertyValue);
                }

                //valori di tipo guid
                foreach (EntityAttributo entAtt in ent.Attributi.Values.Where(item => !item.Attributo.IsInternal && item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid))
                {
                    IEnumerable<Guid> ids = entitiesHelper.GetEntitiesIdReferencedByAttributo(entAtt);

                    Guid refId = ids.FirstOrDefault();
                    string guidReferenceEntityTypeKey = entAtt.Attributo.GuidReferenceEntityTypeKey;

                    string attCodicePath = entAtt.Attributo.Codice;
                    if (codiceAttGuid.Any())
                        attCodicePath = string.Format("{0}{1}{2}", codiceAttGuid, ReportSettingViewHelper.AttributoCodicePathSeparator, entAtt.Attributo.Codice);

                    if (refId != Guid.Empty && attsValues.ContainsKey(refId))
                    {
                        ExtractAttributoValuesRecursive(guidReferenceEntityTypeKey, refId, attCodicePath, attsValues, unknowObjectLocal);
                    }
                }

                return true;
            }
            else
            {
                attsValues.Add(id, new AttributiValoreProperties());
            }


            _staticCount++;


            //valori di tipo guid
            foreach (EntityAttributo entAtt in ent.Attributi.Values.Where(item => !item.Attributo.IsInternal &&
                (item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid/* || item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection*/)))
            {
                HashSet<Guid> ids = entitiesHelper.GetEntitiesIdReferencedByAttributo(entAtt).ToHashSet();

                if (ids.Count > 0)
                {
                    string guidReferenceEntityTypeKey = entAtt.Attributo.GuidReferenceEntityTypeKey;
                    string guidReferenceEntityTypeKeyRep = JReportHelper.ReplaceSymbolNotAllowInReport(guidReferenceEntityTypeKey);

                    Guid refId = ids.First();

                    string attCodicePath = entAtt.Attributo.Codice;
                    if (codiceAttGuid.Any())
                        attCodicePath = string.Format("{0}{1}{2}", codiceAttGuid, ReportSettingViewHelper.AttributoCodicePathSeparator, entAtt.Attributo.Codice);



                    string propertyName = string.Format("{0}{1}", guidReferenceEntityTypeKeyRep, entAtt.AttributoCodice);

                    AddProperty(unknowObjectLocal, propertyName, refId.ToString());
                    attsValues[id].Items.Add(new AttributiValoreProperty() { PropertyName = propertyName, PropertyValue = refId.ToString() });


                    if (guidReferenceEntityTypeKeyRep.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        AddProperty(unknowObjectLocal, JReportHelper.ReplaceSymbolNotAllowInReport(guidReferenceEntityTypeKey + BuiltInCodes.Attributo.Nome), EmptyValue);
                    }


                    //Sommo o concatenazione di AttributoRiferimentoGuidCollection
                    if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        EntityType entType = ent.EntityType;
                        var attsRifGuidCollection = entType.Attributi.Values.Where(att =>
                        {
                            AttributoRiferimento attRif = att as AttributoRiferimento;
                            if (attRif != null)
                            {
                                if (attRif.ReferenceCodiceGuid == entAtt.AttributoCodice)
                                    return true;
                            }
                            return false;
                        });

                        foreach (AttributoRiferimento attRif in attsRifGuidCollection)
                        {

                            string sumVal = entitiesHelper.GetValorePlainText(ent, attRif.Codice, false, false);
                            if (sumVal == LocalizationProvider.GetString(ValoreHelper.Multi))
                                sumVal = LocalizationProvider.GetString("Multi");

                            string propertyName2 = string.Empty;
                            if (codiceAttGuid.Any())
                                propertyName2 = string.Format("{0}{1}{2}{3}{4}{5}", attRif.ReferenceEntityTypeKey, codiceAttGuid, ReportSettingViewHelper.AttributoCodicePathSeparator, attRif.ReferenceCodiceGuid, ReportSettingViewHelper.AttributoCodicePathSeparator, attRif.ReferenceCodiceAttributo);
                            else
                                propertyName2 = string.Format("{0}{1}{2}{3}", attRif.ReferenceEntityTypeKey, attRif.ReferenceCodiceGuid, ReportSettingViewHelper.AttributoCodicePathSeparator, attRif.ReferenceCodiceAttributo);
                            
                            AddProperty(unknowObjectLocal, propertyName2, sumVal);
                        }
                    }
                    else
                        ExtractAttributoValuesRecursive(guidReferenceEntityTypeKey, refId, attCodicePath, attsValues, unknowObjectLocal);

                }

            }


            var attributiUtilizzati = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault();
            if (attributiUtilizzati == null)
                return true;

            var listaAttributi = attributiUtilizzati.AttributiPerEntityType;




            //valori dei padri
            int lastListParentItemIndex = -1;

            if (ent.EntityType.IsTreeMaster)
            {
                TreeEntityType treeEntityType = ent.EntityType as TreeEntityType;
                TreeEntity treeEnt = ent as TreeEntity;

                if (treeEnt.IsParent)
                    return false;

                var parentAtts = treeEntityType.GetParentAttributi();


                treeEnt = treeEnt.Parent;
                while (treeEnt != null)
                {
                    ParenEntity parentEntity = new ParenEntity();

                    int depthNumber = treeEnt.Depth + 1;

                    //guid
                    Guid idParent = treeEnt.EntityId;

                    string idPropertyName = JReportHelper.ReplaceSymbolNotAllowInReport(string.Format("{0}Parent{1}{2}", treeEntityType.GetKey(), codiceAttGuid, depthNumber));
                    AddProperty(unknowObjectLocal, idPropertyName, idParent);
                    attsValues[id].Items.Add(new AttributiValoreProperty() { PropertyName = idPropertyName, PropertyValue = idParent });
                    parentEntity.Guid = idParent;
                    parentEntity.AttributoGuid = idPropertyName;




                    foreach (string codiceAtt in parentAtts)
                    {
                        string valueTreeEnt = string.Empty;

                        if (ent.EntityType.Attributi[codiceAtt].DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                        {
                                
                            var listaAttributiIndex = listaAttributi.IndexOf(codiceAtt);

                            if (listaAttributiIndex >= 0)
                            {

                                ValoreTestoRtf valTestoRtf = entitiesHelper.GetValoreAttributo(treeEnt, codiceAtt, false, false) as ValoreTestoRtf;

                                var printRtf = attributiUtilizzati.Rtf.ElementAt(listaAttributiIndex);
                                var printDescBreve = attributiUtilizzati.DescrfBreve.ElementAt(listaAttributiIndex);


                                if ((printRtf || IsAllFieldRtfFormat) && printDescBreve)
                                {
                                    string rtf = entitiesHelper.GetRtfPreview(valTestoRtf.BriefRtf, RtfDataService);
                                    valueTreeEnt = ValoreHelper.AdjustRtf(rtf);
                                }

                                if ((printRtf || IsAllFieldRtfFormat) && !printDescBreve)
                                {
                                    string rtf = entitiesHelper.GetRtfPreview(valTestoRtf.V, RtfDataService);
                                    valueTreeEnt = ValoreHelper.AdjustRtf(rtf);
                                }

                                if (!printRtf && !IsAllFieldRtfFormat && printDescBreve)
                                    valueTreeEnt = valTestoRtf.BriefPlainText;

                                if (!printRtf && !IsAllFieldRtfFormat && !printDescBreve)
                                    valueTreeEnt = valTestoRtf.PlainText;

                            }
                        }
                        else
                        {
                            valueTreeEnt = entitiesHelper.GetValorePlainText(treeEnt, codiceAtt, false, false);
                        }
                            
                        string propertyName = string.Empty;
                        if (codiceAttGuid.Any())
                            propertyName = JReportHelper.ReplaceSymbolNotAllowInReport(string.Format("{0}{1}{2}{3}{4}", treeEntityType.GetKey(), codiceAttGuid, ReportSettingViewHelper.AttributoCodicePathSeparator, codiceAtt, depthNumber));
                        else
                            propertyName = JReportHelper.ReplaceSymbolNotAllowInReport(string.Format("{0}{1}{2}{3}", treeEntityType.GetKey(), codiceAttGuid, codiceAtt, depthNumber));


                        AddProperty(unknowObjectLocal, propertyName, valueTreeEnt);
                        attsValues[id].Items.Add(new AttributiValoreProperty() { PropertyName = propertyName, PropertyValue = valueTreeEnt });


                        //if (parentEntity.Attributo1 == null)
                        //    parentEntity.Attributo1 = propertyName;
                        //else if (parentEntity.Attributo2 == null)
                        //    parentEntity.Attributo2 = propertyName;

                        if (ent.EntityType.Codice == BuiltInCodes.EntityType.WBS)
                        {
                            if (codiceAtt == BuiltInCodes.Attributo.Codice)
                                parentEntity.Attributo1 = propertyName;
                            else if (codiceAtt == BuiltInCodes.Attributo.Nome)
                                parentEntity.Attributo2 = propertyName;
                        }
                        else if (ent.EntityType is DivisioneItemType || ent.EntityType is DivisioneItemParentType)
                        {
                            if (codiceAtt == BuiltInCodes.Attributo.Nome)
                                parentEntity.Attributo1 = propertyName;
                            else if (codiceAtt == BuiltInCodes.Attributo.DescrizioneRTF)
                                parentEntity.Attributo2 = propertyName;
                            else if (codiceAtt == BuiltInCodes.Attributo.Descrizione)
                                parentEntity.Attributo3 = propertyName;
                        }
                        else
                        {
                            if (codiceAtt == BuiltInCodes.Attributo.Codice)
                                parentEntity.Attributo1 = propertyName;
                            else if (codiceAtt == BuiltInCodes.Attributo.DescrizioneRTF)
                                parentEntity.Attributo2 = propertyName;
                        }

                        parentEntity.Numero = depthNumber.ToString();
                        parentEntity.Sezione = entityTypeKey;
                        parentEntity.Guid = treeEnt.EntityId;


                    }

                    if (ListParentItem.FirstOrDefault(item => item.Guid == parentEntity.Guid) == null)
                    {
                        if (lastListParentItemIndex < 0)
                        {
                            ListParentItem.Add(parentEntity);
                            lastListParentItemIndex = ListParentItem.Count - 1;
                        }
                        else
                            ListParentItem.Insert(lastListParentItemIndex, parentEntity);
                    }

                    treeEnt = treeEnt.Parent;
                }

            }

            //id ent
            if (!string.IsNullOrEmpty(codiceAttGuid))
            {
                string propertyNameId = JReportHelper.ReplaceSymbolNotAllowInReport(string.Format("{0}{1}", entityTypeKey, codiceAttGuid));
                AddProperty(unknowObjectLocal, propertyNameId, ent.EntityId.ToString());
                attsValues[id].Items.Add(new AttributiValoreProperty() { PropertyName = propertyNameId, PropertyValue = ent.EntityId.ToString() });
            }
            else
            {
                string propertyNameId = JReportHelper.ReplaceSymbolNotAllowInReport(string.Format("{0}", entityTypeKey));
                AddProperty(unknowObjectLocal, propertyNameId, ent.EntityId.ToString());
                attsValues[id].Items.Add(new AttributiValoreProperty() { PropertyName = propertyNameId, PropertyValue = ent.EntityId.ToString() });
            }



            //valori
            for (int i=0; i< listaAttributi.Count; i++)
            {
                string codiceAtt = listaAttributi[i];

                Attributo att = null;
                ent.EntityType?.Attributi.TryGetValue(codiceAtt, out att);
                
                if (att == null)
                    continue;

                var properties = AddProperty(entityTypeKey, codiceAttGuid, ent, i, unknowObjectLocal);
                attsValues[id].Items.AddRange(properties);
            }

            return true;
        }

        /// <summary>
        /// by Ale
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="ent"></param>
        /// <param name="codiceAtt"></param>
        /// <param name="unknowObject"></param>
        private List<AttributiValoreProperty> AddProperty(string entityTypeKey, string codiceAttGuid, Entity ent, int indexAttCodice, System.Dynamic.ExpandoObject unknowObject)
        {
            List<AttributiValoreProperty> properties = new List<AttributiValoreProperty>();

            string attCodicePath = codiceAttGuid;
            if (attCodicePath.Any())
                attCodicePath += ReportSettingViewHelper.AttributoCodicePathSeparator;

            AttributiUtilizzati attsUtilizzati = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault();

            string codiceAtt = null;
            attsUtilizzati.AttributiPerEntityType.TryGetValue(indexAttCodice, out codiceAtt);

            if (codiceAtt == null)
                return properties;


            if (!ent.EntityType.Attributi.ContainsKey(codiceAtt))
            {
                return properties; 
            }
            //EntityAttributo entAtt = ent.Attributi[codiceAtt];
            //Attributo att = entAtt.Attributo;

            Attributo att = ent.EntityType.Attributi[codiceAtt];

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                var attRif = att as AttributoRiferimento;
                var attRifGuid = ent.EntityType.Attributi.Values.FirstOrDefault(x => x.Codice == attRif.ReferenceCodiceGuid);
                if (attRifGuid == null || attRifGuid.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    return properties;//Attributo Riferimento a Guid
                }
            }

            EntityAttributo entAtt = ent.Attributi[codiceAtt];

            string etichetta = att.Etichetta;
            string codice = att.Codice;

            if (att.IsInternal) { return properties; }

            //ricavo il valore dell'attributo
            Valore val = null;

            //int indexCodice = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().AttributiPerEntityType.IndexOf(codice);

            val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, false);

            if (val is ValoreData)
            {
                ValoreData valData = val as ValoreData;

                DateTime? data = valData.V;
                string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + attCodicePath + codice);
                AddProperty(unknowObject, codRep, data);
                properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = data });


            }
            if (val is ValoreTesto)
            {
                ValoreTesto valTesto = val as ValoreTesto;

                //MODIFICA POST MODIFICA ALE SU TESTI
                //string plainText = valTesto.V;
                string plainText = valTesto.Result;
                string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + attCodicePath + codice);
                AddProperty(unknowObject, codRep, plainText);
                properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = plainText });
            }
            if (val is ValoreFormatoNumero)
            {
                ValoreFormatoNumero valTesto = val as ValoreFormatoNumero;

                string plainText = valTesto.V;
                string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + attCodicePath + codice);
                AddProperty(unknowObject, codRep, plainText);
                properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = plainText });
            }
            if (val is ValoreContabilita)
            {
                ValoreContabilita valContabilita = val as ValoreContabilita;

                bool StampaFormula = false;
                if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault() != null)
                {
                    if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().StampaFormula != null)
                    {
                        StampaFormula = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().StampaFormula[indexAttCodice];
                    }
                }

                if (StampaFormula)
                {
                    string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice + "Formula");
                    if (string.IsNullOrEmpty(valContabilita.ResultDescription))
                    {
                        AddProperty(unknowObject, codRep, "");
                        properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = "" });
                    }
                    else
                    {
                        AddProperty(unknowObject, codRep, valContabilita.ResultDescriptionFormula);
                        properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = valContabilita.ResultDescriptionFormula });
                    }

                }
                else
                {
                    double? res = 0;
                    string format = attributoFormatHelper.GetValoreFormat(entAtt);//formato

                    if (string.IsNullOrEmpty(format))
                        format = DefaultFormatRelaeContabilità;

                    //rem by Ale 23/01/2023
                    //NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                    //NumericFormatHelper.UpdateCulture(true);
                    //string valueToSet = Nformat.SymbolText + "_" + Nformat.DecimalDigitCount + "_" + NumericFormatHelper.CurrentDecimalSeparator + "_" + NumericFormatHelper.CurrentGroupSeparator;


                    //Nformat.SymbolText = "";
                    //string Result = valContabilita.FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                    //if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                    //res = Convert.ToDouble(Result);
                    //end rem


                    if (valContabilita.RealResult != null)
                        res = (double)valContabilita.RealResult;
                    else
                        res = 0;


                    //SAREBBE DA APPLICARE NAN MA NEL REPORT APPILCO UN FORMATO AL VALORE CHE A  QUEL PUNTO NON DOVREI APPILCARE A NESSUNO DEI VALORI ANCHE SE ALCUNI SONO VALORIZZATI
                    string codRep = "V_DDC_CDlS_CGS" + JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + attCodicePath + codice);
                    AddProperty(unknowObject, codRep, format);
                    properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = format });

                    codRep = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + attCodicePath + codice);
                    AddProperty(unknowObject, codRep, res);
                    properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = res });
                }
            }
            if (val is ValoreReale)
            {
                ValoreReale valReale = val as ValoreReale;

                bool StampaFormula = false;
                if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault() != null)
                {
                    if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().StampaFormula != null)
                    {
                        //StampaFormula = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().StampaFormula[ContatoreAttibuto];
                        StampaFormula = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().StampaFormula[indexAttCodice];
                    }
                }

                if (StampaFormula)
                {
                    string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + attCodicePath + codice + "Formula");
                    if (string.IsNullOrEmpty(valReale.ResultDescription))
                    {
                        AddProperty(unknowObject, codRep, "");
                        properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = "" });
                    }
                    else
                    {
                        AddProperty(unknowObject, codRep, valReale.ResultDescriptionFormula);
                        properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = valReale.ResultDescriptionFormula });
                    }

                }
                else
                {
                    double? res = 0;
                    string format = string.Empty;


                    format = attributoFormatHelper.GetValoreFormat(entAtt);//formato

                    if (string.IsNullOrEmpty(format))
                    {
                        format = DefaultFormatRelaeContabilità;
                    }

                    //rem by Ale 23/01/2023
                    //NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                    //NumericFormatHelper.UpdateCulture(false);
                    //string valueToSet = Nformat.SymbolText + "_" + Nformat.DecimalDigitCount + "_" + NumericFormatHelper.CurrentDecimalSeparator + "_" + NumericFormatHelper.CurrentGroupSeparator;


                    //Nformat.SymbolText = "";
                    //string Result = valReale.FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                    //if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                    //res = Convert.ToDouble(Result);
                    //End rem

                    if (valReale.RealResult != null)
                        res = (double)valReale.RealResult;
                    else
                        res = 0;

                    string codRep = "V_DDC_CDlS_CGS" + JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + attCodicePath + codice);
                    AddProperty(unknowObject, codRep, format);
                    properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = format });

                    codRep = JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + attCodicePath + codice);
                    AddProperty(unknowObject, codRep, res);
                    properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = res });
                }

            }
            if (val is ValoreTestoRtf)
            {
                ValoreTestoRtf valTestoRtf = val as ValoreTestoRtf;

                string rtf = null;

                //var IndexRtf = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().AttributiPerEntityType.IndexOf(codice);

                if (indexAttCodice != -1)
                {
                    if ((AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().Rtf.ElementAt(indexAttCodice) || IsAllFieldRtfFormat) && AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().DescrfBreve.ElementAt(indexAttCodice))
                    {
                        rtf = entitiesHelper.GetRtfPreview(valTestoRtf.BriefRtf, RtfDataService);
                        rtf = ValoreHelper.AdjustRtf(rtf);
                    }

                    if ((AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().Rtf.ElementAt(indexAttCodice) || IsAllFieldRtfFormat) && !AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().DescrfBreve.ElementAt(indexAttCodice))
                    {
                        rtf = entitiesHelper.GetRtfPreview(valTestoRtf.V, RtfDataService);
                        rtf = ValoreHelper.AdjustRtf(rtf);
                    }

                    if (!AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().Rtf.ElementAt(indexAttCodice) && !IsAllFieldRtfFormat && AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().DescrfBreve.ElementAt(indexAttCodice))
                        rtf = valTestoRtf.BriefPlainText;

                    if (!AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().Rtf.ElementAt(indexAttCodice) && !IsAllFieldRtfFormat && !AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == entityTypeKey).FirstOrDefault().DescrfBreve.ElementAt(indexAttCodice))
                        rtf = valTestoRtf.PlainText;
                }

                string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + attCodicePath + codice);
                AddProperty(unknowObject, codRep, rtf);
                properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = rtf });
            }
            if (val is ValoreElenco)
            {
                ValoreElenco valTesto = val as ValoreElenco;
                string plainText = valTesto.V;

                string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + attCodicePath + codice);
                AddProperty(unknowObject, codRep, plainText);
                properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = plainText });
            }
            if (val is ValoreBooleano)
            {
                ValoreBooleano valBooleano = val as ValoreBooleano;

                //MODIFICA POST MODIFICA ALE SU TESTI
                //string plainText = valTesto.V;
                string plainText = valBooleano.PlainText;
                if (plainText == "true")
                {
                    plainText = "☑";
                }
                else
                {
                    plainText = "☐";
                }

                string codRep = JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + attCodicePath + codice);
                AddProperty(unknowObject, codRep, plainText);
                properties.Add(new AttributiValoreProperty() { PropertyName = codRep, PropertyValue = plainText });
            }

            return properties;

        }

        private void GenerateDataToManage(string SezioneItem, HashSet<Guid> filteredEntitiesIds, dynamic unknowObject = null, bool firsExecution = false)
        {
            _staticCount++;

            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[SezioneItem];

            if (EntitySelected.IsTreeMaster == false)
            {
                if (filteredEntitiesIds.Count > 0)
                {
                    Entities = DataService.GetEntitiesById(SezioneItem, filteredEntitiesIds);
                }
                else
                {
                    List<EntityMasterInfo> MasterInfo = null;
                    if (filterOperation == 0)
                    {
                        MasterInfo = DataService.GetFilteredEntities(SezioneItem, null, null, null, out entitiesFound);
                    }
                    if (filterOperation == 1)
                    {
                        entitiesFound = mainOperation.GetSelectedEntitiesId(SezioneItem);
                    }
                    if (filterOperation == 2)
                    {
                        MasterInfo = DataService.GetFilteredEntities(SezioneItem, filter, null, null, out entitiesFound);
                    }

                    Entities = DataService.GetEntitiesById(SezioneItem, entitiesFound);
                }
            }
            else
            {
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(SezioneItem, null, null, out entitiesFound);
                if (filteredEntitiesIds.Count > 0)
                {
                    TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, filteredEntitiesIds);
                }
                else
                {
                    if (filterOperation == 1)
                    {
                        entitiesFound = mainOperation.GetSelectedEntitiesId(SezioneItem);
                    }
                    if (filterOperation == 2)
                    {
                        TreeInfo = DataService.GetFilteredTreeEntities(SezioneItem, filter, null, out entitiesFound);
                    }
                    TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
                }
                TreeEntities = DataService.GetTreeEntitiesById(SezioneItem, TreeInfo.Select(item => item.Id));
            }

            int contatore = 0;

            if (firsExecution == false)
            {
                if (EntitySelected.IsTreeMaster == false)
                {
                    foreach (var E in Entities)
                    {
                        if (E.EntityId == filteredEntitiesIds.FirstOrDefault())
                        {
                            if (!DictionaryDatasourceEsternoPerOrdinamentoGuid.ContainsKey(E.EntityId))
                            {
                                DictionaryDatasourceEsternoPerOrdinamentoGuid.Add(E.EntityId, E.EntityType.GetKey());
                            }
                        }
                    }
                }
                else
                {
                    foreach (var E in TreeEntities.Where(e => e.IsParent == false))
                    {
                        if (E.EntityId == filteredEntitiesIds.FirstOrDefault())
                        {
                            if (!DictionaryDatasourceEsternoPerOrdinamentoGuid.ContainsKey(E.EntityId))
                            {
                                DictionaryDatasourceEsternoPerOrdinamentoGuid.Add(E.EntityId, E.EntityType.GetKey());
                            }
                        }
                    }
                }

            }

            dynamic UnknowObjectLocal = null;
            contatore = 0;

            //CREATE A DYMAMIC OBJECT FROM EVERY TYPE OF SUORCE BASED ON TREE OR NOT
            if (EntitySelected.IsTreeMaster)
            {
                foreach (var ent in TreeEntities)
                {
                    if (firsExecution)
                    {
                        IsTreeMaster = ent.EntityType.IsTreeMaster;
                        UnknowObjectLocal = new System.Dynamic.ExpandoObject();

                        if (IsDesignerPreview)
                        {
                            if (contatore == 5000)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        UnknowObjectLocal = unknowObject;
                    }

                    AddAttributeToExpandoObject((Entity)ent, SezioneItem, UnknowObjectLocal);
                    contatore++;
                    if (firsExecution)
                    {
                        if (!ent.IsParent)
                        {
                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == SezioneItem).FirstOrDefault() != null)
                            {
                                List<string> ListaAttributi = new List<string>();
                                if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == SezioneItem).FirstOrDefault() != null)
                                {
                                    ListaAttributi = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == SezioneItem).FirstOrDefault().AttributiPerEntityType;

                                    if (ListaAttributi.Where(Ape => Ape == BuiltInCodes.Attributo.DescrizioneRTF).FirstOrDefault() != null || IsAllFieldRtfFormat)
                                    {
                                        int IndexRtf = ListaAttributi.FindIndex(Ape => Ape == BuiltInCodes.Attributo.DescrizioneRTF);
                                        if (IndexRtf == -1)
                                        {
                                            RetrieveAndAddParentsElements(ent.EntityTypeCodice, UnknowObjectLocal, new HashSet<Guid>() { ent.EntityId }, false);
                                        }
                                        else
                                        {
                                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == SezioneItem).FirstOrDefault().Rtf != null)
                                            {
                                                RetrieveAndAddParentsElements(ent.EntityTypeCodice, UnknowObjectLocal, new HashSet<Guid>() { ent.EntityId }, AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == SezioneItem).FirstOrDefault().Rtf.ElementAt(IndexRtf));
                                            }
                                            else
                                            { RetrieveAndAddParentsElements(ent.EntityTypeCodice, UnknowObjectLocal, new HashSet<Guid>() { ent.EntityId }, false); }
                                        }
                                    }
                                    else
                                    {
                                        if (ListaAttributi.Where(Ape => Ape == BuiltInCodes.Attributo.Nome || Ape == BuiltInCodes.Attributo.Codice).FirstOrDefault() != null)
                                        {
                                            RetrieveAndAddParentsElements(ent.EntityTypeCodice, UnknowObjectLocal, new HashSet<Guid>() { ent.EntityId }, false);
                                        }
                                    }
                                }
                            }
                            AddProperty(UnknowObjectLocal, StampeKeys.ConstGruppoPadre, null);
                            UnknowDatasource.Add(UnknowObjectLocal);
                        }
                    }

                }
            }
            else
            {
                foreach (var ent in Entities)
                {
                    if (firsExecution)
                    {
                        IsTreeMaster = ent.EntityType.IsTreeMaster;
                        UnknowObjectLocal = new System.Dynamic.ExpandoObject();

                        if (IsDesignerPreview)
                        {
                            if (contatore == 5000)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        UnknowObjectLocal = unknowObject;
                    }

                    AddAttributeToExpandoObject((Entity)ent, SezioneItem, UnknowObjectLocal);
                    contatore++;
                    if (firsExecution)
                    {
                        AddProperty(UnknowObjectLocal, StampeKeys.ConstGruppoPadre, null);
                        UnknowDatasource.Add(UnknowObjectLocal);
                    }
                }
            }

            if ((TreeEntities.Count == 0 && Entities.Count == 0) && firsExecution) { IsDataSourceEmpty = true; return; }
        }

        private void RetrieveAndAddParentsElements(string sezioneItem, dynamic unknowObject, HashSet<Guid> FilteredEntitiesIds, bool IsRtf = false)
        {
            List<TreeEntity> TreeEntities = null;
            List<TreeEntityMasterInfo> TreeInfo = null;
            List<Guid> entitiesFound = null;

            EntityType EntitySelected = DataService.GetEntityTypes()[sezioneItem];

            int CounterCodice = 1;
            int CounterDescrizione = 1;

            if (!EntitySelected.IsTreeMaster) { return; }

            TreeInfo = DataService.GetFilteredTreeEntities(sezioneItem, null, null, out entitiesFound);
            TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, FilteredEntitiesIds);
            TreeEntities = DataService.GetTreeEntitiesById(sezioneItem, TreeInfo.Select(item => item.Id));

            foreach (var Entity in TreeEntities)
            {
                var AttributiParent = ((TreeEntityType)Entity.EntityType).GetParentAttributi();

                if (Entity.IsParent)
                {
                    AddingParentsElements((Entity)Entity, sezioneItem, unknowObject, AttributiParent, CounterCodice, CounterDescrizione, IsRtf);
                    CounterCodice++;
                    CounterDescrizione++;
                }
            }

        }

        private void AddingParentsElements(Entity ent, string sezioneItem, dynamic unknowObject, List<string> CodiciDaFiltrare, int counterCodice, int counterDescrizione, bool IsRtf = false)
        {
            if (ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault() == null)
            {
                ListParentItem.Add(new ParenEntity() { Guid = ent.EntityId });
                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + StampeKeys.Const_Guid + counterCodice.ToString(), ent.EntityId);
            }
            else
            {
                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + StampeKeys.Const_Guid + counterCodice.ToString(), ent.EntityId);
            }

            //foreach (EntityAttributo entAtt in ent.Attributi.Values)
            foreach (Attributo att in entitiesHelper.GetAttributi(ent).Values)
            {
                if (!CodiciDaFiltrare.Contains(att.Codice)) { continue; }

                //Attributo att = entAtt.Attributo;
                string etichetta = att.Etichetta;
                string codice = att.Codice;

                Valore val = null;

                int IndexCodice = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().AttributiPerEntityType.IndexOf(codice);
                bool IsDescrBreve = false;

                if (IndexCodice != -1)
                    IsDescrBreve = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve[IndexCodice];


                if (IsDescrBreve)
                {
                    val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, true);
                }
                else
                {
                    val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, false);
                }

                if (val is ValoreTesto)
                {
                    ValoreTesto valTesto = val as ValoreTesto;

                    //MODIFICA POST MODIFICA ALE SU TESTI
                    //string plainText = valTesto.V;
                    string plainText = valTesto.Result;
                    var expandoDict = unknowObject as IDictionary<string, object>;
                    if (att.Codice == BuiltInCodes.Attributo.Codice || att.Codice == BuiltInCodes.Attributo.Nome)
                    {
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(att.Codice) + counterCodice.ToString(), plainText);

                        ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Sezione = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey());
                        ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Numero = counterCodice.ToString();
                        if (string.IsNullOrEmpty(ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo1))
                        {
                            ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo1 = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(att.Codice) + counterCodice.ToString();
                        }
                        else
                        {
                            ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo2 = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(att.Codice) + counterCodice.ToString();
                        }
                    }
                    else if (att.Codice == BuiltInCodes.Attributo.Descrizione)
                    {
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(att.Codice) + counterCodice.ToString(), plainText);

                        ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Sezione = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey());
                        ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Numero = counterCodice.ToString();
                        if (string.IsNullOrEmpty(ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo1))
                        {
                            ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo3 = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(att.Codice) + counterCodice.ToString();
                        }
                        else
                        {
                            ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo2 = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(att.Codice) + counterCodice.ToString();
                        }

                    }

                }

                if (val is ValoreTestoRtf)
                {
                    ValoreTestoRtf valTestoRtf = val as ValoreTestoRtf;
                    string rtf = null;

                    if (IndexCodice != -1)
                    {
                        if (!UpdateRtfEseguiti.ContainsKey(ent.EntityId.ToString()))
                        {
                            if ((AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) || IsAllFieldRtfFormat) && AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = entitiesHelper.GetRtfPreview(valTestoRtf.BriefRtf, RtfDataService);

                            if ((AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) || IsAllFieldRtfFormat) && !AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = entitiesHelper.GetRtfPreview(valTestoRtf.V, RtfDataService);

                            if (!AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) && !IsAllFieldRtfFormat && AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = valTestoRtf.BriefPlainText;

                            if (!AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) && !IsAllFieldRtfFormat && !AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = valTestoRtf.PlainText;
                            UpdateRtfEseguiti.Add(ent.EntityId.ToString(), rtf);
                        }
                        else
                        {
                            rtf = UpdateRtfEseguiti[ent.EntityId.ToString()];
                        }
                    }

                    //if (IsRtf || IsAllFieldRtfFormat)
                    //{
                    //    rtf = valTestoRtf.V;
                    //    if (!UpdateRtfEseguiti.ContainsKey(ent.Id.ToString()))
                    //    {
                    //        rtf = entitiesHelper.GetRtfPreview(rtf, RtfDataService);
                    //        UpdateRtfEseguiti.Add(ent.Id.ToString(), rtf);
                    //    }
                    //    else
                    //    {
                    //        rtf = UpdateRtfEseguiti[ent.Id.ToString()];
                    //    }
                    //}
                    //else
                    //{
                    //    rtf = VerificaRtfSeGiaGenerato(valTestoRtf.V, ent.Id);
                    //}

                    var expandoDict = unknowObject as IDictionary<string, object>;
                    if (att.Codice == BuiltInCodes.Attributo.DescrizioneRTF) { AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(BuiltInCodes.Attributo.DescrizioneRTF) + counterDescrizione.ToString(), rtf); }
                    ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Sezione = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey());
                    ListParentItem.Where(p => p.Guid == ent.EntityId).FirstOrDefault().Attributo2 = JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(BuiltInCodes.Attributo.DescrizioneRTF) + counterDescrizione.ToString();
                }
            }
        }



        private void AddAttributeToExpandoObject(Entity ent, string sezioneItem, dynamic unknowObject)
        {
            //AGGIUNTO GUID CORRENTE DELL'ALBERO PER POTER VISUALIZZARE I PADRI
            if (IsTreeMaster)
            {
                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityTypeCodice) + JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityTypeCodice) + "_" + BuiltInCodes.DefinizioneAttributo.Guid, ent.EntityId.ToString());
            }

            List<string> SezioniPerRifDaEscludere = AddGuidAttribute(ent, sezioneItem, unknowObject);

            AddReferencedAttributeWithNoGuid(ent, sezioneItem, unknowObject, SezioniPerRifDaEscludere);

            AddOtherAttribute(ent, sezioneItem, unknowObject);

        }

        private List<string> AddGuidAttribute(Entity ent, string sezioneItem, dynamic unknowObject)
        {
            var ListaSenzaAttributiNull = ent.Attributi.Values.Where(f => f.Attributo != null).ToList();
            List<string> SezioniPerRifDaEscludere = new List<string>();
            foreach (EntityAttributo entAtt in ListaSenzaAttributiNull.Where(f => f.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid))
            {
                Attributo attguid = entAtt.Attributo;
                string etichetta = attguid.Etichetta;
                int myInt;
                string codice = null;
                codice = attguid.Codice;

                Valore val = null;
                val = entitiesHelper.GetValoreAttributo(ent, attguid.Codice, false, true);

                if (attguid.IsInternal) { continue; }

                if (val is ValoreGuid)
                {
                    SezioniPerRifDaEscludere.Add(attguid.GuidReferenceEntityTypeKey);

                    ValoreGuid valData = val as ValoreGuid;

                    Guid guid = valData.V;

                    if (attguid.GuidReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey + attguid.GuidReferenceEntityTypeKey + StampeKeys.Const_Guid), guid.ToString());
                    }
                    else
                    {
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey + codice), guid.ToString());
                    }

                    if (valData.V != Guid.Empty)
                    {
                        HashSet<Guid> FilteredEntitiesIds = new HashSet<Guid>();
                        FilteredEntitiesIds.Add(guid);
                        List<string> ListaAttributi = new List<string>();

                        if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault() != null)
                        {
                            ListaAttributi = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault().AttributiPerEntityType;

                            int IndexRtf = ListaAttributi.FindIndex(Ape => Ape == BuiltInCodes.Attributo.DescrizioneRTF);
                            if (IndexRtf == -1)
                            {
                                RetrieveAndAddParentsElements(attguid.GuidReferenceEntityTypeKey, unknowObject, FilteredEntitiesIds, false);
                            }
                            else
                            {
                                if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault().Rtf != null)
                                {
                                    RetrieveAndAddParentsElements(attguid.GuidReferenceEntityTypeKey, unknowObject, FilteredEntitiesIds, AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault().Rtf.ElementAt(IndexRtf));
                                }
                                else
                                { RetrieveAndAddParentsElements(attguid.GuidReferenceEntityTypeKey, unknowObject, FilteredEntitiesIds, false); }

                            }
                        }

                        if (AttributiDaEstrarrePerDatasource.Count() > 0)
                        {
                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault() != null)
                            {
                                GenerateDataToManage(attguid.GuidReferenceEntityTypeKey, FilteredEntitiesIds, unknowObject);
                            }
                        }
                    }
                    else
                    {
                        Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
                        EntityType EntitySelected = EntitiesList[attguid.GuidReferenceEntityTypeKey];
                        if (EntitySelected.IsTreeMaster)
                        {
                            if (attguid.GuidReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                            {
                                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey + BuiltInCodes.Attributo.Nome)), EmptyValue);
                            }
                            else
                            {
                                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey) + JReportHelper.ReplaceSymbolNotAllowInReport(BuiltInCodes.Attributo.Codice) + "1", EmptyValue);
                            }
                        }
                    }
                }
            }

            return SezioniPerRifDaEscludere;
        }

        private void AddReferencedAttributeWithNoGuid(Entity ent, string sezioneItem, dynamic unknowObject, List<string> sezioniPerRifDaEscludere)
        {
            //var ListaSenzaAttributiNull = ent.Attributi.Values.Where(f => f.Attributo != null).ToList();
            var ListaSenzaAttributiNull = entitiesHelper.GetAttributi(ent).Values.ToList();


            //foreach (EntityAttributo entAtt in ListaSenzaAttributiNull.Where(f => f.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento))
            foreach (Attributo att in ListaSenzaAttributiNull.Where(f => f.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento))
            {
                AttributoRiferimento attributoRiferimento = (AttributoRiferimento)att;
                Attributo attorigine = entitiesHelper.GetSourceAttributo(attributoRiferimento);
                if (sezioniPerRifDaEscludere.Contains(attorigine.EntityTypeKey))
                {
                    continue;
                }
                Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();
                if (!entTypes.ContainsKey(attributoRiferimento.ReferenceEntityTypeKey))
                {
                    continue;
                }
                EntityType entType = entTypes[attributoRiferimento.ReferenceEntityTypeKey];

                //Attributo attguid = entAtt.Attributo;
                Attributo attguid = null;
                attguid = entType.Attributi.Values.Where(f => f.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid && f.GuidReferenceEntityTypeKey == attorigine.EntityTypeKey).FirstOrDefault();
                if (attguid == null)
                    continue;
                string etichetta = attguid.Etichetta;
                int myInt;
                string codice = null;
                codice = attguid.Codice;

                Valore val = null;
                val = entitiesHelper.GetValoreAttributo(ent, attributoRiferimento.ReferenceCodiceGuid, false, true);
                Entity entity = DataService.GetEntityById(attributoRiferimento.ReferenceEntityTypeKey, ((ValoreGuid)val).V);
                val = entitiesHelper.GetValoreAttributo(entity, attguid.Codice, false, true);

                if (attguid.IsInternal) { continue; }

                if (val is ValoreGuid)
                {
                    ValoreGuid valData = val as ValoreGuid;

                    Guid guid = valData.V;

                    if (attguid.GuidReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey + attguid.GuidReferenceEntityTypeKey + StampeKeys.Const_Guid), guid.ToString());
                    }
                    else
                    {
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey + codice), guid.ToString());
                    }

                    if (valData.V != Guid.Empty)
                    {
                        HashSet<Guid> FilteredEntitiesIds = new HashSet<Guid>();
                        FilteredEntitiesIds.Add(guid);
                        List<string> ListaAttributi = new List<string>();

                        if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault() != null)
                        {
                            ListaAttributi = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault().AttributiPerEntityType;

                            int IndexRtf = ListaAttributi.FindIndex(Ape => Ape == BuiltInCodes.Attributo.DescrizioneRTF);
                            if (IndexRtf == -1)
                            {
                                RetrieveAndAddParentsElements(attguid.GuidReferenceEntityTypeKey, unknowObject, FilteredEntitiesIds, false);
                            }
                            else
                            {
                                if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault().Rtf != null)
                                {
                                    RetrieveAndAddParentsElements(attguid.GuidReferenceEntityTypeKey, unknowObject, FilteredEntitiesIds, AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault().Rtf.ElementAt(IndexRtf));
                                }
                                else
                                { RetrieveAndAddParentsElements(attguid.GuidReferenceEntityTypeKey, unknowObject, FilteredEntitiesIds, false); }

                            }
                        }

                        if (AttributiDaEstrarrePerDatasource.Count() > 0)
                        {
                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == attguid.GuidReferenceEntityTypeKey).FirstOrDefault() != null)
                            {
                                GenerateDataToManage(attguid.GuidReferenceEntityTypeKey, FilteredEntitiesIds, unknowObject);
                            }
                        }
                    }
                    else
                    {
                        Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
                        EntityType EntitySelected = EntitiesList[attguid.GuidReferenceEntityTypeKey];
                        if (EntitySelected.IsTreeMaster)
                        {
                            if (attguid.GuidReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                            {
                                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey + BuiltInCodes.Attributo.Nome)), EmptyValue);
                            }
                            else
                            {
                                AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(attguid.GuidReferenceEntityTypeKey) + JReportHelper.ReplaceSymbolNotAllowInReport(BuiltInCodes.Attributo.Codice) + "1", EmptyValue);
                            }
                        }
                    }
                }
            }
        }

        private void AddOtherAttribute(Entity ent, string sezioneItem, dynamic unknowObject)
        {
            int ContatoreAttibuto = 0;

            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault() != null)
            {
                foreach (var item in AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().AttributiPerEntityType)
                {
                    if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault() != null)
                    {
                        if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().PropertyType != null)
                        {
                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().PropertyType[ContatoreAttibuto] == BuiltInCodes.DefinizioneAttributo.Guid)
                            {
                                ContatoreAttibuto++;
                                continue;
                            }
                        }
                    }


                    Attributo att = entitiesHelper.GetAttributo(ent, item);
                    if (att == null)
                    {
                        continue;
                    }

                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    {
                        continue;
                    }

                    //if (!ent.Attributi.ContainsKey(item))
                    //{
                    //    continue;
                    //}
                    //EntityAttributo entAtt = ent.Attributi[item];
                    //Attributo att = entAtt.Attributo;

                    //if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    //{
                    //    continue;
                    //}

                    string etichetta = att.Etichetta;
                    string codice = att.Codice;

                    if (att.IsInternal) { continue; }

                    //ricavo il valore dell'attributo
                    Valore val = null;

                    int IndexCodice = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().AttributiPerEntityType.IndexOf(codice);

                    val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, false);

                    if (val is ValoreData)
                    {
                        ValoreData valData = val as ValoreData;

                        DateTime? data = valData.V;
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice), data);
                    }
                    if (val is ValoreTesto)
                    {
                        ValoreTesto valTesto = val as ValoreTesto;

                        //MODIFICA POST MODIFICA ALE SU TESTI
                        //string plainText = valTesto.V;
                        string plainText = valTesto.Result;
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice), plainText);
                    }
                    if (val is ValoreFormatoNumero)
                    {
                        ValoreFormatoNumero valTesto = val as ValoreFormatoNumero;

                        string plainText = valTesto.V;
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice), plainText);
                    }
                    if (val is ValoreContabilita)
                    {
                        ValoreContabilita valContabilita = val as ValoreContabilita;

                        bool StampaFormula = false;
                        if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault() != null)
                        {
                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().StampaFormula != null)
                            {
                                StampaFormula = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().StampaFormula[ContatoreAttibuto];
                            }
                        }

                        if (StampaFormula)
                        {
                            if (string.IsNullOrEmpty(valContabilita.ResultDescription))
                            { AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice + "Formula"), ""); }
                            else
                            { AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice + "Formula"), valContabilita.ResultDescriptionFormula); }

                        }
                        else
                        {
                            EntityAttributo entAtt = ent.Attributi[item];
                            double? res = 0;
                            string format = attributoFormatHelper.GetValoreFormat(entAtt);//formato

                            if (string.IsNullOrEmpty(format))
                                format = DefaultFormatRelaeContabilità;

                            //rem by Ale 23/01/2023
                            //NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                            //NumericFormatHelper.UpdateCulture(true);
                            //string valueToSet = Nformat.SymbolText + "_" + Nformat.DecimalDigitCount + "_" + NumericFormatHelper.CurrentDecimalSeparator + "_" + NumericFormatHelper.CurrentGroupSeparator;


                            //Nformat.SymbolText = "";
                            //string Result = valContabilita.FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                            //if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                            //res = Convert.ToDouble(Result);
                            //end rem


                            if (valContabilita.RealResult != null)
                                res = (double)valContabilita.RealResult;
                            else
                                res = 0;


                            //SAREBBE DA APPLICARE NAN MA NEL REPORT APPILCO UN FORMATO AL VALORE CHE A  QUEL PUNTO NON DOVREI APPILCARE A NESSUNO DEI VALORI ANCHE SE ALCUNI SONO VALORIZZATI

                            AddProperty(unknowObject, "V_DDC_CDlS_CGS" + JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + codice), format);
                            AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey()) + JReportHelper.ReplaceSymbolNotAllowInReport(codice), res);
                        }
                    }
                    if (val is ValoreReale)
                    {
                        ValoreReale valReale = val as ValoreReale;

                        bool StampaFormula = false;
                        if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault() != null)
                        {
                            if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().StampaFormula != null)
                            {
                                StampaFormula = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().StampaFormula[ContatoreAttibuto];
                            }
                        }

                        if (StampaFormula)
                        {
                            if (string.IsNullOrEmpty(valReale.ResultDescription))
                            { AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice + "Formula"), ""); }
                            else
                            { AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice + "Formula"), valReale.ResultDescriptionFormula); }

                        }
                        else
                        {
                            EntityAttributo entAtt = ent.Attributi[item];
                            double? res = 0;
                            string format = string.Empty;


                            format = attributoFormatHelper.GetValoreFormat(entAtt);//formato
                                
                            if (string.IsNullOrEmpty(format))
                            {
                                format = DefaultFormatRelaeContabilità;
                            }

                            //rem by Ale 23/01/2023
                            //NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                            //NumericFormatHelper.UpdateCulture(false);
                            //string valueToSet = Nformat.SymbolText + "_" + Nformat.DecimalDigitCount + "_" + NumericFormatHelper.CurrentDecimalSeparator + "_" + NumericFormatHelper.CurrentGroupSeparator;


                            //Nformat.SymbolText = "";
                            //string Result = valReale.FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                            //if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                            //res = Convert.ToDouble(Result);
                            //End rem

                            if (valReale.RealResult != null)
                                res = (double)valReale.RealResult;
                            else
                                res = 0;


                            AddProperty(unknowObject, "V_DDC_CDlS_CGS" + JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + codice), format);
                            AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(entAtt.Entity.EntityType.GetKey() + codice), res);
                        }

                    }
                    if (val is ValoreTestoRtf)
                    {
                        ValoreTestoRtf valTestoRtf = val as ValoreTestoRtf;

                        string rtf = null;

                        //var IndexRtf = AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().AttributiPerEntityType.IndexOf(codice);

                        if (IndexCodice != -1)
                        {
                            if ((AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) || IsAllFieldRtfFormat) && AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = entitiesHelper.GetRtfPreview(valTestoRtf.BriefRtf, RtfDataService);

                            if ((AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) || IsAllFieldRtfFormat) && !AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = entitiesHelper.GetRtfPreview(valTestoRtf.V, RtfDataService);

                            if (!AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) && !IsAllFieldRtfFormat && AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = valTestoRtf.BriefPlainText;

                            if (!AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexCodice) && !IsAllFieldRtfFormat && !AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().DescrfBreve.ElementAt(IndexCodice))
                                rtf = valTestoRtf.PlainText;


                            //if (!UpdateRtfEseguiti.ContainsKey(ent.Id.ToString()))
                            //{
                            //    //rtf = entitiesHelper.GetRtfPreview(rtf, RtfDataService);
                            //    UpdateRtfEseguiti.Add(ent.Id.ToString(), rtf);
                            //}
                            //else
                            //{
                            //    rtf = UpdateRtfEseguiti[ent.Id.ToString()];
                            //}
                        }



                        //if (IndexRtf != -1)
                        //{
                        //    if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf != null)
                        //    {
                        //        if (AttributiDaEstrarrePerDatasource.Where(Ade => Ade.EntityType == sezioneItem).FirstOrDefault().Rtf.ElementAt(IndexRtf) || IsAllFieldRtfFormat)
                        //        {
                        //            rtf = valTestoRtf.V;
                        //            if (!UpdateRtfEseguiti.ContainsKey(ent.Id.ToString()))
                        //            {
                        //                rtf = entitiesHelper.GetRtfPreview(rtf, RtfDataService);
                        //                UpdateRtfEseguiti.Add(ent.Id.ToString(), rtf);
                        //            }
                        //            else
                        //            {
                        //                rtf = UpdateRtfEseguiti[ent.Id.ToString()];
                        //            }
                        //        }
                        //    //    else
                        //    //    {
                        //    //        if (!UpdateRtfEseguiti.ContainsKey(ent.Id.ToString()))
                        //    //        {
                        //    //            rtf = VerificaRtfSeGiaGenerato(valTestoRtf.V, ent.Id);
                        //    //        }
                        //    //        else
                        //    //        {
                        //    //            rtf = UpdateRtfEseguiti[ent.Id.ToString()];
                        //    //        }
                        //    //    }
                        //    }
                        //    //else
                        //    //{
                        //    //    if (!UpdateRtfEseguiti.ContainsKey(ent.Id.ToString()))
                        //    //    {
                        //    //        rtf = VerificaRtfSeGiaGenerato(valTestoRtf.V, ent.Id);
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        rtf = UpdateRtfEseguiti[ent.Id.ToString()];
                        //    //    }
                        //    //}
                        //}
                        //else
                        //{
                        //    if (!UpdateRtfEseguiti.ContainsKey(ent.Id.ToString()))
                        //    {
                        //        rtf = VerificaRtfSeGiaGenerato(valTestoRtf.V, ent.Id);
                        //    }
                        //    else
                        //    {
                        //        rtf = UpdateRtfEseguiti[ent.Id.ToString()];
                        //    }
                        //}

                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice), rtf);
                    }
                    if (val is ValoreElenco)
                    {
                        ValoreElenco valTesto = val as ValoreElenco;
                        string plainText = valTesto.V;
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice), plainText);
                    }
                    if (val is ValoreBooleano)
                    {
                        ValoreBooleano valBooleano = val as ValoreBooleano;

                        //MODIFICA POST MODIFICA ALE SU TESTI
                        //string plainText = valTesto.V;
                        string plainText = valBooleano.PlainText;
                        if (plainText == "true")
                        {
                            plainText = "☑";
                        }
                        else
                        {
                            plainText = "☐";
                        }
                        AddProperty(unknowObject, JReportHelper.ReplaceSymbolNotAllowInReport(ent.EntityType.GetKey() + codice), plainText);
                    }
                    ContatoreAttibuto++;
                }
            }
        }

        private string VerificaRtfSeGiaGenerato(string valTestoRtf, Guid Id)
        {
            string rtf = null;
            if (!UpdateRtfEseguiti.ContainsKey(Id.ToString()))
            {
                rtf = ConvertRtfWithField(valTestoRtf, Id);
            }
            else
            {
                rtf = UpdateRtfEseguiti[Id.ToString()];
            }

            return rtf;
        }
        private string ConvertRtfWithField(string valTestoRtf, Guid Id)
        {
            string rtf = valTestoRtf;
            rtf = entitiesHelper.GetRtfPreview(rtf, RtfDataService);
            string plainText = string.Empty;
            ValoreHelper.RtfToPlainString(rtf, out plainText);
            rtf = plainText;
            UpdateRtfEseguiti.Add(Id.ToString(), rtf);
            return rtf;
        }
        private void AddProperty(System.Dynamic.ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }


        public DataSet RetrieveDataSource(string sezioneItem, string DataBaseName)
        {
            //string jsonPre = Newtonsoft.Json.JsonConvert.SerializeObject(UnknowDatasource);
            //System.IO.File.WriteAllText(@"C:\Users\alberto.DIGICORP2\Desktop\Report\Json41000.json", jsonPre);
            RaggruppamentiDatasourceToRemove = new List<Raggruppamento>();

            if (UnknowDatasource.Count() > 0)
            {
                DataTable = JsonConvert.DeserializeObject<DataTable>(Newtonsoft.Json.JsonConvert.SerializeObject(UnknowDatasource));
            }
            else
            {
                DataTable = new DataTable();
                DataColumn column = new DataColumn();
                column.DataType = System.Type.GetType("System.Int32");
                column.ColumnName = StampeKeys.ConstRTFColumnId;
                DataTable.Columns.Add(column);
                DataRow row = DataTable.NewRow();
                DataTable.Rows.Add(row);
            }

            if (UnknowDatasource.Count() > 0)
            {
                //Rev by Ale 15/02/2023
                //OrderDataSource();
                OrderDataSource2();
                //End Rev
            }

            //Azzeramento stringa json
            UnknowDatasource.Clear();

            int contatore = 0;
            // AGGIUGO COLONNE CON DEFAULT PRESENTI NEL WIZARD MA NON NEL DATASOURCE
            if (AttributiDaEstrarrePerDatasource != null)
            {
                foreach (var Attributo in AttributiDaEstrarrePerDatasource)
                {
                    contatore = 0;

                    foreach (var SottoAttributo in Attributo.AttributiPerEntityType)
                    {
                        DataColumn Colonna = null;
                        string columnName = string.Empty;


                        if (DeveloperVariables.IsNewStampa)
                        {

                            if (Attributo.AttributiCodicePathPerEntityType != null && contatore < Attributo.AttributiCodicePathPerEntityType.Count)
                                columnName = JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.AttributiCodicePathPerEntityType[contatore]);
                            else
                                columnName = JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + JReportHelper.ReplaceSymbolNotAllowInReport(SottoAttributo);

                            Colonna = DataTable.Columns[columnName];

                            if (Colonna == null)
                            {
                                //Colonna = new DataColumn();
                                //Colonna.DataType = System.Type.GetType("System.String");
                                //Colonna.ColumnName = columnName;
                                //DataTable.Columns.Add(Colonna);
                            }

                        }
                        else
                        {
                            if (Attributo.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                            {
                                if (Attributo.PropertyType != null)
                                {
                                    if (Attributo.PropertyType[contatore] == BuiltInCodes.DefinizioneAttributo.Guid)
                                    {
                                        Colonna = DataTable.Columns[JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + StampeKeys.Const_Guid];
                                    }
                                    else
                                    {
                                        Colonna = DataTable.Columns[JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + JReportHelper.ReplaceSymbolNotAllowInReport(SottoAttributo)];
                                    }
                                }
                                else
                                {
                                    Colonna = DataTable.Columns[JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + JReportHelper.ReplaceSymbolNotAllowInReport(SottoAttributo)];
                                }
                            }
                            else
                            {
                                Colonna = DataTable.Columns[JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + JReportHelper.ReplaceSymbolNotAllowInReport(SottoAttributo)];
                            }
                        }


                        //SE HO UN RAGGRUPPAMENTO A NULL ON GLI FACCIO FARE IL GRUPPO PER EVITARE I SOMMANO
                        if (!Attributo.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                        {
                            if (RaggruppamentiDatasource.Where(r => r.AttributoCodice == SottoAttributo).FirstOrDefault() != null && Colonna == null)
                            {
                                RaggruppamentiDatasourceToRemove.Add(RaggruppamentiDatasource.Where(r => r.AttributoCodice == SottoAttributo).FirstOrDefault());
                                continue;
                            }
                        }
                        else
                        {
                            if (RaggruppamentiDatasource.Where(r => r.EntityType == Attributo.EntityType && r.AttributoCodice == SottoAttributo).FirstOrDefault() != null)
                            {
                                if (Colonna == null || Colonna != null && !DataTable.AsEnumerable().Any(tt => tt.Field<string>(Colonna.ColumnName) != Guid.Empty.ToString()))
                                {
                                    if (Attributo.PropertyType != null)
                                    {
                                        RaggruppamentiDatasourceToRemove.Add(RaggruppamentiDatasource.Where(r => r.EntityType == Attributo.EntityType && r.AttributoCodice == SottoAttributo && Attributo.PropertyType[contatore] == r.PropertyType).FirstOrDefault());
                                        continue;
                                    }
                                }
                            }
                        }


                        if (DeveloperVariables.IsNewStampa)
                        {
                            if (Colonna == null)
                            {
                                Colonna = new DataColumn();
                                Colonna.DataType = System.Type.GetType("System.String");
                                Colonna.ColumnName = columnName;
                                DataTable.Columns.Add(Colonna);
                            }
                        }
                        else
                        {

                            if (Colonna == null)
                            {
                                DataColumn column = new DataColumn();
                                column.DataType = System.Type.GetType("System.String");
                                column.ColumnName = JReportHelper.ReplaceSymbolNotAllowInReport(Attributo.EntityType) + SottoAttributo;
                                DataTable.Columns.Add(column);
                            }
                        }
                        contatore++;
                    }
                }

            }

            //COMPILO ATTRIBUTI SEMPLICI VUOTI CON VALORE DEFAULT SE PRESENTI IN RAGGRUPPAMENTO
            if (RaggruppamentiDatasource != null)
            {
                foreach (var Raggruppamento in RaggruppamentiDatasource.Where(r => r.PropertyType != BuiltInCodes.DefinizioneAttributo.Guid))
                {
                    string NomeColonna = Raggruppamento.EntityType + Raggruppamento.Attributo;
                    if (DataTable.Columns.Contains(NomeColonna))
                    {
                        foreach (DataRow dr in DataTable.Rows)
                        {
                            if (Raggruppamento.PropertyType == BuiltInCodes.DefinizioneAttributo.Testo)
                            {
                                string Valore = dr[NomeColonna].ToString();
                                if (string.IsNullOrEmpty(Valore))
                                {
                                    dr[Raggruppamento.EntityType + Raggruppamento.Attributo] = EmptyValue;
                                }
                            }
                        }
                    }
                }
            }

            DataTable.TableName = DataBaseName;
            DataSet _Dataset = new DataSet(DataBaseName);
            _Dataset.Tables.Add(DataTable);

            _Dataset.AcceptChanges();

            //AU temp 29/06/2023
            
            //string json = JsonConvert.SerializeObject(_Dataset, Formatting.Indented);
            //string filePath = string.Format("{0}{1}.json", "D:\\Temp\\", DataBaseName);

            //System.IO.File.WriteAllText(filePath, json);
            //


            AttributiDaEstrarrePerDatasource = null;

            return _Dataset;
        }

        public void OrderDataSource()
        {
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            List<DatasourceEsternoPerOrdinamento> DatasourceEsternoPerOrdinamentoOrdinata = new List<DatasourceEsternoPerOrdinamento>();

            int contatore = 1;
            bool Ordinare = true;

            foreach (var raggruppamento in RaggruppamentiDatasource)
            {
                if (!String.IsNullOrEmpty(raggruppamento.Attributo))
                {
                    Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
                    EntityType EntitySelected = null;
                    if (EntitiesList.ContainsKey(raggruppamento.EntityType))
                    {
                        EntitySelected = EntitiesList[raggruppamento.EntityType];
                    }
                    else
                    {
                        continue;
                    }

                    if (EntitySelected.IsTreeMaster == false)
                    {
                        List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(raggruppamento.EntityType, null, null, null, out entitiesFound);
                        Entities = DataService.GetEntitiesById(raggruppamento.EntityType, DictionaryDatasourceEsternoPerOrdinamentoGuid.Where(o => o.Value == raggruppamento.EntityType).Select(o => o.Key));
                    }
                    else
                    {
                        List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(raggruppamento.EntityType, null, null, out entitiesFound);
                        TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, DictionaryDatasourceEsternoPerOrdinamentoGuid.Where(o => o.Value == raggruppamento.EntityType).Select(o => o.Key));
                        TreeEntities = DataService.GetTreeEntitiesById(raggruppamento.EntityType, TreeInfo.Select(item => item.Id));
                    }

                    contatore = 0;

                    if (EntitySelected.IsTreeMaster == false)
                    {
                        bool SimplePropertInserted = false;
                        //RAGGRUPPAMENTO PER ATTRIBUTO SEMPLICE
                        if (!raggruppamento.EntityType.Contains(BuiltInCodes.EntityType.Divisione))
                        {
                            if (!raggruppamento.AttributoCodice.Contains(StampeKeys.ConstGuid))
                            {
                                AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento() { Codice = raggruppamento.AttributoCodice, Index = contatore, IsOrdindeCrescente = raggruppamento.IsOrdineCrescente, IsOrdindeDecrescente = raggruppamento.IsOrdineDecrescente };
                                if (raggruppamento.EntitaAttributoOrdinamento != null)
                                {
                                    attributoPerOrdinamento.CodiceOrdinamento = raggruppamento.EntitaAttributoOrdinamento.AttributoCodice;
                                }

                                if (DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == raggruppamento.EntityType).FirstOrDefault() == null)
                                {
                                    DatasourceEsternoPerOrdinamentoOrdinata.Add(new DatasourceEsternoPerOrdinamento() { Sezione = raggruppamento.EntityType });
                                    DatasourceEsternoPerOrdinamentoOrdinata.LastOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                }
                                else
                                {
                                    DatasourceEsternoPerOrdinamentoOrdinata.Where(s => s.Sezione == raggruppamento.EntityType).FirstOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                }
                                contatore++;
                                SimplePropertInserted = true;
                            }
                        }

                        if (!SimplePropertInserted)
                        {
                            //RAGGRUPPAMENTO PER GUID
                            foreach (var E in Entities)
                            {
                                if (DictionaryDatasourceEsternoPerOrdinamentoGuid.ContainsKey(E.EntityId))
                                {
                                    AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento() { Guid = E.EntityId, Index = contatore, Codice = raggruppamento.AttributoCodice, IsOrdindeCrescente = raggruppamento.IsOrdineCrescente, IsOrdindeDecrescente = raggruppamento.IsOrdineDecrescente };
                                    if (raggruppamento.EntitaAttributoOrdinamento != null)
                                    {
                                        attributoPerOrdinamento.CodiceOrdinamento = raggruppamento.EntitaAttributoOrdinamento.AttributoCodice;
                                    }

                                    if (DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == raggruppamento.EntityType).FirstOrDefault() == null)
                                    {
                                        DatasourceEsternoPerOrdinamentoOrdinata.Add(new DatasourceEsternoPerOrdinamento() { Sezione = raggruppamento.EntityType });
                                        DatasourceEsternoPerOrdinamentoOrdinata.LastOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                        contatore++;
                                    }
                                    else
                                    {
                                        DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == raggruppamento.EntityType).FirstOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                        contatore++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        bool SimplePropertInserted = false;
                        //RAGGRUPPAMENTO PER ATTRIBUTO SEMPLICE
                        if (!raggruppamento.EntityType.Contains(BuiltInCodes.EntityType.Divisione))
                        {
                            if (!raggruppamento.AttributoCodice.Contains(StampeKeys.ConstGuid))
                            {
                                AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento() { Codice = raggruppamento.AttributoCodice, Index = contatore, IsOrdindeCrescente = raggruppamento.IsOrdineCrescente, IsOrdindeDecrescente = raggruppamento.IsOrdineDecrescente };
                                if (raggruppamento.EntitaAttributoOrdinamento != null)
                                {
                                    attributoPerOrdinamento.CodiceOrdinamento = raggruppamento.EntitaAttributoOrdinamento.AttributoCodice;
                                }

                                if (DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == raggruppamento.EntityType).FirstOrDefault() == null)
                                {
                                    contatore++;
                                    DatasourceEsternoPerOrdinamentoOrdinata.Add(new DatasourceEsternoPerOrdinamento() { Sezione = raggruppamento.EntityType });
                                    DatasourceEsternoPerOrdinamentoOrdinata.LastOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                }
                                else
                                {
                                    contatore++;
                                    DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == raggruppamento.EntityType).FirstOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                }
                                contatore++;
                                SimplePropertInserted = true;
                            }
                        }

                        if (!SimplePropertInserted)
                        {
                            //RAGGRUPPAMENTO PER GUID
                            foreach (var E in TreeEntities.Where(e => e.IsParent == false))
                            {
                                string sezioneElaborata = null;
                                string sezioneNonElaborata = null;
                                bool isGuid = false;
                                if (raggruppamento.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                                {
                                    if (raggruppamento.PropertyType == BuiltInCodes.DefinizioneAttributo.Guid)
                                    {
                                        sezioneElaborata = raggruppamento.EntityType + raggruppamento.EntityType + StampeKeys.Const_Guid;
                                        sezioneNonElaborata = raggruppamento.EntityType;
                                        isGuid = true;
                                    }
                                    else
                                    {
                                        sezioneElaborata = raggruppamento.EntityType + raggruppamento.AttributoCodice;
                                        sezioneNonElaborata = raggruppamento.EntityType;
                                        isGuid = false;
                                    }
                                }
                                else { sezioneElaborata = raggruppamento.EntityType; }

                                AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento() { Guid = E.EntityId, Index = contatore, Codice = raggruppamento.AttributoCodice, IsOrdindeCrescente = raggruppamento.IsOrdineCrescente, IsOrdindeDecrescente = raggruppamento.IsOrdineDecrescente };
                                if (raggruppamento.EntitaAttributoOrdinamento != null)
                                {
                                    attributoPerOrdinamento.CodiceOrdinamento = raggruppamento.EntitaAttributoOrdinamento.AttributoCodice;
                                }

                                if (DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == sezioneElaborata).FirstOrDefault() == null)
                                {
                                    contatore++;
                                    DatasourceEsternoPerOrdinamentoOrdinata.Add(new DatasourceEsternoPerOrdinamento() { Sezione = sezioneElaborata, SezioneNonElaborata = sezioneNonElaborata, IsGuid = isGuid });
                                    DatasourceEsternoPerOrdinamentoOrdinata.LastOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);
                                }
                                else
                                {
                                    contatore++;
                                    DatasourceEsternoPerOrdinamentoOrdinata.Where(l => l.Sezione == sezioneElaborata).FirstOrDefault().AttributoPerOrdinamento.Add(attributoPerOrdinamento);

                                }
                            }
                        }
                    }
                }
                else
                {
                    Ordinare = false;
                }
            }

            //Add order for documento (corpo)
            if (OrdinamentoCorpo != null)
            {
                if (OrdinamentoCorpo.IsOrdinamentoCrescente || OrdinamentoCorpo.IsOrdinamentoDecrescente)
                {
                    string sezioneElaborata = null;
                    string sezioneNonElaborata = null;

                    if (OrdinamentoCorpo.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        if (OrdinamentoCorpo.PropertyType == BuiltInCodes.DefinizioneAttributo.Guid)
                        {
                            sezioneElaborata = OrdinamentoCorpo.EntityType + OrdinamentoCorpo.EntityType + StampeKeys.Const_Guid;
                            sezioneNonElaborata = OrdinamentoCorpo.EntityType;
                        }
                        else
                        {
                            sezioneElaborata = OrdinamentoCorpo.EntityType + OrdinamentoCorpo.AttributoCodice;
                            sezioneNonElaborata = OrdinamentoCorpo.EntityType;
                        }
                    }
                    else { sezioneElaborata = OrdinamentoCorpo.EntityType; }

                    DatasourceEsternoPerOrdinamento datasourceEsternoPerOrdinamento = new DatasourceEsternoPerOrdinamento();
                    datasourceEsternoPerOrdinamento.Sezione = sezioneElaborata;
                    datasourceEsternoPerOrdinamento.SezioneNonElaborata = sezioneNonElaborata;
                    datasourceEsternoPerOrdinamento.AttributoPerOrdinamento = new List<AttributoPerOrdinamento>();
                    AttributoPerOrdinamento AttributoPerOrdinamento = new AttributoPerOrdinamento();
                    AttributoPerOrdinamento.IsOrdindeCrescente = OrdinamentoCorpo.IsOrdinamentoCrescente;
                    AttributoPerOrdinamento.IsOrdindeDecrescente = OrdinamentoCorpo.IsOrdinamentoDecrescente;
                    AttributoPerOrdinamento.Codice = OrdinamentoCorpo.AttributoCodice;
                    AttributoPerOrdinamento.CodiceOrdinamento = OrdinamentoCorpo.AttributoCodice;
                    datasourceEsternoPerOrdinamento.AttributoPerOrdinamento.Add(AttributoPerOrdinamento);
                    DatasourceEsternoPerOrdinamentoOrdinata.Add(datasourceEsternoPerOrdinamento);
                }
            }

            // ADD EMPTY GUID IN ORDER TO ORDER ALSO THIS VALUE WHEN I AHVE AN EMPTY GUID
            foreach (DatasourceEsternoPerOrdinamento O in DatasourceEsternoPerOrdinamentoOrdinata)
            {
                if (O.Sezione.Contains(BuiltInCodes.EntityType.Divisione) && O.Sezione.Contains(StampeKeys.Const_Guid) && O.AttributoPerOrdinamento.Count() > 0)
                {
                    AttributoPerOrdinamento AttributoPerOrdinamento = new AttributoPerOrdinamento();
                    AttributoPerOrdinamento.Guid = Guid.Empty;
                    AttributoPerOrdinamento.Codice = O.AttributoPerOrdinamento.FirstOrDefault().Codice;
                    AttributoPerOrdinamento.Index = O.AttributoPerOrdinamento.Max(r => r.Index) + 1;
                    O.AttributoPerOrdinamento.Add(AttributoPerOrdinamento);
                }
                //else
                //{
                //    if (O.AttributoPerOrdinamento.Count() > 0)
                //    {
                //        if (true)
                //        {

                //        }

                //    }
                //}
            }

            if (Ordinare)
            {
                OrdinaDataset(DatasourceEsternoPerOrdinamentoOrdinata);
            }

        }

        public void OrderDataSource2()
        {
            if (!RaggruppamentiDatasource.Any())
                return;
            
            
            OrderedEnumerableRowCollection<DataRow> rows = null;

            bool firstRaggr = true;

            foreach (var raggruppamento in RaggruppamentiDatasource)
            {
                var comparer = new DataSourceComparer();

                string field = string.Empty;
                if (raggruppamento.EntitaAttributoOrdinamento.AttributoPerReport != null)
                {
                    string entityType = raggruppamento.EntityType;
                    string entityType_ = entityType.Replace("-", "_");

                    if (DeveloperVariables.IsNewStampa)
                    {
                        field = string.Format("{0}", JReportHelper.ReplaceSymbolNotAllowInReport(raggruppamento.EntitaAttributoOrdinamento.AttributoPerReport));
                    }
                    else
                        field = string.Format("{0}{1}", entityType_, raggruppamento.EntitaAttributoOrdinamento.AttributoCodice);

                    comparer.Clear();
                }
                else
                {
                    string entityType = raggruppamento.EntityType;
                    string entityType_ = entityType.Replace("-", "_");

                    if (DeveloperVariables.IsNewStampa)
                        field = string.Format("{0}", JReportHelper.ReplaceSymbolNotAllowInReport(raggruppamento.AttributoPerReport));
                    else
                        field = string.Format("{0}{1}_{2}", entityType_, entityType_, "Guid");

                    List<Guid> entsFound = null;
                    DataService.GetFilteredEntities(entityType, null, null, null, out entsFound);

                    Dictionary<Guid, int> guidsIndexes = new Dictionary<Guid, int>();

                    guidsIndexes.Add(Guid.Empty, 0);

                    for (int i = 0; i< entsFound.Count; i++)
                        guidsIndexes.Add(entsFound[i], i+1);

                    comparer.SetOrderedIds(guidsIndexes);

                }
               
                if (!DataTable.Columns.Contains(field))
                {
                    //colonna non trovata
                    continue;
                }

                if (firstRaggr)
                {
                    firstRaggr = false;

                    if (raggruppamento.IsOrdineDecrescente)
                        rows = DataTable.AsEnumerable().OrderByDescending(r => r[field].ToString(), comparer);
                    else
                        rows = DataTable.AsEnumerable().OrderBy(r => r[field].ToString(), comparer);

                    //if (raggruppamento.IsOrdineDecrescente)
                    //    rows = DataTable.AsEnumerable().OrderByDescending(r => r.Field<string>(field), comparer);
                    //else
                    //    rows = DataTable.AsEnumerable().OrderBy(r => r.Field<string>(field), comparer);

                }
                else
                {
                    if (raggruppamento.IsOrdineDecrescente)
                        rows = rows.ThenByDescending(r => r[field].ToString(), comparer);
                    else
                        rows = rows.ThenBy(r => r[field].ToString(), comparer);

                    //if (raggruppamento.IsOrdineDecrescente)
                    //    rows = rows.ThenByDescending(r => r.Field<string>(field), comparer);
                    //else
                    //    rows = rows.ThenBy(r => r.Field<string>(field), comparer);
                }
            }

            if (rows != null)
                DataTable = rows.CopyToDataTable();
        }

        private void OrdinaDataset(List<DatasourceEsternoPerOrdinamento> ListaEntitaPerAttributi)
        {
            List<DataRow> ListaRigheDacancellare = new List<DataRow>();

            List<FiltriPrecedenti> filtriPecedenti = new List<FiltriPrecedenti>();
            filtriPecedenti.Add(new FiltriPrecedenti());
            filtriPecedenti.LastOrDefault().ListaFiltri = new List<string>();

            List<FiltriPrecedenti> filtriDefinitivi = new List<FiltriPrecedenti>();

            int Contatore = 0;

            int ContatoreSezione = 0;
            int ContatoreAttributo = 0;
            string AndOperator = " and ";
            List<string> listaFiltri = new List<string>();
            bool IsAnnidateGuidOrder = false;
            List<string> ListaDaRiordinare = new List<string>();
            List<string> CodiciOrdinamento = new List<string>();

            foreach (var EntitaPerAttributi in ListaEntitaPerAttributi)
            {
                Contatore = 0;


                if (ContatoreSezione != 0)
                {
                    CreazioneFiltri(EntitaPerAttributi, ContatoreSezione, filtriPecedenti);
                }
                else
                {
                    foreach (var Attributo in EntitaPerAttributi.AttributoPerOrdinamento)
                    {
                        DataRow[] Row = null;
                        bool Salta = false;

                        string SelectionSintax = null;


                        try
                        {
                            if (EntitaPerAttributi.Sezione.Contains(BuiltInCodes.EntityType.Divisione))
                            {
                                SelectionSintax = JReportHelper.ReplaceSymbolNotAllowInReport(SelectionSintax + EntitaPerAttributi.Sezione) + " = '" + Attributo.Guid.ToString() + "'";
                                if (Attributo.IsOrdindeCrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                {
                                    AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                    attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                    foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.SezioneNonElaborata, attributoPerOrdinamento, true))
                                    {
                                        if (!EntitaPerAttributi.IsGuid)
                                            ListaDaRiordinare.Add(Espressione);
                                        else
                                            ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);

                                        CodiciOrdinamento.Add(Espressione);
                                        Salta = true;
                                        IsAnnidateGuidOrder = true;
                                    }
                                }
                                if (Attributo.IsOrdindeDecrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                {
                                    AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                    attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                    foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.SezioneNonElaborata, attributoPerOrdinamento, false))
                                    {
                                        if (!EntitaPerAttributi.IsGuid)
                                            ListaDaRiordinare.Add(Espressione);
                                        else
                                            ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);

                                        CodiciOrdinamento.Add(Espressione);
                                        Salta = true;
                                        IsAnnidateGuidOrder = true;
                                    }
                                }
                            }
                            else
                            {
                                //if (Attributo.Guid == Guid.Empty && !Attributo.Codice.Contains("Guid"))
                                if (Attributo.Guid == Guid.Empty && !Attributo.Codice.Contains(StampeKeys.ConstGuid))
                                {
                                    List<string> ListaEspressioni = new List<string>();
                                    if ((Attributo.IsOrdindeCrescente || Attributo.IsOrdindeDecrescente) && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                    {
                                        if (Attributo.IsOrdindeCrescente)
                                        {
                                            ListaEspressioni = EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, Attributo, true);
                                        }
                                        if (Attributo.IsOrdindeDecrescente)
                                        {
                                            ListaEspressioni = EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, Attributo, false);
                                        }
                                    }
                                    else
                                    {
                                        ListaEspressioni = EstrazioneValoriAttributiNonPerGuid(EntitaPerAttributi.Sezione, Attributo);
                                    }

                                    foreach (var Espressione in ListaEspressioni)
                                    {
                                        if (ContatoreAttributo == 0)
                                        {
                                            SelectionSintax = Espressione;
                                            filtriPecedenti.LastOrDefault().ListaFiltri.Add(SelectionSintax);
                                            Salta = true;
                                        }
                                        else
                                        {
                                            foreach (var filtroPrecedente in filtriPecedenti)
                                            {
                                                foreach (var filtro in filtroPrecedente.ListaFiltri)
                                                {
                                                    SelectionSintax = Espressione;
                                                    listaFiltri.Add(filtro + AndOperator + SelectionSintax);
                                                    Salta = true;
                                                }
                                            }
                                        }
                                    }

                                    if (ContatoreAttributo != 0)
                                    {
                                        filtriPecedenti.FirstOrDefault().ListaFiltri.Clear();
                                        listaFiltri = listaFiltri.Distinct().ToList();
                                        foreach (var item in listaFiltri)
                                        {
                                            filtriPecedenti.FirstOrDefault().ListaFiltri.Add(item);
                                        }
                                    }
                                }
                                else
                                {
                                    SelectionSintax = SelectionSintax + EntitaPerAttributi.Sezione + Attributo.Codice + " = '" + Attributo.Guid.ToString() + "'";
                                    if (Attributo.IsOrdindeCrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                    {
                                        AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                        attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                        foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, attributoPerOrdinamento, true))
                                        {
                                            ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);
                                            CodiciOrdinamento.Add(Espressione);
                                            Salta = true;
                                            IsAnnidateGuidOrder = true;
                                        }
                                    }
                                    if (Attributo.IsOrdindeDecrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                    {
                                        AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                        attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                        foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, attributoPerOrdinamento, false))
                                        {
                                            ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);
                                            CodiciOrdinamento.Add(Espressione);
                                            Salta = true;
                                            IsAnnidateGuidOrder = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (!Salta)
                        {
                            filtriPecedenti.LastOrDefault().ListaFiltri.Add(SelectionSintax);
                            ListaRigheDacancellare.Clear();
                        }

                        Salta = false;
                        ContatoreAttributo++;
                    }
                    ContatoreAttributo = 0;
                    ContatoreSezione++;

                    if (IsAnnidateGuidOrder)
                    {
                        CodiciOrdinamento = CodiciOrdinamento.Distinct().ToList();
                        foreach (var codiceOrdinamento in CodiciOrdinamento)
                        {
                            filtriPecedenti.LastOrDefault().ListaFiltri.AddRange(ListaDaRiordinare.Where(f => f.Contains(codiceOrdinamento)).ToList());
                        }
                    }
                    IsAnnidateGuidOrder = false;
                    ListaDaRiordinare = new List<string>();
                    CodiciOrdinamento = new List<string>();
                }
            }

            Contatore = 0;

            foreach (var Filtro in filtriPecedenti.Distinct().FirstOrDefault().ListaFiltri)
            {
                DataRow[] Row = null;
                Row = DataTable.Select(Filtro);
                foreach (var R in Row)
                {
                    ListaRigheDacancellare.Insert(Contatore, R);
                    Contatore++;
                }
            }


            Contatore = 0;
            foreach (var Row in ListaRigheDacancellare)
            {
                DataRow selectedRow = Row;
                DataRow newRow = DataTable.NewRow();
                try
                {
                    newRow.ItemArray = selectedRow.ItemArray;
                    DataTable.Rows.Remove(selectedRow);
                    DataTable.Rows.InsertAt(newRow, Contatore);

                    Contatore++;
                }
                catch (Exception)
                {
                }
            }
        }

        private void CreazioneFiltri(DatasourceEsternoPerOrdinamento EntitaPerAttributi, int ContatoreSezione, List<FiltriPrecedenti> filtriPecedenti)
        {
            string AndOperator = " and ";

            List<string> listaFiltri = new List<string>();

            int ContatoreAttributo = 0;
            bool IsAnnidateGuidOrder = false;
            List<string> ListaDaRiordinare = new List<string>();
            List<string> CodiciOrdinamento = new List<string>();
            int contatoreFiltro = 0;

            foreach (var filtroPrecedente in filtriPecedenti)
            {
                foreach (var filtro in filtroPrecedente.ListaFiltri)
                {
                    contatoreFiltro++;

                    foreach (var Attributo in EntitaPerAttributi.AttributoPerOrdinamento)
                    {
                        DataRow[] Row = null;
                        bool Salta = false;
                        string SelectionSintax = null;

                        try
                        {
                            if (EntitaPerAttributi.Sezione.Contains(BuiltInCodes.EntityType.Divisione))
                            {
                                SelectionSintax = JReportHelper.ReplaceSymbolNotAllowInReport(SelectionSintax + EntitaPerAttributi.Sezione) + " = '" + Attributo.Guid.ToString() + "'";
                                if (Attributo.IsOrdindeCrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                {
                                    AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                    attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                    foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.SezioneNonElaborata, attributoPerOrdinamento, true))
                                    {
                                        if (!EntitaPerAttributi.IsGuid)
                                        {
                                            if (string.IsNullOrWhiteSpace(filtro))
                                            {
                                                ListaDaRiordinare.Add(Espressione);
                                            }
                                            else
                                            {
                                                ListaDaRiordinare.Add(filtro + AndOperator + Espressione);
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrWhiteSpace(filtro))
                                            {
                                                ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);
                                            }
                                            else
                                            {
                                                ListaDaRiordinare.Add(filtro + AndOperator + SelectionSintax + AndOperator + Espressione);
                                            }
                                        }

                                        CodiciOrdinamento.Add(Espressione);
                                        Salta = true;
                                        IsAnnidateGuidOrder = true;
                                    }
                                }
                                if (Attributo.IsOrdindeDecrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                {
                                    AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                    attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                    foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.SezioneNonElaborata, attributoPerOrdinamento, false))
                                    {
                                        if (!EntitaPerAttributi.IsGuid)
                                        {
                                            if (string.IsNullOrWhiteSpace(filtro))
                                            {
                                                ListaDaRiordinare.Add(Espressione);
                                            }
                                            else
                                            {
                                                ListaDaRiordinare.Add(filtro + AndOperator + Espressione);
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrWhiteSpace(filtro))
                                            {
                                                ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);
                                            }
                                            else
                                            {
                                                ListaDaRiordinare.Add(filtro + AndOperator + SelectionSintax + AndOperator + Espressione);
                                            }
                                        }

                                        CodiciOrdinamento.Add(Espressione);
                                        Salta = true;
                                        IsAnnidateGuidOrder = true;
                                    }
                                }
                            }
                            else
                            {
                                if (Attributo.Guid == Guid.Empty && !Attributo.Codice.Contains(StampeKeys.ConstGuid))
                                {
                                    List<string> ListaEspressioni = new List<string>();

                                    if ((Attributo.IsOrdindeCrescente || Attributo.IsOrdindeDecrescente) && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                    {
                                        if (Attributo.IsOrdindeCrescente)
                                        {
                                            ListaEspressioni = EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, Attributo, true);
                                        }
                                        if (Attributo.IsOrdindeDecrescente)
                                        {
                                            ListaEspressioni = EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, Attributo, false);
                                        }
                                    }
                                    else
                                    {
                                        ListaEspressioni = EstrazioneValoriAttributiNonPerGuid(EntitaPerAttributi.Sezione, Attributo);
                                    }

                                    foreach (var Espressione in ListaEspressioni)
                                    {
                                        SelectionSintax = Espressione;
                                        if (string.IsNullOrEmpty(filtro))
                                        {
                                            listaFiltri.Add(SelectionSintax);
                                        }
                                        else
                                        {
                                            listaFiltri.Add(filtro + AndOperator + SelectionSintax);
                                        }
                                        Salta = true;
                                    }
                                }
                                else
                                {
                                    SelectionSintax = SelectionSintax + EntitaPerAttributi.Sezione + Attributo.Codice + " = '" + Attributo.Guid.ToString() + "'";
                                    if (Attributo.IsOrdindeCrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                    {
                                        AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                        attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                        foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, attributoPerOrdinamento, true))
                                        {
                                            string condizioneOrdinamento = Espressione;
                                            if (string.IsNullOrEmpty(filtro))
                                            {
                                                ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);
                                            }
                                            else
                                            {
                                                ListaDaRiordinare.Add(filtro + AndOperator + SelectionSintax + AndOperator + Espressione);
                                            }
                                            CodiciOrdinamento.Add(Espressione);
                                            Salta = true;
                                            IsAnnidateGuidOrder = true;
                                        }
                                    }
                                    if (Attributo.IsOrdindeDecrescente && !string.IsNullOrEmpty(Attributo.CodiceOrdinamento))
                                    {
                                        AttributoPerOrdinamento attributoPerOrdinamento = new AttributoPerOrdinamento();
                                        attributoPerOrdinamento.Codice = Attributo.CodiceOrdinamento;
                                        foreach (var Espressione in EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(EntitaPerAttributi.Sezione, attributoPerOrdinamento, false))
                                        {
                                            string condizioneOrdinamento = Espressione;
                                            if (string.IsNullOrEmpty(filtro))
                                            {
                                                ListaDaRiordinare.Add(SelectionSintax + AndOperator + Espressione);
                                            }
                                            else
                                            {
                                                ListaDaRiordinare.Add(filtro + AndOperator + SelectionSintax + AndOperator + Espressione);
                                            }
                                            CodiciOrdinamento.Add(Espressione);
                                            Salta = true;
                                            IsAnnidateGuidOrder = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (!Salta)
                        {
                            if (string.IsNullOrEmpty(filtro))
                            {
                                listaFiltri.Add(SelectionSintax);
                            }
                            else
                            {
                                listaFiltri.Add(filtro + AndOperator + SelectionSintax);
                            }
                        }
                        Salta = false;

                        ContatoreAttributo++;
                    }

                    if (IsAnnidateGuidOrder)
                    {
                        CodiciOrdinamento = CodiciOrdinamento.Distinct().ToList();
                        foreach (var codiceOrdinamento in CodiciOrdinamento)
                        {
                            listaFiltri.AddRange(ListaDaRiordinare.Where(f => f.Contains(codiceOrdinamento)).ToList());
                        }
                    }
                    IsAnnidateGuidOrder = false;
                    ListaDaRiordinare = new List<string>();
                    CodiciOrdinamento = new List<string>();
                }

            }

            filtriPecedenti.FirstOrDefault().ListaFiltri.Clear();
            listaFiltri = listaFiltri.Distinct().ToList();
            foreach (var item in listaFiltri)
            {
                filtriPecedenti.FirstOrDefault().ListaFiltri.Add(item);
            }
        }

        private List<string> EstrazioneValoriAttributiNonPerGuid(string Sezione, AttributoPerOrdinamento Attributo)
        {
            List<string> ExpressionString = new List<string>();
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[Sezione];

            if (EntitySelected.IsTreeMaster == false)
            {
                List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(Sezione, null, null, null, out entitiesFound);
                Entities = DataService.GetEntitiesById(Sezione, entitiesFound);
            }
            else
            {
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(Sezione, null, null, out entitiesFound);
                TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
                TreeEntities = DataService.GetTreeEntitiesById(Sezione, TreeInfo.Select(item => item.Id));
            }

            if (EntitySelected.IsTreeMaster)
            {
                foreach (var ent in TreeEntities)
                {
                    if (!ent.IsParent)
                    {
                        if (ent.Attributi.ContainsKey(Attributo.Codice))
                        {
                            var attributoRicercato = ent.Attributi[Attributo.Codice];
                            string Espressione = Sezione + Attributo.Codice + " = '" + attributoRicercato.Valore.PlainText + "'";

                            if (attributoRicercato.Valore is ValoreReale)
                            {
                                double? res;
                                string format = attributoFormatHelper.GetValoreFormat(attributoRicercato);
                                if (string.IsNullOrEmpty(format))
                                {
                                    format = DefaultFormatRelaeContabilità;
                                }
                                NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                                NumericFormatHelper.UpdateCulture(false);
                                Nformat.SymbolText = "";
                                string Result = (attributoRicercato.Valore as ValoreReale).FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                                if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                                res = Convert.ToDouble(Result);
                                res = (attributoRicercato.Valore as ValoreReale).RealResult;
                                Result = GeneraNumeroComeSuDbPerOrdinamento(Nformat, res);
                                Espressione = Sezione + Attributo.Codice + " = " + Result + "";
                            }
                            if (attributoRicercato.Valore is ValoreContabilita)
                            {
                                double? res;
                                string format = attributoFormatHelper.GetValoreFormat(attributoRicercato);
                                if (string.IsNullOrEmpty(format))
                                {
                                    format = DefaultFormatRelaeContabilità;
                                }
                                NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                                NumericFormatHelper.UpdateCulture(false);
                                Nformat.SymbolText = "";
                                string Result = (attributoRicercato.Valore as ValoreContabilita).FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                                if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                                res = Convert.ToDouble(Result);
                                if ((attributoRicercato.Valore as ValoreContabilita).RealResult != null)
                                    res = (double)(attributoRicercato.Valore as ValoreContabilita).RealResult;
                                else
                                    res = 0;
                                Result = GeneraNumeroComeSuDbPerOrdinamento(Nformat, res);
                                Espressione = Sezione + Attributo.Codice + " = " + Result + "";
                            }
                            if (attributoRicercato.Valore is ValoreContabilita == false && attributoRicercato.Valore is ValoreReale == false)
                            {
                                Espressione = Sezione + Attributo.Codice + " = '" + attributoRicercato.Valore.PlainText + "'";
                            }

                            if (ExpressionString.Where(e => e == Espressione).FirstOrDefault() == null)
                            {
                                ExpressionString.Add(Espressione);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var ent in Entities)
                {

                    if (ent.Attributi.ContainsKey(Attributo.Codice))
                    {
                        var attributoRicercato = ent.Attributi[Attributo.Codice];
                        string Espressione = null;

                        if (attributoRicercato.Valore is ValoreReale)
                        {
                            double? res;
                            string format = attributoFormatHelper.GetValoreFormat(attributoRicercato);
                            if (string.IsNullOrEmpty(format))
                            {
                                format = DefaultFormatRelaeContabilità;
                            }
                            NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                            NumericFormatHelper.UpdateCulture(false);
                            Nformat.SymbolText = "";
                            string Result = (attributoRicercato.Valore as ValoreReale).FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                            if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                            res = Convert.ToDouble(Result);
                            res = (attributoRicercato.Valore as ValoreReale).RealResult;
                            Result = GeneraNumeroComeSuDbPerOrdinamento(Nformat, res);
                            Espressione = Sezione + Attributo.Codice + " = " + Result + "";
                        }
                        if (attributoRicercato.Valore is ValoreContabilita)
                        {
                            double? res;
                            string format = attributoFormatHelper.GetValoreFormat(attributoRicercato);
                            NumberFormat Nformat = NumericFormatHelper.DecomposeFormat(format);
                            if (string.IsNullOrEmpty(format))
                            {
                                format = DefaultFormatRelaeContabilità;
                            }
                            NumericFormatHelper.UpdateCulture(false);
                            Nformat.SymbolText = "";
                            string Result = (attributoRicercato.Valore as ValoreContabilita).FormatRealResult(NumericFormatHelper.GetPaddedFormat(NumericFormatHelper.ComposeFormat(Nformat)));
                            if (string.IsNullOrEmpty(Result)) { Result = "0"; }
                            res = Convert.ToDouble(Result);
                            if ((attributoRicercato.Valore as ValoreContabilita).RealResult != null)
                                res = (double)(attributoRicercato.Valore as ValoreContabilita).RealResult;
                            else
                                res = 0;
                            Result = GeneraNumeroComeSuDbPerOrdinamento(Nformat, res);
                            Espressione = Sezione + Attributo.Codice + " = " + Result + "";
                        }
                        if (attributoRicercato.Valore is ValoreContabilita == false && attributoRicercato.Valore is ValoreReale == false)
                        {
                            Espressione = Sezione + Attributo.Codice + " = '" + attributoRicercato.Valore.PlainText + "'";
                        }


                        if (ExpressionString.Where(e => e == Espressione).FirstOrDefault() == null)
                        {
                            ExpressionString.Add(Espressione);
                        }
                    }

                }
            }

            return ExpressionString;
        }

        private List<string> EstrazioneValoriAttributiNonPerGuidOrdinatiCrescenteDecrescente(string Sezione, AttributoPerOrdinamento Attributo, bool IsOrdineCrescente)
        {
            List<string> ExpressionString = new List<string>();
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[Sezione];

            if (EntitySelected.IsTreeMaster == false)
            {
                List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(Sezione, null, null, null, out entitiesFound);
                Entities = DataService.GetEntitiesById(Sezione, entitiesFound);
            }
            else
            {
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(Sezione, null, null, out entitiesFound);
                TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
                TreeEntities = DataService.GetTreeEntitiesById(Sezione, TreeInfo.Select(item => item.Id));
            }

            List<double> ListaValoriNumero = new List<double>();
            List<string> ListaValoriStringa = new List<string>();
            string Espressione = null;

            if (EntitySelected.IsTreeMaster)
            {
                foreach (var ent in TreeEntities)
                {
                    if (!ent.IsParent)
                    {
                        if (ent.Attributi.ContainsKey(Attributo.Codice))
                        {
                            var attributoRicercato = ent.Attributi[Attributo.Codice];

                            if (attributoRicercato.Valore is ValoreReale)
                            {
                                double? res;
                                res = (attributoRicercato.Valore as ValoreReale).RealResult;
                                ListaValoriNumero.Add(res.Value);
                            }
                            if (attributoRicercato.Valore is ValoreContabilita)
                            {
                                double? res;
                                if ((attributoRicercato.Valore as ValoreContabilita).RealResult != null)
                                    res = (double)(attributoRicercato.Valore as ValoreContabilita).RealResult;
                                else
                                    res = 0;
                                ListaValoriNumero.Add(res.Value);
                            }
                            if (attributoRicercato.Valore is ValoreContabilita == false && attributoRicercato.Valore is ValoreReale == false)
                            {
                                ListaValoriStringa.Add(attributoRicercato.Valore.PlainText);
                            }

                        }
                    }
                }
            }
            else
            {
                foreach (var ent in Entities)
                {
                    if (ent.Attributi.ContainsKey(Attributo.Codice))
                    {
                        var attributoRicercato = ent.Attributi[Attributo.Codice];

                        if (attributoRicercato.Valore is ValoreReale)
                        {
                            double? res;
                            res = (double)(attributoRicercato.Valore as ValoreReale).RealResult;
                            ListaValoriNumero.Add(res.Value);
                        }
                        if (attributoRicercato.Valore is ValoreContabilita)
                        {
                            double? res;
                            if ((attributoRicercato.Valore as ValoreContabilita).RealResult != null)
                                res = (double)(attributoRicercato.Valore as ValoreContabilita).RealResult;
                            else
                                res = 0;
                            ListaValoriNumero.Add(res.Value);
                        }
                        if (attributoRicercato.Valore is ValoreContabilita == false && attributoRicercato.Valore is ValoreReale == false)
                        {
                            ListaValoriStringa.Add(attributoRicercato.Valore.PlainText);
                        }
                    }
                }
            }

            if (IsOrdineCrescente)
            {
                if (ListaValoriNumero.Count() > 0)
                {
                    ListaValoriNumero = ListaValoriNumero.OrderBy(r => r).ToList();
                    foreach (var Numero in ListaValoriNumero)
                    {
                        Espressione = Sezione + Attributo.Codice + " = " + Numero.ToString().Replace(",", ".") + "";
                        ExpressionString.Add(Espressione.Replace("-", "_"));
                    }
                }
                if (ListaValoriStringa.Count() > 0)
                {
                    ListaValoriStringa = ListaValoriStringa.OrderBy(r => r).ToList();
                    foreach (var Stringa in ListaValoriStringa)
                    {
                        //Espressione = Sezione + Attributo.Codice + " = '" + Stringa + "'";
                        Espressione = (Sezione + Attributo.Codice).Replace("-", "_") + " = '" + Stringa + "'";
                        //ExpressionString.Add(Espressione.Replace("-", "_"));
                        ExpressionString.Add(Espressione);
                    }
                }
            }
            else
            {
                if (ListaValoriNumero.Count() > 0)
                {
                    ListaValoriNumero = ListaValoriNumero.OrderByDescending(r => r).ToList();
                    foreach (var Numero in ListaValoriNumero)
                    {
                        Espressione = Sezione + Attributo.Codice + " = " + Numero.ToString().Replace(",", ".") + "";
                        ExpressionString.Add(Espressione.Replace("-", "_"));
                    }
                }
                if (ListaValoriStringa.Count() > 0)
                {
                    ListaValoriStringa = ListaValoriStringa.OrderByDescending(r => r).ToList();
                    foreach (var Stringa in ListaValoriStringa)
                    {
                        //Espressione = Sezione + Attributo.Codice + " = '" + Stringa + "'";
                        Espressione = (Sezione + Attributo.Codice).Replace("-", "_") + " = '" + Stringa + "'";
                        //ExpressionString.Add(Espressione.Replace("-", "_"));
                        ExpressionString.Add(Espressione);
                    }
                }
            }

            return ExpressionString;
        }

        public string GeneraNumeroComeSuDbPerOrdinamento(NumberFormat numberFormat, double? res)
        {
            string str = ".";
            string zeros = "0000000000000000000000000";
            //str = string.Format("{0}{1}", str, zeros.Substring(0, numberFormat.DecimalDigitCount));
            //str = str.Insert(0, "0");
            //return String.Format("{0:" + str + "}", res).Replace(",", ".");
            return res.ToString().Replace(",", ".");
        }

        public static string convertNumberToReadableString(long num)
        {
            string result = "";
            long mod = 0;
            long i = 0;
            string[] unita = { "zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove", "dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciassette", "diciotto", "diciannove" };
            string[] decine = { "", "dieci", "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottonta", "novanta" };
            if (num > 0 && num < 20)
            {

                result = unita[num];
            }
            else
            {
                if (num < 100)
                {
                    mod = num % 10;
                    i = num / 10;
                    switch (mod)
                    {
                        case 0:
                            result = decine[i];
                            break;
                        case 1:
                            result = decine[i].Substring(0, decine[i].Length - 1) + unita[mod];
                            break;
                        case 8:
                            result = decine[i].Substring(0, decine[i].Length - 1) + unita[mod];
                            break;
                        default:
                            result = decine[i] + unita[mod];
                            break;
                    }
                }
                else
                {
                    if (num < 1000)
                    {
                        mod = num % 100;
                        i = (num - mod) / 100;
                        switch (i)
                        {
                            case 1:
                                result = "cento";
                                break;
                            default:
                                result = unita[i] + "cento";
                                break;
                        }
                        result = result + convertNumberToReadableString(mod);
                    }
                    else
                    {
                        if (num < 10000)
                        {
                            mod = num % 1000;
                            i = (num - mod) / 1000;
                            switch (i)
                            {
                                case 1:
                                    result = "mille";
                                    break;
                                default:
                                    result = unita[i] + "mila";
                                    break;
                            }
                            result = result + convertNumberToReadableString(mod);
                        }
                        else
                        {
                            if (num < 1000000)
                            {
                                mod = num % 1000;
                                i = (num - mod) / 1000;
                                switch ((num - mod) / 1000)
                                {
                                    default:
                                        if (i < 20)
                                        {
                                            result = unita[i] + "mila";
                                        }
                                        else
                                        {
                                            result = convertNumberToReadableString(i) + "mila";
                                        }
                                        break;
                                }
                                result = result + convertNumberToReadableString(mod);
                            }
                            else
                            {
                                if (num < 1000000000)
                                {
                                    mod = num % 1000000;
                                    i = (num - mod) / 1000000;
                                    switch (i)
                                    {
                                        case 1:
                                            result = "unmilione";
                                            break;

                                        default:
                                            result = convertNumberToReadableString(i) + "milioni";

                                            break;
                                    }
                                    result = result + convertNumberToReadableString(mod);
                                }
                                else
                                {
                                    if (num < 1000000000000)
                                    {
                                        mod = num % 1000000000;
                                        i = (num - mod) / 1000000000;
                                        switch (i)
                                        {
                                            case 1:
                                                result = "unmiliardo";
                                                break;

                                            default:
                                                result = convertNumberToReadableString(i) + "miliardi";

                                                break;
                                        }
                                        result = result + convertNumberToReadableString(mod);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;

        }

    }


    public class DatasourceEsternoPerOrdinamento
    {
        public string Sezione { get; set; }
        public string SezioneNonElaborata { get; set; }
        public bool IsGuid { get; set; }
        public List<AttributoPerOrdinamento> AttributoPerOrdinamento { get; set; }

        public DatasourceEsternoPerOrdinamento()
        {
            AttributoPerOrdinamento = new List<AttributoPerOrdinamento>();
        }
    }

    public class FiltriPrecedenti
    {
        public string Sezione { get; set; }
        public List<string> ListaFiltri { get; set; }
    }

    public class AttributoPerOrdinamento
    {
        public Guid Guid { get; set; }
        public string Valore { get; set; }
        public string Codice { get; set; }
        public int Index { get; set; }
        public bool IsOrdindeCrescente { get; set; }
        public bool IsOrdindeDecrescente { get; set; }
        public string CodiceOrdinamento { get; set; }
        public string SezioneOrdinamento { get; set; }
    }

    public class ParenEntity
    {
        public Guid Guid { get; set; }
        public string Sezione { get; set; }
        public string Numero { get; set; }
        public string Attributo1 { get; set; }
        public string Attributo2 { get; set; }
        public string Attributo3 { get; set; }
        public string AttributoGuid { get; set; }
    }

    class DataSourceComparer : IComparer<string>
    {
        Dictionary<Guid, int> _guidIndexes { get; set; } = new Dictionary<Guid, int>();

        public void SetOrderedIds(Dictionary<Guid, int> guidIndexes)
        {
            _guidIndexes = guidIndexes;
        }

        public void Clear()
        {
            _guidIndexes.Clear();
        }
        

        public int Compare(string x, string y)
        {
            int res = 0;

            if (x == y)
                return res;

            if (Guid.TryParse(x, out Guid xId) && Guid.TryParse(y, out Guid yId))
            {

                if (_guidIndexes.ContainsKey(xId) && _guidIndexes.ContainsKey(yId))
                {
                    if (_guidIndexes[xId] < _guidIndexes[yId])
                        res = -1;
                    else if (_guidIndexes[xId] > _guidIndexes[yId])
                        res = 1;
                    else
                        res = 0;
                }
            }
            else
            {
                res = string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }

            return res;
        }
    }

    ///// <summary>
    ///// Identifica univocamente un'entità di una sezione 
    ///// </summary>
    //struct EntityTypeEntityId
    //{
    //    public string EntityTypeKey { get; set; }
    //    public Guid EntityId { get; set; }
    //}

    /// <summary>
    /// Proprietà dell'expando object
    /// </summary>
    struct AttributiValoreProperty
    {
        public string PropertyName { get; set; } = string.Empty;
        public object PropertyValue { get; set; } = null;

        public AttributiValoreProperty() {}
    }

    struct AttributiValoreProperties
    {
        public List<AttributiValoreProperty> Items { get; set; } = new List<AttributiValoreProperty>();
        
        public AttributiValoreProperties() {}
    }
}
