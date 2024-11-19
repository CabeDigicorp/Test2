using CommonResources;
using log4net.Core;
using Model;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.RichTextBoxUI.Menus;
using Telerik.Windows.Documents.Layout;
using Telerik.Windows.Documents.Lists;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Model.Fields;
using Telerik.Windows.Documents.Model.Styles;
using Telerik.Windows.Documents.RichTextBoxCommands;

namespace MasterDetailWpf
{
    public class RichTextBoxCustom : RadRichTextBox
    {
        public string RtfFieldCommandsName { get => "RtfFieldCommands"; }
        public string PasteClipboardRtfFieldName { get => "PasteRtfField"; }

        public RichTextBoxCustom()
        {
            Telerik.Windows.Controls.RichTextBoxUI.ContextMenu contextMenu = new Telerik.Windows.Controls.RichTextBoxUI.ContextMenu();
            contextMenu.Showing += ContextMenu_Showing;

            ContextMenu = contextMenu;

        }


        private void ContextMenu_Showing(object sender, Telerik.Windows.Controls.RichTextBoxUI.Menus.ContextMenuEventArgs e)
        {
            IDataObject dataObject = Clipboard.GetDataObject();
            string[] formats = dataObject.GetFormats();

            AddRtfFieldContextGroup(e, dataObject);
            
        }

        #region RtfField

        private void AddRtfFieldContextGroup(Telerik.Windows.Controls.RichTextBoxUI.Menus.ContextMenuEventArgs e, IDataObject dataObject)
        {


            if (dataObject.GetDataPresent(RtfEntityDataService.RtfFieldClipboardFormat))
            {
                RadMenuItem pasteFieldMenuItem = null;
                ContextMenuGroup contextMenuGroup = e.ContextMenuGroupCollection.FirstOrDefault(item => item.Name == RtfFieldCommandsName);
                if (contextMenuGroup != null)
                {
                    pasteFieldMenuItem = contextMenuGroup.FirstOrDefault(item => item.Name == PasteClipboardRtfFieldName);
                    if (pasteFieldMenuItem != null)
                    {
                        RadMenuItem removeFieldPathMenuItem = pasteFieldMenuItem.Items[0] as RadMenuItem;
                        removeFieldPathMenuItem.Click -= RemoveFieldsMenuItem_Click;
                        removeFieldPathMenuItem.Click += RemoveFieldsMenuItem_Click;

                        for (int i=1; i< pasteFieldMenuItem.Items.Count; i++)
                        {
                            RadMenuItem pasteFieldPathMenuItem = pasteFieldMenuItem.Items[i] as RadMenuItem;

                            pasteFieldPathMenuItem.Click -= PasteFieldPathMenuItem_Click;
                            pasteFieldPathMenuItem.Click += PasteFieldPathMenuItem_Click;

                        }
                    }
                }
                else
                {
                    contextMenuGroup = new ContextMenuGroup(RtfFieldCommandsName);
                    pasteFieldMenuItem = new RadMenuItem()
                    {
                        Name = PasteClipboardRtfFieldName,
                        Header = LocalizationProvider.GetString("IncollaCampo"),
                        Icon = GetIcon(@"pack://application:,,,/Resources/RtfEditor/IncollaCampo16.png"),
                        //Icon = GetIcon(@"pack://application:,,,/Resources/JoinIcon.ico"),
                    };


                    RadMenuItem removeFieldsMenuItem = new RadMenuItem()
                    {
                        Header = LocalizationProvider.GetString("SvuotaAppunti"),
                        Icon = GetIcon(@"pack://application:,,,/Resources/RtfEditor/SvuotaAppunti16.png"),
                    };
                    removeFieldsMenuItem.Click += RemoveFieldsMenuItem_Click;

                    pasteFieldMenuItem.Items.Add(removeFieldsMenuItem);
                    pasteFieldMenuItem.Items.Add(new RadMenuItem() { IsSeparator = true });


                    List<string> fieldsPath = dataObject.GetData(RtfEntityDataService.RtfFieldClipboardFormat) as List<string>;
                    foreach (string fieldPath in fieldsPath)
                    {
                        RadMenuItem pasteFieldPathMenuItem = new RadMenuItem() {Header = fieldPath, };
                        pasteFieldPathMenuItem.Click += PasteFieldPathMenuItem_Click;
                        pasteFieldMenuItem.Items.Add(pasteFieldPathMenuItem);
                    }

                    contextMenuGroup.Add(pasteFieldMenuItem);
                    e.ContextMenuGroupCollection.Add(contextMenuGroup);
                }
            }
        }

        System.Windows.Controls.Image GetIcon(string uri)
        {
            BitmapImage img = new BitmapImage(new Uri(uri));
            return new System.Windows.Controls.Image()
            {
                Source = img,
            };
        }

        private void RemoveFieldsMenuItem_Click(object sender, RadRoutedEventArgs e)
        {
            Clipboard.Clear();
        }

        private void PasteFieldPathMenuItem_Click(object sender, RadRoutedEventArgs e)
        {
            RadMenuItem item = sender as RadMenuItem;
            if (item == null)
                return;

            string path = item.Header as string;
            AddRtfField(path);
        }

        public bool AddRtfField(string propertyPath)
        {
            if (propertyPath == null || !propertyPath.Any())
                return false;

            var rtfField = new RtfField() { PropertyPath = propertyPath };
            return InsertField(rtfField, FieldDisplayMode.DisplayName);
        }

        #endregion RtfField


        #region ChangeFontFamilyCommand Wordaround
        //workaround per riuscire a salvare gli stili dell'elenco numerato

        protected override void OnCommandExecuted(CommandExecutedEventArgs e)
        {
            base.OnCommandExecuted(e);


            Debug.WriteLine(e.Command.ToString());

            var documentLists = Document.GetType().GetProperty("DocumentLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(Document, null);
            var selectedParagraphs = Document.Selection.GetSelectedParagraphs();

            if (selectedParagraphs.Count() > 0)
            {
                var list = (documentLists as Dictionary<int, DocumentList>).FirstOrDefault(dL => dL.Value.FirstParagraph == selectedParagraphs.First()).Value;

                if (list == null)
                    return;

                var firstSelectedParagraphs = selectedParagraphs.FirstOrDefault();

                foreach (var level in list.Style.Levels)
                {
                    if (e.Command is ChangeListStyleCommand)
                    {
                        level.SpanProperties.FontSize = selectedParagraphs.FirstOrDefault().FontSize;
                        

                    }    
                    if (e.Command is ChangeFontFamilyCommand)
                    {
                        var fontFamily = e.CommandParameter as FontFamilyInfo;
                        level.FontFamily = fontFamily;
                        level.SpanProperties.FontFamily = fontFamily;
                    }
                    else if (e.Command is ChangeFontSizeCommand)
                    {
                        
                        string size = e.CommandParameter.ToString();
                        double dSize = double.Parse(size, CultureInfo.InvariantCulture);
                        level.SpanProperties.FontSize = dSize;
                        

                    }
                    else if (e.Command is ChangeFontForeColorCommand)
                    {
                        level.ForeColor = (Color) e.CommandParameter;
                        level.SpanProperties.ForeColor = (Color)e.CommandParameter;

                    }
                    else if (e.Command is ToggleBoldCommand)
                    {
                        if (level.FontWeight == FontWeights.Bold)
                        {
                            level.FontWeight = FontWeights.Normal;
                            level.SpanProperties.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            level.FontWeight = FontWeights.Bold;
                            level.SpanProperties.FontWeight = FontWeights.Bold;
                        }
                    }
                    

                }
            }

        }
        #endregion

    }
}
