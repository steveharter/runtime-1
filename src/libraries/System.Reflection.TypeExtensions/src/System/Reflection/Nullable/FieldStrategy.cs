// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Reflection
{
    internal sealed class FieldStrategy : MetadataProviderStrategy
    {
        public override IEnumerable<CustomAttributeData> GetCustomAttributeData(ICustomAttributeProvider info)
        {
            return ((FieldInfo)info).GetCustomAttributesData();
        }

        public override Type GetType(ICustomAttributeProvider info)
        {
            return ((FieldInfo)info).FieldType;
        }

        public override Type GetDeclaringType(ICustomAttributeProvider info)
        {
            return ((FieldInfo)info).DeclaringType!;
        }
    }
}
