// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class DomUsageTests
    {
        [Fact]
        public static void DomImplicitOperators()
        {
            var jObj = new JsonObject
            {
                // Primitives
                ["MyString"] = "Hello!",
                ["MyNull"] = null,
                ["MyBoolean"] = false,

                // Nested array
                ["MyArray"] = new JsonArray(2, 3, 42),

                // Additional primitives
                ["MyInt"] = 43,
                ["MyDateTime"] = new DateTime(2020, 7, 8),
                ["MyGuid"] = new Guid("ed957609-cdfe-412f-88c1-02daca1b4f51"),

                // Nested objects
                ["MyObject"] = new JsonObject
                {
                    ["MyString"] = "Hello!!"
                },

                ["Child"] = new JsonObject()
                {
                    ["ChildProp"] = 1
                }
            };

            string json = jObj.ToJsonString();
            JsonTestHelper.AssertJsonEqual(JsonNodeTests.ExpectedDomJson, json);
        }

        [Fact]
        public static void DomExplicit()
        {
            var jObj = new JsonObject
            {
                // Primitives
                ["MyString"] = JsonValue.Create("Hello!"),
                ["MyNull"] = null,
                ["MyBoolean"] = JsonValue.Create(false),

                // Nested array
                ["MyArray"] = new JsonArray
                (
                    JsonValue.Create(2),
                    JsonValue.Create(3),
                    JsonValue.Create(42)
                ),

                // Additional primitives
                ["MyInt"] = JsonValue.Create(43),
                ["MyDateTime"] = JsonValue.Create(new DateTime(2020, 7, 8)),
                ["MyGuid"] = JsonValue.Create(new Guid("ed957609-cdfe-412f-88c1-02daca1b4f51")),

                // Nested objects
                ["MyObject"] = new JsonObject
                {
                    ["MyString"] = JsonValue.Create("Hello!!")
                },

                ["Child"] = new JsonObject
                {
                    ["ChildProp"] = JsonValue.Create(1)
                }
            };

            string json = jObj.ToJsonString();
            JsonTestHelper.AssertJsonEqual(JsonNodeTests.ExpectedDomJson, json);
        }

        [Fact]
        public static void VerifyMutableDom_WithoutUsingDynamicKeyword()
        {
            const string Json =
                "{\"MyString\":\"Hello\",\"MyNull\":null,\"MyBoolean\":true,\"MyArray\":[1,2],\"MyInt\":42,\"MyDateTime\":\"2020-07-08T00:00:00\",\"MyGuid\":\"ed957609-cdfe-412f-88c1-02daca1b4f51\",\"MyObject\":{\"MyString\":\"World\"}}";

            JsonNode obj = JsonSerializer.Deserialize<JsonObject>(Json);
            Verify();

            // Verify the values are round-trippable.
            ((JsonArray)obj["MyArray"]).RemoveAt(2);
            Verify();

            void Verify()
            {
                // Change some primitives.
                obj["MyString"] = JsonValue.Create("Hello!");
                obj["MyBoolean"] = JsonValue.Create(false);
                obj["MyInt"] = JsonValue.Create(43);

                // Add nested objects.
                obj["MyObject"] = new JsonObject();
                obj["MyObject"]["MyString"] = JsonValue.Create("Hello!!");

                obj["Child"] = new JsonObject();
                obj["Child"]["ChildProp"] = JsonValue.Create(1);

                // Modify number elements.
                obj["MyArray"][0] = JsonValue.Create(2);
                obj["MyArray"][1] = JsonValue.Create(3);

                // Add an element.
                ((JsonArray)obj["MyArray"]).Add(JsonValue.Create(42));

                string json = obj.ToJsonString();
                JsonTestHelper.AssertJsonEqual(JsonNodeTests.ExpectedDomJson, json);
            }
        }
    }
}
