using Commons;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class GanttGenerator : NotificationBase
    {
        public ClientDataService DataService { get; set; } = null;
        public GanttView GanttView { get; set; }

        private DateTime _DataDa;
        public DateTime DataDa
        {
            get
            {

                return _DataDa;
            }
            set
            {
                SetProperty(ref _DataDa, value);
            }
        }

        private DateTime _DataA;
        public DateTime DataA
        {
            get
            {

                return _DataA;
            }
            set
            {
                SetProperty(ref _DataA, value);
            }
        }

        public bool ZoomActive { get; set; }
        public bool IsIn { get; set; }
        public TimeSpan ZoomStored { get; set; }

        private DevExpress.Xpf.Printing.PrintableControlLink PreviewResult;
        //public DevExpress.Xpf.Printing.PrintableControlLink PreviewResult
        //{
        //    get
        //    {
        //        return _PreviewResult;
        //    }
        //    set
        //    {
        //        SetProperty(ref _PreviewResult, value);
        //    }
        //}
        public GanttGenerator()
        {

        }
        public List<ImageForPage> GenerateGantt(string Dimensioni, string Orientamento, string MargineSuperiore, string MargineInferiore, string MargineSinistro, string MargineDestro, StampeData reportSetting)
        {

            GanttCtrl GanttCtrl = new GanttCtrl();
            //GanttCtrl.DataContext = GanttView;
            GanttView ganttview = new GanttView();
            GanttCtrl.DataContext = ganttview;
            GeneraGanttView(ganttview, GanttView);

            DateTime DataInizio = new DateTime();
            DateTime DataFine = new DateTime();
            TimeSpan Zoom = new TimeSpan();

            //if (GanttView.GanttData == null || GanttView.GanttData.ColumnWidth == null)
            if (reportSetting.GanttSetting != null)
            {
                //if (GanttView.GanttData == null || GanttView.GanttData.ColumnWidth == null)
                if (reportSetting.GanttSetting.ColumnWidth == null)
                {
                    ganttview.WidthCode = 70;
                    ganttview.WidthDescription = 300;
                    ganttview.WidthDurata = 70;
                    ganttview.WidthDurataCalendario = 70;
                    ganttview.WidthStartDate = 70;
                    ganttview.WidthFinishDate = 70;
                }
                else
                {
                    for (int i = 0; i < reportSetting.GanttSetting.ColumnWidth.Count(); i++)
                    {
                        if (i == 0)
                            ganttview.WidthCode = reportSetting.GanttSetting.ColumnWidth[i];
                        if (i == 1)
                            ganttview.WidthDescription = reportSetting.GanttSetting.ColumnWidth[i];
                        if (i == 2)
                            ganttview.WidthDurata = reportSetting.GanttSetting.ColumnWidth[i];
                        if (i == 3)
                            ganttview.WidthDurataCalendario = reportSetting.GanttSetting.ColumnWidth[i];
                        if (i == 4)
                            ganttview.WidthStartDate = reportSetting.GanttSetting.ColumnWidth[i];
                        if (i == 5)
                            ganttview.WidthFinishDate = reportSetting.GanttSetting.ColumnWidth[i];
                    }
                }

                //DataInizio = reportSetting.GanttSetting.GanttDateFrom;
                DataInizio = reportSetting.GanttSetting.GanttDateFrom;
                DataFine = reportSetting.GanttSetting.GanttDateTo;
                Zoom = reportSetting.GanttSetting.GanttZoom;
            }

            GanttCtrl.Width = 0;
            GanttCtrl.Height = 0;
            GanttCtrl.WindowStyle = System.Windows.WindowStyle.None;
            GanttCtrl.ShowInTaskbar = false;
            GanttCtrl.ShowActivated = false;
            GanttCtrl.Show();
            GanttCtrl.Hide();

            AttivitaWpf.View.WBSVicibleEventArgs WBSVicibleEventArgs = ganttview.GetRamiApertiChiusi();
            //WBSVicibleEventArgs.IndexWBSToCollapse = new List<Guid>();
            //WBSVicibleEventArgs.IndexWBSToExpande = new List<Guid>();

            //if (GanttView.GanttData.GuidToOpenOrClose != null)
            //{
            //    foreach (var item in GanttView.GanttData.GuidToOpenOrClose.IndexWBSToCollapse)
            //        WBSVicibleEventArgs.IndexWBSToCollapse.Add(item);
            //    foreach (var item in GanttView.GanttData.GuidToOpenOrClose.IndexWBSToExpande)
            //        WBSVicibleEventArgs.IndexWBSToExpande.Add(item);
            //}

            foreach (var Index in WBSVicibleEventArgs.IndexWBSToCollapse)
            {
                DevExpress.Xpf.Grid.TreeListNode treeListNode = (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).GetNodeByKeyValue(Index);
                //(GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).CollapseNode(Index);
                if (treeListNode != null)
                    (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).CollapseNode(treeListNode.RowHandle);
            }
            foreach (var Index in WBSVicibleEventArgs.IndexWBSToExpande)
            {
                DevExpress.Xpf.Grid.TreeListNode treeListNode = (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).GetNodeByKeyValue(Index);
                //(GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).ExpandNode(Index);
                if (treeListNode != null)
                    (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).ExpandNode(treeListNode.RowHandle);
            }

            //DateTime DataInizio = GanttView.GanttData.GanttDateFrom;
            //DateTime DataFine = GanttView.GanttData.GanttDateTo;
            //TimeSpan Zoom = GanttView.GanttData.GanttZoom;

            if (DataInizio == new DateTime())
                DataInizio = GanttView.WBSView.GetMinDataInizioWBSItems(true);
            if (DataFine == new DateTime())
                DataFine = GanttView.WBSView.GetMaxDataFineWBSItems(true).AddDays(1);

            if (DataInizio  == DataFine)
            {
                DataInizio = GanttView.WBSView.GetMinDataInizioWBSItems(true);
                DataFine = GanttView.WBSView.GetMaxDataFineWBSItems(true);
            }

            DataFine = DataFine.AddDays(1);

            if (ganttview.IsActiveProgressiva)
                GanttCtrl.Gantt.View.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(GanttView.GetDataAnonima(DataInizio), GanttView.GetDataAnonima(DataFine,true));
            else
                GanttCtrl.Gantt.View.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataInizio, DataFine);

            if (Zoom != new TimeSpan())
                GanttCtrl.Gantt.View.Zoom = Zoom;

            //GanttView.TableView.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataDa, DataA);

            PreviewResult = new DevExpress.Xpf.Printing.PrintableControlLink(GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView);
            //link = new DevExpress.Xpf.Printing.PrintableControlLink(GanttView.TableView as DevExpress.Xpf.Grid.TreeListView);

            //PreviewResult.Margins = new System.Drawing.Printing.Margins(Int32.Parse(MargineSinistro), Int32.Parse(MargineDestro), Int32.Parse(MargineSuperiore), Int32.Parse(MargineInferiore));
            PreviewResult.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);

            if (Orientamento == "0")
                PreviewResult.Landscape = false;
            else
                PreviewResult.Landscape = true;

            //PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.Custom;
            PreviewResult.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.Custom;

            if (Orientamento == "0")//portrait
            {
                if (Dimensioni.Contains("A3")) { PreviewResult.CustomPaperSize = new System.Drawing.Size(39 * 40,29 * 40); };
                if (Dimensioni.Contains("A4")) { PreviewResult.CustomPaperSize = new System.Drawing.Size(26 * 40,21 * 40); };
                if (Dimensioni.Contains("A5")) { PreviewResult.CustomPaperSize = new System.Drawing.Size(18 * 40,14 * 40); };
            }
            else //landscape
            {
                if (Dimensioni.Contains("A3")) { PreviewResult.CustomPaperSize = new System.Drawing.Size(27 * 40, 42 * 40); };
                if (Dimensioni.Contains("A4")) { PreviewResult.CustomPaperSize = new System.Drawing.Size(18 * 40, 29 * 40); };
                if (Dimensioni.Contains("A5")) { PreviewResult.CustomPaperSize = new System.Drawing.Size(11 * 40, 21 * 40); };
            }


            //if (Dimensioni.Contains("A3")) { PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.A3; }
            //if (Dimensioni.Contains("A4")) { PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.A4; }
            //if (Dimensioni.Contains("A5")) { PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.A5; }

            if (reportSetting.GanttSetting != null)
            {
                if (reportSetting.GanttSetting.ZoomFactor == 0 && reportSetting.GanttSetting.AdjustToPage == 0)
                {
                    reportSetting.GanttSetting.ZoomFactor = 1;
                }
                if (reportSetting.GanttSetting.ZoomFactor == 0)
                    PreviewResult.PrintingSystem.Document.AutoFitToPagesWidth = reportSetting.GanttSetting.AdjustToPage;
                if (reportSetting.GanttSetting.AdjustToPage == 0)
                    PreviewResult.PrintingSystem.Document.ScaleFactor = (float)reportSetting.GanttSetting.ZoomFactor;
            }


            //PreviewResult.PrintingSystem.Document.AutoFitToPagesWidth = 1;
            PreviewResult.CreateDocument();

            GanttCtrl.Close();

            // GENERAZIONE IMMAGINE PER PAGINA
            //List<string> PagineGantt = new List<string>();
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
                        PageBorderWidth = 0
                    });
                    //using (FileStream file = new FileStream("C:\\Users\\alberto.cantele\\Desktop\\Image" + i.ToString() + ".png", FileMode.Create, FileAccess.Write))
                    //    ms.WriteTo(file);
                    //PagineGantt.Add(Convert.ToBase64String(ms.ToArray()));
                    PagineGantt.Add(new ImageForPage() { Image = Convert.ToBase64String(ms.ToArray()), Size = new System.Drawing.SizeF() });
                    //PagineGantt.Add(Encoding.Default.GetString(ms.ToArray()));
                }
            }

            return PagineGantt;

            //System.IO.MemoryStream ExportedGantt = new System.IO.MemoryStream();
            //DevExpress.XtraPrinting.ImageExportOptions exportOption = new DevExpress.XtraPrinting.ImageExportOptions();
            //exportOption.ExportMode = DevExpress.XtraPrinting.ImageExportMode.SingleFile;
            //exportOption.Format = System.Drawing.Imaging.ImageFormat.Emf;
            //PreviewResult.ExportToImage(ExportedGantt, exportOption);
            //return Encoding.Default.GetString(ExportedGantt.ToArray());
        }

        public DevExpress.Xpf.Printing.PrintableControlLink GetPreviewResult(DateTime DataInizio, DateTime DataFine, TimeSpan Zoom, List<int> ColumnWidth, double ZoomFactor, int AdjustToPage)
        {
            GanttCtrl GanttCtrl = new GanttCtrl();

            try
            {

                
                //GanttCtrl.DataContext = GanttView;
                GanttView ganttview = new GanttView();
                GanttCtrl.DataContext = ganttview;
                GeneraGanttView(ganttview, GanttView);

                if (ColumnWidth != null)
                {
                    for (int i = 0; i < ColumnWidth.Count(); i++)
                    {
                        if (i == 0)
                            ganttview.WidthCode = ColumnWidth[i];
                        if (i == 1)
                            ganttview.WidthDescription = ColumnWidth[i];
                        if (i == 2)
                            ganttview.WidthDurata = ColumnWidth[i];
                        if (i == 3)
                            ganttview.WidthDurataCalendario = ColumnWidth[i];
                        if (i == 4)
                            ganttview.WidthStartDate = ColumnWidth[i];
                        if (i == 5)
                            ganttview.WidthFinishDate = ColumnWidth[i];
                    }
                }
                else
                {
                    ganttview.WidthCode = 70;
                    ganttview.WidthDescription = 300;
                    ganttview.WidthDurata = 70;
                    ganttview.WidthDurataCalendario = 70;
                    ganttview.WidthStartDate = 70;
                    ganttview.WidthFinishDate = 70;
                }


                GanttCtrl.Width = 0;
                GanttCtrl.Height = 0;
                GanttCtrl.WindowStyle = System.Windows.WindowStyle.None;
                GanttCtrl.ShowInTaskbar = false;
                GanttCtrl.ShowActivated = false;
                GanttCtrl.Show();
                GanttCtrl.Hide();

                AttivitaWpf.View.WBSVicibleEventArgs WBSVicibleEventArgs = GanttView.GetRamiApertiChiusi();
                foreach (var Index in WBSVicibleEventArgs.IndexWBSToCollapse)
                {
                    DevExpress.Xpf.Grid.TreeListNode treeListNode = (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).GetNodeByKeyValue(Index);
                    //(GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).CollapseNode(Index);
                    if (treeListNode != null)
                        (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).CollapseNode(treeListNode.RowHandle);
                }
                foreach (var Index in WBSVicibleEventArgs.IndexWBSToExpande)
                {
                    DevExpress.Xpf.Grid.TreeListNode treeListNode = (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).GetNodeByKeyValue(Index);
                    //(GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).ExpandNode(Index);
                    if (treeListNode != null)
                        (GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView).ExpandNode(treeListNode.RowHandle);
                }

                if (Zoom != new TimeSpan())
                    GanttCtrl.Gantt.View.Zoom = Zoom;

                //DateTime DataInizio = GanttView.GanttData.GanttDateFrom;
                //DateTime DataFine = GanttView.GanttData.GanttDateTo;

                //if (DataInizio == new DateTime())
                //    DataInizio = GanttView.WBSView.GetMinDataInizioWBSItems(true);
                //if (DataFine == new DateTime())
                //    DataFine = GanttView.WBSView.GetMaxDataFineWBSItems(true);

                //if (ganttview.IsActiveProgressiva)
                //    GanttCtrl.Gantt.View.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataInizio, DataFine);
                //else
                //    GanttCtrl.Gantt.View.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataInizio, DataFine);

                if (ZoomActive)
                {
                    if (IsIn)
                        GanttCtrl.Gantt.View.ZoomIn();
                    else
                        GanttCtrl.Gantt.View.ZoomOut();
                }

                GanttCtrl.Gantt.View.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataInizio, DataFine);


                //GanttView.TableView.PrintDateRange = new DevExpress.Mvvm.DateTimeRange(DataDa, DataA);

                PreviewResult = new DevExpress.Xpf.Printing.PrintableControlLink(GanttCtrl.Gantt.View as DevExpress.Xpf.Grid.TreeListView);
                //link = new DevExpress.Xpf.Printing.PrintableControlLink(GanttView.TableView as DevExpress.Xpf.Grid.TreeListView);
                PreviewResult.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);
                PreviewResult.Landscape = true;
                //PreviewResult.PaperKind = System.Drawing.Printing.PaperKind.A3;
                PreviewResult.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A3;

                if (ZoomFactor == 0)
                    PreviewResult.PrintingSystem.Document.AutoFitToPagesWidth = AdjustToPage;
                if (AdjustToPage == 0)
                    PreviewResult.PrintingSystem.Document.ScaleFactor = (float)ZoomFactor;
                //PreviewResult.PrintingSystem.Document.AutoFitToPagesWidth = 1;
                PreviewResult.CreateDocument();

                ZoomStored = GanttCtrl.Gantt.View.Zoom;
                ZoomActive = false;
                IsIn = false;

                GanttCtrl.Close();

            }
            catch (Exception ex)
            {
                ZoomStored = GanttCtrl.Gantt.View.Zoom;
                ZoomActive = false;
                IsIn = false;

                GanttCtrl.Close();

                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
                return null;
            }

            return PreviewResult;
        }

        public void GeneraGanttView(GanttView ganttview, GanttView GanttViewOld)
        {
            try
            {

                ganttview.DataService = DataService;
                ganttview.WBSView = GanttViewOld.WBSView;
                ganttview.Init();
                ganttview.IsActiveCalendario = GanttViewOld.IsActiveCalendario;
                ganttview.IsActiveNascondiDate = GanttViewOld.IsActiveNascondiDate;
                ganttview.IsActiveProgressiva = GanttViewOld.IsActiveProgressiva;
                ganttview.IsActiveCriticalPath = GanttViewOld.IsActiveCriticalPath;
                ganttview.IsBarreDiRiepilogoChecked = GanttViewOld.IsBarreDiRiepilogoChecked;
                ganttview.SetScalaCronologica();
                ganttview.TimescaleRulerCount = GanttViewOld.TimescaleRulerCount;
                ganttview.ShowSALTglBtn_Checked = GanttViewOld.ShowSALTglBtn_Checked;

            }
            catch (Exception ex) 
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }
        }
    }
}
