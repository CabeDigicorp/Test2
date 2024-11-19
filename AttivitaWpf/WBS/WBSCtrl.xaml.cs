using AttivitaWpf.ImportExport;
using AttivitaWpf.View;
using CommonResources;
using Commons;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Gantt.Native;
using DevExpress.Xpf.Grid;
using MasterDetailModel;
using MasterDetailView;
using MasterDetailWpf;
using Microsoft.Win32;
using Model;
using Syncfusion.Windows.Controls.Gantt;
//using Syncfusion.Windows.Controls.Gantt.Chart;
//using Syncfusion.Windows.Controls.Gantt.Grid;
//using Syncfusion.Windows.Controls.Gantt.Schedule;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace AttivitaWpf
{
    /// <summary>
    /// Interaction logic for WBSCtrl.xaml
    /// </summary>
    public partial class WBSCtrl : UserControl
    {
        public WBSView View { get { return DataContext as WBSView; } }
        public GanttView GanttView { get { if (View != null) return (GanttView)View.GanttView; else return null; } }
        MasterDetailList MasterDetailList { get; set; } = new MasterDetailList();
        bool _isInitialized = false;
        double prevOffset = -1;


        public event EventHandler EntityViewMouseDoubleClick;
        public event PropertyChangedEventHandler PropertyChanged;

        ToolTip _toolTip = new ToolTip();

        protected virtual void OnEntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            GanttView.OnWBSItemDoubleClick();
            EntityViewMouseDoubleClick?.Invoke(sender, e);
        }

        public WBSCtrl()
        {
            InitializeComponent();
            Loaded += WBSCtrl_Loaded;
            this.Unloaded += WBSCtrl_Unloaded;


        }

        private void WBSCtrl_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void WBSCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                Window_Loaded(sender, e);
                Init();

                MouseMove += WBSCtrl_MouseMove;
            }
        }

        public void Init()
        {
            View.ItemsView.RefreshView += ItemsView_RefreshView;

            View.ItemsView.ItemsLoaded += ItemsView_ItemsLoaded;

            WBSItemTree.Loaded += WBSItemList_Loaded;

            WBSItemTree.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;

            MasterDetailList = new MasterDetailList();
            MasterDetailList.MasterListView = WBSItemTree;
            MasterDetailList.DetailListView = DetailListView;
            MasterDetailList.MasterDetailListCtrl = this;
            MasterDetailList.MasterDetailView = View;
            MasterDetailList.MasterDetailScale = MasterDetailScale;
            //
            MasterDetailList.EntityViewMouseDoubleClick += MasterDetailList_EntityViewMouseDoubleClick;

            View.Init(null);
            _isInitialized = true;
        }



        private void ItemsView_ItemsLoaded(object sender, EventArgs e)
        {
            MasterDetailList.ItemsView_ItemsLoaded(sender, e);
        }

        private void MasterDetailList_EntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            OnEntityViewMouseDoubleClick(sender, e);
        }

        private void WBSItemList_Loaded(object sender, RoutedEventArgs e)
        {
            (View.ItemsView as WBSItemsViewVirtualized).CollegaEscape();
            MasterDetailList.ItemList_Loaded(sender, e);
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            MasterDetailList.ItemContainerGenerator_StatusChanged(sender, e);
        }

        private void ItemsView_RefreshView(object sender, EventArgs e)
        {
            MasterDetailList.ItemsView_RefreshView(sender, e);
        }

        private void SetAttributi_Click(object sender, RoutedEventArgs e)
        {
            //ricavo l'attributo corrente
            DetailAttributoView currAttView = DetailListView.SelectedItem as DetailAttributoView;
            string codiceAtt = string.Empty;
            if (currAttView != null)
                codiceAtt = currAttView.CodiceAttributo;

            View.ItemsView.IsMultipleModify = false;
            View.ItemsView.ReadyToModifyEntitiesId.Clear();
            View.ItemsView.UpdateUI();

            string entityTypeKey = View.ItemsView.EntityType.GetKey();
            ClientDataService clientDataService = View.ItemsView.DataService;

            WBSAttributiSettingsWnd setAttributiWnd = new WBSAttributiSettingsWnd();
            setAttributiWnd.SourceInitialized += (x, y) => setAttributiWnd.HideMinimizeAndMaximizeButtons();
            setAttributiWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            setAttributiWnd.Init(clientDataService, View.ItemsView.MainOperation, View.ItemsView.WindowService, entityTypeKey, codiceAtt);

            if (setAttributiWnd.ShowDialog() == true)
            {
                View.UpdateEntityType();

                List<string> detendentEntityTypes = View.ItemsView.GetDependentEntityTypesKey();
                View.MainOperation.UpdateEntityTypesView(detendentEntityTypes);

            }
        }

        private void listViewDetail_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.listViewDetail_PreviewMouseLeftButtonDown(sender, e);
        }

        private void listViewDetail_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            MasterDetailList.listViewDetail_PreviewMouseMove(sender, e);
        }

        private void listViewDetail_DragEnter(object sender, DragEventArgs e)
        {
            MasterDetailList.listViewDetail_DragEnter(sender, e);
        }

        private void RightSplitPaneControl_DragOver(object sender, DragEventArgs e)
        {
            MasterDetailList.RightSplitPaneControl_DragOver(sender, e);
        }

        private void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        {
            MasterDetailList.RightSplitPaneControl_Drop(sender, e);
        }

        private void FilterToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailList.FilterToggleButton_Drop(sender, e);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailList.TextBox_KeyDown(sender, e);
        }

        private void ValoreAttributo_HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ValoreAttributo_HandleMouseDoubleClick(sender, e);
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            MasterDetailList.Grid_PreviewMouseWheel(sender, e);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            MasterDetailList.Grid_PreviewMouseDown(sender, e);
        }

        private void SortToggleButton_Drop(object sender, DragEventArgs e)
        {
            MasterDetailList.SortToggleButton_Drop(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MasterDetailList.TextBox_PreviewKeyDown(sender, e);
        }

        private void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaFiltri_MouseEnter(sender, e);
        }

        private void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaSort_MouseLeave(sender, e);
        }

        private void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaSort_MouseEnter(sender, e);
        }

        private void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        {
            MasterDetailList.ListaSort_MouseLeave(sender, e);
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MasterDetailList.GridSplitter_DragCompleted(sender, e);
        }

        private void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ListaFiltri_HandleDoubleClick(sender, e);
        }

        private void TreeViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MasterDetailList.ListViewItem_MouseLeftButtonDown(sender, e);
        }


        private async void CreateWBSItemsBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateWBSItemsWnd wnd = new CreateWBSItemsWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.View.DataService = View.DataService;

            wnd.View.Load();
            if (wnd.ShowDialog() == true)
            {

                bool ok = await View.DataService.CreateWBSItems(wnd.View.Data);
                if (ok)
                {
                    View.ItemsView.CheckedEntitiesId.Clear();
                    View.ItemsView.ExpandItemsInternal(View.ItemsView.FilteredEntitiesId, false);//collassa tutte le entità
                    View.OnWBSItemsCreated();
                    View.ItemsView.Load();
                }


            }
        }

        // GANTT

        ScrollViewer sv, sv1, sv2, sv3, sv4;
        ScrollViewer ScrollerGantt;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(this.WBSItemTree) == 0)
                return;

            GanttView.AperturaChusuraRami += GanttView_AperturaChusuraRami;
            //FORZATURA RIGENERAZIONE TIMESCALE CON GANTT ATTIVO
            int Livelli = GanttView.TimescaleRulerCount;
            GanttView.TimescaleRulerCount = 0;
            GanttView.TimescaleRulerCount = Livelli;
            GanttView.ScroolToDataInput += GanttView_ScroolToDataInput;
            GanttView.ResetVaraibles += GanttView_ResetVaraibles;
            Gantt.View.Zoom = Gantt.View.Zoom = GetZoom();
            GanttView.AggiornaAperturaChusuraRami();

            sv1 = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.WBSItemTree, 0), 0) as ScrollViewer;
            sv2 = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(Gantt.View, 0), 0) as ScrollViewer;
            sv2.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            sv1.ScrollChanged += new ScrollChangedEventHandler(sv1_ScrollChanged);
            sv2.ScrollChanged += new ScrollChangedEventHandler(sv2_ScrollChanged);
            DevExpress.Xpf.Core.ApplicationThemeHelper.ApplicationThemeName = Theme.NoneName;
        }

        private void Sv1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (prevOffset != e.HorizontalOffset)
            {
                prevOffset = e.HorizontalOffset;
            }
        }
        double _temp = 0;
        void sv1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            double verticalScroll = (sv1.VerticalOffset * sv2.ScrollableHeight) / sv1.ScrollableHeight;
            //double verticalScroll = sv1.VerticalOffset;
            if (sv1.ScrollableHeight == 0)
                verticalScroll = 0;

            //if (verticalScroll == sv1.ScrollableHeight)
            //{
            //    verticalScroll = sv2.ScrollableHeight;
            //    sv1.ScrollToVerticalOffset(verticalScroll);
            //}

            sv2.ScrollToVerticalOffset(verticalScroll);

        }

        void sv2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double verticalScroll = (sv2.VerticalOffset * sv1.ScrollableHeight) / sv2.ScrollableHeight;
            //double verticalScroll = sv2.VerticalOffset;
            if (sv2.ScrollableHeight == 0)
                verticalScroll = 0;

            //if (verticalScroll == sv2.ScrollableHeight)
            //{
            //    verticalScroll = sv1.ScrollableHeight;
            //    sv2.ScrollToVerticalOffset(verticalScroll);
            //}

            sv1.ScrollToVerticalOffset(verticalScroll);
        }

        void sv3_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            sv4.ScrollToVerticalOffset(sv3.VerticalOffset);
        }
        void sv4_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            sv2.ScrollToHorizontalOffset(sv4.HorizontalOffset);
            sv3.ScrollToVerticalOffset(sv4.VerticalOffset);
        }

        private void WBSCtrl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void GanttView_ResetVaraibles(object sender, EventArgs e)
        {
            if (GanttView == null)
                return;

            GanttView.AperturaChusuraRami -= GanttView_AperturaChusuraRami;
            GanttView.ScroolToDataInput -= GanttView_ScroolToDataInput;
        }
        private void ButtonsChangeSchedule_Click(object sender, RoutedEventArgs e)
        {
            //GanttView.SetScalaCronologica();
        }
        private void ButtonsBarButtonGestioneDateProgetto_Click(object sender, RoutedEventArgs e)
        {
            if (GanttView != null)
            {
                DateProgettoWnd DateProgettoWnd = new DateProgettoWnd();
                DateProgettoWnd.SourceInitialized += (x, y) => DateProgettoWnd.HideMinimizeAndMaximizeButtons();
                DateProgettoWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                DateProgettoWnd.DataContext = GanttView.DateProgettoView;
                DateProgettoWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                GanttView.DateProgettoView.UseDefaultCalendar = false;
                GanttView.SetDateProgettoView();
                if (DateProgettoWnd.ShowDialog() == true)
                {
                    GanttView.AggiornaDataInizioLavori();
                }
            }
        }

        private void GanttView_AddingNewPredecessorLink(object sender, DevExpress.Xpf.Gantt.AddingNewPredecessorLinkEventArgs e)
        {
            List<Processo> TaskRelationShip = new List<Processo>();
            TaskRelationShip.Add(e.PredecessorTask as Processo);
            TaskRelationShip.Add(e.Task as Processo);
            GanttView.UpdatePredecessor(TaskRelationShip, e.LinkType);
        }

        private void GanttView_PredecessorLinkEdited(object sender, DevExpress.Xpf.Gantt.PredecessorLinkEditedEventArgs e)
        {
            if (e.ChangeType == DevExpress.Xpf.Gantt.PredecessorLinkAction.Delete)
            {
                List<Guid> Guids = new List<Guid>();
                Guids.Add((e.OldSuccessor.Content as Processo).Id);
                Guids.Add((e.Predecessor.Content as Processo).Id);
                GanttView.ScollegaPredecessori(Guids);
            }
        }

        private void GanttView_TaskFinishDateMoved(object sender, DevExpress.Xpf.Gantt.TaskFinishDateMovedEventArgs e)
        {
            Processo ProcessoGestito = e.Node.Content as Processo;
            GanttView.ModificaDataFineProcesso(ProcessoGestito, e.FinishDate);
        }

        private void GanttView_TaskMoved(object sender, DevExpress.Xpf.Gantt.TaskMovedEventArgs e)
        {
            Processo ProcessoGestito = e.Node.Content as Processo;
            GanttView.TraslaProcesso(ProcessoGestito, e.StartDate);
        }

        private void GanttView_ProgressMoved(object sender, DevExpress.Xpf.Gantt.ProgressMovedEventArgs e)
        {
            Processo ProcessoGestito = e.Node.Content as Processo;
            GanttView.ModificaProgressProcesso(ProcessoGestito, e.Progress);
        }

        private void ButtonsZoomIndietro_Click(object sender, RoutedEventArgs e)
        {
            Gantt.View.ZoomOut();
        }

        private void ButtonsZoomAvanti_Click(object sender, RoutedEventArgs e)
        {
            Gantt.View.ZoomIn();
        }

        private void ButtonsZoom100_Click(object sender, RoutedEventArgs e)
        {
            Gantt.View.Zoom = GetZoom();
        }

        private void GanttView_RequestTimescaleRulers(object sender, DevExpress.Xpf.Gantt.RequestTimescaleRulersEventArgs e)
        {
            if (GanttView != null)
            {
                e.TimescaleRulers.Clear();
                bool ContinueCreateTimeRuler = false;
                double TimeScaleHeight = 0;

                if (GanttView.ScalaCronologicaView.GetNumeroLivelli() == 3)
                {
                    TabItemView TabItemViewSup = GanttView.ScalaCronologicaView.TabItemViews.ElementAt(0);
                    GanttView.GenerateGanttTimeScaleRuler(TabItemViewSup, e);
                    ContinueCreateTimeRuler = true;
                    if (TimeScaleHeight == 0)
                        TimeScaleHeight = 21;
                }
                if (GanttView.ScalaCronologicaView.GetNumeroLivelli() == 2 || ContinueCreateTimeRuler)
                {
                    TabItemView TabItemViewInt = GanttView.ScalaCronologicaView.TabItemViews.ElementAt(1);
                    GanttView.GenerateGanttTimeScaleRuler(TabItemViewInt, e);
                    ContinueCreateTimeRuler = true;
                    if (TimeScaleHeight == 0)
                        TimeScaleHeight = 21;
                    else
                        TimeScaleHeight = TimeScaleHeight + 21;
                }
                if (GanttView.ScalaCronologicaView.GetNumeroLivelli() == 1 || ContinueCreateTimeRuler)
                {
                    TabItemView TabItemViewInf = GanttView.ScalaCronologicaView.TabItemViews.ElementAt(2);
                    GanttView.GenerateGanttTimeScaleRuler(TabItemViewInf, e);
                    if (TimeScaleHeight == 0)
                        TimeScaleHeight = 21;
                    else
                        TimeScaleHeight = TimeScaleHeight + 21;
                }

                if (GanttView.IsActiveNascondiDate)
                {
                    e.NonworkingDayVisibility = System.Windows.Visibility.Collapsed;
                    e.NonworkingTimeVisibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    e.NonworkingDayVisibility = System.Windows.Visibility.Visible;
                    e.NonworkingTimeVisibility = System.Windows.Visibility.Visible;
                }

                if (GanttView.IsActiveProgressiva)
                    GanttView.GetNoWotkingdays(true);
                else
                    GanttView.GetNoWotkingdays();

                GridStrutturaWBS.Height = TimeScaleHeight;
                GanttView.UpdateUi();
            }
        }

        public TimeSpan GetZoom()
        {
            switch (GanttView.ScalaCronologicaView.TabItemViews.LastOrDefault().SelectedUnita.Key)
            {
                case 0:
                    return new TimeSpan(288, 1, 0);
                    break;
                case 1:
                    return new TimeSpan(0, 1, 0);
                    break;
                case 2:
                    return new TimeSpan(0, 1, 0);
                    break;
                case 3:
                    return new TimeSpan(26, 0, 0);
                    break;
                case 4:
                    return new TimeSpan(0, 1, 0);
                    //DECADI
                    break;
                case 5:
                    return new TimeSpan(3, 0, 0);
                    break;
                case 6:
                    return new TimeSpan(0, 35, 0);
                    break;
                case 7:
                    return new TimeSpan(0, 2, 0);
                    break;
                case 8:
                    break;
                default:
                    return new TimeSpan(0, 1, 0);
                    break;
            }
            return new TimeSpan(0, 1, 0);
        }

        private bool IsDeselection;
        private Guid SelectionId;
        private void Gantt_SelectionChanged(object sender, DevExpress.Xpf.Grid.TreeList.TreeListSelectionChangedEventArgs e)
        {
            if (GanttView != null)
                if (!GanttView.StopSelectionChabged)
                    GanttView.SelezionaDaGanttAWBS(IsDeselection, SelectionId);
            IsDeselection = false;
            SelectionId = Guid.Empty;
        }

        private void ButtonsBarButtonScalaCronolgica_Click(object sender, RoutedEventArgs e)
        {
            if (GanttView != null)
            {
                ScalaCronologicaWnd ScalaCronologicaWnd = new ScalaCronologicaWnd();
                ScalaCronologicaWnd.SourceInitialized += (x, y) => ScalaCronologicaWnd.HideMinimizeAndMaximizeButtons();
                ScalaCronologicaWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                ScalaCronologicaWnd.DataContext = GanttView.ScalaCronologicaView;
                ScalaCronologicaWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                int Livelli = GanttView.ScalaCronologicaView.TimescaleRulerCount;
                if (ScalaCronologicaWnd.ShowDialog() == true)
                {
                    GanttView.SetTimeRulerToGanttData();
                    Gantt.View.Zoom = Gantt.View.Zoom = GetZoom();
                }
                else
                {
                    // PER RIPORTARE I DATI PRECEDENTI VISTO CHE SI BASA TUTTO SUL DATACONTEXT
                    GanttView.RipristinaDataContextFinestraScalaCronologica();
                }
            }
        }

        private void ButtonsBarButtonExportProject_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                MSProjectIO msProject = new MSProjectIO();
                    
                msProject.Init(GanttView);

                msProject.RunExport();

                msProject.Save(saveFileDialog.FileName);


                GanttView.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Gantt esportato con successo"));
            }


            //GanttView.GenereteGanttSyncItemSource();
            //SyncGanttCtrl winExport = new SyncGanttCtrl();
            //SyncGanttView View = new SyncGanttView();
            //winExport.DataContext = View;
            //View.Tasks = GanttView.TasksSync;
            //View.Holidays = GanttView.SyncHolidays;
            //View.Weekends = GanttView.SyncWeekends;
            //winExport.Width = 0;
            //winExport.Height = 0;
            //winExport.WindowStyle = WindowStyle.None;
            //winExport.ShowInTaskbar = false;
            //winExport.ShowActivated = false;
            //winExport.Show();
            //winExport.Hide();
            //System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            //saveFileDialog.DefaultExt = "xml";
            //saveFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
            //saveFileDialog.ShowDialog();
            //if (winExport.Gantt.ExportToXML(saveFileDialog.FileName))
            //{
            //    MessageBox.Show(LocalizationProvider.GetString("Gantt esportato con successo"), LocalizationProvider.GetString("Esportazione XML"),
            //    MessageBoxButton.OK,
            //    MessageBoxImage.Information);
            //}

         
            
        }
        private void ContextMenuDeselect_Click(object sender, RoutedEventArgs e)
        {
            GanttView.DeselezinoaSelezionati();
        }

        private void ContextMenuScollega_Click(object sender, RoutedEventArgs e)
        {
            GanttView.ScollegaPredecessoriSuSelezionati();
        }

        private List<TreeListNode> _previousNode = new List<TreeListNode>();
        int ContatorePreviewMouseDown = 0;
        private void GanttView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed)
            {
                if (e.OriginalSource is GanttTaskDragThumb)
                {
                    ContatorePreviewMouseDown++;
                    var context = ((GanttTaskDragThumb)e.OriginalSource).DataContext as DevExpress.Xpf.Grid.TreeList.TreeListRowData;

                    if (context != null)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            if (ContatorePreviewMouseDown == 2)
                            {
                                Guid g = (context.Node.Content as Processo).Id;
                                if (GanttView.SelectedItems.Where(r => r.Id == g).FirstOrDefault() != null)
                                {
                                    IsDeselection = true;
                                    Gantt.UnselectItem(context.Node);
                                }
                                else
                                {
                                    SelectionId = g;
                                    IsDeselection = false;
                                    Gantt.SelectItem(context.Node);
                                }
                                    
                                ContatorePreviewMouseDown = 0;
                            }
                        }
                        else
                        {

                            if (ContatorePreviewMouseDown == 2)
                            {
                                if (!GanttView.WBSView.IsMultipleModify())
                                {
                                    Gantt.UnselectAll();
                                    Gantt.SelectItem(context.Node);
                                    Processo task = (context.Node.Content as Processo);
                                }
                                ContatorePreviewMouseDown = 0;
                            }
                        }
                    }
                }
            }
        }

        private void GanttView_QueryAllowedTaskEditAction(object sender, DevExpress.Xpf.Gantt.QueryAllowedTaskEditActionEventArgs e)
        {
            Processo task = e.Node.Content as Processo;
            if (task != null)
            {
                if (task.Children.Count() > 0)
                {
                    e.VisibleConnectorThumbKind = DevExpress.Xpf.Gantt.ConnectorThumbKind.None;
                    if (GanttView.HeaderNodeVisibility == Visibility.Visible)
                        e.AllowTaskMove = true;
                    else
                        e.AllowTaskMove = false;
                    e.AllowTaskFinishDateMove = false;
                    e.AllowTaskProgressMove = false;
                }
            }
        }

        private void GanttView_QueryAllowPredecessorEdit(object sender, DevExpress.Xpf.Gantt.QueryAllowPredecessorEditEventArgs e)
        {
            Processo task = e.Node.Content as Processo;
            if (task != null)
            {
                if (task.Children.Count() > 0)
                {
                    e.VisibleConnectorThumbKind = DevExpress.Xpf.Gantt.ConnectorThumbKind.None;
                }
            }
        }


        private void AttivitaSelezionateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Gantt.SelectedItems.Count > 0)
            {
                var tasks = Gantt.SelectedItems.Cast<Processo>();
                var start = tasks.Select(x => x.StartDate).Min();
                var end = tasks.Select(x => x.FinishDate).Max();
                if (start != null && end != null)
                    Gantt.View.FitRangeToWidth(start, end);
            }

        }

        private void ParserFunctionsHelp_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            //System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = e.Uri.AbsoluteUri;
            process.Start();
            e.Handled = true;
        }

        private void SynGanttModelTglBtn_Checked(object sender, RoutedEventArgs e)
        {
            GanttView.ApplicaScalePuntiNotevoli();
        }

        private void TrackBarEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            GanttView?.GoToNextDateIn3DModel((double?)e.NewValue);
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (((ButtonsBarButton)sender).Content == "\ue11e")
            {
                GanttView.Cancel3DModelExecution();
            }
            else
            {
                GanttView.IterateDateIn3DModel();
            }
        }

        private void SettingGanttSALBtn_Click(object sender, RoutedEventArgs e)
        {
            GanttView.UpdateCalendarioDefaultInProgrammazioneSAL();
            GanttView.ProgrammazioneSALView.Init();
            if (GanttView.IsActiveCalendario)
                GanttView.ProgrammazioneSALView.CreateDataColumn = true;
            ProgrammazioneSALWnd programmazioneSALWnd = new ProgrammazioneSALWnd();
            programmazioneSALWnd.SourceInitialized += (x, y) => programmazioneSALWnd.HideMinimizeAndMaximizeButtons();
            programmazioneSALWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            programmazioneSALWnd.DataContext = GanttView.ProgrammazioneSALView;
            GanttView.ProgrammazioneSALView.RunUpdateColumn();
            //GanttView.UpdateDataInizioFineInProgrammazioneSAL();
            programmazioneSALWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (programmazioneSALWnd.ShowDialog() == true)
            {
                //xAlbi
                //View.On_UpdateFolgioDiCalcoloSal();
                GanttView.UpdateSALOnGantt();
            }
        }

        private void ButtonsBarButtonSettingGantt_Click(object sender, RoutedEventArgs e)
        {
            if (GanttView != null)
            {
                GanttChartStyleSettingWnd GanttChartStyleSettingWnd = new GanttChartStyleSettingWnd();
                GanttChartStyleSettingWnd.SourceInitialized += (x, y) => GanttChartStyleSettingWnd.HideMinimizeAndMaximizeButtons();
                GanttChartStyleSettingWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                GanttChartStyleSettingWnd.DataContext = GanttView.GanttChartStyleSettingView;
                GanttChartStyleSettingWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (GanttChartStyleSettingWnd.ShowDialog() == true)
                {
                    GanttView.SetChartSetting();
                }
                else
                {
                    // PER RIPORTARE I DATI PRECEDENTI VISTO CHE SI BASA TUTTO SUL DATACONTEXT
                    GanttView.SetGanttStyles();
                }
            }
        }

        private async void GanttView_AperturaChusuraRami(object sender, WBSVicibleEventArgs e)
        {
            try
            {
                if (GanttView != null)
                {
                    foreach (Guid Index in e.IndexWBSToCollapse)
                    {
                        TreeListNode treeListNode = (Gantt.View as TreeListView).GetNodeByCellValue("Id",Index);
                        if (treeListNode != null)
                            (Gantt.View as TreeListView).CollapseNode(treeListNode.RowHandle);
                    }
                    foreach (Guid Index in e.IndexWBSToExpande)
                    {
                        TreeListNode treeListNode = (Gantt.View as TreeListView).GetNodeByCellValue("Id", Index);
                        if (treeListNode != null)
                            (Gantt.View as TreeListView).ExpandNode(treeListNode.RowHandle);
                    }
                }
            }
            catch (System.Exception)
            {

            }
        }

        private async void GanttView_ScroolToDataInput(object sender, DateEventArgs e)
        {
            ScroolToDataInput(e.Data);
        }

        private async void ScroolToDataInput(DateTime? data = null)
        {
            //Rev by Ale 25/11/2022
            //if (data == null)
            //{
            //    data = GanttView.DateProgettoView.DataInizioGantt;
            //    Gantt.View.FirstVisibleDate = data.Value;
            //}
            //else
            //{
            //    data = data.Value.AddDays(-1);
            //    Gantt.View.FirstVisibleDate = data.Value;
            //}

            if (data == null)
                return;

            data = data.Value.AddDays(-1);                
            Gantt.View.FirstVisibleDate = data.Value;

            //End Rev
        }
    }
}
