using _3DModelExchange;
using CommonResources;
using Commons;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using MasterDetailView;
using Model;
using Model.Calculators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComputoWpf
{

    public class ComputoItemView : MasterDetailGridItemView
    {
        public ComputoItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
        }
    }


    public class ComputoView : MasterDetailGridView
    {
        public I3DModelService Model3dService { get; set; }

        public ComputoItemsViewVirtualized ComputoItemsView { get => ItemsView as ComputoItemsViewVirtualized; }

        EntityTypeViewSettings _addComputoItemsViewSettings = null;
        string _externalPrezzarioFileName = string.Empty;

        public ComputoView()
        {
            ItemsView = new ComputoItemsViewVirtualized(this);
             
        }

        public override bool HasTableSummaryRow()
        {
            EntityType entType = DataService.GetEntityType(BuiltInCodes.EntityType.Computo);
            if (entType.Attributi.ContainsKey(BuiltInCodes.Attributo.FileOrigine))
                return false;

            return true;
        }

        internal void ShowCurrentRule(Guid ruleId)
        {
            if (Model3dService == null)
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Modello3dNonCaricato"));
                //MessageBox.Show(LocalizationProvider.GetString("Modello3dNonCaricato"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            ComputoItem currentComputoItem = ItemsView.SelectedEntityView.Entity as ComputoItem;
            if (currentComputoItem != null)
            {
                Model3dService.ShowRule(currentComputoItem.GetRuleId());
            }
        }

        public void ApplyRules(List<ComputoItemByRule> computoItems)
        {

            //inserisco le voci di computo (ComputoItem)
            ModelActionResponse mar;
            ModelAction action = new ModelAction() { EntityTypeKey = ComputoItemType.CreateKey() };
            action.ActionName = ActionName.MULTI_NODEPENDENTS;

            


            List<Guid> computoItemsFilteredIds = ComputoItemsView.FilteredEntitiesId;

            if (computoItemsFilteredIds.Any())
            {
                action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = computoItemsFilteredIds.Last(), TargetReferenceName = TargetReferenceName.AFTER } };
            }

            foreach (ComputoItemByRule computoItemByRule in computoItems)//per ogni voce di computo da aggiungere o modificare
            {
                if (computoItemByRule.ExistingComputoItemId != Guid.Empty)//azione di modifica attributi
                {
                    if (computoItemByRule.ToRemove)//item da rimuovere
                    {
                        ModelAction removeAction = new ModelAction() { EntityTypeKey = action.EntityTypeKey, ActionName = ActionName.ENTITY_DELETE };
                        removeAction.EntitiesId = new HashSet<Guid>() { computoItemByRule.ExistingComputoItemId };
                        action.NestedActions.Add(removeAction);
                    }
                    else //item da modificare
                    {

                        ModelAction attActionPrezzarioItemGuid = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.PrezzarioItem_Guid, EntityTypeKey = action.EntityTypeKey };
                        attActionPrezzarioItemGuid.NewValore = new ValoreGuid() { V = computoItemByRule.Rule.PrezzarioItemId };
                        attActionPrezzarioItemGuid.EntitiesId = new HashSet<Guid>() { computoItemByRule.ExistingComputoItemId };
                        action.NestedActions.Add(attActionPrezzarioItemGuid);

                        ModelAction attActionQuantita = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Quantita, EntityTypeKey = action.EntityTypeKey };
                        attActionQuantita.NewValore = new ValoreReale() { V = computoItemByRule.Rule.FormulaQta };
                        attActionQuantita.EntitiesId = new HashSet<Guid>() { computoItemByRule.ExistingComputoItemId };
                        action.NestedActions.Add(attActionQuantita);

                        foreach (string codiceAtt in computoItemByRule.Rule.FormuleByAttributoComputo.Keys)
                        {
                            string formula = computoItemByRule.Rule.FormuleByAttributoComputo[codiceAtt];

                            if (!string.IsNullOrEmpty(formula) && ItemsView.EntityType.Attributi.TryGetValue(codiceAtt, out Attributo att))
                            {
                                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                                {
                                    ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                                    attAction.NewValore = new ValoreTesto() { V = formula };
                                    attAction.EntitiesId = new HashSet<Guid>() { computoItemByRule.ExistingComputoItemId };
                                    action.NestedActions.Add(attAction);
                                }

                                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                                {
                                    ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                                    attAction.NewValore = new ValoreReale() { V = formula };
                                    attAction.EntitiesId = new HashSet<Guid>() { computoItemByRule.ExistingComputoItemId };
                                    action.NestedActions.Add(attAction);
                                }

                                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                                {
                                    ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                                    attAction.NewValore = new ValoreContabilita() { V = formula };
                                    attAction.EntitiesId = new HashSet<Guid>() { computoItemByRule.ExistingComputoItemId };
                                    action.NestedActions.Add(attAction);
                                }


                            }

                        }


                    }

                }
                else //azione di aggiunta della voce di computo e modifica attributi
                {
                    ModelAction addAction = new ModelAction() { EntityTypeKey = action.EntityTypeKey };

                    if (computoItemsFilteredIds.Any())
                        addAction.ActionName = ActionName.ENTITY_INSERT;
                    else
                        addAction.ActionName = ActionName.ENTITY_ADD;

                    ModelAction attActionElementiItemGuid = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.ElementiItem_Guid, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                    attActionElementiItemGuid.NewValore = new ValoreGuid() { V = computoItemByRule.ElementiItemId };
                    addAction.NestedActions.Add(attActionElementiItemGuid);

                    ModelAction attActionRuleGuid = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Model3dRuleId, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                    attActionRuleGuid.NewValore = new ValoreGuid() { V = computoItemByRule.Rule.Id };
                    addAction.NestedActions.Add(attActionRuleGuid);

                    ModelAction attActionRuleElementItemGuid = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Model3dRuleElementiItemId, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                    attActionRuleElementItemGuid.NewValore = new ValoreGuid() { V = computoItemByRule.ElementiItemId };
                    addAction.NestedActions.Add(attActionRuleElementItemGuid);

                    ModelAction attActionPrezzarioItemGuid = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.PrezzarioItem_Guid, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                    attActionPrezzarioItemGuid.NewValore = new ValoreGuid() { V = computoItemByRule.Rule.PrezzarioItemId };
                    addAction.NestedActions.Add(attActionPrezzarioItemGuid);

                    ModelAction attActionRule = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Model3dRule, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                    attActionRule.NewValore = new ValoreTesto() { V = computoItemByRule.Filter.Descri };
                    addAction.NestedActions.Add(attActionRule);

                    ModelAction attActionQuantita = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Quantita, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                    attActionQuantita.NewValore = new ValoreReale() { V = computoItemByRule.Rule.FormulaQta };
                    addAction.NestedActions.Add(attActionQuantita);

                    foreach (string codiceAtt in computoItemByRule.Rule.FormuleByAttributoComputo.Keys)
                    {
                        string formula = computoItemByRule.Rule.FormuleByAttributoComputo[codiceAtt];


                        if (!string.IsNullOrEmpty(formula) && ItemsView.EntityType.Attributi.TryGetValue(codiceAtt, out Attributo att))
                        {
                            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                            {
                                ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                                attAction.NewValore = new ValoreTesto() { V = formula };
                                addAction.NestedActions.Add(attAction);
                            }

                            else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                            {
                                ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                                attAction.NewValore = new ValoreReale() { V = formula };
                                addAction.NestedActions.Add(attAction);
                            }

                            else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                            {
                                ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAtt, EntitiesId = null, EntityTypeKey = action.EntityTypeKey };
                                attAction.NewValore = new ValoreContabilita() { V = formula };
                                addAction.NestedActions.Add(attAction);
                            }


                        }

                    }
                    


                    action.NestedActions.Add(addAction);
                }

            }

            //mar = ModelActionsStack.CommitAction(action, ItemsView);
            mar = ItemsView.CommitAction(action);

            if (mar.ActionResponse == ActionResponse.OK)
            {

                //Tolgo le voci di computo con quantità = 0
                var filter = new FilterData();
                filter.Items.Add(new AttributoFilterData
                {
                    EntityTypeKey = BuiltInCodes.EntityType.Computo,
                    FilterType = FilterTypeEnum.Result,
                    CodiceAttributo = BuiltInCodes.Attributo.Quantita,
                    IsFiltroAttivato = true,
                    CheckedValori = new HashSet<string>() { LocalizationProvider.GetString(ValoreHelper.ZeroRealResult) },
                    FoundEntitiesId = null,
                });

                List<Guid> entsFound = new List<Guid>();
                ItemsView.DataService.GetFilteredEntities(BuiltInCodes.EntityType.Computo, filter, null, null, out entsFound);

                if (entsFound.Count > 0)
                {
                    var res = System.Windows.MessageBox.Show(LocalizationProvider.GetString("CiSonoVociConQuantitaZeroVuoiEliminarle"), LocalizationProvider.GetString("AppName"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                    {
                        ModelAction deleteModelAction = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Computo, ActionName = ActionName.ENTITY_DELETE, EntitiesId = entsFound.ToHashSet() };
                        mar = ItemsView.CommitAction(deleteModelAction);
                    }
                }


                ItemsView.CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);

                ItemsView.Load();

            }

        }





        public ICommand AddComputoItemsCommand
        {
            get
            {
                return new CommandHandler(() => this.AddComputoItems());
            }
        }


        internal HashSet<Guid> AddComputoItems()
        {
            ComputoItem targetEntity = ComputoItemsView.GetActionTargetEntity() as ComputoItem;
            if (RightPanesView.FilterView.IsFilterApplied() && targetEntity == null)
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("SelezionareTargetSeFiltroAttivo"));
                return null;
            }

            List<Guid> selectedItems = new List<Guid>();
            EntityTypeViewSettings addComputoItemsViewSettings = _addComputoItemsViewSettings;
            string externalPrezzarioFileName = _externalPrezzarioFileName;
            if (WindowService.SelectPrezzarioIdsWindow(ref selectedItems, ref externalPrezzarioFileName,
                LocalizationProvider.GetString("AggiungiArticoli"), SelectIdsWindowOptions.Nothing, true, true, true, ref addComputoItemsViewSettings))
            {

                _addComputoItemsViewSettings = addComputoItemsViewSettings;
                _externalPrezzarioFileName = externalPrezzarioFileName;

                IEnumerable<Guid> prezzarioItems = null;

                Dictionary<string, IDataService> prezzariCache = MainOperation.GetPrezzariCache();
                if (prezzariCache.ContainsKey(externalPrezzarioFileName))
                {
                    //Importazione nel prezzario interno degli articoli 

                    EntitiesImportStatus importStatus = new EntitiesImportStatus();
                    importStatus.TargetPosition = TargetPosition.Bottom;
                    importStatus.ConflictAction = EntityImportConflictAction.Undefined;
                    importStatus.Source = prezzariCache[externalPrezzarioFileName];
                    importStatus.SourceName = externalPrezzarioFileName;
                    selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = PrezzarioItemType.CreateKey() }));

                    while (importStatus.Status != EntityImportStatusEnum.Completed)
                    {
                        DataService.ImportEntities(importStatus);
                        if (importStatus.Status == EntityImportStatusEnum.Waiting)
                        {
                            if (!WindowService.EntitiesImportWindow(importStatus))
                                break;
                        }
                    }


                    prezzarioItems = importStatus.StartingEntitiesId.Where(item => item.TargetId != Guid.Empty).Select(item => item.TargetId);

                    MainOperation.UpdateEntityTypesView(new List<string>(importStatus.EntityTypes.Keys));

                }
                else
                {
                    prezzarioItems = new List<Guid>(selectedItems);
                }


                return ComputoItemsView.AddComputoItems(prezzarioItems);
            }
            else
            {
                //potrei aver fatto modifiche al prezzario
                ItemsView.UpdateCache(true);
            }

            return null;
        }

        public bool IsAddComputoItemsEnabled { get => !ComputoItemsView.IsMoveEntitiesAfterEnabled; }



        /// <summary>
        /// Sostituisci Attributo Guid di riferimento attRif nell'item corrente
        /// </summary>
        public async override void ReplaceCurrentItemAttributoGuid(AttributoRiferimento attRif, EntityTypeViewSettings viewSettings)
        {
            if (ItemsView.IsMultipleModify && !ItemsView.IsAnyChecked)
                return;

            if (!ItemsView.IsMultipleModify && ItemsView.SelectedEntityId == Guid.Empty)
                return;


            DataService.Suspended = true;

            EntityView currentItemView = null;
            if (ItemsView.SelectedEntityId != Guid.Empty)
                currentItemView = ItemsView.SelectedEntityView;


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


            if (entityType.GetKey() == BuiltInCodes.EntityType.Prezzario)
            {
                //EntityTypeViewSettings viewSettings = null;
                string externalPrezzarioFileName = string.Empty;

                if (WindowService.SelectPrezzarioIdsWindow(ref selectedItems, ref externalPrezzarioFileName,
                    title, SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, true, true, true, ref viewSettings))
                {

                    IEnumerable<Guid> prezzarioInternoIds = null;

                    Dictionary<string, IDataService> prezzariCache = MainOperation.GetPrezzariCache();
                    if (prezzariCache.ContainsKey(externalPrezzarioFileName))
                    {
                        //Importazione nel prezzario interno degli articoli 

                        EntitiesImportStatus importStatus = new EntitiesImportStatus();
                        importStatus.TargetPosition = TargetPosition.Bottom;
                        importStatus.ConflictAction = EntityImportConflictAction.Undefined;
                        importStatus.Source = prezzariCache[externalPrezzarioFileName];
                        importStatus.SourceName = externalPrezzarioFileName;
                        selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = PrezzarioItemType.CreateKey() }));

                        //DataService.ImportEntities(importStatus);
                        while (importStatus.Status != EntityImportStatusEnum.Completed)
                        {
                            DataService.ImportEntities(importStatus);
                            if (importStatus.Status == EntityImportStatusEnum.Waiting)
                            {
                                if (!WindowService.EntitiesImportWindow(importStatus))
                                    break;
                            }
                        }

                        prezzarioInternoIds = importStatus.StartingEntitiesId.Select(item => item.TargetId);

                        if (prezzarioInternoIds.Count() == 1)
                        {
                            Guid entityId = prezzarioInternoIds.First();
                            ItemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = entityId });
                            MainOperation.UpdateEntityTypesView(new List<string>(importStatus.EntityTypes.Keys));
                        }

                    }
                    else
                    {
                        //aggiunta di articoli dal prezzario interno
                        prezzarioInternoIds = selectedItems;
                        if (prezzarioInternoIds.Count() == 1)
                        {
                            Guid entityId = prezzarioInternoIds.First();
                            ItemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = entityId });
                            MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.Prezzario });
                        }
                    }

                }
            }
            else
            {
                if (WindowService.SelectEntityIdsWindow(entityType.GetKey(), ref selectedItems, title,
                    SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, null, attRif))
                {
                    if (selectedItems.Count == 1)
                    {
                        Guid entityId = selectedItems.First();
                        ItemsView.AttributiEntities.SetValoreAttributo(attRif.ReferenceCodiceGuid, new ValoreGuid() { V = entityId });
                    }
                }
            }

            await ItemsView.UpdateCache(true);

        }


        #region Model 3d Selection

        public ICommand SelectInModel3dCommand
        {
            get
            {
                return new CommandHandler(() => this.SelectInModel3d());
            }
        }

        void SelectInModel3d()
        {
            List<Model3dObjectKey> model3DObjectsKey = GetModel3dObjectsKeyByComputoItemsId(ItemsView.CheckedEntitiesId);

            if (Model3dService != null)
                Model3dService.SelectElements(model3DObjectsKey);
            else
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Modello3dNonCaricato"));


        }

        private List<Model3dObjectKey> GetModel3dObjectsKeyByComputoItemsId(HashSet<Guid> computoItemsId)
        {
            //entities selected (checked)
            List<Entity> computoItems = DataService.GetEntitiesById(BuiltInCodes.EntityType.Computo, computoItemsId);

            //Ricavo gli ElementId da computoItems
            HashSet<Guid> elementsId = new HashSet<Guid>();
            computoItems.ForEach(item =>
            {
                ComputoItem computoItem = item as ComputoItem;
                //Guid elmGuid = computoItem.GetElementiItemId();
                var elmGuid = computoItem.GetElementiItemsId();
                elementsId.UnionWith(elmGuid);
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

            if (computoItemsFound.Any())
            {
                ItemsView.RightPanesView.FilterView.LoadTemporaryFilterByIds(BuiltInCodes.EntityType.Computo, computoItemsFound);
                //ItemsView.RightPanesView.FilterView.SearchNext();

            }
            else
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NessunElementoTrovatoNellaSezioneCorrente"));
            }
        }


        #endregion Model 3d Selection


    }

    public class ComputoItemsViewVirtualized : MasterDetailGridItemsViewVirtualized
    {

        public ComputoItemsViewVirtualized(MasterDetailGridView computoView) :base(computoView)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Computo];

            AttributiEntities.Load(new HashSet<Guid>());


        }



        protected override EntityView NewItemView(Entity entity)
        {
            return new ComputoItemView(this, entity);
        }

        public HashSet<Guid> AddComputoItems(/*ComputoItem insertPositionItem,*/ IEnumerable<Guid> prezzarioItemIds)
        {
            //MULTIACTION
            //-ENTITY_INSERT
            //--ATTRIBUTO_VALORE_MODIFY
            //--ATTRIBUTO_VALORE_MODIFY
            //--...
            //-ENTITY_INSERT
            //--ATTRIBUTO_VALORE_MODIFY
            //--...

            

            ModelActionResponse mar;
            ModelAction action = new ModelAction() { EntityTypeKey = this.EntityType.GetKey() };

            List<string> groupNames = RightPanesView.GroupView.Items.Select(item => item.Attributo.Codice).ToList();

            //ottengo il primo elemento del gruppo corrente (per l'inserimento di nuovi articoli)
            Guid firstPrezzarioItemsOfCurrentGroup = FindNextInCurrentGroup(Guid.Empty);

            Guid lastComputoItem = FilteredEntitiesId.LastOrDefault();

            //lista degli articoli (id) utilizzati nel computo
            HashSet<Guid> prezzarioItemsId = GetPrezzarioItemsId();

            ComputoItem targetEntity = GetActionTargetEntity() as ComputoItem;

            List<Guid> prezzarioItemIdsList = prezzarioItemIds.ToList();

            if (FilteredEntitiesId.Any())
                prezzarioItemIdsList.Reverse();

            for (int i=0; i < prezzarioItemIdsList.Count(); i++)
            {
                Guid prezItemId = prezzarioItemIdsList[i];

                ModelAction addAction = new ModelAction() { EntityTypeKey = EntityType.GetKey() };
                if (FilteredEntitiesId.Any())
                    addAction.ActionName = ActionName.ENTITY_INSERT;
                else
                    addAction.ActionName = ActionName.ENTITY_ADD;


                //Setto l'attributo PrezzarioItemGuid        
                ModelAction attAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.PrezzarioItem_Guid, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };/*ComputoItem_*/
                attAction.NewValore = new ValoreGuid() { V = prezItemId };
                addAction.NestedActions.Add(attAction);

                SetValoriOfTargetGroups(addAction, targetEntity);
                SetValoriOfCurrentFilter(addAction/*, targetEntity*/);


                //////////////////////////////
                //Se è stato raggruppato per PrezzarioItem l'aggiunta di articoli viene gestita in modo particolare

                //Attributi del computo riferiti a "PrezzatioItem.Guid"
                IEnumerable<string> prezzarioItemAttsCode = EntityType.Attributi.Values.Where(item =>
                {
                    AttributoRiferimento attRif = item as AttributoRiferimento;
                    if (attRif != null)
                    {
                        if (attRif.ReferenceCodiceGuid == BuiltInCodes.Attributo.PrezzarioItem_Guid)
                            return true;
                    }
                    return false;
                }).Select(item => item.Codice);

                //testo se è stato raggruppato per almeno un attributo dell'articolo (PrezzarioItem)
                bool prezzarioItemGrouped = false;
                foreach (AttributoGroupView attGroup in RightPanesView.GroupView.Items)
                {
                    if (prezzarioItemAttsCode.Contains(attGroup.Attributo.Codice))
                    {
                        prezzarioItemGrouped = true;
                        break;
                    }
                }

                if (prezzarioItemGrouped && CurrentGroupKey != null && CurrentGroupKey.Any())//un item selezionato
                {
                    //1. I nuovi articoli vanno sotto il primo item del gruppo corrente
                    //2. Gli articoli già presenti vanno in fondo alla lista 
                    //3. Gli articoli che sono del gruppo corrente vanno sotto l'item selezionato

                    if (!prezzarioItemsId.Contains(prezItemId))
                    {
                       addAction.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = firstPrezzarioItemsOfCurrentGroup, TargetReferenceName = TargetReferenceName.AFTER } };
                    }
                    else
                    {
                        if (prezItemId != targetEntity.GetPrezzarioItemId()) //aggiungo in fondo
                            addAction.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = lastComputoItem, TargetReferenceName = TargetReferenceName.AFTER } };
                        else
                            addAction.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = targetEntity.EntityId, TargetReferenceName = TargetReferenceName.AFTER } };

                    }
                }
                else if (targetEntity != null)
                    addAction.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = targetEntity.EntityId, TargetReferenceName = TargetReferenceName.AFTER } };
                else
                    addAction.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = lastComputoItem, TargetReferenceName = TargetReferenceName.AFTER } };



                action.NestedActions.Add(addAction);
            }

            //inserimento dell'entità
            if (SelectedIndex >= 0 && SelectedIndex < Entities.Count - 1) //inserisco dopo SelectedIndex
            {
                action.ActionName = ActionName.MULTI;
                action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = FilteredEntitiesId[SelectedIndex], TargetReferenceName = TargetReferenceName.AFTER } };
                //mar = ModelActionsStack.CommitAction(action, this);
                mar = CommitAction(action);

            }
            else //aggiungo in coda
            {

                action.ActionName = ActionName.MULTI;
                if (FilteredEntitiesId.Any())
                {
                    action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = FilteredEntitiesId.Last(), TargetReferenceName = TargetReferenceName.AFTER } };
                }

                //mar = ModelActionsStack.CommitAction(action, this);
                mar = CommitAction(action);

            }

            if (mar.ActionResponse == ActionResponse.OK)
            {
                CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);

                ApplyFilterAndSort(mar.NewIds.FirstOrDefault());


                PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandNewEntitiesGroup;//per l'espansione dopo aver aggiunto un nuovo item di cui non esistono ancora i raggruppamenti
                PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                PendingCommand |= EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView;
                UpdateCache();

                RaisePropertyChanged(GetPropertyName(() => this.EntitiesCount));

            }
            return mar.NewIds;
        }


        public bool IsPrezzarioItemReplaceEnabled
        {
            get { return IsAnySelected; }
        }

        public bool IsPrezzarioItemAddEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode())
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            RaisePropertyChanged(GetPropertyName(() => IsPrezzarioItemAddEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsSelectInModel3dEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsExportExcelEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsRecalculateItemsNeeded));
            RaisePropertyChanged(GetPropertyName(() => IsRecalculateItemsCommandAllowed));
            RaisePropertyChanged(GetPropertyName(() => RecalculateItemsCommandColor));
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

        public bool IsExportExcelEnabled
        {
            get
            {
                if (DataService == null || DataService.IsReadOnly)
                    return false;

                if (IsRestrictedCommandsMode())
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }

        public HashSet<Guid> GetPrezzarioItemsId()
        {
            string sepInKey = "|";
            Dictionary<string, Guid> computoItemIdsByPrezzarioItemsId = DataService.CreateKey(BuiltInCodes.EntityType.Computo, sepInKey, new List<string>() { BuiltInCodes.Attributo.PrezzarioItem_Guid }, out _);

            HashSet<Guid> prezzarioItemsId = new HashSet<Guid>();
            foreach (string key in computoItemIdsByPrezzarioItemsId.Keys)
            {
                Guid id = new Guid(key);
                prezzarioItemsId.Add(id);
            }
            return prezzarioItemsId;
        }

        protected override void SetValoriBuiltIn(ModelAction pasteAction, Entity targetEntity)
        {
            base.SetValoriBuiltIn(pasteAction, targetEntity);

            ModelAction attActionRuleId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Model3dRuleId, EntitiesId = null, EntityTypeKey = EntityType.GetKey() };
            attActionRuleId.NewValore = new ValoreGuid() { V = Guid.Empty };
            pasteAction.NestedActions.Add(attActionRuleId);
        }

        

        public override void ReplaceValore(ValoreView valoreView)
        {
            if (DataService == null || DataService.IsReadOnly)
                return;

            string codiceAttributo = valoreView.Tag as string;
            Attributo att = EntityType.Attributi[codiceAttributo];
            if (att == null)
                return;

            if (att.Codice == BuiltInCodes.Attributo.Model3dRule) //doppio click sul valore testo della regola
            {
                ComputoItem currentComputoItem = SelectedEntityView.Entity as ComputoItem;
                if (currentComputoItem != null)
                {
                    (Owner as ComputoView).ShowCurrentRule(currentComputoItem.GetRuleId());
                }
            }

            base.ReplaceValore(valoreView);
        }

        public override IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        {
            return base.LoadRange(startIndex, count, sortDescriptions, out overallCount);
        }

        public ICommand RecalculateComputoItemsCommand { get => new CommandHandler(() => this.RecalculateComputoItems()); }
        void RecalculateComputoItems()
        {
            EntitiesError error = new EntitiesError();
            var calcOpt = new EntityCalcOptions() { CalcolaAttributiResults = true, ResetCalulatedValues = true };
            DataService.CalcolaEntities(EntityType.GetKey(), calcOpt, null, error);

            List<string> dependentEntityTypes = GetDependentEntityTypesKey();
            dependentEntityTypes.Insert(0, BuiltInCodes.EntityType.Computo);
            MainOperation.UpdateEntityTypesView(dependentEntityTypes);

            //IsRecalculateItemsNeeded = false;
        }

    }
}
