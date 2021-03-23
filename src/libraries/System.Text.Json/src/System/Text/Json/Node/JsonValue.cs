// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Node
{
    /// <summary>
    /// todo
    /// </summary>
    public abstract partial class JsonValue : JsonNode
    {
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="options"></param>
        private protected JsonValue(JsonNodeOptions? options = null) : base(options) { }

        /// <summary>
        /// Factory and deserialize methods below that doen't require specifying T due to generic type inference. This is necessary for anonymous types.
        /// T is normally a primitive but can also be JsonElement or any other type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonValue? Create<T>(T value, JsonNodeOptions? options = null)
        {
            return new JsonValue<T>(value, options);
        }

        internal override void GetPath(List<string> path, JsonNode? child)
        {
            Debug.Assert(child == null);

            if (Parent != null)
            {
                Parent.GetPath(path, this);
            }
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public abstract bool TryGetValue<T>(
            out T? value,
            JsonSerializerOptions? options = null);
    }
}
