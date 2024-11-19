using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using _3DModelExchange;
using CommonResources;
using Syncfusion.Windows.Controls.Gantt;
using DevExpress.Mvvm.Native;

namespace DivisioniWpf
{
    public class DivisioniView : NotificationBase
    {
        public ClientDataService DataService { get; set; } = null;//ref
        public IEntityWindowService WindowService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref
        //public ViewSettings ViewSettings { get; set; } = null;//ref

        public DivisioneView CurrentDivisioneView { get; set; }
        public ObservableCollection<DivisioniItemView> DivisioniItems { get; set; } = new ObservableCollection<DivisioniItemView>();
        List<string> _divisioniCodice = new List<string>();
        public List<CalculatorFunction> CalculatorFunctions { get; set; } = new List<CalculatorFunction>();



        public void Init()
        {
            int selectedIndex = SelectedIndex;
            Load();

            if (selectedIndex < 0 && _divisioniCodice.Any())
                SelectedIndex = 0;
            else
                SelectedIndex = selectedIndex;



            UpdateUI();
        }

        public void Clear()
        {
            SelectedIndex = -1;

        }

        int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                ViewSettings viewSettings = DataService.GetViewSettings();
                EntityTypeViewSettings divViewSettings = null;
                int precSelectedIndex = _selectedIndex;

                _selectedIndex = value;

                DivisioneItemType divType = null;


