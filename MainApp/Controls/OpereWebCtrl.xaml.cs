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
    /// Interaction logic for OpereWebCtrl.xaml
    /// </summary>
    public partial class OpereWebCtrl : UserControl
    {
        public OpereWebView View { get => DataContext as OpereWebView; }

        public event EventHandler<EventArgs> SelectedOperaIdChanged;
        public Guid SelectedOperaId { get; set; } = Guid.Empty;
        public string SelectedOperaNome { get; set; } = string.Empty;


        public OpereWebCtrl()
        {
            InitializeComponent();
        }

        internal void Load()
        {
            View.Load();
            
        }

        private void OpereList_SelectionChanged(object sender, EventArgs e)
        {
            if (OpereList.SelectedItem == null)
            {
                SelectedOperaId = Guid.Empty;
                SelectedOperaNome = "-";
            }
            else
            {
                var selectedOpera = OpereList.SelectedItem as OperaView;
                SelectedOperaId = selectedOpera.Id;
                SelectedOperaNome = selectedOpera.Nome;
            }

            SelectedOperaIdChanged?.Invoke(this, e);
        }
    }
}
