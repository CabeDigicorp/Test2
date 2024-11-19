using _3DModelExchange;
using Autodesk.Revit.DB;

namespace ReJo.Utility
{
    public class FilterElementConverter
    {
        //oggetti temporaneri
        static Document? Document { get; set; } = null;
        static string ProjectIfcGuid { get; set; } = string.Empty;

        public static RvtFilterForIO Convert(string projectIfcGuid, string filterElUniqueId)
        {

            Document = CmdInit.This.GetDocument(projectIfcGuid);
            if (Document == null)
                return null;

            FilterElement? filterEl = Document.GetElement(filterElUniqueId) as FilterElement;
            if (filterEl == null)
                return null;

            ProjectIfcGuid = projectIfcGuid;


            RvtFilterForIO rvtFilter = null;

            if (filterEl is ParameterFilterElement parFilterEl)
            {
                rvtFilter = new RvtFilterForIO();

                rvtFilter.RvtFilterUniqueId = filterEl.UniqueId;
                rvtFilter.ProjectIfcGuid = ProjectIfcGuid;
                rvtFilter.RvtFilterName = filterEl.Name;

#if DEBUG


                rvtFilter.RvtParameterFilterData = Convert(parFilterEl, Document);

                ////long builtInParameterCode = (long) BuiltInParameter.AREA_TYPE;

                ////categorie
                //var categoriesId = parFilterEl.GetCategories();
                //foreach (ElementId catId in categoriesId)
                //{
                //    Category category = Category.GetCategory(Document, catId);
                //    long builtInCategoryCode = (long)category.BuiltInCategory;
                //    rvtFilter.BuiltInCategories.Add(builtInCategoryCode);
                //}


                //ElementFilter elFilter = parFilterEl.GetElementFilter();
                //rvtFilter.RvtElementFilter = ConvertRecursive(elFilter);
#endif

            }
            else if (filterEl is SelectionFilterElement selFilter)
            {

            }




            return rvtFilter;
        }

//        private static RvtFilterForIO Convert(FilterElement filterEl)
//        {

//            RvtFilterForIO rvtFilter = null;

//            if (filterEl is ParameterFilterElement parFilterEl)
//            {     
//                rvtFilter = new RvtFilterForIO();

//                rvtFilter.RvtFilterUniqueId = filterEl.UniqueId;
//                rvtFilter.ProjectIfcGuid = ProjectIfcGuid;
//                rvtFilter.RvtFilterName = filterEl.Name;

//#if DEBUG


//                RvtParameterFilterData parFilterData = Convert(parFilterEl, Document);

//                ////long builtInParameterCode = (long) BuiltInParameter.AREA_TYPE;

//                ////categorie
//                //var categoriesId = parFilterEl.GetCategories();
//                //foreach (ElementId catId in categoriesId)
//                //{
//                //    Category category = Category.GetCategory(Document, catId);
//                //    long builtInCategoryCode = (long)category.BuiltInCategory;
//                //    rvtFilter.BuiltInCategories.Add(builtInCategoryCode);
//                //}


//                //ElementFilter elFilter = parFilterEl.GetElementFilter();
//                //rvtFilter.RvtElementFilter = ConvertRecursive(elFilter);
//#endif

//            }
//            else if (filterEl is SelectionFilterElement selFilter)
//            {

//            }



//            return rvtFilter;
//        }

        //public static string CreateRvtFilter(RvtFilterForIO filterData, out string errorMsg)
        //{
        //    string filterElUniqueId = string.Empty;
        //    errorMsg = string.Empty;

        //    try
        //    {

                

        //        Document doc = CmdInit.This.GetDocument(filterData.ProjectIfcGuid);
        //        if (doc == null)
        //            return null;


        //        CmdInit.This.AddFilterHandler.Document = doc;
        //        CmdInit.This.AddFilterHandler.FilterData = filterData;
        //        CmdInit.This.AddFilterHandler.RaiseExternalEvent();

        //        //using (Transaction t = new Transaction(doc, "Add FilterElement"))
        //        //{
        //        //    t.Start();
        //        //    if (!FilterElement.IsNameUnique(doc, filterData.RvtFilterName))
        //        //    {
        //        //        //filtro già presente per nome

        //        //        ParameterFilterElement parFilterEl = Utils.GetFilterElementByName(doc, filterData.RvtFilterName);

