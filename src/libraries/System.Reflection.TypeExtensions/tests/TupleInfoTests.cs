// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Reflection.Tests
{
    public class TupleTests
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        private class TestClass
        {
            public (int a, bool b) TwoParamFunction()
            {
                throw null;
            }

            public (int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10) TenParamFunction()
            {
                throw null;
            }

            public Tuple<int, bool> ReferenceTuple2()
            {
                throw null;
            }
        }

        [Fact]
        public void ValueTuple2()
        {
            MethodInfo mi = typeof(TestClass).GetMethod("TwoParamFunction");
            Assert.NotNull(mi);

            TupleInfo[] tupleInfo = mi.GetTupleInfo();

            Assert.Equal(2, tupleInfo.Length);

            Assert.Equal("a", tupleInfo[0].TransformName);
            Assert.Equal("Item1", tupleInfo[0].Name);
            Assert.Equal(typeof(int), tupleInfo[0].Type);

            Assert.Equal("b", tupleInfo[1].TransformName);
            Assert.Equal("Item2", tupleInfo[1].Name);
            Assert.Equal(typeof(bool), tupleInfo[1].Type);
        }

        [Fact]
        public void ReferenceTupleArguments2()
        {
            MethodInfo mi = typeof(TestClass).GetMethod("ReferenceTuple2");
            Assert.NotNull(mi);

            TupleInfo[] tupleInfo = mi.GetTupleInfo();

            Assert.Equal(2, tupleInfo.Length);

            Assert.Equal("Item1", tupleInfo[0].Name);
            Assert.Equal(typeof(int), tupleInfo[0].Type);
            Assert.Null(tupleInfo[0].TransformName);

            Assert.Equal("Item2", tupleInfo[1].Name);
            Assert.Equal(typeof(bool), tupleInfo[1].Type);
            Assert.Null(tupleInfo[1].TransformName);
        }

        [Fact]
        public void Arguments10()
        {
            MethodInfo mi = typeof(TestClass).GetMethod("TenParamFunction");
            Assert.NotNull(mi);

            Assert.Equal(8, mi.GetTupleInfo().Length);

            TupleInfo[] tupleInfo = mi.GetTupleInfo();

            for (int i = 0; i <=6; i++)
            {
                Assert.Equal($"a{i + 1}", tupleInfo[i].TransformName);
                Assert.Equal($"Item{i + 1}", tupleInfo[i].Name);
                Assert.Equal(typeof(int), tupleInfo[i].Type);
            }

            //Assert.Equal("a8", tupleInfo[7].TransformName);
            //Assert.Equal("Rest", tupleInfo[7].Name);
            //Assert.Equal(typeof(int), tupleInfo[7].Type); //todo: typeof(System.ValueTuple<int, int, int>)

            //Assert.Equal("a9", tupleInfo[8].TransformName);
            //Assert.Equal("Rest", tupleInfo[8].Name);
            //Assert.Equal(typeof(int), tupleInfo[8].Type);
        }


        // todo: anonymous methods

    }
}
