using MasterDetailModel;
using StampeWpf.Wizard;
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
using CommonResources.Controls;
using Microsoft.Xaml.Behaviors;
using CommonResources;
using System.Collections.ObjectModel;

namespace StampeWpf
{
    /// <summary>
    /// Interaction logic for TextBoxItemCtrl.xaml
    /// </summary>
    public partial class TextBoxItemCtrl : UserControl
    {
        public TextBoxItemView View { get => DataContext as TextBoxItemView; }
        string PlaceHoder = StampeKeys.LocalizeEtichettaWizard;
        public TextBoxItemCtrl()
        {
            InitializeComponent();
            this.PreviewKeyDown += ComboBoxCtrl_PreviewKeyDown;
            Etichetta.TextChanged += Etichetta_TextChanged;
            Etichetta.GotFocus += Etichetta_GotFocus;
            Etichetta.LostFocus += Etichetta_LostFocus; 
        }
        private void Etichetta_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Etichetta.Text))
            {
                Etichetta.Text = PlaceHoder;
                Etichetta.Foreground = Brushes.Gray;
            }
        }

        private void Etichetta_GotFocus(object sender, RoutedEventArgs e)
        {
            View.ClosePopUp(true);
        }

        private void SelectText(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        private void Etichetta_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Etichetta.Text == PlaceHoder)
            { Etichetta.Foreground = Brushes.Gray; }
            else
            { Etichetta.Foreground = Brushes.Black; }
        }

        private void ComboBoxCtrl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { }
            View.PopUpIsOpen = false;
        }

    }
}
