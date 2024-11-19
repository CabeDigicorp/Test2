using _3DModelExchange;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using log4net.Filter;
using ReJo.UI;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Material = Autodesk.Revit.DB.Material;

namespace ReJo.Utility
{
    public class Utils
    {
        public static string IdKey => "_Id";
        public static string CategoryKey => "_Category";
        public static string WorksetKey => "_Workset";
        public static string FileName => "_FileName";
        public static string CreatedPhaseKey => "_CreatedPhase";
        public static string DemolishedPhaseKey => "_DemolishedPhase";
        public static string Mark => "P>_Mark";
        public static string ParameterKey => "P>";
        public static string MaterialKey => "M>";
        public static string AreaKey => "Area{";
        public static string PaintAreaKey => "PaintArea{";
        public static string VolumeKey => "Volume{";
        public static string GlobalIdKey => "GlobalId";
        public static string NameKey => "_Name";
        public static string RoomKey => "_Room";
        public static string FromRoomKey => "_FromRoom";
        public static string ToRoomKey => "_ToRoom";
        public static string Family => "_Family";


        public static Dictionary<string, Element> GetElementsByIfcGuid(string projectIfcGuid, IEnumerable<string> elementsIfcGuid)
        {
            var elems = new Dictionary<string, Element>();

            var doc = GetDocument(projectIfcGuid);
            if (doc == null)
                return elems;

            foreach (var elIfcGuid in elementsIfcGuid)
            {
                ElementId? elId = GetElementId(projectIfcGuid, elIfcGuid);

                if (elId != null)
                {
                    var el = doc.GetElement(elId);
                    elems.Add(elIfcGuid, el);
                }
            }

            return elems;
        }





        public static ElementId? GetElementId(string prjIfcGuid, string ifcGuid)
        {
            return CmdInit.This.GetElementId(prjIfcGuid, ifcGuid);
        }

        public static Document? GetDocument(string projectIfcGuid)
        {
            Document? document = null;

            foreach (Document doc in CmdInit.This.UIApplication.Application.Documents)
            {
                var prjIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);

                if (prjIfcGuid == projectIfcGuid)
                {
                    document = doc;
                    break;
                }
            }

            return document;
        }

