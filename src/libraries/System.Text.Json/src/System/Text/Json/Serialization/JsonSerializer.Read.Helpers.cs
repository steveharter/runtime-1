// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    public static partial class JsonSerializer
    {
        private static TValue ReadCore<TValue>(ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions options)
        {
            ReadStack state = default;
            state.InitializeFromRootApi(returnType, options, supportContinuation: false);

            JsonConverter converterBase = state.Current.JsonPropertyInfo!.ConverterBase!;
            if (converterBase is JsonConverter<TValue> converter)
            {
                // Call the strongly-typed ReadCore that will not box structs.
                return converter.ReadCore(ref reader, options, ref state);
            }
            else
            {
                object? value = converterBase.ReadCoreAsObject(ref reader, options, ref state);
                Debug.Assert(value == null || value is TValue);
                return (TValue)value!;
            }
        }
    }
}
