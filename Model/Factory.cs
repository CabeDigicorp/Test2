

using MasterDetailModel;
using System;
using System.Collections.Generic;

namespace Model
{

    public class Factory
    {
        ProjectServiceBase _projectService;

        public Factory(ProjectServiceBase projectService)
        {
            _projectService = projectService;
        }

        
        //Dictionary<string, EntityType> _entityTypes;
        //Dictionary<string, DefinizioneAttributo> _defAttributi;
        //Dictionary<string, List<Entity>> _entitiesDictionary;

        public Entity NewEntity(string entityTypeCode)
        {

            EntityType entType = _projectService.GetEntityTypes()[entityTypeCode];

            if (entType is DivisioneItemType)
            {
                DivisioneItemType divisioneItemType = entType as DivisioneItemType;
                return NewDivisioneItem(divisioneItemType.DivisioneId);
            }
            if (entType is DivisioneItemParentType)
            {
                DivisioneItemParentType divisioneItemParentType = entType as DivisioneItemParentType;
                return NewDivisioneItem(divisioneItemParentType.DivisioneId);
            }
            else if (entType is PrezzarioItemType || entType is PrezzarioItemParentType)
            {
                return NewPrezzarioItem();
            }
            else if (entType is CapitoliItemType || entType is CapitoliItemParentType)
            {
                return NewCapitoliItem();
            }
            else if (entType is ComputoItemType)
            {
                return NewComputoItem();
            }
            else if (entType is ElementiItemType)
            {
                return NewElementiItem();
            }
            else if (entType is ContattiItemType)
            {
                return NewContattiItem();
            }
            else if (entType is InfoProgettoItemType)
            {
                return NewInfoProgettoItem();
            }
            else if (entType is DocumentiItemType || entType is DocumentiItemParentType)
            {
                return NewDocumentiItem();
            }
            else if (entType is ReportItemType)
            {
                return NewReportItem();
            }
            else if (entType is StiliItemType)
            {
                return NewStiliItem();
            }
            else if (entType is ElencoAttivitaItemType)
            {
                return NewAttivitaItem();
            }
            else if (entType is WBSItemType || entType is WBSItemParentType)
            {
                return NewWBSItem();
            }
            else if (entType is CalendariItemType)
            {
                return NewCalendariItem();
            }
            else if (entType is AllegatiItemType)
            {
                return NewAllegatiItem();
            }
            else if (entType is TagItemType)
            {
                return NewTagItem();
            }

            return null;
            //switch (entityTypeCode)
            //{
            //    case BuiltInCodes.EntityType.Prezzario:
            //    case "PrezzarioItemParent":
            //        return NewPrezzarioItem();
            //    case BuiltInCodes.EntityType.Computo:
            //        return NewComputoItem();
            //    default:
            //        return null;
            //}
            
        }

        public PrezzarioItem NewPrezzarioItem()
        {
            PrezzarioItem art = new PrezzarioItem();
            art.ResolveReferences(_projectService.GetEntityTypes());
            return art;
        }

        public CapitoliItem NewCapitoliItem()
        {
            CapitoliItem cap = new CapitoliItem();
            cap.ResolveReferences(_projectService.GetEntityTypes());
            return cap;
        }

        public ComputoItem NewComputoItem()
        {
            ComputoItem compItem = new ComputoItem();
            compItem.ResolveReferences(_projectService.GetEntityTypes());
            return compItem;
        }

        public DivisioneItem NewDivisioneItem(Guid divTypeId)
        {
            DivisioneItem div = new DivisioneItem(divTypeId);
            div.ResolveReferences(_projectService.GetEntityTypes());
            return div;
        }

        public ElementiItem NewElementiItem()
        {
            ElementiItem elItem = new ElementiItem();
            elItem.ResolveReferences(_projectService.GetEntityTypes());
            return elItem;
        }

        public ContattiItem NewContattiItem()
        {
            ContattiItem cntItem = new ContattiItem();
            cntItem.ResolveReferences(_projectService.GetEntityTypes());
            return cntItem;
        }

        public InfoProgettoItem NewInfoProgettoItem()
        {
            InfoProgettoItem infoItem = new InfoProgettoItem();
            infoItem.ResolveReferences(_projectService.GetEntityTypes());
            return infoItem;
        }

        public DocumentiItem NewDocumentiItem()
        {
            DocumentiItem infoItem = new DocumentiItem();
            infoItem.ResolveReferences(_projectService.GetEntityTypes());
            return infoItem;
        }

        public ReportItem NewReportItem()
        {
            ReportItem infoItem = new ReportItem();
            infoItem.ResolveReferences(_projectService.GetEntityTypes());
            return infoItem;
        }
        public StiliItem NewStiliItem()
        {
            StiliItem stileItem = new StiliItem();
            stileItem.ResolveReferences(_projectService.GetEntityTypes());
            return stileItem;
        }
        public ElencoAttivitaItem NewAttivitaItem()
        {
            ElencoAttivitaItem cntItem = new ElencoAttivitaItem();
            cntItem.ResolveReferences(_projectService.GetEntityTypes());
            return cntItem;
        }

        public WBSItem NewWBSItem()
        {
            WBSItem wbs = new WBSItem();
            wbs.ResolveReferences(_projectService.GetEntityTypes());
            return wbs;
        }

        public CalendariItem NewCalendariItem()
        {
            CalendariItem calItem = new CalendariItem();
            calItem.ResolveReferences(_projectService.GetEntityTypes());
            return calItem;
        }

        public VariabiliItem NewVariabiliItem()
        {
            VariabiliItem varItem = new VariabiliItem();
            varItem.ResolveReferences(_projectService.GetEntityTypes());
            return varItem;
        }

        public AllegatiItem NewAllegatiItem()
        {
            AllegatiItem allItem = new AllegatiItem();
            allItem.ResolveReferences(_projectService.GetEntityTypes());
            return allItem;
        }

        public TagItem NewTagItem()
        {
            TagItem tagItem = new TagItem();
            tagItem.ResolveReferences(_projectService.GetEntityTypes());
            return tagItem;
        }
    }

}