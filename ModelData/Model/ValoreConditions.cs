using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class ValoreConditions
    {
        [ProtoMember(1)]
        public ValoreConditionsGroup MainGroup { get; set; } = new ValoreConditionsGroup();
    }

    [ProtoContract]
    public interface ValoreCondition
    {
    }

    [ProtoContract]
    public class ValoreConditionsGroup : ValoreCondition
    {

        [ProtoMember(1)]
        public ValoreConditionsGroupOperator Operator { get; set; } = ValoreConditionsGroupOperator.Nothing;

        [ProtoMember(2)]
        public List<ValoreCondition> Conditions { get; set; } = new List<ValoreCondition>();
    }

    [ProtoContract]
    public class AttributoValoreConditionSingle : ValoreCondition
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; } = string.Empty;

        [ProtoMember(2)]
        public ValoreConditionSingle ValoreConditionSingle { get; set; } = null;
    }

    [ProtoContract]
    public class ValoreConditionSingle : ValoreCondition
    {
        [ProtoMember(1)]
        public ValoreConditionEnum Condition { get; set; } = ValoreConditionEnum.Nothing;

        [ProtoMember(2)]
        public Valore Valore { get; set; } = null;
    }

    public enum ValoreConditionsGroupOperator
    {
        Nothing = 0,
        And = 1,
        Or = 2,
    }



    public enum ValoreConditionEnum
    {
        Nothing = 0,
        Equal = 1,
        Unequal = 2,
        LessThan = 3,
        LessOrEqualThan = 4,
        GreaterThan = 5,
        GreaterOrEqualThan = 6,
        StartsWith = 7,
        EndsWith = 8,
        Contains = 9,
        NotContains = 10,
        EqualIgnoreCase = 11,
    }

}
