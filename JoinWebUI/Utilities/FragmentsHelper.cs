using JoinWebUI.Services;
using Microsoft.JSInterop;
using ModelData.Dto;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Formats.Tar;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ModelData.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;


namespace JoinWebUI.Utilities
{
    public class FragmentsHelper : IDisposable, INotifyPropertyChanged
    {
        public static string NullPlaceHolder { get; } = "[non definito]";

        private LoadResult _modelLoadResult = LoadResult.NotLoaded;
        public LoadResult ModelLoadResult
        {
            get => _modelLoadResult;
            private set
            {
                LoadResult old = _modelLoadResult;
                _modelLoadResult = value;
                if (old != _modelLoadResult) NotifyPropertyChanged(nameof(ModelLoadResult));
            }
        }

        public List<ModelNavTreeNode> ClassesTree { get; private set; } = new List<ModelNavTreeNode>();
        public List<ModelNavTreeNode> TypesTree { get; private set; } = new List<ModelNavTreeNode>();
        public List<ModelNavTreeNode> SpatialStructureTree { get; set; } = new List<ModelNavTreeNode>();
        public List<ModelNavTreeNode> GroupsTree { get; private set; } = new List<ModelNavTreeNode>();

        //public List<ModelNavTreeNode> SelectedClassesTreeNodes { get; private set; } = new List<ModelNavTreeNode>();
        Dictionary<ObjectKey, ModelNavTreeNode> ClassesFlatList { get; set; } = new Dictionary<ObjectKey, ModelNavTreeNode>();
        //public List<ModelNavTreeNode> SelectedTypesTreeNodes { get; private set; } = new List<ModelNavTreeNode>();
        Dictionary<ObjectKey, ModelNavTreeNode> TypesFlatList { get; set; } = new Dictionary<ObjectKey, ModelNavTreeNode>();
        //public List<ModelNavTreeNode> SelectedSpatialStructureTreeNodes { get; set; } = new List<ModelNavTreeNode>();
        Dictionary<ObjectKey, ModelNavTreeNode> SpatialItemsFlatList { get; set; } = new Dictionary<ObjectKey, ModelNavTreeNode>();
        //public List<ModelNavTreeNode> SelectedGroupsTreeNodes { get; private set; } = new List<ModelNavTreeNode>();
        Dictionary<ObjectKey, ModelNavTreeNode> GroupsFlatList { get; set; } = new Dictionary<ObjectKey, ModelNavTreeNode>();

        Dictionary<string, string> ModelGlobalIds { get; set; } = new Dictionary<string, string>();

        //private bool _classesTypesLoaded = false;
        //private bool _spatialStructureLoaded = false;
        //private bool _groupsLoaded = false;

