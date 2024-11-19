using CommonResources;
using Commons;
using DevExpress.Mvvm;
using DevExpress.Xpf.Gantt;
using MasterDetailModel;
using Model;
using Syncfusion.Windows.Controls.Gantt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class ScalaCronologicaView : NotificationBase
    {
        public IDataService DataService { get; set; }
        public bool FirstInitialization { get; set; }

        private ObservableCollection<Processo> _Tasks;
        public ObservableCollection<Processo> Tasks
        {
            get
            {

                return _Tasks;
            }
            set
            {
                if (SetProperty(ref _Tasks, value))
                    _Tasks = value;
            }
        }

        private ObservableCollection<WorkdayRule> _WorkdayRulesSource;
        public ObservableCollection<WorkdayRule> WorkdayRulesSource
        {
            get
            {

                return _WorkdayRulesSource;
            }
            set
            {
                if (SetProperty(ref _WorkdayRulesSource, value))
                    _WorkdayRulesSource = value;
            }
        }

        private int _TimescaleRulerCount;
        public int TimescaleRulerCount
        {
            get
            {

                return _TimescaleRulerCount;
            }
            set
            {
                if (SetProperty(ref _TimescaleRulerCount, value))
                    _TimescaleRulerCount = value;
            }
        }
        //public int TimescaleRulerCount { get;}

        public System.Windows.Media.Brush ColorBackground
        {
            get
            {
                return StileConPropieta?.ColorBackground;
            }
        }

        public System.Windows.Media.Brush ColorCharacther
        {
            get
            {
                return StileConPropieta?.ColorCharacther;
            }
        }

        public string FontFamily
        {
            get
            {
                return StileConPropieta?.FontFamily;
            }
        }

        public double FontSize
        {
            get
            {
                if (StileConPropieta == null)
                    return 9;
                else
                    return (double)StileConPropieta?.Size;
            }
        }

        public string FontWeight
        {
            get
            {
                if (StileConPropieta != null)
                {
                    if (StileConPropieta.Grassetto)
                        return "Bold";
                    return null;
                }
                return null;
            }
        }
        public string TextDecorations
        {
            get
            {
                if (StileConPropieta != null)
                {
                    if (StileConPropieta.Barrato)
                        return "Strikethrough";

                    if (StileConPropieta.Sottolineato)
                        return "Underline";
                    return null;
                }
                else
                    return null;
            }
        }

        public string FontStyle
        {
            get
            {
                if (StileConPropieta != null)
                {
                    if (StileConPropieta.Corsivo)
                        return "Italic";
                    return null;
                }
                return null;
            }
        }

        //private ObservableCollection<GanttScheduleRowInfo> _CustomSchedule;
        //public ObservableCollection<GanttScheduleRowInfo> CustomSchedule
        //{
        //    get
        //    {
        //        return _CustomSchedule;
        //    }

        //    set
        //    {
        //        if (SetProperty(ref _CustomSchedule, value))
        //            _CustomSchedule = value;
        //    }
        //}

        private Dictionary<int, string> _LayoutLivelli;
        public Dictionary<int, string> LayoutLivelli
        {
            get
            {
                return _LayoutLivelli;
            }
            set
            {
                if (SetProperty(ref _LayoutLivelli, value))
                    _LayoutLivelli = value;
            }
        }

        private KeyValuePair<int, string> _LayoutLivello;
        public KeyValuePair<int, string> LayoutLivello
        {
            get
            {
                return _LayoutLivello;
            }
            set
            {
                if (SetProperty(ref _LayoutLivello, value))
                    _LayoutLivello = value;
            }
        }

        private List<TabItemView> _TabItemViews;
        public List<TabItemView> TabItemViews
        {
            get
            {
                return _TabItemViews;
            }
            set
            {
                if (SetProperty(ref _TabItemViews, value))
                    _TabItemViews = value;
            }
        }

        private TabItemView _TabItemViewCorrente;
        public TabItemView TabItemViewCorrente
        {
            get
            {
                return _TabItemViewCorrente;
            }
            set
            {
                if (SetProperty(ref _TabItemViewCorrente, value))
                    _TabItemViewCorrente = value;
            }
        }

        private int _ZoomFactor;
        public int ZoomFactor
        {
            get
            {
                return _ZoomFactor;
            }
            set
            {
                if (SetProperty(ref _ZoomFactor, value))
                    _ZoomFactor = value;
            }
        }

        private bool _IsSeparatoreHoriz;
        public bool IsSeparatoreHoriz
        {
            get
            {
                return _IsSeparatoreHoriz;
            }
            set
            {
                if (SetProperty(ref _IsSeparatoreHoriz, value))
                {
                    _IsSeparatoreHoriz = value;
                    RaisePropertyChanged("BordiCelle");
                }

            }
        }

        private bool _IsSeparatoreVert;
        public bool IsSeparatoreVert
        {
            get
            {
                return _IsSeparatoreVert;
            }
            set
            {
                if (SetProperty(ref _IsSeparatoreVert, value))
                {
                    _IsSeparatoreVert = value;
                    RaisePropertyChanged("BordiCelle");
                }
            }
        }

        private Dictionary<int, string> _Allineamento;
        public Dictionary<int, string> Allineamento
        {
            get
            {
                return _Allineamento;
            }
            set
            {
                if (SetProperty(ref _Allineamento, value))
                    _Allineamento = value;
            }
        }

        private KeyValuePair<int, string> _SelectedAllineamento;
        public KeyValuePair<int, string> SelectedAllineamento
        {
            get
            {
                return _SelectedAllineamento;
            }
            set
            {
                if (SetProperty(ref _SelectedAllineamento, value))
                {
                    _SelectedAllineamento = value;
                    RaisePropertyChanged("AllineamentoTrigger");
                }

            }
        }

        private bool _IsEnabledSup;
        public bool IsEnabledSup
        {
            get
            {
                return _IsEnabledSup;
            }
            set
            {
                if (SetProperty(ref _IsEnabledSup, value))
                    _IsEnabledSup = value;
            }
        }

        private bool _IsEnabledInt;
        public bool IsEnabledInt
        {
            get
            {
                return _IsEnabledInt;
            }
            set
            {
                if (SetProperty(ref _IsEnabledInt, value))
                    _IsEnabledInt = value;
            }
        }

        private bool _IsEnabledInf;

        public bool IsEnabledInf
        {
            get
            {
                return _IsEnabledInf;
            }
            set
            {
                if (SetProperty(ref _IsEnabledInf, value))
                    _IsEnabledInf = value;
            }
        }

        private int _TabSelectedIndex;
        public int TabSelectedIndex
        {
            get
            {
                return _TabSelectedIndex;
            }
            set
            {
                if (SetProperty(ref _TabSelectedIndex, value))
                {
                    _TabSelectedIndex = value;
                    TabSelectionChanged();
                }

            }
        }

        public System.Windows.HorizontalAlignment AllineamentoTrigger
        {
            get
            {
                if (SelectedAllineamento.Key == 0)
                    return System.Windows.HorizontalAlignment.Left;
                if (SelectedAllineamento.Key == 1)
                    return System.Windows.HorizontalAlignment.Center;
                if (SelectedAllineamento.Key == 2)
                    return System.Windows.HorizontalAlignment.Right;
                return System.Windows.HorizontalAlignment.Center;
            }
        }

        public System.Windows.Thickness BordiCelle
        {
            get
            {
                if (IsSeparatoreHoriz && IsSeparatoreVert)
                    return new System.Windows.Thickness(1, 1, 0, 0);
                if (IsSeparatoreHoriz && !IsSeparatoreVert)
                    return new System.Windows.Thickness(0, 1, 0, 0);
                if (!IsSeparatoreHoriz && IsSeparatoreVert)
                    return new System.Windows.Thickness(1, 0, 0, 0);
                if (!IsSeparatoreHoriz && !IsSeparatoreVert)
                    return new System.Windows.Thickness(0, 0, 0, 0);
                return new System.Windows.Thickness(1, 1, 0, 0);
            }
        }

        public TimeRuler TimeRulerLocal { get; private set; }

        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (SetProperty(ref _Title, value))
                    _Title = value;
            }
        }

        //public Dictionary<DateTime, ScalaNumerica> ScalaNumericaFeriale { get; set; }
        //public Dictionary<DateTime, ScalaNumerica> ScalaNumericaAnonima2024 { get; set; }

        public int RulerType;

        private bool _ShowWeekends;
        public bool ShowWeekends
        {
            get
            {
                return _ShowWeekends;
            }
            set
            {
                if (SetProperty(ref _ShowWeekends, value))
                    _ShowWeekends = value;
            }
        }

        private DateTime _StartTimeGantt;
        public DateTime StartTimeGantt
        {
            get
            {
                return _StartTimeGantt;
            }
            set
            {
                if (SetProperty(ref _StartTimeGantt, value))
                    _StartTimeGantt = value;
            }
        }

        private DateTime _EndTimeGantt;
        public DateTime EndTimeGantt
        {
            get
            {
                return _EndTimeGantt;
            }
            set
            {
                if (SetProperty(ref _EndTimeGantt, value))
                    _EndTimeGantt = value;
            }
        }

        private ObservableCollection<StileConProprieta> _ListStiliConPropieta;
        public ObservableCollection<StileConProprieta> ListStiliConPropieta
        {
            get
            {
                return _ListStiliConPropieta;
            }
            set
            {
                if (SetProperty(ref _ListStiliConPropieta, value))
                {
                    _ListStiliConPropieta = value;
                }
            }
        }

        private StileConProprieta _StileConPropieta;
        public StileConProprieta StileConPropieta
        {
            get
            {
                return _StileConPropieta;
            }
            set
            {
                if (SetProperty(ref _StileConPropieta, value))
                {
                    _StileConPropieta = value;
                }
            }
        }

        private System.Windows.Media.SolidColorBrush _NonWorkingHoursBackground;
        public System.Windows.Media.SolidColorBrush NonWorkingHoursBackground
        {
            get
            {
                return _NonWorkingHoursBackground;
            }
            set
            {
                if (SetProperty(ref _NonWorkingHoursBackground, value))
                    _NonWorkingHoursBackground = value;
            }
        }

        public ScalaCronologicaView(IDataService dataservice)
        {
            TimescaleRulerCount = 0;
            Tasks = new ObservableCollection<Processo>();
            WorkdayRulesSource = new ObservableCollection<WorkdayRule>();
            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Saturday } });
            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Sunday } });
            DataService = dataservice;
            FirstInitialization = true;
            SetDefault();
            //CustomSchedule = GenerateSchedule();
            FirstInitialization = false;
            Title = LocalizationProvider.GetString("Scala cronologica") + " " + LocalizationProvider.GetString("calendario");
        }

        private void SetDefault()
        {
            TabItemViews = new List<TabItemView>();
            for (int i = 0; i < 3; i++)
            {
                TabItemView tabItemView = new TabItemView();
                TabItemViews.Add(tabItemView);
            }
            TabItemViewCorrente = new TabItemView();
            LayoutLivelli = new Dictionary<int, string>();
            LayoutLivelli.Add(0, GanttKeys.LocalizeMenuUnLivello);
            LayoutLivelli.Add(1, GanttKeys.LocalizeMenuDueLivelli);
            LayoutLivelli.Add(2, GanttKeys.LocalizeMenuTreLivelli);
            Allineamento = new Dictionary<int, string>();
            Allineamento.Add(0, GanttKeys.LocalizeSinistra);
            Allineamento.Add(1, GanttKeys.LocalizeCentro);
            Allineamento.Add(2, GanttKeys.LocalizeDestra);
            ZoomFactor = 100;

            ListStiliConPropieta = new ObservableCollection<StileConProprieta>();

            ObservableCollection<ColorInfo> Colors = new ObservableCollection<ColorInfo>();

            var coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);
            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    Colors.Add(colInfo);
                }
            }

            List<Guid> entitiesFound = null;
            List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(MasterDetailModel.BuiltInCodes.EntityType.Stili, new MasterDetailModel.FilterData(), null, null, out entitiesFound);
            List<Entity> Entities = DataService.GetEntitiesById(MasterDetailModel.BuiltInCodes.EntityType.Stili, entitiesFound);
            Model.EntitiesHelper entsHelper = new Model.EntitiesHelper(DataService);

            var converter = new System.Windows.Media.BrushConverter();
            string Hexadecimal = null;

            foreach (var Ent in Entities)
            {
                StileConProprieta carattere = new StileConProprieta();
                MasterDetailModel.Valore val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Nome, true, true);
                carattere.Nome = val.PlainText;
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Codice, true, true);
                carattere.Codice = val.PlainText;
                carattere.NomeECodice = carattere.Nome + " (" + carattere.Codice + ")";
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Carattere, true, true);
                carattere.FontFamily = val.PlainText;
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.DimensioneCarattere, true, true);
                carattere.Size = double.Parse(val.PlainText);
                Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreCarattere].Valore).Hexadecimal;
                if (Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault() != null)
                {
                    carattere.ColorCharacther = Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault().SampleBrush;
                }
                Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreSfondo].Valore).Hexadecimal;
                if (Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault() != null)
                {
                    carattere.ColorBackground = Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault().SampleBrush;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Grassetto].Valore).V.Value)
                {
                    carattere.Grassetto = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Grassetto].Valore).V.Value;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Italic].Valore).V.Value)
                {
                    carattere.Corsivo = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Italic].Valore).V.Value;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Sottolineato].Valore).V.Value)
                {
                    carattere.Sottolineato = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Sottolineato].Valore).V.Value;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Barrato].Valore).V.Value)
                {
                    carattere.Barrato = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Barrato].Valore).V.Value;
                }
                ListStiliConPropieta.Add(carattere);
            }
            StileConPropieta = ListStiliConPropieta.FirstOrDefault();
        }

        public void Init(TimeRuler timeRuler, int rulerType = 0)
        {
            if (timeRuler != null)
            {
                if (timeRuler.CodiceStileScala != null)
                    StileConPropieta = ListStiliConPropieta.Where(c => c.NomeECodice == timeRuler.CodiceStileScala).FirstOrDefault();

                int Contatore = 0;
                foreach (TabItemView TabItemView in TabItemViews)
                {
                    if (Contatore == 0)
                    {
                        TabItemView.SelectedUnita = TabItemView.UnitaTempo.Where(r => r.Key == timeRuler.TimeRulersDetail.ElementAt(0).UnitTime).FirstOrDefault();
                        TabItemView.Formato = timeRuler.TimeRulersDetail.ElementAt(0).Format;
                        if (rulerType == 0)
                            TabItemView.Formato = SetFormatoSeVuoto(TabItemView.SelectedUnita.Key, TabItemView.Formato);
                    }

                    if (Contatore == 1)
                    {
                        TabItemView.SelectedUnita = TabItemView.UnitaTempo.Where(r => r.Key == timeRuler.TimeRulersDetail.ElementAt(1).UnitTime).FirstOrDefault();
                        TabItemView.Formato = timeRuler.TimeRulersDetail.ElementAt(1).Format;
                        if (rulerType == 0)
                            TabItemView.Formato = SetFormatoSeVuoto(TabItemView.SelectedUnita.Key, TabItemView.Formato);
                    }

                    if (Contatore == 2)
                    {
                        TabItemView.SelectedUnita = TabItemView.UnitaTempo.Where(r => r.Key == timeRuler.TimeRulersDetail.ElementAt(2).UnitTime).FirstOrDefault();
                        TabItemView.Formato = timeRuler.TimeRulersDetail.ElementAt(2).Format;
                        if (rulerType == 0)
                            TabItemView.Formato = SetFormatoSeVuoto(TabItemView.SelectedUnita.Key, TabItemView.Formato);
                    }
                    Contatore++;
                }
                LayoutLivello = LayoutLivelli.Where(r => r.Key == timeRuler.LayoutLevel).FirstOrDefault();
                if (LayoutLivello.Key == 0)
                {
                    TabSelectedIndex = 0;
                    TabItemViewCorrente.SelectedUnita = TabItemViewCorrente.UnitaTempo.Where(r => r.Key == timeRuler.TimeRulersDetail.ElementAt(0).UnitTime).FirstOrDefault();
                    TabItemViewCorrente.Formato = timeRuler.TimeRulersDetail.ElementAt(0).Format;
                    if (rulerType == 0)
                        TabItemViewCorrente.Formato = SetFormatoSeVuoto(TabItemViewCorrente.SelectedUnita.Key, TabItemViewCorrente.Formato);
                }
                if (LayoutLivello.Key == 1)
                {
                    TabSelectedIndex = 1;
                    TabItemViewCorrente.SelectedUnita = TabItemViewCorrente.UnitaTempo.Where(r => r.Key == timeRuler.TimeRulersDetail.ElementAt(1).UnitTime).FirstOrDefault();
                    TabItemViewCorrente.Formato = timeRuler.TimeRulersDetail.ElementAt(1).Format;
                    if (rulerType == 0)
                        TabItemViewCorrente.Formato = SetFormatoSeVuoto(TabItemViewCorrente.SelectedUnita.Key, TabItemViewCorrente.Formato);
                }
                if (LayoutLivello.Key == 2)
                {
                    TabSelectedIndex = 2;
                    TabItemViewCorrente.SelectedUnita = TabItemViewCorrente.UnitaTempo.Where(r => r.Key == timeRuler.TimeRulersDetail.ElementAt(2).UnitTime).FirstOrDefault();
                    TabItemViewCorrente.Formato = timeRuler.TimeRulersDetail.ElementAt(2).Format;
                    if (rulerType == 0)
                        TabItemViewCorrente.Formato = SetFormatoSeVuoto(TabItemViewCorrente.SelectedUnita.Key, TabItemViewCorrente.Formato);
                }

                SelectedAllineamento = Allineamento.Where(r => r.Key == timeRuler.HorizontalAlignement).FirstOrDefault();
                IsSeparatoreHoriz = timeRuler.HorizontalSeparator;
                IsSeparatoreVert = timeRuler.VerticalSeparator;
                EnableTabs();

                if (rulerType == 0)
                    Title = LocalizationProvider.GetString("Scala cronologica") + " " + LocalizationProvider.GetString("calendario");
                if (rulerType == 1)
                    Title = LocalizationProvider.GetString("Scala cronologica") + " " + LocalizationProvider.GetString("progressiva");
                if (rulerType == 2)
                    Title = LocalizationProvider.GetString("Scala cronologica") + " " + LocalizationProvider.GetString("feriale");

                if (rulerType == 2)
                {
                    foreach (var item in TabItemViews)
                    {
                        //RIMUOVO PER SCALA FERIALE SCALA A ORE E MINUTI
                        item.UnitaTempo.Remove(7);
                        item.UnitaTempo.Remove(8);
                    }
                    TabItemViewCorrente.UnitaTempo.Remove(7);
                    TabItemViewCorrente.UnitaTempo.Remove(8);
                }
                else
                {
                    foreach (var item in TabItemViews)
                    {
                        if (!item.UnitaTempo.ContainsKey(7))
                            item.UnitaTempo.Add(7, GanttKeys.LocalizeOre);
                        if (!item.UnitaTempo.ContainsKey(8))
                            item.UnitaTempo.Add(8, GanttKeys.LocalizeMinuti);
                    }
                }

                RulerType = rulerType;

                if (RulerType == 0)
                {
                    ShowWeekends = true;
                }

                if (RulerType == 1)
                {
                    ShowWeekends = true;
                }

                if (RulerType == 2)
                {
                    ShowWeekends = false;
                }

                if (rulerType == 1)
                {
                    StartTimeGantt = new DateTime(2074, 1, 1).AddDays(-14);
                    EndTimeGantt = StartTimeGantt.AddDays(60);
                }
                EndTimeGantt = StartTimeGantt.AddDays(60);
            }
        }

        private string SetFormatoSeVuoto(int IdUnita, string formato)
        {
            if (string.IsNullOrEmpty(formato))
            {
                switch (IdUnita)
                {
                    case 0:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "yyyy";
                        break;
                    case 1:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "yyyy";
                        break;
                    case 2:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "yyyy";
                        break;
                    case 3:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "MMM yyyy";
                        break;
                    case 4:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "dd MM yy";
                        break;
                    case 5:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "dd MM yy";
                        break;
                    case 6:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "ddd d";
                        break;
                    case 7:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "HH";
                        break;
                    case 8:
                        if (RulerType == 1 || RulerType == 2)
                            return "0";
                        else
                            return "mm";
                        break;
                    default:
                        return "";
                        break;
                }
            }
            else
            {
                return formato;
            }
        }

        public void GenerateTimeScale(RequestTimescaleRulersEventArgs e)
        {
            e.TimescaleRulers.Clear();

            if (IsEnabledSup)
            {
                TabItemView TabItemViewSup = TabItemViews.ElementAt(0);
                GenerateGanttTimeScaleRuler(TabItemViewSup, e);
            }
            if (IsEnabledInt)
            {
                TabItemView TabItemViewInt = TabItemViews.ElementAt(1);
                GenerateGanttTimeScaleRuler(TabItemViewInt, e);
            }
            if (IsEnabledInf)
            {
                TabItemView TabItemViewInf = TabItemViews.ElementAt(2);
                GenerateGanttTimeScaleRuler(TabItemViewInf, e);
            }

            if (RulerType == 2)
            {
                e.NonworkingDayVisibility = System.Windows.Visibility.Collapsed;
                e.NonworkingTimeVisibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                e.NonworkingDayVisibility = System.Windows.Visibility.Visible;
                e.NonworkingTimeVisibility = System.Windows.Visibility.Visible;
            }
        }
        private void GenerateGanttTimeScaleRuler(TabItemView TabItemView, RequestTimescaleRulersEventArgs e)
        {
            switch (TabItemView.SelectedUnita.Key)
            {
                case 0:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Year, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 1:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.HalfYear, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 2:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Quarter, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 3:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Month, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 4:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Week, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    //DECADI
                    break;
                case 5:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Week, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 6:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Day, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 7:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Hour, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                case 8:
                    e.TimescaleRulers.Add(new DevExpress.Xpf.Gantt.TimescaleRuler(DevExpress.Xpf.Gantt.TimescaleUnit.Minute, formatProvider: new CustomProviderTimeScale(TabItemView.Formato, TabItemView.SelectedUnita.Key, RulerType)));
                    break;
                default:
                    break;
            }
        }

        public TimeSpan GetZoom()
        {
            switch (TabItemViews.LastOrDefault().SelectedUnita.Key)
            {
                case 0:
                    return new TimeSpan(288, 1, 0);
                    break;
                case 1:
                    return new TimeSpan(0, 1, 0);
                    break;
                case 2:
                    return new TimeSpan(0, 1, 0);
                    break;
                case 3:
                    return new TimeSpan(26, 0, 0);
                    break;
                case 4:
                    return new TimeSpan(0, 1, 0);
                    //DECADI
                    break;
                case 5:
                    return new TimeSpan(3, 0, 0);
                    break;
                case 6:
                    return new TimeSpan(0, 35, 0);
                    break;
                case 7:
                    return new TimeSpan(0, 2, 0);
                    break;
                case 8:
                    break;
                default:
                    return new TimeSpan(0,1,0);
                    break;
            }
            return new TimeSpan(0, 1, 0);
        }

        //public void ScheduleCellCreated(ScheduleCellCreatedEventArgs args)
        //{
        //    if (RulerType == 0)
        //    {
        //        if (CustomSchedule.Where(f => f.TimeUnit == args.CurrentCell.CellTimeUnit).FirstOrDefault() != null)
        //        {
        //            string Format = CustomSchedule.Where(f => f.TimeUnit == args.CurrentCell.CellTimeUnit).FirstOrDefault().CellTextFormat;
        //            if (!string.IsNullOrEmpty(Format))
        //            {
        //                if (args.CurrentCell.CellTimeUnit == TimeUnit.Weeks)
        //                {
        //                    args.CurrentCell.CellToolTip = args.CurrentCell.CellDate.ToString(Format) + "-" + args.CurrentCell.CellDate.AddDays(7).ToString(Format);
        //                }
        //                else
        //                {
        //                    args.CurrentCell.CellToolTip = args.CurrentCell.CellDate.ToString(Format);
        //                }
        //            }
        //        }
        //    }

        //    if (RulerType == 1)
        //    {
        //        int IdUnit = (int)args.CurrentCell.CellTimeUnit;
        //        if (ScalaNumericaAnonima2024.ContainsKey(args.CurrentCell.CellDate))
        //        {
        //            string Format = CustomSchedule.Where(f => f.TimeUnit == args.CurrentCell.CellTimeUnit).FirstOrDefault().CellTextFormat;
        //            if (string.IsNullOrEmpty(Format))
        //                Format = "N0";
        //            switch (IdUnit)
        //            {
        //                case 1:
        //                    args.CurrentCell.Content = ScalaNumericaAnonima2024[args.CurrentCell.CellDate].ProgressivoNumericoMinutoAnonima.ToString(Format);
        //                    break;
        //                case 2:
        //                    args.CurrentCell.Content = ScalaNumericaAnonima2024[args.CurrentCell.CellDate].ProgressivoNumericoOraAnonima.ToString(Format);
        //                    break;
        //                case 3:
        //                    args.CurrentCell.Content = ScalaNumericaAnonima2024[args.CurrentCell.CellDate].ProgressivoNumericoGiornoAnonima.ToString(Format);
        //                    break;
        //                case 4:
        //                    args.CurrentCell.Content = ScalaNumericaAnonima2024[args.CurrentCell.CellDate].ProgressivoNumericoSettimanaAnonima.ToString(Format);
        //                    break;
        //                case 5:
        //                    args.CurrentCell.Content = ScalaNumericaAnonima2024[args.CurrentCell.CellDate].ProgressivoNumericoMeseAnonima.ToString(Format);
        //                    break;
        //                case 6:
        //                    args.CurrentCell.Content = ScalaNumericaAnonima2024[args.CurrentCell.CellDate].ProgressivoNumericoAnnoAnonima.ToString(Format);
        //                    break;
        //                default:

        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            args.CurrentCell.Content = "";
        //        }
        //    }

        //    if (RulerType == 2)
        //    {
        //        int IdUnit = (int)args.CurrentCell.CellTimeUnit;
        //        if (ScalaNumericaFeriale.ContainsKey(args.CurrentCell.CellDate))
        //        {
        //            string Format = CustomSchedule.Where(f => f.TimeUnit == args.CurrentCell.CellTimeUnit).FirstOrDefault().CellTextFormat;
        //            if (string.IsNullOrEmpty(Format))
        //                Format = "N0";
        //            switch (IdUnit)
        //            {
        //                case 1:
        //                    if (ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoMinuto != -1)
        //                        args.CurrentCell.Content = ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoMinuto.ToString(Format);
        //                    else
        //                        args.CurrentCell.Content = "";
        //                    break;
        //                case 2:
        //                    if (ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoOra != -1)
        //                        args.CurrentCell.Content = ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoOra.ToString(Format);
        //                    else
        //                        args.CurrentCell.Content = "";
        //                    break;
        //                case 3:
        //                    if (ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoGiorno != -1)
        //                        args.CurrentCell.Content = ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoGiorno.ToString(Format);
        //                    else
        //                        args.CurrentCell.Content = "";
        //                    break;
        //                case 4:
        //                    if (ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoSettimana != -1)
        //                        args.CurrentCell.Content = ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoSettimana.ToString(Format);
        //                    else
        //                        args.CurrentCell.Content = "";
        //                    break;
        //                case 5:
        //                    if (ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoMese != -1)
        //                        args.CurrentCell.Content = ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoMese.ToString(Format);
        //                    else
        //                        args.CurrentCell.Content = "";
        //                    break;
        //                case 6:
        //                    if (ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoAnno != -1)
        //                        args.CurrentCell.Content = ScalaNumericaFeriale[args.CurrentCell.CellDate].ProgressivoNumericoAnno.ToString(Format);
        //                    else
        //                        args.CurrentCell.Content = "";
        //                    break;
        //                default:

        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            args.CurrentCell.Content = "";
        //        }
        //    }
        //}
        public void RigenerateCustomSchedule()
        {
            if (PreviousIndex != -1 && PreviousIndex == TabSelectedIndex)
            {
                if (LayoutLivello.Key == 2)
                {
                    TimescaleRulerCount = 0;
                    TimescaleRulerCount = 3;
                }

                if (LayoutLivello.Key == 1)
                {
                    TimescaleRulerCount = 0;
                    TimescaleRulerCount = 2;
                }

                if (LayoutLivello.Key == 0)
                {
                    TimescaleRulerCount = 0;
                    TimescaleRulerCount = 1;

                }

                TabItemViews.ElementAt(PreviousIndex).SelectedUnita = TabItemViews.ElementAt(PreviousIndex).UnitaTempo.Where(f => f.Key == TabItemViewCorrente.SelectedUnita.Key).FirstOrDefault();
                TabItemViews.ElementAt(PreviousIndex).Formato = TabItemViewCorrente.Formato;
                //CustomSchedule = GenerateSchedule();
            }
        }
        public void SelectionChanged()
        {
            EnableTabs();

            if (LayoutLivello.Key == 2)
                TabSelectedIndex = 0;
            if (LayoutLivello.Key == 1)
                TabSelectedIndex = 1;
            if (LayoutLivello.Key == 0)
                TabSelectedIndex = 2;

            //CustomSchedule = GenerateSchedule();

        }
        private void EnableTabs()
        {
            if (LayoutLivello.Key == 2)
            {
                TimescaleRulerCount = 0;
                IsEnabledSup = IsEnabledInt = IsEnabledInf = true;
                TimescaleRulerCount = 3;
            }

            if (LayoutLivello.Key == 1)
            {
                TimescaleRulerCount = 0;
                IsEnabledInt = IsEnabledInf = true;
                IsEnabledSup = false;
                TimescaleRulerCount = 2;
            }

            if (LayoutLivello.Key == 0)
            {
                TimescaleRulerCount = 0;
                IsEnabledSup = IsEnabledInt = false;
                IsEnabledInf = true;
                TimescaleRulerCount = 1;
            }
        }

        private int PreviousIndex = -1;
        public void TabSelectionChanged()
        {
            if (PreviousIndex != -1)
            {
                TabItemViews.ElementAt(PreviousIndex).SelectedUnita = TabItemViews.ElementAt(PreviousIndex).UnitaTempo.Where(f => f.Key == TabItemViewCorrente.SelectedUnita.Key).FirstOrDefault();
                TabItemViews.ElementAt(PreviousIndex).Formato = TabItemViews.ElementAt(PreviousIndex).Formato;
            }

            TabItemView tabItemView = TabItemViews.ElementAt(TabSelectedIndex);
            TabItemViewCorrente = new TabItemView();
            if (RulerType == 2)
            {
                TabItemViewCorrente.UnitaTempo.Remove(7);
                TabItemViewCorrente.UnitaTempo.Remove(8);
            }
            TabItemViewCorrente.SelectedUnita = TabItemViewCorrente.UnitaTempo.Where(f => f.Key == tabItemView.SelectedUnita.Key).FirstOrDefault();
            TabItemViewCorrente.Formato = tabItemView.Formato;

            PreviousIndex = TabSelectedIndex;
        }
        public bool Accept()
        {
            TimeRulerLocal = new TimeRuler();
            TimeRulerLocal.LayoutLevel = LayoutLivello.Key;
            TimeRulerLocal.HorizontalAlignement = SelectedAllineamento.Key;
            TimeRulerLocal.HorizontalSeparator = IsSeparatoreHoriz;
            TimeRulerLocal.VerticalSeparator = IsSeparatoreVert;
            TimeRulerLocal.CodiceStileScala = StileConPropieta.NomeECodice;
            TimeRulerLocal.TimeRulersDetail = new List<TimeRulerDetail>();
            foreach (TabItemView TabItemView in TabItemViews)
            {
                TimeRulerDetail TimeRulerDetail = new TimeRulerDetail();
                TimeRulerDetail.UnitTime = TabItemView.SelectedUnita.Key;
                TimeRulerDetail.Format = TabItemView.Formato;
                TimeRulerLocal.TimeRulersDetail.Add(TimeRulerDetail);
            }
            return true;
        }
        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ColorBackground));
            RaisePropertyChanged(GetPropertyName(() => ColorCharacther));
            RaisePropertyChanged(GetPropertyName(() => FontFamily));
            RaisePropertyChanged(GetPropertyName(() => FontSize));
            RaisePropertyChanged(GetPropertyName(() => FontWeight));
            RaisePropertyChanged(GetPropertyName(() => FontStyle));
            RaisePropertyChanged(GetPropertyName(() => TextDecorations));
            RaisePropertyChanged(GetPropertyName(() => NonWorkingHoursBackground));
        }

        public void UpdateUITimescaleRulerCount()
        {
            RaisePropertyChanged(GetPropertyName(() => TimescaleRulerCount));
        }

        public int GetNumeroLivelli()
        {
            if (LayoutLivello.Key == 2)
                return 3;

            if (LayoutLivello.Key == 1)
                return 2;

            if (LayoutLivello.Key == 0)
                return 1;

            return 0;
        }

        public void SetFormatoDefault()
        {
            TabItemViewCorrente.Formato = SetFormatoSeVuoto(TabItemViewCorrente.SelectedUnita.Key,"");
        }
    }
    public class TabItemView : NotificationBase
    {
        private Dictionary<int, string> _UnitaTempo;
        public Dictionary<int, string> UnitaTempo
        {
            get
            {
                return _UnitaTempo;
            }
            set
            {
                if (SetProperty(ref _UnitaTempo, value))
                    _UnitaTempo = value;
            }
        }

        private KeyValuePair<int, string> _SelectedUnita;
        public KeyValuePair<int, string> SelectedUnita
        {
            get
            {
                return _SelectedUnita;
            }
            set
            {
                if (SetProperty(ref _SelectedUnita, value))
                    _SelectedUnita = value;
            }
        }

        private string _Formato;
        public string Formato
        {
            get
            {
                return _Formato;
            }
            set
            {
                if (SetProperty(ref _Formato, value))
                    _Formato = value;
            }
        }
        public TabItemView()
        {
            UnitaTempo = new Dictionary<int, string>();
            UnitaTempo.Add(0, GanttKeys.LocalizeAnni);
            //UnitaTempo.Add(1, GanttKeys.LocalizeSemestri);
            //UnitaTempo.Add(2, GanttKeys.LocalizeTrimestri);
            UnitaTempo.Add(3, GanttKeys.LocalizeMesi);
            //UnitaTempo.Add(4, GanttKeys.LocalizeDecadi);
            UnitaTempo.Add(5, GanttKeys.LocalizeSettimane);
            UnitaTempo.Add(6, GanttKeys.LocalizeGiorni);
            UnitaTempo.Add(7, GanttKeys.LocalizeOre);
            //UnitaTempo.Add(8, GanttKeys.LocalizeMinuti);
        }
    }

}
