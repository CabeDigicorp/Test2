using JoinWebUI.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;
using System.ComponentModel;
using System.Data;
using ModelData.Model;

namespace JoinWebUI.Components
{
    public partial class IfcViewerWrapper
    {

        [Parameter] public string DivId { get; set; } = "3dViewer";
        [Parameter] public Guid ProgettoId { get; set; } = Guid.Empty;
        [Parameter] public Guid OperaId { get; set; } = Guid.Empty;
        [Parameter] public bool Visible { get; set; } = false;

        [Parameter] public EventCallback<List<GlobalIdPair>> OnShowInBoQ { get; set; }

        public List<GlobalIdPair>? Preselection = null;

        private bool ShowNullProperties { get; set; } = true;

        private SfContextMenu<MenuItem> contextMenuWithoutSelection;
        private SfContextMenu<MenuItem> contextMenuWithSelection;

        private SfSidebar _propertiesSidebar;
        private bool ShowPropertiesSidebar { get; set; } = false;

        private SfSidebar _indexSidebar;
        private bool ShowIndexSidebar { get; set; } = false;
        private void ToggleIndexSidebar()
        {
            ShowIndexSidebar = !ShowIndexSidebar;
        }


        private SfTab _treesTabs;
        private SfTab _propertiesTabs;
        //private SfSplitter _splitter;

        private SfDialog jsonDiag;
        private bool jsonDiagVisible;
        private string jsonDiagContent;

        private ModelNavTabItem _spatialTabItem;
        private ModelNavTabItem _elementsTabItem;
        private ModelNavTabItem _typesTabItem;
        private ModelNavTabItem _groupsTabItem;

        // private SfTreeGrid<SpatialRelation> _structureTree;

        private PropertiesTabHeader _propHeader;

        private PropertiesTabItem _generalTabItem;
        private PropertiesTabItem _typeTabItem;
        private PropertiesTabItem _propTabItem;
        private PropertiesTabItem _quantitiesTabItem;
        private PropertiesTabItem _materialsTabItem;

        private FragmentsHelper? _fragmentsHelper;

        private bool NothingSelected { get => (_fragmentsHelper?.SelectedNodes?.Count ?? 0) <= 0; }
        private bool SomethingSelected { get => (_fragmentsHelper?.SelectedNodes?.Count ?? 0) > 0; }
        private bool NoGeometriesSelected { get => NothingSelected || !(_fragmentsHelper?.GeometriesSelected ?? false); }

        private const string RESET_HIDDEN_SUFFIX = ":ResetHidden";
        private const string INCLUDE_HIDDEN_SUFFIX = ":IncludeHidden";

