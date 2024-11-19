using _3DModelExchange;
using MasterDetailModel;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    public class Project
    {
        [ProtoMember(1)]
        public Dictionary<string, DefinizioneAttributo> DefinizioniAttributo { get; set; } = new Dictionary<string, DefinizioneAttributo>();

        //key: entType.GetKey()
        [ProtoMember(2)]
        public Dictionary<string, EntityType> EntityTypes { get; set; } = new Dictionary<string, EntityType>();

        //Tutte le entità non cancellate
        [ProtoMember(3)]
        public List<TreeEntity> PrezzarioItems { get; set; } = new List<TreeEntity>();

        [ProtoMember(4)]
        public List<Entity> ComputoItems { get; set; } = new List<Entity>();

        [ProtoMember(5)]
        public List<Entity> ElementiItems { get; set; } = new List<Entity>();

        /// <summary>
        /// Settaggi della vista (raggruppamenti, etc...)
        /// </summary>
        [ProtoMember(6)]
        public ViewSettings ViewSettings { get; set; } = new ViewSettings();

        /// <summary>
        /// Divisioni (key: id)
        /// </summary>
        [ProtoMember(7)]
        public Dictionary<Guid, List<TreeEntity>> DivisioniItems { get; set; } = new Dictionary<Guid, List<TreeEntity>>();

        /// <summary>
        /// Files Modello 3d
        /// </summary>
        [ProtoMember(8)]
        public Model3dFilesInfo Model3dFilesInfo { get; set; } = new Model3dFilesInfo();

        /// <summary>
        /// Filtri del modello 3d
        /// </summary>
        [ProtoMember(9)]
        public Model3dFiltersData Model3dFiltersData { get; set; } = new Model3dFiltersData();

        /// <summary>
        /// Valori provenienti dal modello 3d
        /// </summary>
        [ProtoMember(10)]
        public Model3dValuesData Model3dValuesData { get; set; } = new Model3dValuesData();


        [ProtoMember(11)]
        public List<Entity> ContattiItems { get; set; } = new List<Entity>();

        [ProtoMember(12)]
        public InfoProgettoItem InfoProgettoItem { get; set; } = null;

        [ProtoMember(13)]
        public List<TreeEntity> CapitoliItems { get; set; } = new List<TreeEntity>();

        /// <summary>
        /// Tags dei filtri del modello 3d
        /// </summary>
        [ProtoMember(14)]
        public Model3dTagsData Model3dTagsData { get; set; } = new Model3dTagsData();

        /// <summary>
        /// Preferenze al caricamento del modello ifc
        /// </summary>
        [ProtoMember(15)]
        public Model3dPreferencesData Model3dPreferencesData { get; set; } = new Model3dPreferencesData();

        ///// <summary>
        ///// Dati per le stampe (non viene utilizzato)
        ///// </summary>
        //[ProtoMember(16)]
        //public StampeData StampeData { get; set; } = new StampeData();

        /// <summary>
        /// Documenti
        /// </summary>
        [ProtoMember(17)]
        public List<TreeEntity> DocumentiItems { get; set; } = new List<TreeEntity>();

        /// <summary>
        /// Report
        /// </summary>
        [ProtoMember(18)]
        public List<Entity> ReportItems { get; set; } = new List<Entity>();

        /// <summary>
        /// Dati per gli stili
        /// </summary>
        [ProtoMember(19)]
        public List<Entity> StiliItems { get; set; } = new List<Entity>();

        /// <summary>
        /// Formati numerici
        /// </summary>
        [ProtoMember(20)]
        public List<NumericFormat> NumericFormats { get; set; } = new List<NumericFormat>();

        /// <summary>
        /// Preferenze al caricamento del modello ifc
        /// </summary>
        [ProtoMember(21)]
        public Model3dUserViewList Model3dUserViews { get; set; } = new Model3dUserViewList();

        /// <summary>
        /// Dati per gli ElencoAttivita
        /// </summary>

        [ProtoMember(22)]
        public List<Entity> ElencoAttivitaItems { get; set; } = new List<Entity>();

        /// <summary>
        /// Dati per WBS
        /// </summary>
        [ProtoMember(23)]
        public List<TreeEntity> WBSItems { get; set; } = new List<TreeEntity>();

        /// <summary>
        /// Rototraslazione di ogni file ifc del modello 3d
        /// </summary>
        [ProtoMember(24)]
        public Model3dUserRotoTranslation Model3dUserRotoTranslation { get; set; } = new Model3dUserRotoTranslation();

        /// <summary>
        /// Gantt
        /// </summary>
        [ProtoMember(25)]
        public GanttData Gantt { get; set; } = new GanttData();

        /// <summary>
        /// Sezioni calendari
        /// </summary>
        [ProtoMember(26)]
        public List<Entity> CalendariItems { get; set; } = new List<Entity>();

        /// <summary>
        /// Dati per la creazione automatica della wbs
        /// </summary>
        [ProtoMember(27)]
        public WBSItemsCreationData WBSItemsCreationData { get; set; } = new WBSItemsCreationData();


        /// <summary>
        /// FogliDiCalcolo
        /// </summary>
        [ProtoMember(28)]
        public FogliDiCalcoloData FogliDiCalcolo { get; set; } = new FogliDiCalcoloData();

        /// <summary>
        /// Sezione variabili
        /// </summary>
        [ProtoMember(29)]
        public VariabiliItem VariabiliItem { get; set; } = null;

        /// <summary>
        /// Sezione Allegati
        /// </summary>
        [ProtoMember(30)]
        public List<Entity> AllegatiItems { get; set; } = new List<Entity>();
        /// <summary>
        /// 
        /// Sezione Tag
        /// </summary>
        [ProtoMember(31)]
        public List<Entity> TagItems { get; set; } = new List<Entity>();

        //[ProtoMember(32)] //già utilizzato
    }
}
