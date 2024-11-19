

namespace MasterDetailView
{

    public enum EntitiesListMasterDetailViewCommands
    {
        Nessuno = 0,
        ScrollCurrentItemIntoView = 1,
        ApplyGroups = 2,
        ExpandAll = 4,
        CollapseAll = 8,
        SelectRows = 16,
        ExpandCheckedEntityGroups = 32,
        ExpandNewEntitiesGroup = 64,
        UpdateGridColumns = 128,
        ClearGridColumns = 256,
        SelectAll = 512,
        UpdateTableSummaryRow = 1024,

    }

    public enum ReadyToPasteEntitiesCommands
    {
        Nothing = 0,
        Move = 1,
        Copy = 2,
        MultiModify = 3,
    }

    public enum SelectIdsWindowOptions
    {
        Nothing = 0,
        IsSingleSelection = 1,
        AllowNoSelection = 2,
        NotAllowAcceptSelection = 4,
    }


}