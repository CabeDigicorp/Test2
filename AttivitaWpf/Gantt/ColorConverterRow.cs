using AttivitaWpf.View;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AttivitaWpf
{
    public class ColorConverterRow : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SALProgrammatoView)
            {
                var input = true;
                //(value as SALProgrammatoView).IsUserInsert;

                //custom condition is checked based on data.

                if (input)
                    return new SolidColorBrush(Colors.AliceBlue);

                else
                    return new SolidColorBrush(Colors.White);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
