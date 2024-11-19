


using MasterDetailView;
using Commons;
using MasterDetailModel;
using System.Collections.Generic;
using System;

namespace PrezzariWpf.View
{

    public class CapitoliItemView : TreeEntityView
    {

        public CapitoliItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }

    }

    public class CapitoliView : MasterDetailTreeView, SectionItemTemplateView
    {

        public CapitoliView()
        {
            _itemsView = new CapitoliItemsViewVirtualized(this);
        }

        public int Code => (int) ElencoPrezziSectionItemsId.Capitoli;

    }

    /// <summary>
    /// Classe che rappresenta una lista virtualizzata di Prezzi 
    /// Tipico ordine delle chiamate per l'update della lista dopo una modifica:
    /// 
    /// SelectEntityById
    /// UpdateCache
    /// RefreshView
    /// LoadRange
    /// LoadingStateChanged
    /// 
    /// </summary>
    public class CapitoliItemsViewVirtualized : MasterDetailTreeViewVirtualized
    {

        public CapitoliItemsViewVirtualized(MasterDetailTreeView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityParentType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.CapitoliParent];
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Capitoli];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override TreeEntityView NewItemView(TreeEntity entity)
        {
            return new CapitoliItemView(this, entity);
        }

    }


} 
