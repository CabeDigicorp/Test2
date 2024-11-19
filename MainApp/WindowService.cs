
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PrezzariWpf;
using DivisioniWpf;
using MasterDetailModel;
using Model;
using ElementiWpf;
using System.Windows.Input;
using _3DModelExchange;
using Commons;
using MasterDetailWpf;
using MasterDetailView;
using ContattiWpf;
using MainApp.Windows;
using StampeWpf;
using ComputoWpf;
using DatiGeneraliWpf.Stili;
using AttivitaWpf;
using CommonResources;
using DatiGeneraliWpf;
using System.Threading;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;

namespace MainApp
{


    public class WindowService : IEntityWindowService
    {
        public ClientDataService DataService { get; set; } = null;//ref
        public ModelActionsStack ModelActionsStack { get; set; } = null;//ref
        public IMainOperation MainOperation { get; set; } = null;//ref

        /// <summary>
        /// key: entity type key
        /// </summary>
        public Dictionary<string, EntityTypeViewSettings> DefaultViewSettings = new Dictionary<string, EntityTypeViewSettings>();

        public Dictionary<string, CalculatorFunction> CalculatorFunctions { get; set; } = new Dictionary<string, CalculatorFunction>();

        public void ShowEditRtfWindow(ref string rtfText, string title, out string plainText)
        {

            var win = new RtfEditWindow();
            win.SourceInitialized += (x, y) => win.HideMinimizeAndMaximizeButtons();
            win.Owner = System.Windows.Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            win.Title = title;

            if (rtfText.Any())
            {
                win.Init(rtfText, DataService, MainOperation);
            }


            try
            {
                win.ShowDialog();
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }

            string rtf = win.RtfText;
            if (rtf != null)
                rtfText = rtf;

            plainText = win.Text;


            //var win = new EditRtfWindow();
            //win.SourceInitialized += (x, y) => win.HideMinimizeAndMaximizeButtons();
            //win.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            //win.Title = title;

            //if (rtfText.Any())
            //{
            //    win.RichTextBoxLoad(rtfText, new RtfFormatProvider(), DataService, MainOperation);//AU 28/10/2020
            //}


            //try
            //{
            //    win.ShowDialog();
            //}
            //catch (Exception exc)
            //{
            //    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            //}


          

            ////devexpress_test
            //string rtf = win.RtfText;
            //if (rtf != null)
            //    rtfText = rtf;

            //plainText = win.Text;


        }

        //public void ReplaceInRtfText(ref string rtf, string oldStr, string newStr)
        //{

        //    Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
        //    rtb.TextFormatter = new Xceed.Wpf.Toolkit.RtfFormatter();
        //    rtb.Text = rtf;

        //    TextRange text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

        //    TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
        //    while (current != null)
        //    {
        //        string textInRun = current.GetTextInRun(LogicalDirection.Forward);
        //        if (!string.IsNullOrWhiteSpace(textInRun))
        //        {
        //            int index = textInRun.IndexOf(oldStr);
        //            if (index != -1)
        //            {
        //                TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
        //                TextPointer selectionEnd = selectionStart.GetPositionAtOffset(oldStr.Length, LogicalDirection.Forward);
        //                TextRange selection = new TextRange(selectionStart, selectionEnd);
        //                selection.Text = newStr;
        //                //selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        //                rtb.Selection.Select(selection.Start, selection.End);
        //                //rtb.Focus();
        //            }
        //        }
        //        current = current.GetNextContextPosition(LogicalDirection.Forward);
        //    }

        //    rtf = rtb.Text;
        //}

        public void ShowReplaceTextWindow(object viewModel)
        {
            ReplaceTextWindow replaceTextWindow = new ReplaceTextWindow();
            replaceTextWindow.SourceInitialized += (x, y) => replaceTextWindow.HideMinimizeAndMaximizeButtons();
            replaceTextWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            replaceTextWindow.DataContext = viewModel;
            replaceTextWindow.ShowDialog();
        }

        public void ShowWaitCursor(bool wait)
        {
            if (Thread.CurrentThread.ApartmentState != ApartmentState.STA)
                return;

            if (wait)
                Mouse.OverrideCursor = Cursors.Wait;
            else
                Mouse.OverrideCursor = null;
        }

        public int FilterByEntityIdsWindow(EntitiesListMasterDetailView master, AttributoFilterData attFilterData, string sourceAttEntityTypeKey, ref List<Guid> itemsId, string title)
        {



            int res = 0;

            EntityType sourceEntType = DataService.GetEntityTypes()[sourceAttEntityTypeKey];

            if (sourceEntType.MasterType == MasterType.Tree || sourceEntType.MasterType == MasterType.List)
            {

                FilterByEntityIdsWnd wnd = new FilterByEntityIdsWnd(master, attFilterData);
                wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
                wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                wnd.View.DataService = DataService;
                wnd.View.WindowService = this;
                wnd.View.ModelActionsStack = ModelActionsStack;
                wnd.View.MainOperation = MainOperation;

                wnd.Title = title;

                wnd.View.EntityItemSelectedIds = new HashSet<Guid>(itemsId);
                wnd.View.Init();

                if (wnd.ShowDialog() == true)
                {
                    itemsId = wnd.View.EntityItemSelectedIds.ToList();
                    res = 1;

                }


            }
            else
            {


                //if (sourceAttEntityTypeKey == BuiltInCodes.EntityType.Contatti)
                //{
                //    res = FilterByContattiIdsWindow(ref itemsId, title);
                //}
                if (sourceAttEntityTypeKey == BuiltInCodes.EntityType.Elementi)
                {
                    res = FilterByElementiIdsWindow(ref itemsId, title);
                }
                else if (sourceAttEntityTypeKey == BuiltInCodes.EntityType.Report)
                {
                    res = FilterByReportIdsWindow(ref itemsId, title);
                }
                //else if (sourceAttEntityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
                //{
                //    res = FilterByElencoAttivitaIdsWindow(ref itemsId, title);
                //}
                else if (sourceAttEntityTypeKey == BuiltInCodes.EntityType.Computo)
                {
                    res = FilterByComputoIdsWindow(ref itemsId, title);
                }
                //else if (sourceAttEntityTypeKey == BuiltInCodes.EntityType.Allegati)
                //{
                //    res = FilterByAllegatiIdsWindow(ref itemsId, title);
                //}
            }


            return res;
            //#else
            //            DataService.Suspended = true;

            //            if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
            //            {
            //                return FilterByContattiIdsWindow(ref itemsId, title);
            //            }
            //            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
            //            {
            //                return FilterByPrezzarioIdsWindow(ref itemsId, title);
            //            }
            //            else if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
            //            {
            //                return FilterByCapitoliIdsWindow(ref itemsId, title);
            //            }
            //            else if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
            //            {
            //                return FilterByElementiIdsWindow(ref itemsId, title);
            //            }
            //            else if (entityTypeKey == BuiltInCodes.EntityType.Report)
            //            {
            //                return FilterByReportIdsWindow(ref itemsId, title);
            //            }
            //#endif

            return 0;
        }

