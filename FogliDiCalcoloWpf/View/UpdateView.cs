using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf.View
{
    public class UpdateView : NotificationBase
    {
        public string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (SetProperty(ref _Name, value))
                {
                    _Name = value;
                }
            }
        }
        public bool _Value;
        public bool Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (SetProperty(ref _Value, value))
                {
                    _Value = value;
                }
            }
        }
    }
}
