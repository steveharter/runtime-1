// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    internal abstract class MetadataProviderStrategy
    {
        public static MetadataProviderStrategy s_fieldStrategy = new FieldStrategy();
        public static MetadataProviderStrategy s_methodStrategy = new MethodStrategy();
        public static MetadataProviderStrategy s_parameterStrategy = new ParameterStrategy();
        public static MetadataProviderStrategy s_propertyStrategy = new PropertyStrategy();

        public abstract IEnumerable<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider info);

        public abstract Type GetType(ICustomAttributeProvider info);

        public abstract Type GetDeclaringType(ICustomAttributeProvider info);

        public virtual NullableInCondition GetNullableIn(
            ICustomAttributeProvider info,
            bool hasNullableContext)
        {
            Type type = GetType(info);

            if (hasNullableContext)
            {
                if (!type.IsValueType)
                {
                    // A reference type is not null by default in a nullable context.
                    return NullableInCondition.DisallowNull;
                }
                else if (IsSystemNullableOfT(type))
                {
                    // Nullable<T> is always nullable.
                    return NullableInCondition.AllowNull;
                }

                // Other value types are never null.
                return NullableInCondition.DisallowNull;
            }
            else
            {
                if (!type.IsValueType)
                {
                    // A reference type is null by default.
                    return NullableInCondition.AllowNull;
                }
                else if (IsSystemNullableOfT(type))
                {
                    // Nullable<T> is always nullable.
                    return NullableInCondition.AllowNull;
                }

                // Other value types are never null.
                return NullableInCondition.DisallowNull;
            }
        }

        public virtual NullableOutCondition GetNullableOut(
            ICustomAttributeProvider info,
            bool hasNullableContext)
        {
            Type type = GetType(info);

            if (hasNullableContext)
            {
                if (!type.IsValueType)
                {
                    // A reference type is not null by default in a nullable context.
                    return NullableOutCondition.NotNull;
                }
                else if (IsSystemNullableOfT(type))
                {
                    // Nullable<T> is always nullable.
                    return NullableOutCondition.MaybeNull;
                }

                // Other value types are never null.
                return NullableOutCondition.NotNull;
            }
            else
            {
                if (!type.IsValueType)
                {
                    // A reference type is null by default.
                    return NullableOutCondition.MaybeNull;
                }
                else if (IsSystemNullableOfT(type))
                {
                    // Nullable<T> is always nullable.
                    return NullableOutCondition.MaybeNull;
                }

                return NullableOutCondition.NotNull;
            }
        }

        public static bool IsSystemNullableOfT(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Nullable`1";
        }
    }
}
