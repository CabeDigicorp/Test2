using MasterDetailModel;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class GuidCollectionCalculated
    {
        public List<Guid> FilterIdsCalculated = null;
        //public Dictionary<ValoreOperationType, Valore> RiferimentoValoreCalculated = new Dictionary<ValoreOperationType, Valore>();

        //key:codice attributo
        public Dictionary<string, RiferimentoValoreCalculated> RiferimentoValoreCalculated = new Dictionary<string, RiferimentoValoreCalculated>();
    }

    public class RiferimentoValoreCalculated
    {
        public Dictionary<ValoreOperationType, Valore> ValoreCalculated = new Dictionary<ValoreOperationType, Valore>();
    }
}
