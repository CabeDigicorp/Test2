import * as flatbuffers from "flatbuffers";
import { Fragment } from "../../fragments/index/fragment.js";
export declare class Fragments {
    bb: flatbuffers.ByteBuffer | null;
    bb_pos: number;
    __init(i: number, bb: flatbuffers.ByteBuffer): Fragments;
    static getRootAsFragments(bb: flatbuffers.ByteBuffer, obj?: Fragments): Fragments;
    static getSizePrefixedRootAsFragments(bb: flatbuffers.ByteBuffer, obj?: Fragments): Fragments;
    items(index: number, obj?: Fragment): Fragment | null;
    itemsLength(): number;
    static startFragments(builder: flatbuffers.Builder): void;
    static addItems(builder: flatbuffers.Builder, itemsOffset: flatbuffers.Offset): void;
    static createItemsVector(builder: flatbuffers.Builder, data: flatbuffers.Offset[]): flatbuffers.Offset;
    static startItemsVector(builder: flatbuffers.Builder, numElems: number): void;
    static endFragments(builder: flatbuffers.Builder): flatbuffers.Offset;
    static finishFragmentsBuffer(builder: flatbuffers.Builder, offset: flatbuffers.Offset): void;
    static finishSizePrefixedFragmentsBuffer(builder: flatbuffers.Builder, offset: flatbuffers.Offset): void;
    static createFragments(builder: flatbuffers.Builder, itemsOffset: flatbuffers.Offset): flatbuffers.Offset;
}
