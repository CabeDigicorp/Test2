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

namespace AttivitaWpf
{
    public class CreateWBSItemsView : NotificationBase
    {
        //I/O
        public List<Guid> EntsIdToFilter { get; set; } = new List<Guid>();
        public WBSItemsCreationData Data { get; set; } = new WBSItemsCreationData();
        //

        static WBSItemsCreationData _dataTemp = new WBSItemsCreationData();
        public IDataService DataService { get; set; }
        Dictionary<string, EntityType> _entityTypes = null;
        
        EntitiesHelper _entitiesHelper = null;
        public EntitiesHelper EntitiesHelper => _entitiesHelper;

        public List<AttributoItemView> AttributiItem { get; protected set; } = null;

        SetAttributoFilterView _attributoFilterView = null;
        public SetAttributoFilterView AttributoFilterView
        {
            get => _attributoFilterView;
            set
            {
                SetProperty(ref _attributoFilterView, value);
            }
        }

        private ObservableCollection<WBSLevelView> _levelItems = new ObservableCollection<WBSLevelView>();
        public ObservableCollection<WBSLevelView> LevelItems
        {
            get { return _levelItems; }
            set { SetProperty(ref _levelItems, value); }
        }

        string CurrentEntityTypeKey
        {
            get
            {
                if (_entityTypeIndex == 0)
                    return BuiltInCodes.EntityType.Computo;
                else if (_entityTypeIndex == 1)
                    return BuiltInCodes.EntityType.Elementi;

                return string.Empty;
            }
        }

        internal void Load()
        {
            Data = _dataTemp.Clone();
            _entityTypes = DataService.GetEntityTypes();
            _entitiesHelper = new EntitiesHelper(DataService);

            if (Data.EntityTypeKey == BuiltInCodes.EntityType.Computo)
                EntityTypeIndex = 0;
            else if (Data.EntityTypeKey == BuiltInCodes.EntityType.Elementi)
                EntityTypeIndex = 1;
            else
                EntityTypeIndex = 0;

            LoadLevelItemsView(0);

            //IsAddAttivita = Data.IsAddAttivita;

        }

        internal void Accept()
        {
            _dataTemp = Data;
        }

        void LoadAttributoFilterView()
        {
            

            AttributoFilterView = new SetAttributoFilterView();
            AttributoFilterView.AttributoChanged += AttributoFilterView_AttributoChanged;
            AttributoFilterView.ValoriCheckedChanged += AttributoFilterView_ValoriCheckedChanged;

            AttributoFilterView.DataService = DataService;
            AttributoFilterView.EntityTypesKey = new HashSet<string>() { CurrentEntityTypeKey };
            

            if (CurrentLevel != null)
            {
                List<Guid> entsToFilter = ApplyUpperFilter();
                AttributoFilterView.AttributoFilterData = Data.Items[_levelItems.IndexOf(CurrentLevel)].AttributoFilterData;
                AttributoFilterView.EntsIdToFilter = entsToFilter;
            }
            else
                AttributoFilterView.AttributoFilterData = new AttributoFilterData();
            
            //if (IsValoriAuto)
            //    AttributoFilterView.AttributoFilterData.IsAllChecked = true;

            AttributoFilterView.Load();

            AttributiItem = new List<AttributoItemView>(AttributoFilterView.AttributiItem);
        }

        List<Guid> ApplyUpperFilter()
        {
            if (CurrentLevel == null)
                return null;

            int  currIndex = _levelItems.IndexOf(CurrentLevel);

            FilterData filterData = new FilterData();

            for (int i = 0; i< currIndex; i++)
            {
                if (i >= Data.Items.Count)
                    continue;

                AttributoFilterData attFilterData = Data.Items[i].AttributoFilterData;
                attFilterData.FoundEntitiesId = null;
                filterData.Items.Add(attFilterData);
            }

            if (!filterData.Items.Any())
                return EntsIdToFilter;

            HashSet<Guid> entsToFilter = DataService.ApplyFilter(CurrentEntityTypeKey, new HashSet<Guid>(EntsIdToFilter), filterData);
            return entsToFilter.ToList();
        }

        private void AttributoFilterView_AttributoChanged(object sender, EventArgs e)
        {
            UpdateUI();
            UpdateLevelItemsView();
        }

        private void AttributoFilterView_ValoriCheckedChanged(object sender, EventArgs e)
        {
            UpdateLevelItemsView();
        }


