using _3DModelExchange;
using AttivitaWpf;
using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.Charts.Designer.Native;
using DevExpress.Charts.Native;
using DevExpress.Pdf;
using DevExpress.Pdf.Native.BouncyCastle.Utilities;
using DevExpress.Spreadsheet;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Ribbon;
using DevExpress.Xpf.Spreadsheet;
using FogliDiCalcoloWpf.View;
using MasterDetailModel;
using Model;
using ModelData.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BuiltInCodes = MasterDetailModel.BuiltInCodes;


namespace FogliDiCalcoloWpf
{
    /// <summary>
    /// Interaction logic for FoglioDiCalcoloCtrl.xaml
    /// </summary>
    public partial class FogliDiCalcoloCtrl : UserControl
    {
        public FogliDiCalcoloView View { get => DataContext as FogliDiCalcoloView; }
        private string SezioneKey;
        private string TabellaKey;
        private string RadiceKey;
        public FogliDiCalcoloCtrl()
        {
            InitializeComponent();

            DataContextChanged += FogliDiCalcoloCtrl_DataContextChanged;
            SpreadSheetCtrl.ModifiedChanged += SpreadSheetCtrl_ModifiedChanged;
            SpreadSheetCtrl.Loaded += SpreadSheetCtrl_Loaded;

            SpreadSheetCtrl.Document.SheetRenaming += Document_SheetRenaming;

        }

        private void SpreadSheetCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            View.LoadData();
            RibbonControl integratedRibbon = LayoutHelper.FindElementByName(SpreadSheetCtrl, "PART_RibbonControl") as RibbonControl;
            if (integratedRibbon != null)
            {
                integratedRibbon.SelectedPage = integratedRibbon.GetFirstSelectablePage();
            }
        }

