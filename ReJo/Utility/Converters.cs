using System;

using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ReJo.UI;


namespace ReJo.Utility
{

    /// <summary>
    /// A type converter for visibility and boolean values. (Visible-Collapsed)
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = (bool)value;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Visible;
        }
    }

    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = !(bool)value;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Collapsed;
        }
    }

    /// <summary>
    /// A type converter for visibility and boolean values. (Visible-Hidden)
    /// </summary>
    public class VisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = (bool)value;
            return visibility ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Visible;
        }
    }

    public class InverseVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visibility = !(bool)value;
            return visibility ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Hidden;
        }
    }


    public class InverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }



    //public class StaticResourceConverter : IValueConverter
    //{

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        //converto il key della risorsa statica nella risorsa
    //        string staticResourceKey = (string)value;
    //        return Application.Current.Resources[staticResourceKey];
    //    }


    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class GlyphConverter : IValueConverter
    {
        public string Glyph1 { get; set; }
        public string Glyph2 { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isStato1 = value as bool?;
            if (isStato1.HasValue && isStato1.Value)
            {
                return Glyph1;
            }
            else
            {
                return Glyph2;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isStato1 = value as string;
            if (isStato1 == Glyph1)
                return true;
            else
                return false;

        }


    }









}



