// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Metadata
{
    /// <summary>
    /// Provides JSON serialization-related metadata about a type.
    /// </summary>
    /// <typeparam name="T">The generic definition of the type.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class JsonTypeInfo<T> : JsonTypeInfo
    {
        internal JsonTypeInfo(Type type, JsonSerializerOptions options, ConverterStrategy converterStrategy) :
            base(type, options, converterStrategy)
        { }

        internal JsonTypeInfo(Type type, JsonConverter converter, JsonSerializerOptions options) :
            base(type, converter, runtimeType: type, options)
        {
        }

        internal JsonTypeInfo()
        {
            Debug.Assert(false, "This constructor should not be called.");
        }

        /// <summary>
        /// A method that serializes an instance of <typeparamref name="T"/> using
        /// <see cref="JsonSerializerOptionsAttribute"/> values specified at design time.
        /// </summary>
        public Action<Utf8JsonWriter, T>? Serialize { get; private protected set; }

        private Action<T, JsonTypeInfo>? _onSerializing;
        /// <summary>
        /// todo
        /// </summary>
        public Action<T, JsonTypeInfo>? OnSerializing
        {
            get
            {
                return _onSerializing;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onSerializing = value;
            }
        }

        private Action<T, JsonTypeInfo>? _onSerialized;
        /// <summary>
        /// todo
        /// </summary>
        public Action<T, JsonTypeInfo>? OnSerialized
        {
            get
            {
                return _onSerialized;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onSerialized = value;
            }
        }

        private Action<T, JsonTypeInfo>? _onDeserializing;
        /// <summary>
        /// todo
        /// </summary>
        public Action<T, JsonTypeInfo>? OnDeserializing
        {
            get
            {
                return _onDeserializing;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onDeserializing = value;
            }
        }

        private Action<T, JsonTypeInfo>? _onDeserialized;
        /// <summary>
        /// todo
        /// </summary>
        public Action<T, JsonTypeInfo>? OnDeserialized
        {
            get
            {
                return _onDeserialized;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onDeserialized = value;
            }
        }

    }
}
