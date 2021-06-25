// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Text.Json.Serialization.Tests
{
    public static class PropertyOrderTests
    {
        private class MyPoco
        {
            public int B { get; set; }

            [JsonPropertyOrder(1)]
            public int A { get; set; }

            [JsonPropertyOrder(-1)]
            public int C { get; set; }
        }

        [Fact]
        public static void CamelCaseDeserializeNoMatch()
        {
            string json = JsonSerializer.Serialize<MyPoco>(new MyPoco());
            Assert.Equal("{\"C\":0,\"B\":0,\"A\":0}", json);
        }
    }
}
