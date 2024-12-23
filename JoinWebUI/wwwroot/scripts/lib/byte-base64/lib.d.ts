export declare function bytesToBase64(bytes: number[] | Uint8Array): string;
export declare function base64ToBytes(str: string): Uint8Array;
export declare function base64encode(str: string, encoder?: {
    encode: (str: string) => Uint8Array | number[];
}): string;
export declare function base64decode(str: string, decoder?: {
    decode: (bytes: Uint8Array) => string;
}): string;