        public ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)> GeneralInfo { get; private set; } = new ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)>();
        public ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)> TypeInfo { get; private set; } = new ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)>();
        public ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)> Properties { get; private set; } = new ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)>();
        public ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)> Quantities { get; private set; } = new ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)>();
        public ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)> Materials { get; private set; } = new ObservableCollection<(string Key, ObservableCollection<PropertyItem> Value)>();

        private Dictionary<(string, string), string> _baseUnits = new Dictionary<(string, string), string>();

        //public List<ObjectKey> SelectedExpressIDs { get; private set; } = new List<ObjectKey>();
        //public Dictionary<ObjectKey, NodeType> SelectedExpressIDs { get; private set; } = new Dictionary<ObjectKey, NodeType>();
        public List<ModelNavTreeNode> SelectedNodes { get; private set; } = new List<ModelNavTreeNode>();

        //public List<ObjectKey> ShownExpressIDs { get; private set; } = new List<ObjectKey>();
        public bool ContextMenuVisible { get; private set; } = false;
        public (double X, double Y) ContextMenuCoordinates { get; private set; } = (0, 0);
        public bool Highlightable { get; private set; } = false;

        public ObservableCollection<ObjectKey> SelectionHistory { get; private set; } = new ObservableCollection<ObjectKey>();

        public bool HasHiddenObjects { get => HiddenObjects.Length > 0; }
        public long[] HiddenObjects { get; private set; } = Array.Empty<long>();

        private int? _clippingState = null;
        public int? ClippingState
        {
            get => _clippingState;
            private set
            {
                int? old = _clippingState;
                _clippingState = value;
                if (old != _clippingState) NotifyPropertyChanged(nameof(ClippingState));
            }
        }

        private int _lengthMeasureState = 0;
        public int LengthMeasureState
        {
            get => _lengthMeasureState;
            private set
            {
                int old = _lengthMeasureState;
                _lengthMeasureState = value;
                if (old != _lengthMeasureState) NotifyPropertyChanged(nameof(LengthMeasureState));
            }
        }

        private int _areaMeasureState = 0;
        public int AreaMeasureState
        {
            get => _areaMeasureState;
            private set
            {
                int old = _areaMeasureState;
                _areaMeasureState = value;
                if (old != _areaMeasureState) NotifyPropertyChanged(nameof(AreaMeasureState));
            }
        }

        private int _angleMeasureState = 0;
        public int AngleMeasureState
        {
            get => _angleMeasureState;
            private set
            {
                int old = _angleMeasureState;
                _angleMeasureState = value;
                if (old != _angleMeasureState) NotifyPropertyChanged(nameof(AngleMeasureState));
            }
        }

        private bool _cameraProjectionPerspective = false;
        public bool CameraProjectionPerspective
        {
            get => _cameraProjectionPerspective;
            private set
            {
                bool old = _cameraProjectionPerspective;
                _cameraProjectionPerspective = value;
                if (old != _cameraProjectionPerspective) NotifyPropertyChanged(nameof(CameraProjectionPerspective));
            }
        }

        private bool _ifcSpacesShown = false;
        public bool IfcSpacesShown
        {
            get => _ifcSpacesShown;
        }

        public async Task ToggleIfcSpaces()
        {
            _ifcSpacesShown = !IfcSpacesShown;

            if (!_ifcSpacesShown)
            {
               await UnCheckIfcSpacesInTree();
            }
            NotifyPropertyChanged(nameof(IfcSpacesShown));
            await _jsModule!.InvokeVoidAsync("setIfcSpaces", _ifcSpacesShown);

        }

        public async Task UnCheckIfcSpacesInTree()
        {
            var all = new List<ModelNavTreeNode>();
            var geomObjects = new List<ObjectKey>();

            var toAdd = await GetDeselectionNodesList(SpatialStructureTree);
            all.AddRange(toAdd);
            toAdd = await GetDeselectionNodesList(ClassesTree);
            all.AddRange(toAdd);
            toAdd = await GetDeselectionNodesList(TypesTree);
            all.AddRange(toAdd);
            toAdd = await GetDeselectionNodesList(GroupsTree);
            all.AddRange(toAdd);

            foreach (ModelNavTreeNode node in all)
            {
                node.Checked &= node.IsCheckable(_ifcSpacesShown);
                if (node.IsFragment && SelectedNodes.Contains(node) && !geomObjects.Contains(node.ID))
                {
                    geomObjects.Add(node.ID);
                }
            }

            await RemoveFromSelection(all);
            await _jsModule!.InvokeVoidAsync("removeHighlightMany", geomObjects);
        }

        private bool _geometriesSelected = false;
        public bool GeometriesSelected
        {
            get => _geometriesSelected;
            private set
            {
                bool old = _geometriesSelected;
                _geometriesSelected = value;
                //if (old != _geometriesSelected) NotifyPropertyChanged(nameof(GeometriesSelected));
            }
        }

        private int _selectedExpressIdsCount = 0;
        public int SelectedExpressIdsCount
        {
            get => _selectedExpressIdsCount;
            private set
            {
                int old = _selectedExpressIdsCount;
                _selectedExpressIdsCount = value;
                //if (old != _selectedExpressIdsCount) NotifyPropertyChanged(nameof(SelectedExpressIdsCount));
            }
        }


        private Guid _progettoId;
        private Guid _operaId;

        private long _lastFakeExpressID = long.MinValue;

        private string _uploadAllegatiUrl;
        private int _fileChunkSize = 10000000;
        private Guid _uploadingFileParentGuid = Guid.Empty;
        private string _uploadingFileName = string.Empty;

        private DotNetObjectReference<FragmentsHelper>? dotNetReference;
        private IJSRuntime _jsRuntime;
        private JoinWebApiClient _apiClient;

        private IJSObjectReference? _jsModule;
        //public bool ModelLoaded { get; private set; } = false;
        private static SemaphoreSlim _loadMutex = new SemaphoreSlim(1);
        private static SemaphoreSlim _uploadMutex = new SemaphoreSlim(1);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        public enum Spinners
        {
            Init,
            Download,
            Conversion,
            //Properties,
            Geometries,
            None
        }

        private bool[] _activeSpinners = new bool[5];


        public FragmentsHelper(string uploadAllegatiUrl, IJSRuntime jsRuntime, JoinWebApiClient apiClient)
        {
            _uploadAllegatiUrl = uploadAllegatiUrl;
            _jsRuntime = jsRuntime;
            _apiClient = apiClient;

            GeneralInfo.Add(("Oggetto", new ObservableCollection<PropertyItem>()));
            TypeInfo.Add(("Tipo", new ObservableCollection<PropertyItem>()));
            Quantities.Add(("Quantità", new ObservableCollection<PropertyItem>()));
            Materials.Add(("Materiali", new ObservableCollection<PropertyItem>()));

            dotNetReference = DotNetObjectReference.Create(this);
        }


        private void ClearSpinners()
        {
            for (int i = _activeSpinners.Length - 1; i >= 0; i--)
            {
                _activeSpinners[i] = false;
            }
            NotifyPropertyChanged(nameof(ActiveSpinner));
        }

        private void SetSpinner(Spinners spinner, bool value)
        {
            _activeSpinners[(int)spinner] = value;
            NotifyPropertyChanged(nameof(ActiveSpinner));
        }

        public Spinners ActiveSpinner
        {
            get
            {
                for (int i = 0; i < _activeSpinners.Length; i++)
                {
                    if (_activeSpinners[i])
                    {
                        return Enum.Parse<Spinners>(i.ToString());
                    }
                }
                return Spinners.None;
            }
        }

        

        public async Task LoadModels(string divId, Guid operaId, Guid progettoId, List<GlobalIdPair>? preselection)
        {
            _operaId = operaId;
            _progettoId = progettoId;
            SpatialStructureTree.Clear();
            //_spatialStructureLoaded = false;
            ClassesTree.Clear();
            TypesTree.Clear();
            //_classesTypesLoaded = false;
            GroupsTree.Clear();
            //_groupsLoaded = false;

            try
            {
                await _loadMutex.WaitAsync();

                if (_jsModule == null)
                {
                    SetSpinner(Spinners.Init, true);

                    _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./scripts/dist/thatOpen.mjs");
                    await _jsModule.InvokeVoidAsync("initViewer", [dotNetReference, divId]);

                    SetSpinner(Spinners.Init, false);
                }

                if (_jsModule != null && ModelLoadResult == LoadResult.NotLoaded)
                {

                    //ModelLoadResult = LoadResult.InProgress;

                    SetSpinner(Spinners.Download, true);

                    IDictionary<string, string> query = new Dictionary<string, string>()
                    {
                        { "progettoId", _progettoId.ToString() }
                    };

                    //bool error = false;

                    var idResult = await _apiClient.JsonGetAsync<List<IfcIdsDto>>("allegati/get-download-ifc-ids", query: query);

                    if (!idResult.Success && idResult.ResponseStatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        //error = true;
                        ModelLoadResult = LoadResult.Fail;
                        Console.WriteLine("Errore download file - Modelli 3D");
                    }
                    else if (idResult.ResponseStatusCode == System.Net.HttpStatusCode.NotFound || idResult.ResponseContentData == null || idResult.ResponseContentData.Count == 0)
                    {
                        //error = true;
                        ModelLoadResult = LoadResult.NotFound;
                        Console.WriteLine("Modello 3D non trovato");
                    }
                    else
                    {
                        for (int i = 0; i < idResult.ResponseContentData!.Count; i++)
                        {
                            //STEP 1: se necessario scarico l'IFC

                            if (idResult.ResponseContentData[i].GeometriesId == Guid.Empty || idResult.ResponseContentData[i].PropertiesId == Guid.Empty)
                            {
                                JoinWebApiClient.RequestFileResult ifcFileResult = await _apiClient.DownloadAllegatoAsync(_operaId,
                                                                                                                          idResult.ResponseContentData[i].IfcId,
                                                                                                                          false, false);

                                if (ifcFileResult.Success && ifcFileResult.FileContentBytes?.Length > 0)
                                {
                                    _uploadingFileName = Path.GetFileNameWithoutExtension(ifcFileResult.FileName!);
                                    _uploadingFileParentGuid = idResult.ResponseContentData[i].IfcId;

                                    await _jsModule.InvokeVoidAsync("loadModelFromIFC", [ifcFileResult!.FileContentBytes!, _uploadingFileName]);
                                }
                                else
                                {
                                    //error = true;
                                    ModelLoadResult = LoadResult.Fail;
                                    Console.WriteLine("Errore download file - IFC");
                                    break;
                                }

                                SetSpinner(Spinners.Download, false);
                            }


                            //STEP 2: Carico geometrie e proprietà

                            SetSpinner(Spinners.Geometries, true);

                            if (idResult.ResponseContentData[i].GeometriesId != Guid.Empty)
                            {
                                SetSpinner(Spinners.Download, true);

                                var fragFileResult = await _apiClient.DownloadAllegatoAsync(_operaId, idResult.ResponseContentData[i].GeometriesId, false, false);

                                var propFileResult = await _apiClient.DownloadAllegatoAsync(_operaId, idResult.ResponseContentData[i].PropertiesId, false, false);

                                SetSpinner(Spinners.Download, false);

                                if (fragFileResult.Success && fragFileResult.FileContentBytes?.Length > 0 && propFileResult.Success && propFileResult.FileContentBytes?.Length > 0)
                                {
                                    _uploadingFileName = Path.GetFileNameWithoutExtension(fragFileResult.FileName!);

                                    await _jsModule.InvokeVoidAsync("loadModelFromFragments", [fragFileResult.FileContentBytes, propFileResult.FileContentBytes, _uploadingFileName]);
                                }

                                fragFileResult?.Dispose();
                            }
                            else
                            {
                                SetSpinner(Spinners.Download, false);

                                SetSpinner(Spinners.Conversion, true);
                                int fragLength = await _jsModule.InvokeAsync<int>("getFragmentsInfo", [i]);
                                int propLegth = await _jsModule.InvokeAsync<int>("getPropertiesInfo", [i]);

                                await SetFragments(fragLength);
                                await SetCompleteProperties(propLegth);

                                SetSpinner(Spinners.Conversion, false);
                            }

                            //SetSpinner(Spinners.Geometries, false);

                            await Task.Delay(50);

                        }
                    }

                    if (ModelLoadResult == LoadResult.NotLoaded)
                    {
                        await _jsModule.InvokeVoidAsync("initModels");
                        ModelLoadResult = LoadResult.Success;
                        if (preselection?.Count > 0)
                        {
                            await HighlightByGlobalIDs(preselection);
                        }
                    }

                }
            }
            catch (Exception exc)
            {
                // _requestErrorMessage = exc.Message;
                // _requestError = true;
                Console.WriteLine(exc.Message);
            }
            finally
            {
                ClearSpinners();
                _loadMutex.Release();
                //GC.Collect();
            }

        }

        //public async Task LoadSpatialStructure()
        //{
        //    if (!_spatialStructureLoaded) await _jsModule.InvokeVoidAsync("extractSpatialStructure");
        //}

        [JSInvokable("UpdateUnits")]
        public async Task UpdateUnits(string ifcModelKey, string[][] units)
        {
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i].Length > 1 && !string.IsNullOrWhiteSpace(units[i][1]))
                {
                    _baseUnits.Add((ifcModelKey, units[i][0]), units[i][1]);

                    //Console.WriteLine(units[i][0] + ": " + _baseUnits[units[i][0]]);
                }
            }

        }

        //[JSInvokable("UpdateSpatialRelations")]
        //public async Task UpdateSpatialRelations(long expressID, string name, string type, bool isFragment, SpatialRelationBaseInfo[]? children)
        //{
        //    var relationObj = _flatSpatialItems.Find(s => s.ID == expressID);
        //    if (relationObj == null)
        //    {
        //        //var stType = IfcCategories.IfcCategoryMapById[type];
        //        relationObj = new ModelNavTreeNode(ModelNavTreeNodeTypes.SpatialRelation, expressID, name, type, isFragment, null);
        //        _flatSpatialItems.Add(relationObj);
        //    }

        //    if (children != null)
        //    {
        //        foreach (var child in children)
        //        {
        //            var childObj = _flatSpatialItems.Find(x => x.ID == child.ExpressID);
        //            if (childObj == null)
        //            {
        //                //var elType = IfcCategories.IfcCategoryMapById[child.Type];
        //                childObj = new ModelNavTreeNode(ModelNavTreeNodeTypes.SpatialRelation, child.ExpressID, child.Name, child.Type, child.IsFragment, relationObj);
        //                _flatSpatialItems.Add(childObj);
        //            }
        //            else
        //            {
        //                childObj.Parent = relationObj;
        //            }
        //            relationObj.Children.Add(childObj);
        //        }
        //        relationObj.Children.Sort();             
        //    }
        //}

        [JSInvokable("UpdateModelGlobalIds")]
        public async Task UpdateModelGlobalIds(ModelIdPair[] models)
        {
            foreach (var model in models)
            {
                ModelGlobalIds.Add(model.IfcModelKey, model.ModelGlobalID);
            }
        }

        [JSInvokable("BuildSpatialStructure")]
        public async Task BuildSpatialStructure(SpatialRelationBaseInfo[] relations)
        {
            //Dictionary<ObjectKey, ModelNavTreeNode> _flatSpatialItems = new Dictionary<ObjectKey, ModelNavTreeNode>();
            SpatialItemsFlatList.Clear();

            foreach (SpatialRelationBaseInfo rel in relations)
            {

                ModelNavTreeNode relationObj;

                if (SpatialItemsFlatList.ContainsKey(rel.ID))
                {
                    relationObj = SpatialItemsFlatList[rel.ID];
                }
                else
                {
                    //var stType = IfcCategories.IfcCategoryMapById[type];
                    relationObj = new ModelNavTreeNode(ModelNavTreeNodeTypes.SpatialRelation, rel.ID, rel.Name, rel.GlobalId, rel.TypeName, rel.IsFragment, rel.IsIfcSpace, rel.HasProperties, null);
                    SpatialItemsFlatList.Add(rel.ID, relationObj);
                }

                if (rel.Children != null)
                {
                    foreach (var child in rel.Children)
                    {
                        ModelNavTreeNode? childObj;
                        if (SpatialItemsFlatList.ContainsKey(child.ID))
                        {
                            childObj = SpatialItemsFlatList[child.ID];
                            childObj.Parent = relationObj;
                        }
                        else
                        {
                            //var elType = IfcCategories.IfcCategoryMapById[child.Type];
                            childObj = new ModelNavTreeNode(ModelNavTreeNodeTypes.SpatialRelation, child.ID, child.Name, child.GlobalId, child.TypeName, child.IsFragment, child.IsIfcSpace, child.HasProperties, relationObj);
                            SpatialItemsFlatList.Add(child.ID, childObj);
                        }
                        relationObj.Children.Add(childObj);
                    }
                    relationObj.Children.Sort();
                }
            }

            SpatialStructureTree.Clear();

            foreach (var item in SpatialItemsFlatList.Values)
            {
                if ((item as ModelNavTreeNode)!.Parent == null)
                {
                    SpatialStructureTree.Add((item as ModelNavTreeNode)!);
                }
            }

            GroupTreeLevelByClass(SpatialStructureTree);

            //SpatialStructure.Sort();

            //_spatialStructureLoaded = true;

            foreach (var item in SpatialStructureTree)
            {
                item.ExpandSingle();
            }

            NotifyPropertyChanged(PropertyNames.SpatialStructure);
        }

        private void GroupTreeLevelByClass(List<ModelNavTreeNode> children)
        {
            if (children?.Count > 0)
            {
                string[] classes = children.Select(x => x.ObjectType).Distinct().ToArray() ?? Array.Empty<string>();

                foreach (string c in classes)
                {
                    var items = children.Where(x => x.ObjectType == c).ToList();
                    var cItem = new ModelNavTreeNode(ModelNavTreeNodeTypes.GroupByType, new ObjectKey(_lastFakeExpressID++), c, null, c, false, false, false, items[0].Parent);
                    foreach (var item in items)
                    {
                        children.Remove(item);
                        cItem.Children.Add(item);
                        item.Parent = cItem;
                        GroupTreeLevelByClass(item.Children);
                    }
                    cItem.Children.Sort();
                    children.Add(cItem);
                }

                children.Sort();

            }

        }



        private (ModelNavTreeNode Parent, int Index)? FindSpatialRelation(ObjectKey id, IEnumerable<ModelNavTreeNode> list)
        {
            (ModelNavTreeNode parent, int index)? result = null;

            foreach (ModelNavTreeNode item in list)
            {
                if (item.Children != null)
                {
                    int index = -1;
                    for (int i = 0; i < item.Children.Count; i++)
                    {
                        if (item.Children[i].ID.Equals(id))
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index >= 0)
                    {
                        result = (item, index);
                    }
                    else
                    {
                        result = FindSpatialRelation(id, item.Children);
                    }

                    if (result.HasValue) break;
                }
            }

            return result;
        }


        //public async Task LoadTypes()
        //{
        //    if (!_classesTypesLoaded) await _jsModule.InvokeVoidAsync("extractTypes");
        //}

        [JSInvokable("UpdateTypesStructures")]
        public async Task UpdateTypesStructures(TypeBaseInfo[] classList, TypeBaseInfo[] typeList)
        {
            ClassesTree.Clear();
            foreach (var classItem in classList)
            {
                var node = CreateTreeNode(classItem, null);
                ClassesTree.Add(node);
                AddToDictionaryWithRecursion(ClassesFlatList, node);
            }
            ClassesTree.Sort();

            NotifyPropertyChanged(PropertyNames.ClassesTree);

            TypesTree.Clear();
            foreach (var type in typeList)
            {
                var node = CreateTreeNode(type, null);
                TypesTree.Add(node);
                AddToDictionaryWithRecursion(TypesFlatList, node);
            }
            TypesTree.Sort();

            NotifyPropertyChanged(PropertyNames.TypesTree);

        }

        //public async Task LoadGroups()
        //{
        //    if (!_groupsLoaded) await _jsModule.InvokeVoidAsync("extractGroups");
        //}

        [JSInvokable("UpdateGroups")]
        public async Task UpdateGroups(GroupBaseInfo[] groups)
        {
            GroupsTree.Clear();

            foreach (var groupItem in groups)
            {
                var node = CreateTreeNode(groupItem, null);
                //GroupTreeLevelByClass(node.Children);
                GroupsTree.Add(node);
                AddToDictionaryWithRecursion(GroupsFlatList, node);
            }
            GroupTreeLevelByClass(GroupsTree);

            //_groupsLoaded = true;

            NotifyPropertyChanged(PropertyNames.GroupsTree);

        }

        private ModelNavTreeNode CreateTreeNode(TypeBaseInfo info, ModelNavTreeNode? parent)
        {
            ModelNavTreeNode result = new ModelNavTreeNode(ModelNavTreeNodeTypes.Type, info.ID, info.Name, info.GlobalId, info.SuperTypeName, false, info.IsIfcSpace, info.HasProperties, parent);

            foreach (var child in info.ChildrenTypes)
            {
                result.Children.Add(CreateTreeNode(child, result));
            }
            foreach (var child in info.ChildrenElements)
            {
                result.Children.Add(CreateTreeNode(child, result));
            }
            result.Children.Sort();

            return result;
        }

        private ModelNavTreeNode CreateTreeNode(GroupBaseInfo info, ModelNavTreeNode? parent)
        {
            ModelNavTreeNode result = new ModelNavTreeNode(ModelNavTreeNodeTypes.Group, info.ID, info.Name, info.GlobalId, info.TypeName, info.IsFragment, info.IsIfcSpace, info.HasProperties, parent);

            foreach (var child in info.ChildrenElements)
            {
                result.Children.Add(CreateTreeNode(child, result));
            }
            //TODO
            //foreach (var child in info.Buildings)
            //{
            //    result.Children.Add(CreateTreeNode(child, result));
            //}
            result.Children.Sort();

            return result;
        }

        private ModelNavTreeNode CreateTreeNode(ElementBaseInfo info, ModelNavTreeNode? parent)
        {
            ModelNavTreeNode result = new ModelNavTreeNode(ModelNavTreeNodeTypes.Element, info.ID, info.Name, info.GlobalId, info.TypeName, info.IsFragment, info.IsIfcSpace, info.HasProperties, parent);

            return result;
        }

        private void AddToDictionaryWithRecursion(Dictionary<ObjectKey, ModelNavTreeNode> dict, ModelNavTreeNode root)
        {
            try
            {
                if (!dict.ContainsKey(root.ID))
                {
                    dict.Add(root.ID, root);
                }
                if (root.HasChildren)
                {
                    foreach (var child in root.Children)
                    {
                        AddToDictionaryWithRecursion(dict, child);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(root.ID.ExpressID + " - " + root.ID.IfcModelKey);
            }

        }

        public void GoBack()
        {
            if (SelectionHistory.Count > 1)
            {
                var item = SelectionHistory[SelectionHistory.Count - 2];
                LoadProperties(item, true);
            }
        }

        private async Task<List<ModelNavTreeNode>> GetSubTreeNodeList(ModelNavTreeNode root)
        {
            //List<ObjectKey> geometries = new List<ObjectKey>();
            //List<ObjectKey> noGeometries = new List<ObjectKey>();
            //List<ObjectKey> groupings = new List<ObjectKey>();
            List<ModelNavTreeNode> descendants = new List<ModelNavTreeNode>();

            //if (root.IsFragment)
            //{
            //    geometries.Add(root.ID);
            //}
            //else if (root.IsSelectable)
            //{
            //    noGeometries.Add(root.ID);
            //}
            //else if(root.HasSelectableChildren(IfcSpacesShown))
            //{
            //    groupings.Add(root.ID);
            //}

            if (root != null && root.IsCheckable(IfcSpacesShown))
            {
                descendants.Add(root);
            }
            foreach (var child in root.Children)
            {
                //var children = await GetSubTreeNodeList((child));
                //geometries.AddRange(children.geometries);
                //noGeometries.AddRange(children.noGeometries);
                //groupings.AddRange(children.groupings);
                descendants.AddRange(await GetSubTreeNodeList((child)));

            }
            

            //return (geometries, noGeometries, groupings);
            return descendants;

        }

        public async Task CheckSubTrees(List<ModelNavTreeNode> tree)
        {
            foreach (ModelNavTreeNode node in tree)
            {
                if (node.ID != null && node.Checked && node.HasChildren)
                {
                    await CheckSubTree(node);
                }
                else if (node.IsCheckable(IfcSpacesShown))
                {
                    await CheckSubTrees(node.Children);
                }
            }
        }

        public async Task CheckSubTree(ModelNavTreeNode root)
        {

            var checkedList = await GetSubTreeNodeList(root);
            await UpdateSelection(checkedList, false); // vengono selezionati solo i selezionabili
            var geomObjects = checkedList.Where(x => x.IsFragment).Select(x=>x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightMany", [geomObjects, false, true]);
            //foreach (var item in result.Where(x => !x.IsSelectable && x.HasSelectableChildren(IfcSpacesShown)))
            //{
            //    await Highlight((item, false));
            //}
        }

        private async Task<List<ModelNavTreeNode>> GetDeselectionNodesList(List<ModelNavTreeNode> tree)
        {
            List<ModelNavTreeNode> descendants = new List<ModelNavTreeNode>();

            foreach (var root in tree)
            {
                if (root != null)
                {
                    if ((!root.IsSelectable(_ifcSpacesShown) && SelectedNodes.Contains(root)) || (!root.IsCheckable(_ifcSpacesShown) && root.Checked))
                    {
                        descendants.Add(root);
                    }
                    if (root.HasChildren)
                    {
                        descendants.AddRange(await GetDeselectionNodesList(root.Children));
                    }
                }
            }

            return descendants;

        }

        public async Task Highlight((ModelNavTreeNode node, bool RemovePrevious) args)
        {
          
        //}

        //public async Task Highlight(ObjectKey ID, NodeType type, bool RemovePrevious)
        //{
            //if (args.node.NodeType != ModelNavTreeNodeTypes.GroupByType)
            //{
                await UpdateSelection(new List<ModelNavTreeNode>() { args.node }, args.RemovePrevious);
                if (args.node.IsFragment)
                {
                    await _jsModule!.InvokeVoidAsync("highlight", [args.node.ID.IfcModelKey, args.node.ID.ExpressID, args.RemovePrevious, true]);
                }
            //}
            //else //è il caso dei raggruppamenti
            //{
            //    //await UpdateSelectedIDs([], [args.ID], args.RemovePrevious);
            //    if (!SelectedExpressIDs.ContainsKey(args.node.ID))
            //    {
            //        SelectedExpressIDs.Add(args.node.ID, NodeType.Grouping);
            //        NotifyPropertyChanged(PropertyNames.SelectedExpressIDs);
            //    }
            //}

        }

        public async Task HighlightCurrent()
        {
            if (SelectionHistory.Count > 1)
            {
                var item = SelectionHistory.Last();
                ModelNavTreeNode? node;
                if (SpatialItemsFlatList.TryGetValue(item, out node))
                {
                    await Highlight((node, true));
                }
                              
                
            }
        }

        //public async Task RemoveHighlight(List<ModelNavTreeNode> nodes)
        //{
        //    await RemoveFromSelection(nodes);
        //    if (nodes.Any(x => x.IsFragment))
        //    {
        //        var toHighlight = nodes.Where(x => x.IsFragment).Select(x => x.ID).Distinct().ToList();
        //        if (toHighlight != null && toHighlight.Any())
        //        {
        //            await _jsModule!.InvokeVoidAsync("removeHighlightMany", toHighlight);
        //        }
        //    }

        //}

        public async Task RemoveHighlight(ModelNavTreeNode node)
        {
            await RemoveFromSelection(node);
            if (node.IsFragment)
            {               
                await _jsModule!.InvokeVoidAsync("removeHighlight", [node.ID.IfcModelKey, node.ID.ExpressID]);
            }
            
        }

        public async Task HighlightNone()
        {
            await _jsModule!.InvokeVoidAsync("highlightNone");
        }


        private List<ModelNavTreeNode> GetSelectedClassAllItems()
        {
            var result = new List<ModelNavTreeNode>();

            //foreach (var root in SelectedClassesTreeNodes)
            //{
            //    var parent = root.Parent;
            //    if (parent != null)
            //    {
            //        foreach (var child in parent.Children)
            //        {
            //            result.Add(child.ID);
            //        }
            //    }
            //}

            foreach (var item in ClassesFlatList)
            {
                if (SelectedNodes.Contains(item.Value))
                {
                    var parent = item.Value.Parent;
                    if (parent != null)
                    {
                        foreach (var child in parent.Children)
                        {
                            result.Add(child);
                        }
                    }
                }
            }

            return result;
        }

        private List<ModelNavTreeNode> GetSelectedTypeAllItems()
        {
            var result = new List<ModelNavTreeNode>();

            //foreach (var root in SelectedTypesTreeNodes)
            //{
            //    var parent = root.Parent;
            //    if (parent != null)
            //    {
            //        foreach (var child in parent.Children)
            //        {
            //            result.Add(child.ID);
            //        }
            //    }
            //}

            foreach (var item in TypesFlatList)
            {
                if (SelectedNodes.Contains(item.Value))
                {
                    var parent = item.Value.Parent;
                    if (parent != null)
                    {
                        foreach (var child in parent.Children)
                        {
                            result.Add(child);
                        }
                    }
                }
            }

            return result;

        }

        private async Task<List<ModelNavTreeNode>> GetSelectedStoreyAllItems()
        {
            //var geometries = new List<ObjectKey>();
            //var noGeometries = new List<ObjectKey>();
            //var groupings = new List<ObjectKey>();
            var result = new List<ModelNavTreeNode>();

            //foreach (var root in SelectedSpatialStructureTreeNodes)
            //{
            //    ModelNavTreeNode? parent = root;
            //    while (parent != null && parent.ObjectType != "IFCBUILDINGSTOREY")
            //    {
            //        parent = parent.Parent;
            //    }
            //    if (parent != null)
            //    {
            //        result.AddRange(await GetSubTreeNodeList(parent));
            //    }

            //}

            foreach (var item in SpatialItemsFlatList)
            {
                if (SelectedNodes.Contains(item.Value))
                {
                    ModelNavTreeNode? parent = item.Value;
                    while (parent != null && parent.ObjectType != "IFCBUILDINGSTOREY")
                    {
                        parent = parent.Parent;
                    }
                    if (parent != null)
                    {
                        //var found = await GetSubTreeNodeList(parent);
                        //geometries.AddRange(found.geometries);
                        //noGeometries.AddRange(found.noGeometries);
                        //groupings.AddRange(found.groupings);
                        result.AddRange(await GetSubTreeNodeList(parent));
                    }
                }
            }

            //return (geometries.Distinct().ToArray(), noGeometries.Distinct().ToArray(), groupings.Distinct().ToArray());
            return result;

        }

        public async Task RestoreAll()
        {
            await _jsModule!.InvokeVoidAsync("showAll");
            await _jsModule!.InvokeVoidAsync("removeAllTransparency");
            await _jsModule!.InvokeVoidAsync("resetZoom");
        }

        public async Task HighlightClass(bool includeHidden)
        {
            var objects = GetSelectedClassAllItems();
            await UpdateSelection(objects, false);
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightMany", [geomObjects, false, includeHidden]);
        }

        public async Task HighlightType(bool includeHidden)
        {
            var objects = GetSelectedTypeAllItems();
            await UpdateSelection(objects, false);
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightMany", [geomObjects, false, includeHidden]);
        }

        public async Task HighlightStorey(bool includeHidden)
        {
            var objects = await GetSelectedStoreyAllItems();
            await UpdateSelection(objects, false);
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightMany", [geomObjects, false, includeHidden]);
        }

        //public async Task HighlightContainer(bool includeHidden)
        //{
        //    var parents = new List<ObjectKey>();

        //    foreach (var item in SpatialItemsFlatList)
        //    {
        //        if (SelectedExpressIDs.Contains(item.Key))
        //        {
        //            ModelNavTreeNode? parent = item.Value.Parent;
        //            while (parent != null && parent.NodeType == ModelNavTreeNodeTypes.GroupByType)
        //            {
        //                parent = parent.Parent;
        //            }
        //            if (parent != null)
        //            {
        //                if (parent.IsFragment)
        //                {
        //                    parents.Add(parent.ID);
        //                }
        //                else
        //                {
        //                    await UpdateSelectedIDs([parent.ID], false, [], false, false);
        //                    await _jsModule!.InvokeVoidAsync("highlight", [parent.ID.IfcModelKey, parent.ID.ExpressID, true, includeHidden]);
        //                    return;
        //                }
        //            }
        //        }
        //    }

        //    var objects = parents.Distinct().ToArray();
        //    await UpdateSelectedIDs(objects, false, [], false, false);
        //    await _jsModule!.InvokeVoidAsync("highlightMany", [objects, true, includeHidden]);
        //}

        public async Task IsolateClass(bool resetHidden)
        {
            var objects = GetSelectedClassAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("isolateMany", [geomObjects, resetHidden]);
        }

        public async Task IsolateType(bool resetHidden)
        {
            var objects = GetSelectedTypeAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("isolateMany", [geomObjects, resetHidden]);
        }

        public async Task IsolateStorey(bool resetHidden)
        {
            var objects = await GetSelectedStoreyAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("isolateMany", [geomObjects, resetHidden]);
        }

        public async Task HighlightOpaqueClass(bool resetHidden)
        {
            var objects = GetSelectedClassAllItems();
            await UpdateSelection(objects, true);
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightOpaqueMany", [geomObjects, resetHidden]);
        }

        public async Task HighlightOpaqueType(bool resetHidden)
        {
            var objects = GetSelectedTypeAllItems();
            await UpdateSelection(objects, true);
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightOpaqueMany", [geomObjects, resetHidden]);
        }

        public async Task HighlightOpaqueStorey(bool resetHidden)
        {
            var objects = await GetSelectedStoreyAllItems();
            await UpdateSelection(objects, true);
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightOpaqueMany", [geomObjects, resetHidden]);
        }

        public async Task ShowClass()
        {
            var objects = GetSelectedClassAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("showMany", [geomObjects]);
        }

        public async Task ShowType()
        {
            var objects = GetSelectedTypeAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("showMany", [geomObjects]);
        }

        public async Task ShowStorey()
        {
            var objects = await GetSelectedStoreyAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("showMany", [geomObjects]);
        }

        public async Task HideClass()
        {
            var objects = GetSelectedClassAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("hideMany", [geomObjects]);
        }

        public async Task HideType()
        {
            var objects = GetSelectedTypeAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("hideMany", [geomObjects]);
        }

        public async Task HideStorey()
        {
            var objects = await GetSelectedStoreyAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("hideMany", [geomObjects]);
        }

        public async Task SetTransparentClass()
        {
            var objects = GetSelectedClassAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("setTransparentMany", [geomObjects]);
        }

        public async Task SetTransparentType()
        {
            var objects = GetSelectedTypeAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("setTransparentMany", [geomObjects]);
        }

        public async Task SetTransparentStorey()
        {
            var objects = await GetSelectedStoreyAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("setTransparentMany", [geomObjects]);
        }

        public async Task RemoveTransparencyClass()
        {
            var objects = GetSelectedClassAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("removeTransparencyMany", [geomObjects]);
        }

        public async Task RemoveTransparencyType()
        {
            var objects = GetSelectedTypeAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("removeTransparencyMany", [geomObjects]);
        }

        public async Task RemoveTransparencyStorey()
        {
            var objects = await GetSelectedStoreyAllItems();
            var geomObjects = objects.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("removeTransparencyMany", [geomObjects]);
        }

        
        [JSInvokable("UpdateHasHiddenObjects")]
        public void UpdateHasHiddenObjects(long[] hiddenObjects)
        {
            //HasHiddenObjects = value;
            HiddenObjects = hiddenObjects ?? new long[0];
        }

        //public async Task RefreshView()
        //{
        //    await _jsModule!.InvokeVoidAsync("refreshView");
        //}

        #region Selection properties

        //[JSInvokable("UpdateSelectedIds")]
        //public async Task<int> UpdateSelectedIDs(ObjectKey[] geometriesExpressIDs, bool removePreviousFragments, ObjectKey[] noGeometriesExpressIDs, bool removePreviousNoFragments, ObjectKey[] groupingsExpressIDs, bool removePreviousGroups)
        //{
        //    var all = new Dictionary<ObjectKey, NodeType>();

        //    foreach (var item in SelectedExpressIDs) {
        //        switch (item.Value)
        //        {
        //            case NodeType.Fragment:
        //                if (!removePreviousFragments)
        //                {
        //                    all.Add(item.Key, NodeType.Fragment);
        //                }
        //                break;
        //            case NodeType.NoFragment:
        //                if (!removePreviousNoFragments)
        //                {
        //                    all.Add(item.Key, NodeType.NoFragment);
        //                }
        //                break;
        //            case NodeType.Grouping:
        //                if (!removePreviousGroups)
        //                {
        //                    all.Add(item.Key, NodeType.Grouping);
        //                }
        //                break;
        //        }
        //    }

        //    foreach (var id in geometriesExpressIDs)
        //    {
        //        if (!all.ContainsKey(id))
        //        {
        //            all.Add(id, NodeType.Fragment);
        //        }
        //    }
        //    foreach (var id in noGeometriesExpressIDs)
        //    {
        //        if (!all.ContainsKey(id))
        //        {
        //            all.Add(id, NodeType.NoFragment);
        //        }
        //    }
        //    foreach (var id in groupingsExpressIDs)
        //    {
        //        if (!all.ContainsKey(id))
        //        {
        //            all.Add(id, NodeType.Grouping);
        //        }
        //    }

        //    SelectedExpressIDs = all;

        //    GeometriesSelected = all.Any(x => x.Value == NodeType.Fragment);
        //    NotifyPropertyChanged(PropertyNames.SelectedExpressIDs);
        //    if (SelectedExpressIDs.Count == 1)
        //    {
        //        LoadProperties(SelectedExpressIDs.First().Key);
        //    }
        //    return SelectedExpressIDs.Count;
        //}

        [JSInvokable("UpdateSelection")]
        public async Task<int> UpdateSelection(ObjectKey[] geometriesExpressIDs, bool removePreviousFragments, bool removePreviousNoFragments, bool removePreviousGroups)
        {
            if (removePreviousFragments && removePreviousNoFragments && removePreviousGroups)
            {
                SelectedNodes.Clear();
                UncheckAllNodes(SpatialStructureTree);
                UncheckAllNodes(ClassesTree);
                UncheckAllNodes(TypesTree);
                UncheckAllNodes(GroupsTree);
            }
            else
            {
                if (removePreviousFragments)
                {
                    SelectedNodes.RemoveAll(x => x.IsFragment);
                }
                if (removePreviousNoFragments)
                {
                    SelectedNodes.RemoveAll(x => !x.IsFragment && x.NodeType != ModelNavTreeNodeTypes.GroupByType);
                }
                if (removePreviousGroups)
                {
                    SelectedNodes.RemoveAll(x => x.NodeType == ModelNavTreeNodeTypes.GroupByType);
                }
            }

            foreach(var key in geometriesExpressIDs)
            {
                if (SpatialItemsFlatList.ContainsKey(key))
                {
                    var value = SpatialItemsFlatList[key];
                    AddToSelectedNodes(value);
                }
                if (ClassesFlatList.ContainsKey(key))
                {
                    var value = ClassesFlatList[key];
                    AddToSelectedNodes(value);
                }
                if (TypesFlatList.ContainsKey(key))
                {
                    var value = TypesFlatList[key];
                    AddToSelectedNodes(value);
                }
                if (GroupsFlatList.ContainsKey(key))
                {
                    var value = GroupsFlatList[key];
                    AddToSelectedNodes(value);
                }
            }

            await UpdateAuxSelectionInfo();
            return SelectedExpressIdsCount;

        }

        public async Task<int> UpdateSelection(List<ModelNavTreeNode> nodes, bool removePrevious)
        {
            try
            {
                if (removePrevious)
                {
                    SelectedNodes.Clear();
                    UncheckAllNodes(SpatialStructureTree);
                    UncheckAllNodes(ClassesTree);
                    UncheckAllNodes(TypesTree);
                    UncheckAllNodes(GroupsTree);
                }


                foreach (var node in nodes)
                {
                    AddToSelectedNodes(node);

                    if (SpatialItemsFlatList.ContainsKey(node.ID))
                    {
                        var value = SpatialItemsFlatList[node.ID];
                        AddToSelectedNodes(value);
                    }
                    if (ClassesFlatList.ContainsKey(node.ID))
                    {
                        var value = ClassesFlatList[node.ID];
                        AddToSelectedNodes(value);
                    }
                    if (TypesFlatList.ContainsKey(node.ID))
                    {
                        var value = TypesFlatList[node.ID];
                        AddToSelectedNodes(value);
                    }
                    if (GroupsFlatList.ContainsKey(node.ID))
                    {
                        var value = GroupsFlatList[node.ID];
                        AddToSelectedNodes(value);
                    }
                }

                await UpdateAuxSelectionInfo();

                //List<ObjectKey> geometriesExpressIDs = new List<ObjectKey>();
                //List<ObjectKey> noGeometriesExpressIDs = new List<ObjectKey>();
                //List<ObjectKey> groupingsExpressIDs = new List<ObjectKey>();

                //foreach (var node in nodes)
                //{
                //    if (node.IsFragment)
                //    {
                //        geometriesExpressIDs.Add(node.ID);
                //    }
                //    else if (node.NodeType != ModelNavTreeNodeTypes.GroupByType)
                //    {
                //        noGeometriesExpressIDs.Add(node.ID);
                //    }
                //    else
                //    {
                //        groupingsExpressIDs.Add(node.ID);
                //    }
                //}
                //return await UpdateSelectedIDs(geometriesExpressIDs.ToArray(), removePrevious, noGeometriesExpressIDs.ToArray(), removePrevious, groupingsExpressIDs.ToArray(), removePrevious);
            }
            catch
            {
                foreach (var node in nodes)
                {
                    Console.WriteLine(node.ID.ExpressID);
                }
            }
            return SelectedExpressIdsCount;
        }

        private void AddToSelectedNodes(ModelNavTreeNode node)
        {
            if (!SelectedNodes.Contains(node))
            {
                node.Checked = true;
                if (node.IsSelectable(_ifcSpacesShown))
                {
                    SelectedNodes.Add(node);
                }
            }
        }

        public async Task<int> RemoveFromSelection(ModelNavTreeNode node)
        {
            //for (int i = SelectedNodes.Count - 1; i >= 0; i--)
            //{
            //    if (SelectedNodes[i].ID.Equals(node.ID))
            //    {
            //        SelectedNodes.RemoveAt(i);
            //    }
            //}

            var brothers = new List<ModelNavTreeNode>(SelectedNodes.Where(x => x.ID.Equals(node.ID)));
            if (brothers != null)
            {
                foreach (var brother in brothers)
                {
                    SelectedNodes.Remove(brother);
                    brother.Checked = false;
                }
            }

            await UpdateAuxSelectionInfo();
            return SelectedExpressIdsCount;
        }

        
        public async Task<int> RemoveFromSelection(List<ModelNavTreeNode> nodes)
        {
            //si attende l'elenco completo dei nodi su tutti gli alberi, quindi non cerca i nodi con chiave uguale
            foreach (ModelNavTreeNode node in nodes)
            {
                SelectedNodes.Remove(node);
                //node.Checked = false;
                //var brothers = new List<ModelNavTreeNode>(SelectedNodes.Where(x => x.ID.Equals(node.ID)));
                //if (brothers != null)
                //{
                //    foreach (var brother in brothers)
                //    {
                //        SelectedNodes.Remove(brother);
                //        brother.Checked = false;
                //    }
                //}
            }

            await UpdateAuxSelectionInfo();
            return SelectedExpressIdsCount;
        }

        [JSInvokable("ClearSelection")]
        public async Task<int> ClearSelection()
        {
            SelectedNodes.Clear();
            UncheckAllNodes(SpatialStructureTree);
            UncheckAllNodes(ClassesTree);
            UncheckAllNodes(TypesTree);
            UncheckAllNodes(GroupsTree);
            await UpdateAuxSelectionInfo();
            return SelectedExpressIdsCount;
        }

        private void UncheckAllNodes(List<ModelNavTreeNode> tree)
        {
            foreach (var node in tree)
            {
                node.Checked = false;
                if (node.HasChildren)
                {
                    UncheckAllNodes(node.Children);
                }
            }
        }

        private async Task UpdateAuxSelectionInfo()
        {
            Dictionary<ObjectKey, int> dict = new Dictionary<ObjectKey, int>();
            GeometriesSelected = false;
            foreach (ModelNavTreeNode node in SelectedNodes)
            {
                if (dict.ContainsKey(node.ID))
                {
                    dict[node.ID]++;
                }
                else
                {
                    dict.Add(node.ID, 1);
                }
                if (node.IsFragment) GeometriesSelected = true;
                if (node.IsCheckable(_ifcSpacesShown)) node.Checked = true;
            }
            SelectedExpressIdsCount = dict.Count;
            if (SelectedExpressIdsCount == 1)
            {
                LoadProperties(SelectedNodes.First().ID, false);
            }
            else
            {
                await UpdateProperties(null, string.Empty, false);
            }
            NotifyPropertyChanged(PropertyNames.Selection);
        }


        public async Task ShowSelectedInAllTrees()
        {
            await ShowSelectedInTree(SpatialStructureTree, PropertyNames.SpatialStructure);
            await ShowSelectedInTree(ClassesTree, PropertyNames.ClassesTree);
            await ShowSelectedInTree(TypesTree, PropertyNames.TypesTree);
            await ShowSelectedInTree(GroupsTree, PropertyNames.GroupsTree);
        }
        public async Task ShowSelectedInSpatialStructureTree()
        {
            await ShowSelectedInTree(SpatialStructureTree, PropertyNames.SpatialStructure);
        }
        public async Task ShowSelectedInClassesTree()
        {
            await ShowSelectedInTree(ClassesTree, PropertyNames.ClassesTree);
        }
        public async Task ShowSelectedInTypesTree()
        {
            await ShowSelectedInTree(TypesTree, PropertyNames.TypesTree);
        }
        public async Task ShowSelectedInGroupsTree()
        {
            await ShowSelectedInTree(GroupsTree, PropertyNames.GroupsTree);
        }

        private async Task ShowSelectedInTree(List<ModelNavTreeNode> tree, string idPrefix)
        {
            if (SelectedExpressIdsCount > 0)
            {
                UpdateNavExpansion(tree);
                await ScrollToElement(idPrefix, SelectedNodes.First().ID);
            }
        }

        private bool UpdateNavExpansion(List<ModelNavTreeNode> list)
        {
            var expandParent = false;
            foreach (var item in list)
            {
                //item.Highlighted = SelectedExpressIDs.Contains(item.ID);
                var forceExpand = item.HasChildren && UpdateNavExpansion(item.Children);
                item.Expanded |= forceExpand;
                expandParent |= (forceExpand || SelectedNodes.Contains(item)); // (forceExpand || item.Highlighted);
                //if (item.Highlighted)
                //{
                //    selectedNodesList.Add(item);
                //}
            }
            return expandParent;
        }

        private async Task ScrollToElement(string tree, ObjectKey id)
        {
            await _jsRuntime.InvokeVoidAsync("scrollToElement", tree + "_" + id.IfcModelKey + "-" + id.ExpressID);
        }

        //[JSInvokable("UpdateShownIds")]
        //public async Task<int> UpdateShownIDs(ObjectKey[] expressIDs)
        //{
        //    ShownExpressIDs = new List<ObjectKey>(expressIDs.Distinct<ObjectKey>());
        //    NotifyPropertyChanged(PropertyNames.ShownExpressIDs);

        //    return ShownExpressIDs.Count;
        //}

        public string propJson { get; private set; }

        [JSInvokable("UpdateProperties")]
        public async Task UpdateProperties(ObjectKey? selectedItem, string properties, bool keepHistory)
        {
            try
            {
                //var start = DateTime.Now;

                propJson = properties;

                this.GeneralInfo[0].Value.Clear();
                this.TypeInfo[0].Value.Clear();
                this.Properties.Clear();
                this.Quantities[0].Value.Clear();
                this.Materials.Clear();

                if (selectedItem != null && selectedItem.IfcModelKey != null)
                {
                    if (!keepHistory)
                    {
                        SelectionHistory.Clear();
                        SelectionHistory.Add(selectedItem);
                    }
                    else if (SelectionHistory.Count > 1 && SelectionHistory[SelectionHistory.Count - 2].Equals(selectedItem))
                    {
                        SelectionHistory.RemoveAt(SelectionHistory.Count - 1);
                    }
                    else
                    {
                        SelectionHistory.Add(selectedItem);
                    }

                    if (SelectionHistory.Count > 1)
                    {
                        Highlightable = await _jsModule!.InvokeAsync<bool>("isFragment", [selectedItem.IfcModelKey, selectedItem.ExpressID]);
                    }
                    else
                    {
                        Highlightable = false;
                    }

                    //_start = DateTime.Now;
                    if (!properties.StartsWith("["))
                    {
                        properties = "[" + properties + "]";
                    }
                    List<JToken> props = ExtractProperties(JArray.Parse(properties ?? string.Empty));
                    //JToken item = JToken.Parse(properties);

                    if (props?.Count > 0 && props[0] is JObject)
                    //if (item != null)
                    {
                        GeneralInfo[0] = (GeneralInfo[0].Key, await ExtractGeneralProperties(selectedItem, (props[0] as JObject)!, true));
                        //GeneralInfo[0] = (GeneralInfo[0].Key, await ExtractGeneralProperties((item as JObject)!, true));

                        for (int i = 1; i < props.Count(); i++)
                        {
                            var item = props[i];
                            long? eID = item?.Value<long>("expressID");
                            string ifcType = Utilities.IfcCategories.IfcCategoryMapById[item!.Value<long>("type")];

                            if (eID.HasValue)
                            {
                                //Console.WriteLine("1 - " + ifcType);
                                switch (ifcType)
                                {
                                    case "IFCPROPERTYSET":

                                        //Proprietà dell'oggetto
                                        await ProcessPropertySet(selectedItem.IfcModelKey, item);
                                        break;

                                    case "IFCELEMENTQUANTITY":
                                        await ProcessQuantities(selectedItem.IfcModelKey, item);
                                        break;

                                    case "IFCMATERIALLIST":
                                        if (Materials.Count == 0)
                                        {
                                            Materials.Add(("Materiali", new ObservableCollection<PropertyItem>()));

                                            JArray? materialArray = item.Value<JArray>("Materials");
                                            if (materialArray != null)
                                            {
                                                foreach (var material in materialArray)
                                                {
                                                    var id = material.Value<long?>("value");
                                                    var name = await _jsModule.InvokeAsync<string>("getName", [selectedItem.IfcModelKey, id]);

                                                    Materials[0].Value.Add(new PropertyItem("Materiale", name));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Unexpected: Item with multiple material sets.");
                                        }
                                        break;

                                    case "IFCMATERIALLAYERSETUSAGE":
                                        if (Materials.Count == 0)
                                        {
                                            Materials.Add(("Insieme di livelli", new ObservableCollection<PropertyItem>()));
                                            long? layerSetEID = item["ForLayerSet"]?.Value<long?>("value");
                                            string? direction = item["LayerSetDirection"]?.Value<string?>("value");
                                            Materials[0].Value.Add(new PropertyItem("LayerSetDirection", direction));
                                            string? sense = item["DirectionSense"]?.Value<string?>("value");
                                            Materials[0].Value.Add(new PropertyItem("DirectionSense", sense));
                                            double? offset = item["OffsetFromReferenceLine"]?.Value<double?>("value");
                                            Materials[0].Value.Add(new PropertyItem("OffsetFromReferenceLine", offset.ToString()));
                                            await _jsModule!.InvokeVoidAsync("getMaterials", selectedItem.IfcModelKey, layerSetEID);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Unexpected: Item with multiple material sets.");
                                        }
                                        break;

                                    case "IFCBUILDINGSTOREY":
                                        //var tmp3 = await _jsModule!.InvokeAsync<string>("getProperties", eID);

                                        var propertyKeys = new string[] { "Name", "LongName", "Description", "Elevation" };

                                        foreach (string name in propertyKeys)
                                        {
                                            if (item[name] != null && item[name]!.HasValues)
                                            {
                                                var value = item[name]!.Value<string>("value") ?? string.Empty;
                                                GeneralInfo.First().Value.Add(new PropertyItem("Piano: " + name, value));
                                            }
                                        }

                                        break;

                                    default:

                                        JArray? propertySets = item.Value<JArray>("HasPropertySets");
                                        if (propertySets != null && item is JObject)
                                        {
                                            // E' un tipo ?!?
                                            string typeName = item["Name"]?.Value<string>("value") ?? string.Empty;
                                            GeneralInfo.First().Value.Add(new PropertyItem("Defining Type", typeName, eID));
                                            TypeInfo[0] = (TypeInfo[0].Key, await ExtractGeneralProperties(selectedItem, (item as JObject)!, false));
                                        }
                                        break;
                                }
                            }

                        }

                        //var elapsed = DateTime.Now - _start;

                    }

                }

                //Console.WriteLine(DateTime.Now - start);

                NotifyPropertyChanged(PropertyNames.Properties);
                NotifyPropertyChanged(PropertyNames.Materials);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private List<JToken> ExtractProperties(JArray? propObj)
        {
            var result = new List<JToken>();

            if (propObj != null)
            {
                foreach (var item in propObj)
                {
                    if (item.Type == JTokenType.Object)
                    {
                        int expressID = item.Value<int>("expressID");
                        result.Add(item as JToken);
                    }
                    else
                    {
                        Console.WriteLine("unknown found");
                    }
                }

            }

            return result;

        }

        private async Task<ObservableCollection<PropertyItem>> ExtractGeneralProperties(ObjectKey id, JObject obj, bool isRoot)
        {
            ObservableCollection<PropertyItem> result = new ObservableCollection<PropertyItem>();

            long eID = -1;

            foreach (var item in obj!.Properties())
            {
                try
                {
                    //var val = obj[key];
                    if (item.Name != null)
                    {
                        string name = item.Name;
                        string? value = null;

                        long linkExpressID = -1;

                        if ((item.Value is JValue && (item.Value as JValue)?.Value != null)
                            || (item.Value is JObject && item.Value.HasValues)
                            || item.Value is JArray)
                        {

                            switch (name)
                            {
                                case "expressID":
                                    name = "ID";
                                    eID = (long)((item.Value as JValue)?.Value ?? 0);
                                    value = "#" + eID.ToString();
                                    break;
                                case "type":
                                    name = "Class";
                                    if (!IfcCategories.IfcCategoryMapById.TryGetValue((long)((item.Value as JValue)!.Value!), out value))
                                    {
                                        value = (item.Value as JValue)!.Value!.ToString();
                                    }
                                    if (value != null && IfcCategories.IfcCategoryFormattedNames.ContainsKey(value))
                                    {
                                        value = IfcCategories.IfcCategoryFormattedNames[value];
                                    }
                                    break;
                                case "HasPropertySets":
                                    if (!(item.Value is JArray)) break;

                                    JArray? propertySets = item.Value as JArray;
                                    if (propertySets == null) break;

                                    if (isRoot)
                                    {
                                        foreach (JObject pSetPair in propertySets)
                                        {
                                            JToken? pSetValue = null;
                                            if (pSetPair.TryGetValue("value", out pSetValue) && pSetValue is JValue)
                                            {
                                                var objValue = (pSetValue as JValue)?.Value;
                                                if (objValue is long)
                                                {
                                                    long pSetExpressID = (long)objValue;
                                                    string pSetString = await _jsModule!.InvokeAsync<string>("getPropertiesString", [id.IfcModelKey, pSetExpressID]);
                                                    if (!pSetString.StartsWith("["))
                                                    {
                                                        pSetString = "[" + pSetString + "]";
                                                    }
                                                    foreach (JToken pSet in JArray.Parse(pSetString ?? string.Empty))
                                                    {
                                                        //Teoricamente c'è solo un elemento nell'array
                                                        await ProcessPropertySet(id.IfcModelKey, pSet);
                                                    }

                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (JToken pSetPair in propertySets)
                                        {
                                            var pSetId = pSetPair["value"];
                                            if (pSetId != null)
                                            {
                                                string pSetString = await _jsModule!.InvokeAsync<string>("getPropertiesString", [id.IfcModelKey, (long)pSetId]);
                                                await ProcessPropertySet(id.IfcModelKey, JToken.Parse(pSetString ?? string.Empty), eID);
                                            }
                                        }

                                    }

                                    break;
                                default:
                                    //TODO Cosa fare se è un array?
                                    if (item.Value is JArray || !item.HasValues) break;

                                    try
                                    {
                                        long type = -1;

                                        if (item.Value is JValue)
                                        {
                                            value = (item.Value as JValue)?.Value?.ToString();
                                        }
                                        else
                                        {
                                            value = item.Value["value"]?.ToString() ?? NullPlaceHolder;
                                            type = Int64.Parse(item.Value["type"]?.ToString() ?? "-1");

                                        }

                                        if (type == 5 && Int64.TryParse(value, out linkExpressID))
                                        {
                                            value = "#" + value;
                                            bool hasProperties = await _jsModule.InvokeAsync<bool>("hasGeneralProperties", [id.IfcModelKey, linkExpressID]);
                                            if (!hasProperties) linkExpressID = -1;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message + " ### " + item.ToString());
                                    }
                                    break;
                            }

                        }
                        result.Add(new PropertyItem(name, value ?? NullPlaceHolder, linkExpressID >= 0 ? linkExpressID : null));
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " ### " + item.ToString());
                }

            }

            if (eID >= 0)
            {
                //long? containingStructure = SpatialStructure.FirstOrDefault(s => s.ID == eID)?.Parent?.ID;
                ModelNavTreeNode? containingStructure = FindSpatialStructure(SpatialStructureTree, new ObjectKey(id.IfcModelKey, eID));
                long? containingStructureId = containingStructure?.Parent?.Parent?.ID.ExpressID;
                if (containingStructureId.HasValue && containingStructureId.Value > 0)
                {
                    result.Add(new PropertyItem("Containing structure", "#" + containingStructureId.ToString(), containingStructureId));
                }
                else
                {
                    result.Add(new PropertyItem("Containing structure", NullPlaceHolder));
                }
            }

            return result;

        }

        private ModelNavTreeNode? FindSpatialStructure(List<ModelNavTreeNode> structure, ObjectKey id)
        {
            ModelNavTreeNode? result = structure.FirstOrDefault(s => s.ID.Equals(id));
            if (result == null)
            {
                foreach (ModelNavTreeNode node in structure)
                {
                    if (node.HasChildren && node.Children.Count > 0)
                    {
                        result = FindSpatialStructure(node.Children, id);
                        if (result != null) break;
                    }
                }
            }
            return result;

        }

        private async Task ProcessPropertySet(string ifcModelKey, JToken pSet, long? parentKey = null)
        {

            if (pSet != null)
            {
                try
                {
                    //long eID = pSet.Value<long>("expressID");
                    var nameToken = pSet["Name"];
                    string? pSetName = "unnamed";
                    if (nameToken != null && nameToken.HasValues)
                    {
                        string? name = nameToken.Value<string?>("value");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            pSetName = name;
                        }

                    }
                    JArray? pSetProperties = pSet.Value<JArray?>("HasProperties");

                    if (pSetProperties != null)
                    {
                        ObservableCollection<PropertyItem> collection = new ObservableCollection<PropertyItem>();

                        foreach (JToken pPropertyPair in pSetProperties)
                        {
                            JToken? property = pPropertyPair["value"];
                            if (property != null)
                            {
                                JToken? token = null;
                                if (property is JObject)
                                {
                                    ProcessProperty((property as JObject)!, collection, parentKey);
                                }
                                else if (property is JValue)
                                {
                                    var objValue = (property as JValue)?.Value;
                                    if (objValue is long)
                                    {
                                        string pString = await _jsModule!.InvokeAsync<string>("getPropertiesString", [ifcModelKey, (long)objValue]);
                                        JObject item = JObject.Parse(pString ?? string.Empty);
                                        string ifcType = Utilities.IfcCategories.IfcCategoryMapById[item.Value<long?>("type") ?? -1];
                                        //Console.WriteLine(ifcType);
                                        switch (ifcType)
                                        {
                                            //case "IFCMATERIALLIST":
                                            //    if (Materials.Count == 0)
                                            //    {
                                            //        Materials.Add(("Materiali", new ObservableCollection<PropertyItem>()));

                                            //        JArray? materialArray = item.Value<JArray>("Materials");
                                            //        if (materialArray != null)
                                            //        {
                                            //            foreach (var material in materialArray)
                                            //            {
                                            //                var id = material.Value<long?>("value");
                                            //                var name = await _jsModule.InvokeAsync<string>("getName", id);

                                            //                Materials[0].Value.Add(new PropertyItem("Materiale", name));
                                            //            }
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        Console.WriteLine("Unexpected: Item with multiple material sets.");
                                            //    }
                                            //    NotifyPropertyChanged(PropertyNames.Materials);
                                            //    break;

                                            //case "IFCMATERIALLAYERSETUSAGE":
                                            //    if (Materials.Count == 0)
                                            //    {
                                            //        Materials.Add(("Insieme di livelli", new ObservableCollection<PropertyItem>()));
                                            //        long? layerSetEID = item["ForLayerSet"]?.Value<long?>("value");
                                            //        string? direction = item["LayerSetDirection"]?.Value<string?>("value");
                                            //        Materials[0].Value.Add(new PropertyItem("LayerSetDirection", direction));
                                            //        string? sense = item["DirectionSense"]?.Value<string?>("value");
                                            //        Materials[0].Value.Add(new PropertyItem("DirectionSense", sense));
                                            //        double? offset = item["OffsetFromReferenceLine"]?.Value<double?>("value");
                                            //        Materials[0].Value.Add(new PropertyItem("OffsetFromReferenceLine", offset.ToString()));
                                            //        await _jsModule!.InvokeVoidAsync("getMaterials", layerSetEID);
                                            //    }
                                            //    else
                                            //    {
                                            //        Console.WriteLine("Unexpected: Item with multiple material sets.");
                                            //    }
                                            //    break;

                                            default:
                                                ProcessProperty(item, collection, parentKey);
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        if (collection.Count > 0) Properties.Add((pSetName, collection));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " ### " + pSet.ToString());
                }

            }

        }

        private void ProcessProperty(JObject property, ObservableCollection<PropertyItem> collection, long? parentKey)
        {
            string name = property["Name"]?.Value<string?>("value");
            string? value = property["NominalValue"]?.Type != JTokenType.Null ? property["NominalValue"]?.Value<string?>("value") : "NULL";
            if (!string.IsNullOrEmpty(name))
            {
                collection.Add(new PropertyItem(name, value, parentKey));
            }
        }

        private async Task ProcessQuantities(string ifcModelKey, JToken quantities)
        {
            if (quantities != null)
            {
                long eID = quantities.Value<long>("expressID");
                var nameToken = quantities["Name"];
                string? quantitiesName = "unnamed";
                if (nameToken != null && nameToken.HasValues)
                {
                    string? name = nameToken.Value<string?>("value");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        quantitiesName = name;
                    }

                }
                JArray? qArray = quantities.Value<JArray?>("Quantities");
                string ifcType = Utilities.IfcCategories.IfcCategoryMapById[quantities.Value<long?>("type") ?? -1];

                if (qArray != null)
                {
                    foreach (JToken quantityPair in qArray)
                    {
                        eID = quantityPair.Value<long>("value");
                        var qStr = await _jsModule!.InvokeAsync<string>("getPropertiesString", [ifcModelKey, eID]);
                        var quantity = JToken.Parse(qStr);

                        if (quantity != null)
                        {
                            //foreach (var quantity in qObj) //Dovrebbe esssere sempre 1 solo elemento
                            //{
                            ifcType = Utilities.IfcCategories.IfcCategoryMapById[quantity["type"]?.Value<long>() ?? -1];
                            nameToken = quantity["Name"];
                            quantitiesName = "unnamed";
                            if (nameToken != null && nameToken.HasValues)
                            {
                                string? name = nameToken.Value<string?>("value");
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    quantitiesName = name;
                                }

                            }
                            var q = new QuantityItem(quantitiesName, ifcType);
                            try { q.Description = quantity["Description"]?.Value<string>("value"); }
                            catch { }
                            try { q.Description = quantity["Unit"]?.Value<string>("value"); }
                            catch { }
                            string? valueKey = null;
                            switch (ifcType)
                            {
                                case "IFCQUANTITYAREA":
                                    valueKey = "AreaValue";
                                    q.Unit = q.Unit ?? (_baseUnits.ContainsKey((ifcModelKey, "AREAUNIT")) ? _baseUnits[(ifcModelKey, "AREAUNIT")] : null);
                                    break;
                                case "IFCQUANTITYCOUNT":
                                    valueKey = "CountValue";
                                    q.Unit = null;
                                    break;
                                case "IFCQUANTITYLENGTH":
                                    valueKey = "LengthValue";
                                    q.Unit = q.Unit ?? (_baseUnits.ContainsKey((ifcModelKey, "LENGTHUNIT")) ? _baseUnits[(ifcModelKey, "LENGTHUNIT")] : null);
                                    break;
                                case "IFCQUANTITYSET":
                                    valueKey = "SetValue";
                                    q.Unit = null;
                                    break;
                                case "IFCQUANTITYTIME":
                                    valueKey = "TimeValue";
                                    q.Unit = q.Unit ?? (_baseUnits.ContainsKey((ifcModelKey, "TIMEUNIT")) ? _baseUnits[(ifcModelKey, "TIMEUNIT")] : null);
                                    break;
                                case "IFCQUANTITYVOLUME":
                                    valueKey = "VolumeValue";
                                    q.Unit = q.Unit ?? (_baseUnits.ContainsKey((ifcModelKey, "VOLUMEUNIT")) ? _baseUnits[(ifcModelKey, "VOLUMEUNIT")] : null);
                                    break;
                                case "IFCQUANTITYWEIGHT":
                                    valueKey = "WeightValue";
                                    q.Unit = q.Unit ?? (_baseUnits.ContainsKey((ifcModelKey, "MASSUNIT")) ? _baseUnits[(ifcModelKey, "MASSUNIT")] : null);
                                    break;
                                case "IFCMONETARYMEASURE":
                                    valueKey = "MoneyValue";
                                    q.Unit = q.Unit ?? (_baseUnits.ContainsKey((ifcModelKey, "CURRENCY")) ? _baseUnits[(ifcModelKey, "CURRENCY")] : null);
                                    break;
                                default:
                                    break;

                            }

                            q.Value = string.IsNullOrWhiteSpace(valueKey) ? null : quantity[valueKey]?.Value<string>("value");
                            if (q.Value == null) q.Value = NullPlaceHolder;

                            //if (ifcType item.Name == "Description")
                            //                                        {
                            //        if (item.Value?.Type == JTokenType.Object)
                            //        {
                            //            string? value = item.Value.Value<string?>("value"); ;
                            //            {
                            //                Quantities.Add(new PropertyItem(name, item.Name, value, null));
                            //            }
                            //        }
                            //        else
                            //        {
                            //            string? value = item.Value?.ToString();
                            //            {
                            //                Quantities.Add(new PropertyItem(name, item.Name, value, null));
                            //            }
                            //        }
                            //    }

                            //}
                            Quantities.First().Value.Add(q);
                            //}

                        }
                    }
                }
            }
        }

        [JSInvokable("UpdateMaterials")]
        public async void UpdateMaterials(long layerSetExpressID, string layerSetName, MaterialLayerItem[] layers)
        {
            Materials[0] = (layerSetName, Materials[0].Value);
            foreach (var layer in layers)
            {
                foreach (var prop in layer.ChildrenProperties)
                {
                    prop.Value ??= NullPlaceHolder;
                }
                Materials.Add((layer.Name, new ObservableCollection<PropertyItem>(layer.ChildrenProperties)));
            }

            NotifyPropertyChanged(PropertyNames.Materials);

        }


        public ModelNavTreeNode? SearchTree(List<ModelNavTreeNode> tree, long expressId)
        {
            ModelNavTreeNode? result = null;

            foreach (var item in tree)
            {
                result = (item.ID.Equals(expressId)) ? item : (item.HasChildren ? SearchTree(item.Children, expressId) : null);
                if (result != null) break;
            }

            return result;
        }

        #endregion

        #region BoQ sync

        public async Task HighlightByGlobalIDs(List<GlobalIdPair> globalIDs)
        {
            List<ModelNavTreeNode> newSelectedNodes = new List<ModelNavTreeNode>();

            Dictionary<ObjectKey, ModelNavTreeNode>[] dicts = { SpatialItemsFlatList, ClassesFlatList, TypesFlatList, GroupsFlatList };
            foreach (var dict in dicts)
            {
                foreach (var item in dict.Values)
                {
                    if (item.GlobalId != null && item.ID.IfcModelKey != null && ModelGlobalIds.ContainsKey(item.ID.IfcModelKey))
                    {
                        var modelGlobalId = ModelGlobalIds[item.ID.IfcModelKey];
                        var pair = new GlobalIdPair(modelGlobalId, item.GlobalId);
                        if (globalIDs.Contains(pair))
                        {
                            //await Highlight((item, false));
                            newSelectedNodes.Add(item);
                        }
                    }
                }
            }

            if (newSelectedNodes.Count > 0)
            {
                await UpdateSelection(newSelectedNodes, true);
            }

            NotifyPropertyChanged(PropertyNames.HighlightedFromBoQ);
            
            //await _jsModule!.InvokeVoidAsync("highlightManyByGlobalIDs", globalIDs);
                        
            var geomObjects = newSelectedNodes.Where(x => x.IsFragment).Select(x => x.ID).ToArray();
            await _jsModule!.InvokeVoidAsync("highlightMany", [geomObjects, true, true]);
        }

        public async Task<List<GlobalIdPair>> GetSelectedGlobalIds()
        {
            var result = new List<GlobalIdPair>();

            foreach (var node in SelectedNodes)
            {
                if (node.ID.IfcModelKey != null)
                {
                    string modelGlobalId = await _jsModule!.InvokeAsync<string>("getModelGlobalId", node.ID.IfcModelKey);
                    string objectGlobalId = await _jsModule!.InvokeAsync<string>("getObjectGlobalId", node.ID.IfcModelKey, node.ID.ExpressID);
                    var pair = new GlobalIdPair(modelGlobalId, objectGlobalId);
                    if (!result.Contains(pair))
                    {
                        result.Add(pair);
                    }
                }
            }

            return result;

        }

        #endregion


        #region IFC to frag/json conversion

        //[JSInvokable("SetCompleteProperties")]
        public async Task SetCompleteProperties(int length)
        {
            if (length > 0)
            {
                await _uploadMutex.WaitAsync();

                byte[]? properties = new byte[length];

                int offset = 0;

                while (offset < length)
                {
                    var tmp = await _jsModule!.InvokeAsync<string>("getPropertiesChunk", [offset, offset + Math.Min(_fileChunkSize, length - offset)]);

                    var decoded = System.Convert.FromBase64String(tmp);
                    decoded.CopyTo(properties, offset);
                    offset += decoded.Length;

                    tmp = "";
                    decoded = new byte[0];

                }

                GC.Collect();

                await _jsModule!.InvokeVoidAsync("clearPropertiesExport");

                await _apiClient.UploadAllegatoAsync(_uploadAllegatiUrl, _uploadingFileName + ".prop", "application/octet-stream", properties, _operaId, _uploadingFileParentGuid, JoinWebApiClient.CompressionOptions.AlreadyCompressed, true);

                properties = null;

                _uploadMutex.Release();

                GC.Collect();

            }
            else
            {

            }
        }

        //[JSInvokable("SetFragments")]
        public async Task SetFragments(int length)
        {
            if (length > 0)
            {
                await _uploadMutex.WaitAsync();

                byte[] fragments = new byte[length];

                int offset = 0;

                while (offset < length)
                {
                    var tmp = await _jsModule!.InvokeAsync<string>("getFragmentsChunk", [offset, offset + Math.Min(_fileChunkSize, length - offset)]);

                    var decoded = System.Convert.FromBase64String(tmp);
                    decoded.CopyTo(fragments, offset);
                    offset += decoded.Length;

                    tmp = "";
                    decoded = new byte[0];

                }

                GC.Collect();

                await _jsModule!.InvokeVoidAsync("clearFragmentsExport");

                await _apiClient.UploadAllegatoAsync(_uploadAllegatiUrl, _uploadingFileName + ".frag", "application/octet-stream", fragments, _operaId, _uploadingFileParentGuid, JoinWebApiClient.CompressionOptions.AlreadyCompressed, true);

                fragments = null;

                _uploadMutex.Release();

                GC.Collect();

            }
            else
            {

            }
        }

        #endregion

        #region Mouse events

        [JSInvokable("RightClick")]
        public async Task RightClick(double x, float y)
        {
            Console.WriteLine("x: " + x + "; y: " + y);
            ContextMenuVisible = true;
            ContextMenuCoordinates = (x, y);
            NotifyPropertyChanged(nameof(ContextMenuVisible));
        }

        #endregion

        #region Scene controls

        public async Task InvokeSimpleFunction(SimpleFunctionNames name, object?[]? parameters)
        {
            await _jsModule!.InvokeVoidAsync(name.ToString(), parameters);
        }

        public void LoadProperties(ObjectKey item, bool keepHistory)
        {
            //string subProp = await _jsModule!.InvokeAsync<string>("getPropertiesString", expressID);
            //await UpdateProperties(expressID, subProp, true);
            _jsModule!.InvokeVoidAsync("updateProperties", item.IfcModelKey, item.ExpressID, keepHistory);
        }

        [JSInvokable("UpdateClipping")]
        public async Task UpdateClipping(bool enabled, int mode)
        {
            ClippingState = enabled ? mode : null;
        }

        [JSInvokable("UpdateLengthMeasurement")]
        public async Task UpdateLengthMeasurement(int mode)
        {
            LengthMeasureState = mode;
        }

        [JSInvokable("UpdateAreaMeasurement")]
        public async Task UpdateAreaMeasurement(int mode)
        {
            AreaMeasureState = mode;
        }

        [JSInvokable("UpdateAngleMeasurement")]
        public async Task UpdateAngleMeasurement(int mode)
        {
            AngleMeasureState = mode;
        }

        [JSInvokable("UpdateCameraProjection")]
        public async Task UpdateCameraProjection(bool isPerspective)
        {
            CameraProjectionPerspective = isPerspective;
        }

        public void ClearMeasures()
        {
            _jsModule?.InvokeVoidAsync("deleteAllLengthMeasures");
            _jsModule?.InvokeVoidAsync("deleteAllAreaMeasures");
            _jsModule?.InvokeVoidAsync("deleteAllAngleMeasures");
            _jsModule?.InvokeVoidAsync("toggleLengthMeasure", false);
            _jsModule?.InvokeVoidAsync("toggleAreaMeasure", false);
            _jsModule?.InvokeVoidAsync("toggleAngleMeasure", false);
        }

        //[JSInvokable("UpdateIfcSpaces")]
        //public async Task UpdateIfcSpaces(bool shown)
        //{
        //    IfcSpacesShown = shown;
        //}

        #endregion

        #region Enums
        public enum SimpleFunctionNames
        {
            //setCameraPerspective,
            //setCameraOrthographic,
            toggleCameraProjection,
            resetZoom,
            focusSelection,
            isolate,
            isolateOrthogonal,
            isolateContainer,
            highlightOpaque,
            hide,
            show,
            flipHidden,
            setTransparent,
            setAllTransparent,
            removeTransparency,
            removeAllTransparency,
            flipTransparency,
            showAll,
            highlightVisible,
            propJson,
            toggleClipper,
            createClip,
            deleteClip,
            deleteAllClips,
            toggleLengthMeasure,
            deleteLengthMeasure,
            deleteAllLengthMeasures,
            toggleAngleMeasure,
            //deleteAngleMeasure,
            deleteAllAngleMeasures,
            toggleAreaMeasure,
            //deleteAreaMeasure,
            deleteAllAreaMeasures,

        }

        public enum ComplexFunctionNames
        {
            RestoreAll,
            HighlightClass,
            HighlightType,
            HighlightStorey,
            HighlightContainer,
            IsolateClass,
            IsolateType,
            IsolateStorey,
            HighlightOpaqueClass,
            HighlightOpaqueType,
            HighlightOpaqueStorey,
            SetTransparentClass,
            SetTransparentType,
            SetTransparentStorey,
            RemoveTransparencyClass,
            RemoveTransparencyType,
            RemoveTransparencyStorey,
            ShowClass,
            ShowType,
            ShowStorey,
            HideClass,
            HideType,
            HideStorey,
            ToggleIfcSpaces,

            ShowSelectedInAllTrees,
            ShowSelectedInSpatialStructureTree,
            ShowSelectedInClassesTree,
            ShowSelectedInTypesTree,
            ShowSelectedInGroupsTree,

        }

        public enum LoadResult
        {
            //NotLoaded = -2,
            NotLoaded = -1,
            Success = 0,
            NotFound = 2,
            Fail = 31,            
            

        }

        public static class PropertyNames
        {
            public const string Selection = "SelectedExpressIDs";
            //public const string SelectedExpressIdsCount = "SelectedExpressIdsCount";
            public const string ShownExpressIDs = "ShownExpressIDs";
            public const string SpatialStructure = "SpatialStructureTree";
            public const string ClassesTree = "ClasseTree";
            public const string TypesTree = "TypesTree";
            public const string GroupsTree = "GroupsTree";
            public const string Properties = "Properties";
            public const string Materials = "Materials";
            public const string HighlightedFromBoQ = "HighlightedFromBoQ";
        }

        //public enum NodeType
        //{
        //    Fragment,
        //    NoFragment,
        //    Grouping
        //}
        #endregion


        public async void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_jsModule != null && ModelLoadResult <= LoadResult.NotLoaded)
            {
                await _jsModule.InvokeVoidAsync("disposeViewer");
                await _jsModule.DisposeAsync();
                _jsModule = null;

                dotNetReference?.Dispose();
                dotNetReference = null;

                //GeneralInfo.CollectionChanged -= OnGeneralInfoChanged;
                //TypeInfo.CollectionChanged -= OnTypeInfoChanged;
                //Properties.CollectionChanged -= OnPropertiesChanged;
                //Quantities.CollectionChanged -= OnQuantitiesChanged;
                //Materials.CollectionChanged -= OnMaterialsChanged;

                //GeneralInfo.Clear();
                //TypeInfo.Clear();
                //Properties.Clear();
                //Quantities.Clear();
                //Materials.Clear();

            }

            GC.Collect();

        }
    }


}
