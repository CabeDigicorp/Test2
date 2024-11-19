using CommonResources;
using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class GanttGridSettingView : NotificationBase
    {
        private ObservableCollection<ColorInfo> _ColorsHorizontal;
        public ObservableCollection<ColorInfo> ColorsHorizontal
        {
            get
            {
                return _ColorsHorizontal;
            }
            set
            {
                if (SetProperty(ref _ColorsHorizontal, value))
                {
                    _ColorsHorizontal = value;
                }
            }
        }
        private ObservableCollection<ColorInfo> _ColorsVertical;
        public ObservableCollection<ColorInfo> ColorsVertical
        {
            get
            {
                return _ColorsVertical;
            }
            set
            {
                if (SetProperty(ref _ColorsVertical, value))
                {
                    _ColorsVertical = value;
                }
            }
        }

        private ColorInfo _ColorCharactherHorizontal;
        public ColorInfo ColorCharactherHorizontal
        {
            get
            {
                return _ColorCharactherHorizontal;
            }
            set
            {
                if (SetProperty(ref _ColorCharactherHorizontal, value))
                {
                    _ColorCharactherHorizontal = value;
                }
            }
        }

        private ColorInfo _ColorCharactherVertical;
        public ColorInfo ColorCharactherVertical
        {
            get
            {
                return _ColorCharactherVertical;
            }
            set
            {
                if (SetProperty(ref _ColorCharactherVertical, value))
                {
                    _ColorCharactherVertical = value;
                }
            }
        }

        private bool _IsCheckedHorizontal;
        public bool IsCheckedHorizontal
        {
            get
            {
                return _IsCheckedHorizontal;
            }
            set
            {
                if (SetProperty(ref _IsCheckedHorizontal, value))
                {
                    _IsCheckedHorizontal = value;
                }
            }
        }

        private bool _IsCheckedVertical;
        public bool IsCheckedVertical
        {
            get
            {
                return _IsCheckedVertical;
            }
            set
            {
                if (SetProperty(ref _IsCheckedVertical, value))
                {
                    _IsCheckedVertical = value;
                }
            }
        }

        public GanttGridSettingView()
        {
            ColorsHorizontal = new ObservableCollection<ColorInfo>();
            ColorsVertical = new ObservableCollection<ColorInfo>();

            var coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);

            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    ColorsHorizontal.Add(colInfo);
                    ColorsVertical.Add(colInfo);
                }
            }

            DefaultSetting();
        }

        public void DefaultSetting()
        {
            IsCheckedHorizontal = false;
            IsCheckedVertical = false;
            ColorCharactherHorizontal = ColorsHorizontal.Where(d => d.HexValue == "#FFF5F5F5").FirstOrDefault();
            ColorCharactherVertical = ColorsVertical.Where(d => d.HexValue == "#FFF5F5F5").FirstOrDefault();
        }

        public bool Accept()
        {
            return true;
        }
    }
}
