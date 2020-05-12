// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Base class for dictionary converters such as IDictionary, Hashtable, Dictionary{,} IDictionary{,} and SortedList.
    /// </summary>
    internal abstract class JsonDictionaryConverter<TCollection> : JsonResumableConverter<TCollection>
    {
        internal JsonDictionaryConverter(Type typeToConvert) : base(typeToConvert) { }
        internal sealed override ClassType ClassType => ClassType.Dictionary;
        protected internal abstract bool OnWriteResume(Utf8JsonWriter writer, object dictionary, JsonSerializerOptions options, ref WriteStack state);
    }
}
