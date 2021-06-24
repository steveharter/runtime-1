//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.

//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using System.Text.Encodings.Web;
//using System.Text.Unicode;
//using Xunit;

//namespace System.Text.Json.Serialization.Tests
//{
//    public static partial class OnSerializeTests
//    {
//        private class ClassWithOnSerializeMethods :
//            IJsonOnDeserializing,
//            IJsonOnDeserialized,
//            IJsonOnSerialing,
//            IJsonOnSerialized
//        {
//            public int MyInt { get; set; }

//            internal bool OnSerializingCalled;
//            internal bool OnSerializedCalled;
//            internal bool OnDeserializingCalled;
//            internal bool OnDeserializedCalled;

//            [JsonOnSerializing]
//            private void OnSerializing()
//            {
//                OnSerializingCalled = true;
//                Assert.Equal(1, MyInt);
//                MyInt = 42;
//            }

//            [JsonOnSerialized]
//            private void OnSerialized()
//            {
//                OnSerializedCalled = true;
//                Assert.Equal(42, MyInt);
//            }

//            [JsonOnDeserializing]
//            private void OnDeserializing()
//            {
//                OnDeserializingCalled = true;
//                Assert.Equal(0, MyInt);
//            }

//            [JsonOnDeserialized]
//            private void OnDeserialized()
//            {
//                OnDeserializedCalled = true;
//                Assert.Equal(1, MyInt);
//                MyInt = 42;
//            }
//        }

//        [Fact]
//        public static void OnXXX()
//        {
//            ClassWithOnSerializeMethods obj = new();
//            obj.MyInt = 1;

//            JsonSerializerOptions options = new();
//            string json = JsonSerializer.Serialize(obj);
//            Assert.Equal("{\"MyInt\":42}", json);
//            Assert.True(obj.OnSerializingCalled);
//            Assert.True(obj.OnSerializedCalled);
//            Assert.False(obj.OnDeserializingCalled);
//            Assert.False(obj.OnDeserializedCalled);

//            obj = JsonSerializer.Deserialize<ClassWithOnSerializeMethods>("{\"MyInt\":1}");
//            Assert.Equal(42, obj.MyInt);
//            Assert.False(obj.OnSerializingCalled);
//            Assert.False(obj.OnSerializedCalled);
//            Assert.True(obj.OnDeserializingCalled);
//            Assert.True(obj.OnDeserializedCalled);
//        }
//    }
//}
