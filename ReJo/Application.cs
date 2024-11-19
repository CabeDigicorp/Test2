
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommonResources;
using ReJo.UI;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPFLocalizeExtension.Engine;
using RUI = Autodesk.Revit.UI;




namespace ReJo
{
    public class Application : IExternalApplication
    {
        public static String TabReJoName { get; private set; } = "ReJo";

        public static string ExecutingAssemblyPath { get; private set; } = System.Reflection.Assembly.GetExecutingAssembly().Location;
        

        public ExternalEvent FiltersPaneExternalEvent { get; private set; } = null;

        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication application)
        {
            Result rc = Result.Succeeded;

            //Claim License Key  (licenza community) https://www.syncfusion.com/sales/communitylicense                                                                                                                                                              
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHJqVk1hXk5Hd0BLVGpAblJ3T2ZQdVt5ZDU7a15RRnVfR1xrSH9SdERlWnddeQ==;Mgo+DSMBPh8sVXJ1S0R+X1pFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jTH9bd0RiW35YdHRUTw==;ORg4AjUWIQA/Gnt2VFhiQlJPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtSfkRiWH1fdnBXRmk=;MjEwMzAwMkAzMjMxMmUzMjJlMzNvM3gvRmNsRFozY2FXQlZWdWRvMzBlSk1Mb0Z5Y1RXMEtISE0xWS80a1VBPQ==;MjEwMzAwM0AzMjMxMmUzMjJlMzNHYXBJNTNTN203T0YvYjh3eHZHK2FpVW1IR3R5bWpxOEU2cUd2cGZZMTVrPQ==;NRAiBiAaIQQuGjN/V0d+Xk9HfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5Wd01iWH5ccXJQR2BV;MjEwMzAwNUAzMjMxMmUzMjJlMzNvQ2U2VDdoUFljZmdoR0pRU1hHRCtZYWt2TmRucUs0d21ZK3JiNlZCOHhrPQ==;MjEwMzAwNkAzMjMxMmUzMjJlMzNJMTE5TzBwVW9VR1p4K3lHano3Z09Oc0xVOSt1dW15Z0tBL1NTZDhUZytNPQ==;Mgo+DSMBMAY9C3t2VFhiQlJPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXtSfkRiWH1fdnJTQ2k=;MjEwMzAwOEAzMjMxMmUzMjJlMzNtVmV1cUMzVWhhalU4UlpVMllNYXpseXBjWk93UktpRHYwQlppVWYzcU9jPQ==;MjEwMzAwOUAzMjMxMmUzMjJlMzNhZHAwQzJCa0pUTnBYUFJDVnhoYnpqNlAyVVZ5Y0Q0dUU0WE9SVTAxV09BPQ==;MjEwMzAxMEAzMjMxMmUzMjJlMzNvQ2U2VDdoUFljZmdoR0pRU1hHRCtZYWt2TmRucUs0d21ZK3JiNlZCOHhrPQ==");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH9cc3RRRmRYUkd2W0A=");


            SetApplicationLanguage("IT-it");

            CreateRibbon(application);

            RegisterFiltersPane(application);

            InitModificationUpdater(application);





            application.ViewActivating += Application_ViewActivating;

