using Commons;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Commons
{
    public static class Extensions
    {
	
        /// <summary>
        /// Extended version of the string.Contains() method, 
        /// accepting a [StringComparison] object to perform different kind of comparisons
        /// </summary>
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }


        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(this FileInfo source)
        {
            if (source.Exists)
                return GetBytesReadable(source.Length);
            else
                return string.Empty;

        }

        public static string GetBytesReadable(long lenght)
        {
            long i = lenght;
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        public static string GetFirstLine(this string source)
        {
            string line1 = source.Split(new[] { '\r', '\n' }).FirstOrDefault();
            return line1;
        }


        public static Dictionary<TValue, TKey> ReverseKeyValue<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            var dictionary = new Dictionary<TValue, TKey>();
            foreach (var entry in source)
            {
                if (!dictionary.ContainsKey(entry.Value))
                    dictionary.Add(entry.Value, entry.Key);
            }
            return dictionary;
        }

        ///// <summary>
        ///// Case insensitive version of String.Replace().
        ///// </summary>
        ///// <param name="s">String that contains patterns to replace</param>
        ///// <param name="oldValue">Pattern to find</param>
        ///// <param name="newValue">New pattern to replaces old</param>
        ///// <param name="comparisonType">String comparison type</param>
        ///// <returns></returns>
        //public static string Replace(this string s, string oldValue, string newValue,
        //    StringComparison comparisonType)
        //{
        //    if (s == null)
        //        return null;

        //    if (String.IsNullOrEmpty(oldValue))
        //        return s;

        //    StringBuilder result = new StringBuilder(Math.Min(4096, s.Length));
        //    int pos = 0;

        //    while (true)
        //    {
        //        int i = s.IndexOf(oldValue, pos, comparisonType);
        //        if (i < 0)
        //            break;

        //        result.Append(s, pos, i - pos);
        //        result.Append(newValue);

        //        pos = i + oldValue.Length;
        //    }
        //    result.Append(s, pos, s.Length - pos);

        //    return result.ToString();
        //}
    }




}
