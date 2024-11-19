using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public interface RichTextHelper
    {
        void ReplaceInText(ref string richText, string oldStr, string newStr);

        void ConvertFromPlainString(string str, out string richText);

        void ConvertToPlainString(string richText, out string str);

        void HighlightText(ref string richText, string highlightedText);

        void Concat(ref string richText, string rtfToAppend);

        void UpdateStyles(ref string richText, List<StyleInfo> styles/*, RtfMailMergeDataService rtfDataService*/);
        
        bool IsEmpty(string rtf);

        string RtfMailMerge(string rtf, RtfFieldsDataService mailMergeDataService);

        void GetBriefRtf(string rtf, out string briefRtf);

        string AdjustRtf(string rtf);
    }

    public class StyleInfo
    {
        
        public string Key { get; set; }
        //public string DisplayName { get; set; }
        public double FontSize { get; set; }
        public bool IsBold { get; set; }
        public string ForeColorHex { get; set; }
        public string FontFamilyName { get; set; }
        public bool IsUnderline { get; set; }
        public string BackColorHex { get; set; }
        public bool IsStrikethrough { get; set; }
        public bool IsItalic { get; set; }
        public TextAlignmentEnum TextAlignment { get; set; }
    }

    public enum TextAlignmentEnum
    {
        Nothing = 0,
        Left,
        Center,
        Right,
        Justify,
    }







}
