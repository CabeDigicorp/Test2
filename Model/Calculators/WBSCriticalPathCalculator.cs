using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WBSCriticalPathCalculator
    {
        IDataService _dataService = null;
        Dictionary<Guid, WBSItem> _allWBSItemsById = null;
        Dictionary<Guid, DateTimeCalculator> _timeCalulatorsByCalendarioId = new Dictionary<Guid, DateTimeCalculator>();


        public void Init(IDataService dataService)
        {
            _dataService = dataService;

            List<Guid> allWBSItemsId = null;
            _dataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, null, null, null, out allWBSItemsId);

            _allWBSItemsById = _dataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, allWBSItemsId).ToDictionary(item => item.EntityId, item => item as WBSItem);

            _timeCalulatorsByCalendarioId.Clear();
            foreach (Guid entId in allWBSItemsId)
            {
                string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                Guid wbsCalendarioId = _allWBSItemsById[entId].GetAttributoGuidId(wbsCalendarioIdCodice);
                if (!_timeCalulatorsByCalendarioId.ContainsKey(wbsCalendarioId))
                {
                    _timeCalulatorsByCalendarioId.Add(wbsCalendarioId, CreateDateTimeCalculator(wbsCalendarioId));
                }
            }

        }

        public HashSet<Guid> Run()
        {
            HashSet<Guid> criticalWbsItems = new HashSet<Guid>();

            if (!_allWBSItemsById.Any())
                return criticalWbsItems;


            DateTime maxDataFine = _allWBSItemsById.Values.Max(item => item.GetDataFine()).Value;
            
            FilterData filter = new FilterData();
            filter.Items.Add(new AttributoFilterData()
            {
                EntityTypeKey = BuiltInCodes.EntityType.WBS,
                CodiceAttributo = BuiltInCodes.Attributo.DataFine,
                CheckedValori = new HashSet<string>() { maxDataFine.ToShortDateString() },
                IsFiltroAttivato = true,
            });

            List<Guid> entsFound = null;
            _dataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, filter, null, null, out entsFound);
            foreach (Guid wbsItemId in entsFound)
            {
                criticalWbsItems.Add(wbsItemId);

                AddCriticalPredecessorsRecursive(wbsItemId, criticalWbsItems);
            }
            return criticalWbsItems;
        }

        private void AddCriticalPredecessorsRecursive(Guid wbsItemId, HashSet<Guid> criticalPredecessors)
        {
            WBSItem depWBSItem = _allWBSItemsById[wbsItemId];

            WBSPredecessors preds = depWBSItem.GetPredecessors();
            foreach (WBSPredecessor pred in preds.Items)
            {
                if (IsCriticalPredecessor(pred, depWBSItem))
                {
                    criticalPredecessors.Add(pred.WBSItemId);
                    AddCriticalPredecessorsRecursive(pred.WBSItemId, criticalPredecessors);
                }
            }
        }

        private bool IsCriticalPredecessor(WBSPredecessor pred, WBSItem depWBSItem)
        {
            Guid predId = pred.WBSItemId;
            WBSItem predWBSItem = _allWBSItemsById[predId];
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            Guid wbsCalendarioId = depWBSItem.GetAttributoGuidId(wbsCalendarioIdCodice);

            DateTimeCalculator depTimeCalc = null;
            if (_timeCalulatorsByCalendarioId.TryGetValue(wbsCalendarioId, out depTimeCalc))
            {
                DateTimeCalculator predTimeCalc = null;
                if (_timeCalulatorsByCalendarioId.TryGetValue(predWBSItem.GetAttributoGuidId(wbsCalendarioIdCodice), out predTimeCalc))
                {
                    //double delayDays = 0;
                    //if (WBSPredecessorCalculator.TryCalculatePredecessorDelay(depWBSItem, predWBSItem, timeCalc, out delayDays))
                    //{
                    //    if (delayDays <= pred.DelayDays)
                    //        return true;
                    //}
                    return WBSPredecessorCalculator.IsCriticalPredecessor(depWBSItem, predWBSItem, depTimeCalc, predTimeCalc);
                }
            }

            return false;
        }



        private DateTimeCalculator CreateDateTimeCalculator(Guid wbsCalendarioId)
        {
            DateTimeCalculator timeCalc = null;
            if (wbsCalendarioId != Guid.Empty)
            {
                CalendariItem calendario = _dataService.GetEntityById(BuiltInCodes.EntityType.Calendari, wbsCalendarioId) as CalendariItem;
                if (calendario == null)
                    return null;

                timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
            }
            return timeCalc;
        }
    }
}
