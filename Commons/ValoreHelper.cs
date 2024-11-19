

using Net.Sgoliver.NRtfTree.Core;
using Net.Sgoliver.NRtfTree.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Commons
{

    public static class ValoreHelper
    {
        static RichTextHelper _formattedTextHelper = new RtfHelperDevExpress();//devexpress_test


        public static string ValoreNullAsText { get => "[     Ø     ]"; }//"Ø null"
        public static string ValoreAsItem { get => "_AsItem"; }//LocalizationProvider.GetString("_AsItem")
        public static string ItselfResult { get => "att{{{0}}}"; }//{ get => "att{}"; }
        public static string ItselfFormula { get => "attf{{{0}}}"; }
        public static string ZeroRealResult { get => "_ZeroRealResult"; }
        public static string Multi { get => "_Multi"; }

        public static RichTextHelper FormattedTextHelper { get => _formattedTextHelper; }



        /// <summary>
        /// Ricerca y in xyz
        /// </summary>
        /// <param name="xyz">Testo contentente</param>
        /// <param name="y">Testo contenuto</param>
        /// <returns></returns>
        static public bool ContainsTesto(string xyz, string y)
        {
            if (xyz == null)
                return false;

            string XYZ = xyz.ToUpper();

            if (y == null)
                return true;

            string Y = y.ToUpper();

            if (XYZ.Contains(Y))
                return true;

            //ricerca le substringhe virgolettate
            String pattern = @"\""([^\[\]]+)\""";
            foreach (Match m in Regex.Matches(Y, pattern))
            {
                if (XYZ.Contains(m.Groups[1].Value))
                    return true;
            }

            //ricerca ogni stringa divisa da spazio
            string[] str = Y.Split(' ');
            foreach (string s in str)
            {
                if (!XYZ.Contains(s))
                    return false;
            }

            return true;
        }

        static public void ReplaceInRtfText(ref string rtf, string oldStr, string newStr)
        {
            _formattedTextHelper.ReplaceInText(ref rtf, oldStr, newStr);
        }

        static public void RtfFromPlainString(string str, out string rtf)
        {
            _formattedTextHelper.ConvertFromPlainString(str, out rtf);
            
        }

        static public void RtfToPlainString(string rtf, out string str)
        {
            _formattedTextHelper.ConvertToPlainString(rtf, out str);
        }

        public static void HighlightText(ref string rtf, string highlightedText)
        {
            _formattedTextHelper.HighlightText(ref rtf, highlightedText);
        }

        static public bool RtfIsEmpty(string rtf)
        {
            return _formattedTextHelper.IsEmpty(rtf);
        }

        public static void RtfConcat(ref string rtf, string rtfToAppend)
        {
            _formattedTextHelper.Concat(ref rtf, rtfToAppend);
        }

        //public static List<string> RtfSplit(string rtf)
        //{
        //    return _formattedTextHelper.Split(rtf);
        //}

        public static void RtfUpdateStyles(ref string rtf, List<StyleInfo> styles/*, RtfMailMergeDataService rtfDataService*/)
        {
            _formattedTextHelper.UpdateStyles(ref rtf, styles/*, rtfDataService*/);
        }

        public static string RtfMailMerge(string rtf, RtfFieldsDataService mailMergeDataService)
        {
            return _formattedTextHelper.RtfMailMerge(rtf, mailMergeDataService);
        }

        public static string AdjustRtf(string rtf)
        {
            return _formattedTextHelper.AdjustRtf(rtf);
        }


    }

}