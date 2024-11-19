using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JReport;
using FastReport;
using FastReport.Utils;
using Commons.View;
using System.Drawing;
using System.Runtime.Serialization;
using FastReport.Data;
using FastReport.Format;
using Microsoft.SqlServer.Server;
using System.Data;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Windows.Controls;
using Syncfusion.Windows.Shared;
using System.Windows.Forms;
using StampeWpf.Report;
using Commons;
using Newtonsoft.Json;
using CommonResources;

namespace StampeWpf
{
    public class ReportStructureGenerator
    {
        public Model.StampeData ReportSetting { get; set; }

        public JReport.Report ReportObject;

        private Int64 ContatorePagina = 1;

        private bool FromDocumento;

        public ReportStructureGenerator(StampeData reportSetting, bool fromDocumento, int initialNumberPage = 1)
        {
            ReportObject = new JReport.Report();
            ReportObject.InitialPageNumber = initialNumberPage;
            //ReportObject.DoublePass = true;
            ReportSetting = reportSetting;
            FromDocumento = fromDocumento;

            // per gestione riche alternate
            SolidFill SolidFill = new SolidFill();
            SolidFill.Color = Color.WhiteSmoke;
            Style RigheAlternateStyle = new Style();
            RigheAlternateStyle.Name = "RigheAlternate";
            RigheAlternateStyle.Fill = SolidFill;
            ReportObject.Styles.Add(RigheAlternateStyle);

            //FastReport.Border Border = new FastReport.Border();
            //Border.Lines = BorderLines.Left | BorderLines.Right;
            //Style RigheBordateAiLati = new Style();
            //RigheBordateAiLati.Name = "RigheBordateAiLati";
            //RigheBordateAiLati.Border = Border;
            //ReportObject.Styles.Add(RigheBordateAiLati);
        }

        public void CreateAndAddNewPage(DataSet dataSet, string dbName, List<ParenEntity> listParentItem, string sezioneKey, bool IsTreeMaster, IMainOperation MainOperation, int contatoreReport, string Watermark, bool Bordatura, bool printOnPreviousPage = false, ClientDataService DataService = null)
        {
            //string json = JsonConvert.SerializeObject(dataSet, Formatting.Indented);

            if (sezioneKey == "Gantt" || sezioneKey == LocalizationProvider.GetString("FogliDiCalcolo"))
            {
                if (ReportSetting.ImagesForPage !=null)
                {
                    foreach (var Gantt in ReportSetting.ImagesForPage)
                    {
                        PageStructureGenerator PageGenerator = new PageStructureGenerator(DataService, MainOperation, ReportSetting, ReportObject, ContatorePagina);
                        //if (ContatorePagina == 1)
                            PageGenerator.Page.PrintOnPreviousPage = printOnPreviousPage;
                        PageGenerator.CreateGanttPageStructure(Gantt, FromDocumento);
                        PageGenerator.IndexDb = (int)(contatoreReport * 100 + ContatorePagina);

                        ReportObject.Pages.Add(PageGenerator.Page);
                        ContatorePagina++;
                    }
                }
            }
            else
            {
                ReportObject.RegisterData(dataSet.Tables[dbName], dbName);
                ReportObject.GetDataSource(dbName).Enabled = true;
                //TableDataSource ds = ReportObject.GetDataSource(dbName) as TableDataSource;
                //ds.StoreData = true;

                PageStructureGenerator PageGenerator = new PageStructureGenerator(DataService, MainOperation, ReportSetting, ReportObject, ContatorePagina);
                PageGenerator.Page.PrintOnPreviousPage = printOnPreviousPage;
                PageGenerator.IndexDb = contatoreReport;
                PageGenerator.DbName = dbName;

                //if (dataSet.Tables[0].Columns[0].ColumnName != ReportDataSourceGenerator.RTFColumnId)
                if (dataSet.Tables[0].Columns[0].ColumnName != StampeKeys.ConstRTFColumnId)
                {
                    PageGenerator.ListParentItem = listParentItem;
                    PageGenerator.Sezione = sezioneKey;
                    PageGenerator.IsTreeMaster = IsTreeMaster;
                    PageGenerator.Bordatura = Bordatura;
                    PageGenerator.CreateDatiPageStructure(FromDocumento);
                }
                else
                {
                    PageGenerator.CreateRtfPageStructure(FromDocumento);
                }

                if (!string.IsNullOrEmpty(Watermark))
                {
                    Watermark watermark = new Watermark();
                    watermark.Enabled = true;
                    watermark.Text = Watermark;
                    PageGenerator.Page.Watermark = watermark;
                }

                ReportObject.Pages.Add(PageGenerator.Page);

                ContatorePagina++;
            }
        }
    }
}
