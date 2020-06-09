// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Converts an object or value to or from JSON.
    /// </summary>
    public abstract partial class JsonUntypedConverter : JsonConverter
    {
        /// <summary>
        /// Creates (todo)
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <param name="genericTypeToConvert"></param>
        protected JsonUntypedConverter(Type typeToConvert, Type genericTypeToConvert)
        {
            TypeToConvert = typeToConvert;
            GenericTypeToConvert = genericTypeToConvert;

            HandleNull = IsValueType;
            CanBeNull = !IsValueType || Nullable.GetUnderlyingType(TypeToConvert) != null;

            // Today only typeof(object) can have polymorphic writes.
            // In the future, this will be check for !IsSealed (and excluding value types).
            CanBePolymorphic = typeToConvert == typeof(object);
            IsValueType = typeToConvert.IsValueType;
            IsInternalConverter = GetType().Assembly == typeof(JsonConverter).Assembly;
            CanUseDirectReadOrWrite = !CanBePolymorphic && IsInternalConverter && ClassType == ClassType.Value;
        }

        /// <summary>
        /// Can <see langword="null"/> be assigned to <see cref="TypeToConvert"/>?
        /// </summary>
        internal bool CanBeNull { get; }


        internal virtual ClassType ClassType
        {
            get
            {
                return ClassType.None;
            }
        }

        /// <summary>
        /// Can direct Read or Write methods be called (for performance).
        /// </summary>
        internal bool CanUseDirectReadOrWrite { get; set; }

        /// <summary>
        /// Can the converter have $id metadata.
        /// </summary>
        internal virtual bool CanHaveIdMetadata => true;

        internal bool CanBePolymorphic { get; set; }

        internal virtual JsonPropertyInfo CreateJsonPropertyInfo()
        {
            throw new NotSupportedException();
        }

        internal virtual JsonParameterInfo CreateJsonParameterInfo()
        {
            throw new NotSupportedException();
        }

        internal virtual Type? ElementType
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// The {T} value in JsonConverter{T}.
        /// </summary>
        internal Type GenericTypeToConvert { get; }

        // Whether a type (ClassType.Object) is deserialized using a parameterized constructor.
        internal virtual bool ConstructorIsParameterized => false;

        /// <summary>
        /// Indicates whether <see langword="null"/> should be passed to the converter on serialization,
        /// and whether <see cref="JsonTokenType.Null"/> should be passed on deserialization.
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="true"/> for converters for value types, and <see langword="false"/> for converters for reference types.
        /// </remarks>
        public virtual bool HandleNull { get; }

        /// <summary>
        /// Is the converter built-in.
        /// </summary>
        internal bool IsInternalConverter { get; set; }

        /// <summary>
        /// Cached value of TypeToConvert.IsValueType, which is an expensive call.
        /// </summary>
        internal bool IsValueType { get; set; }

        /// <summary>
        /// Loosely-typed ReadCore() that forwards to strongly-typed ReadCore().
        /// </summary>
        internal virtual object? ReadCoreAsObject(ref Utf8JsonReader reader, JsonSerializerOptions options, ref ReadStack state)
        {
            throw new NotSupportedException();
        }

        internal virtual bool TryReadAsObject(ref Utf8JsonReader reader, JsonSerializerOptions options, ref ReadStack state, out object value)
        {
            throw new NotSupportedException();
        }

        // For polymorphic cases, the concrete type to create.
        internal virtual Type RuntimeType => TypeToConvert;

        internal bool ShouldFlush(Utf8JsonWriter writer, ref WriteStack state)
        {
            // If surpassed flush threshold then return false which will flush stream.
            return (state.FlushThreshold > 0 && writer.BytesPending > state.FlushThreshold);
        }

        internal virtual bool TryWriteAsObject(Utf8JsonWriter writer, object? value, JsonSerializerOptions options, ref WriteStack state)
        {
            throw new NotSupportedException();
        }

        internal Type TypeToConvert { get; }

        /// <summary>
        /// Loosely-typed WriteCore() that forwards to strongly-typed WriteCore().
        /// </summary>
        internal virtual bool WriteCoreAsObject(Utf8JsonWriter writer, object? value, JsonSerializerOptions options, ref WriteStack state)
        {
            throw new NotSupportedException();
        }
    }
}
