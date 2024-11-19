import * as OBC
from "@thatopen/components";
//from "../../lib/@thatopen/components";
import * as FRAG
from "@thatopen/fragments";
//from "../../lib/@thatopen/fragments";
import * as THREE
from 'three';
//from "../../lib/three"

export class AuxFragments {

    private world: OBC.SimpleWorld;
    private transparentFragments: {
        [opacity: number]: Map<string, FRAG.Fragment>;
    } = {};
    private highlightedFragments: Map<string, FRAG.Fragment> | undefined;

    constructor(world: OBC.SimpleWorld) {
        this.world = world;
    }


    public get(): AuxFragments {
        return this;
    }

    ///**
    // * Creates a transparent copy of each Fragment, with the selected opacity level
    // * @param opacityLevel the opacity level
    // * @returns
    // */
    //public createTransparentFragments(baseFragments: FRAG.Fragment[], opacityLevel: number): void {
    //    if (this.transparentFragments[opacityLevel]) {
    //        return;
    //    }

    //    this.transparentFragments[opacityLevel] = new Map<string, FRAG.Fragment>();

    //    for (let frag of baseFragments) {

    //        //create transparent materials for fragment
    //        let transparentMaterials = new Array<THREE.Material>();
    //        for (const material of frag.mesh.material) {
    //            transparentMaterials[transparentMaterials.length] = this.copyToTransparentMaterial(material, opacityLevel);
    //        }

    //        //create new transparent fragment
    //        let tFrag = new FRAG.Fragment(frag.mesh.geometry, transparentMaterials, 1)

    //        //copy items
    //        let transparentItems: FRAG.Item[] = [];
    //        for (const iid of frag.ids) {
    //            const item = frag.get(iid);
    //            let tItem: FRAG.Item = { id: item.id, transforms: [], colors: [] };
    //            //tItem.id = item.id;
    //            //tItem.transforms = [];
    //            for (const m of item.transforms) {
    //                tItem.transforms![tItem.transforms!.length] = m.clone();
    //            }
    //            //tItem.colors = [];
    //            if (item.colors) {
    //                for (const c of item.colors) {
    //                    tItem.colors![tItem.colors!.length] = c.clone();
    //                }
    //            }
    //            transparentItems[transparentItems.length] = tItem;
    //        }
    //        tFrag.add(transparentItems);
    //        (tFrag.mesh as THREE.InstancedMesh).frustumCulled = false;

    //        //finalize creation
    //        tFrag.group = frag.group;
    //        tFrag.setVisibility(false);
    //        this.transparentFragments[opacityLevel].set(frag.id, tFrag);


    //        //add fragment to scene
    //        this.world.scene.three.add(tFrag.mesh);
    //        this.world.meshes.add(tFrag.mesh);
    //    }

    //}

    /**
     * Creates a transparent copy of each Fragment, with the selected opacity level
     * @param opacityLevel the opacity level
     * @returns
     */
    public createTransparentFragment(baseFragment: FRAG.Fragment, modelPos: THREE.Vector3, opacityLevel: number): void {
        if (!this.transparentFragments[opacityLevel]) {
            this.transparentFragments[opacityLevel] = new Map<string, FRAG.Fragment>();
        }

        //create transparent materials for fragment
        let transparentMaterials = new Array<THREE.Material>();
        for (const material of baseFragment.mesh.material) {
            transparentMaterials[transparentMaterials.length] = this.copyToTransparentMaterial(material, opacityLevel);
        }

        //create new transparent fragment
        let tFrag = new FRAG.Fragment(baseFragment.mesh.geometry, transparentMaterials, 1)

        //copy items
        let transparentItems: FRAG.Item[] = [];
        for (const iid of baseFragment.ids) {
            const item = baseFragment.get(iid);
            let tItem: FRAG.Item = { id: item.id, transforms: [], colors: [] };
            //tItem.id = item.id;
            //tItem.transforms = [];
            for (const m of item.transforms) {
                tItem.transforms![tItem.transforms!.length] = m.clone();
            }
            //tItem.colors = [];
            if (item.colors) {
                for (const c of item.colors) {
                    tItem.colors![tItem.colors!.length] = c.clone();
                }
            }
            transparentItems[transparentItems.length] = tItem;
        }
        tFrag.add(transparentItems);
        (tFrag.mesh as THREE.InstancedMesh).frustumCulled = false;

        //if (modelPos.x != 0 || modelPos.y != 0 || modelPos.z != 0) {
        //    (tFrag.mesh as THREE.Object3D).translateX(modelPos.x);
        //    (tFrag.mesh as THREE.Object3D).translateY(modelPos.y);
        //    (tFrag.mesh as THREE.Object3D).translateZ(modelPos.z);
        //}

        //finalize creation

        tFrag.setVisibility(false);
        this.transparentFragments[opacityLevel].set(baseFragment.id, tFrag);


        //add fragment to scene
        this.world.scene.three.add(tFrag.mesh);
        this.world.meshes.add(tFrag.mesh);
    }

    

