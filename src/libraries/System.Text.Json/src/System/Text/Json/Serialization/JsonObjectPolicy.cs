// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Determines the policy used to define the metadata for a JSON object which includes the list of properties
    /// and their behavior.
    /// </summary>
    public abstract class JsonObjectPolicy
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonObjectPolicy"/>.
        /// </summary>
        protected JsonObjectPolicy(JsonSerializerOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// todo
        /// </summary>
        public JsonSerializerOptions Options { get; }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public abstract JsonTypeInfo GetTypeInfo(Type type);

        /// <summary>
        /// todo - create an empty typeinfo
        /// </summary>
        /// <returns></returns>
        protected JsonTypeInfo CreateEmptyTypeInfo(Type type)
        {
            return new JsonTypeInfo(type, Options);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected JsonTypeInfo CreateTypeInfoFromMetadata(Type type)
        {
            return Options.CreateTypeInfo(type);
        }

        /// <summary>
        /// todo - supports multiple threads
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        protected bool TryAddTypeInfo(JsonTypeInfo typeInfo)
        {
            return Options.TryAddTypeInfo(typeInfo);
        }
    }
}
