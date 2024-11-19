
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Text;
using System.Windows;
using System.IO;
using Autodesk.Revit.UI.Selection;
using System.Windows.Controls;
using _3DModelExchange;
using RUI = Autodesk.Revit.UI;
using ReJo.UI;



namespace ReJo
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class CmdFilters : IExternalCommand, IExternalCommandAvailability
    {


        #region IExternalCommand Members

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            FiltersPane.This.Update();
            FiltersPane.This.Show(true);

            return Result.Succeeded;
        }



        #endregion

        #region IExternalCommandAvailability Members

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            

            if (!CmdInit.IsInitialized)
                return false;

            if (CmdInit.This.UIApplication.ActiveUIDocument == null)
                return false;


            return true;
        }



        #endregion
    }



}
