using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Commons
{

    public class SectionItemView : NotificationBase
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string HierarchyText { get; set; }
        public string Icon { get; set; }//digicorp font
        public string Description { get; set; }

        public SectionItemView(int id, params SectionItemView[] myItems)
        {
            Id = id;
            _sectionItems = new ObservableCollection<SectionItemView>();
            foreach (var item in myItems)
            {
                _sectionItems.Add(item);
            }
            SectionItems = _sectionItems;
        }

        private ObservableCollection<SectionItemView> _sectionItems = new ObservableCollection<SectionItemView>();
        public ObservableCollection<SectionItemView> SectionItems
        {
            get { return _sectionItems; }
            set
            {
                SetProperty(ref _sectionItems, value);
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Title));
            RaisePropertyChanged(GetPropertyName(() => Description));
        }


        public bool IsSectionItemsVisible 
        { 
            get => !string.IsNullOrEmpty(HierarchyText);
        }

    }

    public interface SectionItemTemplateView
    {
        int Code { get; }
        
    }

    public enum RootSectionItemsId
    {
        Nothing = 0,
        FogliDiCalcolo,
    }

}
