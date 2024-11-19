using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;

namespace DatiGeneraliWpf.View
{
    public class InfoProgettoItemView : MasterDetailListItemView
    {
        public InfoProgettoItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }


    public class InfoProgettoItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public InfoProgettoItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        

        public override void Init()
        {
            AttributiEntities.Load(new HashSet<Guid>());
            
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.InfoProgetto];
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new InfoProgettoItemView(this, entity);
        }

        protected override void OnItemsLoaded(EventArgs e)
        {
            base.OnItemsLoaded(e);
            SelectIndex(0);
        }


    }


    public class InfoProgettoView : MasterDetailListView, SectionItemTemplateView
    {
        public int Code => (int) DatiGeneraliSectionItemsId.InfoProgetto;

        //string _test = string.Empty;
        //public string Test
        //{
        //    get => _test;
        //    set
        //    {
        //        SetProperty(ref _test, value);
        //    }
        //}

        public InfoProgettoView()
        {
            _itemsView = new InfoProgettoItemsViewVirtualized(this);
        }

    }
}
