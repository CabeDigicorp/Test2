import * as flatbuffers from "flatbuffers";
import { Alignment } from "../../fragments/index/alignment.js";
export declare class Civil {
    bb: flatbuffers.ByteBuffer | null;
    bb_pos: number;
    __init(i: number, bb: flatbuffers.ByteBuffer): Civil;
    static getRootAsCivil(bb: flatbuffers.ByteBuffer, obj?: Civil): Civil;
    static getSizePrefixedRootAsCivil(bb: flatbuffers.ByteBuffer, obj?: Civil): Civil;
    alignmentHorizontal(obj?: Alignment): Alignment | null;
    alignmentVertical(obj?: Alignment): Alignment | null;
    alignment3d(obj?: Alignment): Alignment | null;
    static startCivil(builder: flatbuffers.Builder): void;
    static addAlignmentHorizontal(builder: flatbuffers.Builder, alignmentHorizontalOffset: flatbuffers.Offset): void;
    static addAlignmentVertical(builder: flatbuffers.Builder, alignmentVerticalOffset: flatbuffers.Offset): void;
    static addAlignment3d(builder: flatbuffers.Builder, alignment3dOffset: flatbuffers.Offset): void;
    static endCivil(builder: flatbuffers.Builder): flatbuffers.Offset;
}
