using ProtoBuf;

namespace ModelData.Model
{

    /// <summary>
    /// Classe per la codifica di un attributo di codice AttributoCodice nell'entità con chiave EntityTypeKey
    /// </summary>
    [ProtoContract]
    public class AttributoCoding
    {
        [ProtoMember(1)]
        public string EntityTypeKey { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string AttributoCodice { get; set; } = string.Empty;

        [ProtoMember(3)]
        public List<AttributoLevelCoding> LevelsCoding = new List<AttributoLevelCoding>();

        [ProtoMember(4)]
        public int PosizionamentoRispettoCodiceEsistente { get; set; }
    }

    /// <summary>
    /// Classe per la codifica di un livello 
    /// </summary>
    [ProtoContract]
    public class AttributoLevelCoding
    {
        [ProtoMember(1)]
        public int Level { get; set; } = 0;

        [ProtoMember(2)]
        public bool IsCoding { get; set; } = false;

        [ProtoMember(3)]
        public string Prefix { get; set; } = string.Empty;

        [ProtoMember(4)]
        public string IncrementalValue { get; set; } = string.Empty;

        [ProtoMember(5)]
        public string Suffix { get; set; } = string.Empty;

        [ProtoMember(6)]
        public int Step { get; set; } = 1;

        [ProtoMember(7)]
        public bool AddHigherLevel { get; set; } = false;


    }

}