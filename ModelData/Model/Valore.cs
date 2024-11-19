
using ProtoBuf;



namespace ModelData.Model
{
    [ProtoContract]
    [ProtoInclude(1001, typeof(ValoreTesto))]
    [ProtoInclude(1002, typeof(ValoreTestoRtf))]
    [ProtoInclude(1003, typeof(ValoreContabilita))]
    [ProtoInclude(1004, typeof(ValoreReale))]
    [ProtoInclude(1006, typeof(ValoreCollection))]
    [ProtoInclude(1007, typeof(ValoreData))]
    [ProtoInclude(1008, typeof(ValoreGuid))]
    [ProtoInclude(1009, typeof(ValoreBooleano))]
    [ProtoInclude(1010, typeof(ValoreElenco))]
    [ProtoInclude(1011, typeof(ValoreColore))]
    [ProtoInclude(1012, typeof(ValoreFormatoNumero))]
    public interface Valore
    {
    }


    public interface ValoreSingle : Valore
    {
    }


    [ProtoContract]
    public class ValoreTesto : ValoreSingle
    {

        [ProtoMember(1)]
        public string V { get; set; }


        [ProtoMember(2)]
        public string Result { get; set; }



    }

    [ProtoContract]
    public class ValoreData : ValoreSingle
    {

        [ProtoMember(1)]
        public DateTime? V { get; set; }

    }


    [ProtoContract]
    public class ValoreCollection : Valore
    {
        [ProtoMember(1)]
        public List<ValoreCollectionItem> V { get; set; } = new List<ValoreCollectionItem>();


    }

    [ProtoContract]
    public class ValoreTestoCollection : ValoreCollection
    {
    }

    [ProtoContract]
    public class ValoreGuidCollection : ValoreCollection
    {
        [ProtoMember(2)]
        public FilterData Filter { get; set; } = new FilterData();

    }


    [ProtoContract]
    public class ValoreTestoRtf : ValoreSingle
    {
        [ProtoMember(1)]
        public string V { get; set; }

      
    }


    [ProtoContract]
    public class ValoreReale : ValoreSingle
    {
        [ProtoMember(1)]
        public string V { get; set; }

        [ProtoMember(2)]
        public double? RealResult { get; set; }

        [ProtoMember(3)]
        public string Format { get; set; }

        [ProtoMember(4)]
        public string ResultDescription { get; set; }

 
    }

    [ProtoContract]
    public class ValoreContabilita : ValoreSingle
    {


        [ProtoMember(1)]
        public string V { get; set; }


        [ProtoMember(2)]
        public decimal? RealResult { get; set; }


        [ProtoMember(3)]
        public string Format { get; set; }

        [ProtoMember(4)]
        public string ResultDescription { get; set; }

 
    }

    [ProtoContract]
    public class ValoreGuid : ValoreSingle
    {
        [ProtoMember(1)]
        public Guid V { get; set; } = Guid.Empty;

    }

    [ProtoContract]
    public class ValoreBooleano : ValoreSingle
    {
        [ProtoMember(1)]
        public bool? V { get; set; } = false;

    }


    [ProtoContract]
    public class ValoreElenco : ValoreSingle
    {
        [ProtoMember(1)]
        public string V { get; set; }

        [ProtoMember(3)]
        public int ValoreAttributoElencoId { get; set; }

    }

    [ProtoContract]
    public class ValoreColore : ValoreSingle
    {
        [ProtoMember(1)]
        public string V { get; set; }

        [ProtoMember(2)]
        public string Hexadecimal { get; set; }
    }


    [ProtoContract]
    public class ValoreFormatoNumero : ValoreSingle
    {
        [ProtoMember(1)]
        public string V { get; set; }

        [ProtoMember(2)]
        public Guid ValoreAttributoFormatoNumeroId { get; set; }

    }


}
