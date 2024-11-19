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
using System.Windows;
using System.Windows.Input;

namespace AttivitaWpf.View
{
    public class ProgrammazioneSALView : NotificationBase
    {
        public event EventHandler<EventArgs> UpdateColumn;
        public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public IEntityWindowService WindowService { get; set; }

        private string _TextSearched;
        public string TextSearched
        {
            get
            {

                return _TextSearched;
            }
            set
            {
                SetProperty(ref _TextSearched, value);
                SubmitEnter();
            }
        }
        private ObservableCollection<AttibutiFogiloDiCalcoloView> _ListaFiltrati;

        public ObservableCollection<AttibutiFogiloDiCalcoloView> ListaFiltrati
        {
            get
            {

                return _ListaFiltrati;
            }
            set
            {
                SetProperty(ref _ListaFiltrati, value);
            }
        }
        public ObservableCollection<AttibutiFogiloDiCalcoloView> ListaAttributiNonFiltrati;

        private ObservableCollection<SALProgrammatoView> _ListaProgrammazioneSAL;
        public ObservableCollection<SALProgrammatoView> ListaProgrammazioneSAL
        {
            get
            {

                return _ListaProgrammazioneSAL;
            }
            set
            {
                SetProperty(ref _ListaProgrammazioneSAL, value);
            }
        }

        private SALProgrammatoView _ProgrammazioneSALSelezionato;
        public SALProgrammatoView ProgrammazioneSALSelezionato
        {
            get
            {

                return _ProgrammazioneSALSelezionato;
            }
            set
            {
                SetProperty(ref _ProgrammazioneSALSelezionato, value);
            }
        }

        private ObservableCollection<object> _ProgrammazioneSALSelezionati;
        public ObservableCollection<object> ProgrammazioneSALSelezionati
        {
            get
            {

                return _ProgrammazioneSALSelezionati;
            }
            set
            {
                SetProperty(ref _ProgrammazioneSALSelezionati, value);
            }
        }

        private DateTime _DataInzioGantt;
        public DateTime DataInzioGantt
        {
            get
            {

                return _DataInzioGantt;
            }
            set
            {
                SetProperty(ref _DataInzioGantt, value);
            }
        }

        private DateTime _DataFineGantt;
        public DateTime DataFineGantt
        {
            get
            {

                return _DataFineGantt;
            }
            set
            {
                SetProperty(ref _DataFineGantt, value);
            }
        }

        private CalendariItem _CalendarioDefault;
        public CalendariItem CalendarioDefault
        {
            get
            {

                return _CalendarioDefault;
            }
            set
            {
                SetProperty(ref _CalendarioDefault, value);
            }
        }

        public ProgrammazioneSALCalculator programmazioneSALCalculator { get; set; }
        public bool CreateDataColumn { get; set; } = false;

        public ProgrammazioneSALView(IDataService dataService, IEntityWindowService windowService, IMainOperation mainOperation)
        {
            DataService = dataService;
            WindowService = windowService;
            MainOperation = mainOperation;
            ListaFiltrati = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
            ListaAttributiNonFiltrati = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
            ListaProgrammazioneSAL = new ObservableCollection<SALProgrammatoView>();
            programmazioneSALCalculator = new ProgrammazioneSALCalculator(dataService);
        }

