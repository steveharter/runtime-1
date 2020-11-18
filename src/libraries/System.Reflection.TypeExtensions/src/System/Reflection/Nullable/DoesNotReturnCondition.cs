// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Reflection
{
    public enum DoesNotReturnCondition
    {
        NotApplicable, // todo: return for cases including when not a method
        Returns,
        DoesNotReturn,
        DoesNotReturnWhenTrue,
        DoesNotReturnWhenFalse,
    }
}
