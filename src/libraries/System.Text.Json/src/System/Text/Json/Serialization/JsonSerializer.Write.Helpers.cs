// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    public static partial class JsonSerializer
    {
        private static void WriteCore<TValue>(Utf8JsonWriter writer, TValue value, Type inputType, JsonSerializerOptions options)
        {
            Debug.Assert(writer != null);

            //  We treat typeof(object) special and allow polymorphic behavior.
            if (inputType == typeof(object) && value != null)
            {
                inputType = value!.GetType();
            }

            WriteStack state = default;
            state.InitializeFromRootApi(inputType, options, supportContinuation: false);

            JsonConverter converterBase = state.Current.JsonClassInfo!.PolicyProperty.ConverterBase;
            if (converterBase is JsonConverter<TValue> converter)
            {
                // Call the strongly-typed ReadCore that will not box structs.
                bool success = converter.WriteCore(writer, value, options, ref state);
                Debug.Assert(success);
            }
            else
            {
                bool success = converterBase.WriteCoreAsObject(writer, value, options, ref state);
                Debug.Assert(success);
            }

            writer.Flush();
        }
    }
}
