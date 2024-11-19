using CommonResources;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ContattiWpf.View
{
    public class ContattiAttributiSettingsView : AttributiSettingsView
    {


        //protected override void LoadEntityTypesName()
        //{
        //    List<RiferimentiComboItem> items = new List<RiferimentiComboItem>();

        //    //combo tipi di entità (sezioni)
        //    _entityTypesKeyOnInit.Clear();


        //    //Aggiungo le divisioni
        //    foreach (EntityType entType in EntityTypes.Values)
        //    {
        //        if (entType is DivisioneItemType)
        //        {
        //            DivisioneItemType divType = entType as DivisioneItemType;
        //            string comboItem = entType.Name;
        //            if (comboItem != null)
        //            {
        //                string key = divType.GetKey();
        //                _entityTypesKeyOnInit.Add(key);
        //                items.Add(new RiferimentiComboItem() { Key = EntityTypes[key].GetKey(), Name = comboItem, Category = LocalizationProvider.GetString("Divisioni") });
        //            }
        //        }

        //    }

        //    EntityTypesNameLoc = new ListCollectionView(items);
        //    EntityTypesNameLoc.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

        //}
    }
}
