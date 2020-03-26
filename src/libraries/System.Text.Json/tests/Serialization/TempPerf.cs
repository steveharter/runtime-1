// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Converters;
using Xunit;

namespace Temp
{
    public static class Serializer
    {
        public static JsonSerializerOptions Options { get; }

        public static DateTimeConverter DateTimeConverter { get; }
        public static Int32Converter Int32Converter { get; }
        public static StringConverter StringConverter { get; }

        static Serializer()
        {
            var sw = new Stopwatch();
            sw.Start();

            Options = new JsonSerializerOptions();
            DateTimeConverter = (DateTimeConverter)Options.GetConverter(typeof(DateTime));
            Int32Converter = (Int32Converter)Options.GetConverter(typeof(int));
            StringConverter = (StringConverter)Options.GetConverter(typeof(string));

            sw.Stop();
            Console.WriteLine($"Initialize1: {sw.ElapsedMilliseconds}");
        }
    }

    public class WeatherForecast
    {
        static WeatherForecast()
        {
            var sw = new Stopwatch();
            sw.Start();

            ClassInfo classInfo = new ClassInfo(Serializer.Options);
            classInfo.Initialize();
            Serializer.Options.Classes.TryAdd(typeof(WeatherForecast), classInfo);

            sw.Stop();
            Console.WriteLine($"Initialize2: {sw.ElapsedMilliseconds}");
        }

        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; set; }

        private class ClassInfo : JsonClassInfo
        {
            public ClassInfo(JsonSerializerOptions options) : base(typeof(WeatherForecast), options)
            {
                CreateObject = () => { return new WeatherForecast(); };
            }

            protected override IList<JsonPropertyInfo> GetProperties()
            {
                var properties = new List<JsonPropertyInfo>(4);

                properties.Add(new JsonPropertyInfo<DateTime>
                {
                    Options = Options,
                    Converter = Serializer.DateTimeConverter,
                    NameAsString = nameof(WeatherForecast.Date),
                    HasGetter = true,
                    HasSetter = true,
                    ShouldSerialize = true,
                    ShouldDeserialize = true,
                    Get = (obj) => { return ((WeatherForecast)obj).Date; },
                    Set = (obj, value) => { ((WeatherForecast)obj).Date = value; }
                });

                properties.Add(new JsonPropertyInfo<int>
                {
                    Options = Options,
                    Converter = Serializer.Int32Converter,
                    NameAsString = nameof(WeatherForecast.TemperatureC),
                    HasGetter = true,
                    HasSetter = true,
                    Get = (obj) => { return ((WeatherForecast)obj).TemperatureC; },
                    Set = (obj, value) => { ((WeatherForecast)obj).TemperatureC = value; }
                });

                properties.Add(new JsonPropertyInfo<int>
                {
                    Options = Options,
                    Converter = Serializer.Int32Converter,
                    HasGetter = true,
                    NameAsString = nameof(WeatherForecast.TemperatureF),
                    Get = (obj) => { return ((WeatherForecast)obj).TemperatureF; },
                });

                properties.Add(new JsonPropertyInfo<string>
                {
                    Options = Options,
                    Converter = Serializer.StringConverter,
                    NameAsString = nameof(WeatherForecast.Summary),
                    HasGetter = true,
                    HasSetter = true,
                    Get = (obj) => { return ((WeatherForecast)obj).Summary; },
                    Set = (obj, value) => { ((WeatherForecast)obj).Summary = value; }
                });

                return properties;

                //new JsonPropertyInfo<int>(
                //    Get : (obj) => { return obj.MyIntProp; },
                //    Set : (obj, value) => { obj.MyIntProp = value; })
            }
        }
    }

    public class Perf
    {
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

            //Writer(result); // 8
            Auto(result); //48->8+19+15=42  (6 slower than Custom)
            //CustomConverter(result); // 22 --> 9
        }

        static void Writer(IEnumerable<WeatherForecast> result)
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
                JsonSerializer.SerializeToUtf8Bytes(result, Serializer.Options);
                //string s = JsonSerializer.Serialize(result, Serializer.Options);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        static void CustomConverter(IEnumerable<WeatherForecast> result)
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
    }
}
