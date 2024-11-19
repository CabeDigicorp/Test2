using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class ScalaNumerica
    {
        public int ProgressivoNumerico;
        public int ProgressivoRispettoAPrecedente;
        public Dictionary<string, ScalaNumerica> UnitaTempoProgressivoNumerico;

        public int ProgressivoNumericoAnno { get; set; }
        public int ProgressivoNumericoMese { get; set; }
        public int ProgressivoNumericoSettimana { get; set; }
        public int ProgressivoNumericoGiorno { get; set; }
        public int ProgressivoNumericoOra { get; set; }
        public int ProgressivoNumericoMinuto { get; set; }
        public int ProgressivoNumericoAnnoSubordinato { get; set; }
        public int ProgressivoNumericoMeseSubordinato { get; set; }
        public int ProgressivoNumericoSettimanaSubordinato { get; set; }
        public int ProgressivoNumericoGiornoSubordinato { get; set; }
        public int ProgressivoNumericoOraSubordinato { get; set; }
        public int ProgressivoNumericoMinutoSubordinato { get; set; }
        public int ProgressivoNumericoAnnoAnonima { get; set; }
        public int ProgressivoNumericoMeseAnonima { get; set; }
        public int ProgressivoNumericoSettimanaAnonima { get; set; }
        public int ProgressivoNumericoGiornoAnonima { get; set; }
        public int ProgressivoNumericoOraAnonima { get; set; }
        public int ProgressivoNumericoMinutoAnonima { get; set; }
        public DateTime Date { get; set; }
    }
}
