// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace System.Text.Json.Serialization.Tests
{
    public static class DynamicTests_CustomConverters
    {
        [Fact]
        public static void SystemObjectDeserializationPolicy_JsonElementDynamic()
        {
            var options = new JsonSerializerOptions();
            options.SetSystemObjectDeserializationPolicy(SystemObjectDeserializationPolicy.JsonElementDynamic);

            dynamic obj = JsonSerializer.Deserialize<dynamic>(DynamicTests_DefaultConverters.Json, options);
            Assert.IsType<ExpandoObject>(obj);

            VerifyPrimitives();
            VerifyObject();
            VerifyArray();

            void VerifyPrimitives()
            {
                // Primitives use JsonElement.
                Assert.Equal("Hello", obj.MyString.GetString());
                Assert.True(obj.MyBoolean.GetBoolean());
                Assert.Equal(42, obj.MyInt.GetInt32());
                Assert.Equal(4.2, obj.MyDouble.GetDouble());
                Assert.Equal(DynamicTests_DefaultConverters.MyDateTime, obj.MyDateTime.GetDateTime());
            }

            void VerifyObject()
            {
                Assert.IsType<ExpandoObject>(obj.MyObject);
                Assert.Equal("World", obj.MyObject.MyString.GetString());
            }

            void VerifyArray()
            {
                Assert.IsType<JsonArray>(obj.MyArray);
                Assert.Equal(2, obj.MyArray.Count);
                Assert.Equal(1, obj.MyArray[0].GetInt32());
                Assert.Equal(2, obj.MyArray[1].GetInt32());
            }
        }

        [Fact]
        public static void SystemObjectDeserializationPolicy_AutoConversion()
        {
            var options = new JsonSerializerOptions();
            options.SetSystemObjectDeserializationPolicy(SystemObjectDeserializationPolicy.AutoConversion);

            VerifyAutoConvertPrimitives(options);
            VerifyAutoConvertArray(options);

            // Although we could use 'dynamic' here to wrap first-level properties,
            // the intent of using AutoConversion is to avoid dynamic code.
            object obj = JsonSerializer.Deserialize<object>(DynamicTests_DefaultConverters.Json, options);
            Assert.IsAssignableFrom<JsonObject>(obj);
            var jsonObj = (JsonObject)obj;

            VerifyPrimitives();
            VerifyObject();
            VerifyArray();

            void VerifyPrimitives()
            {
                // Primitives are auto-converted.
                Assert.Equal("Hello", jsonObj["MyString"]);
                Assert.Equal(true, jsonObj["MyBoolean"]);
                Assert.Equal(42L, jsonObj["MyInt"]);
                Assert.Equal(4.2, jsonObj["MyDouble"]);
                Assert.Equal(DynamicTests_DefaultConverters.MyDateTime, jsonObj["MyDateTime"]);
            }

            void VerifyObject()
            {
                Assert.IsType<JsonObject>(jsonObj.GetObject("MyObject"));
                Assert.Equal("World", jsonObj.GetObject("MyObject")["MyString"]);
            }

            void VerifyArray()
            {
                Assert.IsType<JsonArray>(jsonObj.GetArray("MyArray"));
                Assert.Equal(2, jsonObj.GetArray("MyArray").Count);
                Assert.Equal(1L, jsonObj.GetArray("MyArray")[0]);
                Assert.Equal(2L, jsonObj.GetArray("MyArray")[1]);
            }
        }

        [Fact]
        public static void SystemObjectDeserializationPolicy_AutoConversionDynamic()
        {
            var options = new JsonSerializerOptions();
            options.SetSystemObjectDeserializationPolicy(SystemObjectDeserializationPolicy.AutoConversionDynamic);

            VerifyAutoConvertPrimitives(options);
            VerifyAutoConvertArray(options);

            dynamic obj = JsonSerializer.Deserialize<dynamic>(DynamicTests_DefaultConverters.Json, options);
            Assert.IsType<ExpandoObject>(obj);

            VerifyPrimitives();
            VerifyObject();
            VerifyArray();

            void VerifyPrimitives()
            {
                // Primitives are auto-converted.
                Assert.Equal("Hello", obj.MyString);
                Assert.True(obj.MyBoolean);
                Assert.Equal(42L, obj.MyInt);
                Assert.Equal(4.2, obj.MyDouble);
                Assert.Equal(DynamicTests_DefaultConverters.MyDateTime, obj.MyDateTime);
            }

            void VerifyObject()
            {
                Assert.IsType<ExpandoObject>(obj.MyObject);
                Assert.Equal("World", obj.MyObject.MyString);
            }

            void VerifyArray()
            {
                Assert.IsType<JsonArray>(obj.MyArray);
                Assert.Equal(2, obj.MyArray.Count);
                Assert.Equal(1, obj.MyArray[0]);
                Assert.Equal(2, obj.MyArray[1]);
            }
        }

        private static void VerifyAutoConvertPrimitives(JsonSerializerOptions options)
        {
            {
                const string Value = @"null";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.Null(obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.Null(newtonsoftObj);
            }

            {
                const string Value = @"""mystring""";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<string>(obj);
                Assert.Equal("mystring", obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<string>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }

            {
                const string Value = "true";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<bool>(obj);
                Assert.True((bool)obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<bool>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }

            {
                const string Value = "false";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<bool>(obj);
                Assert.False((bool)obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<bool>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }

            {
                const string Value = "123";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<long>(obj);
                Assert.Equal((long)123, obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<long>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }

            {
                const string Value = "123.45";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<double>(obj);
                Assert.Equal(123.45d, obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<double>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }

            {
                const string Value = @"""2019-01-30T12:01:02Z""";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<DateTime>(obj);
                Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<DateTime>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }

            {
                const string Value = @"""2019-01-30T12:01:02+01:00""";

                object obj = JsonSerializer.Deserialize<object>(Value, options);
                Assert.IsType<DateTime>(obj);

                object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
                Assert.IsType<DateTime>(newtonsoftObj);
                Assert.Equal(newtonsoftObj, obj);
            }
        }

        private static void VerifyAutoConvertArray(JsonSerializerOptions options)
        {
            const string Value = "[]";

            object obj = JsonSerializer.Deserialize<object>(Value, options);
            Assert.IsAssignableFrom<IList<object>>(obj);

            // Types are different.
            object newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            Assert.IsType<Newtonsoft.Json.Linq.JArray>(newtonsoftObj);
        }
    }
}
