// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    internal sealed class ParameterStrategy : MetadataProviderStrategy
    {
        public override IEnumerable<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider info)
        {
            return ((ParameterInfo)info).GetCustomAttributesData();
        }

        public override Type GetType(ICustomAttributeProvider info)
        {
            return ((ParameterInfo)info).ParameterType;
        }

        public override Type GetDeclaringType(ICustomAttributeProvider info)
        {
            return ((ParameterInfo)info).Member.DeclaringType!;
        }

        public override NullableOutCondition GetNullableOut(
            ICustomAttributeProvider info,
            bool hasNullableContext)
        {
            ParameterInfo parameterInfo = (ParameterInfo)info;
            if (!parameterInfo.ParameterType.IsByRef)
            {
                return NullableOutCondition.NotApplicable;
            }

            return base.GetNullableOut(info, hasNullableContext);
        }
    }
}
