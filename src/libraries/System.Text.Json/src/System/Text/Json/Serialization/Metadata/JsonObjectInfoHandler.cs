// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Json.Serialization.Metadata
{
    // notes:
    // JsonTypeInfo to add
    // - static JsonTypeInfo<TClass> Create<TClass>(JsonSerializerOptions options)
    // - Converter {get;set} (null by default)
    // - NumberHandling {get;set;}
    // - JsonPropertyInfo CreateJsonProperty<TProperty>(string name, Func<in TClass, TProperty>? getter, Action<in TClass, TProperty>? setter)

    // Make JsonTypeInfo<T> public
    // - Func<TClass> Constructor {get;set}

    // Remove JsonTypeInfoInternal<T>? Combine with JsonTypeInfo<T>

    // JsonPropertyInfo to add:
    // - JsonIgnoreCondition
    // - JsonNumberHandling
    // - IList<JsonPropertyInfo> ConstructorParameters
    // - ConstructorFunc ConstructorFunc
    // - JsonTypeInfo.CreateFunc
    // - JsonConverter Converter {get;set;}
    // Remove internal JsonParameterInfo and combine with JsonPropertyInfo

    // Internal for now and just used for constructors:
    // - DefaultValue<TProperty> {get;set;}

    /// <summary>
    /// Determines the policy used to define the metadata for a JSON object which includes the list of properties
    /// and their behavior.
    /// </summary>
    public abstract class JsonObjectInfoHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonObjectInfoHandler"/>.
        /// </summary>
        protected JsonObjectInfoHandler(JsonSerializerOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// todo
        /// </summary>
        public JsonSerializerOptions Options { get; }

        internal void OnCreated(JsonTypeInfo objectTypeInfo)
        {
            Created(objectTypeInfo);
        }

        /// <summary>
        /// todo
        /// </summary>
        protected abstract void Created(JsonTypeInfo objectTypeInfo);
    }
}
