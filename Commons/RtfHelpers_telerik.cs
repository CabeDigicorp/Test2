

using Net.Sgoliver.NRtfTree.Core;
using Net.Sgoliver.NRtfTree.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents;
using Telerik.Windows.Documents.FormatProviders.OpenXml.Docx;
using Telerik.Windows.Documents.FormatProviders.Rtf;
using Telerik.Windows.Documents.FormatProviders.Txt;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Model.Merging;
using Telerik.Windows.Documents.Model.Styles;
using Telerik.Windows.Documents.TextSearch;
using Telerik.Windows.Documents.UI.TextDecorations.DecorationProviders;
using Paragraph = Telerik.Windows.Documents.Model.Paragraph;
using Section = Telerik.Windows.Documents.Model.Section;

namespace Commons
{

    public class TelerikRtfHelper : RichTextHelper
    {
        TxtFormatProvider _txtProvider = new TxtFormatProvider();
        
        RtfFormatProvider _rtfProvider = new RtfFormatProvider();
        public RtfFormatProvider RtfProvider { get => _rtfProvider; }
        

        public static string TelerikRtfDefault { get; set; } = @"{\rtf\ansi\ansicpg1252\uc1\deff0\deflang1033{\fonttbl{\f0 Segoe UI;}{\f1 Verdana;}}{\colortbl\red0\green0\blue0;}\nouicompat\viewkind4\paperw12240\paperh15840\margl1425\margr1425\margt1425\margb1425\deftab720\sectd\pgwsxn12240\pghsxn15840\marglsxn1425\margrsxn1425\margtsxn1425\margbsxn1425\headery720\footery720\pard\sl0\slmult1\qj\ltrpar{\ltrch\f0\fs18\i0\b0\strike0\cf0\ulc0\ulnone\par}}";

        public void ReplaceInText(ref string rtf, string oldStr, string newStr)
        {
            RadRichTextBox rtb = new RadRichTextBox();
            rtb.Document = _rtfProvider.Import(rtf);

            rtb.Document.Selection.Clear(); // this clears the selection before processing 
            DocumentTextSearch search = new DocumentTextSearch(rtb.Document);
            List<Telerik.Windows.Documents.TextSearch.TextRange> rangesTrackingDocumentChanges = new List<Telerik.Windows.Documents.TextSearch.TextRange>();
            foreach (var textRange in search.FindAll(Regex.Escape(oldStr)))
            {
                Telerik.Windows.Documents.TextSearch.TextRange newRange = new Telerik.Windows.Documents.TextSearch.TextRange(new DocumentPosition(textRange.StartPosition, true), new DocumentPosition(textRange.EndPosition, true));
                rangesTrackingDocumentChanges.Add(newRange);
            }
            foreach (var textRange in rangesTrackingDocumentChanges)
            {
                rtb.Document.Selection.AddSelectionStart(textRange.StartPosition);
                rtb.Document.Selection.AddSelectionEnd(textRange.EndPosition);
                rtb.Insert(newStr);
                textRange.StartPosition.Dispose();
                textRange.EndPosition.Dispose();
            }
            rtf = _rtfProvider.Export(rtb.Document);


        }

        public void ConvertFromPlainString(string str, out string rtf)
        {
            //oss: bisogna per questione di tempo: quando carico un rtf da file viene comunque creato vuoto per poi essere sovrascritto
            if (str == null || !str.Any())
            {
                rtf = TelerikRtfDefault;
                return;
            }

            //RadDocument document = _txtProvider.Import(str);
            //rtf = _rtfProvider.Export(document);

            Net.Sgoliver.NRtfTree.Util.RtfDocument doc = new Net.Sgoliver.NRtfTree.Util.RtfDocument();
            doc.AddText(str);
            rtf = doc.Rtf;

            ////Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree1 = new Net.Sgoliver.NRtfTree.Core.RtfTree();
            ////rtfTree1.LoadRtfText(rtf);

            ////Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree = new Net.Sgoliver.NRtfTree.Core.RtfTree();


            ////string s = @"{\rtf1\ansi\ansicpg1252\deff0\deflang3082{\fonttbl{\f0\fnil Arial;}}{\colortbl\red0\green0\blue0 ;}{\*\generator NRtfTree Library 1.3.0;}\viewkind4\uc1\pard\cf0\fs20\f0\b[TagTextRTF1]\par}";
            ////rtfTree.LoadRtfText(s);

            ////RtfMerger rtfMerger = new RtfMerger(rtfTree);
            ////rtfMerger.AddPlaceHolder("[TagTextRTF1]", rtfTree1);
            ////rtfMerger.Merge();


            ////rtf = rtfTree.Rtf;
        }

