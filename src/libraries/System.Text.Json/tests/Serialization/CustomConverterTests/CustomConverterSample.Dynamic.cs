// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace System.Text.Json.Serialization.Tests
{
    /// <summary>
    /// Used to represent a JSON object.
    /// </summary>
    internal class JsonObject : Dictionary<string, object>
    {
        // todo: implement IDictionary<string, object> and forward to Dictionary<string, object>

        /// <summary>
        /// A shortcut to obtain an array value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns></returns>
        public JsonArray GetArray(string propertyName)
        {
            return (JsonArray)this[propertyName];
        }

        /// <summary>
        /// A shortcut to obtain an array value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns></returns>
        public JsonObject GetObject(string propertyName)
        {
            return (JsonObject)this[propertyName];
        }
    }

    /// <summary>
    /// Used to represent a JSON array.
    /// </summary>
    internal class JsonArray : List<object>
    {
        // todo: implement IList<object> and forward to List<object>.

        /// <summary>
        /// A shortcut to obtain an array value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns></returns>
        public JsonArray GetArray(int index)
        {
            return (JsonArray)this[index];
        }

        /// <summary>
        /// A shortcut to obtain an array value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns></returns>
        public JsonObject GetObject(int index)
        {
            return (JsonObject)this[index];
        }
    }

    /// <summary>
    /// Determines how JSON is deserialized when the target type is <see cref="object"/>.
    /// </summary>
    internal enum SystemObjectDeserializationPolicy
    {
        /// <summary>
        /// The default behavior. <see cref="System.Text.Json.JsonElement"/> is deserialized for all JSON except <see cref="System.Text.Json.JsonTokenType.Null"/>.
        /// For <see cref="System.Text.Json.JsonTokenType.Null"/>, <see langword="null"/> is returned.
        /// This supports an explicit model where the caller needs to know the specific primitive types.
        /// </summary>
        JsonElement = 0,

        /// <summary>
        /// JSON objects support <see langword="dynamic"/>.
        /// JSON primitives use <see cref="System.Text.Json.JsonElement"/>.
        /// JSON arrays use JsonArray.
        /// Note this loads the assembly "System.Text.RegularExpressions.dll".
        /// This supports an explicit model where the caller needs to know the specific primitive type.
        /// For <see cref="System.Text.Json.JsonTokenType.StartObject"/>, <see cref="System.Dynamic.ExpandoObject"/> is returned
        /// which allows for usage of the <see langword="dynamic"/> feature.
        /// For <see cref="System.Text.Json.JsonTokenType.StartArray"/>, <see cref="System.Collections.IList<object>"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.Null"/>, <see langword="null"/> is returned.
        /// For all other tokens, <see cref="System.Text.Json.JsonElement"/> is returned.
        /// </summary>
        JsonElementDynamic = 1,

        /// <summary>
        /// JSON objects use JsonObject.
        /// JSON primitives are converted to their respective types. This mapping is not exact.
        /// JSON arrays use JsonArray.
        /// <see cref="System.Text.Json.JsonElement"/> is not used.
        /// For <see cref="System.Text.Json.JsonTokenType.StartObject"/>, <see cref="JsonObject"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.StartArray"/>, <see cref="JsonArray"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.String"/>, <see cref="System.DateTime"/> is returned if possible otherwise <see cref="string"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.Number"/>, <see cref="System.Int64"/> is returned if possible otherwise <see cref="System.Double"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.True"/> and <see cref="System.Text.Json.JsonTokenType.False"/>, <see cref="System.Boolean"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.Null"/>, <see langword="null"/> is returned.
        /// </summary>
        AutoConversion = 2,

        /// <summary>
        /// JSON objects support <see langword="dynamic"/>.
        /// JSON primitives are converted to their respective types. This mapping is not exact.
        /// JSON arrays use JsonArray.
        /// Note this loads the assembly "System.Text.RegularExpressions.dll".
        /// <see cref="System.Text.Json.JsonElement"/> is not used.
        /// For <see cref="System.Text.Json.JsonTokenType.StartObject"/>, <see cref="System.Dynamic.ExpandoObject"/> is returned
        /// which allows for usage of the <see langword="dynamic"/> feature.
        /// For <see cref="System.Text.Json.JsonTokenType.StartArray"/>, <see cref="JsonArray"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.String"/>, <see cref="System.DateTime"/> is returned if possible otherwise <see cref="string"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.Number"/>, <see cref="System.Int64"/> is returned if possible otherwise <see cref="System.Double"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.True"/> and <see cref="System.Text.Json.JsonTokenType.False"/>, <see cref="System.Boolean"/> is returned.
        /// For <see cref="System.Text.Json.JsonTokenType.Null"/>, <see langword="null"/> is returned.
        /// </summary>
        AutoConversionDynamic = 3
    }

    internal static class JsonSerializerExtensions
    {
        // Options for delay-loading System.Linq.Expressions.dll (SLE):
        // 1) Add a System.Text.Extensions.dll assembly with extension method and converters like what exists
        //  in this project. System.Text.Json.dll does not reference SLE.
        // 2) Put both the converters and knob in System.Text.Json.dll.
        // 2a) Add policy property to JsonSerializerOptions and when set, dynamically load SLE,
        // find the converter Types, instantiate, and add them to the option instance.
        // 2b) Reference SLE from System.Text.Json.dll.
        // Use the linker functionality to remove the reference to SLE in some way,
        // perhaps by adding a new JsonSerializerOptions(bool useDynamicMode) ctor that adds the dynamic
        // converter which is linked out if not called.
        public static void SetSystemObjectDeserializationPolicy(this JsonSerializerOptions options, SystemObjectDeserializationPolicy value)
        {
            if (value == SystemObjectDeserializationPolicy.JsonElementDynamic ||
                value == SystemObjectDeserializationPolicy.AutoConversionDynamic)
            {
                options.Converters.Add(new ExpandoObjectConverter());
            }

            if (value != SystemObjectDeserializationPolicy.JsonElement)
            {
                options.Converters.Add(new SystemObjectConverter(value));
            }
        }
    }

    internal sealed class SystemObjectConverter : JsonConverter<object>
    {
        private SystemObjectDeserializationPolicy _policy;
        JsonConverter<JsonArray> JsonArrayConverter { get; set; }
        JsonConverter<ExpandoObject> ExpandoObjectConverter { get; set; }
        JsonConverter<JsonObject> JsonObjectConverter { get; set; }

        public SystemObjectConverter(SystemObjectDeserializationPolicy policy)
        {
            if (policy != SystemObjectDeserializationPolicy.JsonElementDynamic &&
                policy != SystemObjectDeserializationPolicy.AutoConversion &&
                policy != SystemObjectDeserializationPolicy.AutoConversionDynamic)
            {
                throw new NotSupportedException($"SystemObjectDeserializationPolicy enum value '{policy}'not known.");
            }

            _policy = policy;
        }

        public override sealed object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return GetString(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return GetNumber(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return GetArray(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return GetObject(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return GetFalse(ref reader, options);
            }

            if (reader.TokenType == JsonTokenType.True)
            {
                return GetTrue(ref reader, options);
            }

            Debug.Fail("Unexpected token type.");
            throw new JsonException();
        }

        private object GetArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (JsonArrayConverter == null)
            {
                JsonArrayConverter = (JsonConverter<JsonArray>)options.GetConverter(typeof(JsonArray));
            }

            return JsonArrayConverter.Read(ref reader, typeof(JsonArray), options);
        }

        private object GetNumber(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (_policy == SystemObjectDeserializationPolicy.JsonElementDynamic)
            {
                return GetJsonElement(ref reader, options);
            }

            // Ideally we call into the configured Int64 converter, but there is no "Try" mechanism.
            if (reader.TryGetInt64(out long l))
            {
                return l;
            }

            return reader.GetDouble();
        }

        private object GetString(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (_policy == SystemObjectDeserializationPolicy.JsonElementDynamic)
            {
                return GetJsonElement(ref reader, options);
            }

            // Ideally we call into the configured DateTime converter, but there is no "Try" mechanism.
            if (reader.TryGetDateTime(out DateTime datetime))
            {
                return datetime;
            }

            return reader.GetString();
        }

        private object GetObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (_policy == SystemObjectDeserializationPolicy.AutoConversion)
            {
                if (JsonObjectConverter == null)
                {
                    JsonObjectConverter = (JsonConverter<JsonObject>)options.GetConverter(typeof(JsonObject));
                }

                return JsonObjectConverter.Read(ref reader, typeof(JsonObject), options);
            }

            if (ExpandoObjectConverter == null)
            {
                ExpandoObjectConverter = (JsonConverter<ExpandoObject>)options.GetConverter(typeof(ExpandoObject));
            }

            // This will either be a configured ExpandoObject converter, or the default implementation of IDictionary<string, object>.

            return ExpandoObjectConverter.Read(ref reader, typeof(ExpandoObject), options);
        }

        private object GetFalse(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (_policy == SystemObjectDeserializationPolicy.JsonElementDynamic)
            {
                return GetJsonElement(ref reader, options);
            }

            return false;
        }

        private object GetTrue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (_policy == SystemObjectDeserializationPolicy.JsonElementDynamic)
            {
                return GetJsonElement(ref reader, options);
            }

            return true;
        }

        private object GetJsonElement(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                return document.RootElement.Clone();
            }
        }

        public override sealed void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException("Should not get here.");
        }
    }


    internal sealed class ExpandoObjectConverter : JsonConverter<ExpandoObject>
    {
        private JsonConverter<IDictionary<string, object>> Converter { get; set; }
        private JsonSerializerOptions _options;

        JsonConverter<IDictionary<string, object>> GetConverter(JsonSerializerOptions options)
        {
            if (Converter == null)
            {
                _options = options;
                Converter = (JsonConverter<IDictionary<string, object>>)options.GetConverter(typeof(IDictionary<string, object>));
            }
            else if (_options != options)
            {
                throw new JsonException("ExpandoObjectConverter should not be shared across JsonSerializerOptions instances.");
            }

            return Converter;
        }

        public override sealed ExpandoObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            ExpandoObject expando = new ExpandoObject();
            IDictionary<string, object> expandoDictionary = expando;

            // Deserialize into Dictionary and then copy the values to the expando instance.
            // We could write a loop to process each property that would avoid the temporary Dictionary instance,
            // but that would have issues with reporting Path in JsonException as well as skip fast-path branches.
            IDictionary<string, object> allProps = GetConverter(options).Read(ref reader, typeof(IDictionary<string, object>), options);
            foreach (KeyValuePair<string, object> item in allProps)
            {
                expandoDictionary[item.Key] = item.Value;
            }

            return expando;
        }

        public override void Write(Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options)
        {
            GetConverter(options).Write(writer, value, options);
        }
    }
}
