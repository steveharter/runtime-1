// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace System.Reflection.Tests
{
    public class NullableConditionTests_NullableContext
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public class TestClass
        {
            public TestClass()
            {
                NonNullableObject = 1;
                NonNullableObjectWithAttribute = 1;
            }

            public object NonNullableObject { get; set; }
            [NotNull] public object NonNullableObjectWithAttribute { get; set; }
            //[NotNull] [MYATTR] public object NonNullableObjectWithAttribute { get; set; }
            //public object NonNullableObjectWithAttribute { get; set; }
        }

        [Fact]
        public void T()
        {
            var tc = new TestClass();
            tc.NonNullableObject = null!;

            PropertyInfo info = typeof(TestClass).GetProperty("NonNullableObjectWithAttribute")!;
            Assert.Equal(NullableCondition.NotNull, info.GetNullability());

            info = typeof(TestClass).GetProperty("NonNullableObject")!;
            Assert.Equal(NullableCondition.NotNull, info.GetNullability());
       }
    }
}