        public void ConvertToPlainString(string rtf, out string str)
        {

            //RadDocument document = _rtfProvider.Import(rtf);
            //str = _txtProvider.Export(document);

            Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree = new Net.Sgoliver.NRtfTree.Core.RtfTree();
            rtfTree.LoadRtfText(rtf);
            str = rtfTree.Text.Trim();


            //string rtff = null;
            //ConvertFromPlainString("1\r\n2", out rtff);


        }

        public void HighlightText(ref string rtf, string highlightedText)
        {
            RadRichTextBox rtb = new RadRichTextBox();
            rtb.Document = _rtfProvider.Import(rtf);

            rtb.Document.Selection.Clear(); // this clears the selection before processing 
            DocumentTextSearch search = new DocumentTextSearch(rtb.Document);


            List<string> highlightedTexts = new List<string>();// highlightedText.Split(' ');

            string hTextTrimmed = highlightedText.Trim();

            if (hTextTrimmed.StartsWith("\"") && hTextTrimmed.EndsWith("\"") && hTextTrimmed.Count() > 2)
                highlightedTexts.Add(hTextTrimmed.Substring(1, hTextTrimmed.Count() - 2));
            else
                highlightedTexts = hTextTrimmed.Split(' ').ToList();


            //string[] highlightedTexts = highlightedText.Split(' ');
            foreach (string hiTxt in highlightedTexts)
            {
                foreach (var textRange in search.FindAll(Regex.Escape(hiTxt)))
                {
                    rtb.Document.Selection.AddSelectionStart(textRange.StartPosition);
                    rtb.Document.Selection.AddSelectionEnd(textRange.EndPosition);
                }
            }

            rtb.ChangeTextHighlightColor(Colors.Yellow);  // will highlight 

            rtf = _rtfProvider.Export(rtb.Document);

        }


        //        public void HighlightText(ref string rtf, string highlightedText)
        //        {
        //            Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
        //            rtb.TextFormatter = new Xceed.Wpf.Toolkit.RtfFormatter();
        //            rtb.Text = rtf;



        //            System.Windows.Documents.TextRange text = new System.Windows.Documents.TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

        //            TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
        //            while (current != null)
        //            {
        //                string textInRun = current.GetTextInRun(LogicalDirection.Forward);
        //                if (!string.IsNullOrWhiteSpace(textInRun))
        //                {
        //                    List<string> highlightedTexts = new List<string>();// highlightedText.Split(' ');

        //                    string hTextTrimmed = highlightedText.Trim();

        //                    if (hTextTrimmed.StartsWith("\"") && hTextTrimmed.EndsWith("\"") && hTextTrimmed.Count() > 2)
        //                        highlightedTexts.Add( hTextTrimmed.Substring(1, hTextTrimmed.Count() - 2));
        //                    else
        //                        highlightedTexts = hTextTrimmed.Split(' ').ToList();


        //                    //string[] highlightedTexts = highlightedText.Split(' ');
        //                    foreach (string hiTxt in highlightedTexts)
        //                    {
        //                        int index = textInRun.IndexOf(hiTxt, StringComparison.CurrentCultureIgnoreCase);
        //                        if (index != -1)
        //                        {
        //                            TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
        //                            TextPointer selectionEnd = selectionStart.GetPositionAtOffset(hiTxt.Length, LogicalDirection.Forward);
        //                            System.Windows.Documents.TextRange selection = new System.Windows.Documents.TextRange(selectionStart, selectionEnd);
        //                            //selection.Text = newStr;
        //                            selection.ApplyPropertyValue(TextElement.BackgroundProperty, "Yellow");
        //                            rtb.Selection.Select(selection.Start, selection.End);
        //                            //rtb.Focus();


        //                            ////aggiunto per evidenziare più stringhe nello stesso paragrafo
        //                            textInRun = current.GetTextInRun(LogicalDirection.Forward);
        //                        }

        //                    }
        //                }
        //                current = current.GetNextContextPosition(LogicalDirection.Forward);
        //            }

        //            rtf = rtb.Text;