        //        //        doc.Delete(parFilterEl.Id);
        //        //    }


        //        //    ParameterFilterElement parameterFilterElement = Convert(filterData.RvtFilterName, filterData.RvtParameterFilterData, doc);


        //        //    doc.ActiveView.AddFilter(parameterFilterElement.Id);
        //        //    t.Commit();

        //        //    filterElUniqueId = parameterFilterElement.UniqueId;
        //        //}

        //    }
        //    catch
        //    {
                
        //    }


        //    return filterElUniqueId;
        //}





        //private static RvtElementFilter ConvertRecursive(ElementFilter elFilter)
        //{
        //    RvtElementFilter rvtElementFilter = null;



        //    if (elFilter is ElementLogicalFilter elementLogicalFilter)
        //    {
        //        RvtElementLogicalFilter rvtElementLogicalFilter = new RvtElementLogicalFilter();

        //        var filters = elementLogicalFilter.GetFilters();
        //        foreach (var filter in filters)
        //        {
        //            RvtElementFilter elFilter2 = ConvertRecursive(filter);
        //            rvtElementLogicalFilter.RvtElementFilter = elFilter2;
        //        }
        //        rvtElementFilter = rvtElementLogicalFilter;
        //    }
        //    else if (elFilter is ElementQuickFilter elementQuickFilter)
        //    {
        //    }
        //    else if (elFilter is ElementParameterFilter elementSlowFilter)//ElementSlowFilter
        //    {
        //        RvtElementParameterFilter rvtElementParameterFilter = new RvtElementParameterFilter();
        //        rvtElementFilter = rvtElementParameterFilter;

        //        IList<FilterRule> rules = elementSlowFilter.GetRules();
        //        foreach (var rule in rules)
        //        {
        //            RvtFilterRule rvtFilterRule = null;

        //            InternalDefinition parameterDef = null;
        //            ElementId parameterId = rule.GetRuleParameter();
        //            ParameterElement? parameterElement = Document!.GetElement(parameterId) as ParameterElement;
        //            if (parameterElement != null)
        //            {
        //                parameterDef = parameterElement.GetDefinition();
        //            }


        //            if (rule is FilterCategoryRule filterCatRule)
        //            {

        //            }
        //            else if (rule is FilterInverseRule filterInverseRule)
        //            {

        //            }
        //            else if (rule is FilterValueRule filterValueRule)
        //            {



        //                if (rule is FilterNumericValueRule filterNumericValueRule)
        //                {
        //                    if (rule is FilterDoubleRule filterDoubleRule)
        //                    {
        //                        RvtFilterDoubleRule rvtFilterDoubleRule = new RvtFilterDoubleRule();
        //                        rvtFilterRule = rvtFilterDoubleRule;

        //                        double val = filterDoubleRule.RuleValue;
        //                        rvtFilterDoubleRule.RuleValue = val;


        //                        FilterNumericRuleEvaluator ev = filterDoubleRule.GetEvaluator();

        //                        if (ev is FilterNumericEquals filterNumericEquals)
        //                        {
        //                            rvtFilterDoubleRule.Condition = RvtValoreConditionEnum.Equal;
        //                        }
        //                        if (ev is FilterNumericGreater filterNumericGreater)
        //                        {
        //                            rvtFilterDoubleRule.Condition = RvtValoreConditionEnum.GreaterThan;
        //                        }
        //                        if (ev is FilterNumericGreaterOrEqual filterNumericGreaterOrEqual)
        //                        {
        //                            rvtFilterDoubleRule.Condition = RvtValoreConditionEnum.GreaterOrEqualThan;
        //                        }
        //                        if (ev is FilterNumericLess filterNumericLess)
        //                        {
        //                            rvtFilterDoubleRule.Condition = RvtValoreConditionEnum.LessThan;
        //                        }
        //                        if (ev is FilterNumericLessOrEqual filterNumericLessOrEqual)
        //                        {
        //                            rvtFilterDoubleRule.Condition = RvtValoreConditionEnum.LessOrEqualThan;
        //                        }
        //                    }
        //                    else if (rule is FilterElementIdRule filterElementIdRule)
        //                    {
        //                        ElementId elId = filterElementIdRule.RuleValue;

        //                        if (parameterDef != null)
        //                        {

        //                        }

