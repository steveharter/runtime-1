// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    public sealed class AttributedInfo
    {
        public NullableInCondition NullableIn { get; }
        public NullableOutCondition NullableOut { get; }
        public DoesNotReturnCondition DoesNotReturn { get; }
        public bool HasNullableContext { get; }
        public string? MemberReference { get; }
        public string[]? MemberReferences { get; }
        // todo: expose nullability for generic type paramters and array types

        internal AttributedInfo(ICustomAttributeProvider info, MetadataProviderStrategy strategy)
        {
            foreach (CustomAttributeData cad in strategy.GetCustomAttributeData(info))
            {
                string? attributeName = cad.AttributeType.FullName;
                if (attributeName == null)
                {
                    continue;
                }

                if (attributeName.StartsWith("System.Diagnostics.CodeAnalysis"))
                {
                    if (attributeName == "System.Diagnostics.CodeAnalysis.NullableAttribute")
                    {
                        // todo. https://github.com/dotnet/roslyn/blob/7bc44488c661fd6bbb6c53f39512a6fe0cc5ef84/docs/features/nullable-metadata.md
                    }
                    if (attributeName == "System.Diagnostics.CodeAnalysis.DisallowNullAttribute")
                    {
                        NullableIn = NullableInCondition.DisallowNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.AllowNullAttribute")
                    {
                        NullableIn = NullableInCondition.AllowNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.MaybeNullAttribute")
                    {
                        NullableOut = NullableOutCondition.MaybeNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.NotNullAttribute")
                    {
                        NullableOut = NullableOutCondition.NotNull;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute")
                    {
                        NullableOut = ((bool)cad.ConstructorArguments[0].Value!)
                            ? NullableOutCondition.MaybeNullWhenTrue : NullableOutCondition.MaybeNullWhenFalse;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute")
                    {
                        NullableOut = NullableOutCondition.NotNullIfNotNull;
                        MemberReference = ((string?)cad.ConstructorArguments[0].Value);
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.NotNullWhenAttribute")
                    {
                        NullableOut = ((bool)cad.ConstructorArguments[0].Value!)
                            ? NullableOutCondition.NotNullWhenTrue : NullableOutCondition.NotNullWhenFalse;
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")
                    {
                        DoesNotReturn = DoesNotReturnCondition.DoesNotReturn;
                        MemberReference = ((string?)cad.ConstructorArguments[0].Value);
                    }
                    else if (attributeName == "System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute")
                    {
                        DoesNotReturn = ((bool)cad.ConstructorArguments[0].Value!)
                            ? DoesNotReturnCondition.DoesNotReturnWhenTrue : DoesNotReturnCondition.DoesNotReturnWhenFalse;

                        MemberReferences = ((string[]?)cad.ConstructorArguments[1].Value);
                    }
                }
            }

            Type declaringType = strategy.GetDeclaringType(info);

            // Todo: context attribute could also exist on assembly or in base class
            // Todo: context attribute on per-member basis, not just declaring type
            foreach (CustomAttributeData cad in declaringType.GetCustomAttributesData())
            {
                string? attributeName = cad.AttributeType.FullName;
                if (attributeName == null)
                {
                    continue;
                }

                if (attributeName == "System.Runtime.CompilerServices.NullableContextAttribute")
                {
                    // todo: inspect byte[]: 0 for oblivious, 1 for not annotated, and 2 for annotated.
                    HasNullableContext = true;
                    break;
                }
            }

            // Apply defaults if no attributes.

            if (NullableIn == NullableInCondition.NotApplicable)
            {
                NullableIn = strategy.GetNullableIn(info, HasNullableContext);
            }

            if (NullableOut == NullableOutCondition.NotApplicable)
            {
                NullableOut = strategy.GetNullableOut(info, HasNullableContext);
            }
        }
    }
}
