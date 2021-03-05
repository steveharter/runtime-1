// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace System.Reflection
{
    [StructLayout(LayoutKind.Sequential)]
    //public readonly ref struct InvokeParameter<T>
    public ref struct InvokeParameter<T>
    {
        internal bool IsRef;
        internal ByReference<T> Ref; // storage for TypedReference
        internal T Value;
        internal Type ByRefLikeType;

        internal InvokeParameter(T value)
        {
            IsRef = false;
            Ref = default;
            Value = value;
            ByRefLikeType = default!;
        }

        internal InvokeParameter(ByReference<T> value)
        {
            IsRef = true;
            Ref = value;
            Value = default!;
            ByRefLikeType = default!;
        }

        internal InvokeParameter(IntPtr value, Type type)
        {
            IsRef = false;
            Ref = default;
            Value = (T)(object)value;
            ByRefLikeType = type;
        }

        public static InvokeParameter<T> Create(T value)
        {
            return new InvokeParameter<T>(value);
        }

        public static InvokeParameter<T> CreateRef(ref T value)
        {
            return new InvokeParameter<T>(new ByReference<T>(ref value));
        }

        public static InvokeParameter<IntPtr> CreateByRefLike(IntPtr value, Type type)
        {
            return new InvokeParameter<IntPtr>(value, type);
        }
    }

    public static class InvokeParameters
    {
        public static InvokeParameters<T1> Add<T1>(T1 value1)
        {
            return new InvokeParameters<T1>(InvokeParameter<T1>.Create(value1));
        }
        public static InvokeParameters<T1> AddRef<T1>(ref T1 value1)
        {
            return new InvokeParameters<T1>(InvokeParameter<T1>.CreateRef(ref value1));
        }
        //public static InvokeParameters<IntPtr> AddCreateByRefLike(IntPtr value1, Type type)
        //{
        //    return new InvokeParameters<IntPtr>(InvokeParameter<IntPtr>.CreateByRefLike(value1, type));
        //}

        [System.CLSCompliantAttribute(false)]
        public static void Invoke(MethodBase methodBase, TypedReference returnValue, TypedReference obj)
        {
            InvokeHelpers.InvokeDelegate0 d = InvokeHelpers.GetOrCreateInvokeAndCreateDelegate<InvokeHelpers.InvokeDelegate0>(methodBase);
            d(returnValue, obj);
        }

        [System.CLSCompliantAttribute(false)]
        public static void Invoke(MethodBase methodBase, TypedReference returnValue, TypedReference obj, TypedReference arg1)
        {
            InvokeHelpers.InvokeDelegate1 d = InvokeHelpers.GetOrCreateInvokeAndCreateDelegate<InvokeHelpers.InvokeDelegate1>(methodBase);
            d(returnValue, obj, arg1);
        }

        [System.CLSCompliantAttribute(false)]
        public static void Invoke(MethodBase methodBase, TypedReference returnValue, TypedReference obj, TypedReference arg1, TypedReference arg2)
        {
            InvokeHelpers.InvokeDelegate2 d = InvokeHelpers.GetOrCreateInvokeAndCreateDelegate<InvokeHelpers.InvokeDelegate2>(methodBase);
            d(returnValue, obj, arg1, arg2);
        }

        [System.CLSCompliantAttribute(false)]
        public static void Invoke(MethodBase methodBase, TypedReference returnValue, TypedReference obj, TypedReference arg1, TypedReference arg2, TypedReference arg3)
        {
            InvokeHelpers.InvokeDelegate3 d = InvokeHelpers.GetOrCreateInvokeAndCreateDelegate<InvokeHelpers.InvokeDelegate3>(methodBase);
            d(returnValue, obj, arg1, arg2, arg3);
        }
    }

    public ref struct InvokeParameters<T1>
    {
        public InvokeParameter<T1> Value1;

        public InvokeParameters(InvokeParameter<T1> value1)
        {
            Value1 = value1;
        }

        public InvokeParameters<T1, T2> Add<T2>(T2 value2)
        {
            return new InvokeParameters<T1, T2>(Value1, InvokeParameter<T2>.Create(value2));
        }

        public InvokeParameters<T1, T2> AddRef<T2>(ref T2 value2)
        {
            return new InvokeParameters<T1, T2>(Value1, InvokeParameter<T2>.CreateRef(ref value2));
        }

        //public InvokeParameters<T1, IntPtr> AddCreateByRefLike(IntPtr value2, Type type)
        //{
        //    return new InvokeParameters<T1, IntPtr>(Value1, InvokeParameter<IntPtr>.CreateByRefLike(value2, type));
        //}
    }

    public ref struct InvokeParameters<T1, T2>
    {
        public InvokeParameter<T1> Value1;
        public InvokeParameter<T2> Value2;

        public InvokeParameters(InvokeParameter<T1> value1, InvokeParameter<T2> value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public InvokeParameters<T1, T2, T3> Add<T3>(T3 value3)
        {
            return new InvokeParameters<T1, T2, T3>(Value1, Value2, InvokeParameter<T3>.Create(value3));
        }

        public InvokeParameters<T1, T2, T3> AddRef<T3>(ref T3 value3)
        {
            return new InvokeParameters<T1, T2, T3>(Value1, Value2, InvokeParameter<T3>.CreateRef(ref value3));
        }

        //public InvokeParameters<T1, T2, IntPtr> AddPointer(IntPtr value3, Type type)
        //{
        //    return new InvokeParameters<T1, T2, IntPtr>(Value1, Value2, InvokeParameter<IntPtr>.CreatePointer(value3, type));
        //}
    }


    public ref struct InvokeParameters<T1, T2, T3>
    {
        public InvokeParameter<T1> Value1;
        public InvokeParameter<T2> Value2;
        public InvokeParameter<T3> Value3;

        public InvokeParameters(InvokeParameter<T1> value1, InvokeParameter<T2> value2, InvokeParameter<T3> value3)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        public void Invoke(MethodInfo methodInfo)
        {
            throw new NotImplementedException();
            //InvokeHelpers.InvokeDelegate3 d = InvokeHelpers.GetOrCreateInvokeAndCreateDelegate<InvokeHelpers.InvokeDelegate3>(methodInfo);
            //d(
            //    TypedReference.Create<T1>(ref Value1),
            //    TypedReference.Create<T2>(ref Value2),
            //    TypedReference.Create<T3>(ref Value3)
            //);
        }
    }

    internal static class InvokeHelpers
    {
        public delegate void InvokeDelegate0(TypedReference retVal, TypedReference obj);
        public delegate void InvokeDelegate1(TypedReference retVal, TypedReference obj, TypedReference arg1);
        public delegate void InvokeDelegate2(TypedReference retVal, TypedReference obj, TypedReference arg1, TypedReference arg2);
        public delegate void InvokeDelegate3(TypedReference retVal, TypedReference obj, TypedReference arg1, TypedReference arg2, TypedReference arg3);

        private static volatile Delegate? _lazyInvokeAndCreateDelegate; // should not be static of course
        public static T GetOrCreateInvokeAndCreateDelegate<T>(MethodBase methodBase) where T : Delegate
        {
            Delegate? invokeAndCreateDelegate = _lazyInvokeAndCreateDelegate;
            if (invokeAndCreateDelegate == null)
            {
                invokeAndCreateDelegate = _lazyInvokeAndCreateDelegate = CreateInvokeDelegate(methodBase, emitNew: false);// true);
            }

            try
            {
                return (T)invokeAndCreateDelegate;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("todo Wrong number of parameters for invoking");
            }
        }

        //private static void InvokeAndCreate(TypedReference retVal, TypedReference arg0, TypedReference arg1)
        //{
        //    InvokeDelegate3 d = GetOrCreateInvokeAndCreateDelegate<InvokeDelegate3>();
        //    d(retVal, arg0, arg1);
        //}

        private static Delegate CreateInvokeDelegate(MethodBase method, bool emitNew)
        {
            Debug.Assert(!(emitNew && !(method is ConstructorInfo)));

            MethodInfo? methodInfo = method as MethodInfo;
            ConstructorInfo? constructorInfo = method as ConstructorInfo;

            if (method.ContainsGenericParameters)
                throw new InvalidOperationException("Method must not contain open generic parameters.");

            Type returnType = emitNew ? method.DeclaringType! : (methodInfo == null ? typeof(void) : methodInfo.ReturnType);
            if (returnType.IsByRef)
                throw new NotSupportedException("Ref returning Methods not supported.");

            bool hasRetVal = returnType != typeof(void);
            bool isValueType = method.DeclaringType!.IsValueType;

            ParameterInfo[] parameters = method.GetParametersNoCopy();
            bool hasThis = !(emitNew || method.IsStatic);
            //int numDelegateParameters = (hasRetVal ? 1 : 0) + parameters.Length + (hasThis ? 1 : 0);
            int numDelegateParameters = 2 + parameters.Length;// + (hasThis ? 1 : 0);
            Type[] delegateParameters = new Type[numDelegateParameters];
            Array.Fill(delegateParameters, typeof(TypedReference));

            DynamicMethod dm = new DynamicMethod(
                "InvokeStub_" + method.DeclaringType!.Name + "." + method.Name,
                typeof(void),
                delegateParameters,
                restrictedSkipVisibility: true);

            ILGenerator ilg = dm.GetILGenerator();

            int typeRefIndex = 0;
            if (hasRetVal)
            {
                ilg.Emit(OpCodes.Ldarg, typeRefIndex);
                ilg.Emit(OpCodes.Refanyval, returnType);
            }

            typeRefIndex++; // skip over the 'returnValue' parameter

            if (hasThis)
            {
                ilg.Emit(OpCodes.Ldarg, typeRefIndex);
                ilg.Emit(OpCodes.Refanyval, method.DeclaringType);
                if (!isValueType)
                {
                    ilg.Emit(OpCodes.Ldobj, method.DeclaringType);
                }
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                Type parameterType = parameter.ParameterType;
                ilg.Emit(OpCodes.Ldarg, ++typeRefIndex);
                if (parameterType.IsByRef)
                {
                    ilg.Emit(OpCodes.Refanyval, parameterType.GetElementType()!);
                }
                else
                {
                    ilg.Emit(OpCodes.Refanyval, parameterType);
                    ilg.Emit(OpCodes.Ldobj, parameterType);
                }
            }

            if (emitNew)
            {
                if (constructorInfo!.IsStatic)
                    throw new NotSupportedException("Cannot call static constructor.");
                ilg.Emit(OpCodes.Newobj, constructorInfo);
            }
            else
            {
                if (isValueType)
                {
                    ilg.Emit(OpCodes.Constrained, method.DeclaringType);
                }

                if (methodInfo != null)
                {
                    ilg.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, methodInfo);
                }
                else
                {
                    ilg.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, constructorInfo!);
                }
            }

            if (hasRetVal)
            {
                ilg.Emit(OpCodes.Stobj, returnType);
            }

            ilg.Emit(OpCodes.Ret);

            Type delegateType;
            switch (parameters.Length) //numDelegateParameters
            {
                case 0:
                    delegateType = typeof(InvokeDelegate0);
                    break;
                case 1:
                    delegateType = typeof(InvokeDelegate1);
                    break;
                case 2:
                    delegateType = typeof(InvokeDelegate2);
                    break;
                case 3:
                    delegateType = typeof(InvokeDelegate3);
                    break;
                default:
                    throw new NotImplementedException("TODO:Unsupported number of arguments.");
            }
            return dm.CreateDelegate(delegateType);
        }
    }
}
