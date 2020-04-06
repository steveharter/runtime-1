// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class StackOfTConverter<TElement, TConverterGenericParameter>
        : IEnumerableDefaultConverter<Stack<TElement>, TElement, TConverterGenericParameter>
        where TElement : TConverterGenericParameter
    {
        public StackOfTConverter(Type typeToConvert, Type elementType) : base(typeToConvert, elementType) { }

        protected override void Add(TElement value, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is Stack<TElement>);
            ((Stack<TElement>)state.Current.ReturnValue!).Push(value);
        }

        protected override void CreateCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            if (state.Current.JsonClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.JsonClassInfo.Type);
            }

            state.Current.ReturnValue = state.Current.JsonClassInfo.CreateObject();
        }

        protected override bool OnWriteResume(Utf8JsonWriter writer, object objValue, JsonSerializerOptions options, ref WriteStack state)
        {
            var value = (Stack<TElement>)objValue;

            IEnumerator<TElement> enumerator;
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
                Debug.Assert(state.Current.CollectionEnumerator is IEnumerator<TElement>);
                enumerator = (IEnumerator<TElement>)state.Current.CollectionEnumerator;
            }

            JsonConverter<TConverterGenericParameter> converter = GetElementConverter(options);
            do
            {
                if (ShouldFlush(writer, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                TElement element = enumerator.Current;
                if (!converter.TryWrite(writer, element, options, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }
            } while (enumerator.MoveNext());

            return true;
        }
    }
}
