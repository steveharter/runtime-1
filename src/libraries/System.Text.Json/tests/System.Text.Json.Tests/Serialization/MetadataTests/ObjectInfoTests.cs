// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace System.Text.Json.Tests.Serialization
{
    public abstract partial class MetadataTests
    {
        private sealed class ReorderPropertiesHandler : JsonObjectInfoHandler
        {
            public ReorderPropertiesHandler(JsonSerializerOptions options) : base(options) { }

            protected override void Created(JsonTypeInfo objectTypeInfo)
            {
                IList<JsonPropertyInfo> list = objectTypeInfo.Properties.List;

                List<JsonPropertyInfo> ordered = list
                    .OrderBy(p => p.JsonName)
                    .ToList();

                objectTypeInfo.Properties = new JsonPropertyInfoCollection(ordered, Options.PropertyNameCaseInsensitive);
            }
        }

        private sealed class ChangePropertyNameHandler : JsonObjectInfoHandler
        {
            public ChangePropertyNameHandler(JsonSerializerOptions options) : base(options) { }

            protected override void Created(JsonTypeInfo objectTypeInfo)
            {
                // Make a copy of the original since changing the property name affects the underlying list and existing enumerators.
                List<JsonPropertyInfo> original = objectTypeInfo.Properties.List.ToList();

                foreach (JsonPropertyInfo info in original)
                {
                    info.JsonName += "_NEW";
                }
            }
        }

        private class PocoOrderedProperties
        {
            public int P2 { get; set; }
            public int P3 { get; set; }
            public int P1 { get; set; }
        }

        [Fact]
        public void ReorderProperties()
        {
            string Expected = "{\"P1\":0,\"P2\":0,\"P3\":0}";
            PocoOrderedProperties obj = new();

            string json = JsonSerializer.Serialize(obj);
            Assert.NotEqual(Expected, json);

            JsonSerializerOptions options = new();
            options.ObjectInfoHandler = new ReorderPropertiesHandler(options);

            string json2 = JsonSerializer.Serialize(obj, options);
            Assert.Equal(Expected, json2);
        }

        [Fact]
        public void ChangePropertyNames()
        {
            string Expected = "{\"P2_NEW\":0,\"P3_NEW\":0,\"P1_NEW\":0}";
            PocoOrderedProperties obj = new();

            string json = JsonSerializer.Serialize(obj);
            Assert.NotEqual(Expected, json);

            JsonSerializerOptions options = new();
            options.ObjectInfoHandler = new ChangePropertyNameHandler(options);

            string json2 = JsonSerializer.Serialize(obj, options);
            Assert.Equal(Expected, json2);
        }
    }
}
