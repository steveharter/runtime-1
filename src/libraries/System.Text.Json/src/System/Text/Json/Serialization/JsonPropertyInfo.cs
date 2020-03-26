// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    /// <summary>
    /// todo
    /// </summary>
    [DebuggerDisplay("PropertyInfo={PropertyInfo}")]
    public abstract class JsonPropertyInfo
    {
        internal static readonly JsonPropertyInfo s_missingProperty = GetPropertyPlaceholder();

        private string? _nameAsString;
        private JsonClassInfo? _runtimeClassInfo;

        internal ClassType ClassType;

        internal JsonPropertyInfo()
        {
            // prevent non-internal derived classes.
        }

        internal abstract JsonConverter ConverterBase { get; set; }

        internal static JsonPropertyInfo GetPropertyPlaceholder()
        {
            JsonPropertyInfo info = new JsonPropertyInfo<object>();
            info.IsPropertyPolicy = false;
            info.ShouldDeserialize = false;
            info.ShouldSerialize = false;
            return info;
        }

        // Create a property that is ignored at run-time. It uses the same type (typeof(sbyte)) to help
        // prevent issues with unsupported types and helps ensure we don't accidently (de)serialize it.
        internal static JsonPropertyInfo CreateIgnoredPropertyPlaceholder(PropertyInfo propertyInfo, JsonSerializerOptions options)
        {
            JsonPropertyInfo jsonPropertyInfo = new JsonPropertyInfo<sbyte>();
            jsonPropertyInfo.Options = options;
            jsonPropertyInfo.DeterminePropertyName(propertyInfo);
            jsonPropertyInfo.IsIgnored = true;

            Debug.Assert(!jsonPropertyInfo.ShouldDeserialize);
            Debug.Assert(!jsonPropertyInfo.ShouldSerialize);

            return jsonPropertyInfo;
        }

        internal Type DeclaredPropertyType { get; private set; } = null!;

        private void DeterminePropertyName(PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
            {
                return;
            }

            ClrNameAsString = propertyInfo.Name;

            JsonPropertyNameAttribute? nameAttribute = GetAttribute<JsonPropertyNameAttribute>(propertyInfo);
            if (nameAttribute != null)
            {
                string name = nameAttribute.Name;
                if (name == null)
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameNull(ParentClassType, this);
                }

                NameAsString = name;
            }
            else if (Options.PropertyNamingPolicy != null)
            {
                string name = Options.PropertyNamingPolicy.ConvertName(propertyInfo.Name);
                if (name == null)
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameNull(ParentClassType, this);
                }

                NameAsString = name;
            }
            else
            {
                NameAsString = propertyInfo.Name;
            }

            Debug.Assert(NameAsString != null);
        }

        private void DetermineSerializationCapabilities()
        {
            if ((ClassType & (ClassType.Enumerable | ClassType.Dictionary)) == 0)
            {
                // We serialize if there is a getter + not ignoring readonly properties.
                ShouldSerialize = HasGetter && (HasSetter || !Options.IgnoreReadOnlyProperties);

                // We deserialize if there is a setter.
                ShouldDeserialize = HasSetter;
            }
            else
            {
                if (HasGetter)
                {
                    Debug.Assert(ConverterBase != null);

                    ShouldSerialize = true;

                    if (HasSetter)
                    {
                        ShouldDeserialize = true;
                    }
                }
            }
        }

        internal static TAttribute? GetAttribute<TAttribute>(PropertyInfo propertyInfo) where TAttribute : Attribute
        {
            return (TAttribute?)propertyInfo.GetCustomAttribute(typeof(TAttribute), inherit: false);
        }

        internal abstract bool GetMemberAndWriteJson(object obj, ref WriteStack state, Utf8JsonWriter writer);
        internal abstract bool GetMemberAndWriteJsonExtensionData(object obj, ref WriteStack state, Utf8JsonWriter writer);

        internal virtual void GetPolicies(PropertyInfo? propertyInfo)
        {
            DetermineSerializationCapabilities();
            DeterminePropertyName(propertyInfo);
            IgnoreNullValues = Options.IgnoreNullValues;
        }

        internal abstract object? GetValueAsObject(object obj);

        /// <summary>
        /// todo
        /// </summary>
        public bool HasGetter { get; set; }

        /// <summary>
        /// todo
        /// </summary>
        public bool HasSetter { get; set; }

        internal virtual void Initialize(
            Type parentClassType,
            Type declaredPropertyType,
            Type? runtimePropertyType,
            ClassType runtimeClassType,
            PropertyInfo? propertyInfo,
            JsonConverter converter,
            JsonSerializerOptions options)
        {
            Debug.Assert(converter != null);

            ParentClassType = parentClassType;
            DeclaredPropertyType = declaredPropertyType;
            RuntimePropertyType = runtimePropertyType;
            ClassType = runtimeClassType;
            ConverterBase = converter;
            Options = options;
        }

        /// <summary>
        /// todo
        /// </summary>
        public bool IgnoreNullValues { get; protected set; }

        internal bool IsPropertyPolicy { get; set; }

        // <PropertyNames>

        /// <summary>
        /// The name from a Json value. This is cached for performance on first deserialize.
        /// </summary>
        public byte[]? JsonPropertyName { get; set; }

        // The escaped name passed to the writer.
        // Use a field here (not a property) to avoid value semantics.
        internal JsonEncodedText? EscapedName;

        // The name of the property with any casing policy or the name specified from JsonPropertyNameAttribute.
        internal byte[]? Name { get; private set; }

        /// <summary>
        /// todo
        /// </summary>
        public string? NameAsString
        {
            get
            {
                return _nameAsString;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _nameAsString = value!;

                // value is valid unescaped UTF16 so just call the simple UTF16->UTF8 encoder.
                Name = Encoding.UTF8.GetBytes(value!);

                // Cache the escaped property name.
                EscapedName = JsonEncodedText.Encode(Name, Options.Encoder);
            }
        }
        internal string? ClrNameAsString { get; private set; }

        // </PropertyNames>

        // Options can be referenced here since all JsonPropertyInfos originate from a JsonClassInfo that is cached on JsonSerializerOptions.
        /// <summary>
        /// todo
        /// </summary>
        public JsonSerializerOptions Options { get; set; } = null!; // initialized in Init method

        internal bool ReadJsonAndAddExtensionProperty(object obj, ref ReadStack state, ref Utf8JsonReader reader)
        {
            object propValue = GetValueAsObject(obj)!;

            if (propValue is IDictionary<string, object?> dictionaryObject)
            {
                // Handle case where extension property is System.Object-based.

                if (reader.TokenType == JsonTokenType.Null)
                {
                    // A null JSON value is treated as a null object reference.
                    dictionaryObject[state.Current.JsonPropertyNameAsString!] = null;
                }
                else
                {
                    JsonConverter<object> converter = (JsonConverter<object>)
                        state.Current.JsonPropertyInfo!.RuntimeClassInfo.ElementClassInfo!.PropertyInfoForClassInfo.ConverterBase;

                    if (!converter.TryRead(ref reader, typeof(JsonElement), Options, ref state, out object? value))
                    {
                        return false;
                    }

                    dictionaryObject[state.Current.JsonPropertyNameAsString!] = value;
                }
            }
            else
            {
                // Handle case where extension property is JsonElement-based.

                Debug.Assert(propValue is IDictionary<string, JsonElement>);
                IDictionary<string, JsonElement> dictionaryJsonElement = (IDictionary<string, JsonElement>)propValue;

                JsonConverter<JsonElement> converter = (JsonConverter<JsonElement>)
                    state.Current.JsonPropertyInfo!.RuntimeClassInfo.ElementClassInfo!.PropertyInfoForClassInfo.ConverterBase;

                if (!converter.TryRead(ref reader, typeof(JsonElement), Options, ref state, out JsonElement value))
                {
                    return false;
                }

                dictionaryJsonElement[state.Current.JsonPropertyNameAsString!] = value;
            }

            return true;
        }

        internal abstract bool ReadJsonAndSetMember(object obj, ref ReadStack state, ref Utf8JsonReader reader);

        internal abstract bool ReadJsonAsObject(ref ReadStack state, ref Utf8JsonReader reader, out object? value);

        internal bool ReadJsonExtensionDataValue(ref ReadStack state, ref Utf8JsonReader reader, out object? value)
        {
            Debug.Assert(this == state.Current.JsonClassInfo.DataExtensionProperty);

            if (RuntimeClassInfo.ElementType == typeof(object) && reader.TokenType == JsonTokenType.Null)
            {
                value = null;
                return true;
            }

            JsonConverter<JsonElement> converter = (JsonConverter<JsonElement>)Options.GetConverter(typeof(JsonElement));
            if (!converter.TryRead(ref reader, typeof(JsonElement), Options, ref state, out JsonElement jsonElement))
            {
                // JsonElement is a struct that must be read in full.
                value = null;
                return false;
            }

            value = jsonElement;
            return true;
        }

        /// <summary>
        /// todo
        /// </summary>
        internal Type ParentClassType { get; set; } = null!;

        internal JsonClassInfo RuntimeClassInfo
        {
            get
            {
                if (_runtimeClassInfo == null)
                {
                    _runtimeClassInfo = Options.GetOrAddClass(RuntimePropertyType!);
                }

                return _runtimeClassInfo;
            }
        }

        internal Type? RuntimePropertyType { get; private set; } = null;

        internal abstract void SetValueAsObject(object obj, object? value);

        /// <summary>
        /// todo
        /// </summary>
        public bool ShouldSerialize { get; set; }

        /// <summary>
        /// todo
        /// </summary>
        public bool ShouldDeserialize { get; set; }

        internal bool IsIgnored { get; private set; }
    }
}
