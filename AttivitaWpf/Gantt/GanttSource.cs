using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.Mvvm.Gantt;
using DevExpress.Xpf.Gantt;
using Model;
using Syncfusion.Windows.Controls.Gantt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GanttView = AttivitaWpf.View.GanttView;

namespace AttivitaWpf
{
    public class GanttSource : NotificationBase
    {
        public GanttData GanttData { get; set; }
        public DateProgettoView DateProgettoView { get; set; }
        public ScalaCronologicaView ScalaCronologicaView { get; set; }
        public ScalaCronologicaView ScalaCronologicaViewLocal { get; set; }
        public GanttChartStyleSettingView GanttChartStyleSettingView { get; set; }
        public ProgrammazioneSALView ProgrammazioneSALView { get; set; }
        public ObservableCollection<TaskDetails> TasksSync { get; set; }
        public static Dictionary<DateTime, ScalaNumerica> ScalaNumericaFeriale { get; set; }
        public static Dictionary<DateTime, ScalaNumerica> ScalaNumericaAnonima { get; set; }
        public GanttHolidayCollection SyncHolidays { get; set; }
        public Days SyncWeekends { get; set; }

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

        private ObservableCollection<Processo> _SelectedItems;
        public ObservableCollection<Processo> SelectedItems
        {
            get
            {

                return _SelectedItems;
            }
            set
            {
                if (SetProperty(ref _SelectedItems, value))
                    _SelectedItems = value;
            }
        }

        public HashSet<Guid> PreviousSelected { get; set; }

        private bool _IsBarreDiRiepilogoChecked;
        public bool IsBarreDiRiepilogoChecked
        {
            get
            {
                return _IsBarreDiRiepilogoChecked;
            }
            set
            {
                if (SetProperty(ref _IsBarreDiRiepilogoChecked, value))
                {
                    _IsBarreDiRiepilogoChecked = value;
                    if (value)
                        HeaderNodeVisibility = System.Windows.Visibility.Visible;
                    else
                        HeaderNodeVisibility = System.Windows.Visibility.Collapsed;
                }

            }
        }

        private System.Windows.Visibility _HeaderNodeVisibility;
        public System.Windows.Visibility HeaderNodeVisibility
        {
            get
            {
                return _HeaderNodeVisibility;
            }
            set
            {
                if (SetProperty(ref _HeaderNodeVisibility, value))
                    _HeaderNodeVisibility = value;
            }
        }

