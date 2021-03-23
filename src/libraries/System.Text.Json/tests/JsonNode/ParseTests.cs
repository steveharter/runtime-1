// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Text.Json.Serialization.Tests;
using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class ParseTests
    {
        [Fact]
        public static void NullReference_Fail()
        {
            Assert.Throws<ArgumentNullException>(() => JsonSerializer.Deserialize<JsonNode>((string)null));
            Assert.Throws<ArgumentNullException>(() => JsonNode.Parse(null));
            Assert.Throws<ArgumentNullException>(() => JsonNode.ParseUtf8(null));
        }

        [Fact]
        public static void NullLiteral()
        {
            Assert.Null(JsonSerializer.Deserialize<JsonNode>("null"));
            Assert.Null(JsonNode.Parse("null"));

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes("null")))
            {
                Assert.Null(JsonNode.ParseUtf8(stream));
            }
        }

        [Fact]
        public static void ReadSimpleObject()
        {
            using (MemoryStream stream = new MemoryStream(SimpleTestClass.s_data))
            {
                JsonNode node = JsonNode.ParseUtf8(stream);

                string actual = node.ToJsonString();
                // Replace the escaped "+" sign used with DateTimeOffset.
                actual = actual.Replace("\\u002B", "+");

                Assert.Equal(SimpleTestClass.s_json.StripWhitespace(), actual);
            }
        }

        [Fact]
        public static void ReadSimpleObjectWithTrailingTrivia()
        {
            byte[] data = Encoding.UTF8.GetBytes(SimpleTestClass.s_json + " /* Multi\r\nLine Comment */\t");
            using (MemoryStream stream = new MemoryStream(data))
            {
                var options = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip
                };

                JsonNode node = JsonNode.ParseUtf8(stream, nodeOptions: null, options);

                string actual = node.ToJsonString();
                // Replace the escaped "+" sign used with DateTimeOffset.
                actual = actual.Replace("\\u002B", "+");

                Assert.Equal(SimpleTestClass.s_json.StripWhitespace(), actual);
            }
        }

        [Fact]
        public static void ReadPrimitives()
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(@"1")))
            {
                int i = JsonNode.ParseUtf8(stream).AsValue().GetValue<int>();
                Assert.Equal(1, i);
            }
        }

        [Fact]
        public static void ParseThenEdit()
        {
            const string Expected = "{\"MyString\":null,\"Node\":42,\"Array\":[43],\"Value\":44,\"IntValue\":45,\"Object\":{\"Property\":46}}";

            JsonNode node = JsonNode.Parse(Expected);
            Assert.Equal(Expected, node.ToJsonString());

            // Change a primitive
            node["IntValue"] = 1;
            const string ExpectedAfterEdit1 = "{\"MyString\":null,\"Node\":42,\"Array\":[43],\"Value\":44,\"IntValue\":1,\"Object\":{\"Property\":46}}";
            Assert.Equal(ExpectedAfterEdit1, node.ToJsonString());

            // Change element
            node["Array"][0] = 2;
            const string ExpectedAfterEdit2 = "{\"MyString\":null,\"Node\":42,\"Array\":[2],\"Value\":44,\"IntValue\":1,\"Object\":{\"Property\":46}}";
            Assert.Equal(ExpectedAfterEdit2, node.ToJsonString());

            // Change property
            node["MyString"] = "3";
            const string ExpectedAfterEdit3 = "{\"MyString\":\"3\",\"Node\":42,\"Array\":[2],\"Value\":44,\"IntValue\":1,\"Object\":{\"Property\":46}}";
            Assert.Equal(ExpectedAfterEdit3, node.ToJsonString());
        }
    }
}