        //                    }
        //                    else if (rule is FilterGlobalParameterAssociationRule filterGlobalParameterAssociationRule)
        //                    { }
        //                    else if (rule is FilterIntegerRule filterIntegerRule)
        //                    { }
        //                }
        //                else if (rule is FilterStringRule filterStringRule)
        //                { }
        //            }
        //            else if (rule is ParameterValuePresenceRule parameterValuePresenceRule)
        //            { }
        //            else if (rule is SharedParameterApplicableRule sharedParameterApplicableRule)
        //            { }

        //            if (parameterElement != null)
        //            {
        //                InternalDefinition def = parameterElement.GetDefinition();
        //                rvtFilterRule.BuiltInParameterCode = (long) def.BuiltInParameter;
        //                rvtFilterRule.ParameterName = def.Name;
        //            }

        //            rvtElementParameterFilter.RvtFilterRules.Add( rvtFilterRule );
        //        }


        //    }




        //    return rvtElementFilter;
        //}

        //private static void GetFilterRecursive(ElementFilter elFilter)
        //{
        //    if (elFilter is ElementLogicalFilter elementLogicalFilter)
        //    {
        //        var filters = elementLogicalFilter.GetFilters();
        //        foreach (var filter in filters)
        //            GetFilterRecursive(filter);

        //    }
        //    else if (elFilter is ElementQuickFilter elementQuickFilter)
        //    {


        //    }
        //    else if (elFilter is ElementParameterFilter elementSlowFilter)//ElementSlowFilter
        //    {
        //        IList<FilterRule> rules = elementSlowFilter.GetRules();
        //        foreach (var rule in rules)
        //        {
        //            if (rule is FilterCategoryRule filterCatRule)
        //            {

        //            }
        //            else if (rule is FilterInverseRule filterInverseRule)
        //            {

        //            }
        //            else if (rule is FilterValueRule filterValueRule)
        //            {
        //                if (rule is FilterNumericValueRule filterNumericValueRule)
        //                {
        //                    if (rule is FilterDoubleRule filterDoubleRule)
        //                    {
        //                        double val = filterDoubleRule.RuleValue;
        //                        FilterNumericRuleEvaluator ev = filterDoubleRule.GetEvaluator();

        //                    }
        //                    else if (rule is FilterElementIdRule filterElementIdRule)
        //                    { }
        //                    else if (rule is FilterGlobalParameterAssociationRule filterGlobalParameterAssociationRule)
        //                    { }
        //                    else if (rule is FilterIntegerRule filterIntegerRule)
        //                    { }
        //                }
        //                else if (rule is FilterStringRule filterStringRule)
        //                { }
        //            }
        //            else if (rule is ParameterValuePresenceRule parameterValuePresenceRule)
        //            { }
        //            else if (rule is SharedParameterApplicableRule sharedParameterApplicableRule)
        //            { }

        //        }
        //    }
        //}


        // Metodo per salvare i dati di un ParameterFilterElement
        static RvtParameterFilterDataForIO Convert(ParameterFilterElement filterElement, Document doc)
        {
            RvtParameterFilterDataForIO filterData = new RvtParameterFilterDataForIO();


            // Salva le categorie
            foreach (ElementId categoryId in filterElement.GetCategories())
            {
                Category category = Category.GetCategory(doc, categoryId);
                if (category != null)
                {
                    long builtInCategoryCode = (long)category.BuiltInCategory;

                    filterData.Categories.Add(new RvtCategoryDataForIO()
                    {
                        Name = category.Name,
                        BuiltInCode = (long)category.BuiltInCategory,
                    });
                }
            }


            ElementFilter elFilter = filterElement.GetElementFilter();

            if (elFilter is LogicalAndFilter logicalAndFilter)
            {
                filterData.LogicalFilter = new RvtLogicalFilterForIO();
                filterData.LogicalFilter.LogicalOperation = (int)LogicalOperation.And;

                var elFilters = logicalAndFilter.GetFilters();
                foreach (ElementFilter filter in elFilters)
                {
                    ConvertRecursive(doc, filterData.LogicalFilter, filter);
                }
            }
            else if (elFilter is LogicalOrFilter logicalOrFilter)
            {
                filterData.LogicalFilter = new RvtLogicalFilterForIO();
                filterData.LogicalFilter.LogicalOperation = (int)LogicalOperation.Or;

                var elFilters = logicalOrFilter.GetFilters();
                foreach (ElementFilter filter in elFilters)
                {
                    ConvertRecursive(doc, filterData.LogicalFilter, filter);
                }
            }





            return filterData;
        }

