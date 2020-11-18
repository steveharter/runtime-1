// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Reflection.Tests
{
    public class NullableConditionTests_NoNullableContext
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public class TestClass
        {
            public TestClass() { }

            public object Object { get; set; }
            public int IntProperty { get; set; }
            public int? NullableIntProperty { get; set; }
            public object ObjectNoSetter { get; }
        }

        [Fact]
        public void Test()
        {
            var tc = new TestClass();
            PropertyInfo info;

            info = typeof(TestClass).GetProperty("Object")!;
            Assert.Equal(NullableInCondition.AllowNull, info.GetAttributedInfo().NullableIn);
            Assert.Equal(NullableOutCondition.MaybeNull, info.GetAttributedInfo().NullableOut);
            Assert.False(info.GetAttributedInfo().HasNullableContext);

            info = typeof(TestClass).GetProperty("ObjectNoSetter")!;
            Assert.Equal(NullableInCondition.NotApplicable, info.GetAttributedInfo().NullableIn);
            Assert.Equal(NullableOutCondition.MaybeNull, info.GetAttributedInfo().NullableOut);
            Assert.False(info.GetAttributedInfo().HasNullableContext);

            info = typeof(TestClass).GetProperty("IntProperty")!;
            Assert.Equal(NullableInCondition.DisallowNull, info.GetAttributedInfo().NullableIn);
            Assert.Equal(NullableOutCondition.NotNull, info.GetAttributedInfo().NullableOut);
            Assert.False(info.GetAttributedInfo().HasNullableContext);

            info = typeof(TestClass).GetProperty("NullableIntProperty")!;
            Assert.Equal(NullableInCondition.AllowNull, info.GetAttributedInfo().NullableIn);
            Assert.Equal(NullableOutCondition.MaybeNull, info.GetAttributedInfo().NullableOut);
            Assert.False(info.GetAttributedInfo().HasNullableContext);
        }
    }
}
