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
using System.Windows.Media;

namespace MasterDetailView
{
    public class SetAttributoFilterView : NotificationBase
    {
        //I/O 
        public HashSet<string> EntityTypesKey { get; set; } = new HashSet<string>();
        public List<Guid> EntsIdToFilter { get; set; } = new List<Guid>();
        public AttributoFilterData AttributoFilterData { get; set; } = null;
        //
        public IDataService DataService { get; set; } = null;
        Dictionary<string, EntityType> _allEntityTypes = null;
        List<ValoreItemView> _valoriItemTemp = new List<ValoreItemView>();
        Dictionary<Guid, TreeEntityMasterInfo> _dictTreeEntityMasterInfo = new Dictionary<Guid, TreeEntityMasterInfo>();
        Dictionary<Guid, ValoreItemView> _dictItemsViewById = new Dictionary<Guid, ValoreItemView>();

        private string Nessuno { get; set; } = string.Empty;
        public bool IsModelToViewLoading { get; private set; }
        int _valoriUnivociCount = 0;
        private int _treeViewMaxItemsCount = 500;
        bool _isBusy = false;

        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public HashSet<Guid> EntityItemSelectedIds { get; set; } = new HashSet<Guid>();

        public event EventHandler AttributoChanged = null;
        public event EventHandler ValoriCheckedChanged = null;

        public ValoreConditionsGroupView ValoreConditionsGroupView { get; set; } = null;




        //EntityType (key: Name)
        Dictionary<string, EntityType> _entityTypes = null;
        ObservableCollection<string> _entityTypesItem = null;
        public ObservableCollection<string> EntityTypesItem
        {
            get => _entityTypesItem;
            protected set => _entityTypesItem = value;
        }



        //Attributi (Key: etichetta)
        Dictionary<string, Attributo> _attributi = null;
        ObservableCollection<AttributoItemView> _attributiItem = null;
        public ObservableCollection<AttributoItemView> AttributiItem
        {
            get => _attributiItem;
            protected set => _attributiItem = value;
        }

        //Valore (Key: Result o V)
        ObservableCollection<ValoreItemView> _valoriItem = null;
        public ObservableCollection<ValoreItemView> ValoriItem
        {
            get => _valoriItem;
            protected set => SetProperty(ref _valoriItem, value);
        }

        private EntitiesHelper EntitiesHelper = null;
        bool _isLoading = false;


        Attributo CurrentAttributo
        {
            get
            {
                if (_currentAttributoItem == null)
                    return null;

                if (_attributi == null)
                    return null;

                Attributo currentAtt = null;
                if (_attributi.TryGetValue(_currentAttributoItem.Name, out currentAtt))
                    return currentAtt;
                return null;
            }
        }

        EntityType CurrentEntityType
        {
            get
            {
                EntityType currentEntType = null;
                if (_entityTypes.TryGetValue(_currentEntityTypeItem, out currentEntType))
                    return currentEntType;

                return null;
            }
        }

        public void Load()
        {
            _isLoading = true;

            EntitiesHelper = new EntitiesHelper(DataService);
            Nessuno = LocalizationProvider.GetString("Nessuno");

            _allEntityTypes = DataService.GetEntityTypes();
            _entityTypes = _allEntityTypes.Values.Where(item => EntityTypesKey.Contains(item.GetKey())).ToDictionary(item => item.Name, item => item);
            
            if (_entityTypes.Any())
            {
                _entityTypesItem = new ObservableCollection<string>(_entityTypes.Values.Select(item => item.Name));
                _entityTypesItem.Insert(0, Nessuno);
            }
            else
            {
                _entityTypesItem = new ObservableCollection<string>() { Nessuno };
            }


            //set current entityType
            string currentEntityTypeItem = _entityTypesItem[0];
            if (AttributoFilterData != null)
            {
                EntityType entType = _entityTypes.Values.FirstOrDefault(item => item.Codice == AttributoFilterData.EntityTypeKey);
                if (entType != null)
                    currentEntityTypeItem = entType.Name;
            }
            else
            {
                AttributoFilterData = new AttributoFilterData();

                if (_entityTypes.Count == 1)
                {
                    EntityType entType = _entityTypes.Values.FirstOrDefault();
                    if (entType != null)
                        currentEntityTypeItem = entType.Name;
                }
            }

            CurrentEntityTypeItem = currentEntityTypeItem;


            _isLoading = false;
            UpdateUI();
        }

