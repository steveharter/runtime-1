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

            VerifyJsonElementPrimitives(obj);
            VerifyObjectWithJsonElement(obj);
            VerifyJsonArrayWithJsonElement(obj);

            static void VerifyJsonElementPrimitives(dynamic obj)
            {
                Assert.IsType<ExpandoObject>(obj);

                // Primitives use JsonElement.
                Assert.Equal("Hello", obj.MyString.GetString());
                Assert.True(obj.MyBoolean.GetBoolean());
                Assert.Equal(42, obj.MyInt.GetInt32());
                Assert.Equal(4.2, obj.MyDouble.GetDouble());
                Assert.Equal(DynamicTests_DefaultConverters.MyDateTime, obj.MyDateTime.GetDateTime());

                // Primitives use JsonElement which is immutable, but being dynamic allows the property's type to be changed
                // to a non-JsonElement type.
                DynamicTests_CustomConverters.VerifyMutablePrimitives_Dynamic(obj);
            }

            static void VerifyObjectWithJsonElement(dynamic obj)
            {
                Assert.IsType<ExpandoObject>(obj);
                Assert.IsType<ExpandoObject>(obj.MyObject);
                Assert.Equal("World", obj.MyObject.MyString.GetString());

                DynamicTests_CustomConverters.VerifyMutableObject_Dynamic(obj);
            }

            static void VerifyJsonArrayWithJsonElement(dynamic obj)
            {
                Assert.IsType<ExpandoObject>(obj);

                Assert.IsType<JsonArray>(obj.MyArray);
                Assert.Equal(2, obj.MyArray.Count);
                Assert.Equal(1, obj.MyArray[0].GetInt32());
                Assert.Equal(2, obj.MyArray[1].GetInt32());

                DynamicTests_CustomConverters.VerifyMutableArray_JsonArray(obj);
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

            VerifyDynamicPrimitivesFromJsonObject(jsonObj);
            VerifyJsonObject(jsonObj);
            VerifyJsonArrayFromJsonObject(jsonObj);

            static void VerifyDynamicPrimitivesFromJsonObject(JsonObject obj)
            {
                // Primitives are auto-converted.
                Assert.Equal("Hello", obj["MyString"]);
                Assert.Equal(true, obj["MyBoolean"]);
                Assert.Equal(42L, obj["MyInt"]);
                Assert.Equal(4.2, obj["MyDouble"]);
                Assert.Equal(DynamicTests_DefaultConverters.MyDateTime, obj["MyDateTime"]);

                obj["MyString"] = "World";
                Assert.Equal("World", obj["MyString"]);

                obj["MyBoolean"] = false;
                Assert.Equal(false, obj["MyBoolean"]);

                obj["MyInt"] = 43;
                Assert.Equal(43, obj["MyInt"]);

                obj["MyDouble"] = 4.4;
                Assert.Equal(4.4, obj["MyDouble"]);

                DateTime dt = DateTime.Now;
                obj["MyDateTime"] = dt;
                Assert.Equal(dt, obj["MyDateTime"]);
            }

            static void VerifyJsonObject(JsonObject obj)
            {
                Assert.IsType<JsonObject>(obj["MyObject"]);
                Assert.Equal("World", obj.GetObject("MyObject")["MyString"]);

                JsonObject newJsonObj = new JsonObject();
                newJsonObj["MyString"] = "Hello";
                obj["MyObject"] = newJsonObj;
                Assert.Equal("Hello", obj.GetObject("MyObject")["MyString"]);
            }

            static void VerifyJsonArrayFromJsonObject(JsonObject obj)
            {
                Assert.IsType<JsonArray>(obj.GetArray("MyArray"));
                Assert.Equal(2, obj.GetArray("MyArray").Count);
                Assert.Equal(1L, obj.GetArray("MyArray")[0]);
                Assert.Equal(2L, obj.GetArray("MyArray")[1]);

                obj["MyArray"] = new JsonArray();
                Assert.IsType<JsonArray>(obj.GetArray("MyArray"));
                Assert.Equal(0, obj.GetArray("MyArray").Count);
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

            VerifyDynamicPrimitives(obj);
            VerifyDynamic(obj);
            VerifyJsonArrayWithDynamicPrimitives(obj);

            static void VerifyDynamicPrimitives(dynamic obj)
            {
                Assert.IsType<ExpandoObject>(obj);

                // Primitives are auto-converted.
                Assert.Equal("Hello", obj.MyString);
                Assert.True(obj.MyBoolean);
                Assert.Equal(42L, obj.MyInt);
                Assert.Equal(4.2, obj.MyDouble);
                Assert.Equal(DynamicTests_DefaultConverters.MyDateTime, obj.MyDateTime);

                DynamicTests_CustomConverters.VerifyMutablePrimitives_Dynamic(obj);
            }

            static void VerifyDynamic(dynamic obj)
            {
                Assert.IsType<ExpandoObject>(obj.MyObject);
                Assert.Equal("World", obj.MyObject.MyString);

                DynamicTests_CustomConverters.VerifyMutableObject_Dynamic(obj);
            }

            static void VerifyJsonArrayWithDynamicPrimitives(dynamic obj)
            {
                Assert.IsType<ExpandoObject>(obj);

                Assert.IsType<JsonArray>(obj.MyArray);
                Assert.Equal(2, obj.MyArray.Count);
                Assert.Equal(1, obj.MyArray[0]);
                Assert.Equal(2, obj.MyArray[1]);

                int count = 0;
                foreach (long value in obj.MyArray)
                {
                    count++;
                }
                Assert.Equal(2, count);

                DynamicTests_CustomConverters.VerifyMutableArray_JsonArray(obj);
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

        private static void VerifyMutablePrimitives_Dynamic(dynamic obj)
        {
            Assert.IsType<ExpandoObject>(obj);

            obj.MyString = "World";
            Assert.Equal("World", obj.MyString);

            obj.MyBoolean = false;
            Assert.False(obj.MyBoolean);

            obj.MyInt = 43;
            Assert.Equal(43, obj.MyInt);

            obj.MyDouble = 4.3;
            Assert.Equal(4.3, obj.MyDouble);

            DateTime dt = DateTime.Now;
            obj.MyDateTime = dt;
            Assert.Equal(dt, obj.MyDateTime);
        }

        private static void VerifyMutableObject_Dynamic(dynamic obj)
        {
            Assert.IsType<ExpandoObject>(obj);

            obj.MyObject = new ExpandoObject();
            obj.MyObject.MyString = "Hello";
            Assert.Equal("Hello", obj.MyObject.MyString);
        }

        private static void VerifyMutableArray_JsonArray(dynamic obj)
        {
            Assert.IsType<ExpandoObject>(obj);

            obj.MyArray = new JsonArray();
            Assert.Equal(0, obj.MyArray.Count);
        }
    }
}
