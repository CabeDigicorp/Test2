using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatiGeneraliWpf.View
{
    public class AllegatiItemView : MasterDetailListItemView
    {
        public AllegatiItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }

        //public Guid AttributoFileId
        //{
        //    get
        //    {
                
        //        if (This.Attributi.ContainsKey(BuiltInCodes.Attributo.FileUploadId))
        //        {
        //            return (This.Attributi[BuiltInCodes.Attributo.FileUploadId].Valore as ValoreGuid).V;
        //        }

        //        return Guid.Empty;
        //    }
        //}

        public string AttributoNavigationUrl
        {
            get
            {
                if (This.Attributi.ContainsKey(BuiltInCodes.Attributo.Link))
                {
                    return This.Attributi[BuiltInCodes.Attributo.Link].Valore.ToPlainText();
                }

                return String.Empty;
            }
        }

        public string AttributoNome
        {
            get
            {
                if (This.Attributi.ContainsKey(BuiltInCodes.Attributo.Nome))
                {
                    return This.Attributi[BuiltInCodes.Attributo.Nome].Valore.ToPlainText();
                }

                return String.Empty;
            }
        }
    }


    public class AllegatiView : MasterDetailListView, SectionItemTemplateView
    {

        public AllegatiView()
        {
            _itemsView = new AllegatiItemsViewVirtualized(this);
        }
        public int Code => (int)DatiGeneraliSectionItemsId.Allegati;

    }


    public class AllegatiItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public AllegatiItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Allegati];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new AllegatiItemView(this, entity);
        }


    }


}
