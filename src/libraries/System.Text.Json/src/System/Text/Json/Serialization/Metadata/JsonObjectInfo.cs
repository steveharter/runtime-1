// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization.Converters;

namespace System.Text.Json.Serialization.Metadata
{
    internal sealed class JsonObjectInfo<T> : JsonTypeInfo<T>
    {
        // todo: expose
        //public JsonObjectInfo(JsonSerializerOptions options) : base(ConverterStrategy.Object, options)
        //{
        //}

        /// <summary>
        /// Creates serialization metadata for a <see cref="ConverterStrategy.Object"/>.
        /// </summary>
        public JsonObjectInfo(JsonSerializerOptions options) : base(ConverterStrategy.Object, null!)
        {
        }


        /// <summary>
        /// Initializes serialization metadata for a <see cref="ConverterStrategy.Object"/>.
        /// </summary>
        public void InitializeAsObject(
            JsonSerializerOptions options,
            Func<T>? createObjectFunc,
            Func<JsonSerializerContext, JsonPropertyInfo[]> propInitFunc,
            JsonNumberHandling numberHandling)
        {
            Options = options;

#pragma warning disable CS8714
            // The type cannot be used as type parameter in the generic type or method.
            // Nullability of type argument doesn't match 'notnull' constraint.
            JsonConverter converter = new ObjectSourceGenConverter<T>();
#pragma warning restore CS8714

            PropertyInfoForTypeInfo = JsonMetadataServices.CreateJsonPropertyInfoForClassInfo(typeof(T), this, converter, options);
            NumberHandling = numberHandling;
            PropInitFunc = propInitFunc;
            SetCreateObjectFunc(createObjectFunc);
        }
    }
}
