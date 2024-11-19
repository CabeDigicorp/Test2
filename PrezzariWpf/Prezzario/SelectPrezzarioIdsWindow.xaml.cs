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
    public partial class SelectPrezzarioIdsWindow : Window
    {
        internal SelectPrezzarioIdsView View { get => DataContext as SelectPrezzarioIdsView; }
        public SelectPrezzarioIdsIOData IOData { get; set; } = new SelectPrezzarioIdsIOData();

        static double[] StartupLocation = new double[] { 0, 0 };
        static double[] Dimension = new double[] { 0, 0 };
        static double DetailGridWidth { get; set; } = 0;


        public SelectPrezzarioIdsWindow()
        {
            InitializeComponent();

            if (StartupLocation[0] != 0)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = StartupLocation[0];
                Top = StartupLocation[1];

            }

            if (Dimension[0] > 0)
            {
                Width = Dimension[0];
                Height = Dimension[1];
            }

            if (DetailGridWidth > 0) 
            {
                PrezzarioCtrl.DetailColumn.Width = new GridLength(DetailGridWidth);
            }

            base.Closing += SelectPrezzarioIdsWindow_Closing;
            base.Closed += SelectPrezzarioIdsWindow_Closed;
        }

        private void SelectPrezzarioIdsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StartupLocation[0] = Left;
            StartupLocation[1] = Top;

            Dimension[0] = Width;
            Dimension[1] = Height;

            DetailGridWidth = PrezzarioCtrl.DetailColumn.ActualWidth;
        }

        private void SelectPrezzarioIdsWindow_Closed(object sender, EventArgs e)
        {
            View.PrezzarioView.ItemsView.AllowNoSelection = false;
        }

        public void Init()
        {
            PrezzarioCtrl.Init();
            View.IOData = IOData;
            
            View.Init();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            View.PrezzarioView = PrezzarioCtrl.DataContext as PrezzarioView;
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

        /// <summary>
        /// Uscita dal dialogo con doppio click su un'entità
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrezzarioCtrl_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            View.PrezzarioView.ItemsView.CheckedEntitiesId = new HashSet<Guid>() { View.PrezzarioView.ItemsView.SelectedEntityId };

            AcceptSelection();
        }


        private void AcceptSelection()
        {
            //N.B. non so bene perche alla chiusura della finestra questo evento a volte parta 2 volte
            if (DialogResult != null)
                return;

            

            //prendo solo gli elementi con prezzo (foglie)
            IEnumerable<Guid> selectedItemsId = View.PrezzarioView.ItemsView.CheckedEntitiesId.Where(item => !View.PrezzarioView.ItemsView.HasChildren(item));

            UpdateViewSettings();

            if (IOData.IsSingleSelection)
            {

                if (selectedItemsId.Count() == 1)
                {
                    IOData.PrezzarioItemSelectedIds = selectedItemsId.ToList<Guid>();

                    if (View.SelectedItem != null && View.SelectedItem.FileName.Any())
                        IOData.ExternalPrezzarioFileName = View.SelectedItem.FileName;
                    else
                        IOData.ExternalPrezzarioFileName = string.Empty;

                    DialogResult = true;
                }
                else if (selectedItemsId.Count() > 1)
                {
                    IOData.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("NonPossibileSelezionarePiuDiUnArticolo"));
                    DialogResult = false;
                }
                else
                {
                    if (IOData.AllowNoSelection && View.PrezzarioView.ItemsView.IsNoSelectionChecked)
                    {
                        IOData.PrezzarioItemSelectedIds = new List<Guid>() { Guid.Empty };

                        if (View.SelectedItem != null && View.SelectedItem.FileName.Any())
                            IOData.ExternalPrezzarioFileName = View.SelectedItem.FileName;
                        else
                            IOData.ExternalPrezzarioFileName = string.Empty;

                        DialogResult = true;
                    }
                    else
                    {
                        IOData.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("OccorreSelezionareUnArticolo"));
                        DialogResult = false;
                    }
                }


            }
            else
            {
                IOData.PrezzarioItemSelectedIds = selectedItemsId.ToList<Guid>();

                if (View.SelectedItem != null && View.SelectedItem.FileName.Any())
                    IOData.ExternalPrezzarioFileName = View.SelectedItem.FileName;
                else
                    IOData.ExternalPrezzarioFileName = string.Empty;

                DialogResult = true;
            }
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
