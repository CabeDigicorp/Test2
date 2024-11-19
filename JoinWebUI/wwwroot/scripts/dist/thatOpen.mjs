//"build": "npm run build:esm",
//    "build:lint": "eslint -c .eslintrc.json --ext .ts ./src",
//        "build:esm": "node node_modules/typescript/bin/tsc --project ./tsconfig.json",
import * as ByteBase64 from "byte-base64"; //../lib/byte-base64/lib.es6.js";
import { gzip, ungzip } from "pako"; //"../lib/pako/dist/pako.esm.mjs";
//import * as THREE from "../lib/three"
import * as THREE from "three";
//import * as WEBIFC from "../lib/web-ifc/web-ifc-api.js";
//import * as TO_Frag from "../lib/@thatopen/fragments";
//import * as TO_Core from "../lib/@thatopen/components";
//import * as TO_Front from "../lib/@thatopen/components-front";
import * as WEBIFC from "web-ifc";
import * as TO_Core from "@thatopen/components";
import * as TO_Front from "@thatopen/components-front";
import * as AUX_FRAGMENTS from "./auxFragments/auxFragments.js";
export { initViewer, 
//setCameraPerspective,
//setCameraOrthographic,
toggleCameraProjection, loadModelFromIFC, loadModelFromFragments, initModels, extractUnits, extractSpatialStructure, extractTypes, extractGroups, updateProperties, getName, getMaterials, hasGeneralProperties, isFragment, getModelGlobalId, getObjectGlobalId, highlight, highlightMany, highlightManyByGlobalIDs, removeHighlight, removeHighlightMany, highlightNone, highlightVisible, highlightOpaque, highlightOpaqueMany, setTransparent, setTransparentMany, setAllTransparent, removeTransparency, removeTransparencyMany, removeAllTransparency, flipTransparency, getPropertiesString, getFragmentsInfo, getFragmentsChunk, clearFragmentsExport, getPropertiesInfo, getPropertiesChunk, clearPropertiesExport, decodeIfcType, disposeViewer, resetZoom, focusSelection, 
//refreshView,
isolate, isolateMany, isolateOrthogonal, show, showMany, hide, hideMany, flipHidden, showAll, setIfcSpaces, toggleClipper, createClip, deleteClip, deleteAllClips, toggleLengthMeasure, deleteLengthMeasure, deleteAllLengthMeasures, toggleAreaMeasure, 
//deleteAreaMeasure,
deleteAllAreaMeasures, toggleAngleMeasure, 
//deleteAngleMeasure,
deleteAllAngleMeasures };
let dotNetObj;
let viewerContainer;
let fragmentsManager;
let viewer;
let worlds;
let world;
let culler;
let highlighter;
let outliner;
let hider;
let clipper;
let ifcLoader;
let indexer;
let raycasters;
let raycaster;
let cullers;
let intersectionObserver;
let resizeObserver;
let ifcFileNames = [];
let highlightedFragmentsMap;
let highlightedObjectsCounter;
//let highlightedNoFragmentList: Set<{ ifcModelKey: string, expressID: number }>
//let highlightedModel: TO_Frag.FragmentsGroup | null;
let auxFragments;
let transparentFragmentsMap;
let allFragmentsMap;
let itemModelFragmentsMap;
let modelFragmentItemsMap;
let modelItemsInFragmentMap;
let fragmentModelMap;
let ifcSpacesFragmentsMap;
let hideIfcSpaces = true;
let shownFragmentsMap;
let shownFragmentsFilteredMap;
//let boundingBoxer: TO_Core.BoundingBoxer;
let camTargetMesh;
let lengthMeasurement;
let areaMeasurement;
let angleMeasurement;
let ctrlPressed = false;
let shiftPressed = false;
let mouseDownPosition;
const mouseMoveThreshold = 5;
//let mouseDown: boolean = false;
//let setRotCenter: boolean = false;
//let showRotCenter: boolean = false;
//let pointerPos: THREE.Vector2;
//let raycasterRes: any;
let disableClick = false;
//let disableContextMenu = false;
const transparentOpacity = 0.3;
//let cameraRotationPoint: null | THREE.Vector3 = new THREE.Vector3();
//const highlightMaterial: THREE.MeshBasicMaterial = new THREE.MeshBasicMaterial({
//    color: '#F3A401',
//    depthTest: false,
//    opacity: 0.8,
//    transparent: false,
//});
async function initViewer(dotNetReference, container) {
    dotNetObj = dotNetReference;
    const host = document.querySelector("#" + container);
    const shadow = host.attachShadow({ mode: "open" });
    const div = document.createElement("div");
    div.id = "thatOpenViewerDiv";
    div.style.width = '100%';
    div.style.height = '100%';
    div.style.backgroundColor = 'darkblue';
    div.style.zIndex = "30";
    const options = {
        get capture() {
            return true;
        }
    };
    shadow.appendChild(div);
    var cssId = 'IfcViewerWrapper.razor.css'; // you could encode the css path itself to generate id..
    if (!document.getElementById(cssId)) {
        var link = document.createElement('link');
        link.id = 'icons.css';
        link.rel = 'stylesheet';
        link.type = 'text/css';
        link.href = '../../css/icons.css';
        link.media = 'all';
        shadow.appendChild(link);
    }
    viewer = new TO_Core.Components();
    const worlds = viewer.get(TO_Core.Worlds);
    world = worlds.create();
    world.scene = new TO_Core.SimpleScene(viewer);
    viewerContainer = shadow.getElementById("thatOpenViewerDiv");
    viewerContainer.style.zIndex = "35";
    world.renderer = new TO_Front.PostproductionRenderer(viewer, viewerContainer);
    world.camera = new TO_Core.OrthoPerspectiveCamera(viewer);
    world.camera.three.near = 0.001;
    world.camera.three.far = 100000;
    console.log(world.camera.three.position);
    world.renderer.postproduction.enabled = true;
    const material = new THREE.MeshLambertMaterial({ color: "#bcf124" });
    material.depthTest = false;
    const geometry = new THREE.SphereGeometry(1, 32, 32);
    camTargetMesh = new THREE.Mesh(geometry, material);
    camTargetMesh.renderOrder = Infinity;
    camTargetMesh.frustumCulled = false;
    camTargetMesh.position.set(0, 0, 0);
    world.scene.setup();
    //const grid = new TO_Core.SimpleGrid(viewer, new THREE.Color(0x666666))
    //grid.get().position.y = -3.1;
    //postproduction.customEffects.excludedMeshes.push(grid.get())
    ifcLoader = viewer.get(TO_Core.IfcLoader);
    const wasm = {
        path: "../lib/web-ifc/", //"https://unpkg.com/web-ifc@0.0.44/",
        absolute: false
    };
    ifcLoader.settings.wasm = wasm;
    //ifcLoader.settings.coordinate = false;
    const excludedCats = [
        WEBIFC.IFCTENDONANCHOR,
        WEBIFC.IFCREINFORCINGBAR,
        WEBIFC.IFCREINFORCINGELEMENT,
    ];
    for (const cat of excludedCats) {
        ifcLoader.settings.excludedCategories.add(cat);
    }
    ifcLoader.settings.webIfc.COORDINATE_TO_ORIGIN = false;
    //ifcLoader.settings.webIfc.OPTIMIZE_PROFILES = true;
    //ifcLoader.settings.optionalCategories = [3856911033, 3856911033];
    await ifcLoader.setup();
    //const tiler = viewer.get(TO_Core.IfcGeometryTiler);
    //tiler.settings.wasm = wasm;
    //tiler.settings.minGeometrySize = 20;
    //tiler.settings.minAssetsSize = 1000;
    raycasters = viewer.get(TO_Core.Raycasters);
    raycaster = raycasters.get(world);
    //let vpConfig = {
    //    snapDistance: 1,
    //    showOnlyVertex: true,
    //};
    //vertexPicker = new TO_Core.VertexPicker(viewer, vpConfig);
    //vertexPicker.enabled = true;
    cullers = viewer.get(TO_Core.Cullers);
    culler = cullers.create(world);
    culler.threshold = 200;
    culler.needsUpdate = true;
    highlightedFragmentsMap = {};
    //highlightedNoFragmentList = new Set<{ ifcModelKey: string, expressID: number }>();
    //highlightedModel = null;
    highlighter = viewer.get(TO_Front.Highlighter);
    outliner = viewer.get(TO_Front.Outliner);
    outliner.world = world;
    outliner.enabled = true;
    outliner.create("selectionOutline", new THREE.MeshBasicMaterial({
        color: 0xbcf124,
        transparent: true,
        opacity: 0.4,
    }));
    let hlConfig = {
        selectionColor: new THREE.Color(0xF3A401),
        //hoverColor: new THREE.Color(0x002840),
        hoverEnabled: false,
        autoHighlightOnClick: true,
        world: world
    };
    highlighter.setup(hlConfig);
    //highlighter.setup(null);
    highlighter.events.select.onHighlight.add(selection => onHighlight(selection));
    highlighter.events.select.onClear.add(() => onHighlighterCleared());
    //propertiesManager = viewer.get(TO_Core.IfcPropertiesManager); // new TO_Core.IfcPropertiesManager(viewer)
    indexer = viewer.get(TO_Core.IfcRelationsIndexer);
    //propertiesProcessor.attributesToIgnore = [];
    //highlighter.events.select.onClear.add(() => {
    //    propertiesManager.cleanPropertiesList();
    //})
    fragmentsManager = await viewer.get(TO_Core.FragmentsManager);
    hider = viewer.get(TO_Core.Hider);
    hider.enabled = true;
    transparentFragmentsMap = {};
    auxFragments = new AUX_FRAGMENTS.AuxFragments(world);
    world.camera.controls.addEventListener("controlstart", onCameraControlStart);
    world.camera.controls.addEventListener("controlend", onCameraControlEnd);
    world.camera.controls.addEventListener("wake", onCameraWake);
    world.camera.controls.addEventListener("sleep", onCameraSleep);
    world.camera.controls.infinityDolly = true;
    world.camera.controls.minDistance = 50;
    world.camera.controls.maxDistance = Infinity;
    world.camera.controls.minZoom = 0.01;
    world.camera.controls.maxZoom = Infinity;
    clipper = viewer.get(TO_Core.Clipper);
    clipper.enabled = false;
    lengthMeasurement = viewer.get(TO_Front.LengthMeasurement);
    lengthMeasurement.world = world;
    lengthMeasurement.enabled = false;
    lengthMeasurement.snapDistance = 10;
    areaMeasurement = viewer.get(TO_Front.AreaMeasurement);
    areaMeasurement.world = world;
    areaMeasurement.enabled = false;
    angleMeasurement = viewer.get(TO_Front.AngleMeasurement);
    angleMeasurement.world = world;
    angleMeasurement.enabled = false;
    world.renderer.three.domElement.onclick = (ev) => onClick(ev);
    if (!isTouchDevice()) {
        world.renderer.three.domElement.addEventListener("contextmenu", e => onContextMenu(e), options);
    }
    world.renderer.three.domElement.addEventListener("mousedown", e => onMouseDown(e), options);
    world.renderer.three.domElement.addEventListener("mousemove", e => onMouseMove(e), options);
    world.renderer.three.domElement.addEventListener("mouseup", () => onPointerUp(), options);
    //viewerContainer.addEventListener("mousewheel", () => onMouseWheel(), options)
    world.renderer.three.domElement.addEventListener("touchstart", e => onTouchStart(e), options);
    world.renderer.three.domElement.addEventListener("touchmove", e => onTouchMove(e), options);
    world.renderer.three.domElement.addEventListener("touchend", e => onTouchEnd(e), options);
    allFragmentsMap = {};
    modelFragmentItemsMap = new Map();
    ifcSpacesFragmentsMap = new Map();
    hideIfcSpaces = true;
    viewer.init();
    world.renderer.enabled = false;
    const observerOptions = {
        root: document.documentElement,
    };
    intersectionObserver = new IntersectionObserver((entries, observer) => {
        let ratio = 0;
        entries.forEach(entry => {
            ratio += entry.intersectionRatio;
        });
        if (ratio > 0) {
            refreshView();
        }
    }, observerOptions);
    resizeObserver = new ResizeObserver((entries) => {
        refreshView();
    });
    intersectionObserver.observe(viewerContainer);
    resizeObserver.observe(viewerContainer);
}
function refreshView() {
    if (world?.renderer) {
        const previous = world.renderer.enabled;
        world.renderer.enabled = true;
        world.renderer.update();
        //Timeout(100)
        world.renderer.enabled = previous;
    }
}
//async function setCameraPerspective() {
//    await viewer.camera.setProjection("Perspective");
//    NotifyCameraProjection();
//}
//async function setCameraOrthographic() {
//    await viewer.camera.setProjection("Orthographic");
//    NotifyCameraProjection();
//}
async function toggleCameraProjection() {
    await world.camera.projection.toggle();
    NotifyCameraProjection();
}
function NotifyCameraProjection() {
    const isPerspective = world.camera.projection.current == "Perspective";
    dotNetObj.invokeMethodAsync('UpdateCameraProjection', isPerspective);
}
async function initModels() {
    let first = true;
    let basePositions = new Map();
    fragmentsManager.coordinate();
    for (const [modelKey, model] of fragmentsManager.groups) {
        ////let basePos = new THREE.Vector3(
        ////    (model as THREE.Object3D).position.x,
        ////    (model as THREE.Object3D).position.y,
        ////    (model as THREE.Object3D).position.z
        ////);
        //const basePos = basePositions.get(modelKey);
        //    //if (!first)
        //    //    fragmentsManager.applyBaseCoordinateSystem(model);
        //    //first = false;
        world.scene.three.attach(model);
        for (const child of model.items) {
            if (child.mesh) {
                //(child.mesh as THREE.Object3D).translateX(basePos.x);
                //(child.mesh as THREE.Object3D).translateY(basePos.y);
                //(child.mesh as THREE.Object3D).translateZ(basePos.z);
                world.meshes.add(child.mesh);
            }
        }
        await indexer.process(model);
    }
    extractAllFragmentsMap();
    let count = 0;
    for (const frag of [...fragmentsManager.list.values()]) {
        const modelKey = fragmentModelMap.get(frag.id);
        const modelPos = fragmentsManager.groups.get(modelKey).position;
        auxFragments.createTransparentFragment(frag, modelPos, transparentOpacity);
        //auxFragments.createHighlightedFragment(frag, modelPos);
        count++;
    }
    transparentFragmentsMap = {};
    shownFragmentsMap = structuredClone(allFragmentsMap);
    hider.set(true);
    //hider.update();
    world.renderer.update();
    culler.needsUpdate = true;
    extractUnits();
    extractSpatialStructure();
    extractTypes();
    extractGroups();
    await refreshHiddenTransparent(true);
    await resetZoom();
    refreshView();
    NotifyCameraProjection();
}
async function onMouseDown(e) {
    ctrlPressed = e.ctrlKey;
    shiftPressed = e.shiftKey;
    mouseDownPosition = { x: e.clientX, y: e.clientY };
    await onPointerDown();
}
let timer;
let showContextMenuFlag = false;
async function onTouchStart(e) {
    if (e.touches.length == 1) {
        ctrlPressed = e.ctrlKey;
        shiftPressed = e.shiftKey;
        //touchMoved = false;
        showContextMenuFlag = false;
        mouseDownPosition = { x: e.touches[0].clientX, y: e.touches[0].clientY };
        await onPointerDown();
        timer = setTimeout(() => {
            showContextMenuFlag = true;
        }, 500);
    }
}
async function onPointerDown() {
    world.camera.controls.enabled = !(lengthMeasurement.enabled || areaMeasurement.enabled || angleMeasurement.enabled);
    if (world.renderer)
        world.renderer.enabled = clipper.enabled || world.camera.controls.enabled;
}
async function onMouseMove(e) {
    await onPointerMove(e.clientX, e.clientY);
}
/*let touchMoved: boolean = false;*/
async function onTouchMove(e) {
    if (e.touches.length == 1) {
        //touchMoved = true;
        clearTimeout(timer);
        showContextMenuFlag = false;
        await onPointerMove(e.touches[0].clientX, e.touches[0].clientY);
    }
}
async function onPointerMove(x, y) {
    // Calculate the distance the mouse has moved since mouse down
    if (mouseDownPosition && !disableClick) {
        const dx = x - mouseDownPosition.x;
        const dy = y - mouseDownPosition.y;
        const moveDistance = Math.sqrt(dx * dx + dy * dy);
        // If the distance is greater than the threshold, set dragging to true
        if (moveDistance > mouseMoveThreshold) {
            disableClick = true;
            if (world.camera.controls.enabled && !(lengthMeasurement.enabled || areaMeasurement.enabled || angleMeasurement.enabled)) {
                if (world.renderer) {
                    world.renderer.enabled = true;
                    world.renderer.update();
                }
                //cameraRotationPoint = null;
                world.camera.controls.minDistance = 0.01;
                if (highlightedObjectsCounter == 1) {
                    let boundingBoxer = viewer.get(TO_Core.BoundingBoxer);
                    boundingBoxer.addFragmentIdMap(highlightedFragmentsMap);
                    let bBoxSphere = boundingBoxer.getSphere();
                    //cameraRotationPoint = new THREE.Vector3(bBoxSphere.center.x, bBoxSphere.center.y, bBoxSphere.center.z);
                    world.camera.controls.setOrbitPoint(bBoxSphere.center.x, bBoxSphere.center.y, bBoxSphere.center.z);
                    boundingBoxer.reset();
                }
                else {
                    const bounds = viewerContainer.getBoundingClientRect();
                    const x = ((mouseDownPosition.x - bounds.left) / (bounds.right - bounds.left)) * 2 - 1;
                    const y = -((mouseDownPosition.y - bounds.top) / (bounds.bottom - bounds.top)) * 2 + 1;
                    let pointerPos = new THREE.Vector2(x, y);
                    raycaster.three.setFromCamera(pointerPos, world.camera.three);
                    let raycasterRes = intersect(Array.from(world.meshes));
                    if (raycasterRes) {
                        //cameraRotationPoint = new THREE.Vector3(raycasterRes.point.x, raycasterRes.point.y, raycasterRes.point.z);
                        world.camera.controls.setOrbitPoint(raycasterRes.point.x, raycasterRes.point.y, raycasterRes.point.z);
                    }
                    //else {
                    //    cameraRotationPoint = world.camera.controls.getTarget();
                    //}
                }
                world.camera.controls.minDistance = 50;
                const target = world.camera.controls.getTarget();
                camTargetMesh.position.set(target.x, target.y, target.z);
                let factor;
                if (world.camera.projection.current == "Perspective") {
                    let distance = target.distanceTo(world.camera.threePersp.position);
                    let zooming = Math.min(0.02 * Math.tan(Math.PI * world.camera.threePersp.fov / 360) / world.camera.threePersp.zoom, 0.02);
                    factor = distance * zooming;
                }
                else {
                    factor = (world.camera.threeOrtho.top - world.camera.threeOrtho.bottom) / world.camera.threeOrtho.zoom;
                }
                camTargetMesh.scale.set(1, 1, 1).multiplyScalar(factor);
                world.scene.three.add(camTargetMesh);
            }
        }
    }
}
async function onTouchEnd(e) {
    await onPointerUp();
    //if (!touchMoved && showContextMenuFlag) {
    if (showContextMenuFlag) {
        e.stopPropagation();
        e.preventDefault();
        ShowContextMenu(e.changedTouches[0].clientX, e.changedTouches[0].clientY);
    }
    clearTimeout(timer);
    //touchMoved = false;
    showContextMenuFlag = false;
}
async function onPointerUp() {
    mouseDownPosition = undefined;
}
function onClick(e) {
    if (disableClick) {
        e.stopPropagation();
    }
    else if (lengthMeasurement.enabled) {
        measureLength();
    }
    else if (areaMeasurement.enabled) {
        measureArea();
    }
    else if (angleMeasurement.enabled) {
        measureAngle();
    }
    else if (clipper.enabled) {
        clip();
    }
    //else {
    //    highlighter.highlight('select', !ctrlPressed);
    //}
    disableClick = false;
}
////function onTouchEnd(e: TouchEvent) {
//    if (touching) {
//        //if (disableClick) {
//        //    e.stopPropagation();
//        //}
//        //else if (lengthMeasurement.enabled) {
//        //    measureLength();
//        //}
//        //else if (areaMeasurement.enabled) {
//        //    measureArea();
//        //}
//        //else if (angleMeasurement.enabled) {
//        //    measureAngle();
//        //}
//        //else if (clipper.enabled) {
//        //    clip();
//        //}
//        //else {
//        //    highlighter.highlight('select', !ctrlPressed);
//        //}
//        //disableClick = false;
//        world?.renderer?.three.domElement.dispatchEvent(new MouseEvent('mouseup'));
//    }
//    touching = false;
//    disableClick = true;
//}
function onContextMenu(e) {
    e.stopPropagation();
    e.preventDefault();
    ShowContextMenu(e.x, e.y);
}
function ShowContextMenu(x, y) {
    if (disableClick) {
        disableClick = false;
        return;
    }
    if (areaMeasurement != null && areaMeasurement.enabled && areaMeasureMode > 0) {
        areaMeasurement.endCreation();
        return;
    }
    dotNetObj.invokeMethodAsync('RightClick', x, y);
}
function onCameraControlStart() {
    //if (world.renderer) {
    //if (!(lengthMeasurement.enabled || areaMeasurement.enabled || angleMeasurement.enabled)) {
    //    const target = world.camera.controls.getTarget();
    //    camTargetMesh.position.set(target.x, target.y, target.z);
    //    let factor: number;
    //    if (world.camera.projection.current == "Perspective") {
    //        let distance = target.distanceTo(world.camera.threePersp.position);
    //        let zooming = Math.min(0.02 * Math.tan(Math.PI * world.camera.threePersp.fov / 360) / world.camera.threePersp.zoom, 0.02);
    //        factor = distance * zooming;
    //    } else {
    //        factor = (world.camera.threeOrtho.top - world.camera.threeOrtho.bottom) / world.camera.threeOrtho.zoom;
    //    }
    //    camTargetMesh.scale.set(1, 1, 1).multiplyScalar(factor);
    //    world.scene.three.add(camTargetMesh);
    //}
}
function intersect(items) {
    if (!world.renderer) {
        throw new Error("Renderer not found!");
    }
    const result = raycaster.three.intersectObjects(items);
    const filtered = filterClippingPlanes(result);
    return filtered.length > 0 ? filtered[0] : null;
}
function filterClippingPlanes(objs) {
    if (!world.renderer) {
        throw new Error("Renderer not found!");
    }
    if (!world.renderer.three.clippingPlanes) {
        return objs;
    }
    const planes = world.renderer.three.clippingPlanes;
    if (objs.length <= 0 || !planes || planes?.length <= 0)
        return objs;
    return objs.filter((elem) => planes.every((elem2) => elem2.distanceToPoint(elem.point) > 0));
}
function onCameraControlEnd() {
    //world.scene.three.remove(camTargetMesh);
    //culler.needsUpdate = true;
}
function onCameraWake() {
    //disableClick = true;
    //if (cameraRotationPoint) {
    //    camTargetMesh.position.set(cameraRotationPoint.x, cameraRotationPoint.y, cameraRotationPoint.z);
    //    //world.camera.controls.minDistance = 50;
    //    pointerPos = null;
    //    camTargetMesh.position.set(cameraRotationPoint.x, cameraRotationPoint.y, cameraRotationPoint.z);
    //    let factor: number;
    //    if (world.camera.projection.current == "Perspective") {
    //        let distance = cameraRotationPoint.distanceTo(world.camera.threePersp.position);
    //        let zooming = Math.min(0.02 * Math.tan(Math.PI * world.camera.threePersp.fov / 360) / world.camera.threePersp.zoom, 0.02);
    //        factor = distance * zooming;
    //    } else {
    //        factor = (world.camera.threeOrtho.top - world.camera.threeOrtho.bottom) / world.camera.threeOrtho.zoom;
    //    }
    //    camTargetMesh.scale.set(1, 1, 1).multiplyScalar(factor);
    //    world.scene.three.add(camTargetMesh);
    //}
    if (world.renderer) {
        world.renderer.enabled = true;
        world.renderer.update();
    }
}
function onCameraSleep() {
    disableClick = false;
    world.scene.three.remove(camTargetMesh);
    culler.needsUpdate = true;
    if (world.renderer) {
        //world.scene.three.remove(camTargetMesh);
        world.renderer.update();
        world.renderer.enabled = false;
    }
}
function focusSelection() {
    if (highlightedObjectsCounter == 1) {
        world.camera.controls.minDistance = 0.01;
        let boundingBoxer = viewer.get(TO_Core.BoundingBoxer);
        boundingBoxer.addFragmentIdMap(highlightedFragmentsMap);
        const bBoxSphere = boundingBoxer.getSphere();
        let cameraRotationPoint = new THREE.Vector3();
        cameraRotationPoint.copy(bBoxSphere.center);
        let cameraTarget = world.camera.controls.getTarget();
        let cameraPos = world.camera.controls.getPosition();
        let targetVector = new THREE.Vector3().subVectors(cameraTarget, cameraPos);
        let distanceVector = new THREE.Vector3().subVectors(cameraRotationPoint, cameraPos);
        distanceVector.projectOnVector(targetVector);
        let newCameraPos = new THREE.Vector3().subVectors(cameraRotationPoint, distanceVector);
        world.camera.controls.setLookAt(newCameraPos.x, newCameraPos.y, newCameraPos.z, cameraRotationPoint.x, cameraRotationPoint.y, cameraRotationPoint.z, true);
        //world.camera.controls.setTarget(cameraRotationPoint.x, cameraRotationPoint.y, cameraRotationPoint.z, true);
        boundingBoxer.reset();
        /*        camTargetMesh.position.set(cameraRotationPoint.x, cameraRotationPoint.y, cameraRotationPoint.z);*/
        world.camera.controls.minDistance = 50;
    }
}
let forceRemovePrevious = null;
let updateDotNet = true;
async function onHighlight(selection) {
    let counter = 3000;
    while (highlighterClearing && counter > 0) {
        await Timeout(20);
        counter--;
    }
    /*    let removePreviousFragments = (forceRemovePrevious == null)*/
    let removePrevious = forceRemovePrevious ?? !ctrlPressed;
    forceRemovePrevious = null;
    ctrlPressed = false;
    if (selection) {
        highlightedFragmentsMap = selection;
    }
    let itemIDs = new Array();
    for (const fragId of Object.keys(highlightedFragmentsMap)) {
        const modelKey = fragmentModelMap.get(fragId);
        if (modelKey) {
            for (let eid of highlightedFragmentsMap[fragId]) {
                let found = false;
                for (let iid of itemIDs) {
                    if (iid.ifcModelKey == modelKey && iid.expressID == eid) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    itemIDs[itemIDs.length] = { ifcModelKey: modelKey, expressID: eid };
                }
            }
        }
    }
    if (updateDotNet) {
        await dotNetObj.invokeMethodAsync('UpdateSelection', itemIDs, true, removePrevious, false);
    }
    updateDotNet = true;
    await refreshHiddenTransparent();
}
async function updateProperties(modelKey, expressID, keepHistory = false) {
    let properties = new Array();
    const model = fragmentsManager.groups.get(modelKey);
    if (model) {
        properties[0] = await model.getProperties(expressID);
        if (properties[0] != null && properties[0].expressID != undefined) {
            properties = properties.concat(await getRelatedProperties(model, expressID));
        }
    }
    if (properties != null && properties.length > 0) {
        if (modelKey == null) {
            for (let [cKey, cModel] of fragmentsManager.groups) {
                if (cModel === model) {
                    modelKey = cKey;
                }
            }
        }
        await dotNetObj.invokeMethodAsync('UpdateProperties', { ifcModelKey: modelKey, expressID: expressID }, JSON.stringify(properties), keepHistory);
    }
    else {
        await dotNetObj.invokeMethodAsync('UpdateProperties', null, "", keepHistory);
    }
}
async function getRelatedProperties(model, expressID) {
    let properties = new Array();
    let psetIds = indexer.getEntityRelations(model, expressID, "IsDefinedBy");
    if (psetIds) {
        for (const eID of psetIds) {
            // You can get the pset attributes like this
            const pset = await model.getProperties(eID);
            properties[properties.length] = pset;
            // You can get the pset props like this or iterate over pset.HasProperties yourself
            //await TO_Core.IfcPropertiesUtils.getPsetProps(
            //    model,
            //    eID,
            //    async (propExpressID) => {
            //        const prop = await model.getProperties(propExpressID);
            //        console.log(prop);
            //    },
            //);
        }
    }
    psetIds = indexer.getEntityRelations(model, expressID, "IsTypedBy");
    if (psetIds) {
        for (const eID of psetIds) {
            const pset = await model.getProperties(eID);
            properties[properties.length] = pset;
        }
    }
    psetIds = indexer.getEntityRelations(model, expressID, "IsGroupedBy");
    if (psetIds) {
        for (const eID of psetIds) {
            const pset = await model.getProperties(eID);
            properties[properties.length] = pset;
        }
    }
    psetIds = indexer.getEntityRelations(model, expressID, "ContainedInStructure");
    if (psetIds) {
        for (const eID of psetIds) {
            const pset = await model.getProperties(eID);
            properties[properties.length] = pset;
        }
    }
    psetIds = indexer.getEntityRelations(model, expressID, "HasAssociations");
    if (psetIds) {
        for (const eID of psetIds) {
            const pset = await model.getProperties(eID);
            properties[properties.length] = pset;
        }
    }
    return properties;
}
async function getName(modelKey, expressId) {
    let name = '';
    const model = await fragmentsManager.groups.get(modelKey);
    if (model) {
        const prop = await model.getProperties(expressId);
        if (prop) {
            name = prop["Name"]["value"];
        }
    }
    return name;
}
async function getMaterials(modelKey, materialLayerSetExpressID) {
    //1838606355: "IFCMATERIAL";
    //                        248100487: "IFCMATERIALLAYER",
    //                            3303938423: "IFCMATERIALLAYERSET",
    //                                1303795690: "IFCMATERIALLAYERSETUSAGE",
    const model = await fragmentsManager.groups.get(modelKey);
    if (!model)
        return;
    const mls = await model.getProperties(materialLayerSetExpressID);
    if (!mls)
        return;
    const mlsTypeId = Number(mls["type"]);
    if (mlsTypeId == 3303938423) {
        let mlsName = "";
        if (mls["LayerSetName"] != null) {
            mlsName = mls["LayerSetName"]["value"];
        }
        const layers = mls["MaterialLayers"];
        if (layers != null) {
            let childrenLayers = new Array();
            for (let i = 0; i < layers.length; i++) {
                const layerId = layers[i]["value"];
                if (model) {
                    const layer = await model.getProperties(layerId);
                    if (layer != null) {
                        let childrenProperties = new Array();
                        for (let key of Object.keys(layer)) {
                            switch (key.toLowerCase()) {
                                case "expressid":
                                case "type":
                                    break;
                                case "material":
                                    if (layer[key] != null) {
                                        const materialId = layer[key]["value"];
                                        //const material = await highlightedModel.getProperties(materialId);
                                        //const materialName: string = material["Name"]["value"];
                                        const materialName = await getName(modelKey, materialId);
                                        childrenProperties[childrenProperties.length] = { Name: "Materiale", Value: materialName };
                                    }
                                    break;
                                default:
                                    let value = null;
                                    if (layer[key] != null) {
                                        if (layer[key]["value"] !== undefined) {
                                            value = String(layer[key]["value"]);
                                        }
                                        else {
                                            value = String(layer[key]);
                                        }
                                    }
                                    childrenProperties[childrenProperties.length] = { Name: key, Value: value };
                                    break;
                            }
                        }
                        const levelName = "Livello " + i;
                        childrenLayers[childrenLayers.length] = { Name: levelName, ChildrenProperties: childrenProperties };
                    }
                }
            }
            dotNetObj.invokeMethodAsync('UpdateMaterials', Number(materialLayerSetExpressID), mlsName, childrenLayers);
        }
    }
    else {
        console.log("DEBUG - Wrong type for Material Layer Set");
    }
}
let highlighterClearedCallback;
let highlighterClearing = false;
async function onHighlighterCleared() {
    highlighterClearing = true;
    /*if (!manualCleaning) {*/
    highlightedFragmentsMap = {};
    //for (const fragID in allFragmentsMap) {
    //    const fragment = fragmentsManager.list.get(fragID);
    //    if (!fragment || !fragment.mesh) continue;
    //    if (highlightedOriginalMaterials[fragID]) {
    //        (fragment.mesh as THREE.InstancedMesh).renderOrder = highlightedOriginalMaterials[fragID][0]
    //        fragment.mesh.material = highlightedOriginalMaterials[fragID][1];
    //        highlightedOriginalMaterials[fragID] = undefined;
    //    }
    //}
    //culler.needsUpdate = true;
    //world.renderer!.update();
    //propertiesManager.cleanPropertiesList()
    await refreshHiddenTransparent();
    if (updateDotNet) {
        await dotNetObj.invokeMethodAsync('ClearSelection');
    }
    updateDotNet = true;
    //dotNetObj.invokeMethodAsync('UpdateProperties', null, "", false)
    //}
    //manualCleaning = false;
    if (highlighterClearedCallback) {
        await highlighterClearedCallback.function(...highlighterClearedCallback.args);
    }
    highlighterClearing = false;
}
async function hasGeneralProperties(modelKey, expressID) {
    try {
        const model = fragmentsManager.groups.get(modelKey);
        if (model) {
            const properties = await model.getProperties(expressID); //propertiesProcessor.getProperties(model, expressID);
            if (properties != null) {
                return properties.expressID == expressID; //properties.length > 0 && properties[0].expressID == expressID;
            }
        }
    }
    catch (error) {
        console.log(error);
    }
    return false;
}
async function getModelGlobalId(modelKey) {
    try {
        const model = fragmentsManager.groups.get(modelKey);
        if (model) {
            const properties = await model.getAllPropertiesOfType(103090709);
            if (properties != null) {
                return Object.values(properties)[0]["GlobalId"]["value"];
            }
        }
    }
    catch (error) {
        console.log(error);
    }
    return "";
}
async function getObjectGlobalId(modelKey, expressID) {
    try {
        const model = fragmentsManager.groups.get(modelKey);
        if (model) {
            const properties = await model.getProperties(expressID); //propertiesProcessor.getProperties(model, expressID);
            if (properties != null) {
                return properties["GlobalId"]["value"];
            }
        }
    }
    catch (error) {
        console.log(error);
    }
    return "";
}
function isFragment(modelKey, expressID) {
    /*    let array = [...modelFragmentItemsMap.get(modelKey)!.values()].flatMap(x => [...x]);*/
    let array = [...modelItemsInFragmentMap.get(modelKey)];
    return binarySearch(array, expressID);
}
function isIfcSpace(typeId) {
    const result = (typeId == 3856911033 || typeId == 1095909175);
    return result;
}
function binarySearch(arr, x) {
    let start = 0, end = arr.length - 1;
    // Iterate while start not meets end
    while (start <= end) {
        // Find the mid index
        let mid = Math.floor((start + end) / 2);
        // If element is present at 
        // mid, return True
        if (arr[mid] === x)
            return true;
        // Else look in left or 
        // right half accordingly
        else if (arr[mid] < x)
            start = mid + 1;
        else
            end = mid - 1;
    }
    return false;
}
async function highlightByID(name, fragmentIdMap, removePrevious) {
    forceRemovePrevious = removePrevious;
    updateDotNet = false;
    await highlighter.highlightByID(name, fragmentIdMap, removePrevious);
}
async function highlight(modelKey, expressID, removePrevious, includeHidden) {
    if (removePrevious) {
        //highlightedNoFragmentList.clear();
        updateDotNet = false;
        highlighterClearedCallback = { function: highlight2, args: [modelKey, expressID, includeHidden] };
        highlighter.clear('select');
    }
    else {
        highlight2(modelKey, expressID, includeHidden);
    }
}
async function highlight2(...args) {
    const modelKey = args[0];
    const expressID = args[1];
    //const removePrevious: boolean = args[2];
    const includeHidden = args[2];
    highlighterClearedCallback = undefined;
    let newSelection = {};
    const fids = itemModelFragmentsMap.get(expressID)?.get(modelKey);
    if (fids) {
        for (const fid of fids) {
            let remove = false;
            if (hideIfcSpaces) {
                let map = ifcSpacesFragmentsMap.get(modelKey);
                remove = (map && map[fid]?.has(expressID)) ?? false;
            }
            if (!includeHidden) {
                remove = !((shownFragmentsMap && shownFragmentsMap[fid]?.has(expressID)) ?? false);
            }
            if (!remove) {
                if (newSelection[fid] != null) {
                    newSelection[fid].add(expressID);
                }
                else {
                    newSelection[fid] = new Set([expressID]);
                }
            }
        }
    }
    if (Object.keys(newSelection).length > 0) {
        await highlightByID('select', newSelection, false);
        return;
    }
    ////E' un oggetto senza rappresentazione grafica
    //if (await (hasGeneralProperties(modelKey, expressID))) {
    //    //if (removePrevious) {
    //    ////    await onHighlighterCleared();
    //    ////    manualCleaning = true;
    //    ////    highlighter.clear();
    //    ////    highlightedNoFragmentList.clear();
    //    //}
    //    //const newSelectedId = { ifcModelKey: modelKey, expressID: expressID }
    //    //highlightedNoFragmentList.add(newSelectedId);
    //    //const selectedCount: number = await dotNetObj.invokeMethodAsync('UpdateSelection', [], removePrevious, [newSelectedId], removePrevious, true);
    //    //if (selectedCount == 1) {
    //    //    updateProperties(modelKey, expressID);
    //    //}
    //}
}
async function highlightMany(itemIDs, removePrevious, includeHidden) {
    //if (resetHidden) {
    //    showMany(itemIDs, true);
    //}
    if (removePrevious) {
        highlightedFragmentsMap = {};
        //highlightedNoFragmentList.clear();
    }
    //let newSelection: TO_Frag.FragmentIdMap = {};
    //let newSelectionWithoutFragment = new Array<{ ifcModelKey: string, expressID: number }>
    for (const id of itemIDs) {
        const fids = itemModelFragmentsMap.get(id.expressID)?.get(id.ifcModelKey);
        if (fids) {
            for (const fid of fids) {
                let remove = false;
                if (hideIfcSpaces) {
                    let map = ifcSpacesFragmentsMap.get(id.ifcModelKey);
                    remove = (map && map[fid]?.has(id.expressID)) ?? false;
                }
                if (!includeHidden) {
                    remove = !((shownFragmentsMap && shownFragmentsMap[fid]?.has(id.expressID)) ?? false);
                }
                if (!remove) {
                    if (highlightedFragmentsMap[fid] != null) {
                        highlightedFragmentsMap[fid].add(id.expressID);
                    }
                    else {
                        highlightedFragmentsMap[fid] = new Set([id.expressID]);
                    }
                }
            }
        }
        else if (await (hasGeneralProperties(id.ifcModelKey, id.expressID))) {
            //highlightedNoFragmentList.add(id);
        }
    }
    //let eids = new Set<number>();
    //for (const fid of Object.keys(newSelection)) {
    //    for (const eid of newSelection[fid]) {
    //        eids.add(eid);
    //    }
    //}
    await highlightByID('select', highlightedFragmentsMap, removePrevious);
    /*    await dotNetObj.invokeMethodAsync('UpdateSelectedIds', [...highlightedNoFragmentList], false, false);*/
}
async function highlightManyByGlobalIDs(globalIDs) {
    highlightedFragmentsMap = {};
    //highlightedNoFragmentList.clear();
    for (const mid of modelFragmentItemsMap.keys()) {
        const mgid = await getModelGlobalId(mid);
        const frags = modelFragmentItemsMap.get(mid);
        for (const fid of frags.keys()) {
            for (const eid of frags.get(fid)) {
                let ogid = await getObjectGlobalId(mid, eid);
                for (let item of globalIDs) {
                    if (item.modelGlobalID == mgid && item.objectGlobalID == ogid) {
                        if (highlightedFragmentsMap[fid] != null) {
                            highlightedFragmentsMap[fid].add(eid);
                        }
                        else {
                            highlightedFragmentsMap[fid] = new Set([eid]);
                        }
                    }
                }
            }
        }
    }
    await highlightByID('select', highlightedFragmentsMap, true);
    //await dotNetObj.invokeMethodAsync('UpdateSelectedIds', [...highlightedNoFragmentList], false, false);
}
async function removeHighlight(modelKey, expressID) {
    const fids = itemModelFragmentsMap.get(expressID)?.get(modelKey);
    //if (fids) {
    //    for (const fid of fids) {
    //        if (highlightedFragmentsMap[fid].has(expressID)) {
    //            highlightedFragmentsMap[fid].delete(expressID);
    //            if (highlightedFragmentsMap[fid].size == 0) {
    //                delete highlightedFragmentsMap[fid];
    //            }
    //        }
    //    }
    //}
    //else {
    //    for (let key of highlightedNoFragmentList) {
    //        if (key.ifcModelKey == modelKey && key.expressID == expressID) {
    //            highlightedNoFragmentList.delete(key);
    //        }
    //    }
    //    //highlightedNoFragmentList.delete({ifcModelKey: modelKey, expressID:expressID})
    //}
    let fim = {};
    if (fids) {
        for (const fid of fids) {
            fim[fid] = new Set([expressID]);
        }
    }
    forceRemovePrevious = false;
    updateDotNet = false;
    highlighter.highlightByID('select', fim, false, false, undefined, undefined, true);
    //await onHighlight();
    //await dotNetObj.invokeMethodAsync('UpdateSelectedIds', [...highlightedNoFragmentList], false, false);
}
async function removeHighlightMany(itemIDs) {
    let fim = {};
    for (const item of itemIDs) {
        const fids = itemModelFragmentsMap.get(item.expressID)?.get(item.ifcModelKey);
        if (fids) {
            for (const fid of fids) {
                if (fim[fid]) {
                    fim[fid].add(item.expressID);
                }
                else {
                    fim[fid] = new Set([item.expressID]);
                }
            }
        }
    }
    forceRemovePrevious = false;
    updateDotNet = false;
    highlighter.highlightByID('select', fim, false, false, undefined, undefined, true);
    //await onHighlight();
    //await dotNetObj.invokeMethodAsync('UpdateSelectedIds', [...highlightedNoFragmentList], false, false);
}
async function highlightNone() {
    highlighter.clear();
}
async function highlightVisible() {
    let newSelection = structuredClone(shownFragmentsFilteredMap);
    await highlightByID('select', newSelection, true);
}
async function setTransparent() {
    for (let fid of Object.keys(highlightedFragmentsMap)) {
        if (!Object.keys(transparentFragmentsMap).includes(fid)) {
            transparentFragmentsMap[fid] = new Set();
        }
        for (let eid of highlightedFragmentsMap[fid]) {
            transparentFragmentsMap[fid].add(eid);
        }
    }
    await refreshHiddenTransparent();
}
async function setTransparentMany(itemIDs) {
    for (let id of itemIDs) {
        const fids = itemModelFragmentsMap.get(id.expressID)?.get(id.ifcModelKey);
        if (fids) {
            for (const fid of fids) {
                if (!Object.keys(transparentFragmentsMap).includes(fid)) {
                    transparentFragmentsMap[fid] = new Set();
                }
                transparentFragmentsMap[fid].add(id.expressID);
            }
        }
    }
    await refreshHiddenTransparent();
}
async function setAllTransparent(refresh = true) {
    transparentFragmentsMap = structuredClone(shownFragmentsFilteredMap);
    if (refresh) {
        await refreshHiddenTransparent();
    }
}
async function removeTransparency() {
    for (let fid of Object.keys(highlightedFragmentsMap)) {
        if (Object.keys(transparentFragmentsMap).includes(fid)) {
            for (let eid of highlightedFragmentsMap[fid]) {
                transparentFragmentsMap[fid].delete(eid);
            }
            if (transparentFragmentsMap[fid].size == 0) {
                delete transparentFragmentsMap[fid];
            }
        }
    }
    await refreshHiddenTransparent();
}
async function removeTransparencyMany(itemIDs) {
    for (let id of itemIDs) {
        for (let fid of Object.keys(transparentFragmentsMap)) {
            transparentFragmentsMap[fid].delete(id.expressID);
            if (transparentFragmentsMap[fid].size == 0) {
                delete transparentFragmentsMap[fid];
            }
        }
    }
    //if (refresh) {
    await refreshHiddenTransparent();
    //}
}
async function removeAllTransparency() {
    transparentFragmentsMap = {};
    await refreshHiddenTransparent();
}
async function flipTransparency() {
    const old = transparentFragmentsMap;
    transparentFragmentsMap = structuredClone(allFragmentsMap);
    for (let fid of Object.keys(transparentFragmentsMap)) {
        if (Object.keys(old).includes(fid)) {
            for (let eid of old[fid]) {
                transparentFragmentsMap[fid].delete(eid);
            }
        }
        if (transparentFragmentsMap[fid].size == 0) {
            delete transparentFragmentsMap[fid];
        }
    }
    await refreshHiddenTransparent();
}
async function highlightOpaque(resetHidden) {
    if (resetHidden) {
        show(false);
    }
    transparentFragmentsMap = structuredClone(allFragmentsMap);
    for (let fid of Object.keys(highlightedFragmentsMap)) {
        if (Object.keys(allFragmentsMap).includes(fid)) {
            for (let eid of highlightedFragmentsMap[fid]) {
                transparentFragmentsMap[fid].delete(eid);
            }
            if (transparentFragmentsMap[fid].size == 0) {
                delete transparentFragmentsMap[fid];
            }
        }
    }
    await refreshHiddenTransparent();
}
async function highlightOpaqueMany(itemIDs, resetHidden) {
    if (resetHidden) {
        showMany(itemIDs, false);
    }
    await setAllTransparent(false);
    await removeTransparencyMany(itemIDs);
    await highlightMany(itemIDs, true, false);
}
async function getPropertiesString(modelKey, expressID) {
    let result = "";
    //for (let model of fragmentsManager.groups.values()) {
    //const properties = await propertiesManager.getProperties(model, expressID);
    const model = fragmentsManager.groups.get(modelKey);
    if (model) {
        const properties = await model.getProperties(expressID);
        if (properties) {
            result = JSON.stringify(properties);
            //break;
        }
    }
    return result;
}
async function loadModelFromIFC(data, name) {
    const unzipped = await ungzip(data);
    let model = await ifcLoader.load(unzipped);
    model.ifcMetadata.name = name;
    data = new Uint8Array();
}
async function loadModelFromFragments(fragments, properties, name) {
    const unzippedFragments = await ungzip(fragments);
    let model = await fragmentsManager.load(unzippedFragments);
    model.ifcMetadata.name = name;
    fragments = new Uint8Array();
    const unzippedProperties = await ungzip(properties, { to: 'string' });
    model.setLocalProperties(JSON.parse(unzippedProperties));
    properties = new Uint8Array();
}
async function extractUnits() {
    for (let [key, model] of fragmentsManager.groups) {
        const props = model.getLocalProperties();
        if (props) {
            for (let currentObject of Object.values(props)) {
                const typeId = Number(currentObject["type"]);
                if (typeId == 180925521) {
                    ////IFCUNITASSIGNMENT = 180925521;
                    let units = currentObject["Units"];
                    let results = new Array();
                    for (let unit of units) {
                        const unitInfo = await GetUnitInfo(model, unit["value"]);
                        if (unitInfo["Type"] != null) {
                            results[results.length] = new Array(unitInfo["Type"], unitInfo["Name"]);
                        }
                    }
                    await dotNetObj.invokeMethodAsync('UpdateUnits', key, results);
                }
            }
        }
    }
}
async function GetUnitInfo(model, expressId) {
    const props = await model.getProperties(expressId);
    let unitInfo = {};
    if (props) {
        const type = props["type"];
        switch (type) {
            case 448429030: //IFCSIUNIT
            case 2889183280: //IFCONVERSIONBASEDUNIT
                unitInfo["Type"] = props["UnitType"]["value"];
                unitInfo["Name"] = "";
                if (props["Prefix"] != null) {
                    unitInfo["Name"] += props["Prefix"]["value"] + "_";
                }
                if (props["Name"] != null) {
                    unitInfo["Name"] += props["Name"]["value"];
                }
                break;
            case 1765591967: //IFCDERIVEDUNIT
                unitInfo["Type"] = props["UserDefinedType"] == undefined ? props["UnitType"]["value"] : props["UserDefinedType"]["value"];
                unitInfo["Name"] = "";
                for (let elem of props.Elements) {
                    const subInfo = await GetUnitInfo(model, elem["value"]);
                    unitInfo["Name"] += subInfo["Name"];
                }
                break;
            case 1045800335: //IFCDERIVEDUNITELEMENT
                const ueid = props["Unit"]["value"];
                unitInfo = await GetUnitInfo(model, ueid);
                if (props["Exponent"] != undefined && props["Exponent"] != 1) {
                    unitInfo["Name"] += "<sup>" + String(props["Exponent"]) + "</sup>";
                }
                break;
            case 2706619895: //IFCMONETARYUNIT
                unitInfo["Type"] = "CURRENCY";
                if (props["Currency"] != null) {
                    unitInfo["Name"] = props["Currency"]["value"];
                }
                break;
            default:
                break;
        }
    }
    return unitInfo;
}
async function extractSpatialStructure() {
    let spatialRelations = new Array();
    let models = new Array();
    for (const [modelKey, model] of fragmentsManager.groups) {
        const modelGlobalId = await getModelGlobalId(modelKey);
        models[models.length] = { IfcModelKey: modelKey, ModelGlobalId: modelGlobalId };
        let props = await model.getAllPropertiesOfType(3242617779); //IFCRELCONTAINEDINSPATIALSTRUCTURE
        if (props != null) {
            for (let currentObject of Object.values(props)) {
                const structureID = Number(currentObject["RelatingStructure"]["value"]);
                const structProp = await model.getProperties(structureID);
                if (structProp) {
                    const structureName = structProp["Name"]["value"];
                    const structureGlobalId = structProp["GlobalId"]["value"];
                    const structureType = structProp["type"];
                    const structureTypeName = TO_Core.IfcCategoryMap[structureType] ?? "";
                    const structureIsFragment = isFragment(modelKey, structureID);
                    const structureIsIfcSpace = isIfcSpace(structureType);
                    const structureHasProperties = await hasGeneralProperties(modelKey, structureID);
                    const relElem = currentObject["RelatedElements"];
                    let children = new Array(relElem.length);
                    for (let i = 0; i < relElem.length; i++) {
                        const eid = relElem[i]["value"];
                        const childProp = await model.getProperties(eid);
                        if (childProp) {
                            const elementName = childProp["Name"] == null ? "" : childProp["Name"]["value"];
                            const elementGlobalId = childProp["GlobalId"]["value"];
                            const elementType = childProp["type"];
                            const elementTypeName = TO_Core.IfcCategoryMap[elementType] ?? "";
                            const elementIsFragment = isFragment(modelKey, eid);
                            const elementIsIfcSpace = isIfcSpace(elementType);
                            const elementHasProperties = await hasGeneralProperties(modelKey, eid);
                            children[i] = { ID: { ifcModelKey: modelKey, expressID: eid }, Name: elementName, GlobalId: elementGlobalId, TypeName: elementTypeName, IsFragment: elementIsFragment, IsIfcSpace: elementIsIfcSpace, HasProperties: elementHasProperties };
                        }
                    }
                    spatialRelations[spatialRelations.length] = { ID: { ifcModelKey: modelKey, expressID: structureID }, Name: structureName, GlobalId: structureGlobalId, TypeName: structureTypeName, IsFragment: structureIsFragment, IsIfcSpace: structureIsIfcSpace, HasProperties: structureHasProperties, Children: children };
                }
            }
        }
        props = await model.getAllPropertiesOfType(160246688); //IFCRELAGGREGATES
        if (props != null) {
            for (let currentObject of Object.values(props)) {
                const structureID2 = Number(currentObject["RelatingObject"]["value"]);
                const structProp2 = await model.getProperties(structureID2);
                if (structProp2) {
                    let structureName2 = structProp2["Name"]["value"];
                    const structureGlobalId2 = structProp2["GlobalId"]["value"];
                    const structureType2 = structProp2["type"];
                    const structureTypeName2 = TO_Core.IfcCategoryMap[structureType2] ?? "";
                    if (structureType2 == 103090709) {
                        structureName2 += " (" + model.ifcMetadata.name + ")";
                    }
                    const structureIsFragment2 = isFragment(modelKey, structureID2);
                    const structureIsIfcSpace2 = isIfcSpace(structureType2);
                    const structureHasProperties2 = await hasGeneralProperties(modelKey, structureID2);
                    const relObj2 = currentObject["RelatedObjects"];
                    let children2 = new Array(relObj2.length);
                    for (let i = 0; i < relObj2.length; i++) {
                        const eid = relObj2[i]["value"];
                        const childProp = await model.getProperties(eid);
                        if (childProp) {
                            const elementName = childProp["Name"] == null ? "" : childProp["Name"]["value"];
                            const elementGlobalId = childProp["GlobalId"]["value"];
                            const elementType = childProp["type"];
                            const elementTypeName = TO_Core.IfcCategoryMap[elementType] ?? "";
                            const elementIsFragment = isFragment(modelKey, eid);
                            const elementIsIfcSpace = isIfcSpace(elementType);
                            const elementHasProperties2 = await hasGeneralProperties(modelKey, structureID2);
                            children2[i] = { ID: { ifcModelKey: modelKey, expressID: eid }, Name: elementName, GlobalId: elementGlobalId, TypeName: elementTypeName, IsFragment: elementIsFragment, IsIfcSpace: elementIsIfcSpace, HasProperties: elementHasProperties2 };
                        }
                    }
                    spatialRelations[spatialRelations.length] = { ID: { ifcModelKey: modelKey, expressID: structureID2 }, Name: structureName2, GlobalId: structureGlobalId2, TypeName: structureTypeName2, IsFragment: structureIsFragment2, IsIfcSpace: structureIsIfcSpace2, HasProperties: structureHasProperties2, Children: children2 };
                }
            }
        }
    }
    await dotNetObj.invokeMethodAsync('UpdateModelGlobalIds', models);
    await dotNetObj.invokeMethodAsync('BuildSpatialStructure', spatialRelations);
}
async function extractTypes() {
    let ifcTypes = {};
    let superTypes = {};
    for (let [modelKey, model] of fragmentsManager.groups) {
        let types = {};
        let ifcSpacesList = new Set();
        const props = model.getLocalProperties();
        if (props) {
            for (let currentObject of Object.values(props)) {
                const typeId = Number(currentObject["type"]);
                const typeName = TO_Core.IfcCategoryMap[typeId] ?? "";
                const objType = currentObject["ObjectType"];
                if (typeId !== undefined && typeId !== null) {
                    if (objType !== undefined) {
                        if (ifcTypes[typeId] == null) {
                            const propFlag = await hasGeneralProperties(modelKey, typeId);
                            ifcTypes[typeId] = { ID: { ifcModelKey: null, expressID: typeId }, Name: typeName, HasProperties: false, ChildrenElements: new Array() };
                        }
                        const expressId = Number(currentObject["expressID"]);
                        const globalId = currentObject["GlobalId"]["value"];
                        let name = expressId.toString();
                        if (currentObject["Name"] !== undefined && currentObject["Name"] !== null) {
                            name = currentObject["Name"]["value"];
                        }
                        const isFrag = isFragment(modelKey, expressId);
                        const isTypeIfcSpace = isIfcSpace(typeId);
                        const hasProp = await hasGeneralProperties(modelKey, expressId);
                        ifcTypes[typeId].ChildrenElements[ifcTypes[typeId].ChildrenElements.length] = { ID: { ifcModelKey: modelKey, expressID: expressId }, Name: name, GlobalId: globalId, TypeExpressID: typeId, TypeName: typeName, IsFragment: isFrag, IsIfcSpace: isTypeIfcSpace, HasProperties: hasProp };
                        if (isTypeIfcSpace) { //(typeId == 3856911033 || typeId == 1095909175) {
                            //IFCSPACE
                            ifcSpacesList.add(expressId);
                        }
                    }
                    else if ((typeName.toUpperCase().endsWith("TYPE") && typeName.toUpperCase() != "IFCRELDEFINESBYTYPE") || typeName.toUpperCase().endsWith("STYLE")) {
                        if (!Object.keys(superTypes).includes(String(typeId))) {
                            const propFlag = await hasGeneralProperties(modelKey, typeId);
                            superTypes[typeId] = { ID: { ifcModelKey: null, expressID: typeId }, Name: typeName, HasProperties: false, ChildrenTypes: new Array() };
                        }
                        const expressId = Number(currentObject["expressID"]);
                        const globalId = currentObject["GlobalId"]["value"];
                        let name = expressId.toString();
                        if (currentObject["Name"] !== undefined && currentObject["Name"] !== null) {
                            name = currentObject["Name"]["value"];
                        }
                        const propFlag = await hasGeneralProperties(modelKey, expressId);
                        let type = { ID: { ifcModelKey: modelKey, expressID: expressId }, Name: name, GlobalId: globalId, SuperTypeExpressID: typeId, SuperTypeName: typeName, HasProperties: propFlag, ChildrenElements: new Array() };
                        types[expressId] = type;
                        superTypes[typeId].ChildrenTypes[superTypes[typeId].ChildrenTypes.length] = type;
                    }
                }
            }
            for (let currentObject of Object.values(props)) {
                const typeId = Number(currentObject["type"]);
                const typeName = TO_Core.IfcCategoryMap[typeId] ?? "";
                /*const objType = currentObject["ObjectType"];*/
                if (typeId == 781010003) {
                    //IFCRELDEFINESBYTYPE
                    const relTypeId = currentObject["RelatingType"]["value"];
                    if (relTypeId !== undefined && relTypeId !== null) {
                        let type = types[relTypeId];
                        if (type == null) {
                            console.log("DEBUG - type not found: " + relTypeId.toString());
                        }
                        else {
                            let typeName = type["Name"];
                            const relObjs = currentObject["RelatedObjects"];
                            let children = new Array(relObjs.length);
                            for (let i = 0; i < relObjs.length; i++) {
                                const objId = relObjs[i]["value"];
                                let objName = objId.toString();
                                let objGlobalId = null;
                                let isTypeIfcSpace = false;
                                const prop = await model.getProperties(objId);
                                if (prop != null) {
                                    const name = prop["Name"];
                                    if (name !== undefined) {
                                        objName = name["value"];
                                    }
                                    objGlobalId = prop["GlobalId"]["value"];
                                    const objTypeID = prop["type"];
                                    isTypeIfcSpace = isIfcSpace(objTypeID);
                                }
                                const isFrag = isFragment(modelKey, objId);
                                const hasProp = await hasGeneralProperties(modelKey, objId);
                                children[i] = { ID: { ifcModelKey: modelKey, expressID: objId }, Name: objName, GlobalId: objGlobalId, TypeExpressID: typeId, TypeName: typeName, IsFragment: isFrag, IsIfcSpace: isTypeIfcSpace, HasProperties: hasProp };
                            }
                            types[relTypeId].ChildrenElements = [...types[relTypeId].ChildrenElements, ...children];
                        }
                    }
                }
            }
        }
        if (!ifcSpacesFragmentsMap.get(modelKey)) {
            ifcSpacesFragmentsMap.set(modelKey, {});
        }
        for (let [key, fid] of model.keyFragments) {
            for (let eid of allFragmentsMap[fid]) {
                if (ifcSpacesList.has(eid)) {
                    if (!ifcSpacesFragmentsMap.get(modelKey)[fid]) {
                        ifcSpacesFragmentsMap.get(modelKey)[fid] = new Set();
                    }
                    ifcSpacesFragmentsMap.get(modelKey)[fid].add(eid);
                }
            }
        }
    }
    await dotNetObj.invokeMethodAsync('UpdateTypesStructures', Object.values(ifcTypes), Object.values(superTypes));
    await refreshHiddenTransparent(true);
}
async function extractGroups() {
    for (let [modelKey, model] of fragmentsManager.groups) {
        let groups = {};
        let typeId;
        typeId = 2706460486; //IFCGROUP
        let propTypeName = TO_Core.IfcCategoryMap[typeId] ?? "";
        let prop = await model.getAllPropertiesOfType(typeId);
        extractParentGroups(modelKey, prop, groups, typeId, propTypeName);
        typeId = 1033361043; //IFCZONE
        propTypeName = TO_Core.IfcCategoryMap[typeId] ?? "";
        prop = await model.getAllPropertiesOfType(typeId);
        extractParentGroups(modelKey, prop, groups, typeId, propTypeName);
        typeId = 2254336722; //IFCSYSTEM
        propTypeName = TO_Core.IfcCategoryMap[typeId] ?? "";
        prop = await model.getAllPropertiesOfType(typeId);
        extractParentGroups(modelKey, prop, groups, typeId, propTypeName);
        typeId = 3205830791; //IFCDISTRIBUTIONSYSTEM
        propTypeName = TO_Core.IfcCategoryMap[typeId] ?? "";
        prop = await model.getAllPropertiesOfType(typeId);
        extractParentGroups(modelKey, prop, groups, typeId, propTypeName);
        typeId = 3460190687; //IFCASSET
        propTypeName = TO_Core.IfcCategoryMap[typeId] ?? "";
        prop = await model.getAllPropertiesOfType(typeId);
        extractParentGroups(modelKey, prop, groups, typeId, propTypeName);
        typeId = 1307041759; //IFCRELASSIGNSTOGROUP
        prop = await model.getAllPropertiesOfType(typeId);
        if (prop != null) {
            for (let currentObject of Object.values(prop)) {
                const groupId = currentObject["RelatingGroup"]["value"];
                if (groupId) {
                    let group = groups[groupId];
                    if (group == null) {
                        console.log("DEBUG - Group not found");
                    }
                    else {
                        const relObjs = currentObject["RelatedObjects"];
                        let children = new Array(relObjs.length);
                        for (let i = 0; i < relObjs.length; i++) {
                            const objId = relObjs[i]["value"];
                            let objName = objId.toString();
                            let objGlobalId = null;
                            const prop = await model.getProperties(objId);
                            if (prop) {
                                const name = prop["Name"];
                                if (name) {
                                    objName = name["value"];
                                }
                                objGlobalId = prop["GlobalId"]["value"];
                                const typeId = Number(prop["type"]);
                                const typeName = TO_Core.IfcCategoryMap[typeId] ?? "";
                                const isFrag = isFragment(modelKey, objId);
                                const isIfc = isIfcSpace(typeId);
                                const hasProp = await hasGeneralProperties(modelKey, objId);
                                children[i] = { ID: { ifcModelKey: modelKey, expressID: objId }, Name: objName, GlobalId: objGlobalId, TypeExpressID: typeId, TypeName: typeName, IsFragment: isFrag, IsIfcSpace: isIfc, Hasproperties: hasProp };
                            }
                        }
                        groups[groupId].ChildrenElements = [...groups[groupId].ChildrenElements, ...children];
                    }
                }
            }
        }
        await dotNetObj.invokeMethodAsync('UpdateGroups', Object.values(groups));
    }
    // E. Caberlotto 2024-03-27
    // Codice funzionante, commentato in attesa di decisioni sul come rappresentare le relazioni sistemi-edifici.
    //case 366585022:
    //    //IFCRELSERVICESBUILDINGS
    //    const systemId2: number = currentObject["RelatingSystem"]["value"];
    //    if (systemId2 !== undefined && systemId2 !== null) {
    //        let system = systems[systemId2]
    //        if (system == null) {
    //            console.log("DEBUG - System not found")
    //        }
    //        else {
    //            let systemName: string = system["Name"];
    //            const relObjs = currentObject["RelatedBuildings"];
    //            let buildings = new Array(relObjs.length)
    //            for (let i = 0; i < relObjs.length; i++) {
    //                const objId: number = relObjs[i]["value"];
    //                let objName: string = objId.toString();
    //                const prop = model.getProperties(objName]
    //                if (prop != null) {
    //                    const name = prop["Name"];
    //                    if (name !== undefined) {
    //                        objName = name["value"];
    //                    }
    //                }
    //                const typeId = Number(prop["type"]);
    //                const typeName: string = TO_Core.IfcCategoryMap[String(typeId)] ?? "";
    //                const isFrag: boolean = isFragment(objId);
    //                buildings[i] = { expressID: objId, Name: objName, TypeExpressID: typeId, TypeName: typeName, IsFragment: isFrag, IsIfcSpace: };
    //            }
    //            systems[systemId2].Buildings = [...systems[systemId2].Buildings, ...buildings];
    //        }
    //    }
    //    break;
    //}
    //}
}
async function extractParentGroups(modelKey, prop, dest, typeId, typeName) {
    if (prop != null && dest != null) {
        for (let currentObject of Object.values(prop)) {
            if (currentObject["ObjectType"] !== undefined) {
                const expressId = Number(currentObject["expressID"]);
                let name = expressId.toString();
                if (currentObject["Name"] !== undefined && currentObject["Name"] !== null) {
                    name = currentObject["Name"]["value"];
                }
                let globalId = currentObject["GlobalId"]["value"];
                const isFrag = isFragment(modelKey, expressId);
                const isIfc = isIfcSpace(typeId);
                const hasProp = await hasGeneralProperties(modelKey, expressId);
                dest[expressId] = { ID: { ifcModelKey: modelKey, expressID: expressId }, Name: name, GlobalId: globalId, IsFragment: isFrag, IsIfcSpace: isIfc, HasProperties: hasProp, TypeName: typeName, ChildrenElements: new Array(), Buildings: new Array() };
            }
        }
    }
}
let fragmentsBytes;
async function getFragmentsInfo(groupIndex) {
    const group = [...fragmentsManager.groups.values()][groupIndex];
    const rawFragments = fragmentsManager.export(group);
    fragmentsBytes = await gzip(rawFragments);
    return fragmentsBytes.length;
}
function getFragmentsChunk(startIndex, endIndex) {
    const retVal = ByteBase64.bytesToBase64(fragmentsBytes.slice(startIndex, endIndex));
    return retVal;
}
//finish transition and clean up the string
async function clearFragmentsExport() {
    fragmentsBytes = new Uint8Array();
}
let propertiesBytes;
async function getPropertiesInfo(groupIndex) {
    const group = [...fragmentsManager.groups.values()][groupIndex];
    const rawProperties = JSON.stringify(group.getLocalProperties());
    propertiesBytes = await gzip(rawProperties);
    return propertiesBytes.length;
}
function getPropertiesChunk(startIndex, endIndex) {
    const retVal = ByteBase64.bytesToBase64(propertiesBytes.slice(startIndex, endIndex));
    return retVal;
}
//finish transition and clean up the string
async function clearPropertiesExport() {
    propertiesBytes = new Uint8Array();
}
function decodeIfcType(key) {
    return TO_Core.IfcCategoryMap[key];
}
async function resetZoom() {
    let prevRenderer = false;
    if (world.renderer) {
        let previous = world.renderer.enabled;
        world.renderer.enabled = true;
    }
    let boundingBoxer = viewer.get(TO_Core.BoundingBoxer);
    boundingBoxer.addFragmentIdMap(shownFragmentsFilteredMap);
    const bBoxSphere = boundingBoxer.getSphere();
    let prevCamera = world.camera.controls.enabled;
    world.camera.controls.enabled = true;
    world.camera.controls.minDistance = 0.01;
    let res = await world.camera.controls.fitToSphere(bBoxSphere, true);
    world.camera.controls.minDistance = 50;
    world.camera.controls.enabled = prevCamera;
    if (world.renderer) {
        world.renderer.update();
        world.renderer.enabled = prevRenderer;
    }
    boundingBoxer.reset();
    world.renderer?.update();
}
async function isolate(resetHidden) {
    if (Object.keys(highlightedFragmentsMap).length == 0)
        return;
    if (resetHidden) {
        shownFragmentsMap = highlightedFragmentsMap;
    }
    else {
        for (let fid of Object.keys(shownFragmentsMap)) {
            if (highlightedFragmentsMap[fid]) {
                for (let eid of shownFragmentsMap[fid]) {
                    if (!highlightedFragmentsMap[fid].has(eid)) {
                        shownFragmentsMap[fid].delete(eid);
                    }
                }
                if (shownFragmentsMap[fid].size == 0) {
                    delete shownFragmentsMap[fid];
                }
            }
            else {
                delete shownFragmentsMap[fid];
            }
        }
    }
    await refreshHiddenTransparent(true);
    await resetZoom();
}
async function isolateMany(itemIDs, resetHidden) {
    let geomFound = false;
    for (const iid of itemIDs) {
        if (await (hasGeneralProperties(iid.ifcModelKey, iid.expressID))) {
            geomFound = true;
            break;
        }
    }
    if (!geomFound)
        return;
    if (resetHidden) {
        shownFragmentsMap = {};
        for (let id of itemIDs) {
            const fids = itemModelFragmentsMap.get(id.expressID)?.get(id.ifcModelKey);
            if (fids) {
                for (const fid of fids) {
                    if (!shownFragmentsMap[fid]) {
                        shownFragmentsMap[fid] = new Set();
                    }
                    shownFragmentsMap[fid].add(id.expressID);
                }
            }
        }
    }
    else {
        let shownFragmentsMapNew = {};
        for (const id of itemIDs) {
            const fids = itemModelFragmentsMap.get(id.expressID)?.get(id.ifcModelKey);
            if (fids) {
                for (const fid of fids) {
                    if (shownFragmentsMap[fid]?.has(id.expressID)) {
                        if (!shownFragmentsMapNew[fid]) {
                            shownFragmentsMapNew[fid] = new Set();
                        }
                        shownFragmentsMapNew[fid].add(id.expressID);
                    }
                }
            }
        }
        shownFragmentsMap = shownFragmentsMapNew;
    }
    await refreshHiddenTransparent(true);
    await resetZoom();
}
async function isolateOrthogonal() {
    await isolate(false);
    //const fid = Object.keys(shownFragmentsMap)[0];
    //const eid = [...shownFragmentsMap[fid]][0];
    //const fragment = fragmentsManager.list.get(fid);
    ///*    if (fragment) {*/
    //const instances = fragment!.itemToInstances.get(eid)!
    //const instId = [...instances][0];
    //let mesh: THREE.InstancedMesh = fragment!.mesh
    //let matrix = new THREE.Matrix4();
    //mesh.getMatrixAt(instId, matrix)
    //const originalBoundingBox = mesh.geometry.boundingBox!.clone();
    //const points = [
    //    new THREE.Vector3(originalBoundingBox.min.x, originalBoundingBox.min.y, originalBoundingBox.min.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.min.x, originalBoundingBox.min.y, originalBoundingBox.max.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.min.x, originalBoundingBox.max.y, originalBoundingBox.min.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.min.x, originalBoundingBox.max.y, originalBoundingBox.max.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.max.x, originalBoundingBox.min.y, originalBoundingBox.min.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.max.x, originalBoundingBox.min.y, originalBoundingBox.max.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.max.x, originalBoundingBox.max.y, originalBoundingBox.min.z).applyMatrix4(matrix),
    //    new THREE.Vector3(originalBoundingBox.max.x, originalBoundingBox.max.y, originalBoundingBox.max.z).applyMatrix4(matrix)];
    //let transformedBoundingBox = new THREE.Box3().setFromPoints(points);
    //let bboxCenter = new THREE.Vector3();
    //transformedBoundingBox.getCenter(bboxCenter);
    let boundingBoxer = viewer.get(TO_Core.BoundingBoxer);
    boundingBoxer.addFragmentIdMap(shownFragmentsMap);
    const bBox = boundingBoxer.get();
    let bBoxCenter = new THREE.Vector3();
    bBox.getCenter(bBoxCenter);
    const xSize = Math.abs(bBox.max.x - bBox.min.x);
    const ySize = Math.abs(bBox.max.y - bBox.min.y);
    const zSize = Math.abs(bBox.max.z - bBox.min.z);
    let minSize;
    let minAxis;
    if (zSize < xSize && zSize < ySize) {
        world.camera.controls.setLookAt(bBoxCenter.x, bBoxCenter.y, bBoxCenter.z - 20, bBoxCenter.x, bBoxCenter.y, bBoxCenter.z);
    }
    else if (xSize <= ySize) {
        world.camera.controls.setLookAt(bBoxCenter.x - 20, bBoxCenter.y, bBoxCenter.z, bBoxCenter.x, bBoxCenter.y, bBoxCenter.z);
    }
    else {
        world.camera.controls.setLookAt(bBoxCenter.x, bBoxCenter.y + 20, bBoxCenter.z, bBoxCenter.x, bBoxCenter.y, bBoxCenter.z);
    }
    world.camera.controls.fitToBox(bBox, true);
    //}
    boundingBoxer.reset();
}
async function hide() {
    for (let fid of Object.keys(highlightedFragmentsMap)) {
        if (Object.keys(shownFragmentsMap).includes(fid)) {
            for (let eid of highlightedFragmentsMap[fid]) {
                shownFragmentsMap[fid].delete(eid);
            }
        }
    }
    await refreshHiddenTransparent(true);
    //altrimenti resta visibile
    highlighter.clear();
}
async function hideMany(itemIDs) {
    for (const id of itemIDs) {
        const fids = itemModelFragmentsMap.get(id.expressID)?.get(id.ifcModelKey);
        if (fids) {
            for (const fid of fids) {
                shownFragmentsMap[fid]?.delete(id.expressID);
            }
        }
    }
    await refreshHiddenTransparent(true);
    //altrimenti resta visibile
    highlighter.clear();
}
async function show(refresh = true) {
    for (let fid of Object.keys(highlightedFragmentsMap)) {
        if (!Object.keys(shownFragmentsMap).includes(fid)) {
            shownFragmentsMap[fid] = new Set();
        }
        for (let eid of highlightedFragmentsMap[fid]) {
            shownFragmentsMap[fid].add(eid);
        }
    }
    if (refresh) {
        await refreshHiddenTransparent(true);
    }
}
async function showMany(itemIDs, refresh = true) {
    for (let fid of Object.keys(allFragmentsMap)) {
        for (let id of itemIDs) {
            if (allFragmentsMap[fid].has(id.expressID)) {
                if (!Object.keys(shownFragmentsMap).includes(fid)) {
                    shownFragmentsMap[fid] = new Set();
                }
                shownFragmentsMap[fid].add(id.expressID);
            }
        }
    }
    if (refresh) {
        await refreshHiddenTransparent(true);
    }
}
async function showAll() {
    shownFragmentsMap = structuredClone(allFragmentsMap);
    await refreshHiddenTransparent(true);
}
async function flipHidden() {
    const tmp = shownFragmentsMap;
    shownFragmentsMap = structuredClone(allFragmentsMap);
    for (let fid of Object.keys(tmp)) {
        if (Object.keys(shownFragmentsMap).includes(fid)) {
            for (let eid of tmp[fid]) {
                //if (!(hideIfcSpaces && ifcSpacesList.has(eid))) {
                shownFragmentsMap[fid].delete(eid);
                //}
            }
            if (shownFragmentsMap[fid].size == 0) {
                delete shownFragmentsMap[fid];
            }
        }
    }
    await refreshHiddenTransparent(true);
    //altrimenti resta visibile
    highlighter.clear();
}
async function setIfcSpaces(show) {
    hideIfcSpaces = !show;
    await refreshHiddenTransparent(true);
}
async function refreshHiddenTransparent(updateHidden = false) {
    /*    dotNetObj.invokeMethodAsync('UpdateIfcSpaces', !hideIfcSpaces)*/
    auxFragments.setAllNotTransparent(transparentOpacity); //, allFragmentsMap)
    //auxFragments.setAllNotHighlighted();
    outliner.clear("selectionOutline");
    //hider.update();
    hider.set(false);
    shownFragmentsFilteredMap = structuredClone(shownFragmentsMap);
    if (hideIfcSpaces) {
        for (let modelKey of [...ifcSpacesFragmentsMap.keys()]) {
            for (let fid of Object.keys(ifcSpacesFragmentsMap.get(modelKey))) {
                if (shownFragmentsFilteredMap[fid] != undefined) {
                    for (let eid of ifcSpacesFragmentsMap.get(modelKey)[fid]) {
                        shownFragmentsFilteredMap[fid].delete(eid);
                    }
                    if (shownFragmentsFilteredMap[fid].size == 0) {
                        delete shownFragmentsFilteredMap[fid];
                    }
                }
            }
        }
    }
    //let itemIDs = new Array<{ ifcModelKey: string, expressID: number }>();
    //for (let fid of Object.keys(shownFragmentsFilteredMap)) {
    //    const modelKey = fragmentModelMap.get(fid);
    //    if (modelKey) {
    //        for (let eid of shownFragmentsFilteredMap[fid]) {
    //            itemIDs[itemIDs.length] = { ifcModelKey: modelKey, expressID: eid };
    //        }
    //    }
    //}
    //await dotNetObj.invokeMethodAsync('UpdateShownIds', itemIDs);
    let shownFragmentsProcessedMap = structuredClone(shownFragmentsFilteredMap);
    let transparentFragmentsProcessedMap = structuredClone(transparentFragmentsMap);
    for (let fid of Object.keys(highlightedFragmentsMap)) {
        //if (Object.keys(shownFragmentsProcessedMap).includes(fid)) {
        //    for (let eid of highlightedFragmentsMap[fid]) {
        //        if (shownFragmentsProcessedMap[fid].has(eid)) {
        //            shownFragmentsProcessedMap[fid].delete(eid);
        //        }
        //    }
        //}
        if (Object.keys(transparentFragmentsProcessedMap).includes(fid)) {
            for (let eid of highlightedFragmentsMap[fid]) {
                if (transparentFragmentsProcessedMap[fid].has(eid)) {
                    transparentFragmentsProcessedMap[fid].delete(eid);
                }
            }
        }
    }
    for (let fid of Object.keys(transparentFragmentsMap)) {
        if (Object.keys(shownFragmentsProcessedMap).includes(fid)) {
            for (let eid of transparentFragmentsProcessedMap[fid]) {
                if (shownFragmentsProcessedMap[fid].has(eid)) {
                    shownFragmentsProcessedMap[fid].delete(eid);
                }
                else {
                    transparentFragmentsProcessedMap[fid].delete(eid);
                }
            }
        }
        else {
            delete transparentFragmentsProcessedMap[fid];
        }
    }
    if (Object.keys(transparentFragmentsProcessedMap).length > 0) {
        auxFragments.setTransparencyByIdMap(transparentOpacity, transparentFragmentsProcessedMap, true);
    }
    if (Object.keys(highlightedFragmentsMap).length > 0) {
        //auxFragments.setHighlightingByIdMap(highlightedFragmentsMap, true);
        outliner.add("selectionOutline", highlightedFragmentsMap);
    }
    if (Object.keys(shownFragmentsProcessedMap).length > 0) {
        hider.set(true, shownFragmentsProcessedMap);
    }
    //let hasHiddenObjects = false;
    //for (let fid in allFragmentsMap) {
    //    if (!shownFragmentsMap[fid] || shownFragmentsMap[fid].size < allFragmentsMap[fid].size) {
    //        hasHiddenObjects = true;
    //        break;
    //    }
    //}
    let highlightedObjectsList = new Set();
    for (const fid of Object.keys(highlightedFragmentsMap)) {
        for (const eid of highlightedFragmentsMap[fid]) {
            highlightedObjectsList.add(eid);
        }
    }
    highlightedObjectsCounter = highlightedObjectsList.size;
    if (updateHidden) {
        let hiddenObjects = [];
        for (let fid in allFragmentsMap) {
            if (!shownFragmentsMap[fid]) {
                hiddenObjects.push(...allFragmentsMap[fid]);
            }
            else if (shownFragmentsMap[fid].size < allFragmentsMap[fid].size) {
                for (let eid of allFragmentsMap[fid]) {
                    if (!shownFragmentsMap[fid].has(eid))
                        hiddenObjects.push(eid);
                }
            }
        }
        dotNetObj.invokeMethod("UpdateHasHiddenObjects", hiddenObjects);
    }
    //culler.needsUpdate = true;
    refreshView();
}
let clipperMode = 0;
async function toggleClipper(enabled = undefined) {
    const previous = clipper.enabled;
    if (enabled == undefined) {
        clipper.enabled = !clipper.enabled;
    }
    else {
        clipper.enabled = enabled;
    }
    //    await onToggledClipper();
    //}
    //async function onToggledClipper() {
    clipperMode = 0;
    if (previous != clipper.enabled) {
        //if (clipper.enabled) {
        //    toggleLengthMeasure(false);
        //    toggleAreaMeasure(false);
        //    toggleAngleMeasure(false);
        //}
        //else {
        //}
        refreshHighlighterAndCameraState();
    }
    await dotNetObj.invokeMethodAsync("UpdateClipping", clipper.enabled, clipperMode);
}
async function createClip() {
    clipperMode = clipperMode > 0 ? 0 : 1;
    await dotNetObj.invokeMethodAsync("UpdateClipping", clipper.enabled, clipperMode);
}
async function deleteClip() {
    clipperMode = clipperMode < 0 ? 0 : -1;
    await dotNetObj.invokeMethodAsync("UpdateClipping", clipper.enabled, clipperMode);
}
async function deleteAllClips() {
    clipper.deleteAll();
    clipperMode = 0;
    await dotNetObj.invokeMethodAsync("UpdateClipping", clipper.enabled, clipperMode);
}
//async function disableClipping() {
//    clipper.enabled = false;
//    onToggledClipper();
//}
async function endClippingAction() {
    clipperMode = 0;
    await dotNetObj.invokeMethodAsync("UpdateClipping", clipper.enabled, clipperMode);
}
async function clip() {
    if (clipperMode == 0) {
        return;
    }
    if (clipperMode > 0) {
        clipper.create(world);
    }
    else if (clipperMode < 0) {
        clipper.delete(world);
    }
    endClippingAction();
}
let lengthMeasureMode = 0;
async function toggleLengthMeasure(enabled = undefined) {
    const previous = lengthMeasurement.enabled;
    if (enabled == undefined) {
        lengthMeasurement.enabled = !lengthMeasurement.enabled;
    }
    else {
        lengthMeasurement.enabled = enabled;
    }
    if (previous != lengthMeasurement.enabled) {
        if (lengthMeasurement.enabled) {
            //await toggleClipper(false); //await disableClipping();
            await toggleAreaMeasure(false); //await disableAreaMeasure();
            await toggleAngleMeasure(false);
            lengthMeasureMode = 1;
        }
        else {
            lengthMeasureMode = 0;
        }
        refreshHighlighterAndCameraState();
        await dotNetObj.invokeMethodAsync("UpdateLengthMeasurement", lengthMeasureMode);
    }
}
async function deleteLengthMeasure() {
    lengthMeasureMode = -1;
    await dotNetObj.invokeMethodAsync("UpdateLengthMeasurement", lengthMeasureMode);
}
async function deleteAllLengthMeasures() {
    lengthMeasurement.deleteAll();
    await endDeleteLengthMeasure();
}
async function endDeleteLengthMeasure() {
    lengthMeasureMode = lengthMeasurement.enabled ? 1 : 0;
    await dotNetObj.invokeMethodAsync("UpdateLengthMeasurement", lengthMeasureMode);
}
//async function disableLengthMeasure() {
//    lengthMeasurement.enabled = false;
//    await onToggledLengthMeasure();
//}
async function measureLength() {
    if (lengthMeasureMode == 0) {
        return;
    }
    if (lengthMeasureMode > 0) {
        lengthMeasurement.create();
    }
    else if (lengthMeasureMode < 0) {
        lengthMeasurement.delete();
        await endDeleteLengthMeasure();
    }
}
let areaMeasureMode = 0;
async function toggleAreaMeasure(enabled = undefined) {
    const previous = areaMeasurement.enabled;
    if (enabled == undefined) {
        areaMeasurement.enabled = !areaMeasurement.enabled;
    }
    else {
        areaMeasurement.enabled = enabled;
    }
    if (previous != areaMeasurement.enabled) {
        if (areaMeasurement.enabled) {
            //await toggleClipper(false);  //await disableClipping();
            await toggleLengthMeasure(false); // await disableLengthMeasure();
            await toggleAngleMeasure(false);
            areaMeasureMode = 1;
        }
        else {
            //renderer.container.oncontextmenu = null;
            if (areaMeasureMode > 0) {
                areaMeasurement.endCreation();
            }
            areaMeasureMode = 0;
        }
        refreshHighlighterAndCameraState();
        await dotNetObj.invokeMethodAsync("UpdateAreaMeasurement", areaMeasureMode);
    }
}
//async function deleteAreaMeasure() {
//    areaMeasureMode = -1;
//    await dotNetObj.invokeMethodAsync("UpdateAreaMeasurement", areaMeasureMode);
//}
async function deleteAllAreaMeasures() {
    areaMeasurement.deleteAll();
    await endDeleteAreaMeasure();
}
async function endDeleteAreaMeasure() {
    areaMeasureMode = areaMeasurement.enabled ? 1 : 0;
    await dotNetObj.invokeMethodAsync("UpdateAreaMeasurement", areaMeasureMode);
}
async function measureArea() {
    if (areaMeasureMode == 0) {
        return;
    }
    if (areaMeasureMode > 0) {
        areaMeasurement.create();
    }
    else if (areaMeasureMode < 0) {
        areaMeasurement.delete();
        await endDeleteAreaMeasure();
    }
}
let angleMeasureMode = 0;
async function toggleAngleMeasure(enabled = undefined) {
    const previous = angleMeasurement.enabled;
    if (enabled == undefined) {
        angleMeasurement.enabled = !angleMeasurement.enabled;
    }
    else {
        angleMeasurement.enabled = enabled;
    }
    if (previous != angleMeasurement.enabled) {
        if (angleMeasurement.enabled) {
            //await toggleClipper(false);  //await disableClipping();
            await toggleLengthMeasure(false); // await disableLengthMeasure();
            await toggleAreaMeasure(false);
            angleMeasureMode = 1;
        }
        else {
            //renderer.container.oncontextmenu = null;
            if (angleMeasureMode > 0) {
                angleMeasurement.endCreation();
            }
            angleMeasureMode = 0;
        }
        refreshHighlighterAndCameraState();
        await dotNetObj.invokeMethodAsync("UpdateAngleMeasurement", angleMeasureMode);
    }
}
//async function deleteAngleMeasure() {
//    angleMeasureMode = -1;
//    await dotNetObj.invokeMethodAsync("UpdateAngleMeasurement", angleMeasureMode);
//}
async function deleteAllAngleMeasures() {
    angleMeasurement.deleteAll();
    await endDeleteAngleMeasure();
}
async function endDeleteAngleMeasure() {
    angleMeasureMode = angleMeasurement.enabled ? 1 : 0;
    await dotNetObj.invokeMethodAsync("UpdateAngleMeasurement", angleMeasureMode);
}
async function measureAngle() {
    if (angleMeasureMode == 0) {
        return;
    }
    if (angleMeasureMode > 0) {
        angleMeasurement.create();
    }
    else if (angleMeasureMode < 0) {
        angleMeasurement.delete();
        await endDeleteAngleMeasure();
    }
}
function refreshHighlighterAndCameraState() {
    //highlighter.enabled = !(clipper.enabled || lengthMeasurement.enabled || areaMeasurement.enabled || angleMeasurement.enabled);
    highlighter.enabled = !(lengthMeasurement.enabled || areaMeasurement.enabled || angleMeasurement.enabled);
    world.camera.controls.enabled = highlighter.enabled;
    if (world.renderer) {
        world.renderer.enabled = true;
        world.renderer.update();
        world.renderer.enabled = clipper.enabled || !highlighter.enabled;
    }
}
function extractAllFragmentsMap() {
    allFragmentsMap = {};
    modelFragmentItemsMap = new Map();
    modelItemsInFragmentMap = new Map();
    fragmentModelMap = new Map();
    itemModelFragmentsMap = new Map();
    //ricalcolo la mappa di tutto...
    for (let [groupKey, group] of fragmentsManager.groups) {
        let itemsInFragments = new Array();
        for (let kvp of group.data) {
            //let idNum: number = kvp[0];
            //const keys = group.data[idNum][0];
            const keys = kvp[1][0];
            for (let i = 0; i < keys.length; i++) {
                const fragKey = keys[i];
                const fragID = group.keyFragments.get(fragKey) ?? "";
                //console.log(fragID);
                const fragment = fragmentsManager.list.get(fragID);
                if (fragment) {
                    fragmentModelMap.set(fragID, groupKey);
                    if (!allFragmentsMap[fragID]) {
                        allFragmentsMap[fragID] = new Set();
                    }
                    for (let eid of fragment.ids) {
                        allFragmentsMap[fragID].add(eid);
                        itemsInFragments[itemsInFragments.length] = eid;
                        if (!itemModelFragmentsMap.get(eid)) {
                            itemModelFragmentsMap.set(eid, new Map());
                        }
                        if (!itemModelFragmentsMap.get(eid).get(groupKey)) {
                            itemModelFragmentsMap.get(eid).set(groupKey, new Set());
                        }
                        itemModelFragmentsMap.get(eid).get(groupKey).add(fragID);
                        if (!modelFragmentItemsMap.get(groupKey)) {
                            modelFragmentItemsMap.set(groupKey, new Map());
                        }
                        if (!modelFragmentItemsMap.get(groupKey).get(fragID)) {
                            modelFragmentItemsMap.get(groupKey).set(fragID, new Set());
                        }
                        modelFragmentItemsMap.get(groupKey).get(fragID).add(eid);
                    }
                }
                //if (fragment.composites != undefined) {
                //    const composites = fragment.composites[idNum];
                //    if (composites) {
                //        for (let i = 1; i < composites; i++) {
                //            const compositeID = toCompositeID(idNum, i);
                //            allFragmentsMap[fragment.mesh.fragment.id].add(compositeID);
                //        }
                //    }
                //}
                //    if (itemExpressIDs.length > 0) {
                //        /*itemExpressIDs.sort(function (a, b) { return a - b });*/
                //        if (!modelFragmentItemsMap.get(groupKey)) {
                //            modelFragmentItemsMap.set(groupKey, new Map<string, Set<number>>());
                //        }
                //        modelFragmentItemsMap.get(groupKey)!.set(fragID, new Set<number>(itemExpressIDs));
                //    }
            }
        }
        itemsInFragments.sort(function (a, b) { return a - b; });
        modelItemsInFragmentMap.set(groupKey, new Set(itemsInFragments));
    }
}
function getFragmentsMap(fragmentID) {
    let fragmentMap = {};
    if (Object.keys(allFragmentsMap).includes(fragmentID)) {
        fragmentMap[fragmentID] = structuredClone(allFragmentsMap[fragmentID]);
    }
    return fragmentMap;
}
//function getFragmentMap(fragmentID: string, expressID: number): TO_Frag.FragmentIdMap {
//    let fragmentMap: TO_Frag.FragmentIdMap = {};
//    fragmentMap[fragmentID] = new Set<number>([expressID]);
//    return fragmentMap;
//}
async function Timeout(ms) {
    await new Promise(resolve => setTimeout(resolve, ms));
}
let disposedRegistry;
async function disposeViewer() {
    if (viewer) {
        const disposer = await viewer.get(TO_Core.Disposer);
        world?.renderer?.three.domElement.removeEventListener("contextmenu", e => onContextMenu(e));
        world?.renderer?.three.domElement.removeEventListener("mousedown", e => onMouseDown(e));
        world?.renderer?.three.domElement.removeEventListener("mousemove", e => onMouseMove(e));
        world?.renderer?.three.domElement.removeEventListener("mouseup", () => onPointerUp());
        world?.renderer?.three.domElement.removeEventListener("touchstart", e => onTouchStart(e));
        world?.renderer?.three.domElement.removeEventListener("touchmove", e => onTouchMove(e));
        world?.renderer?.three.domElement.removeEventListener("touchend", () => onPointerUp());
        world?.camera?.controls.removeEventListener("controlstart", onCameraControlStart);
        world?.camera?.controls.removeEventListener("controlend", onCameraControlEnd);
        world?.camera?.controls.removeEventListener("wake", onCameraWake);
        world?.camera?.controls.removeEventListener("sleep", onCameraSleep);
        allFragmentsMap = {};
        highlightedFragmentsMap = {};
        transparentFragmentsMap = {};
        shownFragmentsMap = {};
        shownFragmentsFilteredMap = {};
        fragmentModelMap?.clear();
        ifcSpacesFragmentsMap?.clear();
        itemModelFragmentsMap?.clear();
        modelFragmentItemsMap?.clear();
        modelItemsInFragmentMap?.clear();
        //highlightedNoFragmentList?.clear();
        auxFragments?.disposeAllOpacityLevels(disposer);
        if (world) {
            for (const mesh of world.meshes) {
                disposer.destroy(mesh);
            }
            world.meshes.clear();
        }
        if (camTargetMesh) {
            disposer.destroy(camTargetMesh);
            camTargetMesh = undefined;
        }
        disposedRegistry = new Array;
        disposedRegistry.push(new ItemDisposedInfo("outliner", outliner));
        disposedRegistry.push(new ItemDisposedInfo("highlighter", highlighter));
        disposedRegistry.push(new ItemDisposedInfo("ifcLoader", ifcLoader));
        disposedRegistry.push(new ItemDisposedInfo("worlds", worlds));
        disposedRegistry.push(new ItemDisposedInfo("fragmentsManager", fragmentsManager));
        disposedRegistry.push(new ItemDisposedInfo("indexer", indexer));
        disposedRegistry.push(new ItemDisposedInfo("casters", raycasters));
        disposedRegistry.push(new ItemDisposedInfo("cullers", cullers));
        disposedRegistry.push(new ItemDisposedInfo("clipper", clipper));
        for (const item of disposedRegistry) {
            if (item.component && item.component.isDisposeable && !item.disposed) {
                item.component.onDisposed.add(() => onDisposedComponent(item));
                item.component?.dispose();
            }
            else {
                item.disposed = true;
            }
        }
    }
}
let count = 0;
async function onDisposedComponent(item) {
    if (!item.disposed) {
        item.disposed = true;
        item.component?.onDisposed.remove(() => onDisposedComponent(item));
        for (const comp of viewer.list) {
            if (comp[1] == item.component) {
                viewer.list.delete(comp[0]);
            }
        }
    }
    let go = true;
    for (const item of disposedRegistry) {
        go && (go = item.disposed);
    }
    if (go) {
        disposedRegistry = [];
        dotNetObj.dispose();
        viewer.dispose();
    }
}
class ItemDisposedInfo {
    constructor(name, component, disposed = false) {
        Object.defineProperty(this, "name", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: ""
        });
        Object.defineProperty(this, "component", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "disposed", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: false
        });
        this.name = name;
        this.component = component;
        this.disposed = disposed;
    }
}
function isTouchDevice() {
    return ("ontouchstart" in window) && ("ontouchend" in window);
}
//function download(file: File) {
//    const link = document.createElement('a');
//    link.href = URL.createObjectURL(file);
//    link.download = file.name;
//    document.body.appendChild(link);
//    link.click();
//    link.remove();
//}
//function numberOfDigits(x: number) {
//    return Math.max(Math.floor(Math.log10(Math.abs(x))), 0) + 1;
//}
//function toCompositeID(id: number, count: number) :number {
//    const factor = 0.1 ** numberOfDigits(count);
//    id += count * factor;
//    return id;
//    //let idString = id.toString();
//    //// add missing zeros
//    //if (count % 10 === 0) {
//    //    for (let i = 0; i < factor; i++) {
//    //        idString += "0";
//    //    }
//    //}
//    //return idString;
//}
//# sourceMappingURL=thatOpen.mjs.map