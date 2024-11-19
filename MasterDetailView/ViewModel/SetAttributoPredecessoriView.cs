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
using System.Windows.Input;

namespace MasterDetailView
{
    public class SetAttributoPredecessoriView : NotificationBase
    {
        private string _Codice;
        public string Codice
        {
            get
            {
                return _Codice;
            }
            set
            {
                if (SetProperty(ref _Codice, value))
                {
                    _Codice = value;
                }
            }
        }

        private string _Descrizione;
        public string Descrizione
        {
            get
            {
                return _Descrizione;
            }
            set
            {
                if (SetProperty(ref _Descrizione, value))
                {
                    _Descrizione = value;
                }
            }
        }

        private ObservableCollection<Predecessore> _Predecessors;
        public ObservableCollection<Predecessore> Predecessors
        {
            get
            {
                return _Predecessors;
            }
            set
            {
                if (SetProperty(ref _Predecessors, value))
                {
                    _Predecessors = value;
                }
            }
        }

        private Predecessore _Predecessor;
        public Predecessore Predecessor
        {
            get
            {
                return _Predecessor;
            }
            set
            {
                if (SetProperty(ref _Predecessor, value))
                {
                    _Predecessor = value;
                }
            }
        }

        public WBSPredecessors WBSPredecessorsData { get; set; }
        public ClientDataService DataService { get; set; }

        public SetAttributoPredecessoriView()
        {
            Predecessors = new ObservableCollection<Predecessore>();
        }

        public void Init(Guid Guid)
        {
            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            List<Guid> Guids = new List<Guid>();
            Guids.Add(Guid);
            Entity Entity = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, Guids).FirstOrDefault();
            Codice = entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Codice, false, true).PlainText;
            Descrizione = entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Nome, false, true).PlainText;
            WBSPredecessors WBSPredecessors = ((WBSItem)Entity).GetPredecessors();
            foreach (WBSPredecessor predecessor in WBSPredecessors?.Items)
            {
                Predecessore pred = new Predecessore();
                pred.IsRitardoBloccato = predecessor.DelayFixed;
                pred.Ritardo = (int)predecessor.DelayDays;
                if (predecessor.Type == WBSPredecessorType.FinishToStart)
                    pred.TipoRelazione = pred.TipiRelazioni.ElementAt(0);
                if (predecessor.Type == WBSPredecessorType.StartToStart)
                    pred.TipoRelazione = pred.TipiRelazioni.ElementAt(1);
                if (predecessor.Type == WBSPredecessorType.FinishToFinish)
                    pred.TipoRelazione = pred.TipiRelazioni.ElementAt(2);
                if (predecessor.Type == WBSPredecessorType.StartToFinish)
                    pred.TipoRelazione = pred.TipiRelazioni.ElementAt(3);
                Guids.Clear();
                Guids.Add(predecessor.WBSItemId);
                Entity CurrentEntity = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, Guids).FirstOrDefault();
                pred.Codice = entsHelper.GetValoreAttributo(CurrentEntity, BuiltInCodes.Attributo.Codice, false, true).PlainText;
                pred.Descrizione = entsHelper.GetValoreAttributo(CurrentEntity, BuiltInCodes.Attributo.Nome, false, true).PlainText;
                pred.Guid = predecessor.WBSItemId;
                Predecessors.Add(pred);
            }
        }

       

        public bool Accept()
        {
            WBSPredecessorsData = new WBSPredecessors();
            WBSPredecessorsData.Items = new List<WBSPredecessor>();
            foreach (Predecessore predecessor in Predecessors)
            {
                WBSPredecessor pred = new WBSPredecessor();
                pred.DelayFixed = predecessor.IsRitardoBloccato;
                pred.DelayDays = predecessor.Ritardo;
                if (predecessor.TipoRelazione.Key == 0)
                    pred.Type = WBSPredecessorType.FinishToStart;
                if (predecessor.TipoRelazione.Key == 1)
                    pred.Type = WBSPredecessorType.StartToStart;
                if (predecessor.TipoRelazione.Key == 2)
                    pred.Type = WBSPredecessorType.FinishToFinish;
                if (predecessor.TipoRelazione.Key == 3)
                    pred.Type = WBSPredecessorType.StartToFinish;
                pred.WBSItemId = predecessor.Guid;
                WBSPredecessorsData.Items.Add(pred);
            }
            return true;
        }
    }

    public class Predecessore : NotificationBase
    {
        public static string LocalizeFineFine { get { return LocalizationProvider.GetString("Fine-Fine (FF)"); } }
        public static string LocalizeInizioInizio { get { return LocalizationProvider.GetString("Inizio-Inizio (II)"); } }
        public static string LocalizeFineInizio { get { return LocalizationProvider.GetString("Fine-Inizio (FI)"); } }
        public static string LocalizeInizioFine { get { return LocalizationProvider.GetString("Inizio-Fine (IF)"); } }
        public Guid Guid { get; set; }

        private string _Codice;
        public string Codice
        {
            get
            {
                return _Codice;
            }
            set
            {
                if (SetProperty(ref _Codice, value))
                {
                    _Codice = value;
                }
            }
        }

        private string _Descrizione;
        public string Descrizione
        {
            get
            {
                return _Descrizione;
            }
            set
            {
                if (SetProperty(ref _Descrizione, value))
                {
                    _Descrizione = value;
                }
            }
        }

        private Dictionary<int, string> _TipiRelazioni;
        public Dictionary<int, string> TipiRelazioni
        {
            get
            {
                return _TipiRelazioni;
            }
            set
            {
                if (SetProperty(ref _TipiRelazioni, value))
                {
                    _TipiRelazioni = value;
                }
            }
        }

        private KeyValuePair<int, string> _TipoRelazione;
        public KeyValuePair<int, string> TipoRelazione
        {
            get
            {
                return _TipoRelazione;
            }
            set
            {
                if (SetProperty(ref _TipoRelazione, value))
                {
                    _TipoRelazione = value;
                }
            }
        }

        public string NomeRelazione
        {
            get
            {
                return TipoRelazione.Value;
            }
        }

        private int _Ritardo;
        public int Ritardo
        {
            get
            {
                return _Ritardo;
            }
            set
            {
                int ritardo = value;
                if (ritardo < 0)
                    ritardo = 0;

                if (SetProperty(ref _Ritardo, ritardo))
                {
                    _Ritardo = value;
                }
            }
        }

        private bool _IsRitardoBloccato;
        public bool IsRitardoBloccato
        {
            get
            {
                return _IsRitardoBloccato;
            }
            set
            {
                if (SetProperty(ref _IsRitardoBloccato, value))
                {
                    _IsRitardoBloccato = value;
                }
            }
        }
        public Predecessore()
        {
            TipiRelazioni = new Dictionary<int, string>();
            TipiRelazioni.Add(0, LocalizeFineInizio);
            TipiRelazioni.Add(1, LocalizeInizioInizio);
            TipiRelazioni.Add(2, LocalizeFineFine);
            TipiRelazioni.Add(3, LocalizeInizioFine);
            TipoRelazione = TipiRelazioni.ElementAt(0);
            IsRitardoBloccato = true;
        }
        public ICommand LockCommand { get { return new CommandHandler(() => this.Lock()); } }
        void Lock()
        {
            IsRitardoBloccato = !IsRitardoBloccato;
        }
    }
}
