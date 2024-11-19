using _3DModelExchange;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommonResources;
using DevExpress.Utils.Filtering.Internal;
using Rejo.UI;
using ReJo.Utility;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.RowFilter;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FilterElement = Autodesk.Revit.DB.FilterElement;

namespace ReJo.UI
{
    public class FiltersPaneView : NotificationBase
    {
        FiltersData _FiltersData = null;

        ObservableCollection<FilterItemView> _FilterItems = new ObservableCollection<FilterItemView>();
        public ObservableCollection<FilterItemView> FilterItems
        {
            get => _FilterItems;
            set => SetProperty(ref _FilterItems, value);
        }

        HashSet<long> _FiltersId = new HashSet<long>();


        ObservableCollection<FilterTagItem> _TagCmbItems = new ObservableCollection<FilterTagItem>();
        public ObservableCollection<FilterTagItem> TagCmbItems
        {
            get => _TagCmbItems;
            set => SetProperty(ref _TagCmbItems, value);
        }


        private ObservableCollection<object> _TagCmbSelectedItems = new ObservableCollection<object>();
        public ObservableCollection<object> TagCmbSelectedItems
        {
            get { return _TagCmbSelectedItems; }
            set { SetProperty(ref _TagCmbSelectedItems, value); }
        }

        //errori calcolati dal comando check rules
        private List<RuleError> _RuleErrors = new List<RuleError>();


        public bool Update(HashSet<long>? filtersChanged = null)
        {
            CmdInit.This.LoadModel3dFiles(CmdInit.This.UIApplication.ActiveUIDocument.Document);

            if (!UpdateTags())
            {
                TagCmbItems.Clear();
                FilterItems.Clear();
                return false;
            }

            

            UpdateFilterItems(filtersChanged);

            return true;    
        }



        private bool UpdateTags()
        {

            TagsData tags = CmdInit.This.JoinService.GetCurrentProjectModel3dTags();

            if (tags == null)
                return false;

            List<FilterTagItem> tagItemsView = new List<FilterTagItem>();

            foreach (var tag in tags.Items)
            {
                tagItemsView.Add(new FilterTagItem()
                {
                    Name = tag.TagDescr,
                    RvtAssociatedFilters = tag.RvtAssociatedFilters.ToHashSet(),
                });
            }

            TagCmbItems = new ObservableCollection<FilterTagItem>(tagItemsView);

            ///???

            var items = new ObservableCollection<object>();

            TagCmbSelectedItems = new ObservableCollection<object>(items);
            TagCmbSelectedItems.CollectionChanged += TagCmbSelectedItems_CollectionChanged;


            return true;
        }

        private void TagCmbSelectedItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFilterItems();
        }