        public bool SelectEntityIdsWindow(string entityTypeKey, ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options, EntityTypeViewSettings viewSettings, AttributoRiferimento senderAttRif)
        {

            Dictionary<string, EntityType> entTypes = DataService.GetEntityTypes();
            DivisioneItemType divType = entTypes.GetValueOrNull(entityTypeKey) as DivisioneItemType;
            if (divType != null)
            {
                return SelectDivisioneIdsWindow(divType.DivisioneId, ref selectedItems, title, options, senderAttRif);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
            {
                return SelectContattiIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
            {
                string externalPrezzarioFileName = string.Empty;
                return SelectPrezzarioIdsWindow(ref selectedItems, ref externalPrezzarioFileName, title, options, true, false, true, ref viewSettings);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
            {
                return SelectCapitoliIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
            {
                return SelectElementiIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Report)
            {
                return SelectReportIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Stili)
            {
                return SelectStiliIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                return SelectComputoIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Documenti)
            {
                return SelectDocumentiIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
            {
                return SelectElencoAttivitaIdsWindow(ref selectedItems, title, options, viewSettings);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Calendari)
            {
                return SelectCalendariIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Allegati)
            {
                return SelectAllegatiIdsWindow(ref selectedItems, title, options);
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Tag)
            {
                return SelectTagIdsWindow(ref selectedItems, title, options);
            }
            //else if (entityTypeKey == BuiltInCodes.EntityType.WBS)
            //{
            //    return SelectWBSIdsWindow(ref selectedItems, title, options);
            //}



                return false;
        }

        #region Prezzario
        public event EventHandler PrezzarioItemsChanged;
        protected void OnPrezzarioItemsChanged(EventArgs e)
        {
            PrezzarioItemsChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="windowService"></param>
        /// <param name="selectedItems"></param>
        /// <param name="title"></param>
        /// <returns>0: nothing, 1: Find, 2:Filter</returns>
        public int FilterByPrezzarioIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = PrezzarioItemType.CreateKey();

            DataService.Suspended = true;
            FilterByPrezzarioIdsWindow filterByPrezzarioIdsWindow = new FilterByPrezzarioIdsWindow();
            filterByPrezzarioIdsWindow.SourceInitialized += (x, y) => filterByPrezzarioIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByPrezzarioIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByPrezzarioIdsWindow.DataService = DataService;
            filterByPrezzarioIdsWindow.WindowService = this;
            filterByPrezzarioIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByPrezzarioIdsWindow.MainOperation = MainOperation;

            filterByPrezzarioIdsWindow.CurrentPrezzarioId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByPrezzarioIdsWindow.Title = title;
            filterByPrezzarioIdsWindow.AllowNoSelection = true;
            filterByPrezzarioIdsWindow.PrezzarioItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByPrezzarioIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByPrezzarioIdsWindow.Init();

            if (filterByPrezzarioIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByPrezzarioIdsWindow.PrezzarioItemSelectedIds;
                //if (filterByPrezzarioIdsWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }

        public bool SelectPrezzarioIdsWindow(ref List<Guid> selectedItems, ref string externalPrezzarioFileName,
            string title,
            SelectIdsWindowOptions options,
            bool allowPrezzarioInterno,
            bool allowPrezzariEsterni,
            bool updateItemsOnClose,
            ref EntityTypeViewSettings viewSettings)
        {



            int rit = 0;
            string entityTypeKey = PrezzarioItemType.CreateKey();

            SelectPrezzarioIdsWindow selectPrezzarioIdWnd = new SelectPrezzarioIdsWindow();
            selectPrezzarioIdWnd.SourceInitialized += (x, y) => selectPrezzarioIdWnd.HideMinimizeAndMaximizeButtons();
            selectPrezzarioIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectPrezzarioIdWnd.Title = title;
            selectPrezzarioIdWnd.IOData.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; // isSingleSelection;
            selectPrezzarioIdWnd.IOData.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectPrezzarioIdWnd.IOData.AllowPrezzarioInterno = allowPrezzarioInterno;
            selectPrezzarioIdWnd.IOData.AllowPrezzariEsterni = allowPrezzariEsterni;
            selectPrezzarioIdWnd.IOData.PrezzarioItemSelectedIds = selectedItems;
            selectPrezzarioIdWnd.IOData.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectPrezzarioIdWnd.IOData.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;
            selectPrezzarioIdWnd.IOData.ExternalPrezzarioFileName = externalPrezzarioFileName;

            //if (DefaultViewSettings.ContainsKey(entityTypeKey))
            //    selectPrezzarioIdWnd.IOData.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            if (viewSettings != null)
                selectPrezzarioIdWnd.IOData.CurrentViewSettings = viewSettings;

            selectPrezzarioIdWnd.IOData.DataService = DataService;
            selectPrezzarioIdWnd.IOData.WindowService = this;
            selectPrezzarioIdWnd.IOData.ModelActionsStack = ModelActionsStack;
            selectPrezzarioIdWnd.IOData.MainOperation = MainOperation;
            selectPrezzarioIdWnd.Init();

            bool res = false;

            if (selectPrezzarioIdWnd.ShowDialog() == true)
            {
                //if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                //    DefaultViewSettings.Add(entityTypeKey, selectPrezzarioIdWnd.IOData.CurrentViewSettings);
                //else
                //    DefaultViewSettings[entityTypeKey] = selectPrezzarioIdWnd.IOData.CurrentViewSettings;

                if (selectPrezzarioIdWnd.IOData.CurrentViewSettings != null)
                    viewSettings = selectPrezzarioIdWnd.IOData.CurrentViewSettings;

                selectedItems = selectPrezzarioIdWnd.IOData.PrezzarioItemSelectedIds;

                externalPrezzarioFileName = selectPrezzarioIdWnd.IOData.ExternalPrezzarioFileName;

                res = true;
            }

            if (updateItemsOnClose)
                OnPrezzarioItemsChanged(new EventArgs());

            //EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //MainOperation.UpdateEntityTypesView(entTypesToUpdate);

            return res;
        }

        public bool SelectPrezzarioWindow(ref string externalPrezzarioFileName, string title)
        {

            int rit = 0;
            string entityTypeKey = PrezzarioItemType.CreateKey();

            SelectPrezzarioWindow selectPrezzarioWnd = new SelectPrezzarioWindow();
            selectPrezzarioWnd.SourceInitialized += (x, y) => selectPrezzarioWnd.HideMinimizeAndMaximizeButtons();
            selectPrezzarioWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectPrezzarioWnd.Title = title;
            selectPrezzarioWnd.IOData.AllowPrezzariEsterni = true;
            selectPrezzarioWnd.IOData.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectPrezzarioWnd.IOData.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;
            selectPrezzarioWnd.IOData.ExternalPrezzarioFileName = externalPrezzarioFileName;

            selectPrezzarioWnd.IOData.DataService = DataService;
            selectPrezzarioWnd.IOData.WindowService = this;
            selectPrezzarioWnd.IOData.ModelActionsStack = ModelActionsStack;
            selectPrezzarioWnd.IOData.MainOperation = MainOperation;
            selectPrezzarioWnd.Init();

            bool res = false;

            if (selectPrezzarioWnd.ShowDialog() == true)
            {
                externalPrezzarioFileName = selectPrezzarioWnd.IOData.ExternalPrezzarioFileName;
                res = true;
            }

            return res;
        }

        #endregion

        #region Capitoli

        public event EventHandler CapitoliItemsChanged;
        protected void OnCapitoliItemsChanged(EventArgs e)
        {
            CapitoliItemsChanged?.Invoke(this, e);
        }

        public int FilterByCapitoliIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = CapitoliItemType.CreateKey();

            DataService.Suspended = true;
            FilterByCapitoliIdsWindow filterByCapitoliIdsWindow = new FilterByCapitoliIdsWindow();
            filterByCapitoliIdsWindow.SourceInitialized += (x, y) => filterByCapitoliIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByCapitoliIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByCapitoliIdsWindow.DataService = DataService;
            filterByCapitoliIdsWindow.WindowService = this;
            filterByCapitoliIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByCapitoliIdsWindow.MainOperation = MainOperation;

            filterByCapitoliIdsWindow.CurrentCapitoliId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByCapitoliIdsWindow.Title = title;
            filterByCapitoliIdsWindow.AllowNoSelection = true;
            filterByCapitoliIdsWindow.CapitoliItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByCapitoliIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByCapitoliIdsWindow.Init();

            if (filterByCapitoliIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByCapitoliIdsWindow.CapitoliItemSelectedIds;
                //if (filterByCapitoliIdsWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }

        public bool SelectCapitoliIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            string entityTypeKey = CapitoliItemType.CreateKey();

            SelectCapitoliIdsWindow selectCapitoliIdWnd = new SelectCapitoliIdsWindow();
            selectCapitoliIdWnd.SourceInitialized += (x, y) => selectCapitoliIdWnd.HideMinimizeAndMaximizeButtons();
            selectCapitoliIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectCapitoliIdWnd.Title = title;
            selectCapitoliIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;//isSingleSelection;
            selectCapitoliIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectCapitoliIdWnd.CapitoliItemSelectedIds = selectedItems;
            selectCapitoliIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectCapitoliIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectCapitoliIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectCapitoliIdWnd.DataService = DataService;
            selectCapitoliIdWnd.WindowService = this;
            selectCapitoliIdWnd.ModelActionsStack = ModelActionsStack;
            selectCapitoliIdWnd.MainOperation = MainOperation;
            selectCapitoliIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectCapitoliIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectCapitoliIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectCapitoliIdWnd.CurrentViewSettings;

                selectedItems = selectCapitoliIdWnd.CapitoliItemSelectedIds;

                res = true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnCapitoliItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }


        #endregion

        #region Elementi
        public event EventHandler ElementiItemsChanged;
        protected void OnElementiItemsChanged(EventArgs e)
        {
            ElementiItemsChanged?.Invoke(this, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="windowService"></param>
        /// <param name="selectedItems"></param>
        /// <param name="title"></param>
        /// <returns>0: nothing, 1: Find, 2:Filter</returns>
        public int FilterByElementiIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = BuiltInCodes.EntityType.Elementi;

            DataService.Suspended = true;
            FilterByElementiIdsWindow filterByElementiIdsWindow = new FilterByElementiIdsWindow();
            filterByElementiIdsWindow.SourceInitialized += (x, y) => filterByElementiIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByElementiIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            filterByElementiIdsWindow.DataService = DataService;
            filterByElementiIdsWindow.WindowService = this;
            filterByElementiIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByElementiIdsWindow.MainOperation = MainOperation;



            filterByElementiIdsWindow.CurrentElementoId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByElementiIdsWindow.Title = title;
            filterByElementiIdsWindow.AllowNoSelection = true;
            filterByElementiIdsWindow.ElementiItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByElementiIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByElementiIdsWindow.Init();

            if (filterByElementiIdsWindow.ShowDialog() == true)
            {

                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, filterByElementiIdsWindow.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = filterByElementiIdsWindow.CurrentViewSettings;


                selectedItems = filterByElementiIdsWindow.ElementiItemSelectedIds;
                //if (filterByElementiIdsWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }


        public bool SelectElementiIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            //EntityType entType = dataService.GetEntityTypes()[BuiltInCodes.EntityType.Elementi];
            //string entityTypeKey = entType.GetKey();
            string entityTypeKey = ElementiItemType.CreateKey();

            SelectElementiIdsWindow selectElementiIdWnd = new SelectElementiIdsWindow();
            selectElementiIdWnd.SourceInitialized += (x, y) => selectElementiIdWnd.HideMinimizeAndMaximizeButtons();
            selectElementiIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            selectElementiIdWnd.DataService = DataService;
            selectElementiIdWnd.WindowService = this;
            selectElementiIdWnd.ModelActionsStack = ModelActionsStack;
            selectElementiIdWnd.MainOperation = MainOperation;

            selectElementiIdWnd.Title = title;
            selectElementiIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; //isSingleSelection;
            selectElementiIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectElementiIdWnd.ElementiItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectElementiIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectElementiIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();
            

            selectElementiIdWnd.Owner = System.Windows.Application.Current.MainWindow;
            if (selectElementiIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectElementiIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectElementiIdWnd.CurrentViewSettings;

                selectedItems = selectElementiIdWnd.ElementiItemSelectedIds;

                res = true;
            }
            
            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnElementiItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}
            

            return res;
        }
        #endregion

        #region Report
        public event EventHandler ReportItemsChanged;
        protected void OnReportItemsChanged(EventArgs e)
        {
            ReportItemsChanged?.Invoke(this, e);
        }

        public int FilterByReportIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = ReportItemType.CreateKey();

            DataService.Suspended = true;
            FilterByReportIdsWindow filterByReportIdsWindow = new FilterByReportIdsWindow();
            filterByReportIdsWindow.SourceInitialized += (x, y) => filterByReportIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByReportIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByReportIdsWindow.DataService = DataService;
            filterByReportIdsWindow.WindowService = this;
            filterByReportIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByReportIdsWindow.MainOperation = MainOperation;

            filterByReportIdsWindow.CurrentReportId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByReportIdsWindow.Title = title;
            filterByReportIdsWindow.AllowNoSelection = true;
            filterByReportIdsWindow.ReportItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByReportIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByReportIdsWindow.Init();

            if (filterByReportIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByReportIdsWindow.ReportItemSelectedIds;
                //if (filterByReportIdsWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }

        public bool SelectReportIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            //EntityType entType = dataService.GetEntityTypes()[BuiltInCodes.EntityType.Elementi];
            //string entityTypeKey = entType.GetKey();
            string entityTypeKey = ReportItemType.CreateKey();

            StampeWpf.Report.SelectReportIdsWindow selectReportIdWnd = new StampeWpf.Report.SelectReportIdsWindow();
            selectReportIdWnd.SourceInitialized += (x, y) => selectReportIdWnd.HideMinimizeAndMaximizeButtons();
            selectReportIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            selectReportIdWnd.DataService = DataService;
            selectReportIdWnd.WindowService = this;
            selectReportIdWnd.ModelActionsStack = ModelActionsStack;
            selectReportIdWnd.MainOperation = MainOperation;

            selectReportIdWnd.Title = title;
            selectReportIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; //isSingleSelection;
            selectReportIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectReportIdWnd.ReportItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectReportIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectReportIdWnd.Init();

            selectReportIdWnd.Owner = System.Windows.Application.Current.MainWindow;
            if (selectReportIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectReportIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectReportIdWnd.CurrentViewSettings;

                selectedItems = selectReportIdWnd.ReportItemSelectedIds;

                OnReportItemsChanged(new EventArgs());
                return true;
            }
            else
            {
                OnReportItemsChanged(new EventArgs());
            }

            return false;
        }

        #endregion Report

        #region Documenti

        public event EventHandler DocumentiItemsChanged;
        protected void OnDocumentiItemsChanged(EventArgs e)
        {
            DocumentiItemsChanged?.Invoke(this, e);
        }

        public bool SelectDocumentiIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            string entityTypeKey = DocumentiItemType.CreateKey();

            SelectDocumentiIdsWindow selectDocumentiIdWnd = new SelectDocumentiIdsWindow();
            selectDocumentiIdWnd.SourceInitialized += (x, y) => selectDocumentiIdWnd.HideMinimizeAndMaximizeButtons();
            selectDocumentiIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectDocumentiIdWnd.Title = title;
            selectDocumentiIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; //isSingleSelection;
            selectDocumentiIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectDocumentiIdWnd.DocumentiItemSelectedIds = selectedItems;
            //selectDocumentiIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            //selectDocumentiIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectDocumentiIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectDocumentiIdWnd.DataService = DataService;
            selectDocumentiIdWnd.WindowService = this;
            selectDocumentiIdWnd.ModelActionsStack = ModelActionsStack;
            selectDocumentiIdWnd.MainOperation = MainOperation;
            selectDocumentiIdWnd.Init();

            if (selectDocumentiIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectDocumentiIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectDocumentiIdWnd.CurrentViewSettings;

                selectedItems = selectDocumentiIdWnd.DocumentiItemSelectedIds;

                OnDocumentiItemsChanged(new EventArgs());
                return true;
            }
            else
            {
                OnDocumentiItemsChanged(new EventArgs());
            }

            return false;
        }



        #endregion documenti

        #region Elenco Attivita

        public event EventHandler ElencoAttivitaItemsChanged;
        protected void OnElencoAttivitaItemsChanged(EventArgs e)
        {
            ElencoAttivitaItemsChanged?.Invoke(this, e);
        }

        public bool SelectElencoAttivitaIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options, EntityTypeViewSettings viewSettings)
        {
            int rit = 0;
            string entityTypeKey = ElencoAttivitaItemType.CreateKey();

            SelectElencoAttivitaIdsWindow selectElencoAttivitaIdWnd = new SelectElencoAttivitaIdsWindow();
            selectElencoAttivitaIdWnd.SourceInitialized += (x, y) => selectElencoAttivitaIdWnd.HideMinimizeAndMaximizeButtons();
            selectElencoAttivitaIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectElencoAttivitaIdWnd.Title = title;
            selectElencoAttivitaIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; //isSingleSelection;
            selectElencoAttivitaIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectElencoAttivitaIdWnd.ElencoAttivitaItemSelectedIds = selectedItems;
            selectElencoAttivitaIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectElencoAttivitaIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;


            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectElencoAttivitaIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectElencoAttivitaIdWnd.DataService = DataService;
            selectElencoAttivitaIdWnd.WindowService = this;
            selectElencoAttivitaIdWnd.ModelActionsStack = ModelActionsStack;
            selectElencoAttivitaIdWnd.MainOperation = MainOperation;
            selectElencoAttivitaIdWnd.ViewSettings = viewSettings;
            selectElencoAttivitaIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectElencoAttivitaIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectElencoAttivitaIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectElencoAttivitaIdWnd.CurrentViewSettings;

                selectedItems = selectElencoAttivitaIdWnd.ElencoAttivitaItemSelectedIds;
                res = true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnElencoAttivitaItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }

        public int FilterByElencoAttivitaIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = ElencoAttivitaItemType.CreateKey();

            DataService.Suspended = true;
            FilterByElencoAttivitaIdsWindow filterByElencoAttivitaIdsWindow = new FilterByElencoAttivitaIdsWindow();
            filterByElencoAttivitaIdsWindow.SourceInitialized += (x, y) => filterByElencoAttivitaIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByElencoAttivitaIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByElencoAttivitaIdsWindow.DataService = DataService;
            filterByElencoAttivitaIdsWindow.WindowService = this;
            filterByElencoAttivitaIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByElencoAttivitaIdsWindow.MainOperation = MainOperation;

            filterByElencoAttivitaIdsWindow.CurrentElencoAttivitaId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByElencoAttivitaIdsWindow.Title = title;
            filterByElencoAttivitaIdsWindow.AllowNoSelection = true;
            filterByElencoAttivitaIdsWindow.ElencoAttivaItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByElencoAttivitaIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByElencoAttivitaIdsWindow.Init();

            if (filterByElencoAttivitaIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByElencoAttivitaIdsWindow.ElencoAttivaItemSelectedIds;
                return 1;
            }

            return 0;

        }
        #endregion

        #region WBS

        public event EventHandler WBSItemsChanged;
        protected void OnWBSItemsChanged(EventArgs e)
        {
            WBSItemsChanged?.Invoke(this, e);
        }

        public int FilterByWBSIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = WBSItemType.CreateKey();

            DataService.Suspended = true;
            FilterByWBSIdsWindow filterByWBSIdsWindow = new FilterByWBSIdsWindow();
            filterByWBSIdsWindow.SourceInitialized += (x, y) => filterByWBSIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByWBSIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByWBSIdsWindow.DataService = DataService;
            filterByWBSIdsWindow.WindowService = this;
            filterByWBSIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByWBSIdsWindow.MainOperation = MainOperation;

            filterByWBSIdsWindow.CurrentWBSId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByWBSIdsWindow.Title = title;
            filterByWBSIdsWindow.AllowNoSelection = true;
            filterByWBSIdsWindow.WBSItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByWBSIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByWBSIdsWindow.Init();

            if (filterByWBSIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByWBSIdsWindow.WBSItemSelectedIds;
                //if (filterByWBSIdsWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }

        public bool SelectWBSIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            string entityTypeKey = WBSItemType.CreateKey();

            SelectWBSIdsWindow selectWBSIdWnd = new SelectWBSIdsWindow();
            selectWBSIdWnd.SourceInitialized += (x, y) => selectWBSIdWnd.HideMinimizeAndMaximizeButtons();
            selectWBSIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectWBSIdWnd.Title = title;
            selectWBSIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;
            selectWBSIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection;
            selectWBSIdWnd.WBSItemSelectedIds = selectedItems;
            selectWBSIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectWBSIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectWBSIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectWBSIdWnd.DataService = DataService;
            selectWBSIdWnd.WindowService = this;
            selectWBSIdWnd.ModelActionsStack = ModelActionsStack;
            selectWBSIdWnd.MainOperation = MainOperation;
            selectWBSIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectWBSIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectWBSIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectWBSIdWnd.CurrentViewSettings;

                selectedItems = selectWBSIdWnd.WBSItemSelectedIds;
                res = true;
            }


            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnWBSItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}


            return res;
        }


        #endregion

        #region Stili
        public event EventHandler StiliItemsChanged;
        protected void OnStiliItemsChanged(EventArgs e)
        {
            StiliItemsChanged?.Invoke(this, e);
        }

        public bool SelectStiliIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            //EntityType entType = dataService.GetEntityTypes()[BuiltInCodes.EntityType.Elementi];
            //string entityTypeKey = entType.GetKey();
            string entityTypeKey = StiliItemType.CreateKey();

            SelectStiliIdsWindow selectStiliIdWnd = new SelectStiliIdsWindow();
            selectStiliIdWnd.SourceInitialized += (x, y) => selectStiliIdWnd.HideMinimizeAndMaximizeButtons();
            selectStiliIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            selectStiliIdWnd.DataService = DataService;
            selectStiliIdWnd.WindowService = this;
            selectStiliIdWnd.ModelActionsStack = ModelActionsStack;
            selectStiliIdWnd.MainOperation = MainOperation;

            selectStiliIdWnd.Title = title;
            selectStiliIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; //isSingleSelection;
            selectStiliIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectStiliIdWnd.StiliItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectStiliIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectStiliIdWnd.Init();

            selectStiliIdWnd.Owner = System.Windows.Application.Current.MainWindow;
            if (selectStiliIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectStiliIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectStiliIdWnd.CurrentViewSettings;

                selectedItems = selectStiliIdWnd.StiliItemSelectedIds;

                OnStiliItemsChanged(new EventArgs());
                return true;
            }
            else
            {
                OnStiliItemsChanged(new EventArgs());
            }

            return false;
        }
        #endregion Stili


        #region Divisione
        public event EventHandler DivisioneItemsChanged;
        protected void OnDivisioneItemsChanged(DivisioneItemsChangedEventArgs e)
        {
            DivisioneItemsChanged?.Invoke(this, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="windowService"></param>
        /// <param name="divId"></param>
        /// <param name="selectedItems"></param>
        /// <param name="title"></param>
        /// <returns>0: nothing, 1: Find, 2:Filter</returns>
        public int FilterByDivisioneIdsWindow(Guid divId, ref List<Guid> selectedItems, string title)
        {
            DivisioneItemType divType = new EntitiesHelper(DataService).GetDivisioneTypeById(divId);

            string entityTypeKey = DivisioneItemType.CreateKey(divId);

            DataService.Suspended = true;
            FilterByDivisioneItemIdWindow filterByDivisioneItemIdWindow = new FilterByDivisioneItemIdWindow();
            filterByDivisioneItemIdWindow.SourceInitialized += (x, y) => filterByDivisioneItemIdWindow.HideMinimizeAndMaximizeButtons();
            filterByDivisioneItemIdWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByDivisioneItemIdWindow.DataService = DataService;
            filterByDivisioneItemIdWindow.WindowService = this;
            filterByDivisioneItemIdWindow.ModelActionsStack = ModelActionsStack;
            filterByDivisioneItemIdWindow.MainOperation = MainOperation;

            filterByDivisioneItemIdWindow.CurrentDivisioneItemId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByDivisioneItemIdWindow.Title = title;
            filterByDivisioneItemIdWindow.AllowNoSelection = true;
            filterByDivisioneItemIdWindow.DivisioneItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByDivisioneItemIdWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByDivisioneItemIdWindow.Init(divId);

            if (filterByDivisioneItemIdWindow.ShowDialog() == true)
            {
                selectedItems = filterByDivisioneItemIdWindow.DivisioneItemSelectedIds;
                //if (filterByDivisioneItemIdWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }

        public bool SelectDivisioneIdsWindow(Guid divId, ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options, AttributoRiferimento senderAttRif)
        {
            DivisioneItemType divType = new EntitiesHelper(DataService).GetDivisioneTypeById(divId);

            string entityTypeKey = DivisioneItemType.CreateKey(divId);

            SelectDivisioneItemIdWindow selectDivisioneItemIdWnd = new SelectDivisioneItemIdWindow();
            selectDivisioneItemIdWnd.SourceInitialized += (x, y) => selectDivisioneItemIdWnd.HideMinimizeAndMaximizeButtons();
            selectDivisioneItemIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            selectDivisioneItemIdWnd.DataService = DataService;
            selectDivisioneItemIdWnd.WindowService = this;
            selectDivisioneItemIdWnd.ModelActionsStack = ModelActionsStack;
            selectDivisioneItemIdWnd.MainOperation = MainOperation;

            selectDivisioneItemIdWnd.Title = title;
            selectDivisioneItemIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;//isSingleSelection;
            selectDivisioneItemIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectDivisioneItemIdWnd.DivisioneItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectDivisioneItemIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectDivisioneItemIdWnd.SenderAttRif = senderAttRif;

            if (senderAttRif != null && senderAttRif.EntityTypeKey == BuiltInCodes.EntityType.Computo)
                selectDivisioneItemIdWnd.IsItemsSummarized = true;


            selectDivisioneItemIdWnd.Init(DataService, this, divId, ModelActionsStack);

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectDivisioneItemIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectDivisioneItemIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectDivisioneItemIdWnd.CurrentViewSettings;

                selectedItems = selectDivisioneItemIdWnd.DivisioneItemSelectedIds;
                res = true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnDivisioneItemsChanged(new DivisioneItemsChangedEventArgs() { Id = divId });

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(divType.GetKey());
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }

        #endregion

        #region Contatti
        public event EventHandler ContattiItemsChanged;
        protected void OnContattiItemsChanged(EventArgs e)
        {
            ContattiItemsChanged?.Invoke(this, e);
        }

        public bool SelectContattiIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            int rit = 0;
            string entityTypeKey = ContattiItemType.CreateKey();

            SelectContattiIdsWindow selectContattiIdWnd = new SelectContattiIdsWindow();
            selectContattiIdWnd.SourceInitialized += (x, y) => selectContattiIdWnd.HideMinimizeAndMaximizeButtons();
            selectContattiIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectContattiIdWnd.Title = title;
            selectContattiIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;//isSingleSelection;
            selectContattiIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectContattiIdWnd.ContattiItemSelectedIds = selectedItems;
            selectContattiIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectContattiIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectContattiIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectContattiIdWnd.DataService = DataService;
            selectContattiIdWnd.WindowService = this;
            selectContattiIdWnd.ModelActionsStack = ModelActionsStack;
            selectContattiIdWnd.MainOperation = MainOperation;
            selectContattiIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectContattiIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectContattiIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectContattiIdWnd.CurrentViewSettings;

                selectedItems = selectContattiIdWnd.ContattiItemSelectedIds;
                res = true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnContattiItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }

        public int FilterByContattiIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = ContattiItemType.CreateKey();

            DataService.Suspended = true;
            FilterByContattiIdsWindow filterByContattiIdsWindow = new FilterByContattiIdsWindow();
            filterByContattiIdsWindow.SourceInitialized += (x, y) => filterByContattiIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByContattiIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByContattiIdsWindow.DataService = DataService;
            filterByContattiIdsWindow.WindowService = this;
            filterByContattiIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByContattiIdsWindow.MainOperation = MainOperation;

            filterByContattiIdsWindow.CurrentContattiId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByContattiIdsWindow.Title = title;
            filterByContattiIdsWindow.AllowNoSelection = true;
            filterByContattiIdsWindow.ContattiItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByContattiIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByContattiIdsWindow.Init();

            if (filterByContattiIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByContattiIdsWindow.ContattiItemSelectedIds;
                return 1;
            }

            return 0;

        }
        #endregion

        #region Computo

        public event EventHandler ComputoItemsChanged;
        protected void OnComputoItemsChanged(EventArgs e)
        {
            ComputoItemsChanged?.Invoke(this, e);
        }

        internal bool WebSaveProjectWnd(out Guid operaId, out Guid progettoId, ref string nomeProgetto)
        {

            operaId = Guid.Empty;
            progettoId = Guid.Empty;
            //nomeProgetto = string.Empty;

            WebSelectProjectWnd wnd = new WebSelectProjectWnd();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.DataService = DataService;
            wnd.WindowService = this;
            wnd.ModelActionsStack = ModelActionsStack;
            wnd.MainOperation = MainOperation;
            wnd.NomeProgetto = nomeProgetto;
            wnd.Type = WebSelectProjectWndType.Save;

            wnd.Init();

            if (wnd.ShowDialog() == true)
            {
                operaId = wnd.OperaId;
                nomeProgetto = wnd.NomeProgetto;
                progettoId = wnd.ProgettoId;

                return true;
            }
            return false;
        }

        internal bool WebOpenProjectWnd(out Guid operaId, out Guid progettoId, ref string nomeProgetto)
        {

            operaId = Guid.Empty;
            progettoId = Guid.Empty;
            //nomeProgetto = string.Empty;

            WebSelectProjectWnd wnd = new WebSelectProjectWnd();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.DataService = DataService;
            wnd.WindowService = this;
            wnd.ModelActionsStack = ModelActionsStack;
            wnd.MainOperation = MainOperation;
            wnd.NomeProgetto = nomeProgetto;
            wnd.Type = WebSelectProjectWndType.Open;

            wnd.Init();

            if (wnd.ShowDialog() == true)
            {
                operaId = wnd.OperaId;
                nomeProgetto = wnd.NomeProgetto;
                progettoId = wnd.ProgettoId;

                return true;
            }
            return false;
        }

        public bool SelectComputoIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            bool res = false;

            //EntityType entType = dataService.GetEntityTypes()[BuiltInCodes.EntityType.Elementi];
            //string entityTypeKey = entType.GetKey();
            string entityTypeKey = ComputoItemType.CreateKey();

            SelectComputoIdsWindow selectComputoIdWnd = new SelectComputoIdsWindow();
            selectComputoIdWnd.SourceInitialized += (x, y) => selectComputoIdWnd.HideMinimizeAndMaximizeButtons();
            selectComputoIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            selectComputoIdWnd.DataService = DataService;
            selectComputoIdWnd.WindowService = this;
            selectComputoIdWnd.ModelActionsStack = ModelActionsStack;
            selectComputoIdWnd.MainOperation = MainOperation;

            selectComputoIdWnd.Title = title;
            selectComputoIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection; //isSingleSelection;
            selectComputoIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectComputoIdWnd.ComputoItemSelectedIds = selectedItems;
            if ((options & SelectIdsWindowOptions.NotAllowAcceptSelection) == SelectIdsWindowOptions.NotAllowAcceptSelection)
                selectComputoIdWnd.AllowAcceptSelection = false;

            //selectComputoIdWnd.DataService.IsReadOnly = true;

            selectComputoIdWnd.Init();

            selectComputoIdWnd.Owner = System.Windows.Application.Current.MainWindow;

            
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectComputoIdWnd.ShowDialog() == true)
            {
                selectedItems = selectComputoIdWnd.ComputoItemSelectedIds;

                OnComputoItemsChanged(new EventArgs());
                res = true;
            }

            //selectComputoIdWnd.DataService.IsReadOnly = false;
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="windowService"></param>
        /// <param name="selectedItems"></param>
        /// <param name="title"></param>
        /// <returns>0: nothing, 1: Find, 2:Filter</returns>
        public int FilterByComputoIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = BuiltInCodes.EntityType.Computo;

            DataService.Suspended = true;
            FilterByComputoIdsWindow filterByComputoIdsWindow = new FilterByComputoIdsWindow();
            filterByComputoIdsWindow.SourceInitialized += (x, y) => filterByComputoIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByComputoIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            filterByComputoIdsWindow.DataService = DataService;
            filterByComputoIdsWindow.WindowService = this;
            filterByComputoIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByComputoIdsWindow.MainOperation = MainOperation;



            filterByComputoIdsWindow.CurrentElementoId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByComputoIdsWindow.Title = title;
            filterByComputoIdsWindow.AllowNoSelection = true;
            filterByComputoIdsWindow.ComputoItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByComputoIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByComputoIdsWindow.Init();

            if (filterByComputoIdsWindow.ShowDialog() == true)
            {

                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, filterByComputoIdsWindow.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = filterByComputoIdsWindow.CurrentViewSettings;


                selectedItems = filterByComputoIdsWindow.ComputoItemSelectedIds;
                //if (filterByComputoIdsWindow.IsFilter)
                //    return 2;
                //else
                return 1;
            }

            return 0;

        }

        #endregion Computo

        #region Calendari
        public event EventHandler CalendariItemsChanged;
        protected void OnCalendariItemsChanged(EventArgs e)
        {
            CalendariItemsChanged?.Invoke(this, e);
        }

        public bool SelectCalendariIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            int rit = 0;
            string entityTypeKey = CalendariItemType.CreateKey();

            SelectCalendariIdsWindow selectCalendariIdWnd = new SelectCalendariIdsWindow();
            selectCalendariIdWnd.SourceInitialized += (x, y) => selectCalendariIdWnd.HideMinimizeAndMaximizeButtons();
            selectCalendariIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectCalendariIdWnd.Title = title;
            selectCalendariIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;//isSingleSelection;
            selectCalendariIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectCalendariIdWnd.CalendariItemSelectedIds = selectedItems;
            selectCalendariIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectCalendariIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectCalendariIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectCalendariIdWnd.DataService = DataService;
            selectCalendariIdWnd.WindowService = this;
            selectCalendariIdWnd.ModelActionsStack = ModelActionsStack;
            selectCalendariIdWnd.MainOperation = MainOperation;
            selectCalendariIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectCalendariIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectCalendariIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectCalendariIdWnd.CurrentViewSettings;

                selectedItems = selectCalendariIdWnd.CalendariItemSelectedIds;

                return true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnCalendariItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }

        public int FilterByCalendariIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = CalendariItemType.CreateKey();

            DataService.Suspended = true;
            FilterByCalendariIdsWindow filterByCalendariIdsWindow = new FilterByCalendariIdsWindow();
            filterByCalendariIdsWindow.SourceInitialized += (x, y) => filterByCalendariIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByCalendariIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByCalendariIdsWindow.DataService = DataService;
            filterByCalendariIdsWindow.WindowService = this;
            filterByCalendariIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByCalendariIdsWindow.MainOperation = MainOperation;

            filterByCalendariIdsWindow.CurrentCalendariId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByCalendariIdsWindow.Title = title;
            filterByCalendariIdsWindow.AllowNoSelection = true;
            filterByCalendariIdsWindow.CalendariItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByCalendariIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByCalendariIdsWindow.Init();

            if (filterByCalendariIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByCalendariIdsWindow.CalendariItemSelectedIds;
                return 1;
            }

            return 0;

        }

