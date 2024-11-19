using DevExpress.Xpf.Spreadsheet;
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
    /// Interaction logic for SpreadsheetWnd.xaml
    /// </summary>
    public partial class SpreadsheetWnd : Window
    {
        public SpreadsheetWnd()
        {
            InitializeComponent();
        }
        public SpreadsheetControl GetSpreadSheetControl()
        {
            return this.SpreadSheetCtrl;
        }
    }
}
