using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class PreviewGanttView : NotificationBase
    {
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
                    IsEnabled = true;
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
                    IsEnabled = true;
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
                    IsEnabled = true;
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
                    IsEnabled = true;
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

        private bool _IsEnabled;

        public bool IsEnabled
        {
            get
            {

                return _IsEnabled;
            }
            set
            {
                SetProperty(ref _IsEnabled, value);
            }
        }

        public TimeSpan Zoom { get; set; }
        public List<int> ColumnWidth { get; set; }

        public PreviewGanttSettingView previewGanttSettingView { get; set; }

        //private DevExpress.Xpf.Gantt.GanttView ganttView;
        private GanttView view;
        public bool IsEnablePreview = false;
        private GanttGenerator GanttGenerator;
        public DateTime DataDaDefault { get; set; }
        public DateTime DataADefault { get; set; }
        public int NumberDaDefault { get; set; }
        public int NumberADefault { get; set; }
        public PreviewGanttView(DevExpress.Xpf.Gantt.GanttView GanttView, GanttView View)
        {
            previewGanttSettingView = new PreviewGanttSettingView();
            GanttGenerator = new GanttGenerator();
            GanttGenerator.DataService = (Model.ClientDataService)View.DataService;
            GanttGenerator.GanttView = View;
            //ganttView = GanttView;
            view = View;
        }

        public void Init()
        {
            DateTime DataInizio = new DateTime();
            DateTime DataFine = new DateTime();

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

            if (view.IsActiveProgressiva)
            {
                DataInizio = GetDataSuScalaProgressiva(NumberDa);
                DataFine = GetDataSuScalaProgressiva(NumberA);
                DataDa = DataInizio;
                DataA = DataFine;
            }

            if (view.IsActiveNascondiDate)
            {
                DataInizio = GetDataSuScalaFeriale(NumberDa);
                DataFine = GetDataSuScalaFeriale(NumberA);
                DataDa = DataInizio;
                DataA = DataFine;
            }

            if (view.IsActiveCalendario)
            {
                DataInizio = DataDa;
                DataFine = DataA;
            }

            //ganttView.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataInizio, DataFine);

            //PreviewResult = new DevExpress.Xpf.Printing.PrintableControlLink(ganttView);
            //link = new DevExpress.Xpf.Printing.PrintableControlLink(GanttView.TableView as DevExpress.Xpf.Grid.TreeListView);

            var previewResult = GanttGenerator.GetPreviewResult(DataInizio, DataFine, Zoom, ColumnWidth, 1, 0);
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

                IsEnabled = false;
            }


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
            Init();
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
                IsEnabled = true;
                //previewGanttSettingView.Stored = false;
            //}
        }

        public void GetDefaultDa()
        {
            DataDa = DataDaDefault;
            NumberDa = NumberDaDefault;
            IsEnabled = true;
        }

        public void GetDefaultA()
        {
            DataA = DataADefault;
            NumberA = NumberADefault;
            IsEnabled = true;
        }

        public void Preview()
        {
            Init();
        }

        public void AcceptButton()
        {

        }

    }
}
