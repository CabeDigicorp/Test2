using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf.View
{
    public class RibbonView : NotificationBase
    {
        private ObservableCollection<RibbonAttributeView> _SALAttibutes;
        public ObservableCollection<RibbonAttributeView> SALAttibutes
        {
            get
            {
                return _SALAttibutes;
            }
            set
            {
                SetProperty(ref _SALAttibutes, value);
            }
        }

        private RibbonAttributeView _SALAttibute;
        public RibbonAttributeView SALAttibute
        {
            get
            {
                return _SALAttibute;
            }
            set
            {
                SetProperty(ref _SALAttibute, value);
            }
        }
        public RibbonView()
        {
            SALAttibutes = new ObservableCollection<RibbonAttributeView>();
        }

        //public void  UpdateUI()
        //{
        //    RaisePropertyChanged(GetPropertyName(() => SALAttibutes));
        //    //RaisePropertyChanged(GetPropertyName(() => SALAttibute));
        //}
    }

    public class RibbonAttributeView : NotificationBase
    { 
        public string Etichetta { get; set; }
        public string Codice { get; set; }
        public string DefinizioneAttributo { get; set; }

    }

}
