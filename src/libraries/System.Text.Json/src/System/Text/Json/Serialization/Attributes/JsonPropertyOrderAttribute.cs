// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Specifies the property order that is present in the JSON when serializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class JsonPropertyOrderAttribute : JsonAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonPropertyNameAttribute"/> with the specified order.
        /// </summary>
        /// <param name="order">The order of the property.</param>
        public JsonPropertyOrderAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// The order of the property.
        /// </summary>
        public int Order { get; }
    }
}
