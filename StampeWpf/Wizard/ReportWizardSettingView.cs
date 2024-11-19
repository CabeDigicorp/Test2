using CommonResources;
using Commons;
using DevExpress.XtraRichEdit;
using FogliDiCalcoloWpf;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StampeWpf.Wizard
{
    public class ReportWizardSettingView
    {
        public ClientDataService DataService { get; set; }//ref
        public IEntityWindowService WindowService { get; set; }//ref
        public ModelActionsStack ModelActionsStack { get; set; }//ref
        public ReportWizardSettingGanttView reportWizardSettingGanttView { get; set; }
        public ReportWizardSettingDataView reportWizardSettingDataView { get; set; }
        public ReportWizardSettingFogliDiCalcoloView reportWizardSettingFogliDiCalcoloView { get; set; }
        public ReportWizardSettingDataWnd reportWizardSettingDataWnd { get; set; }
        public ReportWizardSettingGanttWnd reportWizardSettingGanttWnd { get; set; }
        public ReportWizardSettingFogliDiCalcoloWnd reportWizardSettingFogliDiCalcoloWnd { get; set; }
        public Guid SelectedEntityId { get; set; }
        public bool DataNotValid { get; set; }
        public string CodiceReportIniziale { get; set; }
        public bool IsDigicorpOwnerLocal { get; set; }
        public AttivitaWpf.View.GanttView GanttView { get; internal set; }
        public FogliDiCalcoloView FogliDiCalcoloView { get; internal set; }

        private string SezioneReport;

        public List<string> ListEntityReportToEsclude = new List<string>() { LocalizationProvider.GetString("Report"), LocalizationProvider.GetString("Documenti") };
        public ReportWizardSettingView()
        {

        }

        public void Init(ClientDataService dataService, IEntityWindowService windowService, IMainOperation MainOperation, Guid selectedEntityId)
        {
            DataService = dataService;
            WindowService = windowService;
            SelectedEntityId = selectedEntityId;
            // Inizializzo datacontext per view

            Entity ent = DataService.GetEntityById(BuiltInCodes.EntityType.Report, SelectedEntityId);
            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            ValoreElenco valoreElenco = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Sezione, true, true);
            SezioneReport = valoreElenco.PlainText.Trim();

            

            if (SezioneReport == "Gantt")
                InitGanttView(MainOperation);
            else if (SezioneReport == LocalizationProvider.GetString("FogliDiCalcolo"))
                InitFogliDiCalcoloView(MainOperation);
            else
                InitDataView(MainOperation);
        }

        private void InitGanttView(IMainOperation MainOperation)
        {
            reportWizardSettingGanttView = new ReportWizardSettingGanttView(GanttView);
            reportWizardSettingGanttView.DataService = DataService;
            reportWizardSettingGanttView.WindowService = WindowService;
            reportWizardSettingGanttView.MainOperation = MainOperation;

            Entity ent = DataService.GetEntityById(BuiltInCodes.EntityType.Report, SelectedEntityId);
            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            Valore ReportWizardAttribute = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.ReportWizardSetting, true, true);
            StampeData ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(ReportWizardAttribute.PlainText);
            if (ReportSetting != null)
            {
                if (ReportSetting.GanttSetting != null)
                {
                    reportWizardSettingGanttView.ReportSetting.GanttSetting.GanttDateFrom = ReportSetting.GanttSetting.GanttDateFrom;
                    reportWizardSettingGanttView.ReportSetting.GanttSetting.GanttDateTo = ReportSetting.GanttSetting.GanttDateTo;
                    reportWizardSettingGanttView.ReportSetting.GanttSetting.GanttZoom = ReportSetting.GanttSetting.GanttZoom;
                    reportWizardSettingGanttView.ReportSetting.GanttSetting.AdjustToPage = ReportSetting.GanttSetting.AdjustToPage;
                    reportWizardSettingGanttView.ReportSetting.GanttSetting.ColumnWidth = ReportSetting.GanttSetting.ColumnWidth;
                }
            }
            reportWizardSettingGanttView.ReportSetting.Codice = ent.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText;
            Valore valore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.DescrizioneReport, true, true);
            reportWizardSettingGanttView.ReportSetting.DescrizioneReport = valore.PlainText;
            ValoreElenco valoreElenco = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Sezione, true, true);
            reportWizardSettingGanttView.ReportSetting.GuidSezione = valoreElenco.ValoreAttributoElencoId;
            reportWizardSettingGanttView.ReportSetting.SezioneKey = "Gantt";
            Valore IsDigicorpOwner = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.IsDigicorpOwner, true, true);
            IsDigicorpOwnerLocal = Convert.ToBoolean(IsDigicorpOwner.PlainText);
            reportWizardSettingGanttView.CodiceReport = ent.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText;
            CodiceReportIniziale = reportWizardSettingGanttView.CodiceReport;
            //if (ReportSetting != null)
            //{
            //    reportWizardSettingGanttView.ReportSetting.GanttDa = ReportSetting.GanttDa;
            //    reportWizardSettingGanttView.ReportSetting.GanttA = ReportSetting.GanttA;
            //}

            bool ret = reportWizardSettingGanttView.Init();

            if (!ret)
                return;

            reportWizardSettingGanttWnd = new ReportWizardSettingGanttWnd();
            reportWizardSettingGanttWnd.SourceInitialized += (x, y) => reportWizardSettingGanttWnd.HideMinimizeAndMaximizeButtons();
            reportWizardSettingGanttWnd.Owner = System.Windows.Application.Current.MainWindow;
            reportWizardSettingGanttWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            reportWizardSettingGanttWnd.DataContext = reportWizardSettingGanttView;
            reportWizardSettingGanttView.IsEnablePreview = true;

            //reportWizardSettingGanttWnd.Init();

            DataNotValid = true;

        }

        private void InitFogliDiCalcoloView(IMainOperation MainOperation)
        {
            reportWizardSettingFogliDiCalcoloView = new ReportWizardSettingFogliDiCalcoloView(FogliDiCalcoloView);
            reportWizardSettingFogliDiCalcoloView.DataService = DataService;
            reportWizardSettingFogliDiCalcoloView.WindowService = WindowService;
            reportWizardSettingFogliDiCalcoloView.MainOperation = MainOperation;
            Entity ent = DataService.GetEntityById(BuiltInCodes.EntityType.Report, SelectedEntityId);
            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            Valore ReportWizardAttribute = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.ReportWizardSetting, true, true);
            StampeData ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(ReportWizardAttribute.PlainText);
            if (ReportSetting != null)
            {
                if (ReportSetting.FoglioDiCalcoloSetting != null)
                {
                    reportWizardSettingFogliDiCalcoloView.ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint = ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint;
                    reportWizardSettingFogliDiCalcoloView.ReportSetting.FoglioDiCalcoloSetting.FitToPageKey = ReportSetting.FoglioDiCalcoloSetting.FitToPageKey;
                }
            }

            Valore valore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.DescrizioneReport, true, true);
            reportWizardSettingFogliDiCalcoloView.ReportSetting.DescrizioneReport = valore.PlainText;
            reportWizardSettingFogliDiCalcoloView.ReportSetting.GuidSezione = BuiltInCodes.SectionItemsId.FogliDiCalcolo;
            Valore IsDigicorpOwner = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.IsDigicorpOwner, true, true);
            IsDigicorpOwnerLocal = Convert.ToBoolean(IsDigicorpOwner.PlainText);
            reportWizardSettingFogliDiCalcoloView.CodiceReport = ent.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText;
            CodiceReportIniziale = reportWizardSettingFogliDiCalcoloView.CodiceReport;

            reportWizardSettingFogliDiCalcoloView.Init();

            reportWizardSettingFogliDiCalcoloWnd = new ReportWizardSettingFogliDiCalcoloWnd();
            reportWizardSettingFogliDiCalcoloWnd.SourceInitialized += (x, y) => reportWizardSettingFogliDiCalcoloWnd.HideMinimizeAndMaximizeButtons();
            reportWizardSettingFogliDiCalcoloWnd.Owner = System.Windows.Application.Current.MainWindow;
            reportWizardSettingFogliDiCalcoloWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            reportWizardSettingFogliDiCalcoloWnd.DataContext = reportWizardSettingFogliDiCalcoloView;

            DataNotValid = true;
        }
        private void InitDataView(IMainOperation MainOperation)
        {
            reportWizardSettingDataView = new ReportWizardSettingDataView();
            reportWizardSettingDataView.DataService = DataService;
            reportWizardSettingDataView.WindowService = WindowService;
            reportWizardSettingDataView.MainOperation = MainOperation;
            reportWizardSettingDataView.Sezioni = PreapareDataForComboSezioni(LoadEntityTypesNameForCombo());

            // initialize view  in the center of the screen
            reportWizardSettingDataWnd = new ReportWizardSettingDataWnd();
            reportWizardSettingDataWnd.SourceInitialized += (x, y) => reportWizardSettingDataWnd.HideMinimizeAndMaximizeButtons();
            reportWizardSettingDataWnd.Owner = System.Windows.Application.Current.MainWindow;
            reportWizardSettingDataWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            reportWizardSettingDataWnd.DataContext = reportWizardSettingDataView;
            reportWizardSettingDataWnd.StampeWizardStep2Ctrl.DataContext = reportWizardSettingDataView;

            LoadReportAttribute();

            if (reportWizardSettingDataView.Sezione == null)
            {
                MessageBox.Show(LocalizationProvider.GetString("SelezionareUnaSezionePerPoterProcedereAllEditazioneDelReport"));
                DataNotValid = false;
                return;
            }
            reportWizardSettingDataView.IsTreeMaster = reportWizardSettingDataView.Sezione.IsTreeMaster;
            reportWizardSettingDataView.Init();
            DataNotValid = true;
        }


        public ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel> PreapareDataForComboSezioni(List<RiferimentiComboItemStrampe> RiferimentiComboItem)
        {
            ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel> Sezioni = new ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel>();
            string Categoria = null;

            foreach (var item in RiferimentiComboItem.OrderByDescending(f => f.Category))
            {
                if (Categoria != item.Category)
                {
                    Sezioni.Add(new CommonResources.Controls.ComboBoxTreeLevel() { Content = item.Category });
                }
                Sezioni.LastOrDefault().TreeContent.Add(new CommonResources.Controls.ComboBoxTreeLevel() { Content = item.Name, Key = item.Key, Name = item.Name, IsTreeMaster = item.IsTreeMaster });
                Categoria = item.Category;
            }

            return Sezioni;
        }

        public List<RiferimentiComboItemStrampe> LoadEntityTypesNameForCombo()
        {
            List<string> _entityTypesKeyOnInit = new List<string>();

            List<RiferimentiComboItemStrampe> items = new List<RiferimentiComboItemStrampe>();

            var EntityTypes = DataService.GetEntityTypes();

            foreach (EntityType entType in EntityTypes.Values)
            {
                int dependencyEnum = (int)entType.DependencyEnum;

                if (!entType.IsParentType())
                {
                    string Category = null;
                    if (!(entType is DivisioneItemType)) { Category = LocalizationProvider.GetString("Sezioni"); } else { Category = LocalizationProvider.GetString("Divisione"); }
                    {
                        string entTypeKey = entType.GetKey();

                        _entityTypesKeyOnInit.Add(entTypeKey);
                        if (!ListEntityReportToEsclude.Contains(EntityTypes[entTypeKey].Name))
                        {
                            items.Add(new RiferimentiComboItemStrampe()
                            {
                                Key = EntityTypes[entTypeKey].GetKey(),
                                Name = EntityTypes[entTypeKey].Name,
                                Category = Category,
                                IsTreeMaster = entType.IsTreeMaster,
                            });
                        }
                    }
                }
            }
            return items;
        }

        private void LoadReportAttribute()
        {
            Entity ent = DataService.GetEntityById(BuiltInCodes.EntityType.Report, SelectedEntityId);
            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            Valore ReportWizardAttribute = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.ReportWizardSetting, true, true);
            Valore IsDigicorpOwner = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.IsDigicorpOwner, true, true);

            if (ent != null)
            {
                reportWizardSettingDataView.CodiceReport = ent.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText;
                CodiceReportIniziale = reportWizardSettingDataView.CodiceReport;
                IsDigicorpOwnerLocal = Convert.ToBoolean(IsDigicorpOwner.PlainText);
            }

            //if (MainViewStatus.IsAdvancedMode)
            //{
            //    reportWizardSettingDataView.ButtonImage = "\xe086";
            //    reportWizardSettingDataView.ButtonContent = null;
            //}
            //else
            //{
            //    if (IsDigicorpOwnerLocal)
            //    {
            //        reportWizardSettingDataView.ButtonImage = "\xe0c6";
            //        reportWizardSettingDataView.ButtonContent = LocalizationProvider.GetString("CreaReport");
            //    }
            //    else
            //    {
            //        reportWizardSettingDataView.ButtonImage = "\xe086";
            //        reportWizardSettingDataView.ButtonContent = null;
            //    }
            //}
            // IN SOSTITUZIONE A SOLUZIONE PRECEDENTE CON CREA REPORT
            reportWizardSettingDataView.ButtonImage = "\xe086";
            reportWizardSettingDataView.ButtonContent = null;

            Valore valore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.DescrizioneReport, true, true);

            reportWizardSettingDataView.Title = LocalizationProvider.GetString("Layout") + " " + valore.PlainText;
            reportWizardSettingDataView.DescrizionrReport = valore.PlainText;

            valore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.NumeroColonne, true, true);

            if (String.IsNullOrEmpty(valore.PlainText) || valore.PlainText == "0")
                reportWizardSettingDataView.NumeroColonne = "3";
            else
                reportWizardSettingDataView.NumeroColonne = valore.PlainText;

            ValoreBooleano valoreBool = (ValoreBooleano)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.TabellaBordata, true, true);
            reportWizardSettingDataView.IsTabellaBordata = (bool)valoreBool.V;

            valoreBool = (ValoreBooleano)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.UsaRftAttributi, true, true);
            reportWizardSettingDataView.IsAllFieldRtfFormat = (bool)valoreBool.V;

            ValoreElenco valoreElenco = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Sezione, true, true);
            reportWizardSettingDataView.Sezione = reportWizardSettingDataView.Sezioni.Where(s => s.Name == valoreElenco.PlainText).FirstOrDefault();

            foreach (var SezionePadre in reportWizardSettingDataView.Sezioni)
            {
                foreach (var SezioneFiglia in SezionePadre.TreeContent)
                {
                    //if (contatore == valoreElenco.ValoreAttributoElencoId)
                    //{
                    //    reportWizardSettingDataView.GuidSezione = valoreElenco.ValoreAttributoElencoId;
                    //    reportWizardSettingDataView.Sezione = SezioneFiglia;
                    //}
                    if (SezioneFiglia.Name == valoreElenco.PlainText.Trim())
                    {
                        reportWizardSettingDataView.GuidSezione = valoreElenco.ValoreAttributoElencoId;
                        reportWizardSettingDataView.Sezione = SezioneFiglia;
                        break;
                    }
                }
            }

            if (ReportWizardAttribute != null)
            {
                if (!string.IsNullOrEmpty(ReportWizardAttribute.PlainText))
                {
                    StampeData ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(ReportWizardAttribute.PlainText);
                    reportWizardSettingDataView.IsInLoadReportSaved = true;
                    //reportWizardSettingDataView.IsAllFieldRtfFormat = ReportSetting.IsAllFieldRtfFormat;
                    reportWizardSettingDataView.ReportSetting = ReportSetting;
                }
            }
        }

        public StampeData StartWizard()
        {
            // await close window 
            StampeData EntityReportToSave = null;

            try
            {

                bool? IsMainClosed;

                if (SezioneReport == "Gantt")
                {
                    IsMainClosed = reportWizardSettingGanttWnd.ShowDialog();
                    EntityReportToSave = reportWizardSettingGanttView.ReportSetting;
                }
                else if (SezioneReport == LocalizationProvider.GetString("FogliDiCalcolo"))
                {
                    IsMainClosed = reportWizardSettingFogliDiCalcoloWnd.ShowDialog();
                    EntityReportToSave = reportWizardSettingFogliDiCalcoloView.ReportSetting;
                }
                else
                {
                    IsMainClosed = reportWizardSettingDataWnd.ShowDialog();
                    EntityReportToSave = reportWizardSettingDataView.ReportSetting;
                }

                if (EntityReportToSave != null)
                {
                    EntityReportToSave.Codice = GenerateCodeReport(EntityReportToSave);
                    return EntityReportToSave;
                }
                else
                {
                    return null;
                }


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GenerateCodeReport(StampeData reportSetting, ClientDataService dataservice = null)
        {
            if (DataService == null) { DataService = dataservice; }

            string _CodiceReport = null;
            int myInt;


            if (SezioneReport == "Gantt")
            {
                if (reportWizardSettingGanttView != null)
                {
                    if (!string.IsNullOrWhiteSpace(reportWizardSettingGanttView.CodiceReport))
                    {
                        return reportWizardSettingGanttView.CodiceReport;
                    }
                }
            }
            else if (SezioneReport == LocalizationProvider.GetString("FogliDiCalcolo"))
            {
                if (reportWizardSettingFogliDiCalcoloView != null)
                {
                    if (!string.IsNullOrWhiteSpace(reportWizardSettingFogliDiCalcoloView.CodiceReport))
                    {
                        return reportWizardSettingFogliDiCalcoloView.CodiceReport;
                    }
                }
            }
            else
            {
                if (reportWizardSettingDataView != null)
                {
                    if (!string.IsNullOrWhiteSpace(reportWizardSettingDataView.CodiceReport))
                    {
                        return reportWizardSettingDataView.CodiceReport;
                    }
                }
            }

           

            FilterData filter = null;
            ViewSettings viewSettings = DataService.GetViewSettings();
            EntityTypeViewSettings EntityViewSettings = viewSettings.EntityTypes[BuiltInCodes.EntityType.Report];

            filter = new FilterData();

            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

            List<Guid> entitiesFound = null;

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.Report];
            List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(BuiltInCodes.EntityType.Report, filter, null, null, out entitiesFound);
            List<Entity> Entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.Report, entitiesFound);

            int ContatoreReport = 1;
            HashSet<int> ListaCodici = new HashSet<int>();
            //retrieve the last number for the report for assigned a new code
            foreach (var ent in Entities)
            {
                if (ent.Attributi.Values.FirstOrDefault().AttributoCodice == BuiltInCodes.Attributo.Codice)
                {
                    Attributo att = ent.Attributi.Values.FirstOrDefault().Attributo;
                    Valore val = null;
                    val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, true);

                    if (val is ValoreTesto)
                    {
                        ValoreTesto valTesto = val as ValoreTesto;
                        string plainText = valTesto.V;
                        if (int.TryParse(plainText, out myInt))
                        {
                            ListaCodici.Add(myInt);
                        }
                    }
                }
            }

            for (int i = 1; i < ListaCodici.Count() + 1; i++)
            {
                if (!ListaCodici.Contains(i))
                {
                    ContatoreReport = i;
                    break;
                }
                ContatoreReport++;
            }

            _CodiceReport = ContatoreReport.ToString();
            reportSetting.IsNew = true;
            return _CodiceReport;

        }
        public IEnumerable<ModelAction> CreateNestedAction(StampeData etityReportToSave, bool IsDigicorpOwner, bool IsFromStdToCst = false)
        {
            List<ModelAction> Actions = new List<ModelAction>();

            ModelAction actionReportSettings = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.Report,
                ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                AttributoCode = BuiltInCodes.Attributo.ReportWizardSetting
            };

            actionReportSettings.NewValore = new ValoreTesto() { V = Newtonsoft.Json.JsonConvert.SerializeObject(etityReportToSave) };
            Actions.Add(actionReportSettings);

            ModelAction actionCodice = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.Report,
                ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                AttributoCode = BuiltInCodes.Attributo.Codice
            };

            actionCodice.NewValore = new ValoreTesto() { V = etityReportToSave.Codice };
            Actions.Add(actionCodice);

            if (!MainViewStatus.IsAdvancedMode)
            {
                ModelAction actionDescrizione = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Report,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = BuiltInCodes.Attributo.DescrizioneReport
                };

                //if (IsFromStdToCst)
                //{
                //    List<Guid> entitiesFound = null;
                //    List<Entity> Entities = new List<Entity>();
                //    List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(BuiltInCodes.EntityType.Report, null, null, null, out entitiesFound);
                //    Entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.Report, entitiesFound);
                //    string nome = LocalizationProvider.GetString("CopiaDi");
                //    var ListaNonNUlli = Entities.Where(e => !string.IsNullOrEmpty(e.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText)).ToList();
                //    foreach (var item in ListaNonNUlli.Where(e => e.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText.StartsWith(LocalizationProvider.GetString("CopiaDi")) && e.Attributi[BuiltInCodes.Attributo.DescrizioneReport].Valore.PlainText.Contains(etityReportToSave.DescrizioneReport)))
                //    {
                //        nome = nome + LocalizationProvider.GetString("CopiaDi");
                //    }

                //    actionDescrizione.NewValore = new ValoreTesto() { V = nome + etityReportToSave.DescrizioneReport };
                //}
                //else
                //{
                actionDescrizione.NewValore = new ValoreTesto() { V = etityReportToSave.DescrizioneReport };
                //}

                Actions.Add(actionDescrizione);

                ModelAction actionSezione = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Report,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = BuiltInCodes.Attributo.Sezione
                };

                actionSezione.NewValore = new ValoreElenco() { ValoreAttributoElencoId = etityReportToSave.GuidSezione };
                Actions.Add(actionSezione);
            }

            ModelAction actionNumeroColonne = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.Report,
                ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                AttributoCode = BuiltInCodes.Attributo.NumeroColonne
            };

            actionNumeroColonne.NewValore = new ValoreReale() { V = etityReportToSave.NumeroColonne };
            Actions.Add(actionNumeroColonne);

            if (reportWizardSettingDataView != null)
            {
                ModelAction actionBordatura = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Report,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = BuiltInCodes.Attributo.TabellaBordata
                };

                actionBordatura.NewValore = new ValoreBooleano() { V = reportWizardSettingDataView.IsTabellaBordata };
                Actions.Add(actionBordatura);

                ModelAction actionUsaRtfAttributi = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Report,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = BuiltInCodes.Attributo.UsaRftAttributi
                };

                actionUsaRtfAttributi.NewValore = new ValoreBooleano() { V = reportWizardSettingDataView.IsAllFieldRtfFormat };
                Actions.Add(actionUsaRtfAttributi);

                ModelAction actionIsCompilato = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Report,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = BuiltInCodes.Attributo.Compilato
                };

                actionIsCompilato.NewValore = new ValoreBooleano() { V = reportWizardSettingDataView.IsReportFilledUp() };
                Actions.Add(actionIsCompilato);
            }

            //if (IsDigicorpOwner)
            //{
                ModelAction actionIsDigicorpOwner = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Report,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = BuiltInCodes.Attributo.IsDigicorpOwner
                };

                actionIsDigicorpOwner.NewValore = new ValoreBooleano() { V = IsDigicorpOwner };
                Actions.Add(actionIsDigicorpOwner);
            //}

            return Actions;

        }
    }

    public class RiferimentiComboItemStrampe : RiferimentiComboItem
    {
        public bool IsTreeMaster { get; set; }
    }
}
