using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Commons;
using MasterDetailModel;
using CommonResources;
using Model;
using DevExpress.Data.Extensions;

namespace MasterDetailView
{


    public abstract class EntitiesTreeMasterDetailView : EntitiesListMasterDetailView
    {

        public EntitiesTreeMasterDetailView() : base()
        {
        }

        
        public EntityType EntityParentType { get; set; }
        

        public class TreeEntityViewInfo
        {
            public TreeEntityViewInfo() { }
            public TreeEntityViewInfo(TreeEntityMasterInfo entInfo)
            {
                //Depth = entInfo.Depth;
                ParentId = entInfo.ParentId;
            }

            public bool IsExpanded { get; set; } = false;
            //
            public int Depth = 0;//mi serve per calcolare la DisplayedDescendantCount anche degli elementi non realizzati (viene aggiornato quando viene realizzata di nuovo l'entità )
            public Guid ParentId = Guid.Empty;
            //public int FilteredIndex = -1;
            public int DisplayedIndex = -1;

            public TreeEntityViewInfo Clone()
            {
                string json = "";
                JsonSerializer.JsonSerialize(this, out json);

                TreeEntityViewInfo clone = null;
                JsonSerializer.JsonDeserialize(json, out clone, GetType());

                return clone;
            }
        }

        /// <summary>
        /// Informazioni sulle entità ritornate da ApplyFilterAndSort
        /// </summary>
        public new Dictionary<Guid, TreeEntityViewInfo> FilteredEntitiesViewInfo = new Dictionary<Guid, TreeEntityViewInfo>();

        /// <summary>
        /// Lista degli Id delle entità filtrate mappate con corrispondenti ListViewItem
        /// </summary>
        public List<Guid> DisplayedEntitiesId { get; set; } = new List<Guid>();


        //public override async Task ApplyFilterAndSort(Guid? selectEntityId = null, bool searchOnly = false)
        public override void ApplyFilterAndSort(Guid? selectEntityId = null, bool searchOnly = false)
        {
            try
            {

                if (EntityType == null)
                    return;

                IsBusy = true;

                RightPanesView.SortView.CreateData();
                List<Guid> entitiesFound = null;

                List<TreeEntityMasterInfo> entitiesInfo = DataService.GetFilteredTreeEntities(EntityType.GetKey(), RightPanesView.FilterView.Data, RightPanesView.SortView.Data, out entitiesFound);

                FilteredKeys = entitiesInfo.ToDictionary(item => item.Id, item => item.ComparerKey);
                LoadAlert();

                if (!searchOnly)
                {

                    UpdateFilteredEntitiesId(entitiesInfo);
                    UpdateDisplayedEntitiesId(entitiesInfo);

                    Guid selectedEntityId = Guid.Empty;
                    if (selectEntityId.HasValue)
                        selectedEntityId = selectEntityId.Value;

                    _selectedEntityId = selectedEntityId;
                }

                RightPanesView.FilterView.UpdateSearch(entitiesFound);

                if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Nothing)
                    ReadyToModifyEntitiesId.Clear();

                CheckedEntitiesId.IntersectWith(FilteredEntitiesId);
                IsMultipleModify = false;

                IsBusy = false;
            }
            catch (Exception e )
            {
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
                //log.Trace(e.ToString());
            }
        }

        void UpdateFilteredEntitiesId(List<TreeEntityMasterInfo> entitiesInfo)
        {
            //Aggiorno FilteredEntitiesId
            FilteredEntitiesId = entitiesInfo.Select(item => item.Id).ToList();
            FilteredIndexes = FilteredEntitiesId.ToDictionary(item => item, item => -1);

            Dictionary<Guid, TreeEntityViewInfo> newFilteredEntitiesViewInfo = entitiesInfo.ToDictionary(item => item.Id ,item => new TreeEntityViewInfo(item));

            for (int i=0; i<FilteredEntitiesId.Count; i++)
            {
                Guid id = FilteredEntitiesId[i];

                //aggiorno depth da parentId

                int depth = 0;
                Guid parentId = newFilteredEntitiesViewInfo[id].ParentId;
                while (parentId != Guid.Empty)
                {
                    depth++;
                    parentId = newFilteredEntitiesViewInfo[parentId].ParentId;
                }
                newFilteredEntitiesViewInfo[id].Depth = depth;

                FilteredIndexes[id] = i;

                //mantengo gli item expanded
                if (FilteredEntitiesViewInfo.ContainsKey(id))
                    newFilteredEntitiesViewInfo[id].IsExpanded = FilteredEntitiesViewInfo[id].IsExpanded;

            }

            FilteredEntitiesViewInfo = newFilteredEntitiesViewInfo;

        }


        
        void UpdateDisplayedEntitiesId(List<TreeEntityMasterInfo> entitiesInfo)
        {
            //Cerco la lista delle entità che o non hanno padre o ce l'hanno expanded
            DisplayedEntitiesId = entitiesInfo.Where(item =>
            {
                Guid parentId = FilteredEntitiesViewInfo[item.Id].ParentId;
                if (parentId == Guid.Empty || FilteredEntitiesViewInfo[parentId].IsExpanded)
                    return true;

                return false;
            }).Select(item => item.Id).ToList();

            //Aggiorno gli indici di FilteredEntitiesViewInfo
            MapDisplayedIndexes();
        }

        public virtual void MapDisplayedIndexes()
        {
            //Aggiorno gli indici di FilteredEntitiesViewInfo

            foreach (TreeEntityViewInfo t in FilteredEntitiesViewInfo.Values)
                t.DisplayedIndex = -1;
            
            for (int i = 0; i < DisplayedEntitiesId.Count; i++)
                FilteredEntitiesViewInfo[DisplayedEntitiesId[i]].DisplayedIndex = i;

        }

        public bool DisplayedContains(Guid id)
        {
            if (FilteredEntitiesViewInfo.ContainsKey(id) && FilteredEntitiesViewInfo[id].DisplayedIndex >= 0)
                return true;

            return false;
        }

        public int DisplayedIndexOf(Guid id)
        {
            if (FilteredEntitiesViewInfo.ContainsKey(id))
                return FilteredEntitiesViewInfo[id].DisplayedIndex;
            else
                return -1;
        }

        public TreeEntityView SelectedTreeEntityView { get { return _selectedEntityView as TreeEntityView; } }

