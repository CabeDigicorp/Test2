
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
using ReJo.Utility;

namespace ReJo
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class CmdApplyRules : IExternalCommand, IExternalCommandAvailability
    {


        #region IExternalCommand Members

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            FiltersData filtersData = JoinService.This.GetCurrentProjectModel3dFilters();
            if (filtersData == null)
                return Result.Failed;

            UpdateRvtFilters(filtersData);


            JoinService.This.RvtFilterIdMap = filtersData.Items.ToDictionary(key => key.ID, value => value.RvtFilter);

            JoinService.This.ApplyComputoRules();

            return Result.Succeeded;
        }

        private void UpdateRvtFilters(FiltersData filtersData)
        {
            if (filtersData == null) return;

            foreach (var filterData in filtersData.Items)
            {
                RvtFilterForIO rvtFilter = filterData.RvtFilter;
                filterData.RvtFilter = FilterElementConverter.Convert(rvtFilter.ProjectIfcGuid, rvtFilter.RvtFilterUniqueId);
            }

            JoinService.This.SetCurrentProjectModel3dFilters(filtersData);

        }



        #endregion

        #region IExternalCommandAvailability Members

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            if (!CmdInit.IsInitialized)
                return false;

            if (CmdInit.This.UIApplication.ActiveUIDocument == null)
                return false;

            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
            if (doc == null)
                return false;

            if (!JoinService.This.Model3dFilesLoaded.ContainsKey(doc.PathName))
                return false;


            return true;
        }



        #endregion
    }



}
