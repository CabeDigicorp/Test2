
using CommonResources;
using Commons;
using DevExpress.Pdf.Xmp;
using DevExpress.XtraRichEdit.Import.Doc;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using MasterDetailView;
using Microsoft.Win32;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Spreadsheet;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MasterDetailWpf
{


    public class MasterDetailGrid
    {

//        public static bool _testSelection = true;

//        public static bool _testSelection = true;


        public UserControl MasterDetailGridCtrl { get; set; }//MasterDetail control
        public SfDataGrid MasterDataGrid { get; set; }//master control
        public ListView DetailListView { get; set; }//detail control
        public MasterDetailGridView MasterDetailGridView { get; set; }//MasterDetail View
        public ScaleTransform MasterDetailScale { get; set; }

        //Classe per il riordino manuale delle colonne
        ColumnChooser columnChooserWindow { get; set; } = null;

        //Numero di colonne fisse (colonna delle icone)
        int FixedColumnCount { get; set; }

        //colonne ordinate quando non è presente nessun raggruppamento
        List<string> OrderedColumnsMappingName = new List<string>();

        /// <summary>
        /// key:codice attributo, value: column mappingName
        /// </summary>
        Dictionary<string, string> ColumnAttributiMappingName = new Dictionary<string, string>();

        public static string GridTableSummaryRowTitle => "GridTableSummaryRowTitle";

        public bool MouseIndentButtonPressed { get; protected set; }
        

        private bool _isColumnsUpdating = false;

        public MasterDetailGrid()
        {
        }

        public void Init()
        {

            FixedColumnCount = MasterDataGrid.Columns.Count;

            //scopo: scrivere in TableSummary il totale per ogni colonna
            MasterDataGrid.CellRenderers.Remove("TableSummary");
            MasterDataGrid.CellRenderers.Add("TableSummary", new GridTableSummaryCellRendererExt());

            //scopo: scrivere in CaptionSummary quando raggruppato
            MasterDataGrid.CellRenderers.Remove("CaptionSummary");
            MasterDataGrid.CellRenderers.Add("CaptionSummary", new GridCaptionSummaryCellRendererExt());

            MasterDataGrid.SelectionController = new GridSelectionControllerExt(this);

            MasterDataGrid.RowGenerator = new CustomRowGenerator(this);
            MasterDataGrid.SummaryGroupComparer = new SortGroupComparers(this);

            //Column Chooser
            columnChooserWindow = new ColumnChooser(MasterDataGrid);
            columnChooserWindow.Title = LocalizationProvider.GetString("Colonne");
            columnChooserWindow.WaterMarkText = LocalizationProvider.GetString("Trascina le colonne da nascondere");
            columnChooserWindow.Width = 300;
            GridColumnChooserControllerExt gridColumnChooserController = new GridColumnChooserControllerExt(MasterDataGrid, columnChooserWindow);
            gridColumnChooserController.IsColumnHiddenChanged += GridColumnChooserController_IsColumnHiddenChanged;
            MasterDataGrid.GridColumnDragDropController = gridColumnChooserController;


            //Eventi
            columnChooserWindow.Closing += ColumnChooserWindow_Closing;

            //MasterDetailGridView.ItemsView.ItemsLoading += ItemsView_ItemsLoading;
            //MasterDetailGridView.ItemsView.ItemsLoaded += ItemsView_ItemsLoaded;
            SubscribeViewEvents();

            MasterDataGrid.Loaded += MasterDataGrid_Loaded;
            MasterDataGrid.Unloaded += MasterDataGrid_Unloaded;
            MasterDataGrid.CurrentCellActivated += MasterDataGrid_CurrentCellActivated;
            MasterDataGrid.ItemsSourceChanged += MasterDataGrid_ItemsSourceChanged;
            //MasterDataGrid.CopyGridCellContent += MasterDataGrid_CopyGridCellContent;
            MasterDataGrid.GridCopyContent += MasterDataGrid_GridCopyContent;
            MasterDataGrid.QueryColumnDragging += MasterDataGrid_QueryColumnDragging; //per prevenire il riordino delle colonne frozen
            MasterDataGrid.SelectionChanging += MasterDataGrid_SelectionChanging;
            MasterDataGrid.SelectionChanged += MasterDataGrid_SelectionChanged;
            MasterDataGrid.PreviewMouseLeftButtonUp += MasterDataGrid_PreviewMouseLeftButtonUp;
            MasterDataGrid.GroupExpanded += MasterDataGrid_GroupExpanded;
            MasterDataGrid.GroupCollapsed += MasterDataGrid_GroupCollapsed;
            MasterDataGrid.GroupExpanding += MasterDataGrid_GroupExpanding;
            MasterDataGrid.GroupCollapsing += MasterDataGrid_GroupCollapsing;
            MasterDataGrid.LayoutUpdated += MasterDataGrid_LayoutUpdated;
            MasterDataGrid.ResizingColumns += MasterDataGrid_ResizingColumns;

        }



        private void MasterDataGrid_ResizingColumns(object sender, ResizingColumnsEventArgs e)
        {
            if (e.Reason == ColumnResizingReason.Resized)
            {
                int attGrouppedCount = MasterDetailGridView.ItemsView.RightPanesView.GroupView.Data.Items.Count;
                int colIndex = e.ColumnIndex - FixedColumnCount - attGrouppedCount;

                if (colIndex < 0)
                {
                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "colIndex < 0");
                    return;
                }

                string mappingName = MasterDataGrid.Columns[colIndex].MappingName;
                string codiceAtt = MasterDetailGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(mappingName);

                string entTypeCode = MasterDetailGridView.ItemsView.EntityType.Codice;
                ViewSettings viewSettings = MasterDetailGridView.DataService.GetViewSettings();
                if (!viewSettings.EntityTypes.ContainsKey(entTypeCode))
                    viewSettings.EntityTypes.Add(entTypeCode, new EntityTypeViewSettings());

                if (!viewSettings.EntityTypes[entTypeCode].GridViewSettings.ColumnsViewSettings.ContainsKey(codiceAtt))
                    viewSettings.EntityTypes[entTypeCode].GridViewSettings.ColumnsViewSettings.Add(codiceAtt, new GridColumnViewSettings());

                viewSettings.EntityTypes[entTypeCode].GridViewSettings.ColumnsViewSettings[codiceAtt].Codice = codiceAtt;
                viewSettings.EntityTypes[entTypeCode].GridViewSettings.ColumnsViewSettings[codiceAtt].Width = e.Width;


                MasterDetailGridView.DataService.SetViewSettings(viewSettings);
            }
        }

        private void MasterDataGrid_GroupCollapsing(object sender, GroupChangingEventArgs e)
        {
            if (!MouseIndentButtonPressed)
                e.Cancel = true;
        }

        private void MasterDataGrid_GroupExpanding(object sender, GroupChangingEventArgs e)
        {
            if (!MouseIndentButtonPressed)
                e.Cancel = true;
        }

        private void MasterDataGrid_LayoutUpdated(object sender, EventArgs e)
        {
            MouseIndentButtonPressed = false;
        }

        private void MasterDataGrid_GroupCollapsed(object sender, GroupChangedEventArgs e)
        {
            //if (_testSelection)
            //    return;

            //SelectItems();
        }

        private void MasterDataGrid_GroupExpanded(object sender, GroupChangedEventArgs e)
        {
            //if (_testSelection)
            //    return;

            //SelectItems();
        }
     
        /// <summary>
        /// Arriva dopo aver realizzato gli item della griglia
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemsView_ItemsLoaded(object sender, EventArgs e)
        {
            if (MasterDataGrid.View == null)
                return;

            Debug.WriteLine("ItemsView_ItemsLoaded");


            if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ExpandNewEntitiesGroup) == EntitiesListMasterDetailViewCommands.ExpandNewEntitiesGroup)
            {
                Debug.WriteLine("ExpandNewEntitiesGroup");
                foreach (Guid entId in MasterDetailGridView.ItemsView.CheckedEntitiesId)
                {
                    ExpandGroupsByEntityId(entId);
                }

                MasterDataGrid.View.Refresh();
                MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.ExpandNewEntitiesGroup;
            }


            if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView) == EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView)
            {

                int selectedIndex = MasterDetailGridView.ItemsView.SelectedIndex;
                if (0 <= selectedIndex && selectedIndex < MasterDetailGridView.ItemsView.VirtualEntities.Count)
                {
                    VirtualListItem<EntityView> virtEntity = MasterDetailGridView.ItemsView.VirtualEntities[selectedIndex];
                    var rowIndex = MasterDataGrid.ResolveToRowIndex(virtEntity);
                    //var rowIndex = GridIndexResolver.ResolveToRowIndex(MasterDataGrid, MasterDetailGridView.ItemsView.SelectedIndex);
                    if (rowIndex < 0)
                        return;

                    ///////////////////////////////////////////////////
                    /////Workaround syncfusion per ScrollInView in caso di AllowFrozenGroupHeaders= true
                    var visibleLastRowIndex = MasterDataGrid.GetVisualContainer().ScrollRows.LastBodyVisibleLineIndex;
                    if (MasterDataGrid.AllowFrozenGroupHeaders && visibleLastRowIndex > rowIndex)
                        rowIndex = rowIndex - MasterDataGrid.GroupColumnDescriptions.Count;
                    /////////////////////////////////////////////////////

                    var columnIndex = MasterDataGrid.ResolveToStartColumnIndex();

                    if (rowIndex >= 0 && columnIndex >= 0)
                    {
                        MasterDataGrid.ScrollInView(new RowColumnIndex(rowIndex, columnIndex));
                    }
                }
            }

            if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.SelectAll) == EntitiesListMasterDetailViewCommands.SelectAll)
            {
                MasterDataGrid.SelectAll();
            }
            else if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.SelectRows) == EntitiesListMasterDetailViewCommands.SelectRows)
            {
                SelectItems();
            }

            MasterDetailGridView.ItemsView.PendingCommand = EntitiesListMasterDetailViewCommands.Nessuno;
            MasterDetailGridView.WindowService.ShowWaitCursor(false);
        }

        /// <summary>
        /// Arriva prima di aver realizzato gli elementi della griglia
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemsView_ItemsLoading(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("ItemsView_ItemsLoading");

                //if (MasterDetailGridView.MainOperation.IsProjectClosing())
                //    return;


                if (MasterDataGrid.View == null)
                    return;

                bool beginInit = false;
                GridSelectionControllerExt selectionController = (MasterDataGrid.SelectionController as GridSelectionControllerExt);
                List<string> groupsKeySelected = null;
                RowColumnIndex rcIndex = new RowColumnIndex();


                //if (MasterDetailGridView.ItemsView.PendingCommand == EntitiesListMasterDetailViewCommands.AfterMultipleModify)
                //{
                //    //salvo temporaneamente le keys dei gruppi selezionati per ripristinarli alla fine dell'operazione
                //    if (MasterDataGrid.View.TopLevelGroup != null)
                //        groupsKeySelected = MasterDataGrid.View.TopLevelGroup.DisplayElements.Where(x => x.IsGroups && selectionController.IsGroupSelected(x as Group)).Select(x => (x as Group).Key as string).ToList();


                //    //salvo temporaneamente il currentItem
                //    rcIndex = selectionController.CurrentCellManager.CurrentRowColumnIndex;
                //}


                if (MasterDetailGridView.ItemsView.PendingCommand != EntitiesListMasterDetailViewCommands.Nessuno)
                {
                    MasterDataGrid.View.BeginInit();
                    beginInit = true;
                }

                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.UpdateTableSummaryRow) == EntitiesListMasterDetailViewCommands.UpdateTableSummaryRow)
                {
                    if (MasterDetailGridView.HasTableSummaryRow())
                    {
                        if (!MasterDataGrid.TableSummaryRows.Any(x => x.Title == MasterDetailGrid.GridTableSummaryRowTitle))
                            MasterDataGrid.TableSummaryRows.Add(new GridTableSummaryRow() { Title = MasterDetailGrid.GridTableSummaryRowTitle, Position = TableSummaryRowPosition.Top, ShowSummaryInRow = false });
                    }
                    else
                    {
                        var row = MasterDataGrid.TableSummaryRows.FirstOrDefault(x => x.Title == MasterDetailGrid.GridTableSummaryRowTitle);
                        if (row != null) 
                            MasterDataGrid.TableSummaryRows.Remove(row);
                    }

                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.UpdateTableSummaryRow;
                }


                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.UpdateGridColumns) == EntitiesListMasterDetailViewCommands.UpdateGridColumns)
                {
                    UpdateGridColumns();
                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.UpdateGridColumns;
                }
                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ApplyGroups) == EntitiesListMasterDetailViewCommands.ApplyGroups)
                {
                    //scopo: Prima di raggruppare si deve almeno aggiornare il numero degli items della griglia. Non occorre che siano realizzate
                    MasterDetailGridView.EnsureRows(0);
                    //fine scopo

                    OrderGridColumns();

                    MasterDataGrid.GroupColumnDescriptions.Clear();
                    foreach (AttributoGroupView attGroupView in MasterDetailGridView.ItemsView.RightPanesView.GroupView.Items)
                    {
                        string groupName = attGroupView.Attributo.Codice;

                        attGroupView.UpdateOrderedItems();//per ordinare gli item raggruppati

                        string colName = ColumnAttributiMappingName[groupName];

                        GroupConverter groupConverter = new GroupConverter(MasterDetailGridView.ItemsView);
                        MasterDataGrid.GroupColumnDescriptions.Add(new GroupColumnDescription() { ColumnName = colName, Converter = groupConverter });


                    }

                    MasterDetailGridView.ItemsView.CreateAndFillGroupData();

                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.ApplyGroups;

                }
                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ExpandAll) == EntitiesListMasterDetailViewCommands.ExpandAll)
                {
                    MasterDataGrid.ExpandAllGroup();
                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.ExpandAll;
                }
                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.CollapseAll) == EntitiesListMasterDetailViewCommands.CollapseAll)
                {
                    MasterDataGrid.CollapseAllGroup();
                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.CollapseAll;
                }

                //if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups) == EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups)
                //{
                //    Debug.WriteLine("ExpandCheckedEntityGroups");
                //    foreach (Guid entId in MasterDetailGridView.ItemsView.CheckedEntitiesId)
                //    {
                //        ExpandGroupsByEntityId(entId);
                //    }
                //    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                //}

                if (beginInit)
                    MasterDataGrid.View.EndInit();


                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups) == EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups)
                {
                    MasterDataGrid.View.BeginInit();

                    Debug.WriteLine("ExpandCheckedEntityGroups");
                    foreach (Guid entId in MasterDetailGridView.ItemsView.CheckedEntitiesId)
                    {
                        ExpandGroupsByEntityId(entId);
                    }

                    MasterDataGrid.View.EndInit();

                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                }

                if (beginInit)                
                    MasterDataGrid.View.Refresh();

                //if (MasterDetailGridView.ItemsView.PendingCommand == EntitiesListMasterDetailViewCommands.AfterMultipleModify)
                //{
                //    //Ripristino la selezione di gruppi
                //    if (groupsKeySelected != null)
                //    {
                //        foreach (string grKey in groupsKeySelected)
                //        {
                //            var gr = MasterDataGrid.View.TopLevelGroup.DisplayElements.FirstOrDefault(x => x.IsGroups && (x as Group).Key == grKey);
                //            selectionController.SelectGroupRow(gr as Group);
                //        }
                //    }

                //    //Ripristino il current item
                //    //MasterDataGrid.MoveCurrentCell(rcIndex, false);

                //    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.AfterMultipleModify;
                //}


                //

            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);

            }
        }


        public RowColumnIndex GetSelectedRowColumnIndex()
        {
            int selectedIndex = MasterDetailGridView.ItemsView.SelectedIndex;
            if (0 <= selectedIndex && selectedIndex < MasterDetailGridView.ItemsView.VirtualEntities.Count)
            {
                VirtualListItem<EntityView> virtEntity = MasterDetailGridView.ItemsView.VirtualEntities[selectedIndex];
                var rowIndex = MasterDataGrid.ResolveToRowIndex(virtEntity);
                //var rowIndex = GridIndexResolver.ResolveToRowIndex(MasterDataGrid, MasterDetailGridView.ItemsView.SelectedIndex);
                if (rowIndex < 0)
                    return RowColumnIndex.Empty;

                var columnIndex = MasterDataGrid.ResolveToStartColumnIndex();

                if (rowIndex >= 0 && columnIndex >= 0)
                {
                    return new RowColumnIndex(rowIndex, columnIndex);
                }
            }
            return RowColumnIndex.Empty;
        }

        public void SubscribeViewEvents()
        {
            MasterDetailGridView.ItemsView.ItemsLoading += ItemsView_ItemsLoading;
            MasterDetailGridView.ItemsView.ItemsLoaded += ItemsView_ItemsLoaded;
        }

        public void UnsubscribeViewEvents()
        {
            MasterDetailGridView.ItemsView.ItemsLoading -= ItemsView_ItemsLoading;
            MasterDetailGridView.ItemsView.ItemsLoaded -= ItemsView_ItemsLoaded;
        }

        public void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            MasterDetailGridExcelUtility excelUtils = new MasterDetailGridExcelUtility();
            excelUtils.Grid = MasterDataGrid;
            excelUtils.View = MasterDetailGridView;

            excelUtils.Export();
        }

        public void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {

            if (MasterDetailGridView.ItemsView.IsMultipleModify)
                return;

            double horizontalChange = e.HorizontalChange / MasterDetailScale.ScaleX;

            MasterDetailGridView.ItemsView.EntityType.DetailAttributoEtichettaWidth += horizontalChange;
            MasterDetailGridView.ItemsView.AttributiEntities.Load(MasterDetailGridView.ItemsView.SelectedEntityView, true);

        }

        public void SelectItems()
        {
            Debug.WriteLine("SelectItems");
            SelectItems5();
        }

        //public void SelectItems2()
        //{
        //    //scopo: selezionare le righe checkate man mano che vengono visualizzate
        //    //Fatta in maniera asincona altrimenti rallenta lo scorrimento
        //    if (MasterDetailGridView.IsSelectingItems)
        //        return;

        //    MasterDetailGridView.IsSelectingItems = true;

        //    //var tokenSource = new CancellationTokenSource();
        //    //CancellationToken ct = tokenSource.Token;

        //    try
        //    {

        //        //await Task.Run(() =>
        //        //{

        //        //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        //    {
        //        List<VirtualListItem<EntityView>> virtEntsToSelect = new List<VirtualListItem<EntityView>>();

        //        VirtualListItem<EntityView> currentItem = MasterDataGrid.CurrentItem as VirtualListItem<EntityView>;

        //        Guid currentItemId = Guid.Empty;
        //        if (MasterDataGrid.CurrentItem != null)
        //        {
        //            int currentItemIndex = MasterDetailGridView.ItemsView.VirtualEntities.IndexOf(currentItem);
        //            if (0 <= currentItemIndex && currentItemIndex < MasterDetailGridView.ItemsView.FilteredEntitiesId.Count)
        //                currentItemId = MasterDetailGridView.ItemsView.FilteredEntitiesId[currentItemIndex];
        //        }

        //        //if (MasterDataGrid.CurrentItem != null)
        //        //{
        //        //    MasterDataGrid.SelectionController.ClearSelections(true);//non cancella la selezione del corrente (non va bene per togliere righe alla selezione)
        //        //}
        //        //else
        //        MasterDataGrid.SelectionController.ClearSelections(false);//cancella la selezione anche del corrente e il corrente (rallenta l'attivazione della riga)


        //        if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Any())
        //        {


        //            //if (MasterDataGrid.View != null)
        //            //    MasterDataGrid.View.BeginInit();

        //            //foreach (VirtualListItem<EntityView> virtEntity in MasterDetailGridView.ItemsView.VirtualEntities)
        //            //{
        //            //    //if (ct.IsCancellationRequested)
        //            //    //    break;

        //            //    if (virtEntity.Data != null)
        //            //    {
        //            //        EntityView entView = virtEntity.Data as EntityView;
        //            //        if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(entView.Id))
        //            //        {
        //            //            virtEntsToSelect.Add(virtEntity);
        //            //        }
        //            //    }
        //            //}

        //            int index = -1;
        //            foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //            {
        //                if (MasterDetailGridView.ItemsView.FilteredIndexes.ContainsKey(id))
        //                    index = MasterDetailGridView.ItemsView.FilteredIndexes[id];

        //                if (0 <= index && index < MasterDetailGridView.ItemsView.VirtualEntities.Count())
        //                    virtEntsToSelect.Add(MasterDetailGridView.ItemsView.VirtualEntities[index]);
        //            }

        //            //scopo: impostare il corrente come ultimo della il primo della selezione

        //            if (virtEntsToSelect.Any())
        //            {
        //                if (/*currentItem != null && */MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(currentItemId))
        //                    virtEntsToSelect.Add(currentItem);
        //                else
        //                    virtEntsToSelect.Add(virtEntsToSelect.FirstOrDefault());
        //            }

        //            ObservableCollection<object> selectedItems = new ObservableCollection<object>(virtEntsToSelect);

        //            MasterDataGrid.SelectedItems = selectedItems;

                

        //            ////////////////////////////////////////////////////////
        //            //Seleziono i gruppi in base alle foglie selezionate
        //            HashSet<string> groupsKey = new HashSet<string>();
                    
        //            foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //            {
        //                List<string> keys = MasterDetailGridView.ItemsView.GetGroupsKeyById(id);

        //                for (int i=0;i<keys.Count;i++)
        //                {
        //                    string groupKey = MasterDetailGridView.ItemsView.RightPanesView.GroupView.JoinGroupKeys(keys.Take(i+1).ToArray());
        //                    groupsKey.Add(groupKey);
        //                }
        //            }

        //            HashSet<Group> groups = GetGroupsByKeys(groupsKey);
        //            foreach (Group gr in groups)
        //            {
        //                List<Guid> groupIds = new List<Guid>();
        //                GetGroupRecordsId(gr, ref groupIds);

        //                Guid id = groupIds.FirstOrDefault(item => !MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(item));
        //                if (id == null || id == Guid.Empty)
        //                {
        //                    //Get the corresponding start index of record by getting it from DisplayElements .
        //                    var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr);

        //                    //Resolve the recordIndex to RowIndex.
        //                    var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //                    //Select the rows from corresponding start and end row index
        //                    MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);

        //                }

        //            }

        //            ////////////////////////////////////////////

        //        }

        //        //if (!ct.IsCancellationRequested)
        //        //{
        //        //    MasterDetailGridView.ItemsView.RightPanesView.GroupView.Update();
        //        //}


        //        //    }));
        //        //}, tokenSource.Token);

        //        //tokenSource.Cancel();

        //        MasterDetailGridView.IsSelectingItems = false;

        //        if (virtEntsToSelect.Any() && MasterDataGrid.CurrentItem != null)
        //            MasterDetailGridView.CurrentItem = virtEntsToSelect.Last();//currentItem;

        //    }
        //    finally
        //    {
        //        //tokenSource.Dispose();
        //    }
        //}