        //        }




//        public void Concat(ref string rtf, string rtfToAppend)
//        {
//            if (!rtf.Any())
//            {
//                rtf = rtfToAppend;
//                return;
//            }

//            Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree1 = new Net.Sgoliver.NRtfTree.Core.RtfTree();
//            Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree2 = new Net.Sgoliver.NRtfTree.Core.RtfTree();
//            rtfTree1.LoadRtfText(rtf);
//            rtfTree2.LoadRtfText(rtfToAppend);

//            Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree = new Net.Sgoliver.NRtfTree.Core.RtfTree();

//            //string s1 = @"{\rtf\ansi\ansicpg1252\uc1\deff0\deflang1033{\fonttbl{\f0 Segoe UI;}{\f1 Verdana;}}{\colortbl\red0\green0\blue0;}\nouicompat\viewkind4\paperw12240\paperh15840\margl1425\margr1425\margt1425\margb1425\deftab720\sectd\pgwsxn12240\pghsxn15840\marglsxn1425\margrsxn1425\margtsxn1425\margbsxn1425\headery720\footery720\pard\sl0\slmult1\qj\ltrpar{\ltrch\f0\fs18\i0\b0\strike0\cf0\ulc0\ulnone\par}}";
//            string s = @"{\rtf\ansi\ansicpg1252\uc1\deff0\deflang1033{\fonttbl{\f0 Segoe UI;}{\f1 Verdana;}}{\colortbl\red0\green0\blue0;}\nouicompat\viewkind4\paperw12240\paperh15840\margl1425\margr1425\margt1425\margb1425\deftab720\sectd\pgwsxn12240\pghsxn15840\marglsxn1425\margrsxn1425\margtsxn1425\margbsxn1425\headery720\footery720\pard\sl0\slmult1\qj\ltrpar[TagTextRTF2][TagTextRTF1]}";
//            //string s = @"{\rtf1\ansi\ansicpg1252\deff0\deflang3082{\fonttbl{\f0\fnil Arial;}}{\colortbl\red0\green0\blue0 ;}{\*\generator NRtfTree Library 1.3.0;}\viewkind4\uc1\pard\cf0\fs20\f0\b[TagTextRTF2][TagTextRTF1]}";
           
//            rtfTree.LoadRtfText(s);

//            RtfMerger rtfMerger = new RtfMerger(rtfTree);
//            rtfMerger.AddPlaceHolder("[TagTextRTF1]", rtfTree1);
//            rtfMerger.AddPlaceHolder("[TagTextRTF2]", rtfTree2);
//            rtfMerger.Merge(false);


//            rtf = rtfTree.Rtf;

//        }


        public void Concat(ref string rtf, string rtfToAppend)
        {
            if (!rtf.Any())
            {
                rtf = rtfToAppend;
                return;
            }


            //RtfFormatProvider provider = new RtfFormatProvider();
            var provider = RtfProvider;

            RadDocument targetDocument = provider.Import(rtf);
            RadDocument sourceDocument = provider.Import(rtfToAppend);

            if (sourceDocument.IsEmpty)
                return;


            RadDocumentMerger merger = new RadDocumentMerger(targetDocument);

            InsertDocumentOptions options = new InsertDocumentOptions();
            options.ConflictingStylesResolutionMode = ConflictingStylesResolutionMode.UseTargetStyle;
            options.InsertLastParagraphMarker = true;

            merger.InsertDocument(sourceDocument, options);

            rtf = provider.Export(merger.Document);
        }



        //public List<string> Split(string rtf)
        //{
        //    List<string> rtfResult = new List<string>();

        //    Net.Sgoliver.NRtfTree.Core.RtfTree rtfTree = new Net.Sgoliver.NRtfTree.Core.RtfTree();
        //    rtfTree.LoadRtfText(rtf);

        //    if (rtfTree.RootNode.ChildNodes.Count > 1)
        //    {
        //        int p = 0;
        //    }

        //    foreach (Net.Sgoliver.NRtfTree.Core.RtfTreeNode child in rtfTree.RootNode.ChildNodes)
        //    {
        //        rtfResult.Add(child.Rtf);
        //    }
        //    return rtfResult;
        //}

