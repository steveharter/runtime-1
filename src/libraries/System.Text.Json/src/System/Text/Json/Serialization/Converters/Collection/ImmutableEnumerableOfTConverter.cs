// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class ImmutableEnumerableOfTConverter<TElement, TConverterGenericParameter>
        : IEnumerableDefaultConverter<IEnumerable<TElement>, TElement, TConverterGenericParameter>
        where TElement : TConverterGenericParameter
    {
        public ImmutableEnumerableOfTConverter(Type typeToConvert, Type elementType) : base(typeToConvert, elementType) { }

        protected override void Add(TElement value, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is List<TElement>);
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        internal override bool CanHaveIdMetadata => false;

        protected override void CreateCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        protected override void ConvertCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            state.Current.ReturnValue = GetCreatorDelegate(ElementType, TypeToConvert, options)((IEnumerable)state.Current.ReturnValue!);
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

        private Func<IEnumerable, IEnumerable>? _creatorDelegate;

        private Func<IEnumerable, IEnumerable> GetCreatorDelegate(Type elementType, Type collectionType, JsonSerializerOptions options)
        {
            if (_creatorDelegate == null)
            {
                _creatorDelegate = options.MemberAccessorStrategy.CreateImmutableEnumerableCreateRangeDelegate(elementType, collectionType);
            }

            return _creatorDelegate;
        }
    }
}
