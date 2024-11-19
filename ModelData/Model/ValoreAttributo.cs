
using ProtoBuf;

namespace ModelData.Model
{

    [ProtoContract]
    [ProtoInclude(1001, typeof(ValoreAttributoElenco))]
    [ProtoInclude(1002, typeof(ValoreAttributoColore))]
    [ProtoInclude(1003, typeof(ValoreAttributoFormatoNumero))]
    [ProtoInclude(1004, typeof(ValoreAttributoRiferimentoGuidCollection))]
    [ProtoInclude(1005, typeof(ValoreAttributoVariabili))]
    [ProtoInclude(1006, typeof(ValoreAttributoGuidCollection))]
    [ProtoInclude(1007, typeof(ValoreAttributoReale))]
    [ProtoInclude(1008, typeof(ValoreAttributoContabilita))]
    [ProtoInclude(1009, typeof(ValoreAttributoGuid))]
    [ProtoInclude(1010, typeof(ValoreAttributoTesto))]
    public interface ValoreAttributo
    {
    }


    [ProtoContract]
    public class ValoreAttributoElenco : ValoreAttributo
    {
        [ProtoMember(1)]
        public List<ValoreAttributoElencoItem> Items { get; set; } = new List<ValoreAttributoElencoItem>();

        [ProtoMember(2)]
        public ValoreAttributoElencoType Type { get; set; } = ValoreAttributoElencoType.Default;

        [ProtoMember(3)]
        public bool IsMultiSelection { get; set; } = false;

    }

    public enum ValoreAttributoElencoType
    {
        Default = 0,
        Font = 1,
    }

    [ProtoContract]
    public class ValoreAttributoElencoItem
    {
        /// <summary>
        /// Obsoleto
        /// </summary>
        //[ProtoMember(1)]
        //public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Text { get; set; }

        [ProtoMember(3)]
        public int Id { get; set; }
    }

    [ProtoContract]
    public class ValoreAttributoColore : ValoreAttributo
    {
        [ProtoMember(1)]
        public List<ValoreAttributoColoreItem> Items { get; set; } = new List<ValoreAttributoColoreItem>();

    }

    [ProtoContract]
    public class ValoreAttributoColoreItem
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Text { get; set; }

        [ProtoMember(3)]
        public string HexValue { get; set; }

    }

    [ProtoContract]
    public class ValoreAttributoFormatoNumero : ValoreAttributo
    {
        [ProtoMember(1)]
        public List<ValoreAttributoFormatoNumeroItem> Items { get; set; } = new List<ValoreAttributoFormatoNumeroItem>();

    }

    [ProtoContract]
    public class ValoreAttributoFormatoNumeroItem
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }

        //[ProtoMember(2)]
        //public string Text { get; set; }

        [ProtoMember(3)]
        public string Format { get; set; }

    }

    [ProtoContract]
    public class ValoreAttributoRiferimentoGuidCollection : ValoreAttributo
    {
        [ProtoMember(1)]
        public ValoreOperationType Operation { get; set; } = ValoreOperationType.Nothing;

    }

    [ProtoContract]
    public class ValoreAttributoVariabili : ValoreAttributo
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; } = String.Empty;

    }

    [ProtoContract]
    public class ValoreAttributoGuidCollection : ValoreAttributo
    {
        [ProtoMember(1)]
        public ItemsSelectionTypeEnum ItemsSelectionType { get; set; } = ItemsSelectionTypeEnum.Nothing;

    }

    [ProtoContract]
    public class ValoreAttributoReale : ValoreAttributo
    {
        [ProtoMember(1)]
        public bool UseSignificantDigitsByFormat { get; set; } = false;
    }

    [ProtoContract]
    public class ValoreAttributoContabilita : ValoreAttributo
    {
        [ProtoMember(1)]
        public bool UseSignificantDigitsByFormat { get; set; } = false;
    }

    [ProtoContract]
    public class ValoreAttributoGuid : ValoreAttributo
    {
        [ProtoMember(1)]
        public ValoreAttributoGuidSummarizeItem SummarizeAttributo3 { get; set; } = new ValoreAttributoGuidSummarizeItem();

        [ProtoMember(2)]
        public ValoreAttributoGuidSummarizeItem SummarizeAttributo4 { get; set; } = new ValoreAttributoGuidSummarizeItem();

        [ProtoMember(3)]
        public string ItemPath { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class ValoreAttributoGuidSummarizeItem
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class ValoreAttributoTesto : ValoreAttributo
    {
        [ProtoMember(1)]
        public bool UseDeepValore { get; set; } = false;
    }

    public enum ItemsSelectionTypeEnum
    {
        Nothing = 0,
        Manual = 1,
        All = 2,
        ByFilter = 3,
    }
}
