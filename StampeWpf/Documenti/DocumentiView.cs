using AttivitaWpf.View;
using CommonResources;
using Commons;
using FogliDiCalcoloWpf;
using MasterDetailModel;
using MasterDetailView;
using Microsoft.Xaml.Behaviors.Media;
using Model;
using StampeWpf.Wizard;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StampeWpf.View
{
    public class DocumentiItemView : TreeEntityView
    {

        public DocumentiItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }

    }
    public class DocumentiView : MasterDetailTreeView, SectionItemTemplateView
    {
        public AttivitaWpf.View.GanttView GanttView { get; set; }
        public FogliDiCalcoloView FogliDiCalcoloView { get; set; }
        public DocumentiItemsViewVirtualized DocumentiItemsView { get => ItemsView as DocumentiItemsViewVirtualized; }
        public DocumentiView()
        {
            _itemsView = new DocumentiItemsViewVirtualized(this);
        }

        public override void Init(EntityTypeViewSettings viewSettings)
        {
            base.Init(viewSettings);

            if (MainOperation != null)
            {
                WBSView wbsView = MainOperation.GetWBSView() as WBSView;
                GanttView = wbsView.GanttView as GanttView;

                FogliDiCalcoloView = MainOperation.GetFogliDiCalcoloView() as FogliDiCalcoloView;
            }
        }

        public int Code => (int)StampeSectionItemsId.Documenti;

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
                        IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, false);
                        DocumentiItemsView.SaveEntityReportSetting(EntityToSave, Actions, reportWizardSettingView.SelectedEntityId);
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
                            IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, true);
                            DocumentiItemsView.SaveEntityReportSetting(EntityToSave, Actions, reportWizardSettingView.SelectedEntityId);
                        }
                        else
                        {
                            IEnumerable<ModelAction> Actions = reportWizardSettingView.CreateNestedAction(EntityToSave, false);
                            DocumentiItemsView.SaveEntityReportSetting(EntityToSave, Actions, reportWizardSettingView.SelectedEntityId);
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

        public void ReportPreview(TreeEntityView SelectedItem)
        {
            MainOperation.UpdateProjectViewSettings();
            List<Entity> ListaDocumentiDaProcessare = new List<Entity>();
            Dictionary<Guid, Guid> ListaIdFigliEPadri = new Dictionary<Guid, Guid>();
            EntitiesHelper entsHelper = new EntitiesHelper(ItemsView.DataService);

            if (SelectedItem.IsParent)
            {
                List<Guid> GuidFigli = ItemsView.GetChildrenOf(ItemsView.SelectedEntityId);
                ListaIdFigliEPadri.Add(SelectedItem.Id, SelectedItem.Id);
                ListaDocumentiDaProcessare.Add(ItemsView.SelectedEntityView.Entity);
                foreach (var GuidFiglio in GuidFigli)
                {
                    DocumentiItem entity = (DocumentiItem)ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, GuidFiglio);
                    ListaDocumentiDaProcessare.Add(ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, GuidFiglio));
                    ListaIdFigliEPadri.Add(GuidFiglio, SelectedItem.Id);
                    if (entity.IsParent)
                    {
                        GetChildDocumentFromFather(entsHelper, entity, ListaDocumentiDaProcessare, ListaIdFigliEPadri);
                    }

                }
            }
            else
            {
                ListaDocumentiDaProcessare.Add(ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, ItemsView.SelectedEntityId));
            }

            CreateReportAndRunPreviewAsync(ListaDocumentiDaProcessare, ListaIdFigliEPadri, entsHelper);
        }

        private void GetChildDocumentFromFather(EntitiesHelper entsHelper, DocumentiItem entityDocumento, List<Entity> ListaDocumentiDaProcessare, Dictionary<Guid, Guid> listaIdFigliEPadri)
        {
            List<Guid> GuidFigli = ItemsView.GetChildrenOf(entityDocumento.EntityId);
            foreach (var GuidFiglio in GuidFigli)
            {
                DocumentiItem entity = (DocumentiItem)ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, GuidFiglio);
                ListaDocumentiDaProcessare.Add(ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, GuidFiglio));
                listaIdFigliEPadri.Add(GuidFiglio, entityDocumento.EntityId);
                if (entity.IsParent)
                {
                    GetChildDocumentFromFather(entsHelper, entity, ListaDocumentiDaProcessare, listaIdFigliEPadri);
                }
            }
        }

        private void CreateReportAndRunPreviewAsync(List<Entity> listaDocumentiDaProcessare, Dictionary<Guid, Guid> listaIdFigliEPadri, EntitiesHelper entsHelper)
        {
            ReportPreviewGenerator ReportGenerator = new ReportPreviewGenerator();

            try
            {
                CreateDataBaseForEachReport(listaDocumentiDaProcessare, listaIdFigliEPadri, entsHelper, ReportGenerator);
            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }
            finally
            {
            }

            //await Task.Delay(300);
            listaDocumentiDaProcessare = null;
            listaIdFigliEPadri = null;
            entsHelper = null;
            ReportGenerator.ReportDataSourceGenerator = null;
            GC.Collect();

            RunPreviewReport(ReportGenerator);
        }

        private void RunPreviewReport(ReportPreviewGenerator ReportGenerator)
        {
            FastReport.Preview.PreviewControl previewControl1 = null;
            PreviewWnd win = null;

            if (DeveloperVariables.SalvaFrx)
            {
                string frxFilePath = "D:\\Temp\\fastReportTest.frx";

                string FrxFile = ReportGenerator?.ReportStructureGenerator?.ReportObject?.SaveToString();
                System.IO.File.WriteAllText(frxFilePath, FrxFile);

            }
            else
            {
                string fileTestPath = string.Format("{0}{1}", MainOperation.GetAppSettingsPath(), "fastReportTest.frx");
                if (File.Exists(fileTestPath))
                {
                    MessageBox.Show("Attenzione: per la stampa viene usato il file fastReportTest.frx presente nella cartella ProgramData", LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Warning);

                    string FrxFile = System.IO.File.ReadAllText(fileTestPath);
                    ReportGenerator?.ReportStructureGenerator?.ReportObject?.LoadFromString(FrxFile);
                }
            }

            try
            {
                win = new PreviewWnd();
                win.SourceInitialized += (x, y) => win.HideMinimizeAndMaximizeButtons();
                win.report = ReportGenerator?.ReportStructureGenerator?.ReportObject;
                win.Init(false);
                win.ShowDialog();

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
            finally
            {
                previewControl1 = null;
                win = null;
                if (ReportGenerator.ReportStructureGenerator != null)
                {
                    ReportGenerator.ReportStructureGenerator.ReportObject = null;
                    ReportGenerator.ReportStructureGenerator.ReportSetting = null;
                }
                WindowService.ShowWaitCursor(false);
                ReportGenerator = null;
            }

            GC.Collect();
        }
        private void CreateDataBaseForEachReport(List<Entity> listaDocumentiDaProcessare, Dictionary<Guid, Guid> listaIdFigliEPadri, EntitiesHelper entsHelper, ReportPreviewGenerator ReportGenerator)
        {
            string attributosezione = string.Join("_", BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Sezione);

            //CREATE DATASOURCE
            ReportGenerator.ReportDataSourceGenerator = new ReportDataSourceGenerator();
            ReportGenerator.ReportDataSourceGenerator.DataService = DataService;

            int ContatoreReport = 0;
            int ContatoreLista = 0;
            int IniziaDaPagIniziale = 0;
            StampeData ReportSettingPadre = new StampeData();
            bool printOnPreviousPage = false;
            bool Bordatura;
            bool UsaRtfAttributi;

            foreach (DocumentiItem ent in listaDocumentiDaProcessare)
            {
                StampeData ReportSetting = null;
                ValoreBooleano valBool = null;
                ValoreBooleano stampaNuovaPagina = (ValoreBooleano)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.StampaNuovaPagina, false, true);
                Valore valCorpo = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Corpo, false, true);
                ValoreGuid validReport = (ValoreGuid)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.ReportItem_Guid, true, true);
                ValoreElenco valStampaVoci = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.StampaVoci, true, true);
                printOnPreviousPage = (bool)!stampaNuovaPagina.V;
                Bordatura = false;

                ReportSettingPadre = new StampeData();
                if (listaDocumentiDaProcessare.Count > 1)
                {
                    DocumentiItem DocumentoPadre = (DocumentiItem)ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, listaIdFigliEPadri.ElementAt(ContatoreLista).Value);
                    EstrazioneAttributiDocumentoPadre(entsHelper, DocumentoPadre, ReportSettingPadre, listaDocumentiDaProcessare, ContatoreLista);
                }

                Entity entReport = null;
                string SezioneKey = null;
                ValoreElenco valSezione = null;
                if (validReport != null)
                {
                    entReport = ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Report, validReport.V);
                    valSezione = (ValoreElenco)entsHelper.GetValoreAttributo(entReport, BuiltInCodes.Attributo.Sezione, false, true);
                    if (valSezione != null)
                        SezioneKey = valSezione.PlainText.TrimStart();
                }

                //string DbName = ReportPreviewGenerator.DataSourceName + ContatoreReport.ToString();
                string DbName = StampeKeys.ConstDataSourceName + ContatoreReport.ToString();

                if (entReport != null)
                {
                    Valore val = entsHelper.GetValoreAttributo(entReport, BuiltInCodes.Attributo.ReportWizardSetting, true, true);
                    valBool = (ValoreBooleano)entsHelper.GetValoreAttributo(entReport, BuiltInCodes.Attributo.TabellaBordata, true, true);
                    Bordatura = (bool)valBool.V;
                    valBool = (ValoreBooleano)entsHelper.GetValoreAttributo(entReport, BuiltInCodes.Attributo.UsaRftAttributi, true, true);
                    UsaRtfAttributi = (bool)valBool.V;

                    if (val != null)
                    {
                        ReportSetting = (StampeData)Newtonsoft.Json.JsonConvert.DeserializeObject<StampeData>(val.PlainText);


                        if (valSezione?.ValoreAttributoElencoId == BuiltInCodes.SectionItemsId.Gantt || valSezione?.ValoreAttributoElencoId == BuiltInCodes.SectionItemsId.FogliDiCalcolo)
                        {
                            //ATTRIBUTI LAYOUT PAGINA
                            Valore Dimensioni = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.DimensioniPagina, true, true);
                            //Valore Orientamento = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Orientamento, true, true); 
                            ValoreElenco Orientamento = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Orientamento, true, true);

                            //MARGINI
                            Valore MargineSuperiore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineSuperiore, true, true);
                            Valore MargineInferiore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineInferiore, true, true);
                            Valore MargineSinistro = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineSinistro, true, true);
                            Valore MargineDestro = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineDestro, true, true);

                            //ReportSetting = new StampeData();

                            if (valSezione?.ValoreAttributoElencoId == BuiltInCodes.SectionItemsId.Gantt)
                            {
                                GanttGenerator GanttGenerator = new GanttGenerator();
                                GanttGenerator.DataService = DataService;
                                GanttGenerator.GanttView = GanttView;
                                if (ReportSetting == null)
                                {
                                    ReportSetting = new StampeData();
                                    ReportSetting.GanttSetting = new GanttSetting();
                                }


                                //if (ReportSetting != null)
                                ReportSetting.ImagesForPage = GanttGenerator.GenerateGantt(Dimensioni.PlainText, Orientamento.ValoreAttributoElencoId.ToString(), MargineSuperiore.PlainText, MargineInferiore.PlainText, MargineSinistro.PlainText, MargineDestro.PlainText, ReportSetting);
                                //else
                                //    ReportSetting = new StampeData();
                            }
                            if (valSezione?.ValoreAttributoElencoId == BuiltInCodes.SectionItemsId.FogliDiCalcolo)
                            {
                                ReportWizardSettingFogliDiCalcoloView reportWizardSettingFogliDiCalcoloView = new ReportWizardSettingFogliDiCalcoloView(FogliDiCalcoloView);
                                reportWizardSettingFogliDiCalcoloView.DataService = DataService;
                                reportWizardSettingFogliDiCalcoloView.WindowService = WindowService;
                                reportWizardSettingFogliDiCalcoloView.MainOperation = MainOperation;
                                reportWizardSettingFogliDiCalcoloView.ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint = ReportSetting?.FoglioDiCalcoloSetting?.SheetNameToPrint;
                                reportWizardSettingFogliDiCalcoloView.ReportSetting.FoglioDiCalcoloSetting.FitToPageKey = ReportSetting.FoglioDiCalcoloSetting?.FitToPageKey ?? 0;
                                reportWizardSettingFogliDiCalcoloView.Init();
                                if (ReportSetting != null)
                                    ReportSetting.ImagesForPage = reportWizardSettingFogliDiCalcoloView.GeneraFoglioDiCacolo(Dimensioni.PlainText, Orientamento.ValoreAttributoElencoId.ToString(), MargineSuperiore.PlainText, MargineInferiore.PlainText, MargineSinistro.PlainText, MargineDestro.PlainText, true);
                                else
                                    ReportSetting = new StampeData();
                            }

                        }
                        else
                        {

                            if (ReportSetting == null)
                            {
                                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Impostare almeno una colonna nel report"));
                                return;
                            }

                            SezioneKey = ReportSetting.SezioneKey;
                        }
                        ReportSettingViewHelper ReportSettingViewHelper = new ReportSettingViewHelper(DataService, SezioneKey);
                        ReportSettingViewHelper.EstraiAlberoDaAttributoSelezionato(ReportSetting.AttributiDaEstrarrePerDatasource);
                        ReportGenerator.ReportDataSourceGenerator.AttributiDaEstrarrePerDatasource = ReportSetting.AttributiDaEstrarrePerDatasource;
                        ReportGenerator.ReportDataSourceGenerator.RaggruppamentiDatasource = ReportSetting.RaggruppamentiDatasource;
                        ReportGenerator.ReportDataSourceGenerator.OrdinamentoCorpo = ReportSetting.OrdinamentoCorpo;
                        if (ReportSetting.IsTreeMaster)
                        {
                            if (!SezioneKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                                ReportGenerator.ReportDataSourceGenerator.AttributiDaEstrarrePerDatasource.Add(new AttributiUtilizzati()
                                {
                                    EntityType = SezioneKey,
                                    AttributiPerEntityType = new List<string>() { SezioneKey + StampeKeys.Const_Guid },
                                    AttributiCodicePathPerEntityType = new List<string>() { SezioneKey + StampeKeys.Const_Guid }
                                });
                        }
                        //ReportGenerator.ReportDataSourceGenerator.IsAllFieldRtfFormat = ReportSetting.IsAllFieldRtfFormat;
                        ReportGenerator.ReportDataSourceGenerator.IsAllFieldRtfFormat = UsaRtfAttributi;
                        ReportSetting.IsAllFieldRtfFormat = UsaRtfAttributi;
                    }

                    if (!string.IsNullOrEmpty(SezioneKey))
                    {
                        ReportGenerator.ReportDataSourceGenerator.CreateGenericDataSource(SezioneKey, false, MainOperation, valStampaVoci.ValoreAttributoElencoId);
                    }
                }
                else
                {
                    ReportSetting = new StampeData();
                }

                if (listaDocumentiDaProcessare.Count() > 1)
                    RiportaDatiPadre(ReportSetting, ReportSettingPadre, ContatoreReport);

                EstrazioneAttributiDocumentoPerReportSetting(entsHelper, ent, ReportSetting);

                ReportSetting.Corpo = ((ValoreTestoRtf)valCorpo).V;
                if (((ValoreTestoRtf)valCorpo).IsEmpty())
                //if (string.IsNullOrEmpty(((ValoreTestoRtf)valCorpo).PlainText))
                {
                    ReportSetting.IsCorpoEmpty = true;
                }

                //CREATE STRUCTURE
                if (ReportGenerator.ReportStructureGenerator == null)
                {
                    ReportGenerator.ReportStructureGenerator = new ReportStructureGenerator(ReportSetting, true, ReportSetting.IniziaDaPag);
                }
                else
                {
                    ReportGenerator.ReportStructureGenerator.ReportSetting = ReportSetting;
                }

                DataSet Dataset = ReportGenerator.ReportDataSourceGenerator.RetrieveDataSource(SezioneKey, DbName);

                //SE HO UN RAGGRUPPAMENTO A NULL ON GLI FACCIO FARE IL GRUPPO PER EVITARE I SOMMANO
                foreach (var RaggruppamentoToRemove in ReportGenerator.ReportDataSourceGenerator.RaggruppamentiDatasourceToRemove)
                {
                    Raggruppamenti TestaToRemove = ReportGenerator.ReportStructureGenerator.ReportSetting.Teste.Where(t => t.Attributo == RaggruppamentoToRemove.Attributo).FirstOrDefault();
                    Raggruppamenti CodaToRemove = ReportGenerator.ReportStructureGenerator.ReportSetting.Code.Where(t => t.Attributo == RaggruppamentoToRemove.Attributo).FirstOrDefault();
                    ReportGenerator.ReportStructureGenerator.ReportSetting.Teste.Remove(TestaToRemove);
                    ReportGenerator.ReportStructureGenerator.ReportSetting.Code.Remove(CodaToRemove);
                    ReportGenerator.ReportStructureGenerator.ReportSetting.RaggruppamentiDatasource.Remove(RaggruppamentoToRemove);
                }

                ReportGenerator.ReportStructureGenerator.ReportSetting.RaggruppamentiDatasource = ReportGenerator.ReportDataSourceGenerator.RaggruppamentiDatasource;

                if (entReport != null || !ReportSetting.IsCorpoEmpty)
                {
                    ReportGenerator.ReportStructureGenerator.CreateAndAddNewPage(Dataset, DbName, ReportGenerator.ReportDataSourceGenerator.ListParentItem, SezioneKey, ReportSetting.IsTreeMaster, MainOperation, ContatoreReport, null, Bordatura, printOnPreviousPage, DataService);
                    ContatoreReport++;
                }

                ContatoreLista++;
                

            EndCreation:

                IniziaDaPagIniziale = ReportSetting.IniziaDaPag;
                ReportGenerator.ReportDataSourceGenerator = new ReportDataSourceGenerator();
                ReportGenerator.ReportDataSourceGenerator.DataService = DataService;
            }
        }

        private void RiportaDatiPadre(StampeData reportSetting, StampeData reportSettingIniziale, int contatoreReport)
        {
            if (reportSetting.AltezzaPagina == 0) { reportSetting.AltezzaPagina = reportSettingIniziale.AltezzaPagina; }
            if (reportSetting.LarghezzaPagina == 0) { reportSetting.LarghezzaPagina = reportSettingIniziale.LarghezzaPagina; }
            if (string.IsNullOrEmpty(reportSetting.Orientamento)) { reportSetting.Orientamento = reportSettingIniziale.Orientamento; }
            if (reportSetting.MargineDestro == 0) { reportSetting.MargineDestro = reportSettingIniziale.MargineDestro; }
            if (reportSetting.MargineInferiore == 0) { reportSetting.MargineInferiore = reportSettingIniziale.MargineInferiore; }
            if (reportSetting.MargineSinistro == 0) { reportSetting.MargineSinistro = reportSettingIniziale.MargineSinistro; }
            if (reportSetting.MargineSuperiore == 0) { reportSetting.MargineSuperiore = reportSettingIniziale.MargineSuperiore; }
            int contatore = 0;

            if (reportSettingIniziale.IntestazioniDocumento != null)
            {
                if (reportSetting.IntestazioniDocumento == null)
                {
                    reportSetting.IntestazioniDocumento = new List<Intestazione>();
                    reportSetting.IntestazioniDocumento.Add(new Intestazione());
                    reportSetting.IntestazioniDocumento.Add(new Intestazione());
                    reportSetting.IntestazioniDocumento.Add(new Intestazione());
                }
                if (reportSetting.PiePaginaDocumento == null)
                {
                    reportSetting.PiePaginaDocumento = new List<Intestazione>();
                    reportSetting.PiePaginaDocumento.Add(new Intestazione());
                    reportSetting.PiePaginaDocumento.Add(new Intestazione());
                    reportSetting.PiePaginaDocumento.Add(new Intestazione());
                }

                foreach (var intestazione in reportSetting.IntestazioniDocumento)
                {
                    intestazione.Attributo = reportSettingIniziale.IntestazioniDocumento[contatore].Attributo;
                    intestazione.Etichetta = reportSettingIniziale.IntestazioniDocumento[contatore].Etichetta;
                    intestazione.Size = reportSettingIniziale.IntestazioniDocumento[contatore].Size;
                    intestazione.StileCarattere = reportSettingIniziale.IntestazioniDocumento[contatore].StileCarattere;
                    contatore++;
                }
                contatore = 0;
                foreach (var PiePagina in reportSetting.PiePaginaDocumento)
                {
                    PiePagina.Attributo = reportSettingIniziale.PiePaginaDocumento[contatore].Attributo;
                    PiePagina.Etichetta = reportSettingIniziale.PiePaginaDocumento[contatore].Etichetta;
                    PiePagina.Size = reportSettingIniziale.PiePaginaDocumento[contatore].Size;
                    PiePagina.StileCarattere = reportSettingIniziale.PiePaginaDocumento[contatore].StileCarattere;
                    contatore++;
                }
            }

            reportSetting.Intestazione = reportSettingIniziale.Intestazione;
            reportSetting.PiePagina = reportSettingIniziale.PiePagina;
            reportSetting.Firme = reportSettingIniziale.Firme;
            reportSetting.IsIntestazioneEmpty = reportSettingIniziale.IsIntestazioneEmpty;
            reportSetting.IsPiePaginaEmpty = reportSettingIniziale.IsPiePaginaEmpty;
            reportSetting.IsFirmeEmpty = reportSettingIniziale.IsFirmeEmpty;
        }
        private void EstrazioneAttributiDocumentoPerReportSetting(EntitiesHelper entsHelper, Entity ent, StampeData reportSetting)
        {
            //ATTRIBUTI LAYOUT PAGINA
            Valore Dimensioni = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.DimensioniPagina, true, true);
            //Valore Orientamento = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Orientamento, true, true); 
            ValoreElenco Orientamento = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Orientamento, true, true);

            //MARGINI
            Valore MargineSuperiore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineSuperiore, true, true);
            Valore MargineInferiore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineInferiore, true, true);
            Valore MargineSinistro = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineSinistro, true, true);
            Valore MargineDestro = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineDestro, true, true);

            //RTF
            ValoreTestoRtf valIntestazione = (ValoreTestoRtf)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Intestazione, false, true);
            ValoreTestoRtf valPiePagina = (ValoreTestoRtf)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.PiePagina, false, true);
            ValoreTestoRtf valFirme = (ValoreTestoRtf)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Firme, false, true);

            //NUMERAZIONE
            var IniziaDaPag = entsHelper.GetValoreAttributo(ent, string.Join("_", BuiltInCodes.Attributo.IniziaDaPag), true, true);

            if (Dimensioni.PlainText.Contains("A3")) { reportSetting.AltezzaPagina = 420; reportSetting.LarghezzaPagina = 297; }
            if (Dimensioni.PlainText.Contains("A4")) { reportSetting.AltezzaPagina = 297; reportSetting.LarghezzaPagina = 210; }
            if (Dimensioni.PlainText.Contains("A5")) { reportSetting.AltezzaPagina = 210; reportSetting.LarghezzaPagina = 148; }

            //reportSetting.Orientamento = Orientamento.PlainText;
            reportSetting.Orientamento = Orientamento.ValoreAttributoElencoId.ToString();

            if (string.IsNullOrEmpty(MargineSuperiore.PlainText)) { reportSetting.MargineSuperiore = 0; } else { reportSetting.MargineSuperiore = Convert.ToInt16(MargineSuperiore.PlainText) * 10; }
            if (string.IsNullOrEmpty(MargineInferiore.PlainText)) { reportSetting.MargineInferiore = 0; } else { reportSetting.MargineInferiore = Convert.ToInt16(MargineInferiore.PlainText) * 10; }
            if (string.IsNullOrEmpty(MargineSinistro.PlainText)) { reportSetting.MargineSinistro = 0; } else { reportSetting.MargineSinistro = Convert.ToInt16(MargineSinistro.PlainText) * 10; }
            if (string.IsNullOrEmpty(MargineDestro.PlainText)) { reportSetting.MargineDestro = 0; } else { reportSetting.MargineDestro = Convert.ToInt16(MargineDestro.PlainText) * 10; }

            if (!String.IsNullOrWhiteSpace(IniziaDaPag.PlainText))
            {
                reportSetting.IniziaDaPag = Convert.ToInt32(IniziaDaPag.PlainText);
            }

            if (!valIntestazione.IsEmpty())
            {
                reportSetting.Intestazione = valIntestazione.V;
                reportSetting.IsIntestazioneEmpty = false;
            }
            else
            {
                reportSetting.IsIntestazioneEmpty = true;
            }

            if (!valPiePagina.IsEmpty())
            {
                reportSetting.PiePagina = valPiePagina.V;
                reportSetting.IsPiePaginaEmpty = false;
            }
            else
            {
                reportSetting.IsPiePaginaEmpty = true;
            }

            if (!valFirme.IsEmpty())
            {

                reportSetting.Firme = valFirme.V;
                reportSetting.IsFirmeEmpty = false;
            }
            else
            {
                if (reportSetting.IsFirmeEmpty && valFirme.IsEmpty())
                {
                    reportSetting.IsFirmeEmpty = true;
                }
                else
                {
                    reportSetting.IsFirmeEmpty = false;
                }
            }
        }

        private void EstrazioneAttributiDocumentoPadre(EntitiesHelper entsHelper, Entity ent, StampeData reportSetting, List<Entity> ListaDocumentiDaProcessare, int ContatoreLista)
        {
            bool IsIntesazioneEmpty = false;
            bool IsPiePaginaEmpty = false;
            bool IsFirmeEmpty = false;

            //ATTRIBUTI LAYOUT PAGINA
            Valore Dimensioni = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.DimensioniPagina, true, true);
            //Valore Orientamento = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Orientamento, true, true);
            ValoreElenco Orientamento = (ValoreElenco)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Orientamento, true, true);

            //MARGINI
            Valore MargineSuperiore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineSuperiore, true, true);
            Valore MargineInferiore = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineInferiore, true, true);
            Valore MargineSinistro = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineSinistro, true, true);
            Valore MargineDestro = entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.MargineDestro, true, true);

            //RTF
            ValoreTestoRtf valIntestazione = (ValoreTestoRtf)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Intestazione, false, true);
            ValoreTestoRtf valPiePagina = (ValoreTestoRtf)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.PiePagina, false, true);
            ValoreTestoRtf valFirme = (ValoreTestoRtf)entsHelper.GetValoreAttributo(ent, BuiltInCodes.Attributo.Firme, false, true);

            //NUMERAZIONE
            var IniziaDaPag = entsHelper.GetValoreAttributo(ent, string.Join("_", BuiltInCodes.Attributo.IniziaDaPag), true, true);

            if (Dimensioni.PlainText.Contains("A3")) { reportSetting.AltezzaPagina = 420; reportSetting.LarghezzaPagina = 297; }
            if (Dimensioni.PlainText.Contains("A4")) { reportSetting.AltezzaPagina = 297; reportSetting.LarghezzaPagina = 210; }
            if (Dimensioni.PlainText.Contains("A5")) { reportSetting.AltezzaPagina = 210; reportSetting.LarghezzaPagina = 148; }

            //reportSetting.Orientamento = Orientamento.PlainText;
            reportSetting.Orientamento = Orientamento.ValoreAttributoElencoId.ToString();

            if (string.IsNullOrEmpty(MargineSuperiore.PlainText)) { reportSetting.MargineSuperiore = 0; } else { reportSetting.MargineSuperiore = Convert.ToInt16(MargineSuperiore.PlainText) * 10; }
            if (string.IsNullOrEmpty(MargineInferiore.PlainText)) { reportSetting.MargineInferiore = 0; } else { reportSetting.MargineInferiore = Convert.ToInt16(MargineInferiore.PlainText) * 10; }
            if (string.IsNullOrEmpty(MargineSinistro.PlainText)) { reportSetting.MargineSinistro = 0; } else { reportSetting.MargineSinistro = Convert.ToInt16(MargineSinistro.PlainText) * 10; }
            if (string.IsNullOrEmpty(MargineDestro.PlainText)) { reportSetting.MargineDestro = 0; } else { reportSetting.MargineDestro = Convert.ToInt16(MargineDestro.PlainText) * 10; }

            if (!String.IsNullOrWhiteSpace(IniziaDaPag.PlainText))
            {
                reportSetting.IniziaDaPag = Convert.ToInt32(IniziaDaPag.PlainText);
            }

            if (valIntestazione.IsEmpty())
            //if (String.IsNullOrEmpty(valIntestazione.PlainText))
            {
                IsIntesazioneEmpty = true;
            }
            if (valPiePagina.IsEmpty())
            //if (String.IsNullOrEmpty(valPiePagina.PlainText))
            {
                IsPiePaginaEmpty = true;
            }
            //if (String.IsNullOrEmpty(valFirme.PlainText))
            //{
            //    IsFirmeEmpty = true;
            //}
            if (valFirme.IsEmpty())
            {
                IsFirmeEmpty = true;
            }

            reportSetting.Intestazione = valIntestazione.V;
            reportSetting.PiePagina = valPiePagina.V;
            reportSetting.Firme = valFirme.V;

            for (int i = ContatoreLista; i >= 0; i--)
            {
                DocumentiItem DocumentoPadrePrecedente = (DocumentiItem)ListaDocumentiDaProcessare.ElementAt(i);
                if (DocumentoPadrePrecedente.IsParent)
                {
                    if (IsIntesazioneEmpty)
                    {
                        ValoreTestoRtf valIntestazionePrecedente = (ValoreTestoRtf)entsHelper.GetValoreAttributo(DocumentoPadrePrecedente, BuiltInCodes.Attributo.Intestazione, false, true);
                        if (!valIntestazionePrecedente.IsEmpty())
                        //if (!String.IsNullOrEmpty(valIntestazionePrecedente.PlainText))
                        {
                            reportSetting.Intestazione = valIntestazionePrecedente.V;
                            reportSetting.IsIntestazioneEmpty = false;
                            IsIntesazioneEmpty = false;
                        }
                        else
                        {
                            reportSetting.IsIntestazioneEmpty = true;
                        }
                    }
                    if (IsPiePaginaEmpty)
                    {
                        ValoreTestoRtf valPiePaginaPrecedente = (ValoreTestoRtf)entsHelper.GetValoreAttributo(DocumentoPadrePrecedente, BuiltInCodes.Attributo.PiePagina, false, true);
                        if (!valPiePaginaPrecedente.IsEmpty())
                        //if (!String.IsNullOrEmpty(valPiePaginaPrecedente.PlainText))
                        {
                            reportSetting.PiePagina = valPiePaginaPrecedente.V;
                            reportSetting.IsPiePaginaEmpty = false;
                            IsPiePaginaEmpty = false;
                        }
                        else
                        {
                            reportSetting.IsFirmeEmpty = true;
                        }
                    }
                    if (IsFirmeEmpty)
                    {
                        ValoreTestoRtf valFirmePrecedente = (ValoreTestoRtf)entsHelper.GetValoreAttributo(DocumentoPadrePrecedente, BuiltInCodes.Attributo.Firme, false, true);
                        if (!valFirmePrecedente.IsEmpty())
                        //if (!String.IsNullOrEmpty(valFirmePrecedente.PlainText))
                        {
                            reportSetting.Firme = valFirmePrecedente.V;
                            reportSetting.IsFirmeEmpty = false;
                            IsFirmeEmpty = false;
                        }
                        else
                        {
                            reportSetting.IsFirmeEmpty = true;
                        }
                    }

                    if (!IsIntesazioneEmpty && !IsPiePaginaEmpty && !IsFirmeEmpty)
                    {
                        break;
                    }
                }
            }
        }

        private void AssegnaStile(Intestazione Elemento, Entity Stile)
        {
            if (Stile != null)
            {
                Elemento.StileCarattere = new ProprietaCarattere();
                Elemento.StileCarattere.FontFamily = Stile.Attributi[BuiltInCodes.Attributo.Carattere].Valore.PlainText;
                Elemento.StileCarattere.Size = Stile.Attributi[BuiltInCodes.Attributo.DimensioneCarattere].Valore.PlainText;
                Elemento.StileCarattere.IsGrassetto = Convert.ToBoolean(Stile.Attributi[BuiltInCodes.Attributo.Grassetto].Valore.PlainText);
                Elemento.StileCarattere.IsBarrato = Convert.ToBoolean(Stile.Attributi[BuiltInCodes.Attributo.Barrato].Valore.PlainText);
                Elemento.StileCarattere.IsSottolineato = Convert.ToBoolean(Stile.Attributi[BuiltInCodes.Attributo.Sottolineato].Valore.PlainText);
                Elemento.StileCarattere.ColorCharacther = ColorInfo.ColoriInstallatiInMacchina.Where(c => c.Name == Stile.Attributi[BuiltInCodes.Attributo.ColoreCarattere].Valore.PlainText).FirstOrDefault();
                Elemento.StileCarattere.ColorBackground = ColorInfo.ColoriInstallatiInMacchina.Where(c => c.Name == Stile.Attributi[BuiltInCodes.Attributo.ColoreSfondo].Valore.PlainText).FirstOrDefault();
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsStarWizardBtnDisable));
        }

        public bool IsReportPreviewBtnVisible { get => (DataService != null) ? !DataService.IsReadOnly : false; }
        public bool IsStarWizardBtnVisible { get => (DataService != null) ? !DataService.IsReadOnly : false; }

        public bool IsStarWizardBtnDisable
        {
            get
            {
                if (ItemsView != null)
                {
                    if (ItemsView.DataService != null)
                    {
                        Entity DocumentoSelezionato = ItemsView.DataService.GetEntityById(BuiltInCodes.EntityType.Documenti, ItemsView.SelectedEntityId);
                        if (DocumentoSelezionato != null)
                        {
                            Valore val = DocumentoSelezionato.GetValoreAttributo(string.Join("_", BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Id), false, true);
                            if (val != null)
                            {
                                if (val.PlainText != Guid.Empty.ToString())
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }


    }
     
    public class DocumentiItemsViewVirtualized : MasterDetailTreeViewVirtualized
    {
        public DocumentiView DocumentiViewOwner { get; set; }
        public DocumentiItemsViewVirtualized(MasterDetailTreeView owner) : base(owner)
        {
            DocumentiViewOwner = owner as DocumentiView;
        }

        public override void Init()
        {
            base.Init();

            _loadingEntities = new List<TreeEntity>();

            EntityParentType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.DocumentiParent];
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Documenti];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override TreeEntityView NewItemView(TreeEntity entity)
        {
            return new DocumentiItemView(this, entity);
        }

        public override bool SelectEntityView(EntityView entView)
        {
            if (entView != null)
            {
                if (entView.Entity != null)
                {
                    UpdateUI();
                }
            }

            return base.SelectEntityView(entView);
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            if (DocumentiViewOwner != null)
            {
                DocumentiViewOwner.UpdateUI();
            }
        }
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
                ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Report };

                //aggiungi in coda
                action.ActionName = ActionName.ENTITY_ADD;

                ////Setto il valore dell'attributo ReportWizardSetting

                action.NestedActions.AddRange(Actions);

                mar = CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    //await UpdateCache();
                    MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.Report });
                }

                RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));



                ModelAction ActionsDocumentiReportGuid = new ModelAction()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Documenti,
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    AttributoCode = string.Join("_", BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Id)
                };

                ActionsDocumentiReportGuid.NewValore = new ValoreGuid() { V = mar.NewId };

                ActionsDocumentiReportGuid.EntitiesId = new HashSet<Guid>() { new Guid(this.SelectedEntityId.ToString()) };
                mar = CommitAction(ActionsDocumentiReportGuid);
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

        public override IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            return base.LoadRange(startIndex, count, sortDescriptions, out overallCount);
        }

        protected override void LoadingStateChanged(object sender, EventArgs e)
        {
            base.LoadingStateChanged(sender, e);
        }

        public override void Load()
        {
            base.Load();
        }

        public override void ReplaceValore(ValoreView valoreView)
        {
            if (SelectedTreeEntityView.IsParent)
                return;

            base.ReplaceValore(valoreView);
        }


    }
}
