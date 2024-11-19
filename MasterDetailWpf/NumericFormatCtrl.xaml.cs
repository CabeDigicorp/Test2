
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterDetailWpf
{
    /// <summary>
    /// Interaction logic for FormatoNumeroCtrl.xaml
    /// </summary>
    public partial class NumericFormatCtrl : UserControl
    {
        public NumericFormatView View { get => DataContext as NumericFormatView; }

        bool _isSelectionChanging = false;

        

        public NumericFormatCtrl()
        {
            InitializeComponent();
            FormatTextBox.TextChanged += FormatTextBox_TextChanged;

            Loaded += FormatNumeroCtrl_Loaded;
            View.SelectionChanged += FormatNumeroView_SelectionChanged;
            DataContextChanged += NumericFormatCtrl_DataContextChanged;

            if (View != null)
                View.Init();
        }

        private void NumericFormatCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void FormatNumeroCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            //View.IsCurrency = IsCurrency;
            //View.Init();
        }
        
        public bool IsSingleSelection
        {
            get => View.IsSingleSelection;
            set => View.IsSingleSelection = value;
        }

        private void FormatTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Format = UserFormatTextBox.Text;
            //Format = FormatNumeroView.CurrentFormat;
        }

        #region Format DP

        /// <summary>
        /// Formato da salvare
        /// </summary>
        public String Format
        {
            get {return (String)GetValue(FormatProperty);}
            set {SetValue(FormatProperty, value);}
        }

        /// <summary>
        /// Identified the Format dependency property
        /// </summary>
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.Register("Format", typeof(string),
              typeof(NumericFormatCtrl), new PropertyMetadata("", new PropertyChangedCallback(OnFormatChanged)));

        private static void OnFormatChanged(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            NumericFormatCtrl uc = d as NumericFormatCtrl;
            uc.OnFormatChanged(e);
        }

        private void OnFormatChanged(DependencyPropertyChangedEventArgs e)
        {
            //UserFormatTextBox.Text = e.NewValue.ToString();
            View.CurrentFormat = e.NewValue.ToString();
        }

        #endregion

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (View.SelectedNumberFormats.Any())
                Format = View.SelectedNumberFormats.FirstOrDefault().Format;
            //FormatTextBox.Text = FormatNumeroView.UserFormatItems[FormatNumeroView.CurrentFormatIndex];
        }

        #region IsCurrency DP

        /// <summary>
        /// Gets or sets the Format which is displayed next to the field
        /// </summary>
        public bool IsCurrency
        {
            get { return (bool)GetValue(IsCurrencyProperty); }
            set { SetValue(IsCurrencyProperty, value); }
        }

        /// <summary>
        /// Identified the Format dependency property
        /// </summary>
        public static readonly DependencyProperty IsCurrencyProperty =
            DependencyProperty.Register("IsCurrency", typeof(bool),
              typeof(NumericFormatCtrl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsCurrencyChanged)));

        private static void OnIsCurrencyChanged(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            NumericFormatCtrl uc = d as NumericFormatCtrl;
            uc.OnIsCurrencyChanged(e);
        }

        private void OnIsCurrencyChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (_isSelectionChanging)
                return;

            View.SelectedNumberFormats = NumberFormatsList.SelectedItems
                .Cast<NumericFormatItemView>()
                .ToList();

            
        }

        private void FormatNumeroView_SelectionChanged(object sender, EventArgs e)
        {
            _isSelectionChanging = true;

            if (View.IsSingleSelection)
            {
                NumberFormatsList.SelectedItem = View.SelectedNumberFormats.FirstOrDefault();
            }
            else
            { 
                NumberFormatsList.SelectedItems.Clear();
                View.SelectedNumberFormats.ForEach(item => NumberFormatsList.SelectedItems.Add(item));
            }
            
            _isSelectionChanging = false;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