        public void UpdateStyles(RadDocument document, List<StyleInfo> styles)
        {

            //Elimino per poi ricreare tutti gli stili join utente (cioè senza codice)
            //N.B. perdo tutte le associazioni a questi stili
            //IEnumerable<StyleDefinition> userStyles = document.StyleRepository.Where(item => item.Name.StartsWith(userStyleNamePrefix));
            //foreach (StyleDefinition sd in userStyles)
            //    document.StyleRepository.Remove(sd);


            //RtfFormatProvider provider = new RtfFormatProvider();
            //string rtf = provider.Export(document);


            foreach (StyleInfo styleInfo in styles)
            {
                string styleKey = styleInfo.Key;

                StyleDefinition styleDef = null;


                //cerco lo stile prima per Name (key) e poi per DisplayName
                styleDef = null;
                if (styleKey != null && styleKey.Any())
                {
                    //styleDef = document.StyleRepository.GetValueOrNull(styleKey); //cerco per Name
                    styleDef = document.StyleRepository.FirstOrDefault(item => item.DisplayName == styleInfo.Key);
                }
                //if (styleDef == null)
                //{
                //    styleDef = document.StyleRepository.FirstOrDefault(item => item.DisplayName == styleInfo.DisplayName);
                //    if (styleDef != null)
                //    {
                        
                //        //document.StyleRepository.Remove(styleDef);
                //        //styleDef = null;
                //    }
                //}

                //aggiungo lo stile
                if (styleDef == null)
                {   
                    //paragraph
                    string styleName = string.Empty;
                    styleName = string.Format("{0}", styleKey);
                    styleDef = new StyleDefinition(document.StyleRepository.GetValueOrNull("Normal"));
                    if (styleDef.Type != StyleType.Paragraph)
                        styleDef.Type = StyleType.Paragraph;
                    styleDef.IsCustom = true;
                    styleDef.IsDefault = false;
                    styleDef.IsPrimary = true;
                    styleDef.BasedOnName = "Normal";
                    styleDef.DisplayName = styleName;
                    styleDef.Name = styleName;
                    //styleDef.Name = styleDef.DisplayName;

                    //char
                    string styleNameChar = string.Empty;
                    styleNameChar = string.Format("{0}Char", styleKey);
                    StyleDefinition styleDefChar = new StyleDefinition();
                    if (styleDefChar.Type != StyleType.Character)
                        styleDefChar.Type = StyleType.Character;
                    styleDefChar.IsCustom = true;
                    styleDefChar.IsPrimary = false;
                    styleDefChar.IsDefault = false;
                    //styleDefChar.DisplayName = string.Format("{0}Char", styleInfo.DisplayName);
                    styleDefChar.DisplayName = styleNameChar;
                    styleDefChar.Name = styleNameChar;
                    //styleDefChar.Name = styleDefChar.DisplayName;

                    //link paragraph
                    styleDef.LinkedStyle = styleDefChar;

                    document.StyleRepository.Add(styleDef);
                    document.StyleRepository.Add(styleDefChar);
                    
                    
                }


                UpdateStyle(styleInfo, styleDef);

                if (styleDef.HasLinkedStyleOfType(StyleType.Character))
                {
                    UpdateStyle(styleInfo, styleDef.LinkedStyle);
                }

                //styleDef.DisplayName = styleInfo.DisplayName;
            }


        }

        public void UpdateStyles(ref string rtf, List<StyleInfo> styles/*, RtfMailMergeDataService rtfDataService*/)
        {
            RadDocument document = _rtfProvider.Import(rtf);
            //document.MailMergeDataSource.ItemsSource = rtfDataService;

            UpdateStyles(document, styles);

            _rtfProvider.ExportSettings.FieldResultMode = Telerik.Windows.Documents.Model.FieldDisplayMode.DisplayName;
            rtf = _rtfProvider.Export(document);

            //rtf = _telerikRtfProvider.Export(document, FieldDisplayMode.DisplayName);
        }

        //public void UpdateStyles2(ref string rtf, List<StyleInfo> styles)
        //{
        //    RadDocument document = _rtfProvider.Import(rtf);

        //    //string userStyleNamePrefix = "J";

        //    //if (document.IsEmpty)//scopo: Settare il paragrafo vuoto ad avere font il Segoe UI
        //    //    document = provider.Import(EmptyTelerikRtf);

        //    //var style = document.StyleRepository["Normal"];
        //    //style.SpanProperties.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");

        //    //rtf = _rtfProvider.Export(document);
        //    //document.BeginStylesUpdate();

