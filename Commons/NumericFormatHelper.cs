using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Commons
{
    public static class NumericFormatHelper
    {
        static string _currentDecimalSeparator = null;
        static string _currentGroupSeparator = null;

        public static string CurrentDecimalSeparator {get => _currentDecimalSeparator;}
        public static string CurrentGroupSeparator { get => _currentGroupSeparator; }

        static string _formatSpecialChars = "[0#.,]";

        static public string DefaultFormat { get => "#,##0.00"; }

        public static void UpdateCulture(bool isCurrency)
        {
            NumericFormatHelper.IsCurrency = isCurrency;

            if (isCurrency)
            {
                _currentDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                _currentGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;

            }
            else
            {
                _currentDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                _currentGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            }
        }

        static bool _isCurrency = false;
        public static bool IsCurrency
        {
            get => _isCurrency;
            set
            {
                if (_isCurrency != value)
                {
                    _isCurrency = value;
                    UpdateCulture(_isCurrency);
                }
            }
        }

        public static bool HasCulture() { return _currentDecimalSeparator != null;}


        /// <summary>
        /// Converte da formato ('.' separatore decimale e ',' separaratore di gruppo) 
        /// a formato utente (_currentDecimalSeparator separatore decimale e _currentGroupSeparator separatore di gruppo)
        /// </summary>
        /// <param name="userFormat"></param>
        /// <returns></returns>
        public static bool ConvertFromUserFormat(string userFormat, out string format, out char excludedChar)
        {
            IsCurrency = IsFormatCurrency(userFormat);

            bool ret = false;
            format = string.Empty;
            excludedChar = char.MinValue;

            if (userFormat != null)
            {
                ret = true;

                string strOut = string.Empty;
                string strIn = userFormat;
                while (strIn.Any())
                {
                    if (strIn.StartsWith(_currentDecimalSeparator))
                        strOut += '.';
                    else if (strIn.StartsWith(_currentGroupSeparator))
                        strOut += ',';
                    else if (strIn[0] == '.' || strIn[0] == ',')
                    { 
                        ret = false;
                        excludedChar = strIn[0];
                    }
                    else
                        strOut += strIn[0];
     
                    strIn = strIn.Substring(1);
                }
                if (ret)
                    format = strOut;
            }
            return ret;
        }

        public static string ConvertToUserFormat(string format)
        {
            

            string userFormat = format;
            if (userFormat != null && _currentDecimalSeparator != null)
            {
                IsCurrency = IsFormatCurrency(format);
                
                string strOut = string.Empty;
                string strIn = format;
                while (strIn.Any())
                {
                    if (strIn[0] == '.')
                        strOut += _currentDecimalSeparator;
                    else if (strIn[0] == ',')
                        strOut += _currentGroupSeparator;
                    else
                        strOut += strIn[0];

                    strIn = strIn.Substring(1);
                }
                userFormat = strOut;
            }

            return userFormat;
        }

        public static string ComposeFormat(NumberFormat numberFormat)
        {

            string str = ".";//separatore decimale
            //.
            //DecimalDigitCount
            string zeros = "0000000000000000000000000";
            str = string.Format("{0}{1}", str, zeros.Substring(0, numberFormat.DecimalDigitCount));
            //.00
            //LeftZeroCount
            for (int i = 0; i < Math.Max(numberFormat.LeftZeroCount, 5); i++)
            {
                if (!numberFormat.UseThousandSeparator && i >= numberFormat.LeftZeroCount)
                    break;

                if (i == 3 && numberFormat.UseThousandSeparator)
                    str = str.Insert(0, ",");//separatore di gruppo
                else if (i < numberFormat.LeftZeroCount)
                    str = str.Insert(0, "0");
                else
                    str = str.Insert(0, "#");

            }
            //#'#00.00

            string symbolSeparator = string.Empty;
            if (numberFormat.IsSymbolSeparated)
                symbolSeparator = " ";

            //IsSymbolAtLeft & SymbolText
            if (numberFormat.IsSymbolAtLeft)
                str = string.Format("{0}{1}{2}", numberFormat.SymbolText, symbolSeparator, str);
            else
                str = string.Format("{0}{1}{2}", str, symbolSeparator, numberFormat.SymbolText);
            //#'#00.00 kg

            return str;
        }

        public static NumberFormat DecomposeFormat(string format)
        {
            NumberFormat numberFormat = new NumberFormat();

            List<string> list = new List<string>();

            string str = format;

            if (!str.Contains("."))//separatore decimale obbligatorio nel formato
                return null;

            list = format.Split('.').ToList();

            string str1 = Regex.Replace(list[0], _formatSpecialChars, string.Empty);
            string str2 = Regex.Replace(list[1], _formatSpecialChars, string.Empty);

            string str1TrimmedEnd = str1.TrimEnd();
            string str2TrimmedStart = str2.TrimStart();

            string str1Trimmed = str1.Trim();
            string str2Trimmed = str2.Trim();

            if (str1Trimmed.Any())
            {
                numberFormat.IsSymbolAtLeft = true;
                numberFormat.SymbolText = str1Trimmed;
                numberFormat.IsSymbolSeparated = str1TrimmedEnd.Length < str1.Length;
            }
            else
            {
                numberFormat.IsSymbolAtLeft = false;
                numberFormat.SymbolText = str2Trimmed;
                numberFormat.IsSymbolSeparated = str2TrimmedStart.Length < str2.Length;
            }

            numberFormat.LeftZeroCount = list[0].Count(item => item == '0');
            numberFormat.DecimalDigitCount = list[1].Count(item => item == '0');

            if (str.Contains(",")) //separatore di gruppo
                numberFormat.UseThousandSeparator = true;
            else
                numberFormat.UseThousandSeparator = false;

            return numberFormat;
        }

        public static string GetFormattedExample(string format)
        {
            double exampleNumber = 1234567.056;

            if (format == null || !format.Any())
                return string.Empty;

            string fullFormat = string.Format("{0}0:{1}{2}", "{", format, "}");
            string formattedExampleNum = string.Format(fullFormat, exampleNumber);

            //string str = string.Format("{0}: {1}", LocalizationProvider.GetString("Esempio"), formattedExampleNum);
            return formattedExampleNum;
        }

        public static bool IsFormatCurrency(string format)
        {
            if (format == null)
                return false;

            string currencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            if (format.Contains(currencySymbol))
                return true;

            return false;
        }

        public static string GetPaddedFormat(string format)
        {
            if (!format.StartsWith("0:"))
                return string.Format("{{0:{0}}}", format);
            else
                return format;//è già padded format
        }
    }

    public class NumberFormat
    {
        public bool IsSymbolAtLeft { get; set; }
        public bool IsSymbolSeparated { get; set; }
        public bool UseThousandSeparator { get; set; }
        public string SymbolText { get; set; }
        public int LeftZeroCount { get; set; }
        public int DecimalDigitCount { get; set; }

    }
}