    /**
     * Creates a transparent copy or a material
     * @param material the source material
     * @param opacityLevel the opacity level
     * @returns
     */
    private copyToTransparentMaterial(material: THREE.Material, opacityLevel: number) {
        const transparentMaterial = new THREE.MeshLambertMaterial();
        transparentMaterial.copy(material);
        transparentMaterial.opacity = opacityLevel * (material.transparent ? material.opacity : 1);
        transparentMaterial.transparent = true;
        return transparentMaterial;
    }

    /**
     * Changes visibility of transparent elements.
     * Visibility of base elements shall be managed elsewhere, as it may depend upon other circumstances.
     * @param opacityLevel the opacity level to be applied
     * @param fragmentIdMap elements to act upon
     * @param transparent whether the transparent elements shall be visible or not
     */
    public setTransparencyByIdMap(opacityLevel: number, fragmentIdMap: FRAG.FragmentIdMap, transparent: boolean) {
        if (!this.transparentFragments[opacityLevel]) {
            throw new Error("Opacity level " + opacityLevel + " not found");
        }
        for (const fragmentId in fragmentIdMap) {
            if (this.transparentFragments[opacityLevel].has(fragmentId)) {
                const ids = [...(fragmentIdMap[fragmentId])]
                this.transparentFragments[opacityLevel].get(fragmentId)!.setVisibility(transparent, ids);
            }
            else {
                throw new Error("Fragment " + fragmentId + " not found");
            }
        }
        
    }


    /**
     * Hides all transparent objects. Corresponding opaque objects are not shown automatically, as they might be hidden for other reasons.
     * @param opacityLevel the opacity level to be hidden
     */
    public setAllNotTransparent(opacityLevel: number) {
        for (const [fid, frag] of this.transparentFragments[opacityLevel]) {
            frag.setVisibility(false);
        }
    }    


    public disposeOpacityLevel(disposer: OBC.Disposer, opacityLevel: number) {
        for (const fid in this.transparentFragments[opacityLevel]) {
            let frag = this.transparentFragments[opacityLevel][fid]
            this.world.scene.three.remove(frag.mesh);
            this.world.meshes.delete(frag.mesh);
            disposer.destroy(frag.mesh);
            frag.dispose();
            delete this.transparentFragments[opacityLevel][fid];
        }
    }

    public disposeAllOpacityLevels(disposer: OBC.Disposer) {
        for (const opacity in this.transparentFragments) {
            this.disposeOpacityLevel(disposer, Number(opacity));
        }
    }


    ///**
    // * Creates a highlighted copy of each Fragment, with the selected opacity level
    // * @returns
    // */
    //public createHighlightedFragments(baseFragments: FRAG.Fragment[], fragmentsManager: OBC.FragmentsManager ): void {
    //    if (this.highlightedFragments) {
    //        return;
    //    }

    //    this.highlightedFragments = new Map<string, FRAG.Fragment>();

    //    for (let frag of baseFragments) {

    //        //create transparent materials for fragment
    //        let highlightedMaterials = new Array<THREE.Material>();
    //        for (const material of frag.mesh.material) {
    //            highlightedMaterials[highlightedMaterials.length] = this.copyToHighlightingMaterial(material)
    //        }

    //        //create new transparent fragment
    //        let hFrag = new FRAG.Fragment(frag.mesh.geometry, highlightedMaterials, 1);

    //        //copy items
    //        let highlightedItems: FRAG.Item[] = [];
    //        for (const iid of frag.ids) {
    //            const item = frag.get(iid);
    //            let tItem: FRAG.Item = { id: item.id, transforms: [], colors: [] };
    //            //tItem.id = item.id;
    //            //tItem.transforms = [];
    //            for (const m of item.transforms) {
    //                tItem.transforms![tItem.transforms!.length] = m.clone();
    //            }
    //            //tItem.colors = [];
    //            if (item.colors) {
    //                for (const c of item.colors) {
    //                    tItem.colors![tItem.colors!.length] = new THREE.Color(0xF3A401);
    //                }
    //            }
    //            highlightedItems[highlightedItems.length] = tItem;
    //        }
    //        hFrag.add(highlightedItems);
    //        (hFrag.mesh as THREE.InstancedMesh).renderOrder = Infinity;
    //        (hFrag.mesh as THREE.InstancedMesh).frustumCulled = false;

