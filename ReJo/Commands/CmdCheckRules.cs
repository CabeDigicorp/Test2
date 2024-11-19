
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using ReJo.UI;
using ReJo.Utility;



namespace ReJo
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class CmdCheckRules : IExternalCommand, IExternalCommandAvailability
    {


        #region IExternalCommand Members

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var rulesCalc = new RulesCalculator();
            List<RuleError> ruleErrors = rulesCalc.CheckRules();

            FiltersPane.This.View?.SetRuleErrors(ruleErrors);
            FiltersPane.This.View?.Update();


            return Result.Succeeded;
        }



        #endregion

        #region IExternalCommandAvailability Members

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            if (CmdInit.IsInitialized)
                return true;
            else
                return false;
        }



        #endregion
    }



}