        private static void ConvertRecursive(Document doc, RvtLogicalFilterForIO logicalFilter, ElementFilter elFilter)
        {


            if (elFilter is LogicalAndFilter logicalAndFilter)
            {
                var subLogicalFilter = new RvtLogicalFilterForIO();
                subLogicalFilter.LogicalOperation = (int)LogicalOperation.And;

                var elFilters = logicalAndFilter.GetFilters();
                foreach (ElementFilter filter in elFilters)
                {
                    ConvertRecursive(doc, logicalFilter, filter);
                }
            }
            else if (elFilter is LogicalOrFilter logicalOrFilter)
            {
                var subLogicalFilter = new RvtLogicalFilterForIO();
                subLogicalFilter.LogicalOperation = (int)LogicalOperation.Or;

                var elFilters = logicalOrFilter.GetFilters();
                foreach (ElementFilter filter in elFilters)
                {
                    ConvertRecursive(doc, logicalFilter, filter);
                }

            }
            else if (elFilter is ElementParameterFilter elementParFilter)
            {

                // Salva le regole di filtro
                foreach (FilterRule rule in elementParFilter.GetRules())
                {

                    var ruleParId = rule.GetRuleParameter();

                    RvtFilterRuleDataForIO ruleData = new RvtFilterRuleDataForIO();

                    
                    if (ruleParId.Value > 0)
                    {
                        ruleData.ParameterName = doc.GetElement(ruleParId).Name;
                        ruleData.ParameterBuiltInCode = (long) BuiltInParameter.INVALID;
                    }
                    else
                    {
                        BuiltInParameter bip = (BuiltInParameter)ruleParId.Value;
                        ForgeTypeId forgeTypeId = ParameterUtils.GetParameterTypeId(bip);

                        ruleData.ParameterName = LabelUtils.GetLabelForBuiltInParameter(forgeTypeId);
                        ruleData.ParameterBuiltInCode = ruleParId.Value;
                    }
                    ruleData.Tolerance = rule is FilterDoubleRule doubleRule2 ? doubleRule2.Epsilon : 0.0;


                    if (rule is FilterDoubleRule doubleRule)
                    {
                        ruleData.Tolerance = doubleRule.Epsilon;

                        FilterNumericRuleEvaluator evaluator = doubleRule.GetEvaluator();
                        if (evaluator is FilterNumericEquals)
                        {
                            ruleData.RuleType = FilterRuleTypes.NumericEquals;
                            ruleData.DoubleValue = doubleRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericGreater)
                        {
                            ruleData.RuleType = FilterRuleTypes.NumericGreater;
                            ruleData.DoubleValue = doubleRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericGreaterOrEqual)
                        {
                            ruleData.RuleType = FilterRuleTypes.NumericGreaterOrEqual;
                            ruleData.DoubleValue = doubleRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericLess)
                        {
                            ruleData.RuleType = FilterRuleTypes.NumericLess;
                            ruleData.DoubleValue = doubleRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericLessOrEqual)
                        {
                            ruleData.RuleType = FilterRuleTypes.NumericLessOrEqual;
                            ruleData.DoubleValue = doubleRule.RuleValue;
                        }
                    }
                    else if (rule is FilterIntegerRule integerRule)
                    {
                        ruleData.Tolerance = 0;

                        FilterNumericRuleEvaluator evaluator = integerRule.GetEvaluator();
                        if (evaluator is FilterNumericEquals)
                        {
                            ruleData.RuleType = FilterRuleTypes.IntegerEquals;
                            ruleData.IntegerValue = integerRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericGreater)
                        {
                            ruleData.RuleType = FilterRuleTypes.IntegerGreater;
                            ruleData.IntegerValue = integerRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericGreaterOrEqual)
                        {
                            ruleData.RuleType = FilterRuleTypes.IntegerGreaterOrEqual;
                            ruleData.IntegerValue = integerRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericLess)
                        {
                            ruleData.RuleType = FilterRuleTypes.IntegerLess;
                            ruleData.IntegerValue = integerRule.RuleValue;
                        }
                        else if (evaluator is FilterNumericLessOrEqual)
                        {
                            ruleData.RuleType = FilterRuleTypes.IntegerLessOrEqual;
                            ruleData.IntegerValue = integerRule.RuleValue;
                        }
                    }
                    else if (rule is FilterStringRule stringRule)
                    {
                        ruleData.Tolerance = 0;

                        FilterStringRuleEvaluator evaluator = stringRule.GetEvaluator();
                        if (evaluator is FilterStringEquals)
                        {
                            ruleData.RuleType = FilterRuleTypes.StringEquals;
                            ruleData.StringValue = stringRule.RuleString;
                        }
                        else if (evaluator is FilterStringContains)
                        {
                            ruleData.RuleType = FilterRuleTypes.StringContains;
                            ruleData.StringValue = stringRule.RuleString;
                        }
                        else if (evaluator is FilterStringBeginsWith)
                        {
                            ruleData.RuleType = FilterRuleTypes.StringBeginsWith;
                            ruleData.StringValue = stringRule.RuleString;
                        }
                        else if (evaluator is FilterStringEndsWith)
                        {
                            ruleData.RuleType = FilterRuleTypes.StringEndsWith;
                            ruleData.StringValue = stringRule.RuleString;
                        }
                    }
                    else if (rule is FilterElementIdRule elementIdRule)
                    {
                        ruleData.Tolerance = 0;

                        FilterNumericRuleEvaluator evaluator = elementIdRule.GetEvaluator();
                        //if (evaluator is FilterElementIdEquals)
                        //{
                        //    ruleData.RuleType = "ElementIdEquals";
                        //    ruleData.ElementIdValue = elementIdRule.Value;
                        //}
                        //else if (evaluator is FilterElementIdNotEquals)
                        //{
                        //    ruleData.RuleType = "ElementIdNotEquals";
                        //    ruleData.ElementIdValue = elementIdRule.Value;
                        //}
                    }


                    logicalFilter.FilterRules.Add(ruleData);
                }

            }
        }

