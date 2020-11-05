// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace System.Reflection.Tests
{
    public class NullableConditionTests_NoNullableContext
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public class TestClass
        {
            public TestClass()
            {
            }

            public object Object { get; set; }
            public int IntProperty { get; set; }
            public int? NullableIntProperty { get; set; }
        }

        [Fact]
        public void T()
        {
            var tc = new TestClass();

            PropertyInfo info = typeof(TestClass).GetProperty("Object")!;
            Assert.Equal(NullableCondition.MaybeNull, info.GetNullability());

            info = typeof(TestClass).GetProperty("IntProperty")!;
            Assert.Equal(NullableCondition.NotNull, info.GetNullability());

            info = typeof(TestClass).GetProperty("NullableIntProperty")!;
            Assert.Equal(NullableCondition.MaybeNull, info.GetNullability());
        }
    }
}
