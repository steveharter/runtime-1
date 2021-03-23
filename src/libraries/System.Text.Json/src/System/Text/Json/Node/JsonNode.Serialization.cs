// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.IO;
using System.Text.Json.Serialization.Converters;
using System.Threading;
using System.Threading.Tasks;

namespace System.Text.Json.Node
{
    public abstract partial class JsonNode
    {
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="nodeOptions"></param>
        /// <returns></returns>
        public static JsonNode? Parse(
            ref Utf8JsonReader reader,
            JsonNodeOptions? nodeOptions = null)
        {
            JsonElement element = JsonElement.ParseValue(ref reader);
            return JsonNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="json"></param>
        /// <param name="nodeOptions"></param>
        /// <param name="documentOptions"></param>
        /// <returns></returns>
        public static JsonNode? Parse(
            string json,
            JsonNodeOptions? nodeOptions = null,
            JsonDocumentOptions documentOptions = default(JsonDocumentOptions))
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            JsonElement element = JsonElement.ParseValue(json, documentOptions);
            return JsonNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="utf8Json"></param>
        /// <param name="nodeOptions"></param>
        /// <param name="documentOptions"></param>
        /// <returns></returns>
        public static JsonNode? ParseUtf8Bytes(
            ReadOnlySpan<byte> utf8Json,
            JsonNodeOptions? nodeOptions = null,
            JsonDocumentOptions documentOptions = default(JsonDocumentOptions))
        {
            JsonElement element = JsonElement.ParseValue(utf8Json, documentOptions);
            return JsonNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="utf8Json"></param>
        /// <param name="nodeOptions"></param>
        /// <param name="documentOptions"></param>
        /// <returns></returns>
        public static JsonNode? ParseUtf8(
            Stream utf8Json,
            JsonNodeOptions? nodeOptions = null,
            JsonDocumentOptions documentOptions = default)
        {
            if (utf8Json == null)
            {
                throw new ArgumentNullException(nameof(utf8Json));
            }

            JsonElement element = JsonElement.ParseValue(utf8Json, documentOptions);
            return JsonNodeConverter.Create(element, nodeOptions);
        }

        /// <summary>
        ///   Converts the current instance to string in JSON format.
        /// </summary>
        /// <param name="options"></param>
        /// <returns>JSON representation of current instance.</returns>
        public string ToJsonString(JsonSerializerOptions? options = null)
        {
            var output = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(output, options == null ? default(JsonWriterOptions) : options.GetWriterOptions()))
            {
                WriteTo(writer, options);
            }
            return JsonHelpers.Utf8GetString(output.WrittenSpan);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Special case for string; don't quote it.
            if (this is JsonValue)
            {
                if (this is JsonValue<string> jsonString)
                {
                    return jsonString.Value;
                }

                if (this is JsonValue<JsonElement> jsonElement &&
                    jsonElement.Value.ValueKind == JsonValueKind.String)
                {
                    return jsonElement.Value.GetString()!;
                }
            }

            var options = new JsonWriterOptions { Indented = true };
            var output = new ArrayBufferWriter<byte>();

            using (var writer = new Utf8JsonWriter(output, options))
            {
                WriteTo(writer);
            }

            return JsonHelpers.Utf8GetString(output.WrittenSpan);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="options"></param>
        public abstract void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions? options = null);
    }
}
