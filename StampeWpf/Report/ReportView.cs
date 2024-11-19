using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using Model;
using System.Windows.Input;
using CommonResources;
using Syncfusion.Data.Extensions;
using Commons.View;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Management;
using StampeWpf.Wizard;
using AttivitaWpf.View;
using FogliDiCalcoloWpf;

namespace StampeWpf.View
{
    public class ReportItemView : MasterDetailGridItemView
    {
        public ReportItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {

        }

        public override bool IsValoreAttributoReadOnly(string attCode)
        {
            if ((attCode == BuiltInCodes.Attributo.Sezione || attCode == BuiltInCodes.Attributo.NumeroColonne) && This.Attributi.ContainsKey(attCode))
            {
                Valore val = Entity.GetValoreAttributo(BuiltInCodes.Attributo.Compilato, false, false);
                if ((val as ValoreBooleano).V.GetValueOrDefault() == true)
                    return true;

                //if (This.Attributi[BuiltInCodes.Attributo.Compilato].Valore.HasValue())
                //    if ((bool)(This.Attributi[BuiltInCodes.Attributo.Compilato].Valore as ValoreBooleano).V)
                //        return true;
            }
            return base.IsValoreAttributoReadOnly(attCode);
        }

    }
    public class ReportView : MasterDetailGridView, SectionItemTemplateView
    {
        public GanttView GanttView { get; internal set; }
        public FogliDiCalcoloView FogliDiCalcoloView { get; internal set; }
        public ReportItemsViewVirtualized ReportItemsView { get => ItemsView as ReportItemsViewVirtualized; }
        public string SelectedItemGuid { get; set; }

        public ReportView()
        {
            ItemsView = new ReportItemsViewVirtualized(this);
            //RowOnTopCount = 1; //columns header
            ReportItemsView.ReportViewOwner = this;
        }
        public int Code => (int)StampeSectionItemsId.Report;

        public override void Init(EntityTypeViewSettings viewSettings)
        {
            base.Init(viewSettings);

            WBSView wbsView = MainOperation.GetWBSView() as WBSView;
            GanttView = wbsView.GanttView as GanttView;

            FogliDiCalcoloView = MainOperation.GetFogliDiCalcoloView() as FogliDiCalcoloView;
        }

        public void OpenWizardOfSelectedReport(Guid Guid)
        {
            ReportWizardSettingView reportWizardSettingView = new ReportWizardSettingView();
            reportWizardSettingView.GanttView = GanttView;
            reportWizardSettingView.FogliDiCalcoloView = FogliDiCalcoloView;
            reportWizardSettingView.Init(ItemsView.DataService, WindowService, MainOperation, Guid);
            if (reportWizardSettingView.DataNotValid)
            {
                StampeData EntityToSave = reportWizardSettingView.StartWizard();
                if (EntityToSave != null)
                {
                    if (MainViewStatus.IsAdvancedMode)
                    {
                        IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, true);
                        ReportItemsView.SaveEntityReportSetting(EntityToSave, Actions, reportWizardSettingView.SelectedEntityId);
                    }
                    else
                    {
                        if (reportWizardSettingView.IsDigicorpOwnerLocal)
                        {
                            //ReportWizardSettingView FakereportWizardSettingView = new ReportWizardSettingView();
                            //StampeData FakeStampeData = new StampeData();
                            //string ValoreCodice = FakereportWizardSettingView.GenerateCodeReport(FakeStampeData, DataService);
                            //EntityToSave.Codice = ValoreCodice;
                            //IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, true);
                            //ReportItemsView.CreateNewOneCustom(EntityToSave, Actions);
                            EntityToSave.Codice = EntityToSave.Codice + "*";
                            IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, reportWizardSettingView.IsDigicorpOwnerLocal);
                            ReportItemsView.SaveEntityReportSetting(EntityToSave, Actions, reportWizardSettingView.SelectedEntityId);
                        }
                        else
                        {
                            IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, false);
                            ReportItemsView.SaveEntityReportSetting(EntityToSave, Actions, reportWizardSettingView.SelectedEntityId);
                        }
                    }
                }

                if (reportWizardSettingView.reportWizardSettingGanttView != null)
                {
                    reportWizardSettingView.reportWizardSettingGanttView = null;
                    reportWizardSettingView.reportWizardSettingGanttWnd = null;
                }
                if (reportWizardSettingView.reportWizardSettingDataView != null)
                {
                    reportWizardSettingView.reportWizardSettingDataView.GroupSetting = null;
                    reportWizardSettingView.reportWizardSettingDataView.DocumentoCorpoView = null;
                    reportWizardSettingView.reportWizardSettingDataWnd = null;
                    reportWizardSettingView.reportWizardSettingDataView = null;
                }

