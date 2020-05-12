// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class ImmutableEnumerableOfTConverter<TElement, TConverterGenericParameter>
        : IEnumerableDefaultConverter<IEnumerable<TElement>, TElement, TConverterGenericParameter>
        where TElement : TConverterGenericParameter
    {
        public ImmutableEnumerableOfTConverter(Type typeToConvert, Type elementType) : base(typeToConvert, elementType) { }

        protected override void Add(TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        internal override bool CanHaveIdMetadata => false;

        protected override void CreateCollection(ref Utf8JsonReader reader, ref ReadStack state, JsonSerializerOptions options)
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        protected override void ConvertCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            JsonClassInfo classInfo = state.Current.JsonClassInfo;

            Func<IEnumerable<TElement>, TCollection>? creator = (Func<IEnumerable<TElement>, TCollection>?)classInfo.CreateObjectWithArgs;
            if (creator == null)
            {
                creator = options.MemberAccessorStrategy.CreateImmutableEnumerableCreateRangeDelegate<TElement, TCollection>();
                classInfo.CreateObjectWithArgs = creator;
            }

            state.Current.ReturnValue = creator((List<TElement>)state.Current.ReturnValue!);
        }

        protected override bool OnWriteResume(Utf8JsonWriter writer, object objValue, JsonSerializerOptions options, ref WriteStack state)
        {
            var value = (IEnumerable<TElement>)objValue;

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
