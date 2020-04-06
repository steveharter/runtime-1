// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class ISetOfTConverter<TElement, TConverterGenericParameter>
        : IEnumerableDefaultConverter<ISet<TElement>, TElement, TConverterGenericParameter>
        where TElement : TConverterGenericParameter
    {
        public ISetOfTConverter(Type typeToConvert, Type elementType) : base(typeToConvert, elementType) { }

        protected override void Add(TElement value, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is ISet<TElement>);
            ((ISet<TElement>)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            JsonClassInfo classInfo = state.Current.JsonClassInfo;

            if (TypeToConvert.IsInterface || TypeToConvert.IsAbstract)
            {
                if (!TypeToConvert.IsAssignableFrom(RuntimeType))
                {
                    ThrowHelper.ThrowNotSupportedException_DeserializeNoDeserializationConstructor(TypeToConvert);
                }

                state.Current.ReturnValue = new HashSet<TElement>();
            }
            else
            {
                if (classInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_DeserializeNoDeserializationConstructor(TypeToConvert);
                }

                ISet<TElement> returnValue = (ISet<TElement>)classInfo.CreateObject()!;

                if (returnValue.IsReadOnly)
                {
                    ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(TypeToConvert);
                }

                state.Current.ReturnValue = returnValue;
            }
        }

        protected override bool OnWriteResume(Utf8JsonWriter writer, object objValue, JsonSerializerOptions options, ref WriteStack state)
        {
            var value = (ISet<TElement>)objValue;

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

        internal override Type RuntimeType
        {
            get
            {
                if (TypeToConvert.IsAbstract || TypeToConvert.IsInterface)
                {
                    return typeof(HashSet<TElement>);
                }

                return TypeToConvert;
            }
        }
    }
}
