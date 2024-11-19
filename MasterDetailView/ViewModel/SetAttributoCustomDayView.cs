using Commons;
using Model;
using PH.WorkingDaysAndTimeUtility.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MasterDetailView
{
    public class SetAttributoCustomDayView : NotificationBase
    {
        public event EventHandler ApplicaStile;
        public List<CustomDay> ListaEccezioni;
        private string _OrarioStd;
        public string OrarioStd
        {
            get
            {
                return _OrarioStd;
            }
            set
            {
                if (SetProperty(ref _OrarioStd, value))
                {
                    _OrarioStd = value;
                }
            }
        }
        private string _OrarioCst;
        public string OrarioCst
        {
            get
            {
                return _OrarioCst;
            }
            set
            {
                if (SetProperty(ref _OrarioCst, value))
                {
                    _OrarioCst = value;
                }
            }
        }
        private ObservableCollection<CustomDayLocal> _ListaEccezioniLocale;
        public ObservableCollection<CustomDayLocal> ListaEccezioniLocale
        {
            get
            {
                return _ListaEccezioniLocale;
            }
            set
            {
                if (SetProperty(ref _ListaEccezioniLocale, value))
                {
                    _ListaEccezioniLocale = value;
                }
            }
        }

        private CustomDayLocal _SelectedItem;
        public CustomDayLocal SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                if (SetProperty(ref _SelectedItem, value))
                {
                    _SelectedItem = value;
                }
            }
        }

        private ObservableCollection<object> _SelectedItemsCalendar;
        public ObservableCollection<object> SelectedItemsCalendar
        {
            get
            {
                return _SelectedItemsCalendar;
            }
            set
            {
                if (SetProperty(ref _SelectedItemsCalendar, value))
                {
                    _SelectedItemsCalendar = value;
                }
            }
        }

        private ObservableCollection<object> _SelectedItemsLista;
        public ObservableCollection<object> SelectedItemsLista
        {
            get
            {
                return _SelectedItemsLista;
            }
            set
            {
                if (SetProperty(ref _SelectedItemsLista, value))
                {
                    _SelectedItemsLista = value;
                }
            }
        }

        public List<DateTime> ListaDateSelezionta;
        public List<string> ListaOrariStd;
        public List<string> ListaOrariEccezioni;
        public ClientDataService DataService { get; set; }
        public List<WeekDay> GiorniLavorativi { get; set; }

        //public List<CustomDay> ListaEccezioni { get; set; }
        private bool FirstExecution;

        public SetAttributoCustomDayView()
        {
            //TotaleOrario = null;
            ListaEccezioniLocale = new ObservableCollection<CustomDayLocal>();
            FirstExecution = true;
            SelectedItemsCalendar = new ObservableCollection<object>();
            SelectedItemsLista = new ObservableCollection<object>();
            ListaDateSelezionta = new List<DateTime>();
            ListaOrariStd = new List<string>();
            ListaOrariEccezioni = new List<string>();
        }
        public void Load()
        {
            if (ListaEccezioni != null)
            {
                foreach (CustomDay Eccezione in ListaEccezioni)
                {
                    ListaEccezioniLocale.Add(new CustomDayLocal() { Day = Eccezione.Day, Hours = Eccezione.Hours });
                }
            }
        }

        public bool Accept()
        {
            ListaEccezioni = new List<CustomDay>();

            foreach (CustomDayLocal Eccezione in ListaEccezioniLocale)
            {
                ListaEccezioni.Add(new CustomDay() { Day = Eccezione.Day, Hours = Eccezione.Hours });
            }

            return true;
        }

        public bool IsActiveSelzioneDaLista;
        public bool IsActiveSelzioneDaCalendario;
        //public ICommand SelectionChangedCommand { get { return new CommandHandlerParam((object param) => this.SelectionChanged(param)); } }
        public void SelectionChanged(System.Collections.IList addedItems)
        {
            if (!IsActiveSelzioneDaLista)
            {
                IsActiveSelzioneDaCalendario = true;
                OrarioCst = null;
                if (System.Windows.Input.Keyboard.Modifiers != ModifierKeys.Control)
                {
                    ListaDateSelezionta.Clear();
                    ListaOrariStd.Clear();
                    ListaOrariEccezioni.Clear();
                }
                  
                SelectedItemsLista.Clear();
                foreach (DateTime Data in addedItems)
                {
                    if (Data.Date.DayOfWeek == DayOfWeek.Monday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == 0) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Monday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (Data.Date.DayOfWeek == DayOfWeek.Tuesday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Tuesday) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Tuesday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (Data.Date.DayOfWeek == DayOfWeek.Wednesday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Wednesday) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Wednesday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (Data.Date.DayOfWeek == DayOfWeek.Thursday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Thursday) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Thursday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (Data.Date.DayOfWeek == DayOfWeek.Friday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Friday) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Friday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (Data.Date.DayOfWeek == DayOfWeek.Saturday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Saturday) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Saturday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (Data.Date.DayOfWeek == DayOfWeek.Sunday)
                        if (GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Sunday) != null)
                        {
                            OrarioStd = GiorniLavorativi.FirstOrDefault(d => d.Id == DayOfWeek.Sunday).Hours;
                            ListaOrariStd.Add(OrarioStd);
                        }
                    if (ListaEccezioniLocale.Where(d => d.Day.Day == Data.Day && d.Day.Month == Data.Month && d.Day.Year == Data.Year).FirstOrDefault() != null)
                    {
                        SelectedItem = ListaEccezioniLocale.Where(d => d.Day.Day == Data.Day && d.Day.Month == Data.Month && d.Day.Year == Data.Year).FirstOrDefault();
                        //SelectedItemsLista.Add(ListaEccezioniLocale.Where(d => d.Day.Day == Data.Day && d.Day.Month == Data.Month && d.Day.Year == Data.Year).FirstOrDefault());
                        OrarioCst = SelectedItem.Hours;
                        ListaOrariEccezioni.Add(SelectedItem.Hours);
                    }

                    ListaDateSelezionta.Add(Data);
                }

                if (ListaOrariStd.Any(d => d != ListaOrariStd.FirstOrDefault()))
                {
                    OrarioStd = "[Multi]";
                    string HoursSelezionato = ListaOrariEccezioni.FirstOrDefault();
                    if (ListaOrariEccezioni.Any(d => d != HoursSelezionato))
                    {
                        OrarioCst = null;
                    }
                }

                foreach (DateTime date in ListaDateSelezionta)
                {
                    if (ListaEccezioniLocale.Where(r => r.Day.Year == date.Year && r.Day.Month == date.Month && r.Day.Day == date.Day).FirstOrDefault() != null)
                    {
                        SelectedItemsLista.Add(ListaEccezioniLocale.Where(r => r.Day.Year == date.Year && r.Day.Month == date.Month && r.Day.Day == date.Day).FirstOrDefault());
                    }
                }

                IsActiveSelzioneDaCalendario = false;
            }
        }

        public void RemoveItems(System.Collections.IList removedItems)
        {
            if (!IsActiveSelzioneDaLista)
            {
                if (System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Control)
                {
                    foreach (DateTime Data in removedItems)
                    {
                        if (ListaDateSelezionta.Where(r => r.Year == Data.Year && r.Month == Data.Month && r.Day == Data.Day).FirstOrDefault() != null)
                        {
                            ListaDateSelezionta.Remove(ListaDateSelezionta.Where(r => r.Year == Data.Year && r.Month == Data.Month && r.Day == Data.Day).FirstOrDefault());
                        }

                        if (ListaEccezioniLocale.Where(r => r.Day.Year == Data.Year && r.Day.Month == Data.Month && r.Day.Day == Data.Day).FirstOrDefault() != null)
                        {
                            SelectedItemsLista.Remove(ListaEccezioniLocale.Where(r => r.Day.Year == Data.Year && r.Day.Month == Data.Month && r.Day.Day == Data.Day).FirstOrDefault());
                        }
                    }
                }
            }
        }

        public ICommand AddEccezioneCommand { get { return new CommandHandler(() => this.AddEccezione()); } }

        private bool IsActiveAddEccezione;
        private void AddEccezione()
        {
            SelectedItemsLista.Clear();
            IsActiveAddEccezione = true;
            foreach (DateTime Data in ListaDateSelezionta)
            {
                if (WorkDaySpan.CheckTimeSpans(OrarioCst))
                {
                    if (ListaEccezioniLocale.Where(d => d.Day.Day == Data.Day && d.Day.Month == Data.Month && d.Day.Year == Data.Year).FirstOrDefault() == null)
                        ListaEccezioniLocale.Add(new CustomDayLocal() { Day = Data, Hours = OrarioCst });
                    else
                        ListaEccezioniLocale.Where(d => d.Day.Day == Data.Day && d.Day.Month == Data.Month && d.Day.Year == Data.Year).FirstOrDefault().Hours = OrarioCst;
                }
                SelectedItemsLista.Add(ListaEccezioniLocale.Where(d => d.Day.Day == Data.Day && d.Day.Month == Data.Month && d.Day.Year == Data.Year).FirstOrDefault());
            }
            IsActiveAddEccezione = false;
            ApplicaStile?.Invoke(this, new EventArgs());
            ListaDateSelezionta.Clear();
            ListaOrariStd.Clear();
            ListaOrariEccezioni.Clear();
        }
        //public ICommand RemoveEccezioneCommand { get { return new CommandHandler(() => this.RemoveEccezione()); } }

        //private void RemoveEccezione()
        //{
        //    List<CustomDayLocal> ListaEccezioniDaElminare = new List<CustomDayLocal>();
        //    foreach (CustomDayLocal customDayLocal in SelectedItems)
        //    {
        //        ListaEccezioniDaElminare.Add(new CustomDayLocal() { Day = customDayLocal.Day, Hours = customDayLocal.Hours });
        //    }
        //    foreach (var EcceDaEl in ListaEccezioniDaElminare)
        //    {
        //        if (ListaEccezioniLocale.FirstOrDefault(d => d.Day.Day == EcceDaEl.Day.Day && d.Day.Month == EcceDaEl.Day.Month && d.Day.Year == EcceDaEl.Day.Year) != null)
        //            ListaEccezioniLocale.Remove(ListaEccezioniLocale.FirstOrDefault(d => d.Day.Day == EcceDaEl.Day.Day && d.Day.Month == EcceDaEl.Day.Month && d.Day.Year == EcceDaEl.Day.Year));
        //    }
        //    ApplicaStile?.Invoke(this, new EventArgs());
        //    //ListaEccezioni.Remove(SelectedItem);
        //}
        public ICommand RemoveEccezioneDaListaCommand { get { return new CommandHandler(() => this.RemoveEccezioneDaLista()); } }

        private void RemoveEccezioneDaLista()
        {
            List<CustomDayLocal> ListaEccezioniDaElminare = new List<CustomDayLocal>();
            foreach (CustomDayLocal customDayLocal in SelectedItemsLista)
            {
                ListaEccezioniDaElminare.Add(new CustomDayLocal() { Day = customDayLocal.Day, Hours = customDayLocal.Hours });
            }
            foreach (var EcceDaEl in ListaEccezioniDaElminare)
            {
                if (ListaEccezioniLocale.FirstOrDefault(d => d.Day.Day == EcceDaEl.Day.Day && d.Day.Month == EcceDaEl.Day.Month && d.Day.Year == EcceDaEl.Day.Year) != null)
                    ListaEccezioniLocale.Remove(ListaEccezioniLocale.FirstOrDefault(d => d.Day.Day == EcceDaEl.Day.Day && d.Day.Month == EcceDaEl.Day.Month && d.Day.Year == EcceDaEl.Day.Year));
            }
            ApplicaStile?.Invoke(this, new EventArgs());
        }
    }
    public class CustomDayLocal : NotificationBase
    {
        private DateTime _Day;
        public DateTime Day
        {
            get
            {
                return _Day;
            }
            set
            {
                if (SetProperty(ref _Day, value))
                {
                    _Day = value;
                }
            }
        }
        private string _Hours;
        public string Hours
        {
            get
            {
                return _Hours;
            }
            set
            {
                if (SetProperty(ref _Hours, value))
                {
                    _Hours = value;
                }
            }
        }
    }
}