        private void SpreadSheetCtrl_ModifiedChanged(object sender, EventArgs e)
        {
            View.ModelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.SAVE_FOGLIDICALCOLODATA });
            SpreadSheetCtrl.Modified = false;
        }

        private void Document_SheetRenaming(object sender, SheetRenamingEventArgs e)
        {
        }
        
        
        private void GeneraData_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SezioneKey = null;
            if (((DevExpress.Xpf.Bars.BarButtonItem)sender).Name == "ComputoData")
            {
                SezioneKey = BuiltInCodes.EntityType.Computo;
                TabellaKey = "Computo" + "Tabella";
                RadiceKey = "Computo";
            }

            View.FoglioDiCalcoloDataView = null;
            FoglioDiCalcoloDataWnd foglioDiCalcoloDataWnd = new FoglioDiCalcoloDataWnd();
            foglioDiCalcoloDataWnd.SourceInitialized += (x, y) => foglioDiCalcoloDataWnd.HideMinimizeAndMaximizeButtons();
            foglioDiCalcoloDataWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            (foglioDiCalcoloDataWnd.DataContext as FoglioDiCalcoloDataView).DataService = View.DataService;
            (foglioDiCalcoloDataWnd.DataContext as FoglioDiCalcoloDataView).MainOperation = View.MainOperation;
            (foglioDiCalcoloDataWnd.DataContext as FoglioDiCalcoloDataView).WindowService = View.WindowService;
            (foglioDiCalcoloDataWnd.DataContext as FoglioDiCalcoloDataView).Init(SezioneKey);
            View.FoglioDiCalcoloDataView = (foglioDiCalcoloDataWnd.DataContext as FoglioDiCalcoloDataView);
            foglioDiCalcoloDataWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (foglioDiCalcoloDataWnd.ShowDialog() == true)
            {
                View.WindowService.ShowWaitCursor(true);
                GeneraFoglio(true);
                RicalculateAllSheet();
                View.WindowService.ShowWaitCursor(false);
            }
        }

        private void AggiornaData_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            View.WindowService.ShowWaitCursor(true);

            SezioneKey = null;
            if (((DevExpress.Xpf.Bars.BarButtonItem)sender).Name == "ComputoUpdate")
            {
                SezioneKey = BuiltInCodes.EntityType.Computo;
                TabellaKey = "Computo" + "Tabella";
                RadiceKey = "Computo";
            }

            GeneraFoglio(false);

            RicalculateAllSheet();

            View.WindowService.ShowWaitCursor(false);

            //var bytefile = SpreadSheetCtrl.Document.SaveDocument(DocumentFormat.Xlsx);
            //System.IO.File.WriteAllBytes("C:\\Users\\alberto.cantele\\Desktop\\FormulaBatrDoesNotUpdate.xlsx", bytefile);
        }

        private void UpdateFoglio(string codeName)
        {
            bool updateTable = false;
            View.WindowService.ShowWaitCursor(true);

            if (codeName == BuiltInCodes.EntityType.Computo)
            {
                SezioneKey = BuiltInCodes.EntityType.Computo;
                TabellaKey = "Computo" + "Tabella";
                RadiceKey = "Computo";
                GeneraFoglio(false);
                //RicalculateAllSheet();
                //View.WindowService.ShowWaitCursor(false);
            }

            

            if (codeName == BuiltInCodes.EntityType.WBS)
            {
                string sheetBaseName = FogliDiCalcoloKeys.GanttDataSheetBaseName;

                View.WindowService.ShowWaitCursor(true);

                IWorkbook workbook = SpreadSheetCtrl.Document;
                //List<string> ListaNomi = workbook.Worksheets.Where(n => n.Name.StartsWith("WBSGanttData")).Select(n => n.Name).ToList();
                List<string> ListaNomi = workbook.Worksheets.Where(n => n.Name.StartsWith(sheetBaseName)).Select(n => n.Name).ToList();
                if (ListaNomi.Count() == 0)
                {
                    View.WindowService.ShowWaitCursor(false);
                    return;
                }

                foreach (Worksheet worksheet in workbook.Worksheets.Where(n => n.Name.StartsWith(sheetBaseName)).ToList())
                {
                    SezioneKey = BuiltInCodes.EntityType.WBS;
                    TabellaKey = "WBSGantt" + "Tabella";
                    RadiceKey = "WBSGantt";

                    var fogliDiCalcoloData = View.DataService.GetFogliDiCalcoloData();
                    var foglioDiCalcoloData = fogliDiCalcoloData.FoglioDiCalcolo.Where(item => item.Foglio == worksheet.Name).FirstOrDefault();//AU
                    if (foglioDiCalcoloData != null)
                    {
                        TabellaKey = foglioDiCalcoloData.Tabella;
                    }
                    else
                    {
                        int NumberLastSheet = 0;
                        string LastString = worksheet.Name.Substring(worksheet.Name.Length - 2);
                        if (int.TryParse(LastString, out NumberLastSheet))
                        {
                            TabellaKey += LastString;
                            RadiceKey += LastString;
                        }
                    }


                    GeneraTabellaGantt(worksheet, true);
                    //RicalculateAllSheet();
                }
            }

            if (codeName == BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSAL)
            {
                string sheetBaseName = FogliDiCalcoloKeys.GanttSALDataSheetBaseName;


                SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSAL;
                TabellaKey = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Tabella";
                RadiceKey = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL;
                //string SheetName = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Data";
                string SheetName = sheetBaseName;

                IWorkbook workbook = SpreadSheetCtrl.Document;
                Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

                if (worksheet == null)
                {
                    workbook.Worksheets.Add(SheetName);
                    workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                    worksheet = workbook.Worksheets.LastOrDefault();
                }
                else
                {
                    updateTable = true;
                }

                GeneraTabellaGantt(worksheet, updateTable, true);
                //RicalculateAllSheet();
            }

            if (codeName == BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL)
            {
                string sheetBaseName = FogliDiCalcoloKeys.GanttProgSALDataSheetBaseName;

                SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL;
                TabellaKey = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Tabella";
                RadiceKey = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL;
                //string SheetName = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Data";
                string SheetName = sheetBaseName;

                IWorkbook workbook = SpreadSheetCtrl.Document;
                Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

                if (worksheet == null)
                {
                    workbook.Worksheets.Add(SheetName);
                    workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                    worksheet = workbook.Worksheets.LastOrDefault();
                }
                else
                {
                    updateTable = true;
                }

                GeneraTabellaGantt(worksheet, updateTable, true);
                //RicalculateAllSheet();
            }

            if (codeName == BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSched)
            {
                string sheetBaseName = FogliDiCalcoloKeys.GanttSchedDataSheetBaseName;

                SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSched;
                //TabellaKey = "WBSGanttSched" + "Tabella";
                //RadiceKey = "WBSGanttSched";
                //string SheetName = "WBSGanttSchedData";
                string SheetName = sheetBaseName;

                IWorkbook workbook = SpreadSheetCtrl.Document;
                Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

                bool isNewWorksheet = false;
                if (worksheet == null)
                {
                    workbook.Worksheets.Add(SheetName);
                    workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                    worksheet = workbook.Worksheets.LastOrDefault();
                    isNewWorksheet = true;
                }

                GeneraTabellaSched(worksheet, SheetName, isNewWorksheet);
                //RicalculateAllSheet();
            }

            RicalculateAllSheet();
            View.WindowService.ShowWaitCursor(false);
        }

        private void GanttView_UpdateProgrammazineSAL(object sender, EventArgs e)
        {
            SezioneKey = null;
            string SheetName = null;

            //SOLO SAL
            SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSAL;
            TabellaKey = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Tabella";
            RadiceKey = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL;
            //SheetName = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Data";
            SheetName = FogliDiCalcoloKeys.GanttSALDataSheetBaseName;
            bool updateTable = false;

            IWorkbook workbook = SpreadSheetCtrl.Document;

            Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

            if (worksheet == null)
            {
                workbook.Worksheets.Add(SheetName);
                workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                worksheet = workbook.Worksheets.LastOrDefault();
            }
            else
            {
                updateTable = true;
            }

            View.WindowService.ShowWaitCursor(true);

            GeneraTabellaGantt(worksheet, updateTable, true);

            //TABELLA PROGRAMMAZIONE SAL
            SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL;
            TabellaKey = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Tabella";
            RadiceKey = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL;
            //SheetName = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Data";
            SheetName = FogliDiCalcoloKeys.GanttProgSALDataSheetBaseName;
            updateTable = false;

            workbook = SpreadSheetCtrl.Document;

            worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

            if (worksheet == null)
            {
                workbook.Worksheets.Add(SheetName);
                workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                worksheet = workbook.Worksheets.LastOrDefault();
            }
            else
            {
                updateTable = true;
            }

            GeneraTabellaGantt(worksheet, updateTable, true);

            RicalculateAllSheet();

            View.WindowService.ShowWaitCursor(false);
        }

        private void GeneraFoglio(bool createSheet)
        {
            IWorkbook workbook = SpreadSheetCtrl.Document;
            Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();
            bool ClearContent = false;

            if (createSheet)
            {
                if (worksheet != null)
                {
                    ClearContent = true;
                }
                else
                {
                    workbook.Worksheets.Add(View.FoglioDiCalcoloDataView.DataText);
                    workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                    worksheet = workbook.Worksheets.LastOrDefault();
                }
            }
            else
            {
                ClearContent = true;
                if (worksheet == null)
                {
                    return;
                }
            }

            if (!worksheet.IsProtected)
                worksheet.Protect("", WorksheetProtectionPermissions.Default);

            GeneraTabella(worksheet, null);
        }

        private void GeneraTabella(Worksheet worksheet, string sheetName)
        {
            List<AttibutiFogiloDiCalcoloView> listaAttributi = View.GetColumns(SezioneKey, sheetName);
            DataTable dataTable = View.GeneraDataSource(SezioneKey);
            var FoundTable = worksheet.Tables.Where(t => t.Name == TabellaKey).FirstOrDefault();
            int TotaleRighe = dataTable.Rows.Count;


            SpreadSheetCtrl.BeginUpdate();

            InsertRemoveColumn2(worksheet, listaAttributi);

            if (FoundTable != null)
            {
                CellRange rangeToClean = worksheet["A2" + ":" + "XFD1000000"];
                worksheet.ClearContents(rangeToClean);
            }

            string FirstCellTable = "A1";
            string LastCellTable = "A1";

            Dictionary<int, string> dictionaryFormule = new Dictionary<int, string>();

            LastCellTable = CompileExcelWithValues(worksheet, TotaleRighe, listaAttributi, dictionaryFormule, dataTable);

            SpreadSheetCtrl.EndUpdate();

            

            if (FirstCellTable == LastCellTable)
                return;

            CellRange range = worksheet[FirstCellTable + ":" + LastCellTable];

            SeTableRangeAndFormulaColumn(worksheet, range, dictionaryFormule, TabellaKey);

            RipristinaDataSourcePivot(TabellaKey, range);
        }

        private void RipristinaDataSourcePivot_old(string tabellaKey, CellRange range)
        {
            try
            {

                string cellRangeWorksheetName = range.Worksheet.Name;

                IWorkbook workbook = SpreadSheetCtrl.Document;

                foreach (Worksheet worksheet in workbook.Worksheets)
                {
                    foreach (PivotTable pivotTable in worksheet.PivotTables)
                    {
                        CellRange oldRange = pivotTable.Cache.SourceRange;
                        if (oldRange != null && oldRange.Worksheet.Name.StartsWith(cellRangeWorksheetName))
                        {
                            pivotTable.BeginUpdate();
                            pivotTable.ChangeDataSource(range);
                            pivotTable.EndUpdate();
                            pivotTable.Cache.Refresh();
                        }
                    }
                }


            }
            catch (Exception exc)
            {

            }

            //IWorkbook workbook = SpreadSheetCtrl.Document;

            //foreach (Worksheet worksheet in workbook.Worksheets)
            //{
            //    for (int i = 1; i < 20; i++)
            //    {
            //        PivotTable pivotTable = worksheet.PivotTables[RadiceKey + "PivotTable" + i];
            //        if (pivotTable == null)
            //        {
            //            continue;
            //        }
            //        pivotTable.BeginUpdate();
            //        pivotTable.ChangeDataSource(range);
            //        pivotTable.EndUpdate();
            //        pivotTable.Cache.Refresh();
            //    }
            //}
        }




        /// <summary>
        /// Riscritta il metodo con un workaround per ovviare ad un crash di syncfusion
        /// L'errore è che se inserisce una colonna prima del BaseField questo non aggiorna il suo indice.
        /// </summary>
        /// <param name="tabellaKey"></param>
        /// <param name="range"></param>
        private void RipristinaDataSourcePivot(string tabellaKey, CellRange range)
        {
            try
            {

                string cellRangeWorksheetName = range.Worksheet.Name;

                IWorkbook workbook = SpreadSheetCtrl.Document;

                foreach (Worksheet worksheet in workbook.Worksheets)
                {
                    foreach (PivotTable pivotTable in worksheet.PivotTables)
                    {
                        CellRange oldRange = pivotTable.Cache.SourceRange;

                        if (oldRange != null && oldRange.Worksheet.Name.StartsWith(cellRangeWorksheetName))
                        {
                            var baseFieldsNameByName = pivotTable.DataFields.Where(x => x.BaseField != null).ToDictionary(x => x.Name, x => x.BaseField.Name);

                            pivotTable.BeginUpdate();
                            pivotTable.ChangeDataSource(range);

                            //foreach (var fieldName in baseFieldsNameByName.Keys)
                            //{
                            //    //estraggo un po di dati da dataField per poterli risettare uguali
                            //    var dataField = pivotTable.DataFields.FirstOrDefault(x => x.Name == fieldName);
                            //    var consolidationFunc = dataField.SummarizeValuesBy;
                            //    var showValuesAs = dataField.ShowValuesAs;
                            //    var baseItemType = dataField.BaseItemType;
                            //    var sourceFieldName = dataField.Field.Name;

                            //    //elimino il dataField
                            //    pivotTable.DataFields.Remove(dataField);

                            //    //Aggiungo il dataField con il baseFiled aggiornato
                            //    var sourceField = pivotTable.Fields[sourceFieldName];
                            //    if (sourceField != null)
                            //    {
                            //        var newField = pivotTable.DataFields.Add(sourceField, fieldName, consolidationFunc);
                            //        newField.ShowValuesWithCalculation(showValuesAs, pivotTable.Fields[baseFieldsNameByName[fieldName]], baseItemType);
                            //    }
                            //}

                            pivotTable.EndUpdate();
                            pivotTable.Cache.Refresh();
                        }
                    }
                }


            }
            catch (Exception exc)
            {

            }

        }

            private void RipristinaDataSourceChart(string tabellaKey, CellRange range)
        {

            string cellRangeWorksheetName = range.Worksheet.Name;

            IWorkbook workbook = SpreadSheetCtrl.Document;

            foreach (ChartSheet chartSheet in workbook.ChartSheets)
            {
                CellRange oldRange = chartSheet.Chart.GetDataRange();
                if (oldRange != null && oldRange.Worksheet.Name.StartsWith(cellRangeWorksheetName))
                {
                    chartSheet.Chart.SelectData(range);
                }

                //if (chartSheet.Name.StartsWith(RadiceKey + "Chart"))
                //{
                //    chartSheet.Chart.SelectData(range);
                //}
            }
        }

        private void RicalculateAllSheet()
        {
            IWorkbook workbook = SpreadSheetCtrl.Document;
            foreach (Worksheet worksheet in workbook.Worksheets)
            {
                worksheet.Calculate();
            }
        }

        private void GeneraWBSGanttData_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SezioneKey = null;
            string SheetName = null;
            if (((DevExpress.Xpf.Bars.BarButtonItem)sender).Name == "WBSGanttData")
            {
                SezioneKey = BuiltInCodes.EntityType.WBS;
                TabellaKey = "WBSGantt" + "Tabella";
                RadiceKey = "WBSGantt";
                SheetName = LocalizationProvider.GetString("ProduttivitaGantt");//"WBSGanttData";
            }

            View.FoglioDiCalcoloGanttDataView = null;
            FoglioDiCalcoloWBSGanttDataWnd foglioDiCalcoloGanttDataWnd = new FoglioDiCalcoloWBSGanttDataWnd();
            foglioDiCalcoloGanttDataWnd.SourceInitialized += (x, y) => foglioDiCalcoloGanttDataWnd.HideMinimizeAndMaximizeButtons();
            foglioDiCalcoloGanttDataWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataService = View.DataService;
            (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).MainOperation = View.MainOperation;
            (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).WindowService = View.WindowService;
            (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).Init(SezioneKey);
            View.FoglioDiCalcoloGanttDataView = (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView);
            View.FoglioDiCalcoloGanttDataView.RimuoviAttributiDiversiDaRealiContabilita();
            foglioDiCalcoloGanttDataWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            IWorkbook workbook = SpreadSheetCtrl.Document;

            //List<string> ListaNomi = workbook.Worksheets.Where(n => n.Name.StartsWith("WBSGanttData")).Select(n => n.Name).ToList();
            List<string> ListaNomi = workbook.Worksheets.Where(n => n.Name.StartsWith(SheetName)).Select(n => n.Name).ToList();
            if (ListaNomi.Count() > 0)
            {
                for (int SheetNumber = 1; SheetNumber < 100; SheetNumber++)
                {
                    string foglioName = string.Format("{0} ({1})", SheetName, SheetNumber.ToString());
                    if (!ListaNomi.Contains(foglioName))
                    {
                        SheetName = foglioName;
                        string dataTextCompleted = string.Format("{0}: {1}", LocalizationProvider.GetString("Foglio"), SheetName);
                        (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataTextCompleted = dataTextCompleted;
                        (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataText = foglioName;
                        (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaText += SheetNumber;
                        (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaTextCompleted += SheetNumber;
                        TabellaKey += SheetNumber;
                        RadiceKey += SheetNumber;
                        break;
                    }    
                }


                //int NumberLastSheet = 0;
                //string LastString = ListaNomi.LastOrDefault().Substring(ListaNomi.LastOrDefault().Length - 2);
                //if (int.TryParse(LastString, out NumberLastSheet))
                //{
                //    string SheetNumber = (NumberLastSheet + 1).ToString();
                //    SheetName = SheetName + SheetNumber;
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataTextCompleted += SheetNumber;
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataText += SheetNumber;
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaText += SheetNumber;
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaTextCompleted += SheetNumber;
                //    TabellaKey += SheetNumber;
                //    RadiceKey += SheetNumber;
                //}
                //else
                //{
                //    SheetName = SheetName + "10";
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataTextCompleted += "10";
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataText += "10";
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaText += "10";
                //    (foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaTextCompleted += "10";
                //    TabellaKey += "10";
                //    RadiceKey += "10";
                //}
            }
            else
            {
                //SheetName = SheetName + "10";
                //(foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataTextCompleted += "10";
                //(foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).DataText += "10";
                //(foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaText += "10";
                //(foglioDiCalcoloGanttDataWnd.DataContext as FoglioDiCalcoloGanttDataView).TabellaTextCompleted += "10";
                //TabellaKey += "10";
                //RadiceKey += "10";
            }

            if (foglioDiCalcoloGanttDataWnd.ShowDialog() != true)
            {
                return;
            }

            workbook.Worksheets.Add(SheetName);
            workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
            Worksheet worksheet = workbook.Worksheets.LastOrDefault();

            View.WindowService.ShowWaitCursor(true);

            try
            {

                GeneraTabellaGantt(worksheet);


                RicalculateAllSheet();

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }


            View.WindowService.ShowWaitCursor(false);
        }

        private void WBSGanttSALUpdate_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SezioneKey = null;
            string SheetName = null;

            //SOLO SAL
            SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSAL;
            TabellaKey = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Tabella";
            RadiceKey = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL;
            //SheetName = "WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Data";
            SheetName = FogliDiCalcoloKeys.GanttSALDataSheetBaseName;
            bool updateTable = false;

            IWorkbook workbook = SpreadSheetCtrl.Document;

            Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

            if (worksheet == null)
            {
                workbook.Worksheets.Add(SheetName);
                workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                worksheet = workbook.Worksheets.LastOrDefault();
            }
            else
            {
                updateTable = true;
            }

            View.WindowService.ShowWaitCursor(true);

            GeneraTabellaGantt(worksheet, updateTable, true);

            //TABELLA PROGRAMMAZIONE SAL
            SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL;
            TabellaKey = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Tabella";
            RadiceKey = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL;
            //SheetName = "WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Data";
            SheetName = FogliDiCalcoloKeys.GanttProgSALDataSheetBaseName;
            updateTable = false;

            workbook = SpreadSheetCtrl.Document;

            worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

            if (worksheet == null)
            {
                workbook.Worksheets.Add(SheetName);
                workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                worksheet = workbook.Worksheets.LastOrDefault();
            }
            else
            {
                updateTable = true;
            }

            GeneraTabellaGantt(worksheet, updateTable, true);

            RicalculateAllSheet();

            View.WindowService.ShowWaitCursor(false);
        }

        private void GeneraTabellaGantt(Worksheet worksheet, bool updateTable = false, bool checkColumnData = false)
        {
            Dictionary<int, string> dictionaryFormule = new Dictionary<int, string>();
            List<AttibutiFogiloDiCalcoloView> listaAttributi = View.GetColumns(SezioneKey, worksheet.Name);
            if (listaAttributi.Where(r => r.Etichetta.StartsWith(GanttKeys.ConstSAL)).FirstOrDefault() != null)
            {
                listaAttributi.Remove(listaAttributi.Where(r => r.Etichetta.StartsWith(GanttKeys.ConstSAL)).FirstOrDefault());
            }
            DataTable dataTable = View.GeneraDataSource(SezioneKey, worksheet.Name);
            var FoundTable = worksheet.Tables.Where(t => t.Name == TabellaKey).FirstOrDefault();
            if (dataTable.Columns.Count == 0)
            {
                return;
            }

            foreach (AttibutiFogiloDiCalcoloView attributo in listaAttributi)
            {
                if (attributo.CodiceOrigine == "Data" || attributo.CodiceOrigine == "GiorniPeriodo" || attributo.CodiceOrigine == "GiorniProgressivo")
                {
                    continue;
                }
                if (attributo.Etichetta.Contains(GanttKeys.LocalizeCumulato))
                {
                    var col = dataTable.Columns[attributo.CodiceOrigine + GanttKeys.LocalizeCumulato];
                    if (col != null)
                        col.ColumnName = attributo.Etichetta;
                }
                if (attributo.Etichetta.Contains(GanttKeys.LocalizeDelta))
                {
                    var col = dataTable.Columns[attributo.CodiceOrigine + GanttKeys.LocalizeDelta];
                    if (col != null)
                        col.ColumnName = attributo.Etichetta;
                }
                if (attributo.Etichetta.Contains(GanttKeys.LocalizeProduttivitaH))
                {
                    var col = dataTable.Columns[attributo.CodiceOrigine + GanttKeys.LocalizeProduttivitaH];
                    if (col != null)
                        col.ColumnName = attributo.Etichetta;
                }

                if (attributo.Etichetta.Contains(GanttKeys.LocalizeValorePercentualeProgressiva))
                {
                    var col = dataTable.Columns[attributo.CodiceOrigine + GanttKeys.LocalizeValorePercentualeProgressiva];
                    if (col != null)
                        col.ColumnName = attributo.Etichetta;
                }
            }

            int TotaleRighe = dataTable.Rows.Count;

            if (checkColumnData)
            {
                if (!View.CheckIfWriteDataColumn())
                {
                    if (listaAttributi.FirstOrDefault() != null)
                    {
                        if (listaAttributi.FirstOrDefault().CodiceOrigine == "Data")
                        {
                            listaAttributi.RemoveAt(0);
                        }
                    }
                }
            }

            InsertRemoveColumn2(worksheet, listaAttributi);

            if (FoundTable != null)
            {
                CellRange rangeToClean = worksheet["A2" + ":" + "XFD1000000"];
                worksheet.ClearContents(rangeToClean);
            }

            string FirstCellTable = "A1";
            string LastCellTable = "A1";

            SpreadSheetCtrl.BeginUpdate();

            LastCellTable = CompileExcelWithValues(worksheet, TotaleRighe, listaAttributi, dictionaryFormule, dataTable);

            SpreadSheetCtrl.EndUpdate();

            CellRange range = worksheet[FirstCellTable + ":" + LastCellTable];

            if (LastCellTable == "A1")
                return;

            SeTableRangeAndFormulaColumn(worksheet, range, dictionaryFormule, TabellaKey);

            RipristinaDataSourcePivot(TabellaKey, range);

            RipristinaDataSourceChart(TabellaKey, range);
        }

        private string CompileExcelWithValues(Worksheet worksheet, int totaleRighe, List<AttibutiFogiloDiCalcoloView> listaAttributi, Dictionary<int, string> dictionaryFormule, DataTable dataTable)
        {
            string lastCellTable = "A1";
            //System.Globalization.CultureInfo currentControlCulture = SpreadSheetCtrl.Options.Culture;
            //string currencySymbol = currentControlCulture.NumberFormat.CurrencySymbol;
            //string lcid = currentControlCulture.LCID.ToString("x");

            for (int i = 0; i < totaleRighe; i++)
            {
                for (int j = 0; j < listaAttributi.Count; j++)
                {
                    string etichetta = listaAttributi.ElementAt(j).Etichetta;

                    DataColumnCollection columns = dataTable.Columns;

                    //Rev by Ale 07/04/2023
                    //string column = ExcelColumnFromNumber(j+1);

                    string column = string.Empty;
                    var table = worksheet.Tables.FirstOrDefault();
                    if (table != null)
                    {
                        var colTable = table?.Columns.FirstOrDefault(item => item.Name == listaAttributi.ElementAt(j).Etichetta);
                        int? colIndex = colTable?.Index;
                        if (colIndex == null)
                            continue;

                        column = ExcelColumnFromNumber(colIndex.Value + 1);
                    }
                    else
                        column = ExcelColumnFromNumber(j + 1);
                    //End Rev



                    //string etichetta = listaAttributi.ElementAt(j).Etichetta;
                    if (columns.Contains(listaAttributi.ElementAt(j).Etichetta))
                    {
                        if (listaAttributi.ElementAt(j).Etichetta.StartsWith(GanttKeys.ConstSAL))
                        {
                            continue;
                        }

                        string valore = dataTable.Rows[i][listaAttributi.ElementAt(j).Etichetta].ToString();
                        int riga = i + 2;
                        Type type = dataTable.Rows[i][listaAttributi.ElementAt(j).Etichetta].GetType();
                        if (type == typeof(string))
                            worksheet.Cells[column + riga].Value = valore;
                        if (type == typeof(Int32) || type == typeof(Int16) || type == typeof(Int64) || type == typeof(double) || type == typeof(decimal))
                        {
                            string attFormat = "#,#00.00";
                            //ricavo il formato della colonna
                            if (listaAttributi.ElementAt(j).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita || listaAttributi.ElementAt(j).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                            {
                                string attCodiceOrigine = listaAttributi.ElementAt(j).CodiceOrigine;
                                AttributoFormatHelper formatHelper = new AttributoFormatHelper(View.DataService);

                                attFormat = formatHelper.GetValoreFormat(listaAttributi.ElementAt(j).EntityTypeKey, attCodiceOrigine);
                                attFormat = FogliDiCalcoloView.ToCellFormat(attFormat);
                            }
                            //

                            worksheet.Cells[column + riga].Formula = "=" + valore;
                            if (listaAttributi.ElementAt(j).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale && !listaAttributi.ElementAt(j).Etichetta.Contains(GanttKeys.LocalizeValorePercentualeProgressiva))
                                worksheet.Cells[column + riga].NumberFormat = attFormat; // "#,#00.00";
                            else if (listaAttributi.ElementAt(j).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita && !listaAttributi.ElementAt(j).Etichetta.Contains(GanttKeys.LocalizeValorePercentualeProgressiva))
                                worksheet.Cells[column + riga].NumberFormat = attFormat; //String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                            else if (listaAttributi.ElementAt(j).Etichetta.Contains(GanttKeys.LocalizeValorePercentualeProgressiva))
                            {
                                worksheet.Cells[column + riga].NumberFormat = "0.00%";
                                worksheet.Cells[column + riga].Formula = "=" + Convert.ToDouble(valore) / 100;
                            }
                            else
                                worksheet.Cells[column + riga].NumberFormat = "#,#00.00";
                        }

                        //if (listaAttributi.ElementAt(j).Etichetta.StartsWith(GanttKeys.ConstSAL))
                        //{
                        //if (View.RibbonView != null)
                        //{
                        //    if (View.RibbonView.SALAttibute?.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                        //        worksheet.Cells[column + riga].NumberFormat = "#,#00.00";
                        //    if (View.RibbonView.SALAttibute?.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        //        worksheet.Cells[column + riga].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                        //}
                        //}

                        if (type == typeof(DateTime))
                        {
                            worksheet.Cells[column + riga].NumberFormat = "dd/mm/yy";
                            worksheet.Cells[column + riga].SetValueFromText(valore, true);
                        }
                        lastCellTable = column + riga;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(listaAttributi.ElementAt(j).Formula))
                        {
                            if (!dictionaryFormule.ContainsKey(j))
                            {
                                dictionaryFormule.Add(j, listaAttributi.ElementAt(j).Formula);
                            }
                            int riga = i + 2;
                            lastCellTable = column + riga;
                        }
                        else
                        {
                            int riga = i + 2;
                            lastCellTable = column + riga;
                        }
                    }
                }
            }
            return lastCellTable;
        }


        private void CompileExcelWithValuesForSched(Worksheet worksheet, List<SALProgrammatoView> valoriWBSPerPeriodo, List<DateTime> dates, List<AttibutiFogiloDiCalcoloView> listaAttributi)
        {
            //System.Globalization.CultureInfo currentControlCulture = SpreadSheetCtrl.Options.Culture;
            //string currencySymbol = currentControlCulture.NumberFormat.CurrencySymbol;
            //string lcid = currentControlCulture.LCID.ToString("x");
            bool changeFillColor = true;
            string formato = "#,#00.00";
            int numeroAttributi = listaAttributi.Count();


            //cellA3.Alignment.Indent = 1; ;
            string column = null;
            int contatoreRiga = 3;
            int contatoreColonna = 0;


            //oss: i gruppi sono intesi come l'insieme dei valoriWBSPerPeriodo da sommare
            foreach (var gruppoGuid in valoriWBSPerPeriodo.GroupBy(v => v.Guid).ToList())
            {
                contatoreColonna = 0;

                column = ExcelColumnFromNumber(contatoreColonna + 1);
                worksheet.Cells[column + contatoreRiga].Value = gruppoGuid.FirstOrDefault().Codice;
                worksheet.Cells[column + contatoreRiga].Alignment.Indent = gruppoGuid.FirstOrDefault().Livello;
                worksheet.Cells[column + contatoreRiga].Alignment.WrapText = true;

                contatoreColonna++;

                column = ExcelColumnFromNumber(contatoreColonna + 1);
                worksheet.Cells[column + contatoreRiga].Value = gruppoGuid.FirstOrDefault().Descrizione;
                worksheet.Cells[column + contatoreRiga].Alignment.Indent = gruppoGuid.FirstOrDefault().Livello;
                worksheet.Cells[column + contatoreRiga].Alignment.WrapText = true;

                contatoreColonna++;


                //CICLO COLONNE ATTRIBUTI REALE E CONTABILITA
                for (int i = 0; i < numeroAttributi - 2; i++)
                {
                    column = ExcelColumnFromNumber(contatoreColonna + 1);
                    if (!gruppoGuid.FirstOrDefault().IsParent)
                    {
                        worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.GetValue(GanttKeys.ColonnaAttributo + (i + 1)));
                        //if (i == 0)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo1);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 1)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo2);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo2).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 2)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo3);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo23).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 3)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo4);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo4).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 4)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo5);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo5).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 5)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo6);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo6).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 6)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo7);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo7).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 7)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo8);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo8).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 8)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo9);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo9).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 9)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo10);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo10).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 10)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo11);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo11).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 11)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo12);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo12).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 12)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo13);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo13).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 13)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo14);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo14).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 14)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo15);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo15).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 15)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo16);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo16).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 16)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo17);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo17).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 17)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo18);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo18).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 18)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo19);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo19).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 19)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo20);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo20).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 20)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo21);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo21).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 21)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo22);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo22).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 22)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo23);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo23).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}
                        //if (i == 23)
                        //{
                        //    worksheet.Cells[column + contatoreRiga].Value = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key).Sum(t => t.ColonnaAttributo24);
                        //    //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo24).ToList().FirstOrDefault().FirstOrDefault().Formato;
                        //}

                    }

                    formato = GetFormato(i, valoriWBSPerPeriodo);

                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                        worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        worksheet.Cells[column + contatoreRiga].NumberFormat = formato;
                    //worksheet.Cells[column + contatoreRiga].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                    //worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                    if (gruppoGuid.FirstOrDefault().IsParent)
                        worksheet.Cells[column + contatoreRiga].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    worksheet.Cells[column + contatoreRiga].FillColor = System.Drawing.Color.WhiteSmoke;
                    contatoreColonna++;
                }



                //CICLO COLONNE ATTRIBUTI REALE E CONTABILITA PER DATE
                int contatoreDate = 1;
                changeFillColor = true;
                foreach (DateTime data in dates)
                {
                    SALProgrammatoView salProgrammatoView = valoriWBSPerPeriodo.Where(v => v.Guid == gruppoGuid.Key && v.Data == data).FirstOrDefault();
                    //if (salProgrammatoView != null)
                    {
                        //TOLGO LE COLONNE FISSE CODICE E DESCRIZIONE
                        for (int i = 0; i < numeroAttributi - 2; i++)
                        {
                            formato = GetFormato(i, valoriWBSPerPeriodo);
                            column = ExcelColumnFromNumber(contatoreColonna + 1);
                            if (salProgrammatoView != null)
                            {
                                worksheet.Cells[column + contatoreRiga].Value = salProgrammatoView.GetValue(GanttKeys.ColonnaAttributo + (i + 1));
                                if (salProgrammatoView.IsParent)
                                    worksheet.Cells[column + contatoreRiga].Font.FontStyle = SpreadsheetFontStyle.Bold;
                            }                             
                            if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                                worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                            if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                                worksheet.Cells[column + contatoreRiga].NumberFormat = formato;
                                //worksheet.Cells[column + contatoreRiga].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                                //worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                                //if (salProgrammatoView.IsParent)
                                //    worksheet.Cells[column + contatoreRiga].Font.FontStyle = SpreadsheetFontStyle.Bold;
                            if (changeFillColor)
                                //worksheet.Cells[column + contatoreRiga].FillColor = System.Drawing.Color.LightSteelBlue;
                                worksheet.Cells[column + contatoreRiga].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);
                            contatoreColonna++;
                        }
                    }

                    if (changeFillColor)
                        changeFillColor = false;
                    else
                        changeFillColor = true;

                    contatoreDate++;
                }

                contatoreRiga++;
            }


            Dictionary<int, double> valoriTotaliPerattributo = new Dictionary<int, double>();
            Dictionary<int, double> valoriProgressiviPerattributo = new Dictionary<int, double>();
            Dictionary<int, double> valoriProgressiviPercentualiPerattributo = new Dictionary<int, double>();
            for (int i = 0; i < numeroAttributi - 2; i++)
            {
                valoriProgressiviPerattributo.Add(i, 0);
                valoriProgressiviPercentualiPerattributo.Add(i, 0);
            }

            contatoreColonna = 0;

            for (int j = 0; j < numeroAttributi; j++)
            {
                column = ExcelColumnFromNumber(contatoreColonna + 1);

                if (j == 0)
                {
                    Borders topBorder = worksheet.Cells[column + contatoreRiga].Borders;
                    topBorder.TopBorder.Color = System.Drawing.Color.Black;
                    contatoreColonna++;
                    continue;
                }

                if (j == 1)
                {
                    worksheet.Cells[column + contatoreRiga].Value = LocalizationProvider.GetString("Totale1");
                    worksheet.Cells[column + contatoreRiga].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    Borders topBorder = worksheet.Cells[column + contatoreRiga].Borders;
                    topBorder.TopBorder.Color = System.Drawing.Color.Black;
                    worksheet.Cells[column + (contatoreRiga + 1)].Value = LocalizationProvider.GetString("TotaleProgressivo");
                    worksheet.Cells[column + (contatoreRiga + 1)].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    worksheet.Cells[column + (contatoreRiga + 2)].Value = LocalizationProvider.GetString("PercPeriodo");
                    worksheet.Cells[column + (contatoreRiga + 2)].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    worksheet.Cells[column + (contatoreRiga + 3)].Value = LocalizationProvider.GetString("PercProgressivo");
                    worksheet.Cells[column + (contatoreRiga + 3)].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    contatoreColonna++;
                    continue;
                }

                if (j == 3)
                {
                    break;
                }


                double totalamount = 0;
                IEnumerable<SALProgrammatoView> valoriPeriodoFoglie = valoriWBSPerPeriodo.Where(v => !v.IsParent);

                for (int i = 0; i < numeroAttributi - 2; i++)
                {
                    if (i == 0)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo1 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 1)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo2 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo2).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 2)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo3 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo3).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 3)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo4 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 4)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo5 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo5).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 5)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo6 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo6).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 6)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo7 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo7).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 7)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo8 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo8).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 8)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo9 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo9).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 9)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo10 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo10).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 10)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo11 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo11).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 11)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo12 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 12)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo13 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo13).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 13)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo14 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo14).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 14)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo15 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo15).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 15)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo16 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo16).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }    
                        
                    if (i == 16)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo17 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo17).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 17)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo18 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo18).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 18)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo19 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo19).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 19)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo20 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo20).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 20)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo21 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo21).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 21)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo22 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo22).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 22)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo23 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo23).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }
                        
                    if (i == 23)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo24 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo24).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    //formato = formato.Replace("{", "").Replace("}","").Replace("0:","");
                    formato = GetFormato(i, valoriWBSPerPeriodo);

                    column = ExcelColumnFromNumber(contatoreColonna + 1);
                    worksheet.Cells[column + contatoreRiga].Value = totalamount;
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                        worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        worksheet.Cells[column + contatoreRiga].NumberFormat = formato;
                    //worksheet.Cells[column + contatoreRiga].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                    //worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                    worksheet.Cells[column + contatoreRiga].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    Borders topBorder = worksheet.Cells[column + contatoreRiga].Borders;
                    topBorder.TopBorder.Color = System.Drawing.Color.Black;
                    worksheet.Cells[column + (contatoreRiga + 1)].Value = totalamount;
                    //worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                        worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = formato;
                    //worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                    worksheet.Cells[column + (contatoreRiga + 1)].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    worksheet.Cells[column + (contatoreRiga + 2)].Value = 1;
                    worksheet.Cells[column + (contatoreRiga + 2)].NumberFormat = "0.00%";
                    worksheet.Cells[column + (contatoreRiga + 3)].Value = 1;
                    worksheet.Cells[column + (contatoreRiga + 3)].NumberFormat = "0.00%";
                    worksheet.Cells[column + contatoreRiga].FillColor = System.Drawing.Color.WhiteSmoke;
                    worksheet.Cells[column + (contatoreRiga + 1)].FillColor = System.Drawing.Color.WhiteSmoke;
                    worksheet.Cells[column + (contatoreRiga + 2)].FillColor = System.Drawing.Color.WhiteSmoke;
                    worksheet.Cells[column + (contatoreRiga + 3)].FillColor = System.Drawing.Color.WhiteSmoke;
                    valoriTotaliPerattributo.Add(i, totalamount);
                    contatoreColonna++;
                }
            }

            //ESCLUDO INIIZIO DA CODICE E DESCRIZIONE
            contatoreColonna = numeroAttributi;

            changeFillColor = true;
            formato = "#,#00.00";

            foreach (DateTime data in dates)
            {
                double totalamount = 0;
                IEnumerable<SALProgrammatoView> valoriPeriodoFoglie = valoriWBSPerPeriodo.Where(v => v.Data == data && !v.IsParent);

                for (int i = 0; i < numeroAttributi - 2; i++)
                {
                    if (i == 0)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo1 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 1)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo2 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo2).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 2)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo3 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo3).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 3)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo4 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 4)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo5 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo5).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 5)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo6 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo6).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 6)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo7 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo7).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 7)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo8 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo8).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 8)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo9 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo9).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 9)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo10 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo10).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 10)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo11 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo11).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 11)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo12 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 12)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo13 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo13).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 13)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo14 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo14).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 14)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo15 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo15).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 15)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo16 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo16).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 16)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo17 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo17).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 17)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo18 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo18).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 18)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo19 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo19).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 19)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo20 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo20).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 20)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo21 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo21).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 21)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo22 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo22).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 22)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo23 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo23).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    if (i == 23)
                    {
                        totalamount = valoriPeriodoFoglie.Sum(t => t.ColonnaAttributo24 ?? 0);
                        //formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo24).ToList().FirstOrDefault().FirstOrDefault().Formato;
                    }

                    formato = GetFormato(i, valoriWBSPerPeriodo);

                    column = ExcelColumnFromNumber(contatoreColonna + 1);
                    worksheet.Cells[column + contatoreRiga].Value = totalamount;
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                        worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        worksheet.Cells[column + contatoreRiga].NumberFormat = formato;
                    //worksheet.Cells[column + contatoreRiga].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);
                    //worksheet.Cells[column + contatoreRiga].NumberFormat = "#,#00.00";
                    worksheet.Cells[column + contatoreRiga].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    Borders topBorder = worksheet.Cells[column + contatoreRiga].Borders;
                    topBorder.TopBorder.Color = System.Drawing.Color.Black;
                    valoriProgressiviPerattributo[i] = valoriProgressiviPerattributo[i] + totalamount;
                    worksheet.Cells[column + (contatoreRiga + 1)].Value = valoriProgressiviPerattributo[i];
                    //worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale)
                        worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = "#,#00.00";
                    if (listaAttributi.ElementAt(2 + i).DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = formato;
                        //worksheet.Cells[column + (contatoreRiga + 1)].NumberFormat = String.Format("#,#00.00 [${0}-{1}]", currencySymbol, lcid);                   
                    worksheet.Cells[column + (contatoreRiga + 1)].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    double percentuale = (1 * totalamount) / valoriTotaliPerattributo[i];
                    if (valoriTotaliPerattributo[i] == 0)
                        worksheet.Cells[column + (contatoreRiga + 2)].Value = "#DIV/0!";
                    else
                        worksheet.Cells[column + (contatoreRiga + 2)].Value = percentuale;
                    if (valoriTotaliPerattributo[i] == 0)
                        percentuale = 0;
                    valoriProgressiviPercentualiPerattributo[i] = valoriProgressiviPercentualiPerattributo[i] + percentuale;
                    worksheet.Cells[column + (contatoreRiga + 2)].NumberFormat = "0.00%";
                    worksheet.Cells[column + (contatoreRiga + 3)].Value = valoriProgressiviPercentualiPerattributo[i];
                    worksheet.Cells[column + (contatoreRiga + 3)].NumberFormat = "0.00%";
                    if (changeFillColor)
                    {
                        worksheet.Cells[column + contatoreRiga].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);
                        worksheet.Cells[column + (contatoreRiga + 1)].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);
                        worksheet.Cells[column + (contatoreRiga + 2)].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);
                        worksheet.Cells[column + (contatoreRiga + 3)].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);
                    }
                    contatoreColonna++;
                }

                if (changeFillColor)
                    changeFillColor = false;
                else
                    changeFillColor = true;
            }
        }

        private string GetFormato(int indice, List<SALProgrammatoView> valoriWBSPerPeriodo)
        {
            string formato = "#,#00.00";
            if (valoriWBSPerPeriodo.Count() > 0)
            {
                if (indice == 0)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo1).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 1)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo2).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo2).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 2)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo3).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo3).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 3)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo4).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo4).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 4)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo5).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo5).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 5)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo6).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo6).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 6)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo7).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo7).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 7)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo8).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo8).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 8)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo9).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo9).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 9)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo10).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo10).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 10)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo11).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo11).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 11)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo12).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo12).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 12)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo13).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo13).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 13)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo14).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo14).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 14)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo15).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo15).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 15)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo16).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo16).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 16)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo17).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo17).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 17)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo18).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo18).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 18)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo19).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo19).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 19)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo20).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo20).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 20)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo21).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo21).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 21)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo22).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo22).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 22)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo23).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo23).ToList().FirstOrDefault().FirstOrDefault().Formato;

                if (indice == 23)
                    if (valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo24).ToList().FirstOrDefault().FirstOrDefault().Formato != null)
                        formato = valoriWBSPerPeriodo.GroupBy(t => t.ColonnaAttributo24).ToList().FirstOrDefault().FirstOrDefault().Formato;
            }
            
            formato = formato.Replace("{", "").Replace("}", "").Replace("0:", "");

            return formato;
        }

        private void SeTableRangeAndFormulaColumn(Worksheet worksheet, CellRange range, Dictionary<int, string> dictionaryFormule, string TabellaKey)
        {
            if (!string.IsNullOrEmpty(TabellaKey))
            {
                if (worksheet.Tables.Where(t => t.Name == TabellaKey).FirstOrDefault() == null)
                {
                    worksheet.Tables.Add(range, true);
                    worksheet.Tables.LastOrDefault().Name = TabellaKey;
                    foreach (KeyValuePair<int, string> formula in dictionaryFormule)
                    {
                        DevExpress.Spreadsheet.TableColumn amountColumn = null;
                        try
                        {
                            var table = worksheet.Tables.LastOrDefault();
                            if (table != null && table.Columns.Count > formula.Key)
                            {
                                amountColumn = table.Columns[formula.Key];
                                amountColumn.Formula = formula.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(LocalizationProvider.GetString("Errore di sintassi della formula") + " : " + amountColumn.Name);
                            //MainAppLog.Error(MethodBase.GetCurrentMethod(), "Errore di sintassi della formula: " + amountColumn.Name);
                        }
                    }
                }
                else
                {
                    worksheet.Tables.Where(t => t.Name == TabellaKey).FirstOrDefault().Range = range;

                    

                    foreach (KeyValuePair<int, string> formula in dictionaryFormule)
                    {
                        DevExpress.Spreadsheet.TableColumn amountColumn = null;
                        try
                        {
                            var table = worksheet.Tables.LastOrDefault();
                            if (table != null && table.Columns.Count > formula.Key)
                            {
                                amountColumn = table.Columns[formula.Key];
                                amountColumn.Formula = formula.Value;
                            }

                            //amountColumn = worksheet.Tables.LastOrDefault().Columns[formula.Key];
                            //amountColumn.Formula = formula.Value;
                        }
                        catch (Exception ex)
                        {

                            MessageBox.Show(LocalizationProvider.GetString("Errore di sintassi della formula") + " : " + amountColumn.Name);
                        }
                    }
                }
            }
        }

        private void AggiornaWBSGantt_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            View.WindowService.ShowWaitCursor(true);

            

            IWorkbook workbook = SpreadSheetCtrl.Document;
            //List<string> ListaNomi = workbook.Worksheets.Where(n => n.Name.StartsWith("WBSGanttData")).Select(n => n.Name).ToList();
            List<string> ListaNomi = workbook.Worksheets.Where(n => n.Name.StartsWith(FogliDiCalcoloKeys.GanttDataSheetBaseName)).Select(n => n.Name).ToList();
            if (ListaNomi.Count() == 0)
            {
                View.WindowService.ShowWaitCursor(false);
                return;
            }


  
            //foreach (Worksheet worksheet in workbook.Worksheets.Where(n => n.Name.StartsWith("WBSGanttData")).ToList())
            foreach (Worksheet worksheet in workbook.Worksheets.Where(n => n.Name.StartsWith(FogliDiCalcoloKeys.GanttDataSheetBaseName)).ToList())
            {
                SezioneKey = BuiltInCodes.EntityType.WBS;
                TabellaKey = "WBSGantt" + "Tabella";
                RadiceKey = "WBSGantt";

                var fogliDiCalcoloData = View.DataService.GetFogliDiCalcoloData();
                var foglioDiCalcoloData = fogliDiCalcoloData.FoglioDiCalcolo.Where(item => item.Foglio == worksheet.Name).FirstOrDefault();//AU
                if (foglioDiCalcoloData != null)
                {
                    TabellaKey = foglioDiCalcoloData.Tabella;
                }
                else
                {
                    int NumberLastSheet = 0;
                    string LastString = worksheet.Name.Substring(worksheet.Name.Length - 2);
                    if (int.TryParse(LastString, out NumberLastSheet))
                    {
                        TabellaKey += LastString;
                        RadiceKey += LastString;
                    }
                }
                GeneraTabellaGantt(worksheet, true);
                RicalculateAllSheet();
            }

            View.WindowService.ShowWaitCursor(false);
        }


        public string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        private void FogliDiCalcoloCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FogliDiCalcoloView)
            {
                FogliDiCalcoloView dataContext = e.NewValue as FogliDiCalcoloView;
                dataContext.SerializedDataChanged += DataContext_SerializedDataChanged;
                dataContext.SerializedDataToChange += DataContext_SerializedDataToChange;

            }

        }

        private void InsertRemoveColumn2(Worksheet worksheet, List<AttibutiFogiloDiCalcoloView> listaAttributi)
        {
            var table = worksheet.Tables.Where(t => t.Name == TabellaKey).FirstOrDefault();
            if (table == null)
            {
                for (int i = 0; i < listaAttributi.Count; i++)
                {
                    string column = ExcelColumnFromNumber(i + 1);
                    worksheet.Cells[column + 1].Value = listaAttributi.ElementAt(i).Etichetta;
                }
            }
            else
            {
                HashSet<string> etichetteAtt = listaAttributi.Select(item => item.Etichetta).ToHashSet();


                int tableColCount = table.Columns.Count;
                for (int i = tableColCount - 1; i >= 1; i--)
                {
                    string name = table.Columns[i].Name;
                    if (!etichetteAtt.Contains(name))
                        table.Columns.RemoveAt(i);
                }

                int position = 0;
                foreach (AttibutiFogiloDiCalcoloView attributo in listaAttributi)
                {
                    if (position == 0)
                    {
                        table.Columns[0].Name = attributo.Etichetta;
                    }
                    else
                    {
                        HashSet<string> colsName = table.Columns.Select(item => item.Name).ToHashSet();

                        if (!colsName.Contains(attributo.Etichetta))
                        {
                            var newCol = table.Columns.Add(position);
                            newCol.Name = attributo.Etichetta;
                        }
                    }
                    position++;
                }


                //position = 0;
                //foreach (AttibutiFogiloDiCalcoloView attributo in listaAttributi)
                //{
                //    table.Columns[position].Name = attributo.Etichetta;
                //    position++;
                //}
            }

        }

        /// <summary>
        /// rifatta da ale (vedi InsertRemoveColumn2)
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="listaAttributi"></param>
        private void InsertRemoveColumn(Worksheet worksheet, List<AttibutiFogiloDiCalcoloView> listaAttributi)
        {
            if (listaAttributi.Count() == 0)
                return;

            var table = worksheet.Tables.Where(t => t.Name == TabellaKey).FirstOrDefault();

            bool firstCreation = false;

            Dictionary<string, int> Headers = new Dictionary<string, int>();
            for (int i = 0; i < 100; i++)
            {
                string column = ExcelColumnFromNumber(i + 1);
                if (worksheet.Cells[column + 1].Value.TextValue == null)
                {
                    if (column == "A")
                    {
                        firstCreation = true;
                    }
                    break;
                }
                Headers.Add(worksheet.Cells[column + 1].Value?.TextValue, i);
            }

            if (table == null)
            {
                for (int i = 0; i < listaAttributi.Count; i++)
                {
                    string column = ExcelColumnFromNumber(i + 1);
                    worksheet.Cells[column + 1].Value = listaAttributi.ElementAt(i).Etichetta;
                }
            }

            int columnDelete = 0;

            foreach (KeyValuePair<string, int> header in Headers)
            {
                AttibutiFogiloDiCalcoloView objectFound = listaAttributi.Where(f => f.Etichetta == header.Key).FirstOrDefault();

                if (objectFound == null)
                {
                    string content = worksheet.Cells[header.Value - columnDelete].Value.TextValue;
                    int nCol = header.Value - columnDelete;
                    Column col = worksheet.Columns[nCol];
                    worksheet.Columns.Remove(header.Value - columnDelete);
                    columnDelete++;
                }
            }

            Headers = new Dictionary<string, int>();
            for (int i = 0; i < 100; i++)
            {
                string column = ExcelColumnFromNumber(i + 1);
                if (worksheet.Cells[column + 1].Value.TextValue == null)
                {
                    break;
                }
                Headers.Add(worksheet.Cells[column + 1].Value.TextValue, i);
            }

            if (table != null)
            {
                CellRange range = worksheet["A1" + ":" + ExcelColumnFromNumber(listaAttributi.Count()) + "2"];
                int contatore = 0;
                int contatoreHeaders = 0;
                foreach (AttibutiFogiloDiCalcoloView attributo in listaAttributi)
                {
                    if (!Headers.ContainsKey(attributo.Etichetta))
                        table.Columns.Add(contatore);
                    else
                        contatoreHeaders++;
                    table.Columns[contatore].Name = attributo.Etichetta;
                    contatore++;
                }
            }
        }

        private void AggiornaFogliDiCalcolo_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            IWorkbook workbook = SpreadSheetCtrl.Document;
            List<string> Lista = workbook.Worksheets.Where(n => !String.IsNullOrEmpty(n.CodeName)).Select(n => n.Name).ToList();
            //RIMUOVO FOGLI PROGRAMMAZIONE SAL PERCHè GESTIONE AGGINORNAMENTO PASSA DIRETTAMENTE DAL GANTT
            int index = -1;
            //int index = Lista.IndexOf("WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Data");
            //if (index != -1)
            //    Lista.RemoveAt(index);
            //index = Lista.IndexOf("WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Data");
            //if (index != -1)
            //    Lista.RemoveAt(index);
            UpdateFoglioDiCalcoloView updateFoglioDiCalcoloView = new UpdateFoglioDiCalcoloView(Lista);
            UpdateFoglioDiCalcoloWnd updateFoglioDiCalcoloWnd = new UpdateFoglioDiCalcoloWnd();
            updateFoglioDiCalcoloWnd.SourceInitialized += (x, y) => updateFoglioDiCalcoloWnd.HideMinimizeAndMaximizeButtons();
            updateFoglioDiCalcoloWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            updateFoglioDiCalcoloWnd.DataContext = updateFoglioDiCalcoloView;
            //(updateFoglioDiCalcoloWnd.DataContext as FoglioDiCalcoloGanttDataView).DataService = View.DataService;
            //(updateFoglioDiCalcoloWnd.DataContext as FoglioDiCalcoloGanttDataView).MainOperation = View.MainOperation;
            //(updateFoglioDiCalcoloWnd.DataContext as FoglioDiCalcoloGanttDataView).WindowService = View.WindowService;
            //(updateFoglioDiCalcoloWnd.DataContext as FoglioDiCalcoloGanttDataView).Init(SezioneKey);
            updateFoglioDiCalcoloWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;



            if (updateFoglioDiCalcoloWnd.ShowDialog() == true)
            {

                List<UpdateView> listaToUpdate = updateFoglioDiCalcoloView.GetFogliToUpdate();

                foreach (UpdateView updateView in listaToUpdate)
                {
                    string codeName = workbook.Worksheets.Where(n => n.Name == updateView.Name).FirstOrDefault().CodeName;
                    UpdateFoglio(codeName);
                }

                //index = Lista.IndexOf("WBSGantt" + FogliDiCalcoloKeys.ConstSAL + "Data");
                index = Lista.IndexOf(FogliDiCalcoloKeys.GanttSALDataSheetBaseName);
                if (index == -1)
                    UpdateFoglio(BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSAL);

                //index = Lista.IndexOf("WBSGantt" + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL + "Data");
                index = Lista.IndexOf(FogliDiCalcoloKeys.GanttProgSALDataSheetBaseName);
                if (index == -1)
                    UpdateFoglio(BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstProg + FogliDiCalcoloKeys.ConstSAL);
            }
        }

        private void SchedulazioneValoriPeriodo_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SezioneKey = null;
            string SheetName = null;
            //if (((DevExpress.Xpf.Bars.BarButtonItem)sender).Name == "WBSGanttSchedData")
            //{
            SezioneKey = BuiltInCodes.EntityType.WBS + FogliDiCalcoloKeys.ConstSched;
            //TabellaKey = "WBSGanttSched" + "Tabella";
            //RadiceKey = "WBSGanttSched";
            //SheetName = "WBSGanttSchedData";
            SheetName = FogliDiCalcoloKeys.GanttSchedDataSheetBaseName;
            //}

            View.FoglioDiCalcoloGanttSchedDataView = null;
            FoglioDiCalcoloWBSGanttSchedDataWnd foglioDiCalcoloWBSGanttSchedDataWnd = new FoglioDiCalcoloWBSGanttSchedDataWnd();
            foglioDiCalcoloWBSGanttSchedDataWnd.SourceInitialized += (x, y) => foglioDiCalcoloWBSGanttSchedDataWnd.HideMinimizeAndMaximizeButtons();
            foglioDiCalcoloWBSGanttSchedDataWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            (foglioDiCalcoloWBSGanttSchedDataWnd.DataContext as FoglioDiCalcoloWBSGanttSchedDataView).DataService = View.DataService;
            (foglioDiCalcoloWBSGanttSchedDataWnd.DataContext as FoglioDiCalcoloWBSGanttSchedDataView).MainOperation = View.MainOperation;
            (foglioDiCalcoloWBSGanttSchedDataWnd.DataContext as FoglioDiCalcoloWBSGanttSchedDataView).WindowService = View.WindowService;
            (foglioDiCalcoloWBSGanttSchedDataWnd.DataContext as FoglioDiCalcoloWBSGanttSchedDataView).Init(SezioneKey);
            View.FoglioDiCalcoloGanttSchedDataView = (foglioDiCalcoloWBSGanttSchedDataWnd.DataContext as FoglioDiCalcoloWBSGanttSchedDataView);
            View.FoglioDiCalcoloGanttSchedDataView.RimuoviAttributiDiversiDaRealiContabilita();
            foglioDiCalcoloWBSGanttSchedDataWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            IWorkbook workbook = SpreadSheetCtrl.Document;



            if (foglioDiCalcoloWBSGanttSchedDataWnd.ShowDialog() == true)
            {   
                View.WindowService.ShowWaitCursor(true);
                
                Worksheet worksheet = workbook.Worksheets.Where(f => f.CodeName == SezioneKey).FirstOrDefault();

                bool isNewWorksheet = false;
                if (worksheet == null)
                {
                    workbook.Worksheets.Add(SheetName);
                    workbook.Worksheets.LastOrDefault().CodeName = SezioneKey;
                    worksheet = workbook.Worksheets.LastOrDefault();
                    isNewWorksheet = true;

                }

                //worksheet.DefaultColumnWidthInPixels = 100;
                GeneraTabellaSched(worksheet, SheetName, isNewWorksheet);

                View.WindowService.ShowWaitCursor(false);
            }
        }

        private void GeneraTabellaSched(Worksheet worksheet, string sheetName, bool isNewWorksheet)
        {
            List<AttibutiFogiloDiCalcoloView> listaAttributi = View.GetColumns(SezioneKey, sheetName);
            List<SALProgrammatoView> ValoriWBSPerPeriodo = View.GeneraDataSourceSched(SezioneKey);
            Dictionary<DateTime, List<ValorePerColonna>> TotaliPerColonnaAttributo = new Dictionary<DateTime, List<ValorePerColonna>>();
            List<DateTime> dates = new List<DateTime>();

            int numeroPeriodi = ValoriWBSPerPeriodo.GroupBy(r => r.Data).Count();
            //dates = ValoriWBSPerPeriodo.GroupBy(r => r.Data).ToList().Select(t => t.Key).ToList();
            dates = ValoriWBSPerPeriodo.OrderBy(d=>d.Data).GroupBy(r => r.Data).ToList().Select(t => t.Key).ToList();
            int numeroAttributi = listaAttributi.Count();
            int numeroAttributiRealiContabilita = numeroAttributi - 2;


            SpreadSheetCtrl.BeginUpdate();

            CellRange rangeToClean = worksheet["A1" + ":" + "XFD1000000"];
            worksheet.ClearFormats(rangeToClean);
            worksheet.ClearContents(rangeToClean);
            //worksheet.Clear(rangeToClean);

            // Split all merged cells in the worksheet.
            foreach (var item in worksheet.Cells.GetMergedRanges())
                item.UnMerge();
           

            for (int i = 0; i < listaAttributi.Count; i++)
            {
                string column = ExcelColumnFromNumber(i + 1);
                worksheet.Cells[column + 2].Value = listaAttributi.ElementAt(i).Etichetta;
                worksheet.Cells[column + 2].Font.FontStyle = SpreadsheetFontStyle.Bold;
                worksheet.Cells[column + 2].Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                worksheet.Cells[column + 2].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                worksheet.Cells[column + 2].Alignment.WrapText = true;
                if (i > 1)
                    worksheet.Cells[column + 2].FillColor = System.Drawing.Color.WhiteSmoke;

                if (isNewWorksheet)
                {
                    if (listaAttributi.ElementAt(i).CodiceOrigine == BuiltInCodes.Attributo.Nome)
                        worksheet.Columns[column].WidthInCharacters = 50;
                    else if (listaAttributi.ElementAt(i).CodiceOrigine == BuiltInCodes.Attributo.Codice)
                        worksheet.Columns[column].WidthInCharacters = 10;
                    else
                        worksheet.Columns[column].WidthInCharacters = 20;
                }
            }

            bool changeFillColor = true;

            int CoulmnAttributeProgressive = numeroAttributi + 1;
            for (int i = 0; i < numeroPeriodi; i++)
            {
                string column = ExcelColumnFromNumber(CoulmnAttributeProgressive);
                worksheet.Cells[column + 1].NumberFormat = View.GetFormatoSchedulazione();
                string ddd = dates.ElementAt(i).ToString();
                worksheet.Cells[column + 1].SetValueFromText(ddd, true);
                worksheet.Cells[column + 1].Font.FontStyle = SpreadsheetFontStyle.Bold;
                worksheet.Cells[column + 1].Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                worksheet.Cells[column + 1].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                worksheet.Cells[column + 1].Alignment.WrapText = true;
                if (changeFillColor)
                    //worksheet.Cells[column + 1].FillColor = System.Drawing.Color.LightSteelBlue;
                    worksheet.Cells[column + 2].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);

                string rangeToMerge = column + 1 + ":" + ExcelColumnFromNumber(CoulmnAttributeProgressive + numeroAttributiRealiContabilita - 1) + 1;
                worksheet.MergeCells(worksheet.Range[rangeToMerge]);

                foreach (var attributo in Enumerable.Reverse(listaAttributi).Take(numeroAttributiRealiContabilita).Reverse().ToList())
                {
                    column = ExcelColumnFromNumber(CoulmnAttributeProgressive);
                    worksheet.Cells[column + 2].Value = attributo.Etichetta;
                    worksheet.Cells[column + 2].Font.FontStyle = SpreadsheetFontStyle.Bold;
                    worksheet.Cells[column + 2].Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                    worksheet.Cells[column + 2].Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    worksheet.Cells[column + 2].Alignment.WrapText = true;
                    if (changeFillColor)
                        //worksheet.Cells[column + 2].FillColor = System.Drawing.Color.LightSteelBlue;
                        worksheet.Cells[column + 2].FillColor = System.Drawing.Color.FromArgb(188, 221, 238);
                    CoulmnAttributeProgressive++;

                    if (isNewWorksheet)
                        worksheet.Columns[column].WidthInCharacters = 20;

                }

                if (changeFillColor)
                    changeFillColor = false;
                else
                    changeFillColor = true;

                if (isNewWorksheet)
                    worksheet.Columns[column].WidthInCharacters = 20;

            }

            CompileExcelWithValuesForSched(worksheet, ValoriWBSPerPeriodo, dates, listaAttributi);

            SpreadSheetCtrl.EndUpdate();
        }

        private void DataContext_SerializedDataToChange(object sender, EventArgs e)
        {
            try
            {
                byte[] docBytes = SpreadSheetCtrl.Document.SaveDocument(DocumentFormat.Xlsx);
                var fogliData = View.DataService.GetFogliDiCalcoloData();
                fogliData.SerializedData = docBytes;

                //Purge dei fogli non più esistenti
                HashSet<string> sheetsName = SpreadSheetCtrl.Document.Sheets.Select(item => item.Name).ToHashSet();
                if (fogliData.FoglioDiCalcolo != null)
                    fogliData.FoglioDiCalcolo.RemoveAll(item => !sheetsName.Contains(item.Foglio));


                View.DataService.SetFogliDiCalcoloData(fogliData);
            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "Errore nella serializzazione dei fogli di calcolo", ex);
                //MessageBox.Show(ex.Message);

            }

        }

        private void DataContext_SerializedDataChanged(object sender, EventArgs e)
        {
            var fogliData = View.DataService.GetFogliDiCalcoloData();
            byte[] docBytes = fogliData.SerializedData;
            if (docBytes != null)
            {
                //System.IO.File.WriteAllBytes("C:\\Users\\alberto.cantele\\Desktop\\Test.xlsx", docBytes);
                SpreadSheetCtrl.Document.LoadDocument(docBytes);
            }
            else
            {
                IWorkbook workbook = SpreadSheetCtrl.Document;
                int NumeroFogli = workbook.Worksheets.Count();
                if (NumeroFogli == 1 && workbook.Worksheets.LastOrDefault().Name.EndsWith("1"))
                {
                    CellRange range = workbook.Worksheets.LastOrDefault()["A2:XFD1000000"];
                    workbook.Worksheets.LastOrDefault().Clear(range);
                    SpreadSheetCtrl.Modified = false;
                    return;
                }
                for (int i = 0; i < NumeroFogli; i++)
                {
                    if (workbook.Worksheets.Count() == 1)
                    {
                        workbook.Worksheets.Add();
                        workbook.Worksheets.RemoveAt(0);
                        workbook.Worksheets.LastOrDefault().Name = "Foglio1";
                        break;
                    }
                    else
                    {
                        workbook.Worksheets.RemoveAt(0);
                    }

                }
                int ChartCounter = workbook.ChartSheets.Count();
                for (int i = 0; i < ChartCounter; i++)
                {
                    workbook.ChartSheets.RemoveAt(0);
                }
                SpreadSheetCtrl.Modified = false;
            }
        }

        private void EvidenziaInModel3dFogliDiCalcolo_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var worksheet = SpreadSheetCtrl.ActiveWorksheet;
            var selection = SpreadSheetCtrl.GetSelectedRanges();


            List<Model3dObjectKey> model3dObjKeys = new List<Model3dObjectKey>();
            foreach (CellRange range1 in selection)
            {
                for (int row = range1.TopRowIndex; row <= range1.BottomRowIndex; row++)
                {
                    for (int column = range1.LeftColumnIndex; column <= range1.RightColumnIndex; column++)
                    {
                        var cell = worksheet.Cells[row, column];
                        string cellValue = cell?.Value?.ToString();

                        model3dObjKeys.Add(new Model3dObjectKey()
                        {
                            ProjectGlobalId = string.Empty,
                            GlobalId = cellValue,
                        }); ;

                        
                    }
                }
            }
            
            View.Model3dService?.SelectElements(model3dObjKeys);
        }
    }
}
