


using System;
using System.Collections.Generic;
using System.Linq;
using MasterDetailView;
using MasterDetailModel;
using Model;
using _3DModelExchange;
using CommonResources;

namespace DivisioniWpf
{

    public class DivisioneItemView : MasterDetailTreeSumItemView
    {

        public DivisioneItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }

    public class DivisioneView : MasterDetailTreeSumView
    {
       
        public DivisioneItemsViewVirtualized DivisioneItemsView { get => _itemsView as DivisioneItemsViewVirtualized; }

        public Guid Id { get; protected set; }

        public DivisioneView()
        {
            _itemsView = new DivisioneItemsViewVirtualized(this);
        }

        public void Init(Guid id, EntityTypeViewSettings viewSettings)
        {
            if (id == Guid.Empty)
            {
                Clear();
                return;
            }

            ItemsView.WindowService = WindowService;
            ItemsView.DataService = DataService;
            ItemsView.ModelActionsStack = ModelActionsStack;
            ItemsView.MainOperation = MainOperation;
            ItemsView.Calculator = new ValoreCalculator(DataService);

            ItemsView.Calculator.NoteCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == NoteCalculatorFunction.Name) as NoteCalculatorFunction;
            ItemsView.Calculator.IfcCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == Model3dCalculatorFunction.Names.Ifc) as Model3dCalculatorFunction;
            ItemsView.Calculator.RvtCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == Model3dCalculatorFunction.Names.Rvt) as Model3dCalculatorFunction;


            DivisioneItemsView.Init(id);
            Id = id;

            DataService.Suspended = false;

            if (viewSettings != null)
            {
                RightPanesView.FilterView.Load(viewSettings);
                RightPanesView.SortView.Load(viewSettings);
                RightPanesView.GroupView.Load(viewSettings);
            }

            ItemsView.Load();

            RaisePropertyChanged(GetPropertyName(() => IsEntityTypeValid));

            if (IsItemsSummarized)
            {
                DivisioneItemsView.SetAttributoSum(3);
                DivisioneItemsView.SetAttributoSum(4);

                DivisioneItemsView.CalcolaSummarizeAttributiAsync();
            }
        }

        private void Clear()
        {
            _itemsView = new DivisioneItemsViewVirtualized(this);
        }

        public bool IsEntityTypeValid { get => ItemsView.EntityType != null; }
    }

    public class DivisioneItemsViewVirtualized : MasterDetailTreeSumViewVirtualized
    {
        public DivisioneItemsViewVirtualized(MasterDetailTreeView owner) : base(owner)
        {
        }

        protected override TreeEntityView NewItemView(TreeEntity entity)
        {
            return new DivisioneItemView(this, entity);
        }

        public void Init(Guid id)
        {
            
            base.Init();

            _loadingEntities = new List<TreeEntity>();

            DivisioneItemType divType = new EntitiesHelper(DataService).GetDivisioneTypeById(id);
            string divCodice = divType.Codice;

            string key = DivisioneItemParentType.CreateKey(id);
            EntityParentType = DataService.GetEntityTypes()[key];

            key = DivisioneItemType.CreateKey(id);
            EntityType = DataService.GetEntityTypes()[key];

            AttributiEntities.Load(new HashSet<Guid>());

            if (IsItemsSummarized)
            {
                HeaderAttributo3 = LocalizationProvider.GetString("_Nessuno");
                HeaderAttributo4 = LocalizationProvider.GetString("_Nessuno");
            }

        }
    }

} 
