// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Collections.Generic;

namespace System.Text.Json.Node
{
    /// <summary>
    /// The base class for all dynamic types supported by the serializer.
    /// </summary>
    public abstract partial class JsonNode
    {
        private JsonNode? _parent;
        private JsonNodeOptions? _options;

        /// <summary>
        /// todo
        /// </summary>
        public JsonNodeOptions? Options
        {
            get
            {
                if (!_options.HasValue && Parent != null)
                {
                    return Parent.Options;
                }

                return _options;
            }

            private set
            {
                _options = value;
            }
        }

        internal JsonNode(JsonNodeOptions? options = null)
        {
            Options = options;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public JsonArray AsArray()
        {
            if (this is JsonArray jArray)
            {
                return jArray;
            }

            throw new InvalidOperationException("todo");
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public JsonObject AsObject()
        {
            if (this is JsonObject jObject)
            {
                return jObject;
            }

            throw new InvalidOperationException("todo");
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public JsonValue AsValue()
        {
            if (this is JsonValue jValue)
            {
                return jValue;
            }

            throw new InvalidOperationException("todo");
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AsValue<T>()
        {
            if (this is JsonValue jValue)
            {
                return jValue.GetValue<T>();
            }

            throw new InvalidOperationException("todo");
        }

        /// <summary>
        /// todo
        /// </summary>
        public JsonNode? Parent
        {
            get
            {
                return _parent;
            }
            internal set
            {
                _parent = value;
            }
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            if (Parent == null)
            {
                return "$";
            }

            var path = new List<string>();
            GetPath(path, null);

            var sb = new StringBuilder("$");
            for (int i = path.Count - 1; i >= 0; i--)
            {
                sb.Append(path[i]);
            }

            return sb.ToString();
        }

        internal abstract void GetPath(List<string> path, JsonNode? child);

        /// <summary>
        /// todo
        /// </summary>
        public JsonNode Root
        {
            get
            {
                JsonNode? parent = Parent;
                if (parent == null)
                {
                    return this;
                }

                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }

                return parent;
            }
        }

        internal abstract bool TryConvert(Type returnType, out object? result, JsonSerializerOptions? options = null);

        /// <summary>
        /// todo; only works with JsonValue
        /// </summary>
        public virtual TValue GetValue<TValue>(JsonSerializerOptions? options = null) =>
            throw new InvalidOperationException("todo");

        /// <summary>
        /// todo; only works with JsonArray
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JsonNode? this[int index]
        {
            get
            {
                return AsArray().GetItem(index);
            }
            set
            {
                AsArray().SetItem(index, value);
            }
        }

        /// <summary>
        /// todo; only works with JsonObject
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JsonNode? this[string key]
        {
            get
            {
                return AsObject().GetItem(key);
            }

            set
            {
                AsObject().SetItem(key, value);
            }
        }

        internal void AssignParent(JsonNode parent)
        {
            if (Parent != null)
            {
                throw new InvalidOperationException("todo:can't add node more than once");
            }

            JsonNode? p = parent;
            while (p != null)
            {
                if (p == this)
                {
                    throw new InvalidOperationException("todo cycle");
                }

                p = p.Parent;
            }

            Parent = parent;
        }
    }
}
