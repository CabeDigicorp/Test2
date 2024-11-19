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
    public class VariabiliItemView : MasterDetailListItemView
    {
        public VariabiliItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }


    public class VariabiliItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public VariabiliItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Variabili];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new VariabiliItemView(this, entity);
        }

        protected override void OnItemsLoaded(EventArgs e)
        {
            base.OnItemsLoaded(e);
            SelectIndex(0);
        }


    }


    public class VariabiliView : MasterDetailListView, SectionItemTemplateView
    {
        public int Code => (int) DatiGeneraliSectionItemsId.Variabili;

        //string _test = string.Empty;
        //public string Test
        //{
        //    get => _test;
        //    set
        //    {
        //        SetProperty(ref _test, value);
        //    }
        //}

        public VariabiliView()
        {
            _itemsView = new VariabiliItemsViewVirtualized(this);
        }

    }
}
