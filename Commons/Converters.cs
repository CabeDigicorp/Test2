using System;

using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.Grid;
using System.Windows.Media;
using System.Collections;
using System.ComponentModel;
using Syncfusion.Data.Extensions;
using System.Windows.Controls;

namespace Commons
{

    //public enum ListViewExSelectionMode
    //{
    //    None = 0,
    //    Single = 1,
    //    Multiple = 2,
    //    Extended = 3
    //}



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
            return (visibility == Visibility.Visible);
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
            return (visibility == Visibility.Collapsed);
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
            return (visibility == Visibility.Visible);
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
            return (visibility == Visibility.Hidden);
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

    //    /// <summary>
    //    /// A type converter for ListViewSelectionMode and ListViewExSelectionMode.
    //    /// </summary>
    //    public class ListViewSelectionModeConverter : IValueConverter
    //    {
    //        //public object Convert(object value, Type targetType, object parameter, string language)
    //        //{
    //        //    ListViewExSelectionMode selectionModeEx = (ListViewExSelectionMode)value;
    //        //    switch (selectionModeEx)
    //        //    {
    //        //        case ListViewExSelectionMode.Extended:
    //        //            return ListViewSelectionMode.Extended;
    //        //        case ListViewExSelectionMode.Multiple:
    //        //            return ListViewSelectionMode.Multiple;
    //        //        case ListViewExSelectionMode.None:
    //        //            return ListViewSelectionMode.None;
    //        //        case ListViewExSelectionMode.Single:
    //        //            return ListViewSelectionMode.Single;
    //        //    }
    //        //    return ListViewSelectionMode.None;
    //        //}

    //        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //        {
    //            ListViewExSelectionMode selectionModeEx = (ListViewExSelectionMode)value;
    //            switch (selectionModeEx)
    //            {
    //                case ListViewExSelectionMode.Extended:
    //                    return ListViewSelectionMode.Extended;
    //                case ListViewExSelectionMode.Multiple:
    //                    return ListViewSelectionMode.Multiple;
    //                case ListViewExSelectionMode.None:
    //                    return ListViewSelectionMode.None;
    //                case ListViewExSelectionMode.Single:
    //                    return ListViewSelectionMode.Single;
    //            }
    //            return ListViewSelectionMode.None;
    //        }

    //        //public object ConvertBack(object value, Type targetType, object parameter, string language)
    //        //{
    //        //    ListViewSelectionMode selectionMode = (ListViewSelectionMode)value;
    //        //    switch (selectionMode)
    //        //    {
    //        //        case ListViewSelectionMode.Extended:
    //        //            return ListViewExSelectionMode.Extended;
    //        //        case ListViewSelectionMode.Multiple:
    //        //            return ListViewExSelectionMode.Multiple;
    //        //        case ListViewSelectionMode.None:
    //        //            return ListViewExSelectionMode.None;
    //        //        case ListViewSelectionMode.Single:
    //        //            return ListViewExSelectionMode.Single;
    //        //    }
    //        //    return ListViewExSelectionMode.None;
    //        //}

    //        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //        {
    //            ListViewSelectionMode selectionMode = (ListViewSelectionMode)value;
    //            switch (selectionMode)
    //            {
    //                case ListViewSelectionMode.Extended:
    //                    return ListViewExSelectionMode.Extended;
    //                case ListViewSelectionMode.Multiple:
    //                    return ListViewExSelectionMode.Multiple;
    //                case ListViewSelectionMode.None:
    //                    return ListViewExSelectionMode.None;
    //                case ListViewSelectionMode.Single:
    //                    return ListViewExSelectionMode.Single;
    //            }
    //            return ListViewExSelectionMode.None;
    //        }
    //    }

