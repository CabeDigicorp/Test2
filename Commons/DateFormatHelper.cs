using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class DateFormatHelper
    {
        static public string DefaultFormat { get => "#,##0.00"; }

        public static string GetFormattedExample(string format, DateTime? DateValue = null)
        {
            DateTime exampleDate = DateTime.Now;
            if (DateValue.HasValue)
                exampleDate = DateValue.Value;
           

            if (format == null || !format.Any())
                return string.Empty;

            string fullFormat = exampleDate.ToString(format);
            string formattedExampleDate = string.Format(fullFormat, exampleDate);

            return formattedExampleDate;
        }

        public static void UpdateCulture()
        {

        }

        public static string ConvertToUserFormat(string format)
        {
            return format;
        }
    }
}