//        public async void SelectItemsAsync()
//        {
//            //scopo: selezionare le righe checkate man mano che vengono visualizzate
//            //Fatta in maniera asincona altrimenti rallenta lo scorrimento
//            if (MasterDetailGridView.IsSelectingItems)
//                return;

//            MasterDetailGridView.IsSelectingItems = true;

//            var tokenSource = new CancellationTokenSource();
//            CancellationToken ct = tokenSource.Token;

//            try
//            {

//                VirtualListItem<EntityView> currentItem1 = await Task.Run(() =>
//                {

//                    List<VirtualListItem<EntityView>> virtEntsToSelect = new List<VirtualListItem<EntityView>>();

//                    //var dispatcherOp = Application.Current.Dispatcher.Invoke((Action)(() =>
//                    Application.Current.Dispatcher.Invoke((Action)(() =>
//                    {
//                        VirtualListItem<EntityView> currentItem = MasterDataGrid.CurrentItem as VirtualListItem<EntityView>;

//                        Guid currentItemId = Guid.Empty;
//                        if (MasterDataGrid.CurrentItem != null)
//                        {
//                            int currentItemIndex = MasterDetailGridView.ItemsView.VirtualEntities.IndexOf(currentItem);
//                            if (0 <= currentItemIndex && currentItemIndex < MasterDetailGridView.ItemsView.FilteredEntitiesId.Count)
//                                currentItemId = MasterDetailGridView.ItemsView.FilteredEntitiesId[currentItemIndex];
//                        }

//                        //if (MasterDataGrid.CurrentItem != null)
//                        //{
//                        //    MasterDataGrid.SelectionController.ClearSelections(true);//non cancella la selezione del corrente (non va bene per togliere righe alla selezione)
//                        //}
//                        //else
//                        MasterDataGrid.SelectionController.ClearSelections(false);//cancella la selezione anche del corrente e il corrente (rallenta l'attivazione della riga)


//                        if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Any())
//                        {


//                            //if (MasterDataGrid.View != null)
//                            //    MasterDataGrid.View.BeginInit();

//                            //foreach (VirtualListItem<EntityView> virtEntity in MasterDetailGridView.ItemsView.VirtualEntities)
//                            //{
//                            //    //if (ct.IsCancellationRequested)
//                            //    //    break;

//                            //    if (virtEntity.Data != null)
//                            //    {
//                            //        EntityView entView = virtEntity.Data as EntityView;
//                            //        if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(entView.Id))
//                            //        {
//                            //            virtEntsToSelect.Add(virtEntity);
//                            //        }
//                            //    }
//                            //}

//                            int index = -1;
//                            foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
//                            {
//                                if (MasterDetailGridView.ItemsView.FilteredIndexes.ContainsKey(id))
//                                    index = MasterDetailGridView.ItemsView.FilteredIndexes[id];

//                                if (0 <= index && index < MasterDetailGridView.ItemsView.VirtualEntities.Count())
//                                    virtEntsToSelect.Add(MasterDetailGridView.ItemsView.VirtualEntities[index]);
//                            }

//                            //scopo: impostare il corrente come ultimo della il primo della selezione

//                            if (virtEntsToSelect.Any())
//                            {
//                                if (/*currentItem != null && */MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(currentItemId))
//                                    virtEntsToSelect.Add(currentItem);
//                                else
//                                    virtEntsToSelect.Add(virtEntsToSelect.FirstOrDefault());
//                            }


//                            List<VirtualListItem<EntityView>> virtEntsToSelectVisible = new List<VirtualListItem<EntityView>>(virtEntsToSelect.Where(item => MasterDetailGridView.VisibleVirtualListItem.Contains(item)));

//                            ObservableCollection<object> selectedItems = new ObservableCollection<object>(virtEntsToSelectVisible);

//                            MasterDataGrid.SelectedItems = selectedItems;



//                            ////////////////////////////////////////////////////////
//                            //Seleziono i gruppi in base alle foglie selezionate
//                            HashSet<string> groupsKey = new HashSet<string>();

//                            foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
//                            {
//                                List<string> keys = MasterDetailGridView.ItemsView.GetGroupsKeyById(id);

//                                for (int i = 0; i < keys.Count; i++)
//                                {
//                                    string groupKey = MasterDetailGridView.ItemsView.RightPanesView.GroupView.JoinGroupKeys(keys.Take(i + 1).ToArray());
//                                    groupsKey.Add(groupKey);
//                                }
//                            }

//                            HashSet<Group> groups = GetGroupsByKeys(groupsKey);
//                            foreach (Group gr in groups)
//                            {
//                                List<Guid> groupIds = new List<Guid>();
//                                GetGroupRecordsId(gr, ref groupIds);

//                                Guid id = groupIds.FirstOrDefault(item => !MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(item));
//                                if (id == null || id == Guid.Empty)
//                                {
//                                    //Get the corresponding start index of record by getting it from DisplayElements .
//                                    var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr);

//                                    //Resolve the recordIndex to RowIndex.
//                                    var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

//                                    //Select the rows from corresponding start and end row index
//                                    MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);

//                                }

//                            }

//                            //////////////////////////////////////////////
//                        }

//                        if (!ct.IsCancellationRequested)
//                        {
////#if !ALE_ASYNC_GROUP
////                            MasterDetailGridView.ItemsView.RightPanesView.GroupView.Update();
////#endif
//                        }




//                        //if (virtEntsToSelect.Any() && MasterDataGrid.CurrentItem != null)
//                        //    return new DispatcherOperation() { Result = virtEntsToSelect.Last() };

//                    }));


//                    if (virtEntsToSelect.Any())
//                        return virtEntsToSelect.Last();

//                    //if (virtEntsToSelect.Any() && MasterDataGrid.CurrentItem != null)
//                    //    return virtEntsToSelect.Last().Data;


//                    return null;

//                }, tokenSource.Token);

//                tokenSource.Cancel();

//                MasterDetailGridView.IsSelectingItems = false;

//                //if (virtEntsToSelect.Any() && MasterDataGrid.CurrentItem != null)
//                //    MasterDetailGridView.CurrentItem = virtEntsToSelect.Last();//currentItem;

//                if (currentItem1 != null && MasterDataGrid.CurrentItem != null)
//                    MasterDetailGridView.CurrentItem = currentItem1;//currentItem;

//            }
//            finally
//            {
//                tokenSource.Dispose();
//            }


