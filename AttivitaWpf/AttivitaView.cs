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

namespace AttivitaWpf.View
{
    public class AttivitaView : NotificationBase, SectionItemTemplateView
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        public Dictionary<string, CalculatorFunction> CalculatorFunctions = null;
        //public ViewSettings ViewSettings { get; set; } = null;

        public int Code { get => (int)AttivitaSectionItemsId.Attivita; }

        Dictionary<int, SectionItemTemplateView> _templatesView = new Dictionary<int, SectionItemTemplateView>();

        public event EventHandler ViewUpdated;

        public ElencoAttivitaView ElencoAttivitaView { get; set; }
        public GanttView GanttView { get; set; }
        public WBSView WBSView { get; set; }
        public CalendariView CalendariView { get; set; }

        private AttivitaHierarchicalItemsSource _itemsSource = new AttivitaHierarchicalItemsSource();
        public AttivitaHierarchicalItemsSource ItemsSource
        {
            get { return _itemsSource; }
            set { _itemsSource = value; }
        }
        public AttivitaView()
        {
        }

        public void Init()
        {
            ViewSettings viewSettings = DataService.GetViewSettings();
            EntityTypeViewSettings entTypeviewSettings = null;

            CreateTemplateViews();

            //ElencoAttivita
            ElencoAttivitaView.CalculatorFunctions.Clear();
            ElencoAttivitaView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            ElencoAttivitaView.DataService = DataService;
            ElencoAttivitaView.WindowService = WindowService;
            ElencoAttivitaView.ModelActionsStack = ModelActionsStack;
            ElencoAttivitaView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(ElencoAttivitaItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[ElencoAttivitaItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            ElencoAttivitaView.Init(entTypeviewSettings);

            //////////////////
            //WBS

            GanttView.DataService = DataService;
            GanttView.MainOperation = MainOperation;
            GanttView.WindowService = WindowService;
            GanttView.WBSView = WBSView;
           

            WBSView.CalculatorFunctions.Clear();
            WBSView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            WBSView.DataService = DataService;
            WBSView.WindowService = WindowService;
            WBSView.ModelActionsStack = ModelActionsStack;
            WBSView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(WBSItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[WBSItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            WBSView.GanttView = GanttView;

            GanttView.Init();
            WBSView.Init(entTypeviewSettings);
            //////////////////

            //Gantt
            //GanttView.DataService = DataService;
            //GanttView.MainOperation = MainOperation;
            //GanttView.WBSView = WBSView;
            //GanttView.Init();

            //Calendari
            CalendariView.CalculatorFunctions.Clear();
            CalendariView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            CalendariView.DataService = DataService;
            CalendariView.WindowService = WindowService;
            CalendariView.ModelActionsStack = ModelActionsStack;
            CalendariView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(CalendariItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[CalendariItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            CalendariView.Init(entTypeviewSettings);

            //
            CurrentSectionItem = ItemsSource[0];
        }

        void CreateTemplateViews()
        {
            _templatesView.Clear();

            _templatesView.Add((int)AttivitaSectionItemsId.Attivita, this);
            _templatesView.Add((int)AttivitaSectionItemsId.ElencoAttivita, new ElencoAttivitaView());
            //_templatesView.Add((int)AttivitaSectionItemsId.Gantt, new GanttView());
            _templatesView.Add((int)AttivitaSectionItemsId.WBS, new WBSView());
            _templatesView.Add((int)AttivitaSectionItemsId.Calendari, new CalendariView());

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
                //controllo chiave
                SectionItemView sectionItemView = value as SectionItemView;
                if (sectionItemView != null && sectionItemView.Id == (int) AttivitaSectionItemsId.WBS)
                {
                    string msg = null;
                    if (!LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_4D }, out msg))
                    {
                        MainOperation.ShowMessageBarView(msg);
                        return;
                    }
                }

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

        protected void OnViewUpdated(EventArgs e)
        {
            ViewUpdated?.Invoke(this, e);
        }

        public void Clear()
        {
            if (ElencoAttivitaView != null)
                ElencoAttivitaView.Clear();
            if (GanttView != null)
                GanttView.Clear();
            if (WBSView != null)
                WBSView.Clear();
            if (CalendariView != null)
                CalendariView.Clear();
        }

    }

    public class AttivitaHierarchicalItemsSource : ObservableCollection<SectionItemView>
    {


        public AttivitaHierarchicalItemsSource()
        {
            Init();
        }

        public void Init(SectionItemTemplateView owner = null)
        {
            ObservableCollection<SectionItemView> sectionItems = new ObservableCollection<SectionItemView>();

            sectionItems.Add(new SectionItemView((int)AttivitaSectionItemsId.WBS)
            {
                Title = LocalizationProvider.GetString("WBS_Gantt"),
                HierarchyText = LocalizationProvider.GetString("WBS_Gantt"),
                Icon = "\ue0e6",
                Description = LocalizationProvider.GetString("Struttura delle attività del progetto"),
            });

            //sectionItems.Add(new SectionItemView((int)AttivitaSectionItemsId.Gantt)
            //{
            //    Title = LocalizationProvider.GetString("Gantt"),
            //    HierarchyText = LocalizationProvider.GetString("Gantt"),
            //    Icon = "\ue0e7",// "\ue029",
            //    Description = LocalizationProvider.GetString("Diagramma di Gantt relativo alla WBS del progetto"),
            //});

            sectionItems.Add(new SectionItemView((int)AttivitaSectionItemsId.ElencoAttivita)
            {
                Title = LocalizationProvider.GetString("ElencoAttivita"),
                HierarchyText = LocalizationProvider.GetString("ElencoAttivita"),
                Icon = "\ue0e8",// "\ue029",
                Description = LocalizationProvider.GetString("Attivita utilizzate nel progetto"),
            });

            sectionItems.Add(new SectionItemView((int)AttivitaSectionItemsId.Calendari)
            {
                Title = LocalizationProvider.GetString("Calendari"),
                HierarchyText = LocalizationProvider.GetString("Calendari"),
                Icon = "\ue0f2",// "\ue029",
                Description = LocalizationProvider.GetString("Calendari"),
            });


            this.Add(new SectionItemView((int)AttivitaSectionItemsId.Attivita,
                                            sectionItems.ToArray())
            {
                Title = LocalizationProvider.GetString("Attivita"),
                HierarchyText = string.Empty,
                Icon = "\ue07a",
            });
        }
    }

    public enum AttivitaSectionItemsId
    {
        Nothing = 0,
        WBS,
        Gantt,
        ElencoAttivita,
        Attivita,
        Calendari
    }
}
