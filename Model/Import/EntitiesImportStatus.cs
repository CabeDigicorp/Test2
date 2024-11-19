using MasterDetailModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class EntitiesImportStatus
    {

        public IDataService Source { get; set; } = null;
        public string SourceName { get; set; } = string.Empty;
        public List<EntityImportId> StartingEntitiesId { get; set; } = new List<EntityImportId>();
        public Dictionary<Guid, EntityImportStatus> EntitiesBySourceId { get; set; } = new Dictionary<Guid, EntityImportStatus>();
        public List<EntityImportId> PerformedEntitiesId { get; set; } = new List<EntityImportId>();
        public EntityImportStatusEnum Status { get; set; } = EntityImportStatusEnum.Undefined;
        public EntityImportWaitingInfo WaitingInfo { get; set; } = null;
        public EntityImportConflictAction ConflictAction { get; set; } = EntityImportConflictAction.Undefined;
        public HashSet<string> LimitedEntityTypes = null;//importazione limitata a queste entityTypes Key


        /// <summary>
        /// Key: Source entityTypeKey
        /// </summary>
        public Dictionary<string, EntityTypeImportStatus> EntityTypes { get; set; } = new Dictionary<string, EntityTypeImportStatus>();

        public Guid TargetId = Guid.Empty;
        public TargetPosition TargetPosition = TargetPosition.Undefined;

        /// <summary>
        /// Key: Target entityTypeKey
        /// </summary>
        public Dictionary<string, EntityComparer> CustomEntityComparers = new Dictionary<string, EntityComparer>(); //comparers custom

        /// <summary>
        /// Opzionale. Voci di target che si possono sovrascrivere (quelle che sono state selezionate per l'aggiornamento prezzi)
        /// </summary>
        public Dictionary<string, Guid> CustomTargetGuidsByKey { get; set; } = null;

        /// <summary>
        /// Opzionale. Tipo delle voci di target che si possono sovrascrivere
        /// </summary>
        public string CustomTargetEntityTypeKey { get; set; } = string.Empty;

    }

    public class EntityTypeImportStatus
    {
        public string SourceEntityTypeKey = string.Empty;
        public string TargetEntityTypeKey = string.Empty;
        public EntityImportConflictAction ConflictAction = EntityImportConflictAction.Undefined;
        


    }

    public class EntityImportStatus
    {
        public string SourceEntityTypeKey { get; set; } = string.Empty;
        public Guid SourceId { get; set; } = Guid.Empty;
        public string TargetEntityTypeKey { get; set; } = string.Empty;
        public string TargetEntityTypeName { get; set; } = string.Empty;
        public Guid TargetId { get; set; } = Guid.Empty;
        public EntityImportConflictAction ConflictAction { get; set; } = EntityImportConflictAction.Undefined;
        public EntityImportStatusEnum Status { get; set; } = EntityImportStatusEnum.Undefined;
        public Dictionary<string, Guid> TargetGuidsByKey { get; set; } = null;

    }

    public class EntityImportWaitingInfo
    {
        public Guid SourceId { get; set; } = Guid.Empty;
        public Guid TargetId { get; set; } = Guid.Empty;

        public string SourceEntityTypeName { get; set; } = string.Empty;
        public string TargetEntityTypeName { get; set; } = string.Empty;

        public string SourceEntityKey { get; set; } = string.Empty;
        public string TargetEntityKey { get; set; } = string.Empty;

        public EntityImportConflictAction ConflictAction { get; set; } = EntityImportConflictAction.Undefined;
    }

    public class EntityImportId
    {
        public string SourceEntityTypeKey { get; set; } = string.Empty;
        public Guid SourceId { get; set; } = Guid.Empty;
        public string TargetEntityTypeKey { get; set; } = string.Empty;
        public string TargetEntityTypeName { get; set; } = string.Empty;
        public Guid TargetId { get; set; } = Guid.Empty;
        public EntityImportConflictAction ConflictAction { get; set; } = EntityImportConflictAction.Undefined;
    }




    public enum EntityImportConflictAction
    {
        Undefined = 0,
        Ignore,
        Overwrite,
    }

    public enum EntityImportStatusEnum
    {
        Undefined = 0,
        Running,
        Completed,
        Waiting,
        Cancel,
    }

    public enum TargetPosition
    {
        Undefined = 0,
        TargetChildBottom,
        TargetChildTop,
        TargetAfter,
        Bottom,
        Top,
    }




}
