using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Collections
{
    [Serializable]
    public class HashSet<T> : ISet<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private T[] _initialValues;

        #region Public Properties

        [field: SerializeField, HideInInspector] public int Count { get; private set; }

        public bool Empty => Count == 0;

        public bool IsReadOnly => false;

        #endregion

        #region Private Fields

        private const int ResizeFactor = 2;

        [Serializable]
        private struct Bucket
        {
            [field: SerializeField] internal T Value { get; set; }
            [field: SerializeField] internal int Hash { get; set; }
            [field: SerializeField] internal bool Inserted { get; set; }
        }
        [SerializeField, HideInInspector] private Bucket[] _buckets;


        #endregion

        #region Constructors

        public HashSet(int capacity)
        {
            Count = 0;
            if (capacity < 0)
                throw new ArgumentException("Capacity cannot be lower than zero");
            _buckets = new Bucket[capacity];
        }

        public HashSet() : this(0) { }

        public HashSet(IEnumerable<T> collection) : this()
        {
            foreach (var value in collection)
                Add(value);
        }

        public HashSet(IEnumerable<T> collection, int capacity) : this(capacity)
        {
            foreach (var value in collection)
                Add(value);
        }

        #endregion

        #region Public Methods

        void ICollection<T>.Add(T value)
        {
            if (value == null)
                return;
            Add(value);
        }

        public bool Add(T value)
        {
            var hash = value?.GetHashCode() ?? 0 & int.MaxValue;
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
                        return false;
                    targetBucket = ++targetBucket % _buckets.Length;
                }
                else
                {
                    _buckets[targetBucket] = new Bucket { Value = value, Hash = hash, Inserted = true };
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

        public void Clear()
        {
            Count = 0;
            _buckets = new Bucket[1];
        }

        public bool Remove(T key)
        {
            if (Count == 0)
                return false;
            var hash = key?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted && _buckets[targetBucket].Hash == hash)
                {
                    _buckets[targetBucket].Inserted = false;
                    _buckets[targetBucket].Hash = default;
                    _buckets[targetBucket].Value = default;
                    --Count;
                    return true;
                }
                targetBucket = ++targetBucket % _buckets.Length;
            }
            return false;
        }

        public void RemoveWhere(Predicate<T> predicate)
        {
            if (Count == 0)
                return;
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[i].Inserted && predicate.Invoke(_buckets[i].Value))
                {
                    _buckets[i].Inserted = false;
                    _buckets[i].Hash = default;
                    _buckets[i].Value = default;
                    --Count;
                }
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException("The destination array is not long enough to contain all the elements of this collection.");
            var i = arrayIndex;
            foreach (var element in this)
            {
                array[i++] = element;
            }
        }

        public bool TryGetValue(T expectedValue, out T actualValue)
        {
            if (Count == 0)
            {
                actualValue = default;
                return false;
            }

            var hash = expectedValue?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted && _buckets[targetBucket].Hash.Equals(hash))
                {
                    actualValue = _buckets[targetBucket].Value;
                    return true;
                }

                targetBucket = ++targetBucket % _buckets.Length;
            }

            actualValue = default;
            return false;
        }

        public bool Contains(T value)
        {
            if (Count == 0)
                return false;

            var hash = value?.GetHashCode() ?? 0 & int.MaxValue;
            var targetBucket = hash * (hash < 0 ? -1 : 1) % _buckets.Length;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (_buckets[targetBucket].Inserted && _buckets[targetBucket].Hash.Equals(hash))
                    return true;
                if (!_buckets[targetBucket].Inserted)
                    return false;
                targetBucket = ++targetBucket % _buckets.Length;
            }

            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            var count = Count;
            foreach (var otherValue in other)
            {
                if (Contains(otherValue))
                    --count;
                else
                    return false;
            }

            return count == 0;
        }

        #region Set Operations

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var element in other)
            {
                Add(element);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (ReferenceEquals(other, this) || SetEquals(other))
            {
                Clear();
                return;
            }
            foreach (var element in other)
            {
                if (!Remove(element))
                    Add(element);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (Count == 0) return;
            var mask = new BitArray(_buckets.Length);
            foreach (var element in other)
            {
                var hash = element.GetHashCode();
                var idx = -1;
                for (var i = 0; i < _buckets.Length; ++i)
                {
                    if (_buckets[i].Inserted && _buckets[i].Hash == hash)
                    {
                        idx = i;
                        break;
                    }
                }

                mask[idx] = idx > -1;
            }
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (!mask[i])
                {
                    _buckets[i].Inserted = false;
                    _buckets[i].Value = default;
                    _buckets[i].Hash = 0;
                    --Count;
                }
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (Count == 0) return;
            if (ReferenceEquals(other, this) || SetEquals(other))
            {
                Clear();
                return;
            }
            foreach (var element in other)
            {
                Remove(element);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (Count == 0) return false;
            foreach (var element in other)
            {
                if (Contains(element))
                    return true;
            }

            return false;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            var otherCount = 0;
            foreach (var element in other)
            {
                ++otherCount;
                if (!Contains(element))
                    return false;
            }

            return otherCount != Count;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            foreach (var element in other)
            {
                if (!Contains(element))
                    return false;
            }

            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            var otherCount = 0;
            var mask = new BitArray(_buckets.Length);
            foreach (var element in other)
            {
                ++otherCount;
                var hash = element.GetHashCode();
                var idx = -1;
                for (var i = 0; i < _buckets.Length; ++i)
                {
                    if (_buckets[i].Inserted && _buckets[i].Hash == hash)
                    {
                        idx = i;
                        break;
                    }
                }

                mask[idx] = idx > -1;
            }
            if (otherCount == Count)
                return false;

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (!mask[i])
                    return false;
            }
            return otherCount != Count;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            var mask = new BitArray(_buckets.Length);
            foreach (var element in other)
            {
                var hash = element.GetHashCode();
                var idx = -1;
                for (var i = 0; i < _buckets.Length; ++i)
                {
                    if (_buckets[i].Inserted && _buckets[i].Hash == hash)
                    {
                        idx = i;
                        break;
                    }
                }

                mask[idx] = idx > -1;
            }

            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (!mask[i])
                    return false;
            }
            return true;
        }

        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _buckets.Length; ++i)
            {
                if (!_buckets[i].Inserted) continue;
                yield return _buckets[i].Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion

        #region Private Methods

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
                    Add(oldBuckets[i].Value);
        }

        private void TryTrim()
        {
            if (_buckets.Length / ResizeFactor >= Count) return;
            Count = 0;

            var oldBuckets = (Bucket[])_buckets.Clone();
            _buckets = new Bucket[_buckets.Length + 1 / ResizeFactor];
            for (var i = 0; i < oldBuckets.Length; ++i)
                if (oldBuckets[i].Inserted)
                    Add(oldBuckets[i].Value);
        }

        #endregion

        #region Unity Serialization

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR

            bool playMode;
            try
            {
                playMode = EditorApplication.isPlaying;
            }
            catch (UnityException)
            {
                playMode = false;
            }

            if (!playMode)
            {
                _buckets = Array.Empty<Bucket>();
                Count = 0;
            }

#endif
            if (_buckets.Length == 0)
            {
                foreach (var value in _initialValues)
                    if (!Add(value))
                        Debug.LogError("Duplicate values in the set. The duplicates will be ignored");
            }
        }

        #endregion
    }
}