        public override bool SelectEntityView(EntityView entView)
        {
            try
            {

                if (entView == null)
                {
                    _selectedEntityView = null;
                    AttributiEntities.Load(new HashSet<Guid>());
                    return false;
                }


                if (_selectedEntityView != null && _selectedEntityView.Id == entView.Id)
                {
                    //Per far in modo che non ricarichi la vista degli attributi quando modifico dalla lista degli attributi
                    if (_selectedEntityView != entView)
                    {
                        _selectedEntityView = entView;
                    }
                    
                    //UpdateUI();
                    return false;
                }

                _selectedEntityView = entView;

                //AttributiEntities.Load(entView);
                LoadAttributiDetailAsync(entView);

                //IsMultipleModify = false;

                //UpdateUI();
                RightPanesView.FilterView.UpdateIsSearchEnabled();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public override void SelectIndex(int index, bool userOrigin = false)
        {
            if (SetProperty(ref _selectedIndex, index))
            {

                if (userOrigin && _selectedIndex < 0) //per la selezione e fuoco del primo elemento del tree
                    return;


                EntityView entView = GetEntityViewByIndex(_selectedIndex);
                if (entView != null)
                {
                    _selectedEntityId = entView.Id;
                    SelectEntityView(entView);
                    SetNoSelectionChecked(false);
                    RightPanesView.FilterView.UpdateCurrentEntityIndexFound(_selectedEntityId);
                }
                else
                {
                    _selectedIndex = -1;
                    _selectedEntityId = Guid.Empty;
                    _selectedEntityView = null;
                }

                //NB: commentato perchè bisogna che vengano selezionate i figli anche se se non userOrigin (ad esempio dopo una cancellazione)
                if (/*userOrigin && */_selectedIndex >= 0)
                {
                    if (userOrigin)
                    {
                        if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify)
                            IsMultipleModify = false;

                        CheckedEntitiesId.Clear();
                    }

                    if (_selectedEntityId != Guid.Empty)
                    {
                        FirstShiftCheckEntityId = _selectedEntityId;
                        CheckEntityById(_selectedEntityId, true);

                    }
                }
            }

            UpdateUI();
        }

        public override bool IsNoSelectionChecked
        {
            get { return _isNoSelectionChecked; }
            set
            {
                if (SetProperty(ref _isNoSelectionChecked, value))
                {
                    ClearCheckedEntities();
                    SelectIndex(-1);// SelectedIndex = -1;
                    SelectEntityView(null);
                }
            }
        }

        //        public override async void CheckBySelection(Guid selectedEntityId, SelectionKeys kk = SelectionKeys.Nothing)
        //        {
        //            if (kk == SelectionKeys.Ctrl)
        //            {

        //            }
        //            else if (kk == SelectionKeys.Shift)
        //            {
        //                //OnShiftChecked(selectedEntityId);
        //            }
        //            else
        //            {
        //                CheckedEntitiesId.Clear();
        //                CheckedEntitiesId.Add(selectedEntityId);
        //                CheckDescendantsOf(selectedEntityId, true);
        //            }

        //}

        public override Attributo GetAttributoByCode(string attCode)
        {
            //Attributo att = EntityType.Attributi.FirstOrDefault(item => item.Codice == attCode);
            //if (att == null)
            //    att = EntityParentType.Attributi.FirstOrDefault(item => item.Codice == attCode);

            Attributo att = null;
            if (EntityType.Attributi.ContainsKey(attCode))
                att = EntityType.Attributi[attCode];
            else
                att = EntityParentType.Attributi[attCode];

            return att;
        }

        #region Add Commands

        public ICommand AddEntityCommand
        {
            get
            {
                return new CommandHandler(() => this.Add());
            }
        }

        public void Add()
        {
            //RaisePropertyChanged(GetPropertyName(() => AddEntityParentText));
            //RaisePropertyChanged(GetPropertyName(() => AddEntityChildText));
            //RaisePropertyChanged(GetPropertyName(() => AddEntityText));

            //if (IsAnySelected)
            //    AddSiblingEntity();
            //else
                AddEntity();


        }

        public ICommand AddEntityChildCommand
        {
            get
            {
                return new CommandHandler(() => this.AddEntityChild());
            }
        }

        /// <summary>
        /// Inserisce una nuova entità in fondo in coda ai figli della selezionata
        /// </summary>
        public async void AddEntityChild()
        {
            Entity targetEntity = GetActionTargetEntity();

            if (targetEntity == null)
                return;

            if (!IsValidAddRequestByFilter())
                return;

            /////////////////////////////////////////////////
            //controllo se è consentito inserire un figlio
            TreeEntity treeTargetEnt = targetEntity as TreeEntity;
            if (!treeTargetEnt.IsParent)
            {
                List<string> sezioniKey = new List<string>();
                List<string> depEntTypesKey = EntitiesHelper.GetDependentEntityTypesKey(EntityType.GetKey());
                foreach (string depEntType in depEntTypesKey)
                {
                    List<Guid> depIds = DataService.GetDependentIds(EntityType.GetKey(), treeTargetEnt.EntityId, depEntType);
                    if (depIds != null && depIds.Count > 0)
                    {
                        sezioniKey.Add(depEntType);
                    }
                }

                if (sezioniKey.Any())
                {
                    Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();
                    IEnumerable<string> sezioniName = sezioniKey.Select(item => entTypes[item].Name);

                    string msg1 = LocalizationProvider.GetString("NonEPossibileInserireUnaVoceSubordinata");
                    string msg2 = string.Format("{0}: {1}", LocalizationProvider.GetString("Sezioni"), string.Join(", ", sezioniName));
                    string msg3 = LocalizationProvider.GetString("VuoiInserireALivelloSuperiore");
                    string msg = string.Format("{0}\n{1}\n\n{2}", msg1, msg2, msg3);

                    if (MessageBox.Show(msg, LocalizationProvider.GetString("AppName"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        AddEntityParent();
                    }

                    return;
                }
            }
            ///////////////////////////////////////////////////
         


            ModelActionResponse mar;
            ModelAction action = new ModelAction() { EntityTypeKey = this.EntityType.GetKey() };

            bool clearAttributiFilter = SetValoriOfCurrentFilter(action/*, targetEntity*/);


            if (SelectedIndex >= 0 && SelectedIndex < DisplayedEntitiesId.Count && SelectedEntityView != null) //inserisco figlio di SelectedIndex
            {
                //Aggiorno in remoto
                action.ActionName = ActionName.TREEENTITY_ADD_CHILD;
                action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = DisplayedEntitiesId[SelectedIndex] } };
                //mar = ModelActionsStack.CommitAction(action, this);
                mar = CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    if (clearAttributiFilter)
                        RightPanesView.FilterView.ClearNextAttributiFilter(0);

                    CheckedEntitiesId.Clear();
                    CheckedEntitiesId.Add(mar.NewId);
                    FilteredEntitiesViewInfo[SelectedEntityId].IsExpanded = true;
                    ApplyFilterAndSort(mar.NewId);
                    UpdateCache();
                }
            }

            RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));

        }


        public string AddEntityChildToolTip
        {
            get
            {
                if (SelectedEntityView != null)
                {
                    string str = SelectedEntityView.Attributo1 != null && SelectedEntityView.Attributo1.Any() ? SelectedEntityView.Attributo1 : SelectedEntityView.Attributo2;
                    return LocalizationProvider.GetString("Aggiungi inferiore dell'elemento corrente") + " \"" + str + "\"";
                    //return StringResoucesTemp.GetResourceString("MasterTreeView_AddChild") + " \"" + str + "\"";
                }
                return "";
            }
        }

        public bool IsAddChildEnabled
        {
            get
            {

                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return IsAddEnabled && SelectedTreeEntityView != null && SelectedTreeEntityView.CanBeParent() && !IsMoveEntitiesAfterEnabled;
            }
        }

        public bool IsAddParentEnabled
        {
            get
            {


                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                return IsAnyChecked && !IsMoveEntitiesAfterEnabled;
            }
        }

        public ICommand AddEntityParentCommand
        {
            get
            {
                return new CommandHandler(() => this.AddEntityParent());
            }
        }

        public async void AddEntityParent()
        {
            try
            {

                ModelActionResponse mar;
                ModelAction action = new ModelAction() { EntityTypeKey = this.EntityType.GetKey() };
                action.ActionName = ActionName.TREEENTITY_ADD_PARENT;

                Entity targetEntity = GetActionTargetEntity();

                bool clearAttributiFilter = SetValoriOfCurrentFilter(action/*, targetEntity*/);

                //Aggiorno in remoto
                action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = DisplayedEntitiesId[SelectedIndex] } };
                mar = CommitAction(action);

                if (mar.ActionResponse == ActionResponse.OK)
                {
                    if (clearAttributiFilter)
                        RightPanesView.FilterView.ClearNextAttributiFilter(0);

                    CheckedEntitiesId.Clear();
                    CheckedEntitiesId.Add(mar.NewId);
                    FilteredEntitiesViewInfo.Add(mar.NewId, new TreeEntityViewInfo() { IsExpanded = true });

                    ApplyFilterAndSort(mar.NewId);
                    _selectedIndex = -1;//per forzare l'aggiornamento (l'item selezionato precedentemente ha lo stesso index)
                    await UpdateCache();

                    RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));
                }
            }
            catch (Exception e)
            {

            }
        }

        public string AddEntityParentToolTip
        {
            get
            {
                if (SelectedEntityView != null)
                {
                    string str = SelectedEntityView.Attributo1 != null && SelectedEntityView.Attributo1.Any() ? SelectedEntityView.Attributo1 : SelectedEntityView.Attributo2;
                    return LocalizationProvider.GetString("Aggiungi superiore dell'elemento corrente") + " \"" + str + "\"";
                    //return StringResoucesTemp.GetResourceString("MasterTreeView_AddParent") + " \"" + str + "\"";
                }
                return "";
            }
        }




        //public ICommand AddSiblingEntityCommand
        //{
        //    get
        //    {
        //        return new CommandHandler(() => this.AddSiblingEntity());
        //    }
        //}

        //public void AddSiblingEntity()
        //{
        //    AddEntity();
        //}


        
        /// <summary>
        /// Inserisce una nuova entità come fratello (sotto) alla selezionata oppure in coda se nessuna entità è selezionata
        /// </summary>
        public override async void AddEntity()
        {
            if (EntityType == null)
                return;

            if (!IsValidAddRequestByFilter())
                return;


            ModelActionResponse mar;
            ModelAction action = new ModelAction() { EntityTypeKey = this.EntityType.GetKey() };

            Entity targetEntity = GetActionTargetEntity();

            bool clearAttributiFilter = SetValoriOfCurrentFilter(action/*, targetEntity*/);

            if (SelectedIndex >= 0 && SelectedIndex < DisplayedEntitiesId.Count - 1 && SelectedEntityView != null) //inserisco dopo SelectedIndex
            {
                //Aggiorno server
                action.ActionName = ActionName.TREEENTITY_INSERT;
                action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = DisplayedEntitiesId[SelectedIndex], TargetReferenceName = TargetReferenceName.AFTER } };
                //mar = ModelActionsStack.CommitAction(action, this);
                mar = CommitAction(action);

                if (mar.ActionResponse == ActionResponse.OK)
                {
                    //UpdateFilteredAndDisplayedIds(mar.NewId);
                    //SaveScrollPosition();

                    if (clearAttributiFilter)
                        RightPanesView.FilterView.ClearNextAttributiFilter(0);

                    CheckedEntitiesId.Clear();
                    CheckedEntitiesId.Add(mar.NewId);
                    ApplyFilterAndSort(mar.NewId);
                    await UpdateCache();
                }
            }
            else if (SelectedIndex == DisplayedEntitiesId.Count - 1 || SelectedEntityView == null)//aggiungo in coda
            {
                if (DisplayedEntitiesId.Any())
                {
                    action.ActionName = ActionName.TREEENTITY_INSERT;
                    action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = DisplayedEntitiesId[DisplayedEntitiesId.Count-1], TargetReferenceName = TargetReferenceName.AFTER } };
                }
                else
                {
                    action.ActionName = ActionName.TREEENTITY_ADD;
                }

                
                //mar = ModelActionsStack.CommitAction(action, this);
                mar = CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    if (clearAttributiFilter)
                        RightPanesView.FilterView.ClearNextAttributiFilter(0);

                    CheckedEntitiesId.Clear();
                    CheckedEntitiesId.Add(mar.NewId);
                    ApplyFilterAndSort(mar.NewId);
                    await UpdateCache();
                }
                //SelectEntityById(mar.NewId);
                
            }

            RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));

        }

        #endregion Add Commands

        protected override bool SetValoriOfCurrentFilter(ModelAction pasteAction/*, Entity targetEntity*/)
        {
            bool res = false;

            //if (targetEntity == null)
            //    return res;

            IEnumerable<AttributoFilterData> filterAtt = RightPanesView.FilterView.Data.Items.Where(item => item.IsFiltroAttivato);

            foreach (AttributoFilterData filAtt in filterAtt)
            {
                if (filAtt.CheckedValori.Count != 1)
                    continue;

                string codiceAtt = filAtt.CodiceAttributo;
                string valStr = filAtt.CheckedValori.First();

                TreeEntityType treeEntType = EntityType as TreeEntityType;
                if (pasteAction.ActionName == ActionName.TREEENTITY_ADD_PARENT)
                {
                    if (!treeEntType.AssociedType.Attributi.ContainsKey(codiceAtt))
                        continue;
                }
                else
                {
                    if (!EntityType.Attributi.ContainsKey(codiceAtt))
                        continue;
                }

                Attributo att = EntityType.Attributi[codiceAtt];
                Valore newVal = att.ValoreDefault.Clone();
                newVal.SetByString(valStr);


                ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
                attAction.NewValore = newVal;

                if (!(pasteAction.NestedActions.Select(item => item.AttributoCode).Contains(attAction.AttributoCode)))//se non è già stata prevista una azione sullo stesso codice (ad esempio è già stato assegnato l'articolo da dialogo)
                    pasteAction.NestedActions.Add(attAction);

                res = true;

            }

            return res;
        }

        /// <summary>
        /// Conta il numero delle entità discendenti nella lista Displayed
        /// </summary>
        /// <param name="treeEntView"></param>
        /// <returns></returns>
        public int DisplayedDescendantCountOf(Guid entityId)
        {
            int DisplayedDescendantCount = 0;

            int depth = 0;
            //int startingIndex = DisplayedEntitiesId.IndexOf(entityId);
            int startingIndex = DisplayedIndexOf(entityId);
            int index = startingIndex;
            do
            {
                index++;
                if (index < DisplayedEntitiesId.Count)
                    depth = FilteredEntitiesViewInfo[DisplayedEntitiesId[index]].Depth;

            } while (depth > FilteredEntitiesViewInfo[entityId].Depth && index < DisplayedEntitiesId.Count);

            DisplayedDescendantCount = index - startingIndex - 1;

            return DisplayedDescendantCount;
        }


        public override void CheckEntityById(Guid id, bool check)
        {
            base.CheckEntityById(id, check);
            CheckDescendantsOf(id, check);
        }

        /// <summary>
        /// Checka o dechecka tutti i discendenti di treeEntView
        /// </summary>
        /// <param name="treeEntView"></param>
        /// <param name="check"> check o uncheck</param>
        public void CheckDescendantsOf(Guid treeEntId, bool check)
        {
            List<Guid> filteredIds = FilteredDescendantsId(treeEntId);

            CheckEntitiesById(filteredIds, check);
        }


        public bool IsAnyDescendantChecked(TreeEntityView treeEntView)
        {
            List<Guid> filteredDescendantsId = FilteredDescendantsId(treeEntView.Id);

            foreach (Guid id in filteredDescendantsId)
            {
                if (CheckedEntitiesId.Contains(id))
                    return true;
            }
            return false;
            
        }

        public List<Guid> GetChildrenOf(Guid entityId)
        {
            List<Guid> childrenId = FilteredEntitiesId.Where(item => FilteredEntitiesViewInfo[item].Depth == FilteredEntitiesViewInfo[entityId].Depth + 1 && FilteredEntitiesViewInfo[item].ParentId == entityId).ToList();
            return childrenId;
        }



        /// <summary>
        /// Ricava gli Id di tutti i discendenti
        /// </summary>
        /// <param name="treeEntView"></param>
        /// <returns></returns>
        public List<Guid> FilteredDescendantsId(Guid entityId)
        {
            //List<Guid> descendantsId = new List<Guid>();

            //descendantsId = GetChildrenOf(entityId);
            //foreach (Guid id in descendantsId)
            //    descendantsId.AddRange(FilteredDescendantsId(id));

            //return descendantsId;

            int treeEntViewIndex;
            int filteredDescendantCount = FilteredDescendantCountOf(entityId, out treeEntViewIndex);


            List<Guid> filteredIds = FilteredEntitiesId.GetRange(treeEntViewIndex + 1, filteredDescendantCount);
            return filteredIds;
        }

        /// <summary>
        /// Ritorna il numero dei discendenti di treeEntViewId
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityIndex"></param>
        /// <returns></returns>
        public int FilteredDescendantCountOf(Guid entityId, out int entityIndex)
        {
            int depth = 0;
            //entityIndex = FilteredEntitiesId.IndexOf(entityId);
            entityIndex = FilteredIndexOf(entityId);
            if (entityIndex < 0)
                return 0;

            int index = entityIndex;
            do
            {
                index++;
                if (index < FilteredEntitiesId.Count)
                    depth = FilteredEntitiesViewInfo[FilteredEntitiesId[index]].Depth;

            } while (depth > FilteredEntitiesViewInfo[entityId].Depth && index < FilteredEntitiesId.Count);

            int filteredDescendantCount = index - entityIndex - 1;
            return filteredDescendantCount;
        }

        /// <summary>
        /// Ritorna true se entityId ha figli
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool HasChildren(Guid entityId)
        {
            //int index = FilteredEntitiesId.IndexOf(entityId);
            int index = FilteredIndexOf(entityId);

            if (index < 0)
                return false;

            if (index >= FilteredEntitiesId.Count - 1)
                return false;

            if (!FilteredEntitiesViewInfo.Any())
                return false;

            if (FilteredEntitiesViewInfo[FilteredEntitiesId[index + 1]].Depth > FilteredEntitiesViewInfo[FilteredEntitiesId[index]].Depth)
                return true;

            return false;
        }

        ///// <summary>
        ///// Ritorna il parent di entId
        ///// </summary>
        ///// <param name="entityId"></param>
        ///// <returns></returns>
        //public Guid GetParentOf(Guid entityId)
        //{
        //    if (entityId == Guid.Empty)
        //        return Guid.Empty;

        //    int entIndex = FilteredEntitiesId.IndexOf(entityId);

        //    if (entIndex < 0)
        //        return Guid.Empty;

        //    int entDepth = FilteredEntitiesViewInfo[entityId].Depth;
        //    Guid parent = Guid.Empty;
        //    while (entIndex > 0)
        //    {
        //        entIndex--;
        //        if (FilteredEntitiesViewInfo[FilteredEntitiesId[entIndex]].Depth < entDepth)
        //        {
        //            parent = FilteredEntitiesId[entIndex];
        //            break;
        //        }
                
        //    }
        //    return parent;
        //}

        /// <summary>
        /// Verifica se un'entità è figlio unico
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool IsEntityOnlyChild(Guid entityId)
        {
            int depth = FilteredEntitiesViewInfo[entityId].Depth;
            //int index = FilteredEntitiesId.IndexOf(entityId);
            int index = FilteredIndexOf(entityId);

            if (depth <= 0)
                return false;

            if (index > 0 && depth == FilteredEntitiesViewInfo[FilteredEntitiesId[index - 1]].Depth)//entità precedente è fratello
                return false;

            if (index < FilteredEntitiesId.Count - 1 && FilteredEntitiesViewInfo[FilteredEntitiesId[index + 1]].Depth == depth) //entità successiva è fratello
                return false;

            return true;


        }



        /// <summary>
        /// Mostra l'entità con Id entityId e tutti i suoi padri
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public int ShowEntity(Guid entityId)
        {
            //int indexInFiltered = FilteredEntitiesId.IndexOf(entityId);
            int indexInFiltered = FilteredIndexOf(entityId);
            //int indexInDisplayed = DisplayedEntitiesId.IndexOf(entityId);
            int indexInDisplayed = DisplayedIndexOf(entityId);

            if (indexInFiltered < 0)
                return -1;

            if (indexInDisplayed < 0)
            {
                int depth = FilteredEntitiesViewInfo[entityId].Depth;
                while (depth >= FilteredEntitiesViewInfo[entityId].Depth && indexInFiltered >= 0)
                {
                    indexInFiltered--;
                    if (indexInFiltered < 0)
                        break;
                    depth = FilteredEntitiesViewInfo[FilteredEntitiesId[indexInFiltered]].Depth;

                }

                ShowEntity(FilteredEntitiesId[indexInFiltered]);

                ExpandEntityById(FilteredEntitiesId[indexInFiltered], true);

            }

            //return DisplayedEntitiesId.IndexOf(entityId);
            return DisplayedIndexOf(entityId);
        }

        /// <summary>
        /// Include nella Displayed list le entità in entitiesId
        /// </summary>
        /// <param name="entitiesId"></param>
        public override void ShowEntities(List<Guid> entitiesId)
        {
            foreach (Guid id in entitiesId)
            {
                ShowEntity(id);
            }
        }


        public override bool IsMultipleModifyEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsMoveEntitiesAfterEnabled)
                    return false;
                ////Modifica multipla consentita solo con elementi dello stesso tipo
                //if (CheckedEntitiesId.Count > 1)
                //{
                //    if (FilteredEntitiesViewInfo.Where(item => CheckedEntitiesId.Contains(item.Key)).All(item => HasChildren(item.Key) == true))
                //        return true;

                //    if (FilteredEntitiesViewInfo.Where(item => CheckedEntitiesId.Contains(item.Key)).All(item => HasChildren(item.Key) == false))
                //        return true;
                //}

                //Modifica multipla consentita solo sulle foglie (viene consentita se c'è almeno una foglia nella selezione)
                if (CheckedEntitiesId.Count > 1)
                {
                    IEnumerable<Guid> entitiesIdWithoutChildren = FilteredEntitiesViewInfo.Where(item => CheckedEntitiesId.Contains(item.Key) && !HasChildren(item.Key)).Select(item => item.Key);
                    if (entitiesIdWithoutChildren.Count() >= 2)
                        return true;
                    
                }

                    return false;
            }
        }

        public override bool IsMultipleModify
        {
            get { return _multipleModify; }
            set
            {
                if (SetProperty(ref _multipleModify, value))
                {
                    if (_multipleModify)
                    {
                        //cerco le entità foglie selezionate
                        HashSet<Guid> entitiesIdWithoutParents = new HashSet<Guid>(FilteredEntitiesViewInfo.Where(item => CheckedEntitiesId.Contains(item.Key) && !HasChildren(item.Key)).Select(item => item.Key));

                        AttributiEntities.Load(entitiesIdWithoutParents);
                        ReadyToModifyEntitiesId = new HashSet<Guid>(entitiesIdWithoutParents);
                        ReadyToModifyEntitiesCommand = ReadyToPasteEntitiesCommands.MultiModify;
                    }
                    else
                    {
                        ReadyToModifyEntitiesId.Clear();

                        if (SelectedEntityView != null)
                            AttributiEntities.Load(SelectedEntityView);
                        else
                            AttributiEntities.Load(new HashSet<Guid>());
                    }
                    UpdateUI();


                }
            }

        }


        public override async void OnCheckedEntity()
        {
            if (!IsModelToViewLoading)
            {
                base.OnCheckedEntity();
            }
        }

        public override bool IsAnyChecked
        {
            get
            {
                return CheckedEntitiesId.Count > 0;
            }
        }

        protected override void CheckUnreferenced()
        {

            HashSet<Guid> refIds = new HashSet<Guid>();
            foreach (Guid id in FilteredEntitiesId)
            {
                List<string> sezioniKey = new List<string>();
                List<string> depEntTypesKey = EntitiesHelper.GetDependentEntityTypesKey(EntityType.GetKey());
                foreach (string depEntType in depEntTypesKey)
                {
                    List<Guid> depIds = DataService.GetDependentIds(EntityType.GetKey(), id, depEntType);
                    if (depIds != null && depIds.Count > 0)
                    {
                        refIds.Add(id);

                        Guid parentId = FilteredEntitiesViewInfo[id].ParentId;
                        while (parentId != null && parentId != Guid.Empty)
                        {
                            refIds.Add(parentId);
                            parentId = FilteredEntitiesViewInfo[parentId].ParentId;
                        }
                    }
                }
            }

            ClearFocus();

            IEnumerable<Guid> unrefIds = FilteredEntitiesId.Where(item => !refIds.Contains(item));

            ShowEntities(unrefIds.ToList());
            CheckedEntitiesId = new HashSet<Guid>(unrefIds);

            if (unrefIds.Any())
                SelectEntityById(unrefIds.First());
            else
                SelectEntityById(Guid.Empty);

        }

        public async override void Load()
        {
            base.Load();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => this.IsAddChildEnabled));
            RaisePropertyChanged(GetPropertyName(() => this.IsAddParentEnabled));
            RaisePropertyChanged(GetPropertyName(() => this.IsMoveEntitiesChildOfEnabled));

            RaisePropertyChanged(GetPropertyName(() => AddEntityParentToolTip));
            RaisePropertyChanged(GetPropertyName(() => AddEntityChildToolTip));

            
        }


        /// <summary>
        /// Cancellazione Entità (flag deleted)
        /// </summary>
        public override async void DeleteCheckedEntities()
        {
            try
            {

                //////////////////////////////////////////
                ///calcolo del nuovo elemento da selezionare
                Guid newSelectedId = SelectedEntityId;
                int selectedIndex = DisplayedIndexOf(newSelectedId);

                while (0<= selectedIndex && selectedIndex < DisplayedEntitiesId.Count)
                {
                    Guid id = DisplayedEntitiesId[selectedIndex];
                    if (!CheckedEntitiesId.Contains(id))
                        break;

                    selectedIndex++;
                }

                if (selectedIndex >= DisplayedEntitiesId.Count)
                    selectedIndex = 0;

                DisplayedEntitiesId.TryGetValue(selectedIndex, out newSelectedId);
                /////////////////////////////////////////////



                ModelActionResponse mar = CommitAction(new ModelAction() { ActionName = ActionName.TREEENTITY_DELETE, EntitiesId = CheckedEntitiesId, EntityTypeKey = this.EntityType.GetKey() });
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    //Guid newSelectedId = SelectedEntityId;

                    
                    //if (newSelectedId == Guid.Empty)//se non c'è nessun elemento prima dei selizionati cerco dopo
                    //{
                    //    //La nuova selezione sarà la prima entità (non cancellata) dopo di SelectedEntityId
                    //    while (CheckedEntitiesId.Contains(newSelectedId))
                    //    {
                    //        int index = DisplayedIndexOf(newSelectedId) + 1;
                    //        if (index <= 0 || index >= DisplayedEntitiesId.Count)
                    //            newSelectedId = Guid.Empty;
                    //        else
                    //            newSelectedId = DisplayedEntitiesId[index];
                    //    }
                    //}

                    ApplyFilterAndSort(newSelectedId);

                    if (!FilteredEntitiesId.Any())
                        newSelectedId = Guid.Empty;

                    if (newSelectedId != Guid.Empty)
                        CheckedEntitiesId = new HashSet<Guid>() { newSelectedId };
                    else
                        CheckedEntitiesId.Clear();

                    SelectEntityView(null);
                    UpdateCache();

                    IsMultipleModify = false;
                    UpdateUI();

                    //RightPanesView.FilterView.Update(null);//aggiorna tutti i filtri attivi

                }

            }
            catch (Exception e)
            {
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
                //log.Trace(e.ToString());

            }
        }



        #region Move - Sposta
        public override async void MoveEntities()
        {
            if (SelectedEntityId == null)
                return;

            int ind = DisplayedIndexOf(SelectedEntityId);


            List<Guid> entitiesCuttedId = FilteredEntitiesId.Where(item => ReadyToModifyEntitiesId.Contains(item)).ToList();

            Guid targetEntityId = FilteredEntitiesId.FirstOrDefault(item => item == SelectedEntityId);

            if (entitiesCuttedId.Contains(targetEntityId))
                return;

            IsBusy = true;


            ModelAction action = new ModelAction() { EntityTypeKey = EntityType.GetKey(), ActionName = ActionName.TREEENTITY_MOVE };
            Guid lastBaseDepthId = Guid.Empty;
            for (int i = 0; i < entitiesCuttedId.Count; i++)
            {
                //Cerco il padre con livello più basso 
                //Guid parentOfParents = GetParentOf(entitiesCuttedId[i]);
                Guid parentOfParents = FilteredEntitiesViewInfo[entitiesCuttedId[i]].ParentId;
                while (parentOfParents != Guid.Empty)
                {
                    //Guid par = GetParentOf(parentOfParents);
                    Guid par = FilteredEntitiesViewInfo[parentOfParents].ParentId;
                    if (par == Guid.Empty)
                        break;
                    //parentOfParents = GetParentOf(parentOfParents);
                    parentOfParents = FilteredEntitiesViewInfo[parentOfParents].ParentId;
                }


                action.EntitiesId.Add(entitiesCuttedId[i]);

                if (i == 0)
                {
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = targetEntityId, TargetReferenceName = TargetReferenceName.AFTER });
                    lastBaseDepthId = entitiesCuttedId[i];
                }
                else if (parentOfParents != lastBaseDepthId)
                {
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = lastBaseDepthId, TargetReferenceName = TargetReferenceName.AFTER });
                    lastBaseDepthId = parentOfParents;
                }
                else
                    //action.NewTargetEntitiesId.Add(new TargetReference() { Id = Guid.Empty, TargetReferenceName = TargetReferenceName.NOT_DEFINED });
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = entitiesCuttedId[i - 1], TargetReferenceName = TargetReferenceName.AFTER });
            }

            HashSet<Guid> readyToModifyEntitiesId = new HashSet<Guid>(ReadyToModifyEntitiesId);

            //Update server
            //ModelActionResponse mar = ModelActionsStack.CommitAction(action, this);
            ModelActionResponse mar = CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                CheckedEntitiesId = readyToModifyEntitiesId;

                ReadyToModifyEntitiesId.Clear();
                ApplyFilterAndSort(CheckedEntitiesId.First());
                await UpdateCache();
                //UpdateIsAllChecked();
            }

            IsBusy = false;

        }

        public ICommand PasteEntitiesChildOfCommand
        {
            get
            {
                return new CommandHandler(() => this.PasteEntitiesChildOf());
            }
        }

        public void PasteEntitiesChildOf()
        {
            if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Move)
                this.MoveEntitiesChildOf();
            else
                this.PasteClipboardEntities(TargetReferenceName.CHILD_OF);
        }

        public async void MoveEntitiesChildOf()
        {
            if (SelectedEntityId == null)
                return;

            List<Guid> entitiesCuttedId = FilteredEntitiesId.Where(item => ReadyToModifyEntitiesId.Contains(item)).ToList();

            Guid targetEntityId = FilteredEntitiesId.FirstOrDefault(item => item == SelectedEntityId);

            if (entitiesCuttedId.Contains(targetEntityId))
                return;

            ModelAction action = new ModelAction() { EntityTypeKey = EntityType.GetKey(), ActionName = ActionName.TREEENTITY_MOVE_CHILD_OF };
            for (int i = 0; i < entitiesCuttedId.Count; i++)
            {
                action.EntitiesId.Add(entitiesCuttedId[i]);

                if (i == 0)
                {
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = targetEntityId, TargetReferenceName = TargetReferenceName.CHILD_OF });
                }
                else
                    action.NewTargetEntitiesId.Add(new TargetReference() { Id = entitiesCuttedId[i - 1], TargetReferenceName = TargetReferenceName.AFTER });
            }


            //List<Guid> entitiesToMoveId = action.EntitiesId.ToList();

            //Update server
            //ModelActionsStack.CommitAction(action, this);
            CommitAction(action);

            FilteredEntitiesViewInfo[targetEntityId].IsExpanded = true;
            ReadyToModifyEntitiesId.Clear();

            ApplyFilterAndSort(entitiesCuttedId.First());
            CheckedEntitiesId = new HashSet<Guid>(entitiesCuttedId);
            await UpdateCache();
            //UpdateIsAllChecked();

        }

        public bool IsMoveEntitiesChildOfEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode() && !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                if (/*!RightPanesView.SortView.IsApplied() && */FilteredEntitiesId.Any())
                {
                    HashSet<Guid> filteredEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
                    if (filteredEntitiesId.Overlaps(ReadyToModifyEntitiesId) && SelectedEntityView != null)
                    {
                        if (ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.MultiModify)
                            return false;

                        if (ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                            return false;

                        if (SelectedTreeEntityView == null || !SelectedTreeEntityView.CanBeParent())
                            return false;

                        return true;
                    }
                }

                return false;
            }
        }

        string _pasteEntitiesChildOfToolTip = "";
        public string PasteEntitiesChildOfToolTip
        {
            get { return _pasteEntitiesChildOfToolTip; }
            set { SetProperty(ref _pasteEntitiesChildOfToolTip, value); }
        }

        public override void PrepareToMoveEntities()
        {
            PasteEntitiesChildOfToolTip = LocalizationProvider.GetString("Sposta inferiore dell'elemento corrente");
            //PasteEntitiesChildOfToolTip = StringResoucesTemp.GetResourceString("MasterTreeView_MoveChildOf");
            base.PrepareToMoveEntities();
        }

        public override void CopyEntities()
        {
            PasteEntitiesChildOfToolTip = LocalizationProvider.GetString("Incolla inferiore dell'elemento corrente");
            //PasteEntitiesChildOfToolTip = StringResoucesTemp.GetResourceString("MasterTreeView_PasteChildOf");
            base.CopyEntities();
        }

        #endregion Move - Sposta


        #region Expand/Collapse




        //public override void ExpandCheckedEntities()
        //{

        //    foreach (Guid id in CheckedEntitiesId)
        //        FilteredEntitiesViewInfo[id].IsExpanded = true;

        //    DisplayedEntitiesId = new List<Guid>(FilteredEntitiesId);
        //    MapDisplayedIndexes();

        //    Guid currentSelected = _selectedEntityId;
        //    SelectEntityView(null);
        //    Load();
        //    SelectEntityById(currentSelected);

        //}

        public override void ExpandCheckedEntities()
        {

            IEnumerable<Guid> inverseIndexOrderedIds = CheckedEntitiesId.OrderBy(item => FilteredIndexOf(item)).Reverse();

            foreach (Guid entityId in inverseIndexOrderedIds)
            {
                if (FilteredEntitiesViewInfo[entityId].IsExpanded == true)
                    continue;

                if (!HasChildren(entityId))
                    continue;

                int filteredIndex = FilteredIndexOf(entityId);
                if (filteredIndex < 0)
                    continue;

                FilteredEntitiesViewInfo[entityId].IsExpanded = true;

                int displayedIndex = DisplayedIndexOf(entityId);
                if (displayedIndex < 0)
                    continue;

                List<Guid> childrenId = new List<Guid>();
                int depth = FilteredEntitiesViewInfo[entityId].Depth;
                int index = filteredIndex;
                do
                {
                    index++;
                    if (index >= FilteredEntitiesId.Count)
                        break;

                    depth = FilteredEntitiesViewInfo[FilteredEntitiesId[index]].Depth;
                    if (depth >= FilteredEntitiesViewInfo[entityId].Depth + 1)
                        childrenId.Add(FilteredEntitiesId[index]);


                } while (depth > FilteredEntitiesViewInfo[entityId].Depth);

                if (displayedIndex < DisplayedEntitiesId.Count - 1)
                    DisplayedEntitiesId.InsertRange(displayedIndex + 1, childrenId);
                else
                    DisplayedEntitiesId.AddRange(childrenId);

                //FilteredEntitiesViewInfo[entityId].IsExpanded = true;

                
            }
            
            MapDisplayedIndexes();
            SelectEntityById(_selectedEntityId);

        }

        /// <summary>
        /// Expand tree item by Id (only one level)
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="expand"></param>
        /// <returns>return id to select</returns>
        public Guid ExpandEntityById(Guid entityId, bool expand)
        {
            if (FilteredEntitiesViewInfo[entityId].IsExpanded == expand)
                return SelectedEntityId;

            if (!HasChildren(entityId))
                return SelectedEntityId;

            if (expand)
            {

                int filteredIndex = FilteredIndexOf(entityId);
                if (filteredIndex < 0)
                    return SelectedEntityId;

                int displayedIndex = DisplayedIndexOf(entityId);
                if (displayedIndex < 0)
                    return SelectedEntityId;

                List<Guid> childrenId = new List<Guid>();
                int depth = FilteredEntitiesViewInfo[entityId].Depth;
                int index = filteredIndex;
                do
                {
                    index++;
                    if (index >= FilteredEntitiesId.Count)
                        break;

                    depth = FilteredEntitiesViewInfo[FilteredEntitiesId[index]].Depth;
                    if (depth == FilteredEntitiesViewInfo[entityId].Depth + 1)
                        childrenId.Add(FilteredEntitiesId[index]);


                } while (depth > FilteredEntitiesViewInfo[entityId].Depth);

                if (displayedIndex < DisplayedEntitiesId.Count - 1)
                    DisplayedEntitiesId.InsertRange(displayedIndex + 1, childrenId);
                else
                    DisplayedEntitiesId.AddRange(childrenId);

                FilteredEntitiesViewInfo[entityId].IsExpanded = expand;

                MapDisplayedIndexes();

            }
            else
            {
                //int index = DisplayedEntitiesId.IndexOf(entityId);
                int index = DisplayedIndexOf(entityId);
                int descendantCount = DisplayedDescendantCountOf(entityId);
                for (int i = index; i < index + descendantCount; i++)
                    FilteredEntitiesViewInfo[DisplayedEntitiesId[i]].IsExpanded = false;

                DisplayedEntitiesId.RemoveRange(index + 1, descendantCount);

                FilteredEntitiesViewInfo[entityId].IsExpanded = expand;

                MapDisplayedIndexes();

                if (index < _selectedIndex && _selectedIndex <= index + descendantCount)
                {
                    CheckedEntitiesId.Add(DisplayedEntitiesId[index]);
                    return DisplayedEntitiesId[index];
                }

                //if (_selectedIndex > index + descendantCount)
                //    _selectedIndex -= descendantCount;
                //else if (_selectedIndex > index)
                //    _selectedIndex = index;
            }

            return SelectedEntityId;
        }

        //public override void  CollapseAll()
        //{
        //    //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
        //    //log.Trace("CollapseAll");

        //    foreach (KeyValuePair<Guid, TreeEntityViewInfo> entry in FilteredEntitiesViewInfo)
        //    {
        //        entry.Value.IsExpanded = false;

        //    }
        //    DisplayedEntitiesId.RemoveAll(item => FilteredEntitiesViewInfo[item].Depth != 0);
        //    MapDisplayedIndexes();

        //    //sposto la selezione al parent a depth 0
        //    Guid entityToSelectId = _selectedEntityId;
        //    if (entityToSelectId != Guid.Empty)
        //    {
        //        //Guid parentId = GetParentOf(_selectedEntityId);
        //        Guid parentId = FilteredEntitiesViewInfo[_selectedEntityId].ParentId;
        //        while (parentId != Guid.Empty)
        //        {
        //            entityToSelectId = parentId;
        //            //parentId = GetParentOf(parentId);
        //            parentId = FilteredEntitiesViewInfo[parentId].ParentId;
        //        }

        //        SelectEntityView(null);
        //    }


        //    Load();
        //    SelectEntityById(entityToSelectId);



        //}

        public override void CollapseAll()
        {
            //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
            //log.Trace("CollapseAll");

            foreach (KeyValuePair<Guid, TreeEntityViewInfo> entry in FilteredEntitiesViewInfo)
            {
                entry.Value.IsExpanded = false;

            }
            DisplayedEntitiesId.RemoveAll(item => FilteredEntitiesViewInfo[item].Depth != 0);
            MapDisplayedIndexes();

            //sposto la selezione al parent a depth 0
            Guid entityToSelectId = _selectedEntityId;
            if (entityToSelectId != Guid.Empty)
            {
                Guid parentId = Guid.Empty;
                TreeEntityViewInfo treeEntViewInfo = null;
                if (FilteredEntitiesViewInfo.TryGetValue(_selectedEntityId, out treeEntViewInfo))
                    parentId = treeEntViewInfo.ParentId;
                
                while (parentId != Guid.Empty)
                {
                    entityToSelectId = parentId;
                    //parentId = GetParentOf(parentId);
                    parentId = FilteredEntitiesViewInfo[parentId].ParentId;
                }

                SelectEntityView(null);
            }

            //Load();
            SelectEntityById(entityToSelectId);
        }

        #endregion Expand/Collapse


        #region Copy&Paste
        //public override void CopyClipboardEntities(bool withHeaders, IEnumerable<Guid> entsId, IEnumerable<string> selectionAttsCode)
        //{
        //    IsBusy = true;

        //    try
        //    {

        //        TreeEntityCollection entityCollection = new TreeEntityCollection();

        //        bool compactTreeFormat = true;

        //        if (entsId == null)
        //            entsId = FilteredEntitiesId.Where(item => CheckedEntitiesId.Contains(item));

        //        entityCollection.TreeEntities = DataService.GetTreeEntitiesById(EntityType.GetKey(), entsId, compactTreeFormat);
        //        Clipboard.Clear();

        //        DataObject dataObject = new DataObject();


        //        //Json (copia l'intera entità a prescindere da selectionAttsCode)
        //        string json = null;
        //        JsonSerializer.JsonSerialize(entityCollection, out json);
        //        dataObject.SetData(EntityType.GetKey(), json);

        //        //Text
        //        string text = "";
        //        if (selectionAttsCode == null)
        //            selectionAttsCode = EntityType.Attributi.Select(item => item.Key);
        //        if (withHeaders)
        //        {
        //            foreach (Attributo att in EntityType.Attributi.Values)
        //            {
        //                string val = att.Etichetta;
        //                text += val;
        //                text += "\t";
        //            }
        //            text += "\r\n";
        //        }

        //        foreach (Entity ent in entityCollection.TreeEntities)
        //        {
        //            foreach (string attCode in selectionAttsCode)
        //            {
        //                if (ent.Attributi.ContainsKey(attCode))
        //                {
        //                    EntityAttributo entAtt = ent.Attributi[attCode];
        //                    //Valore val = ent.GetValoreAttributo(entAtt.AttributoCodice, true, true); 
        //                    //string valStr = val.ToPlainText();
        //                    string valStr = EntitiesHelper.GetValorePlainText(ent, entAtt.AttributoCodice, true, true);

        //                    text += valStr;
        //                    text += "\t";
        //                }
        //            }
        //            //ent.AddValoriAttributiToTabbedString(ref text, selectionAttsCode);
        //            text += "\r\n";
        //        }

        //        dataObject.SetData(DataFormats.Text, text);
        //        Clipboard.SetDataObject(dataObject);

        //    }
        //    catch (Exception e)
        //    {
        //        //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
        //        //log.Trace("CopyEntities: " + e.ToString());
        //    }

        //    IsBusy = false;
        //}
        public override void CopyClipboardEntities(IEnumerable<Guid> entsId)
        {
            IsBusy = true;

            try
            {

                TreeEntityCollection entityCollection = new TreeEntityCollection();

                bool compactTreeFormat = true;

                if (entsId == null)
                    entsId = FilteredEntitiesId.Where(item => CheckedEntitiesId.Contains(item));

                entityCollection.TreeEntities = DataService.GetTreeEntitiesById(EntityType.GetKey(), entsId, compactTreeFormat);
                Clipboard.Clear();

                DataObject dataObject = new DataObject();
                
                string json = null;
                JsonSerializer.JsonSerialize(entityCollection, out json);
                dataObject.SetData(EntityType.GetKey(), json);

                Clipboard.SetDataObject(dataObject);

            }
            catch (Exception e)
            {
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
                //log.Trace("CopyEntities: " + e.ToString());
            }

            IsBusy = false;
        }

        public override void CopyTextClipboardEntities(IEnumerable<Guid> entsId)
        {
            IsBusy = true;

            try
            {

                TreeEntityCollection entityCollection = new TreeEntityCollection();

                bool compactTreeFormat = true;

                if (entsId == null)
                    entsId = FilteredEntitiesId.Where(item => CheckedEntitiesId.Contains(item));

                entityCollection.TreeEntities = DataService.GetTreeEntitiesById(EntityType.GetKey(), entsId, compactTreeFormat);
                Clipboard.Clear();

                DataObject dataObject = new DataObject();

                ///////////////////////TEXT///////////////////////////
                string text = "";

                IEnumerable<string> attsCode = EntityType.Attributi.Values.Where(item =>
                {
                    if (!item.IsVisible)
                        return false;

                    var sourceAtt = EntitiesHelper.GetSourceAttributo(item);

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                        return false;

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                        return false;

                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.TestoCollection)
                        return false;

                    return true;
                })
                .OrderBy(item => item.DetailViewOrder)
                .Select(item => item.Codice);


                //copio di headers
                foreach (string attCode in attsCode)
                {
                    string val = EntityType.Attributi[attCode].Etichetta;
                    text += val;
                    text += "\t";
                }
                text += "\r\n";


                //copio i dati
                for (int i = 0; i < entityCollection.TreeEntities.Count; i++)
                {
                    TreeEntity ent = entityCollection.TreeEntities[i];
                    foreach (string attCode in attsCode)
                    {
                        string valStr = "";


                        Valore val = EntitiesHelper.GetValoreAttributo(ent, attCode, false, false);
                        if (val is ValoreReale)
                        {
                            valStr = ((ValoreReale)val).RealResult?.ToString();
                        }
                        else if (val is ValoreContabilita)
                        {
                            valStr = ((ValoreContabilita)val).RealResult?.ToString(/*CultureInfo.InvariantCulture*/);
                        }
                        else if (val is ValoreTestoRtf)
                        {
                            valStr = val.ToPlainText();
                            valStr = valStr.Replace("\r\n", "¬");
                        }
                        else if (val != null)
                            valStr = val.ToPlainText();

                        //if (ent.Attributi.ContainsKey(attCode)) 
                        //{
                        //    if (ent.Attributi[attCode].Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                        //    {
                        //        ValoreReale valReale = ent.Attributi[attCode].Valore as ValoreReale;
                        //        valStr = valReale.RealResult.ToString();
                        //    }
                        //    else if (ent.Attributi[attCode].Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        //    {
                        //        ValoreContabilita valCont = ent.Attributi[attCode].Valore as ValoreContabilita;
                        //        valStr = valCont.RealResult.ToString();
                        //    }
                        //    else
                        //        valStr = EntitiesHelper.GetValorePlainText(ent, attCode, false, true);
                        //        //valStr = EntitiesHelper.GetValorePlainText(ent, attCode, true, true);
                        //}

                        text += valStr;
                        text += "\t";
                    }

                    text += "\r\n";
                }

                dataObject.SetData(DataFormats.Text, text);
                /////////////////////////////////////////////////////////////////////////////////////////


                Clipboard.SetDataObject(dataObject);

            }
            catch (Exception e)
            {
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<EntitiesTreeMasterDetailView>();
                //log.Trace("CopyEntities: " + e.ToString());
            }

            IsBusy = false;
        }

        public override async void PasteClipboardEntities()
        {
            PasteClipboardEntities(TargetReferenceName.AFTER);
        }

        public async void PasteClipboardEntities(TargetReferenceName targetRef)
        {
            IsBusy = true;
            //Guid firstTarget = SelectedEntityId;
            Guid firstTarget = Guid.Empty;
            if (SelectedEntityView != null)
                firstTarget = SelectedEntityView.Id;

            ModelAction pasteAction = new ModelAction() { ActionName = ActionName.TREEENTITIES_PASTE, EntityTypeKey = EntityType.GetKey()};

            if (firstTarget != Guid.Empty)
                pasteAction.NewTargetEntitiesId = new List<TargetReference>() { new TargetReference() { Id = firstTarget, TargetReferenceName = targetRef } };

            IDataObject dataObject = Clipboard.GetDataObject();


            if (dataObject.GetDataPresent(EntityType.GetKey()))
            {
                string json = dataObject.GetData(EntityType.GetKey()) as string;

                pasteAction.JsonSerializedObject = json;

            }
            else if (dataObject.GetDataPresent(DataFormats.Text))
            {
                string text = dataObject.GetData(DataFormats.Text) as string;
                
                string[] lines = Regex.Split(text, "\r\n|\r|\n");

                TreeEntityCollection entityCollection = new TreeEntityCollection();
                entityCollection.TreeEntities = new List<TreeEntity>();

                foreach (string line in lines)
                {
                    if (line.Trim().Length == 0)
                        continue;

                    TreeEntity localEntity = DataService.NewEntity(EntityType.GetKey()) as TreeEntity;

                    if (localEntity.AddValoriAttributiByTabbedString(line))
                    {
                        entityCollection.TreeEntities.Add(localEntity);
                    }
                }

                string json = null;
                JsonSerializer.JsonSerialize(entityCollection, out json);

                pasteAction.JsonSerializedObject = json;

                IsBusy = false;

            }
            else
            {
                return;
            }


            //ModelActionResponse mar = ModelActionsStack.CommitAction(pasteAction, this);
            ModelActionResponse mar = CommitAction(pasteAction);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                RightPanesView.FilterView.ClearNextAttributiFilter(0);

                if (targetRef == TargetReferenceName.CHILD_OF)
                    FilteredEntitiesViewInfo[firstTarget].IsExpanded = true;


                ApplyFilterAndSort(mar.NewIds.First());
                CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);
                await UpdateCache();
                //UpdateUI();
                //UpdateIsAllChecked();
            }

            IsBusy = false;

        }

        #endregion Copy&Paste

        /// <summary>
        /// Setta gli attributi da programma
        /// </summary>
        public override void SetAttributi()
        {
            Dictionary<string, DefinizioneAttributo> defAtts = DataService.GetDefinizioniAttributo();
            Dictionary<string, EntityType> entAtts = DataService.GetEntityTypes();
            
            //Set EntityType
            TreeEntityType cloneType = EntityType.Clone() as TreeEntityType;
            cloneType.CreaAttributi(defAtts, entAtts);

            DataService.SetEntityType(cloneType, false);
            TreeEntityType newEntityType = DataService.GetEntityTypes()[cloneType.GetKey()] as TreeEntityType;

            //Set EntityParentType
            TreeEntityType cloneParentType = cloneType.AssociedType.Clone() as TreeEntityType;
            cloneParentType.CreaAttributi(defAtts, entAtts);

            DataService.SetEntityType(cloneParentType, false);
            TreeEntityType newEntityParentType = DataService.GetEntityTypes()[cloneParentType.GetKey()] as TreeEntityType;

            newEntityType.AssociedType = newEntityParentType;
            newEntityParentType.AssociedType = newEntityType;

            EntityType = newEntityType;

            ApplyFilterAndSort();
            UpdateCache(true);
        }

        protected override void CodingItems()
        {

            if (CheckedEntitiesId.Count() > 0 )
            {
                var Selezionati = FilteredEntitiesViewInfo.Where(g => CheckedEntitiesId.Contains(g.Key));
                if (Selezionati.Count() > 0)
                {
                    List<int> SelectedLevels = Selezionati.Select(l => l.Value.Depth).Distinct().OrderBy(t => t).ToList();
                    int ProfonditaMassimaAlbero = Selezionati.Max(r => r.Value.Depth);
                    ProfonditaMassimaAlbero = ProfonditaMassimaAlbero + 1;
                    if (WindowService.AttributoCodingWindow(EntityType.GetKey(), CheckedEntitiesId, ProfonditaMassimaAlbero, SelectedLevels))
                    {

                    }
                }
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("Seleziona una voce prima di procedere."));
            }
        }

        public void ExpandItemsInternal(IEnumerable<Guid> ids, bool expand)
        {
            //solo per uso interno, va chiamata prima della ApplyFilterAndSort
            foreach (Guid id in ids)
            {
                if (FilteredEntitiesViewInfo.ContainsKey(id))
                    FilteredEntitiesViewInfo[id].IsExpanded = expand;
            }
        }

        internal override void Clear()
        {
            FilteredEntitiesViewInfo.Clear();
            base.Clear();
        }

        public override List<Guid> FilteredDescendantsId(List<Guid> ids)
        {
            return ids.SelectMany(item => FilteredDescendantsId(item)).ToList();
        }


    }





}


