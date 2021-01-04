// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Internal.Runtime.CompilerServices;

namespace System
{
    internal struct ValueHolders
    {
        private unsafe IntPtr[] _values;
        private LocalVariableType[] _types;

        public unsafe ValueHolders(int count)
        {
            _values = new IntPtr[count];
            _types = new LocalVariableType[count];
        }

        public unsafe ref T GetRefVar<T>(int index)
        {
            //if (!_types[index].ByRef)
            //{
            //    throw new InvalidOperationException("todo");
            //}

            if (typeof(T).TypeHandle != _types[index])
            {
                throw new InvalidOperationException("todo");
            }

            IntPtr address = _values[index];
            return ref Unsafe.AsRef<T>((void*)address);
        }

        public unsafe T GetVar<T>(int index)
        {
            if (typeof(T).TypeHandle != _types[index])
            {
                throw new InvalidOperationException("todo");
            }

            IntPtr address = _values[index];
            //if (_types[index].ByRef)
            //{
            //    address = *(IntPtr*)address.ToPointer();
            //}

            return Unsafe.Read<T>((void*)address);
        }

        public unsafe void SetRefVar<T>(int index, ref T value)
        {
            //if (!_types[index].ByRef)
            //{
            //    throw new InvalidOperationException("todo");
            //}
            _values[index] = (IntPtr)Unsafe.AsPointer(ref value);
            _types[index] = new LocalVariableType(typeof(T).TypeHandle);
        }
    }

    /// <summary>
    /// Abstraction for the type information needed to be a local
    /// </summary>
    internal struct LocalVariableType
    {
        public LocalVariableType(RuntimeTypeHandle typeHandle)
        {
            TypeHandle = typeHandle;
        }

        public RuntimeTypeHandle TypeHandle;

        internal bool IsValueType
        {
            get
            {
                unsafe
                {
                    return TypeHandle.MakePointer().IsValueType;
                }
            }
        }
    }
}
