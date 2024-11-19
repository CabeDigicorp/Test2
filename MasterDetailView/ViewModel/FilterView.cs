

using CommonResources;
using Commons;
using DevExpress.Mvvm.Native;
using DevExpress.Utils.Design;
using DevExpress.XtraRichEdit.Model;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MasterDetailView
{

    public class FilterView : NotificationBase, RightPaneView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public FilterData Data { get; set; } = new FilterData();

        public FilterView(EntitiesListMasterDetailView master)
        {
            //This = this;
            _master = master;

            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUI();
        }


        //public EntitiesListMasterDetailView Master { get; set; }

        AttributoFilterView _currentAttributo;
        public AttributoFilterView CurrentAttributo
        {
            get { return _currentAttributo; }
            set
            {
                if (SetProperty(ref _currentAttributo, value))
                {
                }
            }
        }


        ObservableCollection<AttributoFilterView> _items = new ObservableCollection<AttributoFilterView>();
        public ObservableCollection<AttributoFilterView> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public bool IsFilterApplied()
        {
            return Data.IsFilterApplied();
        }

        public bool IsSearchApplied()
        {
            return Data.IsSearchApplied();
        }

        AttributoFilterView NewAttributoFilter { get; set; } = null;


        /// <summary>
        /// Carica da settings precedenti
        /// </summary>
        /// <param name="viewSettings"></param>
        public void Load(EntityTypeViewSettings viewSettings)
        {
            if (viewSettings == null)
                return;

            if (Data == null)
                Data = new FilterData();

            Items.Clear();
            Data.Items.Clear();

            Master.IsMultipleModify = false;

            foreach (AttributoFilterData filterAtt in viewSettings.Filters)
            {
                AttributoFilterData attFilterData = filterAtt.Clone();
                if (Master.EntityType.Attributi.ContainsKey(filterAtt.CodiceAttributo))
                {
                    AttributoFilterView attFilterView = new AttributoFilterView(Master, attFilterData);
                    Items.Add(attFilterView);
                    Data.Items.Add(attFilterData);
                    attFilterView.SetCheckedValuesAsString();
                }
                else if (filterAtt.CodiceAttributo == BuiltInCodes.Attributo.TemporaryFilterByIds)
                {
                    AttributoFilterView attFilterView = new AttributoFilterView(Master, attFilterData);
                    Items.Add(attFilterView);
                    Data.Items.Add(attFilterData);
                    attFilterView.SetCheckedValuesAsString();
                }

            }
        }

        /// <summary>
        /// Aggiunge una nuova piastrella nell lista dei filtri attivi
        /// </summary>
        /// <param name="att"></param>
        public bool Load(Attributo att, int index)
        {

            Attributo sourceAtt = Master.GetSourceAttributoOf(att);

            Master.IsMultipleModify = false;

            //if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection &&
            //    att.EntityTypeKey == sourceAtt.EntityTypeKey &&
            //    (att.ValoreAttributo as ValoreAttributoGuidCollection)?.ItemsSelectionType != ItemsSelectionTypeEnum.ByHand)
            //{
            //    Master.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileFiltrarePerQuestoAttributo"));
            //    return;
            //}

            if (sourceAtt != null && sourceAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection && sourceAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.TestoCollection)
            {
                //Oss: se l'attributo è un multivalore o multiGuid lascio filtrare più volte per lo stesso attributo per riuscire a fare l'AND

                //nessun filtro per lo stesso attributo
                AttributoFilterView attFilterView = Items.FirstOrDefault(item => item.GetCodice() == sourceAtt.Codice/*sourceCodiceAtt*/);
                if (attFilterView != null)
                {
                    index = Items.IndexOf(attFilterView);
                } 
            }


            AttributoFilterData attFilterData = null;
            if (index < 0) //creao un nuovo filtro attributo per aggiungerlo in seguito
            {
                attFilterData = new AttributoFilterData()
                {
                    EntityTypeKey = att.EntityTypeKey,
                    CodiceAttributo = att.Codice,
                    SourceEntityTypeKey = sourceAtt.EntityTypeKey,// sourceEntityTypeKey,
                    SourceCodiceAttributo = sourceAtt.Codice, //sourceCodiceAtt
                    IsFiltroAttivato = this.IsFiltroAttivato,
                };

                if (sourceAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.Guid
                    && sourceAtt.DefinizioneAttributoCodice != BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    attFilterData.IsAllChecked = true;//AU 27/10/2020
                }


                NewAttributoFilter = new AttributoFilterView(Master, attFilterData);
                CurrentAttributo = NewAttributoFilter;

            }
            else //edito un filtro attributo esistente
            {
                attFilterData = Data.Items[index] as AttributoFilterData;

                if (attFilterData != null)
                {
                    attFilterData = attFilterData.Clone();
                    CurrentAttributo = Items.First(item => item.GetCodice() == attFilterData.CodiceAttributo);
                }
            }

            Master.IsFilterPaneOpen = true;
            //IsClearAllFiltroVisible = Items.Any();
            

            int res = 0;
            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid
                || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {
                List<Guid> itemsId = attFilterData.CheckedValori.Select(item => new Guid(item)).ToList();

                res = Master.WindowService.FilterByEntityIdsWindow(Master, attFilterData, sourceAtt.GuidReferenceEntityTypeKey, ref itemsId,
                    string.Format("{0} {1}", LocalizationProvider.GetString("TrovaFiltraPer"), att.Etichetta));


                if (res == 1)//find
                {
                    attFilterData.CheckedValori = new HashSet<string>(itemsId.Select(item => item.ToString()));
                }
            }
            else
            {
                if (Master.WindowService.AttributoFilterDetailWindow(Master, attFilterData) == true)
                {
                    
                    res = 1;
                }
            }

            
            if (res == 1)
            {

                if (index < 0)//Aggiungo effettivamente il nuovo filtro come tile e FilterData
                {
                    //aggiungo la tile per il nuovo filtro
                    Data.Items.Add(attFilterData);
                    Items.Add(NewAttributoFilter);

                    //IsClearAllFiltroVisible = Items.Any();
                }
                
                //Imposto il nuovo filtro per l'attributo e azzero le ricerche/filtri attributi a valle effettuati precedentemente
                if (index >= 0)
                {
                    Data.Items[index] = attFilterData;
                    Items[index].SetData(attFilterData);

                    for (int i = index; i < Data.Items.Count; i++)
                    {
                        AttributoFilterData attFilterData2 = Data.Items[i] as AttributoFilterData;
                        if (attFilterData2 != null)
                        {
                            attFilterData2.FoundEntitiesId = null;
                        }
                    }
                }

                if (IsFiltroAttivato)
                    CurrentAttributo.Filter();
                else
                    CurrentAttributo.Find();

            }
            return (res == 1);
        }


        internal void UpdateAttributi()
        {

            List<AttributoFilterView> attToRemove = Items.Where(item => item.Attributo != null && !Master.EntityType.Attributi.ContainsKey(item.Attributo.Codice)).ToList();

            foreach (var item in attToRemove)
                item.ClearFiltro();
        }

        public void LoadTemporaryFilterByIds(string entityTypeKey, List<Guid> ids)
        {
            

            AttributoFilterView tempFilterByIds = Items.FirstOrDefault(item => item.GetCodice() == BuiltInCodes.Attributo.TemporaryFilterByIds);
            AttributoFilterData attFilterData = null;

            if (tempFilterByIds == null)
            {
                //Data
                attFilterData = new AttributoFilterData()
                {
                    EntityTypeKey = entityTypeKey,
                    CodiceAttributo = BuiltInCodes.Attributo.TemporaryFilterByIds,
                };
                Data.Items.Add(attFilterData);

                //View
                NewAttributoFilter = new AttributoFilterView(Master, attFilterData);
                Items.Add(NewAttributoFilter);
                CurrentAttributo = NewAttributoFilter;
            }
            else
            {
                attFilterData = Data.Items.FirstOrDefault(item => item.CodiceAttributo == BuiltInCodes.Attributo.TemporaryFilterByIds) as AttributoFilterData;
                CurrentAttributo = tempFilterByIds;
            }

            attFilterData.CheckedValori = new HashSet<string>(ids.Select(item => item.ToString()));

            //Attenzione: questa instruzione fa inchiodare l'apertura dei padri nell'albero degli articoli (non so perchè...)
            //Master.RightPanesView.IsFilterPaneOpen = true;
            

            Master.RightPanesView.FilterView.CurrentAttributo.Find();


            
        }

        public void UnLoad()
        {
            if (NewAttributoFilter != null && NewAttributoFilter.IsValid())
            {
                Items.Remove(NewAttributoFilter);
                NewAttributoFilter = null;
            }

            Master.IsMultipleModify = false;
            //IsClearAllFiltroVisible = Items.Any();
        }

        public ICommand ClearAllFiltroCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearAllFiltro());
            }
        }

        public void ClearAllFiltro(bool load = true)
        {
            CurrentAttributo = null;

            Data.Clear();

            Items.Clear();

            Master.IsMultipleModify = false;

            if (load)
            {
                Master.Load();
                
                if (Master.ModelActionsStack != null)
                    Master.ModelActionsStack.OnViewSettingsChanged();
            }

            UpdateUI();
        }


        public bool AnyFilter
        {
            get { return Items.Any(); }
        }

        public bool IsFiltroAttivato
        {
            get
            {
                if (!Items.Any())
                    return false;

                var ret = Items.All(x => x.IsFiltroAttivato);
                return ret;
            }

            set
            {
                Items.ForEach(x => x.IsFiltroAttivato = value);

                if (value)
                    Filter();
                else
                    Find();

                UpdateUI();
            }
        }

        public void FindOrFilter()
        {
            if (IsFiltroAttivato)
                Filter();
            else
                Find();
        }

        void Filter()
        {
            Master.WindowService.ShowWaitCursor(true);

            Guid selectedEntityIdPrec = Master.SelectedEntityId;

            Master.ApplyFilterAndSort(null, false);
            Master.ShowEntities(Master.RightPanesView.FilterView.FoundEntitiesId);

            Master.CheckedEntitiesId = new HashSet<Guid>(Master.RightPanesView.FilterView.FoundEntitiesId);


            Guid selectedEntityId = Guid.Empty;
            if (Master.RightPanesView.FilterView.FoundEntitiesId.Contains(selectedEntityIdPrec))
                selectedEntityId = selectedEntityIdPrec;

            if (selectedEntityId == Guid.Empty)//perchè non è stato fatto l'updateCache da SelectEntityById
                Master.UpdateCache();
            else
                Master.SelectEntityById(selectedEntityId);

            if (Master.ModelActionsStack != null)
                Master.ModelActionsStack.OnViewSettingsChanged();


            Master.WindowService.ShowWaitCursor(false);

        }

        async void Find()
        {





            Master.WindowService.ShowWaitCursor(true);
            RaisePropertyChanged(GetPropertyName(() => IsFiltroAttivato));

            Guid selectedEntityIdPrec = Master.SelectedEntityId;

            Master.ApplyFilterAndSort(null, false);

            Guid selectedEntityId = selectedEntityIdPrec;
            if (IsSearchApplied() && _foundEntitiesId != null && _foundEntitiesId.Any())
            {
                Master.ShowEntities(FoundEntitiesId);

                Master.CheckedEntitiesId = new HashSet<Guid>(FoundEntitiesId);
                Master.CheckedEntitiesId.UnionWith(Master.FilteredDescendantsId(FoundEntitiesId));

                if (FoundEntitiesId.Contains(selectedEntityIdPrec))
                    selectedEntityId = selectedEntityIdPrec;
                else
                    selectedEntityId = Guid.Empty;
            }

            if (selectedEntityId == Guid.Empty)//perchè non è stato fatto l'updateCache da SelectEntityById
                await Master.UpdateCache();
            else
                await Master.SelectEntityById(selectedEntityId);



            if (Master.ModelActionsStack != null)
                Master.ModelActionsStack.OnViewSettingsChanged();

            Master.WindowService.ShowWaitCursor(false);




        }


        #region Search


        /// <summary>
        /// Lista delle entità cercate, sottoinsieme di FilteredEntitiesId
        /// </summary>
        List<Guid> _foundEntitiesId = null;
        public List<Guid> FoundEntitiesId
        {
            get { return _foundEntitiesId; }
            set
            {
                _foundEntitiesId = value;

            }
        }

        int _currentEntityIndexFound = 0;

        public Guid CurrentSearchedEntityId
        {
            get { return (_currentEntityIndexFound < 0)? Guid.Empty : _foundEntitiesId[_currentEntityIndexFound]; }
        }

        public ICommand SearchNextCommand
        {
            get
            {
                return new CommandHandler(() => this.SearchNext(false));
            }
        }

        //public bool IsSearchNextEnabled
        //{
        //    get
        //    {
        //        if (!IsFilterApplied() && !IsSearchApplied())
        //            return false;

        //        if (_foundEntitiesId == null)
        //            return false;

        //        if (!_foundEntitiesId.Any())
        //            return false;

        //        return true;
        //    }
        //}

        public Guid CurrentEntityFoundId
        {
            get
            {
                if (_foundEntitiesId != null && _foundEntitiesId.Any() && _currentEntityIndexFound >=0)
                    return _foundEntitiesId[_currentEntityIndexFound];
                else
                    return Guid.Empty;
            }
        }

        public void SearchNext(bool initPositon = true)
        {


            if (_foundEntitiesId == null || !_foundEntitiesId.Any())
                return;

            int startingIndex = Master.FilteredIndexOf(Master.SelectedEntityId);

            if (initPositon == false)
            {
                ////if (!initPositon)
                startingIndex++;

                if (startingIndex >= Master.FilteredEntitiesId.Count)
                    startingIndex = 0;


                //cerca dall'entità correntemente selezionata fino alla fine
                for (int i = startingIndex; i < Master.FilteredEntitiesId.Count; i++)
                {
                    _currentEntityIndexFound = _foundEntitiesId.IndexOf(Master.FilteredEntitiesId[i]);
                    if (_currentEntityIndexFound >= 0)
                        break;
                }

                //cerca dall'inizio fino all'entità correntemente selezionata
                if (_currentEntityIndexFound < 0)
                {
                    for (int i = 0; i < startingIndex; i++)
                    {
                        _currentEntityIndexFound = _foundEntitiesId.IndexOf(Master.FilteredEntitiesId[i]);
                        if (_currentEntityIndexFound >= 0)
                            break;
                    }
                }
            }
            else
            {
                _currentEntityIndexFound = 0;
            }


            //if (!initPositon)
            //{
            if (!initPositon)
                    Master.CheckedEntitiesId.Clear();

                Master.SelectEntityById(_foundEntitiesId[_currentEntityIndexFound]);
            //}

            UpdateIsSearchEnabled();

        }

        internal void UpdateCurrentEntityIndexFound(Guid selectedEntityId)
        {
            if (selectedEntityId == null || selectedEntityId == Guid.Empty)
            {
                _currentEntityIndexFound = 0;
            }
            else
            {
                _currentEntityIndexFound = _foundEntitiesId.IndexOf(selectedEntityId);
            }
            UpdateIsSearchEnabled();
        }

        public ICommand SearchPreviousCommand
        {
            get
            {
                return new CommandHandler(() => this.SearchPrevious());
            }
        }

        public bool IsSearchIteratorEnabled
        {
            get
            {


                //if (!IsFilterApplied() && !IsSearchApplied())
                //    return false;

                //if (_foundEntitiesId == null)
                //    return false;

                //if (!_foundEntitiesId.Any())
                //    return false;

                return true;
            }
        }

        public void SearchPrevious(/*bool select = true*/)
        {
            if (_foundEntitiesId == null ||  !_foundEntitiesId.Any())
                return;

            int startingIndex = Master.FilteredIndexOf(Master.SelectedEntityId);

            if (startingIndex >= 0)
            {
                
                startingIndex--;
                if (startingIndex < 0) startingIndex = Master.FilteredEntitiesId.Count - 1;

                //cerca dall'entità correntemente selezionata fino all'inizio
                for (int i = startingIndex; i >= 0; i--)
                {
                    _currentEntityIndexFound = _foundEntitiesId.IndexOf(Master.FilteredEntitiesId[i]);
                    if (_currentEntityIndexFound >= 0)
                        break;
                }

                //cerca dalla fine fino all'entità correntemente selezionata
                if (_currentEntityIndexFound < 0)
                {
                    for (int i = Master.FilteredEntitiesId.Count - 1; i > startingIndex; i--)
                    {
                        _currentEntityIndexFound = _foundEntitiesId.IndexOf(Master.FilteredEntitiesId[i]);
                        if (_currentEntityIndexFound >= 0)
                            break;
                    }
                }
            }
            else
            {
                _currentEntityIndexFound = _foundEntitiesId.Count - 1;
            }

            //if (select)
            //{
                Master.CheckedEntitiesId.Clear();
                Master.SelectEntityById(_foundEntitiesId[_currentEntityIndexFound]);
            //}

            UpdateIsSearchEnabled();
        } 

        public void UpdateIsSearchEnabled()
        {
            RaisePropertyChanged(GetPropertyName(() => IsSearchIteratorEnabled));
            RaisePropertyChanged(GetPropertyName(() => SearchResult));
        }

        /// <summary>
        /// Aggiorna la lista master facendo vedere tutte le entità che sono state trovate
        /// </summary>
        /// <param name="foundEntities"></param>
        public async void UpdateSearch(List<Guid> foundEntities)
        {


            FoundEntitiesId = foundEntities;


            if (foundEntities.Any())
                _currentEntityIndexFound = 0;
            else
                _currentEntityIndexFound = -1;

            if (IsSearchApplied() && _foundEntitiesId != null && _foundEntitiesId.Any())
            {
                //Master.ShowEntities(foundEntities);//viene lanciato solo su find
            }


            UpdateIsSearchEnabled();
        }
        


        /// <summary>
        /// i/n
        /// </summary>
        public string SearchResult
        {
            get
            {
                string str = "";
                if (_foundEntitiesId != null)
                {
                    str = "(" + (_currentEntityIndexFound + 1).ToString() + "/" + _foundEntitiesId.Count.ToString() + ")";
                    
                }
                else
                    str = "(0/0)";
                return str;
            }
        }


