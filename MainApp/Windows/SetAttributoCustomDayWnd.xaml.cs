using MasterDetailView;
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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for SetAttributoCustomDayWnd.xaml
    /// </summary>
    public partial class SetAttributoCustomDayWnd : Window
    {
        public SetAttributoCustomDayView View { get => DataContext as SetAttributoCustomDayView; }

        public SetAttributoCustomDayWnd()
        {
            InitializeComponent();
            //TotaleOrario.Focusable = true;
            //TotaleOrario.Focus();
            AcceptButton.Focusable = false;
            this.Loaded += SetAttributoCustomDayWnd_Loaded;
        }

        private void SetAttributoCustomDayWnd_Loaded(object sender, RoutedEventArgs e)
        {
            View.ApplicaStile += View_ApplicaStile;
        }

        private void View_ApplicaStile(object sender, EventArgs e)
        {
            var selector = Calendar.DayButtonStyleSelector as DayButtonStyleSelector;            
            Calendar.DayButtonStyleSelector = null;
            Calendar.DayButtonStyleSelector = selector;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.Enter))
                if (View.Accept())
                    DialogResult = true;
        }

        private void ListaEccezioni_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (!View.IsActiveSelzioneDaCalendario)
            {
                View.IsActiveSelzioneDaLista = true;
                Calendar.SelectedDates.Clear();
                foreach (CustomDayLocal customDayLocal in View.SelectedItemsLista)
                {
                    DateTime date = new DateTime(customDayLocal.Day.Year, customDayLocal.Day.Month, customDayLocal.Day.Day);
                    Calendar.SelectedDates.Add(date);
                }
                View.IsActiveSelzioneDaLista = false;
            }
        }

        private void Calendar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                View.SelectionChanged(e.AddedItems);
            if (e.RemovedItems.Count > 0)
                View.RemoveItems(e.RemovedItems);
        }

        //private void TotaleOrario_LostFocus(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
