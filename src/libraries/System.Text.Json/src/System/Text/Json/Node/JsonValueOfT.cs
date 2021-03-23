// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Text.Json.Node
{
    /// <summary>
    /// Supports dynamic numbers.
    /// </summary>
    [DebuggerDisplay("{ToJsonString(),nq}")]
    [DebuggerTypeProxy(typeof(JsonValue<>.DebugView))]
    internal sealed partial class JsonValue<TValue> : JsonValue
    {
        internal readonly TValue _value; // keep a field for direct access to avoid copies

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public JsonValue(TValue value, JsonNodeOptions? options = null) : base(options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value is JsonNode)
            {
                throw new ArgumentException("todo", nameof(value));
            }

            _value = value;
        }

        /// <summary>
        /// todo
        /// </summary>
        public TValue Value
        {
            get
            {
                return _value;
            }
        }

        /// <inheritdoc/>
        public override TypeToReturn GetValue<TypeToReturn>(JsonSerializerOptions? options = null)
        {
            if (TryConvert(out TypeToReturn result, options))
            {
                return result!;
            }

            throw new InvalidOperationException($"Cannot change type {_value!.GetType()} to {typeof(TypeToReturn)}.");
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override bool TryGetValue<T>(out T value, JsonSerializerOptions? options = null)
        {
            return TryConvert<T>(out value, options);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="options"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal override bool TryConvert(Type returnType, out object? result, JsonSerializerOptions? options = null)
        {
            // If no conversion is needed, just return the raw value.
            if (returnType == _value?.GetType())
            {
                result = _value;
                return true;
            }

            if (_value is JsonElement jsonElement)
            {
                if (TryConvertJsonElement(jsonElement, returnType, options, out result))
                {
                    return true;
                }

                //// Use the raw bytes which may be recognized by converters such as the Enum converter than can process numbers.
                //ReadOnlySpan<byte> span = jsonElement.GetRawUtf8();
                //result = JsonSerializer.Deserialize(span, returnType, Options)!;
                //return true;
            }

            result = default;
            return false;

            //try
            //{
            //    if (_value == null)
            //    {
            //        result = JsonSerializer.Deserialize($"null", returnType, Options);
            //    }
            //    else if (_value is string strValue)
            //    {
            //        result = JsonSerializer.Deserialize($"\"{strValue}\"", returnType, Options);
            //    }
            //    else if (_value.GetType() == typeof(ReadOnlySpan<byte>))
            //    {
            //        // todo: cannot cast to a ref struct
            //        throw new NotImplementedException("ref struct");
            //    }
            //    else if (_value.GetType() == typeof(ReadOnlySpan<char>))
            //    {
            //        // todo: cannot cast to a ref struct
            //        throw new NotImplementedException("ref struct");
            //    }
            //    else
            //    {
            //        throw new JsonException("Unsupported conversion");
            //    }
            //}
            //catch (JsonException)
            //{
            //    result = default;
            //    return false;
            //}
            // return true;
        }

        private bool TryConvert<TypeToConvert>(out TypeToConvert result, JsonSerializerOptions? options = null)
        {
            Type returnType = typeof(TypeToConvert);

            // If no conversion is needed, just return the raw value.
            if (_value is TypeToConvert value)
            {
                result = value;
                return true;
            }

            if (_value is JsonElement jsonElement)
            {
                try
                {
                    if (TryConvertJsonElement<TypeToConvert>(out result))
                    {
                        return true;
                    }

                    // Use the raw bytes which may be recognized by converters such as the Enum converter than can process numbers.
                    ReadOnlySpan<byte> span = jsonElement.GetRawUtf8();
                    result = JsonSerializer.Deserialize<TypeToConvert>(span, options)!;
                    return true;
                }
                catch (JsonException)
                {
                    result = default!;
                    return false;
                }
            }

            // Attempt to cast.
            // Generics (and boxing) do not support standard cast operators say from 'long' to 'int', so this will throw InvalidCastException.
            result = (TypeToConvert)(object)_value!;

            return true;
        }

        private bool TryConvertJsonElement(in JsonElement jsonElement, Type returnType, JsonSerializerOptions? options, out object? result)
        {
            bool success = false;
            result = default;

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Number:
                    if (returnType == typeof(int))
                    {
                        success = jsonElement.TryGetInt32(out int value);
                        result = value;
                    }
                    else if (returnType == typeof(long))
                    {
                        success = jsonElement.TryGetInt64(out long value);
                        result = value;
                    }
                    else if (returnType == typeof(double))
                    {
                        success = jsonElement.TryGetDouble(out double value);
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

                    break;
                case JsonValueKind.String:
                    if (returnType == typeof(string))
                    {
                        result = jsonElement.GetString();
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

                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    if (returnType == typeof(bool))
                    {
                        result = jsonElement.GetBoolean();
                        success = true;
                    }
                    break;

                default:
                    break;
            }

            if (success)
            {
                return success;
            }

            // Use the raw test which may be recognized by converters such as the Enum converter than can process numbers.
            string strValue = jsonElement.GetRawText();

            try
            {
                result = JsonSerializer.Deserialize($"{strValue}", returnType, options);
            }
            catch (JsonException)
            {
                return false;
            }

            return true;
        }

        internal bool TryConvertJsonElement<TypeToConvert>(out TypeToConvert result)
        {
            bool success;

            JsonElement element = (JsonElement)(object)_value!;
            Type returnType = typeof(TypeToConvert);

            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    if (returnType == typeof(int))
                    {
                        success = element.TryGetInt32(out int value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(long))
                    {
                        success = element.TryGetInt64(out long value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(double))
                    {
                        success = element.TryGetDouble(out double value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(short))
                    {
                        success = element.TryGetInt16(out short value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(decimal))
                    {
                        success = element.TryGetDecimal(out decimal value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(byte))
                    {
                        success = element.TryGetByte(out byte value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(float))
                    {
                        success = element.TryGetSingle(out float value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    else if (returnType == typeof(uint))
                    {
                        success = element.TryGetUInt32(out uint value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(ushort))
                    {
                        success = element.TryGetUInt16(out ushort value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(ulong))
                    {
                        success = element.TryGetUInt64(out ulong value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }

                    if (returnType == typeof(sbyte))
                    {
                        success = element.TryGetSByte(out sbyte value);
                        result = (TypeToConvert)(object)value;
                        return success;
                    }
                    break;

                case JsonValueKind.String:
                    if (returnType == typeof(string))
                    {
                        result = (TypeToConvert)(object)element.GetString()!; // todo: nullability
                        return true;
                    }

                    if (returnType == typeof(DateTime))
                    {
                        result = (TypeToConvert)(object)element.GetDateTime();
                        return true;
                    }

                    if (returnType == typeof(DateTimeOffset))
                    {
                        result = (TypeToConvert)(object)element.GetDateTimeOffset();
                        return true;
                    }

                    if (returnType == typeof(Guid))
                    {
                        result = (TypeToConvert)(object)element.GetGuid();
                        return true;
                    }
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    if (returnType == typeof(bool))
                    {
                        result = (TypeToConvert)(object)element.GetBoolean();
                        return true;
                    }
                    break;
            }

            result = default!;
            return false;
        }

        /// <summary>
        ///   Converts the text value of this instance, which should encode binary data as base-64 digits, to an equivalent 8-bit unsigned <see cref="byte"/> array.
        ///   The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">
        ///   When this method returns, contains the <see cref="byte"/> array equivalent of the text contained in this instance,
        ///   if the conversion succeeded.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if text was converted successfully; otherwise returns <see langword="false"/>.
        /// </returns>
        internal bool TryGetBytesFromBase64([NotNullWhen(true)] out byte[]? value)
        {
            Debug.Assert(this is JsonValue<string>);

            var jsonValue = (JsonValue<string>)(object)this;

            string? strValue = jsonValue._value;

            // Shortest length of a base-64 string is 4 characters.
            if (strValue == null || strValue.Length < 4)
            {
                value = default;
                return false;
            }

            Debug.Assert(strValue != null);

#if BUILDING_INBOX_LIBRARY
            // we decode string -> byte, so the resulting length will
            // be /4 * 3 - padding. To be on the safe side, keep padding and slice later
            int bufferSize = strValue.Length / 4 * 3;

            byte[]? arrayToReturnToPool = null;
            Span<byte> buffer = bufferSize <= JsonConstants.StackallocThreshold
                ? stackalloc byte[JsonConstants.StackallocThreshold]
                : arrayToReturnToPool = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                if (System.Convert.TryFromBase64String(strValue, buffer, out int bytesWritten))
                {
                    buffer = buffer.Slice(0, bytesWritten);
                    value = buffer.ToArray();
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }
            finally
            {
                if (arrayToReturnToPool != null)
                {
                    buffer.Clear();
                    ArrayPool<byte>.Shared.Return(arrayToReturnToPool);
                }
            }

#else
            try
            {
                value = System.Convert.FromBase64String(strValue);
                return true;
            }
            catch (FormatException)
            {
                value = null;
                return false;
            }
#endif
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

            if (_value is JsonElement jsonElement)
            {
                jsonElement.WriteTo(writer);
            }
            else
            {
                JsonSerializer.Serialize(writer, _value, _value!.GetType(), options);
            }
        }
        [DebuggerDisplay("{Json,nq}")]
        private class DebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public JsonValue<TValue> _node;

            public DebugView(JsonValue<TValue> node)
            {
                _node = node;
            }

            public string Json => _node.ToJsonString();
            public string Path => _node.GetPath();
            public TValue? Value => _node.Value;
        }
    }
}
