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
using CommonResources;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using _3DModelExchange;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Controls;
using DevExpress.Mvvm.UI;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;

namespace MasterDetailView
{
    /// <summary>
    /// Rappresenta tutti i tabs (Attributi e Riferimenti)
    /// </summary>
    public class AttributiSettingsView : NotificationBase
    {
        public ClientDataService DataService { get; set; }
        public EntityType EntityType { get;set;}//clone
        public IMainOperation MainOperation { get; set; }
        public IEntityWindowService WindowService { get; set; }

        private ObservableCollection<AttributoSettingsView> _attributiFirstLevelItems = new ObservableCollection<AttributoSettingsView  >();
        public ObservableCollection<AttributoSettingsView> AttributiFirstLevelItems
        {
            get { return _attributiFirstLevelItems; }
            set { _attributiFirstLevelItems = value; }
        }

        private ObservableCollection<AttributoSettingsView> _attributiItems = new ObservableCollection<AttributoSettingsView>();
        public ObservableCollection<AttributoSettingsView> AttributiItems
        {
            get { return _attributiItems; }
            set { _attributiItems = value; }
        }


        /// <summary>
        /// Riga corrente
        /// </summary>
        AttributoSettingsView _currentAttributoSettings = null;
        public AttributoSettingsView CurrentAttributoSettings
        {
            get { return _currentAttributoSettings; }
            set
            {
                if (SetProperty(ref _currentAttributoSettings, value))
                {
                    if (_currentAttributoSettings != null)
                        _currentAttributoSettings.Load();

                    RaisePropertyChanged(GetPropertyName(() => IsAddAttributoChildEnabled));
                }
            }
        }

        /// <summary>
        /// indice dell'attributo guid di apparteneza (>= solo se l'attributo corrente è di rifierimento
        /// </summary>
        public int AttributoGuidRecordIndex { get; set; } = -1;

        protected Dictionary<string, DefinizioneAttributo> DefinizioniAttributo;//ref
        public Dictionary<string, EntityType> EntityTypes;//ref

        protected List<string> _definizioniAttributoCodice = new List<string>();

        public ObservableCollection<string> DefinizioniAttributoLoc { get; set; } = new ObservableCollection<string>();

        HashSet<string> _codiceAttributiOnInit = new HashSet<string>();

        HashSet<string> _attributiNewCodice = new HashSet<string>();
        public bool IsAttributoNew(string codice) { return _attributiNewCodice.Contains(codice); }

        public MessageBarView MessageBarView { get; set; } = new MessageBarView();

        EntitiesHelper _entitiesHelper = null;
        public EntitiesHelper EntitiesHelper { get => _entitiesHelper; }

        /// <summary>
        /// Operazioni per il raggruppamento in griglia
        /// </summary>
        public Dictionary<ValoreOperationType, string> GroupOperationTypes { get; set; } = new Dictionary<ValoreOperationType, string>();

        public event EventHandler RefreshView;
        public void OnRefreshView(EventArgs e)
        {
            RefreshView?.Invoke(this, e);
        }

        public bool AttributiTabUpdatePending { get; set; } = false;

        public void Init(ClientDataService clientDataService, IMainOperation mainOperation, IEntityWindowService windowService, string entityTypeKey, string currentCodiceAtt)
        {
            DataService = clientDataService;
            MainOperation = mainOperation;
            EntityTypes = clientDataService.GetEntityTypes();
            WindowService = windowService;

            _entitiesHelper = new EntitiesHelper(DataService);

            EntityType = EntityTypes[entityTypeKey].Clone();
            Load();

            AttributoSettingsView currAtt = AttributiItems.FirstOrDefault(item => item.GetCodice() == currentCodiceAtt);
            if (currAtt != null)
            {
                CurrentAttributoSettings = currAtt;
                OnRefreshView(new EventArgs());
            }
        }


