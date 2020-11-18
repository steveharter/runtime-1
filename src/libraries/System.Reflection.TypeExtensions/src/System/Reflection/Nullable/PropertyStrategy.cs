// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    internal sealed class PropertyStrategy : MetadataProviderStrategy
    {
        public override IEnumerable<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider info)
        {
            PropertyInfo property = ((PropertyInfo)info);

            MethodInfo? method = property.GetGetMethod();
            if (method != null)
            {
                foreach (CustomAttributeData attr in method.ReturnParameter.GetCustomAttributesData())
                {
                    yield return attr;
                }
            }

            method = property.GetSetMethod();
            if (method != null)
            {
                foreach (CustomAttributeData attr in method.GetParameters()[0].GetCustomAttributesData())
                {
                    yield return attr;
                }
            }
        }

        public override Type GetType(ICustomAttributeProvider info)
        {
            return ((PropertyInfo)info).PropertyType;
        }

        public override Type GetDeclaringType(ICustomAttributeProvider info)
        {
            return ((PropertyInfo)info).DeclaringType!;
        }

        public override NullableInCondition GetNullableIn(
            ICustomAttributeProvider info,
            bool hasNullableContext)
        {
            PropertyInfo propertyInfo = (PropertyInfo)info;
            if (!propertyInfo.CanWrite)
            {
                return NullableInCondition.NotApplicable;
            }

            return base.GetNullableIn(info, hasNullableContext);
        }
    }
}
