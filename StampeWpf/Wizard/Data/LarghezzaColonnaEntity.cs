using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StampeWpf.Wizard
{
    public class LarghezzaColonnaEntity : NotificationBase
    {
        public event EventHandler UpdateCalcoloTotaleLarghezzaColonna;
        private decimal _LarghezzaColonna;
        public decimal LarghezzaColonna
        {
            get
            {
                return _LarghezzaColonna;
            }
            set
            {
                if (SetProperty(ref _LarghezzaColonna, value))
                {
                    _LarghezzaColonna = value;
                    if (UpdateCalcoloTotaleLarghezzaColonna != null)
                    {
                        UpdateCalcoloTotaleLarghezzaColonna.Invoke(this, new EventArgs());
                    }
                }
            }
        }
        public LarghezzaColonnaEntity() { }
    }
}
