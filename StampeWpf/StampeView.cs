using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace StampeWpf.View
{
    public class StampeView : NotificationBase, SectionItemTemplateView
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        //public ViewSettings ViewSettings { get; set; } = null;
        //IDataStampeService StampeService { get; set; }
        List<string> ListaSezioni { get; set; }

        Dictionary<int, SectionItemTemplateView> _templatesView = new Dictionary<int, SectionItemTemplateView>();

        public event EventHandler ViewUpdated;

        public DocumentiView DocumentiView { get; set; }
        public ReportView ReportView { get; set; }

        public AttivitaWpf.View.GanttView GanttView { get; set; }

        public FogliDiCalcoloWpf.FogliDiCalcoloView FogliDiCalcoloView { get; set; }

        private StampeHierarchicalItemsSource _itemsSource = new StampeHierarchicalItemsSource();
        public StampeHierarchicalItemsSource ItemsSource
        {
            get { return _itemsSource; }
            set { _itemsSource = value; }
        }

        public StampeView()
        {
        }

        public void Init()
        {
            ViewSettings viewSettings = DataService.GetViewSettings();
            EntityTypeViewSettings entTypeViewSettings = null;

            CreateTemplateViews();

            //Documenti
            DocumentiView.DataService = DataService;
            DocumentiView.WindowService = WindowService;
            DocumentiView.ModelActionsStack = ModelActionsStack;
            DocumentiView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(DocumentiItemType.CreateKey()))
                entTypeViewSettings = viewSettings.EntityTypes[DocumentiItemType.CreateKey()];
            else
                entTypeViewSettings = null;
            DocumentiView.GanttView = GanttView;
            DocumentiView.FogliDiCalcoloView = FogliDiCalcoloView;
            DocumentiView.Init(entTypeViewSettings);


            //Report
            ReportView.DataService = DataService;
            ReportView.WindowService = WindowService;
            ReportView.ModelActionsStack = ModelActionsStack;
            ReportView.MainOperation = MainOperation;
            if (viewSettings.EntityTypes.ContainsKey(ReportItemType.CreateKey()))
                entTypeViewSettings = viewSettings.EntityTypes[ReportItemType.CreateKey()];
            else
                entTypeViewSettings = null;
            ReportView.GanttView = GanttView;
            ReportView.FogliDiCalcoloView = FogliDiCalcoloView;
            ReportView.Init(entTypeViewSettings);


            CurrentSectionItem = ItemsSource[0];
        }

        void CreateTemplateViews()
        {
            _templatesView.Clear();

            _templatesView.Add((int)StampeSectionItemsId.Stampe, this);
            //_templatesView.Add((int)StampeSectionItemsId.Documenti, new DocumentiView());
            //_templatesView.Add((int)StampeSectionItemsId.Report, new ReportView());
            _templatesView.Add((int)StampeSectionItemsId.Documenti, DocumentiView);
            _templatesView.Add((int)StampeSectionItemsId.Report, ReportView);
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

        public int Code { get => (int)StampeSectionItemsId.Stampe; }

        protected void OnViewUpdated(EventArgs e)
        {
            ViewUpdated?.Invoke(this, e);
        }

        public void Clear()
        {
            DocumentiView.Clear();
            ReportView.Clear();
        }
        //public void Init()
        //{
        //    StampeService = DataService.GetStampeService();

        //    ListaSezioni = new List<string>()
        //    {
        //        LocalizationProvider.GetString("ElencoPrezzi"),
        //        LocalizationProvider.GetString("Computo")
        //    };

        //}

        public static void AddProperty(System.Dynamic.ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

    }

    public class StampeHierarchicalItemsSource : ObservableCollection<SectionItemView>
    {

        public StampeHierarchicalItemsSource()
        {
            ObservableCollection<SectionItemView> sectionItems = new ObservableCollection<SectionItemView>();

            sectionItems.Add(new SectionItemView((int)StampeSectionItemsId.Documenti)
            {
                Title = LocalizationProvider.GetString("Documenti"),
                HierarchyText = LocalizationProvider.GetString("Documenti"),
                Icon = "\ue0c7",
                Description = LocalizationProvider.GetString("Elenco dei documenti disponibili"),
            });

            sectionItems.Add(new SectionItemView((int)StampeSectionItemsId.Report)
            {
                Title = LocalizationProvider.GetString("Report"),
                HierarchyText = LocalizationProvider.GetString("Report"),
                Icon = "\ue0c6",
                Description = LocalizationProvider.GetString("Gestione report"),
            });

            this.Add(new SectionItemView((int)StampeSectionItemsId.Stampe,
                                            sectionItems.ToArray())
            {
                Title = LocalizationProvider.GetString("Stampe"),
                HierarchyText = string.Empty,
                Icon = "\ue07a",
            });
        }
    }

    public enum StampeSectionItemsId
    {
        Nothing = 0,
        Stampe,
        Documenti,
        Report
    }
}