                reportWizardSettingView = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// Consente l'aggiornamento dei codici di attributi e codici di divisoni nei report (tipicamente dopo importa modello)
        /// </summary>
        /// <param name="entTypesKeyRevision"></param>
        /// <param name="attsCodiceRevision"></param>
        public void UpdateCodici(List<Guid> importedEntitiesId, List<EntityTypeCodiceRevision> entTypesCodiceRevision, List<AttributoCodiceRevision> attsCodiceRevision)
        {
            try
            {
                SostituzioneCodici(importedEntitiesId, entTypesCodiceRevision, attsCodiceRevision);
            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

            return;
            //try
            //{
            //    List<TreeEntity> TreeEntities = new List<TreeEntity>();
            //    List<Entity> Entities = new List<Entity>();
            //    EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

            //    Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            //    EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.Report];
            //    Entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.Report, importedEntitiesId);
            //    int count = 0;

            //    foreach (var Entity in Entities)
            //    {
            //        count++;
            //        Valore val = Entity.Attributi[BuiltInCodes.Attributo.ReportWizardSetting].Valore;
            //        StampeData ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(val.PlainText);

            //        foreach (EntityTypeCodiceRevision entTypeCodiceRevision in entTypesCodiceRevision)
            //        {
            //            StampeWpf.Wizard.TreeviewItem TreeViewItem = new StampeWpf.Wizard.TreeviewItem();
            //            TreeViewItem.AttrbutoCodice = entTypeCodiceRevision.NewEntityTypeCodice;
            //            if (string.IsNullOrEmpty(entTypeCodiceRevision.OldEntityTypeCodice))
            //                TreeViewItem.AttrbutoCodiceOrigine = entTypeCodiceRevision.NewEntityTypeCodice;
            //            else
            //                TreeViewItem.AttrbutoCodiceOrigine = entTypeCodiceRevision.OldEntityTypeCodice;

            //            var ListEntityFound = EntitiesList.Where(x => x.Value.Codice == entTypeCodiceRevision.NewEntityTypeCodice).ToList();
            //            foreach (var EntityFound in ListEntityFound)
            //            {
            //                if (EntityFound.Value is DivisioneItemType)
            //                {
            //                    TreeViewItem.EntityType = EntityFound.Value.GetKey();
            //                    Test(ReportSetting, TreeViewItem);
            //                }
            //            }
            //        }

            //        if (entTypesCodiceRevision.Count() > 0 || attsCodiceRevision.Count() > 0)
            //        {
            //            ModelActionResponse mar;
            //            HashSet<Guid> GuidEntity = new HashSet<Guid>();
            //            GuidEntity.Add(Entity.Id);
            //            ModelAction attActionRuleId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.ReportWizardSetting, EntitiesId = GuidEntity, EntityTypeKey = BuiltInCodes.EntityType.Report };
            //            attActionRuleId.NewValore = new ValoreTesto() { V = Newtonsoft.Json.JsonConvert.SerializeObject(ReportSetting) };

            //            mar = ItemsView.CommitAction(attActionRuleId);
            //            if (mar.ActionResponse == ActionResponse.OK)
            //            {
            //            }
            //        }
            //    }

            //    count = 0;

            //    foreach (var Entity in Entities)
            //    {
            //        count++;
            //        Valore val = Entity.Attributi[BuiltInCodes.Attributo.ReportWizardSetting].Valore;
            //        StampeData ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(val.PlainText);

            //        foreach (AttributoCodiceRevision attCodiceRevision in attsCodiceRevision)
            //        {
            //            Dictionary<string, EntityType> entityTypes = DataService.GetEntityTypes();
            //            EntityType entityType = entityTypes[ReportSetting.SezioneKey];
            //            Dictionary<string, Attributo> listattributes = entityType.Attributi;
            //            listattributes = listattributes.Where(x =>
            //            x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection &&
            //            x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection &&
            //            x.Value.IsInternal != true).ToDictionary(i => i.Key, i => i.Value);
            //            StampeWpf.Wizard.TreeviewItem TreeViewItem = new StampeWpf.Wizard.TreeviewItem();

            //            foreach (var Attributo in listattributes.Where(a => a.Value.Codice == attCodiceRevision.NewAttributoCodice))
            //            {
            //                TreeViewItem.AttrbutoCodice = Attributo.Value.Codice;
            //                TreeViewItem.EntityType = Attributo.Value.EntityTypeKey;
            //                TreeViewItem.AttrbutoCodiceOrigine = attCodiceRevision.OldAttributoCodice;

            //                if (Attributo.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            //                {
            //                    Attributo AttributoOrigine = entitiesHelper.GetSourceAttributo(Attributo.Value);
            //                    TreeViewItem.AttrbutoCodice = AttributoOrigine.Codice;
            //                    TreeViewItem.EntityType = AttributoOrigine.EntityTypeKey;
            //                    TreeViewItem.AttrbutoCodiceOrigine = AttributoOrigine.Codice;
            //                    if (AttributoOrigine.EntityTypeKey.Contains(BuiltInCodes.EntityType.Divisione))
            //                    {
            //                        EntityType entityTypeLocal = entityTypes[AttributoOrigine.EntityTypeKey];
            //                        TreeViewItem.DivisioneCodice = entityTypeLocal.Codice;
            //                    }
            //                }

            //                if (Attributo.Value.GuidReferenceEntityTypeKey.Contains(BuiltInCodes.EntityType.Divisione) || Attributo.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
            //                {
            //                    //EntityType entityTypeLocal = entityTypes[Attributo.Value.GuidReferenceEntityTypeKey];
            //                    TreeViewItem.AttrbutoCodice = Attributo.Value.Codice;
            //                    TreeViewItem.EntityType = Attributo.Value.GuidReferenceEntityTypeKey;
            //                }

            //                Test(ReportSetting, TreeViewItem);
            //            }
            //        }
            //    }


            //}
            //catch (Exception)
            //{

            //}
        }


        //private void Test(StampeData ReportSetting, StampeWpf.Wizard.TreeviewItem TreeViewItem)
        //{
        //    Dictionary<string, EntityType> entityTypes = DataService.GetEntityTypes();
        //    EntityType entityType = null;
        //    List<string> AttributiPerEntityTypeLocal = new List<string>();
        //    if (!string.IsNullOrEmpty(TreeViewItem.EntityType))
        //        entityType = entityTypes[TreeViewItem.EntityType];

        //    string OldDivisioneEntityType = null;

        //    foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource)
        //    {
        //        if (Raggruppamento.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || Raggruppamento.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
        //        {
        //            if (Raggruppamento.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine)
        //            {
        //                Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                OldDivisioneEntityType = Raggruppamento.EntityType;
        //                Raggruppamento.EntityType = TreeViewItem.EntityType;
        //            }
        //        }
        //        else
        //        {
        //            if (Raggruppamento.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Raggruppamento.EntityType == TreeViewItem.EntityType)
        //            {
        //                Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //            }
        //        }
        //    }

        //    foreach (var CorpoDocumento in ReportSetting.CorpiDocumento)
        //    {
        //        foreach (var Cella in CorpoDocumento.CorpoColonna.Where(a => !string.IsNullOrEmpty(a.EntityType)))
        //        {
        //            if (Cella.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || Cella.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
        //            {
        //                if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.DivisioneCodice == TreeViewItem.DivisioneCodice)
        //                {
        //                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                    OldDivisioneEntityType = Cella.EntityType;
        //                    Cella.EntityType = TreeViewItem.EntityType;
        //                }
        //                if (Cella.DivisioneCodice == TreeViewItem.AttrbutoCodiceOrigine)
        //                {
        //                    Cella.DivisioneCodice = entityType.Codice;
        //                    OldDivisioneEntityType = Cella.EntityType;
        //                    Cella.EntityType = TreeViewItem.EntityType;
        //                }
        //            }
        //            else
        //            {
        //                if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
        //                {
        //                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                }
        //            }
        //        }
        //    }

        //    foreach (var Testa in ReportSetting.Teste)
        //    {
        //        foreach (var Colonna in Testa.RaggruppamentiDocumento)
        //        {
        //            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
        //            {
        //                if (Cella.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || Cella.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
        //                {
        //                    if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.DivisioneCodice == TreeViewItem.DivisioneCodice)
        //                    {
        //                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                        OldDivisioneEntityType = Cella.EntityType;
        //                        Cella.EntityType = TreeViewItem.EntityType;
        //                    }
        //                    if (Cella.DivisioneCodice == TreeViewItem.AttrbutoCodiceOrigine)
        //                    {
        //                        Cella.DivisioneCodice = entityType.Codice;
        //                        OldDivisioneEntityType = Cella.EntityType;
        //                        Cella.EntityType = TreeViewItem.EntityType;
        //                    }
        //                }
        //                else
        //                {
        //                    if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
        //                    {
        //                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    foreach (var Testa in ReportSetting.Code)
        //    {
        //        foreach (var Colonna in Testa.RaggruppamentiDocumento)
        //        {
        //            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
        //            {
        //                if (Cella.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || Cella.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
        //                {
        //                    if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.DivisioneCodice == TreeViewItem.DivisioneCodice)
        //                    {
        //                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                        OldDivisioneEntityType = Cella.EntityType;
        //                        Cella.EntityType = TreeViewItem.EntityType;
        //                    }
        //                    if (Cella.DivisioneCodice == TreeViewItem.AttrbutoCodiceOrigine)
        //                    {
        //                        Cella.DivisioneCodice = entityType.Codice;
        //                        OldDivisioneEntityType = Cella.EntityType;
        //                        Cella.EntityType = TreeViewItem.EntityType;
        //                    }
        //                }
        //                else
        //                {
        //                    if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
        //                    {
        //                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    foreach (var AttriubutiUtilizzati in ReportSetting.AttributiDaEstrarrePerDatasource)
        //    {
        //        if (AttriubutiUtilizzati.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || AttriubutiUtilizzati.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
        //        {
        //            if (!String.IsNullOrEmpty(OldDivisioneEntityType) && AttriubutiUtilizzati.EntityType == OldDivisioneEntityType)
        //            {
        //                AttriubutiUtilizzati.EntityType = TreeViewItem.EntityType;
        //            }
        //        }

        //        foreach (var Attributo in AttriubutiUtilizzati.AttributiPerEntityType)
        //        {
        //            if (Attributo == TreeViewItem.AttrbutoCodiceOrigine && AttriubutiUtilizzati.EntityType == TreeViewItem.EntityType)
        //            {
        //                AttributiPerEntityTypeLocal.Add(TreeViewItem.AttrbutoCodice);
        //            }
        //        }
        //        AttriubutiUtilizzati.AttributiPerEntityType = AttributiPerEntityTypeLocal;
        //        AttributiPerEntityTypeLocal = new List<string>();
        //    }

        //}

        private void SostituzioneCodici(List<Guid> importedEntitiesId, List<EntityTypeCodiceRevision> entTypesCodiceRevision, List<AttributoCodiceRevision> attsCodiceRevision)
        {
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();
            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.Report];
            Entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.Report, importedEntitiesId);
            int count = 0;

