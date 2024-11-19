
using Commons;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MasterDetailModel;
using System.Windows;
using CommonResources;
using System.Collections.Generic;

namespace MasterDetailView
{

    public class SortView : NotificationBase, RightPaneView
    {
        //public static SortView This { get; set; }

        protected EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public SortData Data { get; set; }

        public SortView(EntitiesListMasterDetailView master)
        {
            //This = this;
            _master = master;

            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUI();
        }

        ObservableCollection<AttributoSortView> _items = new ObservableCollection<AttributoSortView>();
        public ObservableCollection<AttributoSortView> Items
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


        AttributoSortView NewAttributoSort { get; set; } = null;

        AttributoSortView _currentAttributo;
        public AttributoSortView CurrentAttributo
        {
            get { return _currentAttributo; }
            set
            {
                if (SetProperty(ref _currentAttributo, value))
                {
                    foreach (AttributoSortView att in Items)
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
                Data = new SortData();

            Items.Clear();
            Data.Items.Clear();

            Master.IsMultipleModify = false;

            foreach (AttributoSortData sortAtt in viewSettings.Sorts)
            {
                if (Master.EntityType.Attributi.ContainsKey(sortAtt.CodiceAttributo))
                {
                    AttributoSortData clone = sortAtt.Clone();

                    Items.Add(new AttributoSortView(Master, clone));
                    Data.Items.Add(clone);
                }
            }
        }

        public void Load(Attributo tipoAtt, AttributoSortView target = null)
        {
            if (!tipoAtt.AllowSort)
            {
                Master.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileOrdinarePerQuestoAttributo"));
                return;
            }

            Master.IsMultipleModify = false;

            //nessun ordinamento per lo stesso attributo
            //if (Items.FirstOrDefault(item => item.Attributo.Codice == tipoAtt.Codice) != null)
            //    return;

            //in ogni caso elimino il precedente e lo reinserisco nella corretta posizione
            var found = Items.FirstOrDefault(item => item.Attributo.Codice == tipoAtt.Codice);
            if (found != null)
                Items.Remove(found);

            NewAttributoSort = new AttributoSortView(Master, new AttributoSortData() { CodiceAttributo = tipoAtt.Codice, IsOrdinamentoInverso = false });

            int targetIndex = -1;
            if (target != null)
                targetIndex = Items.IndexOf(target);

            if (targetIndex >= 0)
                Items.Insert(targetIndex, NewAttributoSort);
            else
                Items.Add(NewAttributoSort);

            
            CurrentAttributo = NewAttributoSort;
          
            Master.IsSortPaneOpen = true;

            Master.Load();

            if (Master.ModelActionsStack != null)
                Master.ModelActionsStack.OnViewSettingsChanged();
        }
        public void UnLoad()
        {
            if (NewAttributoSort != null && NewAttributoSort.IsValid())
            {
                Items.Remove(NewAttributoSort);
                NewAttributoSort = null;
            }
            //IsClearAllSortVisible = Items.Any();

            //Master.ApplyFilterAndSort();
            Master.Load();

            if (Master.ModelActionsStack != null)
                Master.ModelActionsStack.OnViewSettingsChanged();
        }

        //bool _clearAllSortVisible = false;
        //public bool IsClearAllSortVisible
        //{
        //    get { return _clearAllSortVisible; }
        //    set { SetProperty(ref _clearAllSortVisible, value); }
        //}

        public bool IsClearAllSortVisible
        {
            get { return Items.Any(); }
        }

        internal void UpdateAttributi()
        {
            List<AttributoSortView> attToRemove = Items.Where(item => item.Attributo != null && !Master.EntityType.Attributi.ContainsKey(item.Attributo.Codice)).ToList();

            foreach (var item in attToRemove)
                item.ClearSort();
        }

        //bool _synchronizeSortButtonVisible = false;

        //public bool SynchronizeSortButtonVisible
        //{
        //    get { return _synchronizeSortButtonVisible; }
        //    set { SetProperty(ref _synchronizeSortButtonVisible, value); }
        //}

        public void Update(string codiceAttributo)
        {
            AttributoSortView attSortView = Items.FirstOrDefault(item => item.Attributo.Codice == codiceAttributo);
            if (attSortView != null)
            {
                //SynchronizeSortButtonVisible = true;
            }
        }

        //public ICommand SynchronizeSortCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.SynchronizeSort());
        //    }
        //}

        //public void SynchronizeSort()
        //{

        //    foreach (AttributoSortView attSortView in Items)
        //    {
        //        attSortView.Synchronize();
        //    }
        //    //Master.ApplyFilterAndSort();
        //    Master.Load();
        //}

        public ICommand ClearAllSortCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearAllSort());
            }
        }

