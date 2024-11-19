using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MasterDetailView
{
    public class FilterByEntityIdsView : NotificationBase<AttributoFilterData>
    {
        EntitiesListMasterDetailView _master;
        EntitiesListMasterDetailView Master { get => _master; }

        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public HashSet<Guid> EntityItemSelectedIds { get; set; } = new HashSet<Guid>();

        public event EventHandler Refresh;

        List<FilterDetailTreeEntityView> _valoriFilterTemp = new List<FilterDetailTreeEntityView>();
        Dictionary<Guid, TreeEntityMasterInfo> _dictTreeEntityMasterInfo = new Dictionary<Guid, TreeEntityMasterInfo>();
        Dictionary<Guid, FilterDetailTreeEntityView> _dictItemsView = new Dictionary<Guid, FilterDetailTreeEntityView>();

        Attributo Attributo { get; set; }
        internal Attributo SourceAttributo { get; set; }
        EntityType SourceEntityType { get; set; }

        int _valoriUnivociCount = 0;

        /// <summary>
        /// Id del master da filtrare (che provengono da filtri su altri attributi)
        /// </summary>
        List<Guid> _entitiesFound = new List<Guid>();

        bool _isBusy = false;


        public FilterByEntityIdsView(EntitiesListMasterDetailView master, AttributoFilterData data) : base(data)
        {
            _master = master;

            Attributo = Master.EntityType.Attributi[This.CodiceAttributo];

            EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
            SourceAttributo = entsHelper.GetSourceAttributo(Attributo);

            SourceEntityType = Master.DataService.GetEntityTypes()[SourceAttributo.GuidReferenceEntityTypeKey];
            //_nome = Attributo.Etichetta;

            //if (Attributo.ValoreDefault.ItemTextCount() > 0)
            //    _itemTextIndex = 0;
        }

        

        public bool IsModelToViewLoading { get; private set; }

        public void Init()
        {
            //LoadValoriFiltro(true, !Data.CheckedValori.Any());

            SetCodiciAttributi();
            LoadValoriFiltro(/*true, false*/);
        }

        AttributoFilterData Data { get => This; }


        /// <summary>
        /// Valori visualizzati nel tree
        /// </summary>
        ObservableCollection<FilterDetailTreeEntityView> _valoriFilter = new ObservableCollection<FilterDetailTreeEntityView>();

        private bool _isTreeViewVisible = true;
        public bool IsTreeViewVisible
        {
            get => _isTreeViewVisible;
            set => SetProperty(ref _isTreeViewVisible, value);
        }

        private int _treeViewMaxItemsCount = 500;
        
        public ObservableCollection<FilterDetailTreeEntityView> ValoriFilter
        {
            get
            {
                return _valoriFilter;
            }

            set
            {
                SetProperty(ref _valoriFilter, value);
            }
        }


        public void Load()
        {

        }

        //string _textSearched;
        public string TextSearched
        {
            get { return This.TextSearched; }
            set
            {
                if (SetProperty(This.TextSearched, value, () => This.TextSearched = value))
                {
                    This.CheckedValori.Clear();


                    _isBusy = true;
                    IsTreeViewVisible = false;
                    UpdateUI();

                    LoadValoriFiltroAsync(/*false*/);
                }

            }
        }

        public void LoadValoriFiltro(/*bool isTextSearchedChanged, bool checkAll*/)
        {
            _isBusy = true;
            IsTreeViewVisible = false;
            UpdateUI();



            if (Master.RightPanesView.FilterView.Data.Items.FirstOrDefault(item => item.CodiceAttributo == This.CodiceAttributo) != null)
            {
                Master.WindowService.ShowWaitCursor(true);

                //filtro
                FilterData filterData = Master.RightPanesView.FilterView.Data.Clone();
                filterData.Items.RemoveAll(item => item.CodiceAttributo == this.Attributo.Codice);

                List<Guid> entitiesFound = null;
                if (Master.EntityType.IsTreeMaster)
                    Master.DataService.GetFilteredTreeEntities(Master.EntityType.GetKey(), filterData, null, out entitiesFound);
                else
                    Master.DataService.GetFilteredEntities(Master.EntityType.GetKey(), filterData, null, null, out entitiesFound);

                _entitiesFound = entitiesFound;

                Master.WindowService.ShowWaitCursor(false);
            }
            else
            {
                _entitiesFound = Master.RightPanesView.FilterView.FoundEntitiesId;
            }

            

            LoadValoriFiltroAsync(/*checkAll*/);

        }


        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        public async void LoadValoriFiltroAsync(/*bool checkAll*/)
        {

            try
            {
                if (!Attributo.AllowValoriUnivoci)
                {
                    //IsAllChecked = true;
                    IsAllCheckedEnabled = false;
                    return;
                }

                //WindowService.ShowWaitCursor(true);

                //int itemTextIndex = this.ItemTextIndex;
                //if (itemTextIndex >= Attributo.ValoreDefault.ItemTextCount())
                //    itemTextIndex = -1;

                HashSet<Guid> idUnivoci = null;

                // If a cancellation token already exists (for a previous task),
                // cancel it.
                if (this._loadValoriFiltroCancellationTokenSource != null)
                    this._loadValoriFiltroCancellationTokenSource.Cancel();

                // Create a new cancellation token for the new task.
                this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;

                ////filtro
                //FilterData filterData = Master.RightPanesView.FilterView.Data.Clone();
                //filterData.Items.RemoveAll(item => item.CodiceAttributo == this.Attributo.Codice);


                //List<Guid> entitiesFound = null;
                //List<Guid> entitiesInfoId = null;
                //if (Master.EntityType.IsTreeMaster)
                //{
                //    List<TreeEntityMasterInfo> entitiesInfo = Master.DataService.GetFilteredTreeEntities(Master.EntityType.GetKey(), filterData, null, out entitiesFound);
                //    entitiesInfoId = entitiesInfo.Select(item => item.Id).ToList();
                //}
                //else
                //{
                //    List<EntityMasterInfo> entitiesInfo = Master.DataService.GetFilteredEntities(Master.EntityType.GetKey(), filterData, null, null, out entitiesFound);
                //    entitiesInfoId = entitiesInfo.Select(item => item.Id).ToList();
                //}

                Task<HashSet<Guid>> valoriUnivociTask = null;
                    
                valoriUnivociTask = GetValoriUnivociAsync(Master.EntityType.GetKey(), _entitiesFound, Attributo.Codice, TextSearched);
                idUnivoci = await valoriUnivociTask;

                if (cancellationToken.IsCancellationRequested)
                    return;

                IsModelToViewLoading = true;

                //_valoriUnivociFiltro.Clear();
                _valoriUnivociCount = idUnivoci.Count; 
                _valoriFilterTemp.Clear();
                _dictItemsView.Clear();
                _dictTreeEntityMasterInfo.Clear();

                if (SourceEntityType.IsTreeMaster)
                {

                    List<TreeEntityMasterInfo> allTreeEntsInfo = Master.DataService.GetFilteredTreeEntities(SourceAttributo.GuidReferenceEntityTypeKey, null, null, out _);

                    //entità filtrate con struttura
                    List<TreeEntityMasterInfo> filteredTreeEntsInfo = EntitiesHelper.TreeFilterById(allTreeEntsInfo, idUnivoci);

                    foreach (TreeEntityMasterInfo treeEntInfo in filteredTreeEntsInfo)
                    {

                        bool isChecked = false;

                        if (EntityItemSelectedIds.Contains(treeEntInfo.Id))
                        {
                            isChecked = true;
                        }

                        FilterDetailTreeEntityView itemView = new FilterDetailTreeEntityView(Master, this) { Id = treeEntInfo.Id };
                        if (_dictItemsView.ContainsKey(treeEntInfo.ParentId))
                        {
                            _dictItemsView[treeEntInfo.ParentId].SubItems.Add(itemView);
                        }
                        else
                        {
                            itemView.SetCheckByCode(isChecked);
                            _valoriFilterTemp.Add(itemView);
                        }

                        //show item checked (expand parents)
                        if (isChecked == true && treeEntInfo.ParentId != Guid.Empty)
                        {

                            TreeEntityMasterInfo parent = _dictTreeEntityMasterInfo[treeEntInfo.ParentId];
                            while (parent != null)
                            {
                                _dictItemsView[parent.Id].IsExpanded = true;

                                if (parent.ParentId != Guid.Empty)
                                    parent = _dictTreeEntityMasterInfo[parent.ParentId];
                                else
                                    parent = null;
                            }
                        }

                        if (isChecked == true)
                            itemView.SetCheckByCode(true);


                        _dictItemsView.Add(treeEntInfo.Id, itemView);
                        _dictTreeEntityMasterInfo.Add(treeEntInfo.Id, treeEntInfo);

                    }



                }
                else //not treeMaster
                {
                    foreach (Guid id in idUnivoci)
                    {
                        if (id == Guid.Empty)
                            continue;

                        var itemView = new FilterDetailTreeEntityView(Master, this) { Id = id };
                        
                        if (This.CheckedValori.Contains(id.ToString()))
                            itemView.SetCheckByCode(true);

                        _valoriFilterTemp.Add(itemView);
                        _dictItemsView.Add(id, itemView);
                        _dictTreeEntityMasterInfo.Add(id, new TreeEntityMasterInfo() { Id = id, ParentId = Guid.Empty });
                    }
                    


                }

                //Inserisco [Nessuno] in testa
                if (idUnivoci.Contains(Guid.Empty))
                {
                    var itemView = new FilterDetailTreeEntityView(Master, this) { Id = Guid.Empty };
                        
                    if (This.CheckedValori.Contains(Guid.Empty.ToString()))
                        itemView.SetCheckByCode(true);



                    _valoriFilterTemp.Insert(0, itemView);
                    _dictItemsView.Add(Guid.Empty, itemView);
                    _dictTreeEntityMasterInfo.Add(Guid.Empty, new TreeEntityMasterInfo() { Id = Guid.Empty, ParentId = Guid.Empty });
                }


                ////seleziono la prima foglia dell'albero in modo che aggiorni il detail
                //if (idUnivoci.Any() && TextSearched != null && TextSearched.Any())
                //    SelectedItem = _dictItemsView[idUnivoci.First()].IsSelected = true;
                //else
                //    SelectedItem = null;

                SelectedItem = null;


                //RaisePropertyChanged(GetPropertyName(() => this.IsAllChecked));
                RaisePropertyChanged(GetPropertyName(() => this.ValoriFiltroCount));

                this._loadValoriFiltroCancellationTokenSource = null;

                IsModelToViewLoading = false;


                
                if (_valoriFilterTemp.Count < _treeViewMaxItemsCount)
                {
                    IsTreeViewVisible = true;
                    ValoriFilter = new ObservableCollection<FilterDetailTreeEntityView>(_valoriFilterTemp);
                    UpdateUI();
                }
                else
                {
                    _isBusy = false;
                    IsTreeViewVisible = false;
                    ValoriFilter = new ObservableCollection<FilterDetailTreeEntityView>();
                    UpdateUI();
                }
                
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
            //WindowService.ShowWaitCursor(false);
            

        }

        public string TreeViewPlaceholderText
        {
            get
            {
                string str = string.Empty;
                if (_isBusy)
                {
                    str = LocalizationProvider.GetString("CaricamentoInCorso");
                }
                else
                { 
                    str = string.Format("{0} > {1}. {2}.", LocalizationProvider.GetString("Risultati"), _treeViewMaxItemsCount, LocalizationProvider.GetString("AffinareLaRicerca"));
                }
                return str;
            }
        }

        bool _isAllChechedEnabled = true;
        public bool IsAllCheckedEnabled
        {
            get => _isAllChechedEnabled;
            set { SetProperty(ref _isAllChechedEnabled, value); }
        }

        public string IsAllCheckedText
        {
            get => string.Format("{0} ({1})", LocalizationProvider.GetString("SelezionaTutti"), _valoriUnivociCount);
        }

        public string IsAllCheckedBackground
        {
            get
            {
                //if (IsAnyChildChecked())
                //    return ColorConverter.ColorsEnum.LightGray.ToString();

                //return ColorConverter.ColorsEnum.Transparent.ToString();

                if (IsAnyChildChecked())
                    return Colors.LightGray.ToString();

                return Colors.Transparent.ToString();
            }
        }

        bool IsAnyChildChecked()
        {
            foreach (var item in _valoriFilterTemp)
            {
                if (item.IsChecked || item.IsAnyChildChecked())
                    return true;
            }
            return false;
        }

        public bool IsAllChecked
        {
            get
            {
                if (_valoriFilterTemp.Any() && _valoriFilterTemp.FirstOrDefault(item => !item.IsChecked) == null)
                    return true;

                return false;
            }
            set
            {
                foreach (var item in _valoriFilterTemp)
                    item.IsChecked = value;
            }
        }

        public string ValoriFiltroCount
        {
            get
            {
                string sValoriUnivociCount = _valoriUnivociCount.ToString();

                string sValoriUnivociChecked = This.CheckedValori.Count().ToString();
                //if (IsAllChecked == true)
                //    sValoriUnivociChecked = _valoriUnivociCount.ToString();


                return "(" + sValoriUnivociChecked + "/" + sValoriUnivociCount + ")";
            }
        }

        public async Task<HashSet<Guid>> GetValoriUnivociAsync(string entityTypeKey, List<Guid> entities, string codiceAttributo, string textSearched)
        {
            List<string> list = await Master.DataService.GetValoriUnivociAsync(entityTypeKey, entities, codiceAttributo, -1, textSearched);
            HashSet<Guid> vals = new HashSet<Guid>(list.Select(item => new Guid(item)));
            return vals;
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => this.ValoriFiltroCount));
            RaisePropertyChanged(GetPropertyName(() => this.CurrentEntityAttributo0));
            RaisePropertyChanged(GetPropertyName(() => this.CurrentEntityAttributo1));
            RaisePropertyChanged(GetPropertyName(() => this.IsAllCheckedBackground));
            RaisePropertyChanged(GetPropertyName(() => this.IsAllChecked));
            RaisePropertyChanged(GetPropertyName(() => this.IsAllCheckedText));
            RaisePropertyChanged(GetPropertyName(() => this.TreeViewPlaceholderText));
            RaisePropertyChanged(GetPropertyName(() => this.EtichettaAttributo0));
            RaisePropertyChanged(GetPropertyName(() => this.EtichettaAttributo1));

        }



        #region Detail Codice e DescrizioneRtf

        protected virtual void OnRefresh(EventArgs e)
        {
            Refresh?.Invoke(this, e);
        }

        ValoreTesto CurrentEntityTesto { get; set; } = null;
        ValoreTestoRtf CurrentEntityTestoRtf { get; set; } = null;
        
        object _selectedItem = null;
        public object SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                
                
                if (SetProperty(ref _selectedItem, value))
                {

                    Guid entId = Guid.Empty;
                    if (_selectedItem != null)
                        entId = (_selectedItem as FilterDetailTreeEntityView).Id;

                    UpdateCurrentEntityDetail(entId);
                    //OnRefresh(new EventArgs());
                }
            }
        }

        void UpdateCurrentEntityDetail(Guid entId)
        {
            if (string.IsNullOrEmpty(CodiceAttributo0))
                return;

            if (string.IsNullOrEmpty(CodiceAttributo1))
                return;


            if (entId != Guid.Empty)
            {
                Entity ent = Master.DataService.GetEntityById(SourceAttributo.GuidReferenceEntityTypeKey, entId);

                EntitiesHelper e = new EntitiesHelper(Master.DataService);

                CurrentEntityTesto = e.GetValoreAttributo(ent, CodiceAttributo0, true, false) as ValoreTesto;
                CurrentEntityTestoRtf = e.GetValoreAttributo(ent, CodiceAttributo1, true, false) as ValoreTestoRtf;
            }
            else
            {
                CurrentEntityTestoRtf = null;
                CurrentEntityTesto = null;
            }


            UpdateUI();

        }

        void SetCodiciAttributi()
        {

            //EntityType entType = Master.DataService.GetEntityTypes()[SourceAttributo.GuidReferenceEntityTypeKey];
            if (SourceEntityType is TreeEntityType)
            { 
                TreeEntityType treeEntType = SourceEntityType as TreeEntityType;

                List<string> codiciAtt = treeEntType.GetParentAttributi();
                CodiceAttributo0 = codiciAtt[0];
                CodiceAttributo1 = codiciAtt[1];

                EtichettaAttributo0 = treeEntType.Attributi[CodiceAttributo0].Etichetta;
                EtichettaAttributo1 = treeEntType.Attributi[CodiceAttributo1].Etichetta;
            }
        }
        
        string CodiceAttributo0 { get; set; }
        string CodiceAttributo1 { get; set; }

        public string EtichettaAttributo0 { get; set; }
        public string EtichettaAttributo1 { get; set; }

        public string CurrentEntityAttributo0
        {
            get
            {
                if (CurrentEntityTesto == null)
                    return null;

                return CurrentEntityTesto.V;
            }
        }

    

        public string CurrentEntityAttributo1
        {
            get
            {
                if (CurrentEntityTestoRtf == null)
                    return null;

                string rtf = CurrentEntityTestoRtf.V;
                if (TextSearched != null && TextSearched.Any())
                    ValoreHelper.HighlightText(ref rtf, TextSearched);

                return rtf;
            }
        }

        internal void CheckItem(Guid id, bool isChecked)
        {
            if (!_dictItemsView[id].HasChidren)
            {
                if (isChecked)
                    EntityItemSelectedIds.Add(id);
                else
                    EntityItemSelectedIds.Remove(id);
            }



            //Aggiorno il check dal livello più profondo a quello più superficiale
            TreeEntityMasterInfo itemInfo = _dictTreeEntityMasterInfo[id];
            TreeEntityMasterInfo parent = itemInfo.ParentId == Guid.Empty ? null : _dictTreeEntityMasterInfo[itemInfo.ParentId];
            while (parent != null)
            {
                _dictItemsView[parent.Id].UpdateUI();
                parent = parent.ParentId == Guid.Empty ? null : _dictTreeEntityMasterInfo[parent.ParentId];
            }

            UpdateUI();


        }

        #endregion
    }

    public class FilterDetailTreeEntityView : NotificationBase
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ObservableCollection<FilterDetailTreeEntityView> SubItems { get; set; } = new ObservableCollection<FilterDetailTreeEntityView>();

        FilterByEntityIdsView _owner = null;
        public FilterDetailTreeEntityView(EntitiesListMasterDetailView master, FilterByEntityIdsView owner)
        {
            _master = master;
            _owner = owner;
        }

        public string Text
        {
            get
            {
                //if (Id == Guid.Empty)
                //    return LocalizationProvider.GetString("_Nessuno");

                if (Id == Guid.Empty)
                    return ValoreHelper.ValoreNullAsText;

                Entity ent = Master.DataService.GetEntityById(_owner.SourceAttributo.GuidReferenceEntityTypeKey, Id);
                string str = ent.ToUserIdentity(UserIdentityMode.Nothing);
                return str;
            }
        }

        public string[] TextArray
        {
            get
            {
                if (_owner.TextSearched != null && _owner.TextSearched.Any())
                {
                    string textSearched = _owner.TextSearched.Trim();

                    string[] textSearchedArray = null;
                    if (textSearched.StartsWith("\"") && textSearched.EndsWith("\""))
                        textSearchedArray = new string[] { textSearched.Substring(1, textSearched.Length - 2) };
                    else
                        textSearchedArray = _owner.TextSearched.Trim().Split(' ');

                    string aaa = string.Join("|", textSearchedArray);
                    string pattern = string.Format("({0})", aaa);

                    string[] substrings = Regex.Split(Text, pattern, RegexOptions.IgnoreCase);    // Split on hyphens
                    return substrings;
                }
                return new string[5] { Text, string.Empty, string.Empty, string.Empty, string.Empty };
            }
        }


        Guid _id;
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        bool _isChecked = false;
        public bool IsChecked
        {
            get
            {
                if (HasChidren)
                {
                    if (IsAllChildChecked())
                        return true;
                    return false;
                }
                else
                {
                    return _isChecked;
                }
            }
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    _owner.CheckItem(Id, _isChecked);
                }

                if (HasChidren)
                {
                    if (_isChecked)
                    {
                        foreach (var item in SubItems)
                            item.IsChecked = true;
                    }
                    else
                    {
                        foreach (var item in SubItems)
                            item.IsChecked = false;
                    }
                }
            }
        }

        //bool _isCheckEnabled = true;
        //public bool IsCheckEnabled
        //{
        //    get { return _isCheckEnabled; }
        //    set { SetProperty(ref _isCheckEnabled, value); }
        //}

        public bool HasChidren { get => SubItems.Any(); }

        internal void SetCheckByCode(bool isChecked)
        {
            _isChecked = isChecked;
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                SetProperty(ref _isExpanded, value);
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        public string Background
        {
            get
            {
                if (IsAnyChildChecked())
                    return "LightGray";

                 return "Transparent";
            }
        }

        public bool IsAnyChildChecked()
        {
            if (SubItems.FirstOrDefault(item => item.IsChecked || item.IsAnyChildChecked()) != null)
                return true;

            return false;
        }

        public bool IsAllChildChecked()
        {
            if (SubItems.FirstOrDefault(item => !item.IsChecked) == null)
                return true;

            return false;
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Background));
            RaisePropertyChanged(GetPropertyName(() => IsChecked));
            RaisePropertyChanged(GetPropertyName(() => IsExpanded));

        }
    }
}