            foreach (var Entity in Entities)
            {
                count++;
                Valore val = Entity.Attributi[BuiltInCodes.Attributo.ReportWizardSetting].Valore;
                StampeData ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(val.PlainText);

                if (ReportSetting == null)
                    continue;

                foreach (AttributoCodiceRevision AttributoCodiceRevision in attsCodiceRevision)
                {
                    EntityType entityType = EntitiesList[AttributoCodiceRevision.NewEntityTypeCodice];
                    Dictionary<string, Attributo> listattributes = entityType.Attributi;
                    listattributes = listattributes.Where(x =>
                    x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection &&
                    x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection &&
                    x.Value.IsInternal != true).ToDictionary(i => i.Key, i => i.Value);
                    Attributo Attributo = entityType.Attributi[AttributoCodiceRevision.NewAttributoCodice];

                    TreeviewItem TreeviewItem = new TreeviewItem();

                    // CASO GUID SOLO SE DIVISONE SOSTITUISCO ENTITY TYPE PERCHè CONTIENE IL GUID
                    if (Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        TreeviewItem.PropertyType = Attributo.DefinizioneAttributoCodice;
                        TreeviewItem.AttrbutoCodice = Attributo.Codice;
                        TreeviewItem.AttrbutoCodiceOrigine = AttributoCodiceRevision.OldAttributoCodice;
                        TreeviewItem.EntityType = Attributo.GuidReferenceEntityTypeKey;
                        if (Attributo.GuidReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                            SostituzioneAttributo(ReportSetting, TreeviewItem, true);
                        else
                            SostituzioneAttributo(ReportSetting, TreeviewItem, false);
                        continue;
                    }
                    if (Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    {
                        Attributo SourceAttributo = entitiesHelper.GetSourceAttributo(Attributo);
                        TreeviewItem.PropertyType = SourceAttributo.DefinizioneAttributoCodice;
                        TreeviewItem.AttrbutoCodice = SourceAttributo.Codice;
                        TreeviewItem.AttrbutoCodiceOrigine = AttributoCodiceRevision.OldAttributoCodice;
                        TreeviewItem.EntityType = SourceAttributo.EntityType.GetKey();
                        if (TreeviewItem.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                            SostituzioneAttributo(ReportSetting, TreeviewItem, true, AttributoCodiceRevision.NewAttributoCodice);
                        else
                            SostituzioneAttributo(ReportSetting, TreeviewItem, false, AttributoCodiceRevision.NewAttributoCodice);
                        continue;
                    }
                    TreeviewItem.PropertyType = Attributo.DefinizioneAttributoCodice;
                    TreeviewItem.AttrbutoCodice = Attributo.Codice;
                    TreeviewItem.AttrbutoCodiceOrigine = AttributoCodiceRevision.OldAttributoCodice;
                    TreeviewItem.EntityType = Attributo.EntityType.GetKey();
                    SostituzioneAttributo(ReportSetting, TreeviewItem, false);
                }

                foreach (EntityTypeCodiceRevision EntTypesCodiceRevision in entTypesCodiceRevision.Where(e => string.IsNullOrEmpty(e.OldEntityTypeCodice)).ToList())
                {
                    StampeWpf.Wizard.TreeviewItem TreeViewItem = new StampeWpf.Wizard.TreeviewItem();
                    TreeViewItem.AttrbutoCodice = EntTypesCodiceRevision.NewEntityTypeCodice;
                    TreeViewItem.AttrbutoCodiceOrigine = EntTypesCodiceRevision.NewEntityTypeCodice;
                    TreeViewItem.EntityType = EntitiesList.Where(x => x.Value.Codice == EntTypesCodiceRevision.NewEntityTypeCodice).FirstOrDefault().Key;
                    AggiornaGuidDivisioneBuiltInAttributo(ReportSetting, TreeViewItem);
                }


                if (entTypesCodiceRevision.Count() > 0 || attsCodiceRevision.Count() > 0)
                {
                    ModelActionResponse mar;
                    HashSet<Guid> GuidEntity = new HashSet<Guid>();
                    GuidEntity.Add(Entity.EntityId);
                    ModelAction attActionRuleId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.ReportWizardSetting, EntitiesId = GuidEntity, EntityTypeKey = BuiltInCodes.EntityType.Report };
                    attActionRuleId.NewValore = new ValoreTesto() { V = Newtonsoft.Json.JsonConvert.SerializeObject(ReportSetting) };

                    mar = ItemsView.CommitAction(attActionRuleId);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                    }
                }

            }
        }

