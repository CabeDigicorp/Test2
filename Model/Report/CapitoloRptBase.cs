using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Report
{
    public class CapitoloRptBase
    {
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string idCapitolo { get; set; }
        public int Nodo { get; set; }
        public List<CapitoloRptBase> ListaFigli { get; set; }
    }
}
