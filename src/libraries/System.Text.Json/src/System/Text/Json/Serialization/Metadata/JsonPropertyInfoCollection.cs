// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization.Metadata
{
    /// <summary>
    ///  todo
    /// </summary>
    public sealed class JsonPropertyInfoCollection
    {
        private JsonPropertyDictionary<JsonPropertyInfo> _collection;
        private IListWrapper? _listWrapper;
        private IDictionaryWrapper? _dictionaryWrapper;

        internal JsonPropertyInfoCollection(JsonPropertyDictionary<JsonPropertyInfo> collection)
        {
            _collection = collection;
        }

        /// <summary>
        /// todo
        /// </summary>
        public IList<JsonPropertyInfo> List
        {
            get
            {
                _listWrapper ??= new IListWrapper(_collection);
                return _listWrapper;
            }
        }


        /// <summary>
        /// todo
        /// </summary>
        public IDictionary<string, JsonPropertyInfo> Dictionary
        {
            get
            {
                _dictionaryWrapper ??= new IDictionaryWrapper(_collection);
                return _dictionaryWrapper;
            }
        }

        private sealed class IListWrapper : IList<JsonPropertyInfo>
        {
            private JsonPropertyDictionary<JsonPropertyInfo> _collection;

            public IListWrapper(JsonPropertyDictionary<JsonPropertyInfo> collection)
            {
                _collection = collection;
            }

            public JsonPropertyInfo this[int index]
            {
                get => _collection.RawList[index].Value!;
                set
                {
                    JsonPropertyInfo info = _collection.RawList[index].Value!;
                    _collection.RemoveAt(index);
                    _collection.Add(value.NameAsString, value, index);
                }
            }

            public int Count => _collection.Count;

            public bool IsReadOnly => _collection.IsReadOnly;

            public void Add(JsonPropertyInfo item) => _collection.Add(item.NameAsString, item);
            public void Clear() => _collection.Clear();
            public bool Contains(JsonPropertyInfo item) => _collection.Contains(new KeyValuePair<string, JsonPropertyInfo>(item.NameAsString, item)!);
            public void CopyTo(JsonPropertyInfo[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);
            public IEnumerator<JsonPropertyInfo> GetEnumerator()
            {
                foreach (KeyValuePair<string, JsonPropertyInfo?> item in _collection.RawList)
                {
                    yield return item.Value!;
                }
            }
            public int IndexOf(JsonPropertyInfo item) => _collection.IndexOf(item);
            public void Insert(int index, JsonPropertyInfo item) => _collection.Add(item.NameAsString, item, index);
            public bool Remove(JsonPropertyInfo item) => _collection.Remove(item.NameAsString);
            public void RemoveAt(int index) => _collection.RemoveAt(index);
            IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
        }

        private sealed class IDictionaryWrapper : IDictionary<string, JsonPropertyInfo>
        {
            private JsonPropertyDictionary<JsonPropertyInfo> _collection;

            public IDictionaryWrapper(JsonPropertyDictionary<JsonPropertyInfo> collection)
            {
                _collection = collection;
            }

            public JsonPropertyInfo this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ICollection<string> Keys => _collection.Keys;

            public ICollection<JsonPropertyInfo> Values => _collection.Values!;

            public int Count => _collection.Count;

            public bool IsReadOnly => _collection.IsReadOnly;

            public void Add(string key, JsonPropertyInfo value) => _collection.Add(key, value);
            public void Add(KeyValuePair<string, JsonPropertyInfo> item) => _collection.Add(item.Key, item.Value);
            public void Clear() => _collection.Clear();
            public bool Contains(KeyValuePair<string, JsonPropertyInfo> item) => _collection.Contains(item!);
            public bool ContainsKey(string key) => _collection.ContainsKey(key);
            public void CopyTo(KeyValuePair<string, JsonPropertyInfo>[] array, int arrayIndex) => _collection.CopyTo(array!, arrayIndex);
            public IEnumerator<KeyValuePair<string, JsonPropertyInfo>> GetEnumerator() => _collection.GetEnumerator()!;
            public bool Remove(string key) => _collection.Remove(key);
            public bool Remove(KeyValuePair<string, JsonPropertyInfo> item) => _collection.Remove(item.Key);
            public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonPropertyInfo value) => _collection.TryGetValue(key, out value);
            IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
        }
    }
}