        public void Init()
        {
            CreateDataColumn = false;
            ListaFiltrati.Clear();
            ListaAttributiNonFiltrati.Clear();
            TextSearched = LocalizationProvider.GetString("Filtra");
            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

            IOrderedEnumerable<Attributo> attributi = DataService.GetEntityTypes()[BuiltInCodes.EntityType.WBS].Attributi.Values.OrderBy(item => item.DetailViewOrder);
            foreach (Attributo att in attributi)
            {
                if (att.IsInternal)
                    continue;

                AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                attributoFiltrato.CodiceOrigine = att.Codice;
                attributoFiltrato.Etichetta = att.Etichetta;
                string EntityTypeKey = entitiesHelper.GetSourceAttributo(att).EntityTypeKey;
                string EntityType = null;
                if (BuiltInCodes.EntityType.WBS != EntityTypeKey)
                {
                    EntityType = DataService.GetEntityType(entitiesHelper.GetSourceAttributo(att).EntityTypeKey).Name;
                    attributoFiltrato.SezioneRiferita = EntityType;
                }
                attributoFiltrato.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
                attributoFiltrato.IsChecked = true;
                if (attributoFiltrato.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale || attributoFiltrato.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    ListaFiltrati.Add(attributoFiltrato);
                AttibutiFogiloDiCalcoloView attributo = new AttibutiFogiloDiCalcoloView();
                attributo.CodiceOrigine = att.Codice;
                attributo.Etichetta = att.Etichetta;
                attributo.EntityTypeKey = att.EntityTypeKey;
                if (BuiltInCodes.EntityType.WBS != EntityTypeKey)
                {
                    attributo.SezioneRiferita = EntityType;
                }
                attributo.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
                attributo.IsChecked = true;
                if (attributoFiltrato.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale || attributoFiltrato.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    ListaAttributiNonFiltrati.Add(attributo);
            }

            LoadSavedSetting();

            InitializeProgrammazioneSALCalculator();

            AddStartEndGanttDate();

            SetColumnMapping();

            UpdateExistingSALGrid();

            //RicalculateDefaultColumnValue();

        }

        private void InitializeProgrammazioneSALCalculator()
        {
            List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = GetAttributiUtilizzati();
            programmazioneSALCalculator.Init(AttibutiUtilizzati);
            programmazioneSALCalculator.PrepareProgressiveValuesPerDateEnd();
        }

        private void LoadSavedSetting()
        {
            ListaProgrammazioneSAL.Clear();
            GanttData GanttData = DataService.GetGanttData();
            if (GanttData?.ProgrammazioneSAL != null)
            {
                if (GanttData.ProgrammazioneSAL.PuntiNotevoliPerData != null)
                {
                    ListaProgrammazioneSAL.Clear();

                    foreach (PuntoNotevolePerData puntoNotevolePerData in GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.OrderBy(d => d.Data))
                    {
                        if (puntoNotevolePerData.Data != new DateTime())
                        {
                            SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                            salProgrammatoView.Data = puntoNotevolePerData.Data;
                            salProgrammatoView.IsSAL = puntoNotevolePerData.IsSAL;
                            ListaProgrammazioneSAL.Add(salProgrammatoView);
                        }
                    }
                }

                if (GanttData.ProgrammazioneSAL.AttributiStandard != null)
                {
                    foreach (AttributoFoglioDiCalcolo attributoFoglioDiCalcolo in GanttData.ProgrammazioneSAL.AttributiStandard)
                    {
                        var filtrato = ListaFiltrati.Where(lf => lf.Etichetta == attributoFoglioDiCalcolo.Etichetta &&
                        lf.SezioneRiferita == attributoFoglioDiCalcolo.SezioneRiferita &&
                        lf.CodiceOrigine == attributoFoglioDiCalcolo.CodiceOrigine).FirstOrDefault();
                        if (filtrato != null)
                        {
                            filtrato.Amount = attributoFoglioDiCalcolo.Amount;
                            filtrato.ProgressiveAmount = attributoFoglioDiCalcolo.ProgressiveAmount;
                            filtrato.ProductivityPerHour = attributoFoglioDiCalcolo.ProductivityPerHour;
                        }
                        var nonfiltrato = ListaAttributiNonFiltrati.Where(lf => lf.Etichetta == attributoFoglioDiCalcolo.Etichetta &&
                        lf.SezioneRiferita == attributoFoglioDiCalcolo.SezioneRiferita &&
                        lf.CodiceOrigine == attributoFoglioDiCalcolo.CodiceOrigine).FirstOrDefault();
                        if (nonfiltrato != null)
                        {
                            nonfiltrato.Amount = attributoFoglioDiCalcolo.Amount;
                            nonfiltrato.ProgressiveAmount = attributoFoglioDiCalcolo.ProgressiveAmount;
                            nonfiltrato.ProductivityPerHour = attributoFoglioDiCalcolo.ProductivityPerHour;
                        }
                        
                    }
                }
            }
        }

        private void AddStartEndGanttDate()
        {
            DataInzioGantt = programmazioneSALCalculator.GetDataInizio();
            DataFineGantt = programmazioneSALCalculator.GetDataFine();

            if (DataInzioGantt.Year == 2500)
            {
                DataInzioGantt = DataFineGantt;
            }

            if (ListaProgrammazioneSAL.Where(x => x.Data == DataInzioGantt).FirstOrDefault() == null)
            {
                if (DataInzioGantt != new DateTime())
                {
                    SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                    salProgrammatoView.Data = DataInzioGantt;
                    salProgrammatoView.IsSAL = true;
                    ListaProgrammazioneSAL.Add(salProgrammatoView);
                }
            }
            if (ListaProgrammazioneSAL.Where(x => x.Data == DataFineGantt).FirstOrDefault() == null)
            {
                if (DataFineGantt != new DateTime())
                {
                    SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                    salProgrammatoView.Data = DataFineGantt;
                    salProgrammatoView.IsSAL = true;
                    ListaProgrammazioneSAL.Add(salProgrammatoView);
                }
            }
            ListaProgrammazioneSAL = new ObservableCollection<SALProgrammatoView>(ListaProgrammazioneSAL.OrderBy(x => x.Data).ToList());
        }
        public ICommand SubmitEnterCommand
        {
            get
            {
                return new CommandHandler(() => this.SubmitEnter());
            }
        }
        void SubmitEnter()
        {
            if (TextSearched == LocalizationProvider.GetString("Filtra"))
                return;

            ListaFiltrati = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
            foreach (var att in ListaAttributiNonFiltrati)
            {
                if (string.IsNullOrEmpty(TextSearched))
                {
                    AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                    attributoFiltrato.CodiceOrigine = att.CodiceOrigine;
                    attributoFiltrato.Etichetta = att.Etichetta;
                    attributoFiltrato.EntityTypeKey = att.EntityTypeKey;
                    attributoFiltrato.SezioneRiferita = att.SezioneRiferita;
                    attributoFiltrato.IsChecked = att.IsChecked;
                    attributoFiltrato.Amount = att.Amount;
                    attributoFiltrato.ProgressiveAmount = att.ProgressiveAmount;
                    attributoFiltrato.ProductivityPerHour = att.ProductivityPerHour;
                    ListaFiltrati.Add(attributoFiltrato);
                }
                else if (att.Etichetta.ToLower().Contains(TextSearched.ToLower()))
                {
                    AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                    attributoFiltrato.CodiceOrigine = att.CodiceOrigine;
                    attributoFiltrato.Etichetta = att.Etichetta;
                    attributoFiltrato.EntityTypeKey = att.EntityTypeKey;
                    attributoFiltrato.SezioneRiferita = att.SezioneRiferita;
                    attributoFiltrato.IsChecked = att.IsChecked;
                    attributoFiltrato.Amount = att.Amount;
                    attributoFiltrato.ProgressiveAmount = att.ProgressiveAmount;
                    attributoFiltrato.ProductivityPerHour = att.ProductivityPerHour;
                    ListaFiltrati.Add(attributoFiltrato);
                }
            }
        }

        private void SetBooleanValuesToBaseList()
        {
            //SET THE ORIGINAL LIST BOOL VALUE BEFORE RESET LIST FILTERED
            foreach (AttibutiFogiloDiCalcoloView filtrato in ListaFiltrati)
            {
                ListaAttributiNonFiltrati.Where(c => c.CodiceOrigine == filtrato.CodiceOrigine
                && c.SezioneRiferita == filtrato.SezioneRiferita
                && c.Etichetta == filtrato.Etichetta).FirstOrDefault().Amount = filtrato.Amount;
                ListaAttributiNonFiltrati.Where(c => c.CodiceOrigine == filtrato.CodiceOrigine
                && c.SezioneRiferita == filtrato.SezioneRiferita
                && c.Etichetta == filtrato.Etichetta).FirstOrDefault().ProgressiveAmount = filtrato.ProgressiveAmount;
                ListaAttributiNonFiltrati.Where(c => c.CodiceOrigine == filtrato.CodiceOrigine
                && c.SezioneRiferita == filtrato.SezioneRiferita
                && c.Etichetta == filtrato.Etichetta).FirstOrDefault().ProductivityPerHour = filtrato.ProductivityPerHour;
            }
        }
        public ICommand AddSALCommand
        {
            get
            {
                return new CommandHandler(() => this.AddSAL());
            }
        }

        private void AddSAL()
        {
            //List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = new List<AttributoFoglioDiCalcolo>();
            //foreach (AttibutiFogiloDiCalcoloView item in ListaAttributiNonFiltrati.Where(a => a.ProgressiveAmount || a.Amount || a.ProductivityPerHour))
            //{
            //    AttributoFoglioDiCalcolo attributoFoglioDiCalcolo = new AttributoFoglioDiCalcolo();
            //    attributoFoglioDiCalcolo.CodiceOrigine = item.CodiceOrigine;
            //    attributoFoglioDiCalcolo.Etichetta = item.Etichetta;
            //    attributoFoglioDiCalcolo.Formula = item.Formula;
            //    attributoFoglioDiCalcolo.Note = item.Note;
            //    attributoFoglioDiCalcolo.Amount = item.Amount;
            //    attributoFoglioDiCalcolo.ProgressiveAmount = item.ProgressiveAmount;
            //    attributoFoglioDiCalcolo.ProductivityPerHour = item.ProductivityPerHour;
            //    AttibutiUtilizzati.Add(attributoFoglioDiCalcolo);
            //}

            List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = GetAttributiUtilizzati(false);

            GenerazioneSALView generazioneSALView = new GenerazioneSALView(programmazioneSALCalculator,CreateDataColumn);
            generazioneSALView.Init(DataInzioGantt, DataFineGantt, AttibutiUtilizzati, DataService, WindowService);
            GenerazioneSALWnd generazioneSALWnd = new GenerazioneSALWnd();
            generazioneSALWnd.SourceInitialized += (x, y) => generazioneSALWnd.HideMinimizeAndMaximizeButtons();
            generazioneSALWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            generazioneSALWnd.DataContext = generazioneSALView;
            generazioneSALWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (generazioneSALWnd.ShowDialog() == true)
            {
                AddNewRowCalculated(generazioneSALView.GetSALResults(), !generazioneSALView.DoNotuseCalendar);
            }

            //var pippo = programmazioneSALCalculator.GetSALProgrammatoViewByDate(AttibutiUtilizzati, new DateTime(2022,10,30));
            //var pippo2 = programmazioneSALCalculator.GetSALProgrammatoViewPerc(AttibutiUtilizzati, "Importo", 90, new DateTime(2022, 05, 04,18,0,0), new DateTime(2022, 11, 08));
        }

        private List<AttributoFoglioDiCalcolo> GetAttributiUtilizzati(bool ForceTrue = true)
        {
            List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = new List<AttributoFoglioDiCalcolo>();
            foreach (AttibutiFogiloDiCalcoloView item in ListaAttributiNonFiltrati.Where(a => a.ProgressiveAmount || a.Amount || a.ProductivityPerHour))
            {
                AttributoFoglioDiCalcolo attributoFoglioDiCalcolo = new AttributoFoglioDiCalcolo();
                attributoFoglioDiCalcolo.CodiceOrigine = item.CodiceOrigine;
                attributoFoglioDiCalcolo.Etichetta = item.Etichetta;
                attributoFoglioDiCalcolo.Formula = item.Formula;
                attributoFoglioDiCalcolo.Note = item.Note;
                //attributoFoglioDiCalcolo.Amount = item.Amount;
                //attributoFoglioDiCalcolo.ProgressiveAmount = item.ProgressiveAmount;
                //attributoFoglioDiCalcolo.ProductivityPerHour = item.ProductivityPerHour;
                if (ForceTrue)
                {
                    attributoFoglioDiCalcolo.Amount = true;
                    attributoFoglioDiCalcolo.ProgressiveAmount = true;
                    attributoFoglioDiCalcolo.ProductivityPerHour = true;
                }
                else
                {
                    attributoFoglioDiCalcolo.Amount = item.Amount;
                    attributoFoglioDiCalcolo.ProgressiveAmount = item.ProgressiveAmount;
                    attributoFoglioDiCalcolo.ProductivityPerHour = item.ProductivityPerHour;
                }

                AttibutiUtilizzati.Add(attributoFoglioDiCalcolo);
            }
            return AttibutiUtilizzati;
        }

        private void AddNewRowCalculated(List<SALProgrammatoView> salProgrammatoViews, bool isCheckedNextWorkingDayIfHoliday)
        {
            UpdateColonnaAttributoValues(salProgrammatoViews);

            ListaProgrammazioneSAL = new ObservableCollection<SALProgrammatoView>(ListaProgrammazioneSAL.OrderBy(r => r.Data).ToList());

            if (isCheckedNextWorkingDayIfHoliday)
                MoveToTheFirstWorkDate();

            RicalculateDefaultColumnValue();

            CalulateOtherColumnByUsingProgressiveOne();
        }

        private void UpdateColonnaAttributoValues(List<SALProgrammatoView> salProgrammatoViews)
        {
            foreach (var item in salProgrammatoViews)
            {
                if (ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault() == null)
                {
                    if (item.Data != new DateTime())
                    {
                        SALProgrammatoView salProgrammatoView = new SALProgrammatoView();
                        salProgrammatoView.Data = item.Data;
                        salProgrammatoView.IsSAL = item.IsSAL;
                        salProgrammatoView.ColonnaAttributo1 = item.ColonnaAttributo1;
                        salProgrammatoView.ColonnaAttributo2 = item.ColonnaAttributo2;
                        salProgrammatoView.ColonnaAttributo3 = item.ColonnaAttributo3;
                        salProgrammatoView.ColonnaAttributo4 = item.ColonnaAttributo4;
                        salProgrammatoView.ColonnaAttributo5 = item.ColonnaAttributo5;
                        salProgrammatoView.ColonnaAttributo6 = item.ColonnaAttributo6;
                        salProgrammatoView.ColonnaAttributo7 = item.ColonnaAttributo7;
                        salProgrammatoView.ColonnaAttributo8 = item.ColonnaAttributo8;
                        salProgrammatoView.ColonnaAttributo9 = item.ColonnaAttributo9;
                        salProgrammatoView.ColonnaAttributo10 = item.ColonnaAttributo10;
                        salProgrammatoView.ColonnaAttributo11 = item.ColonnaAttributo11;
                        salProgrammatoView.ColonnaAttributo12 = item.ColonnaAttributo12;
                        salProgrammatoView.ColonnaAttributo13 = item.ColonnaAttributo13;
                        salProgrammatoView.ColonnaAttributo14 = item.ColonnaAttributo14;
                        salProgrammatoView.ColonnaAttributo15 = item.ColonnaAttributo15;
                        salProgrammatoView.ColonnaAttributo16 = item.ColonnaAttributo16;
                        salProgrammatoView.ColonnaAttributo17 = item.ColonnaAttributo17;
                        salProgrammatoView.ColonnaAttributo18 = item.ColonnaAttributo18;
                        salProgrammatoView.ColonnaAttributo19 = item.ColonnaAttributo19;
                        salProgrammatoView.ColonnaAttributo20 = item.ColonnaAttributo20;
                        ListaProgrammazioneSAL.Add(salProgrammatoView);
                    }
                }
                else
                {
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().Data = item.Data;
                    //ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().IsSAL = item.IsSAL;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo1 = item.ColonnaAttributo1;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo2 = item.ColonnaAttributo2;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo3 = item.ColonnaAttributo3;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo4 = item.ColonnaAttributo4;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo5 = item.ColonnaAttributo5;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo6 = item.ColonnaAttributo6;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo7 = item.ColonnaAttributo7;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo8 = item.ColonnaAttributo8;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo9 = item.ColonnaAttributo9;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo10 = item.ColonnaAttributo10;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo11 = item.ColonnaAttributo11;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo12 = item.ColonnaAttributo12;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo13 = item.ColonnaAttributo13;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo14 = item.ColonnaAttributo14;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo15 = item.ColonnaAttributo15;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo16 = item.ColonnaAttributo16;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo17 = item.ColonnaAttributo17;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo18 = item.ColonnaAttributo18;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo19 = item.ColonnaAttributo19;
                    ListaProgrammazioneSAL.Where(d => d.Data == item.Data).FirstOrDefault().ColonnaAttributo20 = item.ColonnaAttributo20;
                }
            }
        }

        private void MoveToTheFirstWorkDate()
        {
            List<DateTime> listDate = new List<DateTime>();
            WeekDay weekHours = null;
            List<int> weekDays = new List<int>();
            DateTimeCalculator timeCalc = new DateTimeCalculator(CalendarioDefault.GetWeekHours(), CalendarioDefault.GetCustomDays());

            foreach (SALProgrammatoView PrSAL in ListaProgrammazioneSAL)
            {
                double workingMinute = timeCalc.GetWorkingMinutesPerDay(PrSAL.Data);
                if (workingMinute == 0)
                {
                    DateTime NewDate = timeCalc.GetNextWorkingDay(PrSAL.Data);
                    if (ListaProgrammazioneSAL.Where(d => d.Data == NewDate).FirstOrDefault() == null)
                    {
                        PrSAL.Data = timeCalc.GetNextWorkingDay(PrSAL.Data);
                    }
                    else
                    {
                        listDate.Add(PrSAL.Data);
                    }
                }

            }
        }

        private void CalulateOtherColumnByUsingProgressiveOne()
        {
            int ContatoreLista = 0;
            foreach (SALProgrammatoView salProgrammatoView in ListaProgrammazioneSAL)
            {
                if (ContatoreLista == 0)
                {
                    ContatoreLista++;
                    continue;
                }
                for (int i = 1; i < SALProgrammatoView.GetTotalColumnForCycle(); i++)
                {
                    if (ColumnMapping.ContainsKey(GanttKeys.ColonnaAttributo + i))
                    {
                        string currentColumnName = ColumnMapping[GanttKeys.ColonnaAttributo + i];
                        if (currentColumnName.Contains(GanttKeys.LocalizeCumulato))
                        {
                            double? TotalValue = ListaProgrammazioneSAL.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i);
                            if (ColumnMapping.ContainsKey(GanttKeys.ColonnaAttributo + (i - 1)))
                            {
                                string codeBefore = ColumnMapping[GanttKeys.ColonnaAttributo + (i - 1)];
                                if (currentColumnName.Replace(GanttKeys.LocalizeCumulato, GanttKeys.LocalizeDelta) == codeBefore)
                                {
                                    double? difference = salProgrammatoView.GetValue(GanttKeys.ColonnaAttributo + i) - ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i);
                                    salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + (i - 1), difference);
                                }
                            }
                            if (ColumnMapping.ContainsKey(GanttKeys.ColonnaAttributo + (i + 1)))
                            {
                                //string codeAfter = ColumnMapping[GanttKeys.ColonnaAttributo + (i + 1)];
                                //if (currentColumnName.Replace(GanttKeys.LocalizeCumulato, GanttKeys.LocalizeProduttivitaH) == codeAfter)
                                //{
                                //    double difference = salProgrammatoView.GetValue(GanttKeys.ColonnaAttributo + i) - ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i);
                                //    double minutiLavorati = 0;
                                //    foreach (CalendariItem calendaroioItem in programmazioneSALCalculator.GetCalendari())
                                //    {
                                //        DateTimeCalculator timeCalc = new DateTimeCalculator(calendaroioItem.GetWeekHours(), calendaroioItem.GetCustomDays());
                                //        minutiLavorati = minutiLavorati + timeCalc.GetWorkingMinutesBetween(ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).Data, salProgrammatoView.Data);
                                //    }
                                //    salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + (i + 1), difference / (minutiLavorati / 60));
                                //}
                            }
                            if (ColumnMapping.ContainsKey(GanttKeys.ColonnaAttributo + (i + 2)))
                            {
                                if (ContatoreLista > 0)
                                {
                                    if (ListaProgrammazioneSAL.Count() == ContatoreLista + 1)
                                    {
                                        salProgrammatoView.SetValue(GanttKeys.ColonnaAttributo + (i + 2), 100);
                                        double? value = 100 / ListaProgrammazioneSAL.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i);
                                        double? preogressivePercValue = ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i) * value;
                                        ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).SetValue(GanttKeys.ColonnaAttributo + (i + 2), preogressivePercValue);
                                    }
                                    else
                                    {
                                        double? value = 100 / ListaProgrammazioneSAL.LastOrDefault().GetValue(GanttKeys.ColonnaAttributo + i);
                                        double? preogressivePercValue = ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).GetValue(GanttKeys.ColonnaAttributo + i) * value;
                                        ListaProgrammazioneSAL.ElementAt(ContatoreLista - 1).SetValue(GanttKeys.ColonnaAttributo + (i + 2), preogressivePercValue);
                                    }
                                }
                            }
                        }
                    }
                }
                ContatoreLista++;
            }
        }

        private void RicalculateDefaultColumnValue()
        {
            double durataPeriodo = 0;
            int contatore = 0;
            int contatoreSAL = 0;

            foreach (SALProgrammatoView salProgrammatoView in ListaProgrammazioneSAL)
            {
                if (contatore != 0)
                {
                    durataPeriodo = (double)(salProgrammatoView.Data - ListaProgrammazioneSAL.ElementAt(contatore - 1).Data).TotalDays;
                }


                salProgrammatoView.GiorniPeriodo = Math.Ceiling(durataPeriodo);
                salProgrammatoView.GiorniProgressivo = Math.Ceiling((salProgrammatoView.Data - ListaProgrammazioneSAL.FirstOrDefault().Data).TotalDays);
                contatore++;

                if (contatoreSAL == 0)
                {
                    salProgrammatoView.ContatoreSAL = 0;
                    contatoreSAL++;
                }
                else
                {
                    if (salProgrammatoView.IsSAL)
                    {
                        salProgrammatoView.ContatoreSAL = contatoreSAL;
                        contatoreSAL++;
                    }
                }
            }
        }

        public void RicalculateSALCounter()
        {
            int contatoreSAL = 0;

            foreach (SALProgrammatoView salProgrammatoView in ListaProgrammazioneSAL)
            {
                if (contatoreSAL == 0)
                {
                    salProgrammatoView.ContatoreSAL = 0;
                    contatoreSAL++;
                }
                else
                {
                    if (salProgrammatoView.IsSAL)
                    {
                        salProgrammatoView.ContatoreSAL = contatoreSAL;
                        contatoreSAL++;
                    }
                    else
                    {
                        salProgrammatoView.ContatoreSAL = null;
                    }
                }
            }
        }

        public ICommand RemoveSALCommand
        {
            get
            {
                return new CommandHandler(() => this.RemoveSAL());
            }
        }

        //public void AddRemoveSALObject(bool IsAdd, SALProgrammatoView salProgrammatoView)
        //{
        //    if (IsAdd)
        //    {
        //        if (ListaProgrammazioneSAL.Where(s => s.Data == salProgrammatoView.Data).FirstOrDefault() == null)
        //        {
        //            ProgrammazioneSALSelezionati.Add(salProgrammatoView);
        //        }
        //    }   
        //    else
        //    {
        //        ProgrammazioneSALSelezionati.Remove(salProgrammatoView);
        //    }
        //}

        private void RemoveSAL(List<DateTime> ListaDateToRemove = null)
        {
            List<DateTime> ListaDate = new List<DateTime>();

            if (ProgrammazioneSALSelezionati != null)
            {
                foreach (SALProgrammatoView item in ProgrammazioneSALSelezionati)
                {
                    if (item.Data != DataInzioGantt && item.Data != DataFineGantt)
                    {
                        ListaDate.Add(item.Data);
                    }
                }
            }

            if (ProgrammazioneSALSelezionato != null)
            {
                ListaDate.Add(ProgrammazioneSALSelezionato.Data);
            }

            if (ListaDateToRemove != null)
            {
                foreach (DateTime Data in ListaDateToRemove)
                {
                    ListaDate.Add(Data);
                }
            }

            foreach (DateTime date in ListaDate)
            {
                SALProgrammatoView sal = ListaProgrammazioneSAL.Where(d => d.Data == date).FirstOrDefault();
                if (sal != null)
                    ListaProgrammazioneSAL.Remove(sal);
            }

            ListaProgrammazioneSAL = new ObservableCollection<SALProgrammatoView>(ListaProgrammazioneSAL.OrderBy(d => d.Data).ToList());

            //int contatore = 0;
            //ListaProgrammazioneSAL.FirstOrDefault().IsSAL = true;
            //ListaProgrammazioneSAL.LastOrDefault().IsSAL = true;
            //foreach (var item in ListaProgrammazioneSAL)
            //{
            //    if (contatore == 0)
            //        item.IsSAL = false;
            //    else
            //        item.IsSAL = true;
            //    contatore++;
            //}

            RicalculateDefaultColumnValue();
        }

        public ICommand SelectAllCommand
        {
            get
            {
                return new CommandHandler(() => this.SelectAll());
            }
        }

        private void SelectAll()
        {
            ProgrammazioneSALSelezionati.Clear();
            ProgrammazioneSALSelezionati = new ObservableCollection<object>(ListaProgrammazioneSAL);
            foreach (var item in ListaProgrammazioneSAL)
            {
                ProgrammazioneSALSelezionati.Add(item);
            }
        }

        public ObservableCollection<SALProgrammatoView> GetAllItem()
        {
            return ListaProgrammazioneSAL;
        }

        public ICommand TabSelectionChangedCommand
        {
            get
            {
                return new CommandHandlerParam(param => this.TabSelectionChangedSAL(param));
            }
        }

        private Dictionary<string, string> ColumnMapping = new Dictionary<string, string>();
        private void TabSelectionChangedSAL(object param)
        {
            if (((System.Windows.Controls.SelectionChangedEventArgs)param).RemovedItems[0] is System.Windows.Controls.TabItem)
            {
                if (((System.Windows.Controls.TabItem)((System.Windows.Controls.SelectionChangedEventArgs)param).RemovedItems[0]).Header == LocalizationProvider.GetString("Attributi"))
                {
                    SetBooleanValuesToBaseList();
                    SetColumnMapping();
                    UpdateExistingSALGrid();
                }
            }
        }

        private void SetColumnMapping()
        {
            int ContatoreColonna = 1;
            ColumnMapping.Clear();
            foreach (AttibutiFogiloDiCalcoloView Attributo in ListaAttributiNonFiltrati)
            {
                if (Attributo.Amount || Attributo.ProgressiveAmount || Attributo.ProductivityPerHour || Attributo.ProgressiveAmount)
                {
                    ColumnMapping.Add(GanttKeys.ColonnaAttributo + ContatoreColonna, Attributo.CodiceOrigine + GanttKeys.LocalizeDelta);
                    ContatoreColonna++;
                    ColumnMapping.Add(GanttKeys.ColonnaAttributo + ContatoreColonna, Attributo.CodiceOrigine + GanttKeys.LocalizeCumulato);
                    ContatoreColonna++;
                    ColumnMapping.Add(GanttKeys.ColonnaAttributo + ContatoreColonna, Attributo.CodiceOrigine + GanttKeys.LocalizeProduttivitaH);
                    ContatoreColonna++;
                    ColumnMapping.Add(GanttKeys.ColonnaAttributo + ContatoreColonna, Attributo.CodiceOrigine + GanttKeys.LocalizeValorePercentualeProgressiva);
                    ContatoreColonna++;
                }
            }
        }

        private void UpdateExistingSALGrid()
        {
            List<AttributoFoglioDiCalcolo> AttibutiUtilizzati = GetAttributiUtilizzati();

            if (ListaProgrammazioneSAL.Count() > 0)
            {
                GenerazioneSALView generazioneSALView = new GenerazioneSALView(programmazioneSALCalculator, CreateDataColumn);
                programmazioneSALCalculator.Init(AttibutiUtilizzati);
                programmazioneSALCalculator.PrepareProgressiveValuesPerDateEnd();
                generazioneSALView.Init(DataInzioGantt, DataFineGantt, AttibutiUtilizzati, DataService, WindowService);
                List<SALProgrammatoView> listaGenerataSAL = generazioneSALView.GetSALResultsByDate(ListaProgrammazioneSAL.Select(f => f.Data).ToList());
                UpdateColonnaAttributoValues(listaGenerataSAL);
            }

            RicalculateDefaultColumnValue();

            CalulateOtherColumnByUsingProgressiveOne();

            UpdateColumn?.Invoke(this, new EventArgs());
        }

        public void RunUpdateColumn()
        {
            UpdateColumn?.Invoke(this, new EventArgs());
        }

        public void AcceptButton()
        {
            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupBegin(UndoGroupsName.ProgrammazioneSAL, null);

            GanttData GanttData = DataService.GetGanttData();
            GanttData.ProgrammazioneSAL = new ProgrammazioneSAL();
            GanttData.ProgrammazioneSAL.PuntiNotevoliPerData = new List<PuntoNotevolePerData>();
            foreach (SALProgrammatoView ProgrammazioneSAL in ListaProgrammazioneSAL)
            {
                PuntoNotevolePerData puntoNotevolePerData = new PuntoNotevolePerData();
                puntoNotevolePerData.Data = ProgrammazioneSAL.Data;
                puntoNotevolePerData.IsSAL = ProgrammazioneSAL.IsSAL;
                GanttData.ProgrammazioneSAL.PuntiNotevoliPerData.Add(puntoNotevolePerData);
            }

            SetBooleanValuesToBaseList();

            GanttData.ProgrammazioneSAL.AttributiStandard = new List<AttributoFoglioDiCalcolo>();
            foreach (AttibutiFogiloDiCalcoloView item in ListaAttributiNonFiltrati.Where(a => a.ProductivityPerHour || a.Amount || a.ProgressiveAmount))
            {
                AttributoFoglioDiCalcolo attributoFoglioDiCalcolo = new AttributoFoglioDiCalcolo();
                attributoFoglioDiCalcolo.CodiceOrigine = item.CodiceOrigine;
                attributoFoglioDiCalcolo.DefinizioneAttributo = item.DefinizioneAttributo;
                attributoFoglioDiCalcolo.Etichetta = item.Etichetta;
                attributoFoglioDiCalcolo.SezioneRiferita = item.SezioneRiferita;
                attributoFoglioDiCalcolo.Formula = item.Formula;
                attributoFoglioDiCalcolo.Note = item.Note;
                attributoFoglioDiCalcolo.Amount = item.Amount;
                attributoFoglioDiCalcolo.ProgressiveAmount = item.ProgressiveAmount;
                attributoFoglioDiCalcolo.ProductivityPerHour = item.ProductivityPerHour;
                GanttData.ProgrammazioneSAL.AttributiStandard.Add(attributoFoglioDiCalcolo);
            }

            DataService.SetGanttData(GanttData);

            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupEnd();
        }

        public void KeepPreviousValueIsSAL(int rowIndex)
        {
            ListaProgrammazioneSAL.ElementAt(rowIndex).IsSAL = false;
        }

        public List<string> GetListaColonne()
        {
            List<string> listacolonna = new List<string>();
            foreach (AttibutiFogiloDiCalcoloView item in ListaAttributiNonFiltrati.Where(a => a.ProgressiveAmount || a.Amount || a.ProductivityPerHour))
            {
                if (item.Amount)
                    listacolonna.Add(item.Etichetta + GanttKeys.LocalizeDelta);
                if (item.ProgressiveAmount)
                    listacolonna.Add(item.Etichetta + GanttKeys.LocalizeCumulato);
                if (item.ProductivityPerHour)
                    listacolonna.Add(item.Etichetta + GanttKeys.LocalizeProduttivitaH);
            }
            return listacolonna;
        }

        public List<ColumnVisible> GetListaColonneB()
        {
            List<ColumnVisible> columnsVisible = new List<ColumnVisible>();
            foreach (AttibutiFogiloDiCalcoloView item in ListaAttributiNonFiltrati.Where(a => a.ProgressiveAmount || a.Amount || a.ProductivityPerHour))
            {
                ColumnVisible columnVisibleA = new ColumnVisible();
                columnVisibleA.Name = item.Etichetta + GanttKeys.LocalizeDelta;
                columnVisibleA.IsVisible = item.Amount;
                columnsVisible.Add(columnVisibleA);
                ColumnVisible columnVisibleC = new ColumnVisible();
                columnVisibleC.Name = item.Etichetta + GanttKeys.LocalizeCumulato;
                columnVisibleC.IsVisible = item.ProgressiveAmount;
                columnsVisible.Add(columnVisibleC);
                ColumnVisible columnVisibleP = new ColumnVisible();
                columnVisibleP.Name = item.Etichetta + GanttKeys.LocalizeProduttivitaH;
                columnVisibleP.IsVisible = item.ProductivityPerHour;
                columnsVisible.Add(columnVisibleP);
                ColumnVisible columnVisiblePer = new ColumnVisible();
                columnVisiblePer.Name = item.Etichetta + GanttKeys.LocalizeValorePercentualeProgressiva;
                columnVisiblePer.IsVisible = item.ProgressiveAmount;
                columnsVisible.Add(columnVisiblePer);
            }
            return columnsVisible;
        }

        //public bool CheckIfSelectedIsEditable()
        //{
        //    if ((bool)ProgrammazioneSALSelezionato?.IsUserInsert)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //    return false; ;
        //}

        public bool GetCreateDataColumn()
        {
            return CreateDataColumn;
        }
    }

    public class ColumnVisible
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
    }
}
