using Commons;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using MasterDetailView;
using Microsoft.Win32;
using Model;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailWpf
{
    public class MasterDetailGridExcelUtility
    {
        public SfDataGrid Grid { get; set; } = null;
        public MasterDetailGridView View { get; set; } = null;


        string FileExtension { get => "xlsx"; }
        Dictionary<Guid, Entity> _entities = new Dictionary<Guid, Entity>();


        public void Export()
        {
            if (Grid == null)
                return;

            if (View == null)
                return;

            _entities = View.ItemsView.DataService.GetEntitiesById(View.ItemsView.EntityType.Codice, View.ItemsView.FilteredEntitiesId)
                        .ToDictionary(item => item.EntityId, item => item);
            

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = FileExtension;
            saveFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", FileExtension, FileExtension, FileExtension);
            if (saveFileDialog.ShowDialog() == true)
            {

                try
                {
                    ExcelExportingOptions options = new ExcelExportingOptions();
                    
                    options.CellsExportingEventHandler = CellExportingHandler;
                    //options.ExportingEventHandler = ExportingHandler;
                    //options.ExcelVersion = ExcelVersion.Xlsx;
                    options.ExcludeColumns.Add("Data.Icons");
                    options.AllowOutlining = false;
                    ExcelEngine excelEngine = Grid.ExportToExcel(Grid.View, options);
                    IWorkbook workBook = excelEngine.Excel.Workbooks[0];
                    //workBook.Worksheets[0].AutoFilters.FilterRange = workBook.Worksheets[0].UsedRange;
                    DeleteBlankRows(workBook);

                    workBook.SaveAs(saveFileDialog.FileName);


                    //Apertura del file
                    //System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = saveFileDialog.FileName;
                    process.Start();

                }
                catch (IOException exc)
                {
                    View.MainOperation.ShowMessageBarView(exc.Message);
                }
            }
        }

        private void DeleteBlankRows(IWorkbook workBook)
        {
            IWorksheet sheet = workBook.Worksheets[0];

            //raccolgo gli indici delle riche bianche
            List<int> blankRowsIndex = new List<int>();
            foreach (IRange row in sheet.Rows)
            {
                if (row.IsBlank)
                    blankRowsIndex.Add(row.Row);
            }

            //cancello le righe raccolte
            blankRowsIndex.Reverse();
            foreach (int i in blankRowsIndex)
            {
                sheet.DeleteRow(i);
            }
            
        }

        private void CellExportingHandler(object sender, GridCellExcelExportingEventArgs e)
        {

            // Based on the column mapping name and the cell type, we can change the cell 
            //values while exporting to excel.
            if (e.CellType == ExportCellType.RecordCell)
            {
                VirtualListItem<EntityView> entViewVirt = e.NodeEntry as VirtualListItem<EntityView>;
                int index = entViewVirt.Index;
                if (index < 0 || index >= View.ItemsView.FilteredEntitiesId.Count)
                    return;

                Guid entityId = View.ItemsView.FilteredEntitiesId[index];

                Entity ent = null;
                if (entityId != Guid.Empty)
                    ent = _entities[entityId];

                string numberFormat = string.Empty;
                e.Range.Cells[0].Value = GetCellValue(ent, e.ColumnName, ref numberFormat);
                if (numberFormat.Any())
                    e.Range.Cells[0].NumberFormat = numberFormat;
                e.Handled = true;
            }
        }

        //private string GetCellDisplayValue(Entity ent, string propertyName)
        //{
        //    try
        //    { 
        //        int index = -1;
        //        index = Convert.ToInt32(propertyName.Remove(0, EntityView.AttributoMappingBaseName.Length));


        //        if (index >= View.ItemsView.EntityType.AttributiMasterCodes.Count)
        //            return null;

        //        EntityAttributo entAtt = ent.Attributi[ent.EntityType.AttributiMasterCodes[index]];
        //        Attributo att = entAtt.Attributo;

        //        Valore val = null;
        //        if (att.ValoreDefault is ValoreTestoRtf)
        //            val = View.ItemsView.EntitiesHelper.GetValoreAttributo(ent, View.ItemsView.EntityType.AttributiMasterCodes[index], true, true);
        //        else
        //            val = View.ItemsView.EntitiesHelper.GetValoreAttributo(ent, View.ItemsView.EntityType.AttributiMasterCodes[index], false, true);


        //        if (val is ValoreData)
        //            return (val as ValoreData).V.ToString();
        //        if (val is ValoreTesto)
        //            return (val as ValoreTesto).V;
        //        if (val is ValoreContabilita)
        //        {
        //            string format = View.ItemsView.AttributoFormatHelper.GetValorePaddedFormat(entAtt);
        //            return (val as ValoreContabilita).FormatRealResult(format);
        //        }
        //        if (val is ValoreReale)
        //        {
        //            string format = View.ItemsView.AttributoFormatHelper.GetValorePaddedFormat(entAtt);
        //            string res = (val as ValoreReale).FormatRealResult(format);
        //            return res;
        //        }
        //        if (val is ValoreTestoRtf)
        //            return (val as ValoreTestoRtf).BriefPlainText;

        //    }
        //    catch (Exception exc)
        //    {
        //        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
        //    }

        //    return string.Empty;
        //}

        private string GetCellValue(Entity ent, string propertyName, ref string numberFormat)
        {
            try
            {
                //int index = -1;
                //index = Convert.ToInt32(propertyName.Remove(0, EntityView.AttributoMappingBaseName.Length));

                //if (index >= View.ItemsView.EntityType.AttributiMasterCodes.Count)
                //    return null;

                //EntityAttributo entAtt = ent.Attributi[ent.EntityType.AttributiMasterCodes[index]];
                //Attributo att = entAtt.Attributo;

                //      Valore val = null;
                //if (att.ValoreDefault is ValoreTestoRtf)
                //    val = View.ItemsView.EntitiesHelper.GetValoreAttributo(ent, View.ItemsView.EntityType.AttributiMasterCodes[index], true, true);
                //else
                //    val = View.ItemsView.EntitiesHelper.GetValoreAttributo(ent, View.ItemsView.EntityType.AttributiMasterCodes[index], false, true);

                string codiceAtt = View.ItemsView.GetCodiceAttributoByMappingName(propertyName);

                
                //EntityAttributo entAtt = ent.Attributi[codiceAtt];
                //Attributo att = entAtt.Attributo;
                Attributo att = ent.EntityType?.Attributi[codiceAtt];

                Valore val = null;
                if (att.ValoreDefault is ValoreTestoRtf)
                    val = View.ItemsView.EntitiesHelper.GetValoreAttributo(ent, codiceAtt, true, true);
                else
                    val = View.ItemsView.EntitiesHelper.GetValoreAttributo(ent, codiceAtt, false, true);


                if (val is ValoreData)
                {
                    string res = val.ToPlainText();
                    return res;
                }
                if (val is ValoreTesto)
                    return (val as ValoreTesto).Result;
                if (val is ValoreContabilita)
                {
                    //numberFormat = View.ItemsView.AttributoFormatHelper.GetValoreFormat(entAtt);
                    string res = (val as ValoreContabilita).RealResult.ToString();
                    return res;
                }
                if (val is ValoreReale)
                {
                    
                    string res = (val as ValoreReale).RealResult.ToString();
                    return res;
                }
                if (val is ValoreTestoRtf)
                    return (val as ValoreTestoRtf).BriefPlainText;
                if (val is ValoreElenco)
                    return (val as ValoreElenco).V;
                if (val is ValoreColore)
                    return (val as ValoreColore).V;
                if (val is ValoreBooleano)
                    return (val as ValoreBooleano).V.ToString();
                if (val is ValoreFormatoNumero)
                    return (val as ValoreFormatoNumero).V;

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }

            return string.Empty;
        }
    }
}