        #endregion

        #region Allegati
        public event EventHandler AllegatiItemsChanged;
        protected void OnAllegatiItemsChanged(EventArgs e)
        {
            AllegatiItemsChanged?.Invoke(this, e);
        }

        public bool SelectAllegatiIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            int rit = 0;
            string entityTypeKey = AllegatiItemType.CreateKey();

            SelectAllegatiIdsWindow selectAllegatiIdWnd = new SelectAllegatiIdsWindow();
            selectAllegatiIdWnd.SourceInitialized += (x, y) => selectAllegatiIdWnd.HideMinimizeAndMaximizeButtons();
            selectAllegatiIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectAllegatiIdWnd.Title = title;
            selectAllegatiIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;//isSingleSelection;
            selectAllegatiIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectAllegatiIdWnd.AllegatiItemSelectedIds = selectedItems;
            selectAllegatiIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectAllegatiIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectAllegatiIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectAllegatiIdWnd.DataService = DataService;
            selectAllegatiIdWnd.WindowService = this;
            selectAllegatiIdWnd.ModelActionsStack = ModelActionsStack;
            selectAllegatiIdWnd.MainOperation = MainOperation;
            selectAllegatiIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectAllegatiIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectAllegatiIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectAllegatiIdWnd.CurrentViewSettings;