//        }

        //public async Task<SelectItemsResponse> SelectItems3Async(VirtualListItem<EntityView> currentItem)
        //{
        //    Debug.WriteLine("SelectItems3Async");

        //    SelectItemsResponse selectedItemsRes = new SelectItemsResponse();
        //    ObservableCollection<object> selectedItems1 = new ObservableCollection<object>();

        //    var tokenSource = new CancellationTokenSource();
        //    CancellationToken ct = tokenSource.Token;
        //    try
        //    {
        //        selectedItemsRes = await Task.Run(() =>
        //        {
                    
        //            List<VirtualListItem<EntityView>> virtEntsToSelect = new List<VirtualListItem<EntityView>>();

        //            Guid currentItemId = Guid.Empty;
        //            if (currentItem != null)
        //            {
        //                int currentItemIndex = MasterDetailGridView.ItemsView.VirtualEntities.IndexOf(currentItem);
        //                if (0 <= currentItemIndex && currentItemIndex < MasterDetailGridView.ItemsView.FilteredEntitiesId.Count)
        //                    currentItemId = MasterDetailGridView.ItemsView.FilteredEntitiesId[currentItemIndex];
        //            }

        //            if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Any())
        //            {

        //                int index = -1;
        //                foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //                {
        //                    if (MasterDetailGridView.ItemsView.FilteredIndexes.ContainsKey(id))
        //                        index = MasterDetailGridView.ItemsView.FilteredIndexes[id];

        //                    if (0 <= index && index < MasterDetailGridView.ItemsView.VirtualEntities.Count())
        //                        virtEntsToSelect.Add(MasterDetailGridView.ItemsView.VirtualEntities[index]);
        //                }

        //                //scopo: impostare il corrente come ultimo della il primo della selezione

        //                if (virtEntsToSelect.Any())
        //                {
        //                    if (/*currentItem != null && */MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(currentItemId))
        //                        virtEntsToSelect.Add(currentItem);
        //                    else
        //                        virtEntsToSelect.Add(virtEntsToSelect.FirstOrDefault());
        //                }


        //                List<VirtualListItem<EntityView>> virtEntsToSelectVisible = new List<VirtualListItem<EntityView>>(virtEntsToSelect.Where(item => MasterDetailGridView.VisibleVirtualListItem.Contains(item)));

        //                selectedItemsRes.SelectedItems = new ObservableCollection<object>(virtEntsToSelectVisible); 
        //                //selectedItems = new ObservableCollection<object>(virtEntsToSelectVisible);

        //                //////////////////////////////////////////////////////////
        //                ////Seleziono i gruppi in base alle foglie selezionate
        //                HashSet<string> groupsKey = new HashSet<string>();

        //                foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //                {
        //                    List<string> keys = MasterDetailGridView.ItemsView.GetGroupsKeyById(id);

        //                    for (int i = 0; i < keys.Count; i++)
        //                    {
        //                        string groupKey = MasterDetailGridView.ItemsView.RightPanesView.GroupView.JoinGroupKeys(keys.Take(i + 1).ToArray());
        //                        groupsKey.Add(groupKey);
        //                    }
        //                }

        //                HashSet<Group> groups = GetGroupsByKeys(groupsKey);


        //                selectedItemsRes.SelectedGroups = groups;
        //                //foreach (Group gr in groups)
        //                //{
        //                //    List<Guid> groupIds = new List<Guid>();
        //                //    GetGroupRecordsId(gr, ref groupIds);

        //                //    Guid id = groupIds.FirstOrDefault(item => !MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(item));
        //                //    if (id == null || id == Guid.Empty)
        //                //    {
        //                //        //Get the corresponding start index of record by getting it from DisplayElements .
        //                //        var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr);

        //                //        //Resolve the recordIndex to RowIndex.
        //                //        var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //                //        //Select the rows from corresponding start and end row index
        //                //        MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);

        //                //    }

        //                //}

        //                ////////////////////////////////////////////////
        //            }

        //            if (!ct.IsCancellationRequested)
        //            {
        //                //MasterDetailGridView.ItemsView.RightPanesView.GroupView.Update();
        //            }

        //            if (virtEntsToSelect.Any())
        //                selectedItemsRes.CurrentItem = virtEntsToSelect.Last();


        //            return selectedItemsRes;

        //        }, tokenSource.Token);

        //        tokenSource.Cancel();

        //    }
        //    finally
        //    {
        //        tokenSource.Dispose();
        //    }

        //    return selectedItemsRes;
        //}

        //public async Task<SelectItemsResponse> SelectItems4Async(VirtualListItem<EntityView> currentItem)
        //{
        //    SelectItemsResponse selectedItemsRes = new SelectItemsResponse();
        //    ObservableCollection<object> selectedItems1 = new ObservableCollection<object>();

        //    try
        //    {
        //        selectedItemsRes = await Task.Run(() =>
        //        {

        //            List<VirtualListItem<EntityView>> virtEntsToSelect = new List<VirtualListItem<EntityView>>();


        //            Guid currentItemId = Guid.Empty;
        //            if (currentItem != null)
        //            {
        //                int currentItemIndex = MasterDetailGridView.ItemsView.VirtualEntities.IndexOf(currentItem);
        //                if (0 <= currentItemIndex && currentItemIndex < MasterDetailGridView.ItemsView.FilteredEntitiesId.Count)
        //                    currentItemId = MasterDetailGridView.ItemsView.FilteredEntitiesId[currentItemIndex];
        //            }

        //            if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Any())
        //            {

        //                int index = -1;
        //                foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //                {
        //                    if (MasterDetailGridView.ItemsView.FilteredIndexes.ContainsKey(id))
        //                        index = MasterDetailGridView.ItemsView.FilteredIndexes[id];

        //                    if (0 <= index && index < MasterDetailGridView.ItemsView.VirtualEntities.Count())
        //                        virtEntsToSelect.Add(MasterDetailGridView.ItemsView.VirtualEntities[index]);
        //                }

        //                //scopo: impostare il corrente come ultimo della il primo della selezione

        //                if (virtEntsToSelect.Any())
        //                {
        //                    if (/*currentItem != null && */MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(currentItemId))
        //                        virtEntsToSelect.Add(currentItem);
        //                    else
        //                        virtEntsToSelect.Add(virtEntsToSelect.FirstOrDefault());
        //                }


        //                List<VirtualListItem<EntityView>> virtEntsToSelectVisible = new List<VirtualListItem<EntityView>>(virtEntsToSelect.Where(item => MasterDetailGridView.VisibleVirtualListItem.Contains(item)));

        //                selectedItemsRes.SelectedItems = new ObservableCollection<object>(virtEntsToSelectVisible);
        //                //selectedItems = new ObservableCollection<object>(virtEntsToSelectVisible);

        //                //////////////////////////////////////////////////////////
        //                ////Seleziono i gruppi in base alle foglie selezionate
        //                HashSet<string> groupsKey = new HashSet<string>();

        //                foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //                {
        //                    List<string> keys = MasterDetailGridView.ItemsView.GetGroupsKeyById(id);

        //                    for (int i = 0; i < keys.Count; i++)
        //                    {
        //                        string groupKey = MasterDetailGridView.ItemsView.RightPanesView.GroupView.JoinGroupKeys(keys.Take(i + 1).ToArray());
        //                        groupsKey.Add(groupKey);
        //                    }
        //                }

        //                HashSet<Group> groups = GetGroupsByKeys(groupsKey);


        //                selectedItemsRes.SelectedGroups = groups;

        //                ////////////////////////////////////////////////
        //            }

        //            if (virtEntsToSelect.Any())
        //                selectedItemsRes.CurrentItem = virtEntsToSelect.Last();


        //            return selectedItemsRes;

        //        });
        //    }
        //    finally
        //    {
        //    }

        //    return selectedItemsRes;
        //}



        //public async void SelectItems3()
        //{
        //    //scopo: selezionare le righe checkate man mano che vengono visualizzate
        //    //Fatta in maniera asincona altrimenti rallenta lo scorrimento
        //    if (MasterDetailGridView.IsSelectingItems)
        //        return;

        //    MasterDetailGridView.IsSelectingItems = true;





        //    VirtualListItem<EntityView> currentItem = MasterDataGrid.CurrentItem as VirtualListItem<EntityView>;


        //    MasterDataGrid.SelectionController.ClearSelections(true);//non cancella la selezione anche del corrente

        //    Task<SelectItemsResponse> resTask = SelectItems3Async(currentItem);
        //    SelectItemsResponse res = await resTask;



        //    //selection
        //    if (res.SelectedItems != null)
        //        MasterDataGrid.SelectedItems = res.SelectedItems;

        //    Debug.WriteLine("SelectItems3");

        //    if (res.SelectedGroups != null)
        //    {
        //        foreach (Group gr in res.SelectedGroups)
        //        {
        //            List<Guid> groupIds = new List<Guid>();
        //            GetGroupRecordsId(gr, ref groupIds);

        //            Guid id = groupIds.FirstOrDefault(item => !MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(item));
        //            if (id == null || id == Guid.Empty)
        //            {
        //                //Get the corresponding start index of record by getting it from DisplayElements .
        //                var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr);

        //                //Resolve the recordIndex to RowIndex.
        //                var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //                //Select the rows from corresponding start and end row index
        //                MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);

        //            }

        //        }
        //    }

            

        //    MasterDetailGridView.IsSelectingItems = false;

        //    ////current item
        //    //if (res.CurrentItem != null && MasterDataGrid.CurrentItem != null)
        //    //{
        //    //    MasterDetailGridView.CurrentItem = res.CurrentItem;
        //    //}
        //    if (res.CurrentItem != null && MasterDataGrid.CurrentItem != res.CurrentItem)
        //    {
        //        MasterDataGrid.CurrentItem = res.CurrentItem;
        //    }


        //}

        // Cancellation token for the latest task.
        private CancellationTokenSource _selectItemsCancellationTokenSource;

        //public async void SelectItems4()
        //{

        //    MasterDetailGridView.IsSelectingItems = true;

        //    // If a cancellation token already exists (for a previous task),
        //    // cancel it.
        //    if (this._selectItemsCancellationTokenSource != null)
        //        this._selectItemsCancellationTokenSource.Cancel();

        //    // Create a new cancellation token for the new task.
        //    this._selectItemsCancellationTokenSource = new CancellationTokenSource();
        //    CancellationToken cancellationToken = this._selectItemsCancellationTokenSource.Token;
            
            
        //    VirtualListItem<EntityView> currentItem = MasterDataGrid.CurrentItem as VirtualListItem<EntityView>;


        //    //bool exceptCurrentRow = currentItem != null && currentItem.Data != null && MasterDetailGridView.ItemsView.FilteredEntitiesId.Contains(currentItem.Data.Id);
        //    //Perchè questa funzione non serve più???
        //    //MasterDataGrid.SelectionController.ClearSelections(exceptCurrentRow);//non cancella la selezione anche del corrente


        //    Task<SelectItemsResponse> resTask = SelectItems4Async(currentItem);
        //    SelectItemsResponse res = await resTask;

        //    if (cancellationToken.IsCancellationRequested)
        //        return;

        //    //MasterDetailGridView.IsSelectingItems = true;

        //    //selection
        //    if (res.SelectedItems != null)
        //        MasterDataGrid.SelectedItems = res.SelectedItems;

        //    if (res.SelectedGroups != null)
        //    {
        //        foreach (Group gr in res.SelectedGroups)
        //        {
        //            List<Guid> groupIds = new List<Guid>();
        //            GetGroupRecordsId(gr, ref groupIds);

        //            Guid id = groupIds.FirstOrDefault(item => !MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(item));
        //            if (id == null || id == Guid.Empty)
        //            {
        //                //Get the corresponding start index of record by getting it from DisplayElements .
        //                var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr);

        //                //Resolve the recordIndex to RowIndex.
        //                var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //                //Select the rows from corresponding start and end row index
        //                MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);

        //            }

        //        }
            
        //    }

        //    this._selectItemsCancellationTokenSource = null;

        //    MasterDetailGridView.IsSelectingItems = false;

        //    ////current item
        //    //if (res.CurrentItem != null && MasterDataGrid.CurrentItem != null)
        //    //{
        //    //   MasterDetailGridView = res.CurrentItem;
        //    //}
        //    if (res.CurrentItem != null)
        //    {
        //        if (MasterDataGrid.CurrentItem != res.CurrentItem)
        //            MasterDataGrid.CurrentItem = res.CurrentItem;
        //        else if (MasterDetailGridView.CurrentItem != res.CurrentItem)
        //            MasterDetailGridView.CurrentItem = res.CurrentItem;
        //    }


        //}

        public void SelectItems5()
        {

            //cerco di mantenere la riga corrente per il comando Vai a...
            MasterDataGrid.SelectionController.ClearSelections(false);

            //Perchè il fuoco venga rimesso al posto giusto questa selezione va fatta prima di tutte le altre
            SelectRecordRow(MasterDetailGridView.ItemsView.SelectedIndex);

            

            foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
            {
                int index = MasterDetailGridView.ItemsView.FilteredEntitiesId.IndexOf(id);
               
                SelectRecordRow(index);
            }
        }

        //public void SelectItemsOld()
        //{
        //    //scopo: selezionare le righe checkate man mano che vengono visualizzate
        //    //Fatta in maniera asincona altrimenti rallenta lo scorrimento
        //    if (MasterDetailGridView.IsSelectingItems)
        //        return;

        //    MasterDetailGridView.IsSelectingItems = true;

        //    //var tokenSource = new CancellationTokenSource();
        //    //CancellationToken ct = tokenSource.Token;

        //    try
        //    {

        //        //await Task.Run(() =>
        //        //{

        //        //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //        //    {
        //        List<VirtualListItem<EntityView>> virtEnts = new List<VirtualListItem<EntityView>>();

        //        //object currentItem = MasterDataGrid.CurrentItem;
        //        //if (MasterDataGrid.CurrentItem != null)
        //        //{
        //        //    MasterDataGrid.SelectionController.ClearSelections(true);//non cancella la selezione del corrente (non va bene per togliere righe alla selezione)
        //        //}
        //        //else
        //        MasterDataGrid.SelectionController.ClearSelections(false);//cancella la selezione anche del corrente e il corrente (rallenta l'attivazione della riga)


        //        if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Any())
        //        {


        //            //if (MasterDataGrid.View != null)
        //            //    MasterDataGrid.View.BeginInit();
        //            int index = 0;
        //            for (index=0; index< MasterDetailGridView.ItemsView.VirtualEntities.Count;  index++)
        //            {

        //                VirtualListItem<EntityView> virtEntity = MasterDetailGridView.ItemsView.VirtualEntities[index];
        //                //if (ct.IsCancellationRequested)
        //                //    break;

        //                Guid entId = MasterDetailGridView.ItemsView.FilteredEntitiesId[index];

        //                //if (virtEntity.Data != null)
        //                //{
        //                    //EntityView entView = virtEntity.Data as EntityView;
        //                    if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(entId))
        //                    {
        //                        virtEnts.Add(virtEntity);
        //                    }


        //                    //if (!MasterDataGrid.SelectedItems.Contains(virtEntity))
        //                    //{
        //                    //    EntityView entView = virtEntity.Data as EntityView;
        //                    //    if (MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(entView.Id))
        //                    //        MasterDataGrid.SelectedItems.Add(virtEntity);
        //                    //}
        //                    //else
        //                    //{
        //                    //    EntityView entView = virtEntity.Data as EntityView;
        //                    //    if (!MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(entView.Id))
        //                    //        MasterDataGrid.SelectedItems.Remove(virtEntity);
        //                    //}

        //                //}
        //            }

        //            virtEnts.Add(virtEnts.FirstOrDefault());
        //            //virtEnts.RemoveAt(0);

        //            MasterDataGrid.SelectedItems = new ObservableCollection<object>(virtEnts);


        //            ////////////////////////////////////////////////////////
        //            //Seleziono i gruppi in base alle foglie selezionate
        //            HashSet<string> groupsKey = new HashSet<string>();

        //            foreach (Guid id in MasterDetailGridView.ItemsView.CheckedEntitiesId)
        //            {
        //                List<string> keys = MasterDetailGridView.ItemsView.GetGroupsKeyById(id);

        //                for (int i = 0; i < keys.Count; i++)
        //                {
        //                    string groupKey = MasterDetailGridView.ItemsView.RightPanesView.GroupView.JoinGroupKeys(keys.Take(i + 1).ToArray());
        //                    groupsKey.Add(groupKey);
        //                }
        //            }

        //            HashSet<Group> groups = GetGroupsByKeys(groupsKey);
        //            foreach (Group gr in groups)
        //            {
        //                List<Guid> groupIds = new List<Guid>();
        //                GetGroupRecordsId(gr, ref groupIds);

        //                Guid id = groupIds.FirstOrDefault(item => !MasterDetailGridView.ItemsView.CheckedEntitiesId.Contains(item));
        //                if (id == null || id == Guid.Empty)
        //                {
        //                    //Get the corresponding start index of record by getting it from DisplayElements .
        //                    var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr);

        //                    //Resolve the recordIndex to RowIndex.
        //                    var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //                    //Select the rows from corresponding start and end row index
        //                    MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);

        //                }

        //            }

        //            ////////////////////////////////////////////

        //        }

        //        //if (!ct.IsCancellationRequested)
        //        //{
        //        //    MasterDetailGridView.ItemsView.RightPanesView.GroupView.Update();
        //        //}


        //        //    }));
        //        //}, tokenSource.Token);

        //        //tokenSource.Cancel();
        //        MasterDetailGridView.IsSelectingItems = false;

        //        if (virtEnts.Any() && MasterDataGrid.CurrentItem != null)
        //            MasterDetailGridView.CurrentItem = virtEnts.Last();
        //    }
        //    finally
        //    {
        //        //tokenSource.Dispose();
        //    }

        //}

        public void MasterDataGrid_ItemsSourceChanged(object sender, GridItemsSourceChangedEventArgs e)
        {
            try
            {
                UpdateGridColumns();
                MasterDetailGridView.ItemsView.UpdateCache(true);
            }
            catch (Exception exc)
            {

            }
        }

        bool _clearCheckedEntities = false;
        public void MasterDataGrid_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            try
            {
                //int rowIndex = e.CurrentRowColumnIndex.RowIndex;
                //SetCurrentGroupKey(rowIndex);

                //N.B. Quando si clicca su un item la griglia non toglie la selezione degli elementi interni ai gruppi chiusi
                //Per Ovviare a questo problema svuoto gli elementi checked se non si sta aggiungendo alla selezione col ctrl
                _clearCheckedEntities = false;
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)
                    && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    //MasterDetailGridView.ItemsView.CheckedEntitiesId.Clear();
                    _clearCheckedEntities = true;
                }

            }
            catch (Exception exp)
            {

            }
        }

        public void MasterDataGrid_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            columnChooserWindow.Visibility = Visibility.Hidden;
        }

        public void MasterDataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MasterDetailGridView == null)
                return;

            //N.B. Questo codice non va bene perchè rende lento il passaggio alla sezione computo o elementi quando ci sono molti item
            //peraltro occorre aggiornare la griglia ogni volta che ci si torna perchè potrebbero esserci state modifiche alle altre sezioni
            if (MasterDetailGridView.ItemsView.IsCacheUpdated())
            {
                MasterDetailGridView.ItemsView.ClearReadyToModifyEntities();
                MasterDetailGridView.ItemsView.UpdateUI();
                return;
            }

            MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), "MasterDataGrid_Loaded");
            

            if (MasterDetailGridView.WindowService != null && MasterDataGrid.View != null)
                MasterDetailGridView.WindowService.ShowWaitCursor(true);

            try
            {
                if (columnChooserWindow.Owner == null)
                    columnChooserWindow.Owner = WindowHelpers.GetParentWindow(MasterDetailGridCtrl as DependencyObject);


                if (MasterDataGrid.View != null)
                {
                    MasterDataGrid.View.SourceCollectionChanged += View_SourceCollectionChanged; ;//per ripristinare lo scroll al "remove" o "add" di elementi

                    var visualContainer = MasterDataGrid.GetVisualContainer();
                    visualContainer.MouseLeftButtonUp += VisualContainer_MouseLeftButtonUp;// Serve per la selezione dell'intera colonna  cliccando sull'header della colonna

                }
//#if !ALE_ASYNC_GROUP
//                MasterDetailGridView.ItemsView.RightPanesView.GroupView.Update();
//#endif
                MasterDetailGridView.ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
                MasterDetailGridView.ItemsView.UpdateCache(true);

            }
            catch (Exception exc)
            {

            }
        }

        public void VisualContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                //GetVisualContainer 
                var visualContainer = MasterDataGrid.GetVisualContainer();
                var rowcolumnindex = visualContainer.PointToCellRowColumnIndex(e.GetPosition(visualContainer));
                var columnindex = MasterDataGrid.ResolveToGridVisibleColumnIndex(rowcolumnindex.ColumnIndex);
                if (columnindex < 0)
                    return;
                //Return if it is not HeaderRow
                if (MasterDataGrid.GetHeaderIndex() != rowcolumnindex.RowIndex)
                    return;
                var firstrowdata = MasterDataGrid.GetRecordAtRowIndex(MasterDataGrid.GetFirstRowIndex());
                //Get the record of LastRowIndex 
                var lastrowdata = MasterDataGrid.GetRecordAtRowIndex(MasterDataGrid.GetLastRowIndex());
                //Get the column of particular index
                var column = MasterDataGrid.Columns[columnindex];
                if (firstrowdata == null || lastrowdata == null)
                    return;
                //Select the column
                MasterDataGrid.SelectCells(firstrowdata, column, lastrowdata, column);
            }
        }

        public void View_SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //ripristina lo scroll verticale precedente (più o meno)
            if (MasterDetailGridView.ItemsView.LastVisibleRowIndex > 0)
            {
                var columnIndex = MasterDataGrid.ResolveToStartColumnIndex();
                MasterDataGrid.ScrollInView(new RowColumnIndex(MasterDetailGridView.ItemsView.LastVisibleRowIndex, columnIndex));
            }
        }

        public void ColumnChooserWindow_Closing(object sender, CancelEventArgs e)
        {
            columnChooserWindow.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
        
        

#region Copy&Paste

        public void MasterDataGrid_GridCopyContent(object sender, GridCopyPasteEventArgs e)
        {
            MasterDetailGridView.ItemsView.CopyClipboardEntities();
            e.Handled = true;
        }
        //public void CopySelection_Click(object sender, RoutedEventArgs e)
        //{
        //    List<GridCellInfo> selectedCells = MasterDataGrid.GetSelectedCells();

        //    if (!selectedCells.Any())
        //        return;

        //    HashSet<int> columnsIndex = new HashSet<int>(selectedCells.Select(item => MasterDataGrid.Columns.IndexOf(item.Column)));
        //    int nColumns = columnsIndex.Count;

        //    IEnumerable<int> indexes = selectedCells.Select(item =>
        //    {
        //        VirtualListItem<EntityView> virtEntityView = item.RowData as VirtualListItem<EntityView>;
        //        return virtEntityView.Index;
        //    });

        //    HashSet<Guid> entsId = new HashSet<Guid>();
        //    foreach (int index in indexes)
        //        entsId.Add(MasterDetailGridView.ItemsView.FilteredEntitiesId[index]);


        //    List<string> attsCode = new List<string>();
        //    if (selectedCells.Count == (entsId.Count * nColumns))
        //    {
        //        foreach (int i in columnsIndex)
        //        {
        //            //attsCode.Add(MasterDetailGridView.ItemsView.EntityType.AttributiMasterCodes[EntityView.GetAttributiMasterCodesIndexByMappingName(MasterDataGrid.Columns[i].MappingName)]);
        //            attsCode.Add(MasterDetailGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(MasterDataGrid.Columns[i].MappingName));
        //        }
        //        MasterDetailGridView.ItemsView.CopyClipboardEntities(false, entsId, attsCode);
        //    }
        //    else
        //    {
        //        MasterDetailGridView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileCopiaDiQuestaSelezione"));
        //    }

        //}

        //private void MasterDataGrid_CopyGridCellContent(object sender, GridCopyPasteCellEventArgs e)
        //{
        //}
        //public void Copy_Click(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailGridView.ItemsView.CopyEntities();
        //}

        //public void Paste_Click(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailGridView.ItemsView.PasteClipboard();
        //}

        //public void CopyText_Click(object sender, RoutedEventArgs e)
        //{
        //    MasterDetailGridView.ItemsView.CopyTextEntities();
        //}
#endregion

#region Selection

        /// <summary>
        /// Scopo: non selezionare quando si preme la freccetta di apertura del gruppo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MasterDataGrid_SelectionChanging(object sender, GridSelectionChangingEventArgs e)
        {
            if (e == null)
                return;

            MasterDetailGridView mdGridView = MasterDataGrid.DataContext as MasterDetailGridView;
            //if (_isSelecting)
            //    return;

            if (MouseIndentButtonPressed == true)
            {
                //MouseIndentButtonPressed = false;
                e.Cancel = true;

                //SelectItems();
                return;
            }

            if (_clearCheckedEntities)
                mdGridView.ItemsView.CheckedEntitiesId.Clear();

            //REMOVE
            //mdGridView.ItemsView.CheckedEntitiesId.Clear();
            HashSet<object> selectedGroupItems = new HashSet<object>();

            if (e.RemovedItems != null)
            {
                foreach (GridRowInfo row in e.RemovedItems)
                {
                    if (row != null && row.NodeEntry != null)
                    {
                        if (row.NodeEntry.IsGroups )
                        {
                            Group gr = row.NodeEntry as Group;
                            //if (!gr.IsExpanded)//(non so perchè) per risolvere l'errore di selezione con shift sui gruppi se il primo gruppo è aperto
                                AddGroupSourceToSelectedItems(selectedGroupItems, gr);
                        }
                        else if (row.NodeEntry is RecordEntry)
                        {
                            RecordEntry recordEntry = row.NodeEntry as RecordEntry;
                            if (recordEntry.Data != null)
                                selectedGroupItems.Add(recordEntry.Data);
                            //mdGridView.ItemsView.CheckedEntitiesId.Add((row.NodeEntry as RecordEntry).Data)
                        }
                    }
                }

                IEnumerable<int> filteredIndexes = selectedGroupItems.Select(item => (item as VirtualListItem<EntityView>).Index);
                foreach (int index in filteredIndexes)
                    mdGridView.ItemsView.CheckedEntitiesId.Remove(mdGridView.ItemsView.FilteredEntitiesId[index]);
            }

            //ADD
            selectedGroupItems.Clear();
            if (e.AddedItems != null)
            {
                foreach (GridRowInfo row in e.AddedItems)
                {
                    if (row != null && row.NodeEntry != null)
                    {
                        if (row.NodeEntry.IsGroups)
                        {
                            AddGroupSourceToSelectedItems(selectedGroupItems, row.NodeEntry as Group);
                        }
                        else if (row.NodeEntry is RecordEntry)
                        {
                            RecordEntry recordEntry = row.NodeEntry as RecordEntry;
                            if (recordEntry.Data != null)
                                selectedGroupItems.Add(recordEntry.Data);
                            //mdGridView.ItemsView.CheckedEntitiesId.Add((row.NodeEntry as RecordEntry).Data)
                        }
                    }
                }

                IEnumerable<int> filteredIndexes = selectedGroupItems.Select(item => (item as VirtualListItem<EntityView>).Index);
                foreach (int index in filteredIndexes)
                    mdGridView.ItemsView.CheckedEntitiesId.Add(mdGridView.ItemsView.FilteredEntitiesId[index]);
            }

        }
        
        private void MasterDataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (MasterDetailGridView.IsSelectingItems)
                return;


            MasterDetailGridView.ItemsView.IsMultipleModify = false;

            //if (_testSelection) { } else

            //SelectItems();
            
            if (MasterDataGrid.DataContext != null)
            {
                MasterDetailGridView mdGridView = MasterDataGrid.DataContext as MasterDetailGridView;
                mdGridView.ItemsView.UpdateUI();
            }

        
        }

        /// <summary>
        /// Scopo: non selezionare quando si preme la freccetta di apertura del gruppo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MasterDataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseIndentButtonPressed = false;

            var border = e.OriginalSource as Border;

            if (border != null && border.DataContext != null && border.DataContext is Group && border.Child != null)
            {
                ContentPresenter contentP = border.Child as ContentPresenter;
                if (contentP != null && contentP.Content == null && contentP.TemplatedParent is GridIndentCell)
                {
                    MouseIndentButtonPressed = true;


                    //MasterDetailGridView.ItemsView.PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;
                    //MasterDetailGridView.ItemsView.UpdateCache();


                    return;
                }
            }

            ////N.B. Quando si clicca su un iitem la griglia non toglie la selezione degli elementi interni ai gruppi chiusi
            ////Per Ovviare a questo problema svuoto gli elementi checked se non si sta aggiungendo alla selezione col ctrl
            //if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)
            //    && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            //{
            //    MasterDetailGridView.ItemsView.CheckedEntitiesId.Clear();
            //}
        }

        public bool IsGroupSelected(string groupKey)
        {
            MasterDetailGridView mdGridView = MasterDataGrid.DataContext as MasterDetailGridView;
            HashSet<string> selectedGroups = new HashSet<string>();

            if (MasterDataGrid.View.TopLevelGroup != null)
            {
                foreach (var row in MasterDataGrid.SelectionController.SelectedRows.ToList())
                {
                    if (row.NodeEntry != null)
                    {
                        if (row.NodeEntry.IsGroups)
                        {
                            Group gr = row.NodeEntry as Group;
                            if (gr.Key.ToString() == groupKey)
                                return true;

                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Selected rows -> Checked entities
        /// </summary>
        public HashSet<object> GetSelectedGroupsItems()
        {
            MasterDetailGridView mdGridView = MasterDataGrid.DataContext as MasterDetailGridView;
            HashSet<object> selectedGroupItems = new HashSet<object>();

            if (MasterDataGrid.View.TopLevelGroup != null)
            {
                foreach (var row in MasterDataGrid.SelectionController.SelectedRows.ToList())
                {
                    if (row.NodeEntry != null)
                    {
                        if (row.NodeEntry.IsGroups)
                        {
                            AddGroupSourceToSelectedItems(selectedGroupItems, row.NodeEntry as Group);
                        }
                    }
                }
            }
            return selectedGroupItems;
        }

        /// <summary>
        /// Selected rows -> Checked entities
        /// </summary>
        public void SelectionToCheckedEntities()
        {
            

            MasterDetailGridView mdGridView = MasterDataGrid.DataContext as MasterDetailGridView;
            IEnumerable<int> filteredIndexes = MasterDataGrid.SelectedItems.Select(item => (item as VirtualListItem<EntityView>).Index);

            //mdGridView.ItemsView.CheckedEntitiesId.Clear();
            foreach (int index in filteredIndexes)
                mdGridView.ItemsView.CheckedEntitiesId.Add(mdGridView.ItemsView.FilteredEntitiesId[index]);

            mdGridView.ItemsView.UpdateUI();
            //mdGridView.ItemsView.UpdateCache();
        }

        ///// <summary>
        ///// Selected rows -> Checked entities
        ///// </summary>
        //public async void SelectionToCheckedEntities()
        //{
        //    MasterDetailGridView mdGridView = MasterDataGrid.DataContext as MasterDetailGridView;

        //    if (MouseIndentButtonPressed)
        //    {
        //        MouseIndentButtonPressed = false;

        //        //SelectItemsAsync();
        //        mdGridView.ItemsView.UpdateCache();
        //        //MasterDataGrid.View.Refresh();

        //        return;


        //    }

        //    if (MasterDataGrid.View.TopLevelGroup != null)
        //    {
        //        HashSet<object> selectedItems = new HashSet<object>(MasterDataGrid.SelectedItems);
        //        HashSet<object> selectedGroupItems = new HashSet<object>(/*MasterDataGrid.SelectedItems*/);

        //        foreach (var row in MasterDataGrid.SelectionController.SelectedRows.ToList())
        //        {
        //            if (row.NodeEntry != null)
        //            {
        //                if (row.NodeEntry.IsGroups)
        //                {
        //                    AddGroupSourceToSelectedItems(selectedGroupItems, row.NodeEntry as Group);

        //                    //ClearCurrentItem();
        //                    //selectedItems.UnionWith((selectedItems));
        //                }
        //                else if (row.NodeEntry.IsRecords)
        //                {
        //                    //selectedItems.Add((row.NodeEntry as RecordEntry).Data);
        //                }
        //            }
        //        }
        //        selectedItems.UnionWith((selectedGroupItems));

        //        //N.B: Modificando per risolvere il problema del Ctrl per deselezionare un item in un gruppo non funziona più il copia/incolla
        //        MasterDataGrid.SelectedItems = new ObservableCollection<object>(selectedItems);


        //    }



        //    IEnumerable<int> filteredIndexes = MasterDataGrid.SelectedItems.Select(item => (item as VirtualListItem<EntityView>).Index);

        //    mdGridView.ItemsView.CheckedEntitiesId.Clear();
        //    foreach (int index in filteredIndexes)
        //        mdGridView.ItemsView.CheckedEntitiesId.Add(mdGridView.ItemsView.FilteredEntitiesId[index]);

        //    mdGridView.ItemsView.UpdateUI();


        //}

        //public void AddGroupSourceToSelectedItems(HashSet<object> selectedItems, Group group)
        //{
        //    if (group.Source != null)
        //    {
        //        foreach (var item in group.Source)
        //            selectedItems.Add(item);
        //    }
        //    if (group.Groups != null)
        //    {
        //        foreach (Group g in group.Groups)
        //        {
        //            AddGroupSourceToSelectedItems(selectedItems, g);
        //        }
        //    }
        //    SelectGroupRows(group);

        //}

        public void AddGroupSourceToSelectedItems(HashSet<object> selectedItems, Group group)
        {
            if (group.Source != null)
            {
                foreach (var item in group.Source)
                    selectedItems.Add(item);
            }
            if (group.Groups != null)
            {
                foreach (Group g in group.Groups)
                {
                    AddGroupSourceToSelectedItems(selectedItems, g);
                }
            }

            //SelectGroupRows(group);

        }

        public void SelectRecordRow(int index)
        {
            int recIndex = index;

            if (MasterDataGrid.View.TopLevelGroup != null)
            {

                RecordEntry rec = MasterDataGrid.View.Records.FirstOrDefault(item => ((item as RecordEntry).Data as VirtualListItem<EntityView>).Index == index);

                recIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(rec);
            }

            int startRowIndex = MasterDataGrid.ResolveToRowIndex(recIndex);
            MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);
        }

        //Below method helps to perform selection to be subgroups based on main group.



        ///// <summary>
        ///// https://help.syncfusion.com/wpf/datagrid/selection
        ///// </summary>
        ///// <param name="group"></param>
        //public void SelectGroupRows(Group group)
        //{

        //    if (group == null || !group.IsExpanded)
        //        return;

        //    //Check whether the group contains inner level groups.

        //    if (group.Groups == null)

        //    {

        //        //Get the corresponding start index of record by getting it from DisplayElements .
        //        var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);

        //        //Resolve the recordIndex to RowIndex.
        //        var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //        //Gets the count of rows in the group.
        //        var count = group.ItemsCount + MasterDataGrid.GroupSummaryRows.Count;

        //        //Select the rows from corresponding start and end row index
        //        MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex + count);


        //    }

        //    else
        //    {

        //        foreach (var gr in group.Groups)
        //        {

        //            //Called recursively, to traverse it inner level of group.
        //            SelectGroupRows(gr);
        //            var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
        //            var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

        //            //Get the corresponding end index of the group by getting it from DisplayElements using the inner level group.
        //            var endIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr as Group);
        //            var endRowIndex = MasterDataGrid.ResolveToRowIndex(endIndex);
        //            MasterDataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
        //        }
        //    }
        //}




        public void GetGroupRowsIndex(Group group, ref int minRowIndex, ref int maxRowIndex)
        {

            if (group == null)
                return;

            //Get the corresponding start index of record by getting it from DisplayElements .
            var startIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);

            //Resolve the recordIndex to RowIndex.
            minRowIndex = maxRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);


            if (!group.IsExpanded)
                return;


            //Check whether the group contains inner level groups.

            if (group.Groups == null)

            {

                //Resolve the recordIndex to RowIndex.
                var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

                //Gets the count of rows in the group.
                var count = group.ItemsCount + MasterDataGrid.GroupSummaryRows.Count;

                
                if (startRowIndex < minRowIndex)
                    minRowIndex = startRowIndex;

                if ((startRowIndex + count) > maxRowIndex)
                    maxRowIndex = startRowIndex + count;

                //Select the rows from corresponding start and end row index
                //MasterDataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex + count);


            }

            else
            {

                foreach (var gr in group.Groups)
                {

                    //Called recursively, to traverse it inner level of group.
                    GetGroupRowsIndex(gr, ref minRowIndex, ref maxRowIndex);
                    var startRowIndex = MasterDataGrid.ResolveToRowIndex(startIndex);

                    //Get the corresponding end index of the group by getting it from DisplayElements using the inner level group.
                    var endIndex = MasterDataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr as Group);
                    var endRowIndex = MasterDataGrid.ResolveToRowIndex(endIndex);


                    if (startRowIndex < minRowIndex)
                        minRowIndex = startRowIndex;

                    if (endRowIndex > maxRowIndex)
                        maxRowIndex = endRowIndex;

                    //MasterDataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
                }
            }
        }

        #endregion Selection

        #region Columns

        //AttributiMasterCodes (EntityType) : codice attributi visualizzabili in griglia
        //OrderedMasterAttributiCode (ViewSettings) : codice attributi ordinati visualizzabili nella griglia
        //OrderedColumnsMappingName (MasterDetailGrid) : mapping names delle colonne ordinate e in stato di NON raggruppamento
        //Columns (SfDataGrid) : Colonne della griglia


        /// <summary>
        /// 
        /// </summary>
        public void MasterDataGrid_QueryColumnDragging(object sender, QueryColumnDraggingEventArgs e)
        {
            //per prevenire il riordino delle colonne frozen

            //Console.WriteLine(string.Format("From: {0}", e.From));
            //Console.WriteLine(string.Format("To: {0}", e.To));

            //Per non permettere di spostare una colonna raggruppata
            if (e.From < MasterDataGrid.FrozenColumnCount)
                e.Cancel = true;

            if (e.To >= 0)
            {
                //N.B. Formula empirica per non permettere di spostare una colonna tra quelle raggruppate
                if (e.To < (MasterDataGrid.FrozenColumnCount * 2) - 1)
                    e.Cancel = true;

                if (e.Reason == QueryColumnDraggingReason.Dropped)
                {
                    if (!MasterDataGrid.Columns[e.From].IsHidden &&
                        (e.To == MasterDataGrid.Columns.Count || !MasterDataGrid.Columns[e.To].IsHidden))
                    {
                        //Deve arrivare solo quando sposto la colonna e non quando la reinserisco
                        UpdateOrderedColumnsMappingName();
                    }

                }
            }
        }

        /// <summary>
        /// Columns -> OrderedColumnsMappingName
        /// OrderedColumnsMappingName sono tutti gli attributi di attributiMasterCodes ordinati
        /// </summary>
        void UpdateOrderedColumnsMappingName(bool isColumnHiding = false)
        {
            if (_isColumnsUpdating)
                return;

            try
            {
                List<string> attributiMasterCodes = MasterDetailGridView.ItemsView.EntityType.AttributiMasterCodes;

                //N.B. Quando una colonna viene reinserita risulta ancora nascosta e perciò non veniva reinserita
                List<string> mappingNames = null;
                mappingNames = MasterDataGrid.Columns.Where(item=> item.IsHidden == false).Select(item => item.MappingName).ToList();   

                List<string> attributiGroupedCodes = MasterDetailGridView.ItemsView.RightPanesView.GroupView.Items.Select(item => item.Attributo.Codice).ToList();

                //OrderedColumnsMappingName.RemoveAll(item => !mappingNames.Contains(item));
                OrderedColumnsMappingName.Clear();

                //Aggiorno in OrderedColumnsMappingName l'ordine delle colonne che non sono state raggruppate
                int firstMappingNameIndex = FixedColumnCount + attributiGroupedCodes.Count;
                //for (int i = 0; i < OrderedColumnsMappingName.Count; i++)
                int orderedColumnsMappingNameIndex = 0;
                for (int i = firstMappingNameIndex - 1; i < mappingNames.Count; i++)
                {
                    string mappingName = mappingNames[i];

                    //int index = EntityView.GetAttributiMasterCodesIndexByMappingName(mappingName);

                    //if (index < 0 || index >= attributiMasterCodes.Count || attributiGroupedCodes.Contains(attributiMasterCodes[index]))
                    //    continue;

                    string codice = MasterDetailGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(mappingName);
                    if (codice == null || codice == string.Empty || attributiGroupedCodes.Contains(codice))
                        continue;


                    //if (orderedColumnsMappingNameIndex < OrderedColumnsMappingName.Count)
                    //    OrderedColumnsMappingName[orderedColumnsMappingNameIndex] = mappingName;
                    //else
                    OrderedColumnsMappingName.Add(mappingName);

                    orderedColumnsMappingNameIndex++;
                }

                int removeCount = OrderedColumnsMappingName.Count - orderedColumnsMappingNameIndex;
                OrderedColumnsMappingName.RemoveRange(orderedColumnsMappingNameIndex, removeCount);

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
                //MessageBox.Show(e.Message);
            }

            UpdateMasterAttributiOrder();
        }

        /// <summary>
        /// OrderedColumnsMappingName -> OrderedMasterAttributiCode
        /// </summary>
        void UpdateMasterAttributiOrder()
        {
            string entityTypeKey = MasterDetailGridView.ItemsView.EntityType.GetKey();
            List<string> attributiMasterCodes = MasterDetailGridView.ItemsView.EntityType.AttributiMasterCodes;

            ViewSettings viewSettings = MasterDetailGridView.ItemsView.DataService.GetViewSettings();

            viewSettings.EntityTypes[entityTypeKey].OrderedMasterAttributiCode.Clear();//OrderedMasterAttributiCode.Clear();
            foreach (string mappingName in OrderedColumnsMappingName)
            {
                //int index = EntityView.GetAttributiMasterCodesIndexByMappingName(mappingName);
                //viewSettings.EntityTypes[entityTypeKey].OrderedMasterAttributiCode.Add(attributiMasterCodes[index]);

                string codice = MasterDetailGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(mappingName);
                viewSettings.EntityTypes[entityTypeKey].OrderedMasterAttributiCode.Add(codice);
            }

            MasterDetailGridView.ItemsView.DataService.SetViewSettings(viewSettings);

            if (MasterDetailGridView.ItemsView.ModelActionsStack != null)
                MasterDetailGridView.ItemsView.ModelActionsStack.OnViewSettingsChanged();

        }

        /// <summary>
        /// Ordino le colonne in base ai raggruppamenti e a OrderedColumnsMappingName
        /// (attributiGroupedCodes, OrderedColumnsMappingName) -> Columns
        /// </summary>
        private void OrderGridColumns()
        {
            _isColumnsUpdating = true;

            //Ordino le colonne in base ai raggruppamenti e a OrderedColumnsMappingName

            //codice attributi da visualizzare in griglia
            List<string> attributiMasterCodes = MasterDetailGridView.ItemsView.EntityType.AttributiMasterCodes;
            List<string> attributiGroupedCodes = MasterDetailGridView.ItemsView.RightPanesView.GroupView.Items.Select(item => item.Attributo.Codice).ToList();
            //List<string> mappingNames = MasterDataGrid.Columns.Select(item => item.MappingName).ToList();

            //Lista ordinata delle colonne da ottenere
            //List<string> orderingMappingNames = attributiGroupedCodes.Select(item => EntityView.GetMasterMappingNameByIndex(attributiMasterCodes.IndexOf(item))).ToList();
            List<string> orderingMappingNames = attributiGroupedCodes.Select(item => ColumnAttributiMappingName[item]).ToList();

            foreach (string mappingName in OrderedColumnsMappingName)
            {
                if (!orderingMappingNames.Contains(mappingName))
                    orderingMappingNames.Add(mappingName);
            }


            //ordino le colonne
            for (int i = 0; i < orderingMappingNames.Count; i++)
            {
                GridColumn col = MasterDataGrid.Columns[orderingMappingNames[i]];
                int colIndex = MasterDataGrid.Columns.IndexOf(col);
                if (col != null && colIndex != FixedColumnCount + i)
                {
                    //sposto la colonna
                    MasterDataGrid.Columns.Remove(col);
                    MasterDataGrid.Columns.Insert(FixedColumnCount + i, col);
                }
            }

            //Nascondo le colonne da nascondere
            for (int i = FixedColumnCount; i < MasterDataGrid.Columns.Count; i++)
            {
                if (!orderingMappingNames.Contains(MasterDataGrid.Columns[i].MappingName))
                    MasterDataGrid.Columns[i].IsHidden = true;
            }

            MasterDataGrid.FrozenColumnCount = attributiGroupedCodes.Count + FixedColumnCount;

            _isColumnsUpdating = false;

        }


        /// <summary>
        /// OrderedMasterAttributiCode -> OrderedColumnsMappingName
        /// 
        /// N.B.
        /// EntityType.AttributiMasterCodes[0] è mappato su Data.EntityAttributoView00 e su EntityAttributoView00
        /// EntityType.AttributiMasterCodes[1] è mappato su Data.EntityAttributoView01 e su EntityAttributoView01
        /// EntityType.AttributiMasterCodes[2] è mappato su Data.EntityAttributoView02 e su EntityAttributoView02
        /// ...
        /// per tutta la sessione a prescindere dagli spostamenti di colonnae e si ordine nei dettagli entità
        /// </summary>
        public void UpdateGridColumns()
        {


            try
            {

                if ((MasterDetailGridView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ClearGridColumns) == EntitiesListMasterDetailViewCommands.ClearGridColumns)
                {
                    _isColumnsUpdating = true;
                    ResetColumns();
                    _isColumnsUpdating = false;

                    ////Nascondo le colonne da nascondere
                    //for (int i = FixedColumnCount; i < MasterDataGrid.Columns.Count; i++)
                    //{
                    //    if (!orderingMappingNames.Contains(MasterDataGrid.Columns[i].MappingName))
                    //        MasterDataGrid.Columns[i].IsHidden = true;
                    //}

                    MasterDetailGridView.ItemsView.PendingCommand &= ~EntitiesListMasterDetailViewCommands.ClearGridColumns;
                }

                //key: mappingName, value: è fissa
                Dictionary<string, bool> existingColumnMappingNames = MasterDataGrid.Columns.ToDictionary(item => item.MappingName, item => false);

                //Colonne fisse
                existingColumnMappingNames["Data.Icons"] = true;

                //codice degli attributi raggruppati (per spostarli all'inizio della griglia)
                List<string> attributiGroupedCodes = MasterDetailGridView.ItemsView.RightPanesView.GroupView.Items.Select(item => item.Attributo.Codice).ToList();

                //codice attributi da visualizzare in griglia
                List<string> attributiMasterCodes = MasterDetailGridView.ItemsView.EntityType.AttributiMasterCodes;

                Dictionary<string, string> attributiMasterCodesEtichette = attributiMasterCodes.ToDictionary(item => item, item => MasterDetailGridView.ItemsView.EntityType.Attributi[item].Etichetta);
                //Dictionary<string, string> columnMappingNamesEtichette = new Dictionary<string, string>();

                ViewSettings viewSettings = MasterDetailGridView.ItemsView.DataService.GetViewSettings();

                ////costruisco dictionary
                ////key: codice attributo, value: mappingName
                //Dictionary<string, string> columnAttributiCodice = new Dictionary<string, string>();
                //foreach (string mappingName in columnMappingNames.Keys)
                //{
                //    int index = EntityView.GetAttributiMasterCodesIndexByMappingName(mappingName);
                //    if (0 <= index && index < attributiMasterCodes.Count)
                //    {
                //        string attCodice = attributiMasterCodes[index];
                //        columnAttributiCodice.Add(attCodice, mappingName);
                //        columnMappingNamesEtichette.Add(attCodice, MasterDetailGridView.ItemsView.EntityType.Attributi[attCodice].Etichetta);
                //    }
                //}

                ColumnAttributiMappingName.Clear();
                foreach (string mappingName in existingColumnMappingNames.Keys)
                {
                    string attCodice = MasterDetailGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(mappingName);
                    if (attCodice != null && attCodice.Any())
                    {
                        ColumnAttributiMappingName.Add(attCodice, mappingName);
                    }
                }


                SerializationOptions options = new SerializationOptions();


                GridColumn col = null;
                for (int i = 0; i < attributiMasterCodes.Count; i++)
                {
                    string codiceAtt = attributiMasterCodes[i];
                    Attributo att = MasterDetailGridView.ItemsView.EntityType.Attributi[codiceAtt];
                    string etichettaAtt = att.Etichetta;

                    //string mapName = EntityView.GetMasterMappingNameByIndex(i);
                    //string valueMapName = EntityView.GetMasterValueMappingNameByIndex(i);

                    //if (!columnAttributiCodice.ContainsKey(att.Codice))
                    if (!ColumnAttributiMappingName.ContainsKey(att.Codice))
                    {

                        string mapName = MasterDetailGridView.ItemsView.MasterMappingNames.CreateNewMappingName();
                        if (mapName == null) //raggiunto il numero massimo di colonne consentito
                            continue;

                        string valueMapName = MasterDetailGridView.ItemsView.MasterMappingNames.CreateNewValueMappingName();


                        if (!existingColumnMappingNames.ContainsKey(mapName))
                        {

                            //Aggiungo la colonna (non visibile)
                            if (att is AttributoRiferimento)
                            {
                                AttributoRiferimento attRif = att as AttributoRiferimento;
                                col = new GridTextColumn();
                                col.AllowEditing = false;
                                col.MappingName = mapName;
                            }
                            else
                            {
                                if (att.ValoreDefault is ValoreContabilita)
                                {
                                    //GridCurrencyColumn currencyCol = new GridCurrencyColumn();
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;

                                    Binding displayBinding = new Binding();
                                    displayBinding.Path = new PropertyPath(mapName);
                                    col.DisplayBinding = displayBinding;

                                    Binding valueBinding = new Binding();
                                    valueBinding.Path = new PropertyPath(valueMapName);
                                    col.ValueBinding = valueBinding;

                                }
                                else if (att.ValoreDefault is ValoreReale)
                                {
                                    //GridNumericColumn realCol = new GridNumericColumn();
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;

                                    Binding displayBinding = new Binding();
                                    displayBinding.Path = new PropertyPath(mapName);
                                    col.DisplayBinding = displayBinding;

                                    Binding valueBinding = new Binding();
                                    valueBinding.Path = new PropertyPath(valueMapName);
                                    col.ValueBinding = valueBinding;
                                }
                                else if (att.ValoreDefault is ValoreTestoRtf)
                                {
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                                else if (att.ValoreDefault is ValoreTesto)
                                {
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                                else if (att.ValoreDefault is ValoreData)
                                {
                                    GridDateTimeColumn colData = new GridDateTimeColumn();
                                    colData.Pattern = Syncfusion.Windows.Shared.DateTimePattern.ShortDate;
                                    col = colData;
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                                else if (att.ValoreDefault is ValoreElenco)
                                {
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                                else if (att.ValoreDefault is ValoreColore)
                                {
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                                else if (att.ValoreDefault is ValoreBooleano)
                                {
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                                else if (att.ValoreDefault is ValoreFormatoNumero)
                                {
                                    col = new GridTextColumn();
                                    col.MappingName = mapName;
                                    col.AllowEditing = false;
                                }
                            }


                            if (col != null)
                            {
                                //col.TextAlignment = TextAlignment.Right;
                                if (MasterDetailGridView.ItemsView.IsAttributoTextAlignmentRight(codiceAtt))
                                    col.TextAlignment = TextAlignment.Right;
                                else
                                    col.TextAlignment = TextAlignment.Left;


                                col.AllowSorting = false;
                                col.AllowFiltering = false;
                                col.AllowGrouping = true;
                                col.HeaderText = att.Etichetta;
                                if (col.AllowEditing)
                                    col.AllowEditing = !att.IsValoreReadOnly;

                                //if (col.AllowEditing)
                                //    col.CellStyle = MasterDetailGridCtrl.FindResource("DataGridCellStyleAllowEditing") as Style;
                                //else
                                //    col.CellStyle = MasterDetailGridCtrl.FindResource("DataGridCellStyleNotAllowEditing") as Style;

                                if (attributiGroupedCodes.Contains(codiceAtt))
                                    col.CellStyle = MasterDetailGridCtrl.FindResource("cellGroupedStyle") as Style;
                                else
                                    col.CellStyle = MasterDetailGridCtrl.FindResource("cellNotGroupedStyle") as Style;

                                //imposto le colonne come da ViewSettings salvato nel file join
                                if (viewSettings != null &&
                                    viewSettings.EntityTypes.ContainsKey(att.EntityTypeKey) &&
                                    viewSettings.EntityTypes[att.EntityTypeKey].GridViewSettings.ColumnsViewSettings.ContainsKey(att.Codice))
                                    col.Width = viewSettings.EntityTypes[att.EntityTypeKey].GridViewSettings.ColumnsViewSettings[att.Codice].Width;

                                MasterDataGrid.Columns.Add(col);
                                //columnMappingNames.Add(mapName, true);


                            }
                        }

                        ColumnAttributiMappingName.Add(codiceAtt, mapName);//AU
                        MasterDetailGridView.ItemsView.MasterMappingNames.SetMappingName(mapName, codiceAtt);
                        MasterDetailGridView.ItemsView.MasterMappingNames.SetValueMappingName(valueMapName, codiceAtt);
                        existingColumnMappingNames[mapName] = true;
                    }
                    else
                    {
                        string mapName = ColumnAttributiMappingName[codiceAtt];

                        col = MasterDataGrid.Columns[mapName];
                        if (attributiGroupedCodes.Contains(codiceAtt))
                            col.CellStyle = MasterDetailGridCtrl.FindResource("cellGroupedStyle") as Style;
                        else
                            col.CellStyle = MasterDetailGridCtrl.FindResource("cellNotGroupedStyle") as Style;

                        col.HeaderText = att.Etichetta;
                        existingColumnMappingNames[mapName] = true;
                    }

                }

                //Elimino le colonne per cui non esiste alcun attributo associato
                foreach (string key in existingColumnMappingNames.Keys)
                {
                    if (existingColumnMappingNames[key] == false)
                    {
                        MasterDataGrid.Columns.Remove(MasterDataGrid.Columns[key]);
                    }
                }

                //Ordinamento delle colonne
                if (ColumnAttributiMappingName.Any())
                {
                    string entityTypeKey = MasterDetailGridView.ItemsView.EntityType.GetKey();
                    //ViewSettings viewSettings = MasterDetailGridView.ItemsView.DataService.GetViewSettings();
                    if (viewSettings.EntityTypes[entityTypeKey].OrderedMasterAttributiCode.Any())
                    {
                        //OrderedColumnsMappingName = viewSettings.EntityTypes[entityTypeKey].OrderedMasterAttributiCode.Select(item => EntityView.GetMasterMappingNameByIndex(attributiMasterCodes.IndexOf(item))).ToList();
                        OrderedColumnsMappingName = viewSettings.EntityTypes[entityTypeKey].OrderedMasterAttributiCode.Select(item => ColumnAttributiMappingName[item]).ToList();
                        OrderGridColumns();
                    }
                    else
                    {

                        List<Attributo> atts = attributiMasterCodes.Select(item => MasterDetailGridView.ItemsView.EntityType.Attributi[item]).ToList();
                        //OrderedColumnsMappingName = atts.Select(item => EntityView.GetMasterMappingNameByIndex(attributiMasterCodes.IndexOf(item.Codice))).ToList();
                        OrderedColumnsMappingName = atts.Select(item => ColumnAttributiMappingName[item.Codice]).ToList();
                    }
                }

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }



        }

        public void ResetColumns()
        {
            int index = 0;
            int count = MasterDataGrid.Columns.Count;
            for (index = count - 1; index > FixedColumnCount; index--)
            {
                MasterDataGrid.Columns.RemoveAt(index);
            }
        }

        public void ColumnChooser_Click(object sender, RoutedEventArgs e)
        {
            columnChooserWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            columnChooserWindow.Show();
        }

        private void GridColumnChooserController_IsColumnHiddenChanged(object sender, EventArgs e)
        {
            UpdateOrderedColumnsMappingName(true);
        }

#endregion Columns

#region Groups
        //public void SetCurrentGroupKey(int rowIndex)
        //{
        //    // Get the resolved current record index
        //    var recordIndex = MasterDataGrid.ResolveToRecordIndex(rowIndex);
        //    if (recordIndex < 0)
        //        return;

        //    string currentGroupKey = "";
        //    char separator = GroupData.GroupKeySeparator;

        //    //if (args.ActivationTrigger == ActivationTrigger.Mouse)
        //    //{
        //    if (MasterDataGrid.View.TopLevelGroup != null)
        //    {
        //        // Get the current row record while grouping
        //        var record = MasterDataGrid.View.TopLevelGroup.DisplayElements[recordIndex];

        //        Group group = null;
        //        if (record.IsRecords)
        //            group = record.Parent as Group;
        //        else
        //            group = record as Group;

        //        while (group != null && group.Level > 0)
        //        {
        //            if (currentGroupKey.Any())
        //                currentGroupKey = string.Format("{0}{1}{2}", group.Key as string, separator, currentGroupKey);
        //            else
        //                currentGroupKey = group.Key as string;

        //            group = group.Parent;
        //        }
        //        //}
        //    }

        //    MasterDetailGridView.ItemsView.CurrentGroupKey = currentGroupKey;
        //}


        private void ExpandGroupsByEntityId(Guid id)
        {
            if (MasterDataGrid.View == null)
                return;

            if (MasterDataGrid.View.TopLevelGroup == null)
                return;

            List<string> groupKeys = MasterDetailGridView.ItemsView.GetGroupsKeyById(id);

            List<Group> groups = MasterDataGrid.View.Groups.Select(item => item as Group).ToList();

            ExpandGroupByKey(groups, groupKeys, 0);

        }

        private void ExpandGroupByKey(List<Group> groups, List<string> groupKeys, int groupKeyIndex)
        {
            if (groupKeyIndex >= groupKeys.Count)
                return;

            //espansione dei gruppi di appartenenza
            foreach (Group group in groups)
            {
                if ((group.Key == null && groupKeys[groupKeyIndex] == null) || ((group.Key as string) == (groupKeys[groupKeyIndex] as string)))
                {
                    group.IsExpanded = true;
                    ExpandGroupByKey(group.Groups, groupKeys, groupKeyIndex + 1);
                }
            }
        }

        private void GetGroupRecordsId(Group groupRecord, ref List<Guid> ids)
        {
            if (groupRecord.Source != null)
            {

                IEnumerable<int> indexes = groupRecord.Source.Select(item => (item as VirtualListItem<EntityView>).Index);

                //IEnumerable<Guid> childsId = groupRecord.Source.Select(item => mdGridView.ItemsView.FilteredEntitiesId[(item as VirtualListItem<EntityView>).Index]);
                //ids.AddRange(childsId);

                foreach (int index in indexes)
                {
                    if (0 <= index && index < MasterDetailGridView.ItemsView.FilteredEntitiesId.Count)
                        ids.Add(MasterDetailGridView.ItemsView.FilteredEntitiesId[index]);
                }
            }

            if (groupRecord.Groups != null)
            {
                foreach (Group grRec in groupRecord.Groups)
                {
                    GetGroupRecordsId(grRec, ref ids);
                }
            }
        }

        private HashSet<Group> GetGroupsByKeys(HashSet<string> keys)
        {
            HashSet<Group> groups = new HashSet<Group>();
            try
            {
                
                if (MasterDataGrid.View != null && MasterDataGrid.View.Groups != null)
                {
                    foreach (Group gr in MasterDataGrid.View.Groups)
                    {
                        GetGroupsByKeys(gr, keys, groups);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                //MessageBox.Show(MasterDataGrid.ToString());
                //MessageBox.Show(MasterDataGrid.View.ToString());
                //MessageBox.Show(MasterDataGrid.View.Groups.ToString());
                //MessageBox.Show(keys.ToString());
                //MessageBox.Show(groups.ToString());

                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }
            return groups;
        }

        private void GetGroupsByKeys(Group group, HashSet<string> keys, HashSet<Group> groups)
        {
            List<string> groupKeys = new List<string>();
            Group parent = group;
            while (parent != null && parent.Key != null)
            {
                groupKeys.Insert(0, parent.Key as string);
                parent = parent.Parent;
            }
            string groupKey = MasterDetailGridView.ItemsView.RightPanesView.GroupView.JoinGroupKeys(groupKeys.ToArray());


            if (keys != null)
            {
                if (keys.Contains(groupKey))
                    groups.Add(group);
            }
            else
                groups.Add(group);

            if (group.Groups != null)
            {
                foreach (Group gr in group.Groups)
                    GetGroupsByKeys(gr,keys, groups);
            }
        }



#endregion Groups

#region Key Down
        public void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        //public void ShortRichTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //        (sender as Xceed.Wpf.Toolkit.RichTextBox).GetBindingExpression(Xceed.Wpf.Toolkit.RichTextBox.TextProperty).UpdateSource();
        //}

        /// <summary>
        /// Scopo: separatore decimale nel tasterino numerico secondo le impostazioni di windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Decimal)
            {
                var txb = sender as TextBox;

                string decimalSeparator = "";
                if (txb.DataContext is ValoreRealeView)
                {
                    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                }
                else if (txb.DataContext is ValoreContabilitaView)
                {
                    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                }
                //else if (txb.DataContext is ValorePercentualeView)
                //{
                //    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.PercentDecimalSeparator;
                //}
                else
                    return;

                int caretPos = txb.CaretIndex;
                txb.Text = txb.Text.Insert(txb.CaretIndex, decimalSeparator);
                txb.CaretIndex = caretPos + 1;
                e.Handled = true;
            }
        }
#endregion

#region Mouse
        /// <summary>
        /// Gestisce il DoubleClick sull'attributo filtrato per visualizzare il dialogo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AttributoFilterView attFilterView = ((ListViewItem)sender).Content as AttributoFilterView;
            if (attFilterView.Attributo != null)
            {
                int index = MasterDetailGridView.ItemsView.RightPanesView.FilterView.Items.IndexOf(attFilterView);

                
                MasterDetailGridView.ItemsView.RightPanesView.FilterView.Load(attFilterView.Attributo, index);
                MasterDetailGridView.ItemsView.RightPanesView.FilterView.SearchNext();
            }
        }

        public void ClearCurrentItem()
        {
            MasterDetailGridView.ItemsView.CurrentGroupKey = "";
            MasterDetailGridView.ItemsView.ClearFocus();
            MasterDetailGridView.CurrentItem = null;
            MasterDetailGridView.ItemsView.UpdateUI();

            //MasterDataGrid.ClearSelections(false);
        }


        public void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        public void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }

        public void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        public void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        public void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }

        public void ListaGroup_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        public void ListaGroup_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailGridView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }

#endregion

#region Drag&Drop
        private Point _startPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope = false;
        

        public void DetailListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        public void DetailLIstView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "DragRectangle")
                    {
                        IEnumerable<Popup> popups = GetOpenPopups();
                        if (!popups.Any())
                            BeginDrag(e);
                    }
                }
            }
        }

        private void BeginDrag(MouseEventArgs e)
        {
            ListView listView = this.DetailListView;
            ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            DetailAttributoView entAttView = listViewItem.Content as DetailAttributoView;
            string codAttributo = entAttView.CodiceAttributo;
            string pacchetto = String.Join(",", new string[] { "AttributoView", codAttributo});

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            //listView.DragEnter += ListViewDragEnter;


            
            DataObject data = new DataObject("AttributoViewFormat", pacchetto);
            DragDropEffects de = DragDrop.DoDragDrop(this.DetailListView, data, DragDropEffects.Move);

            //cleanup
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            //listView.DragEnter -= ListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }


        }

        /// <summary>
        /// Non faccio il drag&drop se sono attivi dei popup altrimenti poi perde il fuoco sui controlli
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Popup> GetOpenPopups()
        {
            return PresentationSource.CurrentSources.OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<FrameworkElement>()
                .Select(f => f.Parent)
                .OfType<Popup>();
                //.Where(p => p.IsOpen);
        }

        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(DetailListView as Visual);
            _layer.Add(_adorner);
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == DetailListView)
            {
                Point p = e.GetPosition(DetailListView);
                Rect r = VisualTreeHelper.GetContentBounds(DetailListView);
                if (!r.Contains(p))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }




        }

        void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(DetailListView).X;
                _adorner.OffsetTop = args.GetPosition(MasterDetailGridCtrl).Y - _startPoint.Y;
            }
        }

        //private void ListViewDragEnter(object sender, DragEventArgs e)
        //{
        //    if (!e.Data.GetDataPresent("AttributoViewFormat") ||
        //        sender == e.Source)
        //    {
        //        e.Effects = DragDropEffects.None;
        //    }
        //}

        public void DetailListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("AttributoViewFormat") ||
                    sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }


        public void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        {
            ContentControl contentCtrl = sender as ContentControl;
            if (contentCtrl.Content is FilterView)
                FilterToggleButton_Drop(sender, e);
            else if (contentCtrl.Content is SortView)
                SortToggleButton_Drop(sender, e);
            else if (contentCtrl.Content is GroupView)
                GroupToggleButton_Drop(sender, e);


        }

        public void FilterToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;

                //string pacchetto = await e.DataView.GetTextAsync();
                var items = pacchetto.Split(',');

                Attributo tipoAttributo = MasterDetailGridView.ItemsView.GetAttributoByCode(items[1]);
                //Attributo tipoAttributo = FilterView.This.Master.GetAttributoByCode(items[1]);

                if (tipoAttributo != null)
                {
                    bool res = MasterDetailGridView.ItemsView.RightPanesView.FilterView.Load(tipoAttributo, -1);
                    
                    if (res)
                        MasterDetailGridView.ItemsView.RightPanesView.FilterView.SearchNext();
                }

            }
        }

        public void SortToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;

                //string pacchetto = await e.DataView.GetTextAsync();
                var items = pacchetto.Split(',');

                Attributo tipoAttributo = MasterDetailGridView.ItemsView.GetAttributoByCode(items[1]);

                AttributoSortView targetAttSortView = null;

                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem != null)
                {
                    targetAttSortView = listViewItem.Content as AttributoSortView;
                }

                if (tipoAttributo != null)
                    MasterDetailGridView.ItemsView.RightPanesView.SortView.Load(tipoAttributo, targetAttSortView);

            }
        }

        public void GroupToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {  
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;
                var items = pacchetto.Split(',');
                
                Attributo tipoAttributo = MasterDetailGridView.ItemsView.GetAttributoByCode(items[1]);

                AttributoGroupView targetAttGroupView = null;

                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem != null)
                {
                   targetAttGroupView = listViewItem.Content as AttributoGroupView;
                }

                if (tipoAttributo != null)
                    MasterDetailGridView.ItemsView.RightPanesView.GroupView.Load(tipoAttributo, targetAttGroupView);

            }
        }

        public T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        #endregion Drag&Drop

        #region Drag&Drop per reorder RightSplitPaneControl

        public void RightSplitPaneControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        public void RightSplitPaneControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (   (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "GroupName")
                        || (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "GroupStackPanel")
                        || (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "SortName")
                        || (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "SortStackPanel"))
                    {
                        IEnumerable<Popup> popups = GetOpenPopups();
                        if (!popups.Any())
                            BeginDragToMove(e);
                    }
                }
            }
        }

        private void BeginDragToMove(MouseEventArgs e)
        {
            ListView listView = this.DetailListView;
            ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            string pacchetto = string.Empty;
            if (listViewItem.Content is AttributoGroupView)
            {
                AttributoGroupView attGroupView = listViewItem.Content as AttributoGroupView;
                string codAttributo = attGroupView.Attributo.Codice;
                pacchetto = String.Join(",", new string[] { "AttributoView", codAttributo});
            }
            else if (listViewItem.Content is AttributoSortView)
            {
                AttributoSortView attGroupView = listViewItem.Content as AttributoSortView;
                string codAttributo = attGroupView.Attributo.Codice;
                pacchetto = String.Join(",", new string[] { "AttributoView", codAttributo });
            }


            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;

            DataObject data = new DataObject("AttributoViewFormat", pacchetto);
            DragDropEffects de = DragDrop.DoDragDrop(this.DetailListView, data, DragDropEffects.Move);

            //cleanup
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }


        }





        #endregion

        #region Layout scale
        public void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if ((e.Delta > 0 && MasterDetailScale.ScaleX < 2.0)
                    || (e.Delta < 0 && MasterDetailScale.ScaleX > 0.5))
                {
                    MasterDetailScale.ScaleX += (e.Delta > 0) ? 0.1 : -0.1;
                    MasterDetailScale.ScaleY += (e.Delta > 0) ? 0.1 : -0.1;
                }
            }
        }

        public void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    MasterDetailScale.ScaleX = 1;
                    MasterDetailScale.ScaleY = 1;

                }

            }
        }