        private void SostituzioneAttributo(StampeData ReportSetting, StampeWpf.Wizard.TreeviewItem TreeViewItem, bool IsDivisione, string NeAttributoCodiceRiferimento = null)
        {
            string OldDivisioneEntityType = null;

            if (ReportSetting.RaggruppamentiDatasource != null)
            {

                foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource)
                {
                    if (IsDivisione)
                    {
                        if (string.IsNullOrEmpty(Raggruppamento.DivisioneCodice))
                        {
                            if (Raggruppamento.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine)
                            {
                                OldDivisioneEntityType = Raggruppamento.EntityType;
                                Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                Raggruppamento.EntityType = TreeViewItem.EntityType;
                                continue;
                            }
                        }
                        else
                        {
                            if (Raggruppamento.DivisioneCodice == TreeViewItem.AttrbutoCodiceOrigine)
                            {
                                OldDivisioneEntityType = Raggruppamento.EntityType;
                                Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                Raggruppamento.EntityType = TreeViewItem.EntityType;
                                continue;
                            }
                        }
                    }
                    if (Raggruppamento.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Raggruppamento.EntityType == TreeViewItem.EntityType)
                    {
                        Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
                        continue;
                    }
                    if (Raggruppamento.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine && Raggruppamento.EntityType == TreeViewItem.EntityType)
                    {
                        Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
                        if (!string.IsNullOrEmpty(NeAttributoCodiceRiferimento))
                        {
                            //Raggruppamento.AttributoCodicePath.Replace(Raggruppamento.AttributoCodiceOrigine, NeAttributoCodiceRiferimento);
                            Raggruppamento.AttributoCodiceOrigine = NeAttributoCodiceRiferimento;
                        }
                        continue;
                    }
                }
            }