        private string RestoreMenuClass { get => _menuShown == MenuItems.RestoreMenu ? "e-toggled" : ""; }
        private string RestoreMenuIconCss { get => "e-icons icon-yellow" + (_menuShown == MenuItems.RestoreMenu ? "-rev" : "") + " icon-ripristina-tutto"; }
        private string ShowMenuClass { get => _menuShown == MenuItems.ShowMenu ? "e-toggled" : ""; }
        private string ShowMenuIconCss { get => "e-icons icon-yellow" + (_menuShown == MenuItems.ShowMenu ? "-rev" : "") + " icon-visualizza-elemento"; }
        private string TransparencyMenuClass { get => _menuShown == MenuItems.TransparencyMenu ? "e-toggled" : ""; }
        private string TransparencyMenuIconCss { get => "e-icons icon-yellow" + (_menuShown == MenuItems.TransparencyMenu ? "-rev" : "") + " icon-trasparenza-on"; }
        private string SpacesMenuClass { get => _fragmentsHelper.IfcSpacesShown ? "e-toggled" : ""; }
        private string SpacesMenuIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.IfcSpacesShown ? "-rev" : "") + " icon-visualizza-nascondi-ifcspace"; }
        //private string ProjectionMenuClass { get => _fragmentsHelper.CameraProjectionPerspective ? "e-toggled" : ""; }
        private string ProjectionMenuIconCss { get => "e-icons icon-yellow " + (_fragmentsHelper.CameraProjectionPerspective ? "icon-vista-assonometrica" : "icon-vista-prospettica"); }
        private string ClippingMenuClass { get => _menuShown == MenuItems.ClippingMenu ? "e-toggled" : ""; }
        private string ClippingMenuIconCss { get => "e-icons icon-yellow" + (_menuShown == MenuItems.ClippingMenu ? "-rev" : "") + " icon-piani-di-taglio"; }
        private string MeasureMenuClass { get => _menuShown == MenuItems.MeasureMenu ? "e-toggled" : ""; }
        private string MeasureMenuIconCss { get => "e-icons icon-yellow" + (_menuShown == MenuItems.MeasureMenu ? "-rev" : "") + " icon-misura"; }

        private string ToggleClipperClass { get => _fragmentsHelper.ClippingState != null ? "e-toggled" : ""; }
        private string ToggleClipperIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.ClippingState != null ? "-rev" : "") + " icon-piani-mostra-nascondi"; }
        private string CreateClipClass { get => _fragmentsHelper.ClippingState != null && _fragmentsHelper.ClippingState > 0 ? "e-toggled" : ""; }
        private string DeleteClipClass { get => _fragmentsHelper.ClippingState != null && _fragmentsHelper.ClippingState < 0 ? "e-toggled" : ""; }
        private string CreateClipIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.ClippingState != null && _fragmentsHelper.ClippingState > 0 ? "-rev" : "") + " icon-piani-di-taglio"; }
        private string DeleteClipIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.ClippingState != null && _fragmentsHelper.ClippingState < 0 ? "-rev" : "") + " icon-cancella-piani-di-taglio"; }

        private string toggleLengthMeasureClass { get => _fragmentsHelper.LengthMeasureState > 0 ? "e-toggled" : ""; }
        private string DeleteLengthMeasureClass { get => _fragmentsHelper.LengthMeasureState < 0 ? "e-toggled" : ""; }
        private string ToggleLengthMeasureIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.LengthMeasureState > 0 ? "-rev" : "") + " icon-misura"; }
        private string DeleteLengthMeasureIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.LengthMeasureState < 0 ? "-rev" : "") + " icon-elimina-lunghezza"; }
        //private string DeleteAllLengthMeasuresIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.LengthMeasureState < 0 ? "-rev" : "") + " icon-elimina-lunghezze"; }
        private string ToggleAngleMeasureClass { get => _fragmentsHelper.AngleMeasureState > 0 ? "e-toggled" : ""; }
        private string DeleteAngleMeasureClass { get => _fragmentsHelper.AngleMeasureState < 0 ? "e-toggled" : ""; }
        private string ToggleAngleMeasureIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.AngleMeasureState > 0 ? "-rev" : "") + " icon-misura_angolo"; }
        private string DeleteAngleMeasureIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.AngleMeasureState < 0 ? "-rev" : "") + " icon-elimina-misura_angolo"; }
        //private string DeleteAllAngleMeasuresIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.AngleMeasureState < 0 ? "-rev" : "") + " icon-elimina-angolo"; }
        private string ToggleAreaMeasureClass { get => _fragmentsHelper.AreaMeasureState > 0 ? "e-toggled" : ""; }
        private string DeleteAreaMeasureClass { get => _fragmentsHelper.AreaMeasureState < 0 ? "e-toggled" : ""; }
        private string ToggleAreaMeasureIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.AreaMeasureState > 0 ? "-rev" : "") + " icon-misura-area"; }
        private string DeleteAreaMeasureIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.AreaMeasureState < 0 ? "-rev" : "") + " icon-elimina-area"; }
        //private string DeleteAllAreaMeasuresIconCss { get => "e-icons icon-yellow" + (_fragmentsHelper.AreaMeasureState < 0 ? "-rev" : "") + " icon-elimina-aree"; }

        private bool ContextMenuActive { get => (_fragmentsHelper.AreaMeasureState < 1); }

        public enum MenuItems
        {
            RestoreMenu,
            ShowMenu,
            TransparencyMenu,
            ClippingMenu,
            MeasureMenu
        }

        private MenuItems? _menuShown = null;

        // private bool DisableHighlightButton { get => _fragmentsHelper == null || !_fragmentsHelper.Highlightable; }

        private async void OnFragmentsHelperPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FragmentsHelper.ActiveSpinner):
                case nameof(FragmentsHelper.ClippingState):
                case nameof(FragmentsHelper.LengthMeasureState):
                case nameof(FragmentsHelper.AreaMeasureState):
                case nameof(FragmentsHelper.CameraProjectionPerspective):
                case nameof(FragmentsHelper.IfcSpacesShown):
                    StateHasChanged();
                    break;
                case FragmentsHelper.PropertyNames.Selection:
                    _spatialTabItem.ForceUpdate();
                    _elementsTabItem?.ForceUpdate();
                    _typesTabItem?.ForceUpdate();
                    _groupsTabItem?.ForceUpdate();
                    StateHasChanged();
                    break;
                case FragmentsHelper.PropertyNames.SpatialStructure:
                    _spatialTabItem?.ForceUpdate();
                    break;
                case FragmentsHelper.PropertyNames.ClassesTree:
                    _elementsTabItem?.ForceUpdate();
                    break;
                case FragmentsHelper.PropertyNames.TypesTree:
                    _typesTabItem?.ForceUpdate();
                    break;
                case FragmentsHelper.PropertyNames.GroupsTree:
                    _groupsTabItem?.ForceUpdate();
                    break;
                case FragmentsHelper.PropertyNames.Properties:
                    _generalTabItem?.ForceUpdate();
                    _typeTabItem?.ForceUpdate();
                    _propTabItem?.ForceUpdate();
                    _quantitiesTabItem?.ForceUpdate();
                    _propHeader.ForceUpdate();
                    break;
                case FragmentsHelper.PropertyNames.Materials:
                    _materialsTabItem?.ForceUpdate();
                    _propHeader.ForceUpdate();
                    break;
                case nameof(FragmentsHelper.ContextMenuVisible):
                    await OnContextMenu();
                    break;
                default:
                    break;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _fragmentsHelper = new FragmentsHelper(_configuration["JoinApiSettings:ServerBaseUrl"] + "allegati/upload", _jsRuntime, _apiClient);
            _fragmentsHelper.PropertyChanged += OnFragmentsHelperPropertyChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await _jsRuntime.InvokeVoidAsync("configResizers");
                await LoadModel();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            base.OnParametersSet();
            await LoadModel();
        }

        public async Task LoadModel()
        {
            if (Visible && _fragmentsHelper != null && OperaId != Guid.Empty)
            {
                if (_fragmentsHelper.ModelLoadResult == FragmentsHelper.LoadResult.NotLoaded)
                {
                    await _fragmentsHelper.LoadModels(DivId, OperaId, ProgettoId, Preselection);
                    Preselection = null;
                    StateHasChanged();
                }
            }
        }

        //private async Task OnRightClick(MouseEventArgs e)
        //{
        //    await (SomethingSelected ? contextMenuWithSelection : contextMenuWithoutSelection).OpenAsync(e.ClientX, e.ClientY, true);

        //}

        public async Task OnContextMenu()
        {
            if (_fragmentsHelper != null && _fragmentsHelper.ContextMenuVisible)
            {
                await (SomethingSelected ? contextMenuWithSelection : contextMenuWithoutSelection).OpenAsync(_fragmentsHelper.ContextMenuCoordinates.X, _fragmentsHelper.ContextMenuCoordinates.Y, true);
            }

        }

        private async Task HighlightCurrent()
        {
            if (_fragmentsHelper != null)
            {
                await _fragmentsHelper.HighlightCurrent();
            }

        }

        private void CollapseAll()
        {
            switch (_treesTabs.SelectedItem)
            {
                case 0:
                    _spatialTabItem?.CollapseAll();
                    _spatialTabItem?.ForceUpdate();
                    break;
                case 1:
                    _elementsTabItem?.CollapseAll();
                    _elementsTabItem?.ForceUpdate();
                    break;
                case 2:
                    _typesTabItem?.CollapseAll();
                    _typesTabItem?.ForceUpdate();
                    break;
                case 3:
                    _groupsTabItem?.CollapseAll();
                    _groupsTabItem?.ForceUpdate();
                    break;
                default:
                    break;
            }
        }

        private void ExpandAll()
        {
            switch (_treesTabs.SelectedItem)
            {
                case 0:
                    _spatialTabItem?.ExpandAll();
                    _spatialTabItem?.ForceUpdate();
                    break;
                case 1:
                    _elementsTabItem?.ExpandAll();
                    _elementsTabItem?.ForceUpdate();
                    break;
                case 2:
                    _typesTabItem?.ExpandAll();
                    _typesTabItem?.ForceUpdate();
                    break;
                case 3:
                    _groupsTabItem?.ExpandAll();
                    _groupsTabItem?.ForceUpdate();
                    break;
                default:
                    break;
            }
        }

        private async Task HighlightSubTrees()
        {
            if (_fragmentsHelper != null)
            {
                switch (_treesTabs.SelectedItem)
                {
                    case 0:
                        await _fragmentsHelper.CheckSubTrees(_fragmentsHelper.SpatialStructureTree);
                        _spatialTabItem?.ForceUpdate();
                        break;
                    case 1:
                        await _fragmentsHelper.CheckSubTrees(_fragmentsHelper.ClassesTree);
                        _elementsTabItem?.ForceUpdate();
                        break;
                    case 2:
                        await _fragmentsHelper.CheckSubTrees(_fragmentsHelper.TypesTree);
                        _typesTabItem?.ForceUpdate();
                        break;
                    case 3:
                        await _fragmentsHelper.CheckSubTrees(_fragmentsHelper.GroupsTree);
                        _groupsTabItem?.ForceUpdate();
                        break;
                    default:
                        break;
                }
            }
        }

        
        private async Task OnMenuItemSelected(MenuEventArgs<MenuItem> e)
        {
            await OnMenuSelected(e.Item.Id);
        }

        private async Task OnMenuSelected(string itemId)
        {
            string[] parsedId = itemId.Split(':');
            if (parsedId.Length > 0)
            {
                object?[]? parameters = null;

                MenuItems menu;
                if (Enum.TryParse<MenuItems>(parsedId[0], true, out menu))
                {
                    var old =_menuShown;
                    _menuShown = (_menuShown == menu) ? null : menu;
                    if (_menuShown != MenuItems.MeasureMenu && old == MenuItems.MeasureMenu)
                    {
                        _fragmentsHelper?.ClearMeasures();
                    }
                    if (_menuShown == MenuItems.ClippingMenu && old != MenuItems.ClippingMenu && _fragmentsHelper != null)
                    {
                        await _fragmentsHelper.InvokeSimpleFunction(FragmentsHelper.SimpleFunctionNames.toggleClipper, [true]);
                    }

                    return;
                }

                if (parsedId[0] == "ShowInBoQ")
                {
                    await ShowInBoQ();
                    return;
                }
                     
                FragmentsHelper.SimpleFunctionNames sfn;
                if (Enum.TryParse<FragmentsHelper.SimpleFunctionNames>(parsedId[0], true, out sfn))
                {
                    if (_fragmentsHelper != null)
                    {
                        if (sfn == FragmentsHelper.SimpleFunctionNames.propJson)
                        {
                            jsonDiagContent = _fragmentsHelper.propJson;
                            jsonDiagVisible = true;
                        }

                        bool resetHidden = parsedId.Length > 1 && ":" + parsedId.Last() == RESET_HIDDEN_SUFFIX;
                        if (resetHidden)
                        {
                            parameters = [resetHidden];
                        }

                        await _fragmentsHelper.InvokeSimpleFunction(sfn, parameters);

                        if (sfn == FragmentsHelper.SimpleFunctionNames.highlightVisible ||
                            //sfn == FragmentsHelper.SimpleFunctionNames.toggleIfcSpaces ||
                            sfn == FragmentsHelper.SimpleFunctionNames.toggleCameraProjection)
                        {
                            _menuShown = null;
                        }

                    }
                    return;
                }

                FragmentsHelper.ComplexFunctionNames cfn;
                if (Enum.TryParse<FragmentsHelper.ComplexFunctionNames>(parsedId[0], true, out cfn))
                {
                    System.Reflection.MethodInfo? info = _fragmentsHelper.GetType().GetMethod(parsedId[0]);
                    if (info != null)
                    {
                        if (info.GetParameters().Any(p => (p.ParameterType == typeof(Boolean) && p.Name == "resetHidden")))
                        {
                            parameters = [(parsedId.Length > 1 && ":" + parsedId.Last() == RESET_HIDDEN_SUFFIX)];
                        }
                        else if (info.GetParameters().Any(p => (p.ParameterType == typeof(Boolean) && p.Name == "includeHidden")))
                        {
                            parameters = [(parsedId.Length > 1 && ":" + parsedId.Last() == INCLUDE_HIDDEN_SUFFIX)];
                        }
                        info.Invoke(_fragmentsHelper, parameters);
                        
                        switch (cfn)
                        {
                            case FragmentsHelper.ComplexFunctionNames.ShowSelectedInSpatialStructureTree:
                                await _treesTabs.SelectAsync(0);
                                break;
                            case FragmentsHelper.ComplexFunctionNames.ShowSelectedInClassesTree:
                                await _treesTabs.SelectAsync(1);
                                break;
                            case FragmentsHelper.ComplexFunctionNames.ShowSelectedInTypesTree:
                                await _treesTabs.SelectAsync(2);
                                break;
                            case FragmentsHelper.ComplexFunctionNames.ShowSelectedInGroupsTree:
                                await _treesTabs.SelectAsync(3);
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
        }

        public async Task HighlightByGlobalIDs(List<GlobalIdPair> globalIDs)
        {
            if (_fragmentsHelper != null && _fragmentsHelper.ModelLoadResult == FragmentsHelper.LoadResult.Success)
            {
                await _fragmentsHelper.HighlightByGlobalIDs(globalIDs);
            }
            else
            {
                Preselection = globalIDs;
            }
        }

        public async Task ShowInBoQ()
        {
            if (_fragmentsHelper != null && OnShowInBoQ.HasDelegate)
            {
                var globalIDs = await _fragmentsHelper.GetSelectedGlobalIds();

                await OnShowInBoQ.InvokeAsync(globalIDs);

            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_fragmentsHelper != null)
            {
                _fragmentsHelper.PropertyChanged -= OnFragmentsHelperPropertyChanged;
                _fragmentsHelper.Dispose();
                _fragmentsHelper = null;
            }

            GC.Collect();

        }


    }
}