using _3DModelExchange;
using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using PrezzariWpf.Prezzario;
using PrezzariWpf.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PrezzariWpf
{
    /// <summary>
    /// Interaction logic for SelectArticoliWindow.xaml
    /// </summary>
    public partial class SelectPrezzarioWindow : Window
    {
        internal SelectPrezzarioIdsView View { get => DataContext as SelectPrezzarioIdsView; }
        public SelectPrezzarioIdsIOData IOData { get; set; } = new SelectPrezzarioIdsIOData();

        public static double[] StartupLocation = new double[] { 0, 0 };

        
        public SelectPrezzarioWindow()
        {
            InitializeComponent();

            if (StartupLocation[0] != 0)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = StartupLocation[0];
                Top = StartupLocation[1];

            }

            base.Closing += SelectPrezzarioIdsWindow_Closing;
            //base.Closed += SelectPrezzarioIdsWindow_Closed;
        }

        private void SelectPrezzarioIdsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StartupLocation[0] = Left;
            StartupLocation[1] = Top;
        }

        //private void SelectPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        //{
        //    View.PrezzarioView.ItemsView.AllowNoSelection = false;
        //    //View.InitPrezzarioView(null);
        //}

        public void Init()
        {
            //PrezzarioCtrl.Init();
            View.IOData = IOData;
            
            View.Init();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //View.PrezzarioView = PrezzarioCtrl.DataContext as PrezzarioView;
            View.Load();
        }

        /// <summary>
        /// Uscita dal dialogo con pulsante con spunta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptSelection();

        }

        ///// <summary>
        ///// Uscita dal dialogo con doppio click su un'entità
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void PrezzarioCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        //{
        //    View.PrezzarioView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { View.PrezzarioView.ItemsView.SelectedEntityId };

        //    AcceptSelection();
        //}


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            if (View.SelectedItem != null && View.SelectedItem.FileName.Any())
                IOData.ExternalPrezzarioFileName = View.SelectedItem.FileName;

            DialogResult = true;

        }

        private void UpdateViewSettings()
        {


            //if (View.PrezzarioView.ItemsView.RightPanesView.GroupView.Data != null && View.PrezzarioView.ItemsView.RightPanesView.GroupView.Data.Items.Any())
            //    IOData.CurrentViewSettings.Groups = View.PrezzarioView.ItemsView.RightPanesView.GroupView.Data.Items.Select(item => item.Clone()).ToList();

            //if (View.PrezzarioView.ItemsView.RightPanesView.SortView.Data != null && View.PrezzarioView.ItemsView.RightPanesView.SortView.Data.Items.Any())
            //    IOData.CurrentViewSettings.Sorts = View.PrezzarioView.ItemsView.RightPanesView.SortView.Data.Items.Select(item => item.Clone()).ToList();

            //if (View.PrezzarioView.ItemsView.RightPanesView.FilterView.Data != null && View.PrezzarioView.ItemsView.RightPanesView.FilterView.Data.Items.Any())
            //    IOData.CurrentViewSettings.Filters = View.PrezzarioView.ItemsView.RightPanesView.FilterView.Data.Items.Select(item => item.Clone()).ToList();

            View.PrezzarioView.ItemsView.RightPanesView.UpdateViewSettings(IOData.CurrentViewSettings);

        }

        private void DownloadPrezzari_Click(object sender, RoutedEventArgs e)
        {
            SelectRemotePrezzariWindow selectRemotePrezzariWnd = new SelectRemotePrezzariWindow();
            selectRemotePrezzariWnd.SourceInitialized += (x, y) => selectRemotePrezzariWnd.HideMinimizeAndMaximizeButtons();
            selectRemotePrezzariWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            selectRemotePrezzariWnd.View.MainOperation = View.IOData.MainOperation;
            selectRemotePrezzariWnd.Init(View.PrezzariInfoDownloaded);

            if (selectRemotePrezzariWnd.ShowDialog() == true)
            {
                View.DownloadPrezzari(selectRemotePrezzariWnd.FileNamesReturned);
                
            }

        }

    }


}
