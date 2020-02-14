// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Text.Json.Serialization
{
    public partial class JsonConverter<T>
    {
        internal sealed override bool WriteCoreAsObject(
            Utf8JsonWriter writer,
            object? value,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            T actualValue = (T)value!;
            return WriteCore(writer, actualValue, options, ref state);
        }

        internal bool WriteCore(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            try
            {
                JsonClassInfo jsonClassInfo = state.Current.JsonClassInfo;
                if (!jsonClassInfo.IsInitialized)
                {
                    // Initialization was deferred until we are in this common exception handler.
                    jsonClassInfo.Initialize(ref state);
                }

                return TryWrite(writer, value, options, ref state);
            }
            catch (InvalidOperationException ex) when (ex.Source == ThrowHelper.ExceptionSourceValueToRethrowAsJsonException)
            {
                ThrowHelper.ReThrowWithPath(state, ex);
                throw;
            }
            catch (JsonException ex)
            {
                ThrowHelper.AddJsonExceptionInformation(state, ex);
                throw;
            }
            catch (NotSupportedException ex)
            {
                NotSupportedException newEx = ThrowHelper.GetNotSupportedException(state, ex);
                if (newEx == ex)
                {
                    // Exception was not modified; just re-throw.
                    throw;
                }

                // Throw a new NotSupportedException with Path information.
                throw newEx;
            }
        }
    }
}
