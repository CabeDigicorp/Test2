
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Text;
using System.Windows;
using System.IO;
using Autodesk.Revit.UI.Selection;
using System.Windows.Controls;
using _3DModelExchange;
using RUI = Autodesk.Revit.UI;
using ReJo.UI;



namespace ReJo
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class CmdTest : IExternalCommand, IExternalCommandAvailability
    {

        public ExternalEvent RulesWndExternalEvent { get; private set; } = null;


        #region IExternalCommand Members

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //GetLinkedElementParameters(commandData);

            //SelectWallsInLinkedDocs(commandData);

            //HideWallsInLinkedDocs(commandData);


            //Globals.JoinService.AddElementi(null, false);

            //SelectElementsInJoin();

            //AddElementiInJoin();

            //GetPhase();

            //ShowDialog();

            //CallRevitFilter();

            //ShowFilterPane();

            //GetFilters();

            return Result.Succeeded;
        }

        static void GetFilters()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            // Search for existing filters with same name
            List<FilterElement> filterList = new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FilterElement))).ToElements().Cast<FilterElement>().ToList();
            foreach (ParameterFilterElement filter1 in filterList)
            {
                ElementId filterId = filter1.Id;
                string uniqueId = filter1.UniqueId;
                


                ElementFilter elFilter = filter1.GetElementFilter();
                GetFilterRecursive(elFilter);

            }
        }

        private static void GetFilterRecursive(ElementFilter elFilter)
        {
            if (elFilter is ElementLogicalFilter elementLogicalFilter)
            {
                var filters = elementLogicalFilter.GetFilters();
                foreach (var filter in filters)
                    GetFilterRecursive(filter);

            }
            else if (elFilter is ElementQuickFilter elementQuickFilter)
            {


            }
            else if (elFilter is ElementParameterFilter elementSlowFilter)//ElementSlowFilter
            {
                IList<FilterRule> rules = elementSlowFilter.GetRules();
                foreach (var rule in rules)
                {
                    if (rule is FilterCategoryRule filterCatRule)
                    {

                    }
                    else if (rule is FilterInverseRule filterInverseRule)
                    {

                    }
                    else if (rule is FilterValueRule filterValueRule)
                    {
                        if (rule is FilterNumericValueRule filterNumericValueRule)
                        {
                            if (rule is FilterDoubleRule filterDoubleRule)
                            {
                                double val = filterDoubleRule.RuleValue;
                                FilterNumericRuleEvaluator ev = filterDoubleRule.GetEvaluator();

                            }
                            else if (rule is FilterElementIdRule filterElementIdRule)
                            { }
                            else if (rule is FilterGlobalParameterAssociationRule filterGlobalParameterAssociationRule)
                            { }
                            else if (rule is FilterIntegerRule filterIntegerRule)
                            { }
                        }
                        else if (rule is FilterStringRule filterStringRule)
                        { }
                    }
                    else if (rule is ParameterValuePresenceRule parameterValuePresenceRule)
                    { }
                    else if (rule is SharedParameterApplicableRule sharedParameterApplicableRule)
                    { }

                }
            }
        }



        //Document document = null;

        //// Creates an ElementParameter filter to find rooms whose area is 
        //// greater than specified value
        //// Create filter by provider and evaluator 
        //// provider
        //ParameterValueProvider pvp = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_AREA));
        //// evaluator
        //FilterNumericRuleEvaluator fnrv = new FilterNumericGreater();
        //// rule value    
        //double ruleValue = 100.0f; // filter room whose area is greater than 100 SF
        //                           // rule
        //FilterRule fRule = new FilterDoubleRule(pvp, fnrv, ruleValue, 1E-6);

        //string aaa = fRule.ToString();

        //// Create an ElementParameter filter
        //ElementParameterFilter filter = new ElementParameterFilter(fRule);




        //doc.ActiveView.AddFilter()

        //// Apply the filter to the elements in the active document
        //FilteredElementCollector collector = new FilteredElementCollector(document);
        //IList<Element> rooms = collector.WherePasses(filter).ToElements();


        //// Find rooms whose area is less than or equal to 100: 
        //// Use inverted filter to match elements
        //ElementParameterFilter lessOrEqualFilter = new ElementParameterFilter(fRule, true);
        //collector = new FilteredElementCollector(document);
        //IList<Element> lessOrEqualFounds = collector.WherePasses(lessOrEqualFilter).ToElements();







        //private static ParameterFilterElement CreateViewFilter(Document doc, BuiltInCategory category, Level CurrentLevel, Level LowerLevel)
        //{
        //    ElementId paramId = new ElementId(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
        //    List<ElementId> catIds = new List<ElementId>();
        //    catIds.Add(new ElementId(category));
        //    List<FilterRule> filterRules = new List<FilterRule>();

        //    filterRules.Add(ParameterFilterRuleFactory.CreateEqualsRule(paramId, LowerLevel.Id));
        //    ParameterFilterElement elemFilter = null;
        //    string filterName = CurrentLevel.Name + "_ExcludeColumnsBelow";

        //    // Search for existing filters with same name
        //    List<FilterElement> filterList = new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FilterElement))).ToElements().Cast<FilterElement>().ToList();
        //    foreach (ParameterFilterElement filter in filterList)
        //    {
        //        if (filter.Name == filterName) // Filter Exists, update rules and use existing filter
        //        {
        //            elemFilter = filter;
        //            elemFilter.ClearRules();
        //            elemFilter.SetRules(filterRules);
        //            break;
        //        }
        //    }

        //    if (elemFilter == null) // Doesn't Exist, create new filter
        //    {
        //        elemFilter = ParameterFilterElement.Create(doc, filterName, catIds, filterRules);
        //    }
        //    return elemFilter;
        //}

        void ShowDialog()
        {
            RulesWndExternalEventHandler handler = new RulesWndExternalEventHandler();
            RulesWndExternalEvent = ExternalEvent.Create(handler);

            RulesWndExternalEvent.Raise();

        }

        void CallRevitFilter()
        {
            //"C:\Users\alessandro.uliana\AppData\Local\Autodesk\Revit\Autodesk Revit 2025\Journals\journal.0307.txt"
            //string journaFileName = CmdInit.This.UIApplication.Application.RecordingJournalFilename;



            var filterCmdId = RevitCommandId.LookupCommandId("ID_SETTINGS_FILTERS");
        }


        void GetPhase()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            var elemsId = CmdInit.This.UIApplication.ActiveUIDocument.Selection.GetElementIds();

            if (!elemsId.Any())
                return;

            Element el = doc.GetElement(elemsId.FirstOrDefault());

            var createdPhase = doc.GetElement(el?.CreatedPhaseId);
            var demolishedPhase = doc.GetElement(el?.DemolishedPhaseId);
            var level = doc.GetElement(el?.LevelId);
            var workset = doc.GetWorksetId(elemsId.FirstOrDefault());
            var type = doc.GetElement(el?.GetTypeId()) as  ElementType;
            string familyName = type.FamilyName;

            var category = el?.Category;
            var group = doc.GetElement(el?.GroupId);


        }

        void SelectElementsInJoin()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            var elemsId = CmdInit.This.UIApplication.ActiveUIDocument.Selection.GetElementIds();

            List<Model3dObjectKey> elems = CreateModel3dObjectsKey(doc, elemsId);

            CmdInit.This.JoinService.SelectElements(elems);
        }

        private static List<Model3dObjectKey> CreateModel3dObjectsKey(Document doc, ICollection<ElementId> elemsId)
        {
            Parameter parPrjIfcGuid = doc.ProjectInformation.get_Parameter(BuiltInParameter.IFC_PROJECT_GUID);
            string prjIfcGuid = parPrjIfcGuid.AsValueString();

            List<Model3dObjectKey> elems = new List<Model3dObjectKey>();

            foreach (var elemId in elemsId)
            {
                Element el = doc.GetElement(elemId);
                if (el != null)
                {
                    Parameter parElIfcGuid = el.get_Parameter(BuiltInParameter.IFC_GUID);
                    string ifcGuid = parElIfcGuid.AsValueString();

                    elems.Add(new Model3dObjectKey() { GlobalId = ifcGuid, ProjectGlobalId = prjIfcGuid, Model3DType = Model3dType.Revit });

                }
            }

            return elems;
        }



        void AddElementiInJoin()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            var elemsId = CmdInit.This.UIApplication.ActiveUIDocument.Selection.GetElementIds();

            List<Model3dObjectKey> elems = CreateModel3dObjectsKey(doc, elemsId);

            CmdInit.This.JoinService.AddElementi(elems, false);
        }


        void HideWallsInLinkedDocs(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(RevitLinkInstance));

            List<Element> allLikedDocElements = new List<Element>();

            List<Reference> references = new List<Reference>();

            StringBuilder linkedDocs = new StringBuilder();
            foreach (Element elem in collector)
            {
                RevitLinkInstance instance = elem as RevitLinkInstance;
                Document linkedDoc = instance.GetLinkDocument();

                //Filtering
                List<Element> linkedDocElements =
                new FilteredElementCollector(linkedDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .ToElements()
                .ToList();

                foreach (var linkedElement in linkedDocElements)
                {
                    


                    // now create a reference from this element
                    // -- this is a reference inside the linked document
                    var reference = new Reference(linkedElement);

                    // convert the reference to be readable from the current document
                    reference = reference.CreateLinkReference(instance);

                    

                    references.Add(reference);


                }
            }

            List<Element> docElements =
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .ToElements()
                .ToList();


            uidoc.ActiveView.HideElementsTemporary(docElements.Select(item => item.Id).ToList());


        }


        void SelectWallsInLinkedDocs(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(RevitLinkInstance));

            List<Element> allLikedDocElements = new List<Element>();

            List<Reference> references = new List<Reference>();

            StringBuilder linkedDocs = new StringBuilder();
            foreach (Element elem in collector)
            {
                RevitLinkInstance instance = elem as RevitLinkInstance;
                Document linkedDoc = instance.GetLinkDocument();

                //Filtering
                List<Element> linkedDocElements =
                new FilteredElementCollector(linkedDoc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .ToElements()
                .ToList();

                foreach (var linkedElement in linkedDocElements)
                {

                    // now create a reference from this element
                    // -- this is a reference inside the linked document
                    var reference = new Reference(linkedElement);

                    // convert the reference to be readable from the current document
                    reference = reference.CreateLinkReference(instance);


                    references.Add(reference);


                }
            }

            List<Element> docElements =
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .ToElements()
                .ToList();


            

            //uidoc.Selection.SetElementIds(docElements.Select(x => x.Id).ToList());



            // now the linked element is highlighted
            uidoc.Selection.SetReferences(references);

        }


        void SelectLinkedElementByCode(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var pickedReference = uidoc.Selection.PickObject(
                ObjectType.PointOnElement
            );

            // get Revit link Instance and its document
            var linkedRvtInstance = doc.GetElement(pickedReference) as RevitLinkInstance;
            var linkedDoc = linkedRvtInstance.GetLinkDocument();

            //get the Linked element from the linked document
            var linkedElement = linkedDoc.GetElement(pickedReference.LinkedElementId);

            // now create a reference from this element
            // -- this is a reference inside the linked document
            var reference = new Reference(linkedElement);

            // convert the reference to be readable from the current document
            reference = reference.CreateLinkReference(linkedRvtInstance);

            // now the linked element is highlighted
            uidoc.Selection.SetReferences([reference]);

            
        }

        void GetLinkedElementParameters(ExternalCommandData commandData)
        {
            StringBuilder st = new StringBuilder();

            try
            {

                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;
                Selection sel = uidoc.Selection;
                Reference pickedReference = sel.PickObject(ObjectType.LinkedElement, "Select Linked Element");
                RevitLinkInstance linkInstance = doc.GetElement(pickedReference) as RevitLinkInstance;
                Document linkedDoc = linkInstance.GetLinkDocument();
                Element elRef = linkedDoc.GetElement(pickedReference.LinkedElementId);
                ElementId xx = elRef.Id;

                foreach (Parameter para in elRef.Parameters)
                {
                    st.AppendLine(GetParameterInformation(para, doc));
                }

            }
            catch (Exception)
            {
                
            }



            MessageBox.Show(st.ToString());
        }


        String GetParameterInformation(Parameter para, Document document)
        {


            string defName = para.Definition.Name + "\t : ";
            string defValue = string.Empty;


            try
            {

                // Use different method to get parameter data according to the storage type
                switch (para.StorageType)
                {
                    case StorageType.Double:
                        //covert the number into Metric
                        defValue = para.AsValueString();
                        break;
                    case StorageType.ElementId:
                        //find out the name of the element
                        Autodesk.Revit.DB.ElementId id = para.AsElementId();
                        if (id.IntegerValue >= 0)
                        {
                            defValue = document.GetElement(id).Name;
                        }
                        else
                        {
                            defValue = id.IntegerValue.ToString();
                        }
                        break;
                    case StorageType.Integer:
                        if (SpecTypeId.Boolean.YesNo == para.Definition.GetDataType())
                        {
                            if (para.AsInteger() == 0)
                            {
                                defValue = "False";
                            }
                            else
                            {
                                defValue = "True";
                            }
                        }
                        else
                        {
                            defValue = para.AsInteger().ToString();
                        }
                        break;
                    case StorageType.String:
                        defValue = para.AsString();
                        break;
                    default:
                        defValue = "Unexposed parameter.";
                        break;
                }
            }
            catch (Exception)
            {
                
            }

            return defName + defValue;
        }

        #endregion

        #region IExternalCommandAvailability Members

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            if (CmdInit.IsInitialized)
                return true;
            else
                return false;
        }



        #endregion
    }



}
