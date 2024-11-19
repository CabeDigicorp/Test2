

using DevExpress.Utils.Url;
using DevExpress.Xpf.RichEdit;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit.API.Native.Implementation;
using DevExpress.XtraRichEdit.Export;
using DevExpress.XtraRichEdit.Export.OpenDocument;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace Commons
{

    public class RtfHelperDevExpress : RichTextHelper
    {
        //RichEditControl _rtfEditCtrl = new RichEditControl();
        RichEditDocumentServer _documentServer = null;
        RichEditDocumentServer DocumentServer { get => _documentServer; }

        public RtfHelperDevExpress()
        {
            _documentServer = new RichEditDocumentServer();
            _documentServer.CalculateDocumentVariable += _documentServer_CalculateDocumentVariable;

            _documentServer.BeginUpdate();
            _documentServer.EndUpdate();
        }

        //public static string RtfDefault_old { get; set; } = @"{\rtf\ansi\ansicpg1252\uc1\deff0\deflang1033{\fonttbl{\f0 Segoe UI;}{\f1 Verdana;}}{\colortbl\red0\green0\blue0;}\nouicompat\viewkind4\paperw12240\paperh15840\margl1425\margr1425\margt1425\margb1425\deftab720\sectd\pgwsxn12240\pghsxn15840\marglsxn1425\margrsxn1425\margtsxn1425\margbsxn1425\headery720\footery720\pard\sl0\slmult1\qj\ltrpar{\ltrch\f0\fs18\i0\b0\strike0\cf0\ulc0\ulnone\par}}";
        public static string RtfDefault { get; set; } = @"{\rtf\deff0\stshfdbch3\stshfloch2\stshfhich2\stshfbi1{\fonttbl{\f0 Segoe UI;}}{\stylesheet{\qj\fcs1\f4\fcs1\f4\hich\af4\loch\af4\dbch\af3\fs18\cf1 Normal;}}\nouicompat\htmautsp\sectd\marglsxn0\margrsxn0\margtsxn0\margbsxn0\headery720\footery720\pgwsxn16839\pghsxn23814\psz8\pard\plain\qj\fcs1\af4\ltrch\fcs0\hich\af4\dbch\af3\loch\f4\fs18\cf1\par}";
        public void ReplaceInText(ref string rtf, string oldStr, string newStr)
        {

            DocumentServer.RtfText = rtf;
            DocumentServer.Document.ReplaceAll(oldStr, newStr, SearchOptions.CaseSensitive);
            rtf = DocumentServer.RtfText;

        }

        public void ConvertFromPlainString(string str, out string rtf)
        {
            //oss: bisogna per questione di tempo: quando carico un rtf da file viene comunque creato vuoto per poi essere sovrascritto
            if (str == null || !str.Any())
            {
                rtf = RtfDefault;
                return;
            }


            //Net.Sgoliver.NRtfTree.Util.RtfDocument doc = new Net.Sgoliver.NRtfTree.Util.RtfDocument();
            //doc.Tree.LoadRtfText(RtfDefault);
            //doc.AddText(str);
            //rtf = doc.Rtf;


            DocumentServer.RtfText = RtfDefault;
            DocumentServer.Document.AppendText(str);

            RtfDocumentExporterOptions options = new RtfDocumentExporterOptions();
            options.ExportTheme = false;

            rtf = DocumentServer.Document.GetRtfText(DocumentServer.Document.Range, options);
            //rtf = DocumentServer.RtfText;
            
        }


        public void ConvertToPlainString(string rtf, out string str)
        {
            str = string.Empty;

            Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree = new Net.Sgoliver.NRtfTree.Core.RtfTree();
            rtfTree.LoadRtfText(rtf);
            str = rtfTree.Text.Trim();

            
            ///oss: è molto lenta
            //try
            //{
            //    _documentServer = new RichEditDocumentServer();
            //    DocumentServer.RtfText = rtf;
            //    str = DocumentServer.Text;
            //}
            //catch (Exception exc)
            //{
            //    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            //}

        }


        public void HighlightText(ref string rtf, string highlightedText)
        {


            DocumentServer.RtfText = rtf;


            //reset
            DocumentRange range = DocumentServer.Document.CreateRange(DocumentServer.Document.Range.Start.ToInt(), DocumentServer.Document.Range.End.ToInt());
            CharacterProperties cp = DocumentServer.Document.BeginUpdateCharacters(range);
            cp.Reset(CharacterPropertiesMask.BackColor);
            DocumentServer.Document.EndUpdateCharacters(cp);


            List<string> highlightedTexts = new List<string>();

            string hTextTrimmed = highlightedText.Trim();

            if (hTextTrimmed.StartsWith("\"") && hTextTrimmed.EndsWith("\"") && hTextTrimmed.Count() > 2)
                highlightedTexts.Add(hTextTrimmed.Substring(1, hTextTrimmed.Count() - 2));
            else
                highlightedTexts = hTextTrimmed.Split(' ').ToList();


            ////string[] highlightedTexts = highlightedText.Split(' ');
            foreach (string hiTxt in highlightedTexts)
            {
                var ranges = DocumentServer.Document.FindAll(Regex.Escape(hiTxt), SearchOptions.None);

                foreach (var find in ranges)
                {
                    var oDoc = find.BeginUpdateDocument();
                    var oChars = oDoc.BeginUpdateCharacters(find);
                    oChars.BackColor = System.Drawing.Color.Yellow;
                    oDoc.EndUpdateCharacters(oChars);
                    find.EndUpdateDocument(oDoc);
                }

            }


            rtf = DocumentServer.RtfText;

        }

        public void Concat(ref string rtf, string rtfToAppend)
        {
            if (!rtf.Any())
            {
                rtf = rtfToAppend;
                return;
            }

            DocumentServer.RtfText = rtfToAppend;
            DocumentServer.Document.AppendText("\n");
            DocumentServer.Document.AppendRtfText(rtf);
                
            rtf = DocumentServer.RtfText;

        }

        private static void UpdateStyle(StyleInfo styleInfo, ParagraphStyle parStyle)
        {
            //span
            CharacterPropertiesBase charStyleProp = parStyle.LinkedStyle;

            if (charStyleProp != null)
            {

                charStyleProp.FontName = styleInfo.FontFamilyName;
                charStyleProp.FontSize = (float)(styleInfo.FontSize);
                charStyleProp.Bold = styleInfo.IsBold;

                if (styleInfo.IsUnderline)
                    charStyleProp.Underline = UnderlineType.Single;
                else
                    charStyleProp.Underline = UnderlineType.None;

                if (styleInfo.IsStrikethrough)
                    charStyleProp.Strikeout = StrikeoutType.Single;
                else
                    charStyleProp.Strikeout = StrikeoutType.None;

                if (styleInfo.ForeColorHex != null && styleInfo.ForeColorHex.Any())
                    charStyleProp.ForeColor = ColorTranslator.FromHtml(styleInfo.ForeColorHex);


                charStyleProp.Italic = styleInfo.IsItalic;

            }

            //////////////////////////////////////////////////////////////////
            ////paragraph

            ParagraphPropertiesBase parStyleProp = parStyle;

            if (parStyleProp != null)
            {

                if (styleInfo.BackColorHex != null && styleInfo.BackColorHex.Any())
                {
                    parStyleProp.BackColor = ColorTranslator.FromHtml(styleInfo.BackColorHex);
                }

                switch (styleInfo.TextAlignment)
                {
                    case TextAlignmentEnum.Left:
                        parStyleProp.Alignment = ParagraphAlignment.Left;
                        break;
                    case TextAlignmentEnum.Center:
                        parStyleProp.Alignment = ParagraphAlignment.Center;
                        break;
                    case TextAlignmentEnum.Right:
                        parStyleProp.Alignment = ParagraphAlignment.Right;
                        break;
                    case TextAlignmentEnum.Justify:
                        parStyleProp.Alignment = ParagraphAlignment.Justify;
                        break;
                }

                parStyleProp.SpacingBefore = 0;
                parStyleProp.SpacingAfter = 0;
                parStyleProp.LineSpacing = 0;

            }
        }

        public void UpdateStyles(Document document, List<StyleInfo> styles)
        {

            foreach (StyleInfo styleInfo in styles)
            {
                string styleKey = styleInfo.Key;

                ParagraphStyle parStyle = null;
                CharacterStyle charStyle = null;


                string parStyleName = string.Format("{0}", styleKey);
                string charStyleName = string.Format("{0}Char", styleKey);

                ////////////////////////////
                ///paragraph

                if (styleKey != null && styleKey.Any())
                {
                    parStyle = document.ParagraphStyles.FirstOrDefault(item => item.Name == styleInfo.Key);
                }

                //aggiungo lo stile
                if (parStyle == null)
                {
                    //paragraph
                    string styleName = string.Empty;
                    styleName = string.Format("{0}", styleKey);
                    parStyle = document.ParagraphStyles.CreateNew();
  
                    if (styleInfo.Key == "Normal")
                        parStyle.Primary = true;

                    parStyle.Name = styleName;
                    switch (styleInfo.TextAlignment)
                    {
                        case TextAlignmentEnum.Left:
                            parStyle.Alignment = ParagraphAlignment.Left;
                            break;
                        case TextAlignmentEnum.Right:
                            parStyle.Alignment = ParagraphAlignment.Right;
                            break;
                        case TextAlignmentEnum.Center:
                            parStyle.Alignment = ParagraphAlignment.Center;
                            break;
                        case TextAlignmentEnum.Justify:
                            parStyle.Alignment = ParagraphAlignment.Justify;
                            break;
                        case TextAlignmentEnum.Nothing: break;
                    }
                }



                //////////////////////
                /////Char

                if (styleKey != null && styleKey.Any())
                {
                    charStyle = document.CharacterStyles.FirstOrDefault(item => item.Name == styleInfo.Key);
                }

                if (charStyle == null)
                { 
                    //char
                    charStyle = document.CharacterStyles.CreateNew();

                    charStyle.Name = charStyleName;
                    if (styleInfo.Key == "Normal")
                        charStyle.Primary = true;

                    //link paragraph
                    parStyle.LinkedStyle = charStyle;
                    charStyle.LinkedStyle = parStyle;

                    document.ParagraphStyles.Add(parStyle);
                    document.CharacterStyles.Add(charStyle);

                }


                UpdateStyle(styleInfo, parStyle);

            }

        }


        public void UpdateStyles(ref string rtf, List<StyleInfo> styles/*, RtfMailMergeDataService rtfDataService*/)
        {

            var doc = DocumentServer;
            doc.RtfText = rtf;

            UpdateStyles(doc.Document, styles);

            rtf = doc.RtfText;

        }


        public bool IsEmpty(string rtf)
        {
            DocumentServer.RtfText = rtf;
            return DocumentServer.Document.IsEmpty;
        }

        public string RtfMailMerge(string rtf, RtfFieldsDataService mailMergeDataService)
        {

            _documentServer.Options.MailMerge.DataSource = mailMergeDataService;

            _documentServer.RtfText = rtf;
            _documentServer.Document.UnlinkAllFields();

            rtf = _documentServer.RtfText;
            return rtf;
        }

        private void _documentServer_CalculateDocumentVariable(object sender, CalculateDocumentVariableEventArgs e)
        {
            string varName = e.VariableName;

            RtfFieldsDataService rtfDataService = _documentServer.Options.MailMerge.DataSource as RtfFieldsDataService;
            if (rtfDataService == null)
                return;

            ResultType resType = ResultType.PlainText;
            string result = rtfDataService.GetResult(varName, out resType);

            if (resType == ResultType.PlainText)
            {
                e.Value = result;
                e.Handled = true;
            }
            else if (resType == ResultType.RtfText)
            {
                var doc = new RichEditDocumentServer();
                doc.RtfText = result;

                e.Value = doc;
                e.Handled = true;
            }
        }

        public static string CreateFieldDisplayName(string name)
        {
            return string.Format("«{0}»", name);
        }

        public void GetBriefRtf(string rtf, out string briefRtf)
        {

            briefRtf = rtf;


            DocumentServer.RtfText = rtf;
            
            if (DocumentServer.Document.Paragraphs.Count <= 1)
                return;
            

            

            DocumentServer.BeginUpdate();
            {
                var parII = DocumentServer.Document.Paragraphs[2];
                var rangeToDelete = DocumentServer.Document.CreateRange(parII.Range.Start.ToInt(), DocumentServer.Document.Range.End.ToInt());
                DocumentServer.Document.Delete(rangeToDelete);
            }
            DocumentServer.EndUpdate();

            briefRtf = DocumentServer.RtfText;
        }

        public string AdjustRtf(string rtf)
        {
            return rtf;

        }

    }

}