// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    /// <summary>
    /// Implementation of <cref>JsonObjectConverter{T}</cref> that supports the deserialization
    /// of JSON objects using parameterized constructors.
    /// </summary>
    internal sealed class SmallObjectWithParameterizedConstructorConverter<T, TArg0, TArg1, TArg2, TArg3> : ObjectWithParameterizedConstructorConverter where T : notnull
    {
        public SmallObjectWithParameterizedConstructorConverter(Type typeToConvert) : base(typeToConvert) { }

        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var createObject = (JsonClassInfo.ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>)
                frame.JsonClassInfo.CreateObjectWithArgs!;
            var arguments = (Arguments<TArg0, TArg1, TArg2, TArg3>)frame.CtorArgumentState!.Arguments;
            return createObject!(arguments.Arg0, arguments.Arg1, arguments.Arg2, arguments.Arg3);
        }

        protected override bool ReadAndCacheConstructorArgument(ref ReadStack state, ref Utf8JsonReader reader, JsonParameterInfo jsonParameterInfo)
        {
            Debug.Assert(state.Current.CtorArgumentState!.Arguments != null);
            var arguments = (Arguments<TArg0, TArg1, TArg2, TArg3>)state.Current.CtorArgumentState.Arguments;

            bool success;

            bool isStronglyTyped = jsonParameterInfo.ConverterBase.ClassType == ClassType.Value;

            switch (jsonParameterInfo.Position)
            {
                case 0:
                    if (isStronglyTyped)
                    {
                        success = ((JsonParameterInfo<TArg0>)jsonParameterInfo).ReadJsonTyped(ref state, ref reader, out TArg0 arg0);
                        if (success)
                        {
                            arguments.Arg0 = arg0!;
                        }
                    }
                    else
                    {
                        success = jsonParameterInfo.ReadJson(ref state, ref reader, out object? arg0);
                        if (success)
                        {
                            arguments.Arg0 = (TArg0)arg0!;
                        }
                    }
                    break;
                case 1:
                    if (isStronglyTyped)
                    {
                        success = ((JsonParameterInfo<TArg1>)jsonParameterInfo).ReadJsonTyped(ref state, ref reader, out TArg1 arg1);
                        if (success)
                        {
                            arguments.Arg1 = arg1!;
                        }
                    }
                    else
                    {
                        success = jsonParameterInfo.ReadJson(ref state, ref reader, out object? arg1);
                        if (success)
                        {
                            arguments.Arg1 = (TArg1)arg1!;
                        }
                    }
                    break;
                case 2:
                    if (isStronglyTyped)
                    {
                        success = ((JsonParameterInfo<TArg2>)jsonParameterInfo).ReadJsonTyped(ref state, ref reader, out TArg2 arg2);
                        if (success)
                        {
                            arguments.Arg2 = arg2!;
                        }
                    }
                    else
                    {
                        success = jsonParameterInfo.ReadJson(ref state, ref reader, out object? arg2);
                        if (success)
                        {
                            arguments.Arg2 = (TArg2)arg2!;
                        }
                    }
                    break;
                case 3:
                    if (isStronglyTyped)
                    {
                        success = ((JsonParameterInfo<TArg3>)jsonParameterInfo).ReadJsonTyped(ref state, ref reader, out TArg3 arg3);
                        if (success)
                        {
                            arguments.Arg3 = arg3!;
                        }
                    }
                    else
                    {
                        success = jsonParameterInfo.ReadJson(ref state, ref reader, out object? arg3);
                        if (success)
                        {
                            arguments.Arg3 = (TArg3)arg3!;
                        }
                    }
                    break;
                default:
                    Debug.Fail("More than 4 params: we should be in override for LargeObjectWithParameterizedConstructorConverter.");
                    throw new InvalidOperationException();
            }

            return success;
        }

        protected override void InitializeConstructorArgumentCaches(ref ReadStack state, JsonSerializerOptions options)
        {
            JsonClassInfo classInfo = state.Current.JsonClassInfo;

            if (classInfo.CreateObjectWithArgs == null)
            {
                classInfo.CreateObjectWithArgs =
                    options.MemberAccessorStrategy.CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(ConstructorInfo!);
            }

            var arguments = new Arguments<TArg0, TArg1, TArg2, TArg3>();

            foreach (JsonParameterInfo parameterInfo in classInfo.ParameterCache!.Values)
            {
                if (parameterInfo.ShouldDeserialize)
                {
                    bool isStronglyTyped = parameterInfo.ConverterBase.ClassType == ClassType.Value;
                    int position = parameterInfo.Position;

                    switch (position)
                    {
                        case 0:
                            if (isStronglyTyped)
                            {
                                arguments.Arg0 = ((JsonParameterInfo<TArg0>)parameterInfo).TypedDefaultValue!;
                            }
                            else
                            {
                                arguments.Arg0 = (TArg0)parameterInfo.DefaultValue!;
                            }
                            break;
                        case 1:
                            if (isStronglyTyped)
                            {
                                arguments.Arg1 = ((JsonParameterInfo<TArg1>)parameterInfo).TypedDefaultValue!;
                            }
                            else
                            {
                                arguments.Arg1 = (TArg1)parameterInfo.DefaultValue!;
                            }
                            break;
                        case 2:
                            if (isStronglyTyped)
                            {
                                arguments.Arg2 = ((JsonParameterInfo<TArg2>)parameterInfo).TypedDefaultValue!;
                            }
                            else
                            {
                                arguments.Arg2 = (TArg2)parameterInfo.DefaultValue!;
                            }
                            break;
                        case 3:
                            if (isStronglyTyped)
                            {
                                arguments.Arg3 = ((JsonParameterInfo<TArg3>)parameterInfo).TypedDefaultValue!;
                            }
                            else
                            {
                                arguments.Arg3 = (TArg3)parameterInfo.DefaultValue!;
                            }
                            break;
                        default:
                            Debug.Fail("More than 4 params: we should be in override for LargeObjectWithParameterizedConstructorConverter.");
                            throw new InvalidOperationException();
                    }
                }
            }

            state.Current.CtorArgumentState!.Arguments = arguments;
        }
    }
}