            if (ReportSetting.CorpiDocumento != null)
            {
                foreach (var CorpoDocumento in ReportSetting.CorpiDocumento)
                {
                    foreach (var Cella in CorpoDocumento.CorpoColonna.Where(a => !string.IsNullOrEmpty(a.EntityType)))
                    {
                        if (IsDivisione)
                        {
                            if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine)
                            {
                                OldDivisioneEntityType = Cella.EntityType;
                                Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                Cella.EntityType = TreeViewItem.EntityType;
                                continue;
                            }
                            if (Cella.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine)
                            {
                                OldDivisioneEntityType = Cella.EntityType;
                                Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                Cella.EntityType = TreeViewItem.EntityType;
                                continue;
                            }
                        }
                        if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
                        {
                            Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                            continue;
                        }
                        if (Cella.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
                        {
                            Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                            if (!string.IsNullOrEmpty(NeAttributoCodiceRiferimento))
                            {
                                //Cella.AttributoCodicePath.Replace(Cella.AttributoCodiceOrigine, NeAttributoCodiceRiferimento);
                                Cella.AttributoCodiceOrigine = NeAttributoCodiceRiferimento;
                            }
                            continue;
                        }
                    }

                    foreach (var Testa in ReportSetting.Teste)
                    {
                        foreach (var Colonna in Testa.RaggruppamentiDocumento)
                        {
                            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
                            {
                                if (IsDivisione)
                                {
                                    if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine)
                                    {
                                        OldDivisioneEntityType = Cella.EntityType;
                                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                        Cella.EntityType = TreeViewItem.EntityType;
                                        Testa.EntityType = Cella.EntityType;
                                        continue;
                                    }
                                    if (Cella.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine)
                                    {
                                        OldDivisioneEntityType = Cella.EntityType;
                                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                        Cella.EntityType = TreeViewItem.EntityType;
                                        Testa.EntityType = Cella.EntityType;
                                        continue;
                                    }
                                }
                                if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
                                {
                                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                }
                                if (Cella.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
                                {
                                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                    if (!string.IsNullOrEmpty(NeAttributoCodiceRiferimento))
                                    {
                                        //.AttributoCodicePath.Replace(Cella.AttributoCodiceOrigine, NeAttributoCodiceRiferimento);
                                        Cella.AttributoCodiceOrigine = NeAttributoCodiceRiferimento;
                                    }
                                    continue;
                                }
                            }
                        }
                    }

                    foreach (var Coda in ReportSetting.Code)
                    {
                        foreach (var Colonna in Coda.RaggruppamentiDocumento)
                        {
                            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
                            {
                                if (IsDivisione)
                                {
                                    if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine)
                                    {
                                        OldDivisioneEntityType = Cella.EntityType;
                                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                        Cella.EntityType = TreeViewItem.EntityType;
                                        Coda.EntityType = Cella.EntityType;
                                        continue;
                                    }
                                    if (Cella.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine)
                                    {
                                        OldDivisioneEntityType = Cella.EntityType;
                                        Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                        Cella.EntityType = TreeViewItem.EntityType;
                                        Coda.EntityType = Cella.EntityType;
                                        continue;
                                    }
                                }
                                if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
                                {
                                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                    continue;
                                }
                                if (Cella.AttributoCodiceOrigine == TreeViewItem.AttrbutoCodiceOrigine && Cella.EntityType == TreeViewItem.EntityType)
                                {
                                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                    if (!string.IsNullOrEmpty(NeAttributoCodiceRiferimento))
                                    {
                                        //Cella.AttributoCodicePath.Replace(Cella.AttributoCodiceOrigine, NeAttributoCodiceRiferimento);
                                        Cella.AttributoCodiceOrigine = NeAttributoCodiceRiferimento;
                                    }
                                    continue;
                                }
                            }
                        }
                    }

                    foreach (var AttriubutiUtilizzati in ReportSetting.AttributiDaEstrarrePerDatasource)
                    {
                        List<string> AttributiPerEntityTypeLocal = new List<string>();
                        foreach (var Attributo in AttriubutiUtilizzati.AttributiPerEntityType)
                        {
                            bool AggintaEseguita = false;
                            if (IsDivisione)
                            {
                                if (AttriubutiUtilizzati.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || AttriubutiUtilizzati.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
                                {
                                    if (!String.IsNullOrEmpty(OldDivisioneEntityType) && AttriubutiUtilizzati.EntityType == OldDivisioneEntityType)
                                    {
                                        AttriubutiUtilizzati.EntityType = TreeViewItem.EntityType;
                                    }
                                }
                            }
                            if (!AggintaEseguita)
                            {
                                if (Attributo == TreeViewItem.AttrbutoCodiceOrigine && AttriubutiUtilizzati.EntityType == TreeViewItem.EntityType)
                                {
                                    AttributiPerEntityTypeLocal.Add(TreeViewItem.AttrbutoCodice);
                                    AggintaEseguita = true;
                                }
                            }
                            if (!AggintaEseguita)
                                AttributiPerEntityTypeLocal.Add(Attributo);
                        }
                        AttriubutiUtilizzati.AttributiPerEntityType = AttributiPerEntityTypeLocal;
                        AttributiPerEntityTypeLocal = new List<string>();
                    }
                }
            }




            //public void ReplaceOldAttributeItemWithTheNewOne(StampeData ReportSetting, AttributoCodiceRevision codiceRevision)
            //{
            //    Dictionary<string, EntityType> EnitityType = DataService.GetEntityTypes();
            //    Dictionary<string, string> AbbinamentoCodiceVecchioNuovo = new Dictionary<string, string>();
            //    AbbinamentoCodiceVecchioNuovo.Add(codiceRevision.OldAttributoCodice, codiceRevision.NewAttributoCodice);
            //    List<string> AttributiPerEntityTypeLocal = new List<string>();

            //    foreach (var AttriubutiUtilizzati in ReportSetting.AttributiDaEstrarrePerDatasource.Where(r => r.EntityType.StartsWith(codiceRevision.OldEntityTypeCodice)))
            //    {
            //        foreach (var Attributo in AttriubutiUtilizzati.AttributiPerEntityType)
            //        {
            //            if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Attributo))
            //            {
            //                AttributiPerEntityTypeLocal.Add(AbbinamentoCodiceVecchioNuovo[Attributo]);
            //            }
            //            else
            //            {
            //                AttributiPerEntityTypeLocal.Add(Attributo);
            //            }
            //        }
            //        AttriubutiUtilizzati.AttributiPerEntityType = AttributiPerEntityTypeLocal;
            //        AttributiPerEntityTypeLocal = new List<string>();
            //    }

