// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// TypedReference is basically only ever seen on the call stack, and in param arrays.
//  These are blob that must be dealt with by the compiler.

using System.Reflection;
using System.Runtime.CompilerServices;
using Internal.Runtime.CompilerServices;

namespace System
{
    public ref struct ValueHolder<T>
    {
        private ByReference<T> _value;

        [CLSCompliant(false)]
        public ValueHolder(T value)
        {
            _value = new ByReference<T>(ref value);
        }

    }
}
