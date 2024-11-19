using _3DModelExchange;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ReJo.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReJo
{

    public class RvtCalculator : Model3dCalculator
    {
        public AttCalculatorFunction AttCalculatorFunction { get; set; } = new AttCalculatorFunction();


        public override List<CalculatorFunction> GetFunctions()
        {
            List<CalculatorFunction> functs = base.GetFunctions();
            if (AttCalculatorFunction != null)
                functs.Add(AttCalculatorFunction);

            return functs;
        }

        public void Clear()
        {
            ResetExpressions();
            AttCalculatorFunction?.Clear();
        }


    }




    public class RvtCalculatorFunction : Model3dCalculatorFunction
    {
        public RvtCalculatorFunction() : base(Model3dCalculatorFunction.Names.Rvt)
        {
        }

        public override bool Calculate(string projectGlobalId, string globalId, Model3dClassEnum className, string itemPath, string valuePath, out double value)
        {
            bool res = false;

            Model3dValue val = new Model3dValue();
            val.ProjectGlobalId = projectGlobalId;
            val.GlobalId = globalId;
            val.ClassName = className;
            val.ValuePath = valuePath;
            val.ItemPath = itemPath;



            Model3dValues model3DValues = new Model3dValues();
            model3DValues.Values.Add(val);

            Utils.UpdateModel3dValues(model3DValues);

            Model3dValue? retValue = model3DValues.Values.FirstOrDefault();

            if (retValue != null)
            {
                string tmp = retValue.Value;

                if (Double.TryParse(tmp, CultureInfo.InvariantCulture,  out value))
                    return true;
            }

            if (res == false)
            {
                Error = true;
                if (!_results.ContainsKey(valuePath))
                    _results.Add(valuePath, double.NaN);

                if (!_resultsDescriptionFormula.ContainsKey(valuePath))
                    _resultsDescriptionFormula.Add(valuePath, "NaN");
            }

            value = Double.NaN;
            return res;
        }

    }

    public class AttCalculatorFunction : CalculatorFunction
    {
        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "att";

        Dictionary<string, double> Results = new Dictionary<string, double>();

        public bool Error { get; protected set; } = false;

        public AttCalculatorFunction() : base()
        {
        }

        public AttCalculatorFunction(double x) : base(x)
        {
        }

        public void Clear()
        {
            Error = false;
            Results.Clear();
        }

        public override string GetFunctionName() { return FunctionName; }
        public override string GetName() { return Name; }

        public override double calculate()
        {
            bool res = false;
            Error = false;

            int valuePathIndex = (int)x;
            string valuePath = ValuesPath[valuePathIndex];

            double result = 0;
            if (Results.TryGetValue(valuePath, out result))
            {
                return result;
            }

            if (!res)
            {
                Error = true;

                if (!_results.ContainsKey(valuePath))
                    _results.Add(valuePath, double.NaN);

                if (!_resultsDescriptionFormula.ContainsKey(valuePath))
                    _resultsDescriptionFormula.Add(valuePath, "NaN");
            }
            return Double.NaN;
        }

        public override Dictionary<string, string> GetFormulaResults()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            //foreach (string valuePath in ValuesPath)
            //{
            //    string formula = CalculatorExpression.CreateFormula(GetFunctionName(), valuePath);
            //    if (!res.ContainsKey(formula))
            //        res.Add(formula, valuePath);
            //}
            return res;
        }

        public void AddResult(string etichettaAtt, double result)
        {
            if (Results.ContainsKey(etichettaAtt))
                Results[etichettaAtt] = result;
            else
                Results.Add(etichettaAtt, result);
        }

    }


}
