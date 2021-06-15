// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection;

namespace System.Text.Json.Serialization.Metadata
{
    public partial class JsonTypeInfo
    {
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public JsonPropertyInfo CreateProperty(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return CreateProperty(memberInfo, propertyInfo.PropertyType);
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                return CreateProperty(memberInfo, fieldInfo.FieldType);
            }

            throw new InvalidOperationException("todo");
        }

        internal JsonPropertyInfo CreateProperty(
            MemberInfo memberInfo,
            Type memberType)
        {
            JsonConverter converter = GetConverter(
                memberType,
                Type,
                memberInfo,
                out Type runtimeType,
                Options);

            JsonPropertyInfo property = converter.CreateJsonPropertyInfo();

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

        /// <summary>
        /// Create a <see cref="JsonPropertyInfo"/> for a given Type.
        /// See <seealso cref="JsonTypeInfo.PropertyInfoForTypeInfo"/>.
        /// </summary>
        internal JsonPropertyInfo CreatePropertyInfoForTypeInfo(
            Type declaredPropertyType,
            Type runtimePropertyType,
            JsonConverter converter)
        {
            // Create the JsonPropertyInfo instance.
            JsonPropertyInfo jsonPropertyInfo = converter.CreateJsonPropertyInfo();

            jsonPropertyInfo.Initialize(
                parentClassType: JsonTypeInfo.ObjectType, // a dummy value (not used),
                declaredPropertyType,
                runtimePropertyType,
                runtimeClassType: converter.ConverterStrategy,
                memberInfo: null,  // Not a real property so this is null.
                converter,
                declaringTypeNumberHandling: null,
                Options);

            Debug.Assert(jsonPropertyInfo.IsForTypeInfo);

            return jsonPropertyInfo;
        }

        private JsonPropertyInfo CreatePropertyOrExtensionProperty(
            Type memberType,
            MemberInfo memberInfo)
        {
            bool hasExtensionAttribute = memberInfo.GetCustomAttribute(typeof(JsonExtensionDataAttribute)) != null;
            if (hasExtensionAttribute && DataExtensionProperty != null)
            {
                ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(Type, typeof(JsonExtensionDataAttribute));
            }

            JsonPropertyInfo jsonPropertyInfo = CreateProperty(memberInfo, memberType);
            Debug.Assert(jsonPropertyInfo.NameAsString != null);

            if (hasExtensionAttribute)
            {
                Debug.Assert(DataExtensionProperty == null);
                ValidateAndAssignDataExtensionProperty(jsonPropertyInfo);
                Debug.Assert(DataExtensionProperty != null);
            }

            return jsonPropertyInfo;
        }
    }
}