        //    //Elimino per poi ricreare tutti gli stili join utente (cioè senza codice)
        //    //N.B. perdo tutte le associazioni a questi stili
        //    //IEnumerable<StyleDefinition> userStyles = document.StyleRepository.Where(item => item.Name.StartsWith(userStyleNamePrefix));
        //    //foreach (StyleDefinition sd in userStyles)
        //    //    document.StyleRepository.Remove(sd);


        //    foreach (StyleInfo styleInfo in styles)
        //    {
        //        string styleKey = styleInfo.Key;

        //        StyleDefinition styleDef = null;


        //        //cerco lo stile prima per Name (key) e poi per DisplayName
        //        styleDef = null;
        //        if (styleKey != null && styleKey.Any())
        //        {
        //            styleDef = document.StyleRepository.GetValueOrNull(styleKey); //cerco per Name
        //        }
        //        if (styleDef == null)
        //        {
        //            styleDef = document.StyleRepository.FirstOrDefault(item => item.DisplayName == styleInfo.DisplayName);
        //        }

        //        //aggiungo lo stile
        //        if (styleDef == null)
        //        {
        //            //char
        //            string styleNameChar = string.Empty;
        //            //if (styleKey.Any())
        //                styleNameChar = string.Format("{0}Char", styleKey);
        //            //else
        //            //    styleNameChar = string.Format("{0}-{1}Char", userStyleNamePrefix, styleInfo.DisplayName);


        //            StyleDefinition styleDefChar = new StyleDefinition();

        //            styleDefChar.DisplayName = string.Format("{0}Char", styleInfo.DisplayName);
        //            styleDefChar.IsCustom = true;
        //            styleDefChar.IsDefault = false;
        //            if (styleDefChar.Type != StyleType.Character)
        //                styleDefChar.Type = StyleType.Character;
        //            styleDefChar.Name = styleNameChar;



        //            //paragraph
        //            string styleName = string.Empty;
        //            //if (styleKey.Any())
        //                styleName = string.Format("{0}", styleKey);
        //            //else
        //            //    styleName = string.Format("{0}-{1}", userStyleNamePrefix, styleInfo.DisplayName);

        //            styleDef = new StyleDefinition(document.StyleRepository.GetValueOrNull("Normal"));

        //            styleDef.DisplayName = styleInfo.DisplayName;
        //            styleDef.IsCustom = true;
        //            styleDef.IsDefault = false;
        //            styleDef.BasedOnName = "Normal";
        //            if (styleDef.Type != StyleType.Paragraph)
        //                styleDef.Type = StyleType.Paragraph;
        //            styleDef.LinkedStyle = styleDefChar;
        //            styleDef.Name = styleName;


        //            document.StyleRepository.Add(styleDef);
        //            document.StyleRepository.Add(styleDefChar);
        //        }


        //        UpdateStyle(styleInfo, styleDef);

        //        if (styleDef.HasLinkedStyleOfType(StyleType.Character))
        //        {
        //            UpdateStyle(styleInfo, styleDef.LinkedStyle);
        //        }

        //        //styleDef.DisplayName = styleInfo.DisplayName;
        //    }

        //    //document.EndStylesUpdate();

        //    rtf = _rtfProvider.Export(document);
        //}

