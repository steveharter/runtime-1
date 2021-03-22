// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Reflection.Tests
{
    public unsafe class InvokeDirectTests
    {
        [Fact]
        public void Test_Class()
        {
            MethodInfo mi = typeof(TestClass).GetMethod(nameof(TestClass.Initialize), BindingFlags.Instance | BindingFlags.Public);

            int intField = 42;
            int intProperty = 43;
            string stringProperty = "Hello";

            var obj = new TestClass();

            mi.InvokeFuncDirect(
                TypedReference.FromRef(ref obj),
                TypedReference.FromRef(ref intField),
                TypedReference.FromRef(ref intProperty),
                TypedReference.FromRef(ref stringProperty));

            Assert.Equal(42, obj.MyIntField);
            Assert.Equal(43, obj.MyIntProperty);
            Assert.Equal("Hello", obj.MyStringProperty);
        }

        [Fact]
        public void Test_Struct()
        {
            MethodInfo mi = typeof(TestStruct).GetMethod(nameof(TestStruct.Initialize), BindingFlags.Instance | BindingFlags.Public);

            int intField = 42;
            int intProperty = 43;
            string stringProperty = "Hello";

            var obj = default(TestStruct);

            mi.InvokeFuncDirect(
                TypedReference.FromRef(ref obj),
                TypedReference.FromRef(ref intField),
                TypedReference.FromRef(ref intProperty),
                TypedReference.FromRef(ref stringProperty));

            Assert.Equal(42, obj.MyIntField);
            Assert.Equal(43, obj.MyIntProperty);
            Assert.Equal("Hello", obj.MyStringProperty);
        }

        [Fact]
        public void Test_RefStruct()
        {
            MethodInfo mi = typeof(TestRefStruct).GetMethod(nameof(TestRefStruct.Initialize), BindingFlags.Instance | BindingFlags.Public);

            int intField = 42;
            int intProperty = 43;

            var obj = default(TestRefStruct);

            mi.InvokeFuncDirect(
                TypedReference.FromIntPtr(new IntPtr(&obj), typeof(TestRefStruct)),
                TypedReference.FromRef(ref intField),
                TypedReference.FromRef(ref intProperty));

            Assert.Equal(42, obj.MyIntField);
            Assert.Equal(43, obj.MyIntProperty);
        }

        private sealed class TestClass
        {
            public int MyIntField;
            public int MyIntProperty { get; set; }
            public string MyStringProperty { get; set; }

            public void Initialize(int myIntField, int myIntProperty, string myStringProperty)
            {
                MyIntField = myIntField;
                MyIntProperty = myIntProperty;
                MyStringProperty = myStringProperty;
            }

            public void AddToMyIntField(int value)
            {
                MyIntField += value;
            }

            public string ConcatToMyStringProperty(string value)
            {
                return MyStringProperty + value;
            }
        }

        private struct TestStruct
        {
            public int MyIntField;
            public int MyIntProperty { get; set; }
            public string MyStringProperty { get; set; }

            public void Initialize(int myIntField, int myIntProperty, string myStringProperty)
            {
                MyIntField = myIntField;
                MyIntProperty = myIntProperty;
                MyStringProperty = myStringProperty;
            }

            public void AddToMyIntField(int value)
            {
                MyIntField += value;
            }

            public string ConcatToMyStringProperty(string value)
            {
                return MyStringProperty + value;
            }
        }

        private ref struct TestRefStruct
        {
            public int MyIntField;
            public int MyIntProperty { get; set; }

            public void Initialize(int myIntField, int myIntProperty)
            {
                MyIntField = myIntField;
                MyIntProperty = myIntProperty;
            }

            public void AddToMyIntField(int value)
            {
                MyIntField += value;
            }
        }
    }
}
