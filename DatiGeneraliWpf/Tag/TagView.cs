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
    public class TagItemView : MasterDetailListItemView
    {
        public TagItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
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


    public class TagView : MasterDetailListView, SectionItemTemplateView
    {

        public TagView()
        {
            _itemsView = new TagItemsViewVirtualized(this);
        }
        public int Code => (int)DatiGeneraliSectionItemsId.Tag;

    }


    public class TagItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public TagItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Tag];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new TagItemView(this, entity);
        }


    }


}
