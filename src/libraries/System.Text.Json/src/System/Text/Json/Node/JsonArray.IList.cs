// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization.Converters;

namespace System.Text.Json.Node
{
    /// <summary>
    /// Supports dynamic arrays.
    /// </summary>
    public sealed partial class JsonArray : JsonNode, IList<JsonNode?>
    {
        /// <summary>
        /// todo
        /// </summary>
        public int Count => List.Count;

        bool ICollection<JsonNode?>.IsReadOnly => ((IList)List).IsReadOnly;

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="item"></param>
        public void Add(JsonNode? item)
        {
            if (item != null)
            {
                item.AssignParent(this);
            }

            List.Add(item);
        }

        /// <summary>
        /// todo
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < List.Count; i++)
            {
                DetachParent(List[i]);
            }

            List.Clear();
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(JsonNode? item) => List.Contains(item);

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void ICollection<JsonNode?>.CopyTo(JsonNode?[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public IEnumerator<JsonNode?> GetEnumerator() => List.GetEnumerator();

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(JsonNode? item) => List.IndexOf(item);

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, JsonNode? item)
        {
            if (item != null)
            {
                item.AssignParent(this);
            }

            List.Insert(index, item);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(JsonNode? item)
        {
            if (List.Remove(item))
            {
                DetachParent(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            JsonNode? item = List[index];
            List.RemoveAt(index);
            DetachParent(item);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)List).GetEnumerator();

        private void DetachParent(JsonNode? item)
        {
            if (item != null)
            {
                item.Parent = null;
            }
        }
    }
}
