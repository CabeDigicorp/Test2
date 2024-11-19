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

namespace Commons
{
    /// <summary>
    /// Interaction logic for NotificationCtrl.xaml
    /// </summary>
    public partial class NotificationCtrl : UserControl
    {
        public NotificationCtrl()
        {
            InitializeComponent();
        }

        #region Text DP

        /// <summary>
        /// Gets or sets the Format which is displayed next to the field
        /// </summary>
        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identified the Format dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
              typeof(NotificationCtrl), new PropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            NotificationCtrl uc = d as NotificationCtrl;
            uc.OnTextChanged(e);
        }

        private void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TextBlock.Text = e.NewValue.ToString();
        }

        #endregion
    }
}