                selectedItems = selectAllegatiIdWnd.AllegatiItemSelectedIds;


                res = true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnAllegatiItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }

        public int FilterByAllegatiIdsWindow(ref List<Guid> selectedItems, string title)
        {
            string entityTypeKey = AllegatiItemType.CreateKey();

            DataService.Suspended = true;
            FilterByAllegatiIdsWindow filterByAllegatiIdsWindow = new FilterByAllegatiIdsWindow();
            filterByAllegatiIdsWindow.SourceInitialized += (x, y) => filterByAllegatiIdsWindow.HideMinimizeAndMaximizeButtons();
            filterByAllegatiIdsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            filterByAllegatiIdsWindow.DataService = DataService;
            filterByAllegatiIdsWindow.WindowService = this;
            filterByAllegatiIdsWindow.ModelActionsStack = ModelActionsStack;
            filterByAllegatiIdsWindow.MainOperation = MainOperation;

            filterByAllegatiIdsWindow.CurrentAllegatiId = selectedItems.Any() ? selectedItems.First() : Guid.Empty;
            filterByAllegatiIdsWindow.Title = title;
            filterByAllegatiIdsWindow.AllowNoSelection = true;
            filterByAllegatiIdsWindow.AllegatiItemSelectedIds = selectedItems;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                filterByAllegatiIdsWindow.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            filterByAllegatiIdsWindow.Init();

            if (filterByAllegatiIdsWindow.ShowDialog() == true)
            {
                selectedItems = filterByAllegatiIdsWindow.AllegatiItemSelectedIds;
                return 1;
            }

            return 0;

        }

