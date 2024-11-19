
using Commons;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MasterDetailModel;
using System.Windows;
using CommonResources;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using System.Windows.Documents;
using DevExpress.XtraEditors;
using Syncfusion.Windows.Primitives;


namespace MasterDetailView
{

    public class GroupView : NotificationBase, RightPaneView
    {

        protected EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public GroupData Data { get; set; }

        public GroupView(EntitiesListMasterDetailView master)
        {
            //This = this;
            _master = master;

            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUI();
        }

        ObservableCollection<AttributoGroupView> _items = new ObservableCollection<AttributoGroupView>();
        public ObservableCollection<AttributoGroupView> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public bool IsApplied()
        {
            if (Items.Count > 0)
                return true;

            return false;
        }


        AttributoGroupView NewAttributoGroup { get; set; } = null;

        AttributoGroupView _currentAttributo;
        public AttributoGroupView CurrentAttributo
        {
            get { return _currentAttributo; }
            set
            {
                if (SetProperty(ref _currentAttributo, value))
                {
                    foreach (AttributoGroupView att in Items)
                        att.IsVisible = false;

                    if (_currentAttributo != null)
                    {
                        _currentAttributo.IsVisible = true;
                    }
                }
            }
        }


        /// <summary>
        /// Carica da settings precedenti
        /// </summary>
        /// <param name="viewSettings"></param>
        public void Load(EntityTypeViewSettings viewSettings)
        {
            if (viewSettings == null)
                return;

            if (Data == null)
                Data = new GroupData();

            Items.Clear();
            Data.Items.Clear();

            Master.IsMultipleModify = false;

            foreach (AttributoGroupData groupAtt in viewSettings.Groups)
            {
                if (Master.EntityType.Attributi.ContainsKey(groupAtt.CodiceAttributo))
                {

                    AttributoGroupData clone = groupAtt.Clone();

                    Items.Add(new AttributoGroupView(Master, clone));
                    Data.Items.Add(clone);
                }
            }

            UpdateUI();
        }

        public void Load(Attributo tipoAtt, AttributoGroupView target = null)
        {
            //var attSource = Master.EntitiesHelper.GetSourceAttributo(tipoAtt);

            //if (attSource != null && !attSource.AllowMasterGrouping)
            //{
            //    Master.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileRaggrupparePerQuestoAttributo"));
            //    return;
            //}

            if (tipoAtt != null && !tipoAtt.AllowMasterGrouping)
            {
                Master.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileRaggrupparePerQuestoAttributo"));
                return;
            }


            Master.IsMultipleModify = false;

            //nessun raggruppamento per lo stesso attributo
            //if (Items.FirstOrDefault(item => item.Attributo.Codice == tipoAtt.Codice) != null)
            //    return;

            //in ogni caso elimino il precedente e lo reinserisco nella corretta posizione
            var found = Items.FirstOrDefault(item => item.Attributo.Codice == tipoAtt.Codice);
            if (found != null)
                Items.Remove(found);
            

            NewAttributoGroup = new AttributoGroupView(Master, new AttributoGroupData() { CodiceAttributo = tipoAtt.Codice});

            int targetIndex = -1;
            if (target != null)
                targetIndex = Items.IndexOf(target);

            if (targetIndex >=0)
                Items.Insert(targetIndex, NewAttributoGroup);
            else
                Items.Add(NewAttributoGroup);

            CurrentAttributo = NewAttributoGroup;
            Master.IsGroupPaneOpen = true;

            Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;

            //Master.SelectedIndex = -1;
            Master.Load();

        }
        public void UnLoad()
        {
            Master.IsMultipleModify = false;

            if (NewAttributoGroup != null && NewAttributoGroup.IsValid())
            {
                Items.Remove(NewAttributoGroup);
                NewAttributoGroup = null;
            }
            Master.ApplyFilterAndSort();
            Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
            Master.Load();


        }


        async void LoadMasterAsync()
        {
            await Task.Run(() => Master.Load());
        }


        //bool _clearAllFiltroVisible = false;
        //public bool IsClearAllGroupVisible
        //{
        //    get { return _clearAllFiltroVisible; }
        //    set { SetProperty(ref _clearAllFiltroVisible, value); }
        //}


        public bool IsClearAllGroupVisible
        {
            get { return Items.Any(); }
        }




        public void Update(string codiceAttributo, string groupKey)
        {
            if (groupKey == null)
                return;

            if (Data != null)
            {
                if (Data.GroupRecords.ContainsKey(groupKey))
                    Data.GroupRecords.Remove(groupKey);
            }
        }

        internal void UpdateAttributi()
        {
            List<AttributoGroupView> attToRemove = Items.Where(item => item.Attributo != null && !Master.EntityType.Attributi.ContainsKey(item.Attributo.Codice)).ToList();

            foreach (var item in attToRemove)
                item.ClearGroup();
        }

        public void Update()
        {
            if (Data != null)
                Data.UpdateGroupRecordsData();
        }


        //bool _synchronizeGroupButtonVisible = false;
        //private EntitiesListMasterDetailView master;

        //public bool SynchronizeGroupButtonVisible
        //{
        //    get { return _synchronizeGroupButtonVisible; }
        //    set { SetProperty(ref _synchronizeGroupButtonVisible, value); }
        //}


        //public ICommand SynchronizeGroupCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.SynchronizeGroup());
        //    }
        //}

        //public void SynchronizeGroup()
        //{