        // Metodo per ricostruire un ParameterFilterElement dai dati salvati
        public static ParameterFilterElement Convert(string filterName, RvtParameterFilterDataForIO filterData, Document doc)
        {


            if (filterData == null)
                return null;

            if (doc == null)
                return null;


            List<ElementId> categoryIds = new List<ElementId>();
            foreach (var categoryData in filterData.Categories)
            {
                Category category = doc.Settings.Categories.get_Item((BuiltInCategory)categoryData.BuiltInCode);
                if (category == null)
                    category = doc.Settings.Categories.get_Item(categoryData.Name);
                if (category != null)
                {
                    categoryIds.Add(category.Id);
                }
            }


            List<FilterRule> rules = new List<FilterRule>();
            ElementFilter elFilter = ConvertRecursive(filterData.LogicalFilter);

            ParameterFilterElement filterElement = null;
            if (elFilter != null)
            {
                filterElement = ParameterFilterElement.Create(doc, filterName, categoryIds, elFilter);
            }
            else
            {
                filterElement = ParameterFilterElement.Create(doc, filterName, categoryIds);
            }

            return filterElement;





        }

        private static ElementFilter ConvertRecursive(IRvtLogicalFilterForIO ilogicalFilter)
        {
            ElementFilter elementFilter = null;

            if (ilogicalFilter is RvtLogicalFilterForIO logicalFilter)
            {


                List<ElementFilter> elementFilters = new List<ElementFilter>();
                foreach (var lf in logicalFilter.FilterRules)
                {
                    ElementFilter elFilter = null;
                    elFilter = ConvertRecursive(lf);

                    elementFilters.Add(elFilter);
                }

                if (logicalFilter.LogicalOperation == (int)LogicalOperation.And)
                {
                    elementFilter = new LogicalAndFilter(elementFilters);
                
                }
                else if (logicalFilter.LogicalOperation == (int)LogicalOperation.Or)
                {
                    elementFilter = new LogicalOrFilter(elementFilters);
                }

                


            }
            else if (ilogicalFilter is RvtFilterRuleDataForIO ruleData)
            {

                

                ElementId parameterId = new ElementId(BuiltInParameter.ALL_MODEL_COST); // Sostituisci con il parametro corretto
                FilterRule rule = null;


                if (ruleData.RuleType == FilterRuleTypes.NumericEquals)
                    rule = ParameterFilterRuleFactory.CreateEqualsRule(parameterId, ruleData.DoubleValue.Value, ruleData.Tolerance);
                if (ruleData.RuleType == FilterRuleTypes.NumericGreater)
                    rule = ParameterFilterRuleFactory.CreateGreaterRule(parameterId, ruleData.DoubleValue.Value, ruleData.Tolerance);
                if (ruleData.RuleType == FilterRuleTypes.NumericGreaterOrEqual)
                    rule = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(parameterId, ruleData.DoubleValue.Value, ruleData.Tolerance);
                if (ruleData.RuleType == FilterRuleTypes.NumericLess)
                    rule = ParameterFilterRuleFactory.CreateLessRule(parameterId, ruleData.DoubleValue.Value, ruleData.Tolerance);
                if (ruleData.RuleType == FilterRuleTypes.NumericLessOrEqual)
                    rule = ParameterFilterRuleFactory.CreateLessOrEqualRule(parameterId, ruleData.DoubleValue.Value, ruleData.Tolerance);
                if (ruleData.RuleType == FilterRuleTypes.IntegerEquals)
                    rule = ParameterFilterRuleFactory.CreateEqualsRule(parameterId, ruleData.IntegerValue.Value);
                if (ruleData.RuleType == FilterRuleTypes.IntegerGreater)
                    rule = ParameterFilterRuleFactory.CreateGreaterRule(parameterId, ruleData.IntegerValue.Value);
                if (ruleData.RuleType == FilterRuleTypes.IntegerGreaterOrEqual)
                    rule = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(parameterId, ruleData.IntegerValue.Value);
                if (ruleData.RuleType == FilterRuleTypes.IntegerLess)
                    rule = ParameterFilterRuleFactory.CreateLessRule(parameterId, ruleData.IntegerValue.Value);
                if (ruleData.RuleType == FilterRuleTypes.IntegerLessOrEqual)
                    rule = ParameterFilterRuleFactory.CreateLessOrEqualRule(parameterId, ruleData.IntegerValue.Value);
                if (ruleData.RuleType == FilterRuleTypes.StringEquals)
                    rule = ParameterFilterRuleFactory.CreateEqualsRule(parameterId, ruleData.StringValue, false);
                if (ruleData.RuleType == FilterRuleTypes.StringContains)
                    rule = ParameterFilterRuleFactory.CreateContainsRule(parameterId, ruleData.StringValue, false);
                if (ruleData.RuleType == FilterRuleTypes.StringBeginsWith)
                    rule = ParameterFilterRuleFactory.CreateBeginsWithRule(parameterId, ruleData.StringValue, false);
                if (ruleData.RuleType == FilterRuleTypes.StringEndsWith)
                    rule = ParameterFilterRuleFactory.CreateEndsWithRule(parameterId, ruleData.StringValue, false);
                //if (ruleData.RuleType == FilterRuleTypes.ElementIdEquals)
                //    rule = ParameterFilterRuleFactory.CreateEqualsRule(parameterId, ruleData.ElementIdValue);
                //if (ruleData.RuleType == FilterRuleTypes.ElementIdNotEquals)
                //    rule = ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, ruleData.ElementIdValue);


                //if (rule != null)
                //{
                //    rules.Add(rule);
                //}

                elementFilter = new ElementParameterFilter(rule);

            }

            return elementFilter;

        }
    }

    public static class FilterRuleTypes
    {
        public static readonly string NumericEquals = "NumericEquals";
        public static readonly string NumericGreater = "NumericGreater";
        public static readonly string NumericGreaterOrEqual = "NumericGreaterOrEqual";
        public static readonly string NumericLess = "NumericLess";
        public static readonly string NumericLessOrEqual = "NumericLessOrEqual";
        public static readonly string IntegerEquals = "IntegerEquals";
        public static readonly string IntegerGreater = "IntegerGreater";
        public static readonly string IntegerGreaterOrEqual = "IntegerGreaterOrEqual";
        public static readonly string IntegerLess = "IntegerLess";
        public static readonly string IntegerLessOrEqual = "IntegerLessOrEqual";
        public static readonly string StringEquals = "StringEquals";
        public static readonly string StringContains = "StringContains";
        public static readonly string StringBeginsWith = "StringBeginsWith";
        public static readonly string StringEndsWith = "StringEndsWith";
        public static readonly string ElementIdEquals = "ElementIdEquals";
        public static readonly string ElementIdNotEquals = "ElementIdNotEquals";
    }

    enum LogicalOperation
    {
        Nothing = 0,
        And = 1,
        Or = 2,
    }






}