        int _entityTypeIndex = -1;
        public int EntityTypeIndex
        {
            get => _entityTypeIndex;
            set
            {
                if (SetProperty(ref _entityTypeIndex, value))
                {
                    List<Guid> entitiesFound = null;
                    DataService.GetFilteredEntities(CurrentEntityTypeKey, null, null, null, out entitiesFound);
                    EntsIdToFilter = entitiesFound;

                    Data.EntityTypeKey = CurrentEntityTypeKey;

                    //if (CurrentEntityTypeKey != BuiltInCodes.EntityType.Computo)
                    //    IsAddAttivita = false;

                    UpdateUI();
                }
            }
        }

        public bool IsEntityTypeReadOnly { get => _levelItems.Any(); }

        //bool _isAddAttivita = false;
        //public bool IsAddAttivita
        //{
        //    get => _isAddAttivita;
        //    set
        //    {
        //        if (SetProperty(ref _isAddAttivita, value))
        //            Data.IsAddAttivita = value;
        //    }
        //}

        public bool IsAddAttivitaEnabled { get => CurrentEntityTypeKey == BuiltInCodes.EntityType.Computo;  }



        public ICommand AddLevelCommand { get { return new CommandHandler(() => this.AddLevel()); } }
        void AddLevel()
        {
            WBSLevel lev = new WBSLevel();
            lev.AttributoFilterData.EntityTypeKey = CurrentEntityTypeKey;
            int newIndex = Data.Items.Count;

            if (CurrentLevel != null)
            {
                newIndex = _levelItems.IndexOf(CurrentLevel) + 1;
                if (0 <= newIndex && newIndex < Data.Items.Count)
                    Data.Items.Insert(newIndex, lev);
                else
                    Data.Items.Add(lev);
            }
            else
            {
                Data.Items.Add(lev);
            }
            

            LoadLevelItemsView(newIndex);
            UpdateUI();
        }

        public ICommand RemoveLevelCommand { get { return new CommandHandler(() => this.RemoveLevel()); } }
        void RemoveLevel()
        {
            if (CurrentLevel == null)
                return;

            int index = _levelItems.IndexOf(CurrentLevel);
            if (index < 0)
                return;

            if (index < Data.Items.Count)
            {
                Data.Items.RemoveAt(index);

                int selectIndex = (index < Data.Items.Count) ? index : index - 1;

                LoadLevelItemsView(selectIndex);
                UpdateUI();
            }
        }

        WBSLevelView _currentLevel = null; 
        public WBSLevelView CurrentLevel
        {
            get => _currentLevel;
            set
            {
                if (SetProperty(ref _currentLevel, value))
                {
                    LoadAttributoFilterView();
                    UpdateUI();
                }
            }
        }

        public bool IsAnyCurrentLevel { get => CurrentLevel != null; }

        void LoadLevelItemsView(int newIndex)
        {
            List<WBSLevelView> levelItems = new List<WBSLevelView>();
            for (int i = 0; i < Data.Items.Count; i++)
            {
                WBSLevelView levelView = new WBSLevelView(this, Data.Items[i]);
                levelItems.Add(levelView);
            }

            LevelItems = new ObservableCollection<WBSLevelView>(levelItems);
            LevelItems.CollectionChanged += LevelItems_CollectionChanged;

            if (0<= newIndex && newIndex < LevelItems.Count)
                CurrentLevel = LevelItems[newIndex];

        }