        #endregion

        #region Tag

        public event EventHandler TagItemsChanged;
        protected void OnTagItemsChanged(EventArgs e)
        {
            TagItemsChanged?.Invoke(this, e);
        }

        public bool SelectTagIdsWindow(ref List<Guid> selectedItems, string title, SelectIdsWindowOptions options)
        {
            int rit = 0;
            string entityTypeKey = TagItemType.CreateKey();

            SelectTagIdsWindow selectTagIdWnd = new SelectTagIdsWindow();
            selectTagIdWnd.SourceInitialized += (x, y) => selectTagIdWnd.HideMinimizeAndMaximizeButtons();
            selectTagIdWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            selectTagIdWnd.Title = title;
            selectTagIdWnd.IsSingleSelection = (options & SelectIdsWindowOptions.IsSingleSelection) == SelectIdsWindowOptions.IsSingleSelection;//isSingleSelection;
            selectTagIdWnd.AllowNoSelection = (options & SelectIdsWindowOptions.AllowNoSelection) == SelectIdsWindowOptions.AllowNoSelection; //allowNoSelection;
            selectTagIdWnd.TagItemSelectedIds = selectedItems;
            selectTagIdWnd.NoteCalculatorFunction = CalculatorFunctions[NoteCalculatorFunction.Name] as NoteCalculatorFunction;
            selectTagIdWnd.EPCalculatorFunction = CalculatorFunctions[EPCalculatorFunction.Name] as EPCalculatorFunction;

            if (DefaultViewSettings.ContainsKey(entityTypeKey))
                selectTagIdWnd.CurrentViewSettings = DefaultViewSettings[entityTypeKey];

            selectTagIdWnd.DataService = DataService;
            selectTagIdWnd.WindowService = this;
            selectTagIdWnd.ModelActionsStack = ModelActionsStack;
            selectTagIdWnd.MainOperation = MainOperation;
            selectTagIdWnd.Init();

            bool res = false;
            //int modelActionCount = ModelActionsStack.GetCount();

            if (selectTagIdWnd.ShowDialog() == true)
            {
                if (!DefaultViewSettings.ContainsKey(entityTypeKey))
                    DefaultViewSettings.Add(entityTypeKey, selectTagIdWnd.CurrentViewSettings);
                else
                    DefaultViewSettings[entityTypeKey] = selectTagIdWnd.CurrentViewSettings;

                selectedItems = selectTagIdWnd.TagItemSelectedIds;
                res = true;
            }

            //int modelActionCount2 = ModelActionsStack.GetCount();
            //if (res || (modelActionCount2 > modelActionCount))
                OnTagItemsChanged(new EventArgs());

            //if (modelActionCount2 > modelActionCount)
            //{
            //    EntitiesHelper entsHelper = new EntitiesHelper(DataService);
            //    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(entityTypeKey);
            //    MainOperation.UpdateEntityTypesView(entTypesToUpdate);
            //}

            return res;
        }

