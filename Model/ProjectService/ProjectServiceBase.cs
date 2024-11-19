using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Xpf.Core;
using DevExpress.XtraPrinting.Native.LayoutAdjustment;
using MasterDetailModel;
using Microsoft.Isam.Esent.Interop;
using Model.Calculators;
using ProtoBuf;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Model
{
    public class ProjectServiceBase
    {
        protected Project Project { get; set; } = null;
        protected EntitiesHelper EntitiesHelper { get; set; } = null;

        protected Factory ServerFactory;

        /// <summary>
        /// Cache per ogni entityType delle entità per guid
        /// key: entityTypeKey
        /// </summary>
        protected Dictionary<string, Dictionary<Guid, Entity>> EntitiesByGuid = new Dictionary<string, Dictionary<Guid, Entity>>();

        /// <summary>
        /// Cache per ogni entityType dei guid per chiave (esempio di chiave: Codice per l'elenco prezzi)
        /// </summary>
        protected Dictionary<string, Dictionary<string, Guid>> EntitiesGuidByKey = new Dictionary<string, Dictionary<string, Guid>>();

        internal ValoreCalculator Calculator { get; set; } = null;

        //funzioni del calcolatore
        internal EPCalculatorFunction _epCalculatorFunction = null;
        internal CmpCalculatorFunction _cmpCalculatorFunction = null;
        internal ValoreM3dCalculatorFunction _ifcCalculatorFunction = null;
        internal ValoreM3dCalculatorFunction _rvtCalculatorFunction = null;
        internal ElmCalculatorFunction _elmCalculatorFunction = null;
        internal CntCalculatorFunction _cntCalculatorFunction = null;
        internal InfCalculatorFunction _infCalculatorFunction = null;
        internal DivCalculatorFunction _divCalculatorFunction = null;
        internal EAtCalculatorFunction _eatCalculatorFunction = null;
        internal WBSCalculatorFunction _wbsCalculatorFunction = null;
        internal VarCalculatorFunction _varCalculatorFunction = null;
        internal CapCalculatorFunction _capCalculatorFunction = null;

        protected string _projectSerialized = string.Empty;

        protected List<EntitiesError> EntitiesErrors = new List<EntitiesError>();




        protected void InitProject(Project project)
        {
            if (project == null)
                return;

            Project = project;

            if (!Project.DefinizioniAttributo.Any())
            {
                //progetto nuovo
                CreaDefinizioniAttributo();
                CreaEntityTypes();
                InitProjectViewSettings();

                CreaVariabiliItem();
                CreaAllegatiItems();
                CreaDivisioniItems();
                CreaContattiItems();
                CreaCalendariItems();
                CreaInfoProgettoItem();
                CreaElementiItems();
                CreaPrezzarioItems();
                CreaComputoItems();
                CreaDocumentiItems();
                CreaReportItems();
                CreaStiliItems();
                CreaElencoAttivitaItems();
            }
            else //progetto aperto
            {
                CreaDefinizioniAttributo();
                CreaEntityTypes();
                InitProjectViewSettings();

                CreaVariabiliItem();
                CreaAllegatiItems();
                CreaDivisioniItems();
                CreaContattiItems();
                CreaCalendariItems();
                CreaElementiItems();
                CreaPrezzarioItems();
                CreaComputoItems();
                CreaInfoProgettoItem();
                CreaDocumentiItems();
                CreaReportItems();
                CreaStiliItems();
                CreaElencoAttivitaItems();
                

                //Project.ResolveReferences();
                ResolveProjectReferences();
            }

        }

        /// <summary>
        /// da spostare
        /// </summary>
        public void ResolveProjectReferences()
        {
            foreach (EntityType entType in EntityTypes.Values)
                entType.ResolveReferences(EntityTypes, Project.DefinizioniAttributo);



            //resolve TagItems
            TagItemCollection tagEntityCollection = new TagItemCollection();
            tagEntityCollection.Entities = Project.TagItems;
            tagEntityCollection.ResolveAllReferences(EntityTypes);


            //resolve AllegatiItems
            AllegatiItemCollection allEntityCollection = new AllegatiItemCollection();
            allEntityCollection.Entities = Project.AllegatiItems;
            allEntityCollection.ResolveAllReferences(EntityTypes);


            //Resolve divisioni
            foreach (List<TreeEntity> entities in Project.DivisioniItems.Values)
            {
                if (entities != null)
                {
                    DivisioneItemCollection divItemsCollection = new DivisioneItemCollection();
                    //divItemsCollection.TreeEntities = entities.Cast<TreeEntity>().ToList();//rem 19/01/2023
                    divItemsCollection.TreeEntities = entities;
                    divItemsCollection.ResolveAllReferences(EntityTypes);
                }
            }

            //resolve ContattiItems
            ContattiItemCollection cntEntityCollection = new ContattiItemCollection();
            cntEntityCollection.Entities = Project.ContattiItems;
            cntEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve CapitoliItems
            CapitoliItemCollection capEntityCollection = new CapitoliItemCollection();
            //capEntityCollection.TreeEntities = Project.CapitoliItems.Cast<TreeEntity>().ToList();//rem 19/01/2023
            capEntityCollection.TreeEntities = Project.CapitoliItems;
            capEntityCollection.ResolveAllReferences(EntityTypes);

            //resolveElementiItems
            EntityCollection elmEntityCollection = new EntityCollection();
            elmEntityCollection.Entities = Project.ElementiItems;
            elmEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve PrezzarioItems
            PrezzarioItemCollection prezEntityCollection = new PrezzarioItemCollection();
            //prezEntityCollection.TreeEntities = Project.PrezzarioItems.Cast<TreeEntity>().ToList();//rem 19/01/2023
            prezEntityCollection.TreeEntities = Project.PrezzarioItems;
            prezEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve ComputoItems
            EntityCollection cmpEntityCollection = new EntityCollection();
            cmpEntityCollection.Entities = Project.ComputoItems;
            cmpEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve DocumentiItems
            DocumentiItemCollection dcEntityCollection = new DocumentiItemCollection();
            //dcEntityCollection.TreeEntities = Project.DocumentiItems.Cast<TreeEntity>().ToList();//rem 19/01/2023
            dcEntityCollection.TreeEntities = Project.DocumentiItems/*.Cast<TreeEntity>().ToList()*/;
            dcEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve ReportItems
            EntityCollection rptEntityCollection = new EntityCollection();
            rptEntityCollection.Entities = Project.ReportItems;
            rptEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve StiliItems
            EntityCollection StiliEntityCollection = new EntityCollection();
            StiliEntityCollection.Entities = Project.StiliItems;
            StiliEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve ElencoAttivitaItems
            EntityCollection ElencoAttivitaEntityCollection = new EntityCollection();
            ElencoAttivitaEntityCollection.Entities = Project.ElencoAttivitaItems;
            ElencoAttivitaEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve WBSItems
            WBSItemCollection wbsEntityCollection = new WBSItemCollection();
            //wbsEntityCollection.TreeEntities = Project.WBSItems.Cast<TreeEntity>().ToList();//rem 19/01/2023
            wbsEntityCollection.TreeEntities = Project.WBSItems/*.Cast<TreeEntity>().ToList()*/;
            wbsEntityCollection.ResolveAllReferences(EntityTypes);

            //resolve CalendariItems
            CalendariItemCollection calendariEntityCollection = new CalendariItemCollection();
            calendariEntityCollection.Entities = Project.CalendariItems;
            calendariEntityCollection.ResolveAllReferences(EntityTypes);

        }


        protected void CreateEntsIndexedByGuid()
        {
            foreach (EntityType entType in EntityTypes.Values)
            {
                EntitiesByGuid.Add(entType.GetKey(), null);
                UpdateEntitiesIndexes(entType.GetKey());
            }
        }

        private void UpdateEntsIndexedByGuid(string entityTypeKey)
        {
            var entityTypes = GetEntityTypes();

            if (entityTypes[entityTypeKey].IsParentType())
                return;


            if (!EntitiesByGuid.ContainsKey(entityTypeKey))
                EntitiesByGuid.Add(entityTypeKey, new Dictionary<Guid, Entity>());


            if (entityTypes[entityTypeKey].IsTreeMaster)
            {
                List<TreeEntity> treeEnts = GetTreeEntitiesList(entityTypeKey);
                ///
                //EntitiesByGuid[entityTypeKey] = treeEnts.ToDictionary(
                //    item => item.Id,
                //    item =>
                //    {
                //        item.IsParent = IsTreeEntityParent(item);//operazione lenta (indexOf)
                //        return (Entity)item;
                //    });

                //by Ale 18/01/2021
                EntitiesByGuid[entityTypeKey].Clear();
                for (int i = 0; i < treeEnts.Count; i++)
                {
                    bool isParent = false;
                    TreeEntity item = treeEnts[i];
                    if (i < treeEnts.Count - 1)
                    {
                        if (treeEnts[i + 1].Depth > item.Depth)
                            isParent = true;
                    }
                    item.IsParent = isParent;
                    EntitiesByGuid[entityTypeKey].Add(item.EntityId, item);

                }
            }
            else
            {
                //EntitiesByGuid[entityTypeKey] = GetEntities(entityTypeKey).ToDictionary(item => item.EntityId, item => item);

                EntitiesByGuid[entityTypeKey].Clear();

                List<Entity> ents = GetEntitiesList(entityTypeKey);
                for (int i = 0; i < ents.Count; i++)
                {
                    Entity item = ents[i];
                    EntitiesByGuid[entityTypeKey].Add(item.EntityId, item);
                }

            }

        }



        public bool AddEntity(string entityTypeKey, Entity ent, Guid parentId)
        {
            if (!EntitiesByGuid.ContainsKey(entityTypeKey))
                return false;

            EntityType entType = null;
            GetEntityTypes().TryGetValue(entityTypeKey, out entType);
            if (entType == null)
                return false;

            if (entType.IsTreeMaster)
            {
                List<TreeEntity> treeEnts = GetTreeEntitiesList(entityTypeKey);
                TreeEntity treeEnt = ent as TreeEntity;


                if (parentId != null && parentId != Guid.Empty)
                {
                    if (treeEnt == null)
                        return false;

                    TreeEntity parent = treeEnts.FirstOrDefault(item => item.EntityId == parentId) as TreeEntity;

                    treeEnt.Parent = parent;

                    int lastChildIndex = -1;
                    int count = DescendantsCountOf(entityTypeKey, parent, out lastChildIndex);

                    int insertIndex = lastChildIndex + 1;

                    if (insertIndex < treeEnts.Count)
                        treeEnts.Insert(insertIndex, treeEnt);
                    else
                        treeEnts.Add(treeEnt);

                    parent.IsParent = true;
                    parent.RimuoviAttributiNonParent();
                }
                else
                {
                    treeEnts.Add(treeEnt);
                }
            }
            else
            {
                List<Entity> ents = GetEntitiesList(entityTypeKey);
                ents.Add(ent);
            }


            if (EntitiesByGuid.ContainsKey(entityTypeKey))
            {
                if (!EntitiesByGuid[entityTypeKey].ContainsKey(ent.EntityId))
                    EntitiesByGuid[entityTypeKey].Add(ent.EntityId, ent);
            }

            if (EntitiesGuidByKey.ContainsKey(entityTypeKey))
            {
                string key = ent.GetComparerKey();
                if (!EntitiesGuidByKey[entityTypeKey].ContainsKey(key))
                    EntitiesGuidByKey[entityTypeKey].Add(key, ent.EntityId);
            }

            return true;
        }


        protected void CreateEntitiesIndexes()
        {
            //CreateTreeEntities();
            CreateEntsIndexedByGuid();
        }

        internal void UpdateEntitiesIndexes()
        {
            foreach (EntityType entType in EntityTypes.Values)
            {
                UpdateEntitiesIndexes(entType.GetKey());
            }
        }

        //internal void UpdateEntitiesIndexes(string entityTypeKey)
        //{
        //    //UpdateTreeEntities(entityTypeKey);
        //    UpdateEntsIndexedByGuid(entityTypeKey);
        //    UpdateEntsGuidIndexedByKey(entityTypeKey);
        //}


        internal void UpdateEntitiesIndexes(string entityTypeKey, Entity entAdd, Entity entRemove)
        {
            if (entAdd != null)
            {
                if (EntitiesByGuid.ContainsKey(entityTypeKey))
                {
                    if (!EntitiesByGuid[entityTypeKey].ContainsKey(entAdd.EntityId))
                        EntitiesByGuid[entityTypeKey].Add(entAdd.EntityId, entAdd);
                }

                if (EntitiesGuidByKey.ContainsKey(entityTypeKey))
                {
                    string key = entAdd.GetComparerKey();
                    if (!EntitiesGuidByKey[entityTypeKey].ContainsKey(key))
                        EntitiesGuidByKey[entityTypeKey].Add(key, entAdd.EntityId);
                }

            }

            if (entRemove != null)
            {
                if (EntitiesByGuid.ContainsKey(entityTypeKey))
                {
                    if (EntitiesByGuid[entityTypeKey].ContainsKey(entRemove.EntityId))
                        EntitiesByGuid[entityTypeKey].Remove(entRemove.EntityId);
                }

                if (EntitiesGuidByKey.ContainsKey(entityTypeKey))
                {
                    string key = entRemove.GetComparerKey();
                    if (!EntitiesGuidByKey[entityTypeKey].ContainsKey(key))
                        EntitiesGuidByKey[entityTypeKey].Remove(key);
                }

            }
        }

        internal void UpdateEntitiesIndexes(string entityTypeKey, List<Entity> entsAdd = null, List<Entity> entsRemove = null)
        {
            if (entsAdd == null && entsRemove == null)
            {
                UpdateEntsIndexedByGuid(entityTypeKey);
                UpdateEntsGuidIndexedByKey(entityTypeKey);
            }
            else
            {
                //Add
                if (entsAdd != null)
                {
                    foreach (Entity ent in entsAdd)
                    {
                        UpdateEntitiesIndexes(entityTypeKey, ent, null);
                    }
                }

                //Remove
                if (entsRemove != null)
                {
                    foreach (Entity ent in entsRemove)
                    {
                        UpdateEntitiesIndexes(entityTypeKey, null, ent);
                    }
                }
            }
        }

        internal void UpdateEntitiesIndexes(string entityTypeKey, List<TreeEntity> entsAdd, List<TreeEntity> entsRemove)
        {
            if (entsAdd == null && entsRemove == null)
            {
                UpdateEntsIndexedByGuid(entityTypeKey);
                UpdateEntsGuidIndexedByKey(entityTypeKey);
            }
            else
            {
                //Add
                if (entsAdd != null)
                {
                    foreach (Entity ent in entsAdd)
                    {
                        UpdateEntitiesIndexes(entityTypeKey, ent, null);
                    }
                }

                //Remove
                if (entsRemove != null)
                {
                    foreach (Entity ent in entsRemove)
                    {
                        UpdateEntitiesIndexes(entityTypeKey, null, ent);
                    }
                }
            }
        }


        private void CreaDivisioniItems()
        {
            //La serializzazione di protobuf non salva la lista di entità se è vuota

            foreach (DivisioneItemType div in EntityTypes.Where(item => item.Value is DivisioneItemType).Select(item => item.Value))
            {
                if (!Project.DivisioniItems.ContainsKey(div.DivisioneId))
                    Project.DivisioniItems.Add(div.DivisioneId, new List<TreeEntity>());

                if (Project.DivisioniItems[div.DivisioneId] == null)
                    Project.DivisioniItems[div.DivisioneId] = new List<TreeEntity>();
            }


            //List<Guid> keys = Project.DivisioniItems.Keys.ToList();

            //foreach (Guid key in keys)
            //{
            //    if (Project.DivisioniItems[key] == null)
            //        Project.DivisioniItems[key] = new List<TreeEntity>();
            //}
        }

        private void CreaElementiItems()
        {
            if (Project.ElementiItems == null)
                Project.ElementiItems = new List<Entity>();
        }

        private void CreaContattiItems()
        {
            string key = ContattiItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                ContattiItemType contattiItemType = new ContattiItemType();
                contattiItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(contattiItemType);
            }


            if (Project.ContattiItems == null)
                Project.ContattiItems = new List<Entity>();

            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        private void CreaElencoAttivitaItems()
        {
            string key = ElencoAttivitaItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                ElencoAttivitaItemType elencoAttivitaItemType = new ElencoAttivitaItemType();
                elencoAttivitaItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(elencoAttivitaItemType);
            }


            if (Project.ElencoAttivitaItems == null)
                Project.ElencoAttivitaItems = new List<Entity>();

            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        private void CreaCalendariItems()
        {
            string key = CalendariItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                CalendariItemType calendariItemType = new CalendariItemType();
                calendariItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(calendariItemType);
            }

            if (Project.CalendariItems == null)
                Project.CalendariItems = new List<Entity>();


            if (!Project.CalendariItems.Any())
            {
                
                //str = str + Giorno + " " + Item.Hours + "\n";

                CalendariItem calendarioStd = ServerFactory.NewCalendariItem();
                calendarioStd.CreaAttributi();

                WeekHours weekHours = calendarioStd.GetWeekHours();
                string weekHoursText = weekHours.ToUserText();

                calendarioStd.Attributi[BuiltInCodes.Attributo.Codice].Valore = new ValoreTesto() { V = LocalizationProvider.GetString("Calendario standard") };
                string json = null;
                if (JsonSerializer.JsonSerialize(calendarioStd.GetWeekHours(), out json))
                {
                    calendarioStd.Attributi[BuiltInCodes.Attributo.WeekHours].Valore = new ValoreTesto() { V = json};
                    calendarioStd.Attributi[BuiltInCodes.Attributo.WeekHoursText].Valore = new ValoreTesto() { V = weekHoursText };
                    Project.CalendariItems.Add(calendarioStd);
                }
            }



            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        private void CreaInfoProgettoItem()
        {
            string key = InfoProgettoItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                InfoProgettoItemType infoProjItemType = new InfoProgettoItemType();
                infoProjItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(infoProjItemType);
            }
            else
            {
                InfoProgettoItemType infoProjItemType = Project.EntityTypes[key] as InfoProgettoItemType;
                infoProjItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
            }

            if (Project.InfoProgettoItem == null)
            {
                Project.InfoProgettoItem = ServerFactory.NewInfoProgettoItem();
                Project.InfoProgettoItem.CreaAttributi();
            }
            else
            {
                Project.InfoProgettoItem.ResolveReferences(GetEntityTypes());
                Project.InfoProgettoItem.CreaAttributi();
            }

            //Setto da codice il calendario standard
            string codiceAttributoCalendarioId = string.Join(InfoProgettoItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            Guid calendarioId = Project.InfoProgettoItem.GetAttributoGuidId(codiceAttributoCalendarioId);
            if (calendarioId == Guid.Empty)
            {
                Entity calendarioStd = Project.CalendariItems.FirstOrDefault();
                if (calendarioStd != null)
                    Project.InfoProgettoItem.Attributi[codiceAttributoCalendarioId].Valore = new ValoreGuid() { V = calendarioStd.EntityId };
            }

            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        private void CreaPrezzarioItems()
        {
            if (Project.PrezzarioItems == null)
                Project.PrezzarioItems = new List<TreeEntity>();
        }

        private void CreaComputoItems()
        {
            if (Project.ComputoItems == null)
                Project.ComputoItems = new List<Entity>();
            //List<Entity> computoItems = new List<Entity>();
            //Project.entitiesDictionary.Add(BuiltInCodes.EntityType.Computo, computoItems);
            //Project.deletedEntitiesDictionary.Add(BuiltInCodes.EntityType.Computo, new List<Entity>());
        }

        private void CreaDocumentiItems()
        {
            if (Project.DocumentiItems == null)
                Project.DocumentiItems = new List<TreeEntity>();
            //List<Entity> computoItems = new List<Entity>();
            //Project.entitiesDictionary.Add(BuiltInCodes.EntityType.Computo, computoItems);
            //Project.deletedEntitiesDictionary.Add(BuiltInCodes.EntityType.Computo, new List<Entity>());
        }
        private void CreaReportItems()
        {
            if (Project.ReportItems == null)
                Project.ReportItems = new List<Entity>();
            //List<Entity> computoItems = new List<Entity>();
            //Project.entitiesDictionary.Add(BuiltInCodes.EntityType.Computo, computoItems);
            //Project.deletedEntitiesDictionary.Add(BuiltInCodes.EntityType.Computo, new List<Entity>());
        }

        private void CreaStiliItems()
        {
            if (Project.StiliItems == null)
                Project.StiliItems = new List<Entity>();
        }

        private void CreaVariabiliItem()
        {
            string key = VariabiliItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                VariabiliItemType varItemType = new VariabiliItemType();
                varItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(varItemType);
            }
            else
            {
                VariabiliItemType varItemType = Project.EntityTypes[key] as VariabiliItemType;
                varItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
            }

            if (Project.VariabiliItem == null)
            {
                Project.VariabiliItem = ServerFactory.NewVariabiliItem();
                Project.VariabiliItem.CreaAttributi();
            }
            else
            {
                Project.VariabiliItem.ResolveReferences(GetEntityTypes());
                Project.VariabiliItem.CreaAttributi();
            }

            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        private void CreaAllegatiItems()
        {
            string key = AllegatiItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                AllegatiItemType allItemType = new AllegatiItemType();
                allItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(allItemType);
            }


            if (Project.AllegatiItems == null)
                Project.AllegatiItems = new List<Entity>();

            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        private void CreaTagItems()
        {
            string key = TagItemType.CreateKey();

            if (!Project.EntityTypes.ContainsKey(key))
            {
                TagItemType tagItemType = new TagItemType();
                tagItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(tagItemType);
            }


            if (Project.TagItems == null)
                Project.TagItems = new List<Entity>();

            if (!Project.ViewSettings.EntityTypes.ContainsKey(key))
                Project.ViewSettings.EntityTypes.Add(key, new EntityTypeViewSettings());

        }

        internal IEnumerable<Entity> GetEntities(string entityTypeKey)
        {
            if (string.IsNullOrEmpty(entityTypeKey))
                return null;

            EntityType entType = Project.EntityTypes[entityTypeKey];

            if (entType is DivisioneItemType)
            {
                DivisioneItemType divItemType = entType as DivisioneItemType;
                if (Project.DivisioniItems.ContainsKey(divItemType.DivisioneId))
                    return Project.DivisioniItems[divItemType.DivisioneId].Cast<Entity>();
                else
                    return new List<TreeEntity>();

                //if (!Project.DivisioniItems.ContainsKey(divItemType.DivisioneId))
                //    Project.DivisioniItems.Add(divItemType.DivisioneId, new List<TreeEntity>());

                //return Project.DivisioniItems[divItemType.DivisioneId].Cast<Entity>();

            }
            else if (entType is DivisioneItemParentType)
            {
                DivisioneItemParentType divItemType = entType as DivisioneItemParentType;
                return Project.DivisioniItems[divItemType.DivisioneId].Cast<Entity>();
            }
            else if (entType is PrezzarioItemType || entType is PrezzarioItemParentType)
            {
                return Project.PrezzarioItems.Cast<Entity>();
            }
            else if (entType is CapitoliItemType || entType is CapitoliItemParentType)
            {
                return Project.CapitoliItems.Cast<Entity>();
            }
            else if (entType is ComputoItemType)
            {
                return Project.ComputoItems;
            }
            else if (entType is ElementiItemType)
            {
                return Project.ElementiItems;
            }
            else if (entType is ContattiItemType)
            {
                return Project.ContattiItems;
            }
            else if (entType is InfoProgettoItemType)
            {
                List<Entity> ents = new List<Entity>();
                ents.Add(Project.InfoProgettoItem);
                return ents;
            }
            else if (entType is DocumentiItemType || entType is DocumentiItemParentType)
            {
                return Project.DocumentiItems.Cast<Entity>();
            }
            else if (entType is ReportItemType)
            {
                return Project.ReportItems;
            }
            else if (entType is StiliItemType)
            {
                return Project.StiliItems;
            }
            else if (entType is ElencoAttivitaItemType)
            {
                return Project.ElencoAttivitaItems;
            }
            else if (entType is WBSItemType || entType is WBSItemParentType)
            {
                return Project.WBSItems.Cast<Entity>();
            }
            else if (entType is CalendariItemType)
            {
                return Project.CalendariItems;
            }
            else if (entType is VariabiliItemType)
            {
                List<Entity> ents = new List<Entity>();
                ents.Add(Project.VariabiliItem);
                return ents;
            }
            else if (entType is AllegatiItemType)
            {
                return Project.AllegatiItems;
            }
            else if (entType is TagItemType)
            {
                return Project.TagItems;
            }

            //if (codiceEntityType == BuiltInCodes.EntityType.Prezzario || codiceEntityType == "PrezzarioItemParent")
            //    return Project.PrezzarioItems;
            //else if (codiceEntityType == BuiltInCodes.EntityType.Computo)
            //    return Project.ComputoItems;

            return null;
        }

        //protected List<TreeEntity> GetTreeEntities(string entityTypeKey)
        //{
        //    if (TreeEntities.ContainsKey(entityTypeKey))
        //        return TreeEntities[entityTypeKey];

        //    return null;
        //}

        internal List<TreeEntity> GetTreeEntitiesList(string entityTypeKey)
        {
            if (!Project.EntityTypes.ContainsKey(entityTypeKey))
                return null;

            EntityType entType = Project.EntityTypes[entityTypeKey];

            if (entType is DivisioneItemType)
            {
                DivisioneItemType divItemType = entType as DivisioneItemType;

                if (Project.DivisioniItems.ContainsKey(divItemType.DivisioneId))
                    return Project.DivisioniItems[divItemType.DivisioneId]/*.Cast<TreeEntity>().ToList()*/;
                else
                    return new List<TreeEntity>();

            }
            else if (entType is PrezzarioItemType || entType is PrezzarioItemParentType)
            {
                return Project.PrezzarioItems/*.Cast<TreeEntity>().ToList()*/;
            }
            else if (entType is CapitoliItemType || entType is CapitoliItemParentType)
            {
                return Project.CapitoliItems/*.Cast<TreeEntity>().ToList()*/;
            }
            else if (entType is DocumentiItemType || entType is DocumentiItemParentType)
            {
                return Project.DocumentiItems/*.Cast<TreeEntity>().ToList()*/;
            }
            else if (entType is WBSItemType || entType is WBSItemParentType)
            {
                return Project.WBSItems/*.Cast<TreeEntity>().ToList()*/;
            }

            return null;
        }

        internal List<Entity> GetEntitiesList(string entityTypeKey)
        {
            if (string.IsNullOrEmpty(entityTypeKey))
                return null;

            EntityType entType = Project.EntityTypes[entityTypeKey];



            if (entType is ComputoItemType)
            {
                return Project.ComputoItems;
            }
            else if (entType is ElementiItemType)
            {
                return Project.ElementiItems;
            }
            else if (entType is ContattiItemType)
            {
                return Project.ContattiItems;
            }
            else if (entType is InfoProgettoItemType)
            {
                List<Entity> ents = new List<Entity>();
                ents.Add(Project.InfoProgettoItem);
                return ents;
            }
            else if (entType is ReportItemType)
            {
                return Project.ReportItems;
            }
            else if (entType is StiliItemType)
            {
                return Project.StiliItems;
            }
            else if (entType is ElencoAttivitaItemType)
            {
                return Project.ElencoAttivitaItems;
            }
            else if (entType is CalendariItemType)
            {
                return Project.CalendariItems;
            }
            else if (entType is VariabiliItemType)
            {
                List<Entity> ents = new List<Entity>();
                ents.Add(Project.VariabiliItem);
                return ents;
            }
            else if (entType is AllegatiItemType)
            {
                return Project.AllegatiItems;
            }
            else if (entType is TagItemType)
            {
                return Project.TagItems;
            }

            return null;

        }

        //protected List<TreeEntity> CacheTreeEntities(string entityTypeKey)
        //{
        //    EntityType entType = Project.EntityTypes[entityTypeKey];

        //    if (entType is DivisioneItemType)
        //    {
        //        DivisioneItemType divItemType = entType as DivisioneItemType;
        //        return Project.DivisioniItems[divItemType.Id].Cast<TreeEntity>().ToList();

        //    }
        //    else if (entType is PrezzarioItemType || entType is PrezzarioItemParentType)
        //    {
        //        return Project.PrezzarioItems.Cast<TreeEntity>().ToList();
        //    }
        //    else if (entType is CapitoliItemType || entType is CapitoliItemParentType)
        //    {
        //        return Project.CapitoliItems.Cast<TreeEntity>().ToList();
        //    }

        //    return null;
        //}

        private void CreaDefinizioniAttributo()
        {
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Testo, new ValoreTesto(), true, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.TestoRTF, new ValoreTestoRtf(), true, true, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Reale, new ValoreReale(), true, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Contabilita, new ValoreContabilita(), true, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Riferimento, new ValoreTesto(), true, true, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Guid, new ValoreGuid(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Data, new ValoreData(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.TestoCollection, new ValoreTestoCollection(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.GuidCollection, new ValoreGuidCollection(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Elenco, new ValoreElenco(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Colore, new ValoreColore(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Booleano, new ValoreBooleano(), false, false, true);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.FormatoNumero, new ValoreFormatoNumero(), false, false, false);
            AddDefinizioneAttributo(BuiltInCodes.DefinizioneAttributo.Variabile, new ValoreTesto(), false, false, true);


        }

        private void AddDefinizioneAttributo(string cod, Valore valoreDefault, bool isExpandable, bool isPreviewable, bool allowAttributoCustom)
        {

            DefinizioneAttributo defAtt = new DefinizioneAttributo
            {
                Codice = cod,
                ValoreDefault = valoreDefault,
                IsExpandable = isExpandable,
                IsPreviewable = isPreviewable,
                AllowAttributoCustom = allowAttributoCustom,
            };

            if (!Project.DefinizioniAttributo.ContainsKey(cod))
                Project.DefinizioniAttributo.Add(cod, defAtt);
            else
                Project.DefinizioniAttributo[cod] = defAtt;

                //if (!Project.DefinizioniAttributo.ContainsKey(cod))
                //{
                //    Project.DefinizioniAttributo.Add(cod, new DefinizioneAttributo
                //    {
                //        Codice = cod,
                //        ValoreDefault = valoreDefault,
                //        IsExpandable = isExpandable,
                //        IsPreviewable = isPreviewable,
                //        AllowAttributoCustom = allowAttributoCustom,
                //    });
                //}
        }

        private void CreaEntityTypes()
        {

            //DivisioniItem

            //bool isEmptyModel = true;
            //if (!isEmptyModel)
            //{
            //    AddDivisione("IfcBuildingStorey", "IfcBuildingStorey", Model3dClassEnum.IfcBuildingStorey);/*"IfcBuildingStorey"*/
            //    AddDivisione("IfcElementType", "IfcElementType", Model3dClassEnum.IfcElementType);/*"IfcElementType"*/
            //}

            //Ordine delle divisioni
            int index = 0;
            foreach (DivisioneItemType divType in GetEntityTypes().Values.Where(item => item is DivisioneItemType))
            {
                if (divType.Position < 0)
                    divType.Position = index++;
            }

            if (index == 0)//il programma deve avere almeno una suddivisione
                AddDivisione("Suddivisione1", "Suddivisione 1", Model3dClassEnum.Nothing);
    


            //ContattiItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Contatti))
            {
                ContattiItemType contattiItemType = new ContattiItemType();
                contattiItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(contattiItemType);
            }

            //InfoProgettoItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.InfoProgetto))
            { 
                InfoProgettoItemType infoProgettoItemType = new InfoProgettoItemType();
                infoProgettoItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(infoProgettoItemType);
            }

            //StiliItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Stili))
            {
                StiliItemType stiliItemType = new StiliItemType();
                stiliItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(stiliItemType);
            }


            //PrezzarioItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Prezzario))
            {
                PrezzarioItemParentType articoloParentType = new PrezzarioItemParentType();
                articoloParentType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                PrezzarioItemType articoloType = new PrezzarioItemType();
                articoloType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                articoloType.AssociedType = articoloParentType;
                articoloParentType.AssociedType = articoloType;

                AddEntityType(articoloType);
                AddEntityType(articoloParentType);
            }

            //CapitoliItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Capitoli))
            {
                CapitoliItemParentType capParentType = new CapitoliItemParentType();
                capParentType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                CapitoliItemType capType = new CapitoliItemType();
                capType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                capType.AssociedType = capParentType;
                capParentType.AssociedType = capType;

                AddEntityType(capType);
                AddEntityType(capParentType);
            }

            //ElementiItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Elementi))
            {
                ElementiItemType elementiItemType = new ElementiItemType();
                elementiItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(elementiItemType);
            }

            //ComputoItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Computo))
            {
                ComputoItemType computoItemType = new ComputoItemType();
                computoItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(computoItemType);
            }

            //DocumentiItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Documenti))
            {
                DocumentiItemParentType documentiParentType = new DocumentiItemParentType();
                documentiParentType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                DocumentiItemType documentiType = new DocumentiItemType();
                documentiType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                documentiType.AssociedType = documentiParentType;
                documentiParentType.AssociedType = documentiType;

                AddEntityType(documentiType);
                AddEntityType(documentiParentType); ;
            }

            //ReportItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Report))
            {
                ReportItemType reportItemType = new ReportItemType();
                reportItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(reportItemType);
            }

            //ElencoAttivitaItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.ElencoAttivita))
            {
                ElencoAttivitaItemType elencoAttivitaItemType = new ElencoAttivitaItemType();
                elencoAttivitaItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(elencoAttivitaItemType);
            }

            //WBSItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.WBS))
            {
                WBSItemParentType wbsParentType = new WBSItemParentType();
                wbsParentType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                WBSItemType wbsType = new WBSItemType();
                wbsType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                wbsType.AssociedType = wbsParentType;
                wbsParentType.AssociedType = wbsType;

                AddEntityType(wbsType);
                AddEntityType(wbsParentType);
            }

            //CalendariItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Calendari))
            {
                CalendariItemType calendariItemType = new CalendariItemType();
                calendariItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(calendariItemType);
            }

            //VariabiliItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Variabili))
            {
                VariabiliItemType variabiliItemType = new VariabiliItemType();
                variabiliItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(variabiliItemType);
            }

            //AllegatiItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Allegati))
            {
                AllegatiItemType allItemType = new AllegatiItemType();
                allItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(allItemType);
            }

            //TagItem
            if (!ContainsEntityType(BuiltInCodes.EntityType.Tag))
            {
                TagItemType tagItemType = new TagItemType();
                tagItemType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
                AddEntityType(tagItemType);
            }
        }

        protected void AddEntityType(EntityType entType)
        {
            string key = entType.GetKey();

            if (!Project.EntityTypes.ContainsKey(key))
                Project.EntityTypes.Add(key, entType);
        }

        protected bool ContainsEntityType(string entTypeKey)
        {
            if (Project.EntityTypes.ContainsKey(entTypeKey))
                return true;
            else
                return false;
        }

        private void InitProjectViewSettings()
        {
            foreach (string entTypeKey in Project.EntityTypes.Select(item => item.Value.GetKey()))
            {
                if (!Project.ViewSettings.EntityTypes.ContainsKey(entTypeKey))
                    Project.ViewSettings.EntityTypes.Add(entTypeKey, new EntityTypeViewSettings());
            }
        }

        protected List<Guid> ApplySearch(string entTypeCode, HashSet<Entity> entitiesFilteredSortedHashSet, FilterData filter)
        {
            //HashSet<Entity> entitiesFound = ApplyEntitySearch(entTypeCode, entitiesFilteredSortedHashSet, filter);
            //return entitiesFound.Select(item => item.Id).ToList();

            HashSet<Entity> entities = new HashSet<Entity>(entitiesFilteredSortedHashSet);

            HashSet<Entity> entitiesFound = ApplyFilter(entTypeCode, entities, filter, false);
            return entitiesFound.Select(item => item.EntityId).ToList();
        }

        /// <summary>
        /// Applica l'ordinamento
        /// </summary>
        /// <param name="entitiesFilteredHashSet"></param>
        /// <param name="sort"></param>
        protected void ApplySort(ref HashSet<Entity> entitiesFilteredHashSet, SortData sort)
        {
            if (sort == null || sort.Items.Count == 0)
                return;

            List<Entity> entitiesToSort = entitiesFilteredHashSet.ToList();
            IOrderedEnumerable<Entity> entitiesSorted = SortEntity(sort, entitiesToSort);

            entitiesFilteredHashSet = new HashSet<Entity>(entitiesSorted);
        }

        protected void ApplyTreeSort(ref HashSet<Entity> entitiesFilteredHashSet, SortData sort)
        {
            if (sort == null || sort.Items.Count == 0)
                return;

            List<Entity> entitiesToSorted = entitiesFilteredHashSet.ToList();

            //ordino il livello 0
            List<Entity> depth0 = entitiesFilteredHashSet.Where(item => (item as TreeEntity).Depth == 0).ToList();
            List<Entity> depth0_sorted = SortEntity(sort, depth0).ToList();

            List<Entity> finalEntities = new List<Entity>(depth0_sorted);

            foreach (TreeEntity ent in depth0_sorted)
                AddSortedChildrenToFinalEntities(sort, ent, entitiesToSorted, finalEntities);

            entitiesFilteredHashSet = new HashSet<Entity>(finalEntities);

        }

        private void AddSortedChildrenToFinalEntities(SortData sort, TreeEntity ent, List<Entity> entitiesToSort, List<Entity> finalEntities)
        {
            if (GetChildrenCountOf(ent.EntityTypeCodice, ent) == 0)
                return;

            //if (!ent.HasChildren)
            //    return;

            List<TreeEntity> children = GetChildrenOf(ent.EntityTypeCodice, ent);
            List<Entity> childrenToSort = new List<Entity>(children.Where(item => entitiesToSort.Contains(item)));
            IOrderedEnumerable<Entity> children_sorted = SortEntity(sort, childrenToSort);

            List<Entity> childrenSorted = new List<Entity>(children_sorted);

            int index = finalEntities.IndexOf(ent);
            if (index < finalEntities.Count - 1)
                finalEntities.InsertRange(index + 1, childrenSorted);
            else
                finalEntities.AddRange(childrenSorted);

            foreach (TreeEntity child in childrenSorted)
                AddSortedChildrenToFinalEntities(sort, child, entitiesToSort, finalEntities);
        }


        private IOrderedEnumerable<Entity> SortEntity(SortData sort, List<Entity> entitiesToSort)
        {
            EntitiesHelper entsHelper = new EntitiesHelper(this as ProjectService);

            IOrderedEnumerable<Entity> entitiesSorted = null;

            string attToSortCode0 = sort.Items[0].CodiceAttributo;
            bool isOrdinamentoInverso0 = sort.Items[0].IsOrdinamentoInverso;

            
            //setto deep per gli attributi testo dei padri
            bool deep0 = false;
            Attributo sourceAtt = entsHelper.GetSourceAttributo(sort.EntityTypeKey, attToSortCode0);
            if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto0)
                deep0 = valAttTesto0.UseDeepValore;


            if (isOrdinamentoInverso0)
            {
                entitiesSorted = entitiesToSort.OrderByDescending(item =>
                {
                    return entsHelper.GetValoreAttributo(item, attToSortCode0, deep0, false);
                }
                , new ValoreComparer());
            }
            else
            {
                entitiesSorted = entitiesToSort.OrderBy(item =>
                {
                    return entsHelper.GetValoreAttributo(item, attToSortCode0, deep0, false);
                }
                , new ValoreComparer());
            }


            for (int attIndex = 1; attIndex < sort.Items.Count; attIndex++)
            {
                bool deep = false;
                string attToSortCode = sort.Items[attIndex].CodiceAttributo;
                bool isOrdinamentoInverso = sort.Items[attIndex].IsOrdinamentoInverso;

                sourceAtt = entsHelper.GetSourceAttributo(sort.EntityTypeKey, attToSortCode);
                if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                    deep = valAttTesto.UseDeepValore;



                if (isOrdinamentoInverso)
                {
                    entitiesSorted = entitiesSorted.ThenByDescending(item =>
                    {
                        return GetValoreAttributo(item, attToSortCode, deep, false);
                    }
                    , new ValoreComparer());
                }
                else
                {
                    entitiesSorted = entitiesSorted.ThenBy(item =>
                    {
                        return GetValoreAttributo(item, attToSortCode, deep, false);
                    }
                    , new ValoreComparer());
                }

            }

            return entitiesSorted;
        }

        /// <summary>
        /// Data un entità ritorna il valore di un attributo anche se riferimento ad un altra entità
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="codiceAttributo"></param>
        /// <returns></returns>
        internal Valore GetValoreAttributo_Old(Entity entity, string codiceAttributo, bool deep = false, bool brief = false)
        {
            if (entity == null)
                return null;

            Entity ent = entity;

            if (!ent.Attributi.ContainsKey(codiceAttributo))
                return null;

            EntityAttributo entAtt = ent.Attributi[codiceAttributo];
            Attributo att = entAtt.Attributo;
            while (att is AttributoRiferimento)
            {
                AttributoRiferimento attRif = att as AttributoRiferimento;

                List<Guid> ids = new List<Guid>();
                EntityAttributo entAttRefGuid = ent.Attributi[attRif.ReferenceCodiceGuid];
                if (entAttRefGuid.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    ValoreGuid valGuid = ent.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuid;
                    ids.Add(valGuid.V);
                }
                else if (entAttRefGuid.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    ValoreGuidCollection valGuids = ent.Attributi[attRif.ReferenceCodiceGuid].Valore as ValoreGuidCollection;
                    ids.AddRange(valGuids.GetEntitiesId());
                }

                IEnumerable<Entity> ents;
                if (EntityTypes[attRif.ReferenceEntityTypeKey].IsTreeMaster)
                {
                    ents = GetTreeEntitiesDeepById(attRif.ReferenceEntityTypeKey, ids);
                }
                else
                {
                    ents = GetEntitiesById(attRif.ReferenceEntityTypeKey, ids);
                }

                if (!ents.Any())
                    return null;//nessun valore (che non vuol dire valore vuoto)

                ent = ents.Last();
                ent.ResolveReferences(EntityTypes);

                if (ent.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                {
                    entAtt = ent.Attributi[attRif.ReferenceCodiceAttributo];
                }
                else
                    return null;

                att = entAtt.Attributo;

            }

            if (ent == null)
                return entAtt.Valore;//operazione non andata a buon fine

            Valore val = ent.GetValoreAttributo(att.Codice, deep, brief);
            return val;
        }

        internal Valore GetValoreAttributo(Entity entity, string codiceAttributo, bool deep = false, bool brief = false)
        {
            Valore val = EntitiesHelper.GetValoreAttributo(entity, codiceAttributo, deep, brief);
            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entTypeCode"></param>
        /// <param name="filter"></param>
        /// <param name="valoriDictionary"></param>
        /// <returns></returns>
        protected HashSet<Entity> ApplyTreeFilter(string entTypeCode, FilterData filter)
        {

            HashSet<Entity> entitiesFilteredHashSet = new HashSet<Entity>(GetEntities(entTypeCode));
            entitiesFilteredHashSet.RemoveWhere(item => item.Deleted);

            List<TreeEntity> filteredEntities = ApplyFilter(entTypeCode, entitiesFilteredHashSet, filter, true).Cast<TreeEntity>().ToList();
            List<TreeEntity> finalfilteredEntities = new List<TreeEntity>(filteredEntities);


            if (filter != null && filter.IsFilterApplied())
            {
                //Aggiungo i padri e i figli a tutte le entità filtrate
                foreach (TreeEntity ent in filteredEntities)
                {

                    //Aggiungo i padri
                    TreeEntity parent = ent.Parent;

                    int index = finalfilteredEntities.IndexOf(ent);
                    while (parent != null)
                    {
                        if (finalfilteredEntities.Contains(parent))
                            break;

                        finalfilteredEntities.Insert(index, parent);
                        parent = parent.Parent;
                    }

                    //Aggiungo i figli
                    ApplyTreeFilter_AddChildren(finalfilteredEntities, ent);

                }
            }

            return new HashSet<Entity>(finalfilteredEntities);
        }

        //static private void ApplyTreeFilter_AddChildren(List<TreeEntity> filteredEntities, TreeEntity ent)
        //{
        //    if (!ent.HasChildren)
        //        return;

        //    int index = filteredEntities.IndexOf(ent);

        //    //IEnumerable<Guid> childrenId = ent.Children.Select(item => item.Id);
        //    if (index < filteredEntities.Count - 1)
        //        filteredEntities.InsertRange(index + 1, ent.Children);
        //    else
        //        filteredEntities.AddRange(ent.Children);

        //    foreach (TreeEntity child in ent.Children)
        //        ApplyTreeFilter_AddChildren(filteredEntities, child);

        //}

        private void ApplyTreeFilter_AddChildren(List<TreeEntity> filteredEntities, TreeEntity ent)
        {
            if (GetChildrenCountOf(ent.EntityTypeCodice, ent) == 0)
                return;

            //if (!ent.HasChildren)
            //    return;

            int index = filteredEntities.IndexOf(ent);


            List<TreeEntity> children = GetChildrenOf(ent.EntityTypeCodice, ent);
            //IEnumerable<Guid> childrenId = ent.Children.Select(item => item.Id);
            if (index < filteredEntities.Count - 1)
                filteredEntities.InsertRange(index + 1, children);
            else
                filteredEntities.AddRange(children);

            foreach (TreeEntity child in children)
                ApplyTreeFilter_AddChildren(filteredEntities, child);

        }


        /// <summary>
        /// Applica il filtro
        /// </summary>
        /// <param name="entTypeCode">Codice del tipo di entità da filtrare</param>
        /// <param name="filter">filtro da applicare</param>
        /// <param name="checkFilter">controlla se il filtro è attivato o filtra comunque</param>
        /// <returns></returns>
        internal HashSet<Entity> ApplyFilter(string entTypeCode, HashSet<Entity> entitiesFilteredHashSet, FilterData filter, bool checkFilter)
        {
            if (filter == null)
                return entitiesFilteredHashSet;

            AttributoFormatHelper attributoFormatHelper = new AttributoFormatHelper(this as ProjectService);
            EntitiesHelper entsHelper = new EntitiesHelper(this as ProjectService);


            foreach (AttributoFilterData attFilter in filter.Items)
            {

                if (attFilter.EntityTypeKey != entTypeCode)
                    continue;

                if (!attFilter.IsValid())
                    continue;

                if (checkFilter && !attFilter.IsFiltroAttivato)
                    continue;



                Attributo attrib = null;
                EntityType entType = EntityTypes[attFilter.EntityTypeKey];
                if (entType.Attributi.ContainsKey(attFilter.CodiceAttributo))
                    attrib = entType.Attributi[attFilter.CodiceAttributo];


                if (attrib != null)//sto filtrando per un attributo
                {
                    //Se siamo in presenza di un attributoRiferimento creo la lista degli attributi su cui filtrare
                    //Esempio se sono sul computo e devo filtrare per capitolo prima filtro gli articoli in base al capitolo e poi il computo in base agli articoli trovati
                    List<AttributoRiferimento> attsRif = new List<AttributoRiferimento>();
                    while (attrib is AttributoRiferimento)
                    {
                        AttributoRiferimento attRif = attrib as AttributoRiferimento;

                        //controllo se è un riferimento a un GuidCollection: in questo caso non devo prendere l'attributo sorgente perchè devo filtrare per la somma
                        if (EntityTypes[attRif.EntityTypeKey].Attributi[attRif.ReferenceCodiceGuid].DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                            break;

                        attsRif.Insert(0, attRif);
                        attrib = EntityTypes[attRif.ReferenceEntityTypeKey].Attributi[attRif.ReferenceCodiceAttributo];
                    }


                    HashSet<Entity> entitiesFound = null;


                    if (attFilter.FoundEntitiesId != null)
                    {
                        entitiesFilteredHashSet = new HashSet<Entity>(GetEntitiesById(attFilter.EntityTypeKey, attFilter.FoundEntitiesId));
                        continue;
                    }
                    else if (attrib.EntityTypeKey == attFilter.EntityTypeKey && !attsRif.Any()) //per far funzionare il filtri in AND sulle entità target
                    {
                        //Se sono sull'attributo origine

                        entitiesFound = entitiesFilteredHashSet;//risultato del filtro precedente
                    }
                    else
                    {
                        entitiesFound = new HashSet<Entity>(GetEntities(attrib.EntityTypeKey));
                        entitiesFound.RemoveWhere(item => item.Deleted);
                    }

                    bool allTextSearch = attFilter.IsAllChecked == true && attFilter.TextSearched != null && attFilter.TextSearched.Trim().Any();
                    if (allTextSearch || (attFilter.CheckedValori.Any() || attFilter.FilterType == FilterTypeEnum.Conditions))
                    {

                        entitiesFound.RemoveWhere(item =>
                        {
                            EntityType treeEntType = entsHelper.GetTreeEntityType(item);
                            Attributo att = null;
                            if (treeEntType.Attributi.TryGetValue(attrib.Codice, out att))
                                if (att.IsSummary)
                                    return true;

                            Valore val = null;
                            if (attrib.Codice == BuiltInCodes.Attributo.DescrizioneRTF)
                            {
                                val = new ValoreTesto() { V = entsHelper.GetValorePlainText(item, attrib.Codice, true, false) };
                            }
                            else
                            {
                                bool deep = false;
                                if (attrib.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                                    deep = valAttTesto.UseDeepValore;

                                val = entsHelper.GetValoreAttributo(item, attrib.Codice, deep, false);
                            }
                                

                            if (val == null)
                                return true;

                            if (attFilter.FilterType == FilterTypeEnum.Conditions)
                            {
                                string format = null;
                                format = attributoFormatHelper.GetValoreFormat(item, attrib.Codice);

                                if (val.CheckFilterConditions(attFilter.ValoreConditions, format))
                                    return false;
                            }
                            else if (allTextSearch)
                            {
                                //if (val != null && val.ResultContainsTesto(attFilter.TextSearched))
                                //    return false;
                                if (val != null)
                                {
                                    if (attFilter.FilterType == FilterTypeEnum.Result)
                                    {
                                        string format = null;
                                        //if (val is ValoreReale || val is ValoreContabilita)
                                            //format = attributoFormatHelper.GetValorePaddedFormat(item.Attributi[attrib.Codice]);
                                        format = attributoFormatHelper.GetValorePaddedFormat(item, attrib.Codice);

                                        if (val.ResultContainsTesto(attFilter.TextSearched, format))
                                            return false;
                                    }
                                    else if (attFilter.FilterType == FilterTypeEnum.Formula)
                                    {
                                        if (val.ContainsTesto(attFilter.TextSearched))
                                            return false;
                                    }
                                }
                            }
                            else
                            {
                                if (attFilter.FilterType == FilterTypeEnum.Conditions)//?
                                {
                                    string format = null;

                                    format = attributoFormatHelper.GetValoreFormat(item, attrib.Codice);

                                    if (val.CheckFilterConditions(attFilter.ValoreConditions, format))
                                        return false;
                                }
                                else
                                {

                                    foreach (string str in attFilter.CheckedValori)
                                    {
                                        
                                        if (attFilter.FilterType == FilterTypeEnum.Result)
                                        {

                                            string format = null;
                                            
                                            format = attributoFormatHelper.GetValorePaddedFormat(item, attrib.Codice);
                                                
                                            if (val.ResultHasTesto(str, format, attFilter.IgnoreCase))
                                                return false;


                                            if (attrib.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                                            {
                                                //cerco il guid anche tra i padri
                                                var parentsId = GetTreeEntitiesDeepById(attrib.GuidReferenceEntityTypeKey, new List<Guid> { (val as ValoreGuid).V }).Select(item => item.EntityId.ToString());

                                                if (parentsId.Contains(str))
                                                    return false;
                                            }
                                        }
                                        else if (attFilter.FilterType == FilterTypeEnum.Formula)
                                        {
                                            if (val.HasTesto(str))
                                                return false;
                                        }
                                        else
                                        {
                                            bool isNothing = true;
                                        }
                                    }
                                }
                            }


                            return true;
                        });

                    }


                    HashSet<Guid> checkedValori =  new HashSet<Guid>(entitiesFound.Select(item => item.EntityId));

                    //filtro su attributo di tipo riferimento
                    foreach (AttributoRiferimento attRif in attsRif)
                    {
                        if (attRif.EntityTypeKey == attFilter.EntityTypeKey) //per far funzionare il filtri in AND sulle entità target
                        {
                            entitiesFound = entitiesFilteredHashSet;//risultato del filtro precedente
                            //entitiesFiltered = entitiesFilteredHashSet.ToHashSet();
                        }
                        else
                        {
                            entitiesFound = new HashSet<Entity>(GetEntities(attRif.EntityTypeKey));
                            entitiesFound.RemoveWhere(item => item.Deleted);
                        }


                        string codiceAttributo = attRif.ReferenceCodiceGuid;
                        entitiesFound.RemoveWhere(item =>
                        {
                            Valore val = GetValoreAttributo(item, codiceAttributo);
                            if (val is ValoreGuid)
                            {
                                ValoreGuid valGuid = val as ValoreGuid;

                                if (valGuid == null)
                                    return true;

                                if (valGuid.V == Guid.Empty)
                                {
                                    if (attFilter.FilterType == FilterTypeEnum.Conditions)
                                    {
                                        bool ret = !valGuid.CheckFilterConditions(attFilter.ValoreConditions, string.Empty);
                                        return ret;
                                    }
                                    else if (attFilter.CheckedValori.Contains(ValoreHelper.ValoreNullAsText))
                                        return false;
                                }

                                foreach (Guid id in checkedValori)
                                {
                                    if (valGuid.V == id)
                                        return false;
                                }
                            }
                            else if (val is ValoreGuidCollection)
                            {
                                ValoreGuidCollection valGuidColl = val as ValoreGuidCollection;

                                foreach (Guid id in checkedValori)
                                {
                                    if (valGuidColl.HasTesto(id.ToString()))
                                        return false;
                                }
                            }

                            return true;
                        });

                        checkedValori = new HashSet<Guid>(entitiesFound.Select(item => item.EntityId));

                        //by Ale 20/12/2022
                        //entitiesFilteredHashSet = entitiesFilteredHashSet.Where(item => checkedValori.Contains(item.EntityId)).ToHashSet();
                    }

                }
                else if (attFilter.CodiceAttributo == BuiltInCodes.Attributo.TemporaryFilterByIds)
                {
                    //filtro temporaneo per ids
                    entitiesFilteredHashSet.RemoveWhere(item =>
                    {
                        return !attFilter.CheckedValori.Contains(item.EntityId.ToString());
                    });

                }

                attFilter.FoundEntitiesId = new HashSet<Guid>(entitiesFilteredHashSet.Select(item => item.EntityId));

            }//fine for attributi filtro

            return entitiesFilteredHashSet;
        }

        

        ///// <summary>
        ///// Ricalcolo tutti i valori dell'entità senza ricalcolare gli attributi riferiti
        ///// </summary>
        ///// <param name="ent"></param>
        ///// <returns></returns>
        //protected bool CalcolaEntityValues(Entity ent, HashSet<Guid> entititesChanged = null)
        //{

        //    EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);
        //    bool res = entityAttributiUpdater.CalcolaEntityValues(ent);
        //    if (entititesChanged != null)
        //        entititesChanged.UnionWith(entityAttributiUpdater.GetEntitiesChanged());

        //    return res;


        //}


        ///// <summary>
        ///// Ricalcola tutti i valori degli attributi dell'entità ent che dipendono da attNewValore
        ///// </summary>
        ///// <param name="ent">entità di cui ricalcolare gli attributi</param>
        ///// <param name="attNewValore">valore modificato</param>
        ///// <returns></returns>
        //protected bool CalcolaEntityValues(Entity ent, EntityAttributo attNewValore, HashSet<Guid> entitiesChanged = null)
        //{
        //    EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);
        //    bool res = entityAttributiUpdater.CalcolaEntityValues(ent, attNewValore);
        //    if (entitiesChanged != null)
        //        entitiesChanged.UnionWith(entityAttributiUpdater.GetEntitiesChanged());
        //    return res;
        //}


        protected List<string> GetValoriUnivoci(string entityTypeKey, List<Guid> entitiesId, string codiceAttributo, int takeResults, string textSearched)
        {
            if (entitiesId == null)
                return new List<string>();

            HashSet<Entity> entities = new HashSet<Entity>(GetEntities(entityTypeKey).Where(item => entitiesId.Contains(item.EntityId) && item.Deleted == false));
            HashSet<string> valoriUnivoci = CreateValoriUnivociAttributo(entityTypeKey, entities, codiceAttributo, takeResults, textSearched);

            List<string> sValori = new List<string>();
            sValori = valoriUnivoci.ToList();

            return sValori;
        }

        private HashSet<string> CreateValoriUnivociAttributo(string codeEntityType, HashSet<Entity> entities, string codiceAtt, int takeResults, string textSearched)
        {
            Attributo att = Project.EntityTypes[codeEntityType].Attributi[codiceAtt];

            if (!att.AllowValoriUnivoci)
                return new HashSet<string>();



            EntitiesHelper entsHelper = new EntitiesHelper(this as IDataService);
            Attributo sourceAtt = entsHelper.GetSourceAttributo(att);

            HashSet<Entity> entitiesWithAttributo = new HashSet<Entity>(entities);
            bool deep = false;

            //introdotto per usare il valore con i padri per i valori univoci dei filtri
            if (sourceAtt.ValoreAttributo is ValoreAttributoTesto valAttTesto)
                deep = valAttTesto.UseDeepValore;

            Dictionary<Guid, Valore> valsByEntId = entitiesWithAttributo.ToDictionary(item => item.EntityId, item => entsHelper.GetValoreAttributo(item, codiceAtt, deep, false));
            List<Valore> vals = valsByEntId.Values.ToList();


            bool nullRemoved = vals.RemoveAll(item => item == null) > 0;

            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid ||
                sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
            {
                HashSet<Guid> uniqueGuids = null;

                if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    uniqueGuids = new HashSet<Guid>(vals.Select(item => (item as ValoreGuid).V));
                }
                else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                {
                    uniqueGuids = new HashSet<Guid>(vals.SelectMany(item => (item as ValoreGuidCollection).Items).Select(item => (item as ValoreGuidCollectionItem).EntityId));
                }


                EntityType sourceEntityTypeKey = Project.EntityTypes[sourceAtt.GuidReferenceEntityTypeKey];
                //TreeEntityType sourceTreeEntityTypeKey = Project.EntityTypes[sourceAtt.GuidReferenceEntityTypeKey] as TreeEntityType;

                HashSet<string> uniqueStrings = null;
                if (sourceEntityTypeKey is TreeEntityType)
                {
                    TreeEntityType sourceTreeEntityTypeKey = sourceEntityTypeKey as TreeEntityType;
                    List<string> attsCode = sourceTreeEntityTypeKey.GetParentAttributi();

                    //Filtro per Codice Unione DescrizioneRtf

                    //Filtro per codice
                    FilterData filterByCodice = new FilterData();
                    filterByCodice.Items.Add(new AttributoFilterData()
                    {
                        CodiceAttributo = attsCode[0],
                        TextSearched = textSearched,
                        SourceCodiceAttributo = attsCode[0],
                        SourceEntityTypeKey = sourceAtt.GuidReferenceEntityTypeKey,
                        EntityTypeKey = sourceAtt.GuidReferenceEntityTypeKey,
                        IsFiltroAttivato = true,
                        IsAllChecked = true,
                    });

                    //List<TreeEntityMasterInfo> treeEntsInfo = (this as ProjectService).GetFilteredTreeEntities(sourceAtt.GuidReferenceEntityTypeKey, null, null, out _);
                    //treeEntsInfo = EntitiesHelper.TreeFilterById(treeEntsInfo, uniqueGuids);
                    //HashSet<Entity> ents = new HashSet<Entity>(GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, treeEntsInfo.Select(item => item.Id)));

                    HashSet<Entity> ents = new HashSet<Entity>(GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, uniqueGuids));
                    HashSet<Entity> entsCodiceFiltered = ApplyFilter(sourceAtt.GuidReferenceEntityTypeKey, ents, filterByCodice, true);

                    //Filtro per descrizioneRtf
                    FilterData filterByDesc = new FilterData();
                    filterByDesc.Items.Add(new AttributoFilterData()
                    {
                        CodiceAttributo = attsCode[1],
                        SourceCodiceAttributo = attsCode[1],
                        TextSearched = textSearched,
                        SourceEntityTypeKey = sourceAtt.GuidReferenceEntityTypeKey,
                        EntityTypeKey = sourceAtt.GuidReferenceEntityTypeKey,
                        IsFiltroAttivato = true,
                        IsAllChecked = true,
                    });

                    
                    ents = new HashSet<Entity>(GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, uniqueGuids));
                    HashSet<Entity> entsDescFiltered = ApplyFilter(sourceAtt.GuidReferenceEntityTypeKey, ents, filterByDesc, true);

                    uniqueStrings = new HashSet<string>(entsCodiceFiltered.Union(entsDescFiltered).Select(item => item.EntityId.ToString()));

                    if (uniqueGuids.Contains(Guid.Empty))
                        uniqueStrings.Add(Guid.Empty.ToString());
                }
                else
                {
                    if (!string.IsNullOrEmpty(textSearched))
                    {
                        List<Entity> ents = GetEntitiesById(sourceAtt.GuidReferenceEntityTypeKey, uniqueGuids);
                        var entsFiltered = ents.Where(item => item.ToUserIdentity(UserIdentityMode.SingleLine).Contains(textSearched, StringComparison.OrdinalIgnoreCase));
                        uniqueStrings = new HashSet<string>(entsFiltered.Select(item => item.EntityId.ToString()));
                    }
                    else
                        uniqueStrings = new HashSet<string>(uniqueGuids.Select(item => item.ToString()));
                }

                //if (nullRemoved)
                //    uniqueStrings.Add(ValoreHelper.ValoreNullAsText);

                return uniqueStrings;

            }
            else
            {
                HashSet<string> uniqueStrings = null;
                Dictionary<Guid, string> formatsByEntId = null;
                AttributoFormatHelper attributoFormatHelper = new AttributoFormatHelper(this as ProjectService);
                if (takeResults == 1)
                {
                    //prendo i risultati (Result)
                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                    {
                        //formatsByEntId = entitiesWithAttributo.ToDictionary(item => item.EntityId, item => attributoFormatHelper.GetValorePaddedFormat(item.Attributi[codiceAtt]));
                        formatsByEntId = entitiesWithAttributo.ToDictionary(item => item.EntityId, item => attributoFormatHelper.GetValorePaddedFormat(item, codiceAtt));
                        //IEnumerable<string> strs = valsByEntId.SelectMany(item => item.Value.ToValoreSingleSet()
                        //                                                                    .Select(item1 => (item1 as ValoreReale)
                        //                                                                    .FormatRealResult(formatsByEntId[item.Key])));
                        IEnumerable<string> strs = valsByEntId.SelectMany(item =>
                        {
                            if (item.Value != null)
                                return item.Value.ToValoreSingleSet()
                                                    .Select(item1 => (item1 as ValoreReale)
                                                    .FormatRealResult(formatsByEntId[item.Key]));
                            else
                                return new List<string>();
                        });

                        uniqueStrings = new HashSet<string>(strs);
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                    else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    {
                        //formatsByEntId = entitiesWithAttributo.ToDictionary(item => item.EntityId, item => attributoFormatHelper.GetValorePaddedFormat(item.Attributi[codiceAtt]));
                        formatsByEntId = entitiesWithAttributo.ToDictionary(item => item.EntityId, item => attributoFormatHelper.GetValorePaddedFormat(item, codiceAtt));
                        //IEnumerable<string> strs = valsByEntId.SelectMany(item => item.Value.ToValoreSingleSet()
                        //                                                                    .Select(item1 => (item1 as ValoreContabilita)
                        //                                                                    .FormatRealResult(formatsByEntId[item.Key])));

                        IEnumerable<string> strs = valsByEntId.SelectMany(item =>
                        {
                            if (item.Value != null)
                                return item.Value.ToValoreSingleSet()
                                                    .Select(item1 => (item1 as ValoreContabilita)
                                                    .FormatRealResult(formatsByEntId[item.Key]));
                            else
                                return new List<string>();
                        });

                        uniqueStrings = new HashSet<string>(strs);
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                    else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                    {
                        uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => (item as ValoreTesto).Result));
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                    else
                    {
                        uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => (item.ToPlainText())));
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                }
                else if (takeResults == 0)
                {
                    //prendo le formule (V)
                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                    {
                        uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => (item as ValoreReale).V));
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                    else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    {
                        uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => (item as ValoreContabilita).V));
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                    else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                    {
                        uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => (item as ValoreTesto).V));
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                    else
                    {
                        uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => item.ToPlainText()));
                        uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                    }
                }
                else
                {
                    //Prendo quello che torna ToPlainText
                    uniqueStrings = new HashSet<string>(vals.SelectMany(item => item.ToValoreSingleSet()).Select(item => item.ToPlainText()));
                    uniqueStrings.RemoveWhere(item => !ValoreHelper.ContainsTesto(item, textSearched));
                }

                if (nullRemoved)
                    uniqueStrings.Add(ValoreHelper.ValoreNullAsText);

                return uniqueStrings;
            }
        }


        private List<TreeEntity> GetChildrenOf(string codiceEntityType, TreeEntity ent)
        {
            List<TreeEntity> ents = GetTreeEntitiesList(codiceEntityType);

            int index = ents.IndexOf(ent);

            List<TreeEntity> children = new List<TreeEntity>();
            for (int i = index + 1; i < ents.Count; i++)
            {
                if (ents[i].Depth <= ent.Depth)
                    break;

                if (ents[i].Depth == ent.Depth + 1)
                    children.Add(ents[i]);
            }
            return children;
        }

        internal TreeEntity GetParentOf(TreeEntity ent)
        {
            if (ent == null)
                return null;

            List<TreeEntity> ents = GetTreeEntitiesList(ent.EntityTypeCodice);
            int index = ents.IndexOf(ent);

            for (int i = index; i >= 0; i--)
                if (ents[i].Depth < ent.Depth)
                    return ents[i];

            return null;
        }


        private int GetChildrenCountOf(string codiceEntityType, TreeEntity ent)
        {
            List<TreeEntity> ents = GetTreeEntitiesList(codiceEntityType);

            int index = ents.IndexOf(ent);

            int count = 0;
            for (int i = index + 1; i < ents.Count; i++)
            {
                if (ents[i].Depth <= ent.Depth)
                    break;

                if (ents[i].Depth == ent.Depth + 1)
                    count++;
            }
            return count;
        }

        private int GetChildrenCountOf(string codiceEntityType, int treeEntIndex)
        {
            List<TreeEntity> ents = GetTreeEntitiesList(codiceEntityType);

            TreeEntity ent = ents[treeEntIndex];
            int index = treeEntIndex;

            int count = 0;
            for (int i = index + 1; i < ents.Count; i++)
            {
                if (ents[i].Depth <= ent.Depth)
                    break;

                if (ents[i].Depth == ent.Depth + 1)
                    count++;
            }
            return count;
        }

        internal int DescendantsCountOf(string EntityTypeKey, TreeEntity ent, out int lastChildIndex)
        {
            List<TreeEntity> ents = GetTreeEntitiesList(EntityTypeKey);

            int entIndex = ents.IndexOf(ent);

            int count = 0;
            for (int i = entIndex + 1; i < ents.Count; i++)
            {
                if (ents[i].Depth <= ent.Depth)
                    break;

                count++;
            }
            lastChildIndex = entIndex + count;

            return count;
        }

        //private void UpdateDescendantsDepthOf(string codiceEntityType, TreeEntity ent, int offset)
        //{
        //    List<TreeEntity> ents = GetTreeEntities(codiceEntityType);
        //    int index = ents.IndexOf(ent);

        //    for (int i = index + 1; i < ents.Count; i++)
        //    {
        //        if (ents[i].Depth <= ent.Depth)
        //            break;

        //        ents[i].Depth += offset;
        //    }

        //}

        //protected bool IsTreeEntityParent(TreeEntity treeEnt)
        //{
        //    if (treeEnt.Attributi.Count > treeEnt.ParentEntityType.Attributi.Count)
        //        return false;

        //    int nChildren = GetChildrenCountOf(treeEnt.EntityTypeCodice, treeEnt);

        //    return (nChildren > 0);
        //}
        protected bool IsTreeEntityParent(Entity ent)
        {
            TreeEntity treeEnt = ent as TreeEntity;
            if (treeEnt == null)
                return false;

            if (treeEnt.Attributi.Count > treeEnt.ParentEntityType.Attributi.Count)
                return false;

            int nChildren = GetChildrenCountOf(treeEnt.EntityTypeCodice, treeEnt);

            return (nChildren > 0);
        }

        private bool IsTreeEntityParent(List<TreeEntity> treeEnts, int treeEntIndex)
        {
            TreeEntity treeEnt = treeEnts[treeEntIndex];

            int nextIndex = treeEntIndex + 1;

            if (nextIndex < treeEnts.Count && treeEnts[nextIndex].Depth > treeEnt.Depth)
                return true;

            return false;
        }

        protected ModelActionResponse CommitActionInternal(ModelAction action, HashSet<Guid> entitiesChangedId)
        {
            //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<ProjectService>();
            //log.Trace("CommitAction: ActionName:{0} - AttributoCode:{1} - EntitiesCount:{2}", action.ActionName.ToString(), action.AttributoCode, action.EntitiesId.Count);


            ModelActionResponse mar = new ModelActionResponse();
            mar.ActionResponse = ActionResponse.OK;

            if (action.ActionName != ActionName.ENTITY_DELETE && action.ActionName != ActionName.TREEENTITY_DELETE)
                entitiesChangedId.UnionWith(action.EntitiesId);
            else
                entitiesChangedId.ExceptWith(action.EntitiesId);


            //List<Entity> entities = GetEntities(action.EntityTypeKey).Where(item => action.EntitiesId.Contains(item.Id)).ToList();
            List<Entity> entities = GetEntitiesById(action.EntityTypeKey, action.EntitiesId);
            //IEnumerable<Entity> allEntities = GetEntities(action.EntityTypeKey);

            switch (action.ActionName)
            {
                case ActionName.ATTRIBUTO_VALORE_MODIFY:
                    {
                        //modifica del valore per ogni entità interessate
                        foreach (Entity ent in entities)
                        {
                            if (!ent.Attributi.ContainsKey(action.AttributoCode))
                            {
                                mar.ActionResponse = ActionResponse.FAILED;
                                continue;
                            }

                            EntityAttributo att = ent.Attributi[action.AttributoCode];
                            if (!att.Valore.IsMultiValore())
                            {
                                //att.Valore = action.NewValore.Clone();

                                Valore newValore = action.NewValore.Clone();

                                string formula = action.NewValore.GetFormula();
                                if (!string.IsNullOrEmpty(formula))
                                {
                                    string functionItselfResult = string.Format(ValoreHelper.ItselfResult, att.Attributo.Etichetta);
                                    formula = formula.Replace(functionItselfResult, ValoreHelper.ItselfResult);

                                    if (formula.Contains(ValoreHelper.ItselfResult))
                                    {
                                        //Formula con auto riferimento
                                        newValore = att.Valore.Clone();
                                        newValore.SetByString(formula);
                                        att.Valore = newValore;
                                    }

                                    string functionItselfFormula = string.Format(ValoreHelper.ItselfFormula, att.Attributo.Etichetta);
                                    formula = formula.Replace(functionItselfFormula, ValoreHelper.ItselfFormula);

                                    if (formula.Contains(ValoreHelper.ItselfFormula))
                                    {
                                        //Formula con auto riferimento
                                        newValore = att.Valore.Clone();
                                        newValore.SetByString(formula);
                                        att.Valore = newValore;
                                    }
                                }

                                att.Valore = newValore;


                            }
                            IsDigicorpOwnerAction(action, ent);

                        }
                        break;
                    }
                case ActionName.TREEENTITY_ADD:
                case ActionName.ENTITY_ADD:
                    {
                        //inserisce in coda
                        Entity newEntity = ServerFactory.NewEntity(action.EntityTypeKey);// EntityTypes[action.EntityTypeCode].NewEntity();
                        newEntity.CreaAttributi();

                        //GetEntities(action.EntityTypeKey).Add(newEntity);// rem 19/01/2023
                        AddEntity(action.EntityTypeKey, newEntity, Guid.Empty);

                        mar.NewId = newEntity.EntityId;

                        //UpdateEntitiesIndexes(action.EntityTypeKey, newEntity, null); // rem 19/01/2023 viene già fatto in AddEntity

                        //perform nested actions
                        foreach (ModelAction nestedAction in action.NestedActions)
                        {
                            if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                nestedAction.EntitiesId = new HashSet<Guid>() { newEntity.EntityId };

                            if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                nestedAction.EntityTypeKey = action.EntityTypeKey;

                            if (nestedAction.ActionName == ActionName.TREEENTITY_ADD_CHILD)
                                nestedAction.NewTargetEntitiesId = new List<TargetReference>()
                                {
                                    new TargetReference()
                                    {
                                        Id = newEntity.EntityId,
                                        TargetReferenceName = TargetReferenceName.CHILD_OF
                                    }
                                };

                            CommitActionInternal(nestedAction, entitiesChangedId);
                        }

                        //CalcolaEntityValues(newEntity);
                        break;
                    }
                case ActionName.ENTITY_DELETE:
                    {
                        List<Entity> allEntities = GetEntitiesList(action.EntityTypeKey);
                        List<Entity> ents = GetEntitiesById(action.EntityTypeKey, action.EntitiesId);
                        ents.ForEach(item =>
                        {
                            item.Deleted = true;
                            allEntities.Remove(item);
                        });

                        //per il ricalcolo delle entità riferite nella stessa sezione
                        List<EntityAttributo> entsAtt = GetEntityAttributoGuidByEntityId(action.EntityTypeKey, action.EntitiesId);
                        entsAtt.ForEach(item =>
                        {
                            //item.Valore = new ValoreGuid();
                            if (item.Entity.EntityTypeCodice == action.EntityTypeKey)//per il ricalcolo delle entità riferite nella stessa sezione
                                entitiesChangedId.Add(item.Entity.EntityId);
                        });

                        //per il ricalcolo delle entità riferite nella stessa sezione
                        entsAtt = GetEntityAttributoGuidCollectionByEntityId(action.EntityTypeKey, action.EntitiesId);
                        entsAtt.ForEach(item =>
                        {
                            if (item.Entity.EntityTypeCodice == action.EntityTypeKey)
                                entitiesChangedId.Add(item.Entity.EntityId);
                        });

                        UpdateEntitiesIndexes(action.EntityTypeKey, null, ents);
                        break;
                    }
                case ActionName.TREEENTITY_DELETE:
                    {
                        List<TreeEntity> allEntities = GetTreeEntitiesList(action.EntityTypeKey);
                        TreeEntityDelete(action, entitiesChangedId, allEntities);

                        break;

                    }

                case ActionName.ENTITY_INSERT:
                    {
                        //inserisce prima o dopo di target
                        
                        List<Entity> allEntities = GetEntitiesList(action.EntityTypeKey);
                        
                        if (action.NewTargetEntitiesId.Count != 1)
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            break;
                        }

                        Entity ent = allEntities.FirstOrDefault(item => item.EntityId == action.NewTargetEntitiesId[0].Id);
                        if (ent == null)
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            break;
                        }

                        int newIndex = 0;
                        if (action.NewTargetEntitiesId[0].TargetReferenceName == TargetReferenceName.BEFORE)
                            newIndex = allEntities.IndexOf(ent);
                        else
                            newIndex = allEntities.IndexOf(ent) + 1;


                        Entity newEntity = ServerFactory.NewEntity(action.EntityTypeKey); // EntityTypes[action.EntityTypeCode].NewEntity();
                        newEntity.CreaAttributi();

                        if (newIndex < allEntities.Count)
                            allEntities.Insert(newIndex, newEntity);
                        else
                            allEntities.Add(newEntity);

                        mar.NewId = newEntity.EntityId;

                        //UpdateEntitiesIndexes(action.EntityTypeKey);
                        UpdateEntitiesIndexes(action.EntityTypeKey, newEntity, null);

                        //perform nested actions
                        foreach (ModelAction nestedAction in action.NestedActions)
                        {
                            if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                nestedAction.EntitiesId = new HashSet<Guid>() { newEntity.EntityId };

                            if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                nestedAction.EntityTypeKey = action.EntityTypeKey;

                            CommitActionInternal(nestedAction, entitiesChangedId);
                        }

                        //CalcolaEntityValues(newEntity);

                        break;
                    }
                case ActionName.TREEENTITY_INSERT:
                    {
                        //inserisce prima o dopo di target
                        List<TreeEntity> allEntities = GetTreeEntitiesList(action.EntityTypeKey);

                        if (action.NewTargetEntitiesId.Count != 1)
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            break;
                        }

                        TreeEntity ent = allEntities.FirstOrDefault(item => item.EntityId == action.NewTargetEntitiesId[0].Id) as TreeEntity;
                        if (ent == null)
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            break;
                        }

                        int newIndex = 0;
                        if (action.NewTargetEntitiesId[0].TargetReferenceName == TargetReferenceName.BEFORE)
                            newIndex = allEntities.IndexOf(ent);
                        else
                            newIndex = allEntities.IndexOf(ent) + 1;


                        TreeEntity newEntity = ServerFactory.NewEntity(action.EntityTypeKey) as TreeEntity;
                        newEntity.CreaAttributi();

                        newIndex += DescendantsCountOf(action.EntityTypeKey, ent, out _);// ent.DescendantsCount();

                        //newEntity.MakeSiblingOf(ent);
                        MakeSiblingOf(newEntity, ent);
                        //UpdateDescendantsDepthOf(newEntity, ent.Depth, 1);

                        if (newEntity.Parent != null)//copio il codice del padre
                        {
                            newEntity.CopyValoriAttributiFrom(newEntity.Parent);
                        }

                        if (newIndex < allEntities.Count)
                            allEntities.Insert(newIndex, newEntity);
                        else
                            allEntities.Add(newEntity);

                        UpdateDescendantsDepthOf(newEntity, ent.Depth, 1);

                        mar.NewId = newEntity.EntityId;

                        //UpdateEntitiesIndexes(action.EntityTypeKey);
                        UpdateEntitiesIndexes(action.EntityTypeKey, newEntity, null);

                        //perform nested actions
                        foreach (ModelAction nestedAction in action.NestedActions)
                        {
                            if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                nestedAction.EntitiesId = new HashSet<Guid>() { newEntity.EntityId };

                            if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                nestedAction.EntityTypeKey = action.EntityTypeKey;

                            CommitActionInternal(nestedAction, entitiesChangedId);
                        }

                        //CalcolaEntityValues(newEntity);

                        break;
                    }

                case ActionName.TREEENTITY_ADD_CHILD:
                    {
                        //Aggiunge un figlio al parent (in fondo)

                        List<TreeEntity> allEntities = GetTreeEntitiesList(action.EntityTypeKey);

                        if (action.NewTargetEntitiesId.Count != 1)
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            break;
                        }

                        TreeEntity parent = allEntities.FirstOrDefault(item => item.EntityId == action.NewTargetEntitiesId[0].Id) as TreeEntity;
                        if (parent != null)
                        {

                            TreeEntity newEntity = ServerFactory.NewEntity(action.EntityTypeKey) as TreeEntity;// EntityTypes[action.EntityTypeCode].NewEntity() as TreeEntity;
                            newEntity.CreaAttributi();


                            //if (parent.HasChildren)// parent.Children.Any())
                            //{
                            if (GetChildrenCountOf(parent.EntityTypeCodice, parent) > 0)
                            {
                                int parentIndex = allEntities.IndexOf(allEntities.FirstOrDefault(item => item.EntityId == parent.EntityId));
                                int index = parentIndex + DescendantsCountOf(action.EntityTypeKey, parent, out _);// parent.DescendantsCount();

                                if (index < allEntities.Count - 1)
                                    allEntities.Insert(index + 1, newEntity);
                                else
                                    allEntities.Add(newEntity);

                                //newEntity.MakeChildOf(parent);
                                MakeChildOf(newEntity, parent);
                                UpdateDescendantsDepthOf(newEntity, parent.Depth + 1, 1);

                                //newEntity.CopyFrom(parent, true);
                                newEntity.CopyValoriAttributiFrom(parent);

                                mar.NewId = newEntity.EntityId;


                            }
                            else //nel caso non ci sia ancora nessun figlio
                            {

                                int parentIndex = allEntities.IndexOf(allEntities.FirstOrDefault(item => item.EntityId == parent.EntityId));
                                if (parentIndex < allEntities.Count - 1)
                                    allEntities.Insert(parentIndex + 1, newEntity);
                                else
                                    allEntities.Add(newEntity);

                                parent.MakeParent();
                                entitiesChangedId.Add(parent.EntityId);

                                //newEntity.MakeChildOf(parent);
                                MakeChildOf(newEntity, parent);
                                UpdateDescendantsDepthOf(newEntity, parent.Depth + 1, 1);

                                newEntity.CopyValoriAttributiFrom(parent);



                                mar.NewId = newEntity.EntityId;


                            }
                            //UpdateEntitiesIndexes(action.EntityTypeKey);
                            UpdateEntitiesIndexes(action.EntityTypeKey, newEntity, null);

                            //perform nested actions
                            foreach (ModelAction nestedAction in action.NestedActions)
                            {
                                if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                    nestedAction.EntitiesId = new HashSet<Guid>() { newEntity.EntityId };

                                if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                    nestedAction.EntityTypeKey = action.EntityTypeKey;

                                if (nestedAction.ActionName == ActionName.TREEENTITY_ADD_CHILD)
                                    nestedAction.NewTargetEntitiesId = new List<TargetReference>()
                                {
                                    new TargetReference()
                                    {
                                        Id = newEntity.EntityId,
                                        TargetReferenceName = TargetReferenceName.CHILD_OF
                                    }
                                };

                                CommitActionInternal(nestedAction, entitiesChangedId);
                            }

                            //CalcolaEntityValues(newEntity);

                        }


                        break;
                    }
                case ActionName.TREEENTITY_ADD_PARENT:
                    {
                        List<TreeEntity> allEntities = GetTreeEntitiesList(action.EntityTypeKey);

                        if (action.NewTargetEntitiesId.Count != 1)
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            break;
                        }

                        TreeEntity ent = allEntities.FirstOrDefault(item => item.EntityId == action.NewTargetEntitiesId[0].Id) as TreeEntity;
                        if (ent != null)
                        {
                            TreeEntity newParent = ServerFactory.NewEntity(action.EntityTypeKey) as TreeEntity;
                            newParent.CreaAttributiParent();

                            // (EntityTypes[action.EntityTypeCode] as TreeEntityType).NewParentEntity() as TreeEntity;
                            int entIndex = allEntities.IndexOf(allEntities.FirstOrDefault(item => item.EntityId == ent.EntityId));

                            allEntities.Insert(entIndex, newParent);


                            if (ent.Parent != null)
                            {
                                MakeChildOf(newParent, ent.Parent);
                                //newParent.MakeChildOf(ent.Parent);
                                UpdateDescendantsDepthOf(newParent, ent.Parent.Depth + 1, 1);
                            }

                            MakeChildOf(ent, newParent);

                            int count = DescendantsCountOf(action.EntityTypeKey, ent, out _) + 1;

                            //ent.MakeChildOf(newParent);
                            //MakeChildOf(ent, newParent);
                            //UpdateDescendantsDepthOf(ent, newParent.Depth + 1, 0);
                            UpdateDescendantsDepthOf(ent, newParent.Depth + 1, count);

                            mar.NewId = newParent.EntityId;

                            //UpdateEntitiesIndexes(action.EntityTypeKey);
                            UpdateEntitiesIndexes(action.EntityTypeKey, newParent, null);


                            //perform nested actions
                            foreach (ModelAction nestedAction in action.NestedActions)
                            {
                                if (nestedAction.EntitiesId == null)
                                    nestedAction.EntitiesId = new HashSet<Guid>() { newParent.EntityId };
                                CommitActionInternal(nestedAction, entitiesChangedId);
                            }
                        }

                        break;
                    }
                case ActionName.ENTITY_MOVE:
                    {
                        //muove una lista di entità sopra o sotto il target

                        List<Entity> allEntities = GetEntitiesList(action.EntityTypeKey);

                        List<Entity> entsToMove = GetEntities(action.EntityTypeKey).Where(item => action.EntitiesId.Contains(item.EntityId)).ToList();


                        for (int i = 0; i < entsToMove.Count; i++)
                        {
                            Entity entToMove = entsToMove[i];
                            allEntities.Remove(entsToMove[i]);

                            Entity ent = allEntities.FirstOrDefault(item => item.EntityId == action.NewTargetEntitiesId[i].Id);
                            if (ent != null)
                            {
                                int newIndex = allEntities.IndexOf(ent);
                                if (action.NewTargetEntitiesId[i].TargetReferenceName == TargetReferenceName.BEFORE)
                                    allEntities.Insert(newIndex, entToMove);
                                else if (action.NewTargetEntitiesId[i].TargetReferenceName == TargetReferenceName.AFTER)
                                {
                                    if (newIndex == allEntities.Count)
                                        allEntities.Add(entToMove);
                                    else
                                        allEntities.Insert(newIndex + 1, entToMove);
                                }
                            }

                        }

                        UpdateEntitiesIndexes(action.EntityTypeKey);

                        //perform nested actions
                        foreach (ModelAction nestedAction in action.NestedActions)
                        {
                            if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                nestedAction.EntitiesId = action.EntitiesId;

                            if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                nestedAction.EntityTypeKey = action.EntityTypeKey;

                            CommitActionInternal(nestedAction, entitiesChangedId);
                        }

                        break;
                    }

                case ActionName.TREEENTITY_MOVE:
                case ActionName.TREEENTITY_MOVE_CHILD_OF:
                    {
                        //muove una lista di entità sopra o sotto il target
                        List<TreeEntity> allEntities = GetTreeEntitiesList(action.EntityTypeKey);

                        TreeEntityMove(action, entitiesChangedId, allEntities);
                        break;
                    }



                case ActionName.ATTRIBUTO_VALORECOLLECTION_ADD:
                    {
                        foreach (Entity ent in entities)
                        {

                            if (ent.Attributi.ContainsKey(action.AttributoCode))
                            {

                                EntityAttributo att = ent.Attributi[action.AttributoCode];

                                if (!att.Valore.IsMultiValore())
                                {
                                    if (att.Valore is ValoreCollection)
                                    {
                                        ValoreCollectionItem newValore = action.NewValore.Clone() as ValoreCollectionItem;

                                        //ValoreCollectionItemTemp newValore = new ValoreCollectionItemTemp() { Id = action.NewValoreCollectionItem.Id };   //aggiunge newValore a tutte le entità interessate (modifica multipla)
                                        (att.Valore as ValoreCollection).Add(newValore);
                                    }

                                }
                            }

                        }

                        break;
                    }
                case ActionName.ATTRIBUTO_VALORECOLLECTION_REMOVE:
                    {
                        foreach (Entity ent in entities)
                        {
                            //EntityAttributo att = ent.Attributi.FirstOrDefault(item => item.Attributo.Codice == action.AttributoCode);
                            EntityAttributo att = ent.Attributi[action.AttributoCode];

                            if (!att.Valore.IsMultiValore())
                            {
                                if (att.Valore is ValoreCollection)
                                {
                                    ValoreCollectionItem valCollectionItem = action.NewValore as ValoreCollectionItem;

                                    (att.Valore as ValoreCollection).Remove(valCollectionItem.Id);
                                }
                            }

                        }

                        break;
                    }
                case ActionName.ATTRIBUTO_VALORECOLLECTION_REPLACE:
                    {
                        foreach (Entity ent in entities)
                        {
                            //EntityAttributo att = ent.Attributi.FirstOrDefault(item => item.Attributo.Codice == action.AttributoCode);
                            EntityAttributo att = ent.Attributi[action.AttributoCode];

                            if (!att.Valore.IsMultiValore())
                            {
                                if (att.Valore is ValoreCollection)
                                {
                                    ValoreCollectionItem valCollectionItem = action.NewValore as ValoreCollectionItem;
                                    (att.Valore as ValoreCollection).Replace(valCollectionItem.Id, valCollectionItem);

                                }

                            }

                        }
                        break;
                    }

                case ActionName.ENTITIES_PASTE:
                    {
                        List<Entity> allEntities = GetEntitiesList(action.EntityTypeKey);

                        Guid targetId = Guid.Empty;
                        TargetReferenceName targetRefName = TargetReferenceName.NOT_DEFINED;

                        if (action.NewTargetEntitiesId.Any())
                        {
                            targetId = action.NewTargetEntitiesId[0].Id;
                            targetRefName = action.NewTargetEntitiesId[0].TargetReferenceName;
                        }

                        EntityCollection ents = new EntityCollection();
                        JsonSerializer.JsonDeserialize(action.JsonSerializedObject, out ents, ents.GetType());
                        ents.Entities.ForEach(item => item.EntityId = Guid.NewGuid());
                        ents.ResolveAllReferences(Project.EntityTypes);

                        int newIndex = allEntities.Count;//se non c'è target aggiunge in coda
                        if (targetId != Guid.Empty) //calcolo newIndex.
                        {
                            Entity ent = allEntities.FirstOrDefault(item => item.EntityId == targetId);
                            if (ent == null)
                            {
                                mar.ActionResponse = ActionResponse.FAILED;
                                break;
                            }

                            if (action.NewTargetEntitiesId[0].TargetReferenceName == TargetReferenceName.BEFORE)
                                newIndex = allEntities.IndexOf(ent);
                            else
                                newIndex = allEntities.IndexOf(ent) + 1;

                        }

                        if (newIndex < allEntities.Count)
                            allEntities.InsertRange(newIndex, ents.Entities);
                        else
                            allEntities.AddRange(ents.Entities);

                        mar.NewIds = new HashSet<Guid>(ents.Entities.Select(item => item.EntityId));


                        UpdateEntitiesIndexes(action.EntityTypeKey);

                        //perform nested actions
                        foreach (ModelAction nestedAction in action.NestedActions)
                        {
                            if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                nestedAction.EntitiesId = mar.NewIds;

                            if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                nestedAction.EntityTypeKey = action.EntityTypeKey;

                            CommitActionInternal(nestedAction, entitiesChangedId);
                        }

                        break;
                    }
                case ActionName.TREEENTITIES_PASTE:
                    {
                        List<TreeEntity> allEntities = GetTreeEntitiesList(action.EntityTypeKey);

                        IEnumerable<TreeEntity> allTreeEntities = allEntities.Cast<TreeEntity>();

                        Guid targetId = Guid.Empty;
                        TargetReferenceName targetRefName = TargetReferenceName.NOT_DEFINED;

                        if (action.NewTargetEntitiesId.Any())
                        {
                            targetId = action.NewTargetEntitiesId[0].Id;
                            targetRefName = action.NewTargetEntitiesId[0].TargetReferenceName;
                        }

                        TreeEntityCollection ents = new TreeEntityCollection();
                        JsonSerializer.JsonDeserialize(action.JsonSerializedObject, out ents, ents.GetType());
                        ents.TreeEntities.ForEach(item => item.EntityId = Guid.NewGuid());
                        if (!ents.TreeEntities.Any())
                        {
                            mar.ActionResponse = ActionResponse.FAILED;
                            return mar;
                        }
                        ents.ResolveAllReferences(Project.EntityTypes);

                        
                        //Calcolo degli attributi delle entità incollate
                        //ents.TreeEntities.ForEach(item =>
                        //{
                        //    foreach (EntityAttributo entAtt in item.Attributi.Values)
                        //    {
                        //        if (entAtt.Valore.HasValue())
                        //            Calculator.Calculate(item, entAtt.Attributo, entAtt.Valore);
                        //    }
                        //});



                        //Inserimento delle entità alla posizione giusta
                        TreeEntity target = null;
                        if (allTreeEntities.Any())
                            target = allTreeEntities.Where(item => item.Depth == 0).Last();

                        int newIndex = allTreeEntities.Count();//se non c'è target aggiunge in coda

                        if (targetId != Guid.Empty) //calcolo newIndex.
                        {
                            target = allTreeEntities.FirstOrDefault(item => item.EntityId == targetId);
                            if (target == null)
                            {
                                mar.ActionResponse = ActionResponse.FAILED;
                                break;
                            }



                            if (action.NewTargetEntitiesId[0].TargetReferenceName == TargetReferenceName.BEFORE)
                                newIndex = allEntities.IndexOf(target) + DescendantsCountOf(action.EntityTypeKey, target, out _);
                            else
                                newIndex = allEntities.IndexOf(target) + DescendantsCountOf(action.EntityTypeKey, target, out _) + 1;

                        }

                        if (newIndex < allEntities.Count)
                            allEntities.InsertRange(newIndex, ents.TreeEntities);
                        else
                            allEntities.AddRange(ents.TreeEntities);

                        List<TreeEntity> entsPackage = ents.TreeEntities;


                        if (targetRefName == TargetReferenceName.CHILD_OF && !target.CanBeParent())
                            break;

                        //Trasformo il target in padre
                        if (targetRefName == TargetReferenceName.CHILD_OF && !IsTreeEntityParent(target)/*target.IsParent()*/)
                        {
                            target.MakeParent();
                            entitiesChangedId.Add(target.EntityId);
                        }



                        //collego l'albero degli elementi che sto incollando
                        //if (targetRefName == TargetReferenceName.CHILD_OF)
                        //    entsPackage[0].MakeChildOf(target);
                        //else if (targetRefName == TargetReferenceName.AFTER || targetRefName == TargetReferenceName.BEFORE)
                        //    entsPackage[0].MakeSiblingOf(target, targetRefName == TargetReferenceName.BEFORE);


                        ////porto a depth 0 le radici
                        //int minDepth = entsPackage.Min(item => item.Depth);
                        //entsPackage.ForEach(item => item.Depth -= minDepth);


                        List<int> entsLevel0 = new List<int>();
                        for (int i = 0; i < entsPackage.Count; i++)
                        {
                            if (entsPackage[i].Depth <= entsPackage[0].Depth)//collego solo la radice (gli altri sono già collegati alla radice)
                                entsLevel0.Add(i);
                        }

                        //collego le radici dell'albero degli elementi che sto muovendo 
                        for (int i = 0; i < entsLevel0.Count; i++)
                        {
                            //int index = entsLevel0[i];
                            //int count = entsPackage.Count - entsLevel0[i] - 1;
                            //if (i < entsLevel0.Count - 1)
                            //    count = entsLevel0[i + 1] - entsLevel0[i] - 1;
                            int index = entsLevel0[i];
                            int count = entsPackage.Count - entsLevel0[i];
                            if (i < entsLevel0.Count - 1)
                                count = entsLevel0[i + 1] - entsLevel0[i];

                            if (targetRefName == TargetReferenceName.CHILD_OF)
                            {
                                MakeChildOf(entsPackage[index], target);
                                //entsPackage[i].MakeChildOf(target);
                                UpdateDescendantsDepthOf(entsPackage[index], target.Depth + 1, count);
                            }
                            else if (targetRefName == TargetReferenceName.AFTER || targetRefName == TargetReferenceName.BEFORE)
                            {
                                MakeSiblingOf(entsPackage[index], target, targetRefName == TargetReferenceName.BEFORE);
                                //entsPackage[i].MakeSiblingOf(target, action.NewTargetEntitiesId[i].TargetReferenceName == TargetReferenceName.BEFORE);
                                UpdateDescendantsDepthOf(entsPackage[index], target.Depth, count);
                            }
                        }


                        //for (int i = 0; i < entsPackage.Count; i++)
                        //{
                        //    if (entsPackage[i].Depth > 0)//collego solo le radici (gli altri sono già collegati alla radice)
                        //        continue;

                        //    //entsPackage[i].MakeSiblingOf(target);
                        //    if (targetRefName == TargetReferenceName.CHILD_OF)
                        //        MakeChildOf(entsPackage[i], target);
                        //        //entsPackage[i].MakeChildOf(target);
                        //    else if (targetRefName == TargetReferenceName.AFTER || targetRefName == TargetReferenceName.BEFORE)
                        //    {
                        //        MakeSiblingOf(entsPackage[i], target, targetRefName == TargetReferenceName.BEFORE);
                        //        //entsPackage[i].MakeSiblingOf(target, targetRefName == TargetReferenceName.BEFORE);
                        //        UpdateDescendantsDepthOf(entsPackage[i], target.Depth, entsPackage.Count);
                        //    }

                        //    targetRefName = TargetReferenceName.AFTER;
                        //    target = entsPackage[i];
                        //}



                        //Aggiunta tra le entità da ricalcolare
                        HashSet<Guid> newIds = new HashSet<Guid>(entsPackage.Select(item => item.EntityId));
                        entitiesChangedId.UnionWith(newIds);

                        UpdateEntitiesIndexes(action.EntityTypeKey);
                        
                        mar.NewIds = newIds;
                        
                        //perform nested actions
                        foreach (ModelAction nestedAction in action.NestedActions)
                        {
                            if (nestedAction.EntitiesId == null || !nestedAction.EntitiesId.Any())
                                nestedAction.EntitiesId = mar.NewIds;

                            if (nestedAction.EntityTypeKey == null || !nestedAction.EntityTypeKey.Any())
                                nestedAction.EntityTypeKey = action.EntityTypeKey;

                            CommitActionInternal(nestedAction, entitiesChangedId);
                        }

                        break;
                    }
                case ActionName.ATTRIBUTO_VALORE_REPLACEINTEXT:
                    {
                        //modifica del valore per ogni entità interessate
                        Attributo att = Project.EntityTypes[action.EntityTypeKey].Attributi[action.AttributoCode];
                        if (att.AllowReplaceInText)
                        {
                            foreach (Entity ent in entities)
                            {
                                //EntityAttributo entAtt = ent.Attributi.FirstOrDefault(item => item.Attributo.Codice == action.AttributoCode);
                                EntityAttributo entAtt = ent.Attributi[action.AttributoCode];

                                if (!entAtt.Valore.IsMultiValore())
                                {
                                    string oldTxt = action.OldValore.ToPlainText();
                                    string newTxt = action.NewValore.ToPlainText();
                                    entAtt.Valore.ReplaceInText(oldTxt, newTxt);
                                }
                            }
                        }

                        break;
                    }
                case ActionName.MULTI:
                case ActionName.MULTI_AND_CALCOLA:
                case ActionName.MULTI_NODEPENDENTS:
                    {
                        //perform nested actions
                        if (action.NewTargetEntitiesId.Any()) //esiste un riferimento (viene usato come punto di inserimento della prima entità)
                        {
                            TargetReference targetRef = new TargetReference() { Id = action.NewTargetEntitiesId[0].Id, TargetReferenceName = action.NewTargetEntitiesId[0].TargetReferenceName };

                            foreach (ModelAction nestedAction in action.NestedActions)
                            {
                                //inserisco in riferimento all'ultimo elemento inserito

                                ModelActionResponse res = null;
                                if (!nestedAction.NewTargetEntitiesId.Any() &&
                                    (nestedAction.ActionName == ActionName.ENTITY_INSERT ||
                                    nestedAction.ActionName == ActionName.TREEENTITY_INSERT))
                                {
                                    nestedAction.NewTargetEntitiesId.Add(targetRef);

                                    res = CommitActionInternal(nestedAction, entitiesChangedId);

                                    targetRef.Id = res.NewId;
                                    targetRef.TargetReferenceName = TargetReferenceName.AFTER;
                                }
                                else
                                {
                                    res = CommitActionInternal(nestedAction, entitiesChangedId);
                                }

                                if (res.ActionResponse == ActionResponse.OK)
                                {
                                    if (res.NewId != Guid.Empty)
                                        mar.NewIds.Add(res.NewId);
                                }

                            }
                        }
                        else  //perform nested actions
                        {

                            foreach (ModelAction nestedAction in action.NestedActions)
                            {
                                ModelActionResponse res = CommitActionInternal(nestedAction, entitiesChangedId);
                                if (res.ActionResponse == ActionResponse.OK)
                                    if (res.NewId != Guid.Empty)
                                        mar.NewIds.Add(res.NewId);
                            }
                        }

                        break;
                    }
            }

            return mar;
        }

        private static void IsDigicorpOwnerAction(ModelAction action, Entity ent)
        {
            if (ent.Attributi.ContainsKey(BuiltInCodes.Attributo.IsDigicorpOwner))
            {
                if (action.AttributoCode != BuiltInCodes.Attributo.Codice && action.AttributoCode != BuiltInCodes.Attributo.IsDigicorpOwner)
                {
                    ValoreBooleano valDigiCorpOwner = ent.Attributi[BuiltInCodes.Attributo.IsDigicorpOwner].Valore as ValoreBooleano;
                    if (valDigiCorpOwner.V == true)
                    {
                        ValoreTesto valCodice = ent.Attributi[BuiltInCodes.Attributo.Codice].Valore as ValoreTesto;
                        valCodice.V += "*";
                        valDigiCorpOwner.V = false;
                    }

                }
            }
        }

        public void TreeEntityDelete(ModelAction action, HashSet<Guid> entitiesChangedId, List<TreeEntity> allEntities)
        {

            HashSet<Guid> entsToRemove = new HashSet<Guid>(action.EntitiesId);
            List<TreeEntity> ents = allEntities.Where(item => action.EntitiesId.Contains(item.EntityId)).ToList();

            List<TreeEntity> allTreeEntities = GetTreeEntitiesList(action.EntityTypeKey);
            HashSet<Guid> entsDone = new HashSet<Guid>();

            ///////////////////////////////////////////////////////////////////
            //Raccolgo i padri di entsToRemove che non sono in entsToRemove
            List<Guid> entsToRemoveParentsIds = ents.Select(item => item.Parent)
                                                                  .Where(item => item != null && !entsToRemove.Contains(item.EntityId))
                                                                  .Select(item => item.EntityId)
                                                                  .ToList();
            ////////////////////////////////////////////////////////////////////


            foreach (TreeEntity ent in ents)//scorro le entità da rimuovere
            {
                if (entsDone.Contains(ent.EntityId))
                    continue;

                TreeEntity entToStart = ent;
                if (entToStart.Parent != null)
                    entToStart = entToStart.Parent;

                if (entToStart.IsParent)
                {
                    int index = allEntities.IndexOf(entToStart);
                    CanRemove(index, allTreeEntities, entsToRemove, entsDone);
                }
                else
                {
                    ent.Deleted = true;
                    MakeAlone(ent);
                    entsDone.Add(ent.EntityId);
                }
                entitiesChangedId.Add(ent.EntityId);
            }
            allEntities.RemoveAll(item => entsToRemove.Contains(item.EntityId));

            ////////////////////////////////////////////////////
            //Trasformo in foglia gli elementi del nuovo albero che non hanno figli mantenendo lo stesso id
            foreach (Guid parentId in entsToRemoveParentsIds)
            {
                TreeEntity parent = GetEntityById(action.EntityTypeKey, parentId) as TreeEntity;
                if (parent != null)
                {
                    if (!IsTreeEntityParent(parent))
                    {
                        if (parent.MakeLeaf())
                            entitiesChangedId.Add(parent.EntityId);
                    }

                    if (parent.IsParentDependsOnChild())
                        entitiesChangedId.Add(parent.EntityId);
                }
            }
            /////////////////////////////////////////////////////

            //per il ricalcolo delle entità riferite nella stessa sezione
            List<EntityAttributo> entsAtt = GetEntityAttributoGuidByEntityId(action.EntityTypeKey, entsToRemove);
            entsAtt.ForEach(item =>
            {
                //item.Valore = new ValoreGuid();

                if (item.Entity.EntityTypeCodice == action.EntityTypeKey)
                    entitiesChangedId.Add(item.Entity.EntityId);
            });

            //per il ricalcolo delle entità riferite nella stessa sezione
            entsAtt = GetEntityAttributoGuidCollectionByEntityId(action.EntityTypeKey, entsToRemove);
            entsAtt.ForEach(item =>
            {
                //var valGuidColl = item.Valore as ValoreGuidCollection;
                //valGuidColl.RemoveEntitiesId(entsToRemove);

                if (item.Entity.EntityTypeCodice == action.EntityTypeKey)
                    entitiesChangedId.Add(item.Entity.EntityId);
            });



            //ents = allEntities.Where(item => entsToRemove.Contains(item.EntityId)).Cast<TreeEntity>().ToList();
            UpdateEntitiesIndexes(action.EntityTypeKey, null, ents);
        }

        public void TreeEntityMove(ModelAction action, HashSet<Guid> entitiesChangedId, List<TreeEntity> allEntities)
        {
            List<TreeEntity> entsToMove = allEntities.Where(item => action.EntitiesId.Contains(item.EntityId)).ToList();

            TreeEntity target = allEntities.FirstOrDefault(item => item.EntityId == action.NewTargetEntitiesId[0].Id) as TreeEntity;
            TargetReferenceName targetReferenceName = action.NewTargetEntitiesId[0].TargetReferenceName;

            if (target == null)
                return;

            if (targetReferenceName == TargetReferenceName.CHILD_OF && !target.CanBeParent())
                return;

            ///////////////////////////////////////////////////////////////////
            //Raccolgo i padri di entsToMove che non sono in entsToMove
            HashSet<Guid> entsToMoveIds = new HashSet<Guid>(entsToMove.Select(item => item.EntityId));
            List<Guid> entsToMoveParentsIds = entsToMove.Select(item => item.Parent)
                                                                    .Where(item => item != null && !entsToMoveIds.Contains(item.EntityId))
                                                                    .Select(item => item.EntityId)
                                                                    .ToList();
            ////////////////////////////////////////////////////////////////////



            //Trasformo il target in padre
            if (targetReferenceName == TargetReferenceName.CHILD_OF && !IsTreeEntityParent(target)/*target.IsParent()*/)
            {
                target.MakeParent();
                entitiesChangedId.Add(target.EntityId);
            }



            TreeEntityCollection collection = new TreeEntityCollection();
            collection.TreeEntities = entsToMove;
            //collection.CompactToMove();
            CompactToMove(collection);

            List<TreeEntity> entsPackage = collection.TreeEntities;


            //elimino e aggiungo nella nuova posizione della lista
            allEntities.RemoveAll(item => entsPackage.Contains(item));


            int targetDescendantsCount = DescendantsCountOf(action.EntityTypeKey, target, out _);//target.DescendantsCount();

            int newIndex = allEntities.IndexOf(target);

            if (targetReferenceName == TargetReferenceName.BEFORE)
                allEntities.InsertRange(newIndex, entsPackage);
            else if (targetReferenceName == TargetReferenceName.AFTER || targetReferenceName == TargetReferenceName.CHILD_OF)
            {
                newIndex += targetDescendantsCount;

                //if (newIndex == allEntities.Count)
                //    allEntities.AddRange(entsPackage);
                //else
                //    allEntities.InsertRange(newIndex + 1, entsPackage);

                if (newIndex >= allEntities.Count - 1)
                    allEntities.AddRange(entsPackage);
                else
                    allEntities.InsertRange(newIndex + 1, entsPackage);

            }

            //collego l'albero degli elementi che sto muovendo 
            List<int> entsLevel0 = new List<int>();
            for (int i = 0; i < entsPackage.Count; i++)
            {
                if (entsPackage[i].Depth <= entsPackage[0].Depth)//collego solo la radice (gli altri sono già collegati alla radice)
                    entsLevel0.Add(i);
            }

            //collego le radici dell'albero degli elementi che sto muovendo 
            for (int i = 0; i < entsLevel0.Count; i++)
            {
                //int index = entsLevel0[i];
                //int count = entsPackage.Count - entsLevel0[i] - 1;
                //if (i < entsLevel0.Count - 1)
                //    count = entsLevel0[i + 1] - entsLevel0[i] - 1;

                int index = entsLevel0[i];
                int count = entsPackage.Count - entsLevel0[i];
                if (i < entsLevel0.Count - 1)
                    count = entsLevel0[i + 1] - entsLevel0[i];

                if (targetReferenceName == TargetReferenceName.CHILD_OF)
                {
                    MakeChildOf(entsPackage[index], target);
                    //entsPackage[i].MakeChildOf(target);
                    UpdateDescendantsDepthOf(entsPackage[index], target.Depth + 1, count);
                }
                else
                {
                    MakeSiblingOf(entsPackage[index], target, action.NewTargetEntitiesId[index].TargetReferenceName == TargetReferenceName.BEFORE);
                    //entsPackage[i].MakeSiblingOf(target, action.NewTargetEntitiesId[i].TargetReferenceName == TargetReferenceName.BEFORE);
                    UpdateDescendantsDepthOf(entsPackage[index], target.Depth, count);
                }
            }

            ////collego l'albero degli elementi che sto muovendo 
            //for (int i = 0; i < entsPackage.Count; i++)
            //{
            //    if (entsPackage[i].Depth > entsPackage[0].Depth)//collego solo la radice (gli altri sono già collegati alla radice)
            //        continue;

            //    if (targetReferenceName == TargetReferenceName.CHILD_OF)
            //        MakeChildOf(entsPackage[i], target);
            //    //entsPackage[i].MakeChildOf(target);
            //    else
            //    {
            //        MakeSiblingOf(entsPackage[i], target, action.NewTargetEntitiesId[i].TargetReferenceName == TargetReferenceName.BEFORE);
            //        //entsPackage[i].MakeSiblingOf(target, action.NewTargetEntitiesId[i].TargetReferenceName == TargetReferenceName.BEFORE);
            //    }
            //}

            ////////////////////////////////////////////////////
            //Trasformo in foglia gli elementi del nuovo albero che non hanno figli mantenendo lo stesso id

            //List<TreeEntity> entsToConvert = allEntities.Where(item => (item as TreeEntity).IsParent() && !(item as TreeEntity).HasChildren).Cast<TreeEntity>().ToList();
            //List<TreeEntity> entsToConvert = allEntities.Where(item =>
            //{
            //    TreeEntity treeEnt = item as TreeEntity;
            //    if (/*treeEnt.IsParent()*/IsTreeEntityParent(treeEnt) && GetChildrenCountOf(treeEnt.EntityTypeCodice, treeEnt) == 0)
            //        return true;
            //    return false;
            //}).Cast<TreeEntity>().ToList();
            //foreach (TreeEntity ent in entsToConvert)
            //{
            //    ent.MakeLeaf();
            //}
            foreach (Guid parentId in entsToMoveParentsIds)
            {
                TreeEntity parent = GetEntityById(action.EntityTypeKey, parentId) as TreeEntity;
                if (parent != null)
                {
                    if (!IsTreeEntityParent(parent))
                    {
                        if (parent.MakeLeaf())
                            entitiesChangedId.Add(parent.EntityId);
                    }
                    if (parent.IsParentDependsOnChild())
                        entitiesChangedId.Add(parent.EntityId);
                }
            }
            /////////////////////////////////////////////////////


            UpdateEntitiesIndexes(action.EntityTypeKey);

     
        }

        public Dictionary<string, EntityType> GetEntityTypes() { return Project.EntityTypes; }
        protected Dictionary<string, EntityType> EntityTypes
        {
            get => Project.EntityTypes;
        }

        protected Guid AddDivisione(string codice, string name = null, 
            Model3dClassEnum model3dClassName = Model3dClassEnum.Nothing, bool isBuiltIn = false)
        {

            //Controllo se esiste una divisione con lo stesso codice e nel caso esco
            if (codice != null && codice.Any())
            {
                EntityType divType = Project.EntityTypes.Values.FirstOrDefault(item => item.Codice == codice);
                if (divType != null)
                    return Guid.Empty;
            }


            Guid id = Guid.NewGuid();

            IEnumerable<DivisioneItemType> divTypes = Project.EntityTypes.Values.Where(item => item is DivisioneItemType).Cast<DivisioneItemType>();
            int maxPosition = -1;
            if (divTypes.Any())
                maxPosition = divTypes.Max(item => item.Position);

            //Aggiungo alla lista dei tipi
            DivisioneItemParentType divisioneParentType = new DivisioneItemParentType(id, codice, name, model3dClassName);
            DivisioneItemType divisioneType = new DivisioneItemType(id, codice, name, model3dClassName);
            divisioneType.AssociedType = divisioneParentType;
            divisioneType.Position = maxPosition + 1;
            divisioneParentType.AssociedType = divisioneType;

            if (isBuiltIn)
            {
                divisioneType.IsBuiltIn = true;
                divisioneParentType.IsBuiltIn = true;
            }

            divisioneParentType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);
            divisioneType.CreaAttributi(GetDefinizioniAttributo(), EntityTypes);


            AddEntityType(divisioneType);
            AddEntityType(divisioneParentType);

            //Aggiungo alle divisioni    
            if (!Project.DivisioniItems.ContainsKey(id))
                Project.DivisioniItems.Add(id, new List<TreeEntity>());

            string divisioneTypeKey = divisioneType.GetKey();

            UpdateEntitiesIndexes(divisioneTypeKey);

            Project.ViewSettings.EntityTypes.TryAdd(divisioneTypeKey, new EntityTypeViewSettings());
                
            return id;
        }

        protected Dictionary<string, DefinizioneAttributo> GetDefinizioniAttributo()
        {
            return Project.DefinizioniAttributo;
        }

        /// <summary>
        /// Ritorna le entità e tutti i padri
        /// </summary>
        /// <param name="entTypeCode"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        internal IEnumerable<TreeEntity> GetTreeEntitiesDeepById(string entTypeCode, IEnumerable<Guid> ids)
        {
            //TreeEntityCollection collection = new TreeEntityCollection();
            //collection.TreeEntities = GetTreeEntitiesList(entTypeCode);

            List<TreeEntity> treeEnts = new List<TreeEntity>();

            foreach (Guid id in ids)
            {
                if (EntitiesByGuid.ContainsKey(entTypeCode))
                {
                    if (EntitiesByGuid[entTypeCode].ContainsKey(id))
                    {
                        TreeEntity treeEntity = EntitiesByGuid[entTypeCode][id] as TreeEntity;

                        //Occorre fare resolve per aggiornare Parent prendendo da depth Rem by Ale 27/02/2023
                        //oss: forse si può anche togliere 
                        //
                        //collection.ResolveReferences(Project.EntityTypes, treeEntity);

                        TreeEntity item = treeEntity;
                        while (item != null)
                        {
                            treeEnts.Insert(0, item);
                            item = item.Parent;
                        }
                    }
                }
            }

            return treeEnts;
            //collection.ResolveAllReferences(Project.EntityTypes);//Aggiorna anche Parent prendendo da depth
            //return collection.TreeEntities;
        }

        /// <summary>
        /// Ritorna le entità passando gli Id
        /// </summary>
        /// <param name="entTypeCode"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Entity> GetEntitiesById(string entTypeCode, IEnumerable<Guid> ids)
        {
            EntityCollection collection = new EntityCollection();
            collection.Entities = new List<Entity>();
            foreach (Guid id in ids)
            {
                if (EntitiesByGuid[entTypeCode].ContainsKey(id))
                {
                    Entity ent = EntitiesByGuid[entTypeCode][id];
                    if (!ent.Deleted)
                        collection.Entities.Add(ent);
                }
            }
            return collection.Entities;
        }

        public bool IsEntityValid(string entityTypeKey, Guid entId)
        {
            if (!EntitiesByGuid.ContainsKey(entityTypeKey))
                return false;

            if (EntitiesByGuid[entityTypeKey].ContainsKey(entId))
                return true;

            return false;
        }


        protected bool SetEntityTypeInternal(EntityType entityTypeNew, bool forceUpdateEntities)
        {
            string key = entityTypeNew.GetKey();
            if (!Project.EntityTypes.ContainsKey(key))
                return false;

            EntitiesHelper entitiesHelper = new EntitiesHelper(this as ProjectService);


            Calculator.ResetExpressions();
            EntityAttributiUpdater entityAttributiUpdater = new EntityAttributiUpdater(this);


            //Costruisco un mappa per le etichette degli attributi modificate
            //Elimino gli attributi di riferimento che per qualche motivo non riferiscono
            List<string> attsCodiceToDelete = new List<string>();

            Dictionary<string, string> etichetteModificate = new Dictionary<string, string>();
            if (entityTypeNew.IsCustomizable())
            {
                foreach (string attKey in entityTypeNew.Attributi.Keys)
                {
                    Attributo newAtt = entityTypeNew.Attributi[attKey];

                    //Elimino gli attributi di riferimento che per qualche motivo non riferiscono
                    Attributo sourceAtt = entitiesHelper.GetSourceAttributo(newAtt);
                    if (sourceAtt == null)
                        attsCodiceToDelete.Add(newAtt.Codice);



                    //mappa etichette
                    if (Project.EntityTypes[key].Attributi.ContainsKey(attKey))
                    {
                        Attributo oldAtt = Project.EntityTypes[key].Attributi[attKey];
                        if (oldAtt.Etichetta != newAtt.Etichetta)
                        {
                            etichetteModificate.Add(oldAtt.Etichetta, newAtt.Etichetta);
                        }
                    }
                }
            }

            //Elimino gli attributi di riferimento che per qualche motivo non riferiscono
            foreach (string attCodice in attsCodiceToDelete)
            {
                entityTypeNew.Attributi.Remove(attCodice);
                entityTypeNew.AttributiMasterCodes.Remove(attCodice);
            }


            //ReplaceAttributiEtichette(entityTypeNew, etichetteModificate);
            EntitiesHelper.ReplaceAttributiEtichette(entityTypeNew, etichetteModificate);

            /////////////////////////////////////////////////////////////////////
            ///Cerco di capire se le entità sono da aggiornare
            bool updateEntities = forceUpdateEntities;
            EntityType entityTypeOld = Project.EntityTypes[key];

            if (!updateEntities)
                updateEntities = EntitiesHelper.AreEntitiesToUpdate(entityTypeOld, entityTypeNew, etichetteModificate);


            //Sostituzione della vecchia EntityType
            Project.EntityTypes[key] = entityTypeNew;
            entityTypeNew.ResolveReferences(EntityTypes, GetDefinizioniAttributo());


#region TreeEntityType
            //In caso entityTypeNew sia TreeEntityType devo modificare anche gli attributi codice e descrizione del parent
            List<TreeEntity> treeEntities = null;
            if (entityTypeNew is TreeEntityType)
            {
                TreeEntityType parentEntityTypeOld = (Project.EntityTypes[key] as TreeEntityType).AssociedType;
                if (parentEntityTypeOld != null)
                {
                    foreach (Attributo att in parentEntityTypeOld.Attributi.Values)
                    {
                        att.IsVisible = entityTypeNew.Attributi[att.Codice].IsVisible;
                        att.Etichetta = entityTypeNew.Attributi[att.Codice].Etichetta;
                        att.ValoreDefault = entityTypeNew.Attributi[att.Codice].ValoreDefault.Clone();
                        att.GroupName = entityTypeNew.Attributi[att.Codice].GroupName;
                        att.IsValoreLockedByDefault = entityTypeNew.Attributi[att.Codice].IsValoreLockedByDefault;
                    }
                }
                treeEntities = GetTreeEntitiesList(key);
            }
#endregion

            if (updateEntities && !entityTypeNew.IsParentType())
            {
                //update entities...
                //ValoreCalculatorFunction func = GetCalculatorFunction(key);
                //func?.ClearCalculatedValue();
                //Calculator.CurrentCalculatorFunction = func;
                //Calculator.ClearCalculatedValue();

                int entIndex = 0;
                IEnumerable<Entity> entities = GetEntities(key);
                //entities.ForEach(item =>
                foreach (var item in entities)
                {
                    string itemEntityTypeCodice = null;
                    TreeEntity treeItem = item as TreeEntity;
                    if (treeItem != null)
                    {
                        //if (IsTreeEntityParent(treeItem))
                        if (IsTreeEntityParent(treeEntities, entIndex))
                            itemEntityTypeCodice = treeItem.ParentEntityTypeCodice;
                        else
                            itemEntityTypeCodice = treeItem.EntityTypeCodice;
                    }
                    else
                    {
                        itemEntityTypeCodice = item.EntityTypeCodice;

                    }

                    EntityType itemEntityType = Project.EntityTypes[itemEntityTypeCodice];
                    


                    item.ResolveReferences(Project.EntityTypes);

                    item.Validate();

                    //Aggiungo i nuovi attributi
                    List<string> attsToAdd = itemEntityType.Attributi.Select(att => att.Key).Where(k => !item.Attributi.Keys.Contains(k)).ToList();
                    attsToAdd.ForEach(k => item.Attributi.Add(k, new EntityAttributo(item, itemEntityType.Attributi[k])));


                    //Elimino gli attributi che non devono più esistere
                    List<string> attsToRemove = item.Attributi.Select(att => att.Key)
                        .Where(k =>
                        {
                            if (!itemEntityType.Attributi.ContainsKey(k))
                                return true;
                            else if (itemEntityType.Attributi[k].DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                                return true;

                            return false;
                        })
                        .ToList();

                    attsToRemove.ForEach(k => item.Attributi.Remove(k));

                    //Aggiorno i valori
                    //
                    ValoreCalculatorFunction func = GetCalculatorFunction(key);
                    func?.ClearCurrentEntityCalculatedValue();
                    
                    foreach (string attCode in item.Attributi.Keys)
                    {
                        EntityAttributo entAtt = item.Attributi[attCode];

                        if (entityTypeOld.Attributi.ContainsKey(attCode) && entityTypeNew.Attributi[attCode].DefinizioneAttributoCodice != entityTypeOld.Attributi[attCode].DefinizioneAttributoCodice)
                        {
                            //è stata cambiata la definizione dell'attributo
                            string oldValStr = entAtt.Valore.PlainText;
                            entAtt.Valore = entityTypeNew.Attributi[attCode].ValoreDefault.Clone();
                            entAtt.Valore.SetByString(oldValStr);
                            if (entAtt.Attributo.ValoreAttributo != null)
                                entAtt.Attributo.ValoreAttributo.UpdateId(entAtt.Valore);
                            
                        }


                        //Attenzione! Non posso permettere di aggiornare GlobalId e ProjectGlobalId
                        if (itemEntityType.Attributi[attCode].IsValoreLockedByDefault && attCode != BuiltInCodes.Attributo.GlobalId && attCode != BuiltInCodes.Attributo.ProjectGlobalId/* && !entityTypeNew.Attributi[attCode].IsBuiltIn*/)//Lucchetto sull'attributo
                        {
                            entAtt.Valore = itemEntityType.Attributi[attCode].ValoreDefault.Clone();
                            Calculator.Calculate(item, entAtt.Attributo, entAtt.Valore);//per aggiornare Result dopo
                        }

                        EntitiesHelper.ReplaceEntityAttributiEtichette(itemEntityType, etichetteModificate, entAtt);

                    }

                    //Questa operazione sta tanto tempo...
                    //CalcolaEntityValues(item);
                    //}
                    entIndex++;
                }//);


                var calcOptions = new EntityCalcOptions() { ResetCalulatedValues = true };
                if (forceUpdateEntities)
                {
                    calcOptions.CalcolaAttributiResults = true;
                }


                EntitiesError entsError = new EntitiesError();
                EntityAttributiUpdater attributiUpdater = new EntityAttributiUpdater(this);
                attributiUpdater.CalcolaEntitiesValues(key, entities, calcOptions, entsError);

                EntitiesErrors.RemoveAll(item => item.EntityTypeKey == key);
                if (entsError.ActionErrorType != ActionErrorType.NOTHING)
                    EntitiesErrors.Add(entsError);

            }
            return true;
        }


        ///// <summary>
        ///// Aggiorno i valori degli attributi di un'entità di tipo Numero sostituendo nella formula eventuali funzioni att{Nome vecchio} con il att{Nome nuovo}
        ///// </summary>
        ///// <param name="entityTypeNew"></param>
        ///// <param name="etichetteModificate"></param>
        ///// <param name="entAtt"></param>
        //private static void ReplaceEntityAttributiEtichette(EntityType entityTypeNew, Dictionary<string, string> etichetteModificate, EntityAttributo entAtt)
        //{
            
        //    foreach (string oldEtichetta in etichetteModificate.Keys)
        //    {
        //        if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
        //        {
        //            ValoreReale vReale = entAtt.Valore as ValoreReale;
        //            if (vReale != null)
        //            {
        //                string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
        //                string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

        //                if (vReale.V != null)
        //                    vReale.V = vReale.V.Replace(oldFunction, newFunction);
        //            }
        //        }
        //        else if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
        //        {
        //            ValoreContabilita vCont = entAtt.Valore as ValoreContabilita;
        //            if (vCont != null)
        //            {
        //                string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
        //                string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

        //                if (vCont.V != null)
        //                    vCont.V = vCont.V.Replace(oldFunction, newFunction);

        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Aggiorno i valori di default degli attributi di tipo Numero sostituendo nella formula eventuali funzioni att{Nome vecchio} con il att{Nome nuovo}
        ///// </summary>
        ///// <param name="entityTypeNew"></param>
        ///// <param name="etichetteModificate"></param>
        //private void ReplaceAttributiEtichette(EntityType entityTypeNew, Dictionary<string, string> etichetteModificate)
        //{


        //    //Sostituzione Nome vecchio con nuovo nel valore di default di tipo numero
        //    foreach (string oldEtichetta in etichetteModificate.Keys)
        //    {
        //        string newEtichetta = etichetteModificate[oldEtichetta];
        //        foreach (Attributo att in entityTypeNew.Attributi.Values)
        //        {

        //            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
        //            {
        //                ValoreReale vReale = att.ValoreDefault as ValoreReale;
        //                if (vReale != null)
        //                {
        //                    string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
        //                    string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

        //                    if (vReale.HasValue())
        //                        vReale.V = vReale.V.Replace(oldFunction, newFunction);
        //                }
        //            }
        //            else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
        //            {
        //                ValoreContabilita vCont = att.ValoreDefault as ValoreContabilita;
        //                if (vCont != null)
        //                {
        //                    string oldFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, oldEtichetta);
        //                    string newFunction = CalculatorExpression.CreateFormula(entityTypeNew.FunctionName, etichetteModificate[oldEtichetta]);

        //                    if (vCont.HasValue())
        //                        vCont.V = vCont.V.Replace(oldFunction, newFunction);
        //                }
        //            }
        //        }
        //    }

        //}

        protected bool RemoveInvalidRiferimenti(EntityType entType)
        {
            bool anyRemoved = false;
            List<string> attsKey = entType.Attributi.Keys.ToList();

            EntitiesHelper entsHelper = new EntitiesHelper(this as ProjectService);

            //riassetto gli attributi AttributoRiferimento
            foreach (string attKey in attsKey)
            {

                //Attributo riferimento
                AttributoRiferimento attRif = entType.Attributi[attKey] as AttributoRiferimento;
                if (attRif != null)
                {
                    bool attRifValidated = false;

                    Attributo sourceAtt = entsHelper.GetSourceAttributo(entType.GetKey(), attKey);

                    if (sourceAtt != null)
                        attRifValidated = true;


                    //if (entType.Attributi.ContainsKey(attRif.ReferenceCodiceGuid))
                    //{
                    //    if (EntityTypes.ContainsKey(attRif.ReferenceEntityTypeKey))
                    //    {
                    //        if (EntityTypes[attRif.ReferenceEntityTypeKey].Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                    //        {
                    //            attRifValidated = true;
                    //        }
                    //    }                       
                    //}

                    if (!attRifValidated)
                    {
                        //Elimino l'attributo non più valido
                        entType.Attributi.Remove(attKey);
                        if (entType.AttributiMasterCodes.Contains(attKey))
                            entType.AttributiMasterCodes.Remove(attKey);

                        anyRemoved = true;
                    }
                }
                else if (entType.Attributi[attKey].DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Variabile)////Attributo variabile
                {
                    bool attVarValidated = false;
                    ValoreAttributoVariabili valAttVar = entType.Attributi[attKey].ValoreAttributo as ValoreAttributoVariabili;
                    if (valAttVar != null)
                    {
                        if (EntityTypes[BuiltInCodes.EntityType.Variabili].Attributi.ContainsKey(valAttVar.CodiceAttributo))
                            attVarValidated = true;
                    }

                    if (!attVarValidated)
                    {
                        //Elimino l'attributo non più valido
                        entType.Attributi.Remove(attKey);
                        if (entType.AttributiMasterCodes.Contains(attKey))
                            entType.AttributiMasterCodes.Remove(attKey);

                        anyRemoved = true;
                    }
                }
            }


            attsKey = entType.Attributi.Keys.ToList();

            //riassetto i guids
            foreach (string attKey in attsKey)
            {
                Attributo att = entType.Attributi[attKey];

                if (att.IsInternal)
                    continue;

                //riassetto i guids
                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    if (!EntityTypes.ContainsKey(att.GuidReferenceEntityTypeKey))
                    {
                        //Elimino l'attributo di tipo Guid
                        entType.Attributi.Remove(att.Codice);
                        anyRemoved = true;
                    }
                }
                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)//riassetto i guids
                {
                    if (!EntityTypes.ContainsKey(att.GuidReferenceEntityTypeKey))
                    {
                        //Elimino l'attributo di tipo GuidCollection
                        entType.Attributi.Remove(att.Codice);
                        anyRemoved = true;
                    }
                }
            }


            return anyRemoved;
        }

        public void CompactToMove(TreeEntityCollection treeEntCollection)
        {
            List<TreeEntity> ents = GetTreeEntitiesList(treeEntCollection.TreeEntities[0].EntityTypeCodice);

            List<TreeEntity> treeEntities = treeEntCollection.TreeEntities.Cast<TreeEntity>().ToList();

            TreeEntity firstEnt = treeEntities.FirstOrDefault();
            if (firstEnt == null)
                return;


            //memorizzo per ogni entità da muovere la lista dei padri
            Dictionary<TreeEntity, List<TreeEntity>> parents = new Dictionary<TreeEntity, List<TreeEntity>>();
            foreach (TreeEntity ent in treeEntities)
            {
                parents.Add(ent, new List<TreeEntity>());
                TreeEntity parent = ent.Parent;
                while (parent != null)
                {
                    parents[ent].Add(parent);
                    parent = parent.Parent;
                }
            }


            //Creo i nuovi collegamenti tra le entità che sto muovento
            List<TreeEntity> entsPackage = new List<TreeEntity>();
            TreeEntity lastDepth0 = null;
            foreach (TreeEntity ent in treeEntities)
            {
                //ent.MakeAlone();
                MakeAlone(ent);

                TreeEntity parent = null;
                foreach (TreeEntity par in parents[ent])
                {
                    if (entsPackage.Contains(par))
                    {
                        parent = par;
                        break;
                    }
                }

                if (parent != null)
                {
                    //ent.MakeChildOf(parent);
                    MakeChildOf(ent, parent);
                    //UpdateDescendantsDepthOf(ent, parent.Depth + 1, entsPackage.Count);
                }
                else
                {
                    if (entsPackage.Any())
                    {
                        MakeSiblingOf(ent, lastDepth0);
                        //ent.MakeSiblingOf(lastDepth0);
                        //UpdateDescendantsDepthOf(ent, lastDepth0.Depth, entsPackage.Count);
                    }

                    lastDepth0 = ent;
                }

                entsPackage.Add(ent);
            }

            UpdateDescendantsDepthOf(firstEnt, firstEnt.Depth, treeEntities.Count);

            treeEntCollection.TreeEntities = entsPackage;

        }

        /// <summary>
        /// Rimuove i riferimenti ad altre entità dell'albero
        /// update children, parent, depth
        /// </summary>
        public void MakeAlone(TreeEntity treeEnt)
        {
            if (treeEnt.Parent != null)
            {
                treeEnt.Parent.Children.Remove(treeEnt);
                treeEnt.Parent.Children.AddRange(treeEnt.Children);
            }

            foreach (TreeEntity ent in treeEnt.Children)
                ent.Parent = treeEnt.Parent;


            //foreach (TreeEntity child in treeEnt.Children)
            //    UpdateDescendantsDepthOf(child, treeEnt.Parent == null ? 0 : treeEnt.Parent.Depth + 1);
            //    //child.UpdateDescendantsDepth(treeEnt.Parent == null ? 0 : treeEnt.Parent.Depth + 1);


            treeEnt.Children.Clear();
            treeEnt.Parent = null;
            //treeEnt.Depth = 0;
        }

        /// <summary>
        /// Aggiunge un fratello subito sotto sibling 
        /// update children, parent, depth
        /// </summary>
        /// <param name="sibling"></param>
        public void MakeSiblingOf(TreeEntity treeEnt, TreeEntity sibling, bool before = false)
        {
            if (sibling == null)
                return;

            if (sibling.Parent != null && !sibling.Parent.Children.Contains(treeEnt)) //!IsChildOf(treeEnt, sibling.Parent))
            {
                int siblingIndex = sibling.Parent.Children.IndexOf(sibling);

                int newIndex = siblingIndex;
                if (!before)
                    newIndex++;

                if (newIndex < sibling.Parent.Children.Count)
                {
                    sibling.Parent.Children.Insert(newIndex, treeEnt);
                }
                else //se sibling è l'ultimo fratello
                {
                    sibling.Parent.Children.Add(treeEnt);
                }

            }



            //treeEnt.Depth = sibling.Depth;
            treeEnt.Parent = sibling.Parent;
            //treeEnt.UpdateDescendantsDepth(treeEnt.Depth);
            //UpdateDescendantsDepthOf(treeEnt, treeEnt.Depth);
            //UpdateDescendantsDepthOf(treeEnt, sibling.Depth);

        }

        bool IsChildOf(TreeEntity treeEnt, TreeEntity parent)
        {
            if (parent == null)
                return false;

            if (treeEnt == null)
                return false;

            List<TreeEntity> ents = GetTreeEntitiesList(treeEnt.EntityTypeCodice);
            int index = ents.IndexOf(parent);

            for (int i = index; i < ents.Count; i++)
            {
                if (ents[i].Depth <= parent.Depth)
                    return false;

                if (ents[i].Depth == parent.Depth+1 && ents[i] == treeEnt)
                    return true;
            }

            return false;

        }

        /// <summary>
        /// Aggiunge in coda ai figli di parent
        /// update children, parent, depth
        /// </summary>
        /// <param name="parent"></param>
        public void MakeChildOf(TreeEntity treeEnt, TreeEntity parent)
        {
            if (parent == null)
            {
                treeEnt.Parent = null;
                treeEnt.Depth = 0;
            }
            else if (!parent.Children.Contains(treeEnt))//()!IsChildOf(treeEnt, parent)
            {
                parent.Children.Add(treeEnt);
                treeEnt.Parent = parent;
                //treeEnt.Depth = parent.Depth + 1;
                //treeEnt.UpdateDescendantsDepth(treeEnt.Depth);
                //UpdateDescendantsDepthOf(treeEnt, parent.Depth + 1);


            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ent">Prima entità di cui aggrnare Depth</param>
        /// <param name="newCurrentDepth">Nuova depth per ent</param>
        /// <param name="count">numero di entità da aggiornare compreso ent</param>
        private void UpdateDescendantsDepthOf(TreeEntity ent, int newCurrentDepth, int count)
        {



            if (ent == null)
                return;

            List<TreeEntity> ents = GetTreeEntitiesList(ent.EntityTypeCodice);
            
            int oldEntDepth = ent.Depth;
            //ent.Depth = newCurrentDepth;
            
            int index = ents.IndexOf(ent);
            if (index < 0)
                return;

            //ent.Depth = newCurrentDepth;

            //if (index + 1 >= ents.Count)
            //    return;

            //int offset = ent.Depth - (ents[index + 1].Depth - 1);
            //int offset = ent.Depth - ents[index].Depth;
            int offset = newCurrentDepth - ents[index].Depth;

            //for (int i = index + 1; i < ents.Count; i++)
            //for (int i = index + 1; i <= index + count; i++)
            for (int i = index; i < index + count; i++)
            {

                if (i < ents.Count)
                    ents[i].Depth += offset;
            }

        }

        public Entity GetEntityById(string entTypeKey, Guid id)
        {
            if (EntitiesByGuid.ContainsKey(entTypeKey))
                if (EntitiesByGuid[entTypeKey].ContainsKey(id))
                {
                    return EntitiesByGuid[entTypeKey][id];
                }

            return null;
        }

        public Dictionary<string, Guid> CreateKey(string entityTypeKey, string separator, List<string> attributiCodice, out Dictionary<Guid, string> keysById)
        {
            IEnumerable<Entity> ents = GetEntities(entityTypeKey);

            Dictionary<string, Guid> idsByKey = new Dictionary<string, Guid>();

            keysById = new Dictionary<Guid, string>();

            if (attributiCodice.Contains(BuiltInCodes.Attributo.Id))
            {
             //...
            }
            else
            {

                foreach (Entity ent in ents)
                {
                    //bool exludeEnt = false;
                    //List<string> attsVal = new List<string>();
                    //foreach (string attributoCodice in attributiCodice)
                    //{
                    //    if (ent.Attributi.ContainsKey(attributoCodice))
                    //    {
                    //        Valore val = ent.Attributi[attributoCodice].Valore;
                    //        attsVal.Add(val.PlainText);
                    //    }
                    //    else
                    //        exludeEnt = true;
                    //}

                    //if (exludeEnt)
                    //    continue;

                    List<string> attsVal = new List<string>();
                    foreach (string attributoCodice in attributiCodice)
                    {
                        Valore val = EntitiesHelper.GetValoreAttributo(ent, attributoCodice, false, false);
                        if (val != null)
                        {
                            attsVal.Add(val.PlainText);
                        }
                        else
                            attsVal.Add(String.Empty);

                        //if (ent.Attributi.ContainsKey(attributoCodice))
                        //{
                        //    Valore val = ent.Attributi[attributoCodice].Valore;
                        //    attsVal.Add(val.PlainText);
                        //}
                        //else
                        //    attsVal.Add(String.Empty);
                    }

                    string key = string.Join(separator, attsVal);
                    if (!idsByKey.ContainsKey(key))
                        idsByKey.Add(key, ent.EntityId);

                    keysById.Add(ent.EntityId, key);
                }
            }
            return idsByKey;
        }

        public Dictionary<string, Guid> UpdateEntsGuidIndexedByKey(string entityTypeCode)
        {
            EntityType entType = GetEntityTypes()[entityTypeCode];
            if (entType == null)
                return null;

            EntityComparer entComparer = entType.EntityComparer;
            if (entComparer == null)
                return null;

            Dictionary<string, Guid> entsGuidByKey = CreateKey(entityTypeCode, entComparer.KeySeparator, entComparer.AttributiCode, out _);

            if (!EntitiesGuidByKey.ContainsKey(entityTypeCode))
            {
                EntitiesGuidByKey.Add(entityTypeCode, entsGuidByKey);
            }
            else
            {
                EntitiesGuidByKey[entityTypeCode] = entsGuidByKey;
            }

            return entsGuidByKey;
        }


        public Dictionary<string, Guid> GetEntitiesGuidByKey(string entityTypeCode)
        {
            if (!EntitiesGuidByKey.ContainsKey(entityTypeCode))
                return null;

            Dictionary<string, Guid> entsGuidByKey = EntitiesGuidByKey[entityTypeCode];
            return entsGuidByKey;

        }

        public void ClearEntitiesGuidByKey()
        {
            EntitiesGuidByKey.Clear();
        }

        /// <summary>
        /// Ritorna tutti gli attributi ValoreGuid che hanno come valore entityId di entityTypeKey
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public List<EntityAttributo> GetEntityAttributoGuidByEntityId(string entityTypeKey, HashSet<Guid> entitiesId)
        {
            Dictionary<string, EntityType> entTypes = GetEntityTypes();
            EntityAttributiUpdater entsUpdater = new EntityAttributiUpdater(this);
            List<string> entTypesKey = entsUpdater.GetDependentEntityTypesKey(entityTypeKey);
            entTypesKey.Insert(0, entityTypeKey);

            List<EntityAttributo> entAtts = new List<EntityAttributo>();
            foreach (string entTypeKey in entTypesKey)
            {
                IEnumerable<Entity> ents = GetEntities(entTypeKey);
                entAtts.AddRange(ents.SelectMany(item => item.Attributi.Values.Where(item1 => item1.Attributo.GuidReferenceEntityTypeKey == entityTypeKey && item1.Valore is ValoreGuid))
                                                        .Where(item => entitiesId.Contains((item.Valore as ValoreGuid).V)).ToList());


            }

            return entAtts;
        }

        /// <summary>
        /// Ritorna tutti gli attributi ValoreGuidCollection che hanno come valore entityId di entityTypeKey
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="entitiesId"></param>
        /// <returns></returns>
        public List<EntityAttributo> GetEntityAttributoGuidCollectionByEntityId(string entityTypeKey, HashSet<Guid> entitiesId)
        {
            Dictionary<string, EntityType> entTypes = GetEntityTypes();
            EntityAttributiUpdater entsUpdater = new EntityAttributiUpdater(this);
            List<string> entTypesKey = entsUpdater.GetDependentEntityTypesKey(entityTypeKey);
            entTypesKey.Insert(0, entityTypeKey);

            List<EntityAttributo> entAtts = new List<EntityAttributo>();
            foreach (string entTypeKey in entTypesKey)
            {
                IEnumerable<Entity> ents = GetEntities(entTypeKey);

                entAtts.AddRange(ents.SelectMany(item => item.Attributi.Values.Where(item =>
                {
                    if (item.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        var valGuidColl = item.Valore as ValoreGuidCollection;
                        var entsId = valGuidColl.GetEntitiesId();
                        foreach (var entId in entsId)
                            if (entitiesId.Contains(entId))
                                return true;


                    }
                    return false;
                })));

            }

            return entAtts;
        }

        /// <summary>
        /// Funzione ricorsiva
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="entitiesId"></param>
        /// <returns></returns>
        public void GetEntityAttributoGuidByEntityIdDeep(string entityTypeKey, HashSet<Guid> entitiesId, ref List<EntityAttributo> entsAtt)
        {
            if (!entitiesId.Any())
                return;

            List<EntityAttributo> entsAtt1 = GetEntityAttributoGuidByEntityId(entityTypeKey, entitiesId);

            entsAtt.AddRange(entsAtt1);

            HashSet<Guid> ids = entsAtt1.Select(item => item.Entity.EntityId).ToHashSet();

            GetEntityAttributoGuidByEntityIdDeep(entityTypeKey, ids, ref entsAtt);
        }

        protected string SerializeProject()
        {
            string str = string.Empty;
            try
            {

                MemoryStream msString = new MemoryStream();
                //Serializer.Serialize(msString, Project);
                ModelSerializer.Serialize(msString, Project);
                str = Convert.ToBase64String(msString.ToArray());
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }

            return str;

        }


        bool CanRemove(int index, List<TreeEntity> ents, HashSet<Guid> entsToRemove, HashSet<Guid> entsDone)
        {
            TreeEntity ent = ents[index] as TreeEntity;

            entsDone.Add(ent.EntityId);

            bool canRemove = true;
            if (ent.IsParent)
            {

                int childCount = 0;
                
                for (int i = index + 1; i < ents.Count; i++)
                {
                    TreeEntity child = ents[i] as TreeEntity;

                    if (child.Depth <= ent.Depth)
                        break;

                    if (child.Depth == ent.Depth + 1)
                    {
                        childCount++;
                        if (CanRemove(i, ents, entsToRemove, entsDone) == false)
                        {
                            entsToRemove.Remove(ent.EntityId);
                            return false;
                        }
                    }
                }
            }

            

            //if (canRemove)
            //{
                if (entsToRemove.Contains(ent.EntityId))
                {
                    ent.Deleted = true;
                    MakeAlone(ent);
                }
                else
                {
                    canRemove = false;
                    //if (ent.IsParent)
                    //    ent.MakeLeaf();
                }
            //}

            return canRemove;
        }

        internal ValoreCalculatorFunction GetCalculatorFunction(string entityTypeKey)
        {
            if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
                return _epCalculatorFunction;

            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
                return _cmpCalculatorFunction;

            if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
                return _elmCalculatorFunction;

            if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
                return _cntCalculatorFunction;

            if (entityTypeKey == BuiltInCodes.EntityType.Divisione)
                return _divCalculatorFunction;

            if (entityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
                return _eatCalculatorFunction;

            if (entityTypeKey == BuiltInCodes.EntityType.WBS)
                return _wbsCalculatorFunction;


            return null;
        }




    }
}
