


using ProtoBuf;


namespace ModelData.Model
{


    [ProtoContract]
    public class EntityAttributo
    {
        //[ProtoMember(1)]
        //public Guid Id;

        [ProtoMember(2)]
        public string AttributoCodice { get; set; }

        [ProtoMember(3)]
        public Valore Valore { get; set; }

    }

}