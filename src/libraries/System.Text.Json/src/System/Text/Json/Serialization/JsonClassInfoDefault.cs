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
    internal sealed class JsonClassInfoDefault : JsonClassInfo
    {
        internal JsonClassInfoDefault(Type type, JsonSerializerOptions options) : base(type, options) { }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        protected override IList<JsonPropertyInfo> GetProperties()
        {
            var jsonPropertes = new List<JsonPropertyInfo>();
            PropertyInfo[] clrProperties = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in clrProperties)
            {
                // Ignore indexers
                if (propertyInfo.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                // For now we only support public getters\setters
                if (propertyInfo.GetMethod?.IsPublic == true ||
                    propertyInfo.SetMethod?.IsPublic == true)
                {
                    JsonPropertyInfo jsonPropertyInfo = AddProperty(propertyInfo.PropertyType, propertyInfo, Type, Options);
                    Debug.Assert(jsonPropertyInfo != null);
                    Debug.Assert(jsonPropertyInfo.NameAsString != null);
                    Debug.Assert(jsonPropertyInfo.ClrNameAsString != null);

                    if (IsExtensionDataProperty(propertyInfo))
                    {
                        if (DataExtensionProperty != null)
                        {
                            ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(Type, typeof(JsonExtensionDataAttribute));
                        }

                        DataExtensionProperty = jsonPropertyInfo;
                    }
                    else
                    {
                        jsonPropertes.Add(jsonPropertyInfo);
                    }
                }
            }

            return jsonPropertes;
        }
    }
}