#endregion Search


        //bool _synchronizeFilterButtonVisible = false;
        //public bool SynchronizeFilterButtonVisible
        //{
        //    get { return _synchronizeFilterButtonVisible; }
        //    set { SetProperty(ref _synchronizeFilterButtonVisible, value);}
        //}


        ///// <summary>
        ///// Vede se visualizzare il pulsante di sincronizzazione del filtro
        ///// </summary>
        ///// <param name="codiceAttributo">null significa tutti gli attributi</param>
        //public void Update(string codiceAttributo)
        //{
        //    if (Items.Any())
        //    {
        //        if (codiceAttributo == null)
        //            SynchronizeFilterButtonVisible = true;

        //        AttributoFilterView attFilterView = Items.FirstOrDefault(item => item.Attributo.Codice == codiceAttributo);
        //        if (attFilterView != null)
        //        {
        //            SynchronizeFilterButtonVisible = true;
        //        }
        //    }
        //}



        //public FilterData GetData()
        //{
        //    FilterData data = new FilterData();

        //    data.IsFilterApplied = IsFilterApplied();
        //    data.IsSearchApplied = IsSearchApplied();

        //    foreach (AttributoFilterView attFilterView in Items)
        //    {
        //        AttributoFilterData attFilterData = new AttributoFilterData()
        //        {
        //            CodiceEntity = attFilterView.Attributo.EntityType.Codice,
        //            CodiceAttributo = attFilterView.Attributo.Codice,
        //            IsAllChecked = attFilterView.IsAllChecked,
        //            TextSearched = attFilterView.TextSearched,
        //            CheckedValori = attFilterView.CheckedValori,
        //            IsFiltroAttivato = attFilterView.IsFiltroAttivato,
        //        };

        //        data.Items.Add(attFilterData);
        //    }
        //    return data;
        //}

        public string ItemsCount { get => string.Format("{0}", Items.Count); }

        public bool IsItemsCountVisible { get => Items.Count > 0; }

        private void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ItemsCount));
            RaisePropertyChanged(GetPropertyName(() => IsItemsCountVisible));
            RaisePropertyChanged(GetPropertyName(() => AnyFilter));
            RaisePropertyChanged(GetPropertyName(() => IsFiltroAttivato));
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

        public void ClearNextAttributiFilter(int filtroAttIndex)
        {
            //Azzero le ricerche/filtri attributi a valle effettuati precedentemente
            if (filtroAttIndex >= 0)
            {
                for (int i = filtroAttIndex; i < Master.RightPanesView.FilterView.Items.Count; i++)
                {
                    AttributoFilterData attFilterData = Master.RightPanesView.FilterView.Data.Items[i] as AttributoFilterData;
                    if (attFilterData != null)
                        attFilterData.FoundEntitiesId = null;
                }
            }
        }

    }

    public class ValoreFilterViewComparer : IComparer<ValoreFilterDetailView>
    {
        public int Compare(ValoreFilterDetailView x, ValoreFilterDetailView y)
        {
            if (x.Valore.GreaterThan(y.Valore)) return 1;
            else if (x.Valore.ResultEquals(y.Valore)) return 0;
            else return -1;
        }
    }

    

    public class AttributoFilterView : NotificationBase<AttributoFilterData>
    {
        EntitiesListMasterDetailView _master;
        EntitiesListMasterDetailView Master { get => _master; }


        public AttributoFilterView(EntitiesListMasterDetailView master, AttributoFilterData data) : base(data)
        {
            _master = master;

            if (data.CodiceAttributo == BuiltInCodes.Attributo.TemporaryFilterByIds)
            {
                //_nome = LocalizationProvider.GetString("SelezioneDaModello3d");
                _nome = LocalizationProvider.GetString("AggregatoCalcolato");
                //_itemTextIndex = 0;
            }
            else
            {
                Attributo = Master.EntityType.Attributi[data.CodiceAttributo];
                _nome = Attributo.Etichetta;

                //if (Attributo.ValoreDefault.ItemTextCount() > 0)
                //    _itemTextIndex = 0;
            }
        }
        public Attributo Attributo { get; set; } = null;

        AttributoFilterData Data { get => This; }

        ///// <summary>
        ///// Valori visualizzati nella list
        ///// </summary>
        //ObservableCollection<ValoreFilterDetailView> _valoriFilter = new ObservableCollection<ValoreFilterDetailView>();
        //public ObservableCollection<ValoreFilterDetailView> ValoriFilter
        //{
        //    get
        //    {
        //        return _valoriFilter;
        //    }

        //    set
        //    {
        //        SetProperty(ref _valoriFilter, value);
        //    }
        //}

        public HashSet<string> CheckedValori
        {
            get => This.CheckedValori;
        }

        public string TextSearched
        {
            get => This.TextSearched;
        }

        //string _textSearched;
        //public string TextSearched
        //{
        //    get { return _textSearched; }
        //    set
        //    {
        //        if (SetProperty(ref _textSearched, value))
        //        {
        //            bool searchToApply = false;
        //            if (IsAnyChecked)
        //                searchToApply = true;

        //            IsAllChecked = false;
        //            LoadValoriFiltro(true, null);
        //            if (searchToApply)
        //            {
        //                Master.RightSplitView.FilterView.CurrentAttributo.SetCheckedValuesAsString();
        //                Master.ApplyFilterAndSort();
        //                Master.Load();
        //            }

        //        }

        //    }
        //}

        //public ICommand SubmitEnterCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.SubmitEnter());
        //    }
        //}

        //void SubmitEnter()
        //{
        //    IsAllChecked = true;
        //}



        public void Find()
        {


            //SetProperty(This.IsFiltroAttivato, false, () => This.IsFiltroAttivato = false);
            //RaisePropertyChanged(GetPropertyName(() => IsFiltroAttivato));

            //Master.RightPanesView.FilterView.IsFiltroAttivato = false;

            SetCheckedValuesAsString();

            Master.RightPanesView.FilterView.FindOrFilter();

            //Guid selectedEntityIdPrec = Master.SelectedEntityId;

            //Master.ApplyFilterAndSort(null, false);

            //Master.ShowEntities(Master.RightPanesView.FilterView.FoundEntitiesId);


            //Master.CheckedEntitiesId = new HashSet<Guid>(Master.RightPanesView.FilterView.FoundEntitiesId);
            //Master.CheckedEntitiesId.UnionWith(Master.FilteredDescendantsId(Master.RightPanesView.FilterView.FoundEntitiesId));

            //Guid selectedEntityId = Guid.Empty;
            //if (Master.RightPanesView.FilterView.FoundEntitiesId.Contains(selectedEntityIdPrec))
            //    selectedEntityId = selectedEntityIdPrec;


            //if (selectedEntityId == Guid.Empty)//perchè non è stato fatto l'updateCache da SelectEntityById
            //    Master.UpdateCache();
            //else
            //    Master.SelectEntityById(selectedEntityId);

            //if (Master.ModelActionsStack != null)
            //    Master.ModelActionsStack.OnViewSettingsChanged();

            //Master.WindowService.ShowWaitCursor(false);
        }

        public void Filter()
        {
            SetCheckedValuesAsString();
            Master.RightPanesView.FilterView.FindOrFilter();


        }

        public bool IsFiltroAttivato
        {
            get { return This.IsFiltroAttivato; }
            set
            {
                if (SetProperty(This.IsFiltroAttivato, value, () => This.IsFiltroAttivato = value))
                {

                    //if (This.IsFiltroAttivato)
                    //{
                    //    Filter();
                    //}
                    //else
                    //{
                    //    Find();
                    //}
                }
            }
        }


        public string EtichettaAttributo
        {
            get
            {
                return Nome;
            }
        }

        public ICommand ClearFiltroCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearFiltro());
            }
        }

        public void ClearFiltro()
        {
            int filtroAttIndex = Master.RightPanesView.FilterView.Items.IndexOf(this);
            Master.RightPanesView.FilterView.ClearNextAttributiFilter(filtroAttIndex);

            Master.RightPanesView.FilterView.CurrentAttributo = null;

            Master.RightPanesView.FilterView.Data.Items.Remove(This);
            Master.RightPanesView.FilterView.Items.Remove(this);

            Master.RightPanesView.FilterView.FindOrFilter();

            //Master.ApplyFilterAndSort(Master.SelectedEntityId, false);

            //Master.UpdateCache();

            //if (Master.ModelActionsStack != null)
            //    Master.ModelActionsStack.OnViewSettingsChanged();


        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            if (Attributo != null)
            {
                int index = Master.RightPanesView.FilterView.Items.IndexOf(this);


                Master.RightPanesView.FilterView.Load(Attributo, index);
                Master.RightPanesView.FilterView.SearchNext();
            }

        }
    

        public bool IsValid()
        {
            return This.IsValid();
        }

        string _selectedValuesAsString = "";
        public string SelectedValuesAsString
        {
            get { return _selectedValuesAsString; }
            set { SetProperty(ref _selectedValuesAsString, value); }
        }


        string _selectedValuesAsStringTooltip = "";
        public string SelectedValuesAsStringTooltip
        {
            get { return _selectedValuesAsStringTooltip; }
            set { SetProperty(ref _selectedValuesAsStringTooltip, value); }
        }



        public void SetCheckedValuesAsString()
        {
            
            if (This.FilterType == FilterTypeEnum.Conditions)
            {
                SelectedValuesAsString = LocalizationProvider.GetString("_Condizione");
                SelectedValuesAsStringTooltip = string.Empty;
            }
            else if (This.IsAllChecked == true && (!Attributo.AllowValoriUnivoci || Data.CheckedValori.Count > 1))
            {
                if (This.TextSearched != null && This.TextSearched.Any())
                {
                    SelectedValuesAsString = This.TextSearched;
                    SelectedValuesAsStringTooltip = SelectedValuesAsString;
                }
                else if (Attributo.AllowValoriUnivoci && Data.CheckedValori.Count > 1)
                {
                    SelectedValuesAsString = LocalizationProvider.GetString(ValoreHelper.Multi);
                    
                    //tooltip
                    string str = "";
                    foreach (string s in Data.CheckedValori)
                    {
                        if (str.Any())
                            str += "\n";
                        str += s;
                    }
                    SelectedValuesAsStringTooltip = str;
                }

            }
            else if (Attributo != null)
            {
                Attributo sourceAtt = Master.GetSourceAttributoOf(Attributo);

                if (sourceAtt is AttributoRiferimento)
                {
                    if (Data.CheckedValori/*SourceCheckedValori*/.Count == 1)
                    {
                        //Guid id = new Guid(Data.SourceCheckedValori.First());
                        //if (id != Guid.Empty)
                        //{
                        //    Entity ent = Master.GetDataServiceEntityById(This.SourceEntityTypeKey, id);
                        //    SelectedValuesAsString = ent.AsString();
                        //}
                        //else
                        //    SelectedValuesAsString = "Nessuno";

                        string str = Data.CheckedValori/*SourceCheckedValori*/.First();
                        if (str == null || !str.Any())
                            SelectedValuesAsString = LocalizationProvider.GetString("Nessuno");
                        else
                            SelectedValuesAsString = str;

                        SelectedValuesAsStringTooltip = SelectedValuesAsString;

                    }
                    else if (Data.CheckedValori/*SourceCheckedValori*/.Count > 1)
                    {
                        LocalizationProvider.GetString(ValoreHelper.Multi);
                        //SelectedValuesAsString = StringResoucesTemp.GetResourceString("AttributoFilterView_Multi");

                        //tooltip
                        string str = "";
                        foreach (string s in Data.CheckedValori/*SourceCheckedValori*/)
                        {
                            //Guid id = new Guid(s);
                            //if (id != Guid.Empty)
                            //{
                            //    Entity ent = Master.GetDataServiceEntityById(This.SourceEntityTypeKey, id);
                            //    if (str.Any())
                            //        str += "\n";
                            //    str += ent.AsString();
                            //}
                            if (s != null)
                            {
                                if (str.Any())
                                    str += "\n";
                                str += s;
                            }
                            else
                            {
                                if (str.Any())
                                    str += "\n";
                                str += LocalizationProvider.GetString("Nessuno"); 
                            }

                        }
                        SelectedValuesAsStringTooltip = str;
                    }
                    else
                    {
                        SelectedValuesAsString = "";
                        SelectedValuesAsStringTooltip = null;
                    }
                }
                else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid
                    || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    SelectedValuesAsString = string.Format("{0} {1}", CheckedValori.Count, LocalizationProvider.GetString("selezionati"));
                    //if (CheckedValori.Count == 1)
                    //{
                    //    Guid id = new Guid(CheckedValori.First());
                    //    Entity ent = Master.EntitiesHelper.GetDataServiceEntityById(sourceAtt.GuidReferenceEntityTypeKey, id);
                    //    SelectedValuesAsStringTooltip = Master.EntitiesHelper.GetEntityTextIdentity(ent, true, true);
                    //}
                    //else
                    //    SelectedValuesAsStringTooltip = string.Empty;
                    List<Entity> ents = null;
                    HashSet<Guid> ids = new HashSet<Guid>(CheckedValori.Select(item => new Guid(item)));
                    string tooltipStr = string.Empty;

                    EntityType sourceEntityType = Master.DataService.GetEntityType(sourceAtt.GuidReferenceEntityTypeKey);
                    
                    if (sourceEntityType.IsTreeMaster)
                    {
                        List<TreeEntityMasterInfo> allTreeEntsInfo = Master.DataService.GetFilteredTreeEntities(sourceAtt.GuidReferenceEntityTypeKey, null, null, out _);
                        //entità filtrate con struttura
                        List<TreeEntityMasterInfo> filteredTreeEntsInfo = EntitiesHelper.TreeFilterById(allTreeEntsInfo, ids);

                        ents = Master.DataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, filteredTreeEntsInfo.Select(item => item.Id));
                    }
                    else
                    {
                        ents = Master.DataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, ids);
                    }

                    if (ids.Contains(Guid.Empty))
                        tooltipStr += string.Format("{0}{1}", LocalizationProvider.GetString("_Nessuno"), "\n");

                    
                    //string spacesStr = "                                          ";
                    foreach (Entity ent in ents)
                    {
                        string indent = string.Empty;
                        TreeEntity treeEnt = ent as TreeEntity;
                        if (treeEnt != null)
                            indent = "\n" + new string(' ', treeEnt.Depth * 3);// spacesStr.Substring(0, treeEnt.Depth*3);//space string

                        tooltipStr += string.Format("{0}{1}",indent, ent.ToUserIdentity(UserIdentityMode.Nothing));
                        //tooltipStr += "\n";
                    }
                    SelectedValuesAsStringTooltip = tooltipStr;


                }
                else
                {
                    if (Data.CheckedValori.Count == 1)
                    {
                        SelectedValuesAsString = CheckedValori.First();
                        SelectedValuesAsStringTooltip = SelectedValuesAsString;
                    }
                    else if (Data.CheckedValori.Count > 1)
                    {
                        SelectedValuesAsString = LocalizationProvider.GetString(ValoreHelper.Multi);
                        //SelectedValuesAsString = StringResoucesTemp.GetResourceString("AttributoFilterView_Multi");

                        //tooltip
                        string str = "";
                        foreach (string s in Data.CheckedValori)
                        {
                            if (str.Any())
                                str += "\n";
                            str += s;
                        }
                        SelectedValuesAsStringTooltip = str;

                    }
                    else if (Data.CheckedValori.Count == 0 && Data.IsAllChecked == true && Data.TextSearched != null && Data.TextSearched.Any())
                    {
                        SelectedValuesAsString = TextSearched;
                        SelectedValuesAsStringTooltip = TextSearched;
                    }
                    else
                    {
                        SelectedValuesAsString = "";
                        SelectedValuesAsStringTooltip = null;
                    }
                }
            }
            else
            {
                SelectedValuesAsString = string.Empty;
            }


        }

        string _nome;
        public string Nome
        {
            get { return _nome; }
        }

        public bool AllowValoriUnivoci
        {
            get
            {
                if (Attributo != null)
                    return Attributo.AllowValoriUnivoci;

                return false;
            }

        }

        public string GetCodice()
        {
            if (Attributo != null)
                return Attributo.Codice;
            else
                return BuiltInCodes.Attributo.TemporaryFilterByIds;


        }

        internal void SetData(AttributoFilterData attFilterData)
        {
            This = attFilterData;
        }
    }


}