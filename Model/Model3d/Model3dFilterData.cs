
#define DATA_CHANGED

using _3DModelExchange;
using Commons;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Model
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
        public List<IfcFilterCondition> IfcFilterConditions { get; set; } = new List<IfcFilterCondition>();

        [ProtoMember(4)]
        public List<Model3dRuleComputo> RulesComputo { get; set; } = new List<Model3dRuleComputo>();

        [ProtoMember(5)]
        public RvtFilter RvtFilter { get; set; } = null;
    }



    [ProtoContract]
    public class IfcFilterCondition//SingleCondiForIO 
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
        public string RvtParameterFilterData { get; set; } = null;//json
    }



    public class Model3dFiltersDataConverter
    {

        public static Model3dFiltersData ConvertFromFiltersData(FiltersData source, Model3dType model3dType)
        {

            Model3dFiltersData target = new Model3dFiltersData();
            foreach (FiltersDataItem s in source.Items)
            {
                if (!IsFilterItemOfType(model3dType, s))
                    continue;

                Model3dFilterData t = new Model3dFilterData();
                t.Descri = s.Descri;
                t.Id = s.ID;

                if (s.FilterConditionsIO != null)
                {
                    foreach (IfcSingleCondiForIO cond in s.FilterConditionsIO)
                    {
                        IfcFilterCondition model3dCond = new IfcFilterCondition();

                        model3dCond.strIfcElemSelected = cond.strIfcElemSelected;
                        model3dCond.ElemSelected_ID = cond.ElemSelected_ID;
                        model3dCond.Id = cond.ID;
                        model3dCond.ItemProperty = cond.ItemProperty;
                        model3dCond.OperatorType = cond.OperatorType;
                        model3dCond.Operations = cond.Operations;
                        model3dCond.OpenedP = cond.OpenedP;
                        model3dCond.ClosedP = cond.ClosedP;
                        model3dCond.SearchType = cond.SearchType;
                        model3dCond.ItemCategory = cond.ItemCategory;
                        model3dCond.ItemCondiType = cond.ItemCondiType;
                        model3dCond.ItemValue = cond.ItemValue;
                        model3dCond.IsNumeric = cond.IsNumeric;
                        model3dCond.IfcElement_Type = cond.IfcElement_Type;

                        t.IfcFilterConditions.Add(model3dCond);
                    }
                }

                if (s.RulesIO != null)
                {
                    foreach (RegoleComputoForIO rule in s.RulesIO)
                    {
                        Model3dRuleComputo model3dRule = new Model3dRuleComputo();

                        model3dRule.Codice = rule.Codice;
                        model3dRule.Descrizione = rule.Descrizione;
                        model3dRule.FormulaQta = rule.FormulaQta;
                        model3dRule.Id = rule.RuleId;
                        model3dRule.Prezzario = rule.Prezzario;
                        model3dRule.PrezzarioItemId = rule.Id;
                        if (rule.FormuleByAttributoComputo != null)
                            model3dRule.FormuleByAttributoComputo = new Dictionary<string, string>(rule.FormuleByAttributoComputo);

                        t.RulesComputo.Add(model3dRule);
                    }
                }

                if (s.RvtFilter != null)
                {

                    t.RvtFilter = new RvtFilter()
                    {
                        RvtFilterUniqueId = s.RvtFilter?.RvtFilterUniqueId,
                        ProjectIfcGuid = s.RvtFilter?.ProjectIfcGuid,
                        RvtFilterName = s.RvtFilter?.RvtFilterName,
                    };

                    if (s.RvtFilter.RvtParameterFilterData != null)
                        t.RvtFilter.RvtParameterFilterData = JsonConvert.SerializeObject(s.RvtFilter.RvtParameterFilterData, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });
                }


                //t.RvtFilter = new RvtFilter()
                //{
                //    RvtFilterUniqueId = s.RvtFilter?.RvtFilterUniqueId,
                //    ProjectIfcGuid = s.RvtFilter?.ProjectIfcGuid,
                //    RvtFilterName = s.RvtFilter?.RvtFilterName,
                //};



                target.Items.Add(t);
            }
            return target;
        }

        public static bool IsFilterItemOfType(Model3dType model3dType, FiltersDataItem s)
        {
            if ((s.FilterConditionsIO == null || s.FilterConditionsIO.Count == 0) && model3dType == Model3dType.Ifc)
                return false;

            if ((s.RvtFilter == null || string.IsNullOrEmpty(s.RvtFilter.RvtFilterUniqueId)) && model3dType == Model3dType.Revit)
                return false;

            return true;
        }

        public static FiltersData ConvertToFiltersData(Model3dFiltersData source, Int32 sourceProjectFileVersion, Model3dType model3dType)
        {
            FiltersData target = new FiltersData();


            //if (projectFileVersion >= x)
            //{

            //}
            //else
            if (sourceProjectFileVersion >= 0)
            {
                foreach (Model3dFilterData s in source.Items)
                {


                    if ((s.IfcFilterConditions == null || s.IfcFilterConditions.Count == 0) && model3dType == Model3dType.Ifc)
                        continue;

                    if ((s.RvtFilter == null || string.IsNullOrEmpty(s.RvtFilter.RvtFilterUniqueId)) && model3dType == Model3dType.Revit)
                        continue;


                    FiltersDataItem t = new FiltersDataItem();
                    t.Descri = s.Descri;
                    t.ID = s.Id;

                    foreach (IfcFilterCondition model3dCond in s.IfcFilterConditions)
                    {
                        IfcSingleCondiForIO cond = new IfcSingleCondiForIO();

                        cond.strIfcElemSelected = model3dCond.strIfcElemSelected;
                        cond.ElemSelected_ID    = model3dCond.ElemSelected_ID;
                        cond.ID                 = model3dCond.Id;
                        cond.ItemProperty       = model3dCond.ItemProperty;
                        cond.OperatorType       = model3dCond.OperatorType;
                        cond.Operations         = model3dCond.Operations;
                        cond.OpenedP            = model3dCond.OpenedP;
                        cond.ClosedP            = model3dCond.ClosedP;
                        cond.SearchType         = model3dCond.SearchType;
                        cond.ItemCategory       = model3dCond.ItemCategory;
                        cond.ItemCondiType      = model3dCond.ItemCondiType;
                        cond.ItemValue          = model3dCond.ItemValue;
                        cond.IsNumeric          = model3dCond.IsNumeric;
                        cond.IfcElement_Type    = model3dCond.IfcElement_Type;

                        t.FilterConditionsIO.Add(cond);
                    }

                    foreach (Model3dRuleComputo model3dRule in s.RulesComputo)
                    {
                        RegoleComputoForIO rule = new RegoleComputoForIO();

                        rule.Codice = model3dRule.Codice;
                        rule.Descrizione = model3dRule.Descrizione;
                        rule.FormulaQta = model3dRule.FormulaQta;
                        rule.RuleId = model3dRule.Id;
                        rule.Prezzario = model3dRule.Prezzario;
                        rule.Id = model3dRule.PrezzarioItemId;
                        if (model3dRule.FormuleByAttributoComputo != null)
                            rule.FormuleByAttributoComputo = new Dictionary<string, string>(model3dRule.FormuleByAttributoComputo);

                        t.RulesIO.Add(rule);
                    }



                    if (s.RvtFilter != null)
                    {
                        t.RvtFilter = new RvtFilterForIO()
                        {
                            RvtFilterUniqueId = s.RvtFilter?.RvtFilterUniqueId,
                            ProjectIfcGuid = s.RvtFilter?.ProjectIfcGuid,
                            RvtFilterName = s.RvtFilter?.RvtFilterName,
                        };

                        try
                        {
                            if (!string.IsNullOrEmpty(s.RvtFilter.RvtParameterFilterData))
                                t.RvtFilter.RvtParameterFilterData = JsonConvert.DeserializeObject<RvtParameterFilterDataForIO>(s.RvtFilter.RvtParameterFilterData, new Newtonsoft.Json.JsonSerializerSettings
                                {
                                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                                });
                        }
                        catch (Exception exc)
                        {
                            MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                        }


                    }


                    //if (s.RvtFilter != null)
                    //{
                    //    t.RvtFilter = new RvtFilterForIO();
                    //    t.RvtFilter.RvtFilterUniqueId = s.RvtFilter.RvtFilterUniqueId;
                    //    t.RvtFilter.ProjectIfcGuid = s.RvtFilter?.ProjectIfcGuid;
                    //    t.RvtFilter.RvtFilterName = s.RvtFilter?.RvtFilterName;

                    //}




                    

                    

                    target.Items.Add(t);
                }

            }
            else
            {
                //versione non supportata
            }
            return target;


        }


    }


}