            return rc;
        }


        private void Application_ViewActivating(object? sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {

            FiltersPaneExternalEventHandler handler = new FiltersPaneExternalEventHandler();
            FiltersPaneExternalEvent = ExternalEvent.Create(handler);
            FiltersPaneExternalEvent.Raise();
        }


        private void CreateRibbon(UIControlledApplication application)
        {
            string assemblyLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string baseDirectory = Path.GetDirectoryName(ExecutingAssemblyPath);


            application.CreateRibbonTab(TabReJoName);

            // Add a new ribbon panel
            RibbonPanel rbnPannGestione = application.CreateRibbonPanel(TabReJoName, LocalizationProvider.GetString("Gestione"));

            string initHelp = LocalizationProvider.GetString("Nel caso di problemi di inizializzazione:");
            initHelp += string.Format("\n\n1. {0}", LocalizationProvider.GetString("Chiudere tutte le istanze di Join e Revit"));
            initHelp += string.Format("\n2. {0}", LocalizationProvider.GetString("Avviare Join"));
            initHelp += string.Format("\n3. {0}", LocalizationProvider.GetString("Lanciare il comando Collega Revit dalla sezione Modelli 3d"));
            initHelp += string.Format("\n4. {0}", LocalizationProvider.GetString("Avviare Revit con lo stesso utente usato per avviare Join"));
            initHelp += string.Format("\n5. {0}", LocalizationProvider.GetString("Lanciare il comando Collega Join"));


            //Init
            PushButton pshBtnInit = rbnPannGestione.AddItem(new PushButtonData("BtnInit", LocalizationProvider.GetString("Collega Join"), ExecutingAssemblyPath, typeof(CmdInit).FullName)) as PushButton;/*LocalizationProvider.GetString()*/
            pshBtnInit.AvailabilityClassName = typeof(CmdInit).FullName;
            pshBtnInit.ToolTip = LocalizationProvider.GetString("Collega Join");
            pshBtnInit.LongDescription = LocalizationProvider.GetString(initHelp);
            pshBtnInit.ToolTipImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_L.png")));
            pshBtnInit.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_L.png")));
            pshBtnInit.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_S.png")));
            //pshBtnInit.SetContextualHelp(contextHelp);



            //Filtri
            PushButton pshBtnFilters = rbnPannGestione.AddItem(new PushButtonData("BtnFilters", LocalizationProvider.GetString("Filtri e regole"), ExecutingAssemblyPath, typeof(CmdFilters).FullName)) as PushButton;
            pshBtnFilters.AvailabilityClassName = typeof(CmdFilters).FullName;
            pshBtnFilters.ToolTip = LocalizationProvider.GetString("Filtri e regole");
            pshBtnFilters.LongDescription = LocalizationProvider.GetString("Filtri e regole");
            pshBtnFilters.ToolTipImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\FiltriJoin_L.png")));
            pshBtnFilters.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\FiltriJoin_L.png")));
            pshBtnFilters.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\FiltriJoin_S.png")));


            //Check Rules
            PushButton pshBtnApplyRules = rbnPannGestione.AddItem(new PushButtonData("BtnApplyRules", LocalizationProvider.GetString("Genera computo"), ExecutingAssemblyPath, typeof(CmdApplyRules).FullName)) as PushButton;
            pshBtnApplyRules.AvailabilityClassName = typeof(CmdApplyRules).FullName;
            pshBtnApplyRules.ToolTip = LocalizationProvider.GetString("Genera computo");
            pshBtnApplyRules.LongDescription = LocalizationProvider.GetString("Genera computo");
            pshBtnApplyRules.ToolTipImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\GeneraComputo_L.png")));
            pshBtnApplyRules.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\GeneraComputo_L.png")));
            pshBtnApplyRules.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\GeneraComputo_S.png")));



            ////Test
            //PushButton pshBtnTest = rbnPannGestione.AddItem(new PushButtonData("BtnTest", "Test", ExecutingAssemblyPath, typeof(CmdTest).FullName)) as PushButton;
            //pshBtnTest.AvailabilityClassName = typeof(CmdTest).FullName;
            //pshBtnTest.ToolTip = "Test1 ReJo";
            //pshBtnTest.LongDescription = "Test1 ReJo";

        }

        public Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;
        }

        public void InitModificationUpdater(UIControlledApplication application)// NB: funziona solo durante startUp
        {
            ModificationUpdater modificationUpdater = new ModificationUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(modificationUpdater);

            //non escludo niente
            ElementClassFilter filter = new ElementClassFilter(typeof(CurveElement), true);

            UpdaterRegistry.AddTrigger(modificationUpdater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
            UpdaterRegistry.AddTrigger(modificationUpdater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
            UpdaterRegistry.AddTrigger(modificationUpdater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());
        }

        public static void RegisterFiltersPane(UIControlledApplication application)
        {
            // dockable window
            FiltersPane fp = FiltersPane.Create();

            // create a new dockable pane id
            try
            {
                // register dockable pane
                application.RegisterDockablePane(fp.DockablePaneId, LocalizationProvider.GetString("Filtri e regole"), fp as IDockablePaneProvider);


            }
            catch (Exception ex)
            {
                // show error info dialog
                TaskDialog.Show("Info Message", ex.Message);
            }
        }

        public static void SetApplicationLanguage(string languageCode)
        {
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = new CultureInfo(languageCode);

            CultureInfo.CurrentCulture = new CultureInfo(languageCode, true);
            CultureInfo.CurrentUICulture = new CultureInfo(languageCode, true);
        }


    }


}
