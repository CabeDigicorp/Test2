import * as WEBIFC from "web-ifc";
import { FragmentsGroup } from "@thatopen/fragments";
import { Disposable, Event, Component, Components } from "../../core";
import { RelationsMap, ModelsRelationMap, InverseAttribute, IfcRelation, RelationsProcessingConfig, EntitiesRelatedEvent } from "./src";
/**
 * Indexer component for IFC entities, facilitating the indexing and retrieval of IFC entity relationships. It is designed to process models properties by indexing their IFC entities' relations based on predefined inverse attributes, and provides methods to query these relations. 📕 [Tutorial](https://docs.thatopen.com/Tutorials/Components/Core/IfcRelationsIndexer). 📘 [API](https://docs.thatopen.com/api/@thatopen/components/classes/IfcRelationsIndexer).
 */
export declare class IfcRelationsIndexer extends Component implements Disposable {
    /**
     * A unique identifier for the component.
     * This UUID is used to register the component within the Components system.
     */
    static readonly uuid: "23a889ab-83b3-44a4-8bee-ead83438370b";
    /** {@link Disposable.onDisposed} */
    readonly onDisposed: Event<string>;
    /**
     * Event triggered when relations for a model have been indexed.
     * This event provides the model's UUID and the relations map generated for that model.
     *
     * @property {string} modelID - The UUID of the model for which relations have been indexed.
     * @property {RelationsMap} relationsMap - The relations map generated for the specified model.
     * The map keys are expressIDs of entities, and the values are maps where each key is a relation type ID and its value is an array of expressIDs of entities related through that relation type.
     */
    readonly onRelationsIndexed: Event<{
        modelID: string;
        relationsMap: RelationsMap;
    }>;
    /**
     * Holds the relationship mappings for each model processed by the indexer.
     * The structure is a map where each key is a model's UUID, and the value is another map.
     * This inner map's keys are entity expressIDs, and its values are maps where each key is an index
     * representing a specific relation type, and the value is an array of expressIDs of entities
     * that are related through that relation type. This structure allows for efficient querying
     * of entity relationships within a model.
     */
    readonly relationMaps: ModelsRelationMap;
    /** {@link Component.enabled} */
    enabled: boolean;
    private _relToAttributesMap;
    private _inverseAttributes;
    private _ifcRels;
    constructor(components: Components);
    private onFragmentsDisposed;
    private indexRelations;
    getAttributeIndex(inverseAttribute: InverseAttribute): number;
    /**
     * Adds a relation map to the model's relations map.
     *
     * @param model - The `FragmentsGroup` model to which the relation map will be added.
     * @param relationMap - The `RelationsMap` to be added to the model's relations map.
     *
     * @fires onRelationsIndexed - Triggers an event with the model's UUID and the added relation map.
     */
    setRelationMap(model: FragmentsGroup, relationMap: RelationsMap): void;
    /**
     * Processes a given model to index its IFC entities relations based on predefined inverse attributes.
     * This method iterates through each specified inverse attribute, retrieves the corresponding relations,
     * and maps them in a structured way to facilitate quick access to related entities.
     *
     * The process involves querying the model for each relation type associated with the inverse attributes
     * and updating the internal relationMaps with the relationships found. This map is keyed by the model's UUID
     * and contains a nested map where each key is an entity's expressID and its value is another map.
     * This inner map's keys are the indices of the inverse attributes, and its values are arrays of expressIDs
     * of entities that are related through that attribute.
     *
     * @param model The `FragmentsGroup` model to be processed. It must have properties loaded.
     * @returns A promise that resolves to the relations map for the processed model. This map is a detailed
     * representation of the relations indexed by entity expressIDs and relation types.
     * @throws An error if the model does not have properties loaded.
     */
    process(model: FragmentsGroup, config?: Partial<RelationsProcessingConfig>): Promise<RelationsMap>;
    /**
     * Processes a given model from a WebIfc API to index its IFC entities relations.
     *
     * @param ifcApi - The WebIfc API instance from which to retrieve the model's properties.
     * @param modelID - The unique identifier of the model within the WebIfc API.
     * @returns A promise that resolves to the relations map for the processed model.
     *          This map is a detailed representation of the relations indexed by entity expressIDs and relation types.
     */
    processFromWebIfc(ifcApi: WEBIFC.IfcAPI, modelID: number): Promise<RelationsMap>;
    /**
     * Retrieves the relations of a specific entity within a model based on the given relation name.
     * This method searches the indexed relation maps for the specified model and entity,
     * returning the IDs of related entities if a match is found.
     *
     * @param model The `FragmentsGroup` model containing the entity, or its UUID.
     * @param expressID The unique identifier of the entity within the model.
     * @param attribute The IFC schema inverse attribute of the relation to search for (e.g., "IsDefinedBy", "ContainsElements").
     * @returns An array of express IDs representing the related entities. If the array is empty, no relations were found.
     */
    getEntityRelations(model: FragmentsGroup | string | RelationsMap, expressID: number, attribute: InverseAttribute): number[];
    /**
     * Serializes the relations of a given relation map into a JSON string.
     * This method iterates through the relations in the given map, organizing them into a structured object where each key is an expressID of an entity,
     * and its value is another object mapping relation indices to arrays of related entity expressIDs.
     * The resulting object is then serialized into a JSON string.
     *
     * @param relationMap - The map of relations to be serialized. The map keys are expressIDs of entities, and the values are maps where each key is a relation type ID and its value is an array of expressIDs of entities related through that relation type.
     * @returns A JSON string representing the serialized relations of the given relation map.
     */
    serializeRelations(relationMap: RelationsMap): string;
    /**
     * Serializes the relations of a specific model into a JSON string.
     * This method iterates through the relations indexed for the given model,
     * organizing them into a structured object where each key is an expressID of an entity,
     * and its value is another object mapping relation indices to arrays of related entity expressIDs.
     * The resulting object is then serialized into a JSON string.
     *
     * @param model The `FragmentsGroup` model whose relations are to be serialized.
     * @returns A JSON string representing the serialized relations of the specified model.
     * If the model has no indexed relations, `null` is returned.
     */
    serializeModelRelations(model: FragmentsGroup): string | null;
    /**
     * Serializes all relations of every model processed by the indexer into a JSON string.
     * This method iterates through each model's relations indexed in `relationMaps`, organizing them
     * into a structured JSON object. Each top-level key in this object corresponds to a model's UUID,
     * and its value is another object mapping entity expressIDs to their related entities, categorized
     * by relation types. The structure facilitates easy access to any entity's relations across all models.
     *
     * @returns A JSON string representing the serialized relations of all models processed by the indexer.
     *          If no relations have been indexed, an empty object is returned as a JSON string.
     */
    serializeAllRelations(): string;
    /**
     * Converts a JSON string representing relations between entities into a structured map.
     * This method parses the JSON string to reconstruct the relations map that indexes
     * entity relations by their express IDs. The outer map keys are the express IDs of entities,
     * and the values are maps where each key is a relation type ID and its value is an array
     * of express IDs of entities related through that relation type.
     *
     * @param json The JSON string to be parsed into the relations map.
     * @returns A `Map` where the key is the express ID of an entity as a number, and the value
     * is another `Map`. This inner map's key is the relation type ID as a number, and its value
     * is an array of express IDs (as numbers) of entities related through that relation type.
     */
    getRelationsMapFromJSON(json: string): RelationsMap;
    /** {@link Disposable.dispose} */
    dispose(): void;
    /**
     * Retrieves the entities within a model that have a specific relation with a given entity.
     *
     * @param model - The BIM model to search for related entities.
     * @param inv - The IFC schema inverse attribute of the relation to search for (e.g., "IsDefinedBy", "ContainsElements").
     * @param expressID - The expressID of the entity within the model.
     *
     * @returns A `Set` with the expressIDs of the entities that have the specified relation with the given entity.
     *
     * @throws An error if the model relations are not indexed or if the inverse attribute name is invalid.
     */
    getEntitiesWithRelation(model: FragmentsGroup, inv: InverseAttribute, expressID: number): Set<number>;
    /**
     * Adds relations between an entity and other entities in a BIM model.
     *
     * @param model - The BIM model to which the relations will be added.
     * @param expressID - The expressID of the entity within the model.
     * @param relationName - The IFC schema inverse attribute of the relation to add (e.g., "IsDefinedBy", "ContainsElements").
     * @param relIDs - The expressIDs of the related entities within the model.
     * @deprecated Use addEntitiesRelation instead. This will be removed in future versions.
     *
     * @throws An error if the relation name is not a valid relation name.
     */
    addEntityRelations(model: FragmentsGroup, expressID: number, relationName: InverseAttribute, ...relIDs: number[]): void;
    /**
     * Converts the relations made into actual IFC data.
     *
     * @remarks This function iterates through the changes made to the relations and applies them to the corresponding BIM model.
     * It only make sense to use it if the relations need to be write in the IFC file.
     *
     * @returns A promise that resolves when all the relation changes have been applied.
     */
    applyRelationChanges(): Promise<void>;
    private readonly _changeMap;
    /**
     * An event that is triggered when entities are related in a BIM model.
     * The event provides information about the type of relation, the inverse attribute,
     * the IDs of the entities related, and the IDs of the entities that are being related.
     */
    readonly onEntitiesRelated: Event<EntitiesRelatedEvent>;
    addEntitiesRelation(model: FragmentsGroup, relatingID: number, rel: {
        type: IfcRelation;
        inv: InverseAttribute;
    }, ...relatedIDs: number[]): void;
    /**
     * Gets the children of the given element recursively. E.g. in a model with project - site - building - storeys - rooms, passing a storey will include all its children and the children of the rooms contained in it.
     *
     * @param model The BIM model whose children to get.
     * @param expressID The expressID of the item whose children to get.
     * @param found An optional parameter that includes a set of expressIDs where the found element IDs will be added.
     *
     * @returns A `Set` with the expressIDs of the found items.
     */
    getEntityChildren(model: FragmentsGroup, expressID: number, found?: Set<number>): Set<number>;
}
export * from "./src";