        public bool Accept()
        {
            if (AttributoFilterData.FilterType == FilterTypeEnum.Conditions)
            {
                //save conditions
                ValoreConditionsGroupView.UpdateData();
                AttributoFilterData.ValoreConditions = ValoreConditionsGroupView.Data;
                AttributoFilterData.IsFiltroAttivato = true;
            }
            else
            {
                //gli altri tab sono già salvati al momento del check
            }
            return true;
        }

        private string _currentEntityTypeItem = null;
        public string CurrentEntityTypeItem
        {
            get => _currentEntityTypeItem;
            set
            {
                if (SetProperty(ref _currentEntityTypeItem, value))
                {

                    if (_currentEntityTypeItem == Nessuno)
                    {
                        AttributoFilterData.EntityTypeKey = string.Empty;
                        _attributiItem = new ObservableCollection<AttributoItemView>() { AttributoItemView.Empty };
                        CurrentAttributoItem = _attributiItem[0];
                    }
                    else if (_entityTypes.ContainsKey(_currentEntityTypeItem))
                    {
                        EntityType currentEntType = _entityTypes[_currentEntityTypeItem];
                        AttributoFilterData.EntityTypeKey = currentEntType.GetKey();

                        IEnumerable<Attributo> atts = currentEntType.Attributi.Values.Where(item =>
                        {
                            if (item.IsInternal)
                                return false;

                            if (!item.IsVisible)
                                return false;

                            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(item);
                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                            {
                                EntityType entType = null;
                                if (!_allEntityTypes.TryGetValue(sourceAtt.GuidReferenceEntityTypeKey, out entType))
                                    return false;

                                if (!entType.IsTreeMaster)
                                    return false;

                            }

                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                                return false;

                            //if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                            //    return false;

                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                                return false;

                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                                return false;

                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                                return false;

                            return true;
                        }
                        );

                        _attributi = atts.ToDictionary(item =>
                        {
                            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(item);
                            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                                return string.Format("[{0}]", item.Etichetta);
                            else
                                return item.Etichetta;
                        });


                        ObservableCollection <AttributoItemView>  attItems = new ObservableCollection<AttributoItemView>();
                        foreach (KeyValuePair<string, Attributo> keyValue in _attributi.OrderBy(item => item.Value.DetailViewOrder))
                        {
                            AttributoItemView attItem = new AttributoItemView();
                            Attributo att = keyValue.Value;

                            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(att);
                            attItem.IsGuid = sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection;

                            attItem.Name = keyValue.Key;
                            attItem.Codice = att.Codice;

                            attItems.Add(attItem);
                        }


                        if (_attributi.Any())
                            _attributiItem = attItems;
                        else
                            _attributiItem = new ObservableCollection<AttributoItemView>() { AttributoItemView.Empty };

                        AttributoItemView currentAttributoItem = _attributiItem[0];
                        if (AttributoFilterData != null)
                        {
                            AttributoItemView attItem = _attributiItem.FirstOrDefault(item => item.Codice == AttributoFilterData.CodiceAttributo);
                            if (attItem != null)
                                currentAttributoItem = attItem;
                        }

                        CurrentAttributoItem = currentAttributoItem;

                    }
                    UpdateUI();
                }
            }
        }

        private AttributoItemView _currentAttributoItem = null;
        public AttributoItemView CurrentAttributoItem
        {
            get => _currentAttributoItem;
            set
            {
                if (_currentAttributoItem != null)
                    AttributoFilterData.CheckedValori.Clear();

                if (SetProperty(ref _currentAttributoItem, value))
                {
                    
                    TextSearched = string.Empty;

                    if (_currentAttributoItem.IsEmpty())
                    {
                        AttributoFilterData.CodiceAttributo = string.Empty;
                        ValoriItem = new ObservableCollection<ValoreItemView>() { };
                        IsAllChecked = false;
                        TextSearched = string.Empty;
                    }
                    else if (_attributi.ContainsKey(_currentAttributoItem.Name))
                    {
                        Attributo att = _attributi[_currentAttributoItem.Name];
                        AttributoFilterData.CodiceAttributo = att.Codice;
                        //if (_currentAttributoItem.IsGuid)
                        //{
                        //    LoadValoriGuidAsync(/*!_isLoading || */AttributoFilterData.IsAllChecked == true);
                        //}
                        //else
                        //{
                        //LoadValoriAsync(/*!_isLoading || */AttributoFilterData.IsAllChecked == true);
                        //}
                        LoadFilterTypes();
                    }

                }
            }
        }

