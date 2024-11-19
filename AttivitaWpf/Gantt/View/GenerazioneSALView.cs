using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class GenerazioneSALView : NotificationBase
    {
        public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public IEntityWindowService WindowService { get; set; }

        private Dictionary<int, string> _ListaTipologiaProgrammazione;
        public Dictionary<int, string> ListaTipologiaProgrammazione
        {
            get
            {

                return _ListaTipologiaProgrammazione;
            }
            set
            {
                SetProperty(ref _ListaTipologiaProgrammazione, value);
            }
        }

        private KeyValuePair<int, string> _TipologiaProgrammazione;
        public KeyValuePair<int, string> TipologiaProgrammazione
        {
            get
            {

                return _TipologiaProgrammazione;
            }
            set
            {
                SetProperty(ref _TipologiaProgrammazione, value);
                if (_TipologiaProgrammazione.Key == 0 && CreateDataColumn)
                {
                    IsFrequencySALVisible = System.Windows.Visibility.Visible;
                    IsDataSALVisible = System.Windows.Visibility.Visible;
                    IsRipetiOgniVisible = System.Windows.Visibility.Visible;
                    IsNumberValueFrequencyVisible = System.Windows.Visibility.Visible;
                    IsDoNotuseCalendarVisible = System.Windows.Visibility.Visible;
                    IsAttributoAmountPercSALVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoAmountSALVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoSALVisible = System.Windows.Visibility.Collapsed;
                }

                if (_TipologiaProgrammazione.Key == 0 && !CreateDataColumn)
                {
                    IsAttributoAmountSALVisible = System.Windows.Visibility.Visible;
                    IsRipetiOgniVisible = System.Windows.Visibility.Visible;
                    IsNumberValueFrequencyVisible = System.Windows.Visibility.Collapsed;
                    IsFrequencySALVisible = System.Windows.Visibility.Collapsed;
                    IsDataSALVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoAmountPercSALVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoSALVisible = System.Windows.Visibility.Collapsed;
                    IsDoNotuseCalendarVisible = System.Windows.Visibility.Collapsed;
                }

                if (_TipologiaProgrammazione.Key == 1)
                {
                    IsAttributoSALVisible = System.Windows.Visibility.Visible;
                    IsAttributoAmountSALVisible = System.Windows.Visibility.Visible;
                    IsRipetiOgniVisible = System.Windows.Visibility.Visible;
                    IsNumberValueFrequencyVisible = System.Windows.Visibility.Collapsed;
                    IsFrequencySALVisible = System.Windows.Visibility.Collapsed;
                    IsDataSALVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoAmountPercSALVisible = System.Windows.Visibility.Collapsed;
                    IsDoNotuseCalendarVisible = System.Windows.Visibility.Collapsed;
                }
                if (_TipologiaProgrammazione.Key == 2)
                {
                    IsAttributoAmountPercSALVisible = System.Windows.Visibility.Visible;
                    IsAttributoSALVisible = System.Windows.Visibility.Visible;
                    IsNumberValueFrequencyVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoAmountSALVisible = System.Windows.Visibility.Collapsed;
                    IsFrequencySALVisible = System.Windows.Visibility.Collapsed;
                    IsDataSALVisible = System.Windows.Visibility.Collapsed;
                    IsRipetiOgniVisible = System.Windows.Visibility.Collapsed;
                    IsDoNotuseCalendarVisible = System.Windows.Visibility.Collapsed;
                }
                if (_TipologiaProgrammazione.Key == 3)
                {
                    IsAttributoAmountPercSALVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoSALVisible = System.Windows.Visibility.Collapsed;
                    IsNumberValueFrequencyVisible = System.Windows.Visibility.Collapsed;
                    IsAttributoAmountSALVisible = System.Windows.Visibility.Collapsed;
                    IsFrequencySALVisible = System.Windows.Visibility.Collapsed;
                    IsDataSALVisible = System.Windows.Visibility.Collapsed;
                    IsRipetiOgniVisible = System.Windows.Visibility.Collapsed;
                    IsDoNotuseCalendarVisible = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private ObservableCollection<AttibutiFogiloDiCalcoloView> _ListaAttributo;

        public ObservableCollection<AttibutiFogiloDiCalcoloView> ListaAttributo
        {
            get
            {

                return _ListaAttributo;
            }
            set
            {
                SetProperty(ref _ListaAttributo, value);
            }
        }

        private object _Attributo;

        public object Attributo
        {
            get
            {

                return _Attributo;
            }
            set
            {
                SetProperty(ref _Attributo, value);
            }
        }

        private double _NumberValueFrequency;

        public double NumberValueFrequency
        {
            get
            {

                return _NumberValueFrequency;
            }
            set
            {
                SetProperty(ref _NumberValueFrequency, value);
            }
        }

        private Dictionary<int, string> _ListTimeInterval;

        public Dictionary<int, string> ListTimeInterval
        {
            get
            {

                return _ListTimeInterval;
            }
            set
            {
                SetProperty(ref _ListTimeInterval, value);
            }
        }

        private KeyValuePair<int, string> _TimeInterval;

        public KeyValuePair<int, string> TimeInterval
        {
            get
            {

                return _TimeInterval;
            }
            set
            {
                SetProperty(ref _TimeInterval, value);
            }
        }

        private double _AmountValueFrequency;

        public double AmountValueFrequency
        {
            get
            {

                return _AmountValueFrequency;
            }
            set
            {
                SetProperty(ref _AmountValueFrequency, value);
            }
        }

        private ObservableCollection<Percentuale> _ListPercent;

        public ObservableCollection<Percentuale> ListPercent
        {
            get
            {

                return _ListPercent;
            }
            set
            {
                SetProperty(ref _ListPercent, value);
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
                //value = ProgrammazioneSALCalculator.ReturnLastDataAvailable(value,!DoNotuseCalendar);
                SetProperty(ref _Data, value);
            }
        }

        private System.Windows.Visibility _IsAttributoSALVisible;
        public System.Windows.Visibility IsAttributoSALVisible
        {
            get
            {

                return _IsAttributoSALVisible;
            }
            set
            {
                SetProperty(ref _IsAttributoSALVisible, value);
            }
        }

        private System.Windows.Visibility _IsFrequencySALVisible;
        public System.Windows.Visibility IsFrequencySALVisible
        {
            get
            {

                return _IsFrequencySALVisible;
            }
            set
            {
                SetProperty(ref _IsFrequencySALVisible, value);
            }
        }

        private System.Windows.Visibility _IsDataSALVisible;
        public System.Windows.Visibility IsDataSALVisible
        {
            get
            {

                return _IsDataSALVisible;
            }
            set
            {
                SetProperty(ref _IsDataSALVisible, value);
            }
        }

        private System.Windows.Visibility _IsAttributoAmountSALVisible;
        public System.Windows.Visibility IsAttributoAmountSALVisible
        {
            get
            {

                return _IsAttributoAmountSALVisible;
            }
            set
            {
                SetProperty(ref _IsAttributoAmountSALVisible, value);
            }
        }

        private System.Windows.Visibility _IsAttributoAmountPercSALVisible;
        public System.Windows.Visibility IsAttributoAmountPercSALVisible
        {
            get
            {

                return _IsAttributoAmountPercSALVisible;
            }
            set
            {
                SetProperty(ref _IsAttributoAmountPercSALVisible, value);
            }
        }

        private System.Windows.Visibility _IsRipetiOgniVisible;
        public System.Windows.Visibility IsRipetiOgniVisible
        {
            get
            {

                return _IsRipetiOgniVisible;
            }
            set
            {
                SetProperty(ref _IsRipetiOgniVisible, value);
            }
        }

        private System.Windows.Visibility _IsNumberValueFrequencyVisible;
        public System.Windows.Visibility IsNumberValueFrequencyVisible
        {
            get
            {

                return _IsNumberValueFrequencyVisible;
            }
            set
            {
                SetProperty(ref _IsNumberValueFrequencyVisible, value);
            }
        }

        private bool _DoNotuseCalendar;
        public bool DoNotuseCalendar
        {
            get
            {

                return _DoNotuseCalendar;
            }
            set
            {
                SetProperty(ref _DoNotuseCalendar, value);
            }
        }

        private System.Windows.Visibility _IsDoNotuseCalendarVisible
;
        public System.Windows.Visibility IsDoNotuseCalendarVisible

        {
            get
            {

                return _IsDoNotuseCalendarVisible;
            }
            set
            {
                SetProperty(ref _IsDoNotuseCalendarVisible, value);
            }
        }

        private bool _IsCheckedOnlyOneTime;
        public bool IsCheckedOnlyOneTime
        {
            get
            {

                return _IsCheckedOnlyOneTime;
            }
            set
            {
                SetProperty(ref _IsCheckedOnlyOneTime, value);
            }
        }

        private List<AttributoFoglioDiCalcolo> AttributiUtilizzati;
        public ProgrammazioneSALCalculator ProgrammazioneSALCalculator { get; set; }
        public List<SALProgrammatoView> salProgrammatoViewResult { get; set; }
        public bool CreateDataColumn { get; set; }

        private DateTime DataInizioGantt;
        private DateTime DataFineGantt;
        public GenerazioneSALView(ProgrammazioneSALCalculator programmazioneSALCalculator, bool createDataColumn)
        {
            ProgrammazioneSALCalculator = programmazioneSALCalculator;
            CreateDataColumn = createDataColumn;

            ListaTipologiaProgrammazione = new Dictionary<int, string>();
            ListaAttributo = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
            ListTimeInterval = new Dictionary<int, string>();
            ListPercent = new ObservableCollection<Percentuale>();

            ListaTipologiaProgrammazione.Add(0, LocalizationProvider.GetString("date"));
            ListaTipologiaProgrammazione.Add(1, LocalizationProvider.GetString("valori"));
            ListaTipologiaProgrammazione.Add(2, LocalizationProvider.GetString("percvalori"));
            ListaTipologiaProgrammazione.Add(3, LocalizationProvider.GetString("produttivita media costante"));
            TipologiaProgrammazione = ListaTipologiaProgrammazione.FirstOrDefault();

            ListTimeInterval.Add(0, LocalizationProvider.GetString("Giorno"));
            ListTimeInterval.Add(1, LocalizationProvider.GetString("Settimana"));
            ListTimeInterval.Add(2, LocalizationProvider.GetString("Mese"));
            ListTimeInterval.Add(4, LocalizationProvider.GetString("FineMese"));
            ListTimeInterval.Add(3, LocalizationProvider.GetString("Anno"));
            TimeInterval = ListTimeInterval.FirstOrDefault();

            ListPercent.Add(new Percentuale() { Value = 30.00 });
            ListPercent.Add(new Percentuale() { Value = 70.00 });
            ListPercent.Add(new Percentuale() { Value = 100.00 });

            NumberValueFrequency = 0;
        }

        public void Init(DateTime dataInzioGantt, DateTime dataFineGantt, List<AttributoFoglioDiCalcolo> attributiUtilizzati, IDataService dataService, IEntityWindowService windowService)
        {
            DataInizioGantt = dataInzioGantt;
            DataFineGantt = dataFineGantt;
            Data = DataInizioGantt;

            AttributiUtilizzati = attributiUtilizzati;
            foreach (AttributoFoglioDiCalcolo attr in AttributiUtilizzati)
            {
                if (attr.Amount || attr.ProductivityPerHour || attr.ProgressiveAmount)
                {
                    AttibutiFogiloDiCalcoloView attibutiFogiloDiCalcoloView = new AttibutiFogiloDiCalcoloView();
                    attibutiFogiloDiCalcoloView.Amount = attr.Amount;
                    attibutiFogiloDiCalcoloView.ProductivityPerHour = attr.ProductivityPerHour;
                    attibutiFogiloDiCalcoloView.ProgressiveAmount = attr.ProgressiveAmount;
                    attibutiFogiloDiCalcoloView.Etichetta = attr.Etichetta;
                    attibutiFogiloDiCalcoloView.CodiceOrigine = attr.CodiceOrigine;
                    attibutiFogiloDiCalcoloView.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                    if (attr.ProgressiveAmount)
                        ListaAttributo.Add(attibutiFogiloDiCalcoloView);
                    attr.Amount = true;
                    attr.ProductivityPerHour = true;
                    attr.ProgressiveAmount = true;
                }
            }

            if (ListaAttributo.Count() == 1)
            {
                Attributo = ListaAttributo.FirstOrDefault();
            }

            DataService = dataService;
            WindowService = windowService;
        }

        List<SALProgrammatoView> salProgrammatoView = new List<SALProgrammatoView>();
        public void AcceptButton()
        {
            WindowService.ShowWaitCursor(true);
            bool UseCalendar = !DoNotuseCalendar;
            DateTime CalendarDate = new DateTime();
            if (UseCalendar)
                CalendarDate = ProgrammazioneSALCalculator.ReturnLastDataAvailable(Data, !DoNotuseCalendar);
            else
                CalendarDate = new DateTime(Data.Year, Data.Month, Data.Day, 0, 0, 0);

            salProgrammatoView = new List<SALProgrammatoView>();
            
            if (TipologiaProgrammazione.Key == 0 && CreateDataColumn)
            {
                if (NumberValueFrequency == 0)
                {
                    if (CalendarDate < DataFineGantt && CalendarDate > DataInizioGantt)
                        salProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, CalendarDate));
                }
                else
                {
                    if (CalendarDate == new DateTime())
                    {
                        WindowService.ShowWaitCursor(false);
                        return;
                    }

                    if (TimeInterval.Key == 0)
                    {
                        if (NumberValueFrequency > 1)
                        {
                            //TOLTO UN GIORNO PER CORREGGERE DATA IN MODO CHE SIA COERENTE CON IMPOSTAZIONE GIORNALIERA
                            AddSalProgrammatoViewByDaysRecursive(salProgrammatoView, Data.AddDays(NumberValueFrequency - 1), NumberValueFrequency, UseCalendar);
                            PostCreationSALRecordOperationPerDate(UseCalendar);
                            List<SALProgrammatoView> listToRemove = new List<SALProgrammatoView>();
                            for (int i = 1; i < salProgrammatoView.Count(); i++)
                            {
                                if ( (i + 1) < salProgrammatoView.Count())
                                {
                                    if (Math.Ceiling((salProgrammatoView.ElementAt(i + 1).Data - salProgrammatoView.ElementAt(i).Data).TotalDays) < NumberValueFrequency && salProgrammatoView.ElementAt(i + 1).Data != salProgrammatoView.ElementAt(i).Data)
                                    {
                                        listToRemove.Add(salProgrammatoView.ElementAt(i + 1));
                                    }
                                }
                            }
                            foreach (var item in listToRemove)
                            {
                                salProgrammatoView.Remove(item);
                            }
                        }
                        else
                        {
                            AddSalProgrammatoViewByDaysRecursive(salProgrammatoView, CalendarDate, NumberValueFrequency,UseCalendar);
                            //RIMUOVO L'ULITMA  PERCHé HO GIA LA RIGA FINE GANTT; SOLO QUANDO NON GESTISCO LE ORE MA LA LA FINE GIONATA(ORE 24) 
                            if (!UseCalendar)
                            {
                                if (salProgrammatoView.Where(x => x.Data >  DataFineGantt).FirstOrDefault() != null)
                                {
                                    salProgrammatoView.Remove(salProgrammatoView.Where(x => x.Data > DataFineGantt).FirstOrDefault());
                                }
                            }
                        }
                    }
                    if (TimeInterval.Key == 1)
                    {
                        AddSalProgrammatoViewByWeeksRecursive(salProgrammatoView, CalendarDate, NumberValueFrequency);
                        PostCreationSALRecordOperationPerDate(UseCalendar);
                    }
                    if (TimeInterval.Key == 2)
                    {
                        AddSalProgrammatoViewByMonthsRecursive(salProgrammatoView, Data, NumberValueFrequency, false);
                        PostCreationSALRecordOperationPerDate(UseCalendar);
                    }
                    if (TimeInterval.Key == 3)
                    {
                        AddSalProgrammatoViewByYearsRecursive(salProgrammatoView, Data, NumberValueFrequency);
                        PostCreationSALRecordOperationPerDate(UseCalendar);
                    }
                    if (TimeInterval.Key == 4)
                    {
                        CalendarDate = new DateTime(CalendarDate.Year, CalendarDate.Month, DateTime.DaysInMonth(CalendarDate.Year, CalendarDate.Month), CalendarDate.Hour, CalendarDate.Minute, 0);
                        AddSalProgrammatoViewByMonthsRecursive(salProgrammatoView, CalendarDate, NumberValueFrequency, true);
                    }
                }
            }
            if (TipologiaProgrammazione.Key == 0 && !CreateDataColumn)
            {
                DateTime newDate = new DateTime(CalendarDate.Year, CalendarDate.Month, CalendarDate.Day, 0, 0, 0);
                if (AmountValueFrequency > 1)
                {
                    AddSalProgrammatoViewByDaysRecursive(salProgrammatoView, newDate.AddDays(AmountValueFrequency), AmountValueFrequency, IsCheckedOnlyOneTime, UseCalendar);
                }
                else
                {
                    AddSalProgrammatoViewByDaysRecursive(salProgrammatoView, newDate, AmountValueFrequency, IsCheckedOnlyOneTime, UseCalendar);
                }
                AmountValueFrequency = 0;
            }
            if (TipologiaProgrammazione.Key == 1)
            {
                if (IsCheckedOnlyOneTime && AmountValueFrequency != 0 && Attributo != null)
                {
                    salProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, ((AttibutiFogiloDiCalcoloView)Attributo).CodiceOrigine, AmountValueFrequency, DataInizioGantt, DataFineGantt,UseCalendar));
                }
                else
                {
                    if (Attributo != null)
                    {
                        Double total = ProgrammazioneSALCalculator.GetTotalAttributoValue(((AttibutiFogiloDiCalcoloView)Attributo).CodiceOrigine);
                        int cycleValue = (int)(total / AmountValueFrequency) + 1;
                        for (int i = 1; i < cycleValue; i++)
                        {
                            var salProgrammato = ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, ((AttibutiFogiloDiCalcoloView)Attributo).CodiceOrigine, AmountValueFrequency * i, DataInizioGantt, DataFineGantt, UseCalendar);
                            if (salProgrammato != null)
                                salProgrammatoView.Add(salProgrammato);
                        }
                    }
                }
                IsCheckedOnlyOneTime = false;
            }
            if (TipologiaProgrammazione.Key == 2)
            {
                if (ListPercent.Count() == 1)
                {
                    if (Attributo != null)
                        salProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoViewPerc(AttributiUtilizzati, ((AttibutiFogiloDiCalcoloView)Attributo).CodiceOrigine, ListPercent.FirstOrDefault().Value, DataInizioGantt, DataFineGantt, UseCalendar));
                }
                else
                {
                    if (Attributo != null)
                    {
                        ListPercent = new ObservableCollection<Percentuale>(ListPercent.OrderBy(item => item.Value).ToList());
                        foreach (Percentuale perc in ListPercent)
                        {
                            if (perc.Value == 100)
                                continue;

                            bool existOneNotZero = false;
                            salProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoViewPerc(AttributiUtilizzati, ((AttibutiFogiloDiCalcoloView)Attributo).CodiceOrigine, perc.Value, DataInizioGantt, DataFineGantt, UseCalendar));
                            for (int i = 1; i < SALProgrammatoView.GetTotalColumnForCycle(); i++)
                            {
                                if (salProgrammatoView.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i) != 0)
                                {
                                    existOneNotZero = true;
                                    if (existOneNotZero)
                                        break;
                                }
                            }
                            if (!existOneNotZero)
                            {
                                salProgrammatoView.RemoveAt(salProgrammatoView.Count() - 1);
                            }
                        }
                    }
                }
            }

            if (TipologiaProgrammazione.Key == 3)
            {
                salProgrammatoView.AddRange(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati));
            }

            if (TipologiaProgrammazione.Key == 1 || TipologiaProgrammazione.Key == 2)
            {
                //PORTO ALLA FINE DELLA GIORNATA PRECEDENTE
                foreach (var item in salProgrammatoView)
                {
                    item.Data = ProgrammazioneSALCalculator.ReturnAsEndingDateTime(item.Data, UseCalendar);
                }
            }

            WindowService.ShowWaitCursor(false);
        }

        private void PostCreationSALRecordOperationPerDate(bool useCalendar)
        {
            //RIMUOVO L'ULITMA  PERCHé HO GIA LA RIGA FINE GANTT; SOLO QUANDO NON GESTISCO LE ORE MA LA LA FINE GIONATA(ORE 24) 
            if (!useCalendar)
            {
                if (salProgrammatoView.Where(x => x.Data > DataFineGantt).FirstOrDefault() != null)
                {
                    salProgrammatoView.Remove(salProgrammatoView.Where(x => x.Data > DataFineGantt).FirstOrDefault());
                }
                //TOLGO STESSO GIORNO QUANDO LA FREQUENZA ON E' GIORNALIERA
                if (DataInizioGantt.Day == salProgrammatoView.ElementAt(0).Data.Day - 1 && DataInizioGantt.Month == salProgrammatoView.ElementAt(0).Data.Month && DataInizioGantt.Year == salProgrammatoView.ElementAt(0).Data.Year)
                    salProgrammatoView.RemoveAt(0);
            }
            else
            {
                //TOLGO STESSO GIORNO QUANDO LA FREQUENZA ON E' GIORNALIERA
                if (DataInizioGantt.Day == salProgrammatoView.ElementAt(0).Data.Day && DataInizioGantt.Month == salProgrammatoView.ElementAt(0).Data.Month && DataInizioGantt.Year == salProgrammatoView.ElementAt(0).Data.Year)
                    salProgrammatoView.RemoveAt(0);
            }
        }


        public List<SALProgrammatoView> GetSALResultsByDate(List<DateTime> Dates)
        {
            CalculateSALResultByDate(Dates);
            return salProgrammatoView;
        }

        private void CalculateSALResultByDate(List<DateTime> Dates)
        {
            salProgrammatoView = new List<SALProgrammatoView>();
            foreach (var date in Dates)
            {
                salProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, date));
            }
        }

        public List<SALProgrammatoView> GetSALResults()
        {
            foreach (var item in salProgrammatoView)
            {
                if (TipologiaProgrammazione.Key == 3)
                    item.IsSAL = false;
                else
                    item.IsSAL = true;
            }
            return salProgrammatoView;
        }


        private void AddSalProgrammatoViewByDaysRecursive(List<SALProgrammatoView> listSALProgrammatoView, DateTime date, double frequency, bool useCalendar,bool OnlyOneTime = false)
        {
            if (date < DataFineGantt)
            {
                listSALProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, ProgrammazioneSALCalculator.ReturnLastDataAvailable(date, useCalendar)));
                if (OnlyOneTime)
                    return;
                AddSalProgrammatoViewByDaysRecursive(listSALProgrammatoView, date.AddDays(frequency), frequency,useCalendar, OnlyOneTime);
            }
        }

        private void AddSalProgrammatoViewByWeeksRecursive(List<SALProgrammatoView> listSALProgrammatoView, DateTime date, double frequency)
        {
            if (date < DataFineGantt)
            {
                double newfrequency = frequency * 7;
                listSALProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, ProgrammazioneSALCalculator.ReturnLastDataAvailable(date, !DoNotuseCalendar)));
                AddSalProgrammatoViewByWeeksRecursive(listSALProgrammatoView, date.AddDays(newfrequency), frequency);
            }
        }

        private void AddSalProgrammatoViewByMonthsRecursive(List<SALProgrammatoView> listSALProgrammatoView, DateTime date, double frequency, bool IsFineMese)
        {
            if (date < DataFineGantt)
            {
                listSALProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, ProgrammazioneSALCalculator.ReturnLastDataAvailable(date, !DoNotuseCalendar)));
                DateTime newDate = date.AddMonths((int)frequency);
                if (IsFineMese)
                    newDate = new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month), newDate.Hour, newDate.Minute, 0);
                AddSalProgrammatoViewByMonthsRecursive(listSALProgrammatoView, newDate, frequency, IsFineMese);
            }
        }
        private void AddSalProgrammatoViewByYearsRecursive(List<SALProgrammatoView> listSALProgrammatoView, DateTime date, double frequency)
        {
            if (date < DataFineGantt)
            {
                listSALProgrammatoView.Add(ProgrammazioneSALCalculator.GetSALProgrammatoView(AttributiUtilizzati, ProgrammazioneSALCalculator.ReturnLastDataAvailable(date, !DoNotuseCalendar)));
                AddSalProgrammatoViewByYearsRecursive(listSALProgrammatoView, date.AddYears((int)frequency), frequency);
            }
        }
    }
    public class Percentuale
    {
        public double Value { get; set; }
    }

}
