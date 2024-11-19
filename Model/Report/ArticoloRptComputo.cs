using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Report
{
    public class ArticoloRptComputo : ArticoloRptBase
    {
        public string Classe_Ifc { get; set; }
        public string Livello { get; set; }

        public string Parti_uguali { get; set; }

        public decimal Quantità { get; set; }

        public decimal Importo { get; set; }
    }
}