        public virtual void Load()
        {
            LoadGroupOperationTypes();
            LoadDefinizioneAttributi();
            LoadAttributiItems();
            //LoadRiferimentiItems();
            LoadEntityTypesName();
            

            _attributiNewCodice.Clear();
            _codiceAttributiOnInit = new HashSet<string>(EntityType.Attributi.Select(item => item.Key));

            UpdateUI();
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsAcceptButtonEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsAdvancedMode));
        }

        protected virtual void LoadDefinizioneAttributi()
        {
            //combo definizione attributo
            DefinizioniAttributo = DataService.GetDefinizioniAttributo();
            _definizioniAttributoCodice.Clear();
            DefinizioniAttributoLoc.Clear();

            _definizioniAttributoCodice = DefinizioniAttributo.Values.Where(item => item.AllowAttributoCustom).Select(item => item.Codice).ToList();

            if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.Riferimento))
                _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.Riferimento);

            foreach (string codice in _definizioniAttributoCodice)
            {
                string comboItem = string.Empty;
                //if (codice == BuiltInCodes.DefinizioneAttributo.Guid)
                //    comboItem = BuiltInCodes.DefinizioneAttributo.Riferimento;
                //else
                //comboItem = LocalizationProvider.GetString(codice);
                comboItem = GetDefinizioneAttributoLocalizedName(codice);


                if (comboItem != null)
                    DefinizioniAttributoLoc.Add(comboItem);
            }
        }

        protected void LoadGroupOperationTypes()
        {
            GroupOperationTypes.Clear();
            GroupOperationTypes.Add(ValoreOperationType.Equivalent, LocalizationProvider.GetString("VisualizzaSeUguale"));
            GroupOperationTypes.Add(ValoreOperationType.Sum, LocalizationProvider.GetString("Somma"));
        }

        public string GetDefinizioneCodiceByIndex(int index)
        {
            return _definizioniAttributoCodice[index];
        }

        public bool Accept()
        {

            bool isValid = Validate();
            if (isValid)
            {
                SetOrder();
                SetAttributiMasterCode();
                EntitiesError entsError = new EntitiesError();
                if (DataService.SetEntityType(EntityType, true, false))
                {
                    ModelAction action = new ModelAction() { ActionName = ActionName.ENTITYTYPE_SET, EntityTypeKey = EntityType.GetKey() };
                    MainOperation.AddToModelActionStack(action);
                }
            }

            return isValid;
        }

        bool Validate()
        {
            List<string> etichetteAttOrdered = EntityType.Attributi/*.Where(item => item.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Guid)*/.Select(item => item.Value.Etichetta).OrderBy(item => item).ToList();

            for (int i=0; i< etichetteAttOrdered.Count-1; i++)
            {
                if (etichetteAttOrdered[i] == etichetteAttOrdered[i+1])
                {
                    string str1 = LocalizationProvider.GetString("Attributo1");
                    string str2 = LocalizationProvider.GetString("AttributiConStessoNome");
                    string msg = string.Format("{0}: {1}. {2}", str1, etichetteAttOrdered[i], str2);
                    MessageBarView.Show(msg);
                    return false;
                }

                if (etichetteAttOrdered[i].Contains("\\"))
                {
                    string str1 = LocalizationProvider.GetString("Attributo1");
                    string str2 = LocalizationProvider.GetString("Carattere non consentito nel nome dell'attributo");
                    string msg = string.Format("{0}: {1}. {2}: {3}", str1, etichetteAttOrdered[i], str2, "\\");
                    MessageBarView.Show(msg);
                    return false;
                }

            }

            return true;
        }

        void SetAttributiMasterCode()
        {


            if (EntityType.MasterType != MasterType.Grid)
                return;

            //aggiungo l'attributo al master (tranne quelli di tipo Guid e collection che non andranno mai in griglia)
            List<string> attMasterCodes = new List<string>(/*EntityType.AttributiMasterCodes*/);

            foreach (AttributoSettingsView attSettingsView in AttributiItems)
            {

                string attCode = attSettingsView.GetCodice();
                string defAttCodice = attSettingsView.GetSourceDefinizioneAttributoCodice();

                if (!attMasterCodes.Contains(attSettingsView.GetCodice()))
                {
                    if (defAttCodice != BuiltInCodes.DefinizioneAttributo.Guid
                        && defAttCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection
                        && defAttCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection)
                        attMasterCodes.Add(attCode);
                }
            }

            EntityType.AttributiMasterCodes = attMasterCodes;
        }

        /// <summary>
        /// Imposto l'ordine degli attributi (0,...,n) tenendo assieme tutti gli attributi dello stesso gruppo
        /// </summary>
        private void SetOrder()
        {

            //imposto l'ordine degli attributi (0,...,n) tenendo assieme tutti gli attributi dello stesso gruppo
            //0: group, 1: last index
            HashSet<string> groupsUnique = new HashSet<string>(AttributiItems.Select(item => item.GroupName));
            Dictionary<string, int> groups = groupsUnique.ToDictionary(item => item, item => -1);

            int newGroupItemIndex = 0;
            string groupPrec = string.Empty;
            foreach (AttributoSettingsView att in AttributiItems)
            {
                if (groups[att.GroupName] == -1)
                {
                    newGroupItemIndex += AttributiItems.Where(item => item.GroupName == groupPrec).Count();
                    groups[att.GroupName] = newGroupItemIndex;
                }
                else
                    groups[att.GroupName]++;

                att.DetailViewOrder = groups[att.GroupName];
                groupPrec = att.GroupName;
            }
        }

        //public int LastAttributiItemsSelectedIndex { get; set; } = -1;
        bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    //if (_isSelected)
                    //{
                    //    SetOrder();
                    //    LoadAttributiItems();
                    //}

                    //OnRefreshView(new EventArgs());
                }

            }
        }

        #region Attributi
        private void LoadAttributiItems()
        {
            AttributiItems.Clear();
            AttributiFirstLevelItems.Clear();
            IOrderedEnumerable<Attributo> attributi = EntityType.Attributi.Values.OrderBy(item => item.DetailViewOrder);

            List<AttributoSettingsView> attributiSettingsView = new List<AttributoSettingsView>();
            //
            foreach (Attributo att in attributi.Where(item => !(item is AttributoRiferimento)))
            {
                if (att.IsInternal)
                    continue;

                AttributoSettingsView attSettingsView = NewAttributoSettingsView(att);

                if (attSettingsView != null)
                {
                    attributiSettingsView.Add(attSettingsView);
                    AttributiFirstLevelItems.Add(attSettingsView);
                }
            }

            foreach (AttributoRiferimento attRif in attributi.Where(item => item is AttributoRiferimento))
            {
                if (attRif.IsInternal)
                    continue;

                AttributoSettingsRiferimentoView attSettingsView = NewAttributoSettingsView(attRif) as AttributoSettingsRiferimentoView;

                if (IsAttributoRiferimentoGuidCollection(attRif))
                {
                    AttributoSettingsGuidCollectionView attGuidColl = AttributiFirstLevelItems.FirstOrDefault(item => item.GetCodice() == attRif.ReferenceCodiceGuid) as AttributoSettingsGuidCollectionView;
                    if (attSettingsView != null && attGuidColl != null)
                    {
                        attributiSettingsView.Add(attSettingsView);
                        attGuidColl.AttributoRiferimentoItems.Add(attSettingsView);
                    }
                }
                else //AttributoSettingsGuidRiferimentoView
                {
                    AttributoSettingsGuidView attGuid = AttributiFirstLevelItems.FirstOrDefault(item => item.GetCodice() == attRif.ReferenceCodiceGuid) as AttributoSettingsGuidView;
                    if (attSettingsView != null && attGuid != null)
                    {
                        attributiSettingsView.Add(attSettingsView);
                        attGuid.AttributoRiferimentoItems.Add(attSettingsView);
                    }
                }
            }





            AttributiItems = new ObservableCollection<AttributoSettingsView>(attributiSettingsView.OrderBy(item => item.DetailViewOrder));
            RaisePropertyChanged(GetPropertyName(() => AttributiItems));
        }

        public void SelectAttributoView(AttributoSettingsView attView)
        {
            AttributoGuidRecordIndex = -1;
            if (attView is AttributoSettingsRiferimentoView)
            {
                AttributoSettingsRiferimentoView currAttRif = attView as AttributoSettingsRiferimentoView;
                AttributoSettingsView currAttGuid = AttributiFirstLevelItems.FirstOrDefault(item => item.GetCodice() == currAttRif.GetReferenceCodiceGuid());
                AttributoGuidRecordIndex = AttributiFirstLevelItems.IndexOf(currAttGuid);
            }
        }

        public ICommand AddAttributoCommand { get { return new CommandHandler(() => this.AddAttributo()); } }
        void AddAttributo()
        {
            string codiceAttributo = null;
            if (MainOperation.IsAdvancedMode())
            {
                IWindowService windowService = MainOperation.GetWindowService();
                codiceAttributo = GetNextNewCodiceAttributo();
                windowService.CodiceAttributoWindow(ref codiceAttributo);

                if (EntityType.Attributi.ContainsKey(codiceAttributo))
                {
                    MessageBarView.Show("Impossibile aggingere l'attributo. Codice già esistente");
                    return;
                }
            }

            Attributo newAtt = AddEntityTypeAttributo(null, codiceAttributo);
            AttributoSettingsView newAttSettings = NewAttributoSettingsView(newAtt);
            _attributiNewCodice.Add(newAtt.Codice);
            int currentIndex = -1;

            if (CurrentAttributoSettings is AttributoSettingsRiferimentoView)
            {
                //Aggiungo la riga di settings dell'attributo nella griglia al secondo livello

                AttributoSettingsRiferimentoView currentAttributoSettingsRif = CurrentAttributoSettings as AttributoSettingsRiferimentoView;
                string attGuidCodice = currentAttributoSettingsRif.GetReferenceCodiceGuid();

                AttributoSettingsView attGuidView = AttributiFirstLevelItems.FirstOrDefault(item => item.GetCodice() == attGuidCodice) as AttributoSettingsView;
                AttributoSettingsRiferimentoView newAttSettingsRif = newAttSettings as AttributoSettingsRiferimentoView;

                currentIndex = attGuidView.AttributoRiferimentoItems.IndexOf(currentAttributoSettingsRif);
                if (currentIndex >= 0 && currentIndex < attGuidView.AttributoRiferimentoItems.Count - 1)
                    attGuidView.AttributoRiferimentoItems.Insert(currentIndex + 1, newAttSettingsRif);
                else
                    attGuidView.AttributoRiferimentoItems.Add(newAttSettingsRif);

            }
            else
            {
                //Aggiungo la riga di settings dell'attributo nella griglia AttributiFirstLevelItems
                currentIndex = AttributiFirstLevelItems.IndexOf(CurrentAttributoSettings);
                if (currentIndex >= 0 && currentIndex < AttributiFirstLevelItems.Count - 1)
                    AttributiFirstLevelItems.Insert(currentIndex + 1, newAttSettings);
                else
                    AttributiFirstLevelItems.Add(newAttSettings);
            }

            //Aggiungo la riga di settings dell'attributo nella griglia AttributiItems
            currentIndex = AttributiItems.IndexOf(CurrentAttributoSettings);
            if (CurrentAttributoSettings is AttributoSettingsGuidView)
            {
                //Scopo: far in modo che venga aggiunto nel posto giusto nel tab ordinamento
                AttributoSettingsGuidView attributoSettingsGuidView = CurrentAttributoSettings as AttributoSettingsGuidView;
                currentIndex += attributoSettingsGuidView.AttributoRiferimentoItems.Count();
            }

            if (currentIndex >= 0 && currentIndex < AttributiItems.Count - 1)
                AttributiItems.Insert(currentIndex + 1, newAttSettings);
            else
                AttributiItems.Add(newAttSettings);

            CurrentAttributoSettings = newAttSettings;

            OnRefreshView(new EventArgs());
        }

        public ICommand AddAttributoChildCommand { get { return new CommandHandler(() => this.AddAttributoChild()); } }
        void AddAttributoChild()
        {
            string codiceAttributo = null;
            if (MainOperation.IsAdvancedMode())
            {
                IWindowService windowService = MainOperation.GetWindowService();
                codiceAttributo = GetNextNewCodiceAttributo();
                windowService.CodiceAttributoWindow(ref codiceAttributo);
            }


           

            AttributoRiferimento newAttRif = AddEntityTypeAttributo(BuiltInCodes.DefinizioneAttributo.Riferimento, codiceAttributo) as AttributoRiferimento;
            AttributoSettingsView newAttSettings = NewAttributoSettingsView(newAttRif);
            _attributiNewCodice.Add(newAttRif.Codice);

            //Aggiungo la riga di settings dell'attributo nella griglia al secondo livello in fondo
            AttributoSettingsRiferimentoView newAttSettingsRif = newAttSettings as AttributoSettingsRiferimentoView;
            newAttSettingsRif.NomeAttributo = newAttSettingsRif.Attributi[0];
            AttributoSettingsRiferimentoView target = CurrentAttributoSettings.AttributoRiferimentoItems.LastOrDefault();
            CurrentAttributoSettings.AttributoRiferimentoItems.Add(newAttSettingsRif);

            //Aggiungo la riga di settings dell'attributo nella griglia AttributiItems
            int index = AttributiItems.IndexOf(target);
            if (index >= 0 && index < AttributiItems.Count - 1)
                AttributiItems.Insert(index + 1, newAttSettings);
            else
                AttributiItems.Add(newAttSettings);

            CurrentAttributoSettings.UpdateUI();
            CurrentAttributoSettings = newAttSettings;


            OnRefreshView(new EventArgs());
        }

        public bool IsAddAttributoChildEnabled
        {
            get
            {
                if (CurrentAttributoSettings != null && CurrentAttributoSettings is AttributoSettingsGuidView)
                    return true;

                if (CurrentAttributoSettings != null && CurrentAttributoSettings is AttributoSettingsGuidCollectionView)
                    return true;

                return false;
            }
        }

        public bool IsAttributoRiferimentoGuidCollection(Attributo att)
        {
            AttributoRiferimento attRif = att as AttributoRiferimento;
            if (attRif == null)
                return false;
            

            Attributo attGuid = EntityType.Attributi[attRif.ReferenceCodiceGuid];
            if (attGuid.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                return true;

            return false;
        }


        public AttributoSettingsView NewAttributoSettingsView(Attributo newAtt)
        {
            AttributoSettingsView newAttSettings = null;

            if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                newAttSettings = new AttributoSettingsTestoView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                if (IsAttributoRiferimentoGuidCollection(newAtt))
                    newAttSettings = new AttributoSettingsGuidCollectionRiferimentoView(this, newAtt);
                else
                    newAttSettings = new AttributoSettingsGuidRiferimentoView(this, newAtt);
            }
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                newAttSettings = new AttributoSettingsTestoRtfView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data)
                newAttSettings = new AttributoSettingsDataView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                newAttSettings = new AttributoSettingsRealeView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                newAttSettings = new AttributoSettingsContabilitaView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                newAttSettings = new AttributoSettingsGuidView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                newAttSettings = new AttributoSettingsTestoCollectionView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                newAttSettings = new AttributoSettingsGuidCollectionView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
                newAttSettings = new AttributoSettingsElencoView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
                newAttSettings = new AttributoSettingsBooleanoView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
                newAttSettings = new AttributoSettingsColoreView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.FormatoNumero)
                newAttSettings = new AttributoSettingsFormatoNumeroView(this, newAtt);
            else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)
                newAttSettings = new AttributoSettingsVariabileView(this, newAtt);
            //else if (newAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Percentuale)
            //    newAttSettings = new AttributoSettingsPercentualeView(this, newAtt);



            return newAttSettings;
        }

        internal Attributo AddEntityTypeAttributo(string definizioneAttributoCodice, string codiceAttributo)
        {
            //Aggiungo l'attributo alla lista attributi dell'entità

            Attributo newAtt = null;
            if (definizioneAttributoCodice != null && definizioneAttributoCodice.Any())
            {
                string codiceNuovo = codiceAttributo;
                if (codiceNuovo == null)
                    codiceNuovo = CreateNewCodiceAttributo();

                if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    newAtt = new AttributoRiferimento(DefinizioniAttributo[definizioneAttributoCodice], EntityType, codiceNuovo);
                else
                    newAtt = new Attributo(DefinizioniAttributo[definizioneAttributoCodice], EntityType, codiceNuovo);

                InitAttributo(newAtt, definizioneAttributoCodice);
                EntityType.Attributi.Add(newAtt.Codice, newAtt);

            }
            else if (CurrentAttributoSettings != null)
            {
                definizioneAttributoCodice = CurrentAttributoSettings.GetDefinizioneAttributoCodice();
                string codiceNuovo = codiceAttributo;
                if (codiceNuovo == null)
                    codiceNuovo = CreateNewCodiceAttributo();
                //newAtt = CurrentAttributoSettings.CloneAttributo(codiceNuovo, LocalizationProvider.GetString("Nuovo"));
                if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    newAtt = new AttributoRiferimento(DefinizioniAttributo[definizioneAttributoCodice], EntityType, codiceNuovo);
                else
                    newAtt = new Attributo(DefinizioniAttributo[definizioneAttributoCodice], EntityType, codiceNuovo);

                InitAttributo(newAtt, definizioneAttributoCodice);
                newAtt.GroupName = CurrentAttributoSettings.GroupName;
                newAtt.Etichetta = CreateDefaultAttributoEtichetta(newAtt);

                EntityType.Attributi.Add(newAtt.Codice, newAtt);

            }
            else
            {
                definizioneAttributoCodice = BuiltInCodes.DefinizioneAttributo.Testo;
                string codiceNuovo = codiceAttributo;
                if (codiceNuovo == null)
                    codiceNuovo = CreateNewCodiceAttributo();
                newAtt = new Attributo(DefinizioniAttributo[definizioneAttributoCodice], EntityType, codiceNuovo);
                InitAttributo(newAtt, definizioneAttributoCodice);
                EntityType.Attributi.Add(newAtt.Codice, newAtt);
            }

            ////aggiungo l'attributo al master (tranne quelli di tipo Guid e collection che non andranno mai in griglia)
            //if (newAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Guid
            //    && newAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection
            //    && newAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection)
            //    EntityType.AttributiMasterCodes.Add(newAtt.Codice);

            return newAtt;
        }

        public ICommand RemoveAttributoCommand { get { return new CommandHandler(() => this.RemoveAttributo()); } }
        void RemoveAttributo()
        {
            if (CurrentAttributoSettings == null)
                return;

            if (!MainOperation.IsAdvancedMode())
            {
                if (CurrentAttributoSettings.IsBuiltIn)
                {
                    MessageBarView.Show(LocalizationProvider.GetString("EliminazioneAttributoNonConsentita"));
                    return;
                }
            }

            //Rimuovo l'attributo dalla lista di attributi
            string codiceAttributo = CurrentAttributoSettings.GetCodice();
            if (EntityType.Attributi.ContainsKey(codiceAttributo))
                EntityType.Attributi.Remove(codiceAttributo);

            //rimuovo l'attributo dal master
            EntityType.AttributiMasterCodes.Remove(codiceAttributo);


            List<AttributoSettingsView> attsToRemove = new List<AttributoSettingsView>();
            attsToRemove.Add(CurrentAttributoSettings);
            int index = -1;

            //Rimuovo l'attributo dalla griglia
            if (CurrentAttributoSettings is AttributoSettingsRiferimentoView)
            {
                //Elimino dalla griglia al secondo livello AttributoRiferimentoItems
                AttributoSettingsRiferimentoView currentAttributoSettingsRif = attsToRemove[0] as AttributoSettingsRiferimentoView;
                string attGuidCodice = currentAttributoSettingsRif.GetReferenceCodiceGuid();

                AttributoSettingsView attGuidView = AttributiFirstLevelItems.FirstOrDefault(item => item.GetCodice() == attGuidCodice);

                index = attGuidView.AttributoRiferimentoItems.IndexOf(currentAttributoSettingsRif);

                if (0 <= index && index < attGuidView.AttributoRiferimentoItems.Count)
                    attGuidView.AttributoRiferimentoItems.RemoveAt(index);

                //setto in corrente
                if (index < attGuidView.AttributoRiferimentoItems.Count)
                    CurrentAttributoSettings = attGuidView.AttributoRiferimentoItems[index];
                else if (index == attGuidView.AttributoRiferimentoItems.Count && index > 0)
                    CurrentAttributoSettings = attGuidView.AttributoRiferimentoItems[index - 1];
                else
                    CurrentAttributoSettings = null;

                attGuidView.UpdateUI();

            }
            else
            {
                //Elimino dalla griglia AttributiFirstLevelItems (primo livello)
                index = AttributiFirstLevelItems.IndexOf(attsToRemove[0]);
                //if (index < AttributiFirstLevelItems.Count - 1)
                //    CurrentAttributoSettings = AttributiFirstLevelItems[index + 1];
                //else if (index - 1 < AttributiFirstLevelItems.Count)
                //    CurrentAttributoSettings = AttributiFirstLevelItems[index - 1];
                //else
                //    CurrentAttributoSettings = null;

                if (attsToRemove[0] is AttributoSettingsGuidView || attsToRemove[0] is AttributoSettingsGuidCollectionView)
                {
                    AttributoSettingsView attributoSettingsView = attsToRemove[0];
                    //AttributoSettingsGuidView attGuidToRemove = attsToRemove[0] as AttributoSettingsGuidView;

                    if (attsToRemove[0].IsBuiltIn)
                    {
                        MessageBarView.Show(LocalizationProvider.GetString("EliminazioneRiferimentoNonConsentita"));
                        return;
                    }

                    RemoveGuid(attsToRemove[0].GetCodice());
                    attsToRemove.AddRange(attributoSettingsView.AttributoRiferimentoItems);

                }

                if (0 <= index && index < AttributiFirstLevelItems.Count)
                    AttributiFirstLevelItems.RemoveAt(index);

                //setto in corrente
                if (index < AttributiFirstLevelItems.Count )
                    CurrentAttributoSettings = AttributiFirstLevelItems[index];
                else if (index == AttributiFirstLevelItems.Count && index > 0)
                    CurrentAttributoSettings = AttributiFirstLevelItems[index - 1];
                else
                    CurrentAttributoSettings = null;
            }

            //Elimino dalla griglia AttributiItems (ordinamento)
            foreach (AttributoSettingsView attView in attsToRemove)
            {
                if (AttributiItems.Contains(attView))
                    AttributiItems.Remove(attView);
            }

            //index = AttributiItems.IndexOf(attToRemove);
            //if (0 <= index && index < AttributiItems.Count)
            //    AttributiItems.RemoveAt(index);

            OnRefreshView(new EventArgs());
        }

        internal void InitAttributo(Attributo att, string definizioneAttributoCodice)
        {
            AttributoSettingsView target = CurrentAttributoSettings;
            if (target == null)
                target = AttributiFirstLevelItems.LastOrDefault();

            att.AllowMasterGrouping = true;
            att.AllowReplaceInText = false;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            //att.IsRiferimentiVisible = true;
            att.DetailViewOrder = att.DetailViewOrder>=0? att.DetailViewOrder : AttributiItems.IndexOf(target) + 1;
            
            att.GroupName = att.GroupName.Any()? att.GroupName : (target != null?target.GroupName : LocalizationProvider.GetString("Gruppo"));
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.IsBuiltIn = false;
            att.IsValoreLockedByDefault = false;
            att.IsValoreReadOnly = false;
            att.IsVisible = true;

            if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
            {
                att.ValoreDefault = new ValoreTesto();
            }
            if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF )/*"TestoRtf"*/
            {
                att.ValoreDefault = new ValoreTestoRtf();
                att.AllowValoriUnivoci = false;
                att.AllowMasterGrouping = false;
                att.AllowReplaceInText = true;
                att.AllowSort = false;
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data)
            {
                att.ValoreDefault = new ValoreData();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
            {
                att.ValoreDefault = new ValoreReale();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
            {
                att.ValoreDefault = new ValoreContabilita();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
            {
                att.ValoreDefault = new ValoreColore();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
            {
                att.ValoreDefault = new ValoreGuid();
                //att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario;
                if (EntityTypesNameLoc.Count > 0)
                    att.GuidReferenceEntityTypeKey =(EntityTypesNameLoc.GetItemAt(0) as RiferimentiComboItem).Key;
                att.AllowSort = false;
                att.AllowMasterGrouping = false;
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;

                if (target is AttributoSettingsGuidRiferimentoView)
                {
                    AttributoSettingsGuidRiferimentoView targetAttRifView = target as AttributoSettingsGuidRiferimentoView;
                    attRif.ReferenceCodiceGuid = targetAttRifView.GetReferenceCodiceGuid();
                    attRif.ReferenceEntityTypeKey = targetAttRifView.GetReferenceEntityTypeKey();

                    //attRif.ReferenceCodiceAttributo = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Where(item => !item.Value.IsInternal).First().Value.Codice;
                    Attributo att1 = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Values
                                       .Where(item => targetAttRifView.IsSourceAttributoAllowed(item))
                                       .FirstOrDefault();
                    attRif.ReferenceCodiceAttributo = att1.Codice;

                    targetAttRifView.UpdateAttributo(attRif);

                }
                else if (target is AttributoSettingsGuidCollectionRiferimentoView)
                {
                    AttributoSettingsGuidCollectionRiferimentoView targetAttRifView = target as AttributoSettingsGuidCollectionRiferimentoView;
                    attRif.ReferenceCodiceGuid = targetAttRifView.GetReferenceCodiceGuid();
                    attRif.ReferenceEntityTypeKey = targetAttRifView.GetReferenceEntityTypeKey();

                    //attRif.ReferenceCodiceAttributo = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Where(item => !item.Value.IsInternal).First().Value.Codice;
                    Attributo att1 = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Values
                                       .Where(item => targetAttRifView.IsSourceAttributoAllowed(item))
                                       .FirstOrDefault();

                    attRif.ReferenceCodiceAttributo = att1.Codice;

                    targetAttRifView.UpdateAttributo(attRif);

                }
                else if (target is AttributoSettingsGuidView)
                {
                    AttributoSettingsGuidView targetAttGuidView = target as AttributoSettingsGuidView;
                    attRif.ReferenceCodiceGuid = targetAttGuidView.GetCodice();
                    attRif.ReferenceEntityTypeKey = targetAttGuidView.GuidReferenceEntityTypeKey;

                    //attRif.ReferenceCodiceAttributo = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Where(item => !item.Value.IsInternal).First().Value.Codice;
                    Attributo att1 = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Values
                           .Where(item => AttributoSettingsGuidRiferimentoView.IsSourceAttributoAllowedS(item))
                           .FirstOrDefault();
                    attRif.ReferenceCodiceAttributo = att1.Codice;
                }
                else if (target is AttributoSettingsGuidCollectionView)
                {
                    AttributoSettingsGuidCollectionView targetAttGuidView = target as AttributoSettingsGuidCollectionView;
                    attRif.ReferenceCodiceGuid = targetAttGuidView.GetCodice();
                    attRif.ReferenceEntityTypeKey = targetAttGuidView.GuidReferenceEntityTypeKey;

                    //attRif.ReferenceCodiceAttributo = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.First().Value.Codice;
                    Attributo att1 = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.Values
                                       .Where(item => AttributoSettingsGuidCollectionRiferimentoView.IsSourceAttributoAllowedS(item))
                                       .FirstOrDefault();
                    attRif.ReferenceCodiceAttributo = att1.Codice;
                }

                att.ValoreDefault = new ValoreTesto();//testo è il predefinito per il riferimento

                //EntitiesHelper entsHelper = new EntitiesHelper(DataService);
                //Attributo sourceAtt = entsHelper.GetSourceAttributo(attRif);
                //if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid ||
                //    sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection ||
                //    sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                //{
                //    att.AllowSort = false;
                //    att.AllowMasterGrouping = false;
                //}

            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
            {
                att.ValoreDefault = new ValoreTestoCollection();
                att.AllowSort = false;
                att.AllowMasterGrouping = false;
                att.Height = 60;

            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {
                att.ValoreDefault = new ValoreGuidCollection();
                if (EntityTypesNameLoc.Count > 0)
                    att.GuidReferenceEntityTypeKey = (EntityTypesNameLoc.GetItemAt(0) as RiferimentiComboItem).Key;
                att.AllowSort = false;
                att.AllowMasterGrouping = false;
                att.Height = 60;

            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
            {
                att.ValoreDefault = new ValoreElenco();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
            {
                att.ValoreDefault = new ValoreColore();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
            {
                att.ValoreDefault = new ValoreBooleano();
            }
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.FormatoNumero)
            {
                att.ValoreDefault = new ValoreFormatoNumero();
            }
            //else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Percentuale)
            //{
            //    att.ValoreDefault = new ValorePercentuale();
            //    att.ValoreFormat = "0.00%";
            //}

            att.Etichetta = att.Etichetta.Any() ? att.Etichetta : CreateDefaultAttributoEtichetta(att);

            att.ResolveReferences(EntityTypes, DefinizioniAttributo);
        }

        internal string GetNextNewCodiceAttributo()
        {
            string candidate = null;

            if (EntityType.UserNewAttributoCodice > 10000000)
                EntityType.UserNewAttributoCodice = 0;

            int i = EntityType.UserNewAttributoCodice;
            while (i < 1000)
            {
                candidate = i.ToString();
                i++;

                if (!_codiceAttributiOnInit.Contains(candidate))
                    break;
            }

            if (i > EntityType.UserNewAttributoCodice)
                EntityType.UserNewAttributoCodice = i;

            return candidate;
        }

        internal string CreateNewCodiceAttributo()
        {
            string candidate = GetNextNewCodiceAttributo();

            _codiceAttributiOnInit.Add(candidate);
            return candidate;
        }

        internal void UpdateCurrentAttributoSettings()
        {
            //_currentAttributoSettings = CurrentAttributoSettingsDetail;
            RaisePropertyChanged(GetPropertyName(() => CurrentAttributoSettings));
        }

        internal void SetAttributoNew(string attCodice)
        {
            _attributiNewCodice.Add(attCodice);
        }
        #endregion Attributi


        protected List<string> _entityTypesKeyOnInit = new List<string>();
        public ListCollectionView EntityTypesNameLoc { get; set; } = null;
        
        protected void LoadEntityTypesName()
        {
            List<RiferimentiComboItem> items = new List<RiferimentiComboItem>();

            //combo tipi di entità (sezioni)
            _entityTypesKeyOnInit.Clear();

            foreach (EntityType entType in EntityTypes.Values)
            {
                int dependencyEnum = (int)entType.DependencyEnum;

                if (dependencyEnum > 0)
                {

                    if (!(entType is DivisioneItemType))
                    {
                        int aaa = (int)EntityType.DependentTypesEnum;
                        if ((dependencyEnum & (int)EntityType.DependentTypesEnum) == dependencyEnum)
                        {
                            string entTypeKey = entType.GetKey();

                            _entityTypesKeyOnInit.Add(entTypeKey);
                            items.Add(new RiferimentiComboItem()
                            {
                                Key = EntityTypes[entTypeKey].GetKey(),
                                Name = EntityTypes[entTypeKey].Name,
                                Category = LocalizationProvider.GetString("Sezioni")
                            });
                        }

                    }
                }
            }

            int div = (int)EntityTypeDependencyEnum.Divisione;
            if ((div  & (int)EntityType.DependentTypesEnum) == div)
            {

                //Aggiungo le divisioni
                foreach (EntityType entType in EntityTypes.Values)
                {
                    int dependencyEnum = (int)entType.DependencyEnum;

                    if (dependencyEnum > 0)
                    {

                        if (entType is DivisioneItemType)
                        {
                            DivisioneItemType divType = entType as DivisioneItemType;
                            string comboItem = entType.Name;
                            if (comboItem != null)
                            {
                                string key = divType.GetKey();
                                _entityTypesKeyOnInit.Add(key);
                                items.Add(new RiferimentiComboItem()
                                {
                                    Key = EntityTypes[key].GetKey(),
                                    Name = comboItem,
                                    Category = LocalizationProvider.GetString("Divisioni")
                                });
                            }
                        }
                    }


                }
            }

            EntityTypesNameLoc = new ListCollectionView(items);
            EntityTypesNameLoc.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
        }

        //public ICommand AddRiferimentoCommand { get { return new CommandHandler(() => this.AddRiferimento()); } }
        //void AddRiferimento()
        //{
        //    //Aggiungo l'attributo alla lista attributi dell'entità
        //    string definizioneAttributoCodice = BuiltInCodes.DefinizioneAttributo.Guid;
        //    string codiceNuovo = CreateNewCodiceRiferimento();
        //    Attributo newAtt = new Attributo(DefinizioniAttributo[definizioneAttributoCodice], EntityType, codiceNuovo);
        //    newAtt.IsVisible = true;
        //    //newAtt.IsRiferimentiVisible = true;
        //    newAtt.Etichetta = LocalizationProvider.GetString("Nuovo");
        //    newAtt.IsBuiltIn = false;
        //    newAtt.GuidReferenceEntityTypeKey = _entityTypesKeyOnInit.First();//BuiltInCodes.EntityType.Prezzario

        //    EntityType.Attributi.Add(newAtt.Codice, newAtt);

        //    //Aggiungo la riga di settings dell'attributo nella griglia
        //    AttributoSettingsView newAttSettings = NewAttributoSettingsView(newAtt);// new RiferimentoSettingsView(this, newAtt);
        //    //newAttSettings.SetNew();
        //    int currentIndex = AttributiFirstLevelItems.IndexOf(CurrentAttOrdinamentoSettings);
        //    if (currentIndex >= 0 && currentIndex < AttributiFirstLevelItems.Count - 1)
        //        AttributiFirstLevelItems.Insert(currentIndex + 1, newAttSettings);
        //    else
        //        AttributiFirstLevelItems.Add(newAttSettings);

        //    CurrentAttOrdinamentoSettings = newAttSettings;

        //    AttributiTabUpdatePending = true;
        //}

        //public ICommand RemoveRiferimentoCommand { get { return new CommandHandler(() => this.RemoveRiferimento()); } }
        void RemoveGuid(string codiceAttributo)
        {
            //Rimuovo l'attributo dalla lista di attributi
            if (EntityType.Attributi.ContainsKey(codiceAttributo))
                EntityType.Attributi.Remove(codiceAttributo);

            //Rimuovo tutti gli attributi che fanno riferimento all'attributo di tipo Guid
            IEnumerable<AttributoRiferimento> attsRif = EntityType.Attributi.Values.Where(item => item is AttributoRiferimento).Select(item => item as AttributoRiferimento);
            List<string> attToRemoveCodice = attsRif.Where(item => item.ReferenceCodiceGuid == codiceAttributo).Select(item => item.Codice).ToList();
            foreach (string attCodice in attToRemoveCodice)
            {
                EntityType.Attributi.Remove(attCodice);
                EntityType.AttributiMasterCodes.Remove(attCodice);
            }

            ////Rimuovo l'attributo dalla griglia First Level
            //int index = AttributiFirstLevelItems.IndexOf(attToRemove);
            //if (index < AttributiFirstLevelItems.Count - 1)
            //    CurrentAttOrdinamentoSettings = AttributiFirstLevelItems[index + 1];
            //else if (index - 1 < AttributiFirstLevelItems.Count)
            //    CurrentAttOrdinamentoSettings = AttributiFirstLevelItems[index - 1];
            //else
            //    CurrentAttOrdinamentoSettings = null;
            //.RemoveAt(index);







            AttributiTabUpdatePending = true;
        }

        //string CreateNewCodiceRiferimento()
        //{
        //    string candidate = null;
        //    int i = -1;
        //    while (i < 1000)
        //    {
        //        i++;
        //        candidate = i.ToString();
        //        if (!_codiceAttributiOnInit.Contains(candidate))
        //            break;
        //    }
        //    _codiceAttributiOnInit.Add(candidate);

        //    return candidate;
        //}

        internal string GetRiferimentiEntityTypeCodiceByIndex(int index)
        {
            return _entityTypesKeyOnInit[index];
        }

        internal int GetRiferimentiEntityTypeIndexByCodice(string guidReferenceEntityTypeKey)
        {
            return _entityTypesKeyOnInit.IndexOf(guidReferenceEntityTypeKey);
        }

        internal string CreateDefaultAttributoEtichetta(Attributo att)
        {
            string etichetta = string.Empty;
            if (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;
                string etichettaAttGuid = EntityType.Attributi[attRif.ReferenceCodiceGuid].Etichetta;

                string sourceAttEtichetta = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi[attRif.ReferenceCodiceAttributo].Etichetta;
                etichetta = string.Format("{0} {1}", etichettaAttGuid, sourceAttEtichetta);

            }
            else
            {
                etichetta = LocalizationProvider.GetString("Nuovo");
            }
            return etichetta;
        }

        //#endregion Riferimenti

        public bool IsAcceptButtonEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                return true;
            }
        }

        public bool IsAdvancedMode
        {
            get => (MainOperation == null)? false : MainOperation.IsAdvancedMode();
        }

        public static string GetDefinizioneAttributoLocalizedName(string codice)
        {
            if (codice == BuiltInCodes.DefinizioneAttributo.Booleano)
                return LocalizationProvider.GetString("Booleano");
            if (codice == BuiltInCodes.DefinizioneAttributo.Colore)
                return LocalizationProvider.GetString("Colore");
            if (codice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                return LocalizationProvider.GetString("NumeroContabilita");
            if (codice == BuiltInCodes.DefinizioneAttributo.Data)
                return LocalizationProvider.GetString("Data");
            if (codice == BuiltInCodes.DefinizioneAttributo.Elenco)
                return LocalizationProvider.GetString("Elenco");
            if (codice == BuiltInCodes.DefinizioneAttributo.FormatoNumero)
                return LocalizationProvider.GetString("FormatoNumero");
            if (codice == BuiltInCodes.DefinizioneAttributo.Guid)
                return LocalizationProvider.GetString("Guid");
            if (codice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                return LocalizationProvider.GetString("GuidCollection");
            if (codice == BuiltInCodes.DefinizioneAttributo.Reale)
                return LocalizationProvider.GetString("NumeroReale");
            if (codice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                return LocalizationProvider.GetString("Riferimento");
            if (codice == BuiltInCodes.DefinizioneAttributo.Testo)
                return LocalizationProvider.GetString("Testo");
            if (codice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                return LocalizationProvider.GetString("TestoCollection");
            if (codice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                return LocalizationProvider.GetString("TestoRTF");
            if (codice == BuiltInCodes.DefinizioneAttributo.Variabile)
                return LocalizationProvider.GetString("Variabile");

            return null;
        }

    }
    #region Attributi

    /// <summary>
    /// Rappresenta una riga della griglia attributi e il dettaglio corrispondente (non deve essere istanziato)
    /// </summary>
    public class AttributoSettingsView : NotificationBase<Attributo>
    {
        protected AttributiSettingsView _owner = null;

        public string GuidReferenceEntityTypeKey { get => This.GuidReferenceEntityTypeKey; }

        public ListCollectionView GroupOperations { get; protected set; } = null;

        public AttributoSettingsView(AttributiSettingsView owner, Attributo a = null) : base(a)
        {
            _owner = owner;
            _owner.MessageBarView.Reply += MessageBarView_Confirmed;
        }


        public virtual void Load()
        {
            LoadGroupOperations();
            UpdateUI();
        }

        protected virtual void LoadGroupOperations() { }

        public string Etichetta
        {
            get => This.Etichetta;
            set
            {
                //if (!IsNew)
                //    _owner.MessageBarView.Show(LocalizationProvider.GetString("RiferimentiAttributoNonSarannoSoddisfatti"), true);

                SetProperty(This.Etichetta, value, () => This.Etichetta = value);
            }
        }

        public string Codice
        {
            get
            {
                return This.Codice;
            }
        }


        private void MessageBarView_Confirmed(object sender, EventArgs e)
        {
        }

        


        public string DefinitionName
        {
            get
            {
                //if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                //    return LocalizationProvider.GetString(BuiltInCodes.DefinizioneAttributo.Riferimento);
                //else
                //    return LocalizationProvider.GetString(This.DefinizioneAttributoCodice);
                return AttributiSettingsView.GetDefinizioneAttributoLocalizedName(This.DefinizioneAttributoCodice);
            }
            set
            {
                if (!IsNew)
                {
                    if (This.IsBuiltIn)
                        _owner.MessageBarView.Show(LocalizationProvider.GetString("ModificaNonConsentitaBuiltIn"), true);
                    else
                        _owner.MessageBarView.Show(LocalizationProvider.GetString("ModificaNonConsentitaNotNew"), true);
                    return;
                }
                else
                {
                    string defCodice = _owner.GetDefinizioneCodiceByIndex(_owner.DefinizioniAttributoLoc.IndexOf(value));

                    if (SetProperty(This.DefinizioneAttributoCodice, defCodice, () => This.DefinizioneAttributoCodice = defCodice))
                    {
                        //dati di attributo da mantenere
                        string etichetta = This.Etichetta;
                        bool isVisble = This.IsVisible;
                        string groupName = This.GroupName;
                        //bool isLock = This.IsValoreLockedByDefault;


                        //Tolgo il vecchio attributo e aggiungo il nuovo alla lista degli attributi con codice diverso
                        _owner.EntityType.Attributi.Remove(This.Codice);
                        _owner.EntityType.AttributiMasterCodes.Remove(This.Codice);

                        //Aggiungo l'attributo alla lista attributi dell'entità
                        //Attributo newAtt = _owner.AddEntityTypeAttributo(defCodice, null);
                        Attributo newAtt = _owner.AddEntityTypeAttributo(defCodice, This.Codice);

                        //risetto gli attributi appena cambiati
                        newAtt.Etichetta = etichetta;
                        newAtt.IsVisible = isVisble;
                        newAtt.GroupName = groupName;
                        //newAtt.IsValoreLockedByDefault = isLock;
                        _owner.SetAttributoNew(newAtt.Codice);

                        //cambio l'item della griglia attributi al primo livello e AttributiItems
                        int attributiFirstLevelItemsIndex = _owner.AttributiFirstLevelItems.IndexOf(_owner.CurrentAttributoSettings);
                        int attributiItemsIndex = _owner.AttributiItems.IndexOf(_owner.CurrentAttributoSettings);
                        AttributoSettingsView newItem = _owner.NewAttributoSettingsView(newAtt);

                        _owner.AttributiFirstLevelItems[attributiFirstLevelItemsIndex] = newItem;
                        _owner.AttributiItems[attributiItemsIndex] = newItem;
                    }
                }
            }
        }

        public bool IsNew { get => _owner.IsAttributoNew(This.Codice); }

        public Commons.ColorConverter.ColorsEnum DefinitionNameColor1
        {
            get
            {
                if (!IsNew)
                    return Commons.ColorConverter.ColorsEnum.DimGray;
                else
                    return Commons.ColorConverter.ColorsEnum.Black;

            }
        }


        public SolidColorBrush DefinitionNameColor
        {
            get
            {
                if (!IsNew)
                    return new SolidColorBrush(Colors.DimGray);
                else
                    return new SolidColorBrush(Colors.Black);

            }
        }


        public string GroupName
        {
            get => This.GroupName;
            set
            {
                SetProperty(This.GroupName, value, () => This.GroupName = value);
            }
        }


        //public string EntityTypeRifName
        //{
        //    get
        //    {
        //        string entityTypeRiferimento = this.CreateEntityTypeDescription(This.Codice);
        //        return entityTypeRiferimento;
        //    }
        //}
        
        //int _entityTypeIndex = -1;
        public int EntityTypeIndex
        {
            get
            {
                int index = _owner.GetRiferimentiEntityTypeIndexByCodice(This.GuidReferenceEntityTypeKey);
                return index;
            }
            set
            {
                
                if (This.IsBuiltIn)
                {
                    _owner.MessageBarView.Show(LocalizationProvider.GetString("ModificaNonConsentitaBuiltIn"));
                    return;
                }
                int index = value;
                if (index < 0 || 
                    (This.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Guid
                    && This.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection))
                    return;

                string guidReferenceEntityTypeKey = _owner.GetRiferimentiEntityTypeCodiceByIndex(index);
                if (SetProperty(This.GuidReferenceEntityTypeKey, guidReferenceEntityTypeKey, () => This.GuidReferenceEntityTypeKey = guidReferenceEntityTypeKey))
                {
                    _owner.AttributiTabUpdatePending = true;
                    RaisePropertyChanged(GetPropertyName(() => EntityTypeName));
                }
            }
        }

        public string EntityTypeName
        {
            get
            {
                if (EntityTypeIndex >= 0)
                {
                    RiferimentiComboItem item = _owner.EntityTypesNameLoc.GetItemAt(EntityTypeIndex) as RiferimentiComboItem;
                    return item.Name;
                }
                return null;
            }
            set
            {

            }
        }

        public Commons.ColorConverter.ColorsEnum EntityTypeNameColor1
        {
            get
            {
                if (!IsEntityTypeNameEnabled)
                    return Commons.ColorConverter.ColorsEnum.DimGray;
                else
                    return Commons.ColorConverter.ColorsEnum.Black;

            }
        }

        public SolidColorBrush EntityTypeNameColor
        {
            get
            {
                if (!IsEntityTypeNameEnabled)
                    return new SolidColorBrush(Colors.DimGray);
                else
                    return new SolidColorBrush(Colors.Black);

            }
        }

        public bool IsVisible
        {
            get => This.IsVisible;
            set
            {
                SetProperty(This.IsVisible, value, () => This.IsVisible = value);
            }
        }

        public virtual string ValoreDefaultPlain
        {
            get => This.ValoreDefault.PlainText;
        }

        public bool IsValoreLockedByDefault
        {
            get => This.IsValoreLockedByDefault;
            set
            {
                if (IsIsValoreLockedByDefaultEditable)
                {
                    SetProperty(This.IsValoreLockedByDefault, value, () => This.IsValoreLockedByDefault = value);
                }
            }
        }

        public Commons.ColorConverter.ColorsEnum IsValoreLockedByDefaultColor1
        {
            get
            {
                if (!IsIsValoreLockedByDefaultEditable)
                    return Commons.ColorConverter.ColorsEnum.DimGray;
                else
                    return Commons.ColorConverter.ColorsEnum.Black;

            }
        }

        public SolidColorBrush IsValoreLockedByDefaultColor
        {
            get
            {
                if (!IsIsValoreLockedByDefaultEditable)
                    return new SolidColorBrush(Colors.DimGray);
                else
                    return new SolidColorBrush(Colors.Black);

            }
        }

        public virtual bool IsIsValoreLockedByDefaultEditable
        {
            get
            {
                //return /*!This.IsBuiltIn && */!(This is AttributoRiferimento);

                if (This.Codice == BuiltInCodes.Attributo.Model3dRule)
                    return false;

                if (This.Codice == BuiltInCodes.Attributo.QuantitaTotale)
                    return false;

                if (This.Codice == BuiltInCodes.Attributo.Importo)
                    return false;

                if (This.Codice == BuiltInCodes.Attributo.AttributoFilterText)
                    return false;

                if (This.Codice == BuiltInCodes.Attributo.PredecessorText)
                    return false;

                if (This.Codice == BuiltInCodes.Attributo.WeekHoursText)
                    return false;

                if (This.Codice == BuiltInCodes.Attributo.CustomDaysText)
                    return false;

                if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    return false;

                if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    return false;

                //if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                //    return false;

                if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                    return false;

                return true;
            }
        }

        public ICommand LockCommand { get { return new CommandHandler(() => this.Lock()); } }
        void Lock()
        {
            IsValoreLockedByDefault = !IsValoreLockedByDefault;
        }

        public void CurrentCellActivated(string mappingName)
        {
            if (mappingName == GetPropertyName(() => IsValoreLockedByDefault))
                Lock();
        }

        public string GetCodice()
        {
            return This.Codice;
        }

        internal string GetDefinizioneAttributoCodice()
        {
            return This.DefinizioneAttributoCodice;
        }

        internal string GetSourceDefinizioneAttributoCodice()
        {
            Attributo sourceAtt = _owner.EntitiesHelper.GetSourceAttributo(This);
            return sourceAtt.DefinizioneAttributoCodice;
        }

        public bool IsBuiltIn { get => This.IsBuiltIn; }

        public bool IsEntityTypeNameEnabled
        {
            get
            {
                if (This.IsBuiltIn)
                    return false;

                if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid
                    || This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    if (!IsNew || _attributoRiferimentoItems.Count > 0)
                        return false;
                }
                else
                    return false;

                return true;
            }

            //get => !This.IsBuiltIn && This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid && IsNew;
        }

        //internal Attributo CloneAttributo(string codiceNuovo, string etichetta)
        //{



        //    Attributo newAtt = This.Clone();
        //    newAtt.IsBuiltIn = false;
        //    newAtt.Codice = codiceNuovo;
        //    newAtt.Etichetta = etichetta;
        //    newAtt.IsValoreLockedByDefault = false;
        //    newAtt.IsValoreReadOnly = false;

        //    return newAtt;
        //}

        public int DetailViewOrder { get => This.DetailViewOrder; set => This.DetailViewOrder = value; }

        /// <summary>
        /// Crea la stringa da visualizzare all'utente
        /// </summary>
        /// <param name="codiceAtt">codice di attributo Guid o Riferimento</param>
        /// <returns></returns>
        internal string CreateEntityTypeDescription(string codiceAtt)
        {
            Attributo att = _owner.EntityType.Attributi[codiceAtt];

            //if (att is AttributoRiferimento)
            //{
            //    AttributoRiferimento attRif = att as AttributoRiferimento;
            //    att = _owner.EntityType.Attributi[attRif.ReferenceCodiceGuid];
            //}

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
            {
                EntityType entityTypeRif = _owner.EntityTypes[att.GuidReferenceEntityTypeKey];
                string EntityTypeRifName = entityTypeRif.Name;
                string str = string.Empty;
                string strDivisione = LocalizationProvider.GetString("Divisione");
                if (entityTypeRif is DivisioneItemType)
                {
                    str = string.Format("{0}>{1}", strDivisione, EntityTypeRifName/*, att.Etichetta*/);
                }
                else
                {
                    str = string.Format("{0}", EntityTypeRifName/*, att.Etichetta*/);
                }
                return str;
            }
            else
            {
                return _owner.EntityType.Name;
            }
        }

        private ObservableCollection<AttributoSettingsRiferimentoView> _attributoRiferimentoItems = new ObservableCollection<AttributoSettingsRiferimentoView>();
        public ObservableCollection<AttributoSettingsRiferimentoView> AttributoRiferimentoItems
        {
            get { return _attributoRiferimentoItems; }
            set { _attributoRiferimentoItems = value; }
        }

        internal virtual void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsEntityTypeNameEnabled));
            RaisePropertyChanged(GetPropertyName(() => EntityTypeNameColor));
            RaisePropertyChanged(GetPropertyName(() => IsAdvancedMode));
        }

        public bool HasDetailSettings
        {
            get
            {
                if (This.Codice == BuiltInCodes.Attributo.Model3dRule)
                    return false;

                return true;
            }
        }

        private string AttributoSyntax
        {
            get => string.Format("att{{{0}}}", Etichetta);
        }

        public string CopyAttributoSyntaxMenuItemHeader
        {
            get
            {
                string headerCopySyntaxAttributo = string.Format("{0} \"{1}\"", LocalizationProvider.GetString("Copia2"), AttributoSyntax);
                return headerCopySyntaxAttributo;
            }
        }

        public ICommand CopyAttributoSyntaxCommand { get { return new CommandHandler(() => this.CopyAttributoSyntax()); } }
        public void CopyAttributoSyntax()
        {
            Clipboard.Clear();
            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Text, AttributoSyntax);
            Clipboard.SetDataObject(dataObject);
        }

        public int GridGroupOperationIndex
        {
            get
            {
                return ((int)This.GroupOperation) - 1;
            }

            set
            {
                ValoreOperationType groupOperation = (ValoreOperationType)(((int)value) + 1);
                SetProperty(This.GroupOperation, groupOperation, () => This.GroupOperation = groupOperation);
            }
        }

        public bool IsGridGroupOperationReadOnly
        {
            get =>  This.IsBuiltIn || GroupOperations == null || GroupOperations.Count < 2 || _owner.EntityType.MasterType != MasterType.Grid;
        }

        public bool IsAdvancedMode
        {
            get => _owner.IsAdvancedMode;
        }

    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo
    /// </summary>
    public class AttributoSettingsTestoView : AttributoSettingsView
    {


        public AttributoSettingsTestoView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
        }

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();
            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Equivalent, Name = _owner.GroupOperationTypes[ValoreOperationType.Equivalent] });

            GroupOperations = new ListCollectionView(items);
        }

        public string ValoreDefault
        {
            get => This.ValoreDefault.ToPlainText();
            set
            {
                This.ValoreDefault = new ValoreTesto() { V = value };
                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));
            }
        }

        public bool IsUseDeepValoreVisible
        {
            get
            {
                if (This.EntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                {
                    if (_owner.EntitiesHelper.IsAttributoDeep(This.EntityTypeKey, This.Codice))
                    {
                        return true;
                    }
                }      
                
                return false;
            }
        }

        public bool UseDeepValore
        {
            get
            {
                if (This.ValoreAttributo != null && This.ValoreAttributo is ValoreAttributoTesto)
                {
                    var valAttTesto = This.ValoreAttributo as ValoreAttributoTesto;
                    return valAttTesto.UseDeepValore;
                }

                return false;

            }
            set
            {
                ValoreAttributoTesto valAttTesto = This.ValoreAttributo as ValoreAttributoTesto;
                if (valAttTesto == null)
                    This.ValoreAttributo = valAttTesto = new ValoreAttributoTesto();

                if (SetProperty(valAttTesto.UseDeepValore, value, () => valAttTesto.UseDeepValore = value))
                {
                    UpdateUI();
                }
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => UseDeepValore));
            RaisePropertyChanged(GetPropertyName(() => IsUseDeepValoreVisible)); 
        }

    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo rtf
    /// </summary>
    public class AttributoSettingsTestoRtfView : AttributoSettingsView
    {
        public AttributoSettingsTestoRtfView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

        public ICommand SetDefaultRtfCommand { get { return new CommandHandler(() => this.SetDefaultRtf()); } }
        void SetDefaultRtf()
        {
            string testoRtf = null;
            if (This.ValoreDefault != null)
                testoRtf = (This.ValoreDefault as ValoreTestoRtf).V;
            else
                testoRtf = RtfHelperDevExpress.RtfDefault;

            _owner.EntitiesHelper.UpdateRtfByStiliItems(ref testoRtf);

            string plainText = "";
            
            _owner.WindowService.ShowEditRtfWindow(ref testoRtf, LocalizationProvider.GetString("Imposta default"), out plainText);

            This.ValoreDefault = new ValoreTestoRtf() { V = testoRtf };

            //Setto il valore default anche nell'attributo padre
            if (_owner.EntitiesHelper.IsAttributoDeep(_owner.EntityType.GetKey(), This.Codice))
            {
                var entTypeParent = _owner.EntitiesHelper.GetEntityTypeParent(_owner.EntityType.GetKey());
                Attributo attParent = null;
                if (entTypeParent.Attributi.TryGetValue(This.Codice, out attParent))
                {
                    attParent.ValoreDefault = new ValoreTestoRtf() { V = testoRtf };
                }
            }

            UpdateUI();
        }

        public ICommand ResetDefaultRtfCommand { get { return new CommandHandler(() => this.ResetDefaultRtf()); } }
        void ResetDefaultRtf()
        {
            This.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };

            //Setto il valore default anche nell'attributo padre
            if (_owner.EntitiesHelper.IsAttributoDeep(_owner.EntityType.GetKey(), This.Codice))
            {
                var entTypeParent = _owner.EntitiesHelper.GetEntityTypeParent(_owner.EntityType.GetKey());
                Attributo attParent = null;
                if (entTypeParent.Attributi.TryGetValue(This.Codice, out attParent))
                {
                    attParent.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
                }
            }

            UpdateUI();
        }

        public bool IsResetEnabled
        {
            get
            {
                string testoRtf = null;
                if (This.ValoreDefault == null)
                    return false;
                
                testoRtf = (This.ValoreDefault as ValoreTestoRtf).V;
                if (testoRtf == RtfHelperDevExpress.RtfDefault)
                    return false;

                return true;
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();
            
            RaisePropertyChanged(GetPropertyName(() => IsResetEnabled));
        }
    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo Reale
    /// </summary>
    public class AttributoSettingsRealeView : AttributoSettingsView
    {
        public AttributoSettingsRealeView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

        public NumericFormatView FormatNumeroView { get; set; }

        public override void Load()
        {
            base.Load();

            if (This.ValoreAttributo == null)
                This.ValoreAttributo = new ValoreAttributoReale();

            NumericFormatHelper.UpdateCulture(false);
            
        }

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();


            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Equivalent, Name = _owner.GroupOperationTypes[ValoreOperationType.Equivalent] });
            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Sum, Name = _owner.GroupOperationTypes[ValoreOperationType.Sum] });


            GroupOperations = new ListCollectionView(items);
        }

        public string ValoreDefault
        {
            get
            {
                return (This.ValoreDefault as ValoreReale).V;
            }
            set
            {
                This.ValoreDefault = new ValoreReale() { V = value };
                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));

            }
        }

        public string ValoreUserFormat
        {
            get
            {
                return NumericFormatHelper.ConvertToUserFormat(ValoreFormat);
            }
        }

        public string ValoreFormat
        {
            get
            {
                return This.ValoreFormat;
            }
            set
            {
                if (SetProperty(This.ValoreFormat, value, () => This.ValoreFormat = value))
                {
                    UpdateUI();
                }
            }
        }

        public string FormattedExample
        {
            get
            {
                string formattedExampleNum = NumericFormatHelper.GetFormattedExample(ValoreFormat);
                string str = string.Format("{0}: {1}", LocalizationProvider.GetString("Esempio"), formattedExampleNum);
                return str;
            }
        }


        public bool UseSignificantDigitsByFormat
        {
            get
            {
                if (This.ValoreAttributo != null && This.ValoreAttributo is ValoreAttributoReale)
                {
                    var valAttReale = This.ValoreAttributo as ValoreAttributoReale;
                    return valAttReale.UseSignificantDigitsByFormat;
                }

                return false;

            }
            set
            {
                ValoreAttributoReale valAttReale = This.ValoreAttributo as ValoreAttributoReale;
                if (valAttReale == null)
                    This.ValoreAttributo = valAttReale = new ValoreAttributoReale();

                if (SetProperty(valAttReale.UseSignificantDigitsByFormat, value, () => valAttReale.UseSignificantDigitsByFormat = value))
                {
                    UpdateUI();
                }
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => FormattedExample));
            RaisePropertyChanged(GetPropertyName(() => ValoreUserFormat));
            RaisePropertyChanged(GetPropertyName(() => UseSignificantDigitsByFormat));


        }

    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo Contabilita
    /// </summary>
    public class AttributoSettingsContabilitaView : AttributoSettingsView
    {
        public AttributoSettingsContabilitaView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

        //public NumericFormatView FormatNumeroView { get; set; }

        public override void Load()
        {
            base.Load();
            NumericFormatHelper.UpdateCulture(true);
            //FormatNumeroView.DataService = _owner.DataService;
        }

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();


            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Equivalent, Name = _owner.GroupOperationTypes[ValoreOperationType.Equivalent] });
            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Sum, Name = _owner.GroupOperationTypes[ValoreOperationType.Sum] });

            GroupOperations = new ListCollectionView(items);
        }

        public string ValoreDefault
        {
            get
            {
                return (This.ValoreDefault as ValoreContabilita).V;
            }
            set
            {
                This.ValoreDefault = new ValoreContabilita() { V = value };
                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));

            }
        }

        public string ValoreUserFormat
        {
            get
            {
                return NumericFormatHelper.ConvertToUserFormat(ValoreFormat);
            }
        }

        public string ValoreFormat
        {
            get
            {
                return This.ValoreFormat;
            }
            set
            {
                if (SetProperty(This.ValoreFormat, value, () => This.ValoreFormat = value))
                {
                    UpdateUI();
                }
            }
        }

        public string FormattedExample
        {
            get
            {
                string formattedExampleNum = NumericFormatHelper.GetFormattedExample(ValoreFormat);
                string str = string.Format("{0}: {1}", LocalizationProvider.GetString("Esempio"), formattedExampleNum);
                return str;
            }
        }



        public bool UseSignificantDigitsByFormat
        {
            get
            {
                if (This.ValoreAttributo != null && This.ValoreAttributo is ValoreAttributoContabilita)
                {
                    var valAttCont = This.ValoreAttributo as ValoreAttributoContabilita;
                    return valAttCont.UseSignificantDigitsByFormat;
                }

                return false;

            }
            set
            {
                ValoreAttributoContabilita valAttCont = This.ValoreAttributo as ValoreAttributoContabilita;
                if (valAttCont == null)
                    This.ValoreAttributo = valAttCont = new ValoreAttributoContabilita();

                if (SetProperty(valAttCont.UseSignificantDigitsByFormat, value, () => valAttCont.UseSignificantDigitsByFormat = value))
                {
                    UpdateUI();
                }
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => FormattedExample));
            RaisePropertyChanged(GetPropertyName(() => ValoreUserFormat));


        }


    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo Contabilita
    /// </summary>
    public class AttributoSettingsGuidView : AttributoSettingsView
    {
        public AttributoSettingsGuidView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

        public override string ValoreDefaultPlain
        {
            get => string.Empty;
        }

        public string ItemPath
        {
            get
            {
                if (This.ValoreAttributo != null && This.ValoreAttributo is ValoreAttributoGuid valAttGuid)
                {
                    return valAttGuid.ItemPath;
                }

                return string.Empty;

            }
            set
            {
                ValoreAttributoGuid valAttGuid = This.ValoreAttributo as ValoreAttributoGuid;
                if (valAttGuid == null)
                    This.ValoreAttributo = valAttGuid = new ValoreAttributoGuid();

                if (SetProperty(valAttGuid.ItemPath, value, () => valAttGuid.ItemPath = value))
                {
                    UpdateUI();
                }
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => ItemPath));
        }

    }

    /// <summary>
    /// Rappresenta una riga della griglia attributi e detail di tipo riferimento
    /// </summary>
    public class  AttributoSettingsRiferimentoView : AttributoSettingsView
    {
        AttributoRiferimento AttRif { get => This as AttributoRiferimento; }

        /// <summary>
        /// classe non istanziabile
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="a"></param>
        protected AttributoSettingsRiferimentoView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            Load();
        }

        private List<string> _riferimentiCodice = new List<string>();
        public ObservableCollection<string> Riferimenti { get; set; } = new ObservableCollection<string>();

        private List<string> _attributiCodice = new List<string>();
        public ObservableCollection<string> Attributi { get; set; } = new ObservableCollection<string>();

        public override void Load()
        {
            LoadRiferimenti();
            LoadAttributi();

            base.Load();
        }

        private void LoadRiferimenti()
        {
            Riferimenti.Clear();
            _riferimentiCodice = _owner.EntityType.Attributi.Where(item => item.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid && item.Value.IsVisible).Select(item => item.Value.Codice).ToList();
            foreach (string cod in _riferimentiCodice)
            {
                string nomeRiferimento = CreateEntityTypeDescription(cod);
                Riferimenti.Add(nomeRiferimento);
            }
        }



        private void LoadAttributi(bool setFirst = false)
        {
            //gestione un po' particolare: prima aggiungo i nuovi attributi e poi tolgo i vecchi per non far impiantare NomeAttributo
            //Attributi.Clear();
            int oldAttCount = Attributi.Count();

            //_attributiCodice = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi.Where(item => item.Value.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Guid).Select(item => item.Value.Codice).ToList();
            //_attributiCodice = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi.Where(item => !(item.Value is AttributoRiferimento) ).Select(item => item.Value.Codice).ToList();
            _attributiCodice = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi.Where(item =>
            {
                return IsSourceAttributoAllowed(item.Value);
            })
            .Select(item => item.Value.Codice).ToList();

            foreach (string cod in _attributiCodice.Reverse<string>())
            {
                Attributo att = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi[cod];
                Attributi.Insert(0, att.Etichetta);
            }

            if (setFirst)
                NomeAttributo = Attributi.First();

            //rimuovo i vecchi
            int startIndex = Attributi.Count() - 1;
            int endIndex = startIndex - oldAttCount;

            for (int i = startIndex ; i > endIndex ; i--)
                Attributi.RemoveAt(i);

        }

        protected virtual void LoadDetail()  {}

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();


            EntitiesHelper entsHelper = new EntitiesHelper(_owner.DataService);
            Attributo sourceAtt = entsHelper.GetSourceAttributo(This);

            if (sourceAtt == null)
                return;


            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Equivalent, Name = _owner.GroupOperationTypes[ValoreOperationType.Equivalent] });

            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita
                || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
            {
                items.Add(new GridGroupOperation() { Key = ValoreOperationType.Sum, Name = _owner.GroupOperationTypes[ValoreOperationType.Sum] });
            }


            
            GroupOperations = new ListCollectionView(items);
        }

        public string NomeRiferimento
        {
            get
            {
                string nomeRiferimento = CreateEntityTypeDescription(AttRif.ReferenceCodiceGuid);
                return nomeRiferimento;
                //return _owner.EntityType.Attributi[AttRif.ReferenceCodiceGuid].Etichetta;
            }
            set
            {
                int index = Riferimenti.IndexOf(value);
                if (index >= 0)
                {
                    string codiceAttGuid = _riferimentiCodice[index];
                    
                    if (SetProperty(AttRif.ReferenceCodiceGuid, value, () => AttRif.ReferenceCodiceGuid = codiceAttGuid))
                    {
                        string referenceEntityTypeKey = _owner.EntityType.Attributi[codiceAttGuid].GuidReferenceEntityTypeKey;
                        AttRif.ReferenceEntityTypeKey = referenceEntityTypeKey;

                        LoadAttributi(true);
                    }
                }

            }

        }

        public string NomeAttributo
        {
            get
            {
                if (_owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi.ContainsKey(AttRif.ReferenceCodiceAttributo))
                {
                    string etichetta = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi[AttRif.ReferenceCodiceAttributo].Etichetta;
                    return etichetta;
                }
                return null;
            }
            set
            {
                string codiceAtt = null;
                int index = Attributi.IndexOf(value);
                if (index >= 0)
                    codiceAtt = _attributiCodice[index];

                if (codiceAtt != null)
                {
                    if (SetProperty(AttRif.ReferenceCodiceAttributo, codiceAtt, () => AttRif.ReferenceCodiceAttributo = codiceAtt))
                    {
                        Etichetta = _owner.CreateDefaultAttributoEtichetta(AttRif);
                        UpdateAttributo(AttRif);
                        LoadDetail();
                    }
                    RaisePropertyChanged(GetPropertyName(() => NomeAttributo));
                }


           }
        }

        public void UpdateAttributo(AttributoRiferimento attRif)
        {
            AttRif.Height = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi[AttRif.ReferenceCodiceAttributo].Height;
            AttRif.AllowSort = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi[AttRif.ReferenceCodiceAttributo].AllowSort;
            AttRif.AllowMasterGrouping = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi[AttRif.ReferenceCodiceAttributo].AllowMasterGrouping;
            AttRif.AllowValoriUnivoci = _owner.EntityTypes[AttRif.ReferenceEntityTypeKey].Attributi[AttRif.ReferenceCodiceAttributo].AllowValoriUnivoci;

        }

        public string GetReferenceCodiceGuid()  {return AttRif.ReferenceCodiceGuid;}
        public string GetReferenceEntityTypeKey() { return AttRif.ReferenceEntityTypeKey; }
        public string GetReferenceCodiceAttributo() { return AttRif.ReferenceCodiceAttributo; }

        public virtual bool IsSourceAttributoAllowed(Attributo att) { return false; }
    }

    public class AttributoSettingsGuidRiferimentoView : AttributoSettingsRiferimentoView
    {
        public AttributoSettingsGuidRiferimentoView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            Load();
        }

        public static bool IsSourceAttributoAllowedS(Attributo sourceAtt)
        { 
            if (sourceAtt.IsInternal)
                return false;

            return true;
        }

        public override bool IsSourceAttributoAllowed(Attributo att)
        {
            return IsSourceAttributoAllowedS(att);
        }
    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo Data
    /// </summary>
    public class AttributoSettingsDataView : AttributoSettingsView
    {
        public ObservableCollection<string> DateFomats { get; set; }
        public AttributoSettingsDataView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            DateFomats = new ObservableCollection<string>();
            DateFomats.Add("ddd dd/MM/yy HH:mm");
            DateFomats.Add("dd/MM/yyyy HH:mm");
            DateFomats.Add("dd/MM/yy HH:mm");
            DateFomats.Add("dd/MM/yyyy");
            DateFomats.Add("dd/MM/yy");
            DateFomats.Add("yyyy/MM/dd");
            DateFomats.Add("yy/MM/dd");
            DateFomats.Add("MM/dd/yy HH:mm");

            if (ValoreFormat == "#,##0.00")
            {
                CurrentFormatIndex = 3;
                ValoreFormat = DateFomats.ElementAt(CurrentFormatIndex);
                TimePickerVisible = false;
            }
        }

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();

            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Equivalent, Name = _owner.GroupOperationTypes[ValoreOperationType.Equivalent] });

            GroupOperations = new ListCollectionView(items);
        }

        private bool _TimePickerVisible;
        public bool TimePickerVisible
        {
            get
            {
                return _TimePickerVisible;
            }
            set
            {
                if (SetProperty(ref _TimePickerVisible, value))
                {
                    UpdateUI();
                }
            }
        }

        public string Mask { get => TimePickerVisible? "g" : "d"; } //for devexpress

        public DateTime? ValoreDefault
        {
            get
            {
                return (This.ValoreDefault as ValoreData).V;
            }
            set
            {
                This.ValoreDefault = new ValoreData() { V = value };
                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));

            }
        }

        public string ValoreUserFormat
        {
            get
            {
                if (ValoreFormat.Contains("H") || ValoreFormat.Contains("m"))
                    TimePickerVisible = true;
                else
                    TimePickerVisible = false;
                return DateFormatHelper.ConvertToUserFormat(ValoreFormat);
            }
        }

        public string ValoreFormat
        {
            get
            {
                return This.ValoreFormat;
            }
            set
            {
                if (SetProperty(This.ValoreFormat, value, () => This.ValoreFormat = value))
                {
                    UpdateUI();
                }
            }
        }

        public string FormattedExample
        {
            get
            {
                string formattedExampleNum = DateFormatHelper.GetFormattedExample(ValoreFormat, ValoreDefault);
                string str = string.Format("{0}: {1}", LocalizationProvider.GetString("Esempio"), formattedExampleNum);
                return str;
            }
        }

        private int _CurrentFormatIndex = -1;
        public int CurrentFormatIndex
        {
            get
            {
                return _CurrentFormatIndex;
            }
            set
            {
                if (SetProperty( ref _CurrentFormatIndex, value))
                {
                    _CurrentFormatIndex = value;
                }
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => FormattedExample));
            RaisePropertyChanged(GetPropertyName(() => ValoreUserFormat));
            RaisePropertyChanged(GetPropertyName(() => Mask));


        }

        public ICommand MouseDoubleClickCommand { get { return new CommandHandler(() => this.MouseDoubleClick()); } }
        void MouseDoubleClick()
        {
            ValoreFormat = DateFomats.ElementAt(CurrentFormatIndex);
        }
    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo
    /// </summary>
    public class AttributoSettingsTestoCollectionView : AttributoSettingsView
    {
        public AttributoSettingsTestoCollectionView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

        public override string ValoreDefaultPlain
        {
            get => string.Empty;
        }

    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo
    /// </summary>
    public class AttributoSettingsGuidCollectionView : AttributoSettingsView
    {
        public AttributoSettingsGuidCollectionView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            Load();


        }

        ObservableCollection<ItemsSelectionTypeView> _itemsSelectionTypes = new ObservableCollection<ItemsSelectionTypeView>();
        public ObservableCollection<ItemsSelectionTypeView> ItemsSelectionTypes
        {
            get => _itemsSelectionTypes;
            set => SetProperty(ref _itemsSelectionTypes, value);
        }

        public override void Load()
        {
            base.Load();

            _itemsSelectionTypes.Clear();

            _itemsSelectionTypes.Add(new ItemsSelectionTypeView() { Nome = LocalizationProvider.GetString("Manuale"), Type = ItemsSelectionTypeEnum.ByHand });
            _itemsSelectionTypes.Add(new ItemsSelectionTypeView() { Nome = LocalizationProvider.GetString("Da filtro"), Type = ItemsSelectionTypeEnum.ByFilter });


            if (This.EntityTypeKey == BuiltInCodes.EntityType.InfoProgetto)
                _itemsSelectionTypes.Add(new ItemsSelectionTypeView() { Nome = LocalizationProvider.GetString("Tutte"), Type = ItemsSelectionTypeEnum.All });

            if (ItemsSelectionTypesIndex < 0)
                ItemsSelectionTypesIndex = 0;

            UpdateUI();
        }

        public override string ValoreDefaultPlain
        {
            get => string.Empty;
        }

        public int ItemsSelectionTypesIndex
        {
            get
            {
                int index = -1;
                if (This.ValoreAttributo != null)
                {
                    ItemsSelectionTypeEnum type = (ItemsSelectionTypeEnum)(This.ValoreAttributo as ValoreAttributoGuidCollection).ItemsSelectionType;
                    index = ItemsSelectionTypes.IndexOf(ItemsSelectionTypes.FirstOrDefault(item => item.Type == type));
                }
                return index;
            }
            set
            {
                int index = value;
                if (index < 0)
                    return;

                ItemsSelectionTypeEnum type = (ItemsSelectionTypeEnum)ItemsSelectionTypes[index].Type;

                if (This.ValoreAttributo == null)
                    This.ValoreAttributo = new ValoreAttributoGuidCollection();

                (This.ValoreAttributo as ValoreAttributoGuidCollection).ItemsSelectionType = type;

                if (type == ItemsSelectionTypeEnum.ByHand)
                    This.Height = 60;
                else
                    This.Height = 20;

                UpdateUI();
            }
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => ItemsSelectionTypes));
            RaisePropertyChanged(GetPropertyName(() => IsItemsSelectionTypesReadOnly));
            RaisePropertyChanged(GetPropertyName(() => ItemsSelectionTypesIndex));
            RaisePropertyChanged(GetPropertyName(() => IsIsValoreLockedByDefaultEditable));
        }

        public bool IsByCode
        {
            get
            {
                if (This.IsBuiltIn && This.IsValoreReadOnly)
                    return true;
                return false;
            }
        }

        public bool IsItemsSelectionTypesReadOnly
        {
            get => IsNew;
        }

        public override bool IsIsValoreLockedByDefaultEditable
        {
            get
            {
                if (This.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    if (This.ValoreAttributo is ValoreAttributoGuidCollection)
                    {
                        ItemsSelectionTypeEnum type = (ItemsSelectionTypeEnum)(This.ValoreAttributo as ValoreAttributoGuidCollection).ItemsSelectionType;
                        if (type != ItemsSelectionTypeEnum.ByFilter)
                            return false;
                    }
                }

                return base.IsIsValoreLockedByDefaultEditable;
            }
        }

        public ICommand ValoreDefaultCommand { get { return new CommandHandler(() => this.ValoreDefault()); } }
        void ValoreDefault()
        {
            ValoreConditions valConds = null;

            if (This.ValoreDefault is ValoreGuidCollection)
            {
                ValoreGuidCollection valGuidColl = This.ValoreDefault as ValoreGuidCollection;
                valConds = valGuidColl.Filter.ToConditions();
            }

            if (valConds == null)
                valConds = new ValoreConditions();

            if (_owner.WindowService.SetValoreConditionsWnd(This.GuidReferenceEntityTypeKey/*_owner.EntityType.GetKey()*/, valConds, true))
            {
                FilterData filterData = new FilterData();
                filterData.FromConditions(This.GuidReferenceEntityTypeKey/*_owner.EntityType.GetKey()*/, valConds);

                This.ValoreDefault = new ValoreGuidCollection()
                {
                    Filter = filterData,
                };

            }

        }
    }

    public class ItemsSelectionTypeView
    {
        public ItemsSelectionTypeEnum Type { get; set; }
        public string Nome { get; set; }
    }


    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo
    /// </summary>
    public class AttributoSettingsElencoView : AttributoSettingsView
    {
        public AttributoSettingsElencoView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            SelectedItems = new ObservableCollection<object>();
        }

        private ObservableCollection<AttributoSettingsElencoItemView> _elencoItems = new ObservableCollection<AttributoSettingsElencoItemView>();
        public ObservableCollection<AttributoSettingsElencoItemView> ElencoItems
        {
            get { return _elencoItems; }
            set { _elencoItems = value; }
        }

        ObservableCollection<object> _SelectedItems;
        public ObservableCollection<object> SelectedItems
        {
            get
            {
                return _SelectedItems;
            }

            set
            {
                _SelectedItems = value;
                RaisePropertyChanged(GetPropertyName(() => SelectedItems));
            }
        }

        AttributoSettingsElencoItemView _selectedItem = null;
        public AttributoSettingsElencoItemView SelectedItem
        {
            get
            {
                if (IsMultiSelection)
                {
                    SelectedItems.Clear();
                    ValoreElenco valEl = This.ValoreDefault as ValoreElenco;
                    ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                    foreach (ValoreAttributoElencoItem Item in elenco.Items)
                    {
                        if ((valEl.ValoreAttributoElencoId & Item.Id) == Item.Id)
                            if (!SelectedItems.Contains(Item))
                                SelectedItems.Add(ElencoItems.FirstOrDefault(a => a.Name == Item.Text));
                    }
                }
                else
                {
                    ValoreElenco valEl = This.ValoreDefault as ValoreElenco;
                    ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                    ValoreAttributoElencoItem elItem = elenco.Items.FirstOrDefault(item => item.Id == valEl.ValoreAttributoElencoId);
                    if (elItem != null)
                        return ElencoItems.Where(d => d.Name == elItem.Text).FirstOrDefault();

                }
                return _selectedItem;
            }
            set
            {
                if (IsMultiSelection)
                {
                    int Ids = 0;
                    ValoreElenco valEl = This.ValoreDefault as ValoreElenco;
                    ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;

                    foreach (AttributoSettingsElencoItemView Item in SelectedItems)
                    {
                        ValoreAttributoElencoItem elItem = elenco.Items.FirstOrDefault(item => item.Text == Item.Name);
                        if (elItem != null)
                            Ids |= elItem.Id;
                    }
                    This.ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = Ids };
                }
                else
                {
                    _selectedItem = value;
                    if (_selectedItem != null)
                    {
                        ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                        ValoreAttributoElencoItem elItem = elenco.Items.FirstOrDefault(item => item.Text == _selectedItem.Name);
                        if (elItem == null)
                            return;

                        This.ValoreDefault = new ValoreElenco() { V = elItem.Text, ValoreAttributoElencoId = elItem.Id };

                    }
                }
            }
        }

        public string Delimiter { get; set; } = ";";

        public override void Load()
        {
            base.Load();

            if (This.ValoreAttributo == null)
                This.ValoreAttributo = new ValoreAttributoElenco();


            ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
            ElencoItems = new ObservableCollection<AttributoSettingsElencoItemView>(elenco.Items.Select(item => new AttributoSettingsElencoItemView() { Name = item.Text, Owner = this }));
            ElencoItems.CollectionChanged += ElencoItems_CollectionChanged;
            IsMultiSelection = elenco.IsMultiSelection;

        }

        private void ElencoItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {  
            ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) //o drag drop
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    AttributoSettingsElencoItemView newItem = e.NewItems[0] as AttributoSettingsElencoItemView;
                    newItem.Owner = this;

                    if (e.NewStartingIndex >= elenco.Items.Count)
                        elenco.Items.Add(new ValoreAttributoElencoItem() { Id = elenco.NewId(), Text = newItem.Name });
                    else if (e.NewStartingIndex >= 0)
                        elenco.Items.Insert(e.NewStartingIndex, new ValoreAttributoElencoItem() { Id = elenco.NewId(), Text = newItem.Name });
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                elenco.Items.RemoveAt(e.OldStartingIndex);
            }
        }

        public void ReplaceText(AttributoSettingsElencoItemView item, string newText)
        {
            int index = ElencoItems.IndexOf(item);

            ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
            if (0 <= index && index < elenco.Items.Count)
                elenco.Items[index].Text = newText;
        }



        public override string ValoreDefaultPlain
        {
            get => string.Empty;
        }

        public string ValoreDefault
        {
            get
            {
                ValoreElenco valEl = This.ValoreDefault as ValoreElenco;
                ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                ValoreAttributoElencoItem elItem = elenco.Items.FirstOrDefault(item => item.Id == valEl.ValoreAttributoElencoId);
                if (elItem == null)
                    return null;

                return elItem.Text;
            }
            //=> This.ValoreDefault.ToPlainText();
            set
            {
                ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                ValoreAttributoElencoItem elItem = elenco.Items.FirstOrDefault(item => item.Text == value);
                if (elItem == null)
                    return;

                This.ValoreDefault = new ValoreElenco() { V = elItem.Text, ValoreAttributoElencoId= elItem.Id };

                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));
            }
        }
        public bool IsMultiSelection
        {
            get
            {
                ValoreElenco valEl = This.ValoreDefault as ValoreElenco;
                ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                return (This.ValoreAttributo as ValoreAttributoElenco).IsMultiSelection;
            }
            //=> This.ValoreDefault.ToPlainText();
            set
            {
                ValoreAttributoElenco elenco = This.ValoreAttributo as ValoreAttributoElenco;
                elenco.IsMultiSelection = value;
                RaisePropertyChanged(GetPropertyName(() => IsMultiSelection));
            }
        }

        public ICommand AddElencoItemCommand { get { return new CommandHandler(() => this.AddElencoItem()); } }
        void AddElencoItem()
        {

            ElencoItems.Add(new AttributoSettingsElencoItemView() { Name = LocalizationProvider.GetString("Nuovo"), Owner = this });
        }

    }

    public class AttributoSettingsElencoItemView : NotificationBase
    {
        public AttributoSettingsElencoView Owner { get; set; } = null;

        string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                
                if (SetProperty(ref _name, value))
                {
                    if (Owner != null)
                        Owner.ReplaceText(this, _name);
                }
            }
        }
    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo
    /// </summary>
    public class AttributoSettingsColoreView : AttributoSettingsView
    {


        public AttributoSettingsColoreView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;


            //foreach (var item in ColorInfo.ColoriInstallatiInMacchina)
            //{
            //    AttributoSettingsColoreItemView attributo = new AttributoSettingsColoreItemView();
            //    attributo.HexValue = item.HexValue;
            //    attributo.Name = item.Name;
            //    attributo.Color = item.Color;
            //    ColoreItems.Add(attributo);
            //}

            var coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);

            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    AttributoSettingsColoreItemView attributo = new AttributoSettingsColoreItemView();
                    attributo.HexValue = colInfo.HexValue;
                    attributo.Name = colInfo.Name;
                    attributo.Color = colInfo.Color;
                    ColoreItems.Add(attributo);
                }
            }

        }

        private ObservableCollection<AttributoSettingsColoreItemView> _ColoreItems = new ObservableCollection<AttributoSettingsColoreItemView>();
        public ObservableCollection<AttributoSettingsColoreItemView> ColoreItems
        {
            get { return _ColoreItems; }
            set { _ColoreItems = value; }
        }

        public override void Load()
        {
            base.Load();

            if (This.ValoreAttributo == null)
                This.ValoreAttributo = new ValoreAttributoColore();


            ValoreAttributoColore colore = This.ValoreAttributo as ValoreAttributoColore;
            //ColoreItems.CollectionChanged += ElencoItems_CollectionChanged;

        }

        private Color ConvertFromSystemDarwaing(System.Drawing.Color p)
        {
           return System.Windows.Media.Color.FromArgb(p.A, p.R, p.G, p.B);
        }

        private void ElencoItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ValoreAttributoColore colore = This.ValoreAttributo as ValoreAttributoColore;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) //o drag drop
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    AttributoSettingsColoreItemView newItem = e.NewItems[0] as AttributoSettingsColoreItemView;
                    newItem.Owner = this;

                    if (e.NewStartingIndex >= colore.Items.Count)
                        colore.Items.Add(new ValoreAttributoColoreItem() { Id = Guid.NewGuid(), Text = newItem.Name });
                    else if (e.NewStartingIndex >= 0)
                        colore.Items.Insert(e.NewStartingIndex, new ValoreAttributoColoreItem() { Id = Guid.NewGuid(), Text = newItem.Name });
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                colore.Items.RemoveAt(e.OldStartingIndex);
            }
        }

        public void ReplaceText(AttributoSettingsColoreItemView item, string newText)
        {
            int index = ColoreItems.IndexOf(item);

            ValoreAttributoColore colore = This.ValoreAttributo as ValoreAttributoColore;
            if (0 <= index && index < colore.Items.Count)
                colore.Items[index].Text = newText;
        }



        public override string ValoreDefaultPlain
        {
            get => string.Empty;
        }

        public string ValoreDefault
        {
            get
            {
                ValoreColore valCol = This.ValoreDefault as ValoreColore;
                ValoreAttributoColore colore = This.ValoreAttributo as ValoreAttributoColore;
                ValoreAttributoColoreItem colItem = colore.Items.FirstOrDefault(item => item.HexValue == valCol.Hexadecimal);
                if (colItem == null)
                    return null;

                return (This.ValoreDefault as ValoreColore).V;
            }
            //=> This.ValoreDefault.ToPlainText();
            set
            {
                ValoreAttributoColore colore = This.ValoreAttributo as ValoreAttributoColore;
                var ColorFound = ColoreItems.FirstOrDefault(item => item.Name == value);
                //colore.Items = new List<ValoreAttributoColoreItem>(from PropertyInfo property in typeof(System.Windows.Media.Colors).GetProperties()
                //                                                         orderby property.Name
                //                                                         select new ValoreAttributoColoreItem(property.Name, (System.Windows.Media.Color)property.GetValue(null, null))); 
                //ValoreAttributoColoreItem colItem = colore.Items.FirstOrDefault(item => item.Text == value);
                if (ColorFound == null)
                    return;

                This.ValoreDefault = new ValoreColore() { V = ColorFound.Name, Hexadecimal = ColorFound.HexValue };

                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));
            }
        }



    }

    public class AttributoSettingsColoreItemView : NotificationBase
    {
        public AttributoSettingsColoreView Owner { get; set; } = null;

        public System.Windows.Media.Color Color { get; set; }

        string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {

                if (SetProperty(ref _name, value))
                {
                    if (Owner != null)
                        Owner.ReplaceText(this, _name);
                }
            }
        }

        public System.Windows.Media.SolidColorBrush SampleBrush
        {
            get { return new System.Windows.Media.SolidColorBrush(Color); }
        }
        public string _HexValue;
        public string HexValue
        {
            get { return Color.ToString(); }
            set
            {

                if (SetProperty(ref _HexValue, value))
                {
                    _HexValue = value;
                }
            }
        }
        public AttributoSettingsColoreItemView() { }
        public AttributoSettingsColoreItemView(string color_name, System.Windows.Media.Color color)
        {
            Name = color_name;
            Color = color;
        }

        public AttributoSettingsColoreItemView(string color_name, System.Windows.Media.Color color, AttributoSettingsColoreView owner)
        {
            Name = color_name;
            Color = color;
            Owner = owner;
        }
    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo booleano
    /// </summary>
    public class AttributoSettingsBooleanoView : AttributoSettingsView
    {
        public AttributoSettingsBooleanoView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            
        }

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();

            items.Add(new GridGroupOperation() { Key = ValoreOperationType.Equivalent, Name = _owner.GroupOperationTypes[ValoreOperationType.Equivalent] });

            GroupOperations = new ListCollectionView(items);
        }

        public bool? ValoreDefault
        {
            get
            {
                return (This.ValoreDefault as ValoreBooleano).V;
            }
            set
            {
                This.ValoreDefault = new ValoreBooleano() { V = value };
                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));

            }
        }

    }

    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo testo rtf
    /// </summary>
    public class AttributoSettingsBooleanoRtfView : AttributoSettingsView
    {
        public AttributoSettingsBooleanoRtfView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

    }


    /// <summary>
    /// Rappresenta una riga della grigli attributi e detail di tipo FormatoNumero
    /// </summary>
    public class AttributoSettingsFormatoNumeroView : AttributoSettingsView
    {


        public AttributoSettingsFormatoNumeroView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
        }

        private ObservableCollection<AttributoSettingsFormatoNumeroItemView> _formatoNumeroItems = new ObservableCollection<AttributoSettingsFormatoNumeroItemView>();
        public ObservableCollection<AttributoSettingsFormatoNumeroItemView> FormatoNumeroItems
        {
            get { return _formatoNumeroItems; }
            set { _formatoNumeroItems = value; }
        }

        public override void Load()
        {
            base.Load();

            NumericFormatHelper.UpdateCulture(false);

            if (This.ValoreAttributo == null)
                This.ValoreAttributo = new ValoreAttributoFormatoNumero();


            ValoreAttributoFormatoNumero formatoNumero = This.ValoreAttributo as ValoreAttributoFormatoNumero;
            FormatoNumeroItems = new ObservableCollection<AttributoSettingsFormatoNumeroItemView>(
                formatoNumero.Items.Select(item => new AttributoSettingsFormatoNumeroItemView()
                {
                    Owner = this,
                    //Name = item.Text,
                    Format = item.Format,
                    UserFormat = NumericFormatHelper.ConvertToUserFormat(item.Format),
                }));
            FormatoNumeroItems.CollectionChanged += FormatoNumeroItems_CollectionChanged;

        }

        private void FormatoNumeroItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ValoreAttributoFormatoNumero formatoNumero = This.ValoreAttributo as ValoreAttributoFormatoNumero;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) //o drag drop
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    AttributoSettingsFormatoNumeroItemView newItem = e.NewItems[0] as AttributoSettingsFormatoNumeroItemView;
                    newItem.Owner = this;

                    if (e.NewStartingIndex >= formatoNumero.Items.Count)
                        formatoNumero.Items.Add(new ValoreAttributoFormatoNumeroItem()
                        {
                            Id = Guid.NewGuid(),
                            //Text = newItem.Name,
                            Format = newItem.Format,

                        });
                    else if (e.NewStartingIndex >= 0)
                        formatoNumero.Items.Insert(e.NewStartingIndex, new ValoreAttributoFormatoNumeroItem()
                        {
                            Id = Guid.NewGuid(),
                            //Text = newItem.Name,
                            Format = newItem.Format,
                        });
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                formatoNumero.Items.RemoveAt(e.OldStartingIndex);
            }
        }

        AttributoSettingsFormatoNumeroItemView _currentFormatoNumero = null;
        public AttributoSettingsFormatoNumeroItemView CurrentFormatoNumero
        {
            get => _currentFormatoNumero;
            set
            {
                if (SetProperty(ref _currentFormatoNumero, value))
                {
                    UpdateUI();
                }
            }
        }

        public bool IsAnyCurrentFormatoNumero
        {
            get => CurrentFormatoNumero != null;
        }

        //public void ReplaceText(AttributoSettingsFormatoNumeroItemView item, string newText)
        //{
        //    int index = FormatoNumeroItems.IndexOf(item);

        //    ValoreAttributoFormatoNumero unitaMisura = This.ValoreAttributo as ValoreAttributoFormatoNumero;
        //    if (0 <= index && index < unitaMisura.Items.Count)
        //        unitaMisura.Items[index].Text = newText;
        //}



        public override string ValoreDefaultPlain
        {
            get => string.Empty;
        }

        public string ValoreDefault
        {
            get
            {
                ValoreFormatoNumero valUm = This.ValoreDefault as ValoreFormatoNumero;
                ValoreAttributoFormatoNumero formatoNumero = This.ValoreAttributo as ValoreAttributoFormatoNumero;
                ValoreAttributoFormatoNumeroItem elItem = formatoNumero.Items.FirstOrDefault(item => item.Id == valUm.ValoreAttributoFormatoNumeroId);
                if (elItem == null)
                    return null;

                return elItem.Format;
            }
            //=> This.ValoreDefault.ToPlainText();
            set
            {
                ValoreAttributoFormatoNumero formatoNumero = This.ValoreAttributo as ValoreAttributoFormatoNumero;
                ValoreAttributoFormatoNumeroItem elItem = formatoNumero.Items.FirstOrDefault(item => item.Format == value);
                if (elItem == null)
                    return;

                This.ValoreDefault = new ValoreFormatoNumero() { V = elItem.Format, ValoreAttributoFormatoNumeroId = elItem.Id };

                RaisePropertyChanged(GetPropertyName(() => ValoreDefaultPlain));
            }
        }

        public ICommand AddFormatoNumeroCommand { get { return new CommandHandler(() => this.AddFormatoNumero()); } }
        void AddFormatoNumero()
        {
            List<string> formats = new List<string>();
            if (_owner.WindowService.SelectNumberFormatsWnd(ref formats, false) == true)
            {
                foreach (string format in formats)
                {

                    if (FormatoNumeroItems.FirstOrDefault(item => item.Format == format) == null)
                    {
                        NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);
                        FormatoNumeroItems.Add(new AttributoSettingsFormatoNumeroItemView()
                        {
                            Owner = this,
                            Format = format,
                            UserFormat = NumericFormatHelper.ConvertToUserFormat(format),
                            //Name = nf.SymbolText,
                        });
                    }
                }

            }
        }

        public ICommand EditFormatoNumeroCommand { get { return new CommandHandler(() => this.EditFormatoNumero()); } }
        void EditFormatoNumero()
        {
            List<string> formats = new List<string>();
            if (_owner.WindowService.SelectNumberFormatsWnd(ref formats, true) == true)
            {
                string format = formats.FirstOrDefault();
                if (format != null && format.Any())
                {
                    //NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);
                    AttributoSettingsFormatoNumeroItemView um = CurrentFormatoNumero;
                    //um.Name = nf.SymbolText;
                    um.Format = format;
                    um.UserFormat = NumericFormatHelper.ConvertToUserFormat(format);
                }
            }
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsAnyCurrentFormatoNumero));
        }


    }

    public class AttributoSettingsFormatoNumeroItemView : NotificationBase
    {
        public AttributoSettingsFormatoNumeroView Owner { get; set; } = null;

        //string _name = string.Empty;
        //public string Name
        //{
        //    get => _name;
        //    set
        //    {

        //        if (SetProperty(ref _name, value))
        //        {
        //            if (Owner != null)
        //                Owner.ReplaceText(this, _name);
        //        }
        //    }
        //}

        public string Format { get; internal set; }

        string _userFormat = string.Empty;
        public string UserFormat
        {
            get => _userFormat;
            set
            {
                if (SetProperty(ref _userFormat, value))
                {
                }
            }
        }

        
    }

    public class AttributoSettingsGuidCollectionRiferimentoView : AttributoSettingsRiferimentoView
    {
        AttributoRiferimento AttRif { get => This as AttributoRiferimento; }

        Dictionary<ValoreOperationType, int> _guidCollectionOperationIndexes = new Dictionary<ValoreOperationType, int>();
        List<ValoreOperationType> _guidCollectionOperationsType = new List<ValoreOperationType>();

        ObservableCollection<string> _guidCollectionOperations = new ObservableCollection<string>();
        public ObservableCollection<string> GuidCollectionOperations
        {
            get => _guidCollectionOperations;
            set => SetProperty(ref _guidCollectionOperations, value);
        }

        public AttributoSettingsGuidCollectionRiferimentoView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            Load();
        }

        public override void Load()
        {
            base.Load();
            LoadDetail();
        }

        protected override void LoadDetail()
        {
            LoadOperations();
        }



        public override bool IsSourceAttributoAllowed(Attributo att)
        {
            Attributo sourceAtt = _owner.EntitiesHelper.GetSourceAttributo(att);
            return AttributoSettingsGuidCollectionRiferimentoView.IsSourceAttributoAllowedS(sourceAtt);
        }

        public static bool IsSourceAttributoAllowedS(Attributo att)
        {
            if (att.IsInternal)
                return false;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale ||
                att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita ||
                att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo ||
                att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco ||
                att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
                return true;

            return false;

        }

        protected void LoadOperations()
        {
            Attributo sourceAtt = _owner.EntitiesHelper.GetSourceAttributo(AttRif);
            
            _guidCollectionOperationsType.Clear();
            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
            {
                _guidCollectionOperationsType.Add(ValoreOperationType.Sum);
                _guidCollectionOperationsType.Add(ValoreOperationType.Max);
                _guidCollectionOperationsType.Add(ValoreOperationType.Min);
                _guidCollectionOperationsType.Add(ValoreOperationType.Average);
                _guidCollectionOperationsType.Add(ValoreOperationType.Multiplication);
            }
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
            {
                _guidCollectionOperationsType.Add(ValoreOperationType.AppendWithSpace);
                _guidCollectionOperationsType.Add(ValoreOperationType.AppendNewLine);
                _guidCollectionOperationsType.Add(ValoreOperationType.Equivalent);
            }
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
            {
                _guidCollectionOperationsType.Add(ValoreOperationType.AppendWithSpace);
                _guidCollectionOperationsType.Add(ValoreOperationType.AppendNewLine);
                _guidCollectionOperationsType.Add(ValoreOperationType.Equivalent);
            }
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
            {
                _guidCollectionOperationsType.Add(ValoreOperationType.Equivalent);
            }
            _guidCollectionOperationIndexes = _guidCollectionOperationsType.ToDictionary(item => item, item => _guidCollectionOperationsType.IndexOf(item));
            GuidCollectionOperations = new ObservableCollection<string>(_guidCollectionOperationsType.Select(item => GetLocalizedOperationsName(item)));

            int currentIndex = -1;
            if (This.ValoreAttributo != null)
            {
                ValoreAttributoRiferimentoGuidCollection valAtt = This.ValoreAttributo as ValoreAttributoRiferimentoGuidCollection;
                if (_guidCollectionOperationIndexes.ContainsKey(valAtt.Operation))
                    currentIndex = _guidCollectionOperationIndexes[valAtt.Operation];
            }
            if (currentIndex < 0)
                currentIndex = _guidCollectionOperationIndexes[GetDefaultOperation(sourceAtt)];

            GuidCollectionOperationIndex = currentIndex;

        }

        private ValoreOperationType GetDefaultOperation(Attributo sourceAtt)
        {
            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                return ValoreOperationType.Sum;

            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                return ValoreOperationType.Equivalent;

            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
                return ValoreOperationType.Equivalent;

            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
                return ValoreOperationType.Equivalent;

            return ValoreOperationType.Nothing;
        }

        int _guidCollectionOperationIndex = -1;
        public int GuidCollectionOperationIndex
        {
            get => _guidCollectionOperationIndex;
            set
            {
                if (SetProperty(ref _guidCollectionOperationIndex, value))
                {
                    if (This.ValoreAttributo == null)
                        This.ValoreAttributo = new ValoreAttributoRiferimentoGuidCollection();

                    ValoreAttributoRiferimentoGuidCollection valAtt = This.ValoreAttributo as ValoreAttributoRiferimentoGuidCollection;
                    if (0 <= _guidCollectionOperationIndex && _guidCollectionOperationIndex < _guidCollectionOperationsType.Count)
                        valAtt.Operation = _guidCollectionOperationsType[_guidCollectionOperationIndex];
                }

            }
        }

        private string GetLocalizedOperationsName(ValoreOperationType op)
        {
            if (op == ValoreOperationType.Sum)
                return LocalizationProvider.GetString("Somma");
            if (op == ValoreOperationType.Equivalent)
                return LocalizationProvider.GetString("VisualizzaSeUguale");
            if (op == ValoreOperationType.AppendWithSpace)
                return LocalizationProvider.GetString("ConcatenaConSpazio");
            if (op == ValoreOperationType.AppendNewLine)
                return LocalizationProvider.GetString("ConcatenaACapo");
            if (op == ValoreOperationType.Average)
                return LocalizationProvider.GetString("Media");
            if (op == ValoreOperationType.Max)
                return LocalizationProvider.GetString("Massimo");
            if (op == ValoreOperationType.Min)
                return LocalizationProvider.GetString("Minimo");
            if (op == ValoreOperationType.Multiplication)
                return LocalizationProvider.GetString("Prodotto");

            return string.Empty;
        }
    }

    /// Rappresenta una riga della grigli attributi e detail di tipo variabile
    /// </summary>
    public class AttributoSettingsVariabileView : AttributoSettingsView
    {
        ObservableCollection<AttributoSettingsVariabileItemView> _variabiliItems = new ObservableCollection<AttributoSettingsVariabileItemView>();
        public ObservableCollection<AttributoSettingsVariabileItemView> VariabiliItems
        {
            get => _variabiliItems;
            set => SetProperty(ref _variabiliItems, value);
        }

        public AttributoSettingsVariabileView(AttributiSettingsView owner, Attributo a = null) : base(owner, a)
        {
            _owner = owner;
            Load();
        }

        public override void Load()
        {
            base.Load();
            LoadVariabili();
        }

        private void LoadVariabili()
        {
            _variabiliItems.Clear();

            AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(_owner.DataService);
            
            EntityType varType = _owner.DataService.GetEntityType(BuiltInCodes.EntityType.Variabili);
            if (varType == null)
                return;

            foreach (string codiceAtt in varType.Attributi.Keys)
            {
                Attributo att = varType.Attributi[codiceAtt];
                Valore val = _owner.EntitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.Variabili, codiceAtt, false, true);
                string formattedVal = string.Empty;

                if (val is ValoreReale)
                {
                    string format = attFormatHelper.GetValorePaddedFormat(att);
                    formattedVal = (val as ValoreReale).FormatRealResult(format);
                }
                else if (val is ValoreContabilita)
                {
                    string format = attFormatHelper.GetValorePaddedFormat(att);
                    formattedVal = (val as ValoreContabilita).FormatRealResult(format);
                }
                else
                    formattedVal = val.ToPlainText();


                _variabiliItems.Add(new AttributoSettingsVariabileItemView()
                {
                    CodiceAttributo = codiceAtt,
                    Etichetta = att.Etichetta,
                    Nome = String.Format("{0} ({1})", att.Etichetta, formattedVal)
                });
            }

            UpdateUI();
        }

        protected override void LoadGroupOperations()
        {
            List<GridGroupOperation> items = new List<GridGroupOperation>();
            GroupOperations = new ListCollectionView(items);

            
        }

        internal override void UpdateUI()
        {
            base.UpdateUI();
            RaisePropertyChanged(GetPropertyName(() => VariabiliItems));
        }

        public int VariabiliItemsIndex
        {
            get
            {
                int index = -1;
                if (This.ValoreAttributo != null)
                {
                    string codiceAtt = (This.ValoreAttributo as ValoreAttributoVariabili).CodiceAttributo;
                    index = VariabiliItems.IndexOf(VariabiliItems.FirstOrDefault(item => item.CodiceAttributo == codiceAtt));
                }
                return index;
            }
            set
            {
                string codiceAtt = VariabiliItems[value].CodiceAttributo;

                if (This.ValoreAttributo == null)
                    This.ValoreAttributo = new ValoreAttributoVariabili();

                (This.ValoreAttributo as ValoreAttributoVariabili).CodiceAttributo = codiceAtt;
            }
        }

    }

    public class AttributoSettingsVariabileItemView
    {
        public string CodiceAttributo { get; set; }
        public string Etichetta { get; set; }
        public string Nome { get; set; }
    }


#endregion

    /// <summary>
    /// Rappresenta un item del combo della colonna sezione della griglia tab riferimenti
    /// </summary>
    public class RiferimentiComboItem
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Category { get; set; }
    }

    /// <summary>
    /// Rappresenta un gruppo e il suo indice di inserimento di un item nella griglia
    /// </summary>
    public class GroupLastIndex
    {
        public string GroupName { get; set; }
        public int LastIndex { get; set; }
    }

    public class GridGroupOperation
    {
        public ValoreOperationType Key { get; set; }
        public string Name { get; set; }
    }


}