        //public async void LoadValoriAsync_old(bool checkAll)
        //{
        //    if (CurrentEntityType == null)
        //        return;

        //    if (CurrentAttributo == null)
        //        return;


        //    List<Guid> entitiesId = EntsIdToFilter;
        //    if (entitiesId == null)
        //        DataService.GetFilteredEntities(CurrentEntityType.GetKey(), null, null, null, out entitiesId);

        //    List<string> valoriUnivoci = null;
        //    valoriUnivoci = await GetValoriUnivociAsync(CurrentEntityType.GetKey(), entitiesId, CurrentAttributo.Codice, TextSearched);




        //    if (valoriUnivoci != null)
        //    {  
        //        //if (valoriUnivoci.Count > 200)
        //        //    valoriUnivoci = new List<string>(valoriUnivoci.Take(200));
                
                
        //        if (valoriUnivoci.Any())
        //            ValoriItem = new ObservableCollection<ValoreItemView>(valoriUnivoci.OrderBy(item => item).Select(item =>
        //            {
        //                ValoreItemView itemView = new ValoreItemView(this) { Text = item };
        //                itemView.SetCheck(AttributoFilterData.CheckedValori.Contains(item));
        //                return itemView;
        //            }));
                
        //        else
        //            ValoriItem = new ObservableCollection<ValoreItemView>();

        //        if (ValoriItem.Any())
        //        {
        //            if (checkAll)
        //                IsAllChecked = true;
        //            //else
        //            //    UpdateIsAllChecked();
        //        }
        //        else
        //        {
        //            IsAllChecked = false;
        //        }

        //        UpdateUI();
        //        OnAttributoChanged(new EventArgs());

        //    }
        //}

        public async void LoadValoriAsync(bool checkAll)
        {
            if (CurrentEntityType == null)
                return;

            if (CurrentAttributo == null)
                return;

            _isBusy = true;
            UpdateUI();
            IsTreeViewVisible = false;

            List<Guid> entitiesId = EntsIdToFilter;
            if (entitiesId == null)
                DataService.GetFilteredEntities(CurrentEntityType.GetKey(), null, null, null, out entitiesId);


            // If a cancellation token already exists (for a previous task),
            // cancel it.
            if (this._loadValoriFiltroCancellationTokenSource != null)
                this._loadValoriFiltroCancellationTokenSource.Cancel();

            // Create a new cancellation token for the new task.
            this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;


            List<string> valoriUnivoci = null;
            valoriUnivoci = await GetValoriUnivociAsync(CurrentEntityType.GetKey(), entitiesId, CurrentAttributo.Codice, TextSearched);

            if (cancellationToken.IsCancellationRequested)
                return;

            IsModelToViewLoading = true;
            _valoriUnivociCount = valoriUnivoci.Count;

            if (valoriUnivoci == null)
                return;

            _valoriItemTemp.Clear();

            //if (valoriUnivoci.Count > 200)
            //    valoriUnivoci = new List<string>(valoriUnivoci.Take(200));


            if (valoriUnivoci.Any())
                _valoriItemTemp = new List<ValoreItemView>(valoriUnivoci.OrderBy(item => item).Select(item =>
                {
                    ValoreItemView itemView = new ValoreItemView(this) { Text = item };
                    itemView.SetCheck(AttributoFilterData.CheckedValori.Contains(item));
                    return itemView;
                }));

            else
                _valoriItemTemp = new List<ValoreItemView>();

            if (_valoriItemTemp.Any())
            {
                if (checkAll)
                    IsAllChecked = true;
            }
            else
            {
                IsAllChecked = false;
            }

            SelectedItem = null;
            RaisePropertyChanged(GetPropertyName(() => ValoriFiltroCount));

            this._loadValoriFiltroCancellationTokenSource = null;
            IsModelToViewLoading = false;

            if (_valoriItemTemp.Count < _treeViewMaxItemsCount)
            {
                IsTreeViewVisible = true;
                ValoriItem = new ObservableCollection<ValoreItemView>(_valoriItemTemp);
                OnAttributoChanged(new EventArgs());
                UpdateUI();
            }
            else
            {
                _isBusy = false;
                IsTreeViewVisible = false;
                ValoriItem = new ObservableCollection<ValoreItemView>();
                OnAttributoChanged(new EventArgs());
                UpdateUI();
            }

            UpdateUI();
            OnAttributoChanged(new EventArgs());

        }

