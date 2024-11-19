using _3DModelExchange;
using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{


    public class MasterDetailViewBase : NotificationBase
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        public List<CalculatorFunction> CalculatorFunctions { get; set; } = new List<CalculatorFunction>();

        protected EntitiesListMasterDetailView _itemsView = null;
        public EntitiesListMasterDetailView ItemsView { get => _itemsView; }

        public MasterDetailViewBase()
        {
        }

        public RightPanesView RightPanesView { get => _itemsView.RightPanesView; }

        public virtual void Init(EntityTypeViewSettings viewSettings)
        {
            //DataService = dataService;
            //WindowService = windowService;
            //ModelActionsStack = modelActionsStack;

            if (DataService == null)
                return;

            _itemsView.WindowService = WindowService;
            _itemsView.DataService = DataService;
            _itemsView.Calculator = new ValoreCalculator(DataService);
            _itemsView.MainOperation = MainOperation;

            _itemsView.Calculator.NoteCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == NoteCalculatorFunction.Name) as NoteCalculatorFunction;
            //_itemsView.Calculator.EPCalculatorFunction = CalculatorFunctions.FirstOrDefault(item => item.GetName() == EPCalculatorFunction.Name) as EPCalculatorFunction;

            _itemsView.ModelActionsStack = ModelActionsStack;
            _itemsView.Init();
            DataService.Suspended = false;

            if (viewSettings != null)
            {
                RightPanesView.FilterView.Load(viewSettings);
                RightPanesView.SortView.Load(viewSettings);
                RightPanesView.GroupView.Load(viewSettings);
            }



            _itemsView.Load();
        }

        public virtual void Clear()
        {
            RightPanesView.Clear();
            RightPanesView.ClosePanes();
            _itemsView.Clear();
        }

        /// <summary>
        /// Sostituisci Attributo Guid di riferimento attRif nell'item corrente
        /// </summary>
        public async void ReplaceCurrentItemDivisione(AttributoRiferimento attRif)
        {
            if (_itemsView.IsMultipleModify && !_itemsView.IsAnyChecked)
                return;

            if (!_itemsView.IsMultipleModify && _itemsView.SelectedEntityId == Guid.Empty)
                return;


            DataService.Suspended = true;


            //MasterDetailListItemView currentItemView = null;
            //if (_itemsView.SelectedEntityId != Guid.Empty)
            //    currentItemView = _itemsView.SelectedEntityView as MasterDetailListItemView;

            EntityView currentItemView = null;
            if (_itemsView.SelectedEntityId != Guid.Empty)
                currentItemView = _itemsView.SelectedEntityView;


            DivisioneItemType divType = DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey] as DivisioneItemType;

            if (divType == null)
                return;

            List<Guid> selectedItems = new List<Guid>();

            if (currentItemView != null) //esiste una posizione di inserimento
            {
                //DivisioneItem divisioneItem = currentItemView.GetDivisioneItem(attRif.ReferenceCodiceGuid, attRif.ReferenceEntityTypeKey);
                //if (divisioneItem != null)
                //    selectedItems.Add(divisioneItem.Id);

                Guid id = currentItemView.GetAttributoGuidId(attRif.ReferenceCodiceGuid);
                if (id != Guid.Empty)
                    selectedItems.Add(id);
                else if (!ItemsView.IsMultipleModify)
                    selectedItems.Add(Guid.Empty);
            }

            string title = LocalizationProvider.GetString("Sostituisci");

            //if (selectCategoriaIdWnd.ShowDialog() == true)
            if (WindowService.SelectDivisioneIdsWindow(divType.DivisioneId, ref selectedItems, title, SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, attRif))
            {
                if (selectedItems.Count == 1)
                {
                    Guid divisioneItemId = selectedItems.First();
                    _itemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = divisioneItemId });

                    await _itemsView.UpdateCache(true);
                }
            }
            else
            {
                //potrei aver fatto modifiche alla divisione
                await _itemsView.UpdateCache(true);
            }


        }

        /// <summary>
        /// Sostituisci Attributo Guid di riferimento attRif nell'item corrente
        /// </summary>
        public async virtual void ReplaceCurrentItemAttributoGuid(AttributoRiferimento attRif, EntityTypeViewSettings viewSettings)
        {
            if (_itemsView.IsMultipleModify && !_itemsView.IsAnyChecked)
                return;

            if (!_itemsView.IsMultipleModify && _itemsView.SelectedEntityId == Guid.Empty)
                return;


            DataService.Suspended = true;
            EntityView currentItemView = null;
            if (_itemsView.SelectedEntityId != Guid.Empty)
                currentItemView = _itemsView.SelectedEntityView;


            EntityType entityType = DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey];

            if (entityType == null)
                return;

            List<Guid> selectedItems = new List<Guid>();

            if (currentItemView != null) //esiste una posizione di inserimento
            {
                Guid id = currentItemView.GetAttributoGuidId(attRif.ReferenceCodiceGuid);
                if (id != Guid.Empty)
                    selectedItems.Add(id);
                else if (!ItemsView.IsMultipleModify)
                    selectedItems.Add(Guid.Empty);
            }

            string title = LocalizationProvider.GetString("Sostituisci");

            if (WindowService.SelectEntityIdsWindow(entityType.GetKey(), ref selectedItems, title, SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, viewSettings, attRif))
            {
                if (selectedItems.Count == 1)
                {
                    Guid entityId = selectedItems.First();
                    _itemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = entityId });

                    await _itemsView.UpdateCache(true);
                }
            }
            else
            {
                //potrei aver fatto modifiche alla divisione
                await _itemsView.UpdateCache(true);
            }


        }

        public void UpdateViewSettings(EntityTypeViewSettings viewSettings)
        {
            if (viewSettings == null)
                return;

            if (_itemsView.RightPanesView.GroupView.Data != null/* && _itemsView.RightPanesView.GroupView.Data.Items.Any()*/)
                viewSettings.Groups = _itemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            if (_itemsView.RightPanesView.SortView.Data != null/* && _itemsView.RightPanesView.SortView.Data.Items.Any()*/)
                viewSettings.Sorts = _itemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            if (_itemsView.RightPanesView.FilterView.Data != null/* && _itemsView.RightPanesView.FilterView.Data.Items.Any()*/)
                viewSettings.Filters = _itemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();
        }

        public void UpdateEntityType()
        {
            _itemsView.UpdateEntityType();
        }

        public virtual void Refresh(int rowIndex) { }


    }




}
