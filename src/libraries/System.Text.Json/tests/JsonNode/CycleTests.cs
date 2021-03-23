// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class CycleTests
    {
        [Fact]
        public static void DetectCycles_Object()
        {
            var jObject = new JsonObject { };
            Assert.Throws<InvalidOperationException>(() => jObject.Add("a", jObject));

            var jObject2 = new JsonObject { };
            jObject.Add("a", jObject2);
            Assert.Throws<InvalidOperationException>(() => jObject2.Add("b", jObject));
        }

        [Fact]
        public static void DetectCycles_Array()
        {
            var jArray = new JsonArray { };
            Assert.Throws<InvalidOperationException>(() => jArray.Add(jArray));

            var jArray2 = new JsonArray { };
            jArray.Add(jArray2);
            Assert.Throws<InvalidOperationException>(() => jArray2.Add(jArray));
        }
    }
}
