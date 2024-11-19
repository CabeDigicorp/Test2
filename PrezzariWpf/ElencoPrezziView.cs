using _3DModelExchange;
using CommonResources;
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

namespace PrezzariWpf.View
{
    public class ElencoPrezziView : NotificationBase, SectionItemTemplateView
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        public Dictionary<string, CalculatorFunction> CalculatorFunctions = null;
        //public ViewSettings ViewSettings { get; set; } = null;

        Dictionary<int, SectionItemTemplateView> _templatesView = new Dictionary<int, SectionItemTemplateView>();

        public event EventHandler ViewUpdated;

        public PrezzarioView PrezzarioView { get; set; }
        public CapitoliView CapitoliView { get; set; }

        private ElencoPrezziHierarchicalItemsSource _itemsSource = new ElencoPrezziHierarchicalItemsSource();
        public ElencoPrezziHierarchicalItemsSource ItemsSource
        {
            get { return _itemsSource; }
            set { _itemsSource = value; }
        }

        public ElencoPrezziView()
        {
        }

        public void Init()
        {
            ViewSettings viewSettings = DataService.GetViewSettings();
            EntityTypeViewSettings entTypeViewSettings = null;

            CreateTemplateViews();

            //Capitoli
            CapitoliView.CalculatorFunctions.Clear();
            CapitoliView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            CapitoliView.DataService = DataService;
            CapitoliView.WindowService = WindowService;
            CapitoliView.ModelActionsStack = ModelActionsStack;
            CapitoliView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(CapitoliItemType.CreateKey()))
                entTypeViewSettings = viewSettings.EntityTypes[CapitoliItemType.CreateKey()];
            else
                entTypeViewSettings = null;
            CapitoliView.Init(entTypeViewSettings);


            //Prezzario
            //PrezzarioView = _templatesView[(int)ElencoPrezziSectionItemsId.Prezzario] as PrezzarioView;
            PrezzarioView.CalculatorFunctions.Clear();
            PrezzarioView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            PrezzarioView.CalculatorFunctions.Add(CalculatorFunctions[EPCalculatorFunction.Name]);
            PrezzarioView.DataService = DataService;
            PrezzarioView.WindowService = WindowService;
            PrezzarioView.ModelActionsStack = ModelActionsStack;
            PrezzarioView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(PrezzarioItemType.CreateKey()))
                entTypeViewSettings = viewSettings.EntityTypes[PrezzarioItemType.CreateKey()];
            else
                entTypeViewSettings = null;
            PrezzarioView.Init(entTypeViewSettings);


            CurrentSectionItem = ItemsSource[0];
        }

        void CreateTemplateViews()
        {
            _templatesView.Clear();

            _templatesView.Add((int)ElencoPrezziSectionItemsId.ElencoPrezzi, this);
            _templatesView.Add((int)ElencoPrezziSectionItemsId.Capitoli, new CapitoliView());
            _templatesView.Add((int)ElencoPrezziSectionItemsId.Prezzario, new PrezzarioView());
        }


        /// <summary>
        /// Sectionitems List current Item
        /// </summary>
        object _currentSectionItem = null;
        public object CurrentSectionItem
        {
            get
            {
                return _currentSectionItem;
            }
            set
            {
                if (SetProperty(ref _currentSectionItem, value))
                {
                    SectionItemView currItem = _currentSectionItem as SectionItemView;

                    if (currItem != null)
                    {
                        if (_templatesView.ContainsKey(currItem.Id))
                            CurrentTemplateView = _templatesView[currItem.Id];

                        OnViewUpdated(new EventArgs());
                        RaisePropertyChanged(GetPropertyName(() => SiblingSectionItems));
                    }
                }
            }
        }

        public ObservableCollection<SectionItemView> SiblingSectionItems
        {
            get
            {
                List<SectionItemView> siblingSectionitems = new List<SectionItemView>();
                SectionItemView currentSection = CurrentSectionItem as SectionItemView;
                if (currentSection != null)
                {
                    ObservableCollection<SectionItemView> collectionOwner = FindCurrentSectionItems(ItemsSource[0].SectionItems, currentSection.Id);
                    if (collectionOwner != null)
                    {
                        siblingSectionitems = new List<SectionItemView>(collectionOwner);
                        siblingSectionitems.RemoveAll(item => item.Id == currentSection.Id);
                    }
                }
                return new ObservableCollection<SectionItemView>(siblingSectionitems);
            }
        }

        private ObservableCollection<SectionItemView> FindCurrentSectionItems(ObservableCollection<SectionItemView> items, int sectionId)
        {

            if (items.FirstOrDefault(item => item.Id == sectionId) != null)
                return items;

            foreach (var item in items)
            {
                ObservableCollection<SectionItemView> childItems = FindCurrentSectionItems(item.SectionItems, sectionId);
                if (childItems != null)
                    return childItems;
            }

            return null;
        }

        /// <summary>
        /// HierarchyNavigator SelectedItem
        /// </summary>
        public object SelectedItem
        {
            get
            {
                return _currentSectionItem;
            }
            set
            {
                CurrentSectionItem = value;
            }
        }

        SectionItemTemplateView _currentTemplateView = null;
        public SectionItemTemplateView CurrentTemplateView
        {
            get
            {
                return _currentTemplateView;
            }
            set
            {
                if (SetProperty(ref _currentTemplateView, value))
                {
                }
            }
        }

        public int Code { get => (int) ElencoPrezziSectionItemsId.ElencoPrezzi; }

        protected void OnViewUpdated(EventArgs e)
        {
            ViewUpdated?.Invoke(this, e);
        }

        public void Clear()
        {
            PrezzarioView.Clear();
            CapitoliView.Clear();
        }

    }




    public class ElencoPrezziHierarchicalItemsSource : ObservableCollection<SectionItemView>
    {

        public ElencoPrezziHierarchicalItemsSource()
        {
            ObservableCollection<SectionItemView> sectionItems = new ObservableCollection<SectionItemView>();

            sectionItems.Add(new SectionItemView((int)ElencoPrezziSectionItemsId.Capitoli)
            {
                Title = LocalizationProvider.GetString("Capitoli"),
                HierarchyText = LocalizationProvider.GetString("Capitoli"),
                Icon = "\ue0c2",
                Description = LocalizationProvider.GetString("Suddivisioni di articoli del progetto"),
            });

            sectionItems.Add(new SectionItemView((int)ElencoPrezziSectionItemsId.Prezzario)
            {
                Title = LocalizationProvider.GetString("Articoli"),
                HierarchyText = LocalizationProvider.GetString("Articoli"),
                Icon = "\ue0c1",// "\ue029",
                Description = LocalizationProvider.GetString("Elenco degli articoli del progetto utilizzabili nel computo"),
            });


            this.Add(new SectionItemView((int)ElencoPrezziSectionItemsId.ElencoPrezzi,
                                            sectionItems.ToArray())
            {
                Title = LocalizationProvider.GetString("ElencoPrezzi"),
                HierarchyText = string.Empty,
                Icon = "\ue07a",
            });
        }
    }

    public enum ElencoPrezziSectionItemsId
    {
        Nothing = 0,
        ElencoPrezzi,
        Prezzario,
        Capitoli,
        
    }

}