        public void UpdateFilterItems(HashSet<long>? filtersChanged = null)
        {
            //se non riesco ad accedere ai dati del projetto join esco
            _FiltersData = CmdInit.This.JoinService.GetCurrentProjectModel3dFilters();
            if (_FiltersData == null)
            {
                FilterItems.Clear();
                return;
            }

            //se il documento corrente non è tra quelli in join model 3d esco
            UIDocument uiDoc = CmdInit.This.UIApplication.ActiveUIDocument;
            Document doc = uiDoc.Document;

            
            if (!JoinService.This.Model3dFilesLoaded.ContainsKey(doc.PathName))
            {
                FilterItems.Clear();
                return;
            }

            //Aggiungo i filtri di _FiltersData presenti nel disegno di revit
            var filterList = Utils.GetFilters(CmdInit.This.UIApplication.ActiveUIDocument.Document);

            List<FilterItemView> filterItems = new List<FilterItemView>();

            Dictionary<string, FilterItemView> filtersByUniqueId = FilterItems.Where(item => !string.IsNullOrEmpty(item.UniqueId)).ToDictionary(item => item.UniqueId, item => item);

            HashSet<string> rvtFiltersUniqueId = new HashSet<string>();

            foreach (var filter in filterList)
            {

                if (!IsTagPasses(filter))
                    continue;

                if (!IsSearchPasses(filter.Name))
                    continue;


                FilterItemView? filterItemOld = null;
                filtersByUniqueId.TryGetValue(filter.UniqueId, out filterItemOld);

                var filterItem = new FilterItemView(this)
                {
                    Id = filter.Id,
                    UniqueId = filter.UniqueId,
                    Name = filter.Name,
                    ExistRvtFilter = true,
                };

                if (filterItemOld != null)
                {
                    filterItem.IsChecked = filterItemOld.IsChecked;
                }

                string filterUniqueId = filter.UniqueId;

                //aggiorno has rules
                FiltersDataItem? filtersDataItem = _FiltersData.Items.FirstOrDefault(item => item.RvtFilter?.RvtFilterUniqueId == filterUniqueId);
                if (filtersDataItem != null)
                {
                    if (filtersDataItem.RulesIO?.Count > 0)
                        filterItem.HasRules = true;
                    else
                        filterItem.HasRules = false;

                    filterItem.FilterDataId = filtersDataItem.ID;
                    rvtFiltersUniqueId.Add(filterUniqueId);
                }

                var filterRuleErrors = _RuleErrors.Where(item => item.FilterUniqueId == filterUniqueId);
                filterItem.RuleErrors = filterRuleErrors.ToList();


                filterItems.Add(filterItem);
            }




            //Aggiungo i filtri di _FiltersData non presenti nel disegno di revit

            Dictionary<Guid, FilterItemView> filtersDataById = FilterItems.Where(item => item.FilterDataId != Guid.Empty).ToDictionary(item => item.FilterDataId, item => item);

            foreach (var filtersDataItem in _FiltersData.Items)
            {
                if (filtersDataItem.RvtFilter == null)
                    continue;

                if (rvtFiltersUniqueId.Contains(filtersDataItem.RvtFilter.RvtFilterUniqueId))
                    continue;

                if (!IsTagPasses(filtersDataItem))
                    continue;

                if (!IsSearchPasses(filtersDataItem.Descri))
                    continue;

                var filterItem = new FilterItemView(this)
                {
                    Id = ElementId.InvalidElementId,
                    UniqueId = string.Empty,
                    FilterDataId = filtersDataItem.ID,
                    Name = filtersDataItem.Descri,
                    HasRules = true,
                    ExistRvtFilter = false,
                };

                FilterItemView? filterItemOld = null;
                filtersDataById.TryGetValue(filtersDataItem.ID, out filterItemOld);

                if (filterItemOld != null)
                {
                    filterItem.IsChecked = filterItemOld.IsChecked;
                }

                filterItems.Add(filterItem);
            }





            FilterItems = new ObservableCollection<FilterItemView>(filterItems.OrderBy(item => item.Name));

            _FiltersId = filterItems.Select(item => item.Id.Value).ToHashSet();
        } 
        
        public bool ContainsFilter(long filterId)
        {
            return _FiltersId.Contains(filterId);
        }

        public FiltersData GetFiltersData()
        {
            return _FiltersData;
        }

        public ICommand CheckAllFiltersCommand { get { return new CommandHandler(() => this.CheckAllFilters()); } }
        void CheckAllFilters()
        {
            foreach (var item in _FilterItems)
                item.IsChecked = true;

            UpdateUI();
        }

        public ICommand UncheckAllFiltersCommand { get { return new CommandHandler(() => this.UncheckAllFilters()); } }
        void UncheckAllFilters()
        {
            foreach (var item in _FilterItems)
                item.IsChecked = false;

            UpdateUI();
        }

        string _SearchText = string.Empty;
        public string SearchText
        {
            get => _SearchText;
            set
            {
                SetProperty(ref _SearchText, value);
                SearchEnter();
            }
        }

        public ICommand UpdateCommand { get { return new CommandHandler(() => this.Update()); } }


        public ICommand ClearSearchTextCommand { get { return new CommandHandler(() => this.ClearSearchText()); } }
        void ClearSearchText()
        {
            SearchText = string.Empty;
        }

        public ICommand SearchEnterCommand { get { return new CommandHandler(() => this.SearchEnter()); } }
        void SearchEnter()
        {
            UpdateFilterItems();
        }


