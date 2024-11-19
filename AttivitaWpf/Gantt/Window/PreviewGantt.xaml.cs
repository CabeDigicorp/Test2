using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Ribbon;
using DevExpress.XtraPrinting.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for PreviewGantt.xaml
    /// </summary>
    public partial class PreviewGantt : Window
    {
        public bool Save { get; set; }
        private PreviewGanttView view { get { return DataContext as PreviewGanttView; } }
        public PreviewGantt()
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
            RibbonControl rc = LayoutTreeHelper.GetVisualChildren(DocumentPreviewControl).OfType<RibbonControl>().FirstOrDefault();
            rc.MinimizationButtonVisibility = RibbonMinimizationButtonVisibility.Collapsed;
            rc.AllowSimplifiedRibbon = true;
            rc.IsSimplified = true;
        }

        private void ButtonRipristinaFormatoDa_Click(object sender, RoutedEventArgs e)
        {
            view.GetDefaultDa();
        }

        private void ButtonRipristinaFormatoA_Click(object sender, RoutedEventArgs e)
        {
            view.GetDefaultA();
        }
    }
}
