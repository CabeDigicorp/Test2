using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Calculators
{
    public class EntityCalcOptions
    {
        public bool CalcolaAttributiResults { get; set; } = false; //calcola risultati 
        public bool ResetCalulatedValues { get; set; } = false;//cancella i valori precedentemente calcolati

    }
}
