// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Json.Serialization.Metadata
{
    public static partial class JsonMetadataServices
    {
        /// <summary>
        /// todo - create an empty typeinfo
        /// </summary>
        /// <returns></returns>
        public static JsonTypeInfo<T> CreateEmptyObjectInfo<T>(JsonSerializerOptions? options = null)
        {
            JsonTypeInfo<T> typeInfo = new JsonTypeInfoInternal<T>(options ?? JsonSerializerOptions.s_defaultOptions, ConverterStrategy.Object);
            return typeInfo;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonTypeInfo<T> CreateObjectInfo<T>(JsonSerializerOptions? options = null)
        {
            options = options ?? JsonSerializerOptions.s_defaultOptions;
            JsonTypeInfo<T> typeInfo = (JsonTypeInfo<T>)options.GetClassFromContextOrCreate(typeof(T));
            if (typeInfo.PropertyInfoForTypeInfo.ConverterStrategy != ConverterStrategy.Object)
            {
                throw new InvalidOperationException("todo");
            }

            return typeInfo;
        }
    }
}
