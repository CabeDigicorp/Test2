import { Event } from "./event";
/**
 * A class that extends the built-in Map class and provides additional events for item set, update, delete, and clear operations.
 *
 * @template K - The type of keys in the map.
 * @template V - The type of values in the map.
 */
export declare class DataMap<K, V> extends Map<K, V> {
    /**
     * An event triggered when a new item is set in the map.
     */
    readonly onItemSet: Event<{
        key: K;
        value: V;
    }>;
    /**
     * An event triggered when an existing item in the map is updated.
     */
    readonly onItemUpdated: Event<{
        key: K;
        value: V;
    }>;
    /**
     * An event triggered when an item is deleted from the map.
     */
    readonly onItemDeleted: Event<K>;
    /**
     * An event triggered when the map is cleared.
     */
    readonly onCleared: Event<unknown>;
    /**
     * Constructs a new DataMap instance.
     *
     * @param iterable - An iterable object containing key-value pairs to populate the map.
     */
    constructor(iterable?: Iterable<readonly [K, V]> | null | undefined);
    /**
     * Clears the map and triggers the onCleared event.
     */
    clear(): void;
    /**
     * Sets the value for the specified key in the map.
     * If the item is new, then onItemSet is triggered.
     * If the item is already in the map, then onItemUpdated is triggered.
     *
     * @param key - The key of the item to set.
     * @param value - The value of the item to set.
     * @returns The DataMap instance.
     */
    set(key: K, value: V): this;
    /**
     * A function that acts as a guard for adding items to the set.
     * It determines whether a given value should be allowed to be added to the set.
     *
     * @param key - The key of the entry to be checked against the guard.
     * @param value - The value of the entry to be checked against the guard.
     * @returns A boolean indicating whether the value should be allowed to be added to the set.
     *          By default, this function always returns true, allowing all values to be added.
     *          You can override this behavior by providing a custom implementation.
     */
    guard: (key: K, value: V) => boolean;
    /**
     * Deletes the specified key from the map and triggers the onItemDeleted event if the key was found.
     *
     * @param key - The key of the item to delete.
     * @returns True if the key was found and deleted; otherwise, false.
     */
    delete(key: K): boolean;
    /**
     * Clears the map and resets the events.
     */
    dispose(): void;
}
