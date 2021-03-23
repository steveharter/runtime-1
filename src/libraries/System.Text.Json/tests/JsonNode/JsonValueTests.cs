// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;
using Xunit;

namespace System.Text.Json.Node.Tests
{
    public static class JsonValueTests
    {
        private class Polymorphic_Base { }
        private class Polymorphic_Derived : Polymorphic_Base { }

        [Fact]
        public static void Polymorphic()
        {
            JsonValue value;

            Polymorphic_Base baseClass = new Polymorphic_Derived();
            value = JsonValue.Create(baseClass);
            Assert.Same(baseClass, value.GetValue<Polymorphic_Derived>());

            Polymorphic_Derived derivedClass = new Polymorphic_Derived();
            value = JsonValue.Create(derivedClass);
            Assert.Same(derivedClass, value.GetValue<Polymorphic_Base>());
        }

        [Fact]
        public static void QuotedNumbers_Deserialize()
        {
            var options = new JsonSerializerOptions();
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString |
                JsonNumberHandling.AllowNamedFloatingPointLiterals;

            JsonNode obj = JsonSerializer.Deserialize<JsonNode>("\"42\"", options);
            Assert.IsAssignableFrom<JsonValue>(obj);
            Assert.Equal(42, obj.GetValue<int>(options));

            obj = JsonSerializer.Deserialize<JsonNode>("\"NaN\"", options);
            Assert.IsAssignableFrom<JsonValue>(obj);
            Assert.Equal(double.NaN, obj.GetValue<double>(options));
            Assert.Equal(float.NaN, obj.GetValue<float>(options));
        }

    }
}