        #endregion

        public bool AttributoFilterDetailWindow(EntitiesListMasterDetailView master, AttributoFilterData attFilterData)
        {
            AttributoFilterDetailWindow attFilterDetailWindow = new AttributoFilterDetailWindow(master, attFilterData);
            attFilterDetailWindow.SourceInitialized += (x, y) => attFilterDetailWindow.HideMinimizeAndMaximizeButtons();
            attFilterDetailWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (attFilterDetailWindow.ShowDialog() == true)
            {
                return true;
            }
            return false;
        }

        public bool EditAttributoMultiValoreItemWindow(EntitiesListMasterDetailView master, ValoreTestoCollectionItemView itemView)
        {
            //Window mainWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.Name.Contains("JoinWindow"));
            Window mainWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            AttributoTestoCollectionEditWindow attributoTestoCollectionEditWindow = new AttributoTestoCollectionEditWindow();
            attributoTestoCollectionEditWindow.SourceInitialized += (x, y) => attributoTestoCollectionEditWindow.HideMinimizeAndMaximizeButtons();
            attributoTestoCollectionEditWindow.Master = master;
            attributoTestoCollectionEditWindow.ItemView = itemView;
            attributoTestoCollectionEditWindow.Owner = mainWindow;
            attributoTestoCollectionEditWindow.Show();

            return false;
        }


