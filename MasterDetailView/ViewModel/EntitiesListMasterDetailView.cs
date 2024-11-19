



using CommonResources;
using Commons;
using DevExpress.Mvvm;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using Microsoft.Win32;
using Model;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MasterDetailView
{
    public abstract class EntitiesListMasterDetailView : NotificationBase
    {
        //public static EntitiesListMasterDetailView This { get; set; }
        public RightPanesView RightPanesView { get; set; }
        public EntityType EntityType { get; set; }
        public ClientDataService DataService { get; set; } //ref
        //public SuggestionManager SuggestManager { get; set; }
        public IEntityWindowService WindowService { get; set; } //ref
        public ValoreCalculator Calculator { get; set; }
        public ModelActionsStack ModelActionsStack { get; set; } //ref
        public IMainOperation MainOperation { get; set; } //ref
        public MasterMappingNames MasterMappingNames { get; set; }
        public AttributoFormatHelper AttributoFormatHelper { get; set; }
        public EntitiesHelper EntitiesHelper { get; set; }

        public class EntityViewMasterInfo
        {
            public EntityViewMasterInfo() { }
            public EntityViewMasterInfo(EntityMasterInfo entInfo)
            {
                GroupKeys = entInfo.GroupKeys;

            }
            public List<string> GroupKeys
            {
                get;
                set;
            }



        }

        /// <summary>
        /// Informazioni sulle entità ritornate da ApplyFilterAndSort
        /// </summary>
        public Dictionary<Guid, EntityViewMasterInfo> FilteredEntitiesViewInfo = new Dictionary<Guid, EntityViewMasterInfo>();

        internal virtual void Clear()
        {
            FilteredEntitiesViewInfo.Clear();
        }

        /// <summary>
        /// Lista degli Id delle entità filtrate
        /// </summary>
        public List<Guid> FilteredEntitiesId { get; set; } = new List<Guid>();

        /// <summary>
        /// Comando in attesa di essere adempito
        /// </summary>
        public EntitiesListMasterDetailViewCommands PendingCommand { get; set; } = EntitiesListMasterDetailViewCommands.Nessuno;

        /// <summary>
        /// Gruppo corrente (esempio "CDD27;metri" se raggruppato per codice e unità di misura)
        /// </summary>
        public string CurrentGroupKey
        {
            get;
            set;
        }

        public HashSet<Guid> ReadyToModifyEntitiesId { get; set; } = new HashSet<Guid>();
        public ReadyToPasteEntitiesCommands ReadyToModifyEntitiesCommand = ReadyToPasteEntitiesCommands.Nothing;

        public bool IsModelToViewLoading { get; set; } = false;

        /// <summary>
        /// Ultimo indice di ModelActionsStack per il quale è stata chiamata la UpdateCache
        /// </summary>
        public int UpdateCacheModelActionIndex { get; set; } = 0;

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="entityType"></param>
        public EntitiesListMasterDetailView(/*EntityType entityType*/)
        {
            RightPanesView = new RightPanesView(this);
            _attributiEntities = new MultiDetailAttributiView(this);

        }

        virtual public void Init()
        {
            string msg = string.Empty;

            if (DataService == null)
                msg = "DataService == null";

            if (ModelActionsStack == null)
                msg = "ModelActionsStack == null";

            if (WindowService == null)
                msg = "WindowService == null";

            if (MainOperation == null)
                msg = "MainOperation == null";

            if (msg.Any())
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), msg);
            }

            MasterMappingNames = new MasterMappingNames();
            AttributoFormatHelper = new AttributoFormatHelper(DataService);
            EntitiesHelper = new EntitiesHelper(DataService);

            CheckedEntitiesId.Clear();
            IsMultipleModify = false;
            SelectIndex(-1);
            //_attributiEntities.Load(new HashSet<Guid>());


            if (ModelActionsStack != null)
                ModelActionsStack.ActionsChanged += ModelActionsStack_ActionsChanged;
        }

        public Dictionary<Guid, int> FilteredIndexes = new Dictionary<Guid, int>();

        public Dictionary<Guid, string> FilteredKeys { get; set; } = new Dictionary<Guid, string>();

        /// <summary>
        /// Richiesta al service di entità (id) filtrate e ordinate
        /// </summary>
        //public virtual async Task ApplyFilterAndSort(Guid? selectEntityId = null, bool searchOnly = false)
        public virtual void ApplyFilterAndSort(Guid? selectEntityId = null, bool searchOnly = false)
        {
            //Raccolgo le entità filtrate dal server...
            IsBusy = true;



            Dictionary<string, HashSet<string>> valoriUnivociAttributi = null;
            List<Guid> entitiesSearched = null;

            //Inizializza i dati relativi ai raggruppamenti
            RightPanesView.SortView.CreateData();
            RightPanesView.GroupView.CreateData();


            List<EntityMasterInfo> entsInfo = DataService.GetFilteredEntities(EntityType.GetKey(), RightPanesView.FilterView.Data, RightPanesView.SortView.Data, RightPanesView.GroupView.Data, out entitiesSearched);

            UpdateFilteredEntitiesId(entsInfo);
            FilteredEntitiesId = entsInfo.Select(item => item.Id).ToList();

            FilteredIndexes = FilteredEntitiesId.ToDictionary(item => item, item => -1);
            for (int i = 0; i < FilteredEntitiesId.Count; i++)
                FilteredIndexes[FilteredEntitiesId[i]] = i;

            FilteredKeys = entsInfo.ToDictionary(item => item.Id, item => item.ComparerKey);
            LoadAlert();

            if (!searchOnly)
            {
                Guid selectedEntityId = Guid.Empty;
                if (selectEntityId.HasValue)
                    _selectedEntityId = selectEntityId.Value;
                else
                    _selectedEntityId = Guid.Empty;

                _selectedIndex = -1;
            }

            RaisePropertyChanged(GetPropertyName(() => IsGrouped));

            RightPanesView.FilterView.UpdateSearch(entitiesSearched);

            PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;

            if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Nothing)
                ReadyToModifyEntitiesId.Clear();

            CheckedEntitiesId.IntersectWith(FilteredEntitiesId);
            IsMultipleModify = false;

            IsBusy = false;

        }



        void UpdateFilteredEntitiesId(List<EntityMasterInfo> entitiesInfo)
        {
            FilteredEntitiesViewInfo = entitiesInfo.ToDictionary(item => item.Id, item => new EntityViewMasterInfo(item));
        }

        public bool FilteredContains(Guid id)
        {
            if (FilteredIndexes.ContainsKey(id))
                return true;

            return false;
        }

        public int FilteredIndexOf(Guid id)
        {
            if (FilteredIndexes.ContainsKey(id))
                return FilteredIndexes[id];
            else
                return -1;
        }



        #region ScrollPosition
        public virtual string GetRelativeScrollPosition() { return null; }

        protected string ScrollPosition { get; private set; }
        public void SaveScrollPosition()
        {
            ScrollPosition = GetRelativeScrollPosition();

        }
        #endregion

        MultiDetailAttributiView _attributiEntities = null;
        public MultiDetailAttributiView AttributiEntities
        {
            get
            {
                return _attributiEntities;
            }

            set
            {
                _attributiEntities = value;
            }
        }

        public virtual Attributo GetAttributoByCode(string attCode)
        {
            if (EntityType.Attributi.ContainsKey(attCode))
                return EntityType.Attributi[attCode];

            return null;
            //return EntityType.Attributi.FirstOrDefault(item => item.Codice == attCode);
        }

        /// <summary>
        /// Ritorna l'attributo di origine
        /// </summary>
        /// <param name="att"></param>
        /// <returns></returns>
        public Attributo GetSourceAttributoOf(Attributo att)
        {
            Attributo sourceAtt = att;
            while (sourceAtt is AttributoRiferimento)
            {
                AttributoRiferimento attRif = sourceAtt as AttributoRiferimento;
                var entTypes = DataService.GetEntityTypes();
                if (!entTypes.ContainsKey(attRif.ReferenceEntityTypeKey))
                    return null;

                EntityType sourceEntType = entTypes[attRif.ReferenceEntityTypeKey];

                sourceAtt = sourceEntType.Attributi[attRif.ReferenceCodiceAttributo];
            }
            return sourceAtt;
        }



        ///// <summary>
        ///// Lista delle Entità filtrate alcune realizzate altre null
        ///// </summary>  
        //IList<VirtualListItem<EntityView>> _entities = null;
        //public IList<VirtualListItem<EntityView>> Entities
        //{
        //    get { return _entities; }
        //    set { SetProperty(ref _entities, value); }
        //}
        ObservableCollection<EntityView> _entities = null;
        public virtual ObservableCollection<EntityView> Entities
        {
            get { return _entities; }
            set { SetProperty(ref _entities, value); }
        }


        /// <summary>
        /// Lista delle entità realizzate
        /// </summary>
        public List<EntityView> EntitiesViewCached
        {
            get
            {
                List<EntityView> entitiesViewCached = new List<EntityView>();
                if (Entities != null)
                {
                    for (int i = 0; i < Entities.Count; i++)
                    {
                        if (Entities[i] != null && Entities[i] != null)
                        {
                            EntityView entView = Entities[i] as EntityView;
                            entitiesViewCached.Add(entView);
                        }
                    }
                }
                return entitiesViewCached;
            }
        }

        #region Selection

        /// <summary>
        /// Indice entità selezionata
        /// </summary>
        protected int _selectedIndex = -1;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                SelectIndex(value, true);
            }
        }

        public virtual EntityView GetEntityViewByIndex(int index) { return null; }

        public virtual void SelectIndex(int index, bool userOrigin = false)
        {
            if (SetProperty(ref _selectedIndex, index))
            {
                //if (_selectedIndex < 0)
                //    return;

                EntityView entView = GetEntityViewByIndex(_selectedIndex);
                if (entView != null)
                {
                    _selectedEntityId = entView.Id;
                    SelectEntityView(entView);
                    SetNoSelectionChecked(false);
                    RightPanesView.FilterView.UpdateCurrentEntityIndexFound(_selectedEntityId);
                }
                else
                {
                    _selectedIndex = -1;
                    _selectedEntityId = Guid.Empty;//AU 03/09/2019
                    _selectedEntityView = null;

                }

                if (userOrigin && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify)
                {
                    IsMultipleModify = false;
                    ReadyToModifyEntitiesId.Clear();

                }

                UpdateUI();
            }
        }



        public bool UpdateDetail { get; set; } = false;

        public void ClearFocus()
        {
            _selectedEntityId = Guid.Empty;
            _selectedIndex = -1;
        }

        /// <summary>
        /// Selezione di un'entià
        /// </summary>
        /// <param name="entView">entView = null => nessun selezionato</param>
        public virtual bool SelectEntityView(EntityView entView)
        {

            if (entView == null && _selectedEntityView != null)
            {
                _selectedEntityView = null;
                AttributiEntities.Load(new HashSet<Guid>());
                SetCurrentGroupKey();

                return false;
            }


            if (_selectedEntityView != null && _selectedEntityView.Id == entView.Id)
            {
                if (_selectedEntityView != entView)
                {
                    _selectedEntityView = entView;
                }

                return false;
            }
            Guid precSelectedId = _selectedEntityId;

            _selectedEntityView = entView;

            //AttributiEntities.Load(entView);
            LoadAttributiDetailAsync(entView);

            SetCurrentGroupKey();

            RightPanesView.FilterView.UpdateIsSearchEnabled();

            return true;
        }

        /// <summary>
        /// non funziona perchè spariscono i gruppi degli attributi nel dettaglio //obsoleto
        /// si impianta se si apre 2 volte consecutive il file //obsoleto
        /// </summary>
        /// <param name = "entView" ></ param >
        protected void LoadAttributiDetailAsync(EntityView entView)
        {
            //await Task.Run(() => AttributiEntities.Load(entView));
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => AttributiEntities.Load(entView));

        }



        /// <summary>
        /// Entità selezionata
        /// </summary>
        protected EntityView _selectedEntityView = null;
        public EntityView SelectedEntityView { get { return _selectedEntityView; } }

        /// <summary>
        /// Id Entità selezionata
        /// </summary>
        protected Guid _selectedEntityId
        {
            get;
            set;
        } = Guid.Empty;
        public Guid SelectedEntityId { get { return _selectedEntityId; } }

        public virtual async Task SelectEntityById(Guid guid, bool scrollToEntity = true) { }


        public bool IsAnySelected
        {
            get { return SelectedIndex >= 0; }
        }

        public ICommand CheckAllCommand
        {
            get
            {
                return new CommandHandler(() => this.CheckAll());
            }
        }

        public virtual void CheckAll()
        {
            if (FilteredEntitiesId.Any())
            {
                CheckedEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
                UpdateSelectedItems(true);
                UpdateUI();
            }
        }

        public ICommand CheckUnreferencedCommand { get {return new CommandHandler(() => this.CheckUnreferenced()); }}
        protected virtual void CheckUnreferenced()
        {
            HashSet<Guid> unrefIds = new HashSet<Guid>(FilteredEntitiesId);
            foreach (Guid id in FilteredEntitiesId)
            {
                List<string> sezioniKey = new List<string>();
                List<string> depEntTypesKey = EntitiesHelper.GetDependentEntityTypesKey(EntityType.GetKey());
                foreach (string depEntType in depEntTypesKey)
                {
                    List<Guid> depIds = DataService.GetDependentIds(EntityType.GetKey(), id, depEntType);
                    if (depIds != null && depIds.Count > 0)
                    {
                        unrefIds.Remove(id);
                    }
                }
            }

            ClearFocus();

            ShowEntities(unrefIds.ToList());
            CheckedEntitiesId = unrefIds;

            UpdateSelectedItems();
            UpdateUI();



        }


        #endregion Selection

        protected HashSet<Guid> _checkedEntitiesId = new HashSet<Guid>();
        public virtual HashSet<Guid> CheckedEntitiesId
        {
            get { return _checkedEntitiesId; }
            set { _checkedEntitiesId = value; }
        }

        /// <summary>
        /// Caricamento delle entità in Master ListView
        /// </summary>
        public async virtual void Load()
        {
            ////Per gli evidenziatori
            //if (IsAnyHighlighter())
            //{
            //    DataService.CalcolaEntities(EntityType.GetKey(), new EntityCalcOptions(false, false), null, new EntitiesError());
            //    LoadAlert();
            //}

            LoadAlert();
            UpdateUI();
        }

        public bool IsAnyHighlighter()
        {
            if (DataService == null)
                return false;


            ViewSettings viewSettings = DataService.GetViewSettings();
            EntityTypeViewSettings entTypeViewSettings = null;
            if (viewSettings.EntityTypes.TryGetValue(EntityType.GetKey(), out entTypeViewSettings))
            {
                if (entTypeViewSettings.EntityHighlighters != null)
                    if (!string.IsNullOrEmpty(entTypeViewSettings.EntityHighlighters.CodiceAttributo))
                    {
                        if (!EntityType.Attributi.ContainsKey(entTypeViewSettings.EntityHighlighters.CodiceAttributo))
                        {
                            //se l'attributo non esiste viene tolto dagli evidenziatori
                            entTypeViewSettings.EntityHighlighters = new EntityHighlighters();
                            DataService.SetViewSettings(viewSettings);
                        }
                        else return true;
                    }
            }

            return false;
        }

        public virtual void UpdateUI()
        {
            //UpdateIsAllChecked();

            RaisePropertyChanged(GetPropertyName(() => CheckedEntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => EntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => IsPasteEntitiesEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMoveEntitiesAfterEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsAddEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsAnyChecked));
            RaisePropertyChanged(GetPropertyName(() => IsAnySelected));

            RaisePropertyChanged(GetPropertyName(() => IsMultipleModifyEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMultipleModifyNotificationEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsScrollToCurrentEntityEnabled));
            RaisePropertyChanged(GetPropertyName(() => AttributiEntities));
            RaisePropertyChanged(GetPropertyName(() => IsAnyReadyToPaste));
            RaisePropertyChanged(GetPropertyName(() => ReadyToPasteEntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => PasteEntitiesToolTip));
            RaisePropertyChanged(GetPropertyName(() => AddEntityToolTip));
            RaisePropertyChanged(GetPropertyName(() => IsNoSelectionChecked));

            RaisePropertyChanged(GetPropertyName(() => IsAddEntityEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMoveEntitiesEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCopyEntitiesEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMoveEntitiesNotificationEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCopyEntitiesNotificationEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsEscapeEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsColumnChooserEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsExpandAllEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCollapseAllEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMultipleModifyEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsDeleteEntitiesEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsAdvancedMode));
            RaisePropertyChanged(GetPropertyName(() => IsImportItemsEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCodingItemsEnabled));
            RaisePropertyChanged(GetPropertyName(() => CodingItemsToolTip));
            RaisePropertyChanged(GetPropertyName(() => IsEntityHighlightersEnabled));
            RaisePropertyChanged(GetPropertyName(() => AlertItems));
            RaisePropertyChanged(GetPropertyName(() => IsAlertOpen));
            RaisePropertyChanged(GetPropertyName(() => IsAlertVisible));

            AttributiEntities.UpdateUI();

            if (Entities != null)
            {
                foreach (EntityView entView in Entities)
                {
                    if (entView != null)
                        entView.UpdateUI();
                }
            }

            RaisePropertyChanged(GetPropertyName(() => SelectedIndex));



        }

        /// <summary>
        /// Update Check box "All Checked" 
        /// </summary>
        public virtual void UpdateIsAllChecked()
        {
            //if (CheckedEntitiesId.Any())
            //{
            //    //HashSet<Guid> filteredEntitiesId = new HashSet<Guid>(FilteredEntities.Select(item => item.Id).ToList());
            //    HashSet<Guid> filteredEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
            //    if (filteredEntitiesId.SequenceEqual(CheckedEntitiesId))
            //        _isAllChecked = true;
            //    else
            //    {
            //        if (filteredEntitiesId.Overlaps(CheckedEntitiesId))
            //            _isAllChecked = null;
            //        else
            //            _isAllChecked = false;
            //    }
            //}
            //else
            //{
            //    _isAllChecked = false;
            //}


            //RaisePropertyChanged(GetPropertyName(() => this.IsAllChecked));
        }

        public virtual void UpdateSelectedItems(bool selectAll = false)
        {
        }

        public async virtual Task UpdateCache(bool updateDetail = false) { }


        public virtual bool IsMultipleModifyEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsMoveEntitiesAfterEnabled)
                    return false;

                if (CheckedEntitiesId.Count > 1)
                    return true;
                return false;
            }
        }

        public bool IsMultipleModifyNotificationEnabled
        {
            get
            {
                return IsAnyReadyToPaste && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify && IsRestrictedCommandsMode();
            }
        }

        public virtual void OnCheckedEntity()
        {
            if (IsModelToViewLoading)
                return;

            //if (ModelState.ModelToViewLoading)
            //    return;



            //UpdateIsAllChecked();

            RaisePropertyChanged(GetPropertyName(() => this.CheckedEntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => this.IsAnyChecked));
            RaisePropertyChanged(GetPropertyName(() => IsMultipleModifyEnabled));

            if (!IsAnyChecked)
                IsMultipleModify = false;

            if (IsMultipleModify)
                AttributiEntities.Load(CheckedEntitiesId);


        }


        /// <summary>
        /// Guid dell'ultima entità (nel tempo) di cui è stato modificato lo stato del check
        /// </summary>
        public Guid FirstShiftCheckEntityId { get; set; }

        public virtual void CheckEntityById(Guid id, bool check)
        {
            if (check)
                CheckedEntitiesId.Add(id);
            else
                CheckedEntitiesId.Remove(id);

            //LastEntityCheckStateChanged = id;

        }

        public void CheckEntitiesById(IEnumerable<Guid> ids, bool check)
        {

            foreach (Guid id in ids)
            {
                CheckEntityById(id, check);
            }

            //if (check)
            //    CheckedEntitiesId.UnionWith(ids);
            //else
            //    CheckedEntitiesId.ExceptWith(ids);

            //EntitiesViewCached.ForEach(item => { if (ids.Contains(item.Id)) item.SetChecked(check); });

            //if (ids.Any())
            //    LastEntityCheckStateChanged = ids.Last();

        }

        public void OnShiftChecked(Guid id)
        {


            int currIndex = FilteredIndexOf(id);
            int lastIndex = FilteredIndexOf(FirstShiftCheckEntityId);
            if (lastIndex < 0)
                lastIndex = 0;// currIndex;

            int startIndex = Math.Min(lastIndex, currIndex);
            int endIndex = Math.Max(lastIndex, currIndex);

            IEnumerable<Guid> ids = FilteredEntitiesId.Skip(startIndex).Take(endIndex - startIndex + 1);
            CheckEntitiesById(ids, true);

            OnCheckedEntity();
            EntitiesViewCached.ForEach(item => item.UpdateUI());

        }

        public virtual void ClearCheckedEntities()
        {

            CheckedEntitiesId.Clear();

            //UpdateIsAllChecked();

            RaisePropertyChanged(GetPropertyName(() => this.CheckedEntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => this.IsAnyChecked));
            if (!IsAnyChecked)
                IsMultipleModify = false;
            if (IsMultipleModify)
                AttributiEntities.Load(CheckedEntitiesId);


        }

        string _relativeScrollPosition;
        public string RelativeScrollPosition
        {
            get
            {
                return _relativeScrollPosition;
            }
            set
            {
                SetProperty(ref _relativeScrollPosition, value);
            }
        }

        string _entitiesUpdatingScrollMode = "KeepItemsInView";//KeepLastItemInView
        public string EntitiesUpdatingScrollMode
        {
            get { return _entitiesUpdatingScrollMode; }
            set { SetProperty(ref _entitiesUpdatingScrollMode, value); }
        }


        //bool? _isAllChecked = false;
        //public bool? IsAllChecked
        //{
        //    get
        //    {
        //        return _isAllChecked;
        //    }
        //    set
        //    {
        //        if (SetProperty(ref _isAllChecked, value))
        //        {

        //            if (_isAllChecked == true)
        //            {
        //                //CheckedEntitiesId = new HashSet<Guid>(FilteredEntitiesId.Select(item => item.Id).ToList());
        //                CheckedEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
        //                EntitiesViewCached.ForEach(item => item.SetChecked(true));
        //            }
        //            else if (_isAllChecked == false)
        //            {
        //                CheckedEntitiesId = new HashSet<Guid>();
        //                EntitiesViewCached.ForEach(item => item.SetChecked(false));
        //            }
        //            OnCheckedEntity();
        //            EntitiesViewCached.ForEach(item => item.UpdateUI());
        //            //SaveScrollPosition();
        //            UpdateCache();

        //        }
        //    }

        //}

        bool _is3State = false;
        public bool Is3State
        {
            get { return _is3State; }
            set
            {
                SetProperty(ref _is3State, value);
            }
        }

        /// <summary>
        /// Numero entità visualizzate in Master ListView
        /// </summary>
        public string EntitiesCount
        {
            //get { return "("+FilteredEntities.Count.ToString()+")"; }
            get { return "(" + CheckedEntitiesId.Count.ToString() + "/" + FilteredEntitiesId.Count.ToString() + ")"; }
        }


        /// <summary>
        /// Numero di entità checkate in Master ListView
        /// </summary>
        public string CheckedEntitiesCount
        {
            get
            {
                return string.Format("{0}/{1} {2}", CheckedEntitiesId.Count.ToString(), FilteredEntitiesId.Count.ToString(),
                    LocalizationProvider.GetString("Selezionate"));
            }
        }
        public virtual bool IsAnyChecked
        {
            get
            {
                if (EntityType != null && EntityType.Codice == "ElementiItem")
                {

                }

                RaisePropertyChanged(GetPropertyName(() => IsMoveEntitiesAfterEnabled));
                RaisePropertyChanged(GetPropertyName(() => this.IsPasteEntitiesEnabled));
                return CheckedEntitiesId.Count > 0;
            }
        }


        /// <summary>
        /// Modifica Multipla di entità
        /// </summary>
        protected bool _multipleModify = false;
        public virtual bool IsMultipleModify
        {
            get { return _multipleModify; }
            set
            {
                if (SetProperty(ref _multipleModify, value))
                {
                    if (_multipleModify)
                    {
                        AttributiEntities.Load(CheckedEntitiesId);
                        ReadyToModifyEntitiesId = new HashSet<Guid>(CheckedEntitiesId);
                        ReadyToModifyEntitiesCommand = ReadyToPasteEntitiesCommands.MultiModify;
                    }
                    else
                    {
                        ReadyToModifyEntitiesId.Clear();

                        if (SelectedEntityView != null)
                            AttributiEntities.Load(SelectedEntityView);
                        else
                            AttributiEntities.Load(new HashSet<Guid>());
                    }
                    UpdateUI();


                }
            }

        }


        public string ReadyToPasteEntitiesCount { get => ReadyToModifyEntitiesId.Count.ToString(); }

        public ICommand DeleteCheckedCommand
        {
            get
            {
                return new CommandHandler(() => this.DeleteCheckedEntities());
            }
        }

        /// <summary>
        /// Cancellazione Entità (flag deleted)
        /// </summary>
        public virtual async void DeleteCheckedEntities()
        {
            //ModelActionResponse mar = ModelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.ENTITY_DELETE, EntitiesId = CheckedEntitiesId, EntityTypeKey = this.EntityType.GetKey() }, this);
            ModelActionResponse mar = CommitAction(new ModelAction() { ActionName = ActionName.ENTITY_DELETE, EntitiesId = CheckedEntitiesId, EntityTypeKey = this.EntityType.GetKey() });
            if (mar.ActionResponse == ActionResponse.OK)
            {
                Guid newSelectedId = Guid.Empty;

                //La nuova selezione sarà la prima entità (non cancellata) dopo di SelectedEntityId
                newSelectedId = SelectedEntityId;
                while (CheckedEntitiesId.Contains(newSelectedId))
                {
                    newSelectedId = FindNextInCurrentGroup(newSelectedId);
                }

                ApplyFilterAndSort(newSelectedId);
                if (newSelectedId != Guid.Empty)
                    CheckedEntitiesId = new HashSet<Guid>() { newSelectedId };
                else
                    CheckedEntitiesId.Clear();

                SelectEntityView(null);

                LoadAlertByAction(mar);
                UpdateCache();

                IsMultipleModify = false;

                UpdateUI();
            }
        }

        public bool IsDeleteEntitiesEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return IsAnyChecked && !IsMoveEntitiesAfterEnabled;
            }
        }
        public bool IsColumnChooserEnabled
        {
            get
            {
                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }


        /// <summary>
        /// Pannello filtro aperto
        /// </summary>
        public bool IsFilterPaneOpen
        {
            get { return RightPanesView.IsFilterPaneOpen; }
            set { SetProperty(RightPanesView.IsFilterPaneOpen, value, () => RightPanesView.IsFilterPaneOpen = value); }
        }

        /// <summary>
        /// Pannello ordinamento aperto
        /// </summary>
        public bool IsSortPaneOpen
        {
            get { return RightPanesView.IsSortPaneOpen; }
            set { SetProperty(RightPanesView.IsSortPaneOpen, value, () => RightPanesView.IsSortPaneOpen = value); }
        }



        //public bool ValoriAttributiReadOnly { get; set; } = false;

        //internal virtual void OnAttributoValueSelected(ValoreView valoreCollectionView)
        //{
        //}

        //internal virtual void OnAttributoValueRemoved(ValoreView valoreView)
        //{
        //}

        public virtual void ReplaceValore(ValoreView valoreView) { }

        public ICommand AddEntityCommand
        {
            get
            {
                return new CommandHandler(() => this.AddEntity());
            }
        }

        /// <summary>
        /// Controlla se il filtro attivo consente di aggiungere la voce
        /// </summary>
        /// <returns></returns>
        protected bool IsValidAddRequestByFilter()
        {
            //Se è stato filtrato per un attributo di tipo riferimento non posso aggiungere (altrimenti il filtro lo escluderebbe)
            AttributoFilterData attFilterDataRif = RightPanesView.FilterView.Data.Items.FirstOrDefault(item => item.IsFiltroAttivato && item.CodiceAttributo != item.SourceCodiceAttributo);
            if (attFilterDataRif != null)
            {
                string str = string.Format("{0} {1} {2}", LocalizationProvider.GetString("NonPossibileAssociareAutomaticamenteIlFiltro"),
                    LocalizationProvider.GetString("AttivoFiltroPerAttributoRiferito"),
                    LocalizationProvider.GetString("LaVoceNonSaraCreata"));
                MainOperation.ShowMessageBarView(str);
                return false;
            }

            //Se il risultato del filtrato è più di una voce non posso aggiungere (altrimenti il filtro lo escluderebbe)
            attFilterDataRif = RightPanesView.FilterView.Data.Items.FirstOrDefault(item => item.IsFiltroAttivato && item.CheckedValori.Count != 1);
            if (attFilterDataRif != null)
            {
                string str = string.Format("{0} {1} {2}", LocalizationProvider.GetString("NonPossibileAssociareAutomaticamenteIlFiltro"),
                    LocalizationProvider.GetString("AttivoFiltroConRisultatoMultiplo"),
                    LocalizationProvider.GetString("LaVoceNonSaraCreata"));
                MainOperation.ShowMessageBarView(str);
                //MessageBox.Show(str, LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        virtual async public void AddEntity()
        {
            try
            {
                if (!IsValidAddRequestByFilter())
                    return;

                ModelActionResponse mar;
                ModelAction action = new ModelAction() { EntityTypeKey = this.EntityType.GetKey() };

                Entity targetEntity = GetActionTargetEntity();

                if (RightPanesView.FilterView.IsFilterApplied() && targetEntity == null)
                {
                    MainOperation.ShowMessageBarView(LocalizationProvider.GetString("SelezionareTargetSeFiltroAttivo"));
                    return;
                }

                SetValoriOfTargetGroups(action, targetEntity);
                bool clearAttributiFilter = SetValoriOfCurrentFilter(action/*, targetEntity*/);

                //if (SelectedIndex >= 0 && SelectedIndex < Entities.Count - 1) //inserisco dopo SelectedIndex
                if (targetEntity != null)
                {
                    action.ActionName = ActionName.ENTITY_INSERT;
                    action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = targetEntity.EntityId, TargetReferenceName = TargetReferenceName.AFTER } };
                    //mar = ModelActionsStack.CommitAction(action, this);
                    mar = CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        if (clearAttributiFilter)
                            RightPanesView.FilterView.ClearNextAttributiFilter(0);

                        CheckedEntitiesId.Clear();
                        CheckedEntitiesId.Add(mar.NewId);
                        ApplyFilterAndSort(mar.NewId);

                        PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                        PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;
                        await UpdateCache();
                    }
                }
                else //aggiungo in coda
                {
                    if (FilteredEntitiesId.Any())
                    {
                        action.ActionName = ActionName.ENTITY_INSERT;
                        action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = FilteredEntitiesId.Last(), TargetReferenceName = TargetReferenceName.AFTER } };
                    }
                    else
                    {
                        action.ActionName = ActionName.ENTITY_ADD;
                    }
                    //mar = ModelActionsStack.CommitAction(action, this);
                    mar = CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        if (clearAttributiFilter)
                            RightPanesView.FilterView.ClearNextAttributiFilter(0);

                        CheckedEntitiesId.Clear();
                        CheckedEntitiesId.Add(mar.NewId);
                        ApplyFilterAndSort(mar.NewId);

                        PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandNewEntitiesGroup;//per l'espansione dopo aver aggiunto un nuovo item di cui non esistono ancora i raggruppamenti
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                        await UpdateCache();

                        //CheckedEntitiesId.Clear();
                        //CheckedEntitiesId.Add(mar.NewId);

                        //Load();
                        //PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                        //PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;

                        //SelectEntityById(mar.NewId);
                    }

                }

                RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message, exc);
            }

        }

        public bool IsAddEntityEnabled
        {
            get
            {

                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }

        ///// <summary>
        ///// Commit nel server degli attributi per questo tipo di entità
        ///// </summary>
        //internal void SetAttributo(Attributo att)
        //{
        //    ModelAction modelAction = new ModelAction();
        //    modelAction.ActionName = ActionName.ATTRIBUTO_MODIFY;
        //    modelAction.AttributoCode = att.Codice;
        //    modelAction.EntityTypeKey = EntityType.GetKey();

        //    string json = null;
        //    JsonSerializer.JsonSerialize(att, out json);

        //    modelAction.JsonSerializedObjectType = att.GetType();
        //    modelAction.JsonSerializedObject = json;

        //    ModelActionResponse mar = ModelActionsStack.CommitAction(modelAction, this);
        //    if (mar.ActionResponse == ActionResponse.OK)
        //    {
        //        EntityType.Attributi[att.Codice] = att;
        //    }
        //}

        public bool IsAddEnabled
        {
            get
            {
                if (!RightPanesView.SortView.IsApplied()/* && !FilterView.IsFilterApplied()*/)
                {

                    if (Entities == null)
                        LocalizationProvider.GetString("Aggiungi in coda");
                        //AddToolTip = StringResoucesTemp.GetResourceString("MasterListView_Add");
                    else
                    {

                        //EntityView entView = Entities2.FirstOrDefault(item => SelectedEntitiesId.Contains(item.Id));
                        if (SelectedEntityView != null)
                            LocalizationProvider.GetString("Aggiungi dopo l'elemento corrente");
                        //AddToolTip = StringResoucesTemp.GetResourceString("MasterListView_InsertAfter")/* + " \"" + SelectedEntityView.EntityAttributoView00 + "\""*/;
                        else
                            LocalizationProvider.GetString("Aggiungi in coda");
                        //AddToolTip = StringResoucesTemp.GetResourceString("MasterListView_Add");
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        string _addToolTip = "";
        public string AddToolTip
        {
            get { return _addToolTip; }
            set { SetProperty(ref _addToolTip, value); }
        }

        #region Move - Sposta (Entities Cut & Paste)


        public ICommand PasteEntitiesCommand
        {
            get
            {
                return new CommandHandler(() => this.PasteEntities());
            }
        }

        public void PasteEntities()
        {
            if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move)
                this.MoveEntities();
            else
                this.PasteClipboardEntities();

//#if !ALE_ASYNC_GROUP
//            RightPanesView.GroupView.Update(string.Empty, CurrentGroupKey);
//#endif
        }


        /// <summary>
        /// Completamento del comando MUOVI entità nella master list
        /// </summary>
        public virtual async void MoveEntities()
        {
            if (SelectedEntityId == null)
                return;

            List<Guid> entitiesCuttedId = FilteredEntitiesId.Where(item => ReadyToModifyEntitiesId.Contains(item)).ToList();

            Guid targetEntityId = FilteredEntitiesId.FirstOrDefault(item => item == SelectedEntityId);

            if (targetEntityId == null)
                return;


            ModelAction action = new ModelAction() { EntityTypeKey = EntityType.GetKey(), ActionName = ActionName.ENTITY_MOVE };

            List<int> oldIndexes = new List<int>();
            List<int> newIndexes = new List<int>();

            //Calcolo i nuovi indici degli elementi spostati
            int targetEntityIndex = FilteredIndexOf(targetEntityId);
            int newIndex = targetEntityIndex;
            foreach (Guid id in entitiesCuttedId)
            {
                int oldIndex = FilteredIndexOf(id);
                oldIndexes.Add(oldIndex);

                if (oldIndex < targetEntityIndex)
                    newIndex--;
            }

            for (int i = 0; i < entitiesCuttedId.Count; i++)
            {
                newIndex++;
                action.EntitiesId.Add(entitiesCuttedId[i]);
                newIndexes.Add(newIndex);

                if (i == 0)
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = targetEntityId, TargetReferenceName = TargetReferenceName.AFTER });
                else
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = entitiesCuttedId[i - 1], TargetReferenceName = TargetReferenceName.AFTER });
            }


            Entity targetEntity = GetActionTargetEntity();



            //Scopo: Controllare se le entità copiate sono tutte provenienti dallo stesso gruppo del target
            bool IsReadyToModifyEntitiesSameGroup = true;
            string sourceGroupKey = null;
            foreach (Guid id in ReadyToModifyEntitiesId)
            {
                string groupKey = GroupData.JoinGroupKeys(GetGroupsKeyById(id).ToArray());

                if (sourceGroupKey == null)
                {
                    sourceGroupKey = groupKey;
                }
                else if (sourceGroupKey != groupKey)
                {
                    IsReadyToModifyEntitiesSameGroup = false;
                    break;
                }
            }


            #region Elaborazione necessaria per il messaggio
            //
            bool isGroupedByElementiItemRif = false;
            bool isGroupedByPrezzarioItemRif = false;
            foreach (AttributoGroupData attGroupData in RightPanesView.GroupView.Data.Items)
            {
                AttributoRiferimento attRif = EntityType.Attributi[attGroupData.CodiceAttributo] as AttributoRiferimento;
                if (attRif != null)
                {
                    if (attRif.ReferenceEntityTypeKey == BuiltInCodes.EntityType.Elementi)
                        isGroupedByElementiItemRif = true;

                    if (attRif.ReferenceEntityTypeKey == BuiltInCodes.EntityType.Prezzario)
                        isGroupedByPrezzarioItemRif = true;
                }
            }


            //Se gli elementi pronti ad essere incollati non sono dello stesso gruppo o sono dello stesso gruppo ma di un gruppo differente dal target
            if ((isGroupedByElementiItemRif || isGroupedByPrezzarioItemRif) && (!IsReadyToModifyEntitiesSameGroup || sourceGroupKey != CurrentGroupKey))
            {
                MessageBoxResult res = MessageBox.Show(LocalizationProvider.GetString("ConfermaAssegnazioneElementoArticoloTarget"),
                    LocalizationProvider.GetString("AppName"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (res == MessageBoxResult.No)
                    return;
            }
            #endregion


            if (sourceGroupKey != CurrentGroupKey)
                SetValoriOfTargetGroups(action, targetEntity);

            HashSet<Guid> readyToPasteEntitiesId = new HashSet<Guid>(ReadyToModifyEntitiesId);

            //ModelActionResponse mar = ModelActionsStack.CommitAction(action, this);
            ModelActionResponse mar = CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                CheckedEntitiesId = readyToPasteEntitiesId;

                ApplyFilterAndSort(CheckedEntitiesId.First());
                await UpdateCache();
                //UpdateIsAllChecked();
            }

        }

        public bool IsPasteEntitiesEnabled
        {
            get
            {
                if (FilteredEntitiesId == null || !FilteredEntitiesId.Any())
                    return false;

                if (!RightPanesView.SortView.IsApplied())
                {
                    HashSet<Guid> filteredEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
                    if (filteredEntitiesId.Overlaps(ReadyToModifyEntitiesId) && SelectedEntityView != null)
                    {
                        if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify)
                            return false;

                        //if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move)
                        //    return false;

                        if (ReadyToModifyEntitiesCommand != ReadyToPasteEntitiesCommands.Copy && ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                            return false;

                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsMoveEntitiesAfterEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (FilteredEntitiesId == null || !FilteredEntitiesId.Any())
                    return false;

                //if (!RightPanesView.SortView.IsApplied())
                //{
                HashSet<Guid> filteredEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
                if (filteredEntitiesId.Overlaps(ReadyToModifyEntitiesId) && SelectedEntityView != null)
                {
                    if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify)
                        return false;

                    if (IsRestrictedCommandsMode())
                        return false;

                    if (ReadyToModifyEntitiesCommand != ReadyToPasteEntitiesCommands.Copy && ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                        return false;

                    return true;
                }
                //}

                return false;
            }
        }

        string _pasteEntitiesToolTip = "";
        public string PasteEntitiesToolTip
        {
            get { return _pasteEntitiesToolTip; }
            set { SetProperty(ref _pasteEntitiesToolTip, value); }
        }

        public string AddEntityToolTip
        {
            get
            {
                if (SelectedEntityView != null)
                {
                    string str = SelectedEntityView.Attributo1 != null && SelectedEntityView.Attributo1.Any() ? SelectedEntityView.Attributo1 : SelectedEntityView.Attributo2;
                    return LocalizationProvider.GetString("Aggiungi dopo l'elemento corrente");
                    //return StringResoucesTemp.GetResourceString("MasterListView_InsertAfter")/* + " \"" + str + "\""*/;
                }
                else
                    return LocalizationProvider.GetString("Aggiungi in coda");
                //return StringResoucesTemp.GetResourceString("MasterListView_Add");
            }
        }

        //public void MoveEntity(int oldIndex, int newIndex)
        //{
        //    var item = FilteredEntities[oldIndex];
        //    FilteredEntities.RemoveAt(oldIndex);
        //    FilteredEntities.Insert(newIndex, item);
        //}

        #endregion Move - Sposta (Entities Cut & Paste)


        #region Copy&Paste



        public ICommand RightTappedCommand
        {
            get
            {
                return new CommandHandler(() => this.RightTapped());
            }
        }

        void RightTapped()
        {
            //Globals.CurrentEntitiesMasterDetailView.SelectedIndex = -1;

            //if (MasterDetailViewManager.CurrentEntitiesMasterDetailView.SelectedIndex == -1)
            //    MasterDetailViewManager.UpdateClipboardState();

            RaisePropertyChanged(GetPropertyName(() => IsPasteClipboardEnabled));
        }


        public bool IsPasteClipboardEnabled
        {
            get { return SelectedIndex == -1; }/*!MasterDetailViewManager.IsClipboardEmpty &&*/
        }

        public ICommand PasteClipboardCommand
        {
            get
            {
                return new CommandHandler(() => this.PasteClipboard());
            }
        }

        public void PasteClipboard()
        {
            PasteClipboardEntities();
        }

        //public virtual void CopyClipboardEntities(bool withHeaders, IEnumerable<Guid> entsId = null, IEnumerable<string> selectionAttsCode = null)
        //{

        //    IsBusy = true;

        //    try
        //    {
        //        Mouse.OverrideCursor = Cursors.Wait;

        //        EntityCollection entityCollection = new EntityCollection();

        //        if (entsId == null)
        //            entsId = FilteredEntitiesId.Where(item => CheckedEntitiesId.Contains(item));

        //        entityCollection.Entities = DataService.GetEntitiesById(EntityType.GetKey(), entsId);

        //        Clipboard.Clear();

        //        DataObject dataObject = new DataObject();

        //        //////////////////////JSON////////////////////////////
        //        //Json (copia l'intera entità a prescindere da selectionAttsCode)
        //        string json = null;
        //        JsonSerializer.JsonSerialize(entityCollection, out json);
        //        dataObject.SetData(EntityType.GetKey(), json);

        //        ///////////////////////TEXT///////////////////////////
        //        string text = "";
        //        if (selectionAttsCode == null)
        //            selectionAttsCode = EntityType.Attributi.Select(item => item.Key);

        //        //carico tutte le entità riferite se ci sono attributi di tipo AttributoRiferimento
        //        Dictionary<string, List<Entity>> entitiesRiferite = new Dictionary<string, List<Entity>>();
        //        foreach (string attCode in selectionAttsCode)
        //        {
        //            if (EntityType.Attributi[attCode] is AttributoRiferimento)
        //            {
        //                AttributoRiferimento attRif = EntityType.Attributi[attCode] as AttributoRiferimento;

        //                IEnumerable<Guid> refEntsId = entityCollection.Entities.Select(item => ((item.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuid).V));
        //                if (!entitiesRiferite.ContainsKey(attRif.ReferenceEntityTypeKey))
        //                {
        //                    if (this.DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey].IsTreeMaster)
        //                        entitiesRiferite.Add(attRif.ReferenceEntityTypeKey, DataService.GetTreeEntitiesDeepById(attRif.ReferenceEntityTypeKey, refEntsId).ToList<Entity>());
        //                    else
        //                        entitiesRiferite.Add(attRif.ReferenceEntityTypeKey, DataService.GetEntitiesById(attRif.ReferenceEntityTypeKey, refEntsId));
        //                }
        //            }
        //        }

        //        //copio di headers
        //        if (withHeaders)
        //        {
        //            foreach (string attCode in selectionAttsCode)
        //            {
        //                string val = EntityType.Attributi[attCode].Etichetta;
        //                text += val;
        //                text += "\t";
        //            }
        //            text += "\r\n";
        //        }

        //        //copio i dati
        //        for (int i = 0; i < entityCollection.Entities.Count; i++)
        //        {
        //            Entity ent = entityCollection.Entities[i];
        //            foreach (string attCode in selectionAttsCode)
        //            {
        //                EntityAttributo entAtt = ent.Attributi[attCode];
        //                if (entAtt.Attributo.ValoreDefault is ValoreGuid)
        //                    continue;


        //                string valStr = "";
        //                //Valore val = EntitiesHelper.GetValoreAttributo(ent, entAtt.AttributoCodice, true, true); 
        //                //if (val != null)
        //                //    valStr = val.ToPlainText();

        //                valStr = EntitiesHelper.GetValorePlainText(ent, entAtt.AttributoCodice, true, true);

        //                text += valStr;
        //                text += "\t";
        //            }

        //            text += "\r\n";
        //        }

        //        dataObject.SetData(DataFormats.Text, text);
        //        /////////////////////////////////////////////////////////////////////////////////////////



        //        Clipboard.SetDataObject(dataObject);

        //        Mouse.OverrideCursor = null;

        //    }
        //    catch (Exception e)
        //    {
        //        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
        //        //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
        //        //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
        //        //log.Trace("CopyEntities: " + e.ToString());
        //    }

        //    IsBusy = false;
        //}
        public virtual void CopyClipboardEntities(IEnumerable<Guid> entsId = null)
        {

            IsBusy = true;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                EntityCollection entityCollection = new EntityCollection();

                if (entsId == null)
                    entsId = FilteredEntitiesId.Where(item => CheckedEntitiesId.Contains(item));

                entityCollection.Entities = DataService.GetEntitiesById(EntityType.GetKey(), entsId);

                Clipboard.Clear();

                DataObject dataObject = new DataObject();

                //////////////////////JSON////////////////////////////
                //Json (copia l'intera entità a prescindere da selectionAttsCode)
                string json = null;
                JsonSerializer.JsonSerialize(entityCollection, out json);
                dataObject.SetData(EntityType.GetKey(), json);

                Clipboard.SetDataObject(dataObject);

                Mouse.OverrideCursor = null;

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
                //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
                //log.Trace("CopyEntities: " + e.ToString());
            }

            IsBusy = false;
        }
        public virtual void CopyTextClipboardEntities(IEnumerable<Guid> entsId = null)
        {

            IsBusy = true;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                EntityCollection entityCollection = new EntityCollection();

                if (entsId == null)
                    entsId = FilteredEntitiesId.Where(item => CheckedEntitiesId.Contains(item));

                entityCollection.Entities = DataService.GetEntitiesById(EntityType.GetKey(), entsId);

                Clipboard.Clear();

                DataObject dataObject = new DataObject();

                ///////////////////////TEXT///////////////////////////
                string text = "";

                IEnumerable<string> attsCode = EntityType.Attributi.Values.Where(item =>
                {
                    if (!item.IsVisible)
                        return false;

                    var sourceAtt = EntitiesHelper.GetSourceAttributo(item);

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                        return false;

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                        return false;

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                        return false;

                    return true;
                })
                .OrderBy(item => item.DetailViewOrder)
                .Select(item => item.Codice);


                //copio di headers
                foreach (string attCode in attsCode)
                {
                    string val = EntityType.Attributi[attCode].Etichetta;
                    text += val;
                    text += "\t";
                }
                text += "\r\n";


                //copio i dati
                for (int i = 0; i < entityCollection.Entities.Count; i++)
                {
                    Entity ent = entityCollection.Entities[i];
                    foreach (string attCode in attsCode)
                    {
                        string valStr = string.Empty;

                        Valore val = EntitiesHelper.GetValoreAttributo(ent, attCode, false, false);
                        if (val is ValoreReale)
                        {
                            valStr = ((ValoreReale)val).RealResult?.ToString();
                        }
                        else if (val is ValoreContabilita)
                        {
                            valStr = ((ValoreContabilita)val).RealResult?.ToString(/*CultureInfo.InvariantCulture*/);
                        }
                        else if (val != null)
                            valStr = val.ToPlainText();
                        

                        //if (ent.Attributi[attCode].Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                        //{
                        //    ValoreReale valReale = ent.Attributi[attCode].Valore as ValoreReale;
                        //    valStr = valReale.RealResult.ToString();
                        //}
                        //else if (ent.Attributi[attCode].Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        //{
                        //    ValoreContabilita valCont = ent.Attributi[attCode].Valore as ValoreContabilita;
                        //    valStr = valCont.RealResult.ToString();
                        //}
                        //else
                        //{
                        //    valStr = EntitiesHelper.GetValorePlainText(ent, attCode, true, true);
                        //}

                        text += valStr;
                        text += "\t";
                    }

                    text += "\r\n";
                }

                dataObject.SetData(DataFormats.Text, text);
                /////////////////////////////////////////////////////////////////////////////////////////

                Clipboard.SetDataObject(dataObject);

                Mouse.OverrideCursor = null;

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
                //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
                //log.Trace("CopyEntities: " + e.ToString());
            }

            IsBusy = false;
        }

        public virtual async void PasteClipboardEntities()
        {
            IsBusy = true;
            Guid firstTarget = SelectedEntityId;

            ModelAction pasteAction = new ModelAction() { ActionName = ActionName.ENTITIES_PASTE, EntityTypeKey = EntityType.GetKey() };

            if (firstTarget != Guid.Empty)
                pasteAction.NewTargetEntitiesId = new List<TargetReference>() { new TargetReference() { Id = firstTarget, TargetReferenceName = TargetReferenceName.AFTER } };

            IDataObject dataObject = Clipboard.GetDataObject();


            if (dataObject.GetDataPresent(EntityType.GetKey()))
            {
                string json = dataObject.GetData(EntityType.GetKey()) as string;

                pasteAction.JsonSerializedObject = json;

            }
            else if (dataObject.GetDataPresent(DataFormats.Text))
            {
                string text = dataObject.GetData(DataFormats.Text) as string;

                string[] lines = Regex.Split(text, "\r\n|\r|\n");

                EntityCollection entityCollection = new EntityCollection();
                entityCollection.Entities = new List<Entity>();

                foreach (string line in lines)
                {
                    if (line.Trim().Length == 0)
                        continue;

                    Entity localEntity = DataService.NewEntity(EntityType.GetKey()) as Entity;

                    if (localEntity.AddValoriAttributiByTabbedString(line))
                    {
                        entityCollection.Entities.Add(localEntity);
                    }
                }

                string json = null;
                JsonSerializer.JsonSerialize(entityCollection, out json);

                pasteAction.JsonSerializedObject = json;

                IsBusy = false;

            }
            else
            {
                return;
            }
            Entity targetEntity = GetActionTargetEntity();


            //Scopo: Controllare se le entità copiate sono tutte provenienti dallo stesso gruppo del target
            bool isReadyToModifyEntitiesSameGroup = true;
            string sourceGroupKey = null;
            foreach (Guid id in ReadyToModifyEntitiesId)
            {
                string groupKey = GroupData.JoinGroupKeys(GetGroupsKeyById(id).ToArray());

                if (sourceGroupKey == null)
                {
                    sourceGroupKey = groupKey;
                }
                else if (sourceGroupKey != groupKey)
                {
                    isReadyToModifyEntitiesSameGroup = false;
                    break;
                }
            }

            #region Elaborazione necessaria per il messaggio nel computo

            bool isGroupedByElementiItemRif = false;
            bool isGroupedByPrezzarioItemRif = false;
            foreach (AttributoGroupData attGroupData in RightPanesView.GroupView.Data.Items)
            {
                AttributoRiferimento attRif = EntityType.Attributi[attGroupData.CodiceAttributo] as AttributoRiferimento;
                if (attRif != null)
                {
                    if (attRif.ReferenceEntityTypeKey == BuiltInCodes.EntityType.Elementi)
                        isGroupedByElementiItemRif = true;

                    if (attRif.ReferenceEntityTypeKey == BuiltInCodes.EntityType.Prezzario)
                        isGroupedByPrezzarioItemRif = true;
                }
            }

            //Se gli elementi pronti ad essere incollati non sono dello stesso gruppo o sono dello stesso gruppo ma di un gruppo differente dal target
            if ((isGroupedByElementiItemRif || isGroupedByPrezzarioItemRif) && (!isReadyToModifyEntitiesSameGroup || sourceGroupKey != CurrentGroupKey))
            {
                MessageBoxResult res = MessageBox.Show(LocalizationProvider.GetString("ConfermaAssegnazioneElementoArticoloTarget"),
                    LocalizationProvider.GetString("AppName"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (res == MessageBoxResult.No)
                    return;
            }
            #endregion

            if (sourceGroupKey != CurrentGroupKey)
            {
                //Setto gli attributi in base ai raggruppamenti correnti
                SetValoriOfTargetGroups(pasteAction, targetEntity);
            }

            //Cambia i valori in base ai filtri attivi
            SetValoriOfCurrentFilter(pasteAction/*, targetEntity*/);

            //Modifica alcuni valori in base a EntityType
            SetValoriBuiltIn(pasteAction, targetEntity);

            //ModelActionResponse mar = ModelActionsStack.CommitAction(pasteAction, this);
            ModelActionResponse mar = CommitAction(pasteAction);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                RightPanesView.FilterView.ClearNextAttributiFilter(0);

                CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);
                ApplyFilterAndSort(CheckedEntitiesId.First());

                PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
                PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;



                await UpdateCache();
            }

            IsBusy = false;
        }

        protected virtual void SetValoriBuiltIn(ModelAction pasteAction, Entity targetEntity)
        {
        }


        /// <summary>
        /// Trova l'entità target dove applicare l'azione (che di solito è il fuoco ma se si tratta di un gruppo chiuso sarà l'ultima entità del gruppo)
        /// </summary>
        /// <returns></returns>
        public Entity GetActionTargetEntity()
        {
            Entity targetEntity = null;

            if (SelectedEntityView != null)
                targetEntity = SelectedEntityView.Entity;


            //List<string> groupNames = RightPanesView.GroupView.Items.Select(item => item.Attributo.Codice).ToList();

            //if (CurrentGroupKey != null && CurrentGroupKey.Any())
            //{
            //    string[] groupKeys = RightPanesView.GroupView.SplitGroupKey(CurrentGroupKey);

            //    if (SelectedEntityView != null)
            //        targetEntity = SelectedEntityView.Entity;
            //    else
            //    {
            //        Guid targetId = FilteredEntitiesViewInfo.LastOrDefault(item =>
            //        {
            //            string itemGroupkey = RightPanesView.GroupView.JoinGroupKeys(item.Value.GroupKeys.ToArray());
            //            if (CurrentGroupKey == itemGroupkey)
            //                return true;

            //            return false;
            //        }).Key;

            //        if (targetId != null && targetId != Guid.Empty)
            //        {
            //            List<Entity> entities = DataService.GetEntitiesById(EntityType.GetKey(), new List<Guid>() { targetId });
            //            targetEntity = entities.LastOrDefault();
            //        }
            //        else
            //        {
            //            MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "No Target");
            //        }


            //    }
            //}
            //else
            //{
            //    if (SelectedEntityView != null)
            //        targetEntity = SelectedEntityView.Entity;
            //}

            return targetEntity;
        }


        protected void SetValoriOfTargetGroups(ModelAction pasteAction, Entity targetEntity)
        {
            if (targetEntity == null)
                return;

            List<string> groupNames = RightPanesView.GroupView.Items.Select(item => item.Attributo.Codice).ToList();


            if (CurrentGroupKey != null && CurrentGroupKey.Any())
            {
                string[] groupKeys = RightPanesView.GroupView.SplitGroupKey(CurrentGroupKey);
                for (int i = 0; i < groupKeys.Count(); i++)
                {
                    if (i < groupNames.Count)
                    {

                        string groupName = groupNames[i];

                        Attributo att = EntitiesHelper.GetAttributo(targetEntity, groupName);
                        if (att is AttributoRiferimento)
                        {
                            AttributoRiferimento attRif = EntitiesHelper.GetAttributo(targetEntity, groupName) as AttributoRiferimento;
                            groupName = attRif?.ReferenceCodiceGuid;
                        }
                        else
                        {
                            groupName = att.Codice;
                        }

                        ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = groupName, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
                        attAction.NewValore = targetEntity.Attributi[groupName].Valore.Clone();


                        if (!(pasteAction.NestedActions.Select(item => item.AttributoCode).Contains(attAction.AttributoCode)))//se non è già stata prevista una azione sullo stesso codice (ad esempio è già stato assegnato l'articolo da dialogo)
                            pasteAction.NestedActions.Add(attAction);
                    }
                }
            }
        }


        //protected bool SetValoriOfCurrentFilter(ModelAction pasteAction, Entity targetEntity)
        //{
        //    bool res = false;

        //    if (targetEntity == null)
        //        return res;

        //    IEnumerable<string> filterAttNames = RightPanesView.FilterView.Items.Where(item => item.IsFiltroAttivato).Select(item => item.GetCodice());


        //    foreach (string filAttName in filterAttNames)
        //    {
        //        string filterAttName = filAttName;

        //        Attributo att = targetEntity.Attributi[filterAttName].Attributo;

        //        if (targetEntity.Attributi[filterAttName].Attributo is AttributoRiferimento)
        //        {
        //            AttributoRiferimento attRif = targetEntity.Attributi[filterAttName].Attributo as AttributoRiferimento;
        //            filterAttName = attRif.ReferenceCodiceGuid;
        //        }


        //        ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = filterAttName, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
        //        attAction.NewValore = targetEntity.Attributi[filterAttName].Valore.Clone();

        //        if (!(pasteAction.NestedActions.Select(item => item.AttributoCode).Contains(attAction.AttributoCode)))//se non è già stata prevista una azione sullo stesso codice (ad esempio è già stato assegnato l'articolo da dialogo)
        //            pasteAction.NestedActions.Add(attAction);

        //        res = true;

        //    }

        //    return res;
        //}

        protected virtual bool SetValoriOfCurrentFilter(ModelAction pasteAction/*, Entity targetEntity*/)
        {
            bool res = false;

            //if (targetEntity == null)
            //    return res;

            IEnumerable<AttributoFilterData> filterAtt = RightPanesView.FilterView.Data.Items.Where(item => item.IsFiltroAttivato);


            foreach (AttributoFilterData filAtt in filterAtt)
            {
                if (filAtt.CheckedValori.Count != 1)
                    continue;

                string codiceAtt = filAtt.CodiceAttributo;
                string valStr = filAtt.CheckedValori.First();

                if (!EntityType.Attributi.ContainsKey(codiceAtt))
                    continue;


                Attributo att = EntityType.Attributi[codiceAtt];
                Valore newVal = att.ValoreDefault.Clone();
                newVal.SetByString(valStr);


                ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
                attAction.NewValore = newVal;

                if (!(pasteAction.NestedActions.Select(item => item.AttributoCode).Contains(attAction.AttributoCode)))//se non è già stata prevista una azione sullo stesso codice (ad esempio è già stato assegnato l'articolo da dialogo)
                    pasteAction.NestedActions.Add(attAction);

                res = true;

            }

            return res;
        }

        public ICommand MoveEntitiesCommand
        {
            get
            {
                return new CommandHandler(() => this.PrepareToMoveEntities());
            }
        }

        public ICommand CopyEntitiesCommand
        {
            get
            {
                return new CommandHandler(() => this.CopyEntities());
            }
        }

        public virtual void PrepareToMoveEntities()
        {
            ReadyToModifyEntitiesId = new HashSet<Guid>(CheckedEntitiesId);
            ReadyToModifyEntitiesCommand = ReadyToPasteEntitiesCommands.Move;
            PasteEntitiesToolTip = LocalizationProvider.GetString("Sposta dopo l'elemento corrente");
            //PasteEntitiesToolTip = StringResoucesTemp.GetResourceString("MasterListView_Move");
            RaisePropertyChanged(GetPropertyName(() => IsMoveEntitiesWaitingForTarget));

            UpdateCache();
            UpdateUI();
        }

        public virtual void CopyEntities()
        {
            ReadyToModifyEntitiesId = new HashSet<Guid>(CheckedEntitiesId);

            //Oss: passo le FilteredEntitiesId filtrate per  ReadyToModifyEntitiesId al fine di mantenere l'ordine corretto
            CopyClipboardEntities(FilteredEntitiesId.Where(item => ReadyToModifyEntitiesId.Contains(item)));
            ReadyToModifyEntitiesCommand = ReadyToPasteEntitiesCommands.Copy;
            PasteEntitiesToolTip = LocalizationProvider.GetString("Incolla dopo l'elemento corrente");
            //PasteEntitiesToolTip = StringResoucesTemp.GetResourceString("MasterListView_Paste");

            //UpdateCache();//commentato altrimenti perde la selezione del gruppo se tutti i gruppi sono chiusi
            UpdateUI();

        }

        public ICommand CopyTextEntitiesCommand  { get { return new CommandHandler(() => this.CopyTextEntities()); } }
        public virtual void CopyTextEntities()
        {
            CopyTextClipboardEntities(CheckedEntitiesId);
        }

        public bool IsAnyReadyToPaste { get { return ReadyToModifyEntitiesId.Any(); } }

        public bool IsMoveEntitiesEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return ((!IsMoveEntitiesAfterEnabled && IsAnyChecked) || (IsMoveEntitiesAfterEnabled && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move));
            }
        }
        public bool IsCopyEntitiesEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return ((!IsMoveEntitiesAfterEnabled && IsAnyChecked) || (IsMoveEntitiesAfterEnabled && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Copy));
            }
        }

        public bool IsMoveEntitiesNotificationEnabled
        {
            get => IsAnyReadyToPaste && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move;
        }
        public bool IsCopyEntitiesNotificationEnabled
        {
            //get => IsAnyReadyToPaste && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Copy;
            get
            {
                try
                {
                    if (IsRestrictedCommandsMode())
                        return false;

                    if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                        return false;

                    IDataObject dataObject = Clipboard.GetDataObject();
                    if (EntityType != null && dataObject.GetDataPresent(EntityType.GetKey()))
                    {
                        return IsAnyReadyToPaste && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Copy;
                    }
                }
                catch (Exception exc)
                {
                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                }
                return false;
            }
        }

        public bool IsMoveEntitiesWaitingForTarget
        {
            get => IsMoveEntitiesNotificationEnabled && !IsMoveEntitiesAfterEnabled;
        }


        #endregion Copy&Paste


        public ICommand EscapeCommand
        {
            get
            {
                return new CommandHandler(() => this.Escape());
            }
        }

        public virtual void Escape()
        {
            IsMultipleModify = false;
            ReadyToModifyEntitiesId.Clear();
            UpdateCache();
            UpdateUI();
        }

        public bool IsEscapeEnabled
        {
            get => IsMoveEntitiesAfterEnabled;
        }

        public ICommand CtrlACheckedCommand
        {
            get
            {
                return new CommandHandler(() => this.CtrlAChecked());
            }
        }

        /// <summary>
        /// Ctrl A checked (selezione di tutti gli item)
        /// </summary>
        void CtrlAChecked()
        {
            CheckedEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
            UpdateCache();
        }

        /// <summary>
        /// Indice della prima riga visualizzata
        /// </summary>
        public int FirstVisibleRowIndex { get; set; } = 0;

        /// <summary>
        /// Indice dell'ultima riga visualizzata
        /// </summary>
        public int LastVisibleRowIndex { get; set; } = 0;

        public virtual void ShowEntities(List<Guid> entitiesId) { }

        public ICommand ScrollToCurrentEntityCommand
        {
            get
            {
                return new CommandHandler(() => this.ScrollToCurrentEntity());
            }
        }

        public virtual void ScrollToCurrentEntity()
        {
            SelectEntityById(SelectedEntityId);
        }

        public bool IsScrollToCurrentEntityEnabled
        {
            get { return SelectedEntityId != Guid.Empty; }
        }






        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public ICommand SetAttributiCommand
        {
            get
            {
                return new CommandHandler(() => this.SetAttributi());
            }
        }

        /// <summary>
        /// Setta gli attributi da programma
        /// </summary>
        public virtual void SetAttributi()
        {
            Dictionary<string, DefinizioneAttributo> defAtts = DataService.GetDefinizioniAttributo();
            Dictionary<string, EntityType> entAtts = DataService.GetEntityTypes();

            EntityType clone = EntityType.Clone();
            clone.CreaAttributi(defAtts, entAtts);

            DataService.SetEntityType(clone, false);
            EntityType = DataService.GetEntityTypes()[clone.GetKey()];

            ApplyFilterAndSort();
            UpdateCache(true);
        }




        #region Expand/Collapse

        public ICommand ExpandCheckedEntitiesCommand
        {
            get
            {
                return new CommandHandler(() => this.ExpandCheckedEntities());
            }
        }

        public virtual void ExpandCheckedEntities()
        {
            PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
            PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;
            UpdateCache();
        }

        public bool IsExpandAllEnabled
        {
            get
            {
                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }
        public ICommand CollapseAllCommand
        {
            get
            {
                return new CommandHandler(() => this.CollapseAll());
            }
        }
        public virtual void CollapseAll()
        {
            PendingCommand |= EntitiesListMasterDetailViewCommands.CollapseAll;
            UpdateCache();
            UpdateUI();
        }

        public bool IsCollapseAllEnabled
        {
            get
            {
                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }
        #endregion

        #region Model3d
        public ICommand SelectInModel3dCommand
        {
            get
            {
                return new CommandHandler(() => this.SelectInModel3d());
            }
        }

        void SelectInModel3d()
        {

        }
        #endregion


        //public string GetCodiceAttributoByMappingName(string mappingName)
        //{
        //    int index = EntityView.GetAttributiMasterCodesIndexByMappingName(mappingName);
        //    return EntityType.AttributiMasterCodes[index];
        //}

        public string GetCodiceAttributoByMappingName(string mappingName)
        {
            return MasterMappingNames.GetCodiceByMappingName(mappingName);

            //foreach (string key in MappingNames.Keys)
            //    if (MappingNames[key] == mappingName)
            //        return key;
            //return null;
        }


        #region Group

        /// <summary>
        /// gestisce l'espansione del gruppo di attributi nel detail
        /// </summary>
        Dictionary<string, bool> _groupsExpanded = new Dictionary<string, bool>();
        public bool IsAttributoViewGroupExpanded(string groupName)
        {
            if (_groupsExpanded.ContainsKey(groupName))
                return _groupsExpanded[groupName];
            return true;
        }
        public void SetAttributoViewGroupExpanded(string groupName, bool expanded)
        {
            if (!_groupsExpanded.ContainsKey(groupName))
                _groupsExpanded.Add(groupName, expanded);
            else
                _groupsExpanded[groupName] = expanded;
        }

        public int GetGroupLevelByCodiceAttributo(string codAtt)
        {
            int index = -1;
            for (int i = 0; i < RightPanesView.GroupView.Items.Count; i++)
            {
                if (RightPanesView.GroupView.Items[i].Attributo.Codice == codAtt)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public List<string> GetGroupsKeyById(Guid id)
        {
            //esempio: return {"COD001", "metri"}

            if (FilteredEntitiesViewInfo.ContainsKey(id))
                return FilteredEntitiesViewInfo[id].GroupKeys;

            return new List<string>();
        }

        public void SetCurrentGroupKey()
        {
            if (SelectedEntityView != null)
            {
                List<string> groupsKey = GetGroupsKeyById(SelectedEntityView.Id);
                CurrentGroupKey = RightPanesView.GroupView.JoinGroupKeys(groupsKey.ToArray());
            }
            else
                CurrentGroupKey = null;
        }

        public void CreateGroupData()
        {
            GroupData groupData = RightPanesView.GroupView.Data;
            groupData.GroupRecords.Clear();


            string tableSummaryKey = "TableSummary";
            if (!groupData.GroupRecords.ContainsKey(tableSummaryKey))
            {
                GroupRecordData grRecordData = new GroupRecordData();
                foreach (string codiceAtt in EntityType.AttributiMasterCodes)
                    grRecordData.Attributi.Add(codiceAtt, null);

                groupData.GroupRecords.Add(tableSummaryKey, grRecordData);
            }


            foreach (Guid id in FilteredEntitiesId)
            {
                groupData.GroupRecords[tableSummaryKey].ChildsId.Add(id);

                List<string> groupsKey = GetGroupsKeyById(id);
                for (int i = 0; i< groupsKey.Count; i++)
                {
                    string groupKey = RightPanesView.GroupView.JoinGroupKeys(groupsKey.Take(i+1).ToArray());
                    if (groupData.GroupRecords.ContainsKey(groupKey))
                    {
                        groupData.GroupRecords[groupKey].ChildsId.Add(id);
                    }
                    else
                    {
                        GroupRecordData grRecordData = new GroupRecordData();
                        foreach (string codiceAtt in EntityType.AttributiMasterCodes)
                            grRecordData.Attributi.Add(codiceAtt, null);

                        grRecordData.ChildsId.Add(id);
                        groupData.GroupRecords.Add(groupKey, grRecordData);
                        

                    }

                }

            }

        }

        public Task<GroupData> taskGroupDataFill = null;

        public void CreateAndFillGroupData()
        {
            taskGroupDataFill = Task.Run(() =>
            {
                GroupData groupData = RightPanesView.GroupView.Data;
                CreateGroupData();
               
                DataService.FillGroupData(groupData);

                Debug.WriteLine("CreateAndFillGroupData");

                return groupData;
            });
        }

        public async Task<GroupData> GetGroupDataResult()
        {
            return await taskGroupDataFill;
        }


        public bool IsGrouped { get => RightPanesView.GroupView.Items.Any(); }

        /// <summary>
        /// Pannello raggruppamento aperto
        /// </summary>
        public bool IsGroupPaneOpen
        {
            get { return RightPanesView.IsGroupPaneOpen; }
            set { SetProperty(RightPanesView.IsGroupPaneOpen, value, () => RightPanesView.IsGroupPaneOpen = value); }
        }

        public Guid FindNextInCurrentGroup(Guid entId)
        {
            if (CurrentGroupKey == null || !CurrentGroupKey.Any())
                return Guid.Empty;

            Guid foundId = Guid.Empty;
            if (entId == Guid.Empty)
            {
                //trova il primo del gruppo corrente
                foundId = FilteredEntitiesViewInfo.First(item =>
                {
                    string groupKey = RightPanesView.GroupView.JoinGroupKeys(item.Value.GroupKeys.ToArray());
                    if (groupKey == CurrentGroupKey)
                        return true;
                    return false;
                }).Key;
            }
            else if (FilteredEntitiesId.Contains(entId))
            {
                //Trova il successivo di entId del gruppo corrente
                int index = FilteredEntitiesId.IndexOf(entId);
                for (int i = index + 1; i < FilteredEntitiesId.Count; i++)
                {
                    List<string> groupKeys = FilteredEntitiesViewInfo[FilteredEntitiesId[i]].GroupKeys;
                    string groupKey = RightPanesView.GroupView.JoinGroupKeys(groupKeys.ToArray());
                    if (groupKey == CurrentGroupKey)
                    {
                        foundId = FilteredEntitiesId[i];
                        break;//trovato
                    }
                }
            }
            return foundId;

        }

        public void UpdateGroupsKey(AttributoRiferimento attRif, List<Guid> entitiesId)
        {
            //Aggiorno il gruppo di appartenenza
            Entity firstEnt = null;
            foreach (Guid id in entitiesId)
            {
                if (firstEnt == null)
                    firstEnt = DataService.GetEntitiesById(EntityType.Codice, new List<Guid>() { entitiesId.First() }).First();

                foreach (AttributoGroupData groupData in RightPanesView.GroupView.Data.Items)
                {
                    int groupLevel = GetGroupLevelByCodiceAttributo(groupData.CodiceAttributo);

                    AttributoRiferimento attRifGroup = EntityType.Attributi[groupData.CodiceAttributo] as AttributoRiferimento;
                    if (attRifGroup != null && attRifGroup.ReferenceEntityTypeKey == attRif.ReferenceEntityTypeKey)
                    {
                        Valore val = EntitiesHelper.GetValoreAttributo(firstEnt, groupData.CodiceAttributo, false, true);
                        if (groupLevel >= 0)
                            FilteredEntitiesViewInfo[id].GroupKeys[groupLevel] = (val != null) ? val.ToPlainText() : "";
                    }

                }
            }
        }
        #endregion Group

        protected virtual void OnItemsLoading(EventArgs e) {}

        protected virtual void OnItemsLoaded(EventArgs e) {}


        #region Nessuna Selezione
        /// <summary>
        /// Gestione del pulsante per "Nessuna Selezione"
        /// </summary>
        protected bool _isNoSelectionChecked = false;
        public virtual bool IsNoSelectionChecked
        {
            get { return _isNoSelectionChecked; }
            set
            {
                if (SetProperty(ref _isNoSelectionChecked, value))
                {
                    ClearCheckedEntities();
                    //SelectedIndex = -1;
                    SelectEntityById(Guid.Empty);
                }
            }
        }

        public void SetNoSelectionChecked(bool isNoSelectionChecked)
        {
            SetProperty(ref _isNoSelectionChecked, isNoSelectionChecked);
        }

        bool _allowNoSelection = false;
        public bool AllowNoSelection
        {
            get { return _allowNoSelection; }
            set { SetProperty(ref _allowNoSelection, value); }
        }

        #endregion Nessuna Selezione


        public bool IsSingleSelection { get; set; } = false;

        public virtual bool ValidateAction(ModelAction action) { return true; }

        public virtual ModelActionResponse CommitAction(ModelAction action)
        {
            if (!ValidateAction(action))
                return new ModelActionResponse() { ActionResponse = ActionResponse.FAILED };

            if (!IsMoveEntitiesEnabled && !IsCopyEntitiesEnabled)
                ReadyToModifyEntitiesId.Clear();

            WindowService.ShowWaitCursor(true);

            ModelActionResponse mar = DataService.CommitAction(action);

            WindowService.ShowWaitCursor(false);

            return mar;
        }

        public bool IsAttributoTextAlignmentRight(string codiceAttributo)
        {
            if (!EntityType.Attributi.ContainsKey(codiceAttributo))
                return false;



            Attributo att = EntityType.Attributi[codiceAttributo];
            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(att);
                //GetSourceAttributoOf(att);

            if (sourceAtt == null)
                return false;

            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale ||
                sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
            {
                return true;
            }
            return false;
        }

        public virtual async void UpdateEntityType()
        {
            string entityTypeKey = EntityType.GetKey();
            EntityType = DataService.GetEntityTypes()[entityTypeKey];
            EntityType.ResolveReferences(DataService.GetEntityTypes(), DataService.GetDefinizioniAttributo());

            RightPanesView.UpdateAttributi();
            AttributiEntities.Load(new HashSet<Guid>());
            ApplyFilterAndSort(_selectedEntityId);
            await UpdateCache(true);

        }


        public List<string> GetDependentEntityTypesKey()
        {
            return EntitiesHelper.GetDependentEntityTypesKey(EntityType.GetKey());

            //int dependencyEnum = (int)EntityType.DependencyEnum;
            //List<string> entTypesKey = new List<string>();
            //Dictionary<string, EntityType> entTypesDict = DataService.GetEntityTypes();
            //List<EntityType> entsType = entTypesDict.Values.Where(item => item.DependentTypesEnum != DependentEntityTypesEnum.Nothing).OrderBy(item => item.DependencyEnum).ToList();
            //foreach (EntityType entType in entsType)
            //{
            //    if ((dependencyEnum & (int)entType.DependentTypesEnum) == (int)dependencyEnum)
            //        entTypesKey.Add(entType.GetKey());
            //}

            //return entTypesKey;

        }

        public bool IsCacheUpdated()
        {
            if (ModelActionsStack != null)
                if (UpdateCacheModelActionIndex >= ModelActionsStack.GetCount())
                    return true;

            return false;
        }

        public void ClearReadyToModifyEntities()
        {
            ReadyToModifyEntitiesId.Clear();
            
        }

        public bool IsAdvancedMode
        {
            get
            {
                if (MainOperation != null)
                    return MainOperation.IsAdvancedMode();

                return false;

            }
        }

        public ICommand ImportItemsCommand { get => new CommandHandler(() => this.ImportItems()); }
        public async virtual void ImportItems()
        {

            if (MainOperation.IsProjectClosing())
                return;

            string projectFileExtension = MainOperation.GetProjectFileExtension();


            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = projectFileExtension;
                openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", projectFileExtension, projectFileExtension, projectFileExtension);

                IDataService ds = null;

                if (openFileDialog.ShowDialog() == true)
                {
                    string fullFileName = openFileDialog.FileName;

                    string ext = Path.GetExtension(fullFileName).Trim();
                    if (ext != string.Format(".{0}", MainOperation.GetProjectFileExtension()))
                    {
                        MainOperation.ShowMessageBarView(string.Format("{0} .join", LocalizationProvider.GetString("RichiestoFileDiEstensione")));
                        return;
                    }

                    string fileName = Path.GetFileNameWithoutExtension(fullFileName);

                    await Task.Run(() =>
                    {

                        MainOperation.ProgressChanged += MainOperation_ProgressChanged;

                        //open prezzario
                        ds = MainOperation.GetDataServiceByFile(fullFileName, out _);
                        MainOperation.ProgressChanged -= MainOperation_ProgressChanged;

                    });

                    MainOperation.ShowMessageBarView(null, false, -1, true);

                    if (ds == null)
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                        return;
                    }


                    ClientDataService sourceDataService = new ClientDataService(ds, new ModelActionsStack(ds));
                    sourceDataService.IsReadOnly = true;
                    sourceDataService.Init();

                    IEntityWindowService externalWndService = WindowService.CreateWindowService(sourceDataService, null, MainOperation);

                    Dictionary<string, EntityType> sourceEntityTypes = sourceDataService.GetEntityTypes();
                    EntityType sourceEntityType = null;
                    if (EntitiesHelper.IsCodiceBuiltIn(EntityType.Codice))
                        sourceEntityType = sourceEntityTypes.Values.FirstOrDefault(item => item.Codice == EntityType.Codice && !item.IsParentType());
                    else
                        sourceEntityType = sourceEntityTypes.Values.FirstOrDefault(item => item.Name == EntityType.Name && !item.IsParentType());

                    if (sourceEntityType == null)
                    {
                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("SezioneSorgenteNonTrovata"));
                        return;
                    }


                    string title = string.Format("{0} {1}.{2}", LocalizationProvider.GetString("SelezionaVociDa"), fileName, projectFileExtension);

                    List<Guid> selectedItems = new List<Guid>();


                    bool resWnd = false;
                    if (sourceEntityType is DivisioneItemType)
                    {
                        DivisioneItemType sourceDivItemType = sourceEntityType as DivisioneItemType;
                        resWnd = externalWndService.SelectDivisioneIdsWindow(sourceDivItemType.DivisioneId, ref selectedItems, title, SelectIdsWindowOptions.Nothing, null);
                    }
                    else
                        resWnd = externalWndService.SelectEntityIdsWindow(sourceEntityType.GetKey(), ref selectedItems, title, SelectIdsWindowOptions.Nothing, null, null);
                    
                    if (resWnd)
                    {

                        EntitiesImportStatus importStatus = new EntitiesImportStatus();
                        importStatus.TargetPosition = TargetPosition.Bottom;
                        importStatus.ConflictAction = EntityImportConflictAction.Undefined;
                        importStatus.Source = ds;// sourceDataService;
                        importStatus.SourceName = fileName;
                        importStatus.LimitedEntityTypes = EntityType.LimitedEntityTypesOnImport();
                        selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId()
                        {
                            SourceId = item,
                            SourceEntityTypeKey = sourceEntityType.GetKey()
                        }));

                        while (importStatus.Status != EntityImportStatusEnum.Completed)
                        {
                            DataService.ImportEntities(importStatus);
                            if (importStatus.Status == EntityImportStatusEnum.Waiting)
                            {
                                if (!WindowService.EntitiesImportWindow(importStatus))
                                    break;
                            }
                        }

                        IEnumerable<Guid> entitiesIds = importStatus.StartingEntitiesId.Select(item => item.TargetId);
                        MainOperation.UpdateEntityTypesView(new List<string>(importStatus.EntityTypes.Values.Select(item => item.TargetEntityTypeKey)));
                    }

                }

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message, e);
            }
            
        }

        private void MainOperation_ProgressChanged(object sender, EventArgs e)
        {
            ProgressChangedEventArgs args = e as ProgressChangedEventArgs;
            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("CaricamentoInCorso"), false, args.ProgressPercentage);
        }

        bool _isImportItemsEnabled = true;
        public bool IsImportItemsEnabled
        {
            get
            {
                if (!_isImportItemsEnabled)
                    return false;

                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
            set
            {
                _isImportItemsEnabled = value;
            }
        }
        public bool IsCodingItemsEnabled
        {
            get
            {

                if (DataService != null && DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                if (CheckedEntitiesId.Count() > 0)
                    return !IsMoveEntitiesAfterEnabled;
                else
                    return false;
            }
        }

        string _codingItemsToolTip = LocalizationProvider.GetString("Ricodifica");
        public string CodingItemsToolTip
        {
            get { return _codingItemsToolTip; }
            set { SetProperty(ref _codingItemsToolTip, value); }
        }

        public ICommand CodingItemsCommand { get => new CommandHandler(() => this.CodingItems()); }
        protected virtual void CodingItems()
        {
            if (CheckedEntitiesId.Count() > 0)
            {
                var Selezionati = FilteredEntitiesViewInfo.Where(g => CheckedEntitiesId.Contains(g.Key));
                if (Selezionati.Count() > 0)
                {
                    List<int> SelectedLevels = new List<int> { 0 };
                    int ProfonditaMassimaAlbero = 1;
                    if (WindowService.AttributoCodingWindow(EntityType.GetKey(), CheckedEntitiesId, ProfonditaMassimaAlbero, SelectedLevels))
                    {
                       
                    }
                }
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("Seleziona almeno una voce prima di procedere."));
            }

        }

        protected virtual bool IsRestrictedCommandsMode() { return false; }

        #region Alert

        ObservableCollection<AlertItemView> _alertItems = new ObservableCollection<AlertItemView>();
        public ObservableCollection<AlertItemView> AlertItems { get => _alertItems; }


        public bool IsAlertVisible
        {
            get => _alertItems.Where(item => item.AlertEntitiesTypeKey == EntityType.GetKey()).Any();
        }

        bool _isAlertOpen = false;
        public bool IsAlertOpen
        {
            get => _isAlertOpen;
            set => SetProperty(ref _isAlertOpen, value);
        }

        public ICommand ShowAlertCommand { get => new CommandHandler(() => this.ShowAlert()); }
        void ShowAlert()
        {

            IsAlertOpen = true;
        }

        public ICommand AlertCloseCommand { get => new CommandHandler(() => this.CloseAlert()); }
        public void CloseAlert()
        {
            IsAlertOpen = false;
        }


        ///// <summary>
        ///// Ricalcolo totale degli alert
        ///// </summary>
        ///// <param name="entsError"></param>
        //public void LoadAlert(EntitiesError entsError = null)
        //{
        //    if (DataService.IsReadOnly)
        //        return;

        //    AlertText = String.Empty;

        //    //LoadAlertDuplicatedKeys();
        //    LoadAlertDuplicatedKeys2();

        //    if (entsError != null)
        //    {
        //        AlertItems.Clear(); 
        //        LoadAlertLoopReference(entsError);
        //    }

        //    UpdateUI();
        //}

        public void LoadAlert()
        {
            if (DataService.IsReadOnly)
                return;

            List<EntitiesError> entsErrors = DataService.GetEntitiesErrors();

            AlertText = String.Empty;
            AlertItems.Clear();

            LoadAlertDuplicatedKeys2();

            var errors = entsErrors.Where(item => item.EntityTypeKey == EntityType.GetKey());

            foreach (var err in errors)
                LoadAlertLoopReference(err);

            UpdateUI();

        }


        /// <summary>
        /// Aggiornamento alert in base al risultato di un action
        /// </summary>
        /// <param name="modelActionResponse"></param>
        public void LoadAlertByAction(ModelActionResponse modelActionResponse)
        {
            if (modelActionResponse == null)
                return;

            LoadAlertDuplicatedKeys2();

            LoadAlertLoopReference(modelActionResponse);

            UpdateUI();
        }

        //protected virtual void LoadAlertDuplicatedKeys()
        //{
        //    var query = FilteredKeys.Values.GroupBy(item => item)
        //        .Where(item => item.Count() > 1)
        //        .Select(item => item.Key)
        //        .ToList();


        //    if (query.Any())
        //    {
        //        var attEtichette = EntityType.EntityComparer.AttributiCode.Select(item => EntityType.Attributi[item].Etichetta);

        //        string header = string.Join("|", attEtichette);
        //        string duplicates = string.Join("\n", query);
        //        string str = string.Format("{0} ({1})\n\n{2}", LocalizationProvider.GetString("ChiaviDuplicate"), header, duplicates);

        //        str = str.Replace("|", " | "); ;
        //        AlertText += str;
        //    }
        //}

        protected virtual void LoadAlertDuplicatedKeys2()
        {

            var alertItem = AlertItems.FirstOrDefault(item => item.ActionErrorType == ActionErrorType.NOTHING && item.AlertEntitiesTypeKey == EntityType.GetKey());
            if (alertItem != null)
                AlertItems.Remove(alertItem);

            var query = FilteredKeys.GroupBy(item => item.Value)
                .Where(item => item.Count() > 1)
                .Select(group => new { Key = group.Key, Items = group.ToList()  })
                .ToList();



            if (query.Any())
            {
                var attEtichette = EntityType.EntityComparer.AttributiCode.Select(item => EntityType.Attributi[item].Etichetta);

                string header = string.Join(" & ", attEtichette);
                string str = string.Format("{0} ({1})", LocalizationProvider.GetString("ChiaviDuplicate"), header);

                var ids = query.SelectMany(item => item.Items).Select(item => item.Key);
                AlertItems.Add(new AlertItemView(this) { AlertText = str, AlertEntitiesId = ids.ToHashSet(), AlertEntitiesTypeKey = EntityType.GetKey(), ActionErrorType = ActionErrorType.NOTHING});
            }
        }

        protected void LoadAlertLoopReference(ModelActionResponse modelActionError)
        {
            if (modelActionError == null)
                return;


            AlertItemView alertItem = AlertItems.FirstOrDefault(item => item.ActionErrorType == ActionErrorType.LOOP_REFERENCE);

            if (alertItem == null)
            {
                if (modelActionError.EntitiesError != null && modelActionError.EntitiesError.ActionErrorType == ActionErrorType.LOOP_REFERENCE)
                {
                    LoadAlertLoopReference(modelActionError.EntitiesError);
                }
                else
                {
                    //niente da fare
                }
            }
            else
            {

                foreach (Guid id in modelActionError.ChangedEntitiesId)
                {
                    if (alertItem.AlertEntitiesId.Contains(id))//è stata modifica questa entità
                    {
                        if (modelActionError.EntitiesError != null && modelActionError.EntitiesError.ActionErrorType == ActionErrorType.LOOP_REFERENCE)
                        {
                            //la modifica non ha portato miglioramenti
                            if (!modelActionError.EntitiesError.Ids.Contains(id))
                                alertItem.AlertEntitiesId.Remove(id);
                        }
                        else
                        {
                            alertItem.AlertEntitiesId.Remove(id);
                        }
                    }
                    else
                    {
                        if (modelActionError.EntitiesError != null && modelActionError.EntitiesError.ActionErrorType == ActionErrorType.LOOP_REFERENCE)
                        {
                            if (modelActionError.EntitiesError.Ids.Contains(id))
                                alertItem.AlertEntitiesId.Add(id);
                        }
                    }
                }

                //escludo dalle entità allertate quelle che non esistono più
                alertItem.AlertEntitiesId.RemoveWhere(item => !FilteredIndexes.ContainsKey(item));

                //tolgo la riga dell'alert in assenza di entità collegate
                if (!alertItem.AlertEntitiesId.Any())
                    AlertItems.Remove(alertItem);

            }

        }

        protected void LoadAlertLoopReference(EntitiesError error)
        {
            if (error == null || error.ActionErrorType == ActionErrorType.NOTHING)
                return;

            var alertItem = new AlertItemView(this);
            
            string msgLoop = LocalizationProvider.GetString("Riferiemento ciclico tra le voci");
            alertItem.ActionErrorType = error.ActionErrorType;
            alertItem.AlertText = msgLoop;
            alertItem.AlertEntitiesId = error.Ids;
            alertItem.AlertEntitiesTypeKey = error.EntityTypeKey;

            AlertItems.Add(alertItem);


        }

        string _alertText = string.Empty;
        public string AlertText
        {
            get => _alertText;
            set => SetProperty(ref _alertText, value);
        }


        #endregion

        public ICommand EntityHighlightersCommand { get => new CommandHandler(() => this.EntityHighlighters()); }
        protected void EntityHighlighters()
        {
            if (EntityType == null)
                return;

            if (WindowService.EntityHighlightersWnd(EntityType.GetKey()))
            {
                var calcOptions = new EntityCalcOptions() { CalcolaAttributiResults = false, ResetCalulatedValues= false };
                DataService.CalcolaEntities(EntityType.GetKey(), calcOptions, null, new EntitiesError());

                GroupData groupData = RightPanesView.GroupView.Data;
                DataService.FillGroupData(groupData);
                UpdateCache();

            }
        }

        public virtual bool IsEntityHighlightersEnabled
        {
            get
            {
                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }

        public virtual List<Guid> FilteredDescendantsId(List<Guid> ids)
        {
            return new List<Guid>();
        }


        public string RecalculateItemsCommandColor
        {
            //get => IsRecalculateItemsNeeded ? ColorsHelper.Convert(MyColorsEnum.ErrorColor).ToString() : ColorsHelper.Convert(MyColorsEnum.DisabledColor).ToString();
            get => Colors.Black.ToString();
        }

        public bool IsRecalculateItemsCommandAllowed
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                //return EntityType?.Attributi.Values.Any(item =>
                //{
                //    if (item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection || item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                //    {
                //        if (item.GuidReferenceEntityTypeKey == EntityType.GetKey())
                //        {
                //            //Attributo di tipo Guid o GuidCollection che riferisce alla sezione medesima
                //            return true;
                //        }
                //    }

                //    return false;
                //}) == true;

                return true;
            }
        }

        private void ModelActionsStack_ActionsChanged(object sender, ActionsChangedEventArgs e)
        {
            string entityTypeKey = EntityType?.GetKey();

            if (e.ActionAdded != null)
            {
                List<string> depEntTypes = EntitiesHelper.GetDependentEntityTypesKey(e.ActionAdded.EntityTypeKey);
                if (e.ActionAdded.EntityTypeKey == entityTypeKey || depEntTypes.Contains(entityTypeKey))
                {
                    if (e.ActionAdded.ActionName == ActionName.CALCOLA_ENTITES)//non è necessario un ricalcolo
                    {   }
                    else
                    {
                        IsRecalculateItemsNeeded = true;
                        UpdateUI();
                    }

                }
            }
        }


        bool _isRecalculateItemsNeeded = true;
        public bool IsRecalculateItemsNeeded
        {
            get => _isRecalculateItemsNeeded;
            set => SetProperty(ref _isRecalculateItemsNeeded, true);
        }

        public ICommand ImportXlsxCommand { get => new CommandHandler(() => this.ImportXlsx()); }
        public virtual void ImportXlsx()
        {
            string fileExtension = "xlsx";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = fileExtension;
            openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", fileExtension, fileExtension, fileExtension);

            if (openFileDialog.ShowDialog() == true)
            {
                string fullFileName = openFileDialog.FileName;

                string ext = Path.GetExtension(fullFileName).Trim();
                if (ext != string.Format(".{0}", fileExtension))
                {
                    MainOperation.ShowMessageBarView(string.Format("{0} {1}", LocalizationProvider.GetString("RichiestoFileDiEstensione"), fileExtension));
                    return;
                }


                WindowService.ShowWaitCursor(true);

                MainOperation.ImportItems(fullFileName, EntityType.GetKey());
                MainOperation.UpdateEntityTypesView(new List<string> { EntityType.GetKey() });

                WindowService.ShowWaitCursor(false);
            }
        }


    }

    public class AlertItemView : NotificationBase
    {
        EntitiesListMasterDetailView _master = null;

        public AlertItemView(EntitiesListMasterDetailView master)
        {
            _master = master;
        }

        public ActionErrorType ActionErrorType { get; set; } = ActionErrorType.NOTHING;
        public string AlertText { get; set; } = string.Empty;
        public HashSet<Guid> AlertEntitiesId { get; set; } = new HashSet<Guid>();
        public string AlertEntitiesTypeKey = string.Empty;

        public bool AnyEntities { get => AlertEntitiesId.Any(); }

        public ICommand AlertSelectEntitiesCommand { get => new CommandHandler(() => this.AlertSelectEntities()); }
        void AlertSelectEntities()
        {
            try
            {
                _master.CloseAlert();
                _master.RightPanesView.FilterView.LoadTemporaryFilterByIds(_master.EntityType.GetKey(), AlertEntitiesId.ToList());

                


            }
            catch (Exception ex)
            {

            }

        }


    }

}






