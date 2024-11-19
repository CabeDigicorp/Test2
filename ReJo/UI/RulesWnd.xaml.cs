using CommonResources;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.UI;

namespace ReJo.UI
{
    /// <summary>
    /// Interaction logic for TestWnd.xaml
    /// </summary>
    public partial class RulesWnd : Window
    {
        public static RulesWnd? This = null;

        public RulesView View => (RulesView)this.DataContext;

        public static new bool IsLoaded { get; set; } = false;

        public static void Create()
        {

            if (This != null)
                return;

            This = WindowManager.CreateRulesWnd();
            //This = new RulesWnd();
            This.SourceInitialized += (x, y) => This.HideMinimizeAndMaximizeButtons();

            IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            Window? window = HwndSource.FromHwnd(revitHandle)?.RootVisual as Window;
            This.Owner = window;
        }
        public static void Dispose()
        {
            This = null;
        }


        public RulesWnd()
        {
            InitializeComponent();

            This = this;

            this.Loaded += RulesWnd_Loaded;
            this.Closed += RulesWnd_Closed;

            this.ParametersGrid.RowDragDropController.DragStart += ParametersGrid_DragStart;

            this.AttributiGrid.RowDragDropController.DragStart += AttributiGrid_DragStart;
            this.AttributiGrid.RowDragDropController.Dropped += AttributiGrid_Dropped;
            this.QuantitaTextBox.Drop += QuantitaTextBox_Drop;
            this.QuantitaTextBox.PreviewDragOver += QuantitaTextBox_PreviewDragOver; 


        }



        private void ParametersGrid_DragStart(object? sender, GridRowDragStartEventArgs e)
        {

        }

        private void AttributiGrid_DragStart(object? sender, Syncfusion.UI.Xaml.Grid.GridRowDragStartEventArgs e)
        {
            e.Handled = true;
        }

        private void AttributiGrid_Dropped(object? sender, Syncfusion.UI.Xaml.Grid.GridRowDroppedEventArgs e)
        {
               
            var draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

            object? obj = draggingRecords.FirstOrDefault();

            if (obj is ParameterItemView parameterItemView)
            {
                int dropIndex = (int)e.TargetRecord;

                if (dropIndex < View.FormuleAttributi.Count)
                {
                    FormuleAttributiItemView formuleAttItem = View.FormuleAttributi[dropIndex];
                    if (formuleAttItem != null)
                    {
                        formuleAttItem.AttributoValore += parameterItemView.Formula;
                    }
                }
            }
            else if (obj is MaterialQtaItemView matQtaItemView)
            {
                int dropIndex = (int)e.TargetRecord;

                if (dropIndex < View.FormuleAttributi.Count)
                {
                    FormuleAttributiItemView formuleAttItem = View.FormuleAttributi[dropIndex];
                    if (formuleAttItem != null)
                    {
                        formuleAttItem.AttributoValore += matQtaItemView.Formula;
                    }
                }
            }

        }

        private void QuantitaTextBox_Drop(object sender, DragEventArgs e)
        {
            var draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

            object? obj = draggingRecords.FirstOrDefault();

            if (obj is ParameterItemView parameterItemView)
            {
                View.FormulaQuantita += parameterItemView.Formula;
            }
            else if (obj is MaterialQtaItemView matQtaItemView)
            {
                View.FormulaQuantita += matQtaItemView.Formula;
            }
        } 
        
        private void QuantitaTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }


        private void RulesWnd_Loaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = true;
        }

        private void RulesWnd_Closed(object? sender, EventArgs e)
        {
            Dispose();
            IsLoaded = false;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            View.OnOk();
            Close();
            Dispose();
            IsLoaded = false;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Dispose();
            IsLoaded = false;

        }

        private void CurrentElementTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int number = 0;
                if (int.TryParse(CurrentElementTextBox.Text, out number))
                {
                    View.CurrentElementNumber = number;
                }
            }
        }


    }



}
