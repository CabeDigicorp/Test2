using _3DModelExchange;
using CommonResources;
using Commons;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MasterDetailView
{

    public class MasterDetailListItemView : EntityView
    {

        public MasterDetailListItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }

        //internal DivisioneItem GetDivisioneItem(string codiceAttributoGuid, string entityTypeKey)
        //{

        //    Guid itemId = Entity.GetAttributoGuidId(codiceAttributoGuid);

        //    if (itemId != Guid.Empty)
        //    {
        //        IEnumerable<Entity> ents;
        //        ents = _master.DataService.GetEntitiesById(entityTypeKey, new List<Guid>() { itemId });
        //        return ents.LastOrDefault() as DivisioneItem;
        //    }
        //    return null;
        //}

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

    public class MasterDetailListView : MasterDetailViewBase
    {
        public new MasterDetailListViewVirtualized ItemsView { get => _itemsView as MasterDetailListViewVirtualized; }

        public override void Refresh(int index)
        {
            //if (ItemsView == null)
            //    return;

            //if (ItemsView.VirtualEntities == null)
            //    return;

            ItemsView.VirtualEntities.Refresh(index);
        }
    }



    /// <summary>
    /// View di master-detail con ListView virtualizzata
    /// </summary>
    public class MasterDetailListViewVirtualized : EntitiesListMasterDetailView, IVirtualListLoader<EntityView>
    {
        MasterDetailListView _owner = null;
        MasterDetailListView Owner { get => _owner; }

        protected List<Entity> _loadingEntities = null;

        VirtualList<EntityView> _virtualEntities = null;
        public VirtualList<EntityView> VirtualEntities
        {
            get { return _virtualEntities; }
            set { SetProperty(ref _virtualEntities, value); }
        }


        public event EventHandler RefreshView;
        public event EventHandler ItemsLoaded;

        public MasterDetailListViewVirtualized(MasterDetailListView owner) : base()
        {
            _owner = owner;
        }

        public override void Init()
        {
            base.Init();
        }

        public override void SelectIndex(int index, bool userOrigin = false)
        {
            if (SetProperty(ref _selectedIndex, index))
            {
                EntityView entView = GetEntityViewByIndex(_selectedIndex);
                if (entView != null)
                {
                    _selectedEntityId = entView.Id;
                    SelectEntityView(entView);
                    SetNoSelectionChecked(false);
                }
                else
                {
                    _selectedIndex = -1;
                    _selectedEntityView = null;
                }

                //NB: commentato perchè bisogna che vengano selezionate i figli anche se se non userOrigin (ad esempio dopo una cancellazione)
                if (/*userOrigin && */_selectedIndex >= 0)
                {
                    if (userOrigin)
                    {
                        if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify)
                            IsMultipleModify = false;

                        CheckedEntitiesId.Clear();
                    }

                    if (_selectedEntityId != Guid.Empty)
                    {
                        FirstShiftCheckEntityId = _selectedEntityId;
                        CheckEntityById(_selectedEntityId, true);

                    }
                }
            }

            UpdateUI();
        }

        public override EntityView GetEntityViewByIndex(int index)
        {
            if (_virtualEntities == null)
                return null;

            if (0 <= index && index < VirtualEntities.Count)
            {
                return VirtualEntities[index].Data;
            }

            return null;
        }

        public event EventHandler ScrollIntoView;
        protected virtual void OnScrollIntoView(EventArgs e)
        {
            ScrollIntoView?.Invoke(this, e);
        }

        public override async void Load()
        {
            try
            {
                IsModelToViewLoading = true;
                //ModelState.ModelToViewLoading = true;
                _checkedEntitiesId.Clear();

                ApplyFilterAndSort();

                VirtualEntities = new VirtualList<EntityView>(this);
                VirtualEntities.LoadingStateChanged += LoadingStateChanged;

                Entities = new ObservableCollection<EntityView>(VirtualEntities.Select(item => item.Data));

                base.Load();

                IsModelToViewLoading = false;
                //ModelState.ModelToViewLoading = false;

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
        }

        protected override void OnItemsLoading(EventArgs e)
        {
            RefreshView?.Invoke(this, e);
        }

        protected override void OnItemsLoaded(EventArgs e)
        {
            ItemsLoaded?.Invoke(this, e);
        }

        /// <summary>
        /// Ultimo evento che indica la fine della realizzazione delle entità della lista
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadingStateChanged(object sender, EventArgs e)
        {
            if (IsModelToViewLoading == true)
                return;

            //if (PendingCommand != EntitiesListMasterDetailViewCommands.Nessuno)
            //    return;


            try
            {

                Entities = new ObservableCollection<EntityView>(VirtualEntities.Select(item => item.Data));

                int selectedIndex = _selectedIndex;

                if (selectedIndex == -1 || _selectedEntityId != Guid.Empty)
                {
                    selectedIndex = FilteredIndexOf(_selectedEntityId);
                }

                //if (0 <= selectedIndex && selectedIndex < VirtualEntities.Count && VirtualEntities[selectedIndex].Data != null && VirtualEntities[selectedIndex].Data.Id == _selectedEntityId)
                if (0 <= selectedIndex && selectedIndex < VirtualEntities.Count)
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
                        //SelectIndex(selectedIndex);
                        SelectEntityView(VirtualEntities[selectedIndex].Data);
                    }
                    else
                    {
                        SelectIndex(selectedIndex);
                    }
                }

                if (_selectedEntityId == Guid.Empty && !IsMultipleModify)
                    SelectEntityView(null);

                UpdateUI();

                //PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                OnItemsLoaded(new EventArgs());

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
                //MessageBox.Show(ex.Message, LocalizationProvider.GetString("AppName"));
            }

        }

        /// <summary>
        /// Permette la realizzazione delle entità in base alla posizione dello scroll
        /// </summary>
        /// <param name="updateDetail"></param>
        /// <returns></returns>
        public async override Task UpdateCache(bool updateDetail = false)
        {
            IsModelToViewLoading = true;

            if (ModelActionsStack != null)
                UpdateCacheModelActionIndex = ModelActionsStack.GetCount();

            try
            {
                

                UpdateDetail = updateDetail;


                if (VirtualEntities != null)
                {
                    OnItemsLoading(new EventArgs());
                }

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
                //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
            }

            IsModelToViewLoading = false;
            //ModelState.ModelToViewLoading = false;
        }

        /// <summary>
        /// Premette la selezione di una entità e la realizzazione delle entità in base alla posizione dello scroll
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="scrollToEntity"></param>
        /// <returns></returns>
        public override async Task SelectEntityById(Guid entityId, bool scrollToEntity = true)
        {
            if (VirtualEntities == null)
                return;

            _selectedEntityId = entityId;

            if (_selectedEntityId != Guid.Empty)
            {
                int selectedIndex = FilteredIndexOf(_selectedEntityId);
                if (selectedIndex < 0 && entityId != Guid.Empty)
                {
                    //Aggiungo nella lista flat l'entità cercata e i suoi padri
                    //selectedIndex = ShowEntity(entityId);
                }

                //NON POSSO SELEZIONARE IN BASE ALL'INDICE SE Entities sono ancora vecchie
                //SelectIndex(selectedIndex);
                _selectedIndex = selectedIndex;

                if (selectedIndex >= 0)
                {
                    //ComputoView.CurrentItem = VirtualEntities[selectedIndex];
                    if (scrollToEntity)
                    {
                        //CheckedEntitiesId.Clear();//commentato perchè voglio riuscire ad impostare il corrente senza togliere la selezione
                        CheckedEntitiesId.Add(entityId);
                        //PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCurrentEntityGroup;
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                        PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                    }
                }

            }
            else
            {
                _selectedIndex = -1;
            }


            //if (scrollToEntity)
            //    PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;

            await UpdateCache();
        }


        // This method helps to get dependency property value in another thread.
        // Usage: Invoke(() => { return CreationOverhead; });
        private T Invoke<T>(Func<T> callback)
        {
            //return (T)Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
            return (T)Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
        }

        public bool CanSort => true;

        /// <summary>
        /// Realizza le entità in base all'indice richiesto
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="sortDescriptions"></param>
        /// <param name="overallCount"></param>
        /// <returns></returns>
        public IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            IsModelToViewLoading = true;
            //ModelState.ModelToViewLoading = true;

            List<EntityView> entsView = new List<EntityView>();

            try
            {
                if (EntityType == null)
                {
                    IsModelToViewLoading = false;
                    overallCount = 0;
                    return entsView;
                }

                string entityTypeKey = EntityType.GetKey();

                int minIndex = Math.Min(startIndex, FilteredEntitiesId.Count - 1);
                int maxIndex = Math.Min(startIndex + count, FilteredEntitiesId.Count) - 1;

                if (minIndex < 0 || maxIndex < 0 || DataService.Suspended)
                {
                    overallCount = 0;
                    IsModelToViewLoading = false;
                    //ModelState.ModelToViewLoading = false;
                    return entsView;
                }
                List<Guid> ids = FilteredEntitiesId.GetRange(minIndex, maxIndex - minIndex + 1);
                /*List<Entity> ents */
                _loadingEntities = DataService.GetCloneEntitiesById(entityTypeKey, ids);

                overallCount = Invoke(() => { return FilteredEntitiesId.Count; });

                // because the all fields are sorted ascending, the PropertyName is ignored in this sample
                // only Direction is considered.
                SortDescription sortDescription = sortDescriptions == null || sortDescriptions.Count == 0 ? new SortDescription() : sortDescriptions[0];
                ListSortDirection direction = string.IsNullOrEmpty(sortDescription.PropertyName) ? ListSortDirection.Ascending : sortDescription.Direction;


                //foreach (Entity ent in ents)
                foreach (Entity ent in _loadingEntities)
                {
                    //PrezzarioItemView newItem = new PrezzarioItemView(this, ent);
                    EntityView newItem = NewItemView(ent);

                    //FilteredEntitiesViewInfo[ent.Id].Depth = ent.Depth;
                    //newItem.IsExpanded = FilteredEntitiesViewInfo[ent.Id].IsExpanded;

                    if (CheckedEntitiesId.Contains(ent.EntityId))
                        newItem.SetChecked(true);

                    entsView.Add(newItem);
                }

            }
            catch (Exception ex)
            {
                overallCount = 0;
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
                //MessageBox.Show(ex.Message, LocalizationProvider.GetString("AppName"));
            }

            IsModelToViewLoading = false;
            //ModelState.ModelToViewLoading = false;

            return entsView;
        }


        protected virtual EntityView NewItemView(Entity entity) { return null; }

        public override void ReplaceValore(ValoreView valoreView)
        {
            //if (valoreView is ValoreCollectionView)
            //{
            //    string codiceAttributo = valoreView.Tag as string;
            //    AttributoRiferimento attRif = EntityType.Attributi[codiceAttributo] as AttributoRiferimento;
            //    if (attRif == null)
            //        return;

            //    _owner.ReplaceCurrentItemAttributoGuid(attRif);
            //}

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

            }
        }
    }
}
