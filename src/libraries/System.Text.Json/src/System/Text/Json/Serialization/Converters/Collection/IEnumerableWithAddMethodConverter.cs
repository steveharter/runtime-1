// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Diagnostics;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class IEnumerableWithAddMethodConverter
        : IEnumerableDefaultConverter<IEnumerable, object?, object?>
    {
        public IEnumerableWithAddMethodConverter(Type typeToConvert) : base(typeToConvert, typeof(object)) { }

        protected override void Add(object? value, ref ReadStack state)
        {
            Debug.Assert(state.Current.ReturnValue is IEnumerable);
            Debug.Assert(state.Current.AddMethodDelegate != null);
            ((Action<IEnumerable, object?>)state.Current.AddMethodDelegate)((IEnumerable)state.Current.ReturnValue!, value);
        }

        protected override void CreateCollection(ref Utf8JsonReader reader, ref ReadStack state, JsonSerializerOptions options)
        {
            JsonClassInfo.ConstructorDelegate? constructorDelegate = state.Current.JsonClassInfo.CreateObject;

            if (constructorDelegate == null)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
            }

            state.Current.ReturnValue = constructorDelegate();
            state.Current.AddMethodDelegate = GetAddMethodDelegate(options);
        }

        protected override bool OnWriteResume(Utf8JsonWriter writer, object objValue, JsonSerializerOptions options, ref WriteStack state)
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

        private Action<IEnumerable, object?>? _addMethodDelegate;

        internal Action<IEnumerable, object?> GetAddMethodDelegate(JsonSerializerOptions options)
        {
            if (_addMethodDelegate == null)
            {
                // We verified this exists when we created the converter in the enumerable converter factory.
                _addMethodDelegate = options.MemberAccessorStrategy.CreateAddMethodDelegate<IEnumerable>();
            }

            return _addMethodDelegate;
        }
    }
}
