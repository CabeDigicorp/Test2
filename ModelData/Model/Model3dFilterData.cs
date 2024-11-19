

using ProtoBuf;


namespace ModelData.Model
{
    [ProtoContract]
    public class Model3dFiltersData
    {
        [ProtoMember(1)]
        public List<Model3dFilterData> Items { get; set; } = new List<Model3dFilterData>();
    }


    [ProtoContract]
    public class Model3dFilterData //FiltersDataItem
    {
        [ProtoMember(1)]
        public string Descri { get; set; } = "";

        [ProtoMember(2)]
        public Guid Id { get; set; }

        [ProtoMember(3)]
        public List<Model3dFilterCondition> FilterConditions { get; set; } = new List<Model3dFilterCondition>();

        [ProtoMember(4)]
        public List<Model3dRuleComputo> RulesComputo { get; set; } = new List<Model3dRuleComputo>();

        [ProtoMember(5)]
        public RvtFilter RvtFilter { get; set; } = null;
    }

    [ProtoContract]
    public class Model3dFilterCondition //SingleCondiForIO 
    {
        [ProtoMember(1)]
        public string strIfcElemSelected { get; set; }

        [ProtoMember(2)]
        public int ElemSelected_ID { get; set; }

        [ProtoMember(3)]
        public int Id { get; set; }

        [ProtoMember(4)]
        public string ItemProperty { get; set; }

        [ProtoMember(5)]
        public string OperatorType { get; set; }

        [ProtoMember(6)]
        public string Operations { get; set; }

        [ProtoMember(7)]
        public string OpenedP { get; set; }

        [ProtoMember(8)]
        public string ClosedP { get; set; }

        [ProtoMember(9)]
        public string SearchType { get; set; }

        [ProtoMember(10)]
        public string ItemCategory { get; set; }

        [ProtoMember(11)]
        public string ItemCondiType { get; set; }

        [ProtoMember(12)]
        public string ItemValue { get; set; }

        [ProtoMember(13)]
        public bool IsNumeric { get; set; }

        [ProtoMember(14)]
        public int IfcElement_Type { get; set; }
    }

    [ProtoContract]
    public class Model3dRuleComputo
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }
        [ProtoMember(2)]
        public string Prezzario { get; set; }
        [ProtoMember(3)]
        public Guid PrezzarioItemId { get; set; }
        [ProtoMember(4)]
        public string Codice { get; set; }
        [ProtoMember(5)]
        public string Descrizione { get; set; }
        [ProtoMember(6)]
        public string FormulaQta { get; set; }
        [ProtoMember(7)]
        public Dictionary<string, string> FormuleByAttributoComputo { get; set; }


    }

    [ProtoContract]
    public class RvtFilter
    {
        [ProtoMember(1)]
        public string ProjectIfcGuid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string RvtFilterUniqueId { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string RvtFilterName { get; set; } = string.Empty;
    }




}
