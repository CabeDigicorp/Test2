using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.XtraRichEdit.Import.Html;
using FogliDiCalcoloWpf.View;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FogliDiCalcoloWpf
{
    public class FoglioDiCalcoloBaseDataView : NotificationBase
    {
        public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public IEntityWindowService WindowService { get; set; }

        private string _Title;
        public string Title
        {
            get
            {

                return _Title;
            }
            set
            {
                SetProperty(ref _Title, value);
            }
        }

        private string _DataText;
        public string DataText
        {
            get
            {

                return _DataText;
            }
            set
            {
                SetProperty(ref _DataText, value);
            }
        }

        private string _DataTextCompleted;
        public string DataTextCompleted
        {
            get
            {

                return _DataTextCompleted;
            }
            set
            {
                SetProperty(ref _DataTextCompleted, value);
            }
        }

        private string _TabellaText;
        public string TabellaText
        {
            get
            {

                return _TabellaText;
            }
            set
            {
                SetProperty(ref _TabellaText, value);
            }
        }


        private string _TabellaTextCompleted;
        public string TabellaTextCompleted
        {
            get
            {

                return _TabellaTextCompleted;
            }
            set
            {
                SetProperty(ref _TabellaTextCompleted, value);
            }
        }

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

        private bool? _IsAllChecked;
        public bool? IsAllChecked
        {
            get
            {

                return _IsAllChecked;
            }
            set
            {
                SetProperty(ref _IsAllChecked, value);
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

        private AttibutiFogiloDiCalcoloView _FiltratoSelezionato;
        public AttibutiFogiloDiCalcoloView FiltratoSelezionato
        {
            get
            {

                return _FiltratoSelezionato;
            }
            set
            {
                SetProperty(ref _FiltratoSelezionato, value);
            }
        }

        public ObservableCollection<AttibutiFogiloDiCalcoloView> ListaAttributiNonFiltrati;

        private ObservableCollection<AttibutiFogiloDiCalcoloView> _ListaAttributiAggiunti;
        public ObservableCollection<AttibutiFogiloDiCalcoloView> ListaAttributiAggiunti
        {
            get
            {

                return _ListaAttributiAggiunti;
            }
            set
            {
                SetProperty(ref _ListaAttributiAggiunti, value);
            }
        }

        private AttibutiFogiloDiCalcoloView _AttributoAggiuntoSelezionato;
        public AttibutiFogiloDiCalcoloView AttributoAggiuntoSelezionato
        {
            get
            {

                return _AttributoAggiuntoSelezionato;
            }
            set
            {
                SetProperty(ref _AttributoAggiuntoSelezionato, value);
            }
        }

        private string _GetCopiedText;
        public string GetCopiedText
        {
            get
            {

                return _GetCopiedText;
            }
            set
            {
                SetProperty(ref _GetCopiedText, value);
                if (value != null)
                    System.Windows.Clipboard.SetText(value);
            }
        }

        public FogliDiCalcoloData fogliDiCalcoloData;
        public string sezionekey;
        public FoglioDiCalcoloBaseDataView()
        {
            ListaFiltrati = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
            ListaAttributiNonFiltrati = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
            ListaAttributiAggiunti = new ObservableCollection<AttibutiFogiloDiCalcoloView>();
        }

        public virtual void Init(string SezioneKey)
        {
            ListaFiltrati.Clear();
            ListaAttributiNonFiltrati.Clear();
            ListaAttributiAggiunti.Clear();
            TextSearched = LocalizationProvider.GetString("Filtra");
            GetCopiedText = null;

            GeneraDatiPerTabella(SezioneKey);

            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

            sezionekey = SezioneKey;

            fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();

            IOrderedEnumerable<Attributo> attributi = DataService.GetEntityTypes()[sezionekey.Replace(FogliDiCalcoloKeys.ConstSched, "")].Attributi.Values.OrderBy(item => item.DetailViewOrder);
            foreach (Attributo att in attributi)
            {
                if (att.IsInternal)
                    continue;

                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    continue;
                }
                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                {
                    if (entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        continue;
                    }
                }

                AttibutiFogiloDiCalcoloView attributoFiltrato = new AttibutiFogiloDiCalcoloView();
                attributoFiltrato.CodiceOrigine = att.Codice;
                attributoFiltrato.Etichetta = att.Etichetta;
                string EntityTypeKey = entitiesHelper.GetSourceAttributo(att).EntityTypeKey;
                attributoFiltrato.EntityTypeKey = EntityTypeKey;
                string EntityType = null;
                if (sezionekey != EntityTypeKey)
                {
                    EntityType = DataService.GetEntityType(entitiesHelper.GetSourceAttributo(att).EntityTypeKey).Name;
                    attributoFiltrato.SezioneRiferita = EntityType;
                }
                attributoFiltrato.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
                attributoFiltrato.IsChecked = true;
                ListaFiltrati.Add(attributoFiltrato);
                AttibutiFogiloDiCalcoloView attributo = new AttibutiFogiloDiCalcoloView();
                attributo.CodiceOrigine = att.Codice;
                attributo.Etichetta = att.Etichetta;
                attributo.EntityTypeKey = EntityTypeKey;
                if (sezionekey != EntityTypeKey)
                {
                    attributo.SezioneRiferita = EntityType;
                }
                attributo.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
                attributo.IsChecked = true;
                ListaAttributiNonFiltrati.Add(attributo);
            }

            if (fogliDiCalcoloData.FoglioDiCalcolo != null)
            {
                FoglioDiCalcolo foglioDiCalcolo = fogliDiCalcoloData.FoglioDiCalcolo.Where(f => f.SezioneKey == sezionekey).FirstOrDefault();
                if (foglioDiCalcolo != null)
                {
                    foreach (AttributoFoglioDiCalcolo FiltratoSalvato in foglioDiCalcolo.AttributiFormuleFoglioDiCalcolo)
                    {
                        AttibutiFogiloDiCalcoloView attributo = new AttibutiFogiloDiCalcoloView();
                        attributo.Etichetta = FiltratoSalvato.Etichetta;
                        attributo.Formula = FiltratoSalvato.Formula;
                        attributo.Note = FiltratoSalvato.Note;
                        attributo.IsChecked = true;
                        ListaAttributiAggiunti.Add(attributo);
                    }
                }
            }
            IsAllChecked = true;
        }

        public void GeneraDatiPerTabella(string SezioneKey)
        {
            if (SezioneKey == BuiltInCodes.EntityType.Computo)
            {
                DataTextCompleted = LocalizationProvider.GetString("Foglio") + ": " + LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Data");
                DataText = LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Data");
                TabellaText = LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Tabella");
                TabellaTextCompleted = LocalizationProvider.GetString("Intervallo") + ": " + LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Tabella");
                Title = "Gestione dati" + " " + LocalizationProvider.GetString("Computo");
            }
            if (SezioneKey == BuiltInCodes.EntityType.WBS)
            {
                //DataTextCompleted = LocalizationProvider.GetString("Foglio") + ": " + LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Data");
                DataTextCompleted = string.Format("{0}: {1}", LocalizationProvider.GetString("Foglio"), LocalizationProvider.GetString("ProduttivitaGantt"));
                //DataText = LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Data");
                //DataText = "WBSGantt" + "Data";
                DataText = LocalizationProvider.GetString("ProduttivitaGantt");
                //TabellaText = LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Tabella");
                TabellaText = "WBSGantt" + "Tabella";
                TabellaTextCompleted = LocalizationProvider.GetString("Intervallo") + ": " + LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Tabella");
                Title = "Gestione dati" + " " + LocalizationProvider.GetString("WBSGantt");
            }
        }

        public void SetCheckForEachAttribute(bool v)
        {
            foreach (AttibutiFogiloDiCalcoloView att in ListaFiltrati)
            {
                att.IsChecked = (bool)v;
            }
        }
        public bool IsIndeterminateState()
        {
            if (ListaFiltrati.Any(af => af.IsChecked) && ListaFiltrati.Any(af => !af.IsChecked))
            {
                return true;
            }
            else
            {
                return false;
            }
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
            {
                return;
            }
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
                    ListaFiltrati.Add(attributoFiltrato);
                }
            }
        }

        public ICommand AddAttributoCommand { get { return new CommandHandler(() => this.AddAttributo()); } }

        private void AddAttributo()
        {
            int contatore = 0;
            bool enabledAction = true;
            foreach (var item in ListaAttributiAggiunti)
            {
                if (string.IsNullOrEmpty(item.Etichetta))
                {
                    enabledAction = false;
                }
            }

            if (enabledAction)
            {
                foreach (var item in ListaAttributiAggiunti)
                {
                    contatore++;
                    if (AttributoAggiuntoSelezionato != null)
                    {
                        if (item.Etichetta == AttributoAggiuntoSelezionato.Etichetta)
                        {
                            break;
                        }
                    }
                }
            }


            if (enabledAction)
            {
                ListaAttributiAggiunti.Insert(contatore, new AttibutiFogiloDiCalcoloView());
                AttributoAggiuntoSelezionato = ListaAttributiAggiunti.ElementAt(contatore);
            }

        }

        public ICommand RemoveAttributoCommand { get { return new CommandHandler(() => this.DeleteAttributo()); } }

        private void DeleteAttributo()
        {
            ListaAttributiAggiunti.Remove(AttributoAggiuntoSelezionato);
        }

        public virtual bool Accept()
        {
            bool ExistSezione = false;

            if (fogliDiCalcoloData.FoglioDiCalcolo == null)
            {
                fogliDiCalcoloData.FoglioDiCalcolo = new List<Model.FoglioDiCalcolo>();
            }

            foreach (var foglioDiCalcolo in fogliDiCalcoloData.FoglioDiCalcolo)
            {
                if (foglioDiCalcolo.SezioneKey == sezionekey)
                {
                    ExistSezione = true;

                    foglioDiCalcolo.AttributiStandardFoglioDiCalcolo.Clear();
                    foreach (var Aggiunto in ListaAttributiNonFiltrati)
                    {
                        AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                        attributo.Etichetta = Aggiunto.Etichetta;
                        attributo.CodiceOrigine = Aggiunto.CodiceOrigine;
                        attributo.DefinizioneAttributo = Aggiunto.DefinizioneAttributo;
                        foglioDiCalcolo.AttributiStandardFoglioDiCalcolo.Add(attributo);
                    }

                    foglioDiCalcolo.AttributiFormuleFoglioDiCalcolo.Clear();
                    foreach (var Aggiunto in ListaAttributiAggiunti)
                    {
                        AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                        attributo.Etichetta = Aggiunto.Etichetta;
                        attributo.Formula = Aggiunto.Formula;
                        attributo.Note = Aggiunto.Note;
                        foglioDiCalcolo.AttributiFormuleFoglioDiCalcolo.Add(attributo);
                    }
                }
            }

            if (!ExistSezione)
            {
                fogliDiCalcoloData.FoglioDiCalcolo.Add(new Model.FoglioDiCalcolo());
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().Foglio = DataText;
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().Tabella = TabellaText;
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().SezioneKey = sezionekey;
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiFormuleFoglioDiCalcolo = new List<AttributoFoglioDiCalcolo>();
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiStandardFoglioDiCalcolo = new List<AttributoFoglioDiCalcolo>();

                foreach (var Aggiunto in ListaAttributiNonFiltrati)
                {
                    AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                    attributo.Etichetta = Aggiunto.Etichetta;
                    attributo.CodiceOrigine = Aggiunto.CodiceOrigine;
                    attributo.DefinizioneAttributo = Aggiunto.DefinizioneAttributo;
                    fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiStandardFoglioDiCalcolo.Add(attributo);
                }

                foreach (var Aggiunto in ListaAttributiAggiunti)
                {
                    if (!string.IsNullOrEmpty(Aggiunto.Etichetta))
                    {
                        AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                        attributo.Etichetta = Aggiunto.Etichetta;
                        attributo.Formula = Aggiunto.Formula;
                        attributo.Note = Aggiunto.Note;
                        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiFormuleFoglioDiCalcolo.Add(attributo);
                    }
                }
            }



            DataService.SetFogliDiCalcoloData(fogliDiCalcoloData);
            return true;
        }
    }
}