        private static void UpdateStyle(StyleInfo styleInfo, StyleDefinition styleDef)
        {
            //span

            
            styleDef.SpanProperties.FontFamily = new FontFamily(styleInfo.FontFamilyName);
            styleDef.SpanProperties.FontSize = styleInfo.FontSize * 4.0 / 3.0; //point to pixel

            if (styleInfo.IsBold)
                styleDef.SpanProperties.FontWeight = FontWeights.Bold;
            else
                styleDef.SpanProperties.FontWeight = FontWeights.Normal;

            if (styleInfo.IsUnderline)
                styleDef.SpanProperties.UnderlineDecoration = UnderlineTypes.Line;
            else
                styleDef.SpanProperties.UnderlineDecoration = UnderlineTypes.None;

            if (styleInfo.IsStrikethrough)
                styleDef.SpanProperties.Strikethrough = true;
            else
                styleDef.SpanProperties.Strikethrough = false;

            if (styleInfo.ForeColorHex != null && styleInfo.ForeColorHex.Any())
                styleDef.SpanProperties.ForeColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString(styleInfo.ForeColorHex);

            if (styleInfo.IsItalic)
                styleDef.SpanProperties.FontStyle = FontStyles.Italic;
            else
                styleDef.SpanProperties.FontStyle = FontStyles.Normal;

            ////////////////////////////////////////////////////////////////
            //paragraph

            //styleDef.DisplayName = styleInfo.DisplayName;

            if (styleInfo.BackColorHex != null && styleInfo.BackColorHex.Any())
                styleDef.ParagraphProperties.Background = (Color)System.Windows.Media.ColorConverter.ConvertFromString(styleInfo.BackColorHex);

            switch (styleInfo.TextAlignment)
            {
                case TextAlignmentEnum.Left:
                    styleDef.ParagraphProperties.TextAlignment = Telerik.Windows.Documents.Layout.RadTextAlignment.Left;
                    break;
                case TextAlignmentEnum.Center:
                    styleDef.ParagraphProperties.TextAlignment = Telerik.Windows.Documents.Layout.RadTextAlignment.Center;
                    break;
                case TextAlignmentEnum.Right:
                    styleDef.ParagraphProperties.TextAlignment = Telerik.Windows.Documents.Layout.RadTextAlignment.Right;
                    break;
                case TextAlignmentEnum.Justify:
                    styleDef.ParagraphProperties.TextAlignment = Telerik.Windows.Documents.Layout.RadTextAlignment.Justify;
                    break;
            }

            styleDef.ParagraphProperties.SpacingBefore = 0;
            styleDef.ParagraphProperties.SpacingAfter = 0;
            styleDef.ParagraphProperties.LineSpacing = 0;
        }

        public bool IsEmpty(string rtf)
        {
            RadDocument document = _rtfProvider.Import(rtf);
            return document.IsEmpty;
        }

        public string RtfMailMerge(string rtf, RtfFieldsDataService mailMergeDataService)
        {
            RadRichTextBox rtb = new RadRichTextBox();
            rtb.Document = _rtfProvider.Import(rtf);

            /////////////////////
            //Controllo se c'è almeno un MergeField
            bool anyMergeFields = false;
            foreach (FieldRangeStart fieldStart in rtb.Document.EnumerateChildrenOfType<FieldRangeStart>())
            {
                if (fieldStart.Field is MergeField)
                {
                    anyMergeFields = true;
                }
            }
            if (!anyMergeFields)
                return rtf;
            ///////////////////////


            rtb.Document.MailMergeDataSource.ItemsSource = mailMergeDataService;
            rtb.Document.MailMergeDataSource.MoveToFirst();

            


            RadDocument mergedDoc = rtb.MailMergeCurrentRecord();
            rtf = _rtfProvider.Export(mergedDoc);

            return rtf;
        }

        public static string CreateFieldDisplayName(string name)
        {
            return string.Format("«{0}»", name);
        }

        public void GetBriefRtf(string rtf, out string briefRtf)
        {
            briefRtf = rtf;

            RadRichTextBox rtb = new RadRichTextBox();
            rtb.Document = _rtfProvider.Import(rtf);

            Paragraph p = rtb.Document.EnumerateChildrenOfType<Paragraph>().FirstOrDefault();

            RadRichTextBox rtb2 = new RadRichTextBox();
            Paragraph paragraphCopy = p.CreateDeepCopy() as Paragraph;
            Section section = new Section();
            rtb2.Document.Sections.Add(section);
            rtb2.Document.Sections.Last.Blocks.Add(paragraphCopy);


            briefRtf = _rtfProvider.Export(rtb2.Document);

            //foreach (Paragraph p in this.radRichTextBox1.Document.EnumerateChildrenOfType<Paragraph>())
            //{
            //    Paragraph paragraphCopy = p.CreateDeepCopy() as Paragraph;
            //    richTextBox2.Document.Sections.Last.Blocks.Add(paragraphCopy);
            //}



        }

        /// <summary>
        /// uso syncfusion per aggiustare l'rtf in modo che venga letto correttamente da fastreport
        /// </summary>
        /// <param name="rtf"></param>
        /// <returns></returns>
        public string AdjustRtf(string rtf)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(rtf);
            MemoryStream streamRtf = new MemoryStream(byteArray);
            var document = new Syncfusion.DocIO.DLS.WordDocument(streamRtf, Syncfusion.DocIO.FormatType.Rtf);

            streamRtf = new MemoryStream();
            document.Save(streamRtf, Syncfusion.DocIO.FormatType.Rtf);


            streamRtf.Position = 0;
            StreamReader reader = new StreamReader(streamRtf);
            string rtf_out = reader.ReadToEnd();

            return rtf_out;
        }

    }

}