        /// <summary>
        /// Per il drag and drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LevelItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                Data.Items.RemoveAt(e.OldStartingIndex);
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    WBSLevel newItem = (e.NewItems[0] as WBSLevelView).Data;
                    if (e.NewStartingIndex >= Data.Items.Count)
                    {
                        Data.Items.Add(newItem);
                        CurrentLevel = LevelItems[Data.Items.Count-1];
                    }
                    else if (e.NewStartingIndex >= 0)
                    {
                        Data.Items.Insert(e.NewStartingIndex, newItem);
                        CurrentLevel = LevelItems[e.NewStartingIndex];
                    }
                }
            }


               
        }

        internal void UpdateLevelItemsView()
        {


            foreach (var item in LevelItems)
                item.UpdateUI();
        }



        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsEntityTypeReadOnly));
            RaisePropertyChanged(GetPropertyName(() => IsAnyCurrentLevel));
            RaisePropertyChanged(GetPropertyName(() => IsValoriAuto));
            RaisePropertyChanged(GetPropertyName(() => IsAddAttivitaEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsTreeInLevel));
            RaisePropertyChanged(GetPropertyName(() => IsTreeInLevelEnabled));


        }

        internal string GetAttributoEtichetta(string codiceAttributo)
        {
            return _entityTypes[CurrentEntityTypeKey].Attributi[codiceAttributo].Etichetta;
        }

        public bool IsValoriAuto
        {
            get => CurrentLevel != null && CurrentLevel.IsValoriAuto;
            set
            {
                if (CurrentLevel != null)
                    CurrentLevel.IsValoriAuto = value;
            }
        }

        public bool IsTreeInLevel
        {
            get => CurrentLevel != null && CurrentLevel.IsTreeInLevel;
            set
            {
                if (CurrentLevel != null)
                    CurrentLevel.IsTreeInLevel = value;
            }
        }

        public bool IsTreeInLevelEnabled
        {
            get => CurrentLevel != null ? CurrentLevel.IsTreeInLevelEnabled : false;
        }


        internal Attributo GetSourceAttributo(string entityTypeKey, string codiceAttributo)
        {
            return _entitiesHelper.GetSourceAttributo(entityTypeKey, codiceAttributo);
        }


    }

    public class WBSLevelView : NotificationBase<WBSLevel>
    {
        CreateWBSItemsView _owner = null;

        public WBSLevelView(CreateWBSItemsView owner, WBSLevel lev = null) : base(lev)
        {
            _owner = owner;
        }

        internal WBSLevel Data { get => This; }

        public string Level
        {
            get
            {
                int index = _owner.LevelItems.IndexOf(this);
                return string.Format("{0}°", index.ToString());
            }
        }

        public AttributoItemView Attributo
        {
            get
            {
                string codiceAttributo = This.AttributoFilterData.CodiceAttributo;
                return _owner.AttributiItem.FirstOrDefault(item => item.Codice == codiceAttributo);
            }
            set
            {
                _owner.AttributoFilterView.CurrentAttributoItem = value;
            }
        }

        public bool IsValoriAuto
        {
            get => This.IsValoriAuto;
            set
            {
                if (SetProperty(This.IsValoriAuto, value, () => This.IsValoriAuto = value))
                {


                    _owner.UpdateLevelItemsView();
                    _owner.UpdateUI();

                }
            }
        }

        public bool IsTreeInLevel
        {
            get => This.IsTreeInLevel;
            set
            {
                if (SetProperty(This.IsTreeInLevel, value, () => This.IsTreeInLevel = value))
                {
                    _owner.UpdateLevelItemsView();
                    _owner.UpdateUI();

                }
            }
        }

        public bool IsTreeInLevelEnabled
        {
            get
            {
                var sourceAtt = _owner.EntitiesHelper.GetSourceAttributo(Data.AttributoFilterData.EntityTypeKey, Data.AttributoFilterData.CodiceAttributo);

                if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    var entType = _owner.DataService.GetEntityType(sourceAtt.GuidReferenceEntityTypeKey);
                    if (entType.IsTreeMaster)
                        return true;
                }

                IsTreeInLevel = false;
                return false;
            }
        }

        public string Valori
        {
            get
            {
                string str = string.Empty;
                if (!IsValoriAuto)
                {
                    Attributo sourceAtt = _owner.GetSourceAttributo(This.AttributoFilterData.EntityTypeKey, This.AttributoFilterData.CodiceAttributo);
                    if (sourceAtt == null)
                        return str;

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        IEnumerable<Guid> ids = This.AttributoFilterData.CheckedValori.Select(item => new Guid(item));
                        List<Entity> ents = _owner.DataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, ids);

                        str = string.Join(";", ents.Select(item => item.ToUserIdentity(UserIdentityMode.Deep)));
                    }
                    else
                    {
                        str = string.Join(";", This.AttributoFilterData.CheckedValori);
                    }
                }

                return str;
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Level));
            RaisePropertyChanged(GetPropertyName(() => Attributo));
            RaisePropertyChanged(GetPropertyName(() => Valori));
            RaisePropertyChanged(GetPropertyName(() => IsValoriAuto));
            RaisePropertyChanged(GetPropertyName(() => IsTreeInLevelEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsTreeInLevel));
            
        }

    }


}
