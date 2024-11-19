using CommonResources;
using CommonResources.Controls;
using Commons;
using DatiGeneraliWpf;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StampeWpf.Wizard
{
    public class MyEventArgs : EventArgs
    {
        public string MyEventString { get; set; }

        public MyEventArgs(string myString)
        {
            this.MyEventString = myString;
        }
    }
    public class ReportSettingDataViewHelper
    {
        public ClientDataService DataService { get; set; } = null;
        public Dictionary<string, TreeviewItem> DictionaryAttributi { get; set; }
        public string SezioneKey { get; set; }
        public bool IsFirstInitialization { get; set; }


        public ObservableCollection<RaggruppamentiItemsView> ItemsRaggruppamenti { get; set; }
        private RaggruppamentiItemsView _SelectedItemInGroup;
        public RaggruppamentiItemsView SelectedItemInGroup
        {
            get
            {
                return _SelectedItemInGroup;
            }
            set
            {
                _SelectedItemInGroup = value;
            }
        }

        private RaggruppamentiItemsView _ForceSelectionSelectedItemInGroup;
        public RaggruppamentiItemsView ForceSelectionSelectedItemInGroup
        {
            get
            {
                return _ForceSelectionSelectedItemInGroup;
            }
            set
            {
                _ForceSelectionSelectedItemInGroup = value;
                if (_SelectedItemInGroup != null)
                    if (_ForceSelectionSelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem != null)
                        ForceSelectionGroup?.Invoke(this, new MyEventArgs(_ForceSelectionSelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem.GroupKey));
            }
        }

        public bool SostituzioneAttributoRaggruppamento { get; set; }

        public event EventHandler<MyEventArgs> ForceSelectionGroup;
        public FormatCharacterCtrl FormatCharacterWnd { get; set; }
        public ReportSettingDataViewHelper(ClientDataService dataService, string sezioneKey)
        {
            DataService = dataService;
            SezioneKey = sezioneKey;
            DictionaryAttributi = new Dictionary<string, TreeviewItem>();
            IsFirstInitialization = true;
            CreateWindowForStyle();
        }

        public Dictionary<string, Attributo> GetAttributesFoEntitySelected()
        {
            Dictionary<string, EntityType> entityTypes = DataService.GetEntityTypes();
            EntityType entityType = entityTypes[SezioneKey];
            Dictionary<string, Attributo> listattributes = entityType.Attributi;


            listattributes = listattributes.Where(x =>
            x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection &&
            x.Value.IsInternal != true).ToDictionary(i => i.Key, i => i.Value);

        //    listattributes = listattributes.Where(x =>
        //    x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection && 
        //    x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection &&
        //    x.Value.IsInternal != true).ToDictionary(i => i.Key, i => i.Value);

            return listattributes;
        }
        public ObservableCollection<TreeviewItem> GetAttributeTree()
        {
            Dictionary<string, Attributo> Attributes = GetAttributesFoEntitySelected();
            ObservableCollection<TreeviewItem> listWorkItem = new ObservableCollection<TreeviewItem>();
            listWorkItem.Add(new TreeviewItem() { Attrbuto = StampeKeys.LocalizeNessuno, AttrbutoOrigine = StampeKeys.LocalizeNessuno });
            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);
            int _Livello = 0;
            string _Padre = null;
            AddTreeViewElementRecursive(Attributes, listWorkItem, _Livello, _Padre);
            IsFirstInitialization = false;
            return listWorkItem;
        }

        private void AddTreeViewElementRecursive(Dictionary<string, Attributo> Attributes, ObservableCollection<TreeviewItem> ListTreeviewItem, int Livello, string Padre, string pathAttribute = null, Dictionary<string, string> ListaEtichetteOriginarieDiSezione = null, Dictionary<string, string> ListaCodiciOriginarieDiSezione = null, string attributoCodicePath = null)
        {
            if (attributoCodicePath == null)
                attributoCodicePath = string.Empty;

            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);

            //foreach (var Attributo in Attributes.Where(a => a.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Riferimento))
            foreach (var att in Attributes.Values)
            {
                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                {
                    var attRif = att as AttributoRiferimento;
                    var attRifGuid = Attributes.Values.FirstOrDefault(x => x.Codice == attRif.ReferenceCodiceGuid);
                    if (attRifGuid == null || attRifGuid.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        continue;
                    }
                }

                string LocalPath = null;
                string LocalAttCodicePath = null;

                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid/* || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection*/)
                {
                    if (att.EntityTypeKey == att.GuidReferenceEntityTypeKey)
                    {
                        continue;//salto gli attributi che riferiscono a stessa sezione di appartenenza (VediVoce nel computo)
                    }


                    EntityType entityType = DataService.GetEntityTypes()[att.GuidReferenceEntityTypeKey];
                    


                    //ListAttributiAnnidati = ListAttributiAnnidati.Where(x =>
                    //x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection &&
                    //x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection &&
                    //x.Value.IsInternal != true).ToDictionary(i => i.Key, i => i.Value);

                    LocalPath = pathAttribute + att.Etichetta;
                    LocalAttCodicePath = attributoCodicePath + att.Codice;
                    TreeviewItem LastTreeviewItem = new TreeviewItem();
                    LastTreeviewItem.AttrbutoOrigine = entityType.Name;
                    LastTreeviewItem.AttrbutoDestinazione = "(" + att.Etichetta + ")";
                    LastTreeviewItem.Attrbuto = att.Etichetta;
                    LastTreeviewItem.Livello = Livello;
                    LastTreeviewItem.Padre = Padre;
                    LastTreeviewItem.PropertyType = att.DefinizioneAttributoCodice;
                    LastTreeviewItem.AttrbutoCodice = att.Codice;
                    if (att.EntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        LastTreeviewItem.DivisioneCodice = entityType.Codice;
                    }
                    LastTreeviewItem.EntityType = att.GuidReferenceEntityTypeKey;
                    LastTreeviewItem.PathAttribute = LocalPath;
                    LastTreeviewItem.AttributoCodicePath = LocalAttCodicePath;
                    LastTreeviewItem.Icona = RetrieveIconForSezioneItem(att.GuidReferenceEntityTypeKey);
                    ListTreeviewItem.Add(LastTreeviewItem);

                    if (IsFirstInitialization)
                        if (!DictionaryAttributi.ContainsKey(LastTreeviewItem.AttrbutoCodice + LastTreeviewItem.AttrbutoCodiceOrigine + LastTreeviewItem.EntityType + LastTreeviewItem.Attrbuto))
                            DictionaryAttributi.Add(LastTreeviewItem.AttrbutoCodice + LastTreeviewItem.AttrbutoCodiceOrigine + LastTreeviewItem.EntityType + LastTreeviewItem.Attrbuto, LastTreeviewItem);

                    

                    Dictionary<string, string> ListaEtichetteRiferimentiOriginarieDiSezioneLocal = new Dictionary<string, string>();
                    Dictionary<string, string> ListaCodiceOrigineRiferimentiOriginarieDiSezioneLocal = new Dictionary<string, string>();
                    foreach (AttributoRiferimento AttributoRiferimento in Attributes.Where(a => a.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento).Select(o => o.Value).ToList())
                    {
                        if (AttributoRiferimento.ReferenceEntityTypeKey == entityType.GetKey())
                        {
                            if (!ListaEtichetteRiferimentiOriginarieDiSezioneLocal.ContainsKey(AttributoRiferimento.ReferenceEntityTypeKey + AttributoRiferimento.ReferenceCodiceAttributo))
                            {
                                ListaEtichetteRiferimentiOriginarieDiSezioneLocal.Add(AttributoRiferimento.ReferenceEntityTypeKey + AttributoRiferimento.ReferenceCodiceAttributo, AttributoRiferimento.Etichetta);
                            }
                        }
                    }

                    foreach (AttributoRiferimento AttributoRiferimento in Attributes.Where(a => a.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento).Select(o => o.Value).ToList())
                    {
                        if (AttributoRiferimento.ReferenceEntityTypeKey == entityType.GetKey())
                        {
                            if (att.Codice == AttributoRiferimento.ReferenceCodiceGuid)
                            {
                                ListaCodiceOrigineRiferimentiOriginarieDiSezioneLocal.Add(AttributoRiferimento.ReferenceEntityTypeKey + AttributoRiferimento.ReferenceCodiceAttributo, AttributoRiferimento.Codice);
                            }
                        }
                    }

                    //Lista attributi annidati
                    Dictionary<string, Attributo> ListAttributiAnnidati = entityType.Attributi;
                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        ListAttributiAnnidati = ListAttributiAnnidati.Where(x =>
                        x.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection &&
                        x.Value.IsInternal != true).ToDictionary(i => i.Key, i => i.Value);
                    }
                    else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        ListAttributiAnnidati = ListAttributiAnnidati.Where(x =>
                        {
                            if (x.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                                return false;

                            if (x.Value.IsInternal == true)
                                return false;

                            string pathAtt = string.Format("{0}{1}", x.Value.EntityTypeKey, x.Value.Codice);
                            if (ListaCodiceOrigineRiferimentiOriginarieDiSezioneLocal == null || !ListaCodiceOrigineRiferimentiOriginarieDiSezioneLocal.ContainsKey(pathAtt))
                                return false;

                            return true;
                        }).ToDictionary(i => i.Key, i => i.Value);

                    }


                    AddTreeViewElementRecursive(ListAttributiAnnidati, LastTreeviewItem.Items, Livello + 1, att.Etichetta, LocalPath + " > ", ListaEtichetteRiferimentiOriginarieDiSezioneLocal, ListaCodiceOrigineRiferimentiOriginarieDiSezioneLocal, LocalAttCodicePath + ReportSettingViewHelper.AttributoCodicePathSeparator);
                }
                else if (att.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    TreeviewItem TrevieeItem = new TreeviewItem();
                    TrevieeItem.AttrbutoOrigine = att.Etichetta;
                    if (ListaEtichetteOriginarieDiSezione != null)
                    {
                        if (ListaEtichetteOriginarieDiSezione.ContainsKey(att.EntityTypeKey + att.Codice))
                        {
                            TrevieeItem.AttrbutoDestinazione = "(" + ListaEtichetteOriginarieDiSezione[att.EntityTypeKey + att.Codice] + ")";
                        }
                    }
                    if (ListaCodiciOriginarieDiSezione != null)
                    {
                        if (ListaCodiciOriginarieDiSezione.ContainsKey(att.EntityTypeKey + att.Codice))
                        {
                            TrevieeItem.AttrbutoCodiceOrigine = ListaCodiciOriginarieDiSezione[att.EntityTypeKey + att.Codice];
                            //TrevieeItem.AttributoCodicePath = attributoCodicePath + Attributo.Value.Codice;
                        }
                    }
                    TrevieeItem.Attrbuto = att.Etichetta;
                    TrevieeItem.Livello = Livello;
                    TrevieeItem.Padre = Padre;
                    TrevieeItem.PropertyType = att.DefinizioneAttributoCodice;
                    TrevieeItem.AttrbutoCodice = att.Codice;
                    TrevieeItem.AttributoCodicePath = attributoCodicePath + att.Codice;
                    TrevieeItem.EntityType = att.EntityTypeKey;
                    if (att.EntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                    {
                        TrevieeItem.DivisioneCodice = att.EntityType.Codice;
                    }
                    if (!String.IsNullOrWhiteSpace(pathAttribute))
                    {
                        TrevieeItem.PathAttribute = pathAttribute + att.Etichetta;
                    }
                    else
                    {
                        TrevieeItem.PathAttribute = att.Etichetta;
                    }
                    ListTreeviewItem.Add(TrevieeItem);
                    if (IsFirstInitialization)
                        if (!DictionaryAttributi.ContainsKey(TrevieeItem.AttrbutoCodice + TrevieeItem.AttrbutoCodiceOrigine + TrevieeItem.EntityType + TrevieeItem.Attrbuto))
                            DictionaryAttributi.Add(TrevieeItem.AttrbutoCodice + TrevieeItem.AttrbutoCodiceOrigine + TrevieeItem.EntityType + TrevieeItem.Attrbuto, TrevieeItem);
                }
            }
            ListaEtichetteOriginarieDiSezione = new Dictionary<string, string>();
        }
        private string RetrieveIconForSezioneItem(string sezione)
        {
            switch (sezione)
            {
                case "ElementiItem":
                    return "\ue04d";
                case "PrezzarioItem":
                    return "\ue0c1";
                case "CapitoliItem":
                    return "\ue0c2";
                case "InfoProgettoItem":
                    return "\ue0a7";
                case "ContattiItem":
                    return "\ue053";
            }

            if (sezione.StartsWith(BuiltInCodes.EntityType.Divisione))
            {
                return "\ue04b";
            }

            return null;
        }

        public TreeviewItem RicercaAttributoInAlbero(ObservableCollection<TreeviewItem> Albero, string Attributo, string entityType, string attributoCodice, string attributoCodiceOrigine, string attributoDigicorp)
        {
            if (DictionaryAttributi.ContainsKey(attributoCodice + attributoCodiceOrigine + entityType + Attributo) && string.IsNullOrEmpty(attributoDigicorp))
                return DictionaryAttributi[attributoCodice + attributoCodiceOrigine + entityType + Attributo];

            if (string.IsNullOrEmpty(Attributo) && string.IsNullOrEmpty(entityType) && string.IsNullOrEmpty(attributoCodice) && string.IsNullOrEmpty(attributoDigicorp)) { return null; }

            if (!string.IsNullOrEmpty(Attributo) && !string.IsNullOrEmpty(entityType) && !string.IsNullOrEmpty(attributoDigicorp))
            {
                foreach (var PrimoLivello in Albero)
                {
                    if (PrimoLivello.Attrbuto == Attributo && PrimoLivello.EntityType == entityType && PrimoLivello.CodiceDigicorp == attributoDigicorp) { return PrimoLivello; }
                    foreach (var SecondoLivello in PrimoLivello.Items)
                    {
                        if (SecondoLivello.Attrbuto == Attributo && SecondoLivello.EntityType == entityType && SecondoLivello.CodiceDigicorp == attributoDigicorp) { return SecondoLivello; }
                        foreach (var TerzoLivello in SecondoLivello.Items)
                        {
                            if (TerzoLivello.Attrbuto == Attributo && TerzoLivello.EntityType == entityType && TerzoLivello.CodiceDigicorp == attributoDigicorp) { return TerzoLivello; }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(entityType) && string.IsNullOrEmpty(attributoCodice))
            {
                foreach (var PrimoLivello in Albero)
                {
                    if (PrimoLivello.Attrbuto == Attributo) { return PrimoLivello; }
                    foreach (var SecondoLivello in PrimoLivello.Items)
                    {
                        if (SecondoLivello.Attrbuto == Attributo) { return SecondoLivello; }
                        foreach (var TerzoLivello in SecondoLivello.Items)
                        {
                            if (TerzoLivello.Attrbuto == Attributo) { return TerzoLivello; }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(attributoCodice) && !string.IsNullOrEmpty(entityType))
            {
                foreach (var PrimoLivello in Albero)
                {
                    if (PrimoLivello.AttrbutoCodice == attributoCodice && PrimoLivello.EntityType == entityType) { return PrimoLivello; }
                    foreach (var SecondoLivello in PrimoLivello.Items)
                    {
                        if (SecondoLivello.AttrbutoCodice == attributoCodice && SecondoLivello.EntityType == entityType) { return SecondoLivello; }
                        foreach (var TerzoLivello in SecondoLivello.Items)
                        {
                            if (TerzoLivello.AttrbutoCodice == attributoCodice && TerzoLivello.EntityType == entityType) { return TerzoLivello; }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(Attributo) && !string.IsNullOrEmpty(entityType))
            {
                foreach (var PrimoLivello in Albero)
                {
                    if (PrimoLivello.Attrbuto == Attributo && PrimoLivello.EntityType == entityType) { return PrimoLivello; }
                    foreach (var SecondoLivello in PrimoLivello.Items)
                    {
                        if (SecondoLivello.Attrbuto == Attributo && SecondoLivello.EntityType == entityType) { return SecondoLivello; }
                        foreach (var TerzoLivello in SecondoLivello.Items)
                        {
                            if (TerzoLivello.Attrbuto == Attributo && TerzoLivello.EntityType == entityType) { return TerzoLivello; }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(attributoDigicorp))
            {
                foreach (var PrimoLivello in Albero)
                {
                    if (PrimoLivello.CodiceDigicorp == attributoDigicorp) { return PrimoLivello; }
                    foreach (var SecondoLivello in PrimoLivello.Items)
                    {
                        if (SecondoLivello.CodiceDigicorp == attributoDigicorp) { return SecondoLivello; }
                        foreach (var TerzoLivello in SecondoLivello.Items)
                        {
                            if (TerzoLivello.CodiceDigicorp == attributoDigicorp) { return TerzoLivello; }
                        }
                    }
                }
            }
            return null;
        }

        public ObservableCollection<TreeviewItem> FilterAttrivbuteListForGroups(TreeviewItem selectedTreeViewItem)
        {
            ObservableCollection<TreeviewItem> ListAttributesForSelectedGroupLocal = new ObservableCollection<TreeviewItem>();
            ObservableCollection<TreeviewItem> ListaAttributiLocale = GetAttributeTree();

            ListAttributesForSelectedGroupLocal.Add(new TreeviewItem() { Attrbuto = StampeKeys.LocalizeNessuno, AttrbutoOrigine = StampeKeys.LocalizeNessuno, DivisioneCodice = StampeKeys.ConstNessuno });

            bool DaInserire = true;

            //if (selectedTreeViewItem != null)
            //{
            if (selectedTreeViewItem?.Attrbuto != null)
            {
                // aggiungo attributi del raggruppamento selezionato
                TreeviewItem AttributoSelezionato = new TreeviewItem();
                //AttributoSelezionato = RicercaAttributoInAlbero(listAttributesForGroups, selectedTreeViewItem.Attrbuto, null, null, null, null);
                AttributoSelezionato = RicercaAttributoInAlbero(ListaAttributiLocale, selectedTreeViewItem.Attrbuto, selectedTreeViewItem.EntityType, selectedTreeViewItem.AttrbutoCodice, null, null);
                //ListAttributesForSelectedGroupLocal.Add(AttributoSelezionato);
                ListAttributesForSelectedGroupLocal.Add(CreateNewTreeviewItem(AttributoSelezionato));

                // aggiungo attributi presenti nei raggruppamenti ad eccetto di quello già inserito
                List<string> Attributi = new List<string>();

                Attributi.Clear();

                foreach (var item in ItemsRaggruppamenti)
                {
                    if (item.TextBoxItemView.AttributeSelected == selectedTreeViewItem.Attrbuto)
                    {
                        if (ListAttributesForSelectedGroupLocal.Where(a => a.Attrbuto == item.TextBoxItemView.AttributeSelected).FirstOrDefault() == null)
                        {
                            Attributi.Add(item.TextBoxItemView.AttributeSelected);
                        }
                        break;
                    }
                    Attributi.Add(item.TextBoxItemView.AttributeSelected);
                }

                DaInserire = true;

                foreach (var Attributo in Attributi)
                {
                    DaInserire = true;

                    foreach (var PrimoLivello in ListaAttributiLocale)
                    {
                        if (PrimoLivello.Attrbuto == Attributo)
                        {
                            if (DaInserire)
                            {
                                ListAttributesForSelectedGroupLocal.Add(CreateNewTreeviewItem(PrimoLivello));
                                DaInserire = false;
                            }
                        }
                        foreach (var SecondoLivello in PrimoLivello.Items)
                        {
                            if (SecondoLivello.Attrbuto == Attributo)
                            {
                                if (DaInserire)
                                {
                                    ListAttributesForSelectedGroupLocal.Add(CreateNewTreeviewItem(SecondoLivello));
                                    DaInserire = false;
                                }
                            }
                            foreach (var TerzoLivello in SecondoLivello.Items)
                            {
                                if (TerzoLivello.Attrbuto == Attributo)
                                {
                                    if (DaInserire)
                                    {
                                        ListAttributesForSelectedGroupLocal.Add(CreateNewTreeviewItem(TerzoLivello));
                                        DaInserire = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            AddFunctionToTheListOfAttribute(ListAttributesForSelectedGroupLocal);

            return ListAttributesForSelectedGroupLocal;
        }

        private TreeviewItem CreateNewTreeviewItem(TreeviewItem BaseTreeviewItem)
        {
            TreeviewItem NewTreeviewItem = new TreeviewItem();
            NewTreeviewItem.Attrbuto = BaseTreeviewItem.Attrbuto;
            NewTreeviewItem.AttrbutoCodice = BaseTreeviewItem.AttrbutoCodice;
            NewTreeviewItem.AttrbutoCodiceOrigine = BaseTreeviewItem.AttrbutoCodiceOrigine;
            NewTreeviewItem.AttributoCodicePath = BaseTreeviewItem.AttributoCodicePath;
            NewTreeviewItem.AttrbutoDestinazione = BaseTreeviewItem.AttrbutoDestinazione;
            NewTreeviewItem.AttrbutoOrigine = BaseTreeviewItem.AttrbutoOrigine;
            NewTreeviewItem.CodiceDigicorp = BaseTreeviewItem.CodiceDigicorp;
            NewTreeviewItem.DivisioneCodice = BaseTreeviewItem.DivisioneCodice;
            NewTreeviewItem.EntityType = BaseTreeviewItem.EntityType;
            NewTreeviewItem.Icona = BaseTreeviewItem.Icona;
            NewTreeviewItem.Livello = BaseTreeviewItem.Livello;
            NewTreeviewItem.Padre = BaseTreeviewItem.Padre;
            NewTreeviewItem.PropertyType = BaseTreeviewItem.PropertyType;
            NewTreeviewItem.Items = new ObservableCollection<TreeviewItem>();
            if (BaseTreeviewItem.Items.Count() > 0)
            {
                foreach (var Item in BaseTreeviewItem.Items)
                {
                    NewTreeviewItem.Items.Add(CreateNewTreeviewItem(Item));
                }
            }
            return NewTreeviewItem;
        }

        public void AddFunctionToTheListOfAttribute(ObservableCollection<TreeviewItem> listAttributes)
        {
            listAttributes.Add(new TreeviewItem() { Attrbuto = LocalizationProvider.GetString("Funzione"), AttrbutoOrigine = LocalizationProvider.GetString("Funzione") });
            listAttributes.LastOrDefault().Items = new ObservableCollection<TreeviewItem>() { new TreeviewItem() { Attrbuto = StampeKeys.LocalizeSommaWizard, AttrbutoOrigine = StampeKeys.LocalizeSommaWizard, CodiceDigicorp = StampeKeys.ConstSommaWizard }, new TreeviewItem() { Attrbuto = StampeKeys.LocalizeSommaStrutturaWizard, AttrbutoOrigine = StampeKeys.LocalizeSommaStrutturaWizard, CodiceDigicorp = StampeKeys.ConstSommaStrutturaWizard }, new TreeviewItem() { Attrbuto = StampeKeys.LocalizeContaWizard, AttrbutoOrigine = StampeKeys.LocalizeContaWizard, CodiceDigicorp = StampeKeys.ConstContaWizard } };
        }

        public void CreateWindowForStyle()
        {
            FormatCharacterWnd = new FormatCharacterCtrl();
            FormatCharacterWnd.Title = CommonResources.LocalizationProvider.GetString("Impostazioni");
            FormatCharacterWnd.Height = 640;
            FormatCharacterWnd.Width = 500;
            FormatCharacterWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }


    }
}