        public void ClearAllSort(bool load = true)
        {
            Items.Clear();
            //IsClearAllSortVisible = false;

            if (load)
            {
                Master.Load();//Load permette di azzerare tutte le entità realizzate
                //IsClearAllSortVisible = false;

                if (Master.ModelActionsStack != null)
                    Master.ModelActionsStack.OnViewSettingsChanged();
            }

            Master.IsMultipleModify = false;

            UpdateUI();
        }

        //public SortData GetData()
        //{
        //    SortData data = new SortData();
        //    foreach (AttributoSortView attSortView in Items)
        //    {
        //        AttributoSortData attSortData = new AttributoSortData()
        //        {
        //            EntityTypeKey = attSortView.Attributo.EntityType.GetKey(),
        //            CodiceAttributo = attSortView.Attributo.Codice,
        //            IsOrdinamentoInverso = attSortView.IsOrdinamentoInverso
        //        };
        //        data.Items.Add(attSortData);
        //    }
        //    return data;
        //}

        public void CreateData()
        {
            if (Master.EntityType == null)
                return;

            SortData data = new SortData();
            data.EntityTypeKey = Master.EntityType.GetKey();
            foreach (AttributoSortView attSortView in Items)
            {
                AttributoSortData attSortData = new AttributoSortData()
                {
                    CodiceAttributo = attSortView.Attributo.Codice,
                    IsOrdinamentoInverso = attSortView.IsOrdinamentoInverso,
                };
                data.Items.Add(attSortData);
            }
            Data = data;
        }

        

        public string ItemsCount {get => string.Format("{0}", Items.Count);}

        public bool IsItemsCountVisible { get => Items.Count > 0; }

        private void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ItemsCount));
            RaisePropertyChanged(GetPropertyName(() => IsItemsCountVisible));
            RaisePropertyChanged(GetPropertyName(() => IsClearAllSortVisible));
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
    public class AttributoSortView : NotificationBase<AttributoSortData>
    {

        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }
        public AttributoSortView(EntitiesListMasterDetailView master, AttributoSortData data) : base(data)
        {
            _master = master;
            Attributo = Master.EntityType.Attributi[data.CodiceAttributo];
            _nome = Attributo.Etichetta;
        }
        public Attributo Attributo { get; set; }
        AttributoSortData Data { get => This; }

        public ICommand ClearSortCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearSort());
            }
        }

        public void ClearSort()
        {
            Master.RightPanesView.SortView.Items.Remove(this);

            //Master.ApplyFilterAndSort();
            //Master.UpdateCache();
            Master.Load();
            //Master.RightPanesView.SortView.IsClearAllSortVisible = Master.RightPanesView.SortView.Items.Any();

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
        public string Nome
        {
            get { return _nome; }
        }


        public bool IsOrdinamentoInverso
        {
            get { return This.IsOrdinamentoInverso; }
            set
            {
                if (SetProperty(This.IsOrdinamentoInverso, value, () => This.IsOrdinamentoInverso = value))
                {
                    Master.Load();

                    if (Master.ModelActionsStack != null)
                        Master.ModelActionsStack.OnViewSettingsChanged();
                }
            }
        }



        public bool IsValid()
        {
            return true;
        }

    }
}
        