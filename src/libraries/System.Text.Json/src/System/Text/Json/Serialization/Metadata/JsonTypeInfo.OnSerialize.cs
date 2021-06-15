// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Text.Json.Serialization.Metadata
{
    public partial class JsonTypeInfo
    {
        private Action<object>? _onSerializing;
        /// <summary>
        /// todo
        /// </summary>
        public Action<object>? OnSerializing
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

        private Action<object>? _onSerialized;
        /// <summary>
        /// todo
        /// </summary>
        public Action<object>? OnSerialized
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

        private Action<object>? _onDeserializing;
        /// <summary>
        /// todo
        /// </summary>
        public Action<object>? OnDeserializing
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

        private Action<object>? _onDeserialized;
        /// <summary>
        /// todo
        /// </summary>
        public Action<object>? OnDeserialized
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
            OnSerializing = GetOnSerializeAttribute<JsonOnSerializingAttribute>();
            OnSerialized = GetOnSerializeAttribute<JsonOnSerializedAttribute>();
            OnDeserializing = GetOnSerializeAttribute<JsonOnDeserializingAttribute>();
            OnDeserialized = GetOnSerializeAttribute<JsonOnDeserializedAttribute>();
        }

        private Action<object>? GetOnSerializeAttribute<TAttribute>() where TAttribute : Attribute
        {
            MethodInfo[] methods = Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            MethodInfo? current = null;

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo mi = methods[i];
                if (!mi.IsDefined(typeof(TAttribute), false))
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
                return (Action<object>)current.CreateDelegate(typeof(Action<object>));
            }

            return null;
        }
    }
}
