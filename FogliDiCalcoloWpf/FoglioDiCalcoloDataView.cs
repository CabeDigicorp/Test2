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
using System.Windows.Input;

namespace FogliDiCalcoloWpf
{
    public class FoglioDiCalcoloDataView : FoglioDiCalcoloBaseDataView
    {
        //public IDataService DataService { get; set; }
        //public IMainOperation MainOperation { get; set; }
        //public IEntityWindowService WindowService { get; set; }

        //private string _Title;
        //public string Title
        //{
        //    get
        //    {

        //        return _Title;
        //    }
        //    set
        //    {
        //        SetProperty(ref _Title, value);
        //    }
        //}

        //private string _DataText;
        //public string DataText
        //{
        //    get
        //    {

        //        return _DataText;
        //    }
        //    set
        //    {
        //        SetProperty(ref _DataText, value);
        //    }
        //}

        //private string _DataTextCompleted;
        //public string DataTextCompleted
        //{
        //    get
        //    {

        //        return _DataTextCompleted;
        //    }
        //    set
        //    {
        //        SetProperty(ref _DataTextCompleted, value);
        //    }
        //}

        //private string _TabellaText;
        //public string TabellaText
        //{
        //    get
        //    {

        //        return _TabellaText;
        //    }
        //    set
        //    {
        //        SetProperty(ref _TabellaText, value);
        //    }
        //}


        //private string _TabellaTextCompleted;
        //public string TabellaTextCompleted
        //{
        //    get
        //    {

        //        return _TabellaTextCompleted;
        //    }
        //    set
        //    {
        //        SetProperty(ref _TabellaTextCompleted, value);
        //    }
        //}

        //private string _TextSearched;
        //public string TextSearched
        //{
        //    get
        //    {

        //        return _TextSearched;
        //    }
        //    set
        //    {
        //        SetProperty(ref _TextSearched, value);
        //        SubmitEnter();
        //    }
        //}

        //private bool? _IsAllChecked;
        //public bool? IsAllChecked
        //{
        //    get
        //    {

        //        return _IsAllChecked;
        //    }
        //    set
        //    {
        //        SetProperty(ref _IsAllChecked, value);
        //    }
        //}

        //private ObservableCollection<AttributoFoglioDiCalcolo> _ListaFiltrati;
        //public ObservableCollection<AttributoFoglioDiCalcolo> ListaFiltrati
        //{
        //    get
        //    {

        //        return _ListaFiltrati;
        //    }
        //    set
        //    {
        //        SetProperty(ref _ListaFiltrati, value);
        //    }
        //}

        //private AttributoFoglioDiCalcolo _FiltratoSelezionato;
        //public AttributoFoglioDiCalcolo FiltratoSelezionato
        //{
        //    get
        //    {

        //        return _FiltratoSelezionato;
        //    }
        //    set
        //    {
        //        SetProperty(ref _FiltratoSelezionato, value);
        //    }
        //}

        //public ObservableCollection<AttributoFoglioDiCalcolo> ListaAttributiNonFiltrati;

        //private ObservableCollection<AttributoFoglioDiCalcolo> _ListaAttributiAggiunti;
        //public ObservableCollection<AttributoFoglioDiCalcolo> ListaAttributiAggiunti
        //{
        //    get
        //    {

        //        return _ListaAttributiAggiunti;
        //    }
        //    set
        //    {
        //        SetProperty(ref _ListaAttributiAggiunti, value);
        //    }
        //}

        //private AttributoFoglioDiCalcolo _AttributoAggiuntoSelezionato;
        //public AttributoFoglioDiCalcolo AttributoAggiuntoSelezionato
        //{
        //    get
        //    {

        //        return _AttributoAggiuntoSelezionato;
        //    }
        //    set
        //    {
        //        SetProperty(ref _AttributoAggiuntoSelezionato, value);
        //    }
        //}

        //private string _GetCopiedText;
        //public string GetCopiedText
        //{
        //    get
        //    {

        //        return _GetCopiedText;
        //    }
        //    set
        //    {
        //        SetProperty(ref _GetCopiedText, value);
        //        if (value != null)
        //            System.Windows.Clipboard.SetText(value);
        //    }
        //}

        //private FogliDiCalcoloData fogliDiCalcoloData;
        //private string sezionekey;
        //public FoglioDiCalcoloDataView()
        //{
        //    ListaFiltrati = new ObservableCollection<AttributoFoglioDiCalcolo>();
        //    ListaAttributiNonFiltrati = new ObservableCollection<AttributoFoglioDiCalcolo>();
        //    ListaAttributiAggiunti = new ObservableCollection<AttributoFoglioDiCalcolo>();
        //}
        public FoglioDiCalcoloDataView():base()
        {

        }

        //public void Init(string SezioneKey)
        //{
        //    ListaFiltrati.Clear();
        //    ListaAttributiNonFiltrati.Clear();
        //    ListaAttributiAggiunti.Clear();
        //    TextSearched = LocalizationProvider.GetString("Filtra");
        //    GetCopiedText = null;

