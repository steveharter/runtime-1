// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// ------------------------------------------------------------------------------
// Changes to this file must follow the https://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace System.Reflection
{
    public static partial class AssemblyExtensions
    {
        public static System.Type[] GetExportedTypes(this System.Reflection.Assembly assembly) { throw null; }
        public static System.Reflection.Module[] GetModules(this System.Reflection.Assembly assembly) { throw null; }
        public static System.Type[] GetTypes(this System.Reflection.Assembly assembly) { throw null; }
    }
    public static partial class EventInfoExtensions
    {
        public static System.Reflection.MethodInfo? GetAddMethod(this System.Reflection.EventInfo eventInfo) { throw null; }
        public static System.Reflection.MethodInfo? GetAddMethod(this System.Reflection.EventInfo eventInfo, bool nonPublic) { throw null; }
        public static System.Reflection.NullableCondition GetNullability(this System.Reflection.EventInfo eventInfo) { throw null; }
        public static System.Reflection.MethodInfo? GetRaiseMethod(this System.Reflection.EventInfo eventInfo) { throw null; }
        public static System.Reflection.MethodInfo? GetRaiseMethod(this System.Reflection.EventInfo eventInfo, bool nonPublic) { throw null; }
        public static System.Reflection.MethodInfo? GetRemoveMethod(this System.Reflection.EventInfo eventInfo) { throw null; }
        public static System.Reflection.MethodInfo? GetRemoveMethod(this System.Reflection.EventInfo eventInfo, bool nonPublic) { throw null; }
        public static System.Reflection.TupleInfo[] GetTupleInfo(this System.Reflection.EventInfo eventInfo) { throw null; }
    }
    public static partial class FieldInfoExtensions
    {
        public static System.Reflection.NullableCondition GetNullability(this System.Reflection.FieldInfo fieldInfo) { throw null; }
        public static System.Reflection.TupleInfo[] GetTupleInfo(this System.Reflection.FieldInfo fieldInfo) { throw null; }
    }
    public static partial class MemberInfoExtensions
    {
        public static int GetMetadataToken(this System.Reflection.MemberInfo member) { throw null; }
        public static System.Reflection.NullableCondition GetNullability(this System.Reflection.MemberInfo member) { throw null; }
        public static System.Reflection.TupleInfo[] GetTupleInfo(this System.Reflection.MemberInfo member) { throw null; }
        public static bool HasMetadataToken(this System.Reflection.MemberInfo member) { throw null; }
    }
    public static partial class MethodInfoExtensions
    {
        public static System.Reflection.MethodInfo GetBaseDefinition(this System.Reflection.MethodInfo method) { throw null; }
        public static System.Reflection.NullableCondition GetNullability(this System.Reflection.MethodInfo method) { throw null; }
        public static System.Reflection.TupleInfo[] GetTupleInfo(this System.Reflection.MethodInfo method) { throw null; }
    }
    public static partial class ModuleExtensions
    {
        public static System.Guid GetModuleVersionId(this System.Reflection.Module module) { throw null; }
        public static bool HasModuleVersionId(this System.Reflection.Module module) { throw null; }
    }
    public enum NullableCondition
    {
        MaybeNull = 0,
        MaybeNullWhenTrue = 1,
        MaybeNullWhenFalse = 2,
        NotNull = 3,
        NotNullIfNotNull = 4,
        NotNullWhenTrue = 5,
        NotNullWhenFalse = 6,
    }
    public static partial class ParameterInfoExtensions
    {
        public static System.Reflection.NullableCondition GetNullability(this System.Reflection.ParameterInfo parameter) { throw null; }
        public static System.Reflection.TupleInfo[] GetTupleInfo(this System.Reflection.ParameterInfo parameter) { throw null; }
    }
    public static partial class PropertyInfoExtensions
    {
        public static System.Reflection.MethodInfo[] GetAccessors(this System.Reflection.PropertyInfo property) { throw null; }
        public static System.Reflection.MethodInfo[] GetAccessors(this System.Reflection.PropertyInfo property, bool nonPublic) { throw null; }
        public static System.Reflection.MethodInfo? GetGetMethod(this System.Reflection.PropertyInfo property) { throw null; }
        public static System.Reflection.MethodInfo? GetGetMethod(this System.Reflection.PropertyInfo property, bool nonPublic) { throw null; }
        public static System.Reflection.NullableCondition GetNullability(this System.Reflection.PropertyInfo property) { throw null; }
        public static System.Reflection.MethodInfo? GetSetMethod(this System.Reflection.PropertyInfo property) { throw null; }
        public static System.Reflection.MethodInfo? GetSetMethod(this System.Reflection.PropertyInfo property, bool nonPublic) { throw null; }
        public static System.Reflection.TupleInfo[] GetTupleInfo(this System.Reflection.PropertyInfo property) { throw null; }
    }
    public partial struct TupleInfo
    {
        private object _dummy;
        private int _dummyPrimitive;
        public readonly string Name { get { throw null; } }
        public readonly string? TransformName { get { throw null; } }
        public readonly System.Type Type { get { throw null; } }
    }
    public static partial class TypeExtensions
    {
        public static System.Reflection.ConstructorInfo? GetConstructor(this System.Type type, System.Type[] types) { throw null; }
        public static System.Reflection.ConstructorInfo[] GetConstructors(this System.Type type) { throw null; }
        public static System.Reflection.ConstructorInfo[] GetConstructors(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.MemberInfo[] GetDefaultMembers(this System.Type type) { throw null; }
        public static System.Reflection.EventInfo? GetEvent(this System.Type type, string name) { throw null; }
        public static System.Reflection.EventInfo? GetEvent(this System.Type type, string name, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.EventInfo[] GetEvents(this System.Type type) { throw null; }
        public static System.Reflection.EventInfo[] GetEvents(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.FieldInfo? GetField(this System.Type type, string name) { throw null; }
        public static System.Reflection.FieldInfo? GetField(this System.Type type, string name, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.FieldInfo[] GetFields(this System.Type type) { throw null; }
        public static System.Reflection.FieldInfo[] GetFields(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Type[] GetGenericArguments(this System.Type type) { throw null; }
        public static System.Type[] GetInterfaces(this System.Type type) { throw null; }
        public static System.Reflection.MemberInfo[] GetMember(this System.Type type, string name) { throw null; }
        public static System.Reflection.MemberInfo[] GetMember(this System.Type type, string name, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.MemberInfo[] GetMembers(this System.Type type) { throw null; }
        public static System.Reflection.MemberInfo[] GetMembers(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.MethodInfo? GetMethod(this System.Type type, string name) { throw null; }
        public static System.Reflection.MethodInfo? GetMethod(this System.Type type, string name, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.MethodInfo? GetMethod(this System.Type type, string name, System.Type[] types) { throw null; }
        public static System.Reflection.MethodInfo[] GetMethods(this System.Type type) { throw null; }
        public static System.Reflection.MethodInfo[] GetMethods(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Type? GetNestedType(this System.Type type, string name, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Type[] GetNestedTypes(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.PropertyInfo[] GetProperties(this System.Type type) { throw null; }
        public static System.Reflection.PropertyInfo[] GetProperties(this System.Type type, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.PropertyInfo? GetProperty(this System.Type type, string name) { throw null; }
        public static System.Reflection.PropertyInfo? GetProperty(this System.Type type, string name, System.Reflection.BindingFlags bindingAttr) { throw null; }
        public static System.Reflection.PropertyInfo? GetProperty(this System.Type type, string name, System.Type? returnType) { throw null; }
        public static System.Reflection.PropertyInfo? GetProperty(this System.Type type, string name, System.Type? returnType, System.Type[] types) { throw null; }
        public static bool IsAssignableFrom(this System.Type type, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] System.Type? c) { throw null; }
        public static bool IsInstanceOfType(this System.Type type, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object? o) { throw null; }
    }
}
