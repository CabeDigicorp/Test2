

using DevZest.Windows.DataVirtualization;
using MasterDetailView;
using MasterDetailModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;
using Commons;
using Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using CommonResources;

namespace DivisioniWpf
{
    /// <summary>
    /// Interaction logic for DivisioneCtrl.xaml
    /// </summary>
    public partial class DivisioneCtrl : UserControl
    {
        public DivisioneView DivisioneView { get { return DataContext as DivisioneView; } }

        DispatcherTimer timer = new DispatcherTimer();

        bool _isInitialized = false;

        public event EventHandler EntityViewMouseDoubleClick;
        protected virtual void OnEntityViewMouseDoubleClick(EventArgs e)
        {
            EntityViewMouseDoubleClick?.Invoke(this, e);
        }

        

        public DivisioneCtrl()
        {
            InitializeComponent();
            Loaded += DivisioneCtrl_Loaded;

        }

        protected void DivisioneCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                Init();
        }

        public void Init()
        {
            DivisioneView.ItemsView.ScrollIntoView += DivisioneItemView_ScrollIntoView;
            DivisioneView.ItemsView.RefreshView += DivisioneItemsView_RefreshView;

            _isInitialized = true;
        }


        private void DivisioneItemsView_RefreshView(object sender, EventArgs e)
        {
            if ((DivisioneView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView) == EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView)
            {
                if (DivisioneItemTree.SelectedItem != null)
                    DivisioneItemTree.ScrollIntoView(DivisioneItemTree.SelectedItem);

                DivisioneView.ItemsView.VirtualEntities.Refresh(DivisioneView.ItemsView.SelectedIndex);
            }
            else
            {

                ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(DivisioneItemTree); //Extension method
                if (scrollViewer != null)
                {
                    DivisioneView.ItemsView.FirstVisibleRowIndex = (int)scrollViewer.VerticalOffset;
                    DivisioneView.ItemsView.LastVisibleRowIndex = (int)scrollViewer.VerticalOffset + (int)scrollViewer.ViewportHeight;
                }

                DivisioneView.ItemsView.VirtualEntities.Refresh(DivisioneView.ItemsView.FirstVisibleRowIndex);

                //Scopo: Forzo un secondo refresh altrimenti non aggiorna sulla lista al cambio di un valore di attributo (dopo la 50esima entità)
                DivisioneView.ItemsView.VirtualEntities.Refresh(DivisioneView.ItemsView.LastVisibleRowIndex);
            }

            //DivisioneItemTree.Focus();

            DivisioneView.ItemsView.PendingCommand = EntitiesListMasterDetailViewCommands.Nessuno;

        }


        
        private void DivisioneItemView_ScrollIntoView(object sender, EventArgs e)
        {
            //if (DivisioneItemList.SelectedItem != null)
            //    DivisioneItemList.ScrollIntoView(DivisioneItemList.SelectedItem);
        }

