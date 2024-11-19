using MasterDetailView;
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

namespace MasterDetailWpf
{
    /// <summary>
    /// Interaction logic for AttributoTestoCollectionEditWindow.xaml
    /// </summary>
    public partial class AttributoTestoCollectionEditWindow : Window
    {
        public ValoreTestoCollectionItemView ItemView { get; set; }
        public EntitiesListMasterDetailView Master { get; set; }
        private bool _isClosing = false;

        public AttributoTestoCollectionEditWindow()
        {
            InitializeComponent();
            Loaded += AttributoTestoCollectionEditWindow_Loaded;
            Closing += AttributoTestoCollectionEditWindow_Closing;
        }

        private void AttributoTestoCollectionEditWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isClosing = true;
        }

        private void AttributoTestoCollectionEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemView != null)
            {
                EtichettaTextBox.Text = ItemView.Testo1;
                ValoreTextBox.Text = ItemView.Testo2;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //Scopo: Altrimenti alla chiusura non torna sempre col fuoco sull'owner
            Owner = null;

            if (ItemView != null)
            {
                ItemView.Testo1 = EtichettaTextBox.Text;
                ItemView.Testo2 = ValoreTextBox.Text;
            }
            

            if (!_isClosing)
                Close();

            //Master.UpdateCache(true);

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (!_isClosing)
                    Close();//parte la deactivated
            }
        }
    }
}