        bool IsSearchPasses(string filterName)
        {

            string[] searchArray = _SearchText.Split(' ');
            foreach (var item in searchArray)
            {
                if (filterName.Contains(item.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;

            }

            return false;
        }

        bool IsTagPasses(FilterElement filterEl)
        {

            if (TagCmbSelectedItems?.Count == 0)
                return true;

            string filterUniqueId = filterEl.UniqueId;

            foreach (FilterTagItem tag in TagCmbSelectedItems)
            { 
                if (tag.RvtAssociatedFilters.Contains(filterUniqueId))
                    return true;
            
            }

            return false;
        }
        bool IsTagPasses(FiltersDataItem filterItem)
        {
            if (TagCmbSelectedItems?.Count > 0)
                return false;

            return true;
        }




        private HashSet<long>? GetElementsId(Document doc)
        {
            HashSet<long> uniqueElementsId = new HashSet<long>();

            var filtersIdVisibleChecked = FilterItems.Where(item => item.IsChecked).Select(item => item.Id);

            if (filtersIdVisibleChecked.Count() == 0)
                return null;

            foreach (var filterId in filtersIdVisibleChecked)
            {
                FilterElement? filterEl = doc.GetElement(filterId) as FilterElement;

                if (filterEl != null)
                {
                    var elems = Utils.GetFilterElementsId(doc, filterEl);
                    uniqueElementsId.UnionWith(elems.Select(item => item.Value));
                }

            }

            return uniqueElementsId;
        }

        public ICommand SelectElementsCommand { get { return new CommandHandler(() => this.SelectElements()); } }
        void SelectElements()
        {
            UIDocument uiDoc = CmdInit.This.UIApplication.ActiveUIDocument;

            Document doc = uiDoc.Document;

            HashSet<long>? uniqueElementsId = GetElementsId(doc);

            if (uniqueElementsId == null)
                return;

            // clear other temporary views
            //doc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

            List<ElementId> newSelectedElementSet = new List<ElementId>();

            foreach (long id in uniqueElementsId)
            {
                // create a new id and set the value
                ElementId elId = new ElementId(id);
                Element Elm = doc.GetElement(elId);
                if (Elm != null)
                {
                    //Aggiunge all'insieme di selezione
                    newSelectedElementSet.Add(Elm.Id);
                }

            }


            // select collected elements
            //CmdInit.This.UIApplication.ActiveUIDocument.Selection.SetElementIds((uniqueElementsId.Select(item => new ElementId(item)).ToList()));
            uiDoc.Selection.SetElementIds(newSelectedElementSet);
        }

        public ICommand IsolateElementsCommand { get { return new CommandHandler(() => this.IsolateElements()); } }
        void IsolateElements()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            HashSet<long>? uniqueElementsId = GetElementsId(doc);

            if (uniqueElementsId == null)
                return;

            // clear other temporary views
            doc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

            // isolate collected elements
            doc.ActiveView.IsolateElementsTemporary(uniqueElementsId.Select(item => new ElementId(item)).ToList());
            CmdInit.This.UIApplication.ActiveUIDocument.RefreshActiveView();

        }



        public ICommand HideElementsCommand { get { return new CommandHandler(() => this.HideElements()); } }
        void HideElements()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            HashSet<long>? uniqueElementsId = GetElementsId(doc);

            if (uniqueElementsId == null)
                return;

            // clear other temporary views
            doc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

            // hide collected elements
            doc.ActiveView.HideElementsTemporary(uniqueElementsId.Select(item => new ElementId(item)).ToList());

            
            CmdInit.This.UIApplication.ActiveUIDocument.RefreshActiveView();

        }

        public ICommand RestoreElementsCommand { get { return new CommandHandler(() => this.RestoreElements()); } }
        void RestoreElements()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            // clear other temporary views
            doc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