                if ( 0 <= SelectedIndex && SelectedIndex < DivisioniItems.Count)
                {
                    if (precSelectedIndex >= 0)
                    {
                        divType = new EntitiesHelper(DataService).GetDivisioneTypeById(DivisioniItems[precSelectedIndex].DivisioneId);
                        

                        string divisioneKeyPrec = DivisioneItemType.CreateKey(DivisioniItems[precSelectedIndex].DivisioneId);

                        if (viewSettings.EntityTypes.ContainsKey(divisioneKeyPrec))
                            divViewSettings = viewSettings.EntityTypes[divisioneKeyPrec];

                        CurrentDivisioneView.UpdateViewSettings(divViewSettings);
                    }


                    DataService.Suspended = true;

                    //nota bene: CurrentDivisioneView è sempre istanziata e unica per tutte le divisioni. Cambia il contenuto spostandosi da una divisione all'altra
                    CurrentDivisioneView.DataService = DataService;
                    CurrentDivisioneView.WindowService = WindowService;
                    CurrentDivisioneView.ModelActionsStack = ModelActionsStack;
                    CurrentDivisioneView.MainOperation = MainOperation;


                    divType = new EntitiesHelper(DataService).GetDivisioneTypeById(DivisioniItems[SelectedIndex].DivisioneId);
                    
                    string divisioneKey  = DivisioneItemType.CreateKey(DivisioniItems[SelectedIndex].DivisioneId);


                    if (viewSettings.EntityTypes.ContainsKey(divisioneKey))
                        divViewSettings = viewSettings.EntityTypes[divisioneKey];

                    CurrentDivisioneView.Init(DivisioniItems[SelectedIndex].DivisioneId, divViewSettings);

                    CurrentDivisioneView.RightPanesView.FilterView.SearchNext();
                }
                UpdateUI();
            }
        }

        public ICommand AddDivisioneCommand
        {
            get
            {
                return new CommandHandler(() => this.AddDivisione());
            }
        }

        public void AddDivisione()
        {
            string newCodiceDivisione = CreateNewCodiceDivisione();

            if (MainOperation.IsAdvancedMode())
            {
                IWindowService windowService = MainOperation.GetWindowService();
                windowService.CodiceAttributoWindow(ref newCodiceDivisione);
            }

            DataService.AddDivisione(newCodiceDivisione);
            Load();

            //riposiziono la selezione
            SelectedIndex = DivisioniItems.Count - 1;
            UpdateUI();


            


        }

        public void Load()
        {
            Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();

            DivisioniItems.Clear();
            foreach (DivisioneItemType div in entTypes.Where(item => item.Value is DivisioneItemType).Select(item => item.Value).OrderBy(item => (item as DivisioneItemType).Position))
            {
                DivisioniItems.Add(new DivisioniItemView(this, DataService, MainOperation) { Name = div.Name, Codice = div.Codice, DivisioneId = div.DivisioneId });
            }

            //_divisioniCodice = DivisioniItems.Select(item => entTypes[DivisioneItemType.CreateKey(item.DivisioneId)].Codice).ToList();

            EntitiesHelper entsHelper = new EntitiesHelper(DataService);

            _divisioniCodice = DivisioniItems.Select(item =>
            {
                DivisioneItemType divType = entsHelper.GetDivisioneTypeById(item.DivisioneId);
                return divType.Codice;
            }).ToList();

            UpdateUI();
            
        }

        public ICommand DeleteDivisioneCommand
        {
            get
            {
                return new CommandHandler(() => this.DeleteDivisione());
            }
        }

        void DeleteDivisione()
        {
            if (SelectedIndex < 0)
                return;

            if (SelectedIndex >= DivisioniItems.Count)
                return;

            if (DivisioniItems.Count <= 1)//almeno una suddivisione
                return;



            List<string> dependentEntityTypes = CurrentDivisioneView.DivisioneItemsView.GetDependentEntityTypesKey();

            int prevSelectedIndex = SelectedIndex;

            ModelActionsStack.UndoGroupBegin(UndoGroupsName.DeleteDivisione, string.Empty);

            Guid divId = DivisioniItems[SelectedIndex].DivisioneId;
            bool res = DataService.RemoveDivisione(divId);
            if (res)
            {
                

                //List<string> dependentEntityTypes = CurrentDivisioneView.DivisioneItemsView.GetDependentEntityTypesKey();
                MainOperation.UpdateEntityTypesView(dependentEntityTypes);

                ModelActionsStack.UndoGroupEnd();

                Load();

                //riposiziono la selezione
                if (prevSelectedIndex >= DivisioniItems.Count)
                    prevSelectedIndex = DivisioniItems.Count - 1;

                SelectedIndex = prevSelectedIndex;
                UpdateUI();
            }
            else
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("EliminazioneDivisioneNonConsentita"));
            }

            ModelActionsStack.UndoGroupCancel();

        }

        public void RenameDivisione(string newName)
        {
            if (SelectedIndex < 0)
                return;

            if (SelectedIndex >= DivisioniItems.Count)
                return;

            List<string> dependentEntityTypes = CurrentDivisioneView.DivisioneItemsView.GetDependentEntityTypesKey();

            Guid divId = DivisioniItems[SelectedIndex].DivisioneId;
            bool res = DataService.RenameDivisione(divId, newName);
            if (res)
            {
                MainOperation.UpdateEntityTypesView(dependentEntityTypes);
            }
        }
        //public ICommand EditingEndCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.EditingEnd());
        //    }
        //}

        //void EditingEnd()
        //{
        //    foreach (var item in DivisioniItems.Where(item => item.IsRenaming))
        //    {
        //        item.EditingEnd();
        //    }
        //}

        public void AddDivisioniItemsByModel3d_old(List<Model3dElementRelation> elmsKey)
        {


            IEnumerable<EntityType> divTypes = DataService.GetEntityTypes().Values.Where(item => item is DivisioneItemType);
            foreach(DivisioneItemType divType in divTypes)
            {
                //if (divType.Model3dClassName == null)
                //    continue;

                if (divType.Model3dClassName == Model3dClassEnum.Nothing)
                    continue;


                List<Model3dObjectKey> divItemsModel3dKey = new List<Model3dObjectKey>();

                //key: "divItemGlobalId | FileName", value: divItemsModel3dKey Index
                Dictionary<string, int> divisioniIndexesByKey = new Dictionary<string, int>();

                //key: elmIndex, value: divItemsModel3dKey Index
                //Dictionary<int, int> divisioniIndexesByElmIndex = new Dictionary<int, int>();
                Dictionary<int, HashSet<int>> divisioniIndexesByElmIndex = new Dictionary<int, HashSet<int>>();


                Model3dValues model3dValues = new Model3dValues();
                int elmIndex = 0;

                for (elmIndex = 0; elmIndex < elmsKey.Count; elmIndex++)
                {
                    Model3dElementRelation elmKey = elmsKey[elmIndex];

                    Model3dValue model3dValue = new Model3dValue();
                    model3dValue.ClassName = divType.Model3dClassName;
                    model3dValue.ProjectGlobalId = elmKey.Model3dObject.ProjectGlobalId;
                    model3dValue.GlobalId = elmKey.Model3dObject.GlobalId;
                    model3dValue.ValuePath = BuiltInCodes.Attributo.GlobalId;
                    model3dValue.Model3DType = elmKey.Model3dObject.Model3DType;


                    model3dValues.Values.Add(model3dValue);

                }


                DataService.CalculateModel3dValues(model3dValues);

                elmIndex = 0;
                for (elmIndex = 0; elmIndex < elmsKey.Count; elmIndex++)
                {
                    Model3dElementRelation elmKey = elmsKey[elmIndex];
                    string divItemGlobalId = model3dValues.Values[elmIndex].Value;

                    string[] divItemsGlobalId = null;
                    if (divItemGlobalId != null)
                        divItemsGlobalId = divItemGlobalId.Split(";");

                    if (divItemsGlobalId != null && divItemsGlobalId.Length > 1)
                    {
                        //elemento associato a più gruppi

                        divisioniIndexesByElmIndex.Add(elmIndex, new HashSet<int>());

                        foreach (string divItemGlobalId2 in divItemsGlobalId)
                        {
                            int divItemIndex = -1;
                            if (divItemGlobalId2 != null && divItemGlobalId2.Any())
                            {
                                string key = string.Join("|", divItemGlobalId2, elmKey.Model3dObject.ProjectGlobalId);
                                if (divisioniIndexesByKey.ContainsKey(key))
                                {
                                    divItemIndex = divisioniIndexesByKey[key];
                                    divisioniIndexesByElmIndex[elmIndex].Add(divItemIndex);

                                    divisioniIndexesByKey.TryAdd(key, divItemIndex);
                                }
                                else
                                {
                                    divItemsModel3dKey.Add(new Model3dObjectKey() { GlobalId = divItemGlobalId2, ProjectGlobalId = elmKey.Model3dObject.ProjectGlobalId });
                                    
                                    divItemIndex = divItemsModel3dKey.Count - 1;
                                    divisioniIndexesByElmIndex[elmIndex].Add(divItemIndex);

                                    divisioniIndexesByKey.Add(key, divItemIndex);
                                }
                            }
                        }
                    }
                    else
                    {

                        int divItemIndex = -1;
                        if (divItemGlobalId != null && divItemGlobalId.Any())
                        {
                            string key = string.Join("|", divItemGlobalId, elmKey.Model3dObject.ProjectGlobalId);
                            if (divisioniIndexesByKey.ContainsKey(key))
                            {
                                divItemIndex = divisioniIndexesByKey[key];
                            }
                            else
                            {
                                divItemsModel3dKey.Add(new Model3dObjectKey() { GlobalId = divItemGlobalId, ProjectGlobalId = elmKey.Model3dObject.ProjectGlobalId });
                                divItemIndex = divItemsModel3dKey.Count - 1;
                                divisioniIndexesByKey.Add(key, divItemIndex);
                            }
                        }

                        divisioniIndexesByElmIndex.Add(elmIndex, new HashSet<int>());

                        if (divItemIndex >= 0)
                            divisioniIndexesByElmIndex[elmIndex].Add(divItemIndex);
                    }
                }

                //N.B. divItemsModel3dKey e divItemsId sono due liste allineate
                List<Guid> divItemsId = AddDivisioniItemsByModel3d(divType, divItemsModel3dKey);
                for (elmIndex=0; elmIndex < elmsKey.Count; elmIndex++)
                {
                    if (elmIndex < divisioniIndexesByElmIndex.Count)
                    {
                        foreach (int divItemsIndex in divisioniIndexesByElmIndex[elmIndex])
                        {

                            //int divItemsIndex = divisioniIndexesByElmIndex[elmIndex];

                            string divTypeKey = divType.GetKey();

                            elmsKey[elmIndex].Divisioni.TryAdd(divTypeKey, new HashSet<DivisioneItemId>());

                            if (divItemsIndex >= 0)
                                elmsKey[elmIndex].Divisioni[divTypeKey].Add(new DivisioneItemId() { Id = divItemsId[divItemsIndex], EntityTypeKey = divTypeKey, ItemPath = string.Empty });

                        }
                    }
                }

            }


        }

        public void AddDivisioniItemsByModel3d(List<Model3dElementRelation> elmsKey)
        {



            Dictionary<string, Model3dValue> valuesMap = new Dictionary<string, Model3dValue>();

            Model3dValues model3dValues = new Model3dValues();
            int elmIndex = 0;

            for (elmIndex = 0; elmIndex < elmsKey.Count; elmIndex++)
            {
                Model3dElementRelation elmKey = elmsKey[elmIndex];

                foreach (string divTypeKey in elmKey.Divisioni.Keys)
                {
                    var divType = DataService.GetEntityType(divTypeKey) as DivisioneItemType;

                    foreach (var divItemId in elmKey.Divisioni[divTypeKey])
                    {
                        Model3dValue model3dValue = new Model3dValue();
                        model3dValue.ClassName = divType.Model3dClassName;
                        model3dValue.ProjectGlobalId = elmKey.Model3dObject.ProjectGlobalId;
                        model3dValue.GlobalId = elmKey.Model3dObject.GlobalId;
                        model3dValue.ValuePath = BuiltInCodes.Attributo.GlobalId;
                        model3dValue.Model3DType = elmKey.Model3dObject.Model3DType;
                        model3dValue.ItemPath = divItemId.ItemPath;
                        
                        model3dValues.Values.Add(model3dValue);


                        string valuesMapKey = string.Format("{0}|{1}|{2}", elmIndex, divTypeKey, divItemId.ItemPath);
                        valuesMap.TryAdd(valuesMapKey, model3dValue);

                    }
                }
            }

            DataService.CalculateModel3dValues(model3dValues);


            IEnumerable<EntityType> divTypes = DataService.GetEntityTypes().Values.Where(item => item is DivisioneItemType);

            var groupsByClassName = model3dValues.Values.GroupBy(item => item.ClassName);

            /////////////////////////////////
            //Aggiungo le divisioni e creo la mappa (divTypeKey, (divItemModel3dKey, divItemId))
            Dictionary<string, Dictionary<string, Guid>> divisioniMap = new Dictionary<string, Dictionary<string, Guid>>();

            foreach (var className in groupsByClassName)
            {

                DivisioneItemType divType = divTypes.FirstOrDefault(item => (item as DivisioneItemType).Model3dClassName == className.Key) as DivisioneItemType;

                if (divType == null)
                    continue;

                string divTypeKey = divType.GetKey();

                divisioniMap.TryAdd(divTypeKey, new Dictionary<string, Guid>());
                var divItemsMap = divisioniMap[divTypeKey];

                List<Model3dObjectKey> divItemsModel3dKey = new List<Model3dObjectKey>();
                HashSet<string> divItemsModel3dKeyKeys = new HashSet<string>();

                foreach (var item in className)
                {
                    var divItemsGlobalId = item.Value;

                    string[] divItemsGlobalIdArray = null;
                    if (divItemsGlobalId != null)
                    {
                        divItemsGlobalIdArray = divItemsGlobalId.Split(";");

                        foreach (string divItemGlobalId2 in divItemsGlobalIdArray)
                        {
                            if (!string.IsNullOrEmpty(divItemGlobalId2))
                            {
                                var model3dObjKey = new Model3dObjectKey() { GlobalId = divItemGlobalId2, ProjectGlobalId = item.ProjectGlobalId, Model3DType = item.Model3DType };
                                var key = model3dObjKey.GetKey();
                                if (!divItemsModel3dKeyKeys.Contains(key))
                                {
                                    divItemsModel3dKey.Add(model3dObjKey);
                                    divItemsModel3dKeyKeys.Add(key);
                                }
                            }
                        }
                    }
                }


                //N.B. divItemsModel3dKey e divItemsId sono due liste allineate
                List<Guid> divItemsId = AddDivisioniItemsByModel3d(divType, divItemsModel3dKey);


                for (int i = 0; i< divItemsModel3dKey.Count; i++)
                {
                    var divItemModel3dKey = divItemsModel3dKey[i];
                    divItemsMap.TryAdd(divItemModel3dKey.GetKey(), divItemsId[i]);
                }


            }

            //////////////////////////////////////////////////////////////
            
    
            for (elmIndex = 0; elmIndex < elmsKey.Count; elmIndex++)
            {
                Model3dElementRelation elmKey = elmsKey[elmIndex];

                foreach (string divTypeKey in elmKey.Divisioni.Keys)
                {
                    var divType = DataService.GetEntityType(divTypeKey) as DivisioneItemType;

                    List<string> valuesMapKeys = elmKey.Divisioni[divTypeKey].Select(item => string.Format("{0}|{1}|{2}", elmIndex, divTypeKey, item.ItemPath)).ToList();

                    elmKey.Divisioni[divTypeKey].Clear();//????

                    foreach (var valuesMapKey in valuesMapKeys)
                    {
                        Model3dValue model3dValue = null;
                        if (valuesMap.TryGetValue(valuesMapKey, out model3dValue))
                        {
                            var divItemsGlobalId = model3dValue.Value;

                            string[] divItemsGlobalIdArray = null;
                            if (divItemsGlobalId != null)
                            {
                                divItemsGlobalIdArray = divItemsGlobalId.Split(";");

                                foreach (string divItemGlobalId2 in divItemsGlobalIdArray)
                                {
                                    Model3dObjectKey divItemModel3dKey = new Model3dObjectKey() { GlobalId = divItemGlobalId2, ProjectGlobalId = elmKey.Model3dObject.ProjectGlobalId, Model3DType = elmKey.Model3dObject.Model3DType };

                                    Guid id = Guid.Empty;
                                    divisioniMap[divTypeKey].TryGetValue(divItemModel3dKey.GetKey(), out id);

                                    elmKey.Divisioni[divTypeKey].Add(new DivisioneItemId() { EntityTypeKey = divTypeKey, ItemPath = model3dValue.ItemPath, Id = id });
                                }
                            }

                        }
                    }
                }
            }
        }


        private List<Guid> AddDivisioniItemsByModel3d(DivisioneItemType divType, List<Model3dObjectKey> divItemsKey)
        {

            //N.B. guidsReturned e divItemsKey sono liste allineate

            List<Guid> guidsReturned = new List<Guid>();
            divItemsKey.ForEach(item => guidsReturned.Add(Guid.Empty));

            //Costruisco una nuova chiave per controllare i doppioni
            string sepInKey = "|";
            Dictionary<string, Guid> divItemIdsByKey = DataService.CreateKey(divType.GetKey(), sepInKey, new List<string>() { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId }, out _);
            //

            ModelActionResponse mar;
            ModelAction action = new ModelAction() { EntityTypeKey = divType.GetKey() };
            action.ActionName = ActionName.MULTI_NODEPENDENTS;


            List<int> divItemsKeyIndex = new List<int>();

            int index = 0;
            for(index = 0; index < divItemsKey.Count; index++)
            {
                Model3dObjectKey divItemModel3dKey = divItemsKey[index];

                //controllo il doppione (se fileName e globalId uguali
                string key = string.Join(sepInKey, divItemModel3dKey.ProjectGlobalId, divItemModel3dKey.GlobalId);
                if (divItemIdsByKey.ContainsKey(key))
                {
                    guidsReturned[index] = divItemIdsByKey[key];
                    continue;
                }
                else
                {
                    divItemsKeyIndex.Add(index);
                }


                ModelAction addAction = new ModelAction() { EntityTypeKey = divType.GetKey() };
                addAction.ActionName = ActionName.TREEENTITY_ADD;

                //azione di modifica attributo
                ModelAction attActionGlobalId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.GlobalId, EntitiesId = null, EntityTypeKey = divType.GetKey() };
                attActionGlobalId.NewValore = new ValoreTesto() { V = divItemModel3dKey.GlobalId };
                addAction.NestedActions.Add(attActionGlobalId);

                ModelAction attActionFileName = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.ProjectGlobalId, EntitiesId = null, EntityTypeKey = divType.GetKey() };
                attActionFileName.NewValore = new ValoreTesto() { V = divItemModel3dKey.ProjectGlobalId };
                addAction.NestedActions.Add(attActionFileName);

                action.NestedActions.Add(addAction);

            }

            //mar = ModelActionsStack.CommitAction(action, null);
            mar = DataService.CommitAction(action);

            if (mar.ActionResponse == ActionResponse.OK)
            {
                //Aggiornamento della divisione corrente (attualmente visualizzata)
                if (CurrentDivisioneView != null && CurrentDivisioneView.ItemsView != null
                    && CurrentDivisioneView.ItemsView.EntityType != null && CurrentDivisioneView.ItemsView.EntityType.GetKey() == divType.GetKey())
                {
                    CurrentDivisioneView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);

                    CurrentDivisioneView.ItemsView.ApplyFilterAndSort(mar.NewIds.FirstOrDefault());

                    CurrentDivisioneView.ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                    CurrentDivisioneView.ItemsView.UpdateCache();

                    RaisePropertyChanged(GetPropertyName(() => this.CurrentDivisioneView.ItemsView.EntitiesCount));
                }

                //N.B. newIds e divItemsKeyIndex sono due liste allineate
                List<Guid> newIds = mar.NewIds.ToList();

                for (index = 0; index < newIds.Count; index++)
                {
                    guidsReturned[divItemsKeyIndex[index]] = newIds[index];
                }
            }

            return guidsReturned;
        }

        internal string CreateNewCodiceDivisione()
        {

            string candidate = null;
            int i = -1;
            while (i < 1000)
            {
                i++;
                candidate = i.ToString();
                if (!_divisioniCodice.Contains(candidate))
                    break;
            }
            _divisioniCodice.Add(candidate);

            return candidate;
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => SelectedIndex));
            RaisePropertyChanged(GetPropertyName(() => IsAnyReadyToPaste));
            RaisePropertyChanged(GetPropertyName(() => IsMoveDivisioneEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMoveDivisioneNotificationEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMoveDivisioneAfterEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsMoveDivisioneWaitingForTarget));
            RaisePropertyChanged(GetPropertyName(() => IsAddDivisioneEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsDeleteDivisioneEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsEscapeEnabled));
            RaisePropertyChanged(GetPropertyName(() => DivisioniItems));

        }

        public bool IsAddDivisioneEnabled { get => !IsMoveDivisioneAfterEnabled;  }
        public bool IsDeleteDivisioneEnabled { get => !IsMoveDivisioneAfterEnabled; }

        #region Drag&Drop

        public Guid ReadyToModifyDivisioneId { get; set; } = Guid.Empty;

        public ICommand MoveDivisioneCommand => new CommandHandler(() => this.PrepareToMoveDivisione());
        public virtual void PrepareToMoveDivisione()
        {
            if (CurrentDivisioneView == null)
                return;

            if (CurrentDivisioneView.Id == Guid.Empty)
                return;

            ReadyToModifyDivisioneId = CurrentDivisioneView.Id;
            PasteDivisioneToolTip = LocalizationProvider.GetString("Sposta dopo l'elemento corrente");

            DivisioniItems.ForEach(item => item.UpdateUI());
            UpdateUI();
        }
        public ICommand PasteDivisioneCommand => new CommandHandler(() => this.PasteDivisione());
        void PasteDivisione()
        {
            if (CurrentDivisioneView == null)
                return;

            EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            Guid targetDivId = CurrentDivisioneView.Id;

            int readyToModifyDivPosition =  entsHelper.GetDivisioneTypeById(ReadyToModifyDivisioneId).Position;
            int targetDivPosition = entsHelper.GetDivisioneTypeById(targetDivId).Position;

            
            IEnumerable<DivisioneItemType> divTypes = DataService.GetEntityTypes().Values.Where(item => item is DivisioneItemType).Cast<DivisioneItemType>();

            Dictionary<Guid, double> divTypesPosTemp = divTypes.ToDictionary(item => item.DivisioneId, item => (double) item.Position);

            divTypesPosTemp[ReadyToModifyDivisioneId] = divTypesPosTemp[targetDivId] + 0.5;

            Dictionary<Guid, int> divTypesPos = new Dictionary<Guid, int>();
            int index = 0;
            foreach(var keyVal in divTypesPosTemp.OrderBy(item => item.Value))
            {
                divTypesPos.Add(keyVal.Key, index++);
            }

            DataService.SortDivisioni(divTypesPos);

            Load();
            SelectedIndex = divTypesPos[ReadyToModifyDivisioneId];
            Escape();
        }

        string _pasteDivisioneToolTip = "";
        public string PasteDivisioneToolTip
        {
            get { return _pasteDivisioneToolTip; }
            set { SetProperty(ref _pasteDivisioneToolTip, value); }
        }

        public bool IsAnyReadyToPaste { get { return ReadyToModifyDivisioneId != Guid.Empty; } }

        public bool IsMoveDivisioneEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                return true;
            }
        }

        public bool IsMoveDivisioneNotificationEnabled
        {
            get => IsAnyReadyToPaste;// && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move;
        }

        public bool IsMoveDivisioneWaitingForTarget
        {
            get => IsMoveDivisioneNotificationEnabled && !IsMoveDivisioneAfterEnabled;
        }

        public bool IsMoveDivisioneAfterEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;


                if (CurrentDivisioneView == null)
                    return false;

                if (!IsAnyReadyToPaste)
                    return false;

                return CurrentDivisioneView.Id != ReadyToModifyDivisioneId;
            }
        }


        public ICommand EscapeCommand
        {
            get
            {
                return new CommandHandler(() => this.Escape());
            }
        }

        public virtual void Escape()
        {
            ReadyToModifyDivisioneId = Guid.Empty;
            DivisioniItems.ForEach(item => item.UpdateUI());
            UpdateUI();
        }

        public bool IsEscapeEnabled
        {
            get => IsMoveDivisioneAfterEnabled;
        }


        #endregion

    }

    public class DivisioniItemView : NotificationBase
    {
        ClientDataService DataService { get; set; }
        IMainOperation MainOperation { get; set; }
        DivisioniView _owner = null;

        public Guid DivisioneId { get; set; }
        
        public DivisioniItemView(DivisioniView owner, ClientDataService dataService, IMainOperation mainOperation)
        {
            DataService = dataService;
            MainOperation = mainOperation;
            _owner = owner;
        }

        /// <summary>
        /// Nome della divisione
        /// </summary>
        string _name = null;
        public string Name
        {
            get { return _name; }
            set
            {
                string oldName = _name;
                _name = value;

                //DataService.RenameDivisione(DivisioneId, _name);
                if (oldName != null && oldName != _name)
                    _owner.RenameDivisione(_name);
                else
                    DataService.RenameDivisione(DivisioneId, _name);

                UpdateUI();
                
            }
         }

        public bool IsNameNull { get => _name == null; }

        /// <summary>
        /// Codice della divisione
        /// </summary>
        string _codice = string.Empty;
        public string Codice
        {
            get => _codice;
            set
            {
                _codice = value;
                DataService.RenameDivisione(DivisioneId, _name, _codice);
            }
        }

        public bool IsReadyToPaste { get => _owner.ReadyToModifyDivisioneId == DivisioneId; }

        public bool IsAdvancedMode { get => MainOperation.IsAdvancedMode(); }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsNameNull));
            RaisePropertyChanged(GetPropertyName(() => IsAdvancedMode));
            RaisePropertyChanged(GetPropertyName(() => IsReadyToPaste));
        }


    }

}
