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
    public class StiliProgettoItemView : MasterDetailListItemView
    {
        public StiliProgettoItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }

    public class StiliProgettoView : MasterDetailListView, SectionItemTemplateView
    {

        public StiliProgettoView()
        {
            _itemsView = new StiliProgettoItemsViewVirtualized(this);
        }

        public int Code => (int)DatiGeneraliSectionItemsId.StiliProgetto;

    }
    public class StiliProgettoItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public StiliProgettoItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Stili];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new StiliProgettoItemView(this, entity);
        }

    }

}
