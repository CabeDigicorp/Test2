using _3DModelExchange;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IMainOperation
    {
        

        event EventHandler ProgressChanged;

        void UpdateEntityTypesView(List<string> entityTypesKey);
        void ShowMessageBarView(string msg, bool isOkButtonVisible = true, int progressValue = -1, bool close = false);
        Dictionary<string, IDataService> GetPrezzariCache();
        string GetPrezzariFolder();
        string GetModelliFolder();
        string GetAppSettingsPath();
        IDataService GetDataServiceByFile(string fullFileName, out Int32 projectFileVersion);
        bool IsProjectClosing();
        bool IsAdvancedMode();
        IWindowService GetWindowService();
        void ImportModel(IDataService ds);
        Project CreateProjectByModel(string modelFullFileName, out Int32 modelFileVersion);
        string GetProjectFileExtension();
        string GetProjectFullFileName();
        IDataService GetCurrentProjectDataService();
        List<Guid> GetSelectedEntitiesId(string entityTypeKey);
        List<Guid> GetFilteredEntitiesId(string entityTypeKey);
        void UpdateProjectViewSettings();
        string GetDeploymentVersion();
        bool IsValidVersion(string minAppVersion);
        SectionItemTemplateView GetWBSView();
        SectionItemTemplateView GetFogliDiCalcoloView();
        void AddToModelActionStack(ModelAction modelAction);
        bool IsRecalculateItemsNeeded(string entityTypeKey, bool? valueToSet = null);
        bool ImportItems(string fullFileName, string entityTypeKey);
        void UndoGroupBegin(string undoGroupName, string entityTypeKey);
        void UndoGroupEnd();
        void UndoGroupCancel();
        Model3dType GetModel3dType();

    }
}
