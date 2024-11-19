using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class ElencoAttivitaItemView : MasterDetailListItemView
    {
        public ElencoAttivitaItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }


    public class ElencoAttivitaView : MasterDetailListView, SectionItemTemplateView
    {

        public ElencoAttivitaView()
        {
            _itemsView = new ElencoAttivitaItemsViewVirtualized(this);
        }
        public int Code => (int)AttivitaSectionItemsId.ElencoAttivita;

    }


    public class ElencoAttivitaItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public ElencoAttivitaItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.ElencoAttivita];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new ElencoAttivitaItemView(this, entity);
        }


    }


}