    public class StaticResourceConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, string language)
        //{
        //    //converto il key della risorsa statica nella risorsa
        //    string staticResourceKey = (string)value;
        //    return Application.Current.Resources[staticResourceKey];

        //}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //converto il key della risorsa statica nella risorsa
            string staticResourceKey = (string)value;
            return Application.Current.Resources[staticResourceKey];
        }

        //public object ConvertBack(object value, Type targetType, object parameter, string language)
        //{
        //    throw new NotImplementedException();
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //    public class AutoSuggestQueryParameterConverter : IValueConverter
    //    {
    //        public object Convert(object value, Type targetType, object parameter, string language)
    //        {
    //            // cast value to whatever EventArgs class you are expecting here
    //            var args = (AutoSuggestBoxQuerySubmittedEventArgs)value;
    //            // return what you need from the args
    //            return (string)args.ChosenSuggestion;
    //        }
    //        public object ConvertBack(object value, Type targetType, object parameter, string language)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //    public class AutoSuggestBoxSuggestionChosenParameterConverter : IValueConverter
    //    {
    //        public object Convert(object value, Type targetType, object parameter, string language)
    //        {
    //            // cast value to whatever EventArgs class you are expecting here
    //            var args = (AutoSuggestBoxSuggestionChosenEventArgs)value;
    //            // return what you need from the args
    //            return (string)args.SelectedItem;
    //        }
    //        public object ConvertBack(object value, Type targetType, object parameter, string language)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public class AutoSuggestBoxTextChangedCommandParameterConverter : IValueConverter
    //    {
    //        public object Convert(object value, Type targetType, object parameter, string language)
    //        {
    //            // cast value to whatever EventArgs class you are expecting here
    //            var args = (AutoSuggestBoxTextChangedEventArgs)value;
    //            // return what you need from the args
    //            return (bool) (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput);
    //        }
    //        public object ConvertBack(object value, Type targetType, object parameter, string language)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }


    /// <summary>
    /// Indentazione di TreeEntityView
    /// </summary>
    public class IntegerToIndentationConverter : IValueConverter
    {
        int indentMultiplier = 20;

        //public object Convert(object value, Type targetType, object parameter, string language)
        //{
        //    Thickness indent = new Thickness(0);
        //    if (value != null)
        //    {
        //        indent.Left = (int)value * indentMultiplier;
        //        return indent;
        //    }

        //    return indent;
        //}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness indent = new Thickness(0);
            if (value != null)
            {
                indent.Left = (int)value * indentMultiplier;
                return indent;
            }

            return indent;
        }

        //public object ConvertBack(object value, Type targetType, object parameter, string language)
        //{
        //    throw new NotImplementedException();
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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

            //if (isStato1.HasValue && isStato1.Value)
            //{
            //    return Glyph1;
            //}
            //else
            //{
            //    return Glyph2;
            //}
            //return null;
        }
    }


    public class DataGridGroupColumnDescription
    {
        public string Name { get; set; }
    }

    public class DataGridGroupColumnDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<DataGridGroupColumnDescription> groupColumnDescs = value as ObservableCollection<DataGridGroupColumnDescription>;
            if (value != null)
            {
                ObservableCollection<GroupColumnDescription> sfGroupColumnDescs = new ObservableCollection<GroupColumnDescription>();
                foreach (DataGridGroupColumnDescription item in groupColumnDescs)
                {
                    sfGroupColumnDescs.Add(new GroupColumnDescription() { ColumnName = item.Name });
                }
                return sfGroupColumnDescs;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Obsoleta
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        public enum ColorsEnum
        {
            ActiveCaptionBrush = 0,
            Black,
            DesktopBrush,
            DimGray,
            GradientActiveCaptionBrush,
            HighlightBrush,
            EntitySelectionColor,
            Red,
            Transparent,
            ReadOnlyBackgroundColor,
            WhiteSmoke,
            White,
            Gray,
            LightGray,
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;

            ColorsEnum state = (ColorsEnum)value;


            switch (state)
            {
                case ColorsEnum.EntitySelectionColor:// "LightGray":
                    return Application.Current.Resources["EntitySelectionColor"];
                    //return new SolidColorBrush(Colors.LightGray);
                case ColorsEnum.DesktopBrush:// "DesktopBrush":
                    return SystemColors.DesktopBrush;
                case ColorsEnum.HighlightBrush:// "HighlightBrush":
                    return SystemColors.HighlightBrush;
                case ColorsEnum.GradientActiveCaptionBrush:// "GradientActiveCaptionBrush":
                    return SystemColors.GradientActiveCaptionBrush;
                case ColorsEnum.ActiveCaptionBrush:// "ActiveCaptionBrush":
                    return SystemColors.ActiveCaptionBrush;
                case ColorsEnum.Red:// "Red":
                    return new SolidColorBrush(Colors.Red);
                case ColorsEnum.DimGray:
                    return new SolidColorBrush(Colors.DimGray);
                case ColorsEnum.Black:
                    return new SolidColorBrush(Colors.Black);
                case ColorsEnum.Gray:
                    return new SolidColorBrush(Colors.Gray);
                case ColorsEnum.LightGray:
                    return new SolidColorBrush(Colors.LightGray);
                case ColorsEnum.White:
                    return new SolidColorBrush(Colors.White);
                case ColorsEnum.Transparent:
                    return "Transparent";
                case ColorsEnum WhiteSmoke:
                    return new SolidColorBrush(Colors.WhiteSmoke);
                default:
                    return "Transparent";

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
            
    }



    public class DisplayConverter : IValueConverter
    {
        GridColumn cachedColumn;

        public DisplayConverter()
        {

        }

        public DisplayConverter(GridColumn column)
        {
            cachedColumn = column;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var selectedItems = value as IEnumerable;
            var displayMemberPath = string.Empty;

            var column = cachedColumn as GridComboBoxColumn;
            displayMemberPath = column.DisplayMemberPath;

            if (selectedItems == null)
                return null;

            string selectedItem = string.Empty;
            PropertyDescriptorCollection pdc = null;
            var enumerator = selectedItems.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var type = enumerator.Current.GetType();

                pdc = pdc ?? TypeDescriptor.GetProperties(type);

                if (!string.IsNullOrEmpty(displayMemberPath))
                    selectedItem += pdc.GetValue(enumerator.Current, displayMemberPath) + " - ";
            }
            //return selectedItem.Substring(0, selectedItem.Length - 2);
            return selectedItem.Substring(0, selectedItem.Length);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class MediaStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPlay = (bool)value;
            return isPlay ? MediaState.Play : MediaState.Close;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MediaState mediaState = (MediaState)value;
            return (mediaState == MediaState.Play);
        }

    }




}



