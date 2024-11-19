using _3DModelExchange;
using Autodesk.Revit.DB;
using CommonResources;
using Rejo.UI;
using ReJo.Utility;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReJo.UI
{
    public class FiltersTagView : NotificationBase
    {
        TagsData _TagsData = null;

        ObservableCollection<TagFilterItemView> _FilterItems = new ObservableCollection<TagFilterItemView>();
        public ObservableCollection<TagFilterItemView> FilterItems
        {
            get => _FilterItems;
            set => SetProperty(ref _FilterItems, value);
        }

        ObservableCollection<FiltersTagItemView> _FiltersTagItems = new ObservableCollection<FiltersTagItemView>();
        public ObservableCollection<FiltersTagItemView> FiltersTagItems
        {
            get => _FiltersTagItems;
            set => SetProperty(ref _FiltersTagItems, value);
        }

        FiltersTagItemView _CurrentTagItem = null;
        public FiltersTagItemView CurrentTagItem
        {
            get => _CurrentTagItem;
            set => SetProperty(ref _CurrentTagItem, value);
        
        }

        public List<string> SelectedFilterIds { get; set; } = new List<string>();


        public void SetTags(TagsData tagsData)
        {
            _TagsData = tagsData;
        }


        public void Load()
        {
            if (_TagsData == null)
                return;

            UpdateFilterItems();

            UpdateFiltersTagItems(-1);
            UpdateFiltersTagCheck();
        }

        internal void OnOk()
        {
            CmdInit.This.JoinService.SetCurrentProjectModel3dTags(_TagsData);
        }

        private void UpdateFilterItems()
        {
            var filterList = Utils.GetFilters(CmdInit.This.UIApplication.ActiveUIDocument.Document);

            List<TagFilterItemView> filterItems = new List<TagFilterItemView>();

            Dictionary<string, TagFilterItemView> filtersById = FilterItems.ToDictionary(item => item.UniqueId, item => item);

            foreach (var filter in filterList)
            {
                var filterItem = new TagFilterItemView(this)
                {
                    UniqueId = filter.UniqueId,
                    Name = filter.Name,
                };

                filterItems.Add(filterItem);
            }

            FilterItems = new ObservableCollection<TagFilterItemView>(filterItems);

        }

        void UpdateFiltersTagItems(int selectedIndex, bool sortByName = true)
        {

            List<FiltersTagItemView> filtersTagItems = new List<FiltersTagItemView>();

            var tags = _TagsData.Items;

            if (sortByName)
                tags = _TagsData.Items.OrderBy(item => item.TagDescr).ToList();

            foreach (var tag in tags)
            {
                if (tag.lstAssociatedFilters != null && tag.lstAssociatedFilters.Any())
                    continue;

                var tagViewItem = new FiltersTagItemView(this)
                {
                    Id = tag.TagId,
                    Name = tag.TagDescr,
                };

                //bool? isChecked = GetTagCheckByFilterSelection(tag);
                //tagViewItem.IsChecked = isChecked;


                filtersTagItems.Add(tagViewItem);

            }

            FiltersTagItems = new ObservableCollection<FiltersTagItemView>(filtersTagItems);

            if (selectedIndex < 0 && FiltersTagItems.Count > 0)
                selectedIndex = 0;


            if (0 <= selectedIndex && selectedIndex < FiltersTagItems.Count)
            {
                var curr = FiltersTagItems[selectedIndex];
                CurrentTagItem = curr;
            }



        }

        bool? GetTagCheckByFilterSelection(TagsDataItem? tag)
        {
            HashSet<string> rvtAssociatedFilters = new HashSet<string>(tag.RvtAssociatedFilters);

            //////////////////
            //calcolo check
            bool? isChecked = false;
            bool noOne = true;
            bool all = true;
            foreach (string filterId in SelectedFilterIds)
            {
                if (rvtAssociatedFilters.Contains(filterId))
                    noOne = false;
                else
                    all = false;
            }

            if (SelectedFilterIds.Any())
            {
                if (noOne)
                    isChecked = false;
                else if (all)
                    isChecked = true;
                else
                    isChecked = null;
            }
          


            return isChecked;
            /////////////////
        }

        //public ICommand AddTagCommand { get { return new CommandHandler(() => this.AddTag()); } }
        public void AddTag()
        {
            var tag = new TagsDataItem();
            tag.TagDescr = LocalizationProvider.GetString("Nuovo");
            tag.TagId = Guid.NewGuid();

            _TagsData.Items.Add(tag);

            UpdateFiltersTagItems(_TagsData.Items.Count - 1, false);
        }

        public ICommand RemoveTagCommand { get { return new CommandHandler(() => this.RemoveTag()); } }
        void RemoveTag()
        {
            if (CurrentTagItem == null)
                return;

            var tagIndex = _TagsData.Items.FindIndex(item => item.TagId == CurrentTagItem.Id);
            _TagsData.Items.RemoveAt(tagIndex);

            UpdateFiltersTagItems(tagIndex-1, false);


        }

        internal void Rename(Guid id, string name)
        {
            var tagIndex = _TagsData.Items.FindIndex(item => item.TagId == id);

            if (tagIndex >= 0)
                _TagsData.Items[tagIndex].TagDescr = name;
        }

        internal void SetCheck(Guid tagId, bool isChecked)
        {
            var tagIndex = _TagsData.Items.FindIndex(item => item.TagId == tagId);

            if (tagIndex >= 0)
            {
                _TagsData.Items[tagIndex].IsSelected = isChecked;

                HashSet<string> rvtAssociatedFilters = new HashSet<string>(_TagsData.Items[tagIndex].RvtAssociatedFilters);


                if (isChecked)
                {
                    foreach (string filterId in SelectedFilterIds)
                        rvtAssociatedFilters.Add(filterId);
                }
                else
                {
                    foreach (string filterId in SelectedFilterIds)
                        rvtAssociatedFilters.Remove(filterId);
                }

                _TagsData.Items[tagIndex].RvtAssociatedFilters = rvtAssociatedFilters.ToList();

            }
        }

        public void UpdateFiltersTagCheck()
        {

            foreach (var tag in _TagsData.Items)
            {
                bool? isChecked = GetTagCheckByFilterSelection(tag);

                var tagViewItem = FiltersTagItems.FirstOrDefault(item => item.Id == tag.TagId);
                if (tagViewItem != null)
                    tagViewItem.SetIsCheckedInternal(isChecked);

            }

            
        }

        public ICommand CheckAllTagCommand { get { return new CommandHandler(() => this.CheckAllTag()); } }
        void CheckAllTag()
        {
            _TagsData.Items.ForEach(item => SetCheck(item.TagId, true));
            UpdateFiltersTagCheck();
        }

        public ICommand UncheckAllTagCommand { get { return new CommandHandler(() => this.UncheckAllTag()); } }
        void UncheckAllTag()
        {
            _TagsData.Items.ForEach(item => SetCheck(item.TagId, false));
            UpdateFiltersTagCheck();

        }


    }

    public class TagFilterItemView : NotificationBase
    {
        FiltersTagView? _Owner = null;

        public TagFilterItemView(FiltersTagView owner)
        {
            _Owner = owner;
        }

        public string UniqueId { get; set; }

        string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }
    }

    public class FiltersTagItemView : NotificationBase
    {
        FiltersTagView? _Owner = null;

        public FiltersTagItemView(FiltersTagView owner)
        {
            _Owner = owner;
        }

        public Guid Id { get; set; }

        string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set
            {
                if (SetProperty(ref _Name, value))
                {
                    _Owner?.Rename(Id, value);
                }

            }
        }

        bool? _IsChecked = false;
        public bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (_Owner?.SelectedFilterIds.Count <= 0)
                    return;

                bool isChecked = false;
                if (value == true)
                    isChecked = true;

                if (SetProperty(ref _IsChecked, isChecked))
                {
                    _Owner?.SetCheck(Id, isChecked);
                }
            }
        }

        public void SetIsCheckedInternal(bool? isChecked)
        { 
            _IsChecked = isChecked;
            UpdateUI();
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Name));
            RaisePropertyChanged(GetPropertyName(() => IsChecked));
        }
    }
}