        /// <summary>
        /// Gestisce il DoubleClick sull'attributo filtrato per visualizzare il dialogo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AttributoFilterView attFilterView = ((ListViewItem)sender).Content as AttributoFilterView;
            if (attFilterView.Attributo != null)
            {
                int index = DivisioneView.ItemsView.RightPanesView.FilterView.Items.IndexOf(attFilterView);
                DivisioneView.ItemsView.RightPanesView.FilterView.Load(attFilterView.Attributo, index);
            }
        }


        #region Drag&Drop
        private Point _startPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope = false;


        protected void listViewDetail_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);

        }

        protected void listViewDetail_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "DragRectangle")
                    {
                        IEnumerable<Popup> popups = GetOpenPopups();
                        if (!popups.Any())
                            BeginDrag(e);
                    }
                }
            }
        }

        private void BeginDrag(MouseEventArgs e)
        {
            ListView listView = this.DetailListView;
            ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            DetailAttributoView entAttView = listViewItem.Content as DetailAttributoView;
            string codAttributo = entAttView.CodiceAttributo;
            string pacchetto = String.Join(",", new string[] { "AttributoView", codAttributo });

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;

            DataObject data = new DataObject("AttributoViewFormat", pacchetto);
            DragDropEffects de = DragDrop.DoDragDrop(this.DetailListView, data, DragDropEffects.Move);

            //cleanup
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            listView.DragEnter -= ListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
            }
        }

        /// <summary>
        /// Non faccio il drag&drop se sono attivi dei popup altrimenti poi perde il fuoco sui controlli
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Popup> GetOpenPopups()
        {
            return PresentationSource.CurrentSources.OfType<HwndSource>()
                .Select(h => h.RootVisual)
                .OfType<FrameworkElement>()
                .Select(f => f.Parent)
                .OfType<Popup>();
            //.Where(p => p.IsOpen);
        }

        private void InitialiseAdorner(ListViewItem listViewItem)
        {
            VisualBrush brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner((UIElement)listViewItem, listViewItem.RenderSize, brush);
            _adorner.Opacity = 0.5;
            _layer = AdornerLayer.GetAdornerLayer(DetailListView as Visual);
            _layer.Add(_adorner);
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == DetailListView)
            {
                Point p = e.GetPosition(DetailListView);
                Rect r = VisualTreeHelper.GetContentBounds(DetailListView);
                if (!r.Contains(p))
                {
                    this._dragIsOutOfScope = true;
                    e.Handled = true;
                }
            }
        }

        void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(DetailListView).X;
                //_adorner.OffsetTop = args.GetPosition(listViewDetail).Y - _startPoint.Y;
                _adorner.OffsetTop = args.GetPosition(this).Y - _startPoint.Y;
            }
        }

        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("AttributoViewFormat") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        protected void listViewDetail_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("AttributoViewFormat") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        protected void FilterToggleButton_DragOver(object sender, DragEventArgs e)
        {
        }

        protected void FilterToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;

                //string pacchetto = await e.DataView.GetTextAsync();
                var items = pacchetto.Split(',');

                Attributo tipoAttributo = DivisioneView.ItemsView.GetAttributoByCode(items[1]);
                //Attributo tipoAttributo = FilterView.This.Master.GetAttributoByCode(items[1]);

                if (tipoAttributo != null)
                    DivisioneView.ItemsView.RightPanesView.FilterView.Load(tipoAttributo, -1);

            }
        }

        protected void RightSplitPaneControl_DragOver(object sender, DragEventArgs e)
        {

        }

        protected void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        {
            ContentControl contentCtrl = sender as ContentControl;
            if (contentCtrl.Content is FilterView)
                FilterToggleButton_Drop(sender, e);
            else if (contentCtrl.Content is SortView)
                SortToggleButton_Drop(sender, e);
        }

        private void SortToggleButton_DragOver(object sender, DragEventArgs e)
        {

        }

        protected void SortToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;

                //string pacchetto = await e.DataView.GetTextAsync();
                var items = pacchetto.Split(',');

                Attributo tipoAttributo = DivisioneView.ItemsView.GetAttributoByCode(items[1]);
                //Attributo tipoAttributo = SortView.This.Master.GetAttributoByCode(items[1]);

                if (tipoAttributo != null)
                    DivisioneView.ItemsView.RightPanesView.SortView.Load(tipoAttributo);

            }
        }

        public T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        #endregion Drag&Drop


        #region Enter Key
        protected void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        //private void ShortRichTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //        (sender as Xceed.Wpf.Toolkit.RichTextBox).GetBindingExpression(Xceed.Wpf.Toolkit.RichTextBox.TextProperty).UpdateSource();
        //}
        #endregion


        //#region RichText
        //private void RichTextContent_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    ContentControl cc = sender as ContentControl;
        //    if (cc == null)
        //        return;

        //    Xceed.Wpf.Toolkit.RichTextBox richTextBox = cc.Content as Xceed.Wpf.Toolkit.RichTextBox;

        //    if (richTextBox == null)
        //        return;

        //    richTextBox.Width = cc.ActualWidth;
        //}

        //private void RichTextLargeContent_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    ContentControl cc = sender as ContentControl;
        //    if (cc == null)
        //        return;

        //    Xceed.Wpf.Toolkit.RichTextBox richTextBox = cc.Content as Xceed.Wpf.Toolkit.RichTextBox;

        //    if (richTextBox == null)
        //        return;

        //    richTextBox.Width = cc.ActualWidth;
        //}



        //#endregion RichText



        #region Layout scale
        protected void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if ((e.Delta > 0 && DivisioneScale.ScaleX < 2.0)
                    || (e.Delta < 0 && DivisioneScale.ScaleX > 0.5))
                {
                    DivisioneScale.ScaleX += (e.Delta > 0) ? 0.1 : -0.1;
                    DivisioneScale.ScaleY += (e.Delta > 0) ? 0.1 : -0.1;
                }
            }
        }

        protected void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    DivisioneScale.ScaleX = 1;
                    DivisioneScale.ScaleY = 1;

                }

            }
        }
        #endregion  Layout scale

        protected void ValoreAttributo_HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in DivisioneView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        private void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in DivisioneView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }

        private void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in DivisioneView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        private void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in DivisioneView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }


        /// <summary>
        /// Scopo: separatore decimale nel tasterino numerico secondo le impostazioni di windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Decimal)
            {
                var txb = sender as TextBox;

                string decimalSeparator = "";
                if (txb.DataContext is ValoreRealeView)
                {
                    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                }
                else if (txb.DataContext is ValoreContabilitaView)
                {
                    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                }
                //else if (txb.DataContext is ValorePercentualeView)
                //{
                //    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.PercentDecimalSeparator;
                //}
                else
                    return;

                int caretPos = txb.CaretIndex;
                txb.Text = txb.Text.Insert(txb.CaretIndex, decimalSeparator);
                txb.CaretIndex = caretPos + 1;
                e.Handled = true;
            }

        }

        /// <summary>
        /// Scopo: evento di doppio click sul mouse su un EntityView della lista
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TreeViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) //double click
            {
                EntityViewMouseDoubleClick?.Invoke(sender, e);
            }
        }

        protected void SetAttributi_Click(object sender, RoutedEventArgs e)
        {
            //ricavo l'attributo corrente
            DetailAttributoView currAttView = DetailListView.SelectedItem as DetailAttributoView;
            string codiceAtt = string.Empty;
            if (currAttView != null)
                codiceAtt = currAttView.CodiceAttributo;

            if (DivisioneView == null)
                return;

            if (DivisioneView.ItemsView == null)
                return;

            if (DivisioneView.ItemsView.EntityType == null)
                return;

            DivisioneView.ItemsView.IsMultipleModify = false;
            DivisioneView.ItemsView.ReadyToModifyEntitiesId.Clear();
            DivisioneView.ItemsView.UpdateUI();

            string entityTypeKey = DivisioneView.ItemsView.EntityType.GetKey();
            ClientDataService clientDataService = DivisioneView.ItemsView.DataService;

            DivisioneAttributiSettingsWnd setAttributiWnd = new DivisioneAttributiSettingsWnd();
            setAttributiWnd.SourceInitialized += (x, y) => setAttributiWnd.HideMinimizeAndMaximizeButtons();
            setAttributiWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            setAttributiWnd.Init(clientDataService, DivisioneView.ItemsView.MainOperation, DivisioneView.ItemsView.WindowService, entityTypeKey, codiceAtt);

            if (setAttributiWnd.ShowDialog() == true)
            {
                DivisioneView.UpdateEntityType();

                List<string> detendentEntityTypes = DivisioneView.ItemsView.GetDependentEntityTypesKey();
                DivisioneView.MainOperation.UpdateEntityTypesView(detendentEntityTypes);
                //DivisioneView.MainOperation.UpdateEntityTypes(new List<string>()
                //{ BuiltInCodes.EntityType.Prezzario, BuiltInCodes.EntityType.Elementi, BuiltInCodes.EntityType.Computo });
            }
        }

        protected void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (DivisioneView.ItemsView.IsMultipleModify)
                return;

            double horizontalChange = e.HorizontalChange / DivisioneScale.ScaleX;

            DivisioneView.ItemsView.EntityType.DetailAttributoEtichettaWidth += horizontalChange;
            DivisioneView.ItemsView.AttributiEntities.Load(DivisioneView.ItemsView.SelectedEntityView, true);
        }

        private void MasterHeaderAttributo3Btn_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new DivisioneAttributoSumSettingsWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            wnd.DataService = DivisioneView.DataService;
            wnd.SenderAttributoRiferimento = DivisioneView.SenderAttRif;
            wnd.SummarizeAttributoIndex = 3;

            wnd.Init();

            if (wnd.ShowDialog() == true)
            {
                DivisioneView.SetAttributoSum(3, wnd.SummarizeCodiceAttributo, wnd.SummarizeOperation);
            }
        }

        private void MasterHeaderAttributo4Btn_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new DivisioneAttributoSumSettingsWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            wnd.DataService = DivisioneView.DataService;
            wnd.SenderAttributoRiferimento = DivisioneView.SenderAttRif;
            wnd.SummarizeAttributoIndex = 4;

            wnd.Init();

            if (wnd.ShowDialog() == true)
            {
                DivisioneView.SetAttributoSum(4, wnd.SummarizeCodiceAttributo, wnd.SummarizeOperation);
            }
        }

        private void MasterHeaderAttributo3Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DivisioneView.DivisioneItemsView.WidthAttributo3 = MasterHeaderAttributo3Btn.ActualWidth;
            DivisioneView.DivisioneItemsView.UpdateCache();
        }

        private void MasterHeaderAttributo4Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DivisioneView.DivisioneItemsView.WidthAttributo4 = MasterHeaderAttributo4Btn.ActualWidth;
            DivisioneView.DivisioneItemsView.UpdateCache();
        }

    }


}
