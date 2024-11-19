using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Report
{
    public class LarghezzaColonnaEntity : NotificationBase
    {
        private string _LarghezzaColonna;
        public string LarghezzaColonna
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
                }
            }
        }
    }
}
