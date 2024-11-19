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

namespace MainApp
{
    /// <summary>
    /// Interaction logic for WebLoginWnd.xaml
    /// </summary>
    public partial class WebLoginWnd : Window
    {
        public WebLoginView View { get => DataContext as WebLoginView; }

        public WebLoginWnd()
        {
            InitializeComponent();
        }

        private void RevealedPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (View.IsPasswordRevealed)
                View.PasswordText = PasswordBox.Password;
            else
                PasswordBox.Password = View.PasswordText;
        }

        private void AccediBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!View.IsPasswordRevealed)
                View.PasswordText = PasswordBox.Password;
            
            View.Accedi();
            DialogResult = true;
        }


    }
}
