﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class ImmutableDictionaryOfStringTValueConverter<TValue, TConverterGenericParameter>
        : DictionaryDefaultConverter<IReadOnlyDictionary<string, TValue>, TValue, TConverterGenericParameter>
        where TValue : TConverterGenericParameter
    {
        public ImmutableDictionaryOfStringTValueConverter(Type typeToConvert, Type dictionaryValueType) : base(typeToConvert, dictionaryValueType) { }

        protected override void Add(TValue value, JsonSerializerOptions options, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is Dictionary<string, TValue>);

            string key = state.Current.JsonPropertyNameAsString!;
            ((Dictionary<string, TValue>)state.Current.ReturnValue!)[key] = value;
        }

        internal override bool CanHaveIdMetadata => false;

        protected override void CreateCollection(ref ReadStack state)
        {
            state.Current.ReturnValue = new Dictionary<string, TValue>();
        }

        protected override void ConvertCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            state.Current.ReturnValue = GetCreatorDelegate(options)((Dictionary<string, TValue>)state.Current.ReturnValue!);
        }

        protected internal override bool OnWriteResume(
            Utf8JsonWriter writer,
            object objValue,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            var value = (IReadOnlyDictionary<string, TValue>)objValue;

            IEnumerator<KeyValuePair<string, TValue>> enumerator;
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
                Debug.Assert(state.Current.CollectionEnumerator is Dictionary<string, TValue>.Enumerator);
                enumerator = (Dictionary<string, TValue>.Enumerator)state.Current.CollectionEnumerator;
            }

            JsonConverter<TConverterGenericParameter> converter = GetValueConverter(options);
            do
            {
                if (ShouldFlush(writer, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                string key = GetKeyName(enumerator.Current.Key, ref state, options);
                writer.WritePropertyName(key);

                TValue element = enumerator.Current.Value;
                if (!converter.TryWrite(writer, (TConverterGenericParameter)element!, options, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                state.Current.EndDictionaryElement();
            } while (enumerator.MoveNext());

            return true;
        }

        private Func<IEnumerable, IReadOnlyDictionary>? _creatorDelegate;

        private Func<IEnumerable, IReadOnlyDictionary> GetCreatorDelegate(JsonSerializerOptions options)
        {
            if (_creatorDelegate == null)
            {
                _creatorDelegate = options.MemberAccessorStrategy.CreateImmutableDictionaryCreateRangeDelegate(ElementType, TypeToConvert);
            }

            return _creatorDelegate;
        }
    }
}
