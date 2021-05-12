// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace System.Text.Json.Serialization.Metadata
{
    internal sealed class JsonValueInfo<T> : JsonTypeInfo<T>
    {
        public JsonValueInfo(JsonSerializerOptions options) : base(ConverterStrategy.Value, options)
        {
        }
    }
}