        //    GeneraDatiPerTabella(SezioneKey);

        //    EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

        //    sezionekey = SezioneKey;

        //    fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();

        //    IOrderedEnumerable<Attributo> attributi = DataService.GetEntityTypes()[sezionekey].Attributi.Values.OrderBy(item => item.DetailViewOrder);
        //    foreach (Attributo att in attributi)
        //    {
        //        if (att.IsInternal)
        //            continue;

        //        if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
        //        {
        //            continue;
        //        }
        //        if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
        //        {
        //            if (entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
        //            {
        //                continue;
        //            }
        //        }

        //        AttributoFoglioDiCalcolo attributoFiltrato = new AttributoFoglioDiCalcolo();
        //        attributoFiltrato.Codice = att.Codice;
        //        attributoFiltrato.Etichetta = att.Etichetta;
        //        string EntityTypeKey = entitiesHelper.GetSourceAttributo(att).EntityTypeKey;
        //        string EntityType = null;
        //        if (sezionekey != EntityTypeKey)
        //        {
        //            EntityType = DataService.GetEntityType(entitiesHelper.GetSourceAttributo(att).EntityTypeKey).Name;
        //            attributoFiltrato.Sezione = EntityType;
        //        }
        //        attributoFiltrato.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
        //        attributoFiltrato.IsChecked = true;
        //        ListaFiltrati.Add(attributoFiltrato);
        //        AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
        //        attributo.Codice = att.Codice;
        //        attributo.Etichetta = att.Etichetta;
        //        if (sezionekey != EntityTypeKey)
        //        {
        //            attributo.Sezione = EntityType;
        //        }
        //        attributo.DefinizioneAttributo = entitiesHelper.GetSourceAttributo(att).DefinizioneAttributoCodice;
        //        attributo.IsChecked = true;
        //        ListaAttributiNonFiltrati.Add(attributo);
        //    }

        //    if (fogliDiCalcoloData.FoglioDiCalcolo != null)
        //    {
        //        FoglioDiCalcolo foglioDiCalcolo = fogliDiCalcoloData.FoglioDiCalcolo.Where(f => f.SezioneKey == sezionekey).FirstOrDefault();
        //        if (foglioDiCalcolo != null)
        //        {
        //            foreach (AttributoFoglioDiCalcolo FiltratoSalvato in foglioDiCalcolo.AttributiFoglioDiCalcolo)
        //            {
        //                //AttributoFoglioDiCalcolo attributoSalvato = ListaFiltrati.Where(at => at.Codice == FiltratoSalvato.Codice).FirstOrDefault();
        //                //if (attributoSalvato != null)
        //                //{
        //                //    ListaFiltrati.Where(at => at.Codice == FiltratoSalvato.Codice).FirstOrDefault().IsChecked = FiltratoSalvato.IsChecked;
        //                //}
        //                //else
        //                //{
        //                AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
        //                attributo.Etichetta = FiltratoSalvato.Etichetta;
        //                attributo.Formula = FiltratoSalvato.Formula;
        //                attributo.Note = FiltratoSalvato.Note;
        //                attributo.IsChecked = true;
        //                ListaAttributiAggiunti.Add(attributo);
        //                //}
        //            }
        //        }
        //    }
        //    IsAllChecked = true;
        //}

        //public void GeneraDatiPerTabella(string SezioneKey)
        //{
        //    if (sezionekey == BuiltInCodes.EntityType.Computo)
        //    {
        //        DataTextCompleted = LocalizationProvider.GetString("Foglio") + ": " + LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Data");
        //        DataText = LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Data");
        //        TabellaText = LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Tabella");
        //        TabellaTextCompleted = LocalizationProvider.GetString("Intervallo") + ": " + LocalizationProvider.GetString("Computo") + LocalizationProvider.GetString("Tabella");
        //        Title = "Gestione dati" + " " + LocalizationProvider.GetString("Coumputo");
        //    }
        //    if (sezionekey == BuiltInCodes.EntityType.WBS)
        //    {
        //        DataTextCompleted = LocalizationProvider.GetString("Foglio") + ": " + LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Data");
        //        DataText = LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Data");
        //        TabellaText = LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Tabella");
        //        TabellaTextCompleted = LocalizationProvider.GetString("Intervallo") + ": " + LocalizationProvider.GetString("WBSGantt") + LocalizationProvider.GetString("Tabella");
        //        Title = "Gestione dati" + " " + LocalizationProvider.GetString("WBSGantt");
        //    }
        //}

        //public void SetCheckForEachAttribute(bool v)
        //{
        //    foreach (AttributoFoglioDiCalcolo att in ListaFiltrati)
        //    {
        //        att.IsChecked = (bool)v;
        //    }
        //}
        //public bool IsIndeterminateState()
        //{
        //    if (ListaFiltrati.Any(af => af.IsChecked) && ListaFiltrati.Any(af => !af.IsChecked))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public ICommand SubmitEnterCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.SubmitEnter());
        //    }
        //}

