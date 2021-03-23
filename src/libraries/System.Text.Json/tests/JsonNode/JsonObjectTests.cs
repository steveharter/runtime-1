// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Tests;
using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class JsonObjectTests
    {
        [Fact]
        public static void MissingProperty()
        {
            var options = new JsonSerializerOptions();
            JsonObject obj = JsonSerializer.Deserialize<JsonObject>("{}", options);
            Assert.Null(obj["NonExistingProperty"]);
        }

        [Fact]
        public static void CaseSensitivity_ReadMode()
        {
            var options = new JsonSerializerOptions();
            JsonObject obj = JsonSerializer.Deserialize<JsonObject>("{\"MyProperty\":42}", options);

            Assert.Equal(42, obj["MyProperty"].GetValue<int>());
            Assert.Null(obj["myproperty"]);
            Assert.Null(obj["MYPROPERTY"]);

            options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            obj = JsonSerializer.Deserialize<JsonObject>("{\"MyProperty\":42}", options);

            Assert.Equal(42, obj["MyProperty"].GetValue<int>());
            Assert.Equal(42, obj["myproperty"].GetValue<int>());
            Assert.Equal(42, obj["MYPROPERTY"].GetValue<int>());
        }

        [Fact]
        public static void CaseSensitivity_EditMode()
        {
            var jArray = new JsonArray();
            var jObject = new JsonObject();
            jObject.Add("MyProperty", 42);
            jObject.Add("myproperty", 42); // No exception

            // Options on direct node.
            var options = new JsonNodeOptions { PropertyNameCaseInsensitive = true };
            jArray = new JsonArray();
            jObject = new JsonObject(options);
            jObject.Add("MyProperty", 42);
            jArray.Add(jObject);
            Assert.Throws<ArgumentException>(() => jObject.Add("myproperty", 42));

            // Options on parent node.
            jArray = new JsonArray(options);
            jObject = new JsonObject();
            jArray.Add(jObject);
            jObject.Add("MyProperty", 42);
            Assert.Throws<ArgumentException>(() => jObject.Add("myproperty", 42));

            // Dictionary is created when Add is called for the first time, so we need to be added first.
            jArray = new JsonArray(options);
            jObject = new JsonObject();
            jObject.Add("MyProperty", 42);
            jArray.Add(jObject);
            jObject.Add("myproperty", 42); // no exception since options were not set in time.
        }

        [Fact]
        public static void NamingPoliciesAreNotUsed()
        {
            const string Json = "{\"myProperty\":42}";

            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = new SimpleSnakeCasePolicy();

            JsonObject obj = JsonSerializer.Deserialize<JsonObject>(Json, options);
            string json = obj.ToJsonString();
            JsonTestHelper.AssertJsonEqual(Json, json);
        }

        [Fact]
        public static void QuotedNumbers_Serialize()
        {
            var options = new JsonSerializerOptions();
            options.NumberHandling = JsonNumberHandling.WriteAsString;

            JsonValue obj = JsonValue.Create(42);
            string json = obj.ToJsonString(options);
            Assert.Equal("\"42\"", json);

            obj = JsonValue.Create(double.NaN);
            json = obj.ToJsonString(options);
            Assert.Equal("\"NaN\"", json);
        }
    }
}
