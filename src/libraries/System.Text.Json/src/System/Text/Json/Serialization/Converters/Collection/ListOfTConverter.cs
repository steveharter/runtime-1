// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace System.Text.Json.Serialization.Converters
{
    /// Converter for <cref>System.Collections.Generic.List{TElement}</cref>.
    internal sealed class ListOfTConverter<TElement, TConverterGenericParameter>
        : IEnumerableDefaultConverter<List<TElement>, TElement, TConverterGenericParameter>
        where TElement : TConverterGenericParameter
    {
        public ListOfTConverter(Type typeToConvert, Type elementType) : base(typeToConvert, elementType) { }

        protected override void Add(TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref Utf8JsonReader reader, ref ReadStack state, JsonSerializerOptions options)
        {
            if (state.Current.JsonClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.JsonClassInfo.Type);
            }

            state.Current.ReturnValue = state.Current.JsonClassInfo.CreateObject();
        }

        protected override bool OnWriteResume(Utf8JsonWriter writer, object objValue, JsonSerializerOptions options, ref WriteStack state)
        {
            var value = (List<TElement>)objValue;

            // Using an index is 2x faster than using an enumerator.
            int index = state.Current.EnumeratorIndex;
            JsonConverter<TConverterGenericParameter> elementConverter = GetElementConverter(options);

            if (elementConverter.CanUseDirectReadOrWrite)
            {
                // Fast path that avoids validation and extra indirection.
                for (; index < value.Count; index++)
                {
                    elementConverter.Write(writer, list[index], options);
                }
            }
            else
            {
                for (; index < value.Count; index++)
                {
                    TElement element = value[index];
                    if (!elementConverter.TryWrite(writer, element, options, ref state))
                    {
                        state.Current.EnumeratorIndex = index;
                        return false;
                    }

                    if (ShouldFlush(writer, ref state))
                    {
                        state.Current.EnumeratorIndex = ++index;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
