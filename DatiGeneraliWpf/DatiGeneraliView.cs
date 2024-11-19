using _3DModelExchange;
using CommonResources;
using Commons;
using ContattiWpf.View;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatiGeneraliWpf.View
{
    public class DatiGeneraliView : NotificationBase, SectionItemTemplateView
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        public Dictionary<string, CalculatorFunction> CalculatorFunctions = null;
        //public ViewSettings ViewSettings { get; set; } = null;

        Dictionary<int, SectionItemTemplateView> _templatesView = new Dictionary<int, SectionItemTemplateView>();

        public event EventHandler ViewUpdated;

        public ContattiView ContattiView { get; set; }
        public InfoProgettoView InfoProgettoView { get; set; }
        public StiliProgettoView StiliProgettoView { get; set; }
        public UnitaMisuraView UnitaMisuraView { get; set; }
        public VariabiliView VariabiliView { get; set; }
        public AllegatiView AllegatiView { get; set; }
        public TagView TagView { get; set; }

        private DatiGeneraliHierarchicalItemsSource _itemsSource = new DatiGeneraliHierarchicalItemsSource();
        public DatiGeneraliHierarchicalItemsSource ItemsSource
        {
            get { return _itemsSource; }
            set { _itemsSource = value; }
        }

        public DatiGeneraliView()
        {
        }

        public void Init()
        {
            ViewSettings viewSettings = DataService.GetViewSettings();
            EntityTypeViewSettings entTypeviewSettings = null;

            CreateTemplateViews();

            //InfoProgetto
            InfoProgettoView.CalculatorFunctions.Clear();
            InfoProgettoView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            InfoProgettoView.CalculatorFunctions.Add(CalculatorFunctions[InfCalculatorFunction.Name]);
            InfoProgettoView.DataService = DataService;
            InfoProgettoView.WindowService = WindowService;
            InfoProgettoView.ModelActionsStack = ModelActionsStack;
            InfoProgettoView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(InfoProgettoItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[InfoProgettoItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            InfoProgettoView.Init(entTypeviewSettings);


            //Contatti
            ContattiView.CalculatorFunctions.Clear();
            ContattiView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            ContattiView.DataService = DataService;
            ContattiView.WindowService = WindowService;
            ContattiView.ModelActionsStack = ModelActionsStack;
            ContattiView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(ContattiItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[ContattiItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            ContattiView.Init(entTypeviewSettings);


            //Stili
            StiliProgettoView.CalculatorFunctions.Clear();
            StiliProgettoView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            StiliProgettoView.DataService = DataService;
            StiliProgettoView.WindowService = WindowService;
            StiliProgettoView.ModelActionsStack = ModelActionsStack;
            StiliProgettoView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(StiliItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[StiliItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            StiliProgettoView.Init(entTypeviewSettings);

            //Unita di Misura
            UnitaMisuraView.DataService = DataService;
            UnitaMisuraView.WindowService = WindowService;
            UnitaMisuraView.ModelActionsStack = ModelActionsStack;
            UnitaMisuraView.MainOperation = MainOperation;
            UnitaMisuraView.Init();

            //Variabili
            VariabiliView.CalculatorFunctions.Clear();
            VariabiliView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            VariabiliView.CalculatorFunctions.Add(CalculatorFunctions[VarCalculatorFunction.Name]);
            VariabiliView.DataService = DataService;
            VariabiliView.WindowService = WindowService;
            VariabiliView.ModelActionsStack = ModelActionsStack;
            VariabiliView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(VariabiliItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[VariabiliItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            VariabiliView.Init(entTypeviewSettings);

            //Allegati
            AllegatiView.CalculatorFunctions.Clear();
            AllegatiView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            AllegatiView.DataService = DataService;
            AllegatiView.WindowService = WindowService;
            AllegatiView.ModelActionsStack = ModelActionsStack;
            AllegatiView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(AllegatiItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[AllegatiItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            AllegatiView.Init(entTypeviewSettings);

            //Tag
            TagView.CalculatorFunctions.Clear();
            TagView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
            TagView.DataService = DataService;
            TagView.WindowService = WindowService;
            TagView.ModelActionsStack = ModelActionsStack;
            TagView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(TagItemType.CreateKey()))
                entTypeviewSettings = viewSettings.EntityTypes[TagItemType.CreateKey()];
            else
                entTypeviewSettings = null;
            TagView.Init(entTypeviewSettings);


            CurrentSectionItem = ItemsSource[0];
            
        }

        void CreateTemplateViews()
        {
            _templatesView.Clear();

            _templatesView.Add((int)DatiGeneraliSectionItemsId.DatiGenerali, this);
            _templatesView.Add((int)DatiGeneraliSectionItemsId.InfoProgetto, new InfoProgettoView());
            _templatesView.Add((int)DatiGeneraliSectionItemsId.ContattiProgetto, new ContattiProgettoView());
            _templatesView.Add((int)DatiGeneraliSectionItemsId.StiliProgetto, new StiliProgettoView());
            _templatesView.Add((int)DatiGeneraliSectionItemsId.UnitaDiMisura, new UnitaMisuraView());
            _templatesView.Add((int)DatiGeneraliSectionItemsId.Variabili, new VariabiliView());
            _templatesView.Add((int)DatiGeneraliSectionItemsId.Allegati, new AllegatiView());
            _templatesView.Add((int)DatiGeneraliSectionItemsId.Tag, new TagView());
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
                ObservableCollection<SectionItemView> childItems =  FindCurrentSectionItems(item.SectionItems, sectionId);
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

        public int Code { get => (int) DatiGeneraliSectionItemsId.DatiGenerali; }

        protected void OnViewUpdated(EventArgs e)
        {
            ViewUpdated?.Invoke(this, e);
        }

        public void Clear()
        {
            ContattiView.Clear();
            InfoProgettoView.Clear();
            StiliProgettoView.Clear();
            //UnitaMisuraView.Clear();
        }

    }




    public class DatiGeneraliHierarchicalItemsSource : ObservableCollection<SectionItemView>
    {

        //ObservableCollection<SectionItemView> _sectionItems = new ObservableCollection<SectionItemView>();
        //public ObservableCollection<SectionItemView> SectionItems { get => _sectionItems; }

        public DatiGeneraliHierarchicalItemsSource()
        {
            ObservableCollection<SectionItemView> sectionItems = new ObservableCollection<SectionItemView>();

            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.InfoProgetto)
            {
                Title = LocalizationProvider.GetString("Informazioni"),
                HierarchyText = LocalizationProvider.GetString("Informazioni"),
                Icon ="\ue0a7",
                Description = LocalizationProvider.GetString("Dati identificativi del progetto"),
            });
            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.StiliProgetto)
            {
                Title = LocalizationProvider.GetString("Stili"),
                HierarchyText = LocalizationProvider.GetString("Stili"),
                Icon = "\ue0be",
                Description = LocalizationProvider.GetString("Stili applicati nel progetto"),
            });
            //sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.Dashboard)
            //{
            //    Title = "Dashboard",
            //    HierarchyText = "Dashboard",
            //    Description = "yyy"
            //});
            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.ContattiProgetto)
            {
                Title = LocalizationProvider.GetString("Contatti"),
                HierarchyText = LocalizationProvider.GetString("Contatti"),
                Icon= "\ue053",
                Description = LocalizationProvider.GetString("Impostazione degli attributi relativi ai soggetti coinvolti nel progetto"),
            });
            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.UnitaDiMisura)
            {
                Title = LocalizationProvider.GetString("UnitaDiMisura"),
                HierarchyText = LocalizationProvider.GetString("UnitaDiMisura"),
                Icon = "\ue0a8",
                Description = LocalizationProvider.GetString("Impostazioni delle unità di misura e dei formati numerici relativi alle quantità"),
            });

            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.Variabili)
            {
                Title = LocalizationProvider.GetString("Variabili"),
                HierarchyText = LocalizationProvider.GetString("Variabili"),
                Icon = "\ue115",
                Description = LocalizationProvider.GetString("Variabili numeriche utilizzabili nelle sezioni del programma"),
            });


            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.Allegati)
            {
                Title = LocalizationProvider.GetString("Allegati"),
                HierarchyText = LocalizationProvider.GetString("Allegati"),
                Icon = "\ue119",
                Description = LocalizationProvider.GetString("Allegati del progetto"),
            });

            sectionItems.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.Tag)
            {
                Title = LocalizationProvider.GetString("Tag"),
                HierarchyText = LocalizationProvider.GetString("Tag"),
                Icon = "\ue0a9",
                Description = LocalizationProvider.GetString("Tag del progetto"),
            });


            this.Add(new SectionItemView((int)DatiGeneraliSectionItemsId.DatiGenerali,
                                            sectionItems.ToArray())
            {
                Title = LocalizationProvider.GetString("DatiGenerali"),
                HierarchyText = string.Empty,
                Icon = "\ue07a",
            });
        }
    }

    public enum DatiGeneraliSectionItemsId
    {
        Nothing = 0,
        DatiGenerali,
        InfoProgetto,
        Dashboard,
        ContattiProgetto,
        UnitaDiMisura,
        StiliProgetto,
        Variabili,
        Allegati,
        Tag,
    }

}
