// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace System.Text.Json.Serialization.Metadata
{
    internal sealed class JsonCollectionInfo<T> : JsonTypeInfo<T>
    {
        /// <summary>
        /// Creates serialization metadata for a <see cref="ConverterStrategy.Enumerable"/>.
        /// </summary>
        public JsonCollectionInfo(
            JsonSerializerOptions options,
            Func<T>? createObjectFunc,
            JsonConverter<T> converter,
            JsonTypeInfo elementInfo,
            JsonNumberHandling numberHandling) : base(ConverterStrategy.Enumerable, options)
        {
            ElementType = converter.ElementType;
            ElementTypeInfo = elementInfo ?? throw new ArgumentNullException(nameof(elementInfo));
            NumberHandling = numberHandling;
            PropertyInfoForTypeInfo = JsonMetadataServices.CreateJsonPropertyInfoForClassInfo(typeof(T), this, converter, options);
            SetCreateObjectFunc(createObjectFunc);
        }

        /// <summary>
        /// Creates serialization metadata for a <see cref="ConverterStrategy.Dictionary"/>.
        /// </summary>
        public JsonCollectionInfo(
            JsonSerializerOptions options,
            Func<T>? createObjectFunc,
            JsonConverter<T> converter,
            JsonTypeInfo keyInfo,
            JsonTypeInfo valueInfo,
            JsonNumberHandling numberHandling) : base(ConverterStrategy.Dictionary, options)
        {
            KeyType = converter.KeyType;
            KeyTypeInfo = keyInfo ?? throw new ArgumentNullException(nameof(keyInfo)); ;
            ElementType = converter.ElementType;
            ElementTypeInfo = valueInfo ?? throw new ArgumentNullException(nameof(valueInfo));
            NumberHandling = numberHandling;
            PropertyInfoForTypeInfo = JsonMetadataServices.CreateJsonPropertyInfoForClassInfo(typeof(T), this, converter, options);
            SetCreateObjectFunc(createObjectFunc);
        }
    }
}
