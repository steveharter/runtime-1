// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class OperatorTests
    {
        private const string ExpectedPrimitiveJson =
                @"{" +
                @"""MyInt16"":1," +
                @"""MyInt32"":2," +
                @"""MyInt64"":3," +
                @"""MyUInt16"":4," +
                @"""MyUInt32"":5," +
                @"""MyUInt64"":6," +
                @"""MyByte"":7," +
                @"""MySByte"":8," +
                @"""MyChar"":""a""," +
                @"""MyString"":""Hello""," +
                @"""MyBooleanTrue"":true," +
                @"""MyBooleanFalse"":false," +
                @"""MySingle"":1.1," +
                @"""MyDouble"":2.2," +
                @"""MyDecimal"":3.3," +
                @"""MyDateTime"":""2019-01-30T12:01:02Z""," +
                @"""MyDateTimeOffset"":""2019-01-30T12:01:02+01:00""," +
                @"""MyGuid"":""1b33498a-7b7d-4dda-9c13-f6aa4ab449a6""" + // note lowercase
                @"}";

        [Fact]
        public static void ImplicitOperators_FromProperties()
        {
            var jObject = new JsonObject();
            jObject["MyInt16"] = 1;
            jObject["MyInt32"] = 2;
            jObject["MyInt64"] = 3;
            jObject["MyUInt16"] = 4;
            jObject["MyUInt32"] = 5;
            jObject["MyUInt64"] = 6;
            jObject["MyByte"] = 7;
            jObject["MySByte"] = 8;
            jObject["MyChar"] = 'a';
            jObject["MyString"] = "Hello";
            jObject["MyBooleanTrue"] = true;
            jObject["MyBooleanFalse"] = false;
            jObject["MySingle"] = 1.1f;
            jObject["MyDouble"] = 2.2d;
            jObject["MyDecimal"] = 3.3m;
            jObject["MyDateTime"] = new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc);
            jObject["MyDateTimeOffset"] = new DateTimeOffset(2019, 1, 30, 12, 1, 2, new TimeSpan(1, 0, 0));
            jObject["MyGuid"] = new Guid("1B33498A-7B7D-4DDA-9C13-F6AA4AB449A6");

            string json = jObject.ToJsonString();

            Assert.Equal(ExpectedPrimitiveJson, json);
        }

        [Fact]
        public static void ExplicitOperators_FromProperties()
        {
            JsonObject jObject = JsonNode.Parse(ExpectedPrimitiveJson).AsObject();
            Assert.Equal(1, (short)jObject["MyInt16"]);
            Assert.Equal(2, (int)jObject["MyInt32"]);
            Assert.Equal(3, (long)jObject["MyInt64"]);
            Assert.Equal(4, (ushort)jObject["MyUInt16"]);
            Assert.Equal<uint>(5, (uint)jObject["MyUInt32"]);
            Assert.Equal<ulong>(6, (ulong)jObject["MyUInt64"]);
            Assert.Equal(7, (byte)jObject["MyByte"]);
            Assert.Equal(8, (sbyte)jObject["MySByte"]);
            Assert.Equal('a', (char)jObject["MyChar"]);
            Assert.Equal("Hello", (string)jObject["MyString"]);
            Assert.True((bool)jObject["MyBooleanTrue"]);
            Assert.False((bool)jObject["MyBooleanFalse"]);
            Assert.Equal(1.1f, (float)jObject["MySingle"]);
            Assert.Equal(2.2d, (double)jObject["MyDouble"]);
            Assert.Equal(3.3m, (decimal)jObject["MyDecimal"]);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), (DateTime)jObject["MyDateTime"]);
            Assert.Equal(new DateTimeOffset(2019, 1, 30, 12, 1, 2, new TimeSpan(1, 0, 0)), (DateTimeOffset)jObject["MyDateTimeOffset"]);
            Assert.Equal(new Guid("1B33498A-7B7D-4DDA-9C13-F6AA4AB449A6"), (Guid)jObject["MyGuid"]);
        }

        [Fact]
        public static void ExplicitOperators_FromValues()
        {
            Assert.Equal(1, (short)(JsonNode)(short)1);
            Assert.Equal(2, (int)(JsonNode)2);
            Assert.Equal(3, (long)(JsonNode)(long)3);
            Assert.Equal(4, (ushort)(JsonNode)(ushort)4);
            Assert.Equal<uint>(5, (uint)(JsonNode)(uint)5);
            Assert.Equal<ulong>(6, (ulong)(JsonNode)(ulong)6);
            Assert.Equal(7, (byte)(JsonNode)(byte)7);
            Assert.Equal(8, (sbyte)(JsonNode)(sbyte)8);
            Assert.Equal('a', (char)(JsonNode)'a');
            Assert.Equal("Hello", (string)(JsonNode)"Hello");
            Assert.True((bool)(JsonNode)true);
            Assert.False((bool)(JsonNode)false);
            Assert.Equal(1.1f, (float)(JsonNode)1.1f);
            Assert.Equal(2.2d, (double)(JsonNode)2.2d);
            Assert.Equal(3.3m, (decimal)(JsonNode)3.3m);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc),
                (DateTime)(JsonNode)new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc));
            Assert.Equal(new DateTimeOffset(2019, 1, 30, 12, 1, 2, new TimeSpan(1, 0, 0)),
                (DateTimeOffset)(JsonNode)new DateTimeOffset(2019, 1, 30, 12, 1, 2, new TimeSpan(1, 0, 0)));
            Assert.Equal(new Guid("1B33498A-7B7D-4DDA-9C13-F6AA4AB449A6"),
                (Guid)(JsonNode)new Guid("1B33498A-7B7D-4DDA-9C13-F6AA4AB449A6"));
        }

        [Fact]
        public static void ExplicitOperators_FromNullValues()
        {
            Assert.Null((short?)(JsonValue)null);
            Assert.Null((int?)(JsonValue)null);
            Assert.Null((long?)(JsonValue)null);
            Assert.Null((ushort?)(JsonValue)null);
            Assert.Null((uint?)(JsonValue)null);
            Assert.Null((ulong?)(JsonValue)null);
            Assert.Null((sbyte?)(JsonValue)null);
            Assert.Null((char?)(JsonValue)null);
            Assert.Null((string)(JsonValue)null);
            Assert.Null((bool?)(JsonValue)null);
            Assert.Null((float?)(JsonValue)null);
            Assert.Null((double?)(JsonValue)null);
            Assert.Null((decimal?)(JsonValue)null);
            Assert.Null((DateTime?)(JsonValue)null);
            Assert.Null((DateTimeOffset?)(JsonValue)null);
            Assert.Null((Guid?)(JsonValue)null);
        }

        [Fact]
        public static void CastsNotSupported()
        {
            // Since generics and boxing do not support casts, we get InvalidCastExceptions here.
            Assert.Throws<InvalidCastException>(() => (byte)(JsonNode)(long)3); // narrowing
            Assert.Throws<InvalidCastException>(() => (long)(JsonNode)(byte)3); // widening
        }

        [Fact]
        public static void Boxing()
        {
            var node = JsonValue.Create(42);
            Assert.Equal(42, node.GetValue<int>());

            Assert.Equal<object>(42, node.GetValue<object>());
        }
    }
}
