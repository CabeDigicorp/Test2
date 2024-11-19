
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using RUI = Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using ReJo.UI;
using ReJo.Utility;
using Autodesk.Revit.DB.IFC;
using System.Windows.Controls;
using _3DModelExchange;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using Autodesk.Revit.DB.Events;

namespace ReJo
{


    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    class CmdInit : IExternalCommand, IExternalCommandAvailability
    {
        public static CmdInit This { get; private set; } = null;
        public static bool IsFirstExecute { get; private set; } = true;
        public static bool IsInitialized { get; private set; } = false;
        public UIApplication UIApplication { get; private set; } = null;
        public JoinService JoinService { get; private set; } = null;
        public OpenDocumentExternalEventHandler OpenDocumentHandler { get; private set; } = null;
        public RulesWndExternalEventHandler RulesWndHandler { get; private set; } = null;
        public GetElementsByFilterExternalEventHandler GetElementsByFilterHandler { get; private set; } = null;
        public ShowRuleExternalEventHandler ShowRuleHandler { get; private set; } = null;
        public SelectElementsExternalEventHandler SelectElementsHandler { get; private set; } = null;
        public UpdateInitButtonIconExternalEventHandler UpdateInitButtonIconHandler { get; private set; } = null;
        public CloseModelessDialogExternalEventHandler CloseModelessDialogHandler { get; private set; } = null;
        public AddFilterExternalEventHandler AddFilterHandler { get; private set; } = null;



        public bool ResetUI { get; set; } = false;


        Dictionary<string, DocumentIfcMap> _documentsIfcMap = new Dictionary<string, DocumentIfcMap>();//key: project ifc guid
        Dictionary<string, string> _projectsIfcIdByDocumentPath = new Dictionary<string, string>();//key: document path

        SaveAsEventHandler SaveAsHandler = new SaveAsEventHandler();
        SaveEventHandler SaveHandler = new SaveEventHandler();


        #region IExternalCommand Members

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            if (IsFirstExecute)
            {
                This = this;

                if (UIApplication == null)
                    This.UIApplication = commandData.Application;


                if (JoinService.This == null)
                    This.JoinService = new JoinService();
                else
                    This.JoinService = JoinService.This!;

                CreateExternalEventHandler();

                This.UIApplication.Application.DocumentOpened += Application_DocumentOpened;
                This.UIApplication.Application.DocumentCreated += Application_DocumentCreated;
                This.UIApplication.ViewActivated += UIApplication_ViewActivated;
                This.UIApplication.Idling += UIApplication_Idling;
                This.UIApplication.Application.DocumentSaving += Application_DocumentSaving;
                This.UIApplication.Application.DocumentSavingAs += Application_DocumentSavingAs;

                SaveAsHandler.Initialize(This.UIApplication);
                SaveHandler.Initialize(This.UIApplication);

                var ret = This.JoinService.Init();

                IsFirstExecute = false;
            }
            else
            {

                if (IsInitialized)
                    SetInitialized(false);
                else
                    This.JoinService.Init();


                return Result.Succeeded;
            }


            return Result.Succeeded;
        }

        private static void CreateExternalEventHandler()
        {
            This.OpenDocumentHandler = new OpenDocumentExternalEventHandler();
            This.OpenDocumentHandler.CreateExternalEvent();

            This.RulesWndHandler = new RulesWndExternalEventHandler();
            This.RulesWndHandler.CreateExternalEvent();

            This.GetElementsByFilterHandler = new GetElementsByFilterExternalEventHandler();
            This.GetElementsByFilterHandler.CreateExternalEvent();

            This.ShowRuleHandler = new ShowRuleExternalEventHandler();
            This.ShowRuleHandler.CreateExternalEvent();

            This.SelectElementsHandler = new SelectElementsExternalEventHandler();
            This.SelectElementsHandler.CreateExternalEvent();

            This.UpdateInitButtonIconHandler = new UpdateInitButtonIconExternalEventHandler();
            This.UpdateInitButtonIconHandler.CreateExternalEvent();

            This.CloseModelessDialogHandler = new CloseModelessDialogExternalEventHandler();
            This.CloseModelessDialogHandler.CreateExternalEvent();

            This.AddFilterHandler = new AddFilterExternalEventHandler();
            This.AddFilterHandler.CreateExternalEvent();

        }

