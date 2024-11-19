// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// This is a single-file self-contained module to avoid the need for a Webpack build
export var DotNet;
(function (DotNet) {
    const jsonRevivers = [];
    const jsObjectIdKey = "__jsObjectId";
    const dotNetObjectRefKey = "__dotNetObject";
    const byteArrayRefKey = "__byte[]";
    const dotNetStreamRefKey = "__dotNetStream";
    const jsStreamReferenceLengthKey = "__jsStreamReferenceLength";
    // If undefined, no dispatcher has been attached yet.
    // If null, this means more than one dispatcher was attached, so no default can be determined.
    // Otherwise, there was only one dispatcher registered. We keep track of this instance to keep legacy APIs working.
    let defaultCallDispatcher;
    // Provides access to the "current" call dispatcher without having to flow it through nested function calls.
    let currentCallDispatcher;
    class JSObject {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        constructor(_jsObject) {
            this._jsObject = _jsObject;
            this._cachedFunctions = new Map();
        }
        findFunction(identifier) {
            const cachedFunction = this._cachedFunctions.get(identifier);
            if (cachedFunction) {
                return cachedFunction;
            }
            let result = this._jsObject;
            let lastSegmentValue;
            identifier.split(".").forEach(segment => {
                if (segment in result) {
                    lastSegmentValue = result;
                    result = result[segment];
                }
                else {
                    throw new Error(`Could not find '${identifier}' ('${segment}' was undefined).`);
                }
            });
            if (result instanceof Function) {
                result = result.bind(lastSegmentValue);
                this._cachedFunctions.set(identifier, result);
                return result;
            }
            throw new Error(`The value '${identifier}' is not a function.`);
        }
        getWrappedObject() {
            return this._jsObject;
        }
    }
    const windowJSObjectId = 0;
    const cachedJSObjectsById = {
        [windowJSObjectId]: new JSObject(window)
    };
    cachedJSObjectsById[windowJSObjectId]._cachedFunctions.set("import", (url) => {
        // In most cases developers will want to resolve dynamic imports relative to the base HREF.
        // However since we're the one calling the import keyword, they would be resolved relative to
        // this framework bundle URL. Fix this by providing an absolute URL.
        if (typeof url === "string" && url.startsWith("./")) {
            url = new URL(url.substr(2), document.baseURI).toString();
        }
        return import(/* webpackIgnore: true */ url);
    });
    let nextJsObjectId = 1; // Start at 1 because zero is reserved for "window"
    /**
     * Creates a .NET call dispatcher to use for handling invocations between JavaScript and a .NET runtime.
     *
     * @param dotNetCallDispatcher An object that can dispatch calls from JavaScript to a .NET runtime.
     */
    function attachDispatcher(dotNetCallDispatcher) {
        const result = new CallDispatcher(dotNetCallDispatcher);
        if (defaultCallDispatcher === undefined) {
            // This was the first dispatcher registered, so it becomes the default. This exists purely for
            // backwards compatibility.
            defaultCallDispatcher = result;
        }
        else if (defaultCallDispatcher) {
            // There is already a default dispatcher. Now that there are multiple to choose from, there can
            // be no acceptable default, so we nullify the default dispatcher.
            defaultCallDispatcher = null;
        }
        return result;
    }
    DotNet.attachDispatcher = attachDispatcher;
    /**
     * Adds a JSON reviver callback that will be used when parsing arguments received from .NET.
     * @param reviver The reviver to add.
     */
    function attachReviver(reviver) {
        jsonRevivers.push(reviver);
    }
    DotNet.attachReviver = attachReviver;
    /**
     * Invokes the specified .NET public method synchronously. Not all hosting scenarios support
     * synchronous invocation, so if possible use invokeMethodAsync instead.
     *
     * @deprecated Use DotNetObject to invoke instance methods instead.
     * @param assemblyName The short name (without key/version or .dll extension) of the .NET assembly containing the method.
     * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
     * @param args Arguments to pass to the method, each of which must be JSON-serializable.
     * @returns The result of the operation.
     */
    function invokeMethod(assemblyName, methodIdentifier, ...args) {
        const dispatcher = getDefaultCallDispatcher();
        return dispatcher.invokeDotNetStaticMethod(assemblyName, methodIdentifier, ...args);
    }
    DotNet.invokeMethod = invokeMethod;
    /**
     * Invokes the specified .NET public method asynchronously.
     *
     * @deprecated Use DotNetObject to invoke instance methods instead.
     * @param assemblyName The short name (without key/version or .dll extension) of the .NET assembly containing the method.
     * @param methodIdentifier The identifier of the method to invoke. The method must have a [JSInvokable] attribute specifying this identifier.
     * @param args Arguments to pass to the method, each of which must be JSON-serializable.
     * @returns A promise representing the result of the operation.
     */
    function invokeMethodAsync(assemblyName, methodIdentifier, ...args) {
        const dispatcher = getDefaultCallDispatcher();
        return dispatcher.invokeDotNetStaticMethodAsync(assemblyName, methodIdentifier, ...args);
    }
    DotNet.invokeMethodAsync = invokeMethodAsync;
    /**
     * Creates a JavaScript object reference that can be passed to .NET via interop calls.
     *
     * @param jsObject The JavaScript Object used to create the JavaScript object reference.
     * @returns The JavaScript object reference (this will be the same instance as the given object).
     * @throws Error if the given value is not an Object.
     */
    function createJSObjectReference(jsObject) {
        if (jsObject && typeof jsObject === "object") {
            cachedJSObjectsById[nextJsObjectId] = new JSObject(jsObject);
            const result = {
                [jsObjectIdKey]: nextJsObjectId
            };
            nextJsObjectId++;
            return result;
        }
        throw new Error(`Cannot create a JSObjectReference from the value '${jsObject}'.`);
    }
    DotNet.createJSObjectReference = createJSObjectReference;
    /**
     * Creates a JavaScript data reference that can be passed to .NET via interop calls.
     *
     * @param streamReference The ArrayBufferView or Blob used to create the JavaScript stream reference.
     * @returns The JavaScript data reference (this will be the same instance as the given object).
     * @throws Error if the given value is not an Object or doesn't have a valid byteLength.
     */
    function createJSStreamReference(streamReference) {
        let length = -1;
        // If we're given a raw Array Buffer, we interpret it as a `Uint8Array` as
        // ArrayBuffers' aren't directly readable.
        if (streamReference instanceof ArrayBuffer) {
            streamReference = new Uint8Array(streamReference);
        }
        if (streamReference instanceof Blob) {
            length = streamReference.size;
        }
        else if (streamReference.buffer instanceof ArrayBuffer) {
            if (streamReference.byteLength === undefined) {
                throw new Error(`Cannot create a JSStreamReference from the value '${streamReference}' as it doesn't have a byteLength.`);
            }
            length = streamReference.byteLength;
        }
        else {
            throw new Error("Supplied value is not a typed array or blob.");
        }
        const result = {
            [jsStreamReferenceLengthKey]: length
        };
        try {
            const jsObjectReference = createJSObjectReference(streamReference);
            result[jsObjectIdKey] = jsObjectReference[jsObjectIdKey];
        }
        catch (error) {
            throw new Error(`Cannot create a JSStreamReference from the value '${streamReference}'.`);
        }
        return result;
    }
    DotNet.createJSStreamReference = createJSStreamReference;
    /**
     * Disposes the given JavaScript object reference.
     *
     * @param jsObjectReference The JavaScript Object reference.
     */
    function disposeJSObjectReference(jsObjectReference) {
        const id = jsObjectReference && jsObjectReference[jsObjectIdKey];
        if (typeof id === "number") {
            disposeJSObjectReferenceById(id);
        }
    }
    DotNet.disposeJSObjectReference = disposeJSObjectReference;
    function parseJsonWithRevivers(callDispatcher, json) {
        currentCallDispatcher = callDispatcher;
        const result = json ? JSON.parse(json, (key, initialValue) => {
            // Invoke each reviver in order, passing the output from the previous reviver,
            // so that each one gets a chance to transform the value
            return jsonRevivers.reduce((latestValue, reviver) => reviver(key, latestValue), initialValue);
        }) : null;
        currentCallDispatcher = undefined;
        return result;
    }
    function getDefaultCallDispatcher() {
        if (defaultCallDispatcher === undefined) {
            throw new Error("No call dispatcher has been set.");
        }
        else if (defaultCallDispatcher === null) {
            throw new Error("There are multiple .NET runtimes present, so a default dispatcher could not be resolved. Use DotNetObject to invoke .NET instance methods.");
        }
        else {
            return defaultCallDispatcher;
        }
    }
    /**
     * Represents the type of result expected from a JS interop call.
     */
    // eslint-disable-next-line no-shadow
    let JSCallResultType;
    (function (JSCallResultType) {
        JSCallResultType[JSCallResultType["Default"] = 0] = "Default";
        JSCallResultType[JSCallResultType["JSObjectReference"] = 1] = "JSObjectReference";
        JSCallResultType[JSCallResultType["JSStreamReference"] = 2] = "JSStreamReference";
        JSCallResultType[JSCallResultType["JSVoidResult"] = 3] = "JSVoidResult";
    })(JSCallResultType = DotNet.JSCallResultType || (DotNet.JSCallResultType = {}));
    class CallDispatcher {
        // eslint-disable-next-line no-empty-function
        constructor(_dotNetCallDispatcher) {
            this._dotNetCallDispatcher = _dotNetCallDispatcher;
            this._byteArraysToBeRevived = new Map();
            this._pendingDotNetToJSStreams = new Map();
            this._pendingAsyncCalls = {};
            this._nextAsyncCallId = 1; // Start at 1 because zero signals "no response needed"
        }
        getDotNetCallDispatcher() {
            return this._dotNetCallDispatcher;
        }
        invokeJSFromDotNet(identifier, argsJson, resultType, targetInstanceId) {
            const args = parseJsonWithRevivers(this, argsJson);
            const jsFunction = findJSFunction(identifier, targetInstanceId);
            const returnValue = jsFunction(...(args || []));
            const result = createJSCallResult(returnValue, resultType);
            return result === null || result === undefined
                ? null
                : stringifyArgs(this, result);
        }
        beginInvokeJSFromDotNet(asyncHandle, identifier, argsJson, resultType, targetInstanceId) {
            // Coerce synchronous functions into async ones, plus treat
            // synchronous exceptions the same as async ones
            const promise = new Promise(resolve => {
                const args = parseJsonWithRevivers(this, argsJson);
                const jsFunction = findJSFunction(identifier, targetInstanceId);
                const synchronousResultOrPromise = jsFunction(...(args || []));
                resolve(synchronousResultOrPromise);
            });
            // We only listen for a result if the caller wants to be notified about it
            if (asyncHandle) {
                // On completion, dispatch result back to .NET
                // Not using "await" because it codegens a lot of boilerplate
                promise.
                    then(result => stringifyArgs(this, [
                    asyncHandle,
                    true,
                    createJSCallResult(result, resultType)
                ])).
                    then(result => this._dotNetCallDispatcher.endInvokeJSFromDotNet(asyncHandle, true, result), error => this._dotNetCallDispatcher.endInvokeJSFromDotNet(asyncHandle, false, JSON.stringify([
                    asyncHandle,
                    false,
                    formatError(error)
                ])));
            }
        }
        endInvokeDotNetFromJS(asyncCallId, success, resultJsonOrExceptionMessage) {
            const resultOrError = success
                ? parseJsonWithRevivers(this, resultJsonOrExceptionMessage)
                : new Error(resultJsonOrExceptionMessage);
            this.completePendingCall(parseInt(asyncCallId, 10), success, resultOrError);
        }
        invokeDotNetStaticMethod(assemblyName, methodIdentifier, ...args) {
            return this.invokeDotNetMethod(assemblyName, methodIdentifier, null, args);
        }
        invokeDotNetStaticMethodAsync(assemblyName, methodIdentifier, ...args) {
            return this.invokeDotNetMethodAsync(assemblyName, methodIdentifier, null, args);
        }
        invokeDotNetMethod(assemblyName, methodIdentifier, dotNetObjectId, args) {
            if (this._dotNetCallDispatcher.invokeDotNetFromJS) {
                const argsJson = stringifyArgs(this, args);
                const resultJson = this._dotNetCallDispatcher.invokeDotNetFromJS(assemblyName, methodIdentifier, dotNetObjectId, argsJson);
                return resultJson ? parseJsonWithRevivers(this, resultJson) : null;
            }
            throw new Error("The current dispatcher does not support synchronous calls from JS to .NET. Use invokeDotNetMethodAsync instead.");
        }
        invokeDotNetMethodAsync(assemblyName, methodIdentifier, dotNetObjectId, args) {
            if (assemblyName && dotNetObjectId) {
                throw new Error(`For instance method calls, assemblyName should be null. Received '${assemblyName}'.`);
            }
            const asyncCallId = this._nextAsyncCallId++;
            const resultPromise = new Promise((resolve, reject) => {
                this._pendingAsyncCalls[asyncCallId] = { resolve, reject };
            });
            try {
                const argsJson = stringifyArgs(this, args);
                this._dotNetCallDispatcher.beginInvokeDotNetFromJS(asyncCallId, assemblyName, methodIdentifier, dotNetObjectId, argsJson);
            }
            catch (ex) {
                // Synchronous failure
                this.completePendingCall(asyncCallId, false, ex);
            }
            return resultPromise;
        }
        receiveByteArray(id, data) {
            this._byteArraysToBeRevived.set(id, data);
        }
        processByteArray(id) {
            const result = this._byteArraysToBeRevived.get(id);
            if (!result) {
                return null;
            }
            this._byteArraysToBeRevived.delete(id);
            return result;
        }
        supplyDotNetStream(streamId, stream) {
            if (this._pendingDotNetToJSStreams.has(streamId)) {
                // The receiver is already waiting, so we can resolve the promise now and stop tracking this
                const pendingStream = this._pendingDotNetToJSStreams.get(streamId);
                this._pendingDotNetToJSStreams.delete(streamId);
                pendingStream.resolve(stream);
            }
            else {
                // The receiver hasn't started waiting yet, so track a pre-completed entry it can attach to later
                const pendingStream = new PendingStream();
                pendingStream.resolve(stream);
                this._pendingDotNetToJSStreams.set(streamId, pendingStream);
            }
        }
        getDotNetStreamPromise(streamId) {
            // We might already have started receiving the stream, or maybe it will come later.
            // We have to handle both possible orderings, but we can count on it coming eventually because
            // it's not something the developer gets to control, and it would be an error if it doesn't.
            let result;
            if (this._pendingDotNetToJSStreams.has(streamId)) {
                // We've already started receiving the stream, so no longer need to track it as pending
                result = this._pendingDotNetToJSStreams.get(streamId).streamPromise;
                this._pendingDotNetToJSStreams.delete(streamId);
            }
            else {
                // We haven't started receiving it yet, so add an entry to track it as pending
                const pendingStream = new PendingStream();
                this._pendingDotNetToJSStreams.set(streamId, pendingStream);
                result = pendingStream.streamPromise;
            }
            return result;
        }
        completePendingCall(asyncCallId, success, resultOrError) {
            if (!this._pendingAsyncCalls.hasOwnProperty(asyncCallId)) {
                throw new Error(`There is no pending async call with ID ${asyncCallId}.`);
            }
            const asyncCall = this._pendingAsyncCalls[asyncCallId];
            delete this._pendingAsyncCalls[asyncCallId];
            if (success) {
                asyncCall.resolve(resultOrError);
            }
            else {
                asyncCall.reject(resultOrError);
            }
        }
    }
    function formatError(error) {
        if (error instanceof Error) {
            return `${error.message}\n${error.stack}`;
        }
        return error ? error.toString() : "null";
    }
    function findJSFunction(identifier, targetInstanceId) {
        const targetInstance = cachedJSObjectsById[targetInstanceId];
        if (targetInstance) {
            return targetInstance.findFunction(identifier);
        }
        throw new Error(`JS object instance with ID ${targetInstanceId} does not exist (has it been disposed?).`);
    }
    DotNet.findJSFunction = findJSFunction;
    function disposeJSObjectReferenceById(id) {
        delete cachedJSObjectsById[id];
    }
    DotNet.disposeJSObjectReferenceById = disposeJSObjectReferenceById;
    class DotNetObject {
        // eslint-disable-next-line no-empty-function
        constructor(_id, _callDispatcher) {
            this._id = _id;
            this._callDispatcher = _callDispatcher;
        }
        invokeMethod(methodIdentifier, ...args) {
            return this._callDispatcher.invokeDotNetMethod(null, methodIdentifier, this._id, args);
        }
        invokeMethodAsync(methodIdentifier, ...args) {
            return this._callDispatcher.invokeDotNetMethodAsync(null, methodIdentifier, this._id, args);
        }
        dispose() {
            const promise = this._callDispatcher.invokeDotNetMethodAsync(null, "__Dispose", this._id, null);
            promise.catch(error => console.error(error));
        }
        serializeAsArg() {
            return { [dotNetObjectRefKey]: this._id };
        }
    }
    DotNet.DotNetObject = DotNetObject;
    attachReviver(function reviveReference(key, value) {
        if (value && typeof value === "object") {
            if (value.hasOwnProperty(dotNetObjectRefKey)) {
                return new DotNetObject(value[dotNetObjectRefKey], currentCallDispatcher);
            }
            else if (value.hasOwnProperty(jsObjectIdKey)) {
                const id = value[jsObjectIdKey];
                const jsObject = cachedJSObjectsById[id];
                if (jsObject) {
                    return jsObject.getWrappedObject();
                }
                throw new Error(`JS object instance with Id '${id}' does not exist. It may have been disposed.`);
            }
            else if (value.hasOwnProperty(byteArrayRefKey)) {
                const index = value[byteArrayRefKey];
                const byteArray = currentCallDispatcher.processByteArray(index);
                if (byteArray === undefined) {
                    throw new Error(`Byte array index '${index}' does not exist.`);
                }
                return byteArray;
            }
            else if (value.hasOwnProperty(dotNetStreamRefKey)) {
                const streamId = value[dotNetStreamRefKey];
                const streamPromise = currentCallDispatcher.getDotNetStreamPromise(streamId);
                return new DotNetStream(streamPromise);
            }
        }
        // Unrecognized - let another reviver handle it
        return value;
    });
    class DotNetStream {
        // eslint-disable-next-line no-empty-function
        constructor(_streamPromise) {
            this._streamPromise = _streamPromise;
        }
        /**
         * Supplies a readable stream of data being sent from .NET.
         */
        stream() {
            return this._streamPromise;
        }
        /**
         * Supplies a array buffer of data being sent from .NET.
         * Note there is a JavaScript limit on the size of the ArrayBuffer equal to approximately 2GB.
         */
        async arrayBuffer() {
            return new Response(await this.stream()).arrayBuffer();
        }
    }
    class PendingStream {
        constructor() {
            this.streamPromise = new Promise((resolve, reject) => {
                this.resolve = resolve;
                this.reject = reject;
            });
        }
    }
    function createJSCallResult(returnValue, resultType) {
        switch (resultType) {
            case JSCallResultType.Default:
                return returnValue;
            case JSCallResultType.JSObjectReference:
                return createJSObjectReference(returnValue);
            case JSCallResultType.JSStreamReference:
                return createJSStreamReference(returnValue);
            case JSCallResultType.JSVoidResult:
                return null;
            default:
                throw new Error(`Invalid JS call result type '${resultType}'.`);
        }
    }
    let nextByteArrayIndex = 0;
    function stringifyArgs(callDispatcher, args) {
        nextByteArrayIndex = 0;
        currentCallDispatcher = callDispatcher;
        const result = JSON.stringify(args, argReplacer);
        currentCallDispatcher = undefined;
        return result;
    }
    function argReplacer(key, value) {
        if (value instanceof DotNetObject) {
            return value.serializeAsArg();
        }
        else if (value instanceof Uint8Array) {
            const dotNetCallDispatcher = currentCallDispatcher.getDotNetCallDispatcher();
            dotNetCallDispatcher.sendByteArray(nextByteArrayIndex, value);
            const jsonValue = { [byteArrayRefKey]: nextByteArrayIndex };
            nextByteArrayIndex++;
            return jsonValue;
        }
        return value;
    }
})(DotNet || (DotNet = {}));
//# sourceMappingURL=Microsoft.JSInterop.js.map