        //void SubmitEnter()
        //{
        //    if (TextSearched == LocalizationProvider.GetString("Filtra"))
        //    {
        //        return;
        //    }
        //    ListaFiltrati = new ObservableCollection<AttributoFoglioDiCalcolo>();
        //    foreach (var att in ListaAttributiNonFiltrati)
        //    {
        //        if (string.IsNullOrEmpty(TextSearched))
        //        {
        //            AttributoFoglioDiCalcolo attributoFiltrato = new AttributoFoglioDiCalcolo();
        //            attributoFiltrato.Codice = att.Codice;
        //            attributoFiltrato.Etichetta = att.Etichetta;
        //            attributoFiltrato.Sezione = att.Sezione;
        //            attributoFiltrato.IsChecked = att.IsChecked;
        //            ListaFiltrati.Add(attributoFiltrato);
        //        }
        //        else if (att.Etichetta.ToLower().Contains(TextSearched.ToLower()))
        //        {
        //            AttributoFoglioDiCalcolo attributoFiltrato = new AttributoFoglioDiCalcolo();
        //            attributoFiltrato.Codice = att.Codice;
        //            attributoFiltrato.Etichetta = att.Etichetta;
        //            attributoFiltrato.Sezione = att.Sezione;
        //            attributoFiltrato.IsChecked = att.IsChecked;
        //            ListaFiltrati.Add(attributoFiltrato);
        //        }
        //    }
        //}

        //public ICommand AddAttributoCommand { get { return new CommandHandler(() => this.AddAttributo()); } }

        //private void AddAttributo()
        //{
        //    int contatore = 0;
        //    bool enabledAction = true;
        //    foreach (var item in ListaAttributiAggiunti)
        //    {
        //        if (string.IsNullOrEmpty(item.Etichetta))
        //        {
        //            enabledAction = false;
        //        }
        //    }

        //    if (enabledAction)
        //    {
        //        foreach (var item in ListaAttributiAggiunti)
        //        {
        //            contatore++;
        //            if (AttributoAggiuntoSelezionato != null)
        //            {
        //                if (item.Etichetta == AttributoAggiuntoSelezionato.Etichetta)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }


        //    if (enabledAction)
        //    {
        //        ListaAttributiAggiunti.Insert(contatore, new AttributoFoglioDiCalcolo());
        //        AttributoAggiuntoSelezionato = ListaAttributiAggiunti.ElementAt(contatore);
        //    }

        //}

        //public ICommand RemoveAttributoCommand { get { return new CommandHandler(() => this.DeleteAttributo()); } }

        //private void DeleteAttributo()
        //{
        //    ListaAttributiAggiunti.Remove(AttributoAggiuntoSelezionato);
        //}

        //public bool Accept()
        //{
        //    bool ExistSezione = false;

        //    if (fogliDiCalcoloData.FoglioDiCalcolo == null)
        //    {
        //        fogliDiCalcoloData.FoglioDiCalcolo = new List<Model.FoglioDiCalcolo>();
        //    }

        //    foreach (var foglioDiCalcolo in fogliDiCalcoloData.FoglioDiCalcolo)
        //    {
        //        if (foglioDiCalcolo.SezioneKey == sezionekey)
        //        {
        //            ExistSezione = true;

        //            foglioDiCalcolo.AttributiFoglioDiCalcolo.Clear();
        //            //foreach (var Filtrato in ListaFiltrati)
        //            //{
        //            //    if (Filtrato.IsChecked)
        //            //        foglioDiCalcolo.AttributiFoglioDiCalcolo.Add(Filtrato);
        //            //}
        //            foreach (var Aggiunto in ListaAttributiAggiunti)
        //            {
        //                foglioDiCalcolo.AttributiFoglioDiCalcolo.Add(Aggiunto);
        //            }
        //        }
        //    }

        //    if (!ExistSezione)
        //    {
        //        fogliDiCalcoloData.FoglioDiCalcolo.Add(new Model.FoglioDiCalcolo());
        //        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().Foglio = DataText;
        //        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().Tabella = TabellaText;
        //        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().SezioneKey = sezionekey;
        //        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiFoglioDiCalcolo = new List<AttributoFoglioDiCalcolo>();
        //        //foreach (var Filtrato in ListaFiltrati)
        //        //{
        //        //    if (Filtrato.IsChecked)
        //        //        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiFoglioDiCalcolo.Add(Filtrato);
        //        //}
        //        foreach (var Aggiunto in ListaAttributiAggiunti)
        //        {
        //            if (!string.IsNullOrEmpty(Aggiunto.Etichetta))
        //                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiFoglioDiCalcolo.Add(Aggiunto);
        //        }
        //    }



        //    DataService.SetFogliDiCalcoloData(fogliDiCalcoloData);
        //    return true;
        //}
    }
}
