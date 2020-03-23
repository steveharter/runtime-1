// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Temp
{
    public class Perf
    {
        public class WeatherForecast
        {
            public DateTime Date { get; set; }

            public int TemperatureC { get; set; }

            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

            public string Summary { get; set; }
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public static IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [Fact]
        static void TEST()
        {
            IEnumerable<WeatherForecast> result = Get();

            //JsonSerializerOptions options = new JsonSerializerOptions();
            //JsonSerializerOptions options2 = new JsonSerializerOptions();

            //JsonSerializer.SerializeToUtf8Bytes(result);

            //Manual(result); // 8
            // Auto(result); //50
            Custom(result); // 22 --> 21
        }

        static void Manual(IEnumerable<WeatherForecast> result)
        {
            var sw = new Stopwatch();
            sw.Start();

            var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                writer.WriteStartArray();

                foreach (var w in result)
                {
                    writer.WriteStartObject();
                    writer.WriteString("Date", w.Date);
                    writer.WriteNumber("TemperatureC", w.TemperatureC);
                    writer.WriteNumber("TemperatureF", w.TemperatureF);
                    writer.WriteString("Summary", w.Summary);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        static void Auto(IEnumerable<WeatherForecast> result)
        {
            var sw = new Stopwatch();
            sw.Start();

            var ms = new MemoryStream();
            foreach (var w in result)
            {
                JsonSerializer.SerializeToUtf8Bytes(result);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        static void Custom(IEnumerable<WeatherForecast> result)
        {
            var sw = new Stopwatch();
            sw.Start();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new WeatherForecastConverter());
            options.Converters.Add(new IEnumerableWeatherForecastConverter());

            var ms = new MemoryStream();
            foreach (var w in result)
            {
                JsonSerializer.SerializeToUtf8Bytes(result, options);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        public class WeatherForecastConverter : JsonConverter<WeatherForecast>
        {
            public override WeatherForecast Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, WeatherForecast value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Date", value.Date);
                writer.WriteNumber("TemperatureC", value.TemperatureC);
                writer.WriteNumber("TemperatureF", value.TemperatureF);
                writer.WriteString("Summary", value.Summary);
                writer.WriteEndObject();
            }
        }

        public class IEnumerableWeatherForecastConverter : JsonConverter<IEnumerable<WeatherForecast>>
        {
            JsonConverter<WeatherForecast> converter = null;

            public override IEnumerable<WeatherForecast> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, IEnumerable<WeatherForecast> value, JsonSerializerOptions options)
            {
                if (converter == null)
                {
                    converter = (JsonConverter<WeatherForecast>)options.GetConverter(typeof(WeatherForecast));
                }

                writer.WriteStartArray();

                foreach (WeatherForecast element in value)
                {
                    converter.Write(writer, element, options);
                }

                writer.WriteEndArray();
            }
        }

        public class POCO : IJsonSerializable
        {
            JsonClassInfo IJsonSerializable.GetJsonClassInfo()
            {
                return new MyJsonClassInfo(this);
            }

            private class MyJsonClassInfo : JsonClassInfo
            {
                private POCO _obj;

                public MyJsonClassInfo(POCO obj)
                {
                    _obj = obj;
                }

                public override JsonPropertyInfo[] GetProperties()
                {
                    return new JsonPropertyInfo[]
                    {
                    new JsonPropertyInfo<int>(
                        getter : () => { return _obj.MyIntProp; },
                        setter : (value) => { _obj.MyIntProp = value; })
                    };
                }
            }

            public int MyIntProp { get; set; }
        }

        public interface IJsonSerializable
        {
            public JsonClassInfo GetJsonClassInfo();
        }

        public abstract class JsonClassInfo
        {
            public abstract JsonPropertyInfo[] GetProperties();
        }

        public abstract class JsonPropertyInfo
        {
        }

        public class JsonPropertyInfo<T> : JsonPropertyInfo
        {
            private Action<T> _setter;
            private Func<T> _getter;

            public JsonPropertyInfo(Func<T> getter, Action<T> setter)
            {
                _getter = getter;
                _setter = setter;
            }

            public virtual void Set(T value)
            {
                _setter(value);
            }

            public virtual T Get()
            {
                return _getter();
            }
        }
    }
}
