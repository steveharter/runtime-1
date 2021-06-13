// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// todo
    /// </summary>
    public class JsonNotificationHandler
    {
        /// <summary>
        /// todo
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeInfo"></param>
        protected internal virtual void OnSerializing<T>(in T obj, JsonTypeInfo<T> typeInfo)
        {
            typeInfo.OnSerializing?.Invoke(obj, typeInfo);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeInfo"></param>
        protected internal virtual void OnSerialized<T>(in T obj, JsonTypeInfo<T> typeInfo)
        {
            typeInfo.OnSerialized?.Invoke(obj, typeInfo);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeInfo"></param>
        protected internal virtual void OnDeserializing<T>(in T obj, JsonTypeInfo<T> typeInfo)
        {
            typeInfo.OnDeserializing?.Invoke(obj, typeInfo);
        }

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeInfo"></param>
        protected internal virtual void OnDeserialized<T>(in T obj, JsonTypeInfo<T> typeInfo)
        {
            typeInfo.OnDeserialized?.Invoke(obj, typeInfo);
        }
    }
}
