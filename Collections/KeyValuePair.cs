using System;
using UnityEngine;

namespace Collections
{
    [Serializable]
    public struct KeyValuePair<TKey, TValue>
    {
        [field: SerializeField] public TKey Key { get; set; }
        [field: SerializeField] public TValue Value { get; set; }

        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;
            value = Value;
        }

        public static implicit operator System.Collections.Generic.KeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> kvp) =>
            new System.Collections.Generic.KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);

        public static implicit operator KeyValuePair<TKey, TValue>(System.Collections.Generic.KeyValuePair<TKey, TValue> kvp) =>
            new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
    }
}