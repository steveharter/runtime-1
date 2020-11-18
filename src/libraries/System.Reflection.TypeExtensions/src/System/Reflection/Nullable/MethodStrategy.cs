// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    internal sealed class MethodStrategy : MetadataProviderStrategy
    {
        public override IEnumerable<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider info)
        {
            return ((MethodInfo)info).GetCustomAttributesData();
        }

        public override Type GetType(ICustomAttributeProvider info)
        {
            return ((MethodInfo)info).ReturnType;
        }

        public override Type GetDeclaringType(ICustomAttributeProvider info)
        {
            return ((MethodInfo)info).DeclaringType!;
        }

        public override NullableOutCondition GetNullableOut(
            ICustomAttributeProvider info,
            bool hasNullableContext)
        {
            MethodInfo methodInfo = (MethodInfo)info;
            if (methodInfo.ReturnType.Name == "Void")
            {
                return NullableOutCondition.NotApplicable;
            }

            return base.GetNullableOut(info, hasNullableContext);
        }
    }
}
