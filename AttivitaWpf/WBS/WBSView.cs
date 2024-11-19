using _3DModelExchange;
using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AttivitaWpf.View
{
    public class WBSItemView : TreeEntityView
    {

        public WBSItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }

        public bool IsCollega
        {
            get
            {
                WBSItemsViewVirtualized wbsMaster = Master as WBSItemsViewVirtualized;
                if (wbsMaster.IsCollegaEntitiesNotificationEnabled)
                {
                    if (wbsMaster.CollegaEntitiesCount.Contains(Id))
                        return true;
                }
                return false;
            }
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            RaisePropertyChanged((GetPropertyName(() => IsCollega)));
        }
    }

    public interface WBSViewSync
    {
        //On
        void OnWBSCommitAction(ModelAction action, ModelActionResponse actionResponse);
        void OnCheckedItemsChanged(HashSet<Guid> checkedEntitieSId);
        void OnSelectedItemChanged(Guid selectedEntityId);
        void OnDisplayedItemsChanged(Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> filteredEntitiesViewInfo);
        void OnFilteredItemsChanged(Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> filteredEntitiesViewInfo);
        void OnWBSItemsCreated();
        void OnWBSItemsUpdated();
        void OnReplaceAttributoPredecessori(HashSet<Guid> changedEntitiesId);
        void OnWBSClear();
        void OnAddPredecessors(HashSet<Guid> changedEntitiesId);
        void OnWBSAttivitaItemsAdded(HashSet<Guid> changedEntitiesId);
        void OnWBSItemDoubleClick();

        //Get
        bool GetUseDefaultCalendar();
        DateTime GetDataInizioGantt();
        bool GetIsActiveCriticalPath();

    }

    public class WBSView : MasterDetailTreeView, SectionItemTemplateView, GanttViewSync
    {
        public WBSViewSync GanttView { get; set; }
        public bool IsGanttViewSyncCalling { get; set; } = false;
        public bool IsGanttViewToUpdate
        {
            get;
            set;
        } = false;
        public WBSItemsViewVirtualized WBSItemsView { get => ItemsView as WBSItemsViewVirtualized; }
        public I3DModelService Model3dService { get; set; }

        private HashSet<Guid> _criticalPath = new HashSet<Guid>();

        public WBSView()
        {
            _itemsView = new WBSItemsViewVirtualized(this);
        }

        public void OnIsActiveCriticalPathChange(bool IsActive)
        {
            if (GanttView.GetIsActiveCriticalPath())
                CalculateCriticalPath();
        }

        public int Code => (int)AttivitaSectionItemsId.WBS;

        public override void Init(EntityTypeViewSettings viewSettings)
        {
            base.Init(viewSettings);

            UpdateInfoProgetto();
        }

        public override void Clear()
        {
            base.Clear();
            GanttView.OnWBSClear();
        }

        public void OnGestioneDateWndOk(DateTime dataInizioGanttOld, DateTime dataInizioGanttNew, bool dayOffset)
        {
            IsGanttViewSyncCalling = true;

            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            CalendariItem calendarDefault = GetCalendarioDefault();

            bool useDefaultCalendar = GanttView.GetUseDefaultCalendar();

            ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, ActionName = ActionName.MULTI };

            List<Guid> entitiesFoundId = null;
            DataService.GetFilteredTreeEntities(BuiltInCodes.EntityType.WBS, null, null, out entitiesFoundId);
            List<Entity> entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, entitiesFoundId);

            foreach (WBSItem ent in entities)
            {
                if (ent.IsParent)
                    continue;

                DateTimeCalculator timeCalc = null;

                Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
                if (useDefaultCalendar && calendarDefault.EntityId != wbsCalendarioId)
                {
                    timeCalc = new DateTimeCalculator(calendarDefault.GetWeekHours(), calendarDefault.GetCustomDays());

                    //change calendar
                    ModelAction calendarAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntitiesId = new HashSet<Guid>() { ent.EntityId },
                        AttributoCode = wbsCalendarioIdCodice,
                        NewValore = new ValoreGuid() { V = calendarDefault.EntityId },
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    };
                    action.NestedActions.Add(calendarAction);
                }

                DateTime? entDataInizioOld = ent.GetDataInizio();
                if (entDataInizioOld.HasValue/* && dataInizioGanttOld != dataInizioGanttNew*/)
                {
                    if (timeCalc == null)
                        timeCalc = CreateDateTimeCalculator(ent);

                    if (timeCalc == null)
                        continue;

                    DateTime dataInizioGanttItemNew = dataInizioGanttNew;

                    if (!timeCalc.IsWorkingMoment(dataInizioGanttItemNew))
                        dataInizioGanttItemNew = timeCalc.GetNextWorkingMoment(dataInizioGanttItemNew);

                    if (dayOffset)
                    {
                        if (dataInizioGanttOld == dataInizioGanttNew)
                            continue;
                    }
                    else
                    {
                        if (entDataInizioOld >= dataInizioGanttItemNew)
                            continue;
                    }

                    double workingDayOffset = timeCalc.GetWorkingDaysBetween(dataInizioGanttOld, entDataInizioOld.Value);
                    DateTime entDataInizioNew = timeCalc.AddWorkingDays(dataInizioGanttItemNew, workingDayOffset);

                    if (entDataInizioNew < dataInizioGanttItemNew)
                        entDataInizioNew = dataInizioGanttItemNew;

                    entDataInizioNew = timeCalc.AsStartingDateTime(entDataInizioNew);

                    ModelAction dataInizioAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntitiesId = new HashSet<Guid>() { ent.EntityId },
                        AttributoCode = BuiltInCodes.Attributo.DataInizio,
                        NewValore = new ValoreData() { V = entDataInizioNew },
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    };
                    action.NestedActions.Add(dataInizioAction);
                }
            }

            if (action.NestedActions.Any())
            {
                ModelActionResponse mar = DataService.CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                    UpdateInfoProgetto();
                }
            }

            UpdateInfoProgetto();

            IsGanttViewSyncCalling = false;
            IsGanttViewToUpdate = true;

        }



        public HashSet<Guid> OnTasksOffset(List<Guid> WBSItemsId, double workingDayOffset)
        {
            HashSet<Guid> changedIds = new HashSet<Guid>();
            IsGanttViewSyncCalling = true;

            ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, ActionName = ActionName.MULTI_AND_CALCOLA };

            IEnumerable<Entity> entities = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, WBSItemsId);

            foreach (WBSItem ent in entities)
            {
                if (ent.IsParent)
                    continue;

                DateTime dataInizioNew = DateTime.MinValue;
                DateTimeCalculator timeCalc = CreateDateTimeCalculator(ent);

                if (timeCalc != null)
                {

                    DateTime? dataInizioOld = ent.GetDataInizio();
                    if (dataInizioOld.HasValue)
                    {
                        dataInizioNew = timeCalc.AddWorkingDays(dataInizioOld.Value, workingDayOffset);

                        if (!timeCalc.IsWorkingMoment(dataInizioNew))
                        {
                            if (workingDayOffset > 0)
                                dataInizioNew = timeCalc.GetNextWorkingMoment(dataInizioNew);
                            else
                                dataInizioNew = timeCalc.GetPrevWorkingMoment(dataInizioNew);

                            dataInizioNew = timeCalc.GetStartingDateTimeOfDay(dataInizioNew);
                        }
                    }
                    DateTime dataFineNew = timeCalc.AddWorkingMinutes(dataInizioNew, ent.GetOreLavoro().Value * 60.0);


                    HashSet<Guid> wbsItemsConflicts = new HashSet<Guid>(WBSItemsView.ValidateDataInizio(ent, dataInizioNew, dataFineNew, new HashSet<Guid>(WBSItemsId)));
                    if (wbsItemsConflicts.Any())
                    {
                        workingDayOffset = 0;
                        dataInizioNew = dataInizioOld.Value;
                    }


                    ModelAction dataInizioAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntitiesId = new HashSet<Guid>() { ent.EntityId },
                        AttributoCode = BuiltInCodes.Attributo.DataInizio,
                        NewValore = new ValoreData() { V = dataInizioNew },
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    };
                    action.NestedActions.Add(dataInizioAction);

                }

            }


            ModelActionResponse mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                changedIds = mar.ChangedEntitiesId;
                MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                UpdateInfoProgetto();
            }


            IsGanttViewSyncCalling = false;
            IsGanttViewToUpdate = true;
            return changedIds;
        }



        private DateTimeCalculator CreateDateTimeCalculator(WBSItem ent)
        {
            DateTimeCalculator timeCalc = null;
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
            if (wbsCalendarioId == Guid.Empty)
            {
                EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);
                string projectInfoCalendarioIdCodice = string.Join(InfoProgettoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                ValoreGuid valId = entitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.InfoProgetto, projectInfoCalendarioIdCodice, false, false) as ValoreGuid;
                wbsCalendarioId = valId.V;
            }

            if (wbsCalendarioId != Guid.Empty)
            {
                CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
                if (calendario == null)
                    return null;
                else
                    timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
            }
            else
            {
                timeCalc = new DateTimeCalculator(WeekHours.Default, new CustomDays());
            }
            return timeCalc;
        }



        public HashSet<Guid> OnTaskAction(ModelAction action)
        {
            HashSet<Guid> changedIds = new HashSet<Guid>();
            IsGanttViewSyncCalling = true;

            if (action.AttributoCode == BuiltInCodes.Attributo.DataInizio)
            {
                WBSItem wbsItem = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, action.EntitiesId.FirstOrDefault()) as WBSItem;
                if (wbsItem == null)
                    return changedIds;

                action.OldValore = new ValoreData() { V = wbsItem.GetDataInizio() };
            }

            ItemsView.ValidateAction(action);


            ModelActionResponse mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                changedIds = mar.ChangedEntitiesId;
                MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                UpdateInfoProgetto();
            }
            IsGanttViewSyncCalling = false;
            IsGanttViewToUpdate = true;
            return changedIds;
        }

        internal void OnWBSItemsCreated()
        {
            WBSItemsView.OnWBSItemsCreated();
        }

        public Dictionary<Guid, EntitiesTreeMasterDetailView.TreeEntityViewInfo> GetFilteredEntitiesViewInfo()
        {
            return ItemsView.FilteredEntitiesViewInfo;
        }

        public void OnTasksSelected(List<Guid> WBSItemsId)
        {
            IsGanttViewSyncCalling = true;

            if (WBSItemsId != null && WBSItemsId.Any())
            {
                ItemsView.CheckedEntitiesId = new HashSet<Guid>(WBSItemsId);
                ItemsView.UpdateSelectedItems();
                ItemsView.UpdateUI();
            }

            IsGanttViewSyncCalling = false;
        }

        public void OnCurrentTaskChanged(Guid WBSItemId)
        {
            IsGanttViewSyncCalling = true;

            int index = ItemsView.DisplayedIndexOf(WBSItemId);
            ItemsView.SelectIndex(index, true);


            IsGanttViewSyncCalling = false;
        }

        public void AddPredecessors(Guid WBSItemId, WBSPredecessors predecessorsToAdd)
        {
            //return;

            //HashSet<Guid> changedIds = new HashSet<Guid>();
            if (!predecessorsToAdd.Items.Any())
                return;

            WBSItem wbsItem = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, WBSItemId) as WBSItem;
            if (wbsItem == null)
                return;

            WBSPredecessors predecessors = wbsItem.GetPredecessors();
            HashSet<Guid> predsId = new HashSet<Guid>(predecessors.Items.Select(item => item.WBSItemId));


            //if (predecessors.Items.Any())
            //    return;

            //predecessorsToAdd.Items = new List<WBSPredecessor>(predecessorsToAdd.Items.Take(1));

            foreach (WBSPredecessor pred in predecessorsToAdd.Items)
            {
                //controllo che pred.WBSItemId non sia già predecessore di WBSItemId
                if (predsId.Contains(pred.WBSItemId))
                    continue;

                //controllo che WBSItemId non sia già predecessore di pred.WBSItemId
                WBSItem aaa = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, pred.WBSItemId) as WBSItem;
                WBSPredecessors predecessors1 = aaa.GetPredecessors();
                if (predecessors1.Items.Select(item => item.WBSItemId).Contains(WBSItemId))
                    continue;

                predecessors.Items.Add(pred);
            }

            //entities to update (in gantt)
            HashSet<Guid> entitiesId = new HashSet<Guid>(predecessorsToAdd.Items.Select(item => item.WBSItemId));
            entitiesId.Add(WBSItemId);

            string json = string.Empty;
            JsonSerializer.JsonSerialize(predecessors, out json);
            if (!string.IsNullOrEmpty(json))
            {
                ModelAction action = new ModelAction()
                {
                    ActionName = ActionName.MULTI,
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    EntitiesId = entitiesId, //solo per far scattare il ricalcolo
                };

                ModelAction actionPredecessor = new ModelAction()
                {
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    AttributoCode = BuiltInCodes.Attributo.Predecessor,
                    NewValore = new ValoreTesto() { V = json },
                    EntitiesId = new HashSet<Guid>() { WBSItemId },
                };
                action.NestedActions.Add(actionPredecessor);

                string desc = WBSItemsView.GetAttributoPredecessoriTextDescription(predecessors);
                ModelAction actionPredecessorText = new ModelAction()
                {
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    AttributoCode = BuiltInCodes.Attributo.PredecessorText,
                    NewValore = new ValoreTesto() { V = desc },
                    EntitiesId = new HashSet<Guid>() { WBSItemId },
                };
                action.NestedActions.Add(actionPredecessorText);

                ModelActionResponse mar = DataService.CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    //MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                    WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnAddPredecessors, mar.ChangedEntitiesId);
                    UpdateInfoProgetto();

                }

            }

        }

        public HashSet<Guid> OnTaskPredecessorAdd(Guid WBSItemId, WBSPredecessor predecessor)
        {
            HashSet<Guid> changedIds = new HashSet<Guid>();

            IsGanttViewSyncCalling = true;

            WBSItem wbsItem = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, WBSItemId) as WBSItem;
            if (wbsItem == null)
                return changedIds;

            WBSPredecessors predecessors = wbsItem.GetPredecessors();

            //if (predecessors.Items.Any())
            //    return new HashSet<Guid>() { WBSItemId };


            predecessors.Items.Add(predecessor);

            string json = string.Empty;
            JsonSerializer.JsonSerialize(predecessors, out json);
            if (!string.IsNullOrEmpty(json))
            {
                ModelAction action = new ModelAction()
                {
                    ActionName = ActionName.MULTI,
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    EntitiesId = new HashSet<Guid>() { WBSItemId, predecessor.WBSItemId },
                };

                ModelAction actionPredecessor = new ModelAction()
                {
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    AttributoCode = BuiltInCodes.Attributo.Predecessor,
                    NewValore = new ValoreTesto() { V = json },
                    EntitiesId = new HashSet<Guid>() { WBSItemId },
                };
                action.NestedActions.Add(actionPredecessor);

                string desc = WBSItemsView.GetAttributoPredecessoriTextDescription(predecessors);
                ModelAction actionPredecessorText = new ModelAction()
                {
                    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                    EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    AttributoCode = BuiltInCodes.Attributo.PredecessorText,
                    NewValore = new ValoreTesto() { V = desc },
                    EntitiesId = new HashSet<Guid>() { WBSItemId },
                };
                action.NestedActions.Add(actionPredecessorText);

                ModelActionResponse mar = DataService.CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    changedIds = mar.ChangedEntitiesId;
                    MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                    UpdateInfoProgetto();
                }

            }

            IsGanttViewSyncCalling = false;
            IsGanttViewToUpdate = true;
            return changedIds;
        }

        public void OnTasksPredecessorDisconnect(List<Guid> WBSItemsId)
        {
            if (WBSItemsId.Count < 2)
                return;


            HashSet<Guid> WBSItemsIdSet = new HashSet<Guid>(WBSItemsId);
            ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.WBS, ActionName = ActionName.MULTI };

            foreach (Guid WBSItemId in WBSItemsId)
            {
                WBSItem wbsItem = DataService.GetEntityById(BuiltInCodes.EntityType.WBS, WBSItemId) as WBSItem;
                if (wbsItem == null)
                    return;

                WBSPredecessors predecessors = wbsItem.GetPredecessors();
                int predCount = predecessors.Items.Count;
                predecessors.Items.RemoveAll(item => WBSItemsIdSet.Contains(item.WBSItemId));
                if (predecessors.Items.Count < predCount)
                {

                    string json = string.Empty;
                    JsonSerializer.JsonSerialize(predecessors, out json);
                    if (!string.IsNullOrEmpty(json))
                    {
                        ModelAction actionItem = new ModelAction()
                        {
                            ActionName = ActionName.MULTI,
                            EntityTypeKey = BuiltInCodes.EntityType.WBS,
                            EntitiesId = new HashSet<Guid>() { WBSItemId },
                        };

                        ModelAction actionItemPredecessor = new ModelAction()
                        {
                            ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                            EntityTypeKey = BuiltInCodes.EntityType.WBS,
                            AttributoCode = BuiltInCodes.Attributo.Predecessor,
                            NewValore = new ValoreTesto() { V = json },
                            EntitiesId = new HashSet<Guid>() { WBSItemId },
                        };

                        actionItem.NestedActions.Add(actionItemPredecessor);

                        string desc = WBSItemsView.GetAttributoPredecessoriTextDescription(predecessors);
                        ModelAction actionItemPredecessorText = new ModelAction()
                        {
                            ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                            EntityTypeKey = BuiltInCodes.EntityType.WBS,
                            AttributoCode = BuiltInCodes.Attributo.PredecessorText,
                            NewValore = new ValoreTesto() { V = desc },
                            EntitiesId = new HashSet<Guid>() { WBSItemId },
                        };
                        actionItem.NestedActions.Add(actionItemPredecessorText);

                        action.NestedActions.Add(actionItem);
                    }
                }
            }

            ModelActionResponse mar = DataService.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                //MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnReplaceAttributoPredecessori, mar.ChangedEntitiesId);
                UpdateInfoProgetto();
            }

        }


        public void SelectItemsByModel3d(List<Model3dObjectKey> elementsId)
        {
            //filtro sugli elementiItem
            FilterData elementifilterData = new FilterData();

            AttributoFilterData attFilterGlobalId = new AttributoFilterData()
            {
                CodiceAttributo = BuiltInCodes.Attributo.GlobalId,
                CheckedValori = new HashSet<string>(elementsId.Select(item => item.GlobalId)),
                EntityTypeKey = BuiltInCodes.EntityType.Elementi,
                IsFiltroAttivato = true,
            };
            elementifilterData.Items.Add(attFilterGlobalId);

            AttributoFilterData attFilterIfcFileName = new AttributoFilterData()
            {
                CodiceAttributo = BuiltInCodes.Attributo.ProjectGlobalId,
                CheckedValori = new HashSet<string>(elementsId.Select(item => item.ProjectGlobalId)),
                EntityTypeKey = BuiltInCodes.EntityType.Elementi,
                IsFiltroAttivato = true,
            };
            elementifilterData.Items.Add(attFilterIfcFileName);

            List<Guid> elementiItemsFound = new List<Guid>();
            DataService.GetFilteredEntities(BuiltInCodes.EntityType.Elementi, elementifilterData, null, null, out elementiItemsFound);

            //Filtro su computoItems
            List<Guid> computoItemsFound = new List<Guid>();
            if (elementiItemsFound.Any())
            {
                FilterData computofilterData = new FilterData();

                AttributoFilterData computofilterIds = new AttributoFilterData()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Computo,
                    CodiceAttributo = BuiltInCodes.Attributo.ElementiItem_Guid,
                    CheckedValori = new HashSet<string>(elementiItemsFound.Select(item => item.ToString())),
                    IsFiltroAttivato = true,
                };
                computofilterData.Items.Add(computofilterIds);

                DataService.GetFilteredEntities(BuiltInCodes.EntityType.Computo, computofilterData, null, null, out computoItemsFound);

            }

            if (computoItemsFound.Any() || elementiItemsFound.Any())
            {
                // items di wbs trovati
                List<Guid> wbsItemsFoundByComputo = new List<Guid>();

                if (computoItemsFound.Any())
                {
                    FilterData wbsfilterData = new FilterData();

                    AttributoFilterData wbsfilterIds = new AttributoFilterData()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        CodiceAttributo = BuiltInCodes.Attributo.ComputoItemIds,
                        CheckedValori = new HashSet<string>(computoItemsFound.Select(item => item.ToString())),
                        IsFiltroAttivato = true,
                    };
                    wbsfilterData.Items.Add(wbsfilterIds);

                    DataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, wbsfilterData, null, null, out wbsItemsFoundByComputo);

                }

                List<Guid> wbsItemsFoundByElementi = new List<Guid>();

                if (elementiItemsFound.Any())
                {
                    FilterData wbsfilterData = new FilterData();

                    AttributoFilterData wbsfilterIds = new AttributoFilterData()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        CodiceAttributo = BuiltInCodes.Attributo.ElementiItemIds,
                        CheckedValori = new HashSet<string>(elementiItemsFound.Select(item => item.ToString())),
                        IsFiltroAttivato = true,
                    };
                    wbsfilterData.Items.Add(wbsfilterIds);

                    DataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, wbsfilterData, null, null, out wbsItemsFoundByElementi);

                }
                HashSet<Guid> wbsItemsFound = new HashSet<Guid>(wbsItemsFoundByComputo);
                wbsItemsFound.UnionWith(wbsItemsFoundByElementi);

                ItemsView.RightPanesView.FilterView.LoadTemporaryFilterByIds(BuiltInCodes.EntityType.WBS, wbsItemsFound.ToList());

            }
            else
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NessunElementoTrovatoNellaSezioneCorrente"));
            }
        }


        public CalendariItem GetCalendarioDefault()
        {


            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);
            string projectInfoCalendarioIdCodice = string.Join(InfoProgettoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            ValoreGuid valId = entitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.InfoProgetto, projectInfoCalendarioIdCodice, false, false) as ValoreGuid;
            if (valId == null)
                return null;

            if (valId.V == Guid.Empty)
                return null;

            CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { valId.V }).FirstOrDefault() as CalendariItem;
            return calendario;
        }

        public DateTime? GetDataInizioLavori()
        {
            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);
            ValoreData valDataInizio = entitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.InfoProgetto, BuiltInCodes.Attributo.DataInizio, false, false) as ValoreData;
            if (valDataInizio == null || !valDataInizio.V.HasValue)
                return null;

            return valDataInizio.V.Value;
        }

        public DateTime? GetDataFineLavori()
        {
            EntitiesHelper entitiesHelper = new EntitiesHelper(DataService);
            ValoreData valDataFine = entitiesHelper.GetValoreAttributo(BuiltInCodes.EntityType.InfoProgetto, BuiltInCodes.Attributo.DataFine, false, false) as ValoreData;
            if (valDataFine == null || !valDataFine.V.HasValue)
                return null;

            return valDataFine.V.Value;
        }

        public DateTime GetMaxDataFineWBSItems(bool byCurrentFilter = false)
        {
            List<Guid> entsFound = null;

            if (byCurrentFilter)
                entsFound = MainOperation.GetFilteredEntitiesId(BuiltInCodes.EntityType.WBS);
            else
                DataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, null, null, null, out entsFound);

            DataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, null, null, null, out entsFound);
            Task<List<string>> task = Task.Run<List<string>>(async () => await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.WBS, entsFound, BuiltInCodes.Attributo.DataFine, -1, null));

            List<string> dateFine = task.Result;
            if (dateFine != null && dateFine.Any())
            {
                //IEnumerable<DateTime> dates = dateFine.Select(item => Convert.ToDateTime(item));
                IEnumerable<DateTime> dates = dateFine.Select(item =>
                {
                    DateTime result = DateTime.MinValue;
                    DateTime.TryParse(item, out result);
                    return result;
                });
                DateTime max = dates.Max();
                return max;
            }


            return GanttView.GetDataInizioGantt();
        }

        public DateTime GetMinDataInizioWBSItems(bool byCurrentFilter = false)
        {
            List<Guid> entsFound = null;

            if (byCurrentFilter)
                entsFound = MainOperation.GetFilteredEntitiesId(BuiltInCodes.EntityType.WBS);
            else
                DataService.GetFilteredEntities(BuiltInCodes.EntityType.WBS, null, null, null, out entsFound);


            Task<List<string>> task = Task.Run<List<string>>(async () => await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.WBS, entsFound, BuiltInCodes.Attributo.DataInizio, -1, null));

            List<string> dateInizio = task.Result;
            if (dateInizio != null && dateInizio.Any())
            {
                IEnumerable<DateTime> dates = dateInizio.Select(item =>
                {
                    DateTime result = DateTime.MinValue;
                    DateTime.TryParse(item, out result);
                    return result;
                });
                DateTime min = dates.Min();
                return min;
            }
            return GanttView.GetDataInizioGantt();
        }

        public void UpdateInfoProgetto()
        {
            if (DataService != null && !DataService.IsReadOnly)
            {
                if (GanttView != null && GanttView.GetIsActiveCriticalPath())
                    CalculateCriticalPath();

                DateTime maxDataFineWBSItems = GetMaxDataFineWBSItems();

                //Aggiorno il valore di default per la data inizio lavori nella sezione InfoProgetto
                EntityType infoEntType = DataService.GetEntityType(BuiltInCodes.EntityType.InfoProgetto).Clone();
                infoEntType.Attributi[BuiltInCodes.Attributo.DataInizio].ValoreDefault = new ValoreData() { V = GanttView.GetDataInizioGantt() };
                infoEntType.Attributi[BuiltInCodes.Attributo.DataFine].ValoreDefault = new ValoreData() { V = maxDataFineWBSItems };
                DataService.SetEntityType(infoEntType, false, false);

                MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.InfoProgetto });
            }
        }

        public ICommand SelectInModel3dCommand
        {
            get
            {
                return new CommandHandler(() => this.SelectInModel3d());
            }
        }

        void SelectInModel3d()
        {
            List<Model3dObjectKey> model3DObjectsKey = GetModel3dObjectsKeyByWBSItemsId(ItemsView.CheckedEntitiesId);

            if (Model3dService != null)
                Model3dService.SelectElements(model3DObjectsKey);
            else
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Modello3dNonCaricato"));


        }

        public List<Model3dObjectKey> GetModel3dObjectsKeyByWBSItemsId(HashSet<Guid> WBSItemsId)
        {
            //entities selected (checked)
            List<Entity> wbsItems = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, WBSItemsId);

            //Ricavo gli ElementId da wbsItems
            HashSet<Guid> elementsId = new HashSet<Guid>();
            wbsItems.ForEach(item =>
            {
                //Aggiungo gli id derivanti da ElementiItemIds
                WBSItem wbsItem = item as WBSItem;
                ValoreGuidCollection valElementiGuids = wbsItem.GetValoreAttributo(BuiltInCodes.Attributo.ElementiItemIds, false, false) as ValoreGuidCollection;
                if (valElementiGuids != null)
                    elementsId.UnionWith(valElementiGuids.Items.Select(item1 => (item1 as ValoreGuidCollectionItem).EntityId));

                //Aggiungo gli id derivanti da ComputoItemIds

                ValoreGuidCollection valComputoGuids = wbsItem.GetValoreAttributo(BuiltInCodes.Attributo.ComputoItemIds, false, false) as ValoreGuidCollection;
                if (valComputoGuids != null)
                {
                    List<Entity> computoItems = DataService.GetEntitiesById(BuiltInCodes.EntityType.Computo, valComputoGuids.Items.Select(item1 => (item1 as ValoreGuidCollectionItem).EntityId));

                    //Ricavo gli ElementId da computoItems
                    computoItems.ForEach(item1 =>
                    {
                        ComputoItem computoItem = item1 as ComputoItem;
                        //Guid elmGuid = computoItem.GetElementiItemId();
                        var elmGuid = computoItem.GetElementiItemsId();
                        elementsId.UnionWith(elmGuid);
                    });
                }

            });

            List<Entity> elementiItems = DataService.GetEntitiesById(BuiltInCodes.EntityType.Elementi, elementsId);

            EntitiesHelper entsHelper = new EntitiesHelper(DataService);

            List<Model3dObjectKey> model3DObjectsKey = new List<Model3dObjectKey>();
            elementiItems.ForEach(item =>
            {
                entsHelper.GetModel3dObjectsByElement(item, model3DObjectsKey);

                //Model3dObjectKey m3dObjKey = new Model3dObjectKey();
                //Valore valGlobalId = item.GetValoreAttributo(BuiltInCodes.Attributo.GlobalId, false, false);
                //if (valGlobalId != null)
                //    m3dObjKey.GlobalId = valGlobalId.PlainText;

                //Valore valFileIfc = item.GetValoreAttributo(BuiltInCodes.Attributo.ProjectGlobalId, false, false);
                //if (valFileIfc != null)
                //    m3dObjKey.ProjectGlobalId = valFileIfc.PlainText;

                //model3DObjectsKey.Add(m3dObjKey);

            });
            return model3DObjectsKey;
        }

        public void CalculateCriticalPath()
        {

            WBSCriticalPathCalculator criticalPathCalculator = new WBSCriticalPathCalculator();
            criticalPathCalculator.Init(DataService);
            _criticalPath = criticalPathCalculator.Run();
        }

        /// <summary>
        /// Ritorna gli delle wbsItems critiche
        /// </summary>
        /// <returns></returns>
        public HashSet<Guid> GetCriticalPath()
        {
            return _criticalPath;
        }

        public Guid GetSelectedItem()
        {
            return ItemsView.SelectedEntityId;
        }

        public HashSet<Guid> GetCheckedItems()
        {
            return new HashSet<Guid>(ItemsView.CheckedEntitiesId);
        }

        public List<Guid> GetFilteredDescendantsOf(Guid parentId)
        {
            return ItemsView.FilteredDescendantsId(parentId);
        }

        public bool IsMultipleModify()
        {
            return ItemsView.IsMultipleModify;
        }

        public I3DModelService GetModel3dService() { return Model3dService; }
    }

    /// <summary>
    /// Classe che rappresenta una lista virtualizzata di WBS 
    /// Tipico ordine delle chiamate per l'update della lista dopo una modifica:
    /// 
    /// SelectEntityById
    /// UpdateCache
    /// RefreshView
    /// LoadRange
    /// LoadingStateChanged
    /// 
    /// </summary>
    public class WBSItemsViewVirtualized : MasterDetailTreeViewVirtualized
    {
        WBSView Owner { get => View as WBSView; }
        WBSViewSync GanttView { get => Owner.GanttView; }

        string CodiceAttivitaId { get; set; }
        string CodiceAttivitaNome { get; set; }
        bool _isCacheUpdating = false;
        int _WBSViewSyncBits = (int)WBSViewSyncEnum.Nothing;

        public WBSItemsViewVirtualized(MasterDetailTreeView owner) : base(owner)
        {
            CodiceAttivitaId = string.Format("{0}{1}{2}", BuiltInCodes.EntityType.ElencoAttivita, WBSItemType.AttributoCodiceSeparator, BuiltInCodes.Attributo.Id);
            CodiceAttivitaNome = string.Format("{0}{1}{2}", BuiltInCodes.EntityType.ElencoAttivita, WBSItemType.AttributoCodiceSeparator, BuiltInCodes.Attributo.Nome);

        }

        public override void Init()
        {
            base.Init();
            EntityParentType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.WBSParent];
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.WBS];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override TreeEntityView NewItemView(TreeEntity entity)
        {
            return new WBSItemView(this, entity);
        }

        public override void ReplaceValore(ValoreView valoreView)
        {

            string codiceAttributo = valoreView.Tag as string;
            if (codiceAttributo == BuiltInCodes.Attributo.AttributoFilterText && !IsMultipleModify)
            {
                ReplaceAttributoFilterText();
            }
            else if (codiceAttributo == CodiceAttivitaNome)
            {
                ReplaceAttributoAttivita();
            }
            else if (codiceAttributo == BuiltInCodes.Attributo.PredecessorText && !IsMultipleModify)
            {
                ReplaceAttributoPredecessori();
            }
            else
            {
                base.ReplaceValore(valoreView);
            }

        }

        #region AttributoFilterText

        private void ReplaceAttributoFilterText()
        {
            string entityTypeKey = string.Empty;
            List<Guid> entsIdToFilter = CalcolaAttributoFilterResult(SelectedEntityView.Id, true, ref entityTypeKey);

            //get AttributoFilterData
            AttributoFilterData attFilterData = null;
            string json = EntitiesHelper.GetValorePlainText(SelectedEntityView.Entity, BuiltInCodes.Attributo.AttributoFilter, false, false);
            if (!string.IsNullOrEmpty(json))
                if (!JsonSerializer.JsonDeserialize(json, out attFilterData, typeof(AttributoFilterData)))
                    return;

            HashSet<string> entityTypesKey = null;
            if (entityTypeKey != null && entityTypeKey.Any())
                entityTypesKey = new HashSet<string>() { entityTypeKey };
            else
                entityTypesKey = new HashSet<string>() { BuiltInCodes.EntityType.Computo, BuiltInCodes.EntityType.Elementi };


            if (WindowService.SelectAttributoFilterWindow(entityTypesKey, entsIdToFilter, ref attFilterData))
            {
                if (JsonSerializer.JsonSerialize(attFilterData, out json))
                {

                    //prima action
                    ModelAction attributoFilterAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.AttributoFilter,
                    };
                    attributoFilterAction.NewValore = new ValoreTesto() { V = json };

                    //seconda action
                    ModelAction attributoFilterTextAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.AttributoFilterText,
                    };
                    string desc = GetAttributoFilterTextDescription(attFilterData);
                    //string str = string.Join("\n", attFilterData.CheckedValori);
                    attributoFilterTextAction.NewValore = new ValoreTesto() { V = desc, Result = desc };


                    //Main action
                    ModelAction action = new ModelAction()
                    {
                        ActionName = ActionName.MULTI,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    };

                    action.NestedActions.Add(attributoFilterAction);
                    action.NestedActions.Add(attributoFilterTextAction);


                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                    }

                }
            }

            return;
        }

        string GetAttributoFilterTextDescription(AttributoFilterData attFilterData)
        {

            string str = string.Empty;

            if (attFilterData == null)
                return string.Empty;

            EntityType entType = DataService.GetEntityTypes().Values.FirstOrDefault(item => item.Codice == attFilterData.EntityTypeKey);
            if (entType == null)
                return string.Empty;

            Attributo att = null;
            if (!entType.Attributi.TryGetValue(attFilterData.CodiceAttributo, out att))
                return string.Empty;

            

            if (attFilterData.FilterType == FilterTypeEnum.Conditions)
            {
                str = LocalizationProvider.GetString("_Condizione");
            }
            else
            {
                Attributo sourceAtt = EntitiesHelper.GetSourceAttributo(att);
                if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    IEnumerable<Guid> ids = attFilterData.CheckedValori.Select(item => new Guid(item));
                    List<Entity> ents = DataService.GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, ids);

                    str = string.Join("\n", ents.Select(item => item.ToUserIdentity(UserIdentityMode.SingleLine)));
                }
                else
                {
                    str = string.Join("\n", attFilterData.CheckedValori);

                }
            }

            return str;
        }


        //public static string LocalizeAcronimoFineFine { get { return LocalizationProvider.GetString("FF"); } }
        //public static string LocalizeAcronimoInizioInizio { get { return LocalizationProvider.GetString("II"); } }
        //public static string LocalizeAcronimoFineInizio { get { return LocalizationProvider.GetString("FI"); } }
        //public static string LocalizeAcronimoInizioFine { get { return LocalizationProvider.GetString("IF"); } }
        public string GetAttributoPredecessoriTextDescription(WBSPredecessors WBSPredecessors)
        {
            return WBSEntityAttributiUpdater.GetAttributoPredecessoriTextDescription(WBSPredecessors, DataService);

            //EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //string str = string.Empty;

            //foreach (WBSPredecessor Item in WBSPredecessors?.Items)
            //{
            //    List<Guid> Guids = new List<Guid>();
            //    Guids.Add(Item.WBSItemId);
            //    Entity Entity = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, Guids).FirstOrDefault();
            //    string Codice = entsHelper.GetValoreAttributo(Entity, BuiltInCodes.Attributo.Codice, false, true).PlainText;
            //    string Relazione = null;
            //    if (Item.Type == WBSPredecessorType.FinishToFinish)
            //        Relazione = LocalizeAcronimoFineFine;
            //    if (Item.Type == WBSPredecessorType.StartToStart)
            //        Relazione = LocalizeAcronimoInizioInizio;
            //    if (Item.Type == WBSPredecessorType.FinishToStart)
            //        Relazione = LocalizeAcronimoFineInizio;
            //    if (Item.Type == WBSPredecessorType.StartToFinish)
            //        Relazione = LocalizeAcronimoInizioFine;
            //    str = str + Codice + " " + Relazione + " " + "(" + Item.DelayDays + "); ";
            //}

            //return str;
        }
        //string GetAttributoFilterTextDescription(AttributoFilterData attFilterData)
        //{
        //    string str = string.Empty;

        //    if (attFilterData == null)
        //        return string.Empty;

        //    EntityType entType = DataService.GetEntityTypes().Values.FirstOrDefault(item => item.Codice == attFilterData.EntityTypeKey);
        //    if (entType == null)
        //        return string.Empty;

        //    Attributo att = null;
        //    if (!entType.Attributi.TryGetValue(attFilterData.CodiceAttributo, out att))
        //        return string.Empty;

        //    if (attFilterData.CheckedValori.Count == 0)
        //        return string.Empty;
        //    else if (attFilterData.CheckedValori.Count == 1)
        //        str = string.Format("{0} > {1} > {2}", entType.Name, att.Etichetta, attFilterData.CheckedValori.First());
        //    else if (attFilterData.CheckedValori.Count > 1)
        //    {
        //        str = string.Format("{0} > {1} > {2}", entType.Name, att.Etichetta, LocalizationProvider.GetString("_Multi"));
        //        str += "\n" + string.Join("\n", attFilterData.CheckedValori);
        //    }



        //    return str;
        //}

        public List<Guid> CalcolaAttributoFilterResult(Guid id, bool excludeLeaf, ref string entityTypeKey)
        {
            if (entityTypeKey == null)
                return null;


            List<Guid> entitiesFound = new List<Guid>();

            FilterData filterData = null;
            IEnumerable<TreeEntity> ents = DataService.GetTreeEntitiesDeepById(BuiltInCodes.EntityType.WBS, new List<Guid>() { id });
            TreeEntity ent = ents.LastOrDefault();
            if (ent == null)
                return null;

            TreeEntity parent = ent;
            if (excludeLeaf)
                parent = ent.Parent;

            if (parent == null)
                return null;

            while (parent != null)
            {
                if (filterData == null)
                    filterData = new FilterData();

                AttributoFilterData attFilterData = null;
                string json = EntitiesHelper.GetValorePlainText(parent, BuiltInCodes.Attributo.AttributoFilter, false, false);
                if (!JsonSerializer.JsonDeserialize(json, out attFilterData, typeof(AttributoFilterData)))
                    break;

                if (attFilterData != null)
                {
                    //se nello stesso ramo ho due voci con entityTypeKey diverse il risultato del filtro sarà vuoto
                    if (attFilterData.EntityTypeKey.Any() && (!entityTypeKey.Any() || attFilterData.EntityTypeKey == entityTypeKey))
                        filterData.Items.Add(attFilterData);
                    else
                        return entitiesFound;

                    entityTypeKey = attFilterData.EntityTypeKey;
                }

                parent = parent.Parent;
            }

            if (string.IsNullOrEmpty(entityTypeKey))
                return entitiesFound;

            DataService.GetFilteredEntities(entityTypeKey, filterData, null, null, out entitiesFound);

            return entitiesFound;

        }

        #endregion AttributoFilterText

        #region Attività

        private async void ReplaceAttributoAttivita()
        {

            string entityTypeKey = BuiltInCodes.EntityType.Computo;
            List<Guid> entsIdToFilter = new List<Guid>();
            if (SelectedEntityView != null)
                entsIdToFilter = CalcolaAttributoFilterResult(SelectedEntityView.Id, false, ref entityTypeKey);

            AttributoRiferimento attRif = EntityType.Attributi[CodiceAttivitaNome] as AttributoRiferimento;
            if (attRif == null)
                return;

            EntityTypeViewSettings viewSettings = null;
            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                viewSettings = new EntityTypeViewSettings();
                List<string> attivitaItemsId = await AttivitaItemsIdByComputoItemsId(entsIdToFilter);

                viewSettings.Filters.Add(new AttributoFilterData()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita,
                    CodiceAttributo = BuiltInCodes.Attributo.TemporaryFilterByIds,
                    CheckedValori = new HashSet<string>(attivitaItemsId),
                });
            }

            Owner.ReplaceCurrentItemAttributoGuid(attRif, viewSettings);
        }
        public ICommand UpdateWBSItemsCommand { get { return new CommandHandler(() => this.UpdateWBSItems()); } }
        void UpdateWBSItems()
        {
            //ricalcolo
            var calcOptions = new EntityCalcOptions() { ResetCalulatedValues = true, CalcolaAttributiResults = true };
            DataService.CalcolaEntities(BuiltInCodes.EntityType.WBS, calcOptions);
            //_WBSViewSyncBits |= (int)WBSViewSyncEnum.OnUpdateWBSItems;
            //MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
            UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);

        }

        public bool IsUpdateWBSItemsEnabled
        {
            get
            {

                if (IsMoveEntitiesAfterEnabled)
                    return false;

                //if (SelectedEntityView != null)
                //{
                //    if (IsMoveEntitiesAfterEnabled || !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                //        return false;
                //}

                return true;
            }
        }

        private void ReplaceAttributoPredecessori()
        {
            WBSPredecessors WBSPredecessors = null;
            string json = null;

            if (WindowService.SelectAttributoPredecessoriWindow(SelectedEntityView.Id, ref WBSPredecessors))
            {
                if (JsonSerializer.JsonSerialize(WBSPredecessors, out json))
                {
                    //prima action
                    ModelAction attributoPredecessorAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.Predecessor,
                    };
                    attributoPredecessorAction.NewValore = new ValoreTesto() { V = json };

                    //seconda action
                    ModelAction attributoPredecessorTextAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.PredecessorText,
                    };
                    string desc = GetAttributoPredecessoriTextDescription(WBSPredecessors);
                    attributoPredecessorTextAction.NewValore = new ValoreTesto() { V = desc, Result = desc };


                    //Main action
                    ModelAction action = new ModelAction()
                    {
                        ActionName = ActionName.MULTI,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                    };

                    action.NestedActions.Add(attributoPredecessorAction);
                    action.NestedActions.Add(attributoPredecessorTextAction);


                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //_changedEntitiesId = mar.ChangedEntitiesId;
                        //_WBSViewSyncBits |= (int)WBSViewSyncEnum.OnReplaceAttributoPredecessori;
                        //MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                        UpdateWBSView(WBSViewSyncEnum.OnReplaceAttributoPredecessori, mar.ChangedEntitiesId);
                    }
                }
            }

            return;
        }


        /// <summary>
        /// Calcola l'unione di tutte le attività associate agli articoli associati alle voci di computo
        /// </summary>
        /// <param name="computoItemsId"></param>
        /// <returns></returns>
        private async Task<List<string>> AttivitaItemsIdByComputoItemsId(List<Guid> computoItemsId)
        {
            List<string> prezzarioItemsIdStr = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.Computo, computoItemsId, BuiltInCodes.Attributo.PrezzarioItem_Guid, -1, null);
            List<Guid> prezzarioItemsId = prezzarioItemsIdStr.Select(item => new Guid(item)).ToList();

            List<string> attivitaItemsId = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.Prezzario, prezzarioItemsId, BuiltInCodes.Attributo.Attivita, -1, null);
            return attivitaItemsId;
        }

        public ICommand AddAttivitaItemsCommand { get { return new CommandHandler(() => this.AddAttivitaItems()); } }
        public void AddAttivitaItems()
        {
            if (IsMultipleModify)
                return;

            AddSelectedEntityAttivitaItems();

        }
        public async void AddSelectedEntityAttivitaItems()
        {
            if (!IsAddAttivitaEnabled)
                return;

            DataService.Suspended = true;

            List<Guid> selectedAttivitaIds = new List<Guid>();

            string entityTypeKey = string.Empty;
            List<Guid> entsIdToFilter = CalcolaAttributoFilterResult(SelectedTreeEntityView.Id, false, ref entityTypeKey);

            EntityTypeViewSettings viewSettings = null;
            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                viewSettings = new EntityTypeViewSettings();
                List<string> attivitaItemsId = await AttivitaItemsIdByComputoItemsId(entsIdToFilter);

                selectedAttivitaIds = attivitaItemsId.Select(item => new Guid(item)).ToList();

                viewSettings.Filters.Add(new AttributoFilterData()
                {
                    EntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita,
                    CodiceAttributo = BuiltInCodes.Attributo.TemporaryFilterByIds,
                    CheckedValori = new HashSet<string>(attivitaItemsId),
                });
            }


            string title = LocalizationProvider.GetString("AggiungiVociAttivita");

            if (WindowService.SelectEntityIdsWindow(BuiltInCodes.EntityType.ElencoAttivita, ref selectedAttivitaIds, title, SelectIdsWindowOptions.Nothing, viewSettings, null))
            {
                ModelAction action = new ModelAction() { ActionName = ActionName.MULTI, EntityTypeKey = BuiltInCodes.EntityType.WBS };

                //strutture per non inserire attività con lo stesso codice
                List<Guid> wbsChildren = GetChildrenOf(SelectedTreeEntityView.Id);
                List<string> existentAttivitaIdStr = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.WBS, wbsChildren, CodiceAttivitaId, -1, string.Empty);
                List<Guid> existentAttivitaId = new List<Guid>(existentAttivitaIdStr.Select(item => new Guid(item)));
                HashSet<string> existentAttivitaCodice = new HashSet<string>(await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.ElencoAttivita, existentAttivitaId, BuiltInCodes.Attributo.Codice, -1, string.Empty));

                List<Entity> attivitaEnts = DataService.GetEntitiesById(BuiltInCodes.EntityType.ElencoAttivita, selectedAttivitaIds);

                foreach (Entity attivita in attivitaEnts)
                {
                    string codiceAttivita = EntitiesHelper.GetValorePlainText(attivita, BuiltInCodes.Attributo.Codice, false, false);

                    if (existentAttivitaCodice.Contains(codiceAttivita))
                        continue;

                    ModelAction actionAdd = new ModelAction() { ActionName = ActionName.TREEENTITY_ADD_CHILD, EntityTypeKey = BuiltInCodes.EntityType.WBS };
                    TargetReference targetRef = new TargetReference() { Id = SelectedTreeEntityView.Id, TargetReferenceName = TargetReferenceName.CHILD_OF };
                    actionAdd.NewTargetEntitiesId.Add(targetRef);

                    //set attività
                    ModelAction actionSetAttivita = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        AttributoCode = CodiceAttivitaId,
                        NewValore = new ValoreGuid() { V = attivita.EntityId },
                    };
                    actionAdd.NestedActions.Add(actionSetAttivita);

                    //set descrizione
                    Attributo attAttivitaNome = EntityType.Attributi[CodiceAttivitaNome];
                    ModelAction actionSetDesc = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.WBS,
                        AttributoCode = BuiltInCodes.Attributo.Nome,
                        NewValore = new ValoreTesto() { V = string.Format("{0}{1}{2}", "_att{", attAttivitaNome.Etichetta, "}") },
                    };
                    actionAdd.NestedActions.Add(actionSetDesc);

                    action.NestedActions.Add(actionAdd);
                }

                ModelActionResponse mar = DataService.CommitAction(action);
                if (mar.ActionResponse == ActionResponse.OK)
                {
                    CheckedEntitiesId.Clear();
                    CheckedEntitiesId.Add(mar.NewId);
                    FilteredEntitiesViewInfo[SelectedEntityId].IsExpanded = true;

                    //MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
                    UpdateWBSView(WBSViewSyncEnum.OnAddAttivitaItems, mar.ChangedEntitiesId);
                }

            }

        }

        public bool IsAddAttivitaEnabled
        {
            get
            {
                if (SelectedTreeEntityView == null)
                    return false;

                if (IsMoveEntitiesAfterEnabled || !ReadyToModifyEntitiesId.Contains(SelectedEntityView.Id))
                    return false;

                //Non deve avere attività collegate
                ValoreGuid val = SelectedEntityView.Entity.GetValoreAttributo(CodiceAttivitaId, false, false) as ValoreGuid;
                if (val != null)
                {
                    if (val.V != Guid.Empty)
                        return false;
                }

                return true;
            }
        }

        public bool IsCreateWBSItemsEnabled
        {
            get
            {
                if (SelectedEntityView != null)
                {
                    if (IsMoveEntitiesAfterEnabled || !CheckedEntitiesId.Contains(SelectedEntityView.Id))
                        return false;
                }

                return true;
            }
        }


        #endregion Attività

        public override void UpdateUI()
        {
            base.UpdateUI();
            RaisePropertyChanged(GetPropertyName(() => IsAddAttivitaEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCreateWBSItemsEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsUpdateWBSItemsEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCollegaEntitiesEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCollegaModeEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsOtherNuttonEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsScollegaEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsSelectInModel3dEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCollegaEntitiesNotificationEnabled));
            RaisePropertyChanged(GetPropertyName(() => ReadyToCollegaEntitiesCount));
            RaisePropertyChanged(GetPropertyName(() => IsCollegaEntitiesWaitingForTarget));
        }


        public override ModelActionResponse CommitAction(ModelAction action)
        {
            ModelActionResponse mar = base.CommitAction(action);
            if (mar.ActionResponse == ActionResponse.OK)
            {
                Owner.UpdateInfoProgetto();
                _WBSViewSyncBits |= (int)WBSViewSyncEnum.OnWBSCommitAction;
                GanttView.OnWBSCommitAction(action, mar);
            }


            return mar;
        }

        /// <summary>
        /// return conflicts
        /// </summary>
        /// <param name="entChanged"></param>
        /// <param name="newDataInizio"></param>
        /// <returns></returns>
        public List<Guid> ValidateDataInizio(WBSItem entChanged, DateTime newDataInizio, DateTime newDataFine, HashSet<Guid> excludedPredecessors = null)
        {
            List<Guid> wbsItemsConflict = new List<Guid>();
            DateTime dataInizioRes = newDataInizio;

            WBSPredecessors predecessors = entChanged.GetPredecessors();//predecessori dell'entità modificata

            IEnumerable<Guid> itemsId = predecessors.Items.Select(item => item.WBSItemId);
            if (excludedPredecessors != null)
                itemsId = itemsId.Except(excludedPredecessors);

            List<Entity> items = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, itemsId);

            for (int i = 0; i < items.Count; i++)//scorro i predecessori dell'entità modificata
            {
                WBSPredecessor pred = predecessors.Items[i];
                WBSItem item = items[i] as WBSItem; //predecessore

                if (pred.Type == WBSPredecessorType.FinishToStart)
                {
                    //Finish to Start(FS)—The predecessor ends before the successor can begin.

                    DateTime? data = item.GetDataFine();//fine predecessore
                    if (data > newDataInizio)
                        wbsItemsConflict.Add(item.EntityId);
                }
                else if (pred.Type == WBSPredecessorType.StartToStart)
                {
                    //Start to Start(SS)—The predecessor begins before the successor can begin.
                    DateTime? data = item.GetDataInizio();//inizio predecessore
                    if (data > newDataInizio)
                        wbsItemsConflict.Add(item.EntityId);
                }
                else if (pred.Type == WBSPredecessorType.FinishToFinish)
                {
                    //Finish to Finish(FF)—The predecessor ends before the successor can end.
                    DateTime? data = item.GetDataFine();

                    if (data > newDataFine)
                        wbsItemsConflict.Add(item.EntityId);
                }
                else if (pred.Type == WBSPredecessorType.StartToFinish)
                {
                    //Start to Finish(SF)—The predecessor begins before the successor can end.

                    DateTime? data = item.GetDataInizio();
                    if (data > newDataFine)
                        wbsItemsConflict.Add(item.EntityId);
                }
            }



            return wbsItemsConflict;
        }

        public override bool ValidateAction(ModelAction action)
        {
            ModelAction modifyAction = null;


            if (action.ActionName == ActionName.MULTI_AND_CALCOLA)
            {
                modifyAction = action.NestedActions.FirstOrDefault();
            }
            else
            {
                modifyAction = action;
            }

            if (modifyAction == null)
                return false;

            if (modifyAction.ActionName == ActionName.ATTRIBUTO_VALORE_MODIFY)
            {
                Dictionary<Guid, DateTimeCalculator> dateTimeCalculatorsById = new Dictionary<Guid, DateTimeCalculator>();

                bool lavoroRedirect = false;
                List<Entity> ents = DataService.GetEntitiesById(BuiltInCodes.EntityType.WBS, modifyAction.EntitiesId);
                foreach (WBSItem wbsItem in ents)
                {

                    //validazione della data e ora di inizio e fine
                    if (modifyAction.AttributoCode == BuiltInCodes.Attributo.DataInizio)
                    {
                        //if (SelectedEntityView == null)
                        //    return false;

                        //WBSItem wbsItem = SelectedEntityView.Entity as WBSItem;



                        string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                        Guid wbsCalendarioId = wbsItem.GetAttributoGuidId(wbsCalendarioIdCodice);

                        DateTimeCalculator timeCalc = null;
                        if (!dateTimeCalculatorsById.TryGetValue(wbsCalendarioId, out timeCalc))
                        {
                            timeCalc = CreateDateTimeCalculator(wbsCalendarioId);
                            dateTimeCalculatorsById.Add(wbsCalendarioId, timeCalc);
                        }

                        //DateTime? olddata = (modifyAction.OldValore as ValoreData).V;
                        DateTime? olddata = wbsItem.GetDataInizio();
                        DateTime? newData = (modifyAction.NewValore as ValoreData).V;
                        if (newData == null)
                            return false;


                        if (!timeCalc.IsWorkingMoment(newData.Value) && newData.HasValue && olddata != null && olddata.HasValue)
                        {
                            if (olddata < newData)
                                newData = timeCalc.GetNextWorkingMoment(newData.Value);
                            else if (olddata > newData)
                                newData = timeCalc.GetPrevWorkingMoment(newData.Value);


                            if (olddata.Value.Date != newData.Value.Date)
                            {
                                newData = timeCalc.GetStartingDateTimeOfDay(newData.Value);
                                //if (action.AttributoCode == BuiltInCodes.Attributo.DataInizio)
                                //    newData = timeCalc.AsStartingDateTime(newData.Value);
                                //else if (action.AttributoCode == BuiltInCodes.Attributo.DataFine)
                                //    newData = timeCalc.AsEndingDateTime(newData.Value);
                            }
                        }

                        DateTime newDataFine = timeCalc.AddWorkingMinutes(newData.Value, wbsItem.GetOreLavoro().Value * 60.0);
                        if (ValidateDataInizio(wbsItem, newData.Value, newDataFine).Any())
                            newData = olddata;

                        modifyAction.NewValore = new ValoreData() { V = newData };

                    }
                    if (modifyAction.AttributoCode == BuiltInCodes.Attributo.DataFine)
                    {
                        //Al posto della data di fine setto il lavoro
                        lavoroRedirect = true;

                        string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                        Guid wbsCalendarioId = wbsItem.GetAttributoGuidId(wbsCalendarioIdCodice);

                        DateTimeCalculator timeCalc = null;
                        if (!dateTimeCalculatorsById.TryGetValue(wbsCalendarioId, out timeCalc))
                        {
                            timeCalc = CreateDateTimeCalculator(wbsCalendarioId);
                            dateTimeCalculatorsById.Add(wbsCalendarioId, timeCalc);
                        }

                        //DateTime? olddata = (modifyAction.OldValore as ValoreData).V;
                        DateTime? olddata = wbsItem.GetDataFine();
                        DateTime? newData = (modifyAction.NewValore as ValoreData).V;
                        if (newData == null)
                            return false;


                        if (!timeCalc.IsWorkingMoment(newData.Value) && newData.HasValue && olddata != null && olddata.HasValue)
                        {
                            if (olddata < newData)
                                newData = timeCalc.GetNextWorkingMoment(newData.Value);
                            else if (olddata > newData)
                                newData = timeCalc.GetPrevWorkingMoment(newData.Value);


                            if (olddata.Value.Date != newData.Value.Date)
                            {
                                if (modifyAction.AttributoCode == BuiltInCodes.Attributo.DataInizio)
                                    newData = timeCalc.GetStartingDateTimeOfDay(newData.Value);
                                else if (modifyAction.AttributoCode == BuiltInCodes.Attributo.DataFine)
                                    newData = timeCalc.GetEndingDateTimeOfDay(newData.Value);

                            }
                        }

                        double workingMinutes = timeCalc.GetWorkingMinutesBetween(wbsItem.GetDataInizio().GetValueOrDefault(), newData.GetValueOrDefault());

                        double oreLavoro = workingMinutes / 60.0;

                        ModelAction lavoroAction = new ModelAction();
                        lavoroAction.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                        lavoroAction.AttributoCode = BuiltInCodes.Attributo.Lavoro;
                        lavoroAction.ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY;
                        lavoroAction.EntitiesId.Add(wbsItem.EntityId);
                        lavoroAction.NewValore = new ValoreReale() { V = oreLavoro.ToString() };

                        modifyAction.NestedActions.Add(lavoroAction);
                    }
                    else if (modifyAction.AttributoCode == BuiltInCodes.Attributo.Lavoro)
                    {
                        //if (SelectedEntityView != null)
                        //{
                        Attributo att = GetAttributoByCode(modifyAction.AttributoCode);
                        Calculator.Calculate(wbsItem, att, modifyAction.NewValore);
                        double? newVal = (modifyAction.NewValore as ValoreReale).RealResult;
                        if (!newVal.HasValue || newVal.Value < 0)
                            modifyAction.NewValore = att.ValoreDefault;
                        //}
                    }
                    else if (modifyAction.AttributoCode == BuiltInCodes.Attributo.Durata)
                    {
                        //Al posto della durata setto il lavoro

                        lavoroRedirect = true;
                        string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
                        Guid wbsCalendarioId = wbsItem.GetAttributoGuidId(wbsCalendarioIdCodice);

                        DateTimeCalculator timeCalc = null;
                        if (!dateTimeCalculatorsById.TryGetValue(wbsCalendarioId, out timeCalc))
                        {
                            timeCalc = CreateDateTimeCalculator(wbsCalendarioId);
                            dateTimeCalculatorsById.Add(wbsCalendarioId, timeCalc);
                        }
                        Attributo att = GetAttributoByCode(modifyAction.AttributoCode);
                        Calculator.Calculate(wbsItem, att, modifyAction.NewValore);
                        double? newVal = (modifyAction.NewValore as ValoreReale).RealResult;
                        if (!newVal.HasValue)
                            return false;

                        DateTime dataFine = timeCalc.AddWorkingDays(wbsItem.GetDataInizio().GetValueOrDefault(), newVal.Value);
                        double oreLavoro = timeCalc.GetWorkingMinutesBetween(wbsItem.GetDataInizio().GetValueOrDefault(), dataFine) / 60.0;

                        ModelAction lavoroAction = new ModelAction();
                        lavoroAction.EntityTypeKey = BuiltInCodes.EntityType.WBS;
                        lavoroAction.AttributoCode = BuiltInCodes.Attributo.Lavoro;
                        lavoroAction.ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY;
                        lavoroAction.EntitiesId.Add(wbsItem.EntityId);
                        lavoroAction.NewValore = new ValoreReale() { V = oreLavoro.ToString() };

                        modifyAction.NestedActions.Add(lavoroAction);
                    }

                }//end entities

                if (lavoroRedirect)
                {
                    modifyAction.NewValore = null;
                    modifyAction.AttributoCode = string.Empty;
                    modifyAction.ActionName = ActionName.MULTI;
                }
            }
            
            return true;
        }

        DateTimeCalculator CreateDateTimeCalculator(Guid calendarioId)
        {
            CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { calendarioId }).FirstOrDefault() as CalendariItem;
            if (calendario != null)
                return new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
            else
                return new DateTimeCalculator(WeekHours.Default, new CustomDays());
        }

        

        Dictionary<Guid, TreeEntityViewInfo> _filteredEntitiesViewInfo = null;
        List<Guid> _displayedEntitiesId = new List<Guid>();
        HashSet<Guid> _prevCheckedEntitiesId = new HashSet<Guid>();
        HashSet<Guid> _changedEntitiesId = new HashSet<Guid>();

        public override void MapDisplayedIndexes()
        {
            base.MapDisplayedIndexes();

            _filteredEntitiesViewInfo = new Dictionary<Guid, TreeEntityViewInfo>();
            foreach (var item in FilteredEntitiesViewInfo)
            {
                _filteredEntitiesViewInfo.Add(item.Key, item.Value.Clone());
            }



            //if (!Owner.IsGanttViewSyncCall)
            //    GanttView.OnDisplayedItemsChanged(filteredEntitiesViewInfo);
        }

        #region Evento di selezione

        public override void SelectIndex(int index, bool userOrigin = false)
        {
            base.SelectIndex(index, userOrigin);

            if (userOrigin && !_isCacheUpdating && !Owner.IsGanttViewSyncCalling)
            {
                _prevCheckedEntitiesId = new HashSet<Guid>(CheckedEntitiesId);
                GanttView?.OnCheckedItemsChanged(_prevCheckedEntitiesId);

            }
        }

        public override void CheckAll()
        {
            base.CheckAll();
            if (!_isCacheUpdating && !Owner.IsGanttViewSyncCalling)
            {
                _prevCheckedEntitiesId = new HashSet<Guid>(CheckedEntitiesId);
                GanttView.OnCheckedItemsChanged(_prevCheckedEntitiesId);
            }
        }

        public override Task UpdateCache(bool updateDetail = false)
        {
            _isCacheUpdating = true;
            return base.UpdateCache(updateDetail);
        }

        public override void Load()
        {
            base.Load();
        }

        /// <summary>
        /// Metodo da chiamare per aggiornare il gantt dopo aver finito il caricamento del master WBSItemsView
        /// </summary>
        /// <param name="wbsViewSync"></param>
        /// <param name="changedEntitiesId"></param>
        public void UpdateWBSView(WBSViewSyncEnum wbsViewSync, HashSet<Guid> changedEntitiesId)
        {
            _changedEntitiesId = changedEntitiesId;
            _WBSViewSyncBits |= (int)wbsViewSync;
            MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.WBS });
        }


        protected override void OnItemsLoaded(EventArgs e)
        {
            base.OnItemsLoaded(e);
            _isCacheUpdating = false;

            if (Owner.IsGanttViewSyncCalling)
                return;

            if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnWBSCommitAction) == (int)WBSViewSyncEnum.OnWBSCommitAction)
            {
                //non occorre far niente
                if (_filteredEntitiesViewInfo != null)
                {
                    GanttView.OnDisplayedItemsChanged(_filteredEntitiesViewInfo);
                    _filteredEntitiesViewInfo = null;
                    _displayedEntitiesId = new List<Guid>(DisplayedEntitiesId);
                }
            }
            else if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnAddPredecessors) == (int)WBSViewSyncEnum.OnAddPredecessors)
            {
                GanttView.OnAddPredecessors(_changedEntitiesId);
            }
            else if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnWBSItemsCreated) == (int)WBSViewSyncEnum.OnWBSItemsCreated)
            {
                GanttView.OnWBSItemsCreated();
            }
            else if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnUpdateWBSItems) == (int)WBSViewSyncEnum.OnUpdateWBSItems)
            {
                GanttView.OnWBSItemsUpdated();
            }
            else if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnAddAttivitaItems) == (int)WBSViewSyncEnum.OnAddAttivitaItems)
            {
                GanttView.OnWBSAttivitaItemsAdded(_changedEntitiesId);
            }
            else if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnReplaceAttributoPredecessori) == (int)WBSViewSyncEnum.OnReplaceAttributoPredecessori)
            {
                GanttView.OnReplaceAttributoPredecessori(_changedEntitiesId);
            }
            else if ((_WBSViewSyncBits & (int)WBSViewSyncEnum.OnFilterAndSort) == (int)WBSViewSyncEnum.OnFilterAndSort)
            {
                if (_filteredEntitiesViewInfo != null)
                {
                    if (!_displayedEntitiesId.SequenceEqual(DisplayedEntitiesId) || Owner.IsGanttViewToUpdate)
                    {
                        //System.Diagnostics.Debug.WriteLine("GanttView.OnFilteredItemsChanged");


                        GanttView?.OnFilteredItemsChanged(_filteredEntitiesViewInfo);
                        _filteredEntitiesViewInfo = null;
                        _displayedEntitiesId = new List<Guid>(DisplayedEntitiesId);
                    }
                }
            }
            else if (!_displayedEntitiesId.SequenceEqual(DisplayedEntitiesId))
            {

                if (_filteredEntitiesViewInfo != null)
                {
                    GanttView.OnDisplayedItemsChanged(_filteredEntitiesViewInfo);
                    _filteredEntitiesViewInfo = null;
                    _displayedEntitiesId = new List<Guid>(DisplayedEntitiesId);
                }
            }


            if (!_prevCheckedEntitiesId.SequenceEqual(CheckedEntitiesId))
            {
                _prevCheckedEntitiesId = new HashSet<Guid>(CheckedEntitiesId);
                GanttView?.OnCheckedItemsChanged(_prevCheckedEntitiesId);
            }

            _WBSViewSyncBits = (int)WBSViewSyncEnum.Nothing;
            Owner.IsGanttViewToUpdate = false;
        }

        #endregion Evento di selezione

        public override bool SelectEntityView(EntityView entView)
        {
            bool res = base.SelectEntityView(entView);

            if (GanttView != null)
            {
                if (!Owner.IsGanttViewSyncCalling)
                    GanttView.OnSelectedItemChanged(SelectedEntityId);
            }
            return res;


        }

        public override void ApplyFilterAndSort(Guid? selectEntityId = null, bool searchOnly = false)
        {
            _WBSViewSyncBits |= (int)WBSViewSyncEnum.OnFilterAndSort;
            base.ApplyFilterAndSort(selectEntityId, searchOnly);
        }

        public override IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            try
            {
                return base.LoadRange(startIndex, count, sortDescriptions, out overallCount);
            }
            catch (Exception exc)
            {
                overallCount = 0;
            }
            return null;
        }

        protected override void LoadingStateChanged(object sender, EventArgs e)
        {
            try
            {
                base.LoadingStateChanged(sender, e);
            }
            catch
            {

            }
        }

        internal void OnWBSItemsCreated()
        {
            _WBSViewSyncBits |= (int)WBSViewSyncEnum.OnWBSItemsCreated;
        }

        public ICommand CollegaEscapeCommand
        {
            get
            {
                return new CommandHandler(() => this.CollegaEscape());
            }
        }

        public void CollegaEscape()
        {
            IsButtonCollegaActive = false;
            IsActionCollegaReadyTComplete = false;
            CollegaEntitiesCount.Clear();
            UpdateUI();
            //RaisePropertyChanged(GetPropertyName(() => IsCollegaEntitiesNotificationEnabled));
            //RaisePropertyChanged(GetPropertyName(() => ReadyToCollegaEntitiesCount));
        }

        public override void Escape()
        {
            CollegaEscape();
            base.Escape();
        }

        public ICommand CollegaEntitiesCommand
        {
            get
            {
                return new CommandHandler(() => this.CollegaEntities());
            }
        }

        private bool IsButtonCollegaActive;
        private bool IsActionCollegaReadyTComplete;
        internal HashSet<Guid> CollegaEntitiesCount = new HashSet<Guid>();
        public string ReadyToCollegaEntitiesCount { get => CollegaEntitiesCount.Count.ToString(); }
        public bool IsCollegaEntitiesEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                return ((!IsMoveEntitiesAfterEnabled && IsAnyChecked));
                //|| (IsMoveEntitiesAfterEnabled && ReadyToModifyEntitiesCommand == ReadyToPasteEntitiesCommands.Copy)
            }
        }
        public bool IsCollegaEntitiesNotificationEnabled
        {
            get
            {
                try
                {
                    if (EntityType != null)
                    {
                        return IsButtonCollegaActive;
                    }
                }
                catch (Exception exc)
                {
                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                }
                return false;
            }
        }

        public bool IsCollegaModeEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (FilteredEntitiesId == null || !FilteredEntitiesId.Any())
                    return false;

                HashSet<Guid> filteredEntitiesId = new HashSet<Guid>(FilteredEntitiesId);
                if (filteredEntitiesId.Overlaps(CollegaEntitiesCount) && SelectedEntityView != null)
                {
                    if (IsActionCollegaReadyTComplete && !CollegaEntitiesCount.Contains(SelectedEntityView.Id))
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }
        public bool IsOtherNuttonEnabled
        {
            get
            {
                if (IsCollegaModeEnabled)
                    return false;
                else
                    return true;
            }
        }

        public bool IsScollegaEnabled
        {
            get
            {
                if (CheckedEntitiesId.Count() < 2)
                    return false;

                if (IsMoveEntitiesAfterEnabled || (IsRestrictedCommandsMode() && !CollegaEntitiesCount.Contains(SelectedEntityView.Id)))
                    return false;

                return true;
            }
        }


        private void CollegaEntities()
        {
            IsButtonCollegaActive = true;
            CollegaEntitiesCount = new HashSet<Guid>(FilteredEntitiesViewInfo.Where(item => CheckedEntitiesId.Contains(item.Key) && !HasChildren(item.Key)).Select(item => item.Key));
            //CollegaEntitiesCount = new HashSet<Guid>(CheckedEntitiesId);
            //RaisePropertyChanged(GetPropertyName(() => IsCollegaEntitiesNotificationEnabled));
            //RaisePropertyChanged(GetPropertyName(() => ReadyToCollegaEntitiesCount));
            IsActionCollegaReadyTComplete = true;
            UpdateUI();
        }

        public ICommand CollegaCommand
        {
            get
            {
                return new CommandHandlerParam((object param) => this.Collega(param));
            }
        }

        private void Collega(object param)
        {
            int ButtonOperation = int.Parse(param.ToString());
            Guid guid = SelectedEntityId;

            if (HasChildren(guid))
                return;

            WBSPredecessors WBSPredecessors = new WBSPredecessors();
            WBSPredecessors.Items = new List<WBSPredecessor>();
            List<Guid> guids = new List<Guid>();
            List<TreeEntity> TreeEntities = new List<TreeEntity>();

            foreach (var EntityId in CollegaEntitiesCount)
            {
                WBSPredecessor WBSPredecessor = new WBSPredecessor();
                switch (ButtonOperation)
                {

                    case 0:
                        WBSPredecessor.Type = WBSPredecessorType.FinishToStart;
                        break;
                    case 1:
                        WBSPredecessor.Type = WBSPredecessorType.StartToStart;
                        break;
                    case 2:
                        WBSPredecessor.Type = WBSPredecessorType.StartToFinish;
                        break;
                    case 3:
                        WBSPredecessor.Type = WBSPredecessorType.FinishToFinish;
                        break;
                    default:
                        break;
                }

                guids.Add(EntityId);
                TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, guids);
                WBSItem EntitySource = (WBSItem)TreeEntities.FirstOrDefault();
                guids.Clear();
                guids.Add(SelectedEntityId);
                TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, guids);
                WBSItem EntityTarget = (WBSItem)TreeEntities.FirstOrDefault();

                WBSPredecessor.WBSItemId = EntityId;
                WBSPredecessor.DelayDays = CalculateDelayDay(SelectedEntityId, EntitySource, EntityTarget);
                WBSPredecessors.Items.Add(WBSPredecessor);
            }


            _WBSViewSyncBits |= (int)WBSViewSyncEnum.OnAddPredecessors;
            Owner.AddPredecessors(guid, WBSPredecessors);

            CollegaEscape();
        }

        public bool IsCollegaEntitiesWaitingForTarget
        {
            get
            {
                if (IsButtonCollegaActive)
                {
                    if (CollegaEntitiesCount.Contains(SelectedEntityId))
                        return true;
                }
                return false;
            }
        }
        public double CalculateDelayDay(Guid TargetGuid, WBSItem TaskSource, WBSItem TaskTarget)
        {
            double Delays = 0;

            List<Guid> guids = new List<Guid>();
            guids.Add(TargetGuid);
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            TreeEntities = DataService.GetTreeEntitiesById(BuiltInCodes.EntityType.WBS, guids);
            WBSItem Entity = (WBSItem)TreeEntities.FirstOrDefault();
            CalendariItem calendario = GetCalendarioItem(Entity);
            DateTimeCalculator timeCalc = null;
            if (GetCalendarioItem(Entity) != null)
                timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
            else
                return 0;
            Delays = timeCalc.GetWorkingDaysBetween(TaskSource.GetDataInizio().Value, TaskTarget.GetDataInizio().Value) - 1;
            if (Delays < 0)
                Delays = 0;
            return Delays;
        }

        private CalendariItem GetCalendarioItem(WBSItem ent)
        {
            string wbsCalendarioIdCodice = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);

            Guid wbsCalendarioId = ent.GetAttributoGuidId(wbsCalendarioIdCodice);
            if (wbsCalendarioId != Guid.Empty)
            {
                CalendariItem calendario = DataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { wbsCalendarioId }).FirstOrDefault() as CalendariItem;
                return calendario;
            }
            return null;
        }

        public bool IsSelectInModel3dEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode())
                    return false;

                return IsAnyChecked && !IsMoveEntitiesAfterEnabled;
            }
        }

        public override void UpdateEntityType()
        {
            //_WBSViewSyncBits |= (int)WBSViewSyncEnum.OnUpdateWBSItems;
            base.UpdateEntityType();
        }

        


    }

    public enum WBSViewSyncEnum
    {
        Nothing = 0,
        OnWBSCommitAction = 1,
        OnCheckedItemsChanged = 2,
        OnSelectedItemChanged = 4,
        OnDisplayedItemsChanged = 8,
        OnWBSItemsCreated = 16,
        OnUpdateWBSItems = 32,
        OnReplaceAttributoPredecessori = 64,
        OnAddPredecessors = 128,
        OnAddAttivitaItems = 256,
        OnFilterAndSort = 512,
    }

}
