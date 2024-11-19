using Commons;
using MasterDetailView;
using Model;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ContattiWpf
{
    /// <summary>
    /// Interaction logic for ContattiAttributiSettingsWnd.xaml
    /// </summary>
    public partial class ContattiAttributiSettingsWnd : Window
    {
        AttributiSettingsView View { get => DataContext as AttributiSettingsView; }

        //int attributiSettingsViewLastSelectedIndex = -1;
        int _selectedTabIndex = -1;

        public ContattiAttributiSettingsWnd()
        {
            InitializeComponent();
            //AttributiSettingsGrid.EditTrigger = EditTrigger.OnTap;

            OrdinamentoSettingsGrid.RowDragDropController.Drop += RowDragDropController_Drop;
            OrdinamentoSettingsGrid.RowDragDropController.DragOver += RowDragDropController_DragOver;

        }


        internal void Init(ClientDataService dataService, IMainOperation mainOperation, IEntityWindowService windowService, string entityTypeKey, string currentCodiceAtt)
        {
            View.Init(dataService, mainOperation, windowService, entityTypeKey, currentCodiceAtt);
            View.RefreshView += AttributiSettingsView_RefreshView;
            AttributiSettingsGrid.Loaded += AttributiSettingsGrid_Loaded;

            HashSet<string> mappingNames = new HashSet<string>(AttributiSettingsGrid.Columns.Select(item => item.MappingName));
            if (mappingNames.Contains("Codice"))
            {
                AttributiSettingsGrid.Columns["Codice"].IsHidden = true;
                if (View.IsAdvancedMode)
                    AttributiSettingsGrid.Columns["Codice"].IsHidden = false;
            }

            HashSet<string> mappingNamesRif = new HashSet<string>(AttributiRiferimentoGrid.Columns.Select(item => item.MappingName));
            if (mappingNamesRif.Contains("Codice"))
            {
                AttributiRiferimentoGrid.Columns["Codice"].IsHidden = true;
                if (View.IsAdvancedMode)
                    AttributiRiferimentoGrid.Columns["Codice"].IsHidden = false;
            }
        }

        private void AttributiSettingsView_RefreshView(object sender, EventArgs e)
        {
            if (AttributiSettingsGrid != null)
            {
                View.SelectAttributoView(View.CurrentAttributoSettings);
                SelectItem();
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }

        private void AttributiSettingsGrid_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            int columnIndex = AttributiSettingsGrid.ResolveToGridVisibleColumnIndex(e.CurrentRowColumnIndex.ColumnIndex);
            if (columnIndex < 0)
                return;

            string mappingName = AttributiSettingsGrid.Columns[columnIndex].MappingName;
            View.CurrentAttributoSettings.CurrentCellActivated(mappingName);

        }



        #region Drag&Drop
        private void RowDragDropController_DragOver(object sender, GridRowDragOverEventArgs e)
        {
            e.ShowDragUI = false;
        }

        private void RowDragDropController_Drop(object sender, GridRowDropEventArgs e)
        {
            if (e.DraggingRecords == null)
                return;

            AttributoSettingsView draggingAtt = e.DraggingRecords[0] as AttributoSettingsView;
            View.AttributiItems.Remove(draggingAtt);
            View.AttributiItems.Insert((int)e.TargetRecord, draggingAtt);

            View.CurrentAttributoSettings = draggingAtt;
            View.SelectAttributoView(draggingAtt);

        }


        #endregion Drag&Drop

        //private void AddAttributoChild_Click(object sender, RoutedEventArgs e)
        //{
        //    int rowIndex = AttributiSettingsGrid.ResolveToRowIndex(AttributiSettingsGrid.SelectedItem);
        //    int recordIndex = AttributiSettingsGrid.ResolveToRecordIndex(rowIndex);

        //    AttributiSettingsGrid.ExpandDetailsViewAt(recordIndex);
        //}

        private void AttributoRiferimentoGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (AttributiSettingsGrid.SelectedDetailsViewGrid != null)
            {
                object detailCurrentItem = AttributiSettingsGrid.SelectedDetailsViewGrid.CurrentItem;
                if (detailCurrentItem != null)
                    View.CurrentAttributoSettings = detailCurrentItem as AttributoSettingsView;
                else if (AttributiSettingsGrid.CurrentItem != null)
                {
                    AttributoSettingsGuidRiferimentoView currentAttView = AttributiSettingsGrid.CurrentItem as AttributoSettingsGuidRiferimentoView;

                    //Scopo: Aggirare il problema che la griglia non cambia il Current se il fuoco passa dal figlio al padre
                    if (currentAttView != null)
                    {
                        AttributoSettingsView parentAttView = null;
                        object parentObjView = AttributiSettingsGrid.GetGridDetailsViewRecord(AttributiSettingsGrid.SelectedDetailsViewGrid as DetailsViewDataGrid);
                        RecordEntry rec = parentObjView as RecordEntry;
                        if (rec != null)
                            parentAttView = rec.Data as AttributoSettingsView;

                        if (parentAttView != null && currentAttView.GetReferenceCodiceGuid() == parentAttView.GetCodice())
                        {
                            AttributoSettingsView attributoSettingsView = AttributiSettingsGrid.SelectedItem as AttributoSettingsView;
                            View.CurrentAttributoSettings = attributoSettingsView;
                        }
                    }

                }
            }
        }

        private void AttributiSettingsTabCtrl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //Scopo: Portare lo stesso fuoco dalla griglia del primo tab alla seconda

            //nessuna modifica del tab corrente (l'evento parte anche quando si sceglie un item di un combo)
            if (AttributiSettingsTabCtrl.SelectedIndex == _selectedTabIndex)
                return;

            if (AttributiSettingsTabCtrl.SelectedIndex == 0)
            {
                if (AttributiSettingsGrid != null)
                {
                    View.SelectAttributoView(View.CurrentAttributoSettings);
                }
            }
            else if (AttributiSettingsTabCtrl.SelectedIndex == 1)
            {
                if (OrdinamentoSettingsGrid != null)
                    OrdinamentoSettingsGrid.SelectedItem = View.CurrentAttributoSettings;
            }

            _selectedTabIndex = AttributiSettingsTabCtrl.SelectedIndex;

        }

        //private void SelectAttributoView(AttributoSettingsView attView)
        //{
        //    if (attView is AttributoSettingsRiferimentoView)
        //    {
        //        AttributoSettingsRiferimentoView currAttRif = attView as AttributoSettingsRiferimentoView;
        //        AttributoSettingsView currAttGuid = View.AttributiFirstLevelItems.FirstOrDefault(item => item.GetCodice() == currAttRif.GetReferenceCodiceGuid());
        //        int attGuidRecordIndex = View.AttributiFirstLevelItems.IndexOf(currAttGuid);
        //        AttributiSettingsGrid.ExpandDetailsViewAt(attGuidRecordIndex);
        //        SfDataGrid detailDataGrid = AttributiSettingsGrid.GetDetailsViewGrid(attGuidRecordIndex, "AttributoRiferimentoItems");
        //        if (detailDataGrid != null)
        //            detailDataGrid.SelectedItem = View.CurrentAttributoSettings as AttributoSettingsView;
        //    }
        //    else
        //        AttributiSettingsGrid.SelectedItem = attView as AttributoSettingsView;
        //}

        private void AttributiSettingsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SelectItem();
        }

        private void SelectItem()
        {
            if (0 <= View.AttributoGuidRecordIndex && View.AttributoGuidRecordIndex < View.AttributiFirstLevelItems.Count)
            {
                AttributiSettingsGrid.ExpandDetailsViewAt(View.AttributoGuidRecordIndex);
                SfDataGrid detailDataGrid = AttributiSettingsGrid.GetDetailsViewGrid(View.AttributoGuidRecordIndex, "AttributoRiferimentoItems");
                if (detailDataGrid != null)
                    detailDataGrid.SelectedItem = View.CurrentAttributoSettings as AttributoSettingsView;
            }
            else
            {
                AttributiSettingsGrid.SelectedItem = View.CurrentAttributoSettings as AttributoSettingsView;
            }
        }
    }
}
