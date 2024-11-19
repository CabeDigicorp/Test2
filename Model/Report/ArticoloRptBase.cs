using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Report
{
    public class ArticoloRptBase
    {
        public string Codice_Articolo { get; set; }
        public string Descrizione { get; set; }
        public string DescrizioneSecondaria { get; set; }
        public string Unità_di_misura { get; set; }
        public decimal Prezzo { get; set; }

    }
}
