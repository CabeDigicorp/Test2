using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Mvvm.POCO;
using DevExpress.Pdf.Native.BouncyCastle.Ocsp;
using DevZest.Windows.DataVirtualization;
using DivisioniWpf;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ElementiWpf
{
    public class ElementiItemView : MasterDetailGridItemView
    {
        public ElementiItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {

        }

        //public DivisioneItem GetDivisioneItem(string codiceAttributoGuid, string entityTypeKey)
        //{
        //    ElementiItem elmItem = Entity as ElementiItem;
        //    Guid divItemId = elmItem.GetDivisioneItemId(codiceAttributoGuid);

        //    if (divItemId != Guid.Empty)
        //    {
        //        IEnumerable<Entity> ents;
        //        ents = _master.DataService.GetTreeEntitiesDeepById(entityTypeKey, new List<Guid>() { divItemId });/*BuiltInCodes.Divisione*/
        //        return ents.LastOrDefault() as DivisioneItem;
        //    }
        //    return null;
        //}
    }

    public class ElementiView : MasterDetailGridView
    {
        public I3DModelService Model3dService { get; set; }

        public ElementiItemsViewVirtualized ElementiItemsView { get => ItemsView as ElementiItemsViewVirtualized; }

        public ElementiView()
        {
            ItemsView = new ElementiItemsViewVirtualized(this);
            //RowOnTopCount = 1; //columns header

        }


        ///// <summary>
        ///// Aggiunge n Elementi settando l'attributo globalId e nomeFile e li collega con le divisioni già inserite
        ///// </summary>
        ///// <param name="globalIds"></param>
        //public Dictionary<Model3dObjectKey, Guid> AddElementiItemsByModel3d(List<Model3dElementRelation> elmsKey)
        //{
        //    //N.B. guidsReturned e elmsKey sono liste allineate
        //    Dictionary<Model3dObjectKey, Guid> guidsReturned = new Dictionary<Model3dObjectKey, Guid>(new Model3dObjectKeyEqualityComparer());
        //    elmsKey.ForEach(item => guidsReturned.Add(item.Model3dObject, Guid.Empty));


        //    //Costruisco una nuova chiave per controllare i doppioni in base a BuiltInCodes.Attributo.IfcFileName, BuiltInCodes.Attributo.GlobalId
        //    string sepInKey = "|";
        //    Dictionary<string, Guid> elItemIdsByKey = DataService.CreateKey(this.ItemsView.EntityType.GetKey(), sepInKey, new List<string>() { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId });
        //    //

        //    ModelActionResponse mar;
        //    ModelAction action = new ModelAction() { EntityTypeKey = this.ItemsView.EntityType.GetKey() };
        //    action.ActionName = ActionName.MULTI;
        //    if (ItemsView.FilteredEntitiesId.Any())
        //    {
        //        action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = ItemsView.FilteredEntitiesId.Last(), TargetReferenceName = TargetReferenceName.AFTER } };
        //    }

        //    List<Model3dObjectKey> m3dObjKeyAdded = new List<Model3dObjectKey>();
        //    int index = 0;

        //    for (index = 0; index < elmsKey.Count; index++)
        //    {
        //        Model3dElementRelation elmKey = elmsKey[index];

        //        //controllo il doppione (se fileName e globalId uguali)
        //        string key = string.Join(sepInKey, elmKey.Model3dObject.ProjectGlobalId, elmKey.Model3dObject.GlobalId);
        //        if (elItemIdsByKey.ContainsKey(key))
        //        {
        //            guidsReturned[elmKey.Model3dObject] = elItemIdsByKey[key];
        //            continue;
        //        }
        //        else
        //        {
        //            m3dObjKeyAdded.Add(elmKey.Model3dObject);
        //        }


        //        ModelAction addAction = new ModelAction() { EntityTypeKey = ItemsView.EntityType.GetKey() };
        //        if (ItemsView.FilteredEntitiesId.Any())
        //            addAction.ActionName = ActionName.ENTITY_INSERT;
        //        else
        //            addAction.ActionName = ActionName.ENTITY_ADD;

        //        //azione di modifica attributo
        //        ModelAction attActionGlobalId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.GlobalId, EntitiesId = null, EntityTypeKey = ItemsView.EntityType.GetKey() };
        //        attActionGlobalId.NewValore = new ValoreTesto() { V = elmKey.Model3dObject.GlobalId };
        //        addAction.NestedActions.Add(attActionGlobalId);

        //        ModelAction attActionFileName = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.ProjectGlobalId, EntitiesId = null, EntityTypeKey = ItemsView.EntityType.GetKey() };
        //        attActionFileName.NewValore = new ValoreTesto() { V = elmKey.Model3dObject.ProjectGlobalId };
        //        addAction.NestedActions.Add(attActionFileName);

        //        //Attributi di tipo divisioni
        //        foreach (Attributo att in ElementiItemsView.EntityType.Attributi.Values)
        //        {
        //            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
        //            {
        //                string codiceAttributo = att.Codice;
        //                DivisioneItemId rel = elmKey.Divisioni[att.GuidReferenceEntityTypeKey];

        //                ModelAction attActionDiv = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAttributo, EntitiesId = null, EntityTypeKey = ItemsView.EntityType.GetKey() };
        //                attActionDiv.NewValore = new ValoreGuid() { V = rel.Id };
        //                addAction.NestedActions.Add(attActionDiv);
        //            }
        //        }


        //        action.NestedActions.Add(addAction);
        //    }
        //    //mar = ModelActionsStack.CommitAction(action, ItemsView);
        //    mar = ItemsView.CommitAction(action);



        //    if (mar.ActionResponse == ActionResponse.OK)
        //    {
        //        ItemsView.CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);

        //        ItemsView.ApplyFilterAndSort(mar.NewIds.FirstOrDefault());

        //        ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
        //        ItemsView.UpdateCache();

        //        RaisePropertyChanged(GetPropertyName(() => this.ItemsView.EntitiesCount));

        //        //N.B. newIds e elItemsKeyIndex sono due liste allineate
        //        List<Guid> newIds = mar.NewIds.ToList();

        //        for (index = 0; index < newIds.Count; index++)
        //        {
        //            //guidsReturned[elItemsKeyIndex[index]] = newIds[index];
        //            guidsReturned[m3dObjKeyAdded[index]] = newIds[index];
        //        }
        //    }

        //    return guidsReturned;
        //}

        /// <summary>
        /// Aggiunge n Elementi settando l'attributo globalId e nomeFile e li collega con le divisioni già inserite
        /// </summary>
        /// <param name="globalIds"></param>
        public Dictionary<string, Guid> AddElementiItemsByModel3d(List<Model3dElementRelation> elmsKey, Guid groupId)
        {
            //N.B. guidsReturned e elmsKey sono liste allineate
            //Dictionary<Model3dObjectKey, Guid> guidsReturned = new Dictionary<Model3dObjectKey, Guid>(new Model3dObjectKeyEqualityComparer());
            Dictionary<string, Guid> guidsReturned = new Dictionary<string, Guid>();
            elmsKey.ForEach(item => guidsReturned.Add(item.Model3dObject.GetKey(), Guid.Empty));


            //Costruisco una nuova chiave per controllare i doppioni in base a BuiltInCodes.Attributo.IfcFileName, BuiltInCodes.Attributo.GlobalId
            string sepInKey = "|";
            Dictionary<string, Guid> elItemIdsByKey = DataService.CreateKey(this.ItemsView.EntityType.GetKey(), sepInKey, new List<string>() { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId }, out _);
            //


            EntityType elemEntityType = DataService.GetEntityType(BuiltInCodes.EntityType.Elementi);

            ModelActionResponse mar;
            ModelAction action = new ModelAction() { EntityTypeKey = this.ItemsView.EntityType.GetKey() };
            action.ActionName = ActionName.MULTI_NODEPENDENTS;
            
            if (ItemsView.FilteredEntitiesId.Any())
            {
                action.NewTargetEntitiesId = new List<TargetReference>(1) { new TargetReference() { Id = ItemsView.FilteredEntitiesId.Last(), TargetReferenceName = TargetReferenceName.AFTER } };
            }

            List<Model3dObjectKey> m3dObjKeyAdded = new List<Model3dObjectKey>();

            EntitiesHelper entsHelper = new EntitiesHelper(DataService);

            
            if (groupId != Guid.Empty)
            {
                Attributo attIfcClassName = null;
                elemEntityType.Attributi.TryGetValue(BuiltInCodes.Attributo.IfcClass, out attIfcClassName);
                if (attIfcClassName != null)
                {
                    if (attIfcClassName.IsValoreLockedByDefault)
                    {
                        MainOperation.ShowMessageBarView(string.Format("{0}: {1}", LocalizationProvider.GetString("L'attributo risulta lucchettato"), attIfcClassName.Etichetta));
                        return guidsReturned;
                    }
                }

                DivisioneItemType divItemType = entsHelper.GetDivisioneTypeByCodice(BuiltInCodes.Attributo.IfcGroup);
                if (divItemType == null)
                    return guidsReturned;

                Entity group = DataService.GetEntityById(divItemType.GetKey(), groupId);
                if (group == null)
                    return guidsReturned; 
                
                //Aggiungo il gruppo in elementi
                string groupName = entsHelper.GetValorePlainText(group, BuiltInCodes.Attributo.Nome, false, false);


                ////////////////
                //Aggiungo gruppo agli elementi
                ModelAction addAction = new ModelAction() { EntityTypeKey = ItemsView.EntityType.GetKey() };
                addAction.ActionName = ActionName.ENTITY_ADD;

                //azione di modifica attributo
                ModelAction attActionNome = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.Nome, EntitiesId = null, EntityTypeKey = elemEntityType.GetKey() };
                attActionNome.NewValore = new ValoreTesto() { V = String.Format("att{{{0}}}", BuiltInCodes.Attributo.GlobalId) };
                addAction.NestedActions.Add(attActionNome);

                //azione di modifica attributo
                ModelAction attActionClassName = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.IfcClass, EntitiesId = null, EntityTypeKey = elemEntityType.GetKey() };
                attActionClassName.NewValore = new ValoreTesto() { V = BuiltInCodes.Attributo.IfcGroup };
                addAction.NestedActions.Add(attActionClassName);

                //azione di modifica attributo
                ModelAction attActionGlobalId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.GlobalId, EntitiesId = null, EntityTypeKey = elemEntityType.GetKey() };
                attActionGlobalId.NewValore = new ValoreTesto() { V = groupName };
                addAction.NestedActions.Add(attActionGlobalId);

                //azione di modifica attributo
                ModelAction attActionAssociaGroup = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORECOLLECTION_ADD, AttributoCode = BuiltInCodes.Attributo.IfcGroup, EntitiesId = null, EntityTypeKey = elemEntityType.GetKey() };
                ValoreGuidCollectionItem newCollItem = new ValoreGuidCollectionItem() { Id = Guid.NewGuid(), EntityId = groupId };
                attActionAssociaGroup.NewValore = newCollItem;
                addAction.NestedActions.Add(attActionAssociaGroup);

                action.NestedActions.Add(addAction);

                m3dObjKeyAdded.Add(new Model3dObjectKey());

                ///////////////////////////////


                //imposto le relazioni con le divisioni
                foreach (var elmKey in elmsKey)
                {
                    elmKey.Divisioni.TryAdd(divItemType.GetKey(), new HashSet<DivisioneItemId>());
                    elmKey.Divisioni[divItemType.GetKey()].Add(new DivisioneItemId() { EntityTypeKey = divItemType.GetKey(), Id = groupId, ItemPath = string.Empty });
                }
            }


            
            int index = 0;

            for (index = 0; index < elmsKey.Count; index++)
            {
                Model3dElementRelation elmKey = elmsKey[index];

                //controllo il doppione (se fileName e globalId uguali)

                ModelAction entAction = null;

                Guid entityId = Guid.Empty;
                string key = string.Join(sepInKey, elmKey.Model3dObject.ProjectGlobalId, elmKey.Model3dObject.GlobalId);
                if (elItemIdsByKey.ContainsKey(key)) //elemento già esistente
                {
                    guidsReturned[elmKey.Model3dObject.GetKey()] = elItemIdsByKey[key];

                    entAction = new ModelAction() { EntityTypeKey = ItemsView.EntityType.GetKey(), ActionName = ActionName.MULTI, EntitiesId = new HashSet<Guid>() { elItemIdsByKey[key] } };
                    entityId = elItemIdsByKey[key];
                }
                else
                {
                    m3dObjKeyAdded.Add(elmKey.Model3dObject);  
                    
                    
                    entAction = new ModelAction() { EntityTypeKey = ItemsView.EntityType.GetKey() };
                    if (ItemsView.FilteredEntitiesId.Any())
                        entAction.ActionName = ActionName.ENTITY_INSERT;
                    else
                        entAction.ActionName = ActionName.ENTITY_ADD;

                    //azione di modifica attributo
                    ModelAction attActionGlobalId = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.GlobalId, EntitiesId = null, EntityTypeKey = ItemsView.EntityType.GetKey() };
                    attActionGlobalId.NewValore = new ValoreTesto() { V = elmKey.Model3dObject.GlobalId };
                    entAction.NestedActions.Add(attActionGlobalId);

                    ModelAction attActionFileName = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = BuiltInCodes.Attributo.ProjectGlobalId, EntitiesId = null, EntityTypeKey = ItemsView.EntityType.GetKey() };
                    attActionFileName.NewValore = new ValoreTesto() { V = elmKey.Model3dObject.ProjectGlobalId };
                    entAction.NestedActions.Add(attActionFileName);
                }



                //Attributi di tipo divisioni
                foreach (Attributo att in ElementiItemsView.EntityType.Attributi.Values)
                {
                    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                    {
                        EntityType entType = DataService.GetEntityType(att.GuidReferenceEntityTypeKey);
                        if (entType is DivisioneItemType)
                        {
                            string itemPath = string.Empty;
                            if (att.ValoreAttributo is ValoreAttributoGuid valAttGuid)
                            {
                                itemPath = valAttGuid.ItemPath;
                            }


                            string codiceAttributo = att.Codice;
                            if (elmKey.Divisioni.ContainsKey(att.GuidReferenceEntityTypeKey))
                            {
                                DivisioneItemId rel;

                                //if (DeveloperVariables.IsTesting)
                                rel = elmKey.Divisioni[att.GuidReferenceEntityTypeKey].FirstOrDefault(item => item.ItemPath == itemPath);
                                //else
                                //rel = elmKey.Divisioni[att.GuidReferenceEntityTypeKey].FirstOrDefault();

                                ModelAction attActionDiv = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY, AttributoCode = codiceAttributo, EntityTypeKey = ItemsView.EntityType.GetKey() };
                                if (entityId == Guid.Empty)
                                    attActionDiv.EntitiesId = null;
                                else
                                    attActionDiv.EntitiesId.Add(entityId);

                                attActionDiv.NewValore = new ValoreGuid() { V = rel.Id };
                                entAction.NestedActions.Add(attActionDiv);

                            }
                        }
                    }
                    else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        EntityType entType = DataService.GetEntityType(att.GuidReferenceEntityTypeKey);
                        if (entType is DivisioneItemType)
                        {
                            string codiceAttributo = att.Codice;
                            if (elmKey.Divisioni.ContainsKey(att.GuidReferenceEntityTypeKey))
                            {
                                HashSet<DivisioneItemId> rel = elmKey.Divisioni[att.GuidReferenceEntityTypeKey];


                                foreach (DivisioneItemId divItemId in rel)
                                {  
                                    ModelAction attActionDiv = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORECOLLECTION_ADD, AttributoCode = codiceAttributo, EntityTypeKey = ItemsView.EntityType.GetKey() };
                                    if (entityId == Guid.Empty)
                                        attActionDiv.EntitiesId = null;
                                    else
                                        attActionDiv.EntitiesId.Add(entityId);
                                    
                                    attActionDiv.NewValore = new ValoreGuidCollectionItem() { Id = Guid.NewGuid(), EntityId = divItemId.Id };
                                    entAction.NestedActions.Add(attActionDiv);
                                }

                            }
                        }
                    }
                }


                action.NestedActions.Add(entAction);
            }
            //mar = ModelActionsStack.CommitAction(action, ItemsView);
            mar = ItemsView.CommitAction(action);



            if (mar.ActionResponse == ActionResponse.OK)
            {
                ItemsView.CheckedEntitiesId = new HashSet<Guid>(mar.NewIds);

                ItemsView.ApplyFilterAndSort(mar.NewIds.FirstOrDefault());

                ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                ItemsView.UpdateCache();

                RaisePropertyChanged(GetPropertyName(() => this.ItemsView.EntitiesCount));

                //N.B. newIds e elItemsKeyIndex sono due liste allineate
                List<Guid> newIds = mar.NewIds.ToList();

                for (index = 0; index < newIds.Count; index++)
                {
                    guidsReturned[m3dObjKeyAdded[index].GetKey()] = newIds[index];
                }
            }

            return guidsReturned;
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

            if (Model3dService == null)
                return;

            

            //entities selected (checked)
            List<Entity> ents = DataService.GetEntitiesById(BuiltInCodes.EntityType.Elementi, ItemsView.CheckedEntitiesId);

            List<Model3dObjectKey> model3DObjectsKey = new List<Model3dObjectKey>();
            ents.ForEach(item =>
            {
                EntitiesHelper entsHelper = new EntitiesHelper(DataService);
                entsHelper.GetModel3dObjectsByElement(item, model3DObjectsKey);
            });

            if (Model3dService != null)
                Model3dService.SelectElements(model3DObjectsKey);
            else
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Modello3dNonCaricato"));




        }


        public void SelectItemsByModel3d(List<Model3dObjectKey> elementsId)
        {
            FilterData filterData = new FilterData();

            AttributoFilterData attFilterGlobalId = new AttributoFilterData()
            {
                CodiceAttributo = BuiltInCodes.Attributo.GlobalId,
                CheckedValori = new HashSet<string>(elementsId.Select(item => item.GlobalId)),
                EntityTypeKey = BuiltInCodes.EntityType.Elementi,
                IsFiltroAttivato = true,
                IgnoreCase = false,
            };
            filterData.Items.Add(attFilterGlobalId);

            AttributoFilterData attFilterIfcFileName = new AttributoFilterData()
            {
                CodiceAttributo = BuiltInCodes.Attributo.ProjectGlobalId,
                CheckedValori = new HashSet<string>(elementsId.Select(item => item.ProjectGlobalId)),
                EntityTypeKey = BuiltInCodes.EntityType.Elementi,
                IsFiltroAttivato = true,
                IgnoreCase = false,
            };
            filterData.Items.Add(attFilterIfcFileName);

            List<Guid> entitiesFound = new List<Guid>();
            List<EntityMasterInfo> elementiItems = DataService.GetFilteredEntities(BuiltInCodes.EntityType.Elementi, filterData, null, null, out entitiesFound);

            if (entitiesFound.Any())
            {
                ItemsView.RightPanesView.FilterView.LoadTemporaryFilterByIds(BuiltInCodes.EntityType.Elementi, entitiesFound);
            }
            else
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NessunElementoTrovatoNellaSezioneCorrente"));
            }
        }


        #endregion

    }

    public class ElementiItemsViewVirtualized : MasterDetailGridItemsViewVirtualized
    {
        public I3DModelService Model3dService { get; set; }

        public ElementiItemsViewVirtualized(MasterDetailGridView ElementiView) : base(ElementiView)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Elementi];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        //// This method helps to get dependency property value in another thread.
        //// Usage: Invoke(() => { return CreationOverhead; });
        //private T Invoke<T>(Func<T> callback)
        //{
        //    //return (T)Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
        //    return (T)Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Func<object>(() => { return callback(); }));
        //}

        protected override EntityView NewItemView(Entity entity)
        {
            return new ElementiItemView(this, entity);
        }

        //public override IList<EntityView> LoadRange(int startIndex, int count, SortDescriptionCollection sortDescriptions, out int overallCount)
        //{
        //    IsModelToViewLoading = true;


        //    int minIndex = Math.Min(startIndex, FilteredEntitiesId.Count - 1);
        //    int maxIndex = (int)Math.Min(startIndex + count, FilteredEntitiesId.Count) - 1;

        //    if (minIndex < 0 || maxIndex < 0 || DataService.Suspended)
        //    {
        //        overallCount = 0;
        //        IsModelToViewLoading = false;
        //        return new List<EntityView>();
        //    }

        //    List<Guid> ids = FilteredEntitiesId.GetRange(minIndex, maxIndex - minIndex + 1);

        //    IEnumerable<Entity> listEnts = DataService.GetEntitiesById(BuiltInCodes.EntityType.Elementi, ids);

        //    overallCount = Invoke(() => { return FilteredEntitiesId.Count; });

        //    // because the all fields are sorted ascending, the PropertyName is ignored in this sample
        //    // only Direction is considered.
        //    //SortDescription sortDescription = sortDescriptions == null || sortDescriptions.Count == 0 ? new SortDescription() : sortDescriptions[0];
        //    //ListSortDirection direction = string.IsNullOrEmpty(sortDescription.PropertyName) ? ListSortDirection.Ascending : sortDescription.Direction;


        //    //_entities = new List<TreeEntityView>();
        //    List<EntityView> ents = new List<EntityView>();
        //    foreach (Entity ent in listEnts)
        //    {
        //        EntityView newItem = new ElementiItemView(this, ent);

        //        if (CheckedEntitiesId.Contains(ent.Id))
        //            newItem.SetChecked(true);

        //        ents.Add(newItem);
        //    }

        //    IsModelToViewLoading = false;
        //    return ents;

        //}

        public override void UpdateUI()
        {
            base.UpdateUI();
            RaisePropertyChanged(GetPropertyName(() => IsSelectInModel3dEnabled));
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
    }
}
