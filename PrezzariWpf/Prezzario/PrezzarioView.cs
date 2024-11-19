


using MasterDetailView;
using Commons;
using MasterDetailModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.IO;
using System;
using CommonResources;
using Model;
using System.Linq;

namespace PrezzariWpf.View
{

    public class PrezzarioItemView : TreeEntityView
    {

        public PrezzarioItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }

        //internal DivisioneItem GetDivisioneItem(string codiceAttributoGuid, string entityTypeKey)
        //{
        //    PrezzarioItem prezItem = Entity as PrezzarioItem;
        //    Guid divItemId = prezItem.GetDivisioneItemId(codiceAttributoGuid);

        //    if (divItemId != Guid.Empty)
        //    {
        //        IEnumerable<Entity> ents;
        //        ents = _master.DataService.GetTreeEntitiesDeepById(entityTypeKey, new List<Guid>() { divItemId });
        //        return ents.LastOrDefault() as DivisioneItem;
        //    }
        //    return null;
        //}


    }

    public class PrezzarioView : MasterDetailTreeView, SectionItemTemplateView
    {
        //public override MasterDetailTreeViewVirtualized ItemsView { get; set; }
        //public new MasterDetailTreeViewVirtualized ItemsView { get => _itemsView as MasterDetailTreeViewVirtualized; }

        public PrezzarioView()
        {
            _itemsView = new PrezzarioItemsViewVirtualized(this);
        }

        public int Code => (int) ElencoPrezziSectionItemsId.Prezzario;

        public ICommand ImportPrezzarioMosaicoXMLCommand
        {
            get
            {
                return new CommandHandler(() => this.ImportPrezzarioMosaicoXML());
            }
        }

        private void ImportPrezzarioMosaicoXML()
        {
            ImportPrezzarioXML importXML = new ImportPrezzarioXML();
            importXML.Run(DataService);

            MainOperation.UpdateEntityTypesView(new List<string> { BuiltInCodes.EntityType.Capitoli, BuiltInCodes.EntityType.Prezzario });
        }
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
    public class PrezzarioItemsViewVirtualized : MasterDetailTreeViewVirtualized
    {

        public PrezzarioItemsViewVirtualized(MasterDetailTreeView owner) : base(owner)
        {
        }

        protected override TreeEntityView NewItemView(TreeEntity entity)
        {
            return new PrezzarioItemView(this, entity);
        }

        public override void Init()
        {
            base.Init();

            _loadingEntities = new List<TreeEntity>();

            //EntityParentType = DataService.GetEntityTypes()["PrezzarioItemParent"];
            EntityParentType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.PrezzarioParent];
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Prezzario];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        public ICommand ImportItemsPrezzarioCommand { get => new CommandHandler(() => this.ImportItemsPrezzario()); }
        void ImportItemsPrezzario()
        {

            if (MainOperation.IsProjectClosing())
                return;

            string title = string.Format("{0} {1}", LocalizationProvider.GetString("SelezionaVociDa"), LocalizationProvider.GetString("Prezzario"));

            EntityTypeViewSettings viewSettings = null;
            List<Guid> selectedItems = new List<Guid>();
            string externalPrezzarioFileName = string.Empty;
            if (WindowService.SelectPrezzarioIdsWindow(ref selectedItems, ref externalPrezzarioFileName, title, SelectIdsWindowOptions.Nothing, false, true, true,ref viewSettings))
            {

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


            }
        }

        public ICommand UpdateItemsPrezzarioCommand { get => new CommandHandler(() => this.UpdateItemsPrezzario()); }
        async void UpdateItemsPrezzario()
        {

            if (MainOperation.IsProjectClosing())
                return;

            

            string title = string.Format("{0}", LocalizationProvider.GetString("Seleziona prezzario"));

            EntityComparer prezzarioComparer = new PrezzarioCodiceItemKeyComparer();
            EntityComparer capitoliComparer = new CapitoliCodiceItemKeyComparer();


            //ricavo i codici degli articoli selezionati nella sorgente

            List<string> codici = await DataService.GetValoriUnivociAsync(BuiltInCodes.EntityType.Prezzario, CheckedEntitiesId.ToList(), BuiltInCodes.Attributo.Codice, 0, null);

            if (!codici.Any())
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Selezionare almeno una voce"));
                return;
            }

            if (codici.Count < CheckedEntitiesId.Count)
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Selezionare voci con codice univoco"));
                return;
            }



            EntityTypeViewSettings viewSettings = null;
            List<Guid> selectedItems = new List<Guid>();
            string externalPrezzarioFileName = string.Empty;
            if (WindowService.SelectPrezzarioWindow(ref externalPrezzarioFileName, title))
            {
                WindowService.ShowWaitCursor(true);

                IEnumerable<Guid> prezzarioItems = null;

                Dictionary<string, IDataService> prezzariCache = MainOperation.GetPrezzariCache();
                if (prezzariCache.ContainsKey(externalPrezzarioFileName))
                {
                    //Importazione nel prezzario interno degli articoli 

                    EntitiesImportStatus importStatus = new EntitiesImportStatus();
                    importStatus.TargetPosition = TargetPosition.Bottom;
                    importStatus.ConflictAction = EntityImportConflictAction.Overwrite;
                    importStatus.Source = prezzariCache[externalPrezzarioFileName];
                    importStatus.SourceName = externalPrezzarioFileName;

                    //provare e si inchioda
                    importStatus.CustomEntityComparers.Add(BuiltInCodes.EntityType.Prezzario, prezzarioComparer);
                    importStatus.CustomEntityComparers.Add(BuiltInCodes.EntityType.Capitoli, capitoliComparer);

                    //compongo il filtro
                    FilterData filter = new FilterData();
                    filter.Items.Add(new AttributoFilterData()
                    {
                        EntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                        
                        CodiceAttributo = BuiltInCodes.Attributo.Codice,
                        CheckedValori = new HashSet<string>(codici),
                        IsFiltroAttivato = true,
                    });

                    //filtro gli articoli nel source
                    List<Guid> entitiesFound = new List<Guid>();
                    importStatus.Source.GetFilteredTreeEntities(BuiltInCodes.EntityType.Prezzario, filter, null, out entitiesFound);

                    entitiesFound.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = PrezzarioItemType.CreateKey() }));
                  
                    //imposto le voci di target che si possono sovrascrivere
                    List<Entity> checkedEnts = DataService.GetEntitiesById(BuiltInCodes.EntityType.Prezzario, CheckedEntitiesId);
                    Dictionary<string, Guid> checkedIdByCodice = checkedEnts.ToDictionary(ent => ent.GetComparerKey(prezzarioComparer), ent => ent.EntityId);
                    importStatus.CustomTargetGuidsByKey = checkedIdByCodice;
                    importStatus.CustomTargetEntityTypeKey = BuiltInCodes.EntityType.Prezzario;

                    while (importStatus.Status != EntityImportStatusEnum.Completed)
                    {
                        DataService.ImportEntities(importStatus);

                        //in teoria questo dialogo non dovrebbe mai essere chiamato
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


            }

            WindowService.ShowWaitCursor(false);


        }

        public ICommand CheckComputoReferencedCommand { get { return new CommandHandler(() => this.CheckComputoReferenced()); } }
        protected virtual void CheckComputoReferenced()
        {
            HashSet<Guid> refIds = new HashSet<Guid>();
            foreach (Guid id in FilteredEntitiesId)
            {
                List<Guid> depIds = DataService.GetDependentIds(EntityType.GetKey(), id, BuiltInCodes.EntityType.Computo);
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

            ClearFocus();

            ShowEntities(refIds.ToList());
            CheckedEntitiesId = new HashSet<Guid>(refIds);

            if (refIds.Any())
                SelectEntityById(refIds.First());
            else
                SelectEntityById(Guid.Empty);
        }

        public bool IsCheckItemsEnabled
        {
            get
            {
                if (IsRestrictedCommandsMode())
                    return false;

                return !IsMoveEntitiesAfterEnabled;
            }
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            RaisePropertyChanged(GetPropertyName(() => this.IsCheckItemsEnabled));
        }

        public override void Load()
        {
            base.Load();
        }

    }






} 