        private string _DateIcon;
        public string DateIcon
        {
            get
            {
                return _DateIcon;
            }
            set
            {
                if (SetProperty(ref _DateIcon, value))
                    _DateIcon = value;
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

        private System.Windows.Media.SolidColorBrush _TaskNodeBackground;
        public System.Windows.Media.SolidColorBrush TaskNodeBackground
        {
            get
            {
                return _TaskNodeBackground;
            }
            set
            {
                if (SetProperty(ref _TaskNodeBackground, value))
                    _TaskNodeBackground = value;
            }
        }

        private System.Windows.Media.SolidColorBrush _HeaderTaskNodeBackground;
        public System.Windows.Media.SolidColorBrush HeaderTaskNodeBackground
        {
            get
            {
                return _HeaderTaskNodeBackground;
            }
            set
            {
                if (SetProperty(ref _HeaderTaskNodeBackground, value))
                    _HeaderTaskNodeBackground = value;
            }
        }

        private System.Windows.Media.SolidColorBrush _ConnectorStrokeBackground;
        public System.Windows.Media.SolidColorBrush ConnectorStrokeBackground
        {
            get
            {
                return _ConnectorStrokeBackground;
            }
            set
            {
                if (SetProperty(ref _ConnectorStrokeBackground, value))
                    _ConnectorStrokeBackground = value;
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

        private ObservableCollection<WorkingTimeRule> _WorkingTimeRulesSource;
        public ObservableCollection<WorkingTimeRule> WorkingTimeRulesSource
        {
            get
            {

                return _WorkingTimeRulesSource;
            }
            set
            {
                if (SetProperty(ref _WorkingTimeRulesSource, value))
                    _WorkingTimeRulesSource = value;
            }
        }

        private ObservableCollection<StripLineDataItem> _StripLines;
        public ObservableCollection<StripLineDataItem> StripLines
        {
            get
            {

                return _StripLines;
            }
            set
            {
                if (SetProperty(ref _StripLines, value))
                    _StripLines = value;
            }
        }

        private System.Windows.Visibility _IsNascondiDateVisible;
        public System.Windows.Visibility IsNascondiDateVisible
        {
            get
            {
                return _IsNascondiDateVisible;
            }
            set
            {
                if (SetProperty(ref _IsNascondiDateVisible, value))
                    _IsNascondiDateVisible = value;
            }
        }

        public string TimeScaleDescription
        {
            get
            {
                string Description = LocalizationProvider.GetString("Scala") + ": " + ScalaCronologicaView?.TabItemViews.LastOrDefault()?.SelectedUnita.Value;
                return Description;
            }
        }
        
        private int _WidthCode;
        public int WidthCode
        {
            get
            {
                return _WidthCode;
            }
            set
            {
                SetProperty(ref _WidthCode, value);
            }
        }

        private int _WidthDescription;
        public int WidthDescription
        {
            get
            {
                return _WidthDescription;
            }
            set
            {
                SetProperty(ref _WidthDescription, value);
            }
        }

        private int _WidthDurataCalendario;
        public int WidthDurataCalendario
        {
            get
            {
                return _WidthDurataCalendario;
            }
            set
            {
                SetProperty(ref _WidthDurataCalendario, value);
            }
        }

        private int _WidthDurata;
        public int WidthDurata
        {
            get
            {
                return _WidthDurata;
            }
            set
            {
                SetProperty(ref _WidthDurata, value);
            }
        }

        private int _WidthStartDate;
        public int WidthStartDate
        {
            get
            {
                return _WidthStartDate;
            }
            set
            {
                SetProperty(ref _WidthStartDate, value);
            }
        }

        private int _WidthFinishDate;
        public int WidthFinishDate
        {
            get
            {
                return _WidthFinishDate;
            }
            set
            {
                SetProperty(ref _WidthFinishDate, value);
            }
        }
        private DateTime _Data;
        public DateTime Data
        {
            get
            {

                return _Data;
            }
            set
            {
                SetProperty(ref _Data, value);
            }
        }

        private int _MaximunSlider3DModelValue;
        public int MaximunSlider3DModelValue
        {
            get
            {

                return _MaximunSlider3DModelValue;
            }
            set
            {
                if (SetProperty(ref _MaximunSlider3DModelValue, value))
                    _MaximunSlider3DModelValue = value;
            }
        }

        private double? _TrackBarValue;
        public double? TrackBarValue
        {
            get
            {

                return _TrackBarValue;
            }
            set
            {
                SetProperty(ref _TrackBarValue, value);
            }
        }
        private string _PlayPauseButtonFont;
        public string PlayPauseButtonFont
        {
            get
            {

                return _PlayPauseButtonFont;
            }
            set
            {
                SetProperty(ref _PlayPauseButtonFont, value);
            }
        }

        public Dictionary<DateTime, int> ScalaPuntiNotevoliDate { get; set; }   
    }

    public class StripLineDataItem : NotificationBase
    {
        private DateTime _StartDateTime;
        public DateTime StartDateTime
        {
            get
            {

                return _StartDateTime;
            }
            set
            {
                if (SetProperty(ref _StartDateTime, value))
                    _StartDateTime = value;
            }
        }
        private TimeSpan _StripLineDuration;
        public TimeSpan StripLineDuration
        {
            get
            {

                return _StripLineDuration;
            }
            set
            {
                if (SetProperty(ref _StripLineDuration, value))
                    _StripLineDuration = value;
            }
        }
        private System.Windows.Media.Brush _Background;
        public System.Windows.Media.Brush Background
        {
            get
            {

                return _Background;
            }
            set
            {
                if (SetProperty(ref _Background, value))
                    _Background = value;
            }
        }
    }

    public class Processo : NotificationBase
    {
        public IProcesso Iprocesso { get; set; }
        public Processo(IProcesso iprocesso)
        {
            Iprocesso = iprocesso;
            Children = new ObservableCollection<Processo>();
            DependencyLinks = new ObservableCollection<Predecessore>();
        }
        private Guid _Id;
        public Guid Id
        {
            get
            {

                return _Id;
            }
            set
            {
                if (SetProperty(ref _Id, value))
                    _Id = value;
            }
        }

        private string _Code;
        public string Code
        {
            get
            {

                return _Code;
            }
            set
            {
                if (SetProperty(ref _Code, value))
                    _Code = value;
            }
        }

        private string _Description;
        public string Description
        {
            get
            {

                return _Description;
            }
            set
            {
                if (SetProperty(ref _Description, value))
                    _Description = value;
            }
        }

        private string _Name;
        public string Name
        {
            get
            {

                return _Name;
            }
            set
            {
                if (SetProperty(ref _Name, value))
                    _Name = value;
            }
        }

        private DateTime _StartDate;
        public DateTime StartDate
        {
            get
            {

                return _StartDate;
            }
            set
            {
                if (SetProperty(ref _StartDate, value))
                    _StartDate = value;
            }
        }

        private DateTime _FinishDate;
        public DateTime FinishDate
        {
            get
            {

                return _FinishDate;
            }
            set
            {
                if (SetProperty(ref _FinishDate, value))
                    _FinishDate = value;
            }
        }

        private double _Progress;
        public double Progress
        {
            get
            {

                return _Progress;
            }
            set
            {
                if (SetProperty(ref _Progress, value))
                    _Progress = value;
            }
        }

        
        private double _Lavoro;
        public double Lavoro { get => _Lavoro; set => SetProperty(ref _Lavoro, value); }


        private double _Durata;
        public double Durata
        {
            get
            {

                return _Durata;
            }
            set
            {
                if (SetProperty(ref _Durata, value))
                    _Durata = value;
            }
        }

        private double _DurataCalendario;
        public double DurataCalendario
        {
            get
            {

                return _DurataCalendario;
            }
            set
            {
                if (SetProperty(ref _DurataCalendario, value))
                    _DurataCalendario = value;
            }
        }
        //private DateTime _BaselineStartDate;
        //public DateTime BaselineStartDate
        //{
        //    get
        //    {

        //        return _BaselineStartDate;
        //    }
        //    set
        //    {
        //        if (SetProperty(ref _BaselineStartDate, value))
        //            _BaselineStartDate = value;
        //    }
        //}

        //private DateTime _BaselineFinishDate;
        //public DateTime BaselineFinishDate
        //{
        //    get
        //    {

        //        return _BaselineFinishDate;
        //    }
        //    set
        //    {
        //        if (SetProperty(ref _BaselineFinishDate, value))
        //            _BaselineFinishDate = value;
        //    }
        //}

        private ObservableCollection<Processo> _Children;
        public ObservableCollection<Processo> Children
        {
            get
            {

                return _Children;
            }
            set
            {
                if (SetProperty(ref _Children, value))
                    _Children = value;
            }
        }

        private ObservableCollection<Predecessore> _DependencyLinks;
        public ObservableCollection<Predecessore> DependencyLinks
        {
            get
            {

                return _DependencyLinks;
            }
            set
            {
                if (SetProperty(ref _DependencyLinks, value))
                    _DependencyLinks = value;
            }
        }

        public System.Windows.Media.Brush TaskColor
        {
            get
            {
                var color = ((GanttView)Iprocesso).GetColorNode(Id);
                return color;
            }
        }

        

        public void UpdateUI()
        {
            //RaisePropertyChanged(GetPropertyName(() => Children));
            RaisePropertyChanged(GetPropertyName(() => TaskColor));
        }
    }

    public class ProcessoSync : NotificationBase
    {
        public IProcesso Iprocessi { get; set; }
        public bool IsFinishDateEidatble { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public TimeSpan Duration { get; set; }
        public double Cost { get; set; }
        public DateTime BaselineStart { get; set; }
        public DateTime BaselineFinish { get; set; }
        public double BaselineCost { get; set; }
        public ObservableCollection<Syncfusion.Windows.Controls.Gantt.Predecessor> Predecessor { get; set; }
        public ObservableCollection<Resource> Resources { get; set; }
        public double Progress { get; set; }
        public ObservableCollection<ProcessoSync> Child { get; set; }
        public bool IsMileStone { get; set; }
        public double FixedCost { get; set; }
        public double TotalCost { get; set; }
        public double Baseline { get; set; }
        public double Variance { get; set; }
        public double ActualCost { get; set; }
        public double RemainingCost { get; set; }
        public bool IsSummaryRow { get; set; }
    }
    public interface IProcesso
    {

    }

    public class Predecessore
    {
        // Specifies the predecessor task's Id
        public Guid PredecessorId { get; set; }
        // Specifies the task dependency type (FinishToStart, FinishToFinish, etc.)
        public PredecessorLinkType Type { get; set; }
    }

    public class AssociazioneScale
    {
        public Dictionary<DateTime, DateTime> DaRealeAAnonima;
        public Dictionary<DateTime, DateTime> DaAnonimaAReale;
    }

    public class Task3DModelProperty
    {
        public Guid Id { get; set; }
        public bool IsVisible { get; set; }
        public bool IsTrasparent { get; set; }
    }
}
