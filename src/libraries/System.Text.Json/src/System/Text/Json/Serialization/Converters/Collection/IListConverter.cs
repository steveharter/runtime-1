// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    /// Converter for <cref>System.Collections.IList</cref>.
    internal sealed class IListConverter
        : IEnumerableDefaultConverter<IList, object?, object?>
    {
        public IListConverter(Type typeToConvert) : base(typeToConvert, typeof(object)) { }

        protected override void Add(object? value, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is IList);
            ((IList)state.Current.ReturnValue).Add(value);
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

                state.Current.ReturnValue = new List<object?>();
            }
            else
            {
                if (classInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_DeserializeNoDeserializationConstructor(TypeToConvert);
                }

                IList returnValue = (IList)classInfo.CreateObject()!;

                if (returnValue.IsReadOnly)
                {
                    ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(TypeToConvert);
                }

                state.Current.ReturnValue = returnValue;
            }
        }

        protected override bool OnWriteResume(Utf8JsonWriter writer, object objValue, JsonSerializerOptions options, ref WriteStack state)
        {
            var value = (IList)objValue;

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

                object? element = enumerator.Current;

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
                    return typeof(List<object?>);
                }

                return TypeToConvert;
            }
        }
    }
}
