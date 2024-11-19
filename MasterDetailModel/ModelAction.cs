using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{

    public enum ActionName
    {
        NOT_DEFINED = 0,
        ATTRIBUTO_VALORE_MODIFY,
        ENTITY_DELETE,
        ENTITY_ADD,
        ENTITY_INSERT,
        ENTITY_MOVE,
        ATTRIBUTO_VALORECOLLECTION_REPLACE,
        ATTRIBUTO_VALORECOLLECTION_ADD,
        ATTRIBUTO_VALORECOLLECTION_REMOVE,
        TREEENTITY_DELETE,
        TREEENTITY_ADD,
        TREEENTITY_ADD_PARENT,
        TREEENTITY_ADD_CHILD,
        TREEENTITY_INSERT,
        TREEENTITY_MOVE,
        TREEENTITY_MOVE_CHILD_OF,
        ENTITIES_PASTE,
        TREEENTITIES_PASTE,
        ATTRIBUTO_VALORE_REPLACEINTEXT,
        MULTI,
        ENTITYTYPE_SET,
        NUMERICFORMAT_COMMIT,
        DIVISIONE_ADD,
        DIVISIONE_REMOVE,
        DIVISIONE_RENAME,
        ENTITIES_IMPORT,
        VIEW_SETTINGS,
        CREATE_WBS_ITEMS,
        CALCOLA_ENTITES,
        SAVE_GANTTDATA,
        MULTI_AND_CALCOLA,//lancia il ricalcolo mediante la funzione CalcolaEntities
        UNDOGROUP,
        SAVE_FOGLIDICALCOLODATA,
        DIVISIONI_SORT,
        SETMODEL3D_FILTERDATA,
        SETMODEL3D_FILEINFO,
        SETMODEL3D_USERROTOTRANSLATION,
        SETMODEL3D_TAGSDATA,
        SETMODEL3D_USERVIEWS,
        SETMODEL3D_PREFERENCESDATA,
        MULTI_NODEPENDENTS,
    }

    public enum ActionResponse
    {
        NOT_DEFINED = 0,
        OK = 1,
        FAILED = 2,
    }

    public enum TargetReferenceName
    {
        NOT_DEFINED = 0,
        AFTER,
        BEFORE,
        CHILD_OF,
    }


    public class TargetReference
    {
        public Guid Id { get; set; } = Guid.Empty;
        public TargetReferenceName TargetReferenceName { get; set; } = TargetReferenceName.NOT_DEFINED;
    }


    public class ModelAction
    {
        public ActionName ActionName { get; set; } = ActionName.NOT_DEFINED;
        public string EntityTypeKey { get; set; } = null;
        public string AttributoCode { get; set; } = "";
        public HashSet<Guid> EntitiesId { get; set; } = new HashSet<Guid>();
        public Valore OldValore { get; set; } = null;//(per replace in text)
        public Valore NewValore { get; set; } = null;
        public List<TargetReference> NewTargetEntitiesId { get; set; } = new List<TargetReference>();
        public string JsonSerializedObject { get; set; }
        public Type JsonSerializedObjectType { get; set; }//Type della classe serializzata 
        public List<ModelAction> NestedActions { get; set; } = new List<ModelAction>();
    }

    public class ModelActionResponse
    {
        public ModelAction ModelAction { get; set; }
        public ActionResponse ActionResponse { get; set; }
        public HashSet<Guid> NewIds { get; set; } = new HashSet<Guid>();
        public Guid NewId { get; set; }
        public HashSet<Guid> ChangedEntitiesId { get; set; } = new HashSet<Guid>();
        public EntitiesError EntitiesError { get; set;} = null;

    }

    public class EntitiesError
    {
        public string EntityTypeKey { get; set; } = string.Empty;
        public HashSet<Guid> Ids { get; set; } = new HashSet<Guid>();
        public ActionErrorType ActionErrorType = ActionErrorType.NOTHING;

        public void Clear()
        {
            EntityTypeKey = string.Empty;
            Ids.Clear();
            ActionErrorType = ActionErrorType.NOTHING;
        }
    }

    public enum ActionErrorType
    {
        NOTHING = 0,
        LOOP_REFERENCE = 1,
    }

}
