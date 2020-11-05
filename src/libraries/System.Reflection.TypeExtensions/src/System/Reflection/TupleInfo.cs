// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
    public struct TupleInfo
    {
        internal TupleInfo(string name, Type type, string? transformName)
        {
            Name = name;
            Type = type;
            TransformName = transformName;
        }

        public string Name { get; }
        public Type Type { get; }
        public string? TransformName { get; }

        internal static TupleInfo[] GetTupleInfo(MethodInfo method)
        {
            const BindingFlags MemberLookup = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            if (IsValueTupleType(method.ReturnType))
            {
                TypeInfo typeInfo = method.ReturnParameter.ParameterType.GetTypeInfo();
                FieldInfo[] fields = typeInfo.GetFields(MemberLookup);
                TupleInfo[] tupleInfos = new TupleInfo[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    tupleInfos[i] = new TupleInfo(
                        fields[i].Name,
                        fields[i].FieldType,
                        GetTransformName(method, i));
                }

                return tupleInfos;
            }
            else if (IsReferenceTupleType(method.ReturnType))
            {
                TypeInfo typeInfo = method.ReturnParameter.ParameterType.GetTypeInfo();
                PropertyInfo[] properties = typeInfo.GetProperties(MemberLookup);
                TupleInfo[] tupleInfos = new TupleInfo[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                {
                    tupleInfos[i] = new TupleInfo(
                        properties[i].Name,
                        properties[i].PropertyType,
                        GetTransformName(method, i));
                }

                return tupleInfos;
            }

            throw new InvalidOperationException("Not a Tuple or ValueTuple");
        }

        private static bool IsValueTupleType(Type type)
        {
            string? name = type.FullName;
            if (name == null)
            {
                return false;
            }

            return name.StartsWith("System.ValueTuple`");
        }

        private static bool IsReferenceTupleType(Type type)
        {
            string? name = type.FullName;
            if (name == null)
            {
                return false;
            }

            return name.StartsWith("System.Tuple`");
        }

        private static string? GetTransformName(MethodInfo methodInfo, int index)
        {
            foreach (CustomAttributeData cad in methodInfo.ReturnParameter.GetCustomAttributesData())
            {
                if (cad.AttributeType.FullName == "System.Runtime.CompilerServices.TupleElementNamesAttribute")
                {
                    IList<CustomAttributeTypedArgument> ctorArgs = cad.ConstructorArguments;

                    // Currently there is only one constructor, but verify.
                    if (ctorArgs.Count == 1)
                    {
                        if (ctorArgs[0].Value is IReadOnlyList<CustomAttributeTypedArgument> args)
                        {
                            Debug.Assert(args[index].Value is string);
                            return (string)args[index].Value!;
                        }
                    }
                }
            }

            return null;
        }
    }
}
