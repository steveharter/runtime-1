// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace System.Text.Json.Serialization.Samples
{
    /// <summary>
    /// This class is intended as a sample for supporting the <see langword="dynamic"/> feature.
    /// </summary>
    /// <remarks>
    /// It requires a reference to the "System.Linq.Expressions" assembly.
    /// </remarks>
    public static class JsonSerializerExtensions
    {
        /// <summary>
        /// Enable support for the <see langword="dynamic"/> feature.
        /// Changes the default handling for types specified as <see cref="object"/> from deserializing as
        /// <see cref="System.Text.Json.JsonElement"/> to instead deserializing as the one of the
        /// <see cref="JsonNode"/>-derived types including:
        /// <see cref="JsonObject"/>,
        /// <see cref="JsonArray"/> and
        /// <see cref="JsonValue"/>.
        /// </summary>
        /// <remarks>
        /// When deserializing <see cref="System.Text.Json.JsonTokenType.StartObject"/>, <see cref="JsonObject"/>
        /// is returned which implements <see cref="System.Collections.IDictionary{string, object}"/>.
        /// When deserializing <see cref="System.Text.Json.JsonTokenType.StartArray"/>, <see cref="System.Collections.IList{object}"/>
        /// is returned which implements <see cref="System.Collections.IList{object}"/>.
        /// When deserializing <see cref="System.Text.Json.JsonTokenType.String"/>, <see cref="JsonValue"/> is returned.
        /// An explicit cast or assignment to other types, such as <see cref="System.Text.Json.JsonTokenType.DateTime"/>,
        /// is supported provided there is a custom converter for that Type.
        /// When deserializing <see cref="System.Text.Json.JsonTokenType.Number"/>, <see cref="JsonValue"/> is returned.
        /// An explicit cast or assignment is required to the appropriate number type, such as <see cref="decimal"/> or <see cref="long"/>.
        /// When deserializing <see cref="System.Text.Json.JsonTokenType.True"/> and <see cref="System.Text.Json.JsonTokenType.False"/>,
        /// <see cref="JsonValue"/> is returned.
        /// An explicit cast or assignment to other types is supported provided there is a custom converter for that type.
        /// When deserializing <see cref="System.Text.Json.JsonTokenType.Null"/>, <see langword="null"/> is returned.
        /// </remarks>
        public static void EnableDynamicTypes(this JsonSerializerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Converters.Add(new DynamicObjectConverter());
        }

        /// <summary>
        /// The base class for all dynamic types supported by the serializer.
        /// </summary>
        public abstract class JsonNode : DynamicObject
        {
            public JsonSerializerOptions Options { get; private set; }

            internal JsonNode(JsonSerializerOptions options = null)
            {
                Options = options;
            }
            public abstract T GetValue<T>();

            internal void UpdateOptions(JsonNode node)
            {
                node.Options ??= Options;
            }

            public sealed override bool TryConvert(ConvertBinder binder, out object result)
            {
                return TryConvert(binder.ReturnType, out result);
            }

            protected abstract bool TryConvert(Type returnType, out object result);
        }

        /// <summary>
        /// Supports dynamic numbers.
        /// </summary>
        public sealed class JsonValue : JsonNode
        {
            private Type _type = null;
            private object _value = null;
            private object _lastValue = null;

            public JsonValue(object value, JsonSerializerOptions options = null) : base(options)
            {
                Value = value;
            }

            public override T GetValue<T>()
            {
                if (TryConvert(typeof(T), out object result))
                {
                    return (T)result;
                }

                throw new InvalidOperationException($"Cannot change type {_value.GetType()} to {typeof(T)}.");
            }

            protected override bool TryConvert(Type returnType, out object result)
            {
                if (returnType == _type || returnType == typeof(object))
                {
                    result = _lastValue;
                    return true;
                }

                string strValue;

                if (!(_value is JsonElement jsonElement))
                {
                    strValue = _value is string ? $"\"{_value}\"" : _value.ToString();
                }
                else
                {
                    bool success = false;
                    result = null;

                    if (returnType == typeof(long))
                    {
                        success = jsonElement.TryGetInt64(out long value);
                        result = value;
                    }
                    else if (returnType == typeof(double))
                    {
                        success = jsonElement.TryGetDouble(out double value);
                        result = value;
                    }
                    else if (returnType == typeof(int))
                    {
                        success = jsonElement.TryGetInt32(out int value);
                        result = value;
                    }
                    else if (returnType == typeof(short))
                    {
                        success = jsonElement.TryGetInt16(out short value);
                        result = value;
                    }
                    else if (returnType == typeof(decimal))
                    {
                        success = jsonElement.TryGetDecimal(out decimal value);
                        result = value;
                    }
                    else if (returnType == typeof(byte))
                    {
                        success = jsonElement.TryGetByte(out byte value);
                        result = value;
                    }
                    else if (returnType == typeof(float))
                    {
                        success = jsonElement.TryGetSingle(out float value);
                        result = value;
                    }
                    else if (returnType == typeof(uint))
                    {
                        success = jsonElement.TryGetUInt32(out uint value);
                        result = value;
                    }
                    else if (returnType == typeof(ushort))
                    {
                        success = jsonElement.TryGetUInt16(out ushort value);
                        result = value;
                    }
                    else if (returnType == typeof(ulong))
                    {
                        success = jsonElement.TryGetUInt64(out ulong value);
                        result = value;
                    }
                    else if (returnType == typeof(sbyte))
                    {
                        success = jsonElement.TryGetSByte(out sbyte value);
                        result = value;
                    }
                    else if (returnType == typeof(string))
                    {
                        result = jsonElement.GetString();
                        success = true;
                    }
                    else if (returnType == typeof(bool))
                    {
                        result = jsonElement.GetBoolean();
                        success = true;
                    }
                    else if (returnType == typeof(DateTime))
                    {
                        result = jsonElement.GetDateTime();
                        success = true;
                    }
                    else if (returnType == typeof(DateTimeOffset))
                    {
                        result = jsonElement.GetDateTimeOffset();
                        success = true;
                    }
                    else if (returnType == typeof(Guid))
                    {
                        result = jsonElement.GetGuid();
                        success = true;
                    }

                    if (success)
                    {
                        _lastValue = result;
                        _type = result?.GetType();
                        return true;
                    }
                    else
                    {
                        // Use the raw test which may be recognized by converters such as the Enum converter than can process numbers.
                        strValue = jsonElement.GetRawText();
                    }
                }

                try
                {
                    result = _lastValue = JsonSerializer.Deserialize($"{strValue}", returnType, Options);
                }
                catch (JsonException)
                {
                    result = default;
                    return false;
                }

                _type = _lastValue?.GetType();
                return true;

            }

            public object Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _value = _lastValue = value;
                    _type = value.GetType();
                }
            }
        }

        /// <summary>
        /// Supports dynamic objects.
        /// </summary>
        public sealed class JsonObject : JsonNode, IDictionary<string, object>
        {
            private IDictionary<string, object> _value;

            public JsonObject(JsonSerializerOptions options = null)
                : base(options) { }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (Value.TryGetValue(binder.Name, out result))
                {
                    return true;
                }

                // Return null for missing properties.
                result = null;
                return true;
            }

            public override T GetValue<T>()
            {
                Type type = typeof(T);

                if (type == typeof(object) || type == typeof(IDictionary<string, object>))
                {
                    return (T)Value;
                }

                throw new NotImplementedException("GetValue<> currently not implemented");
            }

            private IDictionary<string, object> Value
            {
                get
                {
                    if (_value == null)
                    {
                        bool caseInsensitive = false;
                        if (Options?.PropertyNameCaseInsensitive == true)
                        {
                            caseInsensitive = true;
                        }

                        _value = new Dictionary<string, object>(caseInsensitive ?
                            StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
                    }

                    return _value;
                }
            }

            protected override bool TryConvert(Type returnType, out object result)
            {
                if (returnType.IsAssignableFrom(typeof(IDictionary<string, JsonNode>)))
                {
                    result = this;
                    return true;
                }

                result = null;
                return false;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                Value[binder.Name] = value;
                return true;
            }

            // IDictionary members.
            public void Add(string key, object value)
            {
                if (value is JsonNode jNode)
                {
                    jNode.UpdateOptions(this);
                }

                Value.Add(key, value);
            }

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                if (item.Value is JsonNode jNode)
                {
                    jNode.UpdateOptions(this);
                }

                Value.Add(item);
            }

            public void Clear() => Value.Clear();
            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => Value.Contains(item);
            public bool ContainsKey(string key) => Value.ContainsKey(key);
            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => Value.CopyTo(array, arrayIndex);
            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Value.GetEnumerator();
            public bool Remove(string key) => Value.Remove(key);
            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => Value.Remove(item);

            public object this[string key]
            {
                get => Value[key];
                set
                {
                    if (key == null)
                    {
                        throw new ArgumentNullException(nameof(key));
                    }

                    if (value is JsonNode jNode)
                    {
                        jNode.UpdateOptions(this);
                    }

                    Value[key] = value;
                }
            }

            ICollection<string> IDictionary<string, object>.Keys => Value.Keys;
            ICollection<object> IDictionary<string, object>.Values => Value.Values;
            public int Count => Value.Count;
            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => Value.IsReadOnly;
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
            public bool TryGetValue(string key, out object value) => Value.TryGetValue(key, out value);
        }

        /// <summary>
        /// Supports dynamic arrays.
        /// </summary>
        public sealed class JsonArray : JsonNode, IList<object>
        {
            private IList<object> _value;

            public JsonArray(JsonSerializerOptions options = null) : base(options)
            {
                _value = new List<object>();
            }

            public override T GetValue<T>()
            {
                Type type = typeof(T);

                if (type == typeof(object) || type == typeof(IList<object>))
                {
                    return (T)Value;
                }

                throw new NotImplementedException("GetValue<> currently not implemented");
            }

            protected override bool TryConvert(Type returnType, out object result)
            {
                if (returnType.IsAssignableFrom(typeof(IList<object>)))
                {
                    result = _value;
                    return true;
                }

                result = null;
                return false;
            }

            private IList<object> Value => _value;

            // IList members.
            public object this[int index]
            {
                get => _value[index];
                set
                {
                    if (value is JsonNode jNode)
                    {
                        jNode.UpdateOptions(this);
                    }

                    _value[index] = value;
                }
            }

            public int Count => _value.Count;
            bool ICollection<object>.IsReadOnly => _value.IsReadOnly;

            public void Add(object item)
            {
                if (item is JsonNode jNode)
                {
                    jNode.UpdateOptions(this);
                }

                _value.Add(item);
            }

            public void Clear() => _value.Clear();
            public bool Contains(object item) => _value.Contains(item);
            void ICollection<object>.CopyTo(object[] array, int arrayIndex) => _value.CopyTo(array, arrayIndex);
            public IEnumerator<object> GetEnumerator() => _value.GetEnumerator();
            public int IndexOf(object item) => _value.IndexOf(item);

            public void Insert(int index, object item)
            {
                if (item is JsonNode jNode)
                {
                    jNode.UpdateOptions(this);
                }

                _value.Insert(index, item);
            }

            public bool Remove(object item) => _value.Remove(item);
            public void RemoveAt(int index) => _value.RemoveAt(index);
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_value).GetEnumerator();
        }

        /// <summary>
        /// Supports deserialization of all <see cref="object"/>-declared types, supporting <see langword="dynamic"/>.
        /// supports serialization of all <see cref="JsonNode"/>-derived types.
        /// </summary>
        private sealed class DynamicObjectConverter : JsonConverter<object>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                // For simplicity in adding the converter, we use a single converter instead of two.
                return typeToConvert == typeof(object) ||
                    typeof(JsonNode).IsAssignableFrom(typeToConvert);
            }

            public override sealed object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        return new JsonValue(reader.GetString(), options);
                    case JsonTokenType.StartArray:
                        var dynamicArray = new JsonArray(options);
                        ReadList(dynamicArray, ref reader, options);
                        return dynamicArray;
                    case JsonTokenType.StartObject:
                        var dynamicObject = new JsonObject(options);
                        ReadObject(dynamicObject, ref reader, options);
                        return dynamicObject;
                    case JsonTokenType.False:
                        return new JsonValue(false, options);
                    case JsonTokenType.True:
                        return new JsonValue(true, options);
                    case JsonTokenType.Number:
                        JsonElement jsonElement;
                        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
                        {
                            jsonElement = document.RootElement.Clone();
                        }
                        // In 6.0, this can be used instead for increased performance:
                        //JsonElement jsonElement = JsonElement.ParseValue(ref reader);
                        return new JsonValue(jsonElement, options);
                    case JsonTokenType.Null:
                        return null;
                    default:
                        throw new JsonException("Unexpected token type.");
                }
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                if (value is JsonNode jNode)
                {
                    value = jNode.GetValue<object>();
                }

                JsonSerializer.Serialize(writer, value, typeof(object), options);
            }

            private void ReadList(JsonArray dynamicArray, ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                while (true)
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    JsonNode value = (JsonNode)Read(ref reader, typeof(object), options);
                    dynamicArray.Add(value);
                }
            }

            private void ReadObject(JsonObject dynamicObject, ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                while (true)
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string key = reader.GetString();

                    reader.Read();
                    JsonNode value = (JsonNode)Read(ref reader, typeof(JsonNode), options);
                    dynamicObject.Add(key, value);
                }
            }
        }
    }
}
