// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    /// <summary>
    /// Converter for <cref>System.Collections.Generic.IDictionary{string, TValue}</cref> that
    /// (de)serializes as a JSON object with properties representing the dictionary element key and value.
    /// </summary>
    internal sealed class IDictionaryOfStringTValueConverter<TDictionaryValue, TDictionaryValueGenericParameter>
        : DictionaryDefaultConverter<IDictionary<string, TDictionaryValue>, TDictionaryValue, TDictionaryValueGenericParameter>
        where TDictionaryValue : TDictionaryValueGenericParameter
    {
        public IDictionaryOfStringTValueConverter(Type typeToConvert, Type dictionaryValueType) : base(typeToConvert, dictionaryValueType) { }

        protected override void Add(TDictionaryValue value, JsonSerializerOptions options, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is IDictionary<string, TDictionaryValue>);

            string key = state.Current.JsonPropertyNameAsString!;
            ((IDictionary<string, TDictionaryValue>)state.Current.ReturnValue!)[key] = value;
        }

        protected override void CreateCollection(ref Utf8JsonReader reader, ref ReadStack state)
        {
            JsonClassInfo classInfo = state.Current.JsonClassInfo;

            if (TypeToConvert.IsInterface || TypeToConvert.IsAbstract)
            {
                if (!TypeToConvert.IsAssignableFrom(RuntimeType))
                {
                    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
                }

                state.Current.ReturnValue = new Dictionary<string, TDictionaryValue>();
            }
            else
            {
                if (classInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(TypeToConvert, ref reader, ref state);
                }

                IDictionary<string, TDictionaryValue> returnValue = (IDictionary<string, TDictionaryValue>)classInfo.CreateObject()!;

                if (returnValue.IsReadOnly)
                {
                    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
                }

                state.Current.ReturnValue = returnValue;
            }
        }

        protected internal override bool OnWriteResume(
            Utf8JsonWriter writer,
            object objValue,
            JsonSerializerOptions options,
            ref WriteStack state)
        {
            var value = (IDictionary<string, TDictionaryValue>)objValue;

            IEnumerator<KeyValuePair<string, TDictionaryValue>> enumerator;
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
                Debug.Assert(state.Current.CollectionEnumerator is IEnumerator<KeyValuePair<string, TDictionaryValue>>);
                enumerator = (IEnumerator<KeyValuePair<string, TDictionaryValue>>)state.Current.CollectionEnumerator;
            }

            JsonConverter<TDictionaryValueGenericParameter> converter = GetValueConverter(options);
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

            return true;
        }

        internal override Type RuntimeType
        {
            get
            {
                if (TypeToConvert.IsAbstract || TypeToConvert.IsInterface)
                {
                    return typeof(Dictionary<string, TDictionaryValue>);
                }

                return TypeToConvert;
            }
        }
    }
}