        public IEntityWindowService CreateWindowService(ClientDataService dataService, ModelActionsStack modelActionsStack, IMainOperation mainOperation)
        {
            WindowService ws = new WindowService();
            ws.DataService = dataService;
            ws.ModelActionsStack = modelActionsStack;
            ws.MainOperation = mainOperation;
            ws.CalculatorFunctions = this.CalculatorFunctions;
            return ws;
        }

        public bool EntitiesImportWindow(EntitiesImportStatus importStatus)
        {
            EntitiesImportWindow entitiesImportWindow = new EntitiesImportWindow();
            entitiesImportWindow.SourceInitialized += (x, y) => entitiesImportWindow.HideMinimizeAndMaximizeButtons();
            entitiesImportWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            entitiesImportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            entitiesImportWindow.Init(importStatus);
            if (entitiesImportWindow.ShowDialog() == true)
            {
                return true;
            }

            return false;
        }

        public bool CodiceAttributoWindow(ref string codiceAttributo)
        {

            CodiceAttributoWindow codiceAttributoWindow = new CodiceAttributoWindow();
            codiceAttributoWindow.SourceInitialized += (x, y) => codiceAttributoWindow.HideMinimizeAndMaximizeButtons();
            codiceAttributoWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            codiceAttributoWindow.CodiceAttributo = codiceAttributo;
            if (codiceAttributoWindow.ShowDialog() == true)
            {
                codiceAttributo = codiceAttributoWindow.CodiceAttributo;
                return true;
            }
            return false;
        }

        public bool NewProjectModelWindow(ref string modelFullFileName)
        {
            NewProjectWnd wnd = new NewProjectWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            wnd.ProjectModelView.MainOperation = MainOperation;
            wnd.ProjectModelView.WindowService = this;
            wnd.ProjectModelView.Load();
            if (wnd.ShowDialog() == true)
            {
                modelFullFileName = wnd.ProjectModelView.GetCurrentModelloFullFileName();
                return true;
            }

            return false;
        }

        public bool ImportProjectModelWindow(ref string modelFullFileName)
        {
            ImportProjectModelWnd wnd = new ImportProjectModelWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            wnd.ProjectModelView.MainOperation = MainOperation;
            wnd.ProjectModelView.WindowService = this;
            wnd.ProjectModelView.Load();

            if (wnd.ShowDialog() == true)
            {
                modelFullFileName = wnd.ProjectModelView.GetCurrentModelloFullFileName();
                return true;
            }

            return false;
        }

        public bool SelectNumberFormatsWnd(ref List<string> formats, bool isSingleSelection)
        {

            SelectNumericFormatsWnd wnd = new SelectNumericFormatsWnd();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            wnd.FormatNumeroView.IsSingleSelection = isSingleSelection;
            wnd.FormatNumeroView.DataService = DataService;
            wnd.FormatNumeroView.ModelActionsStack = ModelActionsStack;
            wnd.FormatNumeroView.MainOperation = MainOperation;
            wnd.FormatNumeroView.Init();

            if (wnd.ShowDialog() == true)
            {
                formats = wnd.FormatNumeroView.GetSelectedNumericFormats();
                return true;
            }

            return false;
        }

