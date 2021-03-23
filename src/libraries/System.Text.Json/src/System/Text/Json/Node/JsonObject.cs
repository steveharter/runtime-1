// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization.Converters;

namespace System.Text.Json.Node
{
    /// <summary>
    /// Supports dynamic objects.
    /// </summary>
    [DebuggerDisplay("JsonObject[{Dictionary.Count}]")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed partial class JsonObject : JsonNode
    {
        private JsonElement? _jsonElement;
        private IDictionary<string, JsonNode?>? _value;
        private string? _lastKey;
        private JsonNode? _lastValue;

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="options"></param>
        public JsonObject(JsonNodeOptions? options = null) : base(options) { }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="element"></param>
        /// <param name="options"></param>
        public JsonObject(JsonElement element, JsonNodeOptions? options = null) : base(options)
        {
            _jsonElement = element;
        }

        /// <summary>
        /// todo
        /// </summary>
        internal IDictionary<string, JsonNode?> Dictionary
        {
            get
            {
                CreateNodes();
                Debug.Assert(_value != null);
                return _value;
            }
        }

        internal JsonNode? GetItem(string propertyName)
        {
            if (TryGetPropertyValue(propertyName, out JsonNode? value))
            {
                return value;
            }

            // Return null for missing properties.
            return null;
        }

        internal override void GetPath(List<string> path, JsonNode? child)
        {
            if (child != null)
            {
                bool found = false;

                foreach (KeyValuePair<string, JsonNode?> kvp in Dictionary)
                {
                    if (kvp.Value == child)
                    {
                        string propertyName = kvp.Key;
                        if (propertyName.IndexOfAny(ReadStack.SpecialCharacters) != -1)
                        {
                            path.Add($"['{propertyName}']");
                        }
                        else
                        {
                            path.Add($".{propertyName}");
                        }

                        found = true;
                        break;
                    }
                }

                Debug.Assert(found);
            }

            if (Parent != null)
            {
                Parent.GetPath(path, this);
            }
        }

        internal void SetItem(string propertyName, JsonNode? value)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (value != null)
            {
                value.AssignParent(this);
            }

            Dictionary[propertyName] = value;
            _lastKey = propertyName;
            _lastValue = value;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="result"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal override bool TryConvert(Type returnType, out object? result, JsonSerializerOptions? options = null)
        {
            if (returnType.IsAssignableFrom(typeof(IDictionary<string, JsonNode?>)))
            {
                result = this;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        ///   Returns the value of a property with the specified name.
        /// </summary>
        /// <param name="propertyName">Name of the property to return.</param>
        /// <param name="jsonNode">The JSON value of the property with the specified name.</param>
        /// <returns>
        ///  <see langword="true"/> if a property with the specified name was found;
        ///  otherwise, <see langword="false"/>
        /// </returns>
        public bool TryGetPropertyValue(string propertyName, out JsonNode? jsonNode)
        {
            if (propertyName == _lastKey)
            {
                // Optimize for repeating sections in code:
                // obj.Foo.Bar.One
                // obj.Foo.Bar.Two
                jsonNode = _lastValue;
                return true;
            }

            bool rc = Dictionary.TryGetValue(propertyName, out jsonNode);
            _lastKey = propertyName;
            _lastValue = jsonNode;
            return rc;
        }

        private void CreateNodes()
        {
            if (_value == null)
            {
                bool caseInsensitive = false;
                if (Options?.PropertyNameCaseInsensitive == true)
                {
                    caseInsensitive = true;
                }

                var dictionary = new Dictionary<string, JsonNode?>(
                    caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

                if (_jsonElement != null)
                {
                    JsonElement jElement = _jsonElement.Value;
                    foreach (JsonProperty property in jElement.EnumerateObject())
                    {
                        JsonNode? node = JsonNodeConverter.Create(property.Value, Options);
                        if (node != null)
                        {
                            node.Parent = this;
                        }

                        dictionary.Add(property.Name, node);
                    }

                    // Clear since no longer needed.
                    _jsonElement = null;
                }

                _value = dictionary;
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
                options ??= JsonSerializerOptions.s_defaultOptions;

                writer.WriteStartObject();

                foreach (KeyValuePair<string, JsonNode?> kvp in Dictionary)
                {
                    // todo: check for null against options and skip
                    writer.WritePropertyName(kvp.Key);
                    JsonNodeConverter.Default.Write(writer, kvp.Value!, options);
                }

                writer.WriteEndObject();
            }
        }

        private class DebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private JsonObject _node;

            public DebugView(JsonObject node)
            {
                _node = node;
            }

            public string Json => _node.ToJsonString();
            public string Path => _node.GetPath();

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            private DebugViewProperty[] Items
            {
                get
                {
                    DebugViewProperty[] properties = new DebugViewProperty[_node.Dictionary.Count];

                    int i = 0;
                    foreach (KeyValuePair<string, JsonNode?> property in _node.Dictionary)
                    {
                        properties[i].Value = property.Value;
                        properties[i].PropertyName = property.Key;
                        i++;
                    }

                    return properties;
                }
            }
        }

        [DebuggerDisplay("{Display,nq}")]
        private struct DebugViewProperty
        {
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public JsonNode? Value;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public string PropertyName;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public string Display
            {
                get
                {
                    if (Value == null)
                    {
                        return $"{PropertyName} = null";
                    }

                    if (Value is JsonValue)
                    {
                        return $"{PropertyName} = {Value.ToJsonString()}";
                    }

                    if (Value is JsonObject jsonObject)
                    {
                        return $"{PropertyName} = JsonObject[{jsonObject.Dictionary.Count}]";
                    }

                    JsonArray jsonArray = (JsonArray)Value;
                    return $"{PropertyName} = JsonArray[{jsonArray.List.Count}]";
                }
            }
        }
    }
}
