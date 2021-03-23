// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Text.Json.Node
{
    public partial class JsonObject : IDictionary<string, JsonNode?>
    {
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Add(string propertyName, JsonNode? value)
        {
            if (value != null)
            {
                value.AssignParent(this);
            }

            Dictionary.Add(propertyName, value);
        }

        void ICollection<KeyValuePair<string, JsonNode?>>.Add(KeyValuePair<string, JsonNode?> item)
        {
            JsonNode? value = item.Value;

            if (value != null)
            {
                value.AssignParent(this);
            }

            Dictionary.Add(item);
        }

        /// <summary>
        /// todo
        /// </summary>
        public void Clear()
        {
            foreach (JsonNode? node in Dictionary.Values)
            {
                DetachParent(node);
            }

            Dictionary.Clear();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<KeyValuePair<string, JsonNode?>>.Contains(KeyValuePair<string, JsonNode?> item) => Dictionary.Contains(item);

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool ContainsKey(string propertyName) => Dictionary.ContainsKey(propertyName);

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void ICollection<KeyValuePair<string, JsonNode?>>.CopyTo(KeyValuePair<string, JsonNode?>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, JsonNode?>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool Remove(string propertyName)
        {
            if (!Dictionary.TryGetValue(propertyName, out JsonNode? item))
            {
                return false;
            }

            bool success = Dictionary.Remove(propertyName);
            Debug.Assert(success);
            DetachParent(item);
            return true;
        }

        bool ICollection<KeyValuePair<string, JsonNode?>>.Remove(KeyValuePair<string, JsonNode?> item)
        {
            if (Dictionary.Remove(item))
            {
                JsonNode? node = item.Value;
                DetachParent(node);
                return true;
            }

            return false;
        }

        ICollection<string> IDictionary<string, JsonNode?>.Keys => Dictionary.Keys;
        ICollection<JsonNode?> IDictionary<string, JsonNode?>.Values => Dictionary.Values;

        bool IDictionary<string, JsonNode?>.TryGetValue(string propertyName, out JsonNode? jsonNode) =>
            Dictionary.TryGetValue(propertyName, out jsonNode);

        /// <summary>
        /// todo
        /// </summary>
        public int Count => Dictionary.Count;

        bool ICollection<KeyValuePair<string, JsonNode?>>.IsReadOnly => Dictionary.IsReadOnly;

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

        private void DetachParent(JsonNode? item)
        {
            if (item != null)
            {
                item.Parent = null;
            }

            // Prevent previous child from being returned from these cached variables.
            _lastKey = null;
            _lastValue = null;
        }
    }
}
