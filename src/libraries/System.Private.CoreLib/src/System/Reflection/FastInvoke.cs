// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Reflection
{
    internal static class FastInvoke
    {
        public delegate void Func5(TypedReference arg1, TypedReference arg2, TypedReference arg3, TypedReference arg4, TypedReference arg5);
        public delegate void FieldAccessor(TypedReference arg1, TypedReference arg2, bool isGetter);

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
            bool hasRetVal = returnType != typeof(void);
            Type? refReturnType = default;

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
                    refReturnType = returnType.GetElementType()!;
                    ilg.Emit(OpCodes.Refanyval, refReturnType);
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
                    // todo: enums, System.Object support
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
                if (returnType.IsByRef)
                {
                    Debug.Assert(refReturnType != null);
                    ilg.Emit(OpCodes.Ldobj, refReturnType);
                    ilg.Emit(OpCodes.Stobj, refReturnType);
                }
                else
                {
                    ilg.Emit(OpCodes.Stobj, returnType);
                }
            }

            ilg.Emit(OpCodes.Ret);

            return (Func5)dm.CreateDelegate(typeof(Func5));
        }

        public static FieldAccessor CreateFieldAccessor(FieldInfo field)
        {
            Type? declaringType = field.DeclaringType;
            Debug.Assert(declaringType != null);

            Type fieldType = field.FieldType;

            Type[] delegateParameters = new Type[3] { typeof(TypedReference), typeof(TypedReference), typeof(bool) };

            var dm = new DynamicMethod(
                    name: "FieldAccessorStub_" + declaringType.Name + "." + field.Name,
                    returnType: typeof(void),
                    parameterTypes: delegateParameters,
                    restrictedSkipVisibility: true);

            bool hasThis = !field.IsStatic;
            bool isValueType = declaringType.IsValueType;

            ILGenerator ilg = dm.GetILGenerator();

            Label setField = ilg.DefineLabel();
            Label exit = ilg.DefineLabel();

            LocalBuilder tempRef = ilg.DeclareLocal(fieldType.MakePointerType());

            ilg.Emit(OpCodes.Ldarg_2);
            ilg.Emit(OpCodes.Brfalse_S, setField);

            // Ldfld:
            if (hasThis)
            {
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Refanyval, declaringType);

                //if (!isValueType)
                //{
                //    ilg.Emit(OpCodes.Ldobj, declaringType);
                //}
            }
            // todo: static fields

            ilg.Emit(OpCodes.Stloc, tempRef);

            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Refanyval, fieldType);

            ilg.Emit(OpCodes.Ldloc, tempRef);
            ilg.Emit(OpCodes.Ldind_Ref);

            // Get the value from the field.
            ilg.Emit(OpCodes.Ldfld, field);
            ilg.Emit(OpCodes.Stobj, fieldType);

            ilg.Emit(OpCodes.Br_S, exit);

            // Stfld:
            ilg.MarkLabel(setField);

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

            ilg.MarkLabel(exit);
            ilg.Emit(OpCodes.Ret);

            return (FieldAccessor)dm.CreateDelegate(typeof(FieldAccessor));
        }
    }
}
