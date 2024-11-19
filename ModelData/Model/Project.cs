
using ModelData.Model;
using ProtoBuf;


namespace ModelData.Model
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
        public List<PrezzarioItem> PrezzarioItems { get; set; } = new List<PrezzarioItem>();

        [ProtoMember(4)]
        public List<ComputoItem> ComputoItems { get; set; } = new List<ComputoItem>();

        [ProtoMember(5)]
        public List<ElementiItem> ElementiItems { get; set; } = new List<ElementiItem>();

        /// <summary>
        /// Settaggi della vista (raggruppamenti, etc...)
        /// </summary>
        [ProtoMember(6)]
        public ViewSettings ViewSettings { get; set; } = new ViewSettings();

        /// <summary>
        /// Divisioni (key: id)
        /// </summary>
        [ProtoMember(7)]
        public Dictionary<Guid, List<DivisioneItem>> DivisioniItems { get; set; } = new Dictionary<Guid, List<DivisioneItem>>();

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
        public List<ContattiItem> ContattiItems { get; set; } = new List<ContattiItem>();

        [ProtoMember(12)]
        public InfoProgettoItem InfoProgettoItem { get; set; } = null;

        [ProtoMember(13)]
        public List<CapitoliItem> CapitoliItems { get; set; } = new List<CapitoliItem>();

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

        /// <summary>
        /// Documenti
        /// </summary>
        [ProtoMember(17)]
        public List<DocumentiItem> DocumentiItems { get; set; } = new List<DocumentiItem>();

        /// <summary>
        /// Report
        /// </summary>
        [ProtoMember(18)]
        public List<ReportItem> ReportItems { get; set; } = new List<ReportItem>();

        /// <summary>
        /// Dati per gli stili
        /// </summary>
        [ProtoMember(19)]
        public List<StiliItem> StiliItems { get; set; } = new List<StiliItem>();

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
        public List<ElencoAttivitaItem> ElencoAttivitaItems { get; set; } = new List<ElencoAttivitaItem>();

        /// <summary>
        /// Dati per WBS
        /// </summary>
        [ProtoMember(23)]
        public List<WBSItem> WBSItems { get; set; } = new List<WBSItem>();

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
        public List<CalendariItem> CalendariItems { get; set; } = new List<CalendariItem>();

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
        public List<AllegatiItem> AllegatiItems { get; set; } = new List<AllegatiItem>();

        /// Sezione Tag
        /// </summary>
        [ProtoMember(31)]
        public List<TagItem> TagItems { get; set; } = new List<TagItem>();

    }
}