        public async Task<List<string>> GetValoriUnivociAsync(string entityTypeKey, List<Guid> entities, string codiceAttributo, string textSearched)
        {
            List<string> list = await DataService.GetValoriUnivociAsync(entityTypeKey, entities, codiceAttributo, -1, textSearched);
            return list;
        }

        public async Task<HashSet<Guid>> GetValoriUnivociGuidAsync(string entityTypeKey, List<Guid> entities, string codiceAttributo, string textSearched)
        {
            List<string> list = await DataService.GetValoriUnivociAsync(entityTypeKey, entities, codiceAttributo, -1, textSearched);
            HashSet<Guid> vals = new HashSet<Guid>(list.Select(item => new Guid(item)));
            return vals;
        }

        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        public async void LoadValoriGuidAsync(bool checkAll)
        {

            try
            {
                if (!CurrentAttributo.AllowValoriUnivoci)
                {
                    //IsAllCheckedEnabled = false;
                    return;
                }

                //_isAllChecked = false;

                HashSet<Guid> idUnivoci = null;

                _isBusy = true;
                IsTreeViewVisible = false;

                List<Guid> entitiesId = EntsIdToFilter;
                if (entitiesId == null)
                    DataService.GetFilteredEntities(CurrentEntityType.GetKey(), null, null, null, out entitiesId);

                // If a cancellation token already exists (for a previous task),
                // cancel it.
                if (this._loadValoriFiltroCancellationTokenSource != null)
                    this._loadValoriFiltroCancellationTokenSource.Cancel();

                // Create a new cancellation token for the new task.
                this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;

                Task<HashSet<Guid>> valoriUnivociTask = null;

                valoriUnivociTask = GetValoriUnivociGuidAsync(CurrentEntityType.GetKey(), entitiesId, CurrentAttributo.Codice, TextSearched);
                idUnivoci = await valoriUnivociTask;

                if (cancellationToken.IsCancellationRequested)
                    return;

                IsModelToViewLoading = true;
                _valoriUnivociCount = idUnivoci.Count;

                Attributo sourceAtt = GetCurrentSourceAttributo();

                EntityType guidReferenceEntType = DataService.GetEntityType(sourceAtt.GuidReferenceEntityTypeKey);
                if (guidReferenceEntType.IsTreeMaster)
                {

                    List<TreeEntityMasterInfo> allTreeEntsInfo = DataService.GetFilteredTreeEntities(sourceAtt.GuidReferenceEntityTypeKey, null, null, out _);

                    //entità filtrate con struttura
                    List<TreeEntityMasterInfo> filteredTreeEntsInfo = EntitiesHelper.TreeFilterById(allTreeEntsInfo, idUnivoci);

                    _valoriItemTemp.Clear();
                    _dictItemsViewById.Clear();
                    _dictTreeEntityMasterInfo.Clear();




                    foreach (TreeEntityMasterInfo treeEntInfo in filteredTreeEntsInfo)
                    {

                        bool isChecked = false;

                        if (checkAll || AttributoFilterData.CheckedValori.Contains(treeEntInfo.Id.ToString()))
                        {
                            isChecked = true;
                        }

                        ValoreItemView itemView = new ValoreItemView(this) { Id = treeEntInfo.Id };
                        if (_dictItemsViewById.ContainsKey(treeEntInfo.ParentId))
                        {
                            _dictItemsViewById[treeEntInfo.ParentId].SubItems.Add(itemView);
                        }
                        else
                        {
                            itemView.SetCheckByCode(isChecked);
                            _valoriItemTemp.Add(itemView);
                        }

                        //show item checked (expand parents)
                        if (isChecked == true && treeEntInfo.ParentId != Guid.Empty)
                        {

                            TreeEntityMasterInfo parent = _dictTreeEntityMasterInfo[treeEntInfo.ParentId];
                            while (parent != null)
                            {
                                _dictItemsViewById[parent.Id].IsExpanded = true;

                                if (parent.ParentId != Guid.Empty)
                                    parent = _dictTreeEntityMasterInfo[parent.ParentId];
                                else
                                    parent = null;
                            }
                        }

                        if (isChecked == true)
                            itemView.SetCheckByCode(true);


                        _dictItemsViewById.Add(treeEntInfo.Id, itemView);
                        _dictTreeEntityMasterInfo.Add(treeEntInfo.Id, treeEntInfo);

                    }

                }
                else
                {

                    _valoriItemTemp.Clear();
                    _dictItemsViewById.Clear();
                    _dictTreeEntityMasterInfo.Clear();

                    foreach (Guid id in idUnivoci)
                    {
                        bool isChecked = false;

                        if (checkAll || AttributoFilterData.CheckedValori.Contains(id.ToString()))
                        {
                            isChecked = true;
                        }

                        ValoreItemView itemView = new ValoreItemView(this) { Id = id };

                        itemView.SetCheckByCode(isChecked);
                        _valoriItemTemp.Add(itemView);

                        _dictItemsViewById.Add(id, itemView);
                        _dictTreeEntityMasterInfo.Add(id, new TreeEntityMasterInfo() { Id = id, ParentId = Guid.Empty });

                    }
                }


                //Inserisco [Nessuno] in testa
                if (idUnivoci.Contains(Guid.Empty))
                {
                    var itemView = new ValoreItemView(this) { Id = Guid.Empty };

                    if (AttributoFilterData.CheckedValori.Contains(Guid.Empty.ToString()))
                        itemView.SetCheckByCode(true);



                    _valoriItemTemp.Insert(0, itemView);
                    _dictItemsViewById.Add(Guid.Empty, itemView);
                    _dictTreeEntityMasterInfo.Add(Guid.Empty, new TreeEntityMasterInfo() { Id = Guid.Empty, ParentId = Guid.Empty });
                }

                SelectedItem = null;
                RaisePropertyChanged(GetPropertyName(() => ValoriFiltroCount));

                this._loadValoriFiltroCancellationTokenSource = null;

                IsModelToViewLoading = false;



                if (_valoriItemTemp.Count < _treeViewMaxItemsCount)
                {
                    IsTreeViewVisible = true;
                    ValoriItem = new ObservableCollection<ValoreItemView>(_valoriItemTemp);
                    UpdateUI();
                    OnAttributoChanged(new EventArgs());
                }
                else
                {
                    _isBusy = false;
                    IsTreeViewVisible = false;
                    ValoriItem = new ObservableCollection<ValoreItemView>();
                    OnAttributoChanged(new EventArgs());
                    UpdateUI();
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

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

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => EntityTypesItem));
            RaisePropertyChanged(GetPropertyName(() => CurrentEntityTypeItem));
            RaisePropertyChanged(GetPropertyName(() => AttributiItem));
            RaisePropertyChanged(GetPropertyName(() => CurrentAttributoItem));
            RaisePropertyChanged(GetPropertyName(() => ValoriItem));
            RaisePropertyChanged(GetPropertyName(() => IsPlaceholderVisible));
            RaisePropertyChanged(GetPropertyName(() => IsAllChecked));
            //RaisePropertyChanged(GetPropertyName(() => CheckedValori));
            RaisePropertyChanged(GetPropertyName(() => IsCurrentAttributoSourceGuid));
            RaisePropertyChanged(GetPropertyName(() => IsAllCheckedBackground));
            RaisePropertyChanged(GetPropertyName(() => IsTreeViewVisible));
            RaisePropertyChanged(GetPropertyName(() => TreeViewPlaceholderText));
            RaisePropertyChanged(GetPropertyName(() => CurrentFilterType));
            RaisePropertyChanged(GetPropertyName(() => IsConditionsFilterType));

            if (ValoriItem != null)
            {
                foreach (var item in ValoriItem)
                    item.UpdateUI();
            }
            
        }

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

