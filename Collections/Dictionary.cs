using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Violet.Utilities.Collections
{
    [Serializable]
    public class Dictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, ISerializationCallbackReceiver
    {
        [SerializeField] private KeyValuePair<TKey, TValue>[] _initialValues;

        #region Public Properties

        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var value))
                    throw new KeyNotFoundException($"The key {key} is not present in the dictionary.");
                return value;
            }
            set => Insert(key, value);
        }

        [field: SerializeField, HideInInspector] public int Count { get; private set; }

        public bool Empty => Count == 0;

        public TKey[] Keys
        {
            get
            {
                var keys = new TKey[Count];
                var currentIndex = 0;
                for (var i = 0; i < _buckets.Length; ++i)
                {
                    if (_buckets[i].Inserted)
                        keys[currentIndex++] = _buckets[i].Key;
                }

                return keys;
            }
        }

        public TValue[] Values
        {
            get
            {
                var values = new TValue[Count];
                var currentIndex = 0;
                for (var i = 0; i < _buckets.Length; ++i)
                {
                    if (_buckets[i].Inserted)
                        values[currentIndex++] = _buckets[i].Value;
                }

                return values;
            }
        }

        public bool IsReadOnly => false;

        #endregion

        #region Private Fields

        private const int ResizeFactor = 2;

        [Serializable] private struct Bucket
        {
            [field: SerializeField] internal TKey Key { get; set; }
            [field: SerializeField] internal TValue Value { get; set; }
            [field: SerializeField] internal int Hash { get; set; }
            [field: SerializeField] internal bool Inserted { get; set; }
        }
        [SerializeField, HideInInspector] private Bucket[] _buckets;

        #endregion

        #region Constructors

        public Dictionary(int capacity)
        {
            Count = 0;
            _buckets = new Bucket[capacity];
#if UNITY_EDITOR
            try
            {
                _isEditorInPlayMode = EditorApplication.isPlaying;
            }
            catch (UnityException)
            {
                _isEditorInPlayMode = false;
            }
            EditorApplication.playModeStateChanged += change =>
            {
                _isEditorInPlayMode = change == PlayModeStateChange.EnteredPlayMode;
            };
#endif
        }

        public Dictionary() : this(0) { }

        public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this()
        {
            foreach (var (key, value) in collection)
                TryAdd(key, value);
        }

        public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, int capacity) : this(capacity)
        {
            foreach (var (key, value) in collection)
                TryAdd(key, value);
        }

        #endregion

        #region Public Methods

        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        public void Add(TKey key, TValue value)
        {
            if (!Insert(key, value, true)) 
                throw new DuplicateKeyException($"The dictionary already has the key {key}.");
        }

        public bool TryAdd(TKey key, TValue value) => Insert(key, value, true);

        public void Clear()
        {
            Count = 0;
            _buckets = new Bucket[1];
        }

        public bool Remove(TKey key)
        {
            if (Count == 0)
                return false;
            var hash = key?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted)
                {
                    if (_buckets[targetBucket].Hash == hash)
                    {
                        _buckets[targetBucket].Inserted = false;
                        _buckets[targetBucket].Hash = default;
                        _buckets[targetBucket].Key = default;
                        _buckets[targetBucket].Value = default;
                        --Count;
                        TryTrim();
                        return true;
                    }

                    targetBucket = ++targetBucket % _buckets.Length;
                } else return false;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp)
        {
            if (Count == 0)
                return false;
            var hash = kvp.Key?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted)
                {
                    if (_buckets[targetBucket].Hash == hash)
                    {
                        if (!_buckets[targetBucket].Value.Equals(kvp.Value))
                            return false;
                        _buckets[targetBucket].Inserted = false;
                        _buckets[targetBucket].Hash = default;
                        _buckets[targetBucket].Key = default;
                        _buckets[targetBucket].Value = default;
                        --Count;
                        TryTrim();
                        return true;
                    }

                    targetBucket = ++targetBucket % _buckets.Length;
                } else return false;
            }
            return false;
        }

        public void RemoveWhere(Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            if (Count == 0)
                return;
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[i].Inserted && predicate.Invoke(new KeyValuePair<TKey, TValue>(_buckets[i].Key, _buckets[i].Value)))
                {
                    _buckets[i].Inserted = false;
                    _buckets[i].Hash = default;
                    _buckets[i].Key = default;
                    _buckets[i].Value = default;
                    --Count;
                }
            } 
            TryTrim();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException("The destination array is not long enough to contain all the elements of this collection.");
            var i = arrayIndex;
            foreach (var kvp in this)
            {
                array[i++] = kvp;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            var hash = key?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted)
                {
                    if (_buckets[targetBucket].Hash == hash)
                    {
                        value = _buckets[targetBucket].Value;
                        return true;
                    }
                    targetBucket = ++targetBucket % _buckets.Length;
                }
                else
                {
                    value = default;
                    return false;
                }
            }

            value = default;
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            if (Count == 0)
                return false;
            var hash = key?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted && _buckets[targetBucket].Hash == hash)
                    return true;
                if (!_buckets[targetBucket].Inserted)
                    return false;

                targetBucket = ++targetBucket % _buckets.Length;
            }

            return false;
        }

        public bool ContainsValue(TValue value)
        {
            if (Count == 0)
                return false;
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[i].Inserted && (_buckets[i].Value?.Equals(value) ?? _buckets[i].Value == null && value == null))
                    return true;
            }

            return false;
        }

        public bool Contains(KeyValuePair<TKey, TValue> kvp)
        {
            return TryGetValue(kvp.Key, out var value) && (value?.Equals(kvp.Value) ?? value == null && kvp.Value == null);
        }

        public bool DictionaryEquals(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            var count = Count;
            foreach (var (otherKey, otherValue) in other)
            {
                if (Contains(new KeyValuePair<TKey, TValue>(otherKey, otherValue)))
                    --count;
                else
                    return false;
            }

            return count == 0;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (!_buckets[i].Inserted) continue;
                yield return new KeyValuePair<TKey, TValue>(_buckets[i].Key, _buckets[i].Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        private bool Insert(TKey key, TValue value, bool add = false)
        {
            var hash = key?.GetHashCode() ?? 0 & int.MaxValue;
            if (Count + 1 >= _buckets.Length)
            {
                Count = 0;
                Resize();
            }

            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted)
                {
                    if (_buckets[targetBucket].Hash == hash)
                    {
                        if (add) return false;
                        _buckets[targetBucket].Value = value;
                        return true;
                    }
                    targetBucket = ++targetBucket % _buckets.Length;
                }
                else
                {
                    _buckets[targetBucket] = new Bucket { Key = key, Value = value, Hash = hash, Inserted = true };
                    break;
                }
            }

            if (++Count >= _buckets.Length)
            {
                Count = 0;
                Resize();
            }
            return true;
        }

        private void Resize()
        {
            if (_buckets.Length == 0)
            {
                _buckets = new Bucket[1];
                return;
            }

            var oldBuckets = (Bucket[])_buckets.Clone();
            _buckets = new Bucket[_buckets.Length * ResizeFactor];
            for (var i = 0; i < oldBuckets.Length; ++i)
                if (oldBuckets[i].Inserted)
                    Insert(oldBuckets[i].Key, oldBuckets[i].Value, true);
        }

        private void TryTrim()
        {
            if (_buckets.Length + 1 / ResizeFactor >= Count) return;

            var oldBuckets = (Bucket[])_buckets.Clone();
            _buckets = new Bucket[_buckets.Length + 1 / ResizeFactor];
            for (var i = 0; i < oldBuckets.Length; ++i)
                if (oldBuckets[i].Inserted)
                    Insert(oldBuckets[i].Key, oldBuckets[i].Value, true);
        }

        #endregion

        #region Unity Serialization

#if UNITY_EDITOR
        private bool _isEditorInPlayMode;
#endif

        public void OnBeforeSerialize() { } 

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR

            if (!_isEditorInPlayMode)
            {
                _buckets = Array.Empty<Bucket>();
                Count = 0;
            }

#endif
            if (_buckets.Length == 0)
            {
                foreach (var (key, value) in _initialValues)
                    TryAdd(key, value);
            }
        }

        #endregion
    }

    #region Exceptions

    public class DuplicateKeyException : ArgumentException
    {
        public DuplicateKeyException(string message) : base(message) { }
    }

    public class KeyNotFoundException : ArgumentException
    {
        public KeyNotFoundException (string message) : base(message) { }
    }

    #endregion
}