            CmdInit.This.UIApplication.ActiveUIDocument.Selection.SetElementIds(new List<ElementId>());
        }


        public ICommand RemoveRulesCommand { get { return new CommandHandler(() => this.RemoveRules()); } }
        void RemoveRules()
        {

            FiltersData filtersData = JoinService.This.GetCurrentProjectModel3dFilters();

            if (filtersData == null)
                return;

            int filtersCount = filtersData.Items.Count;



            ////////////////////////////////////////////////
            //Elimino tutte le regole dai filtri checkati che esistono in Revit
            HashSet<string> filtersUniqueIdVisibleChecked = new HashSet<string>(FilterItems.Where(item => item.IsChecked && !string.IsNullOrEmpty(item.UniqueId)).Select(item => item.UniqueId));

            if (filtersUniqueIdVisibleChecked.Count() > 0)
            {
                filtersData.Items.RemoveAll(item =>
                {
                    if (item.RvtFilter != null)
                    {
                        if (filtersUniqueIdVisibleChecked.Contains(item.RvtFilter.RvtFilterUniqueId))
                            return true;
                    }

                    return false;
                });
            }

            ///////////////////////////////////////////////
            //Elimino le regole dai filtri checkati cne NON esistono in Revit

            HashSet<Guid> filtersDataIdVisibleChecked = new HashSet<Guid>(FilterItems.Where(item => item.IsChecked && item.FilterDataId != Guid.Empty).Select(item => item.FilterDataId));

            if (filtersDataIdVisibleChecked.Count() > 0)
            {
                filtersData.Items.RemoveAll(item =>
                {
                    if (filtersDataIdVisibleChecked.Contains(item.ID))
                        return true;

                    return false;
                });
            }

            if (filtersData.Items.Count < filtersCount)
            {
                if (JoinService.This.SetCurrentProjectModel3dFilters(filtersData))
                    UpdateFilterItems();
            }


            
        }


        public ICommand CreateFiltersCommand { get { return new CommandHandler(() => this.CreateRvtFilters()); } }
        async void CreateRvtFilters()
        {

#if !DEBUG
            MessageBox.Show("Comando non ancora disponibile");
return;
#endif


            HashSet<Guid> filtersDataIdVisibleChecked = new HashSet<Guid>(FilterItems.Where(item => item.IsChecked && item.FilterDataId != Guid.Empty).Select(item => item.FilterDataId));

            if (filtersDataIdVisibleChecked.Count == 0)
                return;

            FiltersData filtersData = JoinService.This.GetCurrentProjectModel3dFilters();

            if (filtersData == null)
                return;

            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
            string projectIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);




            HashSet<string> filtersNameNotUnique = new HashSet<string>();

            //controllo se tutti i filtri non hanno omonimi esistenti
            foreach (var filterDataItem in filtersData.Items)
            {
                if (filterDataItem.RvtFilter == null)
                    continue;

                if (filterDataItem.RvtFilter.ProjectIfcGuid != projectIfcGuid)
                    continue;

                FilterElement filterEl = doc.GetElement(filterDataItem.RvtFilter.RvtFilterUniqueId) as FilterElement;

                if (filterEl != null) 
                    continue;

                if (!FilterElement.IsNameUnique(doc, filterDataItem.Descri))
                {
                    filtersNameNotUnique.Add(filterDataItem.Descri);
                }
            }

            bool overwrite = true;

            if (filtersNameNotUnique.Any())
            {
                overwrite = false;

                string str = string.Format("{0} {1}", string.Join(',', filtersNameNotUnique), LocalizationProvider.GetString("filtriGiaPresentiVuoiSovrascriverli"));
                MessageBoxResult res = MessageBox.Show(str, Application.TabReJoName, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    overwrite = true;
            }

            if (!overwrite)
                return;


            TagsData tagsData = JoinService.This.GetCurrentProjectModel3dTags();

            if (tagsData == null)
                return;

            string resultMsg = string.Empty;
            bool dataChanged = false;

            foreach (var filterDataItem in filtersData.Items)
            {
                if (filterDataItem.RvtFilter == null)
                    continue;

                if (filterDataItem.RvtFilter.ProjectIfcGuid != projectIfcGuid)
                    continue;

                FilterElement filterEl = doc.GetElement(filterDataItem.RvtFilter.RvtFilterUniqueId) as FilterElement;

                if (filterEl != null) //filtro già presente per UniqueId
                    continue;


                string rvtFilterUniqueIdOld = filterDataItem.RvtFilter.RvtFilterUniqueId;


                CmdInit.This.AddFilterHandler.Document = doc;
                CmdInit.This.AddFilterHandler.FilterData = filterDataItem.RvtFilter;
                string filterElementUniqueIdNew = await CmdInit.This.AddFilterHandler.RaiseExternalEvent();

                //string filterElementUniqueIdNew = FilterElementConverter.CreateRvtFilter(filterDataItem.RvtFilter, out resultMsg);

                if (string.IsNullOrEmpty(filterElementUniqueIdNew))
                    continue;


                ////////////////////////////////////////////
                //Aggiunta del filtro in revit a buon fine
                dataChanged = true;

                filterDataItem.RvtFilter.RvtFilterUniqueId = filterElementUniqueIdNew;

                //rimappatura filtri associati ai tags
                foreach (var tag in tagsData.Items)
                {
                    for (int i = 0; i < tag.RvtAssociatedFilters.Count; i++)
                    {
                        if (tag.RvtAssociatedFilters[i] == rvtFilterUniqueIdOld)
                            tag.RvtAssociatedFilters[i] = filterElementUniqueIdNew;
                    }
                }

            }

            if (dataChanged)
            {
                JoinService.This.SetCurrentProjectModel3dTags(tagsData);

                JoinService.This.SetCurrentProjectModel3dFilters(filtersData);

                FiltersPane.This.Update();

            }

            if (!string.IsNullOrEmpty(resultMsg))
            {
                //dialogo dei risultati
                var wnd = new ResultWnd();
                wnd.TextBox.Text = resultMsg;
            }



        }

        string _TagsCheckedText = string.Empty;
        public string TagsCheckedText
        {
            get => _TagsCheckedText;
            set => SetProperty(ref _TagsCheckedText, value);
        }


        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => SearchText));
            RaisePropertyChanged(GetPropertyName(() => FilterItems));

            foreach (var item in FilterItems)
            {
                item.UpdateUI();
            }
        }

        internal void SetRuleErrors(List<RuleError> ruleErrors)
        {
            _RuleErrors = ruleErrors;
        }
    }

    public class FilterItemView : NotificationBase
    {
        FiltersPaneView? _Owner = null;
        

        public FilterItemView(FiltersPaneView pane)
        {
            _Owner = pane;
        }

        public ElementId Id { get; set; }
        public string UniqueId { get; set; }

        public Guid FilterDataId { get; set; }

        string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        bool _IsChecked = false;
        public bool IsChecked
        { 
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }

        bool _HasRules = false;
        public bool HasRules
        {
            get => _HasRules;
            set => SetProperty(ref _HasRules, value);
        }


        public bool ExistRvtFilter { get; set; } = false;
        
        public List<RuleError> RuleErrors { get; set; }


        public bool HasRulesError
        {
            get => RuleErrors?.Count > 0;
        }


        public SolidColorBrush HasRulesColor
        {
            get
            {
                if (ExistRvtFilter)
                {

                    if (HasRulesError)
                        return new SolidColorBrush(Colors.Red);
                    else
                        return new SolidColorBrush(Colors.Black);
                }
                else
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
        }
        public SolidColorBrush FilterNameColor
        {
            get
            {
                if (ExistRvtFilter)
                {
                    return new SolidColorBrush(Colors.Black);
                }
                else
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
        }





        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => HasRules));
            RaisePropertyChanged(GetPropertyName(() => HasRulesError));
            RaisePropertyChanged(GetPropertyName(() => HasRulesColor));
            RaisePropertyChanged(GetPropertyName(() => FilterNameColor));

        }



    }

    public class FilterTagItem : NotificationBase
    {
        public string Name { get; set; } = string.Empty;
        public HashSet<string> RvtAssociatedFilters { get; set; } = new HashSet<string>();
    }
}
