// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Text.Json.Serialization.Metadata
{
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="o"></param>
    public delegate void SerializeCallback(object o);

    public partial class JsonTypeInfo
    {
        private SerializeCallback? _onSerializing;
        /// <summary>
        /// todo
        /// </summary>
        public SerializeCallback? OnSerializing
        {
            get
            {
                return _onSerializing;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onSerializing = value;
            }
        }

        private SerializeCallback? _onSerialized;
        /// <summary>
        /// todo
        /// </summary>
        public SerializeCallback? OnSerialized
        {
            get
            {
                return _onSerialized;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onSerialized = value;
            }
        }

        private SerializeCallback? _onDeserializing;
        /// <summary>
        /// todo
        /// </summary>
        public SerializeCallback? OnDeserializing
        {
            get
            {
                return _onDeserializing;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onDeserializing = value;
            }
        }

        private SerializeCallback? _onDeserialized;
        /// <summary>
        /// todo
        /// </summary>
        public SerializeCallback? OnDeserialized
        {
            get
            {
                return _onDeserialized;
            }

            set
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("todo");
                }

                _onDeserialized = value;
            }
        }

        internal void GetAllOnSerializeAttributes()
        {
            MethodInfo[] methods = Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            OnSerializing = GetOnSerializeAttribute(typeof(JsonOnSerializingAttribute), methods);
            OnSerialized = GetOnSerializeAttribute(typeof(JsonOnSerializedAttribute), methods);
            OnDeserializing = GetOnSerializeAttribute(typeof(JsonOnDeserializingAttribute), methods);
            OnDeserialized = GetOnSerializeAttribute(typeof(JsonOnDeserializedAttribute), methods);
        }

        private SerializeCallback? GetOnSerializeAttribute(Type attributeType, MethodInfo[] methods)
        {
            MethodInfo? current = null;

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo mi = methods[i];
                if (!mi.IsDefined(attributeType, false))
                {
                    continue;
                }

                if (current != null)
                {
                    throw new InvalidOperationException("todo: only one allowed");
                }

                if (mi.ReturnType != typeof(void))
                {
                    throw new InvalidOperationException("todo");
                }

                if (mi.GetParameters().Length != 0)
                {
                    throw new InvalidOperationException("todo");
                }

                current = mi;
            }

            if (current != null)
            {
                return (obj) => current.Invoke(obj, null); // todo: IL Emit or strongly-typed delegate without extra thunk
            }

            return null;
        }
    }
}