        public static void UpdateModel3dValues(Model3dValues model3DValues)
        {
            var result = model3DValues.Values.GroupBy(item => item.ProjectGlobalId, (key, g) => new { ProjectGlobalId = key, Values = g });
            foreach (var x in result)
            {
                Document doc = CmdInit.This.GetDocument(x.ProjectGlobalId);

                var elems = Utils.GetElementsByIfcGuid(x.ProjectGlobalId, x.Values.Select(x => x.GlobalId));

                foreach (var model3DValue in x.Values)
                {
                    if (string.IsNullOrEmpty(model3DValue.GlobalId))
                    {
                        ReJoLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "string.IsNullOrEmpty(model3DValue.GlobalId) = true");
                        continue;
                    }

                    Element? element = null;
                    if (elems.TryGetValue(model3DValue.GlobalId, out element))
                    {
                        Utils.UpdateModel3dValue(doc, model3DValue, element);
                    }
                    else
                    {
                        if (model3DValue.GlobalId == model3DValue.ProjectGlobalId) //è il project (document)
                        {
                            if (model3DValue?.ValuePath == Utils.GlobalIdKey)//ifcGlobalId
                                model3DValue.Value = CmdInit.This.GetProjectIfcGuid(doc);
                            else if (model3DValue?.ValuePath == Utils.FileName)
                                model3DValue.Value = Path.GetFileName(doc.PathName);
                            else if (model3DValue?.ValuePath == Utils.NameKey)
                                model3DValue.Value = doc.ProjectInformation.Name;
                            else if (model3DValue?.ValuePath == Utils.CategoryKey)
                                model3DValue.Value = doc.ProjectInformation.Category?.Name;

                        }


                    }
                }
            }
        }


        public static void UpdateModel3dValue(Document doc, Model3dValue? model3DValue, Element? element)
        {
            Element el = null;


            if (model3DValue?.ValuePath == FileName)
            {
                model3DValue.Value = Path.GetFileName(doc.PathName);
                return;
            }

            //////////////////////
            ///Ricavo l'elemento

            if (model3DValue?.ClassName == Model3dClassEnum.Nothing)
            {
                el = element!;
            }
            else if (model3DValue?.ClassName == Model3dClassEnum.RvtProject)
            {
                if (model3DValue?.ValuePath == GlobalIdKey)//ifcGlobalId
                    model3DValue.Value = CmdInit.This.GetProjectIfcGuid(doc);
            }
            else if (model3DValue?.ClassName == Model3dClassEnum.RvtLevel)
            {
                el = doc.GetElement(element?.LevelId);
            }
            else if (model3DValue?.ClassName == Model3dClassEnum.RvtElementType)
            {
                var elTypeId = element.GetTypeId();
                el = doc.GetElement(elTypeId);
            }
            else if (model3DValue?.ClassName == Model3dClassEnum.RvtRoom)
            {
                if (element is FamilyInstance familyInstance)
                {
                    if (model3DValue.ItemPath.StartsWith(FromRoomKey))
                    {
                        el = familyInstance.FromRoom;
                    }
                    else if (model3DValue.ItemPath.StartsWith(ToRoomKey))
                    {
                        el = familyInstance.ToRoom;
                    }
                    else //if (model3DValue.ItemPath.StartsWith(RoomKey))
                    {
                        el = familyInstance.Room;
                    }
                }
            }
            else if (model3DValue?.ClassName == Model3dClassEnum.RvtAssembly)
            {
                el = doc.GetElement(element?.AssemblyInstanceId);
            }
            else if (model3DValue?.ClassName == Model3dClassEnum.RvtGroup)
            {
                el = doc.GetElement(element?.GroupId);
            }


            /////////////////////////////
            //Ricavo il dato dall'elemento

            if (el == null)
                return;




            if (model3DValue?.ValuePath == IdKey)
                model3DValue.Value = el.Id.ToString();
            else if (model3DValue?.ValuePath == GlobalIdKey)//ifcGlobalId
                model3DValue.Value = CmdInit.This.GetElementIfcGuid(doc, el.Id);
            else if (model3DValue?.ValuePath == CategoryKey)
                model3DValue.Value = el.Category?.Name;
            else if (model3DValue?.ValuePath == NameKey)
                model3DValue.Value = el.Name;
            else if (model3DValue?.ValuePath == WorksetKey)
            {
                WorksetTable worksetTable = doc.GetWorksetTable();

                if (el.WorksetId != WorksetId.InvalidWorksetId)
                {
                    Workset worksetByElement = worksetTable.GetWorkset(el.WorksetId);
                    model3DValue.Value = worksetByElement.Name;
                }
            }
            else if (model3DValue?.ValuePath == CreatedPhaseKey)
            {
                if (el.IsPhaseCreatedValid(el.CreatedPhaseId))
                {
                    var createdPhase = doc.GetElement(el?.CreatedPhaseId);
                    if (createdPhase != null)
                        model3DValue.Value = createdPhase.Name;
                    else
                        model3DValue.Value = string.Empty;

                }
            }
            else if (model3DValue?.ValuePath == DemolishedPhaseKey)
            {
                if (el.IsPhaseDemolishedValid(el.DemolishedPhaseId))
                {
                    var demolishedPhase = doc.GetElement(el?.DemolishedPhaseId);
                    if (demolishedPhase != null)
                        model3DValue.Value = demolishedPhase.Name;
                    else
                        model3DValue.Value = string.Empty;
                }
            }
            else if (model3DValue?.ValuePath == Mark)
            {
                Parameter par = el.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                if (par != null)
                {
                    model3DValue.Value = par.AsValueString();
                }
                else if (el.IsValidType(el.Id))//è un tipo
                {
                    Parameter parType = el.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
                    if (parType != null)
                    {
                        model3DValue.Value = parType.AsValueString();
                    }
                }
            }
            else if (model3DValue?.ValuePath == Family)
            {
                if (el is ElementType elType)
                {
                    model3DValue.Value = elType.FamilyName;
                }
            }
            else if (model3DValue != null && model3DValue.ValuePath.StartsWith(ParameterKey)) //parametri
            {
                string parName = model3DValue.ValuePath.Substring(ParameterKey.Length);
                Parameter par = el.LookupParameter(parName);
                if (par != null)
                {
                    if (par.StorageType == StorageType.Double)
                    {
                        double? val = GetParameterValueAsDouble(par);
                        if (val.HasValue)
                            model3DValue.Value = val.Value.ToString(CultureInfo.InvariantCulture);
                        else
                            model3DValue.Value = string.Empty;
                    }
                    else
                    {
                        model3DValue.Value = par.AsValueString();
                    }
                }
                else if (model3DValue?.ClassName == Model3dClassEnum.Nothing)//cerco il parametro sul tipo
                {
                    var elTypeId = el.GetTypeId();
                    var elType = doc.GetElement(elTypeId);
                    if (elType != null)
                    {
                        Parameter parT = elType.LookupParameter(parName);
                        if (parT != null)
                        {
                            if (parT.StorageType == StorageType.Double)
                            {
                                double? val = GetParameterValueAsDouble(parT);
                                if (val.HasValue)
                                    model3DValue.Value = val.Value.ToString(CultureInfo.InvariantCulture);
                                else
                                    model3DValue.Value = string.Empty;
                            }
                            else
                            {
                                model3DValue.Value = parT.AsValueString();
                            }
                        }
                    }
                }

            }
            else if (model3DValue != null && model3DValue.ValuePath.StartsWith(MaterialKey)) //materiali
            {
                double matQta = GetMaterialQta(doc, model3DValue.ValuePath, el);
                if (matQta > 0)
                    model3DValue.Value = matQta.ToString(CultureInfo.InvariantCulture);

            }
        }

        private static double GetMaterialQta(Document doc, string matPath , Element el)
        {
            //M>Area{Intonaco}
            //M>PaintArea{Intonaco}
            //M>Volume{Intonaco}

            string matQtaName = matPath.Substring(MaterialKey.Length);

            if (matQtaName.StartsWith(AreaKey))
            {
                int matNameEnd = matQtaName.IndexOf('}');
                int matNameLenght = matNameEnd - AreaKey.Length;

                if (matNameLenght <= 0)
                    return -1;

                string matName = matQtaName.Substring(AreaKey.Length, matNameLenght);

                var materialsId = el.GetMaterialIds(false);

                ElementId? materialId = null;

                foreach (var matId in materialsId)
                {
                    Material? mat = doc.GetElement(matId) as Material;
                    if (matName == mat?.Name)
                    {
                        materialId = matId;
                        break;
                    }
                }

                if (materialId != null)
                {
                    double area = GetMaterialQtaValue(el, materialId, MaterialQtaType.Area, out _);
                    return area;
                }

            }
            else if (matQtaName.StartsWith(PaintAreaKey))
            {
                int matNameEnd = matQtaName.IndexOf('}');
                int matNameLenght = matNameEnd - PaintAreaKey.Length;

                if (matNameLenght <= 0)
                    return -1;

                string matName = matQtaName.Substring(PaintAreaKey.Length, matNameLenght);

                var materialsId = el.GetMaterialIds(true);

                ElementId? materialId = null;

                foreach (var matId in materialsId)
                {
                    Material? mat = doc.GetElement(matId) as Material;
                    if (matName == mat?.Name)
                    {
                        materialId = matId;
                        break;
                    }
                }

                if (materialId != null)
                {
                    double area = GetMaterialQtaValue(el, materialId, MaterialQtaType.PaintArea, out _);
                    return area;
                }
            }
            else if (matQtaName.StartsWith(VolumeKey))
            {
                int matNameEnd = matQtaName.IndexOf('}');
                int matNameLenght = matNameEnd - VolumeKey.Length;

                if (matNameLenght <= 0)
                    return -1;

                string matName = matQtaName.Substring(VolumeKey.Length, matNameLenght);

                var materialsId = el.GetMaterialIds(false);

                ElementId? materialId = null;

                foreach (var matId in materialsId)
                {
                    Material? mat = doc.GetElement(matId) as Material;
                    if (matName == mat?.Name)
                    {
                        materialId = matId;
                        break;
                    }
                }

                if (materialId != null)
                {
                    double volume = GetMaterialQtaValue(el, materialId, MaterialQtaType.Volume, out _);
                    return volume;
                }


            }

            return -1;
        }

        public static double GetMaterialQtaValue(Element el, ElementId materialId, MaterialQtaType matValueType, out string formattedValue)
        {
            Units units = CmdInit.This.UIApplication.ActiveUIDocument.Document.GetUnits();
            formattedValue = string.Empty;

            double qta = 0;
            if (matValueType == MaterialQtaType.Area)
            {
                FormatOptions areaFormatOptions = units.GetFormatOptions(SpecTypeId.Area);
                ForgeTypeId areaDispUnitType = areaFormatOptions.GetUnitTypeId();

                double dArea = el.GetMaterialArea(materialId, false);
                qta = UnitUtils.ConvertFromInternalUnits(dArea, areaDispUnitType);
                formattedValue = UnitFormatUtils.Format(units, SpecTypeId.Area, dArea, false, new FormatValueOptions() { AppendUnitSymbol = true });
            }
            else if (matValueType == MaterialQtaType.PaintArea)
            {
                FormatOptions areaFormatOptions = units.GetFormatOptions(SpecTypeId.Area);
                ForgeTypeId areaDispUnitType = areaFormatOptions.GetUnitTypeId();

                double dArea = el.GetMaterialArea(materialId, true);
                qta = UnitUtils.ConvertFromInternalUnits(dArea, areaDispUnitType);
                formattedValue = UnitFormatUtils.Format(units, SpecTypeId.Area, dArea, false, new FormatValueOptions() { AppendUnitSymbol = true });
            }
            else if (matValueType == MaterialQtaType.Volume)
            {
                FormatOptions volFormatOptions = units.GetFormatOptions(SpecTypeId.Volume);
                ForgeTypeId volDispUnitType = volFormatOptions.GetUnitTypeId();

                double dVol = el.GetMaterialVolume(materialId);
                qta = UnitUtils.ConvertFromInternalUnits(dVol, volDispUnitType);
                formattedValue = UnitFormatUtils.Format(units, SpecTypeId.Volume, dVol, false, new FormatValueOptions() { AppendUnitSymbol = true });

            }

            return qta;
        }

        public static double? GetParameterValueAsDouble(Parameter prm)
        {

            double? dVal1 = null;
            if (prm.StorageType == StorageType.Double)
                dVal1 = prm.AsDouble();

            if (dVal1.HasValue)
            {
                try
                {

                    if (prm.Ext_HasDisplayUnitType() && prm.StorageType == StorageType.Double)
                    {
                        try { prm.GetUnitTypeId(); }
                        catch (Exception e) { }

                        if (UnitUtils.IsUnit(prm.GetUnitTypeId()))
                        {
                            double dValDisplay = UnitUtils.ConvertFromInternalUnits(dVal1.Value, prm.GetUnitTypeId());
                            return dValDisplay;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            return dVal1;

        }

        public static void GetGroupMembersRecursive(Document d, Group g, List<Element> r)
        {
            if (r == null)
                return;

            List<Element> elems = g.GetMemberIds().Select(q => d.GetElement(q)).ToList();

            foreach (Element el in elems)
            {
                if (el.GetType() == typeof(Group))
                {
                    GetGroupMembersRecursive(d, el as Group, r);
                    continue;
                }

                r.Add(el);
            }
        }

        public static void GetAssemblyInstanceMembersRecursive(Document d, AssemblyInstance a, List<Element> r)
        {
            if (r == null)
                return;


            List<Element> elems = a.GetMemberIds().Select(q => d.GetElement(q)).ToList();

            foreach (Element el in elems)
            {
                if (el.GetType() == typeof(AssemblyInstance))
                {
                    GetAssemblyInstanceMembersRecursive(d, el as AssemblyInstance, r);
                    continue;
                }

                r.Add(el);
            }
        }

        public static List<FilterElement> GetFilters(Document doc)
        {
            // Search for existing filters with same name
            List<FilterElement> filterList = new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FilterElement))).ToElements().Cast<FilterElement>().ToList();

            return filterList;
        }

        public static List<ElementId> GetFilterElementsId(Document doc, FilterElement filterElement)
        {
            //create the list to return
            List<ElementId> elementIds = new List<ElementId>();

            if (filterElement == null)
                return elementIds;

            try
            {


                // check if it's a selection filter or a parameter filter and...
                if (filterElement is SelectionFilterElement selectionFilter)
                {
                    // add the ids of the elements connected to that filter to the list
                    foreach (ElementId id in selectionFilter.GetElementIds())
                    {
                        elementIds.Add(id);
                    }
                }
                else if (filterElement is ParameterFilterElement parameterFilter)
                {

                    // get the ids of the categories accepted by the filter
                    ICollection<ElementId> filterCategoriesIds = parameterFilter.GetCategories();

                    // get an ElementFilter (different from Filter Element) with the conditions of this ParameterFilter Element
                    ElementFilter elementFilter = parameterFilter.GetElementFilter();

                    // if the filter has no parameters (and so the new-created ElementFilter is null)
                    if (elementFilter == null)
                    {
                        HashSet<ElementId> elemsIdSet = new HashSet<ElementId>();

                        // add all elements of the filter categories to the list
                        foreach (ElementId category in filterCategoriesIds)
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(doc)
                                .WhereElementIsNotElementType()
                                .OfCategoryId(category);

                            var f = collector.ToElementIds();
                            elemsIdSet.UnionWith(f);
                        }

                        elementIds = elemsIdSet.ToList();

                    }
                    else
                    {
                        // get the elements in the document which passes the new-created ElementFilter and add them to the list
                        //IList<Element> collector = new FilteredElementCollector(doc)
                        //    .WhereElementIsNotElementType()
                        //    .WherePasses(elementFilter)
                        //    .ToElements();


                        //var f1 = new FilteredElementCollector(doc);
                        //var f2 = f1.WhereElementIsNotElementType();
                        //var f3 = f2.WherePasses(elementFilter);
                        //IList<Element> collector = f3.ToElements();


                        //LEO
                        //foreach (Element element in collector)
                        //{
                        //    if (element.Category != null)
                        //    {
                        //        // check if the element is of a category accepted by the filter [USED TO SOLVE THE BUG]
                        //        if (filterCategoriesIds.Contains(element.Category.Id))
                        //        {
                        //            elementIds.Add(element.Id);
                        //        }
                        //    }

                        //}
                        HashSet<ElementId> elemsIdSet = new HashSet<ElementId>();

//                        ElementClassFilter excludeOpeningFilter = new ElementClassFilter(typeof(Opening), inverted: true);


                        var f1 = new FilteredElementCollector(doc);
                        var f2 = f1.WhereElementIsNotElementType();
                        var f3 = f2.WherePasses(elementFilter);

                        var elemsId = f3.ToElementIds();

                        //var elems = f3.ToElements();


                        foreach (ElementId categoryId in filterCategoriesIds)
                        {
                            var ff1 = new FilteredElementCollector(doc, elemsId);
                            var ff2 = ff1.OfCategoryId(categoryId);
                            var ff3 = ff2.ToElementIds();

                            elemsIdSet.UnionWith(ff3);
                        }

                        elementIds = elemsIdSet.ToList();






                    }
                }

            }
            catch (Exception ex)
            {
                ReJoLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }

            return elementIds;
        }


        public static List<Model3dObjectKey> GetModel3dObjectsKey(Document doc, string? rvtFilterUniqueId)
        {
            List<Model3dObjectKey> m3dObjs = new List<Model3dObjectKey>();


            ReJoLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), "GetModel3dObjectsKey");

            try
            {
                string projIfcId = CmdInit.This.GetProjectIfcGuid(doc);

                FilterElement? filterEl = doc.GetElement(rvtFilterUniqueId) as FilterElement;

                if (filterEl != null)
                {
                    List<ElementId> elementsid = Utils.GetFilterElementsId(doc, filterEl);
                    foreach (ElementId elementId in elementsid)
                    {
                        ReJoLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), elementId.Value.ToString());

                        //var el = doc.GetElement(elementId);
                        string elIfcId = CmdInit.This.GetElementIfcGuid(doc, elementId);

                        m3dObjs.Add(new Model3dObjectKey()
                        {
                            Model3DType = Model3dType.Revit,
                            ProjectGlobalId = projIfcId,
                            GlobalId = elIfcId,
                        });


                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return m3dObjs;
        }

        /// <summary>
        /// Select Revit elements
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elements"></param>
        public static void SelectElements(Document doc, List<Model3dObjectKey> elements)
        {
            //seleziono solo gli elementi del documento con lo stesso IFC_PROJECT_GUID
            var prjIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);


            IEnumerable<Model3dObjectKey> docElements = elements.Where(item => item.ProjectGlobalId == prjIfcGuid);

            if (docElements.Count() <= 0)
                return;

            IList<ElementFilter> elemFilters = new List<ElementFilter>();
            foreach (var el in docElements)
            {
                FilterValueRule fvr = new FilterStringRule(
                    new ParameterValueProvider(new ElementId(BuiltInParameter.IFC_GUID)),
                    new FilterStringEquals(), el.GlobalId);

                ElementParameterFilter elemParamFilter = new ElementParameterFilter(fvr);
                elemFilters.Add(elemParamFilter);


            }

            LogicalOrFilter elemFilter = new LogicalOrFilter(elemFilters);

            ICollection<ElementId> elemsId = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                //.OfClass(typeof(Element))
                .WherePasses(elemFilter)
                .ToElementIds();

            CmdInit.This.UIApplication.ActiveUIDocument.Selection.SetElementIds(elemsId);
        }

        public static ParameterFilterElement GetFilterElementByName(Document doc, string filterName)
        {
            // Crea un FilteredElementCollector per raccogliere tutti i ParameterFilterElement nel documento
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ParameterFilterElement));

            // Filtra gli elementi per nome utilizzando LINQ
            ParameterFilterElement filterElement = collector
                .Cast<ParameterFilterElement>()
                .FirstOrDefault(f => f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase));

            // Restituisce il filtro trovato o null se non esiste
            return filterElement;
        }

    }


    public static class ParameterExtensions
    {
        public static bool Ext_HasDisplayUnitType(
          this Parameter parameter)
        {

            var parameterType =
                parameter.Definition.GetDataType();


            if (parameterType == SpecTypeId.Reference.Image ||
                parameterType == SpecTypeId.Reference.LoadClassification ||
                parameterType == SpecTypeId.Reference.Material ||
                parameterType == SpecTypeId.Boolean.YesNo ||
                parameterType == SpecTypeId.String.MultilineText ||
                parameterType == SpecTypeId.String.Text ||
                parameterType == SpecTypeId.String.Url ||
                parameterType == SpecTypeId.Int.NumberOfPoles
                )
                return false;


            return true;
        }




    }
}
