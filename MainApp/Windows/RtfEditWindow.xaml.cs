using CommonResources;
using Commons;
using DevExpress.CodeParser;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Ribbon;
using DevExpress.Xpf.SpellChecker;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xbim.Common.Collections;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for Test_richEditSpell.xaml
    /// </summary>
    public partial class RtfEditWindow : Window
    {
        public string RtfText { get; protected set; }
        public string Text { get; protected set; }

        IDataService _dataService = null;
        IMainOperation _mainOperaton = null;

        public string RtfFieldCommandsName { get => "RtfFieldCommands"; }
        public string PasteRtfFieldName { get => "PasteRtfField"; }

        static string LayoutSettingsFileName = "richEditCtrlLayout.xml";
        static string LayoutSettingsFullFilePath { get; set; }

        static RtfEditWindow()
        {
            LayoutSettingsFullFilePath = string.Format("{0}{1}", MainMenuView.AppSettingsPath, LayoutSettingsFileName);
        }

        public RtfEditWindow()
        { 
            InitializeComponent();
            richTextBox.SpellChecker.Culture = CultureInfo.CurrentCulture;
            UpdateSelectedLanguageItem();

            Closing += RtfEditWindow_Closing;
            richTextBox.CalculateDocumentVariable += RichTextBox_CalculateDocumentVariable;
            richTextBox.Loaded += RichTextBox_Loaded;
            richTextBox.Unloaded += RichTextBox_Unloaded;
        }

        public void Init(string rtfText, IDataService dataService, IMainOperation mainOperation)
        {
            _dataService = dataService;
            _mainOperaton = mainOperation;

            richTextBox.RtfText = rtfText;

            ResetPageLayout.IsVisible = _mainOperaton.IsAdvancedMode();

        }


        private void RichTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            RibbonControl integratedRibbon = LayoutHelper.FindElementByName(richTextBox, "PART_RibbonControl") as RibbonControl;
            if (integratedRibbon != null)
            {
                string fileFullPath = string.Format("{0}{1}", MainMenuView.AppSettingsPath, LayoutSettingsFileName);
                integratedRibbon.SaveLayout(LayoutSettingsFullFilePath);
            }
        }

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            RibbonControl integratedRibbon = LayoutHelper.FindElementByName(richTextBox, "PART_RibbonControl") as RibbonControl;
            if (integratedRibbon != null)
            {
                integratedRibbon.SelectedPageName = "RibbonPage_Home";
                integratedRibbon.RestoreLayout(LayoutSettingsFullFilePath);

            }

            richTextBox.Options.MailMerge.DataSource = new RtfEntityDataService(_mainOperaton);
        }

        private void RichTextBox_CalculateDocumentVariable(object sender, DevExpress.XtraRichEdit.CalculateDocumentVariableEventArgs e)
        {
            string varName = e.VariableName;

            RtfEntityDataService rtfDataService = richTextBox.Options.MailMerge.DataSource as RtfEntityDataService;
            if (rtfDataService == null)
                return;

            ResultType resType = ResultType.PlainText;
            string result = rtfDataService.GetResult(varName, out resType);


            if (resType == ResultType.PlainText)
            {
                e.Value = result;
                e.Handled = true;
            }
            else
            {
                var doc = new RichEditDocumentServer();
                doc.RtfText = result;

                e.Value = doc;
                e.Handled = true;
            }
        }

        private void RtfEditWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RtfText = richTextBox.RtfText;
            Text = richTextBox.Text;
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.SelectAll();
        }
        private void PasteFieldBtn_Click(object sender, RoutedEventArgs e)
        {

            IDataObject dataObject = Clipboard.GetDataObject();
            string[] formats = dataObject.GetFormats();

            if (dataObject.GetDataPresent(RtfEntityDataService.RtfFieldClipboardFormat))
            {
                RemoveFieldsBtn.ItemClick -= RemoveFieldsBtn_Click;
                RemoveFieldsBtn.ItemClick += RemoveFieldsBtn_Click;

                List<string> fieldsPath = dataObject.GetData(RtfEntityDataService.RtfFieldClipboardFormat) as List<string>;

                BarButtonItem PasteFieldPathBtn = null;

                for (int i = 0; i < 10; i++)
                {
                    if (i == 0)
                        PasteFieldPathBtn = PasteFieldPathBtn0;
                    else if (i == 1)
                        PasteFieldPathBtn = PasteFieldPathBtn1;
                    else if (i == 2)
                        PasteFieldPathBtn = PasteFieldPathBtn2;
                    else if (i == 3)
                        PasteFieldPathBtn = PasteFieldPathBtn3;
                    else if (i == 4)
                        PasteFieldPathBtn = PasteFieldPathBtn4;
                    else if (i == 5)
                        PasteFieldPathBtn = PasteFieldPathBtn5;
                    else if (i == 6)
                        PasteFieldPathBtn = PasteFieldPathBtn6;
                    else if (i == 7)
                        PasteFieldPathBtn = PasteFieldPathBtn7;
                    else if (i == 8)
                        PasteFieldPathBtn = PasteFieldPathBtn8;
                    else if (i == 9)
                        PasteFieldPathBtn = PasteFieldPathBtn9;

                    if (fieldsPath.Count > i)
                    {
                        PasteFieldPathBtn.Content = fieldsPath[i];
                        PasteFieldPathBtn.ItemClick -= PasteFieldPathBtn_Click;
                        PasteFieldPathBtn.ItemClick += PasteFieldPathBtn_Click;
                    }
                    else
                        PasteFieldPathBtn.IsVisible = false;
                }
            }
            else
            {
                PasteFieldPathBtn0.IsVisible = false;
                PasteFieldPathBtn1.IsVisible = false;
                PasteFieldPathBtn2.IsVisible = false;
                PasteFieldPathBtn3.IsVisible = false;
                PasteFieldPathBtn4.IsVisible = false;
                PasteFieldPathBtn5.IsVisible = false;
                PasteFieldPathBtn6.IsVisible = false;
                PasteFieldPathBtn7.IsVisible = false;
                PasteFieldPathBtn8.IsVisible = false;
                PasteFieldPathBtn9.IsVisible = false;

            }

        }

        private void RemoveFieldsBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
        }

        private void PasteFieldPathBtn_Click(object sender, RoutedEventArgs e)
        {
            BarButtonItem item = sender as BarButtonItem;
            string path = item.Content as string;

            path = path.Replace(' ', RtfEntityDataService.PropertySpacePlaceholder);

            var doc = richTextBox.Document;
            DocumentPosition currentPosition = doc.CaretPosition;
            DocumentRange insertedRange = doc.InsertText(currentPosition, string.Format("DOCVARIABLE {0}", path));
            Field createdField = doc.Fields.Create(insertedRange);
            doc.Fields.Update();

            UpdateFields();
        }

        private void ShowFieldPreview_Checked(object sender, RoutedEventArgs e)
        {
            UpdateFields();
        }

        void UpdateFields()
        {
            for (int i = 0; i < richTextBox.Document.Fields.Count; i++)
            {
                // Show field codes.
                richTextBox.Document.Fields[i].ShowCodes = ShowFieldCodes.IsChecked.Value;
            }
        }

        private void AddPageNumberFieldBtn_Click(object sender, RoutedEventArgs e)
        {
            var doc = richTextBox.Document;
            DocumentPosition currentPosition = doc.CaretPosition;
            DocumentRange insertedRange = doc.InsertText(currentPosition, string.Format("DOCVARIABLE {0}", RtfEntityDataService.PageNumberDisplayName));
            Field createdField = doc.Fields.Create(insertedRange);
            doc.Fields.Update();


        }

        private void AddNumberPagesFieldBtn_Click(object sender, RoutedEventArgs e)
        {
            var doc = richTextBox.Document;
            DocumentPosition currentPosition = doc.CaretPosition;
            DocumentRange insertedRange = doc.InsertText(currentPosition, string.Format("DOCVARIABLE {0}", RtfEntityDataService.NumberPagesDisplayName));
            Field createdField = doc.Fields.Create(insertedRange);
            doc.Fields.Update();
        }

        private void AddFileNameFieldBtn_Click(object sender, RoutedEventArgs e)
        {
            var doc = richTextBox.Document;
            DocumentPosition currentPosition = doc.CaretPosition;
            DocumentRange insertedRange = doc.InsertText(currentPosition, string.Format("DOCVARIABLE {0}", RtfEntityDataService.FileNameFieldDisplayName));
            Field createdField = doc.Fields.Create(insertedRange);
            doc.Fields.Update();
        }

        private void ResetPageLayout_ItemClick(object sender, ItemClickEventArgs e)
        {
            richTextBox.Document.Sections[0].Page.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A3;
            richTextBox.Document.Sections[0].Margins.Left = 0;
            richTextBox.Document.Sections[0].Margins.Right = 0;
            richTextBox.Document.Sections[0].Margins.Top = 0;
            richTextBox.Document.Sections[0].Margins.Bottom = 0;

        }

        private void SpellCheck_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (richTextBox.SpellChecker.SpellCheckMode == DevExpress.XtraSpellChecker.SpellCheckMode.AsYouType)
                richTextBox.SpellChecker.SpellCheckMode = DevExpress.XtraSpellChecker.SpellCheckMode.OnDemand;

            else if (richTextBox.SpellChecker.SpellCheckMode == DevExpress.XtraSpellChecker.SpellCheckMode.OnDemand)
                richTextBox.SpellChecker.SpellCheckMode = DevExpress.XtraSpellChecker.SpellCheckMode.AsYouType;

            UpdateSelectedLanguageItem();
        }

        private void ItalianLanguage_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (richTextBox.SpellChecker.Culture.Name != "it-IT")
                richTextBox.SpellChecker.Culture = new CultureInfo("it-IT");

            UpdateSelectedLanguageItem();
        }

        private void EnglishLanguage_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (richTextBox.SpellChecker.Culture.Name != "en-US")
                richTextBox.SpellChecker.Culture = new CultureInfo("en-US");

            UpdateSelectedLanguageItem();
        }

        private void GermanLanguage_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (richTextBox.SpellChecker.Culture.Name != "de-DE")
                richTextBox.SpellChecker.Culture = new CultureInfo("de-DE");

            UpdateSelectedLanguageItem();
        }

        private void UpdateSelectedLanguageItem()
        {
            if (richTextBox.SpellChecker.Culture.Name == "it-IT")
                SelectedLanguage.Content = LocalizationProvider.GetString("Italiano");
            else if (richTextBox.SpellChecker.Culture.Name == "en-US")
                SelectedLanguage.Content = LocalizationProvider.GetString("Inglese");
            else if (richTextBox.SpellChecker.Culture.Name == "de-DE")
                SelectedLanguage.Content = LocalizationProvider.GetString("Tedesco");


            if (richTextBox.SpellChecker.SpellCheckMode == DevExpress.XtraSpellChecker.SpellCheckMode.AsYouType)
                SelectedLanguage.IsEnabled = true;
            else
                SelectedLanguage.IsEnabled = false;

        }
    }
}
