// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// todo
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class JsonOnDeserializingAttribute : JsonAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonOnDeserializingAttribute"/>.
        /// </summary>
        public JsonOnDeserializingAttribute() { }
    }
}
