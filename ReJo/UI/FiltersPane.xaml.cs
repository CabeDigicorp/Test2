
using _3DModelExchange;
using Autodesk.Revit.UI;
using CommonResources;
using ReJo.Utility;
using Syncfusion.Windows.Shared;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;


namespace ReJo.UI
{
    /// <summary>
    /// Interaction logic for FiltersPane.xaml
    /// </summary>
    public partial class FiltersPane : Page, IDockablePaneProvider
    {
        
        public static FiltersPane This { get; private set; }//singleton

        public FiltersPaneView? View => (DataContext as FiltersPaneView);


        public DockablePaneId DockablePaneId = null;


        public FiltersPane()
        {
            InitializeComponent();

            Guid guid = new Guid("2ffc7912-231c-4365-ac53-18068d725545");

            DockablePaneId = new DockablePaneId(guid);

            This = this;
        }


        public static FiltersPane Create()
        {

            return new FiltersPane();
        }

        

        public void Show(bool show)
        {
            try
            {
                //if (!CmdInit.IsInitialized)
                //    return;

                // dockable window id
                var dockablePane = CmdInit.This.UIApplication.GetDockablePane(DockablePaneId);
                if (dockablePane != null)
                {
                    if (show)
                    {
                        dockablePane.Show();


                    }
                    else
                        dockablePane.Hide();

                    
                }
            }
            catch (Exception ex)
            {
                // show error info dialog
                TaskDialog.Show("Info Message", ex.Message);
            }
        }


        internal void Update(HashSet<long>? filtersChanged = null)
        {
            if (!CmdInit.IsInitialized)
                return;


            if (View?.Update(filtersChanged) == true)
               Show(true);
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.VisibleByDefault = false;
            // wpf object with pane's interface
            data.FrameworkElement = this as FrameworkElement;
            // initial state position
            data.InitialState = new DockablePaneState()
            {

                DockPosition = DockPosition.Tabbed,
                TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser,
            };


            //oss: se non si mantiene KeepAlive non funziona più la selezione dal FilterPane
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);


        }

        internal void Dispose()
        {
            var dockablePane = CmdInit.This.UIApplication.GetDockablePane(DockablePaneId);
            if (dockablePane != null)
            {
                dockablePane.Dispose();
            }
        }

        private void OpenFiltersTagWnd_Click(object sender, RoutedEventArgs e)
        {
            TagsData tagsData = CmdInit.This.JoinService.GetCurrentProjectModel3dTags();

            if (tagsData == null)
                return;

            var wnd = WindowManager.CreateFiltersTagWnd();

            wnd.SourceInitialized += (x, y) => wnd.HideMinimizeAndMaximizeButtons();

            IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            Window? window = HwndSource.FromHwnd(revitHandle)?.RootVisual as Window;
            wnd.Owner = window;

            wnd.View.SetTags(tagsData);
            wnd.ShowDialog();

            View?.Update();


        }

        private void OpenRulesWnd_Click(object sender, RoutedEventArgs e)
        {

            if (RulesWnd.This != null)
            {
                RulesWnd.This.Activate();
            }
            else //if (RulesWnd.This == null)
            {   
                
                var filtersData = CmdInit.This.JoinService.GetCurrentProjectModel3dFilters();
                if (filtersData == null)
                    return;


                // Ottieni il button che ha generato l'evento
                Button clickedButton = (Button) sender;

                // Ottieni l'item associato dal DataContext del button
                FilterItemView? filterItem = clickedButton?.DataContext as FilterItemView;
                if (filterItem != null && filterItem.ExistRvtFilter)
                {
                    var filterUniqueId = filterItem.UniqueId;



                    RulesWnd.Create();

                    var ruleErrorsConflict = filterItem.RuleErrors.Where(item =>
                    {
                        if (item is RuleErrorConflict ruleErrConf)
                        {
                            if (item.FilterUniqueId == filterUniqueId)
                                return true;
                            
                        }
                        return false;
                    }).ToList();

                    RulesWnd.This?.View.SetRuleErrorsConflict(ruleErrorsConflict);

                    RulesWnd.This?.View.Load(filterUniqueId, filtersData);
                    CmdInit.This.RulesWndHandler.RaiseExternalEvent();
                }
            }

        }

    }
}
