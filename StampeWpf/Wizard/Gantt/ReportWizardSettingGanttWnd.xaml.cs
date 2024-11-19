using AttivitaWpf;
using CommonResources;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Ribbon;
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

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for ReportWizardSettingGanttWnd.xaml
    /// </summary>
    public partial class ReportWizardSettingGanttWnd : Window
    {
        public bool Save { get; set; }
        private ReportWizardSettingGanttView view { get { return DataContext as ReportWizardSettingGanttView; } }
        public ReportWizardSettingGanttWnd()
        {
            InitializeComponent();
        }

        private void ButtonsZoomIndietro_Click(object sender, RoutedEventArgs e)
        {
            view.DoZoom(true);
        }

        private void ButtonsZoomAvanti_Click(object sender, RoutedEventArgs e)
        {
            view.DoZoom(false);
        }

        private void ButtonsBarButtonSettingGantt_Click(object sender, RoutedEventArgs e)
        {
            PreviewGanttSetting previewGanttSetting = new PreviewGanttSetting();
            previewGanttSetting.SourceInitialized += (x, y) => previewGanttSetting.HideMinimizeAndMaximizeButtons();
            previewGanttSetting.Owner = System.Windows.Application.Current.MainWindow;
            previewGanttSetting.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewGanttSetting.DataContext = view.previewGanttSettingView;
            if (previewGanttSetting.ShowDialog() == true)
                view.UpdateColumnWidth();



        }

        private void ButtonsScala_Click(object sender, RoutedEventArgs e)
        {
            PreviewGanttScalaWnd previewGanttScalaWnd = new PreviewGanttScalaWnd();
            previewGanttScalaWnd.SourceInitialized += (x, y) => previewGanttScalaWnd.HideMinimizeAndMaximizeButtons();
            previewGanttScalaWnd.Owner = System.Windows.Application.Current.MainWindow;
            previewGanttScalaWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewGanttScalaWnd.DataContext = view.previewGanttScalaView;
            view.previewGanttScalaView.Init(view.ZoomFactor,view.AdjustToPage);

            previewGanttScalaWnd.ShowDialog();

            view.UpdateScale();
        }


        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            view.Preview();
        }
        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            view.AcceptButton();
            Save = true;
            this.Close();
        }
        private void DocumentPreviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            //RibbonControl rc = LayoutTreeHelper.GetVisualChildren(DocumentPreviewControl).OfType<RibbonControl>().FirstOrDefault();
            //rc.MinimizationButtonVisibility = RibbonMinimizationButtonVisibility.Collapsed;
            //rc.AllowSimplifiedRibbon = true;
            //rc.IsSimplified = true;
        }

        private void ButtonRipristinaFormatoDa_Click(object sender, RoutedEventArgs e)
        {
            view.GetDefaultDa();
        }

        private void ButtonRipristinaFormatoA_Click(object sender, RoutedEventArgs e)
        {
            view.GetDefaultA();
        }
        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentPreviewControl.ShowNavigationPane = true;
        }
    }
}