// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    public enum NullableCondition
    {
        MaybeNull,
        MaybeNullWhenTrue,
        MaybeNullWhenFalse,
        NotNull,
        NotNullIfNotNull,
        NotNullWhenTrue,
        NotNullWhenFalse
    }

    internal static class NullableConditionFactory
    {
        public static NullableCondition Create(IList<CustomAttributeData> attributes, Type declaringType, Type memberType)
        {
            foreach (CustomAttributeData cad in attributes)
            {
                string? attributeName = cad.AttributeType.FullName;
                if (attributeName == null)
                {
                    continue;
                }

                if (attributeName.StartsWith("System.Diagnostics.CodeAnalysis"))
                {
                    if (attributeName == "System.Diagnostics.CodeAnalysis.MaybeNullAttribute" ||
                        attributeName == "System.Diagnostics.CodeAnalysis.AllowNullAttribute")
                    {
                        return NullableCondition.MaybeNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.NotNullAttribute" ||
                        attributeName == "System.Diagnostics.CodeAnalysis.DisallowNullAttribute")
                    {
                        return NullableCondition.NotNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute")
                    {
                        return ((bool)cad.ConstructorArguments[0].Value!)
                            ? NullableCondition.MaybeNullWhenTrue : NullableCondition.MaybeNullWhenFalse;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute")
                    {
                        return NullableCondition.NotNullIfNotNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.NotNullWhenAttribute")
                    {
                        return ((bool)cad.ConstructorArguments[0].Value!)
                            ? NullableCondition.NotNullWhenTrue : NullableCondition.NotNullWhenFalse;
                    }
                }
            }


            foreach (CustomAttributeData cad in declaringType.GetCustomAttributesData())
            {
                string? attributeName = cad.AttributeType.FullName;
                if (attributeName == null)
                {
                    continue;
                }

                if (attributeName == "System.Runtime.CompilerServices.NullableContextAttribute")
                {
                    if (!memberType.IsValueType)
                    {
                        // A reference type is not null by default in a nullable context.
                        return NullableCondition.NotNull;
                    }

                    break;
                }
            }

            // If not in a nullable context, return CLR semantics.
            if (!memberType.IsValueType ||
                memberType.IsGenericType && memberType.GetGenericTypeDefinition().FullName == "System.Nullable`1")
            {
                return NullableCondition.MaybeNull;
            }

            return NullableCondition.NotNull;
        }
    }
}