            //    foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource.Where(r => r.EntityType.StartsWith(codiceRevision.OldEntityTypeCodice)))
            //    {
            //        if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Raggruppamento.AttributoCodice))
            //        {
            //            Raggruppamento.AttributoCodice = AbbinamentoCodiceVecchioNuovo[Raggruppamento.AttributoCodice];
            //        }
            //    }


            //    foreach (var CorpoDocumento in ReportSetting.CorpiDocumento)
            //    {
            //        foreach (var Cella in CorpoDocumento.CorpoColonna.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //        {
            //            if (Cella.EntityType.StartsWith(codiceRevision.OldEntityTypeCodice))
            //            {
            //                if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.AttributoCodice))
            //                {
            //                    Cella.AttributoCodice = AbbinamentoCodiceVecchioNuovo[Cella.AttributoCodice];
            //                }
            //            }
            //        }
            //    }

            //    foreach (var Testa in ReportSetting.Teste)
            //    {
            //        foreach (var Colonna in Testa.RaggruppamentiDocumento)
            //        {
            //            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //            {
            //                if (Cella.EntityType.StartsWith(codiceRevision.OldEntityTypeCodice))
            //                {
            //                    if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.AttributoCodice))
            //                    {
            //                        Cella.AttributoCodice = AbbinamentoCodiceVecchioNuovo[Cella.AttributoCodice];
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    foreach (var Testa in ReportSetting.Code)
            //    {
            //        foreach (var Colonna in Testa.RaggruppamentiDocumento)
            //        {
            //            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //            {
            //                if (Cella.EntityType.StartsWith(codiceRevision.OldEntityTypeCodice))
            //                {
            //                    if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.AttributoCodice))
            //                    {
            //                        Cella.AttributoCodice = AbbinamentoCodiceVecchioNuovo[Cella.AttributoCodice];
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //public void ReplaceDivisionieOldAttributeItemWithTheNewOne(StampeData ReportSetting, EntityTypeCodiceRevision entTypeCodiceRevision, EntityType entityFound)
            //{
            //    int numero;
            //    Dictionary<string, EntityType> EnitityType = DataService.GetEntityTypes();
            //    Dictionary<string, string> AbbinamentoCodiceVecchioNuovo = new Dictionary<string, string>();
            //    if (string.IsNullOrWhiteSpace(entTypeCodiceRevision.OldEntityTypeCodice))
            //    {
            //        AbbinamentoCodiceVecchioNuovo.Add(entTypeCodiceRevision.NewEntityTypeCodice, entTypeCodiceRevision.NewEntityTypeCodice);
            //    }
            //    else
            //    {
            //        AbbinamentoCodiceVecchioNuovo.Add(entTypeCodiceRevision.OldEntityTypeCodice, entTypeCodiceRevision.NewEntityTypeCodice);
            //    }
            //    List<string> AttributiPerEntityTypeLocal = new List<string>();

            //    foreach (var AttriubutiUtilizzati in ReportSetting.AttributiDaEstrarrePerDatasource.Where(r => r.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione)))
            //    {
            //        if (!string.IsNullOrEmpty(AttriubutiUtilizzati.DivisioneCodice))
            //        {
            //            if (AbbinamentoCodiceVecchioNuovo.ContainsKey(AttriubutiUtilizzati.DivisioneCodice))
            //            {
            //                AttriubutiUtilizzati.EntityType = entityFound.GetKey();
            //                if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                {
            //                    AttriubutiUtilizzati.DivisioneCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                }
            //            }
            //        }
            //    }

            //    foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //    {
            //        if (Raggruppamento.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
            //        {
            //            if (!string.IsNullOrEmpty(Raggruppamento.DivisioneCodice))
            //            {
            //                if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Raggruppamento.DivisioneCodice))
            //                {
            //                    Raggruppamento.EntityType = entityFound.GetKey();
            //                    if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                    {
            //                        Raggruppamento.DivisioneCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Raggruppamento.AttributoCodice))
            //                {
            //                    Raggruppamento.EntityType = entityFound.GetKey();
            //                    if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                    {
            //                        Raggruppamento.AttributoCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                    }
            //                }
            //            }
            //        }
            //    }


