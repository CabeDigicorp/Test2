import { FragmentsGroup } from "./fragments-group";
/**
 * Serializer class for handling the serialization and deserialization of 3D model data. It uses the [flatbuffers library](https://flatbuffers.dev/) for efficient data serialization and deserialization.
 */
export declare class Serializer {
    private fragmentIDSeparator;
    /**
     * Constructs a FragmentsGroup object from the given flatbuffers data.
     *
     * @param bytes - The flatbuffers data as Uint8Array.
     * @returns A FragmentsGroup object constructed from the flatbuffers data.
     */
    import(bytes: Uint8Array): FragmentsGroup;
    /**
     * Exports the FragmentsGroup to a flatbuffer binary file.
     *
     * @param group - The FragmentsGroup to be exported.
     * @returns The flatbuffer binary file as a Uint8Array.
     */
    export(group: FragmentsGroup): Uint8Array;
    private setID;
    private setInstances;
    private constructMaterials;
    private constructFragmentGroup;
    private setGroupData;
    private constructGeometry;
    private constructCivilCurves;
    private saveCivilCurves;
}
