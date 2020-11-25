// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    public partial struct JsonElement
    {
        /// <summary>
        ///   An enumerable and enumerator for the properties of a JSON object.
        /// </summary>
        [DebuggerDisplay("{Current,nq}")]
        public struct ObjectEnumerator : IEnumerable<JsonProperty>, IEnumerator<JsonProperty>
        {
            private readonly JsonElement _target;
            private int _curIdx;
            private readonly int _endIdxOrVersion;
            private IEnumerator<KeyValuePair<string, JsonNode?>>? _current;

            internal ObjectEnumerator(JsonElement target)
            {
                _target = target;
                _curIdx = -1;
                _current = null;

                Debug.Assert(target._parent != null);

                if (target._parent is JsonDocument document)
                {
                    Debug.Assert(target.TokenType == JsonTokenType.StartObject);
                    _endIdxOrVersion = document.GetEndIndex(_target._idx, includeEndElement: false);
                }
                else
                {
                    JsonObject jsonObject = (JsonObject)target._parent;
                    _current = jsonObject.Dictionary.GetEnumerator();
                    _endIdxOrVersion = 0; // Not used.
                }
            }

            /// <inheritdoc />
            public JsonProperty Current
            {
                get
                {
                    if (_target._parent is JsonNode)
                    {
                        if (_current == null)
                        {
                            return default;
                        }

                        JsonNode? node = _current.Current.Value;
                        JsonElement element = node == null ? s_nullLiteral : node.AsJsonElement();

                        return new JsonProperty(element, _current.Current.Key);
                    }

                    if (_curIdx < 0)
                    {
                        return default;
                    }

                    var document = (JsonDocument)_target._parent;
                    return new JsonProperty(new JsonElement(document, _curIdx));
                }
            }

            /// <summary>
            ///   Returns an enumerator that iterates the properties of an object.
            /// </summary>
            /// <returns>
            ///   An <see cref="ObjectEnumerator"/> value that can be used to iterate
            ///   through the object.
            /// </returns>
            /// <remarks>
            ///   The enumerator will enumerate the properties in the order they are
            ///   declared, and when an object has multiple definitions of a single
            ///   property they will all individually be returned (each in the order
            ///   they appear in the content).
            /// </remarks>
            public ObjectEnumerator GetEnumerator()
            {
                if (_target._parent is JsonObject jsonObject)
                {
                    _current = jsonObject.Dictionary.GetEnumerator();
                }

                ObjectEnumerator ator = this;
                ator._curIdx = -1;
                return ator;
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            IEnumerator<JsonProperty> IEnumerable<JsonProperty>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public void Dispose()
            {
                _curIdx = _endIdxOrVersion;
                _current = null;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _curIdx = -1;
                _current = null;
            }

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_target._parent is JsonObject)
                {
                    if (_current == null)
                    {
                        return false;
                    }

                    return _current.MoveNext();
                }

                if (_curIdx >= _endIdxOrVersion)
                {
                    return false;
                }

                if (_curIdx < 0)
                {
                    _curIdx = _target._idx + JsonDocument.DbRow.Size;
                }
                else
                {
                    Debug.Assert(_target._parent != null);
                    var document = (JsonDocument)_target._parent;
                    _curIdx = document.GetEndIndex(_curIdx, includeEndElement: true);
                }

                // _curIdx is now pointing at a property name, move one more to get the value
                _curIdx += JsonDocument.DbRow.Size;

                return _curIdx < _endIdxOrVersion;
            }
        }
    }
}