        //    foreach (AttributoGroupView attGroupView in Items)
        //    {
        //        attGroupView.Synchronize();
        //    }
        //    Master.ApplyFilterAndSort();
        //    Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
        //    Master.UpdateCache();
        //}

        public ICommand ClearAllGroupCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearAllGroup());
            }
        }

        public void ClearAllGroup(bool load = true)
        {
            Items.Clear();

            Master.IsMultipleModify = false;

            Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
            //IsClearAllGroupVisible = false;

            if (load)
            {
                Master.Load();
                if (Master.ModelActionsStack != null)
                    Master.ModelActionsStack.OnViewSettingsChanged();
            }

        }

        public void CreateData()
        {
            GroupData data = new GroupData();
            data.EntityTypeKey = Master.EntityType.GetKey();
            foreach (AttributoGroupView attGroupView in Items)
            {
                AttributoGroupData attGroupData = new AttributoGroupData()
                {
                    CodiceAttributo = attGroupView.Attributo.Codice,
                };
                data.Items.Add(attGroupData);
            }
            Data = data;
        }

        //public GroupData GetData()
        //{
        //    GroupData data = new GroupData();
        //    data.CodiceEntity = Master.EntityType.Codice;
        //    foreach (AttributoGroupView attGroupView in Items)
        //    {
        //        AttributoGroupData attGroupData = new AttributoGroupData()
        //        {
        //            CodiceAttributo = attGroupView.Attributo.Codice,
        //        };
        //        data.Items.Add(attGroupData);
        //    }
        //    return data;
        //}

        public string[] SplitGroupKey(string groupKey)
        {
            return Data.SplitGroupKey(groupKey);
        }

        public string JoinGroupKeys(string[] groupKeys)
        {
            return GroupData.JoinGroupKeys(groupKeys);
        }

        public string ItemsCount { get => string.Format("{0}", Items.Count); }

        public bool IsItemsCountVisible { get => Items.Count > 0; }

        private void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ItemsCount));
            RaisePropertyChanged(GetPropertyName(() => IsItemsCountVisible));
            RaisePropertyChanged(GetPropertyName(() => IsClearAllGroupVisible));
        }

        #region Popup

        public ICommand ViewPopupsCommand { get => new CommandHandler(() => this.ViewPopups()); }
        void ViewPopups()
        {
            if (Master.AttributiEntities.AttributiValoriComuniView.Count > 0)
            {
                //Master.AttributiEntities.AttributiValoriComuniView.FirstOrDefault().IsHilighted = true;
                IsHelpPopupOpen = true;
            }
        }

        public ICommand LostFocusCommand { get => new CommandHandler(() => this.LostFocus()); }
        void LostFocus()
        {
            if (Master.AttributiEntities.AttributiValoriComuniView.Count > 0)
            {
                //Master.AttributiEntities.AttributiValoriComuniView.FirstOrDefault().IsHilighted = false;
                IsHelpPopupOpen = false;
            }
        }

        bool _isHelpPopupOpen = false;
        public bool IsHelpPopupOpen
        {
            get => _isHelpPopupOpen;
            set => SetProperty(ref _isHelpPopupOpen, value);
        }
        #endregion

    }


    /// <summary>
    /// Rappresenta un ordinamento per attributo
    /// </summary>
    public class AttributoGroupView : NotificationBase<AttributoGroupData>
    {

        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }
        public AttributoGroupView(EntitiesListMasterDetailView master, AttributoGroupData data) : base(data)
        {
            _master = master;
            Attributo = Master.EntityType.Attributi[data.CodiceAttributo];
            _nome = Attributo.Etichetta;
        }


        public Attributo Attributo { get; set; }
        AttributoGroupData Data { get => This; }

        /// <summary>
        /// Scopo: ordinare al volo gli items in base alla sorgente
        /// </summary>
        List<string> _orderedItems { get; set; } = new List<string>();

        public ICommand ClearGroupCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearGroup());
            }
        }

        public void ClearGroup()
        {
            Master.RightPanesView.GroupView.Items.Remove(this);

            Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
            Master.Load();

            //Master.ApplyFilterAndSort();
            //Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
            //Master.UpdateCache();
            //Master.RightPanesView.GroupView.IsClearAllGroupVisible = Master.RightPanesView.GroupView.Items.Any();

            if (Master.ModelActionsStack != null)
                Master.ModelActionsStack.OnViewSettingsChanged();
        }

        bool _isVisible = false;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        
        string _nome;
        private AttributoGroupData item;

        public string Nome
        {
            get { return _nome; }
        }


        public bool IsValid()
        {
            return true;
        }

        public void UpdateOrderedItems()
        {
            _orderedItems.Clear();

            EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
            Attributo sourceAtt = entsHelper.GetSourceAttributo(Attributo);

            List<Guid> entsFound = null;
            Master.DataService.GetFilteredEntities(sourceAtt.EntityTypeKey, null, null, null, out entsFound);

            Dictionary<string, Guid> keys = Master.DataService.CreateKey(sourceAtt.EntityTypeKey, ";", new List<string>() { sourceAtt.Codice }, out _);

            _orderedItems = keys.Keys.ToList();

        }

        public int Compare(string key1, string key2)
        {
            int key1Index = _orderedItems.IndexOf(key1);
            int key2Index = _orderedItems.IndexOf(key2);

            if (key1Index < key2Index)
                return -1;
            else if (key1Index > key2Index)
                return 1;

            return 0;


        }
    }
}
        