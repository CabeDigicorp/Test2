import { DotNet } from "@microsoft/dotnet-js-interop";
export { initViewer, toggleCameraProjection, loadModelFromIFC, loadModelFromFragments, initModels, extractUnits, extractSpatialStructure, extractTypes, extractGroups, updateProperties, getName, getMaterials, hasGeneralProperties, isFragment, getModelGlobalId, getObjectGlobalId, highlight, highlightMany, highlightManyByGlobalIDs, removeHighlight, removeHighlightMany, highlightNone, highlightVisible, highlightOpaque, highlightOpaqueMany, setTransparent, setTransparentMany, setAllTransparent, removeTransparency, removeTransparencyMany, removeAllTransparency, flipTransparency, getPropertiesString, getFragmentsInfo, getFragmentsChunk, clearFragmentsExport, getPropertiesInfo, getPropertiesChunk, clearPropertiesExport, decodeIfcType, disposeViewer, resetZoom, focusSelection, isolate, isolateMany, isolateOrthogonal, show, showMany, hide, hideMany, flipHidden, showAll, setIfcSpaces, toggleClipper, createClip, deleteClip, deleteAllClips, toggleLengthMeasure, deleteLengthMeasure, deleteAllLengthMeasures, toggleAreaMeasure, deleteAllAreaMeasures, toggleAngleMeasure, deleteAllAngleMeasures };
declare function initViewer(dotNetReference: DotNet.DotNetObject, container: string): Promise<void>;
declare function toggleCameraProjection(): Promise<void>;
declare function initModels(): Promise<void>;
declare function focusSelection(): void;
declare function updateProperties(modelKey: string, expressID: number, keepHistory?: boolean): Promise<void>;
declare function getName(modelKey: string, expressId: number): Promise<string>;
declare function getMaterials(modelKey: string, materialLayerSetExpressID: number): Promise<void>;
declare function hasGeneralProperties(modelKey: string, expressID: number): Promise<boolean>;
declare function getModelGlobalId(modelKey: string): Promise<string>;
declare function getObjectGlobalId(modelKey: string, expressID: number): Promise<string>;
declare function isFragment(modelKey: string, expressID: number): boolean;
declare function highlight(modelKey: string, expressID: number, removePrevious: boolean, includeHidden: boolean): Promise<void>;
declare function highlightMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>, removePrevious: boolean, includeHidden: boolean): Promise<void>;
declare function highlightManyByGlobalIDs(globalIDs: Array<{
    modelGlobalID: string;
    objectGlobalID: string;
}>): Promise<void>;
declare function removeHighlight(modelKey: string, expressID: number): Promise<void>;
declare function removeHighlightMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>): Promise<void>;
declare function highlightNone(): Promise<void>;
declare function highlightVisible(): Promise<void>;
declare function setTransparent(): Promise<void>;
declare function setTransparentMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>): Promise<void>;
declare function setAllTransparent(refresh?: boolean): Promise<void>;
declare function removeTransparency(): Promise<void>;
declare function removeTransparencyMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>): Promise<void>;
declare function removeAllTransparency(): Promise<void>;
declare function flipTransparency(): Promise<void>;
declare function highlightOpaque(resetHidden: boolean): Promise<void>;
declare function highlightOpaqueMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>, resetHidden: boolean): Promise<void>;
declare function getPropertiesString(modelKey: string, expressID: number): Promise<string>;
declare function loadModelFromIFC(data: Uint8Array, name: string): Promise<void>;
declare function loadModelFromFragments(fragments: Uint8Array, properties: Uint8Array, name: string): Promise<void>;
declare function extractUnits(): Promise<void>;
declare function extractSpatialStructure(): Promise<void>;
declare function extractTypes(): Promise<void>;
declare function extractGroups(): Promise<void>;
declare function getFragmentsInfo(groupIndex: number): Promise<number>;
declare function getFragmentsChunk(startIndex: number, endIndex: number): string;
declare function clearFragmentsExport(): Promise<void>;
declare function getPropertiesInfo(groupIndex: number): Promise<number>;
declare function getPropertiesChunk(startIndex: number, endIndex: number): string;
declare function clearPropertiesExport(): Promise<void>;
declare function decodeIfcType(key: number): string;
declare function resetZoom(): Promise<void>;
declare function isolate(resetHidden: boolean): Promise<void>;
declare function isolateMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>, resetHidden: boolean): Promise<void>;
declare function isolateOrthogonal(): Promise<void>;
declare function hide(): Promise<void>;
declare function hideMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>): Promise<void>;
declare function show(refresh?: boolean): Promise<void>;
declare function showMany(itemIDs: Array<{
    ifcModelKey: string;
    expressID: number;
}>, refresh?: boolean): Promise<void>;
declare function showAll(): Promise<void>;
declare function flipHidden(): Promise<void>;
declare function setIfcSpaces(show: boolean): Promise<void>;
declare function toggleClipper(enabled?: boolean | undefined): Promise<void>;
declare function createClip(): Promise<void>;
declare function deleteClip(): Promise<void>;
declare function deleteAllClips(): Promise<void>;
declare function toggleLengthMeasure(enabled?: boolean | undefined): Promise<void>;
declare function deleteLengthMeasure(): Promise<void>;
declare function deleteAllLengthMeasures(): Promise<void>;
declare function toggleAreaMeasure(enabled?: boolean | undefined): Promise<void>;
declare function deleteAllAreaMeasures(): Promise<void>;
declare function toggleAngleMeasure(enabled?: boolean | undefined): Promise<void>;
declare function deleteAllAngleMeasures(): Promise<void>;
declare function disposeViewer(): Promise<void>;
//# sourceMappingURL=thatOpen.d.mts.map