#endregion  Layout scale




    }

    /// <summary>
    /// Classe per scrivere nella riga TableSummary
    /// </summary>
    public class GridTableSummaryCellRendererExt : GridTableSummaryCellRenderer
    {

        public override async void OnUpdateEditBinding(DataColumnBase dataColumn, GridTableSummaryCell element, object dataContext)
        {
            SummaryRecordEntry summaryRecordEntry = element.DataContext as SummaryRecordEntry;

            if (this.DataGrid.CaptionSummaryRow == null)
            {
            }
            else if (this.DataGrid.CaptionSummaryRow.ShowSummaryInRow)
            {
            }
            else
            {
//#if ALE_ASYNC_GROUP //Ale Group Task

                MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
                string columnCodiceAtt = GetColumnCodiceAtt("TableSummary", summaryRecordEntry, dataColumn.GridColumn.MappingName);
                string gridColumnMappingName = dataColumn.GridColumn.MappingName;

                if (columnCodiceAtt.Any())
                {
                    element.Content = "...";
                    element.Foreground = Brushes.LightGray;
                    //element.Content = await GetCustomizedCaptionTextAsync("TableSummary", columnCodiceAtt);
                    GroupData groupData = await mdGridView.ItemsView.GetGroupDataResult();
                    if (groupData != null && groupData.GroupRecords.ContainsKey("TableSummary"))
                    {
                        if (groupData.GroupRecords["TableSummary"].Attributi.ContainsKey(columnCodiceAtt))
                        {
                            string content = groupData.GroupRecords["TableSummary"].Attributi[columnCodiceAtt];
                            element.Content = content;
                            element.HorizontalContentAlignment = GetCustomizedCaptionTextAlignment(gridColumnMappingName);

                            if (content != null && content.Contains(BuiltInCodes.Others.NaN))
                                element.Foreground = ColorsHelper.Convert(MyColorsEnum.ErrorColor);
                            else
                                element.Foreground = Brushes.Black;
                        }
                    }
                }
                else
                    element.Content = string.Empty;
//#else
//                element.Content = GetCustomizedCaptionText("TableSummary", summaryRecordEntry, dataColumn.GridColumn.MappingName);
//                element.HorizontalContentAlignment = GetCustomizedCaptionTextAlignment(dataColumn.GridColumn.MappingName);
//#endif

            }

        }
        private string GetColumnCodiceAtt(string groupKey, SummaryRecordEntry summaryRecordEntry, string columnMappingName)
        {
            MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
            GroupData groupData = mdGridView.ItemsView.RightPanesView.GroupView.Data;

            string columnCodiceAtt = string.Empty;

            if (!mdGridView.ItemsView.FilteredEntitiesId.Any())
                return columnCodiceAtt;

            columnCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(columnMappingName);

            return columnCodiceAtt;
        }


        private async Task<object> GetCustomizedCaptionTextAsync(string groupKey, string columnCodiceAtt)
        {
            MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
            GroupData groupData = mdGridView.ItemsView.RightPanesView.GroupView.Data;

            return await Task.Run(() =>
            {
                if (groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt] == null)
                {
                    mdGridView.ItemsView.DataService.FillGroupData(groupData);
                }

                object res1 = string.Empty;
                if (groupData.GroupRecords.ContainsKey(groupKey) && groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
                    res1 = groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt];

                return res1;
            });

        }

        private object GetCustomizedCaptionText(string groupKey, SummaryRecordEntry summaryRecordEntry, string columnMappingName)
        {

            MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
            GroupData groupData = mdGridView.ItemsView.RightPanesView.GroupView.Data;

            if (!mdGridView.ItemsView.FilteredEntitiesId.Any())
                return "";

            string columnCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(columnMappingName);
            if (columnCodiceAtt == null || !columnCodiceAtt.Any())
                return string.Empty;


            if (!groupData.GroupRecords.ContainsKey(groupKey))
            {
                List<Guid> childsId = mdGridView.ItemsView.FilteredEntitiesId;
                groupData.GroupRecords.Add(groupKey, new GroupRecordData() { ChildsId = childsId });
            }

            if (groupData.GroupRecords[groupKey] == null || !groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
            {
                mdGridView.ItemsView.DataService.FillGroupData(groupData);
            }

            object res = "";
            if (groupData.GroupRecords.ContainsKey(groupKey) && groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
                res = groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt];

            return res;
        }

        private HorizontalAlignment GetCustomizedCaptionTextAlignment(string dataColumnName)
        {
            MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;

            //int index = EntityView.GetAttributiMasterCodesIndexByMappingName(dataColumnName);
            //if (index >= 0)
            //{
            //    string columnCodiceAtt = mdGridView.ItemsView.EntityType.AttributiMasterCodes[index];
            //    if (mdGridView.ItemsView.EntityType.Attributi.ContainsKey(columnCodiceAtt))
            //    {
            //        if (mdGridView.ItemsView.IsAttributoTextAlignmentRight(columnCodiceAtt))
            //            return HorizontalAlignment.Right;
            //    }
            //}
            string columnCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(dataColumnName);
            if (columnCodiceAtt != null && columnCodiceAtt.Any())
            {
                if (mdGridView.ItemsView.EntityType.Attributi.ContainsKey(columnCodiceAtt))
                {
                    if (mdGridView.ItemsView.IsAttributoTextAlignmentRight(columnCodiceAtt))
                        return HorizontalAlignment.Right;
                }
            }

            return HorizontalAlignment.Left;
        }
    }

    /// <summary>
    /// Classe per scrivere nella riga CaptionSummary quando raggruppato 
    /// </summary>
    public class GridCaptionSummaryCellRendererExt : GridCaptionSummaryCellRenderer
    {
        /// <summary>
        /// Method to update the CaptionSummaryCell.
        /// </summary>
        public override async void OnUpdateEditBinding(Syncfusion.UI.Xaml.Grid.DataColumnBase dataColumn, Syncfusion.UI.Xaml.Grid.GridCaptionSummaryCell element, object dataContext)
        {
            if (element.DataContext is Group && this.DataGrid.View.GroupDescriptions.Count > 0)
            {

                Group groupRecord = element.DataContext as Group;

                var groupedColumn = this.GetGroupedColumn(groupRecord);

                if (this.DataGrid.CaptionSummaryRow == null)
                {
                    if (this.DataGrid.View.GroupDescriptions.Count < groupRecord.Level)
                        return;
                    //Set the captionsummarycell text as customized.

                    element.Content = GetCustomizedCaptionText(DataGrid.DataContext as MasterDetailGridView, groupedColumn.MappingName, groupRecord);
                    element.HorizontalContentAlignment = GetCustomizedCaptionTextAlignment(dataColumn.GridColumn.MappingName);

                }
                else if (this.DataGrid.CaptionSummaryRow.ShowSummaryInRow)
                {
                    element.Content = SummaryCreator.GetSummaryDisplayTextForRow(groupRecord.SummaryDetails, this.DataGrid.View, groupedColumn.HeaderText);
                }
                else
                {

//#if ALE_ASYNC_GROUP //Ale Group Task
                    MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
                    string groupKey = string.Empty;
                    string columnCodiceAtt = string.Empty;
                    string gridColumnMappingName = dataColumn.GridColumn.MappingName;

                    GetGroupDataPosition(DataGrid.DataContext as MasterDetailGridView, groupedColumn.MappingName, groupRecord,
                        out groupKey, out columnCodiceAtt, gridColumnMappingName);


                    //if (groupKey.Any())
                    if (groupKey != null)
                    {
                        if (gridColumnMappingName == "Data.Icons")
                        {
                            element.Content = "...";
                            element.Foreground = Brushes.LightGray;
                            GroupData groupData = await mdGridView.ItemsView.GetGroupDataResult();
                            if (groupData != null && groupData.GroupRecords.ContainsKey(groupKey))
                            {
                                element.Background = ColorsHelper.Convert(groupData.GroupRecords[groupKey].HighlighterColorName);
                                element.Content = string.Empty;
                            }
                        }
                        else if (columnCodiceAtt.Any())
                        {
                            element.Content = "...";
                            element.Foreground = Brushes.LightGray;
                            GroupData groupData = await mdGridView.ItemsView.GetGroupDataResult();

                            if (groupData != null && groupData.GroupRecords.ContainsKey(groupKey))
                            {
                                if (groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
                                {
                                    string content = groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt];
                                    element.Content = content;
                                    element.HorizontalContentAlignment = GetCustomizedCaptionTextAlignment(gridColumnMappingName);

                                    if (content != null && content.Contains(BuiltInCodes.Others.NaN))
                                        element.Foreground = ColorsHelper.Convert(MyColorsEnum.ErrorColor);
                                    else
                                        element.Foreground = Brushes.Black;

                                }
                                
                            }
                        }
                    }
                    else
                    {
                        element.Content = string.Empty;
                    }
                }
            }
        }



        /// <summary>
        /// Method to get the Column that is grouped.
        /// </summary>
        private GridColumn GetGroupedColumn(Group group)
        {
            var groupDesc = this.DataGrid.View.GroupDescriptions[group.Level - 1] as PropertyGroupDescription;
            foreach (var column in this.DataGrid.Columns)
            {
                if (column.MappingName == groupDesc.PropertyName)
                {
                    return column;
                }
            }
            return null;
        }

        private void GetGroupDataPosition(MasterDetailGridView mdGridView, string groupColumnName, Group groupRecord,
            out string groupKey, out string columnCodiceAtt, string dataColumnName = "")
        {
            //MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
            GroupData groupData = mdGridView.ItemsView.RightPanesView.GroupView.Data;

            //string groupKey = groupRecord.Key as string;
            groupKey = string.Empty;
            columnCodiceAtt = string.Empty;

            if (!mdGridView.ItemsView.FilteredEntitiesId.Any())
                return;

            //costruisco il groupKey del gruppo
            groupKey = groupRecord.Key != null ? groupRecord.Key as string : "";
            Group gr = groupRecord.Parent;
            while (gr != null && !gr.IsTopLevelGroup)
            {
                groupKey = string.Format("{0}{1}{2}", gr.Key != null ? gr.Key as string : "", GroupData.GroupKeySeparator, groupKey);
                gr = gr.Parent;
            }

            string groupCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(groupColumnName);
            if (groupCodiceAtt == null || !groupCodiceAtt.Any())
                return;

            columnCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(dataColumnName);
            if (columnCodiceAtt == null || !columnCodiceAtt.Any())
                return;



        }

        private async Task<object> GetCustomizedCaptionTextAsync(string groupKey, string columnCodiceAtt)
        {
            MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
            GroupData groupData = mdGridView.ItemsView.RightPanesView.GroupView.Data;

            return await Task.Run(() =>
            {
                if (groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt] == null)
                {
                    mdGridView.ItemsView.DataService.FillGroupData(groupData);
                }

                object res1 = string.Empty;
                if (groupData.GroupRecords.ContainsKey(groupKey) && groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
                    res1 = groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt];

                return res1;
            });

        }



        /// <summary>
        /// Method to Customize the CaptionSummaryCell Text.
        /// </summary>
        private object GetCustomizedCaptionText(MasterDetailGridView mdGridView, string groupColumnName, Group groupRecord, string dataColumnName = "")
        {
            //MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;
            GroupData groupData = mdGridView.ItemsView.RightPanesView.GroupView.Data;

            //string groupKey = groupRecord.Key as string;

            if (!mdGridView.ItemsView.FilteredEntitiesId.Any())
                return "";

            //int index = EntityView.GetAttributiMasterCodesIndexByMappingName(groupColumnName);
            //if (index < 0)
            //    return "";
            //string groupCodiceAtt = mdGridView.ItemsView.EntityType.AttributiMasterCodes[index];
            string groupCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(groupColumnName);
            if (groupCodiceAtt == null || !groupCodiceAtt.Any())
                return string.Empty;


            //index = EntityView.GetAttributiMasterCodesIndexByMappingName(dataColumnName);
            //if (index < 0)
            //    return "";
            //string columnCodiceAtt = mdGridView.ItemsView.EntityType.AttributiMasterCodes[index];
            string columnCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(dataColumnName);
            if (columnCodiceAtt == null || !columnCodiceAtt.Any())
                return string.Empty;

            //costruisco il groupKey del gruppo
            string groupKey = groupRecord.Key != null ? groupRecord.Key as string : "";
            Group gr = groupRecord.Parent;
            while (gr != null && !gr.IsTopLevelGroup)
            {
                groupKey = string.Format("{0}{1}{2}", gr.Key != null ? gr.Key as string : "", GroupData.GroupKeySeparator, groupKey);
                gr = gr.Parent;
            }


            if (groupKey == null)
                return string.Empty;

            //if (!groupKey.Any())
            //    return "";



            if (!groupData.GroupRecords.ContainsKey(groupKey))
            {
                List<Guid> childsId = new List<Guid>();
                GetGroupRecordsId(mdGridView, groupRecord, ref childsId);
                groupData.GroupRecords.Add(groupKey, new GroupRecordData() { ChildsId = childsId });
            }

            if (groupData.GroupRecords[groupKey] == null || !groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
            {
                mdGridView.ItemsView.DataService.FillGroupData(groupData);
            }


            object res = "";
            if (groupData.GroupRecords.ContainsKey(groupKey) && groupData.GroupRecords[groupKey].Attributi.ContainsKey(columnCodiceAtt))
                res = groupData.GroupRecords[groupKey].Attributi[columnCodiceAtt];

            return res;
        }

        private HorizontalAlignment GetCustomizedCaptionTextAlignment(string dataColumnName)
        {
            MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;

            //int index = EntityView.GetAttributiMasterCodesIndexByMappingName(dataColumnName);
            //if (index >= 0)
            //{
            //    string columnCodiceAtt = mdGridView.ItemsView.EntityType.AttributiMasterCodes[index];
            //    if (mdGridView.ItemsView.EntityType.Attributi.ContainsKey(columnCodiceAtt))
            //    {
            //        if (mdGridView.ItemsView.IsAttributoTextAlignmentRight(columnCodiceAtt))
            //            return HorizontalAlignment.Right;
            //    }
            //}

            string columnCodiceAtt = mdGridView.ItemsView.MasterMappingNames.GetCodiceByMappingName(dataColumnName);
            if (columnCodiceAtt != null && columnCodiceAtt.Any())
            {
                if (mdGridView.ItemsView.EntityType.Attributi.ContainsKey(columnCodiceAtt))
                {
                    if (mdGridView.ItemsView.IsAttributoTextAlignmentRight(columnCodiceAtt))
                        return HorizontalAlignment.Right;
                }
            }

            return HorizontalAlignment.Left;
        }


        private void GetGroupRecordsId(MasterDetailGridView mdGridView, Group groupRecord, ref List<Guid> ids)
        {
            //MasterDetailGridView mdGridView = DataGrid.DataContext as MasterDetailGridView;

            if (groupRecord.Source != null)
            {

                IEnumerable<int> indexes = groupRecord.Source.Select(item => (item as VirtualListItem<EntityView>).Index);

                //IEnumerable<Guid> childsId = groupRecord.Source.Select(item => mdGridView.ItemsView.FilteredEntitiesId[(item as VirtualListItem<EntityView>).Index]);
                //ids.AddRange(childsId);

                foreach (int index in indexes)
                {
                    if (0 <= index && index < mdGridView.ItemsView.FilteredEntitiesId.Count)
                        ids.Add(mdGridView.ItemsView.FilteredEntitiesId[index]);
                }
            }

            if (groupRecord.Groups != null)
            {
                foreach (Group grRec in groupRecord.Groups)
                {
                    GetGroupRecordsId(mdGridView, grRec, ref ids);
                }
            }
        }
    }


    /// <summary>
    /// Scopo: Espadere il gruppo solo su click sulla freccia (invece di click sulla riga)
    /// </summary>
    public class CustomCaptionSummaryRowControl : CaptionSummaryRowControl
    {
        //public SfDataGrid DataGrid { get; set; }
        MasterDetailGrid MasterDetailGrid { get; set; }

        public CustomCaptionSummaryRowControl(MasterDetailGrid mdGrid)
            : base()
        {
            MasterDetailGrid = mdGrid;
            this.DefaultStyleKey = typeof(CustomCaptionSummaryRowControl);
        }

        //protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount == 1 && e.Source is GridIndentCell) //click sulla freccetta
        //    {
        //        MasterDetailGridView mdGridView = this.MasterDetailGrid.MasterDataGrid.DataContext as MasterDetailGridView;

        //        MasterDetailGrid.MasterDataGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit();//Altrimenti se sono in edit su una cella il button freccia per l'espansione non va

        //        base.OnPreviewMouseLeftButtonUp(e);//così effettivamente si apre e chiude

        //        //e.Handled = true; //così non parte l'evento di selezione n.b. questo rallenta di molto l'apertura di un raggruppamento selezionato
        //    }
        //}


    }

    /// <summary>
    /// Scopo 1: Disattivare la selezione multipla tramite trascinamento del mouse
    /// Scopo 2: Selezionare i figli quando si seleziona il padre (in caso di raggruppamento)
    /// </summary>
    public class GridSelectionControllerExt : GridSelectionController
    {
        MasterDetailGrid MasterDetailGrid { get; set; }

        public GridSelectionControllerExt(MasterDetailGrid mdGridCtrl)
            : base(mdGridCtrl.MasterDataGrid)
        {
            MasterDetailGrid = mdGridCtrl;
        }

        public void SetPressedIndex2()
        {
            //SetPressedIndex(new RowColumnIndex(3,4));
            
        }


        public override bool HandleKeyDown(KeyEventArgs args)
        {
            bool ret = base.HandleKeyDown(args);

            

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (args.Key == Key.A)
                {
                    MasterDetailGrid.SelectionToCheckedEntities();
                }
            }
            else if (args.Key == Key.Down || args.Key == Key.Up)
            {
                MasterDetailGrid.SelectionToCheckedEntities();
            }
            return ret;
        }

        public override void HandlePointerOperations(GridPointerEventArgs args, Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex rowColumnIndex)
        {
            base.HandlePointerOperations(args, rowColumnIndex);



            if (args.Operation == PointerOperation.Released)
            {

                if (this.DataGrid.View.TopLevelGroup != null)
                {

                    //Get the group from the DisplayElements by resolving record index of corresponding row index
                    var nodeEntry = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex)];

                    if (nodeEntry != null && nodeEntry is Group)
                    {
                        Group group = nodeEntry as Group;


                        if (group.IsExpanded)
                        {
                            if (MasterDetailGrid.MouseIndentButtonPressed)//apertura del gruppo
                            {
                                if (IsGroupSelected(group))//Apertura del gruppo già selezionato
                                {
                                    SelectGroupRows(group);

                                    ////Check wheater the groups contain subgroups or not.
                                    //if (group.Groups != null)
                                    //{
                                    //    //Subgroups are maintained as selected rows based on group.
                                    //    foreach (var subGroup in group.Groups)
                                    //        CheckGroup(subGroup);
                                    //}
                                    //else
                                    //    CheckGroup(group as Group);
                                }
                                else //apertura del gruppo non selezionato
                                {
                                    //var selectedRowColumnINdex = MasterDetailGrid.GetSelectedRowColumnIndex();
                                    //DataGrid.MoveCurrentCell(selectedRowColumnINdex, false);
                                }
                            }
                            else //selezione/deselezione sulla riga del gruppo aperto
                            {
                                SelectGroupRows(group);

                                MasterDetailGrid.MasterDetailGridView.ItemsView.SelectEntityView(null);
                                MasterDetailGrid.MasterDetailGridView.ItemsView.SelectIndex(-1);
                            }
                        }
                        else 
                        {
                            if (MasterDetailGrid.MouseIndentButtonPressed)//chiusura del gruppo aperto
                            {
                                //var selectedRowColumnINdex = MasterDetailGrid.GetSelectedRowColumnIndex();
                                //DataGrid.MoveCurrentCell(selectedRowColumnINdex, false);
                            }
                            else //selezione/deselezione sulla riga del gruppo chiuso
                            {
                                MasterDetailGrid.MasterDetailGridView.ItemsView.SelectEntityView(null);
                                MasterDetailGrid.MasterDetailGridView.ItemsView.SelectIndex(-1);
                            }

                        }
                        

                    }
                }
            }
        }

        private void SelectGroupRows(Group group)
        {

            var startIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
            var startRowIndex = DataGrid.ResolveToRowIndex(startIndex);

            var subGroupValue = this.GetGridSelectedRow(startRowIndex);
            bool isGroupSelected = SelectedRows.Contains(subGroupValue);

            int endIndex = SelectGroupRows(group, isGroupSelected);
            int endRowIndex = DataGrid.ResolveToRowIndex(endIndex);


            if (isGroupSelected)
            {
                DataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
            }
            else
            {
                SelectedRows.RemoveAll(x => startRowIndex <= x.RowIndex && x.RowIndex <= endRowIndex);

                List<int> rowsIndex = SelectedRows.Select(x => x.RowIndex).ToList();

                DataGrid.SelectionController.ClearSelections(false);

                for (int i = 0; i < rowsIndex.Count; i++)
                {
                    var subGroupValue2 = this.GetGridSelectedRow(rowsIndex[i]);
                    if (subGroupValue2 != null && !SelectedRows.Contains(subGroupValue2))
                        SelectedRows.Add(subGroupValue2);

                    DataGrid.SelectionController.SelectRows(rowsIndex[i], rowsIndex[i]);
                }


            }
 
        }

        int SelectGroupRows(Group group, bool select)
        {
            int endIndex = -1;

            SelectGroupRow(group, select);

            //seleziono i figli
            if (group.Groups != null)
            {
                foreach (Group group2 in group.Groups)
                    SelectGroupRow(group2, select);

                endIndex = SelectGroupRows(group.Groups.Last() as Group, select);
            }

            if (group.Records != null)
                endIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group.Records.Last());

            if (endIndex < 0)
                endIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
            

            return endIndex;
        }


        public bool SelectGroupRow(Group group, bool select = true)
        {
            if (group == null)
                return false;

            //Get the corresponding start index of record by getting it from DisplayElements.
            var startindex = this.DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
            ////Resolve the RowIndex of record
            var startRowIndex = this.DataGrid.ResolveToRowIndex(startindex);

            //Below code helps to get the groupvalue based on index value.
            var subGroupValue = this.GetGridSelectedRow(startRowIndex);

            if (select)
            {
                if (subGroupValue != null)
                {
                    //Add the subgroups value to selectedrows collection.
                    if (!this.SelectedRows.Contains(subGroupValue))
                        this.SelectedRows.Add(subGroupValue);
                }
            }
            else
            {
                if (subGroupValue != null)
                {
                    //Remove the subgroups value to selectedrows collection.
                    if (this.SelectedRows.Contains(subGroupValue))
                        this.SelectedRows.Remove(subGroupValue);
                }
            }

            return false;
        }
        private void SelectGroupRows_old2(Group group)
        {
            if (group.Groups != null)
            {
                //caso che il gruppo abbia sottogruppi

                var startIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
                var startRowIndex = DataGrid.ResolveToRowIndex(startIndex);

                var subGroupValue = this.GetGridSelectedRow(startRowIndex);
                bool isGroupSelected = SelectedRows.Contains(subGroupValue);

                int endIndex = SelectGroupRows(group, isGroupSelected);
                int endRowIndex = DataGrid.ResolveToRowIndex(endIndex);


                if (isGroupSelected)
                {
                    DataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
                }
                else
                {
                    SelectedRows.RemoveAll(x => startRowIndex <= x.RowIndex && x.RowIndex <= endRowIndex);

                    List<int> rowsIndex = SelectedRows.Select(x => x.RowIndex).ToList();

                    DataGrid.SelectionController.ClearSelections(false);

                    for (int i = 0; i < rowsIndex.Count; i++)
                    {
                        var subGroupValue2 = this.GetGridSelectedRow(i);
                        if (subGroupValue2 != null && !SelectedRows.Contains(subGroupValue2))
                            SelectedRows.Add(subGroupValue2);

                        DataGrid.SelectionController.SelectRows(rowsIndex[i], rowsIndex[i]);
                    }


                }
            }
            else
            {
                //caso di gruppo con foglie
                //seleziono le foglie nel gruppo

                var startIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
                var startRowIndex = DataGrid.ResolveToRowIndex(startIndex);

                var subGroupValue = this.GetGridSelectedRow(startRowIndex);

                //Get the corresponding end index of the group by getting it from DisplayElements using the inner level group.
                var endIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group.Records.Last());
                var endRowIndex = DataGrid.ResolveToRowIndex(endIndex);


                //Add the subgroups value to selectedrows collection.
                bool isGroupSelected = this.SelectedRows.Contains(subGroupValue);

                if (isGroupSelected)
                {
                    DataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
                }
                else
                {
                    SelectedRows.RemoveAll(x => startRowIndex <= x.RowIndex && x.RowIndex <= endRowIndex);

                    List<int> rowsIndex = SelectedRows.Select(x => x.RowIndex).ToList();

                    DataGrid.SelectionController.ClearSelections(false);
                    for (int i = 0; i < rowsIndex.Count; i++)
                    {
                        DataGrid.SelectionController.SelectRows(rowsIndex[i], rowsIndex[i]);
                    }
                }
            }
        }



        private void SelectGroupRows_old(Group group)
        {
            if (group.Groups != null)
            {
                //caso che il gruppo abbia sottogruppi

                var startIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
                var startRowIndex = DataGrid.ResolveToRowIndex(startIndex);

                int endIndex = 0;
                int endRowIndex = startRowIndex;

                foreach (var gr in group.Groups)
                {
                    SelectGroupRow(gr);

                    //Get the corresponding end index of the group by getting it from DisplayElements using the inner level group.
                    if (gr.Records != null)
                        endIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr.Records.Last());
                    
                    if (endIndex < 0)
                        endIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(gr as Group);

                    endRowIndex = DataGrid.ResolveToRowIndex(endIndex);

                }

                DataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
            }
            else
            {
                //caso di gruppo con foglie
                //seleziono le foglie nel gruppo

                var startIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
                var startRowIndex = DataGrid.ResolveToRowIndex(startIndex);

                var subGroupValue = this.GetGridSelectedRow(startRowIndex);

                //Get the corresponding end index of the group by getting it from DisplayElements using the inner level group.
                var endIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group.Records.Last());
                var endRowIndex = DataGrid.ResolveToRowIndex(endIndex);


                //Add the subgroups value to selectedrows collection.
                bool isGroupSelected = this.SelectedRows.Contains(subGroupValue);

                if (isGroupSelected)
                {
                    DataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);
                }
                else
                {
                    SelectedRows.RemoveAll(x => startRowIndex <= x.RowIndex && x.RowIndex <= endRowIndex);

                    List<int> rowsIndex = SelectedRows.Select(x => x.RowIndex).ToList();

                    DataGrid.SelectionController.ClearSelections(false);
                    for (int i = 0; i < rowsIndex.Count; i++)
                    {
                        DataGrid.SelectionController.SelectRows(rowsIndex[i], rowsIndex[i]);
                    }
                }
            }
        }

       

        private void CheckGroup(Group group)
        {
            //Check whether the group is null or not
            if (group != null && !group.IsExpanded)
            {
                SelectGroupRow(group);
            }
            else
            {
                //Check whether the Group is null, that means the next level contains records and it’s in expanded state
                if (group.Groups == null && group.IsExpanded)
                {
                    //Get the corresponding start index of record by getting it from DisplayElements.
                    var startindex = this.DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
                    ////Resolve the RowIndex of record
                    var startRowIndex = this.DataGrid.ResolveToRowIndex(startindex);

                    ////Below code helps to get the groupvalue based on index value.
                    var subGroupValue = this.GetGridSelectedRow(startRowIndex);

                    //Add the subgroups value to selectedrows collection.
                    if (!this.SelectedRows.Contains(subGroupValue))
                        this.SelectedRows.Add(subGroupValue);


                    //Initialize end index of record
                    var endIndex = 0;

                    //Get the corresponding end index of record by getting it from DisplayElements.
                    endIndex = this.DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group.Records.Last());

                    //Resolve the row index of end index
                    var endRowIndex = this.DataGrid.ResolveToRowIndex(endIndex);

                    //Select the rows from corresponding start and end row index
                    this.DataGrid.SelectionController.SelectRows(startRowIndex, endRowIndex);

                }

            }

        }




        public bool SelectGroupRow_new(Group group, bool toSelect)
        {
            if (group == null)
                return false;

            //Get the corresponding start index of record by getting it from DisplayElements.
            var startindex = this.DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
            ////Resolve the RowIndex of record
            var startRowIndex = this.DataGrid.ResolveToRowIndex(startindex);

            //Below code helps to get the groupvalue based on index value.
            var subGroupValue = this.GetGridSelectedRow(startRowIndex);

            if (toSelect)
            {
                if (subGroupValue != null)
                {
                    //Add the subgroups value to selectedrows collection.
                    if (!this.SelectedRows.Contains(subGroupValue))
                        this.SelectedRows.Add(subGroupValue);
                }

                DataGrid.SelectionController.SelectRows(startRowIndex, startRowIndex);
            }
            else
            {
                if (subGroupValue != null)
                {
                    if (this.SelectedRows.Contains(subGroupValue))
                        this.SelectedRows.Remove(subGroupValue);
                }

            }
            

            return false;
        }

        public bool IsGroupSelected(Group group)
        {
            if (group == null)
                return false;

            //Get the corresponding start index of record by getting it from DisplayElements.
            var startindex = this.DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group as Group);
            ////Resolve the RowIndex of record
            var startRowIndex = this.DataGrid.ResolveToRowIndex(startindex);

            //Below code helps to get the groupvalue based on index value.
            var subGroupValue = this.GetGridSelectedRow(startRowIndex);

            //Add the subgroups value to selectedrows collection.
            if (this.SelectedRows.Contains(subGroupValue))
                return true;

            return false;
        }

        protected override void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            //base.ProcessDragSelection(args, rowColumnIndex);
        }

    }

    /// <summary>
    /// Scopo: Espadere il gruppo solo su click sulla freccia (invece di click sulla riga)
    /// Scopo2: Realizza MasterDetailItemView (Data Virtualization)
    /// </summary>
    public class CustomRowGenerator : RowGenerator
    {
        MasterDetailGrid MasterDetailGrid { get; set; }

        public CustomRowGenerator(MasterDetailGrid mdGrid)
            : base(mdGrid.MasterDataGrid)
        {
            MasterDetailGrid = mdGrid;
        }

        protected override VirtualizingCellsControl GetVirtualizingCellsControl<T>()
        {
            if (typeof(T) == typeof(CaptionSummaryRowControl))
            {
                CustomCaptionSummaryRowControl rowcontrol = new CustomCaptionSummaryRowControl(MasterDetailGrid);
                return rowcontrol;
            }
            return base.GetVirtualizingCellsControl<T>();
        }

        public override void EnsureRows(VisibleLinesCollection visibleRows)
        {
            base.EnsureRows(visibleRows);

            MasterDetailGridView mdGridView = Owner.DataContext as MasterDetailGridView;

            //int RowOnTopCount = 2; //column header e tableSummary

            if (visibleRows.Count <= mdGridView.RowOnTopCount)
            {
                mdGridView.EnsureRows(0);
                return;
            }

            mdGridView.VisibleVirtualListItem.Clear();

            if (View != null && View.TopLevelGroup != null) //raggruppamento in corso
            {
                VisibleLineInfo firstVisibleLine = visibleRows.FirstOrDefault(item => !item.IsHeader);
                int firstVisibleDisplayIndex = (firstVisibleLine != null ? firstVisibleLine.LineIndex : 0) - mdGridView.RowOnTopCount;

                VisibleLineInfo lastVisibleLine = visibleRows.LastOrDefault(item => !item.IsHeader);
                int lastVisibleDisplayIndex = Math.Min(lastVisibleLine.LineIndex, View.TopLevelGroup.DisplayElements.Count - 1);


                int lastVisibleRecordIndex = -1;
                //N.B. Fare attenzione che gli elementi raggruppati fisicamente potrebbero stare "sparsi" nella lista
                //bool firstEntityOfGroup = false;
                NodeEntry nodeEntry = null;
                for (int i = firstVisibleDisplayIndex; i <= lastVisibleDisplayIndex; i++)
                {
                    nodeEntry = this.View.TopLevelGroup.DisplayElements[i];
                    if (i == firstVisibleDisplayIndex)
                        mdGridView.ItemsView.FirstVisibleRowIndex = Owner.ResolveToRowIndex(nodeEntry);


                    if (nodeEntry.IsGroups)
                    {
                        //firstEntityOfGroup = false;

                        if (lastVisibleRecordIndex >= 0)
                            mdGridView.EnsureRows(lastVisibleRecordIndex);
                    }
                    else if (nodeEntry.IsRecords)
                    {
                        RecordEntry recEntry = nodeEntry as RecordEntry;
                        lastVisibleRecordIndex = (recEntry.Data as VirtualListItem<EntityView>).Index;


                        mdGridView.EnsureRows(lastVisibleRecordIndex);
                        mdGridView.VisibleVirtualListItem.Add(recEntry.Data as VirtualListItem<EntityView>);

                    }
                }

                if (lastVisibleRecordIndex >= 0)
                    mdGridView.EnsureRows(lastVisibleRecordIndex);

                mdGridView.ItemsView.LastVisibleRowIndex = Owner.ResolveToRowIndex(nodeEntry);
            }
            else //nessun raggruppamento
            {
                VisibleLineInfo firstVisibleLine = visibleRows.FirstOrDefault(item => !item.IsHeader);
                int firstVisibleRecordIndex = (firstVisibleLine != null ? firstVisibleLine.LineIndex : 0) - mdGridView.RowOnTopCount;

                VisibleLineInfo lastVisibleLine = visibleRows.LastOrDefault(item => !item.IsHeader);
                int lastVisibleRecordIndex = Math.Min((lastVisibleLine != null ? lastVisibleLine.LineIndex : 0) - 1, mdGridView.ItemsView.Entities.Count - 1);

                if (lastVisibleRecordIndex >= 0 && firstVisibleRecordIndex <= lastVisibleRecordIndex && lastVisibleRecordIndex < this.View.Records.Count)
                {
                    RecordEntry firstRecEntry = this.View.Records[firstVisibleRecordIndex];
                    int firstRecIndex = (firstRecEntry.Data as VirtualListItem<EntityView>).Index;
                    mdGridView.EnsureRows(firstRecIndex);

                    RecordEntry lastRecEntry = this.View.Records[lastVisibleRecordIndex];
                    int lastRecIndex = (lastRecEntry.Data as VirtualListItem<EntityView>).Index;
                    mdGridView.EnsureRows(lastRecIndex);

                    mdGridView.ItemsView.FirstVisibleRowIndex = Owner.ResolveToRowIndex(firstRecIndex);
                    mdGridView.ItemsView.LastVisibleRowIndex = Owner.ResolveToRowIndex(lastRecEntry);

                    for (int i = firstRecIndex; i <= lastRecIndex; i++)
                        mdGridView.VisibleVirtualListItem.Add(this.View.Records[i].Data as VirtualListItem<EntityView>);
                }
            }
        }
    }

    /// <summary>
    /// Classe per l'ordinamento custom (in base alla posizione della griglia non raggruppata)
    /// </summary>
    public class SortGroupComparers : IComparer<Group>, ISortDirection
    {
        private MasterDetailGrid _masterDetailGrid;

        public SortGroupComparers(MasterDetailGrid masterDetailGrid)
        {
            this._masterDetailGrid = masterDetailGrid;
        }

        public ListSortDirection SortDirection { get; set; }

        public int Compare(Group x, Group y)
        {
            //return string.Compare(x.Key.ToString(), y.Key.ToString());

            //ordinamento in base all'origine
            if (!x.IsTopLevelGroup)
            {
                int res = 0;

                var groupDesc = x.GetTopLevelGroup().GroupDescriptions[x.Level-1] as PropertyGroupDescription;
                string codiceAttributo = _masterDetailGrid.MasterDetailGridView.ItemsView.GetCodiceAttributoByMappingName(groupDesc.PropertyName);

                var attGroupView = _masterDetailGrid.MasterDetailGridView.RightPanesView.GroupView.Items.FirstOrDefault(item => item.Attributo.Codice == codiceAttributo);
                var attSortView = _masterDetailGrid.MasterDetailGridView.RightPanesView.SortView.Items.FirstOrDefault(item => item.Attributo.Codice == codiceAttributo);
                if (attSortView != null)
                {
                    //ordino in base alla posizione del primo elemento del gruppo nella base dati (non raggruppata)
                    Group group0 = x;
                    List<object> sourceX = x.Source;
                    while (sourceX == null)
                    {
                        group0 = group0.Groups.FirstOrDefault();
                        if (group0 != null)
                            sourceX = group0.Source;
                    }
                    VirtualListItem<EntityView> xVirtualItem = sourceX[0] as VirtualListItem<EntityView>;

                    group0 = y;
                    List<object> sourceY = y.Source;
                    while (sourceY == null)
                    {
                        group0 = group0.Groups.FirstOrDefault();
                        if (group0 != null)
                            sourceY = group0.Source;
                    }
                    VirtualListItem<EntityView> yVirtualItem = sourceY[0] as VirtualListItem<EntityView>;

                    if (xVirtualItem.Index > yVirtualItem.Index)
                        return 1;
                    else
                        return -1;

                }
                else
                {
                    res = attGroupView.Compare(x.Key.ToString(), y.Key.ToString());
                    return res;
                }
                
            }

            return 0;
        }
    }

    /// <summary>
    /// Scopo: Raggruppare le entità senza Realizzarle (prima del fetch)
    /// </summary>
    public class GroupConverter : IValueConverter
    {
        MasterDetailGrid _masterDetailGrid = null;
        EntitiesListMasterDetailView _master = null;

        public GroupConverter(EntitiesListMasterDetailView master)
        {
            _master = master;
        }

        public GroupConverter(MasterDetailGrid masterDetailGrid)
        {
            _masterDetailGrid = masterDetailGrid;
            _master = masterDetailGrid.MasterDetailGridView.ItemsView;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!_master.FilteredEntitiesId.Any())
                return "";

            int index = (value as VirtualListItem<EntityView>).Index;
            GridColumn gridColumn = parameter as GridColumn;

            

            if (gridColumn == null)
                return "";

            if (index >= _master.FilteredEntitiesId.Count)
                return "";

            Guid entId = _master.FilteredEntitiesId[index];

            string codiceAttributo = _master.GetCodiceAttributoByMappingName(gridColumn.MappingName);

            int groupLevel = _master.GetGroupLevelByCodiceAttributo(codiceAttributo);

            string groupKey = "";
            if (groupLevel >= 0)
                groupKey = _master.FilteredEntitiesViewInfo[entId].GroupKeys.ElementAt(groupLevel);

            return groupKey;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

    }


    /// <summary>
    /// Scopo: Intercettare l'evento che indica che è stata nascosta una colonna
    /// </summary>
    class GridColumnChooserControllerExt : GridColumnChooserController
    {
        public event EventHandler IsColumnHiddenChanged;

        public GridColumnChooserControllerExt(SfDataGrid dataGrid, IColumnChooser columnChooserwindow) : base(dataGrid, columnChooserwindow)
        {
        }

        protected override void OnColumnHiddenChanged(GridColumn column)
        {
            base.OnColumnHiddenChanged(column);
            //if (column.IsHidden)
                IsColumnHiddenChanged?.Invoke(this, new EventArgs());

        }
    }

    public class SelectItemsResponse
    {
        public ObservableCollection<object> SelectedItems = null;
        public VirtualListItem<EntityView> CurrentItem { get; internal set; }
        public HashSet<Group> SelectedGroups { get; internal set; }
    }

    public enum SelectOperation
    {
        AddOnly = 0,
        Add = 1,
        Remove = 2, 
    }

}