            //    foreach (var CorpoDocumento in ReportSetting.CorpiDocumento)
            //    {
            //        foreach (var Cella in CorpoDocumento.CorpoColonna.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //        {
            //            if (Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
            //            {
            //                if (!string.IsNullOrEmpty(Cella.DivisioneCodice))
            //                {
            //                    if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.DivisioneCodice))
            //                    {
            //                        Cella.EntityType = entityFound.GetKey();
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    foreach (var Testa in ReportSetting.Teste)
            //    {
            //        foreach (var Colonna in Testa.RaggruppamentiDocumento)
            //        {
            //            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //            {
            //                if (Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
            //                {
            //                    if (!string.IsNullOrEmpty(Cella.DivisioneCodice))
            //                    {
            //                        if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.DivisioneCodice))
            //                        {
            //                            Cella.EntityType = entityFound.GetKey();
            //                            Testa.EntityType = entityFound.GetKey();
            //                            if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                            {
            //                                Cella.DivisioneCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.AttributoCodice))
            //                        {
            //                            Cella.EntityType = entityFound.GetKey();
            //                            Testa.EntityType = entityFound.GetKey();
            //                            if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                            {
            //                                Cella.AttributoCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    foreach (var Coda in ReportSetting.Code)
            //    {
            //        foreach (var Colonna in Coda.RaggruppamentiDocumento)
            //        {
            //            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
            //            {
            //                if (Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
            //                {
            //                    if (!string.IsNullOrEmpty(Cella.DivisioneCodice))
            //                    {
            //                        if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.DivisioneCodice))
            //                        {
            //                            Cella.EntityType = entityFound.GetKey();
            //                            Coda.EntityType = entityFound.GetKey();
            //                            if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                            {
            //                                Cella.DivisioneCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (AbbinamentoCodiceVecchioNuovo.ContainsKey(Cella.AttributoCodice))
            //                        {
            //                            Cella.EntityType = entityFound.GetKey();
            //                            Coda.EntityType = entityFound.GetKey();
            //                            if (int.TryParse(AbbinamentoCodiceVecchioNuovo.First().Key, out numero))
            //                            {
            //                                Cella.AttributoCodice = AbbinamentoCodiceVecchioNuovo[entTypeCodiceRevision.OldEntityTypeCodice];
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }
        private void AggiornaGuidDivisioneBuiltInAttributo(StampeData ReportSetting, StampeWpf.Wizard.TreeviewItem TreeViewItem)
        {
            List<string> AttributiPerEntityTypeLocal = new List<string>();

            string OldDivisioneEntityType = null;

            if (ReportSetting.RaggruppamentiDatasource != null)
            {

                foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource)
                {
                    if (Raggruppamento.AttributoCodice == TreeViewItem.AttrbutoCodice && Raggruppamento.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        Raggruppamento.AttributoCodice = TreeViewItem.AttrbutoCodice;
                        Raggruppamento.EntityType = TreeViewItem.EntityType;
                        OldDivisioneEntityType = Raggruppamento.EntityType;
                        continue;
                    }
                    if (Raggruppamento.DivisioneCodice == TreeViewItem.AttrbutoCodice && Raggruppamento.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        Raggruppamento.DivisioneCodice = TreeViewItem.AttrbutoCodice;
                        Raggruppamento.EntityType = TreeViewItem.EntityType;
                        OldDivisioneEntityType = Raggruppamento.EntityType;
                        continue;
                    }
                }
            }

            if (ReportSetting.CorpiDocumento != null)
            {

                foreach (var CorpoDocumento in ReportSetting.CorpiDocumento)
                {
                    foreach (var Cella in CorpoDocumento.CorpoColonna.Where(a => !string.IsNullOrEmpty(a.EntityType)))
                    {
                        if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodice && Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                        {
                            OldDivisioneEntityType = Cella.EntityType;
                            Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                            Cella.EntityType = TreeViewItem.EntityType;
                            continue;
                        }
                        if (Cella.DivisioneCodice == TreeViewItem.AttrbutoCodice && Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                        {
                            OldDivisioneEntityType = Cella.EntityType;
                            Cella.DivisioneCodice = TreeViewItem.AttrbutoCodice;
                            Cella.EntityType = TreeViewItem.EntityType;
                            continue;
                        }
                    }

                    foreach (var Testa in ReportSetting.Teste)
                    {
                        foreach (var Colonna in Testa.RaggruppamentiDocumento)
                        {
                            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
                            {
                                if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodice && Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                                {
                                    OldDivisioneEntityType = Cella.EntityType;
                                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                    Cella.EntityType = TreeViewItem.EntityType;
                                    Testa.EntityType = TreeViewItem.EntityType;
                                    continue;
                                }
                                if (Cella.DivisioneCodice == TreeViewItem.AttrbutoCodice && Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                                {
                                    OldDivisioneEntityType = Cella.EntityType;
                                    Cella.DivisioneCodice = TreeViewItem.AttrbutoCodice;
                                    Cella.EntityType = TreeViewItem.EntityType;
                                    Testa.EntityType = TreeViewItem.EntityType;
                                    continue;
                                }
                            }
                        }
                    }

                    foreach (var Coda in ReportSetting.Code)
                    {
                        foreach (var Colonna in Coda.RaggruppamentiDocumento)
                        {
                            foreach (var Cella in Colonna.RaggruppamentiValori.Where(a => !string.IsNullOrEmpty(a.EntityType)))
                            {
                                if (Cella.AttributoCodice == TreeViewItem.AttrbutoCodice && Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                                {
                                    OldDivisioneEntityType = Cella.EntityType;
                                    Cella.AttributoCodice = TreeViewItem.AttrbutoCodice;
                                    Cella.EntityType = TreeViewItem.EntityType;
                                    Coda.EntityType = TreeViewItem.EntityType;
                                    continue;
                                }
                                if (Cella.DivisioneCodice == TreeViewItem.AttrbutoCodice && Cella.EntityType.StartsWith(BuiltInCodes.EntityType.Divisione))
                                {
                                    OldDivisioneEntityType = Cella.EntityType;
                                    Cella.DivisioneCodice = TreeViewItem.AttrbutoCodice;
                                    Cella.EntityType = TreeViewItem.EntityType;
                                    Coda.EntityType = TreeViewItem.EntityType;
                                    continue;
                                }
                            }
                        }
                    }

                    foreach (var AttriubutiUtilizzati in ReportSetting.AttributiDaEstrarrePerDatasource)
                    {
                        foreach (var Attributo in AttriubutiUtilizzati.AttributiPerEntityType)
                        {
                            if (AttriubutiUtilizzati.EntityType.Contains(BuiltInCodes.EntityType.Divisione) || AttriubutiUtilizzati.EntityType.Contains(BuiltInCodes.EntityType.DivisioneParent))
                            {
                                if (!String.IsNullOrEmpty(OldDivisioneEntityType) && AttriubutiUtilizzati.EntityType == OldDivisioneEntityType)
                                {
                                    AttriubutiUtilizzati.EntityType = TreeViewItem.EntityType;
                                }
                            }
                        }
                    }
                }

            }
        }


        public class ReportItemsViewVirtualized : MasterDetailGridItemsViewVirtualized
        {
            public ReportView ReportViewOwner { get; set; }

            public ReportItemsViewVirtualized(MasterDetailGridView reportView) : base(reportView)
            {
            }

            public override void Init()
            {
                base.Init();
                EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Report];

                AttributiEntities.Load(new HashSet<Guid>());
            }

            public override IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
            {
                return base.LoadRange(startIndex, count, sortDescriptions, out overallCount);
            }

            public override void ApplyFilterAndSort(Guid? selectEntityId = null, bool searchOnly = false)
            {
                base.ApplyFilterAndSort(selectEntityId, searchOnly);
            }

            //// This method helps to get dependency property value in another thread.
            //// Usage: Invoke(() => { return CreationOverhead; });
            //private T Invoke<T>(Func<T> callback)
            //{
            //    //return (T)Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
            //    return (T)Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
            //}


            protected override EntityView NewItemView(Entity entity)
            {
                return new ReportItemView(this, entity);
            }


            //public override IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
            //{
            //    IsModelToViewLoading = true;


            //    int minIndex = Math.Min(startIndex, FilteredEntitiesId.Count - 1);
            //    int maxIndex = (int)Math.Min(startIndex + count, FilteredEntitiesId.Count) - 1;

            //    if (minIndex < 0 || maxIndex < 0 || DataService.Suspended)
            //    {
            //        overallCount = 0;
            //        IsModelToViewLoading = false;
            //        return new List<EntityView>();
            //    }

            //    List<Guid> ids = FilteredEntitiesId.GetRange(minIndex, maxIndex - minIndex + 1);

            //    IEnumerable<Entity> listEnts = DataService.GetEntitiesById(BuiltInCodes.EntityType.Report, ids);

            //    overallCount = Invoke(() => { return FilteredEntitiesId.Count; });

            //    //_entities = new List<TreeEntityView>();
            //    List<EntityView> ents = new List<EntityView>();
            //    foreach (Entity ent in listEnts)
            //    {
            //        EntityView newItem = new ReportItemView(this, ent);

            //        if (CheckedEntitiesId.Contains(ent.Id))
            //            newItem.SetChecked(true);

            //        ents.Add(newItem);
            //    }

            //    IsModelToViewLoading = false;
            //    return ents;

            //}

            public async void SaveEntityReportSetting(StampeData EntityToSave, IEnumerable<ModelAction> Actions, Guid selectedEntityId)
            {
                try
                {
                    ModelActionResponse mar;
                    ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Report };
                    action.ActionName = ActionName.MULTI;
                    action.NestedActions.AddRange(Actions);
                    action.NestedActions.ForEach(item => item.EntitiesId = new HashSet<Guid>() { new Guid(selectedEntityId.ToString()) });
                    mar = CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        await UpdateCache(true);
                    }

                    RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));

                }
                catch (Exception exc)
                {

                }
            }
            public async void CreateNewOneCustom(StampeData EtityReportToSave, IEnumerable<ModelAction> Actions)
            {
                try
                {

                    ModelActionResponse mar;
                    ModelAction action = new ModelAction() { EntityTypeKey = this.EntityType.GetKey() };

                    Entity targetEntity = GetActionTargetEntity();

                    if (RightPanesView.FilterView.IsFilterApplied() && targetEntity == null)
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("SelezionareTargetSeFiltroAttivo"));
                        return;
                    }

                    SetValoriOfTargetGroups(action, targetEntity);
                    SetValoriOfCurrentFilter(action/*, targetEntity*/);

                    //aggiungi in coda
                    action.ActionName = ActionName.ENTITY_INSERT;
                    action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = targetEntity.EntityId, TargetReferenceName = TargetReferenceName.AFTER } };

                    ////Setto il valore dell'attributo ReportWizardSetting

                    action.NestedActions.AddRange(Actions);

                    mar = CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        CheckedEntitiesId.Clear();
                        CheckedEntitiesId.Add(mar.NewId);
                        ApplyFilterAndSort(mar.NewId);

                        PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                        await UpdateCache();
                    }

                    RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));

                }
                catch (Exception exc)
                {

                }

            }

            public override void AddEntity()
            {
                base.AddEntity();
            }

            protected override void SetValoriBuiltIn(ModelAction pasteAction, Entity targetEntity)
            {
                base.SetValoriBuiltIn(pasteAction, targetEntity);

                ReportWizardSettingView reportWizardSettingView = new ReportWizardSettingView();

                StampeData FakeStampeData = new StampeData();
                string ValoreCodice = reportWizardSettingView.GenerateCodeReport(FakeStampeData, DataService);

                ModelAction attActionRuleId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Codice, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
                attActionRuleId.NewValore = new ValoreTesto() { V = ValoreCodice };
                pasteAction.NestedActions.Add(attActionRuleId);

                int myInt;

                if (!String.IsNullOrEmpty(targetEntity.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText))
                {
                    if (targetEntity.Attributi[BuiltInCodes.Attributo.IsDigicorpOwner].Valore.PlainText == "true" && !MainViewStatus.IsAdvancedMode)
                    {
                        //List<Guid> entitiesFound = null;
                        //List<Entity> Entities = new List<Entity>();
                        //List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(BuiltInCodes.EntityType.Report, null, null, null, out entitiesFound);
                        //Entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.Report, entitiesFound);
                        //string nome = null;
                        //var ListaNonNUlli = Entities.Where(e => !string.IsNullOrEmpty(e.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText)).ToList();
                        //foreach (var item in ListaNonNUlli.Where(e => e.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText.StartsWith(LocalizationProvider.GetString("CopiaDi"))))
                        //{
                        //    nome = nome + LocalizationProvider.GetString("CopiaDi");
                        //}

                        ModelAction attDescrzione = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.DescrizioneReport, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
                        //attDescrzione.NewValore = new ValoreTesto() { V = nome + targetEntity.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText };
                        attDescrzione.NewValore = new ValoreTesto() { V = targetEntity.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText };
                        pasteAction.NestedActions.Add(attDescrzione);
                    }
                }
            }

            public bool IsStarWizardEnabled { get => (DataService != null) ? !DataService.IsReadOnly : false; }



        }

        


    }
}
