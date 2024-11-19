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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MasterDetailView
{
    public class MasterDetailTreeItemView : TreeEntityView
    {
        public MasterDetailTreeItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }


    public class MasterDetailTreeView : MasterDetailViewBase
    {
        public new MasterDetailTreeViewVirtualized ItemsView { get => _itemsView as MasterDetailTreeViewVirtualized; }

        public override void Refresh(int index)
        {
            ItemsView.VirtualEntities.Refresh(index);
        }
    }

    /// <summary>
    /// View di master-detail con ListView a tree
    /// </summary>
    public class MasterDetailTreeViewVirtualized : EntitiesTreeMasterDetailView, IVirtualListLoader<EntityView>
    {
        protected MasterDetailTreeView _owner = null;
        public MasterDetailTreeView View { get => _owner; }

        protected List<TreeEntity> _loadingEntities = null;

        VirtualList<EntityView> _virtualEntities = null;
        public VirtualList<EntityView> VirtualEntities
        {
            get { return _virtualEntities; }
            set { SetProperty(ref _virtualEntities, value); }
        }


        public event EventHandler RefreshView;
        public event EventHandler ItemsLoaded;

        public MasterDetailTreeViewVirtualized(MasterDetailTreeView owner) : base()
        {
            _owner = owner;
        }

        public override void Init()
        {
            base.Init();
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
        protected virtual void LoadingStateChanged(object sender, EventArgs e)
        {
            if (IsModelToViewLoading == true)
                return;

            try
            {

                Entities = new ObservableCollection<EntityView>(VirtualEntities.Select(item => item.Data));

                int selectedIndex = _selectedIndex;

                if (selectedIndex == -1 || _selectedEntityId != Guid.Empty)
                {
                    selectedIndex = DisplayedIndexOf(_selectedEntityId);
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
                int selectedIndex = DisplayedIndexOf(_selectedEntityId);
                if (selectedIndex < 0 && entityId != Guid.Empty)
                {
                    //Aggiungo nella lista flat l'entità cercata e i suoi padri
                    selectedIndex = ShowEntity(entityId);
                }

                //NON POSSO SELEZIONARE IN BASE ALL'INDICE SE Entities sono ancora vecchie
                //SelectIndex(selectedIndex);
                _selectedIndex = selectedIndex;

                if (selectedIndex >= 0)
                {
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


            //PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;

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
        public virtual IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            IsModelToViewLoading = true;
            //ModelState.ModelToViewLoading = true;

            List<EntityView> ents = new List<EntityView>();

            try
            {
                if (EntityType == null)
                {
                    IsModelToViewLoading = false;
                    overallCount = 0;
                    return ents;
                }

                string entityTypeKey = EntityType.GetKey();

                int minIndex = Math.Min(startIndex, DisplayedEntitiesId.Count - 1);
                int maxIndex = Math.Min(startIndex + count, DisplayedEntitiesId.Count) - 1;


                if (minIndex < 0 || maxIndex < 0 || DataService.Suspended)
                {
                    overallCount = 0;
                    IsModelToViewLoading = false;
                    //ModelState.ModelToViewLoading = false;
                    return ents;
                }
                List<Guid> ids = DisplayedEntitiesId.GetRange(minIndex, maxIndex - minIndex + 1);

                _loadingEntities = DataService.GetCloneTreeEntitiesById(entityTypeKey, ids);
                //_loadingEntities = DataService.GetTreeEntitiesById(entityTypeKey, ids);
                //List <TreeEntity> treeEnts = 

                overallCount = Invoke(() => { return DisplayedEntitiesId.Count; });

                // because the all fields are sorted ascending, the PropertyName is ignored in this sample
                // only Direction is considered.
                SortDescription sortDescription = sortDescriptions == null || sortDescriptions.Count == 0 ? new SortDescription() : sortDescriptions[0];
                ListSortDirection direction = string.IsNullOrEmpty(sortDescription.PropertyName) ? ListSortDirection.Ascending : sortDescription.Direction;


                foreach (TreeEntity ent in _loadingEntities)
                {
                    //PrezzarioItemView newItem = new PrezzarioItemView(this, ent);
                    TreeEntityView newItem = NewItemView(ent);

                    if (FilteredEntitiesViewInfo.ContainsKey(ent.EntityId))
                    {
                        FilteredEntitiesViewInfo[ent.EntityId].Depth = ent.Depth;
                        newItem.IsExpanded = FilteredEntitiesViewInfo[ent.EntityId].IsExpanded;
                    }

                    if (CheckedEntitiesId.Contains(ent.EntityId))
                        newItem.SetChecked(true);

                    ents.Add(newItem);
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

            return ents;
        }

        protected virtual TreeEntityView NewItemView(TreeEntity entity) { return null; }

        public override void ReplaceValore(ValoreView valoreView)
        {
            if (DataService == null || DataService.IsReadOnly)
                return;

            string codiceAttributo = valoreView.Tag as string;
            Attributo att = EntityType.Attributi[codiceAttributo];

            if (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = EntityType.Attributi[codiceAttributo] as AttributoRiferimento;

                if (attRif.ReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                {
                    _owner.ReplaceCurrentItemDivisione(attRif);
                }
                else
                {
                    _owner.ReplaceCurrentItemAttributoGuid(attRif, null);
                }
            }
            else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {

            }


            

        }

    }


}
