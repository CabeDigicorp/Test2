using CommonResources;
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

namespace FogliDiCalcoloWpf
{
    /// <summary>
    /// Interaction logic for FoglioDiCalcoloWBSGanttDataWnd.xaml
    /// </summary>
    public partial class FoglioDiCalcoloWBSGanttDataWnd : Window
    {
        public FoglioDiCalcoloGanttDataView View { get => DataContext as FoglioDiCalcoloGanttDataView; }
        public FoglioDiCalcoloWBSGanttDataWnd()
        {
            InitializeComponent();
            TextBoxFilter.LostFocus += TextBoxFilter_LostFocus;
            TextBoxFilter.GotFocus += TextBoxFilter_GotFocus;
            TextBoxFilter.Foreground = Brushes.DarkGray;
        }

        private void TextBoxFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxFilter.Text == LocalizationProvider.GetString("Filtra"))
            {
                View.TextSearched = "";
                TextBoxFilter.Foreground = Brushes.Black;
            }
        }

        private void TextBoxFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxFilter.Text == "")
            {
                View.TextSearched = LocalizationProvider.GetString("Filtra");
                TextBoxFilter.Foreground = Brushes.DarkGray;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (View.Accept())
                DialogResult = true;
        }

        public string AttributeCopied { get; set; }
        private void CopyAttribute_Click(object sender, RoutedEventArgs e)
        {
            View.GetCopiedText = "[" + View.FiltratoSelezionato.Etichetta + "]";
        }
    }
}
