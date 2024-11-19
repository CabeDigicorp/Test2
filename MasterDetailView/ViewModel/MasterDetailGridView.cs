


using Commons;
using MasterDetailView;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Collections.Generic;
using MasterDetailModel;
using System.Linq;
using DevZest.Windows.DataVirtualization;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using Syncfusion.UI.Xaml.Grid;
using Model;
using _3DModelExchange;
using CommonResources;
using System.Diagnostics;

namespace MasterDetailView
{


    public class MasterDetailGridItemView : EntityView
    {

        public MasterDetailGridItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }

        //internal Entity GetItem(string codiceAttributoGuid, string entityTypeKey)
        //{
        //    Guid itemId = Entity.GetAttributoGuidId(codiceAttributoGuid);
        //    if (itemId != Guid.Empty)
        //    {
        //        IEnumerable<Entity> ents;
        //        ents = _master.DataService.GetEntitiesById(entityTypeKey, new List<Guid>() { itemId });
        //        return ents.LastOrDefault();
        //    }
        //    return null;
        //}

    }

    public class MasterDetailGridView : NotificationBase
    {
        public ClientDataService DataService { get; set; }//ref
        public IEntityWindowService WindowService {get; set;}//ref
        public ModelActionsStack ModelActionsStack { get; set; }//ref
        public IMainOperation MainOperation { get; set; }//ref
        public int RowOnTopCount { get; private set; }//numero di righe in cima alla griglia (es: header e table summary)

        public List<CalculatorFunction> CalculatorFunctions { get; set; } = new List<CalculatorFunction>();

        public MasterDetailGridView()
        {
            //ItemsView = new MasterDetailItemsViewVirtualized(this);
        }

        public MasterDetailGridItemsViewVirtualized ItemsView { get; set; }
        public RightPanesView RightPanesView { get => ItemsView.RightPanesView; }

        public virtual void Init(EntityTypeViewSettings viewSettings)
        {
            RowOnTopCount = 1;

            if (HasTableSummaryRow())
                RowOnTopCount++;

            ItemsView.WindowService = WindowService;
            ItemsView.DataService = DataService;
            ItemsView.ModelActionsStack = ModelActionsStack;
            ItemsView.MainOperation = MainOperation;

            ItemsView.Calculator = new ValoreCalculator(DataService);
            ItemsView.Calculator.NoteCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == NoteCalculatorFunction.Name) as NoteCalculatorFunction;
            ItemsView.Calculator.EPCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == EPCalculatorFunction.Name) as EPCalculatorFunction;
            ItemsView.Calculator.CmpCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == CmpCalculatorFunction.Name) as CmpCalculatorFunction;
            ItemsView.Calculator.IfcCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == Model3dCalculatorFunction.Names.Ifc) as Model3dCalculatorFunction;
            ItemsView.Calculator.RvtCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == Model3dCalculatorFunction.Names.Rvt) as Model3dCalculatorFunction;

            ItemsView.Init();

            if (viewSettings != null)
            {
                ItemsView.RightPanesView.FilterView.Load(viewSettings);
                ItemsView.RightPanesView.SortView.Load(viewSettings);
                ItemsView.RightPanesView.GroupView.Load(viewSettings);
            }

            DataService.Suspended = false;

            ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.ClearGridColumns;
            ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
            ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.UpdateTableSummaryRow;
            ItemsView.Load();

        }

        public bool IsSelectingItems { get; set; }

        object _currentItem = null;
        public object CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (IsSelectingItems)
                    return;

                if (!ItemsView.AttributiEntities.AttributiValoriComuniView.Any())
                    ItemsView.SelectEntityView(null);

                if (SetProperty(ref _currentItem, value))
                {

                    if (_currentItem != null)
                    {
                        VirtualListItem<EntityView> virtCurrItem = _currentItem as VirtualListItem<EntityView>;

                        ItemsView.SelectIndex(virtCurrItem.Index, true);

                        //rem by Ale 13/09/2022
                        //if (virtCurrItem.Data != null)
                        //    ItemsView.SelectEntityView(virtCurrItem.Data);
                    }
                    else
                    {
                        //rem by Ale 08/02/2024
                        //Commentato perchè non si vuole che chiudendo un gruppo della griglia sparisca il dettaglio


                        //ItemsView.SelectIndex(-1, true);
                        //ItemsView.SelectEntityView(null);

                    }

                }

            }
        }


        public void EnsureRows(int recordIndex)
        {
            if (recordIndex < 0)
                return;

            ItemsView.VirtualEntities.Refresh(recordIndex);
        }

        
        //public bool? IsAllChecked
        //{
        //    get { return ItemsView.IsAllChecked; }
        //    set
        //    {
        //        ItemsView.IsAllChecked = value;
        //    }
        //}

        bool _isCellUnitSelection = false;
        public bool IsCellUnitSelection
        {
            get => _isCellUnitSelection;
            set
            {
                if (SetProperty(ref _isCellUnitSelection, value))
                {
                    RaisePropertyChanged(GetPropertyName(() => SelectionUnit));
                }
            }
        }

        public string SelectionMode
        {
            get => "Extended";//Extended, Multiple, None
        }

        public string SelectionUnit
        {
            get => _isCellUnitSelection ? "Cell":"Row";
        }

        public HashSet<VirtualListItem<EntityView>> VisibleVirtualListItem { get; set; } = new HashSet<VirtualListItem<EntityView>>();

        public void UpdateUI()
        {
            //RaisePropertyChanged(GetPropertyName(() => IsAllChecked));
            
        }


        public void UpdateViewSettings(EntityTypeViewSettings viewSettings)
        {
            if (viewSettings == null)
                return;

            if (ItemsView.RightPanesView.GroupView.Data != null/* && ItemsView.RightPanesView.GroupView.Data.Items.Any()*/)
                viewSettings.Groups = ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            if (ItemsView.RightPanesView.SortView.Data != null/* && ItemsView.RightPanesView.SortView.Data.Items.Any()*/)
                viewSettings.Sorts = ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            if (ItemsView.RightPanesView.FilterView.Data != null/* && ItemsView.RightPanesView.FilterView.Data.Items.Any()*/)
                viewSettings.Filters = ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            if (ModelActionsStack != null)
                ModelActionsStack.OnViewSettingsChanged();
        }

        public void UpdateEntityType()
        {
            ItemsView.UpdateEntityType();
        }

        /// <summary>
        /// Sostituisci Attributo Guid di riferimento attRif nell'item corrente
        /// </summary>
        public async virtual void ReplaceCurrentItemAttributoGuid(AttributoRiferimento attRif, EntityTypeViewSettings viewSettings)
        {
            if (ItemsView.IsMultipleModify && !ItemsView.IsAnyChecked)
                return;

            if (!ItemsView.IsMultipleModify && ItemsView.SelectedEntityId == Guid.Empty)
                return;


            DataService.Suspended = true;

            EntityView currentItemView = null;
            if (ItemsView.SelectedEntityId != Guid.Empty)
                currentItemView = ItemsView.SelectedEntityView;


            EntityType entityType = DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey];

            if (entityType == null)
                return;

            List<Guid> selectedItems = new List<Guid>();

            if (currentItemView != null) //esiste una posizione di inserimento
            {
                Guid id = currentItemView.GetAttributoGuidId(attRif.ReferenceCodiceGuid);
                if (id != Guid.Empty)
                    selectedItems.Add(id);
                else if (!ItemsView.IsMultipleModify)
                    selectedItems.Add(Guid.Empty);

            }

            string title = LocalizationProvider.GetString("Sostituisci");

            if (WindowService.SelectEntityIdsWindow(entityType.GetKey(), ref selectedItems, title, SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, null, attRif))
            {
                if (selectedItems.Count == 1)
                {
                    Guid entityId = selectedItems.First();
                    ItemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = entityId });

                    await ItemsView.UpdateCache(true);
                }
            }
            else
            {
                //potrei aver fatto modifiche alla divisione
                await ItemsView.UpdateCache(true);
            }


        }

        public async virtual void ReplaceCurrentItemAttributoGuid(Attributo att, EntityTypeViewSettings viewSettings)
        {
            if (ItemsView.IsMultipleModify && !ItemsView.IsAnyChecked)
                return;

            if (!ItemsView.IsMultipleModify && ItemsView.SelectedEntityId == Guid.Empty)
                return;

            EntityView currentItemView = null;
            if (ItemsView.SelectedEntityId != Guid.Empty)
                currentItemView = ItemsView.SelectedEntityView;

            if (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;
                EntityType entityType = DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey];

                if (entityType == null)
                    return;

                DataService.Suspended = true;

                List<Guid> selectedItems = new List<Guid>();

                if (currentItemView != null) //esiste una posizione di inserimento
                {

                    Guid id = currentItemView.GetAttributoGuidId(attRif.ReferenceCodiceGuid);
                    if (id != Guid.Empty)
                        selectedItems.Add(id);
                    else if (!ItemsView.IsMultipleModify)
                        selectedItems.Add(Guid.Empty);

                }

                string title = LocalizationProvider.GetString("Sostituisci");

                if (WindowService.SelectEntityIdsWindow(entityType.GetKey(), ref selectedItems, title, SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, null, attRif))
                {
                    if (selectedItems.Count == 1)
                    {
                        Guid entityId = selectedItems.First();
                        ItemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = entityId });

                        await ItemsView.UpdateCache(true);
                    }
                }
                else
                {
                    //potrei aver fatto modifiche alla divisione
                    await ItemsView.UpdateCache(true);
                }

            }
            else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {
                EntityType entityType = ItemsView.EntityType;

                if (entityType == null)
                    return;

                ValoreAttributoGuidCollection attSettings = att.ValoreAttributo as ValoreAttributoGuidCollection;
                if (attSettings != null)
                {  
                    if (attSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
                    {
                        ValoreConditions valConds = null;

                        if (ItemsView.IsMultipleModify)
                        {
                            List<Entity> ents = DataService.GetEntitiesById(entityType.GetKey(), ItemsView.CheckedEntitiesId);

                            string filterDataJsonCommon = null;
                            FilterData filterDataCommon = null;
                            foreach (Entity ent in ents)
                            {
                                if (entityType.IsTreeMaster)
                                {
                                    if ((ent as TreeEntity).IsParent)
                                        continue;
                                }

                                ValoreGuidCollection valoreGuidCollection = ItemsView.EntitiesHelper.GetValoreAttributo(ent, att.Codice, false, false) as ValoreGuidCollection;
                                FilterData filterData = valoreGuidCollection.Filter;
                                string filterDataJson = "";
                                JsonSerializer.JsonSerialize(filterData, out filterDataJson);

                                if (filterDataJsonCommon == null)
                                {
                                    filterDataJsonCommon = filterDataJson;
                                    filterDataCommon = filterData;
                                }
                                else if (filterDataJsonCommon != filterDataJson)
                                {
                                    filterDataJsonCommon = null;
                                    break;
                                }
                            }

                            if (filterDataJsonCommon != null)
                                valConds = filterDataCommon.ToConditions();

                        }
                        else if (currentItemView != null) //esiste una posizione di inserimento
                        {
                            Entity ent = ItemsView.EntitiesHelper.GetDataServiceEntityById(entityType.GetKey(), currentItemView.Entity.EntityId);

                            ValoreGuidCollection valoreGuidCollection = ItemsView.EntitiesHelper.GetValoreAttributo(ent, att.Codice, false, false) as ValoreGuidCollection;
                            FilterData filterData = valoreGuidCollection.Filter;
                            valConds = filterData.ToConditions();
                        }

                        if (valConds == null)
                            valConds = new ValoreConditions();

                        bool allowAccept = !(att.IsValoreLockedByDefault || att.IsValoreReadOnly);

                        if (WindowService.SetValoreConditionsWnd(entityType.GetKey(), valConds, allowAccept))
                        {
                            FilterData filterData = new FilterData();
                            filterData.FromConditions(entityType.GetKey(), valConds);

                            ValoreGuidCollection valoreGuidCollection = new ValoreGuidCollection() { Filter = filterData };
                            ItemsView.AttributiEntities.SetValoreAttributo(att.Codice, valoreGuidCollection);
                            await ItemsView.UpdateCache(true);

                        }

                    }
                }
            }


        }

        /// <summary>
        /// Sostituisci il guid di divisione nel item corrente
        /// </summary>
        public void ReplaceCurrentItemDivisioneGuid(AttributoRiferimento attRif)
        {
            if (ItemsView.IsMultipleModify && !ItemsView.IsAnyChecked)
                return;

            if (!ItemsView.IsMultipleModify && CurrentItem == null)
                return;


            DataService.Suspended = true;
            //SelectDivisioneItemIdWindow selectCategoriaIdWnd = new SelectDivisioneItemIdWindow();

            //ComputoItemView currentComputoItemView = null;
            //if (CurrentItem != null)
            //    currentComputoItemView = (CurrentItem as VirtualListItem<EntityView>).Data as ItemView;
            EntityView currentItemView = null;
            if (ItemsView.SelectedEntityId != Guid.Empty)
                currentItemView = ItemsView.SelectedEntityView;


            DivisioneItemType divType = DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey] as DivisioneItemType;

            if (divType == null)
                return;

            List<Guid> selectedItems = new List<Guid>();

            if (currentItemView != null) //esiste una posizione di inserimento
            {
                //DivisioneItem divisioneItem = currentItemView.GetDivisioneItem(attRif.ReferenceCodiceGuid, attRif.ReferenceEntityTypeKey);
                //if (divisioneItem != null)
                //    selectedItems.Add(divisioneItem.Id);

                Guid id = currentItemView.GetAttributoGuidId(attRif.ReferenceCodiceGuid);
                if (id != Guid.Empty)
                    selectedItems.Add(id);
            }


            string title = LocalizationProvider.GetString("Sostituisci");
            if (WindowService.SelectDivisioneIdsWindow(divType.DivisioneId, ref selectedItems, title, SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, attRif))
            {
                if (selectedItems.Count == 1)
                {
                    List<Guid> entitiesId = null;
                    if (ItemsView.IsMultipleModify)
                        entitiesId = new List<Guid>(ItemsView.ReadyToModifyEntitiesId);
                    else
                        entitiesId = ItemsView.AttributiEntities.EntitiesId;

                    HashSet<Guid> modifiedEntitiesid = new HashSet<Guid>(ItemsView.ReadyToModifyEntitiesId);

                    Guid divisioneItemId = selectedItems.First();
                    ItemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = divisioneItemId });

                    //Aggiorno il gruppo di appartenenza
                    ItemsView.UpdateGroupsKey(attRif, entitiesId);

                    ItemsView.UpdateCache(true);
                }
            }
            else
            {
                //potrei aver fatto modifiche alla divisione
                ItemsView.UpdateCache(true);
            }


        }

        public void Clear()
        {
            ItemsView.EntityType?.AttributiMasterCodes.Clear();
            RightPanesView.Clear();
            RightPanesView.ClosePanes();
        }

        public virtual bool HasTableSummaryRow() => false;

    }



    public class MasterDetailGridItemsViewVirtualized : EntitiesListMasterDetailView, IVirtualListLoader<EntityView>
    {
        MasterDetailGridView _owner = null;
        public MasterDetailGridView Owner { get => _owner; }

        /// <summary>
        /// Gruppi di appartenenza dell'elemento corrente
        /// </summary>
        //internal List<object> CurrentItemGroupKey { get; set; } = new List<object>();

        public event EventHandler ItemsLoading;
        public event EventHandler ItemsLoaded;


        //key: Codice attributo, value: column mappingName
        public Dictionary<string, string> ColumnsMappingName { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Method to load items which assigned to the action of IncrementalList
        /// </summary>
        /// <param name="count"></param>
        /// <param name="baseIndex"></param>


        public MasterDetailGridItemsViewVirtualized(MasterDetailGridView masterDetailGridView) : base()
        {
            _owner = masterDetailGridView;
        }

        protected override void OnItemsLoading(EventArgs e)
        {
            ItemsLoading?.Invoke(this, e);
        }

        protected override void OnItemsLoaded(EventArgs e)
        {
            ItemsLoaded?.Invoke(this, e);
        }

        public override async void Load()
        {
            try
            {
                IsModelToViewLoading = true;
                //ModelState.ModelToViewLoading = true;

                ApplyFilterAndSort(SelectedEntityId);


                VirtualEntities = new VirtualList<EntityView>(this);
                VirtualEntities.LoadingStateChanged += VirtualEntities_LoadingStateChanged;

                Entities = new ObservableCollection<EntityView>(VirtualEntities.Select(item => item.Data));

                
                base.Load();

                if (VirtualEntities != null)
                {
                    OnItemsLoading(new EventArgs());
                    
                }

                IsModelToViewLoading = false;
                //ModelState.ModelToViewLoading = false;

            }
            catch (Exception e)
            {

                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
        }

        protected virtual EntityView NewItemView(Entity entity) { return null; }

        private void VirtualEntities_LoadingStateChanged(object sender, EventArgs e)
        {
            //_selectedIndex -> CurrentItem
            Debug.Print("VirtualEntities_LoadingStateChanged");

            if (IsModelToViewLoading == true)
                return;

            Entities = new ObservableCollection<EntityView>(VirtualEntities.Select(item => item.Data));

            int selectedIndex = _selectedIndex;

            if (selectedIndex == -1)
            {
                if (_selectedEntityId != Guid.Empty)
                    selectedIndex = FilteredIndexOf(_selectedEntityId);
                else if (Owner.CurrentItem != null)//nessuna preferenza sul current item. Lascio decidere alla griglia
                {
                    VirtualListItem<EntityView> item = Owner.CurrentItem as VirtualListItem<EntityView>;
                    selectedIndex = item.Index;
                }

                if (UpdateDetail && selectedIndex == -1)
                    SelectEntityView(null);


            }


            if (0 <= selectedIndex && selectedIndex < VirtualEntities.Count/* && VirtualEntities[selectedIndex].Data != null && VirtualEntities[selectedIndex].Data.Id == _selectedEntityId*/)
            {
                if (UpdateDetail)//viene utilizzato per aggiornare il datail dopo una modifica ad una cella della griglia
                {
                    if (VirtualEntities.LoadingState == QueuedBackgroundWorkerState.Standby)
                    {
                        AttributiEntities.UpdateValues();

                        _selectedEntityView = null;
                        UpdateDetail = false;
                    }
                }
                

                if (SelectedIndex == selectedIndex)
                {
                    //Owner.CurrentItem = VirtualEntities[selectedIndex];
                    SelectEntityView(VirtualEntities[selectedIndex].Data);
                }
                else
                {
                    SelectIndex(selectedIndex);
                    //SelectedIndex = selectedIndex;
                }
            }

            if (_selectedEntityId == Guid.Empty)
                Owner.CurrentItem = null;


            UpdateUI();


            //PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;
            OnItemsLoaded(new EventArgs());

            




        }


        VirtualList<EntityView> _virtualEntities = null;
        public VirtualList<EntityView> VirtualEntities
        {
            get { return _virtualEntities; }
            set { SetProperty(ref _virtualEntities, value); }
        }

        public override EntityView GetEntityViewByIndex(int index)
        {
            EntityView entView = null;

            if (_virtualEntities == null)
                return null;

            if (0 <= index && index < VirtualEntities.Count)
            {
                entView = VirtualEntities[index].Data;
            }

            if (0 <= index && index < Entities.Count)
            {
                entView = Entities[index];
            }

            if (entView == null)
            {
                if (0 <= index && index < FilteredEntitiesId.Count)
                {
                    Entity ent = DataService.GetEntityById(EntityType.GetKey(), FilteredEntitiesId[index]);
                    if (ent != null)
                    {
                        entView = NewItemView(ent);
                        Entities[index] = entView;
                    }
                }
            }
            
            return entView;
        }
        public async override Task UpdateCache(bool updateDetail = false)
        {
            try
            {

                if (IsModelToViewLoading)
                    return;

                IsModelToViewLoading = true;

                if (ModelActionsStack != null)
                    UpdateCacheModelActionIndex = ModelActionsStack.GetCount();


                //if (ModelState.ModelToViewLoading)
                //    return;

                //ModelState.ModelToViewLoading = true;

                UpdateDetail = updateDetail;

                if (VirtualEntities != null)
                {
                    //PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                    OnItemsLoading(new EventArgs());
                }

                IsModelToViewLoading = false;
                //ModelState.ModelToViewLoading = false;
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }
        }


        public override async Task SelectEntityById(Guid entityId, bool scrollToEntity = true)
        {

            if (Entities == null)
                return;

            _selectedEntityId = entityId;


            if (_selectedEntityId != Guid.Empty)
            {
                int selectedIndex = FilteredIndexOf(entityId);

                _selectedIndex = selectedIndex;

                if (selectedIndex >= 0)
                {

                    if (scrollToEntity)
                    {
                        CheckedEntitiesId.Add(entityId);

                        
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                    }
                }
            }
            PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
            PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;

            await UpdateCache();

        }

        // This method helps to get dependency property value in another thread.
        // Usage: Invoke(() => { return CreationOverhead; });
        private T Invoke<T>(Func<T> callback)
        {
            //return (T)Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
            return (T)Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
        }

        public virtual IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            //overallCount = 0;
            //return new List<EntityView>();

            IsModelToViewLoading = true;

            string entityTypeKey = EntityType.GetKey();

            int minIndex = Math.Min(startIndex, FilteredEntitiesId.Count - 1);
            int maxIndex = (int)Math.Min(startIndex + count, FilteredEntitiesId.Count) - 1;

            if (minIndex < 0 || maxIndex < 0 || DataService.Suspended)
            {
                overallCount = 0;
                IsModelToViewLoading = false;
                return new List<EntityView>();
            }

            List<Guid> ids = FilteredEntitiesId.GetRange(minIndex, maxIndex - minIndex + 1);

            IEnumerable<Entity> listEnts = DataService.GetCloneEntitiesById(entityTypeKey, ids);

            overallCount = Invoke(() => { return FilteredEntitiesId.Count; });

            // because the all fields are sorted ascending, the PropertyName is ignored in this sample
            // only Direction is considered.
            //SortDescription sortDescription = sortDescriptions == null || sortDescriptions.Count == 0 ? new SortDescription() : sortDescriptions[0];
            //ListSortDirection direction = string.IsNullOrEmpty(sortDescription.PropertyName) ? ListSortDirection.Ascending : sortDescription.Direction;


            //_entities = new List<TreeEntityView>();
            List<EntityView> ents = new List<EntityView>();
            foreach (Entity ent in listEnts)
            {
                EntityView newItem = NewItemView(ent);// new ElementiItemView(this, ent);

                if (CheckedEntitiesId.Contains(ent.EntityId))
                    newItem.SetChecked(true);

                ents.Add(newItem);
            }

            IsModelToViewLoading = false;
            return ents;
        }


        public bool CanSort => true;


        public override void AddEntity()
        {
            base.AddEntity();
        }


        public override void DeleteCheckedEntities()
        {
            base.DeleteCheckedEntities();
        }

        
        public override void UpdateIsAllChecked()
        {
            //base.UpdateIsAllChecked();
            //MasterDetailGridView.UpdateUI();

        }

        public override void OnCheckedEntity()
        {
            base.OnCheckedEntity();
        }

        public async void UpdateEntityType()
        {
            string entityTypeKey = EntityType.GetKey();
            EntityType = DataService.GetEntityTypes()[entityTypeKey];
            EntityType.ResolveReferences(DataService.GetEntityTypes(), DataService.GetDefinizioniAttributo());

            RightPanesView.UpdateAttributi();

            AttributiEntities.Load(new HashSet<Guid>());
            ApplyFilterAndSort(_selectedEntityId);
            PendingCommand |= EntitiesListMasterDetailViewCommands.UpdateGridColumns;
            await UpdateCache(true);
        }

        public override void UpdateSelectedItems(bool selectAll = false)
        {
            if (selectAll)
                PendingCommand |= EntitiesListMasterDetailViewCommands.SelectAll;
            else
                PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;

            OnItemsLoaded(new EventArgs());
        }

        public override void ReplaceValore(ValoreView valoreView)
        {
            if (DataService == null || DataService.IsReadOnly)
                return;

            string codiceAttributo = valoreView.Tag as string;
            Attributo att = EntityType.Attributi[codiceAttributo];

            if (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = EntityType.Attributi[codiceAttributo] as AttributoRiferimento;
                _owner.ReplaceCurrentItemAttributoGuid(attRif, null);
            }
            else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {
                _owner.ReplaceCurrentItemAttributoGuid(att, null);

                //    ValoreAttributoGuidCollection settings = att.ValoreAttributo as ValoreAttributoGuidCollection;
                //    if (settings != null)
                //    {
                //        if (settings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
                //        {
                //            ValoreGuidCollection valoreGuidCollection = EntitiesHelper.GetValoreAttributo(EntityType.GetKey(), codiceAttributo, false, false) as ValoreGuidCollection;
                //            FilterData filterData = valoreGuidCollection.Filter;

                //            AttributoFilterData attFilterData = filterData.Items.FirstOrDefault();
                //            if (attFilterData == null)
                //                attFilterData = new AttributoFilterData();

                //            if (WindowService.SelectAttributoFilterWindow(new HashSet<string>() { EntityType.GetKey() }, FilteredEntitiesId, ref attFilterData))
                //            {
                //                valoreGuidCollection = new ValoreGuidCollection() { Filter = filterData };

                //                AttributiEntities.SetValoreAttributo(codiceAttributo, valoreGuidCollection);
                //            }

                //        }
                //    }
            }
        }


    }

}