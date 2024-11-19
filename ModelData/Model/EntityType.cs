using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public /*abstract*/ class EntityType
    {

        [ProtoMember(1)]
        public string Codice { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(4)]
        public Dictionary<string, Attributo> Attributi { get; set; } = new Dictionary<string, Attributo>();

        [ProtoMember(5)]
        public List<string> AttributiMasterCodes { get; set; } = new List<string>();

        [ProtoMember(6)]
        public string FunctionName { get; set; }

        [ProtoMember(7)]
        public double DetailAttributoEtichettaWidth { get; set; } = 150;

        [ProtoMember(8)]
        public int UserNewAttributoCodice { get; set; } = 0;

    }

    public enum MasterType
    {
        Nothing = 0,
        NoMaster, //ex info
        List, //ex contatti
        Tree, //ex elenco prezzi
        Grid, //ex computo
    }


}
