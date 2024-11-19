
using ProtoBuf;


namespace ModelData.Model
{
    [ProtoContract]
    public class ViewSettings
    {
        /// <summary>
        /// key: EntityTypeKey
        /// </summary>
        [ProtoMember(3)]
        public Dictionary<string, EntityTypeViewSettings> EntityTypes { get; set; } = new Dictionary<string, EntityTypeViewSettings>();

        [ProtoMember(4)]
        public Dictionary<string, GridSettings> GridsSettings = new Dictionary<string, GridSettings>();
    }

    [ProtoContract]
    public class EntityTypeViewSettings
    {
        [ProtoMember(1)]
        string EntityTypeKey { get; set; }

        /// <summary>
        /// Filtri
        /// </summary>
        [ProtoMember(2)]
        public List<AttributoFilterData> Filters { get; set; } = new List<AttributoFilterData>();


        /// <summary>
        /// Ordinamenti
        /// </summary>
        [ProtoMember(3)]
        public List<AttributoSortData> Sorts { get; set; } = new List<AttributoSortData>();


        /// <summary>
        /// Raggruppamenti
        /// </summary>
        [ProtoMember(4)]
        public List<AttributoGroupData> Groups { get; set; } = new List<AttributoGroupData>();

        /// <summary>
        /// Ordine degli attributi nel Master (colonne della griglia)
        /// </summary>
        [ProtoMember(5)]
        public List<string> OrderedMasterAttributiCode { get; set; } = new List<string>();

        /// <summary>
        /// Settaggi della griglia (ampiezza delle colonne,...)
        /// </summary>
        [ProtoMember(6)]
        public GridViewSettings GridViewSettings { get; set; } = new GridViewSettings();

        /// <summary>
        /// Settaggi per la ricodifica di attributi di tipo testo nella sezione
        /// </summary>
        [ProtoMember(7)]
        public List<AttributoCoding> Codings { get; set; } = new List<AttributoCoding>();

        /// <summary>
        /// Dati per gli evidenziatori di entità
        /// </summary>
        [ProtoMember(8)]
        public EntityHighlighters EntityHighlighters { get; set; } = new EntityHighlighters();

    }

    /// <summary>
    /// Settaggi della griglia (ampiezza delle colonne,...)
    /// </summary>
    [ProtoContract]
    public class GridViewSettings
    {
        /// <summary>
        /// key: codice attributo
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<string, GridColumnViewSettings> ColumnsViewSettings { get; set; } = new Dictionary<string, GridColumnViewSettings>();
    }

    /// <summary>
    /// Settaggi di una colonna della griglia (ampiezza,...)
    /// </summary>
    [ProtoContract]
    public class GridColumnViewSettings
    {

        [ProtoMember(1)]
        public string Codice = string.Empty;

        /// <summary>
        /// nota bene!!! L'evento di resize della colonna esiste nella griglia dalla versione Syncfusion.SfGrid.WPF: 18.2460.0.44
        /// </summary>
        [ProtoMember(2)]
        public double Width { get; set; } = 60;

    }

    [ProtoContract]
    public class GridSettings
    {
        /// <summary>
        ///Settaggi della colonna (Key: codice colonna)
        /// </summary>
        [ProtoMember(1)]
        public Dictionary<string, GridColumnViewSettings> ColumnsViewSettings { get; set; }

    }
}