        public void SetInitialized(bool init)
        {

            if (init)
            {
                CmdInit.This.OpenDocumentHandler.RaiseExternalEvent();
            }
            else
            {
                JoinService.This.Model3dFilesLoaded.Clear();
                FiltersPane.This.Show(false);
            }

            IsInitialized = init;
            This.UpdateInitButtonIconHandler.RaiseExternalEvent();

            if (IsInitialized)
                ReJoLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), "ReJo Inizialized");
        }


        #region Events
        private void Application_DocumentSavingAs(object? sender, Autodesk.Revit.DB.Events.DocumentSavingAsEventArgs e)
        {
            //if (!IsInitialized)
            //    return;

            //string docPathName = e.Document.PathName;
            //if (docPathName.StartsWith(JoinService.This.AppSettingsPath))
            //{
            //    e.Cancel();

            //    SaveFileDialog saveFileDialog = new SaveFileDialog();

            //    // Imposta le proprietà del dialogo
            //    saveFileDialog.Filter = "Revit files (*.rvt)|*.rvt|All files (*.*)|*.*";
            //    saveFileDialog.Title = "Save a Revit File";
            //    saveFileDialog.DefaultExt = "rvt";
            //    saveFileDialog.AddExtension = true;
            //    saveFileDialog.InitialDirectory = "C:\\";

            //    // Mostra il dialogo e controlla se l'utente ha cliccato su "Salva"
            //    if (saveFileDialog.ShowDialog() == true)
            //    {
            //        // Ottieni il percorso completo del file selezionato
            //        string docPathNameNew = saveFileDialog.FileName;

            //        CmdInit.This.UIApplication.ActiveUIDocument.Document.SaveAs(docPathNameNew);
            //    }

            //}


        }

        private void Application_DocumentSaving(object? sender, Autodesk.Revit.DB.Events.DocumentSavingEventArgs e)
        {
            //if (!IsInitialized)
            //    return;

            //string docPathName = e.Document.PathName;
            //if (docPathName.StartsWith(JoinService.This.AppSettingsPath))
            //{
            //    e.Cancel();
            //}

        }

        private void UIApplication_Idling(object? sender, IdlingEventArgs e)
        {
            //if (!IsInitialized)
            //    return;

            //if (ResetUI)
            //{
            //    WindowManager.CloseModelessWindows();
            //    ResetUI = false;
            //}


        }


        private void Application_DocumentCreated(object? sender, Autodesk.Revit.DB.Events.DocumentCreatedEventArgs e)
        {
            //OnDocumentOpened(e.Document);
        }

        private void Application_DocumentOpened(object? sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            //OnDocumentOpened(e.Document);
        }

        public void OnDocumentOpened(Document doc)
        {
            //LoadModel3dFiles(doc);
            //FiltersPane.This.Update();
        }



        private void UIApplication_ViewActivated(object? sender, ViewActivatedEventArgs e)
        {
            if (!IsInitialized)
                return;

            FiltersPane.This.Update();
        }

        #endregion
        public void LoadModel3dFiles(Document doc)
        {
            var model3DFiles = JoinService.This.GetCurrentProjectModel3dFiles();
            if (model3DFiles != null)
            {
                var found = model3DFiles.Items.FirstOrDefault(item => item.FileFullPath == doc.PathName);
                if (found != null)
                {
                    CreateIfcGuidMap(doc);
                }

                JoinService.This.UpdateModel3dFiles();
            }
        }
        static string GetProjectIfcGuid(Document doc, bool createIfEmpty)
        {
            Parameter parPrjIfcGuid = doc.ProjectInformation.get_Parameter(BuiltInParameter.IFC_PROJECT_GUID);
            string prjIfcGuid = parPrjIfcGuid.AsValueString();

            if (createIfEmpty)
            {
                if (string.IsNullOrEmpty(prjIfcGuid))
                {
                    prjIfcGuid = ExporterIFCUtils.CreateProjectLevelGUID(doc, IFCProjectLevelGUIDType.Project);
                }
            }

            return prjIfcGuid;
        }

        static string GetElementIfcGuid(Element el)
        {


            string ifcGuid = string.Empty;

            Parameter parElIfcGuid = el.get_Parameter(BuiltInParameter.IFC_GUID);
            if (parElIfcGuid != null)
            {
                ifcGuid = parElIfcGuid.AsValueString();
            }

            return ifcGuid;
        }

        public string? GetProjectIfcGuid(Document doc)
        {
            string documentPathName = doc.PathName;

            _projectsIfcIdByDocumentPath.TryGetValue(documentPathName, out string? projectIfcId);
            return projectIfcId;
        
        }

        public Document GetDocument(string projectIfcGuid)
        {

            foreach (Document doc in CmdInit.This.UIApplication.Application.Documents)
            {
                string? prjIfcGuid = GetProjectIfcGuid(doc);
                if (projectIfcGuid == prjIfcGuid)
                    return doc;
            }

            return null;
        
        }


        public ElementId? GetElementId(string prjIfcGuid, string elIfcGuid)
        {
            DocumentIfcMap? documentIfcMap = null;
            if (_documentsIfcMap.TryGetValue(prjIfcGuid, out documentIfcMap))
            {
                ElementId? elId = null;
                if (documentIfcMap.ElementsIdByIfc.TryGetValue(elIfcGuid, out elId))
                    return elId;
            }

            if (string.IsNullOrEmpty(elIfcGuid))
            {
                ReJoLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "string.IsNullOrEmpty(elIfcGuid)= true");
            }

            return null;
        }


        public string GetElementIfcGuid(Document doc, ElementId elId)
        {
            var prjIfcGuid = GetProjectIfcGuid(doc);

            if (prjIfcGuid == null)
                return null;


            DocumentIfcMap? documentIfcMap = null;
            if (_documentsIfcMap.TryGetValue(prjIfcGuid, out documentIfcMap))
            {
                string elIfcId = null;
                if (documentIfcMap.ElementsIfcById.TryGetValue(elId, out elIfcId))
                    return elIfcId;
            }

            return null;
        }




        private void CreateIfcGuidMap(Document document)
        {
            if (document == null)
                return;

            var prjIfcGuid = GetProjectIfcGuid(document, true);


            if (prjIfcGuid == null)
                return;

            string documentPath = document.PathName;
            _projectsIfcIdByDocumentPath.TryAdd(documentPath, prjIfcGuid);


            //element
            ICollection<ElementId> docElementsId =
                new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .ToElementIds();



            HashSet<ElementId> elemsId = docElementsId.ToHashSet();

            //Aggiungo alla selezione gli elementi di tipo FamilyInstance nested shared
            FilteredElementCollector elCollectors = new FilteredElementCollector(document, elemsId);
            elemsId.UnionWith(elCollectors
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(a => a.SuperComponent == null)
                .SelectMany(a => a.GetSubComponentIds()));



            //Aggiungo alla lista degli elementi quelli appartenenti ad ogni stackedwall selezionato
            FilteredElementCollector elCollectors2 = new FilteredElementCollector(document, elemsId);
            elemsId.UnionWith(elCollectors2
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .Where(a => a.IsStackedWall)
                .SelectMany(a => a.GetStackedWallMemberIds()));


            var docIfcMap = new DocumentIfcMap() { IfcGuid = prjIfcGuid };
            foreach (ElementId elementId in elemsId)
            {
                Element element = document.GetElement(elementId);

                if (element == null)
                    continue;

                //raccolgo i membri di gruppi
                if (element is Group group)
                {
                    List<Element> groupElems = new List<Element>();

                    Utils.GetGroupMembersRecursive(document, group, groupElems);

                    foreach (Element groupEl in groupElems)
                    {
                        var groupElIfcGuid = GetElementIfcGuid(groupEl);

                        if (!string.IsNullOrEmpty(groupElIfcGuid))
                        {
                            docIfcMap.ElementsIfcById.TryAdd(groupEl.Id, groupElIfcGuid);
                            docIfcMap.ElementsIdByIfc.TryAdd(groupElIfcGuid, groupEl.Id);
                        }
                    }
                }
                else if (element is AssemblyInstance assemblyInst)
                {
                    List<Element> assElems = new List<Element>();

                    Utils.GetAssemblyInstanceMembersRecursive(document, assemblyInst, assElems);

                    foreach (Element groupEl in assElems)
                    {
                        var groupElIfcGuid = GetElementIfcGuid(groupEl);

                        if (!string.IsNullOrEmpty(groupElIfcGuid))
                        {
                            docIfcMap.ElementsIfcById.TryAdd(groupEl.Id, groupElIfcGuid);
                            docIfcMap.ElementsIdByIfc.TryAdd(groupElIfcGuid, groupEl.Id);
                        }
                    }

                }

                

                var elIfcGuid = GetElementIfcGuid(element);
                if (!string.IsNullOrEmpty(elIfcGuid))
                {
                    docIfcMap.ElementsIfcById.TryAdd(element.Id, elIfcGuid);
                    docIfcMap.ElementsIdByIfc.TryAdd(elIfcGuid, element.Id);
                }
            }



            //element type
            IList<Element> docElementsType =
                new FilteredElementCollector(document)
                .WhereElementIsElementType()
                .ToElements();

            foreach (Element element in docElementsType)
            {
                var elIfcGuid = GetElementIfcGuid(element);

                if (!string.IsNullOrEmpty(elIfcGuid))
                {
                    docIfcMap.ElementsIfcById.TryAdd(element.Id, elIfcGuid);
                    docIfcMap.ElementsIdByIfc.TryAdd(elIfcGuid, element.Id);
                }
            }



            if (_documentsIfcMap.ContainsKey(prjIfcGuid))
                _documentsIfcMap[prjIfcGuid] = docIfcMap;
            else
                _documentsIfcMap.Add(prjIfcGuid, docIfcMap);


        }

        //public static void UpdateInitButtonIcon()
        //{
        //    // Ottieni il ribbon panel
        //    List<RibbonPanel> panels = This.UIApplication.GetRibbonPanels(Application.TabReJoName); // Assumi che il pulsante sia nel primo pannello

        //    RibbonPanel panel = panels[0];

        //    // Trova il pulsante specifico
        //    PushButton? pushButton = panel.GetItems()
        //        .OfType<PushButton>()
        //        .FirstOrDefault(b => b.Name == "BtnInit");


        //    string? baseDirectory = Path.GetDirectoryName(Application.ExecutingAssemblyPath);

        //    if (pushButton != null && !string.IsNullOrEmpty(baseDirectory))
        //    {
        //        if (!CmdInit.IsInitialized)
        //        {
        //            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_L.png")));
        //            pushButton.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_S.png")));
        //        }
        //        else
        //        {
        //            pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoinOn_L.png")));
        //            pushButton.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoinOn_S.png")));
        //        }
        //    }
        //}

        #endregion

        #region IExternalCommandAvailability Members

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }



        #endregion






    }



    internal class DocumentIfcMap
    {
        public string IfcGuid { get; set; } = string.Empty;

        public Dictionary<string, ElementId> ElementsIdByIfc = new Dictionary<string, ElementId>();
        public Dictionary<ElementId, string> ElementsIfcById = new Dictionary<ElementId, string>();

    }




}
