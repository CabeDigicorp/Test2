using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Charts.Native;
using DevExpress.Mvvm.Gantt;
using DevExpress.Xpf.Gantt;
using MasterDetailModel;
using MasterDetailView;
using Model;
using Syncfusion.Windows.Controls.Gantt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AttivitaWpf.View
{
    public interface GanttViewSync
    {
        //On
        void OnGestioneDateWndOk(DateTime oldDataInizio, DateTime newDataInizio, bool dayOffset);
        HashSet<Guid> OnTasksOffset(List<Guid> WBSItemsId, double workingDayOffset);
        HashSet<Guid> OnTaskAction(ModelAction action);
        void OnTasksSelected(List<Guid> WBSItemsId);
        void OnCurrentTaskChanged(Guid WBSItemId);
        HashSet<Guid> OnTaskPredecessorAdd(Guid WBSItemId, WBSPredecessor predecessor);
        void OnTasksPredecessorDisconnect(List<Guid> WBSItemsId);
        void OnIsActiveCriticalPathChange(bool IsActive);

        //Get
        Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> GetFilteredEntitiesViewInfo();
        CalendariItem GetCalendarioDefault();
        DateTime? GetDataInizioLavori();
        DateTime? GetDataFineLavori();
        DateTime GetMaxDataFineWBSItems(bool byCurrentFilter = false);
        DateTime GetMinDataInizioWBSItems(bool byCurrentFilter = false);
        Guid GetSelectedItem();//Fuoco
        HashSet<Guid> GetCheckedItems();//Selezionate (grigette)
        HashSet<Guid> GetCriticalPath();
        List<Guid> GetFilteredDescendantsOf(Guid parentId);
        bool IsMultipleModify();
        List<Model3dObjectKey> GetModel3dObjectsKeyByWBSItemsId(HashSet<Guid> WBSItemsId);
        I3DModelService GetModel3dService();
    }

    public class GanttView : GanttSource, WBSViewSync, IProcesso
    {
        public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public GanttViewSync WBSView { get; set; }
        public IEntityWindowService WindowService { get; set; }
        public GanttData GanttData { get; set; }

        private HashSet<Guid> AttivitaPercorsoCritico;

        public event EventHandler<EventArgs> ResetVaraibles;
        public event EventHandler<DateEventArgs> ScroolToDataInput;
        public event EventHandler<WBSVicibleEventArgs> AperturaChusuraRami;
        public event EventHandler<EventArgs> GenerateverticalRow;

        private bool _IsActiveCriticalPath;
        public bool IsActiveCriticalPath
        {
            get
            {
                return _IsActiveCriticalPath;
            }
            set
            {
                if (SetProperty(ref _IsActiveCriticalPath, value))
                    _IsActiveCriticalPath = value;
                WBSView.OnIsActiveCriticalPathChange(value);
                UpdateUiElementiListaAttivita();
            }
        }

        private bool _IsActiveCalendario;
        public bool IsActiveCalendario
        {
            get
            {
                return _IsActiveCalendario;
            }
            set
            {
                if (!value && !IsActiveNascondiDate && !IsActiveProgressiva)
                    return;

                SetProperty(ref _IsActiveCalendario, value);

                if (value)
                {
                    IsActiveNascondiDate = false;
                    IsActiveProgressiva = false;
                    DateIcon = "\ue0f6";
                    SetScalaCronologica();
                }
            }
        }

        private bool _IsActiveProgressiva;
        public bool IsActiveProgressiva
        {
            get
            {
                return _IsActiveProgressiva;
            }
            set
            {
                if (!value && !IsActiveNascondiDate && !IsActiveCalendario)
                    return;

                SetProperty(ref _IsActiveProgressiva, value);

                if (value)
                {
                    IsActiveNascondiDate = false;
                    IsActiveCalendario = false;
                    DateIcon = "\ue0f8";
                    SetScalaCronologica();
                }
            }
        }

        private bool _IsActiveNascondiDate;
        public bool IsActiveNascondiDate
        {
            get
            {
                return _IsActiveNascondiDate;
            }
            set
            {
                if (!value && !IsActiveCalendario && !IsActiveProgressiva)
                    return;

                SetProperty(ref _IsActiveNascondiDate, value);

                if (value)
                {
                    IsActiveCalendario = false;
                    IsActiveProgressiva = false;
                    DateIcon = "\ue108";
                    SetScalaCronologica();
                }
            }
        }

        private bool _ShowSALTglBtn_Checked;
        public bool ShowSALTglBtn_Checked
        {
            get
            {
                return _ShowSALTglBtn_Checked;
            }
            set
            {
                SetProperty(ref _ShowSALTglBtn_Checked, value);

                if (value)
                    AddSALStripLine();
                else
                    GeneraStripLines(new Dictionary<DateTime, System.Windows.Media.SolidColorBrush>());
            }
        }

        //SCALA ANONIMA SETTATA PER L'ANNO 2074
        public void Init(bool UpdateWbsButton = false)
        {
            StripLines = new ObservableCollection<StripLineDataItem>();
            WorkdayRulesSource = new ObservableCollection<WorkdayRule>();
            SyncHolidays = new GanttHolidayCollection();
            WorkingTimeRulesSource = new ObservableCollection<WorkingTimeRule>();
            AttivitaPercorsoCritico = new HashSet<Guid>();
            ScalaCronologicaView = new ScalaCronologicaView(DataService);
            ScalaCronologicaViewLocal = new ScalaCronologicaView(DataService);
            GanttChartStyleSettingView = new GanttChartStyleSettingView(DataService);
            ProgrammazioneSALView = new ProgrammazioneSALView(DataService,WindowService,MainOperation);
            DateProgettoView = new DateProgettoView();
            PreviousSelected = new HashSet<Guid>();
            PlayPauseButtonFont = "\ue082";
            if (UpdateWbsButton)
            {
                foreach (var item in SelectedItems)
                    PreviousSelected.Add(item.Id);
            }
            Tasks = new ObservableCollection<Processo>();
            SelectedItems = new ObservableCollection<Processo>();
            GetAndSetAllData(UpdateWbsButton);
        }

        private void GetAndSetAllData(bool UpdateWbsButton)
        {
            IsActiveCalendario = true;
            IsActiveNascondiDate = false;
            IsActiveProgressiva = false;
            IsBarreDiRiepilogoChecked = true;
            ShowSALTglBtn_Checked = false;
            AttivitaPercorsoCritico = WBSView.GetCriticalPath();
            GanttData = DataService.GetGanttData();
            SetGanttStyles(true);
            SetDateProgettoView();
            GeneraScalaNumerica(UpdateWbsButton);
            SetScalaCronologica();
            GetNoWotkingdays();
            GeneraStripLines(new Dictionary<DateTime, System.Windows.Media.SolidColorBrush>());
            GeneraGantt();
        }

        public void SetGanttStyles(bool FirstExecution = false)
        {
            if (GanttData.SettingChart != null)
            {
                GanttChartStyleSettingView.ColorTaskNode = GanttChartStyleSettingView.ColorsTaskNode.Where(c => c.HexValue == GanttData.SettingChart.HexadecimalTaskNode).FirstOrDefault();
                GanttChartStyleSettingView.ColorHeaderTaskNode = GanttChartStyleSettingView.ColorsHeaderTaskNode.Where(c => c.HexValue == GanttData.SettingChart.HexadecimalHeaderTaskNode).FirstOrDefault();
                GanttChartStyleSettingView.ColorConnectorStroke = GanttChartStyleSettingView.ColorsConnectorStroke.Where(c => c.HexValue == GanttData.SettingChart.HexadecimalConnectorStroke).FirstOrDefault();
                GanttChartStyleSettingView.ColorNonWorkingHours = GanttChartStyleSettingView.ColorsNonWorkingHours.Where(c => c.HexValue == GanttData.SettingChart.HexadecimalNonWorkingHours).FirstOrDefault();
                if (GanttData.SettingChart.HexadecimalCriticalPath != null)
                    GanttChartStyleSettingView.ColorCriticalPath = GanttChartStyleSettingView.ColorsCriticalPath.Where(c => c.HexValue == GanttData.SettingChart.HexadecimalCriticalPath).FirstOrDefault();

                if (GanttData.SettingChart != null)
                    GanttChartStyleSettingView.StileConPropieta = GanttChartStyleSettingView.ListStiliConPropieta.Where(c => c.NomeECodice == GanttData.SettingChart.CodiceStileNote).FirstOrDefault();

                HeaderTaskNodeBackground = GanttChartStyleSettingView.ColorHeaderTaskNode.SampleBrush;
                ConnectorStrokeBackground = GanttChartStyleSettingView.ColorConnectorStroke.SampleBrush;
                NonWorkingHoursBackground = GanttChartStyleSettingView.ColorNonWorkingHours.SampleBrush;
            }
            else
            {
                if (FirstExecution)
                {
                    HeaderTaskNodeBackground = GanttChartStyleSettingView.ColorHeaderTaskNode.SampleBrush;
                    ConnectorStrokeBackground = GanttChartStyleSettingView.ColorConnectorStroke.SampleBrush;
                    NonWorkingHoursBackground = GanttChartStyleSettingView.ColorNonWorkingHours.SampleBrush;
                    RaisePropertyChanged(GetPropertyName(() => TaskNodeBackground));
                    RaisePropertyChanged(GetPropertyName(() => HeaderTaskNodeBackground));
                    RaisePropertyChanged(GetPropertyName(() => ConnectorStrokeBackground));
                    RaisePropertyChanged(GetPropertyName(() => NonWorkingHoursBackground));
                }
            }
            GanttChartStyleSettingView.UpdateUI();
            ScalaCronologicaView.NonWorkingHoursBackground = NonWorkingHoursBackground;
        }

        public void Clear()
        {
            
        }

        public void SetDateProgettoView()
        {
            if (RetrieveWBSData().Count() == 0)
                GanttData.DataInizio = DateTime.Today;

            DateTime DataUltimaAttivita = WBSView.GetMaxDataFineWBSItems();
            DateProgettoView.DataInizioGantt = GanttData.DataInizio;
            DateProgettoView.DataFineGantt = GanttData.DataInizio;
            DateProgettoView.Offset = GanttData.Offset;
            Data = GanttData.DataInizio;

            if (!DateProgettoView.UseDefaultCalendar)
                IsNascondiDateVisible = System.Windows.Visibility.Collapsed;
            else
                IsNascondiDateVisible = System.Windows.Visibility.Visible;

            if (WBSView.GetDataInizioLavori() == null)
                DateProgettoView.DataInizioLavori = GanttData.DataInizio.ToString(DateProgettoView.Format);
            else
                DateProgettoView.DataInizioLavori = WBSView.GetDataInizioLavori()?.ToString(DateProgettoView.Format);

            if (WBSView.GetDataFineLavori() == null)
                DateProgettoView.DataFineLavori = DataUltimaAttivita.ToString(DateProgettoView.Format);
            else
                DateProgettoView.DataFineLavori = WBSView.GetDataFineLavori()?.ToString(DateProgettoView.Format);

            DateProgettoView.DataFineGantt = DataUltimaAttivita;
            DateProgettoView.DataInizioPrecedente = GanttData.DataInizio;
            DateProgettoView.DurataCalendario = (DateProgettoView.DataFineGantt - DateProgettoView.DataInizioGantt).TotalDays;

            CalendariItem calendarioDefault = WBSView.GetCalendarioDefault();
            DateTimeCalculator timeCalc = null;
            if (calendarioDefault != null)
            {
                timeCalc = new DateTimeCalculator(calendarioDefault.GetWeekHours(), calendarioDefault.GetCustomDays());
                DateProgettoView.DurataGiorniLavorativi = timeCalc.GetWorkingDaysBetween(DateProgettoView.DataInizioGantt, DateProgettoView.DataFineGantt);
            }
        }

        int PreviousRulerType;
        public void SetScalaCronologica()
        {
            if (GanttData == null)
                return;

            PreviousRulerType = ScalaCronologicaView.RulerType;

            if (IsActiveCalendario)
            {
                if (GanttData.TimeRulerCalendario == null)
                {
                    GanttData.TimeRulerCalendario = new TimeRuler();
                    GanttData.TimeRulerCalendario.SetDefault();
                }

                ScalaCronologicaView.Init(GanttData.TimeRulerCalendario, 0);
            }

            if (IsActiveProgressiva)
            {
                if (GanttData.TimeRulerAnonimo == null)
                {
                    GanttData.TimeRulerAnonimo = new TimeRuler();
                    GanttData.TimeRulerAnonimo.SetDefault();
                }
                ScalaCronologicaView.Init(GanttData.TimeRulerAnonimo, 1);
            }

            if (IsActiveNascondiDate)
            {
                if (GanttData.TimeRulerFeriale == null)
                {
                    GanttData.TimeRulerFeriale = new TimeRuler();
                    GanttData.TimeRulerFeriale.SetDefault();
                }
                ScalaCronologicaView.Init(GanttData.TimeRulerFeriale, 2);
            }

            TimescaleRulerCount = 0;

            TimescaleRulerCount = ScalaCronologicaView.GetNumeroLivelli();

            ScalaCronologicaView.NonWorkingHoursBackground = NonWorkingHoursBackground;

            if (ScalaCronologicaView.RulerType == 1 || PreviousRulerType == 1)
                GeneraGantt();

            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());
        }

        public void SetTimeRulerToGanttData()
        {
            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupBegin(UndoGroupsName.SetTimeRulerToGanttData, BuiltInCodes.EntityType.WBS);

            if (IsActiveCalendario)
                GanttData.TimeRulerCalendario = ScalaCronologicaView.TimeRulerLocal;
            if (IsActiveNascondiDate)
                GanttData.TimeRulerFeriale = ScalaCronologicaView.TimeRulerLocal;
            if (IsActiveProgressiva)
                GanttData.TimeRulerAnonimo = ScalaCronologicaView.TimeRulerLocal;
            DataService.SetGanttData(GanttData);
            TimescaleRulerCount = 0;
            TimescaleRulerCount = ScalaCronologicaView.TimescaleRulerCount;
            ScalaCronologicaView.NonWorkingHoursBackground = NonWorkingHoursBackground;
            ScalaCronologicaView.UpdateUI();
            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());

            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupEnd();
        }

        public void GenerateGanttTimeScaleRuler(TabItemView TabItemView, RequestTimescaleRulersEventArgs e)
        {
            int RulerType = 0;
            if (IsActiveCalendario)
                RulerType = 0;
            if (IsActiveProgressiva)
                RulerType = 1;
            if (IsActiveNascondiDate)
                RulerType = 2;


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

        public void RipristinaDataContextFinestraScalaCronologica()
        {
            if (IsActiveCalendario)
            {
                if (GanttData.TimeRulerCalendario == null)
                    GanttData.TimeRulerCalendario = new TimeRuler();
                ScalaCronologicaView.Init(GanttData.TimeRulerCalendario, 0);
            }

            if (IsActiveProgressiva)
            {
                if (GanttData.TimeRulerAnonimo == null)
                    GanttData.TimeRulerAnonimo = new TimeRuler();
                ScalaCronologicaView.Init(GanttData.TimeRulerAnonimo, 1);
            }

            if (IsActiveNascondiDate)
            {
                if (GanttData.TimeRulerFeriale == null)
                    GanttData.TimeRulerFeriale = new TimeRuler();
                ScalaCronologicaView.Init(GanttData.TimeRulerFeriale, 2);
            }

            ScalaCronologicaView.NonWorkingHoursBackground = NonWorkingHoursBackground;
        }

        public void SetChartSetting()
        {
            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupBegin(UndoGroupsName.SetChartSetting, BuiltInCodes.EntityType.WBS);


            GanttData.SettingChart = new SettingChart();
            GanttData.SettingChart.HexadecimalTaskNode = GanttChartStyleSettingView.ColorTaskNode.HexValue;
            GanttData.SettingChart.HexadecimalHeaderTaskNode = GanttChartStyleSettingView.ColorHeaderTaskNode.HexValue;
            GanttData.SettingChart.HexadecimalConnectorStroke = GanttChartStyleSettingView.ColorConnectorStroke.HexValue;
            GanttData.SettingChart.HexadecimalNonWorkingHours = GanttChartStyleSettingView.ColorNonWorkingHours.HexValue;
            GanttData.SettingChart.HexadecimalCriticalPath = GanttChartStyleSettingView.ColorCriticalPath.HexValue;

            GanttData.SettingChart.CodiceStileNote = GanttChartStyleSettingView.StileConPropieta.NomeECodice;

            DataService.SetGanttData(GanttData);

            SetGanttStyles();

            RegenerateResources(Tasks);

            UpdateUiElementiListaAttivita(Tasks);

            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupEnd();


        }

        public void UpdateChartUI()
        {
            SetGanttStyles();

            RegenerateResources(Tasks);

            UpdateUiElementiListaAttivita(Tasks);
        }

        private void RegenerateResources(ObservableCollection<Processo> listaAttivita)
        {
            foreach (Processo Attivita in listaAttivita)
            {
                List<Guid> Ids = new List<Guid>();
                Ids.Add(Attivita.Id);
                WBSItem Entity = (WBSItem)RetrieveWBSData(Ids).FirstOrDefault();
                if (Entity != null)
                {
                    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
                    ValoreTesto Note = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.TaskNote, false, false);
                    if (Note != null)
                        Attivita.Name = Note.PlainText;
                }
                RegenerateResources(Attivita.Children);
            }
        }

        private void GeneraScalaNumerica(bool UpdateWbsButton = false)
        {
            DateTime LastDate = WBSView.GetMaxDataFineWBSItems().AddDays(365);
            if (UpdateWbsButton == false || (UpdateWbsButton == true && ScalaNumericaFeriale.LastOrDefault().Key < LastDate))
            {
                ScalaNumericaFeriale = RiGeneraValoriScalaNumerica();
                GeneraScalaAssociazioneRealeAnonima();
                ScalaNumericaAnonima = RiGeneraValoriScalaNumericaAnonima();
            }
        }

        private Dictionary<DateTime, ScalaNumerica> RiGeneraValoriScalaNumerica()
        {
            CalendariItem calendarioDefault = WBSView.GetCalendarioDefault();
            List<int> days = new List<int>();

            if (calendarioDefault != null)
            {
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Monday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(1);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Tuesday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(2);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Wednesday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(3);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Thursday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(4);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Friday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(5);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Saturday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(6);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Sunday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(0);
            }

            DateTime DataInizio = new DateTime();
            DataInizio = GanttData.DataInizio;
            DateTime DataInizoGantt = new DateTime(DataInizio.Year, DataInizio.Month, DataInizio.Day);
            DateTime DataFineGantt = WBSView.GetMaxDataFineWBSItems().AddDays(365);

            Dictionary<DateTime, ScalaNumerica> DictScalaNumerica = new Dictionary<DateTime, ScalaNumerica>();

            int ContatoreGobale = 0;

            int AnnoPrecedente = 0;
            int ContatoreAnno = 1;
            int ContatoreAnnoSubordinato = 1;

            int MesePrecedente = 0;
            int ContatoreMese = 1;
            int ContatoreMeseSubordinato = 1;

            int SettimanaPrecedente = 0;
            int ContatoreSettimana = 1;
            int ContatoreSettimanaSubordinato = 1;

            int GiornoPrecedente = 0;
            int ContatoreGiorno = 1;
            int ContatoreGiornoSubordinato = 1;

            int OraPrecedente = 0;
            int ContatoreOra = 1;
            int ContatoreOraSubordinato = 1;

            int MinutoPrecedente = 0;
            int ContatoreMinuto = 1;
            int ContatoreMinutoSubordinato = 1;

            while (DataInizoGantt <= DataFineGantt)
            {
                if (ContatoreGobale != 0)
                {
                    if (AnnoPrecedente != DataInizoGantt.Year)
                    {
                        ContatoreMeseSubordinato = 1;
                    }
                    if (MesePrecedente != DataInizoGantt.Month)
                    {
                        ContatoreGiornoSubordinato = 1;
                    }
                    if (GiornoPrecedente != DataInizoGantt.Day)
                    {
                        ContatoreOraSubordinato = 1;
                    }
                    if (OraPrecedente != DataInizoGantt.Hour)
                    {
                        ContatoreMinutoSubordinato = 1;
                    }
                }

                ScalaNumerica ScalaNumerica = new ScalaNumerica();
                ScalaNumerica.ProgressivoNumericoAnno = ContatoreAnno;
                ScalaNumerica.ProgressivoNumericoAnnoSubordinato = ContatoreAnnoSubordinato;
                ScalaNumerica.ProgressivoNumericoMese = ContatoreMese;
                ScalaNumerica.ProgressivoNumericoMeseSubordinato = ContatoreMeseSubordinato;
                ScalaNumerica.ProgressivoNumericoSettimana = ContatoreSettimana;
                ScalaNumerica.ProgressivoNumericoSettimanaSubordinato = ContatoreSettimanaSubordinato;
                ScalaNumerica.ProgressivoNumericoGiorno = ContatoreGiorno;
                ScalaNumerica.ProgressivoNumericoGiornoSubordinato = ContatoreGiornoSubordinato;
                ScalaNumerica.ProgressivoNumericoOra = ContatoreOra;
                ScalaNumerica.ProgressivoNumericoOraSubordinato = ContatoreOraSubordinato;
                ScalaNumerica.ProgressivoNumericoMinuto = ContatoreMinuto;
                ScalaNumerica.ProgressivoNumericoMinutoSubordinato = ContatoreMinutoSubordinato;

                DictScalaNumerica.Add(DataInizoGantt, ScalaNumerica);

                AnnoPrecedente = DataInizoGantt.Year;
                MesePrecedente = DataInizoGantt.Month;
                DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                System.Globalization.Calendar cal = dfi.Calendar;
                SettimanaPrecedente = cal.GetWeekOfYear(DataInizoGantt.Date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                GiornoPrecedente = DataInizoGantt.Day;
                OraPrecedente = DataInizoGantt.Hour;
                MinutoPrecedente = DataInizoGantt.Minute;

                DataInizoGantt = DataInizoGantt.AddMinutes(1);

                //CONTATORI  
                if (AnnoPrecedente != DataInizoGantt.Year)
                {
                    ContatoreAnno++;
                    ContatoreAnnoSubordinato++;
                }

                if (MesePrecedente != DataInizoGantt.Month)
                {
                    ContatoreMese++;
                    ContatoreMeseSubordinato++;
                }

                if (SettimanaPrecedente != cal.GetWeekOfYear(DataInizoGantt.Date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek))
                {
                    //NEL CASO MANCASSE IL CONTATORE DELLA PRIMA SETTIMANA
                    if (ContatoreSettimana == 1 && !DictScalaNumerica.ContainsKey(DataInizoGantt.Date.AddDays(-7)))
                    {
                        ScalaNumerica ScalaNumericaPrimaSett = new ScalaNumerica();
                        ScalaNumericaPrimaSett.ProgressivoNumericoAnno = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoAnnoSubordinato = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoMese = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoMeseSubordinato = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoSettimana = 1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoSettimanaSubordinato = 1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoGiorno = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoGiornoSubordinato = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoOra = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoOraSubordinato = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoMinuto = -1;
                        ScalaNumericaPrimaSett.ProgressivoNumericoMinutoSubordinato = -1;
                        DictScalaNumerica.Add(DataInizoGantt.Date.AddDays(-7), ScalaNumericaPrimaSett);
                    }

                    ContatoreSettimana++;
                    ContatoreSettimanaSubordinato++;
                }

                if (GiornoPrecedente != DataInizoGantt.Day)
                {
                    List<WorkdayRule> WorkdayRules = WorkdayRulesSource.Where(r => r.Recurrence is SpecificDays).ToList();
                    if (!days.Contains((int)DataInizoGantt.DayOfWeek) && WorkdayRules.Where(r => ((SpecificDays)r.Recurrence).Days.Contains(DataInizoGantt)).FirstOrDefault() == null)
                    {
                        ContatoreGiorno++;
                    }
                    if (!days.Contains((int)DataInizoGantt.DayOfWeek) && WorkdayRules.Where(r => ((SpecificDays)r.Recurrence).Days.Contains(DataInizoGantt)) == null)
                    {
                        ContatoreGiornoSubordinato++;
                    }
                }

                if (OraPrecedente != DataInizoGantt.Hour)
                {
                    ContatoreOra++;
                    ContatoreOraSubordinato++;
                }

                if (MinutoPrecedente != DataInizoGantt.Minute)
                {
                    ContatoreMinuto++;
                    ContatoreMinutoSubordinato++;
                }

                ContatoreGobale++;
            }

            return DictScalaNumerica;
        }

        private Dictionary<DateTime, ScalaNumerica> RiGeneraValoriScalaNumericaAnonima()
        {
            int RangeAnni = (WBSView.GetMaxDataFineWBSItems().Year - GanttData.DataInizio.Year) + 1;
            DateTime DataInizio = new DateTime(2074, 1, 1);

            Dictionary<DateTime, ScalaNumerica> DictScalaNumerica = new Dictionary<DateTime, ScalaNumerica>();

            int ContatoreAnno = 1;
            int ContatoreMese = 1;
            int ContatoreSettimana = 1;
            int ContatoreGiorno = 1;
            int ContatoreOra = 1;
            int ContatoreMinuto = 1;
            int ContatoreAnnoAnonimo = 1;
            int ContatoreMeseAnonimo = 1;
            int ContatoreSettimanaAnonimo = 1;
            int ContatoreGiornoAnonimo = 1;
            int ContatoreOraAnonimo = 1;
            int ContatoreMinutoAnonimo = 1;

            for (int a = 1; a <= RangeAnni + 1; a++)
            {
                for (int M = 1; M < 13; M++)
                {
                    for (int g = 1; g < 31; g++)
                    {
                        for (int h = 1; h < 25; h++)
                        {
                            for (int m = 1; m < 61; m++)
                            {
                                ScalaNumerica ScalaNumerica = new ScalaNumerica();
                                ScalaNumerica.ProgressivoNumericoAnno = ContatoreAnno;
                                ScalaNumerica.ProgressivoNumericoAnnoAnonima = ContatoreAnnoAnonimo;
                                ScalaNumerica.ProgressivoNumericoAnnoSubordinato = a;
                                ScalaNumerica.ProgressivoNumericoMese = ContatoreMese;
                                ScalaNumerica.ProgressivoNumericoMeseAnonima = ContatoreMeseAnonimo;
                                ScalaNumerica.ProgressivoNumericoMeseSubordinato = M;
                                ScalaNumerica.ProgressivoNumericoSettimana = ContatoreSettimana;
                                ScalaNumerica.ProgressivoNumericoSettimanaAnonima = ContatoreSettimanaAnonimo;
                                ScalaNumerica.ProgressivoNumericoGiorno = ContatoreGiorno;
                                ScalaNumerica.ProgressivoNumericoGiornoAnonima = ContatoreGiornoAnonimo;
                                ScalaNumerica.ProgressivoNumericoGiornoSubordinato = g;
                                ScalaNumerica.ProgressivoNumericoOra = ContatoreOra;
                                ScalaNumerica.ProgressivoNumericoOraAnonima = ContatoreOraAnonimo;
                                ScalaNumerica.ProgressivoNumericoOraSubordinato = h;
                                ScalaNumerica.ProgressivoNumericoMinuto = ContatoreMinuto;
                                ScalaNumerica.ProgressivoNumericoMinutoAnonima = ContatoreMinutoAnonimo;
                                ScalaNumerica.ProgressivoNumericoMinutoSubordinato = m;
                                DictScalaNumerica.Add(DataInizio, ScalaNumerica);
                                DataInizio = DataInizio.AddMinutes(1);
                                ContatoreMinutoAnonimo++;
                                if (DataInizio.DayOfWeek != DayOfWeek.Saturday && DataInizio.DayOfWeek != DayOfWeek.Sunday)
                                    ContatoreMinuto++;
                            }
                            ContatoreOraAnonimo++;
                            if (DataInizio.DayOfWeek != DayOfWeek.Saturday && DataInizio.DayOfWeek != DayOfWeek.Sunday)
                                ContatoreOra++;
                        }
                        ContatoreGiornoAnonimo++;
                        if (ContatoreOraAnonimo % 7 == 0)
                        {
                            ContatoreSettimanaAnonimo++;
                        }
                        if (DataInizio.DayOfWeek != DayOfWeek.Saturday && DataInizio.DayOfWeek != DayOfWeek.Sunday)
                        {
                            ContatoreGiorno++;
                            if (ContatoreGiorno % 7 == 0)
                            {
                                ContatoreSettimana++;
                            }
                        }
                    }
                    ContatoreMese++;
                    ContatoreMeseAnonimo++;
                }
                ContatoreAnno++;
                ContatoreAnnoAnonimo++;
            }


            return DictScalaNumerica;
        }

        private AssociazioneScale AssociazioneScale;
        private void GeneraScalaAssociazioneRealeAnonima()
        {
            AssociazioneScale = new AssociazioneScale();
            AssociazioneScale.DaRealeAAnonima = new Dictionary<DateTime, DateTime>();
            AssociazioneScale.DaAnonimaAReale = new Dictionary<DateTime, DateTime>();

            CalendariItem calendarioDefault = WBSView.GetCalendarioDefault();
            List<int> days = new List<int>();

            if (calendarioDefault != null)
            {
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Monday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(1);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Tuesday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(2);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Wednesday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(3);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Thursday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(4);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Friday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(5);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Saturday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(6);
                if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Sunday && string.IsNullOrEmpty(g.Hours)).FirstOrDefault() != null)
                    days.Add(0);
            }

            DateTime DataInizoGantt = new DateTime(GanttData.DataInizio.Year, GanttData.DataInizio.Month, GanttData.DataInizio.Day);
            DateTime DataFineGantt = WBSView.GetMaxDataFineWBSItems().AddYears(1);
            int RangeDateInDays = (DataFineGantt - DataInizoGantt).Days;
            DateTime DataInizoGanttAnonima = new DateTime(2074, 1, 1);
            DateTime DataFineGanttAnonima = DataInizoGanttAnonima.AddDays(RangeDateInDays).AddYears(1);

            while (DataInizoGanttAnonima < DataFineGanttAnonima)
            {
                if (DataInizoGanttAnonima.DayOfWeek == DayOfWeek.Sunday || DataInizoGanttAnonima.DayOfWeek == DayOfWeek.Saturday)
                {
                    DataInizoGanttAnonima = DataInizoGanttAnonima.AddDays(1);
                    continue;
                }

                if (!days.Contains((int)DataInizoGantt.DayOfWeek))
                {
                    AssociazioneScale.DaAnonimaAReale.Add(DataInizoGanttAnonima, DataInizoGantt);
                }
                else
                {
                    DataInizoGantt = RicercaPrimGiornoLavorativoUtile(DataInizoGantt, days);
                    AssociazioneScale.DaAnonimaAReale.Add(DataInizoGanttAnonima, DataInizoGantt);
                }

                DataInizoGanttAnonima = DataInizoGanttAnonima.AddDays(1);
                DataInizoGantt = DataInizoGantt.AddDays(1);
            }

        }

        private DateTime RicercaPrimGiornoLavorativoUtile(DateTime dataInizoGantt, List<int> days)
        {
            DateTime Data = dataInizoGantt.AddDays(1);
            List<WorkdayRule> WorkdayRules = WorkdayRulesSource.Where(r => r.Recurrence is SpecificDays).ToList();
            if (!days.Contains((int)Data.DayOfWeek) && WorkdayRules.Where(r => ((SpecificDays)r.Recurrence).Days.Contains(Data)).FirstOrDefault() == null)
                return Data;
            else
                return RicercaPrimGiornoLavorativoUtile(Data, days);
        }

        public void GetNoWotkingdays(bool IsAnonimous = false)
        {
            if (IsAnonimous)
            {
                WorkdayRulesSource = new ObservableCollection<WorkdayRule>();
                WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Saturday } });
                WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Sunday } });
            }
            else
            {
                CalendariItem calendarioDefault = WBSView.GetCalendarioDefault();
                WorkdayRulesSource = new ObservableCollection<WorkdayRule>();
                WorkingTimeRulesSource = new ObservableCollection<WorkingTimeRule>();
                SyncHolidays = new GanttHolidayCollection();
                SyncWeekends = new Days();
                if (calendarioDefault != null)
                {
                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Monday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Monday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Monday } });
                            SyncWeekends |= Days.Monday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Monday);
                        }
                    }

                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Tuesday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Tuesday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Tuesday } });
                            SyncWeekends |= Days.Tuesday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Tuesday);
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = true, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Tuesday } });
                        }
                    }

                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Wednesday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Wednesday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Wednesday } });
                            SyncWeekends |= Days.Wednesday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Wednesday);
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = true, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Wednesday } });
                        }
                    }

                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Thursday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Thursday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Thursday } });
                            SyncWeekends |= Days.Thursday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Thursday);
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = true, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Thursday } });
                        }
                    }

                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Friday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Friday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Friday } });
                            SyncWeekends |= Days.Friday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Friday);
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = true, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Friday } });
                        }
                    }

                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Saturday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Saturday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Saturday } });
                            SyncWeekends = SyncWeekends | Days.Saturday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Saturday);
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = true, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Saturday } });
                        }
                    }

                    if (calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Sunday).FirstOrDefault() != null)
                    {
                        WeekDay weekHours = calendarioDefault.GetWeekHours().Days.Where(g => g.Id == DayOfWeek.Sunday).FirstOrDefault();
                        if (string.IsNullOrEmpty(weekHours.Hours))
                        {
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Sunday } });
                            SyncWeekends = SyncWeekends | Days.Sunday;
                        }
                        else
                        {
                            AddToWorkingTimeRulesSource(weekHours.Hours, DevExpress.Mvvm.DaysOfWeek.Sunday);
                            WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = true, Recurrence = new Weekly() { DayOfWeek = DevExpress.Mvvm.DaysOfWeek.Sunday } });
                        }
                    }

                    List<DateTime> datetimes = new List<DateTime>();
                    foreach (CustomDay customDay in calendarioDefault.GetCustomDays().Days)
                    {
                        if (string.IsNullOrEmpty(customDay.Hours))
                            datetimes.Add(customDay.Day);

                    }
                    if (datetimes.Count() > 0)
                        WorkdayRulesSource.Add(new WorkdayRule() { IsWorkday = false, Recurrence = new SpecificDays() { Days = datetimes } });
                    foreach (DateTime date in datetimes)
                        SyncHolidays.Add(new GanttHoliday() { Day = date });
                }
            }
        }

        private void AddToWorkingTimeRulesSource(string RangeOrarioStringa, DevExpress.Mvvm.DaysOfWeek DayOfWeek)
        {
            List<DevExpress.Mvvm.TimeSpanRange> Range = new List<DevExpress.Mvvm.TimeSpanRange>();
            foreach (PH.WorkingDaysAndTimeUtility.Configuration.WorkTimeSpan item in PH.WorkingDaysAndTimeUtility.Configuration.WorkDaySpan.GetTimeSpans(RangeOrarioStringa))
            {
                DevExpress.Mvvm.TimeSpanRange s = new DevExpress.Mvvm.TimeSpanRange(item.Start, item.End);
                Range.Add(s);
            }
            WorkingTimeRulesSource.Add(new WorkingTimeRule { WorkingTime = Range, Recurrence = new Weekly() { DayOfWeek = DayOfWeek } });
        }

        private void GeneraStripLines(Dictionary<DateTime, System.Windows.Media.SolidColorBrush> Dates)
        {
            StripLines = new ObservableCollection<StripLineDataItem>();
            StripLineDataItem stripline = new StripLineDataItem();

            if (GanttData != null)
            {
                stripline = new StripLineDataItem()
                {
                    StartDateTime = GanttData.DataInizio,

                    //Riabilitata da Ale 20/03/2024
                    //se viene tolta StripLineDuration il progeamma schioppa nella stampa del gantt qualora la data di inizio progetto sia precedente a tutte le barre del gantt
                    StripLineDuration = TimeSpan.Parse("1:0:0"),
                    
                    
                    Background = System.Windows.Media.Brushes.Orange
                };
            }

            StripLines.Add(stripline);

            if (Dates.Count() > 0)
            {
                if (Dates.Count() == 1)
                {
                    if (Dates.FirstOrDefault().Key != GanttData.DataInizio)
                    {
                        GeneraStripLines3DModelScroll(Dates);
                    }
                }
                else
                    GeneraStripLines3DModelScroll(Dates);
            }

        }
        private void GeneraStripLines3DModelScroll(Dictionary<DateTime, System.Windows.Media.SolidColorBrush> Dates)
        {
            foreach (KeyValuePair<DateTime, System.Windows.Media.SolidColorBrush> date in Dates)
            {
                StripLineDataItem stripline = new StripLineDataItem()
                {
                    StartDateTime = date.Key,
                    //StripLineDuration = TimeSpan.Parse("1:0:0"),
                    Background = date.Value
                    //Background = System.Windows.Media.Brushes.SteelBlue
                };

                StripLines.Add(stripline);
            }
        }

        private bool IsGeneraGanttActive = false;
        private void GeneraGantt(IEnumerable<Guid> Guids = null)
        {
            IsGeneraGanttActive = true;
            List<TreeEntity> TreeEntities = null;
            if (Guids == null)
                TreeEntities = RetrieveWBSData();
            else
                TreeEntities = RetrieveWBSData(Guids);

            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            Tasks = new ObservableCollection<Processo>();

            foreach (TreeEntity TreeEntity in TreeEntities.Where(g => g.Depth == 0))
            {
                WBSItem Entity = (WBSItem)TreeEntity;
                ValoreTesto Description = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Nome, false, false);
                ValoreTesto Code = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Codice, false, false);
                DateTime DataInizio = new DateTime();
                DateTime DataFine = new DateTime();
                if (IsActiveProgressiva)
                {
                    if (Entity.GetDataInizio().HasValue)
                        DataInizio = GetDataAnonima((DateTime)Entity.GetDataInizio());
                    if (Entity.GetDataFine().HasValue)
                        DataFine = GetDataAnonima((DateTime)Entity.GetDataFine());
                }
                else
                {
                    if (Entity.GetDataInizio().HasValue)
                        DataInizio = (DateTime)Entity.GetDataInizio();
                    if (Entity.GetDataFine().HasValue)
                        DataFine = (DateTime)Entity.GetDataFine();
                }
                Processo Task = new Processo(this);
                Task.Code = Code.PlainText;
                Task.Description = Description.PlainText;
                Task.StartDate = DataInizio;
                Task.FinishDate = DataFine;
                Task.Progress = (double)Entity.GetTaskProgress();
                Task.Lavoro = Entity.GetOreLavoro().GetValueOrDefault();
                Task.Durata = Entity.GetGiorniDurata().GetValueOrDefault();
                Task.DurataCalendario = Entity.GetGiorniDurataCalendario().GetValueOrDefault();
                Task.Id = Entity.EntityId;

                ValoreTesto Resources = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.TaskNote, false, false);

                if (Resources != null)
                    Task.Name = Resources.PlainText;

                WBSPredecessors WBSPredecessors = Entity.GetPredecessors();
                if (WBSPredecessors != null)
                {
                    if (WBSPredecessors.Items.Count() > 0)
                    {
                        foreach (WBSPredecessor WBSPredecessor in WBSPredecessors.Items)
                        {
                            Predecessore Predecessor = new Predecessore();
                            Predecessor.PredecessorId = WBSPredecessor.WBSItemId;
                            if (WBSPredecessor.Type == WBSPredecessorType.FinishToStart)
                                Predecessor.Type = PredecessorLinkType.FinishToStart;
                            if (WBSPredecessor.Type == WBSPredecessorType.StartToFinish)
                                Predecessor.Type = PredecessorLinkType.StartToFinish;
                            if (WBSPredecessor.Type == WBSPredecessorType.FinishToStart)
                                Predecessor.Type = PredecessorLinkType.FinishToStart;
                            if (WBSPredecessor.Type == WBSPredecessorType.StartToStart)
                                Predecessor.Type = PredecessorLinkType.StartToStart;
                            Task.DependencyLinks.Add(Predecessor);
                        }
                    }
                }

                Tasks.Add(Task);

                if (Entity.Children.Count() > 0)
                    AggiungiTask(entsHelper, TreeEntities, Entity.EntityId, Tasks.Last());

            }
        }

        public void AggiungiTask(EntitiesHelper entsHelper, List<TreeEntity> TreeEntities, Guid EntityId, Processo TaskPadre)
        {
            foreach (TreeEntity TreeEntity in TreeEntities.Where(g => g.Parent?.EntityId == EntityId))
            {
                WBSItem Entity = (WBSItem)TreeEntity;
                ValoreTesto Description = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Nome, false, false);
                ValoreTesto Code = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Codice, false, false);
                DateTime DataInizio = new DateTime();
                DateTime DataFine = new DateTime();
                if (IsActiveProgressiva)
                {
                    if (Entity.GetDataInizio().HasValue)
                        DataInizio = GetDataAnonima((DateTime)Entity.GetDataInizio());
                    if (Entity.GetDataFine().HasValue)
                        DataFine = GetDataAnonima((DateTime)Entity.GetDataFine());
                }
                else
                {
                    if (Entity.GetDataInizio().HasValue)
                        DataInizio = (DateTime)Entity.GetDataInizio();
                    if (Entity.GetDataFine().HasValue)
                        DataFine = (DateTime)Entity.GetDataFine();
                }
                Processo Task = new Processo(this);
                Task.Code = Code.PlainText;
                Task.Description = Description.PlainText;
                Task.StartDate = DataInizio;
                Task.FinishDate = DataFine;
                Task.Progress = (double)Entity.GetTaskProgress();
                Task.Lavoro = Entity.GetOreLavoro().GetValueOrDefault();
                Task.Durata = Entity.GetGiorniDurata().GetValueOrDefault();
                Task.DurataCalendario = Entity.GetGiorniDurataCalendario().GetValueOrDefault();
                Task.Id = Entity.EntityId;

                ValoreTesto Resources = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.TaskNote, false, false);

                if (Resources != null)
                    Task.Name = Resources.PlainText;

                WBSPredecessors WBSPredecessors = Entity.GetPredecessors();
                if (WBSPredecessors != null)
                {
                    if (WBSPredecessors.Items.Count() > 0)
                    {
                        foreach (WBSPredecessor WBSPredecessor in WBSPredecessors.Items)
                        {
                            Predecessore Predecessor = new Predecessore();
                            Predecessor.PredecessorId = WBSPredecessor.WBSItemId;
                            if (WBSPredecessor.Type == WBSPredecessorType.FinishToStart)
                                Predecessor.Type = PredecessorLinkType.FinishToStart;
                            if (WBSPredecessor.Type == WBSPredecessorType.StartToFinish)
                                Predecessor.Type = PredecessorLinkType.StartToFinish;
                            if (WBSPredecessor.Type == WBSPredecessorType.FinishToStart)
                                Predecessor.Type = PredecessorLinkType.FinishToStart;
                            if (WBSPredecessor.Type == WBSPredecessorType.StartToStart)
                                Predecessor.Type = PredecessorLinkType.StartToStart;
                            Task.DependencyLinks.Add(Predecessor);
                        }
                    }
                }

                TaskPadre.Children.Add(Task);

                if (Entity.Children.Count() > 0)
                    AggiungiTask(entsHelper, TreeEntities, Entity.EntityId, TaskPadre.Children.Last());
            }
        }

        public List<TreeEntity> RetrieveWBSData(IEnumerable<Guid> Guids = null)
        {
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();
            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.WBS];
            if (Guids == null)
            {
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.WBS, null, null, out entitiesFound);
                TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
                TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, TreeInfo.Select(item => item.Id));
            }
            else
            {
                TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, Guids);
            }

            return TreeEntities;
        }

        private CalendariItem GetCaledarioFigliSeComune(Processo processo)
        {
            CalendariItem calendario = null;
            List<Guid> Guids = new List<Guid>();

            foreach (Processo Pr in SelectedItems)
                if (Pr.Children.Count() == 0)
                    Guids.Add(Pr.Id);

            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();
            List<WBSItem> WBSItems = new List<WBSItem>();
            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[BuiltInCodes.EntityType.WBS];
            WBSItems = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, Guids).Cast<WBSItem>().ToList();
            string codcalendario = null;
            int Contatore = 0;
            foreach (var wbsItem in WBSItems)
            {
                if (Contatore == 0)
                    codcalendario = wbsItem.Attributi[string.Join("_", BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id)].Valore.PlainText;
                else
                {
                    if (codcalendario != wbsItem.Attributi[string.Join("_", BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id)].Valore.PlainText)
                    {
                        codcalendario = null;
                        break;
                    }
                }
                Contatore++;
            }

            if (codcalendario != null)
                calendario = GetCalendarioItem(WBSItems.FirstOrDefault());

            return calendario;
        }

        public void UpdatePredecessor(List<Processo> TaskRelationShip, PredecessorLinkType linkType)
        {
            List<WBSPredecessor> WBSpredecessors = new List<WBSPredecessor>();
            Guid Target = Guid.Empty;

            double Delays = CalculateDelayDay(TaskRelationShip.LastOrDefault().Id, TaskRelationShip.FirstOrDefault(), TaskRelationShip.LastOrDefault(), linkType);

            Processo Task = TaskRelationShip.LastOrDefault();

            Target = TaskRelationShip.LastOrDefault().Id;
            Guid Source = TaskRelationShip.FirstOrDefault().Id;
            WBSPredecessorType Type = WBSPredecessorType.StartToFinish;
            if (linkType == PredecessorLinkType.FinishToFinish)
                Type = WBSPredecessorType.FinishToFinish;
            if (linkType == PredecessorLinkType.FinishToStart)
                Type = WBSPredecessorType.FinishToStart;
            if (linkType == PredecessorLinkType.StartToFinish)
                Type = WBSPredecessorType.StartToFinish;
            if (linkType == PredecessorLinkType.StartToStart)
                Type = WBSPredecessorType.StartToStart;
            WBSpredecessors.Add(new WBSPredecessor() { WBSItemId = Source, Type = Type });

            HashSet<Guid> Guids = new HashSet<Guid>();

            foreach (WBSPredecessor pred in WBSpredecessors)
                Guids = WBSView.OnTaskPredecessorAdd(Target, pred);

            NotRelaoadGanttAfterChartInteraction = true;
            if (Guids.Count() > 0)
                UpdateAttivita(Guids, Tasks);

        }

        public double CalculateDelayDay(Guid guid, Processo TaskSource, Processo TaskTarget, PredecessorLinkType linkType)
        {
            double Delays = 0;
            List<Guid> guids = new List<Guid>();
            guids.Add(guid);
            WBSItem Entity = (WBSItem)RetrieveWBSData(guids).FirstOrDefault();
            CalendariItem calendario = GetCalendarioItem(Entity);
            DateTimeCalculator timeCalc = null;
            if (GetCalendarioItem(Entity) != null)
                timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
            else
                return 0;

            if (linkType == PredecessorLinkType.FinishToStart)
                Delays = timeCalc.GetWorkingDaysBetween(TaskSource.FinishDate, TaskTarget.StartDate);
            if (linkType == PredecessorLinkType.FinishToFinish)
                Delays = timeCalc.GetWorkingDaysBetween(TaskSource.FinishDate, TaskTarget.FinishDate) - 1;
            if (linkType == PredecessorLinkType.StartToFinish)
                Delays = timeCalc.GetWorkingDaysBetween(TaskSource.StartDate, TaskTarget.FinishDate);
            if (linkType == PredecessorLinkType.StartToStart)
                Delays = timeCalc.GetWorkingDaysBetween(TaskSource.StartDate, TaskTarget.StartDate);

            if (Delays < 0)
                Delays = 0;
            return Delays;
        }

        private CalendariItem GetCalendarioItem(WBSItem ent)
        {
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
            if (wbsCalendarioId != Guid.Empty)
            {
                CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
                return calendario;
            }
            return null;
        }

        private int contatoreId = 0;
        private Dictionary<Guid, int> DictionatyGuidId = new Dictionary<Guid, int>();
        public void GenereteGanttSyncItemSource()
        {
            contatoreId = 1;
            DictionatyGuidId.Clear();
            TasksSync = new ObservableCollection<TaskDetails>();
            GenereteTaskGanttSyncRecursive(TasksSync, Tasks);
            GenereteTaskGanttSyncPredecessorRecursive(TasksSync, Tasks);
        }

        public void GenereteTaskGanttSyncRecursive(ObservableCollection<TaskDetails> TasksSync, ObservableCollection<Processo> TasksDev)
        {
            foreach (Processo Task in TasksDev)
            {
                TaskDetails TaskSync = new TaskDetails();
                TaskSync.TaskId = contatoreId;
                DictionatyGuidId.Add(Task.Id, contatoreId);
                contatoreId++;
                TaskSync.TaskName = Task.Description;
                //TaskSync.StartDate = new DateTime(Task.StartDate.Year, Task.StartDate.Month, Task.StartDate.Day);
                //TaskSync.FinishDate = new DateTime(Task.FinishDate.Year, Task.FinishDate.Month, Task.FinishDate.Day).AddDays(1);

                TaskSync.StartDate = new DateTime(Task.StartDate.Year, Task.StartDate.Month, Task.StartDate.Day, Task.StartDate.Hour, Task.StartDate.Minute, 0);
                //TaskSync.FinishDate = new DateTime(Task.FinishDate.Year, Task.FinishDate.Month, Task.FinishDate.Day, Task.FinishDate.Hour, Task.FinishDate.Minute, 0);
                TaskSync.Duration = TimeSpan.FromDays(Task.Durata);

                TaskSync.Progress = Task.Progress;

                GenereteTaskChildGanttSyncRecursive(TaskSync.Child, Task.Children);
                TasksSync.Add(TaskSync);
            }
        }

        private void GenereteTaskChildGanttSyncRecursive(ObservableCollection<IGanttTask> TasksSync, ObservableCollection<Processo> TasksDev)
        {
            foreach (Processo Task in TasksDev)
            {
                TaskDetails TaskSync = new TaskDetails();
                TaskSync.TaskId = contatoreId;
                DictionatyGuidId.Add(Task.Id, contatoreId);
                contatoreId++;
                TaskSync.TaskName = Task.Description;
                TaskSync.StartDate = new DateTime(Task.StartDate.Year, Task.StartDate.Month, Task.StartDate.Day);
                TaskSync.FinishDate = new DateTime(Task.FinishDate.Year, Task.FinishDate.Month, Task.FinishDate.Day).AddDays(1);
                TaskSync.Progress = Task.Progress;
                GenereteTaskChildGanttSyncRecursive(TaskSync.Child, Task.Children);
                TasksSync.Add(TaskSync);
            }
        }

        public void GenereteTaskGanttSyncPredecessorRecursive(ObservableCollection<TaskDetails> TasksSync, ObservableCollection<Processo> TasksDev)
        {
            int ContatoreTask = 0;

            foreach (Processo Task in TasksDev)
            {
                foreach (Predecessore WBSPredecessor in Task.DependencyLinks)
                {
                    Predecessor Predecessor = new Predecessor();
                    if (DictionatyGuidId.ContainsKey(WBSPredecessor.PredecessorId))
                    {
                        Predecessor.GanttTaskIndex = DictionatyGuidId[WBSPredecessor.PredecessorId];
                        if (WBSPredecessor.Type == PredecessorLinkType.FinishToFinish)
                            Predecessor.GanttTaskRelationship = GanttTaskRelationship.FinishToFinish;
                        if (WBSPredecessor.Type == PredecessorLinkType.StartToFinish)
                            Predecessor.GanttTaskRelationship = GanttTaskRelationship.StartToFinish;
                        if (WBSPredecessor.Type == PredecessorLinkType.FinishToStart)
                            Predecessor.GanttTaskRelationship = GanttTaskRelationship.FinishToStart;
                        if (WBSPredecessor.Type == PredecessorLinkType.StartToStart)
                            Predecessor.GanttTaskRelationship = GanttTaskRelationship.StartToStart;
                        TasksSync.ElementAt(ContatoreTask).Predecessor.Add(Predecessor);
                    }
                }

                if (Task.Children.Count() > 0)
                    GenereteTaskChildGanttSyncPredecessorRecursive(TasksSync.ElementAt(ContatoreTask).Child, Task.Children);

                ContatoreTask++;
            }
        }

        public void GenereteTaskChildGanttSyncPredecessorRecursive(ObservableCollection<IGanttTask> TasksSync, ObservableCollection<Processo> TasksDev)
        {
            int ContatoreTask = 0;

            foreach (Processo Task in TasksDev)
            {
                foreach (Predecessore WBSPredecessor in Task.DependencyLinks)
                {
                    Predecessor Predecessor = new Predecessor();
                    Predecessor.GanttTaskIndex = DictionatyGuidId[WBSPredecessor.PredecessorId];
                    if (WBSPredecessor.Type == PredecessorLinkType.FinishToFinish)
                        Predecessor.GanttTaskRelationship = GanttTaskRelationship.FinishToFinish;
                    if (WBSPredecessor.Type == PredecessorLinkType.StartToFinish)
                        Predecessor.GanttTaskRelationship = GanttTaskRelationship.StartToFinish;
                    if (WBSPredecessor.Type == PredecessorLinkType.FinishToStart)
                        Predecessor.GanttTaskRelationship = GanttTaskRelationship.FinishToStart;
                    if (WBSPredecessor.Type == PredecessorLinkType.StartToStart)
                        Predecessor.GanttTaskRelationship = GanttTaskRelationship.StartToStart;
                    TasksSync.ElementAt(ContatoreTask).Predecessor.Add(Predecessor);
                }

                if (Task.Children.Count() > 0)
                    GenereteTaskChildGanttSyncPredecessorRecursive(TasksSync.ElementAt(ContatoreTask).Child, Task.Children);

                ContatoreTask++;
            }
        }

        public void UpdateUi()
        {
            RaisePropertyChanged(GetPropertyName(() => TimeScaleDescription));
        }

        public void OnAddPredecessors(HashSet<Guid> changedEntitiesId)
        {
            UpdateAttivita(changedEntitiesId, Tasks);
        }

        private void UpdateAttivita(HashSet<Guid> Guids, ObservableCollection<Processo> CollectionAttivita)
        {
            foreach (var Attivita in CollectionAttivita)
            {
                if (Guids.Contains(Attivita.Id))
                {
                    List<Guid> Ids = new List<Guid>();
                    Ids.Add(Attivita.Id);
                    TreeEntity TreeEntity = RetrieveWBSData(Ids).FirstOrDefault();
                    Attivita.Code = TreeEntity.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText;
                    Attivita.Description = TreeEntity.Attributi[BuiltInCodes.Attributo.Nome].Valore.PlainText;
                    Attivita.Lavoro = (TreeEntity as WBSItem).GetOreLavoro().GetValueOrDefault();
                    Attivita.Durata = (TreeEntity as WBSItem).GetGiorniDurata().GetValueOrDefault();
                    Attivita.DurataCalendario = (TreeEntity as WBSItem).GetGiorniDurataCalendario().GetValueOrDefault();
                    if (TreeEntity != null)
                    {
                        WBSItem Entity = (WBSItem)TreeEntity;
                        if (IsActiveProgressiva)
                        {
                            Attivita.StartDate = GetDataAnonima((DateTime)(Entity).GetDataInizio());
                            Attivita.FinishDate = GetDataAnonima((DateTime)(Entity).GetDataFine());
                        }
                        else
                        {
                            Attivita.StartDate = (DateTime)(Entity).GetDataInizio();
                            Attivita.FinishDate = (DateTime)(Entity).GetDataFine();
                        }
                        Attivita.Progress = (double)Entity.GetTaskProgress();
                        Attivita.DependencyLinks.Clear();
                        foreach (WBSPredecessor wbsPredecessor in Entity.GetPredecessors().Items)
                        {
                            Predecessore predecessor = new Predecessore();
                            predecessor.PredecessorId = wbsPredecessor.WBSItemId;
                            if (wbsPredecessor.Type == WBSPredecessorType.FinishToFinish)
                                predecessor.Type = PredecessorLinkType.FinishToFinish;
                            if (wbsPredecessor.Type == WBSPredecessorType.StartToFinish)
                                predecessor.Type = PredecessorLinkType.StartToFinish;
                            if (wbsPredecessor.Type == WBSPredecessorType.FinishToStart)
                                predecessor.Type = PredecessorLinkType.FinishToStart;
                            if (wbsPredecessor.Type == WBSPredecessorType.StartToStart)
                                predecessor.Type = PredecessorLinkType.StartToStart;
                            Attivita.DependencyLinks.Add(predecessor);
                        }
                        EntitiesHelper entsHelper = new EntitiesHelper(DataService);
                        ValoreTesto Resources = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.TaskNote, false, false);
                        if (Resources != null)
                            Attivita.Name = Resources.PlainText;
                    }
                }

                if (Attivita.Children.Count() > 0)
                {
                    UpdateAttivita(Guids, Attivita.Children);
                }
            }
        }
        public bool StopSelectionChabged { get; set; }
        public void OnCheckedItemsChanged(HashSet<Guid> checkedEntitieSId)
        {
            SelectedItems.Clear();

            StopSelectionChabged = true;

            foreach (Guid Id in checkedEntitieSId)
            {
                SelezionaDaWBSAGantt(Tasks, Id);
            }

            StopSelectionChabged = false;

            if (IsActiveCriticalPath)
                UpdateUiElementiListaAttivita();
        }

        private void SelezionaDaWBSAGantt(ObservableCollection<Processo> Tasks, Guid Id)
        {
            foreach (Processo Task in Tasks)
            {
                if (Id == Task.Id)
                    SelectedItems.Add(Task);

                SelezionaDaWBSAGantt(Task.Children, Id);
            }
        }

        public void SelezionaDaGanttAWBS(bool IsDeselection, Guid selectionId)
        {
            List<Guid> SelectedIds = new List<Guid>();
            foreach (var AttivitaSelezionata in SelectedItems)
            {
                SelectedIds.Add(AttivitaSelezionata.Id);
                if (!IsDeselection)
                {
                    if (AttivitaSelezionata.Children.Count() > 0)
                    {
                        if (selectionId == AttivitaSelezionata.Id)
                        {
                            foreach (Guid Id in WBSView.GetFilteredDescendantsOf(AttivitaSelezionata.Id))
                                SelectedIds.Add(Id);
                        }
                    }
                }
            }




            if (!IsGeneraGanttActive)
            {
                if (SelectedItems.Count() > 0)
                    WBSView.OnCurrentTaskChanged(SelectedItems.LastOrDefault().Id);
                WBSView.OnTasksSelected(SelectedIds);
                OnCheckedItemsChanged(new HashSet<Guid>(SelectedIds));

                if (SelectedItems.Count() > 0)
                {
                    if (SelectedItems.LastOrDefault().Children.Count() > 0)
                    {
                        SelezionaFigliAttivita(SelectedItems.LastOrDefault().Id);
                    }
                }
            }
            else
                IsGeneraGanttActive = false;
        }

        public void SelezionaFigliAttivita(Guid Id)
        {
            HashSet<Guid> Discendenti = new HashSet<Guid>(WBSView.GetFilteredDescendantsOf(Id));
            Discendenti.Add(SelectedItems.LastOrDefault().Id);
            OnCheckedItemsChanged(Discendenti);
            WBSView.OnTasksSelected(Discendenti.ToList());
        }

        public void OnDisplayedItemsChanged(Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> filteredEntitiesViewInfo)
        {
            if (Tasks != null)
            {
                if (Tasks.Count() > 0)
                {
                    AggiornaAperturaChusuraRami(filteredEntitiesViewInfo);
                }
            }
        }

        public WBSVicibleEventArgs GetRamiApertiChiusi()
        {
            WBSVicibleEventArgs WBSVicibleEventArgs = new WBSVicibleEventArgs();
            WBSVicibleEventArgs.IndexWBSToExpande = new List<Guid>();
            WBSVicibleEventArgs.IndexWBSToCollapse = new List<Guid>();
            ContatoreRamo = 0;
            GetIndiceRamo(Tasks, WBSView.GetFilteredEntitiesViewInfo(), WBSVicibleEventArgs);
            return WBSVicibleEventArgs;
        }

        public void AggiornaAperturaChusuraRami()
        {
            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());
        }

        public void AggiornaAperturaChusuraRami(Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> filteredEntitiesViewInfo)
        {
            WBSVicibleEventArgs WBSVicibleEventArgs = new WBSVicibleEventArgs();
            WBSVicibleEventArgs.IndexWBSToExpande = new List<Guid>();
            WBSVicibleEventArgs.IndexWBSToCollapse = new List<Guid>();
            ContatoreRamo = 0;

            if (filteredEntitiesViewInfo.Count() > 0)
                GetIndiceRamo(Tasks, filteredEntitiesViewInfo, WBSVicibleEventArgs);

            AperturaChusuraRami?.Invoke(this, WBSVicibleEventArgs);
        }

        private int ContatoreRamo;
        private void GetIndiceRamo(ObservableCollection<Processo> tasks, Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> filteredEntitiesViewInfo, WBSVicibleEventArgs wBSVicibleEventArgs)
        {
            foreach (Processo task in tasks)
            {
                if (filteredEntitiesViewInfo.ContainsKey(task.Id))
                {
                    if (filteredEntitiesViewInfo[task.Id].IsExpanded)
                        wBSVicibleEventArgs.IndexWBSToExpande.Add(task.Id);
                    else
                        wBSVicibleEventArgs.IndexWBSToCollapse.Add(task.Id);
                }
                ContatoreRamo++;
                if (task.Children.Count() > 0)
                    GetIndiceRamo(task.Children, filteredEntitiesViewInfo, wBSVicibleEventArgs);
            }
        }

        public void OnFilteredItemsChanged(Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> filteredEntitiesViewInfo)
        {
            if (NotRelaoadGanttAfterChartInteraction == false)
            {
                Guid GuidFuoco = WBSView.GetSelectedItem();
                bool PreviousIsBarreDiRiepilogoChecked = IsBarreDiRiepilogoChecked;

                HashSet<Guid> PreviousSelectedLocal = new HashSet<Guid>();
                foreach (var Id in WBSView.GetCheckedItems())
                    PreviousSelectedLocal.Add(Id);

                List<Guid> Guids = new List<Guid>();
                foreach (var item in filteredEntitiesViewInfo)
                    Guids.Add(item.Key);
                GeneraGantt(Guids);

                IsBarreDiRiepilogoChecked = PreviousIsBarreDiRiepilogoChecked;
                WBSView.OnCurrentTaskChanged(GuidFuoco);
                WBSView.OnTasksSelected(PreviousSelectedLocal.ToList());
                OnCheckedItemsChanged(PreviousSelectedLocal);
                PreviousSelectedLocal.Clear();

                AggiornaAperturaChusuraRami(filteredEntitiesViewInfo);
            }
            else
            {
                if (IdsTraslazioneOModifcadataFineFinished.Count() > 0)
                {
                    UpdateAttivita(IdsTraslazioneOModifcadataFineFinished, Tasks);
                    IdsTraslazioneOModifcadataFineFinished.Clear();
                }
                NotRelaoadGanttAfterChartInteraction = false;
            }

            UpdateUiElementiListaAttivita();
        }

        public void OnReplaceAttributoPredecessori(HashSet<Guid> changedEntitiesId)
        {
            UpdateAttivita(changedEntitiesId, Tasks);
        }

        public void OnSelectedItemChanged(Guid selectedEntityId)
        {

        }

        public void OnWBSAttivitaItemsAdded(HashSet<Guid> changedEntitiesId)
        {
            Guid IdPadre = Guid.Empty;

            foreach (var Id in changedEntitiesId)
            {
                HashSet<Guid> Guids = new HashSet<Guid>();
                Guids.Add(Id);
                UpdateAttivita(Guids, Tasks);
                IdPadre = Id;
            }

            foreach (var Id in changedEntitiesId)
            {
                if (Id != IdPadre)
                {
                    HashSet<Guid> NewGuids = new HashSet<Guid>();
                    NewGuids.Add(Id);
                    AggiungiElementoFogliaAPadre(IdPadre, NewGuids, Tasks);
                }
            }

            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());
        }

        private void AggiungiElementoFogliaAPadre(Guid IdPadre, HashSet<Guid> NewGuids, ObservableCollection<Processo> listaAttivita)
        {
            foreach (Processo Attivita in listaAttivita)
            {
                if (Attivita.Id == IdPadre)
                {
                    Processo Task = CreateProcesso(NewGuids.FirstOrDefault());
                    Attivita.Children.Add(Task);
                    break;
                }
                if (Attivita.Children.Count() > 0)
                {
                    AggiungiElementoFogliaAPadre(IdPadre, NewGuids, Attivita.Children);
                }
            }
        }

        public void OnWBSClear()
        {
            ResetVaraibles?.Invoke(this, new EventArgs());
        }

        public void OnWBSCommitAction(ModelAction action, ModelActionResponse actionResponse)
        {
            if (actionResponse.NewId != Guid.Empty)
            {
                InsertAttivita(action, actionResponse, Tasks);
            }
            else
            {
                if (action.NewTargetEntitiesId.Count() > 0)
                {
                    GeneraGantt();
                    ScroolToDataInput?.Invoke(this, new DateEventArgs() { Data = GetDataInizioGantt()});
                    return;
                }
                else
                {
                    if (action.ActionName == ActionName.TREEENTITY_DELETE)
                    {
                        RemoveEntities(action, actionResponse, Tasks);
                    }
                    else
                    {
                        UpdateAttivitaGantt(action, actionResponse, Tasks, action.ActionName);
                    }
                }
            }

            UpdateUiElementiListaAttivita();
        }

        private void InsertAttivita(ModelAction action, ModelActionResponse response, ObservableCollection<Processo> listaAttivita)
        {
            if ((response.NewIds.Count() > 0 || listaAttivita.Count() == 0) && action.ActionName != ActionName.TREEENTITIES_PASTE)
            {
                GeneraGantt();
                return;
            }
            if (response.NewId != Guid.Empty || action.ActionName == ActionName.TREEENTITIES_PASTE)
            {
                AggiungiElementoSingolo(action, response, Tasks);
            }
        }

        private void AggiungiElementoSingolo(ModelAction action, ModelActionResponse response, ObservableCollection<Processo> listaAttivita)
        {
            int Contatore = 0;
            //AGGIUNGI IN CODA
            if (action.ActionName == ActionName.TREEENTITY_INSERT)
            {
                foreach (Processo Attivita in listaAttivita)
                {
                    Contatore++;
                    if (Attivita.Id == action.NewTargetEntitiesId.FirstOrDefault().Id)
                    {
                        Processo Task = CreateProcesso(response.NewId);
                        listaAttivita.Insert(Contatore, Task);
                        break;
                    }

                    if (Attivita.Children.Count() > 0)
                    {
                        AggiungiElementoSingolo(action, response, Attivita.Children);
                    }
                }
            }
            //CREA FIGLIO
            if (action.ActionName == ActionName.TREEENTITY_ADD_CHILD)
            {
                IEnumerable<Guid> EntitiesChanged = response.ChangedEntitiesId.Where(t => t != response.NewId);
                foreach (Processo Attivita in listaAttivita)
                {
                    Contatore++;

                    if (EntitiesChanged.Contains(Attivita.Id))
                    {
                        UpdateProcesso(Attivita, Attivita.Id);
                        Attivita.DependencyLinks.Clear();
                    }

                    if (Attivita.Id == action.NewTargetEntitiesId.FirstOrDefault().Id)
                    {
                        Processo Task = CreateProcesso(response.NewId);
                        Attivita.Children.Add(Task);
                        break;
                    }
                    if (Attivita.Children.Count() > 0)
                    {
                        AggiungiElementoSingolo(action, response, Attivita.Children);
                    }
                }
            }
            //CREA PADRE
            if (action.ActionName == ActionName.TREEENTITY_ADD_PARENT)
            {
                IEnumerable<Guid> EntitiesChanged = response.ChangedEntitiesId.Where(t => t != response.NewId);
                foreach (Processo Attivita in listaAttivita)
                {
                    if (Attivita.Id == action.NewTargetEntitiesId.FirstOrDefault().Id)
                    {
                        StopSelectionChabged = true;
                        Processo Task = CreateProcesso(response.NewId);
                        Processo TaskFoglia = new Processo(this);

                        TaskFoglia.Id = Attivita.Id;
                        TaskFoglia.Code = Attivita.Code;
                        TaskFoglia.Description = Attivita.Description;
                        TaskFoglia.StartDate = Attivita.StartDate;
                        TaskFoglia.FinishDate = Attivita.FinishDate;
                        TaskFoglia.Name = Attivita.Name;
                        TaskFoglia.Progress = Attivita.Progress;
                        TaskFoglia.Lavoro = Attivita.Lavoro;
                        TaskFoglia.Durata = Attivita.Durata;
                        TaskFoglia.DurataCalendario = Attivita.DurataCalendario;

                        Attivita.Id = Task.Id;
                        Attivita.StartDate = Task.StartDate;
                        Attivita.FinishDate = Task.FinishDate;
                        Attivita.Name = Task.Name;
                        Attivita.Progress = Task.Progress;
                        Attivita.Lavoro = Task.Lavoro;
                        Attivita.Durata = Task.Durata;
                        Attivita.DurataCalendario = Task.DurataCalendario;

                        foreach (Processo ProcessoFiglio in Attivita.Children)
                        {
                            Processo Pr = new Processo(this);
                            Pr.Id = ProcessoFiglio.Id;
                            Pr.Code = ProcessoFiglio.Code;
                            Pr.Description = ProcessoFiglio.Description;
                            Pr.StartDate = ProcessoFiglio.StartDate;
                            Pr.FinishDate = ProcessoFiglio.FinishDate;
                            Pr.Name = ProcessoFiglio.Name;
                            Pr.Children = new ObservableCollection<Processo>(ProcessoFiglio.Children);
                            TaskFoglia.Children.Add(Pr);
                        }

                        Attivita.Children.Clear();
                        Attivita.Children.Add(TaskFoglia);

                        StopSelectionChabged = false;
                        break;
                    }
                    if (Attivita.Children.Count() > 0)
                    {
                        AggiungiElementoSingolo(action, response, Attivita.Children);
                    }
                    Contatore++;
                }
            }

            bool Found = false;
            //COPIA ELEMENTI
            if (action.ActionName == ActionName.TREEENTITIES_PASTE)
            {
                foreach (Processo Attivita in listaAttivita)
                {
                    Contatore++;
                    if (Attivita.Id == action.NewTargetEntitiesId.FirstOrDefault().Id)
                    {
                        Found = true;
                        break;
                    }

                    if (Attivita.Children.Count() > 0)
                    {
                        AggiungiElementoSingolo(action, response, Attivita.Children);
                    }
                }

                if (Found)
                {
                    foreach (Guid guid in response.NewIds)
                    {
                        Processo Task = CreateProcesso(guid, true);
                        listaAttivita.Insert(Contatore, Task);
                        Contatore++;
                    }
                }
            }
        }

        private Processo CreateProcesso(Guid Guid, bool AddChildren = false)
        {
            List<Guid> Guids = new List<Guid>();
            Guids.Add(Guid);
            WBSItem Entity = (WBSItem)RetrieveWBSData(Guids).FirstOrDefault();
            DateTime DataInizio = new DateTime();
            DateTime DataFine = new DateTime();
            if (IsActiveProgressiva)
            {
                if (Entity.GetDataInizio().HasValue)
                    DataInizio = GetDataAnonima((DateTime)Entity.GetDataInizio());
                if (Entity.GetDataFine().HasValue)
                    DataFine = GetDataAnonima((DateTime)Entity.GetDataFine());
            }
            else
            {
                if (Entity.GetDataInizio().HasValue)
                    DataInizio = (DateTime)Entity.GetDataInizio();
                if (Entity.GetDataFine().HasValue)
                    DataFine = (DateTime)Entity.GetDataFine();
            }
            Processo Task = new Processo(this);
            Task.StartDate = DataInizio;
            Task.FinishDate = DataFine;
            Task.Id = Entity.EntityId;

            if (AddChildren)
                foreach (WBSItem Child in Entity.Children)
                    Task.Children.Add(CreateProcesso(Child.EntityId, true));

            return Task;
        }

        private void UpdateProcesso(Processo Task, Guid Guid)
        {
            List<Guid> Guids = new List<Guid>();
            Guids.Add(Guid);
            WBSItem Entity = (WBSItem)RetrieveWBSData(Guids).FirstOrDefault();
            DateTime DataInizio = new DateTime();
            DateTime DataFine = new DateTime();
            if (Entity.GetDataInizio().HasValue)
            {
                if (IsActiveProgressiva)
                    DataInizio = GetDataAnonima((DateTime)Entity.GetDataInizio());
                else
                    DataInizio = (DateTime)Entity.GetDataInizio();
            }
            if (Entity.GetDataFine().HasValue)
            {
                if (IsActiveProgressiva)
                    DataFine = GetDataAnonima((DateTime)Entity.GetDataFine());
                else
                    DataFine = (DateTime)Entity.GetDataFine();
            }
            Task.StartDate = DataInizio;
            Task.FinishDate = DataFine;
            Task.Progress = (double)Entity.GetTaskProgress();
        }

        private void UpdateAttivitaGantt(ModelAction action, ModelActionResponse response, ObservableCollection<Processo> ListaAttivitaFiglie, ActionName ActionName)
        {
            foreach (Processo Attivita in ListaAttivitaFiglie)
                UpdateAttivita(action, response, Attivita, ActionName);
        }

        private void UpdateAttivita(ModelAction action, ModelActionResponse response, Processo Attivita, ActionName actionName)
        {
            List<Guid> GuidToUpdate = new List<Guid>();
            GuidToUpdate.AddRange(action.EntitiesId);
            GuidToUpdate.AddRange(response.ChangedEntitiesId);

            if (GuidToUpdate.Contains(Attivita.Id))
            {
                List<Guid> Ids = new List<Guid>();
                Ids.Add(Attivita.Id);
                TreeEntity TreeEntity = RetrieveWBSData(Ids).FirstOrDefault();
                Attivita.Code = TreeEntity.Attributi[BuiltInCodes.Attributo.Codice].Valore.PlainText;
                Attivita.Description = TreeEntity.Attributi[BuiltInCodes.Attributo.Nome].Valore.PlainText;
                Attivita.Lavoro = (TreeEntity as WBSItem).GetOreLavoro().GetValueOrDefault();
                Attivita.Durata = (TreeEntity as WBSItem).GetGiorniDurata().GetValueOrDefault();
                Attivita.DurataCalendario = (TreeEntity as WBSItem).GetGiorniDurataCalendario().GetValueOrDefault();
                if (TreeEntity != null)
                {
                    WBSItem Entity = (WBSItem)TreeEntity;
                    if (IsActiveProgressiva)
                    {
                        Attivita.StartDate = GetDataAnonima((DateTime)(Entity).GetDataInizio());
                        Attivita.FinishDate = GetDataAnonima((DateTime)(Entity).GetDataFine());
                    }
                    else
                    {
                        Attivita.StartDate = (DateTime)(Entity).GetDataInizio();
                        Attivita.FinishDate = (DateTime)(Entity).GetDataFine();
                    }

                    if (((WBSItem)TreeEntity).GetDataFine().Value > WBSView.GetMaxDataFineWBSItems().AddDays(365))
                        GeneraScalaNumerica();
                    Attivita.Progress = (double)Entity.GetTaskProgress();
                    Attivita.DependencyLinks.Clear();
                    foreach (WBSPredecessor wbsPredecessor in Entity.GetPredecessors().Items)
                    {
                        Predecessore predecessor = new Predecessore();
                        predecessor.PredecessorId = wbsPredecessor.WBSItemId;
                        if (wbsPredecessor.Type == WBSPredecessorType.FinishToFinish)
                            predecessor.Type = PredecessorLinkType.FinishToFinish;
                        if (wbsPredecessor.Type == WBSPredecessorType.StartToFinish)
                            predecessor.Type = PredecessorLinkType.StartToFinish;
                        if (wbsPredecessor.Type == WBSPredecessorType.FinishToStart)
                            predecessor.Type = PredecessorLinkType.FinishToStart;
                        if (wbsPredecessor.Type == WBSPredecessorType.StartToStart)
                            predecessor.Type = PredecessorLinkType.StartToStart;
                        Attivita.DependencyLinks.Add(predecessor);
                    }
                    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
                    ValoreTesto Resources = (ValoreTesto)entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.TaskNote, false, false);
                    if (Resources != null)
                    {
                        Attivita.Name = Resources.PlainText;
                    }
                }
            }

            if (Attivita.Children.Count() > 0)
            {
                UpdateAttivitaGantt(action, response, Attivita.Children, actionName);
            }
        }

        private void RemoveEntities(ModelAction action, ModelActionResponse response, ObservableCollection<Processo> listaAttivita)
        {
            GeneraGantt();
            return;
        }

        private bool NotRelaoadGanttAfterChartInteraction;
        private HashSet<Guid> IdsTraslazioneOModifcadataFineFinished = new HashSet<Guid>();
        public void ModificaDataFineProcesso(Processo Processo, DateTime FinishDateValidate)
        {
            NotRelaoadGanttAfterChartInteraction = true;
            IdsTraslazioneOModifcadataFineFinished.Add(Processo.Id);
            SelectedItems.Clear();
            SelezionaDaWBSAGantt(Tasks, Processo.Id);

            List<Guid> GuidsToExclude = new List<Guid>();
            Guid EntityGuid = Processo.Id;
            List<Guid> Ids = new List<Guid>();
            Ids.Add(EntityGuid);
            TreeEntity TreeEntity = RetrieveWBSData(Ids).FirstOrDefault();
            DateTime p = new DateTime();

            double OreLavoro = 0;

            p = FinishDateValidate;

            ModelAction Action = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.WBS,
                EntitiesId = new HashSet<Guid>(),
                NestedActions = new List<ModelAction>(),
            };
            Action.ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY;
            Action.AttributoCode = BuiltInCodes.Attributo.Lavoro;
            Action.EntitiesId.Add(EntityGuid);
            CalendariItem calendario = GetCalendarioItem((WBSItem)TreeEntity);

            if (calendario != null)
            {
                if (IsActiveProgressiva)
                {
                    p = GetEndingDateTimeOfDayPerTimescaleRuler(calendario, GetDataReale(FinishDateValidate));
                    OreLavoro = (double)GetOreLavoroBetweenDates((WBSItem)TreeEntity, GetDataReale(Processo.StartDate), p);
                }

                else
                {
                    p = GetEndingDateTimeOfDayPerTimescaleRuler(calendario, FinishDateValidate);
                    OreLavoro = (double)GetOreLavoroBetweenDates((WBSItem)TreeEntity, Processo.StartDate, p);
                }

                if (FinishDateValidate == Processo.StartDate)
                    return;

                Action.NewValore = new ValoreReale() { V = OreLavoro.ToString() };
                HashSet<Guid> GuidsUpdated = WBSView.OnTaskAction(Action);
                HashSet<Guid> GuidsToUpdate = new HashSet<Guid>();
                GuidsToExclude.Add(EntityGuid);
                foreach (var item in GuidsUpdated)
                    if (!GuidsToExclude.Contains(item))
                        GuidsToUpdate.Add(item);

                if (GuidsToUpdate.Count() > 0)
                    UpdateAttivita(GuidsToUpdate, Tasks);

                GuidsToExclude.Clear();
                TreeEntity = RetrieveWBSData(Ids).FirstOrDefault();
                if (IsActiveProgressiva)
                    Processo.FinishDate = GetDataAnonima(((WBSItem)TreeEntity).GetDataFine().Value);
                else
                    Processo.FinishDate = ((WBSItem)TreeEntity).GetDataFine().Value;

                if (((WBSItem)TreeEntity).GetDataFine().Value > WBSView.GetMaxDataFineWBSItems().AddDays(365))
                    GeneraScalaNumerica();
            }

            UpdateUiElementiListaAttivita();
        }

        private DateTime GetEndingDateTimeOfDayPerTimescaleRuler(CalendariItem Calendario, DateTime Date)
        {
            DateTimeCalculator timeCalc = new DateTimeCalculator(Calendario.GetWeekHours(), Calendario.GetCustomDays());
            DateTime dateCalc = new DateTime();
            switch (ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key)
            {
                case 0:
                    break;
                case 1:

                    break;
                case 2:
                    dateCalc = timeCalc.GetEndingDateTimeOfQuarter(Date).Value;
                    break;
                case 3:
                    dateCalc = timeCalc.GetEndingDateTimeOfMonth(Date).Value;
                    break;
                case 4:
                    break;
                case 5:
                    dateCalc = timeCalc.GetEndingDateTimeOfWeek(Date).Value;
                    break;
                case 6:
                    dateCalc = timeCalc.GetEndingDateTimeOfDay(Date);
                    break;
                case 7:
                    dateCalc = timeCalc.GetEndingDateTimeOfHour(Date).Value;
                    break;
                case 8:
                    break;
                default:
                    break;
            }
            return dateCalc;
        }

        public double? GetOreLavoroBetweenDates(WBSItem ent, DateTime start, DateTime end)
        {
            double? lavoro = null;
            CalendariItem calendario = GetCalendarioItem(ent);

            if (GetCalendarioItem(ent) != null)
            {
                DateTimeCalculator timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                lavoro = timeCalc.GetWorkingMinutesBetween(start, end) / 60;
            }

            return lavoro;
        }

        public void TraslaProcesso(Processo Processo, DateTime StartDateValidate)
        {
            NotRelaoadGanttAfterChartInteraction = true;
            List<Guid> GuidsToExclude = new List<Guid>();
            Guid EntityGuid = Processo.Id;
            List<Guid> Ids = new List<Guid>();
            Ids.Add(EntityGuid);
            TreeEntity TreeEntity = RetrieveWBSData(Ids).FirstOrDefault();
            DateTime p = new DateTime();

            List<Guid> Guids = new List<Guid>();
            double Offset = 0;
            CalendariItem calendario = GetCalendarioItem((WBSItem)TreeEntity);

            if (calendario == null && TreeEntity.IsParent)
                calendario = GetCaledarioFigliSeComune(Processo);

            if (Processo.StartDate.Year < 2074 && IsActiveProgressiva)
            {
                Processo.StartDate = GetDataAnonima(GanttData.DataInizio);
            }

            if (calendario != null)
            {
                DateTimeCalculator timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());

                if (IsActiveProgressiva)
                    p = GetStartingDateTimeOfDayPerTimescaleRuler(calendario, GetDataReale(StartDateValidate));
                else
                    p = GetStartingDateTimeOfDayPerTimescaleRuler(calendario, StartDateValidate);
                Offset = timeCalc.GetWorkingDaysBetween(((WBSItem)TreeEntity).GetDataInizio().Value, p);
            }
            else
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Non è presente un calendario comune tra le attivita selezionate"));
            }
            Guids.Add(EntityGuid);

            foreach (Processo AttivitaSelezionata in SelectedItems)
            {
                Guid Guid = AttivitaSelezionata.Id;
                if (!Guids.Contains(Guid))
                    Guids.Add(Guid);
            }

            HashSet<Guid> GuidsUpdated = WBSView.OnTasksOffset(Guids, Offset);
            foreach (var guid in GuidsUpdated)
            {
                IdsTraslazioneOModifcadataFineFinished.Add(guid);
            }
            UpdateUiElementiListaAttivita();
        }

        public void ModificaProgressProcesso(Processo Processo, double Progress)
        {
            NotRelaoadGanttAfterChartInteraction = true;
            IdsTraslazioneOModifcadataFineFinished.Add(Processo.Id);
            Guid EntityGuid = Processo.Id;
            List<Guid> Ids = new List<Guid>();
            Ids.Add(EntityGuid);

            ModelAction Action = new ModelAction()
            {
                EntityTypeKey = BuiltInCodes.EntityType.WBS,
                EntitiesId = new HashSet<Guid>(),
                NestedActions = new List<ModelAction>(),
            };
            Action.ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY;
            Action.AttributoCode = BuiltInCodes.Attributo.TaskProgress;
            Action.EntitiesId.Add(EntityGuid);

            Action.NewValore = new ValoreReale() { V = Progress.ToString() };
            HashSet<Guid> GuidsUpdated = WBSView.OnTaskAction(Action);
        }

        private DateTime GetStartingDateTimeOfDayPerTimescaleRuler(CalendariItem Calendario, DateTime Date)
        {
            DateTimeCalculator timeCalc = new DateTimeCalculator(Calendario.GetWeekHours(), Calendario.GetCustomDays());
            DateTime dateCalc = new DateTime();
            switch (ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key)
            {
                case 0:
                    break;
                case 1:

                    break;
                case 2:
                    dateCalc = timeCalc.GetStartingDateTimeOfQuarter(Date).Value;
                    break;
                case 3:
                    dateCalc = timeCalc.GetStartingDateTimeOfMonth(Date).Value;
                    break;
                case 4:
                    break;
                case 5:
                    dateCalc = timeCalc.GetStartingDateTimeOfWeek(Date).Value;
                    break;
                case 6:
                    dateCalc = timeCalc.GetStartingDateTimeOfDay(Date);
                    break;
                case 7:
                    dateCalc = timeCalc.GetStartingDateTimeOfHour(Date).Value;
                    break;
                case 8:
                    break;
                default:
                    break;
            }
            return dateCalc;
        }

        public System.Windows.Media.Brush GetColorNode(Guid TaskId)
        {
            System.Windows.Media.Brush color = GanttChartStyleSettingView.ColorTaskNode.SampleBrush;

            if (IsActiveCriticalPath)
            {
                if (AttivitaPercorsoCritico.Contains(TaskId))
                    if (GanttChartStyleSettingView.ColorCriticalPath != null)
                        color = GanttChartStyleSettingView.ColorCriticalPath.SampleBrush;
                    else
                        color = System.Windows.Media.Brushes.Red;
            }
            if (SelectedItems.Where(r => r.Id == TaskId).FirstOrDefault() != null && IsActiveCriticalPath)
            {
                if (AttivitaPercorsoCritico.Contains(TaskId))
                    if (GanttChartStyleSettingView.ColorCriticalPath != null)
                        color = ColorsHelper.Blend(System.Windows.Media.Brushes.LightGray, GanttChartStyleSettingView.ColorCriticalPath.SampleBrush, 0.5);
                    else
                        color = ColorsHelper.Blend(System.Windows.Media.Brushes.LightGray, System.Windows.Media.Brushes.Red, 0.5);
            }
            return color;
        }
        private void UpdateUiElementiListaAttivita()
        {
            AttivitaPercorsoCritico = WBSView.GetCriticalPath();
            UpdateUiElementiListaAttivita(Tasks);
        }
        private void UpdateUiElementiListaAttivita(ObservableCollection<Processo> listaAttivita)
        {
            foreach (Processo Attivita in listaAttivita)
            {
                Attivita.UpdateUI();
                UpdateUiElementiListaAttivita(Attivita.Children);
            }
        }

        public DateTime GetDataAnonima(DateTime DataReale, bool GetLastIfNotExist = false)
        {
            KeyValuePair<DateTime, DateTime> Valore = AssociazioneScale.DaAnonimaAReale.Where(r => r.Value.Year == DataReale.Year && r.Value.Month == DataReale.Month && r.Value.Day == DataReale.Day).FirstOrDefault();
            if (Valore.Key == new DateTime())
            {
                if (GetLastIfNotExist)
                {
                    return new DateTime(AssociazioneScale.DaAnonimaAReale.LastOrDefault().Key.Year, AssociazioneScale.DaAnonimaAReale.LastOrDefault().Key.Month, AssociazioneScale.DaAnonimaAReale.LastOrDefault().Key.Day,
                    AssociazioneScale.DaAnonimaAReale.LastOrDefault().Key.Hour, AssociazioneScale.DaAnonimaAReale.LastOrDefault().Key.Minute, AssociazioneScale.DaAnonimaAReale.LastOrDefault().Key.Second);
                }
                else
                {
                    return new DateTime();
                }
            }
            DateTime DataAnonima = new DateTime(Valore.Key.Year, Valore.Key.Month, Valore.Key.Day, DataReale.Hour, DataReale.Minute, DataReale.Second);
            return DataAnonima;
        }

        public DateTime GetDataReale(DateTime DataAnonima)
        {
            DateTime data = new DateTime(DataAnonima.Year, DataAnonima.Month, DataAnonima.Day);
            if (AssociazioneScale.DaAnonimaAReale.ContainsKey(data))
            {
                DateTime Valore = AssociazioneScale.DaAnonimaAReale[data];
                Valore = Valore.AddHours(DataAnonima.Hour);
                Valore = Valore.AddMinutes(DataAnonima.Minute);
                return Valore;
            }
            else
            {
                return new DateTime();
            }
        }

        public int GetNumberDataSuScalaProgressiva(DateTime Date)
        {
            int Number = 0;
            int unitaTempo = ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key;
            //DateTime DataAssociata = AssociazioneScale.DaAnonimaAReale.FirstOrDefault(r => r.Key == Date).Value;
            switch (unitaTempo)
            {
                //ANNI
                case 0:
                    Number = GanttView.ScalaNumericaAnonima[Date].ProgressivoNumericoAnnoAnonima;
                    break;
                //MESI
                case 3:
                    Number = GanttView.ScalaNumericaAnonima[Date].ProgressivoNumericoMeseAnonima;
                    break;
                //SETTIMANE
                case 5:
                    Number = GanttView.ScalaNumericaAnonima[Date].ProgressivoNumericoSettimanaAnonima;
                    break;
                //GIORNI
                case 6:
                    Number = GanttView.ScalaNumericaAnonima[Date].ProgressivoNumericoGiornoAnonima;
                    break;
                //ORE
                case 7:
                    Number = GanttView.ScalaNumericaAnonima[Date].ProgressivoNumericoOraAnonima;
                    break;
                default:

                    break;
            }

            return Number;
        }

        public int GetNumberSuScalaFeriale(DateTime Date)
        {
            int Number = 0;
            int unitaTempo = ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key;
            switch (unitaTempo)
            {
                //ANNI
                case 0:
                    Number = GanttView.ScalaNumericaFeriale[Date].ProgressivoNumericoAnno;
                    break;
                //MESI
                case 3:
                    Number = GanttView.ScalaNumericaFeriale[Date].ProgressivoNumericoMese;
                    break;
                //SETTIMANE
                case 5:
                    Number = GanttView.ScalaNumericaFeriale[Date].ProgressivoNumericoSettimana;
                    break;
                //GIORNI
                case 6:
                    Number = GanttView.ScalaNumericaFeriale[Date].ProgressivoNumericoGiorno;
                    break;
                //ORE
                case 7:
                    Number = GanttView.ScalaNumericaFeriale[Date].ProgressivoNumericoOra;
                    break;
                default:

                    break;
            }

            return Number;
        }
        public void OnWBSItemDoubleClick()
        {
            VaiSuAttivitaSelezionate();
        }

        private void VaiSuAttivitaSelezionate()
        {
            List<Guid> guids = new List<Guid>();
            guids.Add(WBSView.GetSelectedItem());
            WBSItem Entity = (WBSItem)RetrieveWBSData(guids).FirstOrDefault();
            DateEventArgs DateEventArgs = new DateEventArgs();
            if (IsActiveProgressiva)
            {
                if (Entity.GetDataInizio().HasValue)
                    DateEventArgs.Data = GetDataAnonima((DateTime)Entity.GetDataInizio());
            }
            else
            {

                if (Entity != null && Entity.GetDataInizio().HasValue)
                    DateEventArgs.Data = (DateTime)Entity.GetDataInizio();
            }
            ScroolToDataInput?.Invoke(this, DateEventArgs);
        }

        public ICommand ScollegaCommand
        {
            get
            {
                return new CommandHandler(() => this.ScollegaPredecessoriSuSelezionati());
            }
        }

        public void ScollegaPredecessoriSuSelezionati()
        {
            if (SelectedItems.Count() >= 2)
            {
                List<Guid> Guids = new List<Guid>();

                foreach (Processo AttivitaSelezionata in SelectedItems)
                {
                    Guids.Add(AttivitaSelezionata.Id);
                }
                WBSView.OnTasksPredecessorDisconnect(Guids);

                ScollegaPredecessori(Guids, Tasks);

                NotRelaoadGanttAfterChartInteraction = true;
            }
        }

        public ICommand GotoFirstDateCommand
        {
            get
            {
                return new CommandHandler(() => this.GotoFirstDate3DModel());
            }
        }

        private void GotoFirstDate3DModel()
        {
            if (GanttData != null)
            {
                Data = GanttData.DataInizio;
                TrackBarValue = 0;
            }
        }

        public ICommand DateChangedCommand
        {
            get
            {
                return new CommandHandler(() => this.Set3DModelByDateButton());
            }
        }

        private void Set3DModelByDateButton()
        {
            Set3DModelByDate();
        }

        private void Set3DModelByDate()
        {
            DateEventArgs DateEventArgs = new DateEventArgs();
            DateEventArgs.Data = Data;
            ScroolToDataInput?.Invoke(this, DateEventArgs);

            Dictionary<DateTime, System.Windows.Media.SolidColorBrush> dates = new Dictionary<DateTime, System.Windows.Media.SolidColorBrush>();
            dates.Add(Data, System.Windows.Media.Brushes.SteelBlue);
            GeneraStripLines(dates);
            //GeneraStripLines(new List<DateTime>() { Data });

            var TreeEntities = RetrieveWBSData();
            Dictionary<bool, HashSet<Guid>> tasksVisible3DModel = new Dictionary<bool, HashSet<Guid>>();
            tasksVisible3DModel.Add(true, new HashSet<Guid>());
            tasksVisible3DModel.Add(false, new HashSet<Guid>());
            foreach (WBSItem entity in TreeEntities)
            {
                Task3DModelProperty task3DModelProperty = new Task3DModelProperty();
                task3DModelProperty.Id = entity.EntityId;
                if (entity.GetDataFine() <= Data)
                    tasksVisible3DModel[true].Add(entity.EntityId);
                else
                    tasksVisible3DModel[false].Add(entity.EntityId);
            }
            Update3DModel(tasksVisible3DModel);
        }

        private void Update3DModel(Dictionary<bool, HashSet<Guid>> tasksVisible3DModel)
        {
            HashSet<Guid> wbsItemsIdVisible = tasksVisible3DModel[true].ToHashSet();
            HashSet<Guid> wbsItemsIdHidden = tasksVisible3DModel[false].ToHashSet();
            List<Model3dObjectKey> model3dIdsVisible = WBSView.GetModel3dObjectsKeyByWBSItemsId(wbsItemsIdVisible);
            List<Model3dObjectKey> model3dIdsHidden = WBSView.GetModel3dObjectsKeyByWBSItemsId(wbsItemsIdHidden);

            //Ricavo il Servizio per il collegamento al modello 3d
            I3DModelService model3dService = WBSView.GetModel3dService();
            if (model3dService == null)
                return;

            bool IsVisible = false;
            if (model3dIdsVisible.Count() > 0)
                IsVisible = true;
            bool IsTrasparent = false;
            if (model3dIdsHidden.Count() > 0)
                IsTrasparent = true;

            Dictionary<Model3dObjectKey, bool> model3dIdsVisibility = model3dIdsVisible.ToDictionary(item => item, item => true).Union(model3dIdsHidden.ToDictionary(item => item, item => false)).ToDictionary(item => item.Key, item => item.Value);
            Dictionary<Model3dObjectKey, bool> model3dIdsTransparency = model3dIdsVisible.Union(model3dIdsHidden).ToDictionary(item => item, item => false);
            model3dService?.SetElementsVisibility(model3dIdsVisibility, null, model3dIdsTransparency, null);
        }
        public void ApplicaScalePuntiNotevoli()
        {
            CreateScalePuntiNotevoli();
            SetAllTransparent3DModel();
            Set3DModelByDate();
        }

        public void CreateScalePuntiNotevoli()
        {
            List<DateTime> datetimesList = new List<DateTime>();
            ScalaPuntiNotevoliDate = new Dictionary<DateTime, int>();
            ScalaPuntiNotevoliDate[GanttData.DataInizio] = 0;
            var TreeEntities = RetrieveWBSData();
            DateTime date = new DateTime();
            foreach (WBSItem entity in TreeEntities)
            {
                if (!entity.IsParent)
                {
                    date = entity.GetDataInizio().Value;
                    if (!datetimesList.Contains(date))
                        datetimesList.Add(date);
                    date = entity.GetDataFine().Value;
                    if (!datetimesList.Contains(date))
                        datetimesList.Add(date);
                }
            }

            datetimesList = datetimesList.OrderBy(o => o).ToList();

            int contatore = 1;
            foreach (var item in datetimesList)
            {
                ScalaPuntiNotevoliDate[item] = contatore;
                contatore++;
            }

            MaximunSlider3DModelValue = contatore - 1;
        }

        private void SetAllTransparent3DModel()
        {
            I3DModelService model3dService = WBSView.GetModel3dService();
            model3dService?.SetElementsVisibility(new Dictionary<Model3dObjectKey, bool>(), true, new Dictionary<Model3dObjectKey, bool>(), true);
        }

        public void GoToPreviousDateIn3DModel()
        {
            if (Data == new DateTime())
                return;
            int value = ScalaPuntiNotevoliDate[Data];
            KeyValuePair<DateTime, int> ValorePrecedente = ScalaPuntiNotevoliDate.Where(v => v.Value == (value - 1)).FirstOrDefault();
            if (ValorePrecedente.Key != new DateTime())
            {
                Data = ValorePrecedente.Key;
                Set3DModelByDate();
            }

        }

        public bool BreakCycle;

        System.Threading.CancellationTokenSource tokenSource;
        public async Task IterateDateIn3DModel()
        {
            if (ScalaPuntiNotevoliDate.LastOrDefault().Key == Data)
                Data = ScalaPuntiNotevoliDate.FirstOrDefault().Key;

            PlayPauseButtonFont = "\ue11e";
            BreakCycle = false;

            tokenSource = new System.Threading.CancellationTokenSource();
            var token = tokenSource.Token;

            bool ContinueIteration = false;
            foreach (KeyValuePair<DateTime, int> PuntoNotevole in ScalaPuntiNotevoliDate)
            {
                try
                {
                    await Task.Delay(500, token);
                    if (ScalaPuntiNotevoliDate.LastOrDefault().Key == PuntoNotevole.Key)
                        PlayPauseButtonFont = "\ue082";
                }
                catch (OperationCanceledException)
                {
                    Data = PuntoNotevole.Key;
                    TrackBarValue = PuntoNotevole.Value;
                    break;
                }

                if (BreakCycle)
                {
                    BreakCycle = false;
                    break;
                }

                TrackBarValue = PuntoNotevole.Value;
                if (PuntoNotevole.Key == Data)
                {
                    GoToNextDateIn3DModel(ScalaPuntiNotevoliDate[Data]);
                    ContinueIteration = true;
                }
                else
                {
                    if (ContinueIteration)
                    {
                        GoToNextDateIn3DModel(PuntoNotevole.Value);
                    }
                }
            }
        }

        public void Cancel3DModelExecution()
        {
            BreakCycle = true;
            PlayPauseButtonFont = "\ue082";
            tokenSource.Cancel();
        }

        public void GoToNextDateIn3DModel(double? Value)
        {
            if (Value == null)
                Value = 0;
            KeyValuePair<DateTime, int> ValoreSuccessivo = ScalaPuntiNotevoliDate.Where(v => v.Value == Value).FirstOrDefault();
            if (ValoreSuccessivo.Key != new DateTime())
            {
                Data = ValoreSuccessivo.Key;
                TrackBarValue = ValoreSuccessivo.Value;
                Set3DModelByDate();
            }
        }

        public void ScollegaPredecessori(List<Guid> GuidsToDisconnect)
        {
            List<Guid> Guids = new List<Guid>();

            foreach (Guid GuidAttivita in GuidsToDisconnect)
            {
                Guids.Add(GuidAttivita);
            }
            WBSView.OnTasksPredecessorDisconnect(Guids);

            NotRelaoadGanttAfterChartInteraction = true;
        }

        public void DeselezinoaSelezionati()
        {
            SelectedItems.Clear();
            List<Guid> SelectedIds = new List<Guid>();
            WBSView.OnTasksSelected(SelectedIds);
        }


        static int count = 0;
        private void ScollegaPredecessori(List<Guid> Guids, ObservableCollection<Processo> listaAttivita)
        {

            if (listaAttivita == null || !listaAttivita.Any())
                return;

            foreach (Guid Guid in Guids)
            {
                count++;

                foreach (Processo Attivita in listaAttivita)
                {
                    if (Guid == Attivita.Id)
                    {
                        List<Guid> Ids = new List<Guid>();
                        Ids.Add(Guid);
                        WBSItem Entity = (WBSItem)RetrieveWBSData(Ids).FirstOrDefault();
                        if (Entity != null)
                        {
                            Attivita.DependencyLinks.Clear();
                            foreach (WBSPredecessor wbsPredecessor in Entity.GetPredecessors().Items)
                            {
                                Predecessore predecessor = new Predecessore();
                                predecessor.PredecessorId = wbsPredecessor.WBSItemId;
                                if (wbsPredecessor.Type == WBSPredecessorType.FinishToFinish)
                                    predecessor.Type = PredecessorLinkType.FinishToFinish;
                                if (wbsPredecessor.Type == WBSPredecessorType.StartToFinish)
                                    predecessor.Type = PredecessorLinkType.StartToFinish;
                                if (wbsPredecessor.Type == WBSPredecessorType.FinishToStart)
                                    predecessor.Type = PredecessorLinkType.FinishToStart;
                                if (wbsPredecessor.Type == WBSPredecessorType.StartToStart)
                                    predecessor.Type = PredecessorLinkType.StartToStart;
                                Attivita.DependencyLinks.Add(predecessor);
                            }
                        }
                    }
                    ScollegaPredecessori(Guids, Attivita.Children);
                }
            }
        }

        public void UpdateSALOnGantt()
        {
            if (ShowSALTglBtn_Checked)
            {
                AddSALStripLine();
            }
        }

        private void AddSALStripLine()
        {
            GanttData ganttData = DataService.GetGanttData();
            if (ganttData?.ProgrammazioneSAL != null)
            {
                if (ganttData.ProgrammazioneSAL?.PuntiNotevoliPerData != null)
                {
                    Dictionary<DateTime, System.Windows.Media.SolidColorBrush> Dates = new Dictionary<DateTime, System.Windows.Media.SolidColorBrush>();
                    foreach (var item in ganttData.ProgrammazioneSAL.PuntiNotevoliPerData)
                    {
                        if (item.IsSAL)
                            Dates.Add(item.Data, System.Windows.Media.Brushes.Thistle);
                    }
                    GeneraStripLines(Dates);
                }
            }
        }

        public void OnWBSItemsCreated()
        {
            GeneraGantt();
            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());
        }

        public void OnWBSItemsUpdated()
        {
            Guid GuidFuoco = WBSView.GetSelectedItem();
            bool PreviousIsBarreDiRiepilogoChecked = IsBarreDiRiepilogoChecked;
            Init(true);
            IsBarreDiRiepilogoChecked = PreviousIsBarreDiRiepilogoChecked;
            WBSView.OnCurrentTaskChanged(GuidFuoco);
            WBSView.OnTasksSelected(PreviousSelected.ToList());
            OnCheckedItemsChanged(PreviousSelected);
            PreviousSelected.Clear();
            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());
        }

        public DateTime GetDataInizioGantt()
        {
            if (GanttData == null)
                return DateTime.MinValue;

            return GanttData.DataInizio;
        }

        public bool GetIsActiveCriticalPath()
        {
            return IsActiveCriticalPath;
        }

        public bool GetUseDefaultCalendar()
        {
            if (DateProgettoView == null)
                return false;

            return DateProgettoView.UseDefaultCalendar;
        }

        public void AggiornaDataInizioLavori()
        {
            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupBegin(UndoGroupsName.AggiornaDataInizioLavori, BuiltInCodes.EntityType.WBS);

            GanttData.DataInizio = new DateTime(DateProgettoView.DataInizioGantt.Year, DateProgettoView.DataInizioGantt.Month, DateProgettoView.DataInizioGantt.Day);
            GanttData.Offset = DateProgettoView.Offset;
            if (!DateProgettoView.UseDefaultCalendar)
                IsNascondiDateVisible = System.Windows.Visibility.Collapsed;
            else
                IsNascondiDateVisible = System.Windows.Visibility.Visible;
            DataService.SetGanttData(GanttData);
            WBSView.OnGestioneDateWndOk(DateProgettoView.DataInizioPrecedente, DateProgettoView.DataInizioGantt, DateProgettoView.Offset);
            GeneraScalaNumerica();
            GeneraGantt();
            UpdateNewDataInizio();
            AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());

            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupEnd();
        }

        private void VerifyUpdateDataInizioGantt(DateTime DataInizio)
        {
            //GESTIONE SPOSTAMENTO DATA INZIO PROGETTO
            if (GanttData.DataInizio > DataInizio)
            {
                GanttData.DataInizio = new DateTime(DataInizio.Year, DataInizio.Month, DataInizio.Day);
                DataService.SetGanttData(GanttData);
                GeneraScalaNumerica();
                UpdateNewDataInizio();
                GeneraGantt();
                AggiornaAperturaChusuraRami(WBSView.GetFilteredEntitiesViewInfo());
            }
        }

        public void UpdateNewDataInizio()
        {
            GeneraStripLines(new Dictionary<DateTime, System.Windows.Media.SolidColorBrush>());
        }

        public void UpdateCalendarioDefaultInProgrammazioneSAL()
        {
            //if (ProgrammazioneSALView != null)
            //{
            //    if (MinDateTime.Year == 2394)
            //        ProgrammazioneSALView.DataInzioGantt = DateProgettoView.DataInizioGantt;
            //    else
            //        ProgrammazioneSALView.DataInzioGantt = MinDateTime;
            //    if (Tasks.Count() > 0)
            //        ProgrammazioneSALView.DataFineGantt = MaxDateTime;
            //    else
            //    {
            //        if (MinDateTime.Year == 2394)
            //        {
            //            ProgrammazioneSALView.DataFineGantt = DateProgettoView.DataInizioGantt;
            //        }
            //        else
            //        {
            //            ProgrammazioneSALView.DataFineGantt = MinDateTime;
            //        }
            //    }
                ProgrammazioneSALView.CalendarioDefault = WBSView.GetCalendarioDefault();
            //}
        }
    }
    public class WBSVicibleEventArgs : EventArgs
    {
        public List<Guid> IndexWBSToExpande { get; set; }
        public List<Guid> IndexWBSToCollapse { get; set; }
    }

    public class DateEventArgs : EventArgs
    {
        public DateTime? Data { get; set; }
    }


}
