// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization.Converters;

namespace System.Text.Json.Node
{
    /// <summary>
    /// Supports dynamic arrays.
    /// </summary>
    [DebuggerDisplay("JsonArray[{List.Count}]")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class JsonArray : JsonNode
    {
        private JsonElement? _jsonElement;
        private List<JsonNode?>? _value;

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="options"></param>
        public JsonArray(JsonNodeOptions? options = null) : base(options) { }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="options"></param>
        /// <param name="items"></param>
        public JsonArray(JsonNodeOptions options, params JsonNode?[] items) : base(options)
        {
            _value = new List<JsonNode?>(items);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="items"></param>
        public JsonArray(params JsonNode?[] items) : base()
        {
            _value = new List<JsonNode?>(items);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="element"></param>
        /// <param name="options"></param>
        public JsonArray(JsonElement element, JsonNodeOptions? options = null) : base(options)
        {
            _jsonElement = element;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void Add<T>(T value)
        {
            if (value is JsonNode jsonNode)
            {
                Add(jsonNode);
            }
            else
            {
                Add(new JsonValue<T>(value));
            }
        }

        internal List<JsonNode?> List
        {
            get
            {
                CreateNodes();
                Debug.Assert(_value != null);
                return _value;
            }
        }

        internal override bool TryConvert(Type returnType, out object? result, JsonSerializerOptions? options = null)
        {
            if (returnType.IsAssignableFrom(typeof(IList<object?>)))
            {
                result = _value;
                return true;
            }

            result = null;
            return false;
        }

        internal JsonNode? GetItem(int index)
        {
            return List[index];
        }

        internal override void GetPath(List<string> path, JsonNode? child)
        {
            if (child != null)
            {
                int index = List.IndexOf(child);
                path.Add($"[{index}]");
            }

            if (Parent != null)
            {
                Parent.GetPath(path, this);
            }
        }

        internal void SetItem(int index, JsonNode? value)
        {
            if (value != null)
            {
                value.AssignParent(this);
            }

            List[index] = value;
        }

        /// <summary>
        /// todo
        /// </summary>

        private void CreateNodes()
        {
            if (_value == null)
            {
                List<JsonNode?> list;

                if (_jsonElement == null)
                {
                    list = new List<JsonNode?>();
                }
                else
                {
                    JsonElement jElement = _jsonElement.Value;
                    Debug.Assert(jElement.ValueKind == JsonValueKind.Array);

                    list = new List<JsonNode?>(jElement.GetArrayLength());

                    foreach (JsonElement element in jElement.EnumerateArray())
                    {
                        JsonNode? node = JsonNodeConverter.Create(element, Options);
                        if (node != null)
                        {
                            node.Parent = this;
                        }

                        list.Add(node);
                    }

                    // Clear since no longer needed.
                    _jsonElement = null;
                }

                _value = list;
            }
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="options"></param>
        public override void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions? options = null)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (_jsonElement.HasValue)
            {
                _jsonElement.Value.WriteTo(writer);
            }
            else
            {
                Debug.Assert(_value != null);

                options ??= JsonSerializerOptions.s_defaultOptions;

                writer.WriteStartArray();

                for (int i = 0; i < _value.Count; i++)
                {
                    JsonNodeConverter.Default.Write(writer, _value[i]!, options);
                }

                writer.WriteEndArray();
            }
        }

        private class DebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private JsonArray _node;

            public DebugView(JsonArray node)
            {
                _node = node;
            }

            public string Json => _node.ToJsonString();
            public string Path => _node.GetPath();

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DebugViewItem[] Items
            {
                get
                {
                    DebugViewItem[] properties = new DebugViewItem[_node.List.Count];

                    for (int i = 0; i < _node.List.Count; i++)
                    {
                        properties[i].Value = _node.List[i];
                    }

                    return properties;
                }
            }

            [DebuggerDisplay("{Display,nq}")]
            private struct DebugViewItem
            {
                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public JsonNode? Value;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                public string Display
                {
                    get
                    {
                        if (Value == null)
                        {
                            return $"null";
                        }

                        if (Value is JsonValue)
                        {
                            return Value.ToJsonString();
                        }

                        if (Value is JsonObject jsonObject)
                        {
                            return $"JsonObject[{jsonObject.Dictionary.Count}]";
                        }

                        JsonArray jsonArray = (JsonArray)Value;
                        return $"JsonArray[{jsonArray.List.Count}]";
                    }
                }
            }
        }
    }
}
