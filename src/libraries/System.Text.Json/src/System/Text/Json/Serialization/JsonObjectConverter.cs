// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Base class for non-enumerable, non-primitive objects where public properties
    /// are (de)serialized as a JSON object.
    /// </summary>
    internal abstract class JsonObjectConverter<T> : JsonResumableConverter<T>
    {
        internal sealed override ClassType ClassType => ClassType.Object;
        internal sealed override Type? ElementType => null;
    }

    // to be public... options to be added to JsonValueConverter and removed from read\write
    internal interface IObjectConverter<T>
    {
        // Add list of properties via GetProperties().... override that below
        public JsonClassInfo info { get; }

        public virtual IEnumerable<JsonPropertyInfo> GetProperties() { }
        public virtual void SetProperty<TValue>(ref T obj, JsonPropertyInfo<TValue> info, TValue value, JsonSerializationState state) { }
        public virtual TValue GetProperty<TValue>(ref T obj, JsonPropertyInfo<TValue> info, JsonSerializationState state) { }
        public virtual T CreateObject(JsonSerializationState state) { }
    }

    internal interface ISerializationCallbacks<T>
    {
        public virtual void OnDeserializing(ref T obj, JsonSerializationState state) { }
        public virtual void OnDeserialized(ref T obj, JsonSerializationState state) { }
        public virtual void OnSerializing(ref T obj, JsonSerializationState state) { }
        public virtual void OnSerialized(ref T obj, JsonSerializationState state) { }
    }

    internal abstract class JsonObjectConverter2<T> : JsonValueConverter<T>, IObjectConverter<T>, ISerializationCallbacks<T>
    {
        public virtual void ShouldDeserialize<TValue>(JsonPropertyInfo<TValue> info, JsonSerializationState state) { }
        public virtual void ShouldSerialize<TValue>(JsonPropertyInfo<TValue> info, JsonSerializationState state) { }
    }

    internal ref struct JsonSerializationState
    {
        private ReadStack readStack;
        Utf8JsonReader reader;

        private WriteStack writeStack;
        Utf8JsonWriter writer;

        JsonSerializerOptions options;

        DefaultReferenceResolver referenceHandling;
    }
}
