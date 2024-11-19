using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Report
{
    public class ArticoloRptPrezzario : ArticoloRptBase
    {
        public string Prezzario_origine { get; set; }
        public string Codice_capitolo { get; set; }
        public string Descrizione_capitolo { get; set; }
        public string idArticolo { get; set; }
        public int Nodo { get; set; }
        public List<ArticoloRptPrezzario> ListaFigli { get; set; }
    }
}
