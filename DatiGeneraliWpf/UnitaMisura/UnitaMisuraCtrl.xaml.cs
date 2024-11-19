using DatiGeneraliWpf.View;
using MasterDetailView;
using MasterDetailWpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatiGeneraliWpf
{
    /// <summary>
    /// Interaction logic for UnitaMisuraCtrl.xaml
    /// </summary>
    public partial class UnitaMisuraCtrl : UserControl
    {
        UnitaMisuraView UnitaMisuraView { get => DataContext as UnitaMisuraView; }

        public UnitaMisuraCtrl()
        {
            InitializeComponent();
            DataContextChanged += UnitaMisuraCtrl_DataContextChanged;
        }

        private void UnitaMisuraCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (UnitaMisuraView == null)
                return;

            UnitaMisuraView.NumericFormatView = NumericFormatCtrl.DataContext as NumericFormatView;
            UnitaMisuraView.Init();
        }
    }
}
