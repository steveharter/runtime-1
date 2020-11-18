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

                // these will fail to compile:
                //object obj = AllowNull_DisallowNull;
                //AllowNull_DisallowNull = null;

                //object x = AllowNull_MaybeNull;
            }

            public object NonNullableObject { get; set; }
            [DisallowNull] public object? AllowNull_DisallowNull { get; set; }
            [AllowNull] [MaybeNull] public object AllowNull_MaybeNull { get; set; }
        }

        [Fact]
        public void Test()
        {
            var tc = new TestClass();
            tc.NonNullableObject = null!;

            PropertyInfo info;

            info = typeof(TestClass).GetProperty("AllowNull_MaybeNull")!;
            Assert.Equal(NullableInCondition.AllowNull, info.GetAttributedInfo().NullableIn);
            Assert.Equal(NullableOutCondition.MaybeNull, info.GetAttributedInfo().NullableOut);
            Assert.True(info.GetAttributedInfo().HasNullableContext);

            info = typeof(TestClass).GetProperty("NonNullableObject")!;
            Assert.Equal(NullableInCondition.DisallowNull, info.GetAttributedInfo().NullableIn);
            Assert.Equal(NullableOutCondition.NotNull, info.GetAttributedInfo().NullableOut);
            Assert.True(info.GetAttributedInfo().HasNullableContext);
        }
    }
}
