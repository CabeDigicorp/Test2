using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace MasterDetailView
{
    public class AttributoFilterDetailView : NotificationBase<AttributoFilterData>
    {
        EntitiesListMasterDetailView _master;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreConditionsGroupView ValoreConditionsGroupView { get; set; } = null;

        public AttributoFilterDetailView(EntitiesListMasterDetailView master, AttributoFilterData data) :base(data)
        {
            _master = master;

            Attributo = Master.EntityType.Attributi[This.CodiceAttributo];
            _nome = Attributo.Etichetta;

            //if (Attributo.ValoreDefault.ItemTextCount() > 0)
            //    _itemTextIndex = 0;
        }

        public Attributo Attributo { get; set; }

        /// <summary>
        /// Id del master da filtrare (che provengono da filtri su altri attributi)
        /// </summary>
        List<Guid> _entitiesFound = new List<Guid>();
        
        public AttributoFilterData Data { get => This; }


        bool _isModelToViewLoading = false;
        public bool IsModelToViewLoading
        {
            get => _isModelToViewLoading;
            private set
            { 
                if (SetProperty(ref _isModelToViewLoading, value))
                    UpdateUI();
            }
        }

        public void Accept()
        {
            if (This.FilterType == FilterTypeEnum.Conditions)
            {
                //save conditions
                ValoreConditionsGroupView.UpdateData();
                This.ValoreConditions = ValoreConditionsGroupView.Data;
            }
            else
            {
                //gli altri tab sono già salvati al momento del check
            }

            
        }


        public void Load()
        {
            LoadFilterTypes();
            //LoadValoriFiltro(true, !Data.CheckedValori.Any());/*!This.IsFiltroAttivato*/
        }



        public event EventHandler TextSearchedEnter;
        void OnTextSearchedEnter(EventArgs e)
        {
            TextSearchedEnter?.Invoke(this, e);
        }  

        /// <summary>
        /// Valori visualizzati nella list
        /// </summary>
        ObservableCollection<ValoreFilterDetailView> _valoriFilter = new ObservableCollection<ValoreFilterDetailView>();
        public ObservableCollection<ValoreFilterDetailView> ValoriFilter
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

        //public HashSet<string> CheckedValori { get; set; } = new HashSet<string>();

        //string _textSearched;
        public string TextSearched
        {
            get { return This.TextSearched; }
            set
            {
                //if (SetProperty(ref _textSearched, value))
                if (SetProperty(This.TextSearched, value, () => This.TextSearched = value))
                {
                    //bool searchToApply = false;
                    //if (IsAnyChecked)
                    //    searchToApply = true;

                    //IsAllChecked = false;
                    This.CheckedValori.Clear();
                    LoadValoriFiltroAsync(true, true);
                    //if (searchToApply)
                    //{
                    //    Master.RightSplitView.FilterView.CurrentAttributo.SetCheckedValuesAsString();
                    //    Master.ApplyFilterAndSort();
                    //    Master.Load();
                    //}

                }

            }
        }

        public ICommand SubmitEnterCommand
        {
            get
            {
                return new CommandHandler(() => this.SubmitEnter());
            }
        }

        void SubmitEnter()
        {
            IsAllChecked = true;
            OnTextSearchedEnter(EventArgs.Empty);
        }

        //bool _isFiltroAttivato = false;
        //public bool IsFiltroAttivato
        //{
        //    get { return _isFiltroAttivato; }
        //    set
        //    {
        //        if (SetProperty(ref _isFiltroAttivato, value))
        //        {
        //            if (this.IsAnyChecked)
        //            {
        //                Master.ApplyFilterAndSort(null, false);
        //                Master.Load();
        //            }
        //        }
        //    }
        //}


        //public bool IsAttributoValueMultiText
        //{
        //    get { return Attributo.ValoreDefault is ValoreTestoCollection; }
        //}

        public string EtichettaAttributo
        {
            get { return Attributo.Etichetta; }
        }

        ///// <summary>
        ///// Per scegliere quale filtrare tra testo1, testo2 e testo3 in ValoreTestoCollectionItem
        ///// </summary>
        //int _itemTextIndex = -1;
        //public int ItemTextIndex
        //{
        //    get { return _itemTextIndex; }
        //    set
        //    {
        //        if (SetProperty(ref _itemTextIndex, value))
        //        {
        //            LoadValoriFiltroAsync(true, true);
        //        }

        //    }
        //}

        //public ICommand ClearFiltroCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.ClearFiltro());
        //    }
        //}

        //private void ClearFiltro()
        //{
        //    int filtroIndex = Master.RightSplitView.FilterView.Items.IndexOf(this);
        //    if (filtroIndex > 0)
        //        Master.RightSplitView.FilterView.CurrentAttributo = Master.RightSplitView.FilterView.Items[filtroIndex - 1];
        //    else
        //        Master.RightSplitView.FilterView.CurrentAttributo = null;

        //    Master.RightSplitView.FilterView.Items.Remove(this);

        //    if (Master.RightSplitView.FilterView.CurrentAttributo != null)
        //        Master.ApplyFilterAndSort(null, false, true);
        //    else
        //        Master.ApplyFilterAndSort(null, false);

        //    Master.Load();
        //    Master.RightSplitView.FilterView.ClearAllFiltroVisible = Master.RightSplitView.FilterView.Items.Any();
        //}

        //bool _isVisible = false;
        //public bool IsVisible
        //{
        //    get { return _isVisible; }
        //    set { SetProperty(ref _isVisible, value); }
        //}

        public bool IsValid()
        {
            return This.IsValid();
            ////if (_valoriUnivociCount <= 0)
            ////    return false;

            //if (IsAllChecked == true)
            //    return true;

            //if (This.CheckedValori.Any())
            //    return true;

            //return false;
        }

        string _selectedValuesAsString = "";
        public string SelectedValuesAsString
        {
            get { return _selectedValuesAsString; }
            set { SetProperty(ref _selectedValuesAsString, value); }
        }

        //public void SetCheckedValuesAsString()
        //{
        //    if (IsAllChecked == true)
        //    {
        //        SelectedValuesAsString = TextSearched;
        //    }
        //    else
        //    {
        //        if (This.CheckedValori.Count == 1)
        //            SelectedValuesAsString = This.CheckedValori.First();
        //        else if (This.CheckedValori.Count > 1)
        //            SelectedValuesAsString = StringResoucesTemp.GetResourceString("AttributoFilterView_Multi");
        //        else SelectedValuesAsString = "";
        //    }

        //}

        string _nome;
        public string Nome
        {
            get { return _nome; }
        }


        //bool _isValoriCheckReadonly = false;
        //public bool IsValoriCheckReadOnly
        //{
        //    get { return _isValoriCheckReadonly; }
        //    set { SetProperty(ref _isValoriCheckReadonly, value); }

        //}

        
        public bool? IsAllChecked
        {
            get { return This.IsAllChecked; }
            set
            {
                bool isAllCheched = value == true ? true : false;

                if (SetProperty(This.IsAllChecked, value, () => This.IsAllChecked = isAllCheched))
                {
                    if (This.IsAllChecked == true)
                    {
                        //if (_valoriUnivociFiltro.Count < MaxFilterValueCount)
                            This.CheckedValori = new HashSet<string>(_valoriUnivociFiltro.Select(item => item.Valore.ToPlainText()));
                    }
                    else if (This.IsAllChecked == false)
                    {
                        This.CheckedValori = new HashSet<string>();
                    }

                    LoadValoriFiltroAsync(false, false);

                    //Master.RightSplitView.FilterView.CurrentAttributo.SetCheckedValuesAsString();
                    //Master.ApplyFilterAndSort(null, !IsFiltroAttivato, true);
                    //Master.RightSplitView.FilterView.SearchNext(false);


                    //Globals.CurrentEntitiesMasterDetailView.UpdateCache();
                    //Globals.CurrentEntitiesMasterDetailView.UpdateCache(Master.RightSplitView.FilterView.CurrentEntityFoundId);

                    //if (Master.RightSplitView.FilterView.IsSearchApplied())
                    //    Master.SelectEntityById(Master.RightSplitView.FilterView.CurrentEntityFoundId);

                }

            }
        }

        #region Replace in Text

        public ICommand ReplaceInTextCommand
        {
            get
            {
                return new CommandHandler(() => this.ReplaceInText());
            }
        }

        public void ReplaceInText()
        {
            Master.WindowService.ShowReplaceTextWindow(this);

            RaisePropertyChanged(GetPropertyName(() => ReplaceInTextLabel));
            RaisePropertyChanged(GetPropertyName(() => ReplaceInTextEnabled));
        }

        public bool IsReplaceInTextAllowed
        {
            get { return Attributo.AllowReplaceInText && !Master.DataService.IsReadOnly; }
        }

        public ICommand ReplaceInTextInCurrentEntityCommand
        {
            get
            {
                return new CommandHandler(() => this.ReplaceInTextInCurrentEntity());
            }
        }


        public async void ReplaceInTextInCurrentEntity()
        {
            if (!_textToReplace.Any())
                return;

            HashSet<Guid> entitiesId = new HashSet<Guid>() { Master.SelectedEntityId };
            Valore newValore = new ValoreTesto() { V = ReplaceNewText };
            Valore oldValore =  new ValoreTesto() { V = _textToReplace } ;
            ModelAction action = new ModelAction()
            {
                ActionName = ActionName.ATTRIBUTO_VALORE_REPLACEINTEXT,
                AttributoCode = Attributo.Codice,
                EntityTypeKey = Attributo.EntityTypeKey,
                EntitiesId = entitiesId,
                NewValore = newValore,
                OldValore = oldValore
            };

            ModelActionResponse mar = Master.CommitAction(action);

            if (mar.ActionResponse == ActionResponse.OK)
            {
                //Tolgo dai trovati l'elemento sostituito
                Master.RightPanesView.FilterView.FoundEntitiesId.Remove(Master.SelectedEntityId);

                ////Azzero detail in modo che vengano ricaricati quando sarà completato il caricamento del tree
                //Aggiorno tree
                await Master.UpdateCache(true);
            }
        }

        public ICommand SearchNextCommand
        {
            get
            {
                return new CommandHandler(() => this.SearchNext());
            }
        }

        public void SearchNext()
        {
            if (!_textToReplace.Any())
                return;

            Master.RightPanesView.FilterView.SearchNext();

            if (Master.RightPanesView.FilterView.IsSearchApplied())
                Master.SelectEntityById(Master.RightPanesView.FilterView.CurrentEntityFoundId);
        }

        public ICommand SearchPreviousCommand
        {
            get
            {
                return new CommandHandler(() => this.SearchPrevious());
            }
        }

        public void SearchPrevious()
        {
            if (!_textToReplace.Any())
                return;

            Master.RightPanesView.FilterView.SearchPrevious();

            if (Master.RightPanesView.FilterView.IsSearchApplied())
                Master.SelectEntityById(Master.RightPanesView.FilterView.CurrentEntityFoundId);
        }

        public ICommand ReplaceInTextInAllEntitiesCommand
        {
            get
            {
                return new CommandHandler(() => this.ReplaceInTextInAllEntities());
            }
        }


        public async void ReplaceInTextInAllEntities()
        {
            if (!_textToReplace.Any())
                return;

            HashSet<Guid> entitiesId = new HashSet<Guid>(Master.RightPanesView.FilterView.FoundEntitiesId);
            Valore newValore = new ValoreTesto() { V = ReplaceNewText };
            Valore oldValore = new ValoreTesto() { V = _textToReplace };
            ModelAction action = new ModelAction()
            {
                ActionName = ActionName.ATTRIBUTO_VALORE_REPLACEINTEXT,
                AttributoCode = Attributo.Codice,
                EntityTypeKey = Attributo.EntityTypeKey,
                EntitiesId = entitiesId,
                NewValore = newValore,
                OldValore = oldValore
            };

            ModelActionResponse mar = Master.CommitAction(action);


            if (mar.ActionResponse == ActionResponse.OK)
            {

                Guid currentSelectedEntityId = Master.SelectedEntityId;
                Master.SelectEntityView(null);

                Master.ApplyFilterAndSort(Master.SelectedEntityId, true);//only search

                await Master.UpdateCache();

                Master.SelectEntityById(currentSelectedEntityId, false);

            }
        }

        public string TextToReplace
        {
            get => _textToReplace;
            set
            {
                if (SetProperty(ref _textToReplace, value))
                {
                    TextSearched = TextToReplace;
                    Find();
                }
            }

        }

        string _replaceNewText = "";
        public string ReplaceNewText
        {
            get { return _replaceNewText; }
            set { SetProperty(ref _replaceNewText, value); }
        }


        public string ReplaceInTextLabel
        {
            get { return string.Join(" ", "Sostituisci", TextSearched, "con..."); }
        }

        string _textToReplace = "";

        public bool ReplaceInTextEnabled
        {
            get
            {

                return IsReplaceInTextAllowed;
                //if (TextSearched == null || !TextSearched.Any())
                //    return false;

                //if (!Master.RightSplitView.FilterView.FoundEntitiesId.Any())
                //    return false;

                //string trimmedTxt = TextSearched.Trim();

                //if (trimmedTxt.StartsWith("\"") && trimmedTxt.EndsWith("\"") && trimmedTxt.Length > 2)
                //{
                //    _textToReplace = trimmedTxt.Substring(1, trimmedTxt.Length - 2);
                //    return true;
                //}

                //return false;
            }
        }



        #endregion Replace in Text

        public void UpdateIsAllChecked()
        {
            if (This.CheckedValori.Any())
            {
                HashSet<string> strUnivociFiltro = new HashSet<string>(_valoriUnivociFiltro.Select(item => item.Valore.ToPlainText()));
                if (strUnivociFiltro.SetEquals(This.CheckedValori))
                    This.IsAllChecked = true;
                else
                    This.IsAllChecked = null;//false;// null;
            }
            else
            {
                This.IsAllChecked = false;
            }

            RaisePropertyChanged(GetPropertyName(() => this.IsAllChecked));
        }

        public string ValoriFiltroCount
        {
            get
            {
                string sValoriUnivociCount = _valoriUnivociCount.ToString();

                string sValoriUnivociChecked = This.CheckedValori.Count().ToString();
                if (IsAllChecked == true)
                    sValoriUnivociChecked = _valoriUnivociCount.ToString();


                return "(" + sValoriUnivociChecked + "/" + sValoriUnivociCount + ")";
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsValoriUnivociLoading));
            RaisePropertyChanged(GetPropertyName(() => IsValoriUnivociVisible));
            RaisePropertyChanged(GetPropertyName(() => AllowValoriUnivoci));
            RaisePropertyChanged(GetPropertyName(() => IsFilterTypesVisible));
            RaisePropertyChanged(GetPropertyName(() => IsConditionsFilterType));
            RaisePropertyChanged(GetPropertyName(() => CurrentFilterType));
            RaisePropertyChanged(GetPropertyName(() => TextSearched));
        }

        public bool IsValoriUnivociLoading { get => IsModelToViewLoading; }

        public bool IsValoriUnivociVisible
        {
            get => AllowValoriUnivoci && !IsValoriUnivociLoading;
        }

        public bool AllowValoriUnivoci
        {
            get { if (Attributo != null) return Attributo.AllowValoriUnivoci; else return false; }
        }

        public string ListViewPlaceholderText { get => LocalizationProvider.GetString("CaricamentoInCorso"); }

        /// <summary>
        /// Numero di valori oltre il quale viene proibito il check sul valore
        /// </summary>
        int MaxFilterValueCount { get { return 100; } }


        public bool IsAnyChecked
        {
            get
            {
                return This.CheckedValori.Count > 0/* || IsAllChecked*/;
            }
        }

        List<ValoreFilterDetailView> _valoriUnivociFiltro = new List<ValoreFilterDetailView>();
        SortedSet<ValoreFilterDetailView> _valoriUnivociFiltroSortedSet = new SortedSet<ValoreFilterDetailView>(new ValoreFilterViewComparer());


        int _valoriUnivociCount = 0;
        int _valoriUnivociFiltroIndex = 0;

        public List<ValoreFilterDetailView> GetValoriUnivociFiltro(int count, out bool hasMoreItems)
        {

            List<ValoreFilterDetailView> valori = new List<ValoreFilterDetailView>();
            hasMoreItems = false;

            for (; _valoriUnivociFiltroIndex < _valoriUnivociFiltro.Count; _valoriUnivociFiltroIndex++)
            {
                if (valori.Count >= count)
                {
                    hasMoreItems = true;
                    break;
                }

                ValoreFilterDetailView valView = _valoriUnivociFiltro[_valoriUnivociFiltroIndex];

                valori.Add(valView);

                if (IsAllChecked == true)
                    This.CheckedValori.Add(valView.Valore.ToPlainText());
            }

            return valori;
        }


        public async Task<HashSet<ValoreSingle>> GetValoriUnivociAsync(string entityTypeKey, List<Guid> entities, string codiceAttributo, int takeResult, string textSearched)
        {
            List<string> list = await Master.DataService.GetValoriUnivociAsync(entityTypeKey, entities, codiceAttributo, takeResult, textSearched);

            HashSet<ValoreSingle> vals = new HashSet<ValoreSingle>(list.Select(item => new ValoreTesto() { V = item }));
            return vals;
        }




        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        public void LoadValoriFiltro(bool updateValoriFiltro, bool checkAll)
        {
            if (checkAll)
                Data.TextSearched = string.Empty;

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

            UpdateUI();

            LoadValoriFiltroAsync(updateValoriFiltro, checkAll);
        }

        public async void LoadValoriFiltroAsync(bool updateValoriFiltro, bool checkAll/*, HashSet<string> valoriFiltro*/)
        {

            try
            {


                if (!Attributo.AllowValoriUnivoci)
                {
                    IsAllChecked = true;
                    IsAllCheckedEnabled = false;
                    return;
                }
                

                if (updateValoriFiltro)
                {

                    IsModelToViewLoading = true;
                    int takeResults = 0;// this.ItemTextIndex;

                    if (CurrentFilterType != null && CurrentFilterType.FilterType != FilterTypeEnum.Nothing)
                    {
                        if (CurrentFilterType.FilterType == FilterTypeEnum.Formula)
                            takeResults = 0;
                        else if (CurrentFilterType.FilterType == FilterTypeEnum.Result)
                            takeResults = 1;
                    }

                    //if (takeResults >= Attributo.ValoreDefault.ItemTextCount())
                    //    takeResults = -1;



                    HashSet<ValoreSingle> valoriUnivoci = null;


                    // If a cancellation token already exists (for a previous task),
                    // cancel it.
                    if (this._loadValoriFiltroCancellationTokenSource != null)
                        this._loadValoriFiltroCancellationTokenSource.Cancel();

                    // Create a new cancellation token for the new task.
                    this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
                    CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;

                    Task<HashSet<ValoreSingle>> valoriUnivociTask = null;
                    valoriUnivociTask = GetValoriUnivociAsync(Master.EntityType.GetKey(), _entitiesFound, Attributo.Codice, takeResults, TextSearched);
                    valoriUnivoci = await valoriUnivociTask;

                    if (cancellationToken.IsCancellationRequested)
                        return;


                    IsModelToViewLoading = true;

                    //Elimino le voci checkate precedentemente e non più esistenti
                    HashSet<string> plainTextUnivoci = new HashSet<string>(valoriUnivoci.Select(item => item.PlainText));
                    This.CheckedValori.IntersectWith(plainTextUnivoci);

                    _valoriUnivociFiltro.Clear();
                    _valoriUnivociFiltroSortedSet.Clear();

                    _valoriUnivociCount = valoriUnivoci.Count;
                    foreach (ValoreSingle val in valoriUnivoci)
                    {

                        bool isChecked = false;
                        if (checkAll)
                        {
                            isChecked = true;
                            This.CheckedValori.Add(val.ToPlainText());
                        }
                        else if (This.CheckedValori.Contains(val.ToPlainText()))
                        {
                            isChecked = true;
                        }

                        ValoreFilterDetailView valView = new ValoreFilterDetailView(Master, this) { Valore = val };
                        valView.SetCheckByCode(isChecked);
                        _valoriUnivociFiltroSortedSet.Add(valView);
                    }

                    _valoriUnivociFiltro = _valoriUnivociFiltroSortedSet.ToList();


                    if (checkAll)
                        IsAllChecked = true;
                    else
                        UpdateIsAllChecked();


                    RaisePropertyChanged(GetPropertyName(() => this.IsAllChecked));
                    RaisePropertyChanged(GetPropertyName(() => this.ValoriFiltroCount));

                    this._loadValoriFiltroCancellationTokenSource = null;

                    IsModelToViewLoading = false;
                }
                else
                {
                    //non ricarico i valori del filtro ma solo il valore del check
                    _valoriUnivociFiltro.ForEach(item =>
                    {

                        if (This.CheckedValori.Contains(item.Valore.ToPlainText()))
                            item.SetCheckByCode(true);
                        else
                            item.SetCheckByCode(false);
                    });

                    UpdateIsAllChecked();//AU 28/10/2020
                }


                _valoriUnivociFiltroIndex = 0;
                ValoriFilter = new ObservableCollection<ValoreFilterDetailView>(_valoriUnivociFiltro);
                //ValoriFilter = new ValoreFilterListIncrementalLoading(this);

                //if (IsAllChecked == true /*&& _valoriUnivociCount > MaxFilterValueCount*/)
                //{
                //    IsValoriCheckReadOnly = true;
                //}
                //else
                //{
                //    IsValoriCheckReadOnly = false;
                //}



                RaisePropertyChanged(GetPropertyName(() => this.IsAnyChecked));
                RaisePropertyChanged(GetPropertyName(() => this.ValoriFiltroCount));
                
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, LocalizationProvider.GetString("AppName"));
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

        }


        bool _isAllChechedEnabled = true;
        public bool IsAllCheckedEnabled
        {
            get => _isAllChechedEnabled;
            set {SetProperty(ref _isAllChechedEnabled, value);}
        }

        public void UpdateValoriFiltroCount()
        {
            RaisePropertyChanged(GetPropertyName(() => this.ValoriFiltroCount));
        }

        ///// <summary>
        ///// Ricarica la lista dei valori di filtro mantenendo i check
        ///// </summary>
        //public void Synchronize()
        //{
        //    HashSet<string> checkedValues = new HashSet<string>(This.CheckedValori);
        //    //LoadValoriFiltro();
        //    Master.ApplyFilterAndSort(null, false);
        //    Master.Load();

        //    This.CheckedValori = new HashSet<string>(_valoriUnivociFiltro.Select(item => item.Valore.ToPlainText()).Where(item => checkedValues.Contains(item)));
        //    UpdateValoriFiltroCount();
        //}

        public void Find()
        {
            if (IsModelToViewLoading)
                return;


            Master.RightPanesView.FilterView.CurrentAttributo.Find();
        }


        //public void Filter()
        //{
        //    if (IsModelToViewLoading)
        //        return;

        //    Master.RightPanesView.FilterView.CurrentAttributo.Filter();
        //}

        public bool IsFilterTypesVisible
        {
            get
            {
                return FilterTypes.Any();
            }
        }

        ObservableCollection<FilterTypeView> _filterTypes = new ObservableCollection<FilterTypeView>();
        public ObservableCollection<FilterTypeView> FilterTypes
        {
            get => _filterTypes;
            set => SetProperty(ref _filterTypes, value);
        }

        void LoadFilterTypes()
        {

            ObservableCollection<FilterTypeView> filterTypes = new ObservableCollection<FilterTypeView>();

            if (Master.EntitiesHelper.IsAttributoRiferimentoGuidCollection(Attributo))
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
                FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == This.FilterType);
                if (currentFilterType2 != null)
                    currentFilterType = currentFilterType2;


                SetFilterType(currentFilterType);
            }
            else
            {
                Attributo sourceAtt = Master.GetSourceAttributoOf(Attributo);
                
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
                    FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == This.FilterType);
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
                    FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == This.FilterType);
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
                    FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == This.FilterType);
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
                    FilterTypeView currentFilterType2 = FilterTypes.FirstOrDefault(item => item.FilterType == This.FilterType);
                    if (currentFilterType2 != null)
                        currentFilterType = currentFilterType2;

                    SetFilterType(currentFilterType);
                }

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

            Data.FilterType = _currentFilterType.FilterType;

            if (_currentFilterType.FilterType == FilterTypeEnum.Conditions)
            {
                if (filterType == null)//cambio filterType
                    LoadConditions(true);
                else //caricamento della finestra
                    LoadConditions(false);
            }
            else
            {
                if (filterType == null)//cambio filterType
                    LoadValoriFiltro(true, true);
                else //caricamento della finestra
                    LoadValoriFiltro(true, !Data.CheckedValori.Any() && Attributo.AllowValoriUnivoci);
            }

            UpdateUI();
        }

        private async void LoadConditions(bool clear)
        {
            ValoreConditionsGroupView.CodiceAttributoFixed = Attributo.Codice;
            ValoreConditionsGroupView.EntityType = Attributo.EntityType;
            ValoreConditionsGroupView.DataService = Master.DataService;

            if (clear)
                ValoreConditionsGroupView.Data = new ValoreConditions();
            else
                ValoreConditionsGroupView.Data = Data.ValoreConditions;

            //List<string> valoriUnivociResult = await Master.DataService.GetValoriUnivociAsync(Master.EntityType.GetKey(), Master.FilteredEntitiesId, Attributo.Codice, 1, null);

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

            //ValoreConditionsGroupView.Load(valoriUnivoci.OrderBy(item => item).Select(item => item.ToString()).ToList());
            ValoreConditionsGroupView.Load();
        }

        public static string RemoveNonNumeric(string value) => Regex.Replace(value, @"[^0-9,.-]", string.Empty);



        public bool IsConditionsFilterType { get => CurrentFilterType != null && CurrentFilterType.FilterType == FilterTypeEnum.Conditions; }






    }

    public class ValoreFilterDetailView : NotificationBase
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        
        AttributoFilterDetailView _owner = null;
        public ValoreFilterDetailView(EntitiesListMasterDetailView master, AttributoFilterDetailView owner)
        {
            _master = master;
            _owner = owner;
        }

        ValoreSingle _valore;
        public ValoreSingle Valore
        {
            get
            {
                return _valore;
            }

            set
            {
                SetProperty(ref _valore, value);
            }
        }

        bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {

                if (SetProperty(ref _isChecked, value))
                {
                    if (!_owner.IsModelToViewLoading)
                    {

                        //if (_isChecked)
                        //    Master.RightPanesView.FilterView.CurrentAttributo.CheckedValori.Add(this.Valore.ToPlainText());
                        //else
                        //    Master.RightPanesView.FilterView.CurrentAttributo.CheckedValori.Remove(this.Valore.ToPlainText());

                        if (_isChecked)
                            _owner.Data.CheckedValori.Add(this.Valore.ToPlainText());
                        else
                            _owner.Data.CheckedValori.Remove(this.Valore.ToPlainText());

                        _owner.UpdateIsAllChecked();
                        _owner.UpdateValoriFiltroCount();
                    }
                }
            }
        }

        bool _isCheckEnabled = true;
        public bool IsCheckEnabled
        {
            get { return _isCheckEnabled; }
            set { SetProperty(ref _isCheckEnabled, value); }
        }

        

        internal void SetCheckByCode(bool isChecked)
        {
            _isChecked = isChecked;
        }
    }

    public class FilterTypeView
    {
        public string Name { get; set; } = string.Empty;
        public FilterTypeEnum FilterType { get; set; } = FilterTypeEnum.Nothing;
    }
}
