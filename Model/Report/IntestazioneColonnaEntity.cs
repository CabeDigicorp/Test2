using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Report
{
    public class IntestazioneColonnaEntity : NotificationBase
    {
        private string _IntestazioneColonna;
        public string IntestazioneColonna
        {
            get
            {
                return _IntestazioneColonna;
            }
            set
            {
                if (SetProperty(ref _IntestazioneColonna, value))
                {
                    _IntestazioneColonna = value;
                }
            }
        }
    }
}
