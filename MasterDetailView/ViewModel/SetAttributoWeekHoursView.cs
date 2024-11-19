using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using PH.WorkingDaysAndTimeUtility.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class SetAttributoWeekHoursView : NotificationBase
    {
        public List<WeekDay> AttributoWeekHoursData { get; set; }
        private ObservableCollection<WeekHoursLocal> _WeekHoursItems;
        public ObservableCollection<WeekHoursLocal> WeekHoursItems
        {
            get
            {
                return _WeekHoursItems;
            }
            set
            {
                if (SetProperty(ref _WeekHoursItems, value))
                {
                    _WeekHoursItems = value;
                }
            }
        }

        private string _TotaleOrario;
        public string TotaleOrario
        {
            get
            {
                return _TotaleOrario;
            }
            set
            {
                if (SetProperty(ref _TotaleOrario, value))
                {
                    _TotaleOrario = value;
                }
            }
        }
        public IDataService DataService { get; set; } = null;

        public string DefaultOrario = "8:30-12:30;14:00-18:00";

        public void Load()
        {
            WeekHoursItems = new ObservableCollection<WeekHoursLocal>();

            if (AttributoWeekHoursData == null)
            {
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Lunedi, OrarioLavoro = DefaultOrario });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Martedi, OrarioLavoro = DefaultOrario });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Mercoledi, OrarioLavoro = DefaultOrario });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Giovedi, OrarioLavoro = DefaultOrario });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Venerdi, OrarioLavoro = DefaultOrario });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Sabato });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Domenica });
            }
            else
            {
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Lunedi, OrarioLavoro = AttributoWeekHoursData.ElementAt(0).Hours });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Martedi, OrarioLavoro = AttributoWeekHoursData.ElementAt(1).Hours });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Mercoledi, OrarioLavoro = AttributoWeekHoursData.ElementAt(2).Hours });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Giovedi, OrarioLavoro = AttributoWeekHoursData.ElementAt(3).Hours });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Venerdi, OrarioLavoro = AttributoWeekHoursData.ElementAt(4).Hours });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Sabato, OrarioLavoro = AttributoWeekHoursData.ElementAt(5).Hours });
                WeekHoursItems.Add(new WeekHoursLocal() { Giorno = GiorniSettimana.Domenica, OrarioLavoro = AttributoWeekHoursData.ElementAt(6).Hours });
            }

            foreach (WeekHoursLocal WeekHoursItem in WeekHoursItems)
            {
                WeekHoursItem.SommmtoriaOreHandler += WeekHoursItem_SommmtoriaOreHandler;
            }

            TotaleOrario = WeekHoursItems.Sum(d => d.Ore).ToString(NumericFormatHelper.DefaultFormat);
            AttributoWeekHoursData = new List<WeekDay>();
        }

        private void WeekHoursItem_SommmtoriaOreHandler(object sender, EventArgs e)
        {
            TotaleOrario = WeekHoursItems.Sum(d => d.Ore).ToString(NumericFormatHelper.DefaultFormat);
        }

        public bool Accept()
        {
            foreach (var WeekHoursItem in WeekHoursItems)
            {
                WeekDay attributoWeekHoursData = new WeekDay();
                if (WeekHoursItem.Giorno == GiorniSettimana.Lunedi)
                    attributoWeekHoursData.Id = DayOfWeek.Monday;
                if (WeekHoursItem.Giorno == GiorniSettimana.Martedi)
                    attributoWeekHoursData.Id = DayOfWeek.Tuesday;
                if (WeekHoursItem.Giorno == GiorniSettimana.Mercoledi)
                    attributoWeekHoursData.Id = DayOfWeek.Wednesday;
                if (WeekHoursItem.Giorno == GiorniSettimana.Giovedi)
                    attributoWeekHoursData.Id = DayOfWeek.Thursday;
                if (WeekHoursItem.Giorno == GiorniSettimana.Venerdi)
                    attributoWeekHoursData.Id = DayOfWeek.Friday;
                if (WeekHoursItem.Giorno == GiorniSettimana.Sabato)
                    attributoWeekHoursData.Id = DayOfWeek.Saturday;
                if (WeekHoursItem.Giorno == GiorniSettimana.Domenica)
                    attributoWeekHoursData.Id = DayOfWeek.Sunday;
                attributoWeekHoursData.Hours = WeekHoursItem.OrarioLavoro;
                AttributoWeekHoursData.Add(attributoWeekHoursData);
            }
            return true;
        }
    }
    public class WeekHoursLocal : NotificationBase
    {
        public event EventHandler SommmtoriaOreHandler;
        private string _Giorno;
        public string Giorno
        {
            get
            {
                return _Giorno;
            }
            set
            {
                if (SetProperty(ref _Giorno, value))
                {
                    _Giorno = value;
                }
            }
        }
        private string _OrarioLavor;
        public string OrarioLavoro
        {
            get
            {
                return _OrarioLavor;
            }
            set
            {
                if (SetProperty(ref _OrarioLavor, value))
                {
                    _OrarioLavor = value;
                    RaisePropertyChanged(GetPropertyName(() => Ore));
                    SommmtoriaOreHandler?.Invoke(this, new EventArgs());
                }
            }
        }

        public double Ore
        {
            get
            {
                return CalcolaOrario();
            }
        }

        private double CalcolaOrario()
        {
            if (WorkDaySpan.CheckTimeSpans(OrarioLavoro))
            {
                WorkDaySpan workday = new WorkDaySpan(OrarioLavoro);
                double OreLavorate = workday.WorkingMinutesPerDay / 60;
                return OreLavorate;
            }
            else
                return 0;
        }
    }
    public class GiorniSettimana
    {
        public static string Lunedi { get { return LocalizationProvider.GetString("Lunedi"); } }
        public static string Martedi { get { return LocalizationProvider.GetString("Martedi"); } }
        public static string Mercoledi { get { return LocalizationProvider.GetString("Mercoledi"); } }
        public static string Giovedi { get { return LocalizationProvider.GetString("Giovedi"); } }
        public static string Venerdi { get { return LocalizationProvider.GetString("Venerdi"); } }
        public static string Sabato { get { return LocalizationProvider.GetString("Sabato"); } }
        public static string Domenica { get { return LocalizationProvider.GetString("Domenica"); } }
    }
}
