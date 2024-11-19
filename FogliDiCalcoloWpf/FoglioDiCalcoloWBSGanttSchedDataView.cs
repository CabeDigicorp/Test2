using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class FoglioDiCalcoloWBSGanttSchedDataView : FoglioDiCalcoloBaseDataView
    {
        private Dictionary<int, string> _Periods;
        public Dictionary<int, string> Periods
        {
            get
            {

                return _Periods;
            }
            set
            {
                SetProperty(ref _Periods, value);
            }
        }

        private KeyValuePair<int, string> _Period;
        public KeyValuePair<int, string> Period
        {
            get
            {

                return _Period;
            }
            set
            {
                SetProperty(ref _Period, value);
                if (value.Key == 1)
                    IsDateEnabled = true;
                else
                    IsDateEnabled = false;
            }
        }

        private DateTime _DateFrom;
        public DateTime DateFrom
        {
            get
            {

                return _DateFrom;
            }
            set
            {
                SetProperty(ref _DateFrom, value);
            }
        }

        private bool _IsDateEnabled;
        public bool IsDateEnabled
        {
            get
            {

                return _IsDateEnabled;
            }
            set
            {
                SetProperty(ref _IsDateEnabled, value);
            }
        }
        
        public FoglioDiCalcoloWBSGanttSchedDataView() : base()
        {
            Periods = new Dictionary<int, string>();
            Periods.Add(0, LocalizationProvider.GetString("Giorni"));
            Periods.Add(1, LocalizationProvider.GetString("Settimana"));
            Periods.Add(2, LocalizationProvider.GetString("Mese"));
            Periods.Add(3, LocalizationProvider.GetString("Anno"));
            Periods.Add(4, LocalizationProvider.GetString("Produttivita costante"));
            Periods.Add(5, LocalizationProvider.GetString("SAL Programmati"));

            Period = Periods.ElementAt(2);
        }
        public override void Init(string SezioneKey)
        {
            base.Init(SezioneKey);
            GanttData ganttData = DataService.GetGanttData();
            Period = Periods.ElementAt(2);
            DateFrom = ganttData.DataInizio;
            if (DateFrom == new DateTime())
                DateFrom = DateTime.Today;

            if (ganttData != null)
            {
                if (ganttData.SchedulazioneValori != null)
                {
                    Period = Periods.Where(p => p.Key == ganttData.SchedulazioneValori.Periodo).FirstOrDefault();
                    DateFrom = ganttData.SchedulazioneValori.DateFrom;
                    if (ganttData.SchedulazioneValori.Attributi != null)
                    {
                        foreach (var attributo in ganttData.SchedulazioneValori.Attributi)
                        {
                            AttivitaWpf.View.AttibutiFogiloDiCalcoloView att = ListaAttributiNonFiltrati.Where(a => a.CodiceOrigine == attributo.CodiceOrigine && a.SezioneRiferita == attributo.SezioneRiferita).FirstOrDefault();
                            if (att != null)
                                att.Amount = attributo.Amount;
                            att = ListaFiltrati.Where(a => a.CodiceOrigine == attributo.CodiceOrigine && a.SezioneRiferita == attributo.SezioneRiferita).FirstOrDefault();
                            if (att != null)
                                att.Amount = attributo.Amount;
                        }
                    }
                }
            }

            //DataTextCompleted = LocalizationProvider.GetString("Foglio") + ": " + LocalizationProvider.GetString("WBSGanttSched") + LocalizationProvider.GetString("Data");
            DataTextCompleted = string.Format("{0}: {1}", LocalizationProvider.GetString("Foglio"), LocalizationProvider.GetString("ProgrammaLavori"));
            //DataText = LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Data");
            DataText = "WBSGanttSched" + "Data";
            //TabellaText = LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Tabella");
            TabellaText = "WBSGanttSched" + "Tabella";
            TabellaTextCompleted = LocalizationProvider.GetString("Intervallo") + ": " + LocalizationProvider.GetString("WBSGanttSched") + LocalizationProvider.GetString("Tabella");
            Title = "Gestione dati" + " " + LocalizationProvider.GetString("Schedulazione valori periodo");
        }
        public void RimuoviAttributiDiversiDaRealiContabilita()
        {
            var ListaOrigine = ListaFiltrati.Where(r => r.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale || r.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita).ToList();
            ListaAttributiNonFiltrati.Clear();
            ListaFiltrati.Clear();
            foreach (var item in ListaOrigine)
            {
                ListaAttributiNonFiltrati.Add(item);
                ListaFiltrati.Add(item);
            }
        }

        public override bool Accept()
        {

            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupBegin(UndoGroupsName.ProgrammaLavori, null);

            GanttData ganttData = DataService.GetGanttData();
            if (ganttData != null)
            {  
                ganttData.SchedulazioneValori = new SchedulazioneValori();
                ganttData.SchedulazioneValori.Attributi = new List<AttributoFoglioDiCalcolo>();
                ganttData.SchedulazioneValori.Periodo = Period.Key;
                ganttData.SchedulazioneValori.DateFrom = DateFrom;
                foreach (var Filtrato in ListaAttributiNonFiltrati)
                {
                    if (Filtrato.Amount)
                    {
                        AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                        attributo.CodiceOrigine = Filtrato.CodiceOrigine;
                        attributo.SezioneRiferita = Filtrato.SezioneRiferita;
                        attributo.DefinizioneAttributo = Filtrato.DefinizioneAttributo;
                        attributo.Etichetta = Filtrato.Etichetta;
                        attributo.Formula = Filtrato.Formula;
                        attributo.Note = Filtrato.Note;
                        attributo.Amount = Filtrato.Amount;
                        ganttData.SchedulazioneValori.Attributi.Add(attributo);
                    }
                }

                DataService.SetGanttData(ganttData);
            }

            if (DeveloperVariables.IsUndoActive)
                MainOperation.UndoGroupEnd();

            return true;
        }

        public List<AttributoFoglioDiCalcolo> GetAttributi()
        {
            List<AttributoFoglioDiCalcolo> listaAttributiFoglioDiCalcolo = new List<AttributoFoglioDiCalcolo>();

            foreach (var Filtrato in ListaAttributiNonFiltrati)
            {
                if (Filtrato.Amount)
                {
                    AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                    attributo.CodiceOrigine = Filtrato.CodiceOrigine;
                    attributo.DefinizioneAttributo = Filtrato.DefinizioneAttributo;
                    attributo.Etichetta = Filtrato.Etichetta;
                    attributo.Formula = Filtrato.Formula;
                    attributo.Note = Filtrato.Note;
                    attributo.Amount = Filtrato.Amount;
                    listaAttributiFoglioDiCalcolo.Add(attributo);
                }
            }

            return listaAttributiFoglioDiCalcolo;
        }
    }
}
