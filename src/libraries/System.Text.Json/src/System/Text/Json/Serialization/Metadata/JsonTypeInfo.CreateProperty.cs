// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;

namespace System.Text.Json.Serialization.Metadata
{
    public partial class JsonTypeInfo
    {
        /// <summary>
        /// Create a <see cref="JsonPropertyInfo"/> for a given Type.
        /// See <seealso cref="PropertyInfoForTypeInfo"/>.
        /// </summary>
        internal JsonPropertyInfo CreatePropertyInfoForTypeInfo(
            Type declaredPropertyType,
            Type runtimePropertyType,
            JsonConverter converter,
            JsonNumberHandling? declaringTypeNumberHandling)
        {
            // Create the JsonPropertyInfo instance.
            JsonPropertyInfo property = converter.CreateJsonPropertyInfo();

            property.Initialize(
                parentClassType: ObjectType, // a dummy value (not used),
                declaredPropertyType,
                runtimePropertyType,
                runtimeClassType: converter.ConverterStrategy,
                memberInfo: null,  // Not a real property so this is null.
                converter,
                declaringTypeNumberHandling: declaringTypeNumberHandling,
                Options);

            Debug.Assert(property.IsForTypeInfo);

            return property;
        }

        /// <summary>
        /// Create a <see cref="JsonPropertyInfo"/> for a given reflected property.
        /// </summary>
        private JsonPropertyInfo CreatePropertyOrExtensionProperty(
            Type memberType,
            MemberInfo memberInfo,
            JsonSerializerOptions options)
        {
            JsonIgnoreCondition? ignoreCondition = memberInfo.GetCustomAttribute<JsonIgnoreAttribute>(inherit: false)?.Condition;
            if (ignoreCondition == JsonIgnoreCondition.Always)
            {
                return JsonPropertyInfo.CreateIgnoredPropertyPlaceholder(memberInfo, options);
            }

            bool hasExtensionAttribute = memberInfo.GetCustomAttribute(typeof(JsonExtensionDataAttribute)) != null;
            if (hasExtensionAttribute && DataExtensionProperty != null)
            {
                ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(Type, typeof(JsonExtensionDataAttribute));
            }

            JsonPropertyInfo jsonPropertyInfo = CreateProperty();
            Debug.Assert(jsonPropertyInfo.NameAsString != null);

            if (hasExtensionAttribute)
            {
                Debug.Assert(DataExtensionProperty == null);
                ValidateAndAssignDataExtensionProperty(jsonPropertyInfo);
                Debug.Assert(DataExtensionProperty != null);
            }

            return jsonPropertyInfo;

            JsonPropertyInfo CreateProperty()
            {
                JsonConverter converter = GetConverter(
                    memberType,
                    Type,
                    memberInfo,
                    out Type runtimeType,
                    Options);

                JsonPropertyInfo property = converter.CreateJsonPropertyInfo();
                property.IgnoreCondition = ignoreCondition;

                property.Initialize(
                    parentClassType: Type,
                    declaredPropertyType: memberType,
                    runtimePropertyType: runtimeType,
                    runtimeClassType: converter.ConverterStrategy,
                    memberInfo,
                    converter,
                    declaringTypeNumberHandling: NumberHandling,
                    Options);

                Debug.Assert(property.NameAsString != null);
                Debug.Assert(!property.IsForTypeInfo);

                return property;
            }
        }
    }
}
