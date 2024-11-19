using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputoWpf
{
    public class ApplyComputoRulesView : NotificationBase
    {

        public ApplyComputoRulesData Data { get; protected set; } = new ApplyComputoRulesData();
        
        ApplyComputoRulesEnum _selectedItem = ApplyComputoRulesEnum.Ignore;
        public ApplyComputoRulesEnum SelectedItem { get => Data.SelectedItem; protected set => Data.SelectedItem = value; }


        public bool IsIgnoreSelected
        {
            get => SelectedItem == ApplyComputoRulesEnum.Ignore;
            set
            {
                SelectedItem = ApplyComputoRulesEnum.Ignore;
                UpdateUI();
            }
        }

        public bool IsRemoveSelected
        {
            get => SelectedItem == ApplyComputoRulesEnum.Remove;
            set
            {
                SelectedItem = ApplyComputoRulesEnum.Remove;
                UpdateUI();
            }
        }

        public string RemoveCheck
        {
            get
            {
                return SelectedItem == ApplyComputoRulesEnum.Remove ? "\ue086" : string.Empty; 
            }
        }

        public string IgnoreCheck
        {
            get
            {
                return SelectedItem == ApplyComputoRulesEnum.Ignore ? "\ue086" : string.Empty;
            }
        }


        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsIgnoreSelected));
            RaisePropertyChanged(GetPropertyName(() => IsRemoveSelected));
            RaisePropertyChanged(GetPropertyName(() => IgnoreCheck));
            RaisePropertyChanged(GetPropertyName(() => RemoveCheck));
        }



    }

    public class ApplyComputoRulesData
    {
        public ApplyComputoRulesEnum SelectedItem { get; set; } = ApplyComputoRulesEnum.Ignore;
    }


    public enum ApplyComputoRulesEnum
    {
        Nothing = 0,
        Ignore,
        Remove,

    }


}
