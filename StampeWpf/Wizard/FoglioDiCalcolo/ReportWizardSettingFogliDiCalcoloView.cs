using CommonResources;
using CommonResources.Controls;
using Commons;
using DevExpress.CodeParser;
using DevExpress.XtraRichEdit.Model;
using FogliDiCalcoloWpf;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace StampeWpf
{
    public class ReportWizardSettingFogliDiCalcoloView : NotificationBase
    {
        public ClientDataService DataService { get; set; }//ref
        public IEntityWindowService WindowService { get; set; }//ref
        public IMainOperation MainOperation { get; set; }//ref
        public StampeData ReportSetting { get; set; }

        public string CodiceReport { get; set; }

        private DevExpress.Xpf.Printing.LegacyPrintableComponentLink _PreviewResult;
        public DevExpress.Xpf.Printing.LegacyPrintableComponentLink PreviewResult
        {
            get
            {
                return _PreviewResult;
            }
            set
            {
                SetProperty(ref _PreviewResult, value);
            }
        }
        private ObservableCollection<string> _Sheets;
        public ObservableCollection<string> Sheets
        {
            get
            {
                return _Sheets;
            }
            set
            {
                SetProperty(ref _Sheets, value);
            }
        }

        private List<object> _SelectedSheets;
        public List<object> SelectedSheets
        {
            get
            {
                return _SelectedSheets;
            }
            set
            {
                SetProperty(ref _SelectedSheets, value);
                if (!IsLocked)
                    Preview();
            }
        }

        private Dictionary<int, string> _ModalitaAdattamentoFoglio;
        public Dictionary<int, string> ModalitaAdattamentoFoglio
        {
            get
            {
                return _ModalitaAdattamentoFoglio;
            }
            set
            {
                SetProperty(ref _ModalitaAdattamentoFoglio, value);
            }
        }

        private object _SelectedAdattamentoFoglio;
        public object SelectedAdattamentoFoglio
        {
            get
            {
                return _SelectedAdattamentoFoglio;
            }
            set
            {
                SetProperty(ref _SelectedAdattamentoFoglio, value);
                //if (!IsLocked)
                //    Preview();
            }
        }

        ////public int ModalitaAdattamentoFoglioIndex { get; set; }

        private FogliDiCalcoloWpf.FogliDiCalcoloView FogliDiCalcoloView;

        public bool IsLocked;

        public ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel> Sezioni { get; set; }
        public ComboBoxTreeLevel Sezione { get; set; }

        public ReportWizardSettingFogliDiCalcoloView(FogliDiCalcoloWpf.FogliDiCalcoloView fogliDiCalcoloView)
        {
            IsLocked = true;
            ModalitaAdattamentoFoglio = new Dictionary<int, string>();
            ModalitaAdattamentoFoglio.Add(0, LocalizationProvider.GetString("Nessuna scala"));
            ModalitaAdattamentoFoglio.Add(1, LocalizationProvider.GetString("Adatta foglio ad una pagina"));
            ModalitaAdattamentoFoglio.Add(2, LocalizationProvider.GetString("Adatta tutte le colonne ad una pagina"));
            ModalitaAdattamentoFoglio.Add(3, LocalizationProvider.GetString("Adatta tutte le righe ad una pagina"));
            SelectedAdattamentoFoglio = ModalitaAdattamentoFoglio.FirstOrDefault().Key;
            Sheets = new ObservableCollection<string>();
            SelectedSheets = new List<object>();
            //SelectedAdattamentoFoglio = new object();
            //ModalitaAdattamentoFoglioIndex = 0;
            FogliDiCalcoloView = fogliDiCalcoloView;
            ReportSetting = new StampeData();
            ReportSetting.FoglioDiCalcoloSetting = new FoglioDiCalcoloSetting();
            Sezioni = new ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel>();
        }
        public void Init()
        {
            FogliDiCalcoloView.UpdateFogliDiCalcoloData();
            DevExpress.Xpf.Spreadsheet.SpreadsheetControl spreadsheetctrl = new DevExpress.Xpf.Spreadsheet.SpreadsheetControl();
            spreadsheetctrl.BeforePrintSheet += Spreadsheetctrl_BeforePrintSheet;
            FogliDiCalcoloData fogliData = DataService.GetFogliDiCalcoloData();
            byte[] docBytes = fogliData.SerializedData;
            if (docBytes != null)
            {
                spreadsheetctrl.Document.LoadDocument(docBytes);
                DevExpress.Spreadsheet.IWorkbook workbook = spreadsheetctrl.Document;
                foreach (var item in workbook.Worksheets)
                {
                    Sheets.Add(item.Name);
                }
                foreach (var item in workbook.ChartSheets)
                {
                    Sheets.Add(item.Name);
                }
            }

            //LOAD DATA

            if (ReportSetting != null)
            {
                if (ReportSetting.FoglioDiCalcoloSetting != null)
                {
                    if (ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint != null)
                    {
                        foreach (var SheetName in ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint)
                        {
                            DevExpress.Spreadsheet.IWorkbook workbook = spreadsheetctrl.Document;
                            foreach (var item in workbook.Worksheets)
                            {
                                if (SheetName == item.Name)
                                {
                                    SelectedSheets.Add(item.Name);
                                }
                            }
                            foreach (var item in workbook.ChartSheets)
                            {
                                if (SheetName == item.Name)
                                {
                                    SelectedSheets.Add(item.Name);
                                }
                            }
                        }
                    }
                    if (ReportSetting.FoglioDiCalcoloSetting.FitToPageKey != -1)
                    {
                        SelectedAdattamentoFoglio = ReportSetting.FoglioDiCalcoloSetting.FitToPageKey;
                    }
                }
            }

            PreviewResult = new DevExpress.Xpf.Printing.LegacyPrintableComponentLink(spreadsheetctrl.Document);
            PreviewResult.CreateDocument();


            IsLocked = false;
        }

        private void Spreadsheetctrl_BeforePrintSheet(object sender, DevExpress.Spreadsheet.BeforePrintSheetEventArgs e)
        {
            if (SelectedSheets != null)
            {
                if (SelectedSheets.Contains(e.Name))
                    e.Cancel = false;
                else
                    e.Cancel = true;
            }
        }

        public void Preview()
        {
            DevExpress.Xpf.Spreadsheet.SpreadsheetControl spreadsheetctrl = new DevExpress.Xpf.Spreadsheet.SpreadsheetControl();
            spreadsheetctrl.BeforePrintSheet += Spreadsheetctrl_BeforePrintSheet;
            FogliDiCalcoloData fogliData = DataService.GetFogliDiCalcoloData();
            byte[] docBytes = fogliData.SerializedData;
            if (docBytes != null)
            {
                spreadsheetctrl.Document.LoadDocument(docBytes);
                DevExpress.Spreadsheet.IWorkbook workbook = spreadsheetctrl.Document;
                foreach (var item in workbook.Worksheets)
                {
                    if (SelectedAdattamentoFoglio is int)
                    {
                        if ((int)SelectedAdattamentoFoglio == 0)
                        {
                            item.PrintOptions.FitToPage = false;
                            item.PrintOptions.Scale = 100;
                        }
                        if ((int)SelectedAdattamentoFoglio == 1)
                        {
                            item.PrintOptions.FitToPage = true;
                        }
                        if ((int)SelectedAdattamentoFoglio == 2)
                        {
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToWidth = 1;
                            item.PrintOptions.FitToHeight = 0;
                        }
                        if ((int)SelectedAdattamentoFoglio == 3)
                        {
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToHeight = 1;
                            item.PrintOptions.FitToWidth = 0;
                        }
                    }
                }
                 foreach (DevExpress.Spreadsheet.ChartSheet item in workbook.ChartSheets)
                {
                    //item.ActiveView.PaperKind = System.Drawing.Printing.PaperKind.A4;
                    item.ActiveView.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
                    //item.ActiveView.SetCustomPaperSize(14, 22);
                }
            }

            PreviewResult = new DevExpress.Xpf.Printing.LegacyPrintableComponentLink(spreadsheetctrl.Document);
            PreviewResult.CreateDocument();
        }

        public void Preview(int AddatamentoFoglio)
        {
            DevExpress.Xpf.Spreadsheet.SpreadsheetControl spreadsheetctrl = new DevExpress.Xpf.Spreadsheet.SpreadsheetControl();
            spreadsheetctrl.BeforePrintSheet += Spreadsheetctrl_BeforePrintSheet;
            FogliDiCalcoloData fogliData = DataService.GetFogliDiCalcoloData();
            byte[] docBytes = fogliData.SerializedData;
            if (docBytes != null)
            {
                spreadsheetctrl.Document.LoadDocument(docBytes);
                DevExpress.Spreadsheet.IWorkbook workbook = spreadsheetctrl.Document;
                foreach (var item in workbook.Worksheets)
                {
                    if (AddatamentoFoglio is int)
                    {
                        if ((int)AddatamentoFoglio == 0)
                        {
                            item.PrintOptions.FitToPage = false;
                            item.PrintOptions.Scale = 100;
                        }
                        if ((int)AddatamentoFoglio == 1)
                        {
                            item.PrintOptions.FitToPage = true;
                        }
                        if ((int)AddatamentoFoglio == 2)
                        {
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToWidth = 1;
                            item.PrintOptions.FitToHeight = 0;
                        }
                        if ((int)AddatamentoFoglio == 3)
                        {
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToHeight = 1;
                            item.PrintOptions.FitToWidth = 0;
                        }
                    }
                }
                foreach (DevExpress.Spreadsheet.ChartSheet item in workbook.ChartSheets)
                {
                    //item.ActiveView.PaperKind = System.Drawing.Printing.PaperKind.A4;
                    item.ActiveView.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
                    
                    //item.ActiveView.SetCustomPaperSize(14, 22);
                }
            }

            PreviewResult = new DevExpress.Xpf.Printing.LegacyPrintableComponentLink(spreadsheetctrl.Document);
            PreviewResult.CreateDocument();
        }

        public void AcceptButton()
        {
            //FogliDiCalcoloData fogliDiCalcoloData = DataService.GetFogliDiCalcoloData();
            ReportSetting.FoglioDiCalcoloSetting.FitToPageKey = (int)SelectedAdattamentoFoglio;
            ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint = new List<string>();
            //ReportSetting.Sezione = Sezione.Content;
            //ReportSetting.SezioneKey = Sezione.Key;
            foreach (var item in SelectedSheets)
            {
                ReportSetting.FoglioDiCalcoloSetting.SheetNameToPrint.Add((string)item);
            }
            //DataService.SetFogliDiCalcoloData(fogliDiCalcoloData);
        }

        public List<ImageForPage> GeneraFoglioDiCacolo(string Dimensioni, string Orientamento, string MargineSuperiore, string MargineInferiore, string MargineSinistro, string MargineDestro, bool cutOnDataRange)
        {
            List<SizeF> sheetsPaperSize = new List<SizeF>();

            DevExpress.Xpf.Spreadsheet.SpreadsheetControl spreadsheetctrl = new DevExpress.Xpf.Spreadsheet.SpreadsheetControl();
            spreadsheetctrl.BeforePrintSheet += Spreadsheetctrl_BeforePrintSheet;
            FogliDiCalcoloData fogliData = DataService.GetFogliDiCalcoloData();
            byte[] docBytes = fogliData.SerializedData;
            if (docBytes != null)
            {
                spreadsheetctrl.Document.LoadDocument(docBytes);
                DevExpress.Spreadsheet.IWorkbook workbook = spreadsheetctrl.Document;
                workbook.Unit = DevExpress.Office.DocumentUnit.Centimeter;
                foreach (DevExpress.Spreadsheet.Worksheet item in workbook.Worksheets)
                {
                    if (SelectedSheets == null)
                        break;

                    if (!SelectedSheets.Contains(item.Name))
                        continue;

                    float paperWidth = 0;
                    float paperHeight = 0;
                                        
                    if (Orientamento == "0")//portrait
                    {
                        if (Dimensioni.Contains("A3"))
                        {
                            paperWidth = (float) 29.7 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro);
                            paperHeight = 42-7;

                        }
                        if (Dimensioni.Contains("A4"))
                        {
                            paperWidth = 21 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro);
                            paperHeight = 29-7;

                        }
                        if (Dimensioni.Contains("A5"))
                        {
                            paperWidth = (float) 14.85 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro);
                            paperHeight = 21-7;
                        }
                    }
                    else //landscape
                    {
                        if (Dimensioni.Contains("A3"))
                        {
                            paperWidth = 42 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro);
                            paperHeight = 29-7;
                        }
                        if (Dimensioni.Contains("A4"))
                        {
                            paperWidth = (float) 29.7 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro);
                            paperHeight = 21-7;

                        }
                        if (Dimensioni.Contains("A5"))
                        {
                            paperWidth = 21 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro);
                            paperHeight = 14-7;
                        }
                    }

                    float rangeHeight = GetDataRangeHeight(item);
                    if (rangeHeight >= paperHeight)
                        cutOnDataRange = false;

                    if (SelectedAdattamentoFoglio is int)
                    {
                        if (cutOnDataRange)
                        {
                            item.PrintOptions.FitToPage = false;
                            item.PrintOptions.Scale = 100;
                        }
                        else if ((int)SelectedAdattamentoFoglio == 0)
                        {
                            item.PrintOptions.FitToPage = false;
                            item.PrintOptions.Scale = 100;
                        }
                        else if ((int)SelectedAdattamentoFoglio == 1)
                        {
                            item.PrintOptions.FitToPage = true;
                        }
                        else if ((int)SelectedAdattamentoFoglio == 2)
                        {
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToWidth = 1;
                            item.PrintOptions.FitToHeight = 0;
                        }
                        else if ((int)SelectedAdattamentoFoglio == 3)
                        {
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToHeight = 1;
                            item.PrintOptions.FitToWidth = 0;
                        }

                    }
                    item.ActiveView.Margins.Top = 0;
                    item.ActiveView.Margins.Bottom = 0;
                    item.ActiveView.Margins.Right = 0;
                    item.ActiveView.Margins.Left = 0;


                    if (paperWidth > 0 && paperHeight > 0)
                    {
                        if (cutOnDataRange)
                        {
                            float rangeWidth = GetDataRangeWidth(item);
                            item.PrintOptions.FitToPage = true;
                            item.PrintOptions.FitToWidth = 1;
                            item.PrintOptions.FitToHeight = 0;

                            float height = rangeHeight;

                            if (rangeWidth > paperWidth)
                                height = (paperWidth * rangeHeight) / rangeWidth;



                            var pSize = new SizeF() { Width = Math.Min(paperWidth, rangeWidth), Height = height};
                            sheetsPaperSize.Add(pSize);
                            item.ActiveView.SetCustomPaperSize(pSize.Width, pSize.Height);
                        }
                        else
                            item.ActiveView.SetCustomPaperSize(paperWidth, paperHeight);

                    }


                }//fine worksheet

                foreach (DevExpress.Spreadsheet.ChartSheet item in workbook.ChartSheets)
                {
                    if (SelectedSheets == null)
                        break;

                    if (!SelectedSheets.Contains(item.Name))
                        continue;

                    cutOnDataRange = false;

                    item.ActiveView.Margins.Top = 0;
                    item.ActiveView.Margins.Bottom = 0;
                    item.ActiveView.Margins.Right = 0;
                    item.ActiveView.Margins.Left = 0;
                    item.ActiveView.Orientation = DevExpress.Spreadsheet.PageOrientation.Portrait;

                    float minimunDistance = float.Parse("0,7");
                    if (Orientamento == "0")//portrait
                    {
                        if (Dimensioni.Contains("A3")) { item.ActiveView.SetCustomPaperSize(29 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro) - minimunDistance, 35); }
                        if (Dimensioni.Contains("A4")) { item.ActiveView.SetCustomPaperSize(21 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro) - minimunDistance, 22); }
                        if (Dimensioni.Contains("A5")) { item.ActiveView.SetCustomPaperSize(13 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro) - minimunDistance, 14); }
                    }
                    else
                    {
                        if (Dimensioni.Contains("A3")) { item.ActiveView.SetCustomPaperSize(42 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro) - minimunDistance, 22); }
                        if (Dimensioni.Contains("A4")) { item.ActiveView.SetCustomPaperSize(29 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro) - minimunDistance, 14); }
                        if (Dimensioni.Contains("A5")) { item.ActiveView.SetCustomPaperSize(21 - (float)Convert.ToDouble(MargineSinistro) - (float)Convert.ToDouble(MargineDestro) - minimunDistance, 7); }
                    }
                }
            }

            PreviewResult = new DevExpress.Xpf.Printing.LegacyPrintableComponentLink(spreadsheetctrl.Document);
            PreviewResult.MinMargins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            PreviewResult.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
            //PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.Custom;

            
            //PreviewResult.CustomPaperSize = new Size(30, 8);

            PreviewResult.CreateDocument();


            //PreviewResult.Print();

            // GENERAZIONE IMMAGINE PER PAGINA
            List<ImageForPage> PagineGantt = new List<ImageForPage>();
            for (int i = 0; i < PreviewResult.PrintingSystem.Document.PageCount; i++)
            {
                using (MemoryStream ms = new MemoryStream())
                {

                    PreviewResult.ExportToImage(ms, new DevExpress.XtraPrinting.ImageExportOptions()
                    {
                        ExportMode = DevExpress.XtraPrinting.ImageExportMode.SingleFilePageByPage,
                        PageRange = $"{i + 1}",
                        Format = System.Drawing.Imaging.ImageFormat.Emf,
                        PageBorderWidth = 0,
                    });
                    //using (FileStream file = new FileStream("C:\\Users\\alberto.cantele\\Desktop\\ProvaQE" + i.ToString() + ".png", FileMode.Create, FileAccess.Write))
                    //    ms.WriteTo(file);

                    if (cutOnDataRange && i < 1) //Oss: se cutOnUsedRange  è true deve esserci una sola pagina per sheet  
                        PagineGantt.Add(new ImageForPage() { Image = Convert.ToBase64String(ms.ToArray()), Size = sheetsPaperSize[i] });
                    else //non dovrebbe mai entrare qua
                        PagineGantt.Add(new ImageForPage() { Image = Convert.ToBase64String(ms.ToArray()), Size = new System.Drawing.SizeF() });
                    //PagineGantt.Add(Encoding.Default.GetString(ms.ToArray()));
                }
            }

            return PagineGantt;
        }

        float GetDataRangeHeight(DevExpress.Spreadsheet.Worksheet sheet)
        {
            var range = sheet.GetDataRange();
            

            float heightSum = 0;
            for (int i = 0; i <= range.BottomRowIndex; i++)
            {
                heightSum += (float)((sheet.Rows[i].RowHeight));
            }

            return heightSum;
        }

        float GetDataRangeWidth(DevExpress.Spreadsheet.Worksheet sheet)
        {
            var range = sheet.GetDataRange();

            float widthSum = 0;
            for (int i = 0; i <= range.RightColumnIndex; i++)
            {
                widthSum += (float)(sheet.Columns[i].ColumnWidth);
            }

            

            return widthSum;
        }
    }
}
