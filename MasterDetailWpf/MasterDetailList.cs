using Commons;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MasterDetailWpf
{
    /// <summary>
    /// Classe associata al controllo Lista
    /// </summary>
    public class MasterDetailList
    {
        public UserControl MasterDetailListCtrl { get; set; }
        public ListView MasterListView { get; set; }//master control
        public ListView DetailListView { get; set; }//detail control
        //public MasterDetailListView MasterDetailListView { get; set; }//MasterDetail View
        public MasterDetailViewBase MasterDetailView { get; set; }//MasterDetail View
        public ScaleTransform MasterDetailScale { get; set; }


        public event EventHandler EntityViewMouseDoubleClick;
        protected virtual void OnEntityViewMouseDoubleClick(object sender, EventArgs e)
        {
            EntityViewMouseDoubleClick?.Invoke(sender, e);
        }

        public MasterDetailList()
        {
        }


        /// <summary>
        /// Evento per poter effettuare lo scroll all'entità selezionata
        /// Sembra che finchè i container degli items non sono stati tutti generati non funzioni lo ScrollIntoView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            //if (MasterListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            //{
                if ((MasterDetailView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView) == EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView)
                {
                    if (MasterListView.SelectedItem != null)
                    {
                        MasterListView.ScrollIntoView(MasterListView.SelectedItem);
                        MasterDetailView.ItemsView.PendingCommand = EntitiesListMasterDetailViewCommands.Nessuno;
                    }
                }
            //}
        }


        public void ItemsView_RefreshView(object sender, EventArgs e)//ItemsLoading
        {

            if ((MasterDetailView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView) == EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView)
            {
                MasterDetailView.Refresh(MasterDetailView.ItemsView.SelectedIndex);
            }
            else
            {

                ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(MasterListView); //Extension method
                if (scrollViewer != null)
                {
                    if (scrollViewer.ScrollableHeight > 0)
                    {
                        //scrollViewer.ScrollToVerticalOffset()

                        double firstVertOffset = ((scrollViewer.VerticalOffset * MasterListView.Items.Count) / scrollViewer.ScrollableHeight);
                        double lastVertOffset = (((scrollViewer.VerticalOffset + scrollViewer.ViewportHeight) * MasterListView.Items.Count) / scrollViewer.ScrollableHeight);

                        MasterDetailView.ItemsView.FirstVisibleRowIndex = (int)firstVertOffset;
                        MasterDetailView.ItemsView.LastVisibleRowIndex = (int)lastVertOffset;

                    }
                    else
                    {
                        double firstVertOffset = 0;
                        double lastVertOffset = 0;

                        MasterDetailView.ItemsView.FirstVisibleRowIndex = (int)firstVertOffset;
                        MasterDetailView.ItemsView.LastVisibleRowIndex = (int)lastVertOffset;
                    }
                }
                
                MasterDetailView.Refresh(MasterDetailView.ItemsView.FirstVisibleRowIndex);

                //Scopo: Forzo un secondo refresh altrimenti non aggiorna sulla lista al cambio di un valore di attributo (dopo la 50esima entità)
                MasterDetailView.Refresh(MasterDetailView.ItemsView.LastVisibleRowIndex);

                //if (MasterDetailView.ItemsView.EntityType.Codice == BuiltInCodes.EntityType.Contatti)
                //{
                //    MasterDetailView.Refresh(1);
                //}
                

                MasterDetailView.ItemsView.PendingCommand = EntitiesListMasterDetailViewCommands.Nessuno;

            }

        }

        public void ItemsView_ItemsLoaded(object sender, EventArgs e)
        {
            if ((MasterDetailView.ItemsView.PendingCommand & EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView) == EntitiesListMasterDetailViewCommands.ScrollCurrentItemIntoView)
            {
                //if (MasterListView.SelectedItem != null)
                //    MasterListView.ScrollIntoView(MasterListView.SelectedItem);
                //MasterDetailView.Refresh(MasterDetailView.ItemsView.SelectedIndex);
                
                /////////////////////////////////
                ///Serve esclusivamente per far partire l'evento ItemContainerGenerator_StatusChanged
                ScrollViewer scrollViewer = MasterDetailList.GetVisualChild<ScrollViewer>(MasterListView);
                if (scrollViewer != null)
                    scrollViewer.ScrollToHorizontalOffset(1);
                ////////////////////////////////
            }

        }

        public async void ItemList_Loaded(object sender, RoutedEventArgs e)
        {
            MasterDetailView.ItemsView.ClearReadyToModifyEntities();
            await MasterDetailView.ItemsView.UpdateCache(true);
        }

        /// <summary>
        /// Gestisce il DoubleClick sull'attributo filtrato per visualizzare il dialogo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ListaFiltri_HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AttributoFilterView attFilterView = ((ListViewItem)sender).Content as AttributoFilterView;
            if (attFilterView.Attributo != null)
            {
                int index = MasterDetailView.ItemsView.RightPanesView.FilterView.Items.IndexOf(attFilterView);

                MasterDetailView.ItemsView.RightPanesView.FilterView.Load(attFilterView.Attributo, index);
                MasterDetailView.ItemsView.RightPanesView.FilterView.SearchNext();
            }
        }


        #region Drag&Drop
        private Point _startPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragIsOutOfScope = false;


        public void listViewDetail_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);

        }

        public void listViewDetail_PreviewMouseMove(object sender, MouseEventArgs e)
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

        public void BeginDrag(MouseEventArgs e)
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

        public void ListViewDragLeave(object sender, DragEventArgs e)
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

        public void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(DetailListView).X;
                //_adorner.OffsetTop = args.GetPosition(DetailListView).Y - _startPoint.Y;
                _adorner.OffsetTop = args.GetPosition(MasterDetailListCtrl).Y - _startPoint.Y;
            }
        }

        public void ListViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("AttributoViewFormat") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        public void listViewDetail_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("AttributoViewFormat") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        public void FilterToggleButton_DragOver(object sender, DragEventArgs e)
        {
        }

        public void FilterToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;

                //string pacchetto = await e.DataView.GetTextAsync();
                var items = pacchetto.Split(',');

                Attributo tipoAttributo = MasterDetailView.ItemsView.GetAttributoByCode(items[1]);
                //Attributo tipoAttributo = FilterView.This.Master.GetAttributoByCode(items[1]);

                if (tipoAttributo != null)
                {
                    MasterDetailView.ItemsView.RightPanesView.FilterView.Load(tipoAttributo, -1);
                    if (MasterDetailView.ItemsView.RightPanesView.FilterView.Items.Any())
                        MasterDetailView.ItemsView.RightPanesView.FilterView.SearchNext();
                }

            }
        }

        public void RightSplitPaneControl_DragOver(object sender, DragEventArgs e)
        {

        }

        public void RightSplitPaneControl_Drop(object sender, DragEventArgs e)
        {
            ContentControl contentCtrl = sender as ContentControl;
            if (contentCtrl.Content is FilterView)
                FilterToggleButton_Drop(sender, e);
            else if (contentCtrl.Content is SortView)
                SortToggleButton_Drop(sender, e);
        }

        public void SortToggleButton_DragOver(object sender, DragEventArgs e)
        {

        }

        public void SortToggleButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("AttributoViewFormat"))
            {
                string pacchetto = e.Data.GetData("AttributoViewFormat") as string;

                //string pacchetto = await e.DataView.GetTextAsync();
                var items = pacchetto.Split(',');

                Attributo tipoAttributo = MasterDetailView.ItemsView.GetAttributoByCode(items[1]);
                //Attributo tipoAttributo = SortView.This.Master.GetAttributoByCode(items[1]);

                if (tipoAttributo != null)
                    MasterDetailView.ItemsView.RightPanesView.SortView.Load(tipoAttributo);

            }
        }

        static public T GetVisualChild<T>(Visual parent) where T : Visual
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
        public void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        //public void ShortRichTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //        (sender as Xceed.Wpf.Toolkit.RichTextBox).GetBindingExpression(Xceed.Wpf.Toolkit.RichTextBox.TextProperty).UpdateSource();
        //}
        #endregion


        //#region RichText
        //public void RichTextContent_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    ContentControl cc = sender as ContentControl;
        //    if (cc == null)
        //        return;

        //    Xceed.Wpf.Toolkit.RichTextBox richTextBox = cc.Content as Xceed.Wpf.Toolkit.RichTextBox;

        //    if (richTextBox == null)
        //        return;

        //    richTextBox.Width = cc.ActualWidth;
        //}

        //public void RichTextLargeContent_SizeChanged(object sender, SizeChangedEventArgs e)
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


        /// <summary>
        /// Gestisce l'evento di doppio click sul valore di un attributo di tipo AttributoRiferimento
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ValoreAttributo_HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //esclude l'operazione se il DataService non esiste o è readonly
            if (MasterDetailView.DataService == null || MasterDetailView.DataService.IsReadOnly)
                return;

            //escludo l'operazione se sto agendo su un attributo che non è di riferito all'elenco prezzi.

            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;

            ValoreView valView = frameworkElement.DataContext as ValoreView;
            if (valView == null)
                return;

            AttributoRiferimento attRif = MasterDetailView.ItemsView.EntityType.Attributi[valView.Tag as string] as AttributoRiferimento;
            if (attRif == null)
                return;


            if (MasterDetailView.ItemsView.EntitiesHelper.IsAttributoRiferimentoGuidCollection(attRif))
            {

            }
            else
            {
                if (attRif.ReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione))
                {
                    //MasterDetailListView.ReplaceCurrentPrezzarioItemCategoria(attRif);
                    MasterDetailView.ReplaceCurrentItemDivisione(attRif);
                }
                else
                {
                    MasterDetailView.ReplaceCurrentItemAttributoGuid(attRif, null);
                }
            }
   
        }


        #region Layout scale
        public void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //base.OnPreviewMouseWheel(e);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if ((e.Delta > 0 && MasterDetailScale.ScaleX < 2.0)
                    || (e.Delta < 0 && MasterDetailScale.ScaleX > 0.5))
                {
                    MasterDetailScale.ScaleX += (e.Delta > 0) ? 0.1 : -0.1;
                    MasterDetailScale.ScaleY += (e.Delta > 0) ? 0.1 : -0.1;
                }
            }
        }

        public void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //base.OnPreviewMouseDown(e);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    MasterDetailScale.ScaleX = 1;
                    MasterDetailScale.ScaleY = 1;

                }

            }
        }
        #endregion  Layout scale

        public void ListaFiltri_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        public void ListaFiltri_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }

        public void ListaSort_MouseEnter(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = true;
        }

        public void ListaSort_MouseLeave(object sender, MouseEventArgs e)
        {
            //foreach (DetailAttributoView attView in MasterDetailView.ItemsView.AttributiEntities.AttributiValoriComuniView)
            //    attView.IsHilighted = false;
        }

        /// <summary>
        /// Scopo: separatore decimale nel tasterino numerico secondo le impostazioni di windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
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
                else if (txb.DataContext is ValoreContabilitaView)
                {
                    decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.PercentDecimalSeparator;
                }
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
        public void ListViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) //double click
            {
                OnEntityViewMouseDoubleClick(sender, e);
            }
        }





        public void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {

            if (MasterDetailView.ItemsView.IsMultipleModify)
                return;

            double horizontalChange = e.HorizontalChange / MasterDetailScale.ScaleX;

            MasterDetailView.ItemsView.EntityType.DetailAttributoEtichettaWidth += horizontalChange;
            MasterDetailView.ItemsView.AttributiEntities.Load(MasterDetailView.ItemsView.SelectedEntityView, true);

        }


    }

}