                    //UpdateCurrentEntityDetail(entId);
                }
            }
        }

        public string ValoriFiltroCount
        {
            get
            {
                string sValoriUnivociCount = _valoriUnivociCount.ToString();

                string sValoriUnivociChecked = AttributoFilterData.CheckedValori.Count().ToString();
                //if (IsAllChecked == true)
                //    sValoriUnivociChecked = _valoriUnivociCount.ToString();


                return "(" + sValoriUnivociChecked + "/" + sValoriUnivociCount + ")";
            }
        }

        string _textSearched = string.Empty;
        public string TextSearched
        {
            get => _textSearched;
            set
            {
                if (SetProperty(ref _textSearched, value))
                {


                    if (CurrentAttributoItem.IsGuid)
                        LoadValoriGuidAsync(false);
                    else
                        LoadValoriAsync(false);
                }
                UpdateUI();
            }
        }

        public bool IsPlaceholderVisible { get => _textSearched.Any(); }

        //bool? _isAllChecked = false;
        public bool IsAllChecked
        {
            get
            {
                if (IsCurrentAttributoSourceGuid)
                {
                    if (_valoriItemTemp.Any() && _valoriItemTemp.FirstOrDefault(item => !item.IsChecked) == null)
                        return true;
                }
                else
                {
                    if (_valoriItem != null && _valoriItem.Any() && _valoriItem.FirstOrDefault(item => !item.IsChecked) == null)
                        return true;
                }
                return false;
            }
            set
            {
                
                if (IsCurrentAttributoSourceGuid)
                {
                    foreach (var item in _valoriItemTemp)
                    {
                        item.SetCheck(value);
                        item.SetChildrenCheck(value);
                    }
                }
                else
                {
                    if (ValoriItem != null)
                    {
                        foreach (var item in ValoriItem)
                            item.SetCheck(value);
                    }
                }

                UpdateUI();
                OnValoriCheckedChanged(new EventArgs());

            }
        }


        public bool IsCurrentAttributoSourceGuid
        {
            get
            {
                if (CurrentAttributo != null)
                {
                    Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(CurrentAttributo);
                    return sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid;
                }

                return false;
            }
        }

        public Attributo GetCurrentSourceAttributo()
        {
            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(CurrentAttributo);
            return sourceAtt;
        }

        private bool _isTreeViewVisible = true;
        public bool IsTreeViewVisible
        {
            get => _isTreeViewVisible;
            set => SetProperty(ref _isTreeViewVisible, value);
        }

        internal void CheckItem(Guid id, bool isChecked)
        {
            if (!_dictItemsViewById[id].HasChidren)
            {
                if (isChecked)
                    AttributoFilterData.CheckedValori.Add(id.ToString());
                else
                    AttributoFilterData.CheckedValori.Remove(id.ToString());
            }



            //Aggiorno il check dal livello più profondo a quello più superficiale
            TreeEntityMasterInfo itemInfo = _dictTreeEntityMasterInfo[id];
            TreeEntityMasterInfo parent = itemInfo.ParentId == Guid.Empty ? null : _dictTreeEntityMasterInfo[itemInfo.ParentId];
            while (parent != null)
            {
                _dictItemsViewById[parent.Id].UpdateUI();
                parent = parent.ParentId == Guid.Empty ? null : _dictTreeEntityMasterInfo[parent.ParentId];
            }

            UpdateUI();
        }

        internal void CheckItem2(Guid id, bool isChecked)
        {

            if (isChecked)
                AttributoFilterData.CheckedValori.Add(id.ToString());
            else
                AttributoFilterData.CheckedValori.Remove(id.ToString());

            UpdateUI();
        }


        internal void CheckItem(string text, bool isChecked)
        {
            if (isChecked)
                AttributoFilterData.CheckedValori.Add(text);
            else
                AttributoFilterData.CheckedValori.Remove(text);

            UpdateUI();
            
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
            if (IsCurrentAttributoSourceGuid)
            {

                foreach (var item in _valoriItemTemp)
                {
                    if (item.IsChecked || item.IsAnyChildChecked())
                        return true;
                }
            }
            else if (_valoriItem != null)
            {
                foreach (var item in _valoriItem)
                {
                    if (item.IsChecked || item.IsAnyChildChecked())
                        return true;
                }
            }
            return false;
        }

        protected void OnAttributoChanged(EventArgs e)
        {
            AttributoChanged?.Invoke(this, e);
        }

        internal void OnValoriCheckedChanged(EventArgs e)
        {
            ValoriCheckedChanged?.Invoke(this, e);
        }

        ObservableCollection<FilterTypeView> _filterTypes = new ObservableCollection<FilterTypeView>();
        public ObservableCollection<FilterTypeView> FilterTypes
        {
            get => _filterTypes;
            set => SetProperty(ref _filterTypes, value);
        }

        void LoadFilterTypes()
        {

            Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(CurrentAttributo);

            ObservableCollection<FilterTypeView> filterTypes = new ObservableCollection<FilterTypeView>();
            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale ||
                sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
            {

                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Result,
                    Name = LocalizationProvider.GetString("Valore"),
                });

                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Formula,
                    Name = LocalizationProvider.GetString("Formula2"),
                });


                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Conditions,
                    Name = LocalizationProvider.GetString("Condizioni"),
                });

                FilterTypes = filterTypes;

                FilterTypeView currentFilterType = FilterTypes[0];
                FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == AttributoFilterData.FilterType);
                if (currentFilterType2 != null)
                    currentFilterType = currentFilterType2;


                SetFilterType(currentFilterType);

            }
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
            {

                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Result,
                    Name = LocalizationProvider.GetString("Valore"),
                });


                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Conditions,
                    Name = LocalizationProvider.GetString("Condizioni"),
                });

                FilterTypes = filterTypes;

                FilterTypeView currentFilterType = FilterTypes[0];
                FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == AttributoFilterData.FilterType);
                if (currentFilterType2 != null)
                    currentFilterType = currentFilterType2;

                SetFilterType(currentFilterType);

            }
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data)
            {
                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Result,
                    Name = LocalizationProvider.GetString("Valore"),
                });

                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Conditions,
                    Name = LocalizationProvider.GetString("Condizioni"),
                });

                FilterTypes = filterTypes;

                FilterTypeView currentFilterType = FilterTypes[0];
                FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == AttributoFilterData.FilterType);
                if (currentFilterType2 != null)
                    currentFilterType = currentFilterType2;


                SetFilterType(currentFilterType);
            }
            else
            {
                filterTypes.Add(new FilterTypeView()
                {
                    FilterType = FilterTypeEnum.Result,
                    Name = LocalizationProvider.GetString("Valore"),
                });

                FilterTypes = filterTypes;

                FilterTypeView currentFilterType = FilterTypes[0];
                FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == AttributoFilterData.FilterType);
                if (currentFilterType2 != null)
                    currentFilterType = currentFilterType2;

                SetFilterType(currentFilterType);
            }


            UpdateUI();

        }

        FilterTypeView _currentFilterType = null;
        public FilterTypeView CurrentFilterType
        {
            get => _currentFilterType;
            set
            {
                if (SetProperty(ref _currentFilterType, value))
                {
                    SetFilterType();
                }
            }
        }

        private void SetFilterType(FilterTypeView filterType = null)
        {

            if (filterType != null)
                _currentFilterType = filterType;

            if (_currentFilterType == null)
                return;

            AttributoFilterData.FilterType = _currentFilterType.FilterType;

            if (_currentFilterType.FilterType == FilterTypeEnum.Conditions)
            {
                if (filterType == null)//cambio filterType
                    LoadConditions(true);
                else //caricamento della finestra
                    LoadConditions(false);
            }
            else
            {
                if (_currentAttributoItem.IsGuid)
                {
                    //if (filterType == null)//cambio filterType
                        LoadValoriGuidAsync(AttributoFilterData.IsAllChecked == true);
                    //else //caricamento della finestra
                    //    LoadValoriGuidAsync(!AttributoFilterData.CheckedValori.Any() && CurrentAttributo.AllowValoriUnivoci);
                }
                else
                {
                    //if (filterType == null)//cambio filterType
                        LoadValoriAsync(AttributoFilterData.IsAllChecked == true);
                    //else //caricamento della finestra
                    //    LoadValoriAsync(!AttributoFilterData.CheckedValori.Any() && CurrentAttributo.AllowValoriUnivoci);
                }
            }

            UpdateUI();
        }

        private async void LoadConditions(bool clear)
        {
            ValoreConditionsGroupView.CodiceAttributoFixed = CurrentAttributo.Codice;
            ValoreConditionsGroupView.EntityType = CurrentAttributo.EntityType;
            ValoreConditionsGroupView.DataService = DataService;

            if (clear)
                ValoreConditionsGroupView.Data = new ValoreConditions();
            else
                ValoreConditionsGroupView.Data = AttributoFilterData.ValoreConditions;

            //List<string> valoriUnivociResult = new List<string>();

            //List<double> valoriUnivoci = new List<double>();
            //foreach (string str in valoriUnivociResult)
            //{
            //    string strNum = RemoveNonNumeric(str);

            //    double number = 0.0;
            //    if (Double.TryParse(strNum, out number))
            //    {
            //        valoriUnivoci.Add(number);
            //    }
            //}

            ValoreConditionsGroupView.Load(/*new List<string>()*/);
        } 
        
        //static string RemoveNonNumeric(string value) => Regex.Replace(value, @"[^0-9,.-]", string.Empty);

        public bool IsConditionsFilterType { get => CurrentFilterType != null && CurrentFilterType.FilterType == FilterTypeEnum.Conditions; }
    }



    public class AttributoItemView : NotificationBase
    {
        public static AttributoItemView Empty { get => new AttributoItemView() { Name = string.Empty, IsGuid = false, Codice = string.Empty }; }

        public string Name { get; set; }
        public string Codice { get; set; }
        public bool IsGuid { get; set; }

        public bool IsEmpty() { return Name == string.Empty; }
    }




    public class ValoreItemView : NotificationBase
    {
        SetAttributoFilterView _owner = null;
        public ObservableCollection<ValoreItemView> SubItems { get; set; } = new ObservableCollection<ValoreItemView>();

        public ValoreItemView(SetAttributoFilterView owner)
        {
            _owner = owner;
        }


        bool _isChecked = false;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                SetCheck(value);
                _owner.OnValoriCheckedChanged(new EventArgs());
            }
        }

        internal void SetCheck(bool check)
        {

            if (SetProperty(ref _isChecked, check))
            {
                if (_owner.CurrentAttributoItem.IsGuid)
                    _owner.CheckItem2(Id, check);
                else
                    _owner.CheckItem(Text, check);

                UpdateUI();
            }

        }



