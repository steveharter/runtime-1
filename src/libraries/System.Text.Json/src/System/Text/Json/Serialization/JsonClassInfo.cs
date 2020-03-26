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
    [DebuggerDisplay("ClassType.{ClassType}, {Type.Name}")]
    public abstract partial class JsonClassInfo
    {
        // If enumerable, the JsonClassInfo for the element type.
        private JsonClassInfo? _elementClassInfo;
        private bool _hasTypeBeenCreated;

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public delegate object? ConstructorDelegate();

        internal delegate T ParameterizedConstructorDelegate<T>(object[] arguments);

        internal delegate T ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        /// <summary>
        /// todo
        /// </summary>
        public ConstructorDelegate? CreateObject { get; set; }

        internal ClassType ClassType { get; private set; }

        /// <summary>
        /// todo
        /// </summary>
        public JsonPropertyInfo? DataExtensionProperty { get; set; }

        /// <summary>
        /// todo
        /// </summary>
        internal protected JsonClassInfo(Type type, JsonSerializerOptions options)
        {
            _hasTypeBeenCreated = true;

            Type = type;
            Options = options;
        }

        /// <summary>
        /// todo
        /// </summary>
        public void Initialize()
        {
            JsonConverter converter = GetConverter(
                Type,
                parentClassType: null, // A ClassInfo never has a "parent" class.
                propertyInfo: null, // A ClassInfo never has a "parent" property.
                out Type runtimeType,
                Options);

            ClassType = converter.ClassType;

            PropertyInfoForClassInfo = CreatePropertyInfoForClassInfo(Type, runtimeType, converter, Options);

            switch (ClassType)
            {
                case ClassType.Object:
                    {
                        PropertyInfoForClassInfo = CreatePropertyInfoForClassInfo(Type, runtimeType, converter!, Options);

                        if (CreateObject == null)
                        {
                            CreateObject = Options.MemberAccessorStrategy.CreateConstructor(Type);
                        }

                        IList<JsonPropertyInfo> properties = GetProperties();

                        Dictionary<string, JsonPropertyInfo> cache = CreatePropertyCache(properties.Count);
                        foreach (JsonPropertyInfo jsonPropertyInfo in properties)
                        {
                            if (jsonPropertyInfo != DataExtensionProperty)
                            {
                                if (!JsonHelpers.TryAdd(cache, jsonPropertyInfo.NameAsString!, jsonPropertyInfo))
                                {
                                    // The JsonPropertyNameAttribute resulted in a collision.

                                    JsonPropertyInfo other = cache[jsonPropertyInfo.NameAsString!];

                                    if (other.ShouldDeserialize == false && other.ShouldSerialize == false)
                                    {
                                        // Overwrite the one just added since it has [JsonIgnore].
                                        cache[jsonPropertyInfo.NameAsString!] = jsonPropertyInfo;
                                    }
                                    else if (jsonPropertyInfo.ShouldDeserialize == true || jsonPropertyInfo.ShouldSerialize == true)
                                    {
                                        ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameConflict(Type, jsonPropertyInfo);
                                    }
                                    // else ignore jsonPropertyInfo since it has [JsonIgnore].
                                }
                            }
                        }

                        JsonPropertyInfo[] cacheArray;
                        if (DataExtensionProperty != null)
                        {
                            cacheArray = new JsonPropertyInfo[cache.Count + 1];

                            // Set the last element to the extension property.
                            cacheArray[cache.Count] = DataExtensionProperty;
                        }
                        else
                        {
                            cacheArray = new JsonPropertyInfo[cache.Count];
                        }

                        // Set fields when finished to avoid concurrency issues.
                        PropertyCache = cache;
                        cache.Values.CopyTo(cacheArray, 0);
                        PropertyCacheArray = cacheArray;

                        if (converter.ConstructorIsParameterized)
                        {
                            converter.CreateConstructorDelegate(Options);
                            InitializeConstructorParameters(converter.ConstructorInfo);
                        }
                    }
                    break;
                case ClassType.Enumerable:
                case ClassType.Dictionary:
                    {
                        ElementType = converter.ElementType;
                        //CreateObject = Options.MemberAccessorStrategy.CreateConstructor(runtimeType);
                    }
                    break;
            }
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        protected abstract IList<JsonPropertyInfo> GetProperties();

        /// <summary>
        /// Return the JsonClassInfo for the element type, or null if the type is not an enumerable or dictionary.
        /// </summary>
        /// <remarks>
        /// This should not be called during warm-up (initial creation of JsonClassInfos) to avoid recursive behavior
        /// which could result in a StackOverflowException.
        /// </remarks>
        internal JsonClassInfo? ElementClassInfo
        {
            get
            {
                if (_elementClassInfo == null && ElementType != null)
                {
                    Debug.Assert(ClassType == ClassType.Enumerable || ClassType == ClassType.Dictionary);
                    _elementClassInfo = Options.GetOrAddClass(ElementType);
                }

                return _elementClassInfo;
            }
        }

        internal Type? ElementType { get; set; }

        /// <summary>
        /// todo
        /// </summary>
        public JsonSerializerOptions Options { get; set; } = null!;

        /// <summary>
        /// todo
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The JsonPropertyInfo for this JsonClassInfo. It is used to obtain the converter for the ClassInfo.
        /// </summary>
        /// <remarks>
        /// The returned JsonPropertyInfo does not represent a real property; instead it represents either:
        /// a collection element type,
        /// a generic type parameter,
        /// a property type (if pushed to a new stack frame),
        /// or the root type passed into the root serialization APIs.
        /// For example, for a property returning <see cref="Collections.Generic.List{T}"/> where T is a string,
        /// a JsonClassInfo will be created with .Type=typeof(string) and .PropertyInfoForClassInfo=JsonPropertyInfo{string}.
        /// Without this property, a "Converter" property would need to be added to JsonClassInfo and there would be several more
        /// `if` statements to obtain the converter from either the actual JsonPropertyInfo (for a real property) or from the
        /// ClassInfo (for the cases mentioned above). In addition, methods that have a JsonPropertyInfo argument would also likely
        /// need to add an argument for JsonClassInfo.
        /// </remarks>
        internal JsonPropertyInfo PropertyInfoForClassInfo { get; private set; } = null!;

        private void InitializeConstructorParameters(ConstructorInfo constructorInfo)
        {
            ParameterInfo[] parameters = constructorInfo!.GetParameters();
            Dictionary<string, JsonParameterInfo> parameterCache = CreateParameterCache(parameters.Length, Options);

            Dictionary<string, JsonPropertyInfo> propertyCache = PropertyCache!;

            foreach (ParameterInfo parameterInfo in parameters)
            {
                JsonPropertyInfo? firstMatch = null;
                bool isBound = false;

                foreach (JsonPropertyInfo jsonPropertyInfo in PropertyCacheArray!)
                {
                    string camelCasePropName = JsonNamingPolicy.CamelCase.ConvertName(jsonPropertyInfo.NameAsString!);

                    if (parameterInfo.Name == camelCasePropName &&
                        parameterInfo.ParameterType == jsonPropertyInfo.DeclaredPropertyType)
                    {
                        if (isBound)
                        {
                            Debug.Assert(firstMatch != null);

                            // Multiple object properties cannot bind to the same
                            // constructor parameter.
                            ThrowHelper.ThrowInvalidOperationException_MultiplePropertiesBindToConstructorParameters(
                                Type,
                                parameterInfo,
                                firstMatch.ClrNameAsString!,
                                jsonPropertyInfo.ClrNameAsString!,
                                constructorInfo);
                        }

                        JsonParameterInfo jsonParameterInfo = AddConstructorParameter(parameterInfo, jsonPropertyInfo, Options);

                        // One object property cannot map to multiple constructor
                        // parameters (ConvertName above can't return multiple strings).
                        parameterCache.Add(jsonParameterInfo.NameAsString, jsonParameterInfo);

                        // Remove property from deserialization cache to reduce the number of JsonPropertyInfos considered during JSON matching.
                        propertyCache.Remove(jsonPropertyInfo.NameAsString!);

                        isBound = true;
                        firstMatch = jsonPropertyInfo;
                    }
                }
            }

            // It is invalid for the extension data property to bind with a constructor argument.
            if (DataExtensionProperty != null &&
                parameterCache.ContainsKey(DataExtensionProperty.NameAsString!))
            {
                ThrowHelper.ThrowInvalidOperationException_ExtensionDataCannotBindToCtorParam(DataExtensionProperty.ClrNameAsString!, Type, constructorInfo);
            }

            ParameterCache = parameterCache;
            ParameterCount = parameters.Length;

            PropertyCache = propertyCache;
        }

        internal bool IsExtensionDataProperty(PropertyInfo propertyInfo)
        {
            Attribute? attribute = propertyInfo.GetCustomAttribute(typeof(JsonExtensionDataAttribute));
            if (attribute == null)
            {
                return false;
            }

            Type propertyType = propertyInfo.PropertyType;

            if (typeof(IDictionary<string, object>).IsAssignableFrom(propertyType) ||
                typeof(IDictionary<string, JsonElement>).IsAssignableFrom(propertyType))
            {
                JsonConverter converter = Options.GetConverter(propertyType);
                Debug.Assert(converter != null);
            }
            else
            {
                ThrowHelper.ThrowInvalidOperationException_SerializationDataExtensionPropertyInvalid(Type, propertyType.Name);
            }

            return true;
        }

        private static JsonParameterInfo AddConstructorParameter(
            ParameterInfo parameterInfo,
            JsonPropertyInfo jsonPropertyInfo,
            JsonSerializerOptions options)
        {
            string matchingPropertyName = jsonPropertyInfo.NameAsString!;

            if (jsonPropertyInfo.IsIgnored)
            {
                return JsonParameterInfo.CreateIgnoredParameterPlaceholder(matchingPropertyName, parameterInfo, options);
            }

            JsonConverter converter = jsonPropertyInfo.ConverterBase;

            JsonParameterInfo jsonParameterInfo = converter.CreateJsonParameterInfo();
            jsonParameterInfo.Initialize(
                matchingPropertyName,
                jsonPropertyInfo.DeclaredPropertyType,
                jsonPropertyInfo.RuntimePropertyType!,
                parameterInfo,
                converter,
                options);

            return jsonParameterInfo;
        }

        // This method gets the runtime information for a given type or property.
        // The runtime information consists of the following:
        // - class type,
        // - runtime type,
        // - element type (if the type is a collection),
        // - the converter (either native or custom), if one exists.
        internal static JsonConverter GetConverter(
            Type type,
            Type? parentClassType,
            PropertyInfo? propertyInfo,
            out Type runtimeType,
            JsonSerializerOptions options)
        {
            Debug.Assert(type != null);

            JsonConverter converter = options.DetermineConverter(parentClassType, type, propertyInfo)!;

            // The runtimeType is the actual value being assigned to the property.
            // There are three types to consider for the runtimeType:
            // 1) The declared type (the actual property type).
            // 2) The converter.TypeToConvert (the T value that the converter supports).
            // 3) The converter.RuntimeType (used with interfaces such as IList).

            Type converterRuntimeType = converter.RuntimeType;
            if (type == converterRuntimeType)
            {
                runtimeType = type;
            }
            else
            {
                if (type.IsInterface)
                {
                    runtimeType = converterRuntimeType;
                }
                else if (converterRuntimeType.IsInterface)
                {
                    runtimeType = type;
                }
                else
                {
                    // Use the most derived version from the converter.RuntimeType or converter.TypeToConvert.
                    if (type.IsAssignableFrom(converterRuntimeType))
                    {
                        runtimeType = converterRuntimeType;
                    }
                    else if (converterRuntimeType.IsAssignableFrom(type) || converter.TypeToConvert.IsAssignableFrom(type))
                    {
                        runtimeType = type;
                    }
                    else
                    {
                        runtimeType = default!;
                        ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
                    }
                }
            }

            return converter;
        }

        internal void VerifyMutable()
        {
            if (_hasTypeBeenCreated)
            {
                // todo: need specific exception
                ThrowHelper.ThrowInvalidOperationException_SerializerOptionsImmutable();
            }
        }
    }
}