        public bool AttributoCodingWindow(string entityTypeKey, HashSet<Guid> checkedEntitiesId, int maxDepth, List<int> SelectedLevels)
        {
            AttributoCodingWnd attributoCodingWnd = new AttributoCodingWnd();
            attributoCodingWnd.SourceInitialized += (x, y) => attributoCodingWnd.HideMinimizeAndMaximizeButtons();
            attributoCodingWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            //ESTRAGGO SOLO ATTRIBUTI DI TIPO TESTO
            Dictionary<string, EntityType> entityTypes = DataService.GetEntityTypes();
            EntityType entityType = entityTypes[entityTypeKey];
            List<Attributo> LisTaAttributiTesto = entityType.Attributi.Where(x => x.Value.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo
            && !x.Value.IsInternal && !x.Value.IsValoreReadOnly).ToDictionary(i => i.Key, i => i.Value).Values.ToList();

            //ESTRAGGO SETTING ATTRIBUTI
            ViewSettings viewSettings = DataService.GetViewSettings();
            List<AttributoCoding> attsCoding = viewSettings.EntityTypes[entityTypeKey].Codings;
            EntitiesCodingHelper entsCodingHelper = new EntitiesCodingHelper();
            entsCodingHelper.EntityTypeKey = entityTypeKey;
            entsCodingHelper.DataService = DataService;
            entsCodingHelper.MainOperation = MainOperation;

            AttributoCodingView AttributoCodingView = new AttributoCodingView();
            AttributoCodingView.DataService = DataService;
            AttributoCodingView.MainOperation = MainOperation;
            AttributoCodingView.SelectedLevels = SelectedLevels;
            AttributoCodingView.LisTaAttributiTesto = new List<Attributo>(LisTaAttributiTesto);
            AttributoCodingView.ListaAttributoCoding = new List<AttributoCoding>(attsCoding);
            AttributoCodingView.MaxDepth = maxDepth;
            AttributoCodingView.EntityTypeKey = entityTypeKey;
            AttributoCodingView.EntsCodingHelper = entsCodingHelper;
            AttributoCodingView.Init();

            attributoCodingWnd.DataContext = AttributoCodingView;

            if (attributoCodingWnd.ShowDialog() == true)
            {
                ModelActionsStack.UndoGroupBegin(UndoGroupsName.Coding, entityTypeKey);

                viewSettings.EntityTypes[entityTypeKey].Codings = AttributoCodingView.ListaAttributoCoding;
                DataService.SetViewSettings(viewSettings);
                entsCodingHelper.Run(checkedEntitiesId, AttributoCodingView.ListaAttributoCoding.Where(a => a.AttributoCodice == AttributoCodingView.SelectedAttributeCodice).FirstOrDefault(), AttributoCodingView.SelectedAttributeCodice);

                ModelActionsStack.UndoGroupEnd();
                return true;
            }

            return false;
        }

        public bool SelectAttributoFilterWindow(HashSet<string> entityTypesKey, List<Guid> entsIdToFilter, ref AttributoFilterData attFilterData)
        {

            SetAttributoFilterWnd wnd = new SetAttributoFilterWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.View.DataService = DataService;
            wnd.View.EntityTypesKey = entityTypesKey;
            wnd.View.EntsIdToFilter = entsIdToFilter;
            wnd.View.AttributoFilterData = attFilterData;
            wnd.View.Load();

            if (wnd.ShowDialog() == true)
            {
                attFilterData = wnd.View.AttributoFilterData;
                return true;
            }

            return false;
        }

        public bool SetValoreConditionsWnd(string entityTypesKey, ValoreConditions valCond, bool allowAccept)
        {
            SetValoreConditionsWnd wnd = new SetValoreConditionsWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.ValoreConditionsGroupCtrl.View.DataService = DataService;
            wnd.AcceptButton.IsEnabled = allowAccept;

            EntityType entType = DataService.GetEntityType(entityTypesKey);
            wnd.ValoreConditionsGroupCtrl.View.EntityType = entType;

            wnd.ValoreConditionsGroupCtrl.View.Data = valCond;
            wnd.ValoreConditionsGroupCtrl.View.AND_Limited = true;
            wnd.ValoreConditionsGroupCtrl.View.AllowAsItem = true;
            wnd.ValoreConditionsGroupCtrl.View.Load();

            if (wnd.ShowDialog() == true)
            {
                return true;
            }

            return false;
        }

        public bool SelectAttributoWeekHoursWindow(ref WeekHours WeekHoursData)
        {
            SetAttributoWeekHoursWnd wnd = new SetAttributoWeekHoursWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.View.DataService = DataService;
            if (WeekHoursData != null)
            {
                wnd.View.AttributoWeekHoursData = WeekHoursData.Days;
            }
            else
            {
                WeekHoursData = new WeekHours();
            }

            wnd.View.Load();

            if (wnd.ShowDialog() == true)
            {
                WeekHoursData.Days = wnd.View.AttributoWeekHoursData;
                return true;
            }

            return false;
        }

        public bool SelectAttributoCustomDaysWindow(WeekHours attWeekHours, ref CustomDays CustomDaysData)
        {
            if (attWeekHours == null) return false;

            SetAttributoCustomDayView SetAttributoCustomDayView = new SetAttributoCustomDayView();
            SetAttributoCustomDayView.DataService = DataService;
            SetAttributoCustomDayView.GiorniLavorativi = attWeekHours.Days;

            if (CustomDaysData != null)
            {
                SetAttributoCustomDayView.ListaEccezioni = CustomDaysData.Days;
            }
            else
            {
                CustomDaysData = new CustomDays();
            }

            SetAttributoCustomDayView.Load();
            SetAttributoCustomDayWnd wnd = new SetAttributoCustomDayWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.DataContext = SetAttributoCustomDayView;

            if (wnd.ShowDialog() == true)
            {
                CustomDaysData.Days = wnd.View.ListaEccezioni;
                return true;
            }

            return false;
        }

        public bool SelectAttributoPredecessoriWindow(Guid Guid, ref WBSPredecessors WBSPredecessorsData)
        {
            SetAttributoPredecessoriView SetAttributoPredecessoriView = new SetAttributoPredecessoriView();
            SetAttributoPredecessoriView.DataService = DataService;
            SetAttributoPredecessoriView.Init(Guid);
            SetAttributoPredecessoriWnd wnd = new SetAttributoPredecessoriWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            wnd.DataContext = SetAttributoPredecessoriView;

            if (wnd.ShowDialog() == true)
            {
                WBSPredecessorsData = SetAttributoPredecessoriView.WBSPredecessorsData;
                return true;
            }

            return false;
        }

        public bool EntityHighlightersWnd(string entityTypeKey)
        {
            EntityHighlightersWnd wnd = new EntityHighlightersWnd();
            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();
            wnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            EntityHighlightersView view = wnd.DataContext as EntityHighlightersView;
            view.DataService = DataService;
            view.Load(entityTypeKey);

            if (wnd.ShowDialog() == true)
            {
                return true;
            }
            return false;
        }

        //public bool WebLoginWnd(LoginDto loginDto)
        //{
        //    WebLoginWnd wnd = new WebLoginWnd();
        //    wnd.View.Init(loginDto);

        //    if (wnd.ShowDialog() == true)
        //    {
        //        //loginDto.CodiceCliente = wnd.View.CodiceCliente;
        //        loginDto.Email = wnd.View.Email;
        //        loginDto.Password = wnd.View.PasswordText;
        //        loginDto.RememberMe = wnd.View.RememberMe;

        //        return true;
        //    }
        //    return false;
        //}


    }

    public class DivisioneItemsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Divisione Id changed
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

    }




}
