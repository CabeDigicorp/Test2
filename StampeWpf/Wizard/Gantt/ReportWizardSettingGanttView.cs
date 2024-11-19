using AttivitaWpf.View;
using Commons;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StampeWpf
{
    public class ReportWizardSettingGanttView : NotificationBase
    {
        public ClientDataService DataService { get; set; }//ref
        public IEntityWindowService WindowService { get; set; }//ref
        public IMainOperation MainOperation { get; set; }//ref
        public StampeData ReportSetting { get; set; }
        public string CodiceReport { get; set; }

        private DevExpress.Xpf.Printing.PrintableControlLink _PreviewResult;
        public DevExpress.Xpf.Printing.PrintableControlLink PreviewResult
        {
            get
            {
                return _PreviewResult;
            }
            set
            {
                SetProperty(ref _PreviewResult, value);
            }
        }

        private DateTime _DataDa;
        public DateTime DataDa
        {
            get
            {

                return _DataDa;
            }
            set
            {
                SetProperty(ref _DataDa, value);
                if (IsEnablePreview)
                    Preview();
                //    IsEnabled = true;
            }
        }

        private DateTime _DataA;

        public DateTime DataA
        {
            get
            {

                return _DataA;
            }
            set
            {
                SetProperty(ref _DataA, value);
                if (IsEnablePreview)
                    Preview();
                //    IsEnabled = true;
            }
        }

        private int _NumberDa;
        public int NumberDa
        {
            get
            {

                return _NumberDa;
            }
            set
            {
                SetProperty(ref _NumberDa, value);
                if (IsEnablePreview)
                    Preview();
                //    IsEnabled = true;
            }
        }

        private int _NumberA;

        public int NumberA
        {
            get
            {

                return _NumberA;
            }
            set
            {
                SetProperty(ref _NumberA, value);
                if (IsEnablePreview)
                    Preview();
                //    IsEnabled = true;
            }
        }

        private System.Windows.Visibility _VisibilityData;

        public System.Windows.Visibility VisibilityData
        {
            get
            {

                return _VisibilityData;
            }
            set
            {
                SetProperty(ref _VisibilityData, value);
                RaisePropertyChanged("VisibilityNumber");
            }
        }
        public System.Windows.Visibility VisibilityNumber
        {
            get
            {
                if (VisibilityData == System.Windows.Visibility.Visible)
                {
                    return System.Windows.Visibility.Collapsed;
                }
                else
                {
                    return System.Windows.Visibility.Visible;
                }
            }
        }

        //private bool _IsEnabled;

        //public bool IsEnabled
        //{
        //    get
        //    {

        //        return _IsEnabled;
        //    }
        //    set
        //    {
        //        SetProperty(ref _IsEnabled, value);
        //        if (value)
        //            VisibilityDocumentPreviewControl = System.Windows.Visibility.Collapsed;
        //    }
        //}

        //private System.Windows.Visibility _VisibilityDocumentPreviewControl;

        //public System.Windows.Visibility VisibilityDocumentPreviewControl
        //{
        //    get
        //    {

        //        return _VisibilityDocumentPreviewControl;
        //    }
        //    set
        //    {
        //        SetProperty(ref _VisibilityDocumentPreviewControl, value);
        //    }
        //}

        public TimeSpan Zoom { get; set; }
        public List<int> ColumnWidth { get; set; }

        public PreviewGanttSettingView previewGanttSettingView { get; set; }
        public PreviewGanttScalaView previewGanttScalaView { get; set; }

        //private DevExpress.Xpf.Gantt.GanttView ganttView;
        private GanttView view;
        public bool IsEnablePreview = false;
        private GanttGenerator GanttGenerator;
        public DateTime DataDaDefault { get; set; }
        public DateTime DataADefault { get; set; }
        public int NumberDaDefault { get; set; }
        public int NumberADefault { get; set; }
        public double ZoomFactor { get; set; }
        public int AdjustToPage { get; set; }
        //public ReportWizardSettingGanttView(DevExpress.Xpf.Gantt.GanttView GanttView, GanttView View)
        public ReportWizardSettingGanttView(GanttView View)
        {
            previewGanttSettingView = new PreviewGanttSettingView();
            previewGanttScalaView = new PreviewGanttScalaView();
            ReportSetting = new StampeData();
            ReportSetting.GanttSetting = new GanttSetting();
            GanttGenerator = new GanttGenerator();
            GanttGenerator.DataService = (Model.ClientDataService)View.DataService;
            GanttGenerator.GanttView = View;
            //ganttView = GanttView;
            view = View;
            //VisibilityDocumentPreviewControl = System.Windows.Visibility.Visible;
        }

        public bool Init()
        {

            try
            {

                if (ColumnWidth == null)
                {
                    ColumnWidth = new List<int>();
                    ColumnWidth.Add(70);
                    ColumnWidth.Add(300);
                    ColumnWidth.Add(70);
                    ColumnWidth.Add(70);
                    ColumnWidth.Add(70);
                    ColumnWidth.Add(70);
                }

                if (ColumnWidth.Count() == 5)
                {
                    ColumnWidth.Add(70);
                }

                for (int i = 0; i < ColumnWidth.Count(); i++)
                {
                    if (i == 0)
                        previewGanttSettingView.Codice = ColumnWidth[i];
                    if (i == 1)
                        previewGanttSettingView.Descrizione = ColumnWidth[i];
                    if (i == 2)
                        previewGanttSettingView.Durata = ColumnWidth[i];
                    if (i == 3)
                        previewGanttSettingView.Duratacalendario = ColumnWidth[i];
                    if (i == 4)
                        previewGanttSettingView.Inizio = ColumnWidth[i];
                    if (i == 5)
                        previewGanttSettingView.Fine = ColumnWidth[i];
                }

                DateTime DataInizio = new DateTime();
                DateTime DataFine = new DateTime();

                //DataInizio = view.GanttData.GanttDateFrom;
                //DataFine = view.GanttData.GanttDateTo;
                if (ReportSetting.GanttSetting != null)
                {
                    DataInizio = ReportSetting.GanttSetting.GanttDateFrom;
                    DataFine = ReportSetting.GanttSetting.GanttDateTo;
                    ZoomFactor = ReportSetting.GanttSetting.ZoomFactor;
                    AdjustToPage = ReportSetting.GanttSetting.AdjustToPage;
                }

                if (DataInizio == new DateTime())
                {
                    DataInizio = view.WBSView.GetMinDataInizioWBSItems(true);
                }
                DateTime DatafineMax = view.WBSView.GetMaxDataFineWBSItems(true).AddDays(1);

                if (DataFine == new DateTime())
                {
                    DataFine = DatafineMax;
                    DataFine = DataFine.AddDays(1);
                }

                if (DatafineMax > DataFine)
                {
                    DataFine = DatafineMax;
                }

                if (view.IsActiveProgressiva)
                {
                    DataDaDefault = view.GetDataAnonima(view.WBSView.GetMinDataInizioWBSItems(true));
                    DataADefault = view.GetDataAnonima(view.WBSView.GetMaxDataFineWBSItems(true));
                    NumberDaDefault = view.GetNumberDataSuScalaProgressiva(DataDaDefault);
                    NumberADefault = view.GetNumberDataSuScalaProgressiva(DataADefault);
                    NumberDa = NumberDaDefault;
                    NumberA = NumberADefault + 1;
                    VisibilityData = System.Windows.Visibility.Collapsed;
                    DataInizio = GetDataSuScalaProgressiva(NumberDa);
                    DataFine = GetDataSuScalaProgressiva(NumberA);
                    DataDa = DataInizio;
                    DataA = DataFine;
                    //DataDaDefault = view.GetDataAnonima(view.WBSView.GetMinDataInizioWBSItems(true));
                    //DataADefault = view.GetDataAnonima(view.WBSView.GetMaxDataFineWBSItems(true));
                    //NumberDaDefault = view.GetNumberDataSuScalaProgressiva(DataDaDefault);
                    //NumberADefault = view.GetNumberDataSuScalaProgressiva(DataADefault);
                    //NumberDa = NumberDaDefault;
                    //NumberA = NumberADefault;
                    //VisibilityData = System.Windows.Visibility.Collapsed;
                }

                if (view.IsActiveNascondiDate)
                {
                    DataDaDefault = view.WBSView.GetMinDataInizioWBSItems(true);
                    DataADefault = view.WBSView.GetMaxDataFineWBSItems(true);
                    NumberDaDefault = view.GetNumberDataSuScalaProgressiva(DataDaDefault);
                    NumberADefault = view.GetNumberDataSuScalaProgressiva(DataADefault);
                    NumberDa = NumberDaDefault;
                    NumberA = NumberADefault + 1;
                    VisibilityData = System.Windows.Visibility.Collapsed;
                    DataInizio = GetDataSuScalaFeriale(NumberDa);
                    DataFine = GetDataSuScalaFeriale(NumberA);
                    DataDa = DataInizio;
                    DataA = DataFine;
                    //DataDaDefault = view.WBSView.GetMinDataInizioWBSItems(true);
                    //DataADefault = view.WBSView.GetMaxDataFineWBSItems(true);
                    //NumberDaDefault = view.GetNumberDataSuScalaProgressiva(DataDaDefault);
                    //NumberADefault = view.GetNumberDataSuScalaProgressiva(DataADefault);
                    //NumberDa = NumberDaDefault;
                    //NumberA = NumberADefault;
                    //VisibilityData = System.Windows.Visibility.Collapsed;
                }

                if (view.IsActiveCalendario)
                {
                    DataDa = view.WBSView.GetMinDataInizioWBSItems(true);
                    DataA = view.WBSView.GetMaxDataFineWBSItems(true);
                    DataDaDefault = DataDa;
                    DataADefault = DataA;
                    DataInizio = DataDa;
                    DataFine = DataA.AddDays(1);
                }


                //ZoomFactor = view.GanttData.ZoomFactor;
                //AdjustToPage = view.GanttData.AdjustToPage;
                if (AdjustToPage == 0 && ZoomFactor == 0)
                    ZoomFactor = 1;

                //ganttView.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataInizio, DataFine);

                //PreviewResult = new DevExpress.Xpf.Printing.PrintableControlLink(ganttView);
                //link = new DevExpress.Xpf.Printing.PrintableControlLink(GanttView.TableView as DevExpress.Xpf.Grid.TreeListView);
                var previewResult = GanttGenerator.GetPreviewResult(DataInizio, DataFine, Zoom, ColumnWidth, ZoomFactor, AdjustToPage);

                if (previewResult == null)
                    return false;


                PreviewResult = previewResult;

                PreviewResult.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);
                PreviewResult.Landscape = true;
                //PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.A3;
                PreviewResult.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A3;
                //PreviewResult.PrintingSystem.Document.AutoFitToPagesWidth = 1;
                PreviewResult.CreateDocument();

                Zoom = GanttGenerator.ZoomStored;


            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
            //IsEnabled = false;

            return true;
        }

        public void GenerateGantt()
        {
            if (view.IsActiveProgressiva)
            {
                DateTime DataInizio = GetDataSuScalaProgressiva(NumberDa);
                DateTime DataFine = GetDataSuScalaProgressiva(NumberA + 1);
                IsEnablePreview = false;
                DataDa = DataInizio;
                DataA = DataFine;
                IsEnablePreview = true;
            }

            if (view.IsActiveNascondiDate)
            {
                DateTime DataInizio = GetDataSuScalaFeriale(NumberDa);
                DateTime DataFine = GetDataSuScalaFeriale(NumberA + 1);
                IsEnablePreview = false;
                DataDa = DataInizio;
                DataA = DataFine;
                IsEnablePreview = true;
            }

            if (view.IsActiveCalendario)
            {
                IsEnablePreview = false;
                DataA = DataA.AddDays(1);
                IsEnablePreview = true;
            }

            var previewResult = GanttGenerator.GetPreviewResult(DataDa, DataA, Zoom, ColumnWidth, ZoomFactor,AdjustToPage);
            if (previewResult != null)
            {

                PreviewResult = previewResult;

                PreviewResult.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);
                PreviewResult.Landscape = true;
                //PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.A3;
                PreviewResult.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A3;
                //PreviewResult.PrintingSystem.Document.AutoFitToPagesWidth = 1;
                PreviewResult.CreateDocument();

                Zoom = GanttGenerator.ZoomStored;

                if (view.IsActiveCalendario)
                {
                    IsEnablePreview = false;
                    DataA = DataA.AddDays(-1);
                    IsEnablePreview = true;
                }
            }

            //IsEnabled = false;
        }

        private DateTime GetDataSuScalaProgressiva(int Number)
        {
            DateTime Date = new DateTime();
            int unitaTempo = view.ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key;
            switch (unitaTempo)
            {
                //ANNI
                case 0:
                    Date = KeyByValue(GanttView.ScalaNumericaAnonima, GanttView.ScalaNumericaAnonima.Values.Where(x => x.ProgressivoNumericoAnnoAnonima == Number).FirstOrDefault());
                    break;
                //MESI
                case 3:
                    Date = KeyByValue(GanttView.ScalaNumericaAnonima, GanttView.ScalaNumericaAnonima.Values.Where(x => x.ProgressivoNumericoMeseAnonima == Number).FirstOrDefault());
                    break;
                //SETTIMANE
                case 5:
                    Date = KeyByValue(GanttView.ScalaNumericaAnonima, GanttView.ScalaNumericaAnonima.Values.Where(x => x.ProgressivoNumericoSettimanaAnonima == Number).FirstOrDefault());
                    break;
                //GIORNI
                case 6:
                    Date = KeyByValue(GanttView.ScalaNumericaAnonima, GanttView.ScalaNumericaAnonima.Values.Where(x => x.ProgressivoNumericoGiornoAnonima == Number).FirstOrDefault());
                    break;
                //ORE
                case 7:
                    Date = KeyByValue(GanttView.ScalaNumericaAnonima, GanttView.ScalaNumericaAnonima.Values.Where(x => x.ProgressivoNumericoOraAnonima == Number).FirstOrDefault());
                    break;
                default:

                    break;
            }

            return Date;
        }

        private DateTime GetDataSuScalaFeriale(int Number)
        {
            DateTime Date = new DateTime();
            int unitaTempo = view.ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key;
            switch (unitaTempo)
            {
                //ANNI
                case 0:
                    Date = KeyByValue(GanttView.ScalaNumericaFeriale, GanttView.ScalaNumericaFeriale.Values.Where(x => x.ProgressivoNumericoAnno == Number).FirstOrDefault());
                    break;
                //MESI
                case 3:
                    Date = KeyByValue(GanttView.ScalaNumericaFeriale, GanttView.ScalaNumericaFeriale.Values.Where(x => x.ProgressivoNumericoMese == Number).FirstOrDefault());
                    break;
                //SETTIMANE
                case 5:
                    Date = KeyByValue(GanttView.ScalaNumericaFeriale, GanttView.ScalaNumericaFeriale.Values.Where(x => x.ProgressivoNumericoSettimana == Number).FirstOrDefault());
                    break;
                //GIORNI
                case 6:
                    Date = KeyByValue(GanttView.ScalaNumericaFeriale, GanttView.ScalaNumericaFeriale.Values.Where(x => x.ProgressivoNumericoGiorno == Number).FirstOrDefault());
                    break;
                //ORE
                case 7:
                    Date = KeyByValue(GanttView.ScalaNumericaFeriale, GanttView.ScalaNumericaFeriale.Values.Where(x => x.ProgressivoNumericoOra == Number).FirstOrDefault());
                    break;
                default:

                    break;
            }

            return Date;
        }

        public static T KeyByValue<T, W>(Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }

        public void DoZoom(bool IsIn)
        {
            GanttGenerator.ZoomActive = true;
            GanttGenerator.IsIn = IsIn;
            GenerateGantt();
        }

        public void UpdateColumnWidth()
        {
            //if (previewGanttSettingView.Stored)
            //{
                for (int i = 0; i < ColumnWidth.Count(); i++)
                {
                    if (i == 0)
                        ColumnWidth[i] = previewGanttSettingView.Codice;
                    if (i == 1)
                        ColumnWidth[i] = previewGanttSettingView.Descrizione;
                    if (i == 2)
                        ColumnWidth[i] = previewGanttSettingView.Durata;
                    if (i == 3)
                        ColumnWidth[i] = previewGanttSettingView.Duratacalendario;
                    if (i == 4)
                        ColumnWidth[i] = previewGanttSettingView.Inizio;
                    if (i == 5)
                        ColumnWidth[i] = previewGanttSettingView.Fine;
                }
                //IsEnabled = true;
                //previewGanttSettingView.Stored = false;
                Preview();
            //}
        }

        public void UpdateScale()
        {
            if (previewGanttScalaView.Stored)
            {
                if (previewGanttScalaView.IsZoomRadioButtonChecked)
                {
                    ZoomFactor = (double)previewGanttScalaView.SelectedItemFattoreZoom;
                    AdjustToPage = 0;
                }

                if (previewGanttScalaView.IsAdattaRadioButtonChecked)
                {
                    ZoomFactor = 0;
                    AdjustToPage = (int)previewGanttScalaView.SelectedItemAdattaAPagine;
                }
                //IsEnabled = true;
                Preview();
            }            
        }

        public void GetDefaultDa()
        {
            IsEnablePreview = false;
            DataDa = DataDaDefault;
            NumberDa = NumberDaDefault;
            IsEnablePreview = true;
            Preview();
            //IsEnabled = true;
        }

        public void GetDefaultA()
        {
            IsEnablePreview = false;
            DataA = DataADefault.AddDays(1);
            NumberA = NumberADefault + 1;
            IsEnablePreview = true;
            Preview();
            //IsEnabled = true;
        }

        public void Preview()
        {
            GenerateGantt();
            //VisibilityDocumentPreviewControl = System.Windows.Visibility.Visible;
        }

        public void AcceptButton()
        {
            //view.GanttData.GanttDateFrom = DataDa;
            //view.GanttData.GanttDateTo = DataA;
            //view.GanttData.GanttZoom = Zoom;
            //view.GanttData.ColumnWidth = ColumnWidth;
            //view.GanttData.AdjustToPage = AdjustToPage;
            //view.GanttData.ZoomFactor = ZoomFactor;
            //DataService.SetGanttData(view.GanttData);
            ReportSetting.GanttSetting.GanttDateFrom = DataDa;
            ReportSetting.GanttSetting.GanttDateTo = DataA;
            ReportSetting.GanttSetting.GanttZoom = Zoom;
            ReportSetting.GanttSetting.ColumnWidth = ColumnWidth;
            ReportSetting.GanttSetting.AdjustToPage = AdjustToPage;
            ReportSetting.GanttSetting.ZoomFactor = ZoomFactor;
        }

    }
}
