// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    /// <summary>
    /// Converter for Dictionary{string, TValue} that (de)serializes as a JSON object with properties
    /// representing the dictionary element key and value.
    /// </summary>
    internal sealed class DictionaryOfStringTValueConverter<TDictionaryValue, TDictionaryValueGenericParameter>
        : DictionaryDefaultConverter<Dictionary<string, TDictionaryValue>, TDictionaryValue, TDictionaryValueGenericParameter>
        where TDictionaryValue : TDictionaryValueGenericParameter
    {
        public DictionaryOfStringTValueConverter(Type dictionaryType, Type dictionaryValueType) : base(dictionaryType, dictionaryValueType) { }

        protected override void Add(TDictionaryValue value, JsonSerializerOptions options, ref ReadStack state)
        {
            string key = state.Current.JsonPropertyNameAsString!;
            ((Dictionary<string, TDictionaryValue>)state.Current.ReturnValue!)[key] = value;
        }

        protected override void CreateCollection(ref Utf8JsonReader reader, ref ReadStack state)
        {
            if (state.Current.JsonClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.JsonClassInfo.Type);
            }

            state.Current.ReturnValue = state.Current.JsonClassInfo.CreateObject();
        }

        protected internal override bool OnWriteResume(
            Utf8JsonWriter writer,
            object objValue,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            var value = (Dictionary<string, TDictionaryValue>)objValue;

            Dictionary<string, TDictionaryValue>.Enumerator enumerator;
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
                Debug.Assert(state.Current.CollectionEnumerator is Dictionary<string, TDictionaryValue>.Enumerator);
                enumerator = (Dictionary<string, TDictionaryValue>.Enumerator)state.Current.CollectionEnumerator;
            }

            JsonConverter<TDictionaryValueGenericParameter> converter = GetValueConverter(options);
            if (!state.SupportContinuation && converter.CanUseDirectReadOrWrite)
            {
                // Fast path that avoids validation and extra indirection.
                do
                {
                    string key = GetKeyName(enumerator.Current.Key, ref state, options);
                    writer.WritePropertyName(key);
                    converter.Write(writer, enumerator.Current.Value, options);
                } while (enumerator.MoveNext());
            }
            else
            {
                do
                {
                    if (ShouldFlush(writer, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    if (state.Current.PropertyState < StackFramePropertyState.Name)
                    {
                        state.Current.PropertyState = StackFramePropertyState.Name;
                        string key = GetKeyName(enumerator.Current.Key, ref state, options);
                        writer.WritePropertyName(key);
                    }

                    TDictionaryValue element = enumerator.Current.Value;
                    if (!converter.TryWrite(writer, (TDictionaryValueGenericParameter)element!, options, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    state.Current.EndDictionaryElement();
                } while (enumerator.MoveNext());
            }

            return true;
        }
    }
}