//        bool _isChecked = false;
//        public bool IsChecked
//        {
//            get
//            {

//                if (HasChidren)
//                {
//                    if (IsAllChildChecked())
//                        return true;
//                    return false;
//                }
//                else
//                {
//                    return _isChecked;
//                }
//            }
//            set
//            {
//                SetCheck(value);
//                _owner.OnValoriCheckedChanged(new EventArgs());
//            }
//        }

        //internal void SetCheck(bool check)
        //{

        //    if (SetProperty(ref _isChecked, check))
        //    {
        //        if (_owner.CurrentAttributoItem.IsGuid)
        //            _owner.CheckItem(Id, check);
        //        else
        //            _owner.CheckItem(Text, check);

        //        UpdateUI();
        //        SetChildrenCheck(check);
        //    }
           
        //}

        public void SetChildrenCheck(bool check)
        {
            if (HasChidren)
            {
                foreach (var item in SubItems)
                    item.SetCheck(check);
            }
        }
        //public bool IsChecked
        //{
        //    get
        //    {
        //        return _owner.CheckedValori.Contains(Text);
        //    }
        //    set
        //    {
        //        if (value)
        //            _owner.CheckedValori.Add(Text);
        //        else
        //            _owner.CheckedValori.Remove(Text);

        //        _owner.UpdateIsAllChecked();
        //    }
        //}


        public bool HasChidren { get => SubItems.Any(); }

        internal void SetCheckByCode(bool isChecked)
        {
            _isChecked = isChecked;
        }


        string _text = string.Empty;
        public string Text
        {
            get
            {
                if (_owner.CurrentAttributoItem.IsGuid)
                {
                    if (Id == Guid.Empty)
                        return LocalizationProvider.GetString("_Nessuno");

                    Attributo sourceAtt = _owner.GetCurrentSourceAttributo();
                    Entity ent = _owner.DataService.GetEntityById(sourceAtt.GuidReferenceEntityTypeKey, Id);
                    string str = ent.ToUserIdentity(UserIdentityMode.Nothing);
                    return str;
                }
                else
                {
                    return _text;
                }
            }
            set
            {
                SetProperty(ref _text, value);
            }
        }

        internal void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsChecked));
            RaisePropertyChanged(GetPropertyName(() => Background));
            RaisePropertyChanged(GetPropertyName(() => IsExpanded));

            if (SubItems != null)
            {
                foreach (var item in SubItems)
                    item.UpdateUI();
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

    }
}
