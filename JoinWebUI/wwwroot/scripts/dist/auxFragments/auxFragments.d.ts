import * as OBC from "@thatopen/components";
import * as FRAG from "@thatopen/fragments";
import * as THREE from 'three';
export declare class AuxFragments {
    private world;
    private transparentFragments;
    private highlightedFragments;
    constructor(world: OBC.SimpleWorld);
    get(): AuxFragments;
    /**
     * Creates a transparent copy of each Fragment, with the selected opacity level
     * @param opacityLevel the opacity level
     * @returns
     */
    createTransparentFragment(baseFragment: FRAG.Fragment, modelPos: THREE.Vector3, opacityLevel: number): void;
    /**
     * Creates a transparent copy or a material
     * @param material the source material
     * @param opacityLevel the opacity level
     * @returns
     */
    private copyToTransparentMaterial;
    /**
     * Changes visibility of transparent elements.
     * Visibility of base elements shall be managed elsewhere, as it may depend upon other circumstances.
     * @param opacityLevel the opacity level to be applied
     * @param fragmentIdMap elements to act upon
     * @param transparent whether the transparent elements shall be visible or not
     */
    setTransparencyByIdMap(opacityLevel: number, fragmentIdMap: FRAG.FragmentIdMap, transparent: boolean): void;
    /**
     * Hides all transparent objects. Corresponding opaque objects are not shown automatically, as they might be hidden for other reasons.
     * @param opacityLevel the opacity level to be hidden
     */
    setAllNotTransparent(opacityLevel: number): void;
    disposeOpacityLevel(disposer: OBC.Disposer, opacityLevel: number): void;
    disposeAllOpacityLevels(disposer: OBC.Disposer): void;
    /**
 * Creates a highlighted copy of a Fragment
 * @returns
 */
    createHighlightedFragment(baseFragment: FRAG.Fragment, modelPos: THREE.Vector3): void;
    /**
     * Creates a highlighted copy or a material
     * @param material the source material
     * @returns
     */
    private copyToHighlightingMaterial;
    /**
 * Changes visibility of transparent elements.
 * Visibility of base elements shall be managed elsewhere, as it may depend upon other circumstances.
 * @param fragmentIdMap elements to act upon
 * @param highlighted whether the highlighted elements shall be visible or not
 */
    setHighlightingByIdMap(fragmentIdMap: FRAG.FragmentIdMap, highlighted: boolean): void;
    /**
     * Hides all highlighted objects. Corresponding opaque objects are not shown automatically, as they might be hidden for other reasons.
     */
    setAllNotHighlighted(): void;
}
//# sourceMappingURL=auxFragments.d.ts.map