// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.IEnumerable</cref>.
    /// </summary>
    internal sealed class IEnumerableConverter
        : IEnumerableDefaultConverter<IEnumerable, object, object?>
    {
        public IEnumerableConverter(Type typeToConvert) : base(typeToConvert, typeof(object)) { }

        protected override void Add(object? value, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is List<object?>);
            ((List<object?>)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            if (!TypeToConvert.IsAssignableFrom(RuntimeType))
            {
                ThrowHelper.ThrowNotSupportedException_DeserializeNoDeserializationConstructor(TypeToConvert);
            }

            state.Current.ReturnValue = new List<object?>();
        }

        // Consider overriding ConvertCollection to convert the list to an array since a List is mutable.
        // However, converting from the temporary list to an array will be slower.

        protected override bool OnWriteResume(
            Utf8JsonWriter writer,
            object objValue,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            var value = (IEnumerable)objValue;

            IEnumerator enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = state.Current.CollectionEnumerator;
            }

            JsonConverter<object?> converter = GetElementConverter(options);
            do
            {
                if (ShouldFlush(writer, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                if (!converter.TryWrite(writer, enumerator.Current, options, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }
            } while (enumerator.MoveNext());

            return true;
        }

        internal override Type RuntimeType => typeof(List<object?>);
    }
}