    //        //finalize creation
    //        //const matrix = (frag.group as THREE.Object3D).matrixWorld;
    //        const hFragThree = hFrag.mesh as THREE.Object3D;
    //        const fragThree = frag.mesh as THREE.Object3D;
    //        console.log("B: " + fragThree.position.x + "; " + fragThree.position.y + "; " + fragThree.position.z)
    //        console.log("H: " + hFragThree.position.x + "; " + hFragThree.position.y + "; " + hFragThree.position.z)

    //        //(hFrag.mesh as THREE.Object3D).position = (frag.mesh as THREE.Object3D).position;

    //        hFrag.setVisibility(false);
    //        this.highlightedFragments.set(frag.id, hFrag);

    //        //add fragment to scene
    //        this.world.scene.three.add(hFrag.mesh);
    //        this.world.meshes.add(hFrag.mesh);


    //    }

    //}


    /**
 * Creates a highlighted copy of a Fragment
 * @returns
 */
    public createHighlightedFragment(baseFragment: FRAG.Fragment, modelPos: THREE.Vector3): void {
        if (!this.highlightedFragments) {
            this.highlightedFragments = new Map<string, FRAG.Fragment>();     
        }
        

        //create transparent materials for fragment
        let highlightedMaterials = new Array<THREE.Material>();
        for (const material of baseFragment.mesh.material) {
            highlightedMaterials[highlightedMaterials.length] = this.copyToHighlightingMaterial(material)
        }

        //create new transparent fragment
        let hFrag = new FRAG.Fragment(baseFragment.mesh.geometry, highlightedMaterials, 1);

        //copy items
        let highlightedItems: FRAG.Item[] = [];
        for (const iid of baseFragment.ids) {
            const item = baseFragment.get(iid);
            let tItem: FRAG.Item = { id: item.id, transforms: [], colors: [] };
            //tItem.id = item.id;
            //tItem.transforms = [];
            for (const m of item.transforms) {
                tItem.transforms![tItem.transforms!.length] = m.clone();
            }
            //tItem.colors = [];
            if (item.colors) {
                for (const c of item.colors) {
                    tItem.colors![tItem.colors!.length] = new THREE.Color(0xF3A401);
                }
            }
            highlightedItems[highlightedItems.length] = tItem;
        }
        hFrag.add(highlightedItems);
        (hFrag.mesh as THREE.InstancedMesh).renderOrder = Infinity;
        (hFrag.mesh as THREE.InstancedMesh).frustumCulled = false;

        if (modelPos.x != 0 || modelPos.y != 0 || modelPos.z != 0) {
            (hFrag.mesh as THREE.Object3D).translateX(modelPos.x);
            (hFrag.mesh as THREE.Object3D).translateY(modelPos.y);
            (hFrag.mesh as THREE.Object3D).translateZ(modelPos.z);
        }

        //finalize creation
        
        hFrag.setVisibility(false);
        this.highlightedFragments.set(baseFragment.id, hFrag);

        //add fragment to scene
        this.world.scene.three.add(hFrag.mesh);
        this.world.meshes.add(hFrag.mesh);
        
    }


    /**
     * Creates a highlighted copy or a material
     * @param material the source material
     * @returns
     */
    private copyToHighlightingMaterial(material: THREE.Material) {
        let highlightingMaterial: THREE.Material = new THREE.MeshLambertMaterial();
        highlightingMaterial.copy(material);
        highlightingMaterial.depthTest = false;
        //deve sempre essere dichiarato trasparente, altrimenti viene visualizzato comunque dietro gli altri oggetti trasparenti
        highlightingMaterial.opacity = 0.9 * (material.transparent ? material.opacity : 1);
        //if (!material.transparent) {
        //    highlightingMaterial.opacity = 1;
        //}
        highlightingMaterial.transparent = true;
        return highlightingMaterial;
    }

    /**
 * Changes visibility of transparent elements.
 * Visibility of base elements shall be managed elsewhere, as it may depend upon other circumstances.
 * @param fragmentIdMap elements to act upon
 * @param highlighted whether the highlighted elements shall be visible or not
 */
    public setHighlightingByIdMap(fragmentIdMap: FRAG.FragmentIdMap, highlighted: boolean) {
        if (!this.highlightedFragments) {
            throw new Error("highlightedFragments not found");
        }
        for (const fragmentId in fragmentIdMap) {
            if (this.highlightedFragments.has(fragmentId)) {
                const ids = [...(fragmentIdMap[fragmentId])]
                const frag = this.highlightedFragments.get(fragmentId);
                frag?.setVisibility(highlighted, ids);
            }
            else {
                throw new Error("Fragment " + fragmentId + " not found");
            }
        }

    }


    /**
     * Hides all highlighted objects. Corresponding opaque objects are not shown automatically, as they might be hidden for other reasons.
     */
    public setAllNotHighlighted() {
        if (this.highlightedFragments) {
            for (const [fid, frag] of this.highlightedFragments) {
                frag.setVisibility(false);
            }
        }
    }    

}
