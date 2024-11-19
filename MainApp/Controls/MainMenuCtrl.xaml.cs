using Commons;
using DevExpress.XtraSpreadsheet.Model.NumberFormatting;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for MainMenuCtrl.xaml
    /// </summary>
    public partial class MainMenuCtrl : UserControl
    {

        MainMenuView MainMenuView { get { return DataContext as MainMenuView; } }

        public MainMenuCtrl()
        {
            InitializeComponent();

            MainMenuView.DivisioniView = divisioniInterneCtrl.View;
            MainMenuView.ComputoView = computoCtrl.View;
            MainMenuView.ElencoPrezziView = elencoPrezziCtrl.View;
            MainMenuView.AttivitaView = attivitaCtrl.View;
            MainMenuView.ElementiView = elementiCtrl.View;
            MainMenuView.DatiGeneraliView = datiGeneraliCtrl.View;
            MainMenuView.StampeView = stampeCtrl.View;
            MainMenuView.FogliDiCalcoloView = foglioDiCalcolo.View;

            LayoutUpdated += MainMenuCtrl_LayoutUpdated;
        }

        private void MainMenuCtrl_LayoutUpdated(object sender, EventArgs e)
        {
            if (MainViewStatus.IsAdvancedMode)
                ExtraInfo.Visibility = Visibility.Visible;
            else
                ExtraInfo.Visibility = Visibility.Collapsed;

            string str = string.Format("ExtraInfo");
            str += string.Format("\n Environment.MachineName: {0}", Environment.MachineName);
            str += string.Format("\n Environment.SpecialFolder.MyDocuments: {0}", System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            str += string.Format("\n AppSettingsPath: {0}", MainMenuView.AppSettingsPath);
            str += string.Format("\n WindowsRegistryResult: {0}", MainMenuView.WindowsRegistryResult);
            str += string.Format("\n System.Security.Principal.WindowsIdentity.GetCurrent().Name): {0}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);



            ExtraInfo.Text = str;
        }

        private void RecentProjectsGrid_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            RecentProjectsGrid.SelectedItems.Clear();
            RecentProjectsGrid.View.Refresh();

        }

        private void assistenzaEmail_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = e.Uri.AbsoluteUri;
            process.Start();


            //System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }


        //private void LoadBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    var assembly = Assembly.GetExecutingAssembly();

        //    string[] resourceNames = assembly.GetManifestResourceNames();

        //    var resourceName = "MainApp.test.html";

        //    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        //    using (StreamReader reader = new StreamReader(stream))
        //    {
        //        string result = reader.ReadToEnd();
        //        wvc.NavigationCompleted += Wvc_NavigationCompletedAsync;
        //        wvc.ScriptNotify += Wvc_ScriptNotify;
        //        wvc.NavigateToString(result);

        //        //wvc.NavigateToString(htmlFragment);


        //    }
        //}


    }
}
