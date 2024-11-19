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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for ProgettiWebCtrl.xaml
    /// </summary>
    public partial class ProgettiWebCtrl : UserControl
    {
        ProgettiWebView View { get => DataContext as ProgettiWebView; }

        public ProgettiWebCtrl()
        {
            InitializeComponent();
        }

        internal void Load(Guid operaId)
        {
            View.Load(operaId);
        }

        public string NomeProgetto { get => View.NomeProgetto; set => View.NomeProgetto = value; }
        public Guid ProgettoId { get => View.ProgettoId; set => View.ProgettoId = value; }
        public bool IsNomeProgettoReadOnly { get => View.IsNomeProgettoReadOnly; set => View.IsNomeProgettoReadOnly = value; }

    }
}
