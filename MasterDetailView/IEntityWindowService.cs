using Commons;
using MasterDetailModel;
using Model;
using ModelData.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WebServiceClient;

namespace MasterDetailView
{

    public interface IEntityWindowService : IWindowService
    {
        void ShowEditRtfWindow(ref string rtfText, string title, out string plainText);
        //void ReplaceInRtfText(ref string rtf, string oldStr, string newStr);
        void ShowReplaceTextWindow(object viewModel);
        void ShowWaitCursor(bool wait);

        int FilterByDivisioneIdsWindow(Guid divId, ref List<Guid> selectedItems, string title);
        int FilterByEntityIdsWindow(EntitiesListMasterDetailView master, AttributoFilterData attFilterData, string entityTypeKey, ref List<Guid> itemsId, string title);

        bool SelectPrezzarioIdsWindow(ref List<Guid> selectedItems, ref string externalPrezzarioFileName, string title,
            SelectIdsWindowOptions options, bool allowPrezzarioInterno, bool allowPrezzariEsterni, bool updateItemsOnClose,
            ref EntityTypeViewSettings viewSettings);
        bool SelectPrezzarioWindow(ref string externalPrezzarioFileName, string title);
        bool SelectDivisioneIdsWindow(Guid divId, ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options, AttributoRiferimento senderAttRif);
        bool SelectEntityIdsWindow(string entityTypeKey, ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options, EntityTypeViewSettings viewSettings, AttributoRiferimento senderAttRif);
        //
        bool AttributoFilterDetailWindow(EntitiesListMasterDetailView master, AttributoFilterData attFilterData);
        bool EditAttributoMultiValoreItemWindow(EntitiesListMasterDetailView master, ValoreTestoCollectionItemView itemView);
        //
        IEntityWindowService CreateWindowService(ClientDataService dataService, ModelActionsStack modelActionsStack, IMainOperation mainOperation);
        bool EntitiesImportWindow(EntitiesImportStatus importStatus);
        bool ImportProjectModelWindow(ref string modelFullFileName);
        bool NewProjectModelWindow(ref string modelFullFileName);
        //
        bool AttributoCodingWindow(string entityTypeKey, HashSet<Guid> checkedEntitiesId, int MaxDepth, List<int> SelectedLevels);
        bool SelectAttributoFilterWindow(HashSet<string> EntityTypesKey, List<Guid> entsIdToFilter, ref AttributoFilterData attFilterData);
        bool SetValoreConditionsWnd(string entityTypeKey, ValoreConditions valCond, bool allowAccept);
        bool SelectAttributoWeekHoursWindow(ref WeekHours WeekHoursData);
        bool SelectAttributoCustomDaysWindow(WeekHours attWeekHours,ref CustomDays CustomDaysData);
        bool SelectAttributoPredecessoriWindow(Guid Guid, ref WBSPredecessors WBSPredecessorsData);
        bool EntityHighlightersWnd(string entityTypeKey);
        //bool WebLoginWnd(LoginDto loginDto);
    }


}
