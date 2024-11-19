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

namespace DatiGeneraliWpf.View
{
    public class InfoProgettoAttributiSettingsView : AttributiSettingsView
    {


        //protected override void LoadEntityTypesName()
        //{
        //    List<RiferimentiComboItem> items = new List<RiferimentiComboItem>();

        //    //combo tipi di entità (sezioni)
        //    _entityTypesKeyOnInit.Clear();

        //    //Aggiungo le divisioni
        //    foreach (EntityType entType in EntityTypes.Values)
        //    {
        //        int dependencyEnum = (int) entType.DependencyEnum;

        //        if (dependencyEnum > 0)
        //        {

        //            if (entType is DivisioneItemType)
        //            {
        //                DivisioneItemType divType = entType as DivisioneItemType;
        //                string comboItem = entType.Name;
        //                if (comboItem != null)
        //                {
        //                    string key = divType.GetKey();
        //                    _entityTypesKeyOnInit.Add(key);
        //                    items.Add(new RiferimentiComboItem()
        //                    {
        //                        Key = EntityTypes[key].GetKey(),
        //                        Name = comboItem,
        //                        Category = LocalizationProvider.GetString("Divisioni")
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                if ((dependencyEnum & (int)EntityType.DependentTypesEnum) == dependencyEnum)
        //                {
        //                    string entTypeKey = entType.GetKey();

        //                    _entityTypesKeyOnInit.Add(entTypeKey);
        //                    items.Add(new RiferimentiComboItem()
        //                    {
        //                        Key = EntityTypes[entTypeKey].GetKey(),
        //                        Name = EntityTypes[entTypeKey].Name,
        //                        Category = LocalizationProvider.GetString("Sezioni")
        //                    });
        //                }
        //            }
        //        }


        //    }


        //    EntityTypesNameLoc = new ListCollectionView(items);
        //    EntityTypesNameLoc.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

        //}
    }
}
