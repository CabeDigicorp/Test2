using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Commons
{
    public class TextBoxValore : TextBox
    {
        public TextBoxValore()
        {
            //SizeChanged += TextBoxValore_SizeChanged;
            //TextChanged += TextBoxValore_TextChanged;
        }

        //private void TextBoxValore_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    UpdateView();
        //}

        //private void TextBoxValore_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        //{
        //    UpdateView();
        //}

        //private void UpdateView()
        //{
        //    if (ActualHeight < ExtentHeight)
        //    {
        //        //IsFullTextVisible = false;
        //        //Foreground = Brushes.Red;
        //        BorderThickness = new Thickness(0, 0, 1, 0);
        //        BorderBrush = Brushes.Orange;
        //    }
        //    else
        //    {
        //        //IsFullTextVisible = true;
        //        //Foreground = Brushes.Black;
        //        BorderThickness = new Thickness(0, 0, 0, 1);
        //        BorderBrush = Brushes.Gray;


        //    }
        //}

        /// <summary>
        /// Scopo: aggiornamento della sorgente quando si preme Enter sulla tastiera
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter)
                GetBindingExpression(TextBox.TextProperty).UpdateSource();

        }

        ///// <summary>
        ///// Registers a dependency property as backing store for the Content property
        ///// </summary>
        //public static readonly DependencyProperty IsFullTextVisibleProperty =
        //    DependencyProperty.Register("IsFullTextVisible", typeof(bool), typeof(TextBoxValore),
        //    new FrameworkPropertyMetadata(false,
        //          FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        ///// <summary>
        ///// Gets or sets the Content.
        ///// </summary>
        ///// <value>The Content.</value>
        //public bool IsFullTextVisible
        //{
        //    get { return (bool)GetValue(IsFullTextVisibleProperty); }
        //    set { SetValue(IsFullTextVisibleProperty, value); }
        //}



    }
}
