// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Reflection
{
    internal static class FastInvoke
    {
        public delegate void Func5(TypedReference arg1, TypedReference arg2, TypedReference arg3, TypedReference arg4, TypedReference arg5);
        public delegate void Func2(TypedReference arg1, TypedReference arg2);

        public static Func5 CreateInvokeDelegate(MethodBase method, bool emitNew)
        {
            Debug.Assert(!(emitNew && !(method is ConstructorInfo)));

            MethodInfo? methodInfo = method as MethodInfo;
            ConstructorInfo? constructorInfo = method as ConstructorInfo;

            if (method.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Method must not contain open generic parameters.");
            }

            Type returnType = emitNew ? method.DeclaringType! : (methodInfo == null ? typeof(void) : methodInfo.ReturnType);
            //if (returnType.IsByRef)
            //    throw new NotSupportedException("Ref returning Methods not supported.");

            bool hasRetVal = returnType != typeof(void);

            Type? declaringType = method.DeclaringType;
            Debug.Assert(declaringType != null);

            bool isValueType = declaringType.IsValueType;

            ParameterInfo[] parameters = method.GetParametersNoCopy();
            bool hasThis = !(emitNew || method.IsStatic);
            int numDelegateParameters = 5;// (hasRetVal ? 2 : 1) + parameters.Length;
            Type[] delegateParameters = new Type[numDelegateParameters];
            Array.Fill(delegateParameters, typeof(TypedReference));

            var dm = new DynamicMethod(
                "InvokeStub_" + declaringType.Name + "." + method.Name,
                typeof(void),
                delegateParameters,
                restrictedSkipVisibility: true);

            ILGenerator ilg = dm.GetILGenerator();

            int typeRefIndex = 0;
            if (hasRetVal)
            {
                ilg.Emit(OpCodes.Ldarg, typeRefIndex++);

                if (returnType.IsByRef)
                {
                    // todo https://github.com/dotnet/designs/pull/17/files#diff-95671f0ce30ebf166d73fd612d241c26555bc12eae18c570a26f4fe1b0fb716eR43
                    ilg.Emit(OpCodes.Refanyval, returnType);
                }
                else
                {
                    ilg.Emit(OpCodes.Refanyval, returnType);
                }
            }

            if (hasThis)
            {
                ilg.Emit(OpCodes.Ldarg, typeRefIndex);
                ilg.Emit(OpCodes.Refanyval, declaringType);

                if (!isValueType)
                {
                    ilg.Emit(OpCodes.Ldobj, declaringType);
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
                //if (isValueType && !method.IsStatic)
                if (!method.IsStatic)
                {
                    //ilg.Emit(OpCodes.Constrained, method.DeclaringType);
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

            return (Func5)dm.CreateDelegate(typeof(Func5));
        }

        public static Func2 CreateFieldGetter(FieldInfo field)
        {
            Type? declaringType = field.DeclaringType;
            Debug.Assert(declaringType != null);

            Type fieldType = field.FieldType;

            Type[] delegateParameters = new Type[2] { typeof(TypedReference), typeof(TypedReference) };

            var dm = new DynamicMethod(
                    name: "FieldGetterStub_" + declaringType.Name + "." + field.Name,
                    returnType: typeof(void),
                    parameterTypes: delegateParameters,
                    restrictedSkipVisibility: true);

            bool hasThis = !field.IsStatic;
            bool isValueType = declaringType.IsValueType;

            ILGenerator ilg = dm.GetILGenerator();

            if (hasThis)
            {
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Refanyval, declaringType);

                if (!isValueType)
                {
                    ilg.Emit(OpCodes.Ldobj, declaringType);
                }
            }

            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Ldfld, field);
            ilg.Emit(OpCodes.Refanyval, fieldType);

            ilg.Emit(OpCodes.Ret);

            return (Func2)dm.CreateDelegate(typeof(Func2));
        }

        public static Func2 CreateFieldSetter(FieldInfo field)
        {
            Type? declaringType = field.DeclaringType;
            Debug.Assert(declaringType != null);

            Type fieldType = field.FieldType;

            Type[] delegateParameters = new Type[2] { typeof(TypedReference), typeof(TypedReference) };

            var dm = new DynamicMethod(
                    name: "FieldSetterStub_" + declaringType!.Name + "." + field.Name,
                    returnType: typeof(void),
                    parameterTypes: delegateParameters,
                    restrictedSkipVisibility: true);

            bool hasThis = !field.IsStatic;
            bool isValueType = declaringType.IsValueType;

            ILGenerator ilg = dm.GetILGenerator();

            if (hasThis)
            {
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Refanyval, declaringType);

                if (!isValueType)
                {
                    ilg.Emit(OpCodes.Ldobj, declaringType);
                }
            }

            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Refanyval, fieldType);
            ilg.Emit(OpCodes.Ldobj, declaringType);
            ilg.Emit(OpCodes.Stfld, field);

            ilg.Emit(OpCodes.Ret);

            return (Func2)dm.CreateDelegate(typeof(Func2));
        }
    }
}
