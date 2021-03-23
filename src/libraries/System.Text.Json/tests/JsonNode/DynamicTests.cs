// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if BUILDING_INBOX_LIBRARY

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class DynamicTests
    {
        private enum MyCustomEnum
        {
            Default = 0,
            FortyTwo = 42,
            Hello = 77
        }

        [Fact]
        public static void CodeSample_Simple_Serialization_WithImplicitOperators()
        {
            dynamic jObj = new JsonObject();

            // Dynamic objects do not support object initializers, so they can't be used.

            // Primitives
            jObj.MyString = "Hello!";
            Assert.IsAssignableFrom<JsonValue>(jObj.MyString);

            jObj.MyNull = null;
            jObj.MyBoolean = false;

            // Nested array
            jObj.MyArray = new JsonArray(2, 3, 42);

            // Additional primitives
            jObj.MyInt = 43;
            jObj.MyDateTime = new DateTime(2020, 7, 8);
            jObj.MyGuid = new Guid("ed957609-cdfe-412f-88c1-02daca1b4f51");

            // Nested objects
            jObj.MyObject = new JsonObject();
            jObj.MyObject.MyString = "Hello!!";

            jObj.Child = new JsonObject();
            jObj.Child.ChildProp = 1;

            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;

            string json = jObj.ToJsonString(options);
            JsonTestHelper.AssertJsonEqual(JsonNodeTests.ExpectedDomJson, json);
        }

        [Fact]
        public static void VerifyPrimitives()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
            options.Converters.Add(new JsonStringEnumConverter());

            dynamic obj = JsonSerializer.Deserialize<object>(Serialization.Tests.DynamicTests.Json, options);
            Assert.IsAssignableFrom<JsonObject>(obj);

            // JsonValue created from a JSON string.
            Assert.IsAssignableFrom<JsonValue>(obj.MyString);
            Assert.Equal("Hello", (string)obj.MyString);

            // Verify other string-based types.
            // Since this requires a custom converter, an exception is thrown.
            Assert.ThrowsAny<Exception>(() => (MyCustomEnum)obj.MyString);

            // We must pass the JsonSerializerOptions
            Assert.Equal(MyCustomEnum.Hello, obj.MyString.GetValue<MyCustomEnum>(options));

            Assert.Equal(Serialization.Tests.DynamicTests.MyDateTime, (DateTime)obj.MyDateTime);
            Assert.Equal(Serialization.Tests.DynamicTests.MyGuid, (Guid)obj.MyGuid);

            // JsonValue created from a JSON bool.
            Assert.IsAssignableFrom<JsonValue>(obj.MyBoolean);
            bool b = obj.MyBoolean;
            Assert.True(b);

            // Numbers must specify the type through a cast or assignment.
            Assert.IsAssignableFrom<JsonValue>(obj.MyInt);
            Assert.ThrowsAny<Exception>(() => obj.MyInt == 42L);
            Assert.Equal(42L, (long)obj.MyInt);
            Assert.Equal((byte)42, (byte)obj.MyInt);

            // Verify int-based Enum support through "unknown number type" fallback.
            Assert.Equal(MyCustomEnum.FortyTwo, (MyCustomEnum)obj.MyInt);

            // Verify floating point.
            obj = JsonSerializer.Deserialize<object>("4.2", options);
            Assert.IsAssignableFrom<JsonValue>(obj);

            double dbl = (double)obj;
            Assert.Equal(4.2, dbl);
        }

        [Fact]
        public static void VerifyArray()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
            options.Converters.Add(new JsonStringEnumConverter());

            dynamic obj = JsonSerializer.Deserialize<object>(Serialization.Tests.DynamicTests.Json, options);
            Assert.IsAssignableFrom<JsonObject>(obj);
            Assert.IsAssignableFrom<JsonArray>(obj.MyArray);

            Assert.Equal(2, obj.MyArray.Count);
            Assert.Equal(1, (int)obj.MyArray[0]);
            Assert.Equal(2, (int)obj.MyArray[1]);

            // Ensure we can enumerate
            int count = 0;
            foreach (object value in obj.MyArray)
            {
                count++;
            }
            Assert.Equal(2, count);

            // Ensure we can mutate through indexers
            obj.MyArray[0] = 10;
            // This is actually a JsonValue due to the implicit operators.
            Assert.IsAssignableFrom<JsonValue>(obj.MyArray[0]);

            Assert.Equal(10, (int)obj.MyArray[0]);
        }

        [Fact]
        public static void JsonTypes_Serialize()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;

            // Guid (string)
            string GuidJson = $"{Serialization.Tests.DynamicTests.MyGuid.ToString("D")}";
            string GuidJsonWithQuotes = $"\"{GuidJson}\"";

            // We can't convert an unquoted string to a Guid
            dynamic dynamicString = JsonValue.Create(GuidJson);
            Assert.Throws<InvalidCastException>(() => (Guid)dynamicString);

            string json;

            // Number (JsonElement)
            using (JsonDocument doc = JsonDocument.Parse($"{decimal.MaxValue}"))
            {
                dynamic dynamicNumber = JsonValue.Create(doc.RootElement);
                Assert.Equal<decimal>(decimal.MaxValue, (decimal)dynamicNumber);
                json = dynamicNumber.ToJsonString(options);
                Assert.Equal(decimal.MaxValue.ToString(), json);
            }

            // Boolean
            dynamic dynamicBool = JsonValue.Create(true);
            Assert.True((bool)dynamicBool);
            json = dynamicBool.ToJsonString(options);
            Assert.Equal("true", json);

            // Array
            dynamic arr = new JsonArray();
            arr.Add(1);
            arr.Add(2);
            json = arr.ToJsonString(options);
            Assert.Equal("[1,2]", json);

            // Object
            dynamic dynamicObject = new JsonObject();
            dynamicObject.One = 1;
            dynamicObject.Two = 2;

            json = dynamicObject.ToJsonString(options);
            JsonTestHelper.AssertJsonEqual("{\"One\":1,\"Two\":2}", json);
        }

        [Fact]
        public static void JsonTypes_Deserialize()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;

            Assert.IsType<JsonObject>(JsonSerializer.Deserialize<JsonObject>("{}", options));
            Assert.IsType<JsonObject>(JsonSerializer.Deserialize<object>("{}", options));
            Assert.IsType<JsonObject>(JsonSerializer.Deserialize<JsonNode>("{}", options));
            Assert.IsType<JsonArray>(JsonSerializer.Deserialize<JsonArray>("[]", options));
            Assert.IsType<JsonArray>(JsonSerializer.Deserialize<object>("[]", options));
            Assert.IsType<JsonArray>(JsonSerializer.Deserialize<JsonNode>("[]", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<object>("true", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<object>("true", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonNode>("true", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonValue>("0", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<object>("0", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonNode>("0", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonValue>("1.2", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<object>("1.2", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonNode>("1.2", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonValue>("\"str\"", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<object>("\"str\"", options));
            Assert.IsAssignableFrom<JsonValue>(JsonSerializer.Deserialize<JsonNode>("\"str\"", options));
        }

        /// <summary>
        /// Use a mutable DOM with the 'dynamic' keyword.
        /// </summary>
        [Fact]
        public static void VerifyMutableDom_UsingDynamicKeyword_WithClrPrimitives()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;

            dynamic obj = JsonSerializer.Deserialize<object>(Serialization.Tests.DynamicTests.Json, options);
            Assert.IsAssignableFrom<JsonObject>(obj);

            // Change some primitives.
            obj.MyString = "Hello!";
            obj.MyBoolean = false;
            obj.MyInt = 43;

            // Add nested objects.
            // Use JsonObject; ExpandoObject should not be used since it doesn't have the same semantics including
            // null handling and case-sensitivity that respects JsonSerializerOptions.PropertyNameCaseInsensitive.
            dynamic myObject = new JsonObject();
            myObject.MyString = "Hello!!";
            obj.MyObject = myObject;

            dynamic child = new JsonObject();
            child.ChildProp = 1;
            obj.Child = child;

            // Modify number elements.
            dynamic arr = obj.MyArray;
            arr[0] = (int)arr[0] + 1;
            arr[1] = (int)arr[1] + 1;

            // Add an element.
            arr.Add(42);

            string json = obj.ToJsonString(options);
            JsonTestHelper.AssertJsonEqual(JsonNodeTests.ExpectedDomJson, json);
        }

        /// <summary>
        /// Use a mutable DOM with the 'dynamic' keyword.
        /// </summary>
        [Fact]
        public static void VerifyMutableDom_UsingDynamicKeyword_WithJsonNode()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;

            dynamic obj = JsonSerializer.Deserialize<object>(Serialization.Tests.DynamicTests.Json, options);
            Assert.IsAssignableFrom<JsonObject>(obj);

            // Change some primitives.
            obj.MyString = "Hello!";
            obj.MyBoolean = false;
            obj.MyInt = 43;

            // Add nested objects.
            // Use JsonObject; ExpandoObject should not be used since it doesn't have the same semantics including
            // null handling and case-sensitivity that respects JsonSerializerOptions.PropertyNameCaseInsensitive.
            dynamic myObject = new JsonObject();
            myObject.MyString = "Hello!!";
            obj.MyObject = myObject;

            dynamic child = new JsonObject();
            child.ChildProp = 1;
            obj.Child = child;

            // Modify number elements.
            dynamic arr = obj.MyArray;
            arr[0] = (int)arr[0] + 1;
            arr[1] = (int)arr[1] + 1;

            // Add an element.
            arr.Add(42);

            string json = obj.ToJsonString(options);
            JsonTestHelper.AssertJsonEqual(JsonNodeTests.ExpectedDomJson, json);
        }

        [Fact]
        public static void DynamicObject_CaseSensitivity()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
            dynamic obj = JsonSerializer.Deserialize<object>("{\"MyProperty\":42}", options);

            Assert.IsType<JsonObject>(obj);
            Assert.IsAssignableFrom<JsonValue>(obj.MyProperty);

            //dynamic temp = obj.MyProperty;
            //int sdfsfd = temp;
            int sdfsfdsdf = (int)obj.MyProperty;

            var sdf = obj.MyProperty;

            Assert.Equal(42, (int)obj.MyProperty);
            Assert.Null(obj.myProperty);
            Assert.Null(obj.MYPROPERTY);

            options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
            options.PropertyNameCaseInsensitive = true;
            obj = JsonSerializer.Deserialize<object>("{\"MyProperty\":42}", options);

            Assert.Equal(42, (int)obj.MyProperty);
            Assert.Equal(42, (int)obj.myproperty);
            Assert.Equal(42, (int)obj.MYPROPERTY);
        }

        [Fact]
        public static void DynamicObject_MissingProperty()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;
            dynamic obj = JsonSerializer.Deserialize<object>("{}", options);
            Assert.Equal(null, obj.NonExistingProperty);
        }

        private class BlogPost
        {
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string AuthorTwitter { get; set; }
            public string Body { get; set; }
            public DateTime PostedDate { get; set; }
        }

        [Fact]
        public static void DynamicObject_LINQ_Convert()
        {
            string json = @"
            [
              {
                ""Title"": ""TITLE."",
                ""Author"":
                {
                  ""Name"": ""NAME."",
                  ""Mail"": ""MAIL."",
                  ""Picture"": ""/PICTURE.png""
                },
                ""Date"": ""2021-01-20T19:30:00"",
                ""BodyHtml"": ""Content.""
              }
            ]";

            JsonArray arr = JsonSerializer.Deserialize<JsonArray>(json);

            // Convert nested JSON to a flat POCO.
            IList<BlogPost> blogPosts = arr.Select(p => new BlogPost
            {
                Title = p["Title"].GetValue<string>(),
                AuthorName = p["Author"]["Name"].GetValue<string>(),
                AuthorTwitter = p["Author"]["Mail"].GetValue<string>(),
                PostedDate = p["Date"].GetValue<DateTime>(),
                Body = p["BodyHtml"].GetValue<string>()
            }).ToList();

            const string expected = "[{\"Title\":\"TITLE.\",\"AuthorName\":\"NAME.\",\"AuthorTwitter\":\"MAIL.\",\"Body\":\"Content.\",\"PostedDate\":\"2021-01-20T19:30:00\"}]";

            string json_out = JsonSerializer.Serialize(blogPosts);
            Assert.Equal(expected, json_out);
        }

        const string Linq_Query_Json = @"
        [
          {
            ""OrderId"":100, ""Customer"":
            {
              ""Name"":""Customer1"",
              ""City"":""Fargo""
            }
          },
          {
            ""OrderId"":200, ""Customer"":
            {
              ""Name"":""Customer2"",
              ""City"":""Redmond""
            }
          },
          {
            ""OrderId"":300, ""Customer"":
            {
              ""Name"":""Customer3"",
              ""City"":""Fargo""
            }
          }
        ]";

        [Fact]
        public static void DynamicObject_LINQ_Query()
        {
            JsonArray allOrders = JsonSerializer.Deserialize<JsonArray>(Linq_Query_Json);
            IEnumerable<JsonNode> orders = allOrders.Where<JsonNode>(o => o["Customer"]["City"].GetValue<string>() == "Fargo");

            Assert.Equal(2, orders.Count());
            Assert.Equal(100, orders.ElementAt(0)["OrderId"].GetValue<int>());
            Assert.Equal(300, orders.ElementAt(1)["OrderId"].GetValue<int>());
            Assert.Equal("Customer1", orders.ElementAt(0)["Customer"]["Name"].GetValue<string>());
            Assert.Equal("Customer3", orders.ElementAt(1)["Customer"]["Name"].GetValue<string>());
        }

        [Fact]
        public static void DynamicObject_LINQ_Query_dynamic()
        {
            var options = new JsonSerializerOptions();
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode;

            IEnumerable<dynamic> allOrders = JsonSerializer.Deserialize<IEnumerable<dynamic>>(Linq_Query_Json, options);
            IEnumerable<dynamic> orders = allOrders.Where(o => ((string)o.Customer.City) == "Fargo");

            Assert.Equal(2, orders.Count());
            Assert.Equal(100, (int)orders.ElementAt(0).OrderId);
            Assert.Equal(300, (int)orders.ElementAt(1).OrderId);
            Assert.Equal("Customer1", (string)orders.ElementAt(0).Customer.Name);
            Assert.Equal("Customer3", (string)orders.ElementAt(1).Customer.Name);

            // Verify methods can be called as well.
            Assert.Equal(100, orders.ElementAt(0).OrderId.GetValue<int>());
            Assert.Equal(300, orders.ElementAt(1).OrderId.GetValue<int>());
            Assert.Equal("Customer1", orders.ElementAt(0).Customer.Name.GetValue<string>());
            Assert.Equal("Customer3", orders.ElementAt(1).Customer.Name.GetValue<string>());
        }
    }
}
#endif
