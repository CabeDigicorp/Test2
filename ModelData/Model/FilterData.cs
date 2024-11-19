
using ProtoBuf;
namespace ModelData.Model
{
    [ProtoContract]
    public class FilterData
    {
        [ProtoMember(2)]
        public List<AttributoFilterData> Items { get; set; } = new List<AttributoFilterData>();
    }


    [ProtoContract]
    public class AttributoFilterData
    {
        [ProtoMember(1)]
        public string EntityTypeKey { get; set; }

        [ProtoMember(2)]
        public string CodiceAttributo { get; set; }

        [ProtoMember(3)]
        public string TextSearched { get; set; }

        [ProtoMember(4)]
        public bool? IsAllChecked
        {
            get;
            set;
        } = false;

        [ProtoMember(5)]
        public bool IsFiltroAttivato { get; set; } = false;

        [ProtoMember(6)]
        public HashSet<string> CheckedValori { get; set; } = new HashSet<string>();


        //Utilizzati nel caso di filtro per Attributo di tipo Riferimento (filtro sull'entità sorgente)
        #region solo x Attributo riferimento
        [ProtoMember(7)]
        public string SourceCodiceAttributo { get; set; }

        [ProtoMember(8)]
        public string SourceEntityTypeKey { get; set; }
        #endregion

        [ProtoMember(9)]
        public FilterTypeEnum FilterType { get; set; } = FilterTypeEnum.Result;

        [ProtoMember(10)]
        public ValoreConditions ValoreConditions { get; set; } = new ValoreConditions();

    }

    public enum FilterTypeEnum
    {
        Nothing = 0,
        Formula = 1,
        Result = 2,
        Conditions = 3,

    }


}
