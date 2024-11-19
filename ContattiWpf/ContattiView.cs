using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContattiWpf.View
{
    public class ContattiItemView : MasterDetailListItemView
    {
        public ContattiItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }


    public class ContattiView : MasterDetailListView
    {

        public ContattiView()
        {
            _itemsView = new ContattiItemsViewVirtualized(this);
        }
    }


    public class ContattiItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public ContattiItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Contatti];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new ContattiItemView(this, entity);
        }


    }





}
