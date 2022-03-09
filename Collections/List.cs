using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Violet.Utilities.Collections
{
    public class List<T> : IList<T>
    {
        [SerializeField] private T[] _initialValues;

        #region Public Properties

        public T this[int index]
        {
            get
            {
                if (index > Count || index < 0)
                    throw new IndexOutOfRangeException();
                return _list[index];
            }
            set
            {
                if (index > Count || index < 0)
                    throw new IndexOutOfRangeException();
                _list[index] = value;
            }
        }

        [field: SerializeField, HideInInspector] public int Count { get; private set; }

        public bool Empty => Count == 0;

        public bool IsReadOnly => false;

        #endregion

        #region Private Fields

        private const int ResizeFactor = 2;

        [SerializeField, HideInInspector] private T[] _list;

        #endregion

        #region Constructors

        public List(int capacity)
        {
            Count = 0;
            _list = new T[capacity];
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

        public List() : this(0) { }

        public List(IEnumerable<T> collection) : this()
        {
            AddRange(collection);
        }

        public List(IEnumerable<T> collection, int capacity) : this(capacity)
        {
            AddRange(collection);
        }

        #endregion

        #region Public Methods

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var element in collection)
                Add(element);
        }

        public void Clear()
        {
            Count = 0;
            _list = new T[1];
        }

        public bool Contains(T item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (_list[i]?.Equals(item) ?? (_list[i] == null && item == null))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException("The destination array is not long enough to contain all the elements of this collection.");
            for (var i = 0; i < Count; ++i)
            {
                array[i + arrayIndex] = _list[i];
            }
        }

        public void Insert(int index, T item)
        {
            if (index > Count)
                throw new ArgumentOutOfRangeException();
            if (Count + 1 >= _list.Length)
                Resize();
            for (var i = Count - 1; i >= index; --i)
                _list[i + 1] = _list[i];
            ++Count;
            _list[index] = item;
        }

        public bool Remove(T element)
        {
            if (Count == 0) return false;
            var foundIdx = IndexOf(element);
            if (foundIdx == -1) return false;
            RemoveAt(foundIdx);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (Count == 0 || index >= Count || index < 0) return;
            for (var i = index; i < Count - 1; ++i)
                _list[i] = _list[i + 1];
            _list[--Count] = default;
            TryTrim();
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            if (Count == 0) return;
            var bitMask = new BitArray(Count);
            for (var i = 0; i < Count; ++i)
                bitMask[i] = predicate.Invoke(_list[i]);
            var gaps = 0;
            for (var i = 0; i < Count; ++i)
            {
                if (bitMask[i])
                    ++gaps;
                if (i < Count - gaps)
                    _list[i] = _list[i + gaps];
                else
                    _list[i] = default;
            }

            Count -= gaps;
            TryTrim();
        }

        public bool Exists(Predicate<T> predicate)
        {
            for (var i = 0; i < Count; ++i)
                if (predicate.Invoke(_list[i]))
                    return true;
            return false;
        }

        public T? Find(Predicate<T> predicate)
        {
            var idx = FindIndex(predicate);
            return idx == -1 ? default : _list[idx];
        }

        public T? FindLast(Predicate<T> predicate)
        {
            var idx = FindLastIndex(predicate);
            return idx == -1 ? default : _list[idx];
        }

        public List<T> FindAll(Predicate<T> predicate)
        {
            var results = new List<T>();
            for (var i = 0; i < Count; ++i)
                if (predicate.Invoke(_list[i]))
                    results.Add(_list[i]);
            return results;
        }

        public int FindIndex(Predicate<T> predicate)
        {
            for (var i = 0; i < Count; ++i)
                if (predicate.Invoke(_list[i]))
                    return i;
            return -1;
        }

        public int FindLastIndex(Predicate<T> predicate)
        {
            for (var i = Count - 1; i >= 0; --i)
                if (predicate.Invoke(_list[i]))
                    return i;
            return -1;
        }

        public int IndexOf(T element)
        {
            for (var i = 0; i < Count; ++i)
                if (_list[i]?.Equals(element) ?? (_list[i] == null && element == null))
                    return i;
            return -1;
        }

        public int LastIndexOf(T element)
        {
            for (var i = Count - 1; i >= 0; --i)
                if (_list[i]?.Equals(element) ?? (_list[i] == null && element == null))
                    return i;
            return -1;
        }

        public List<T> ForEach(Action<T> action)
        {
            for (var i = 0; i < Count; ++i)
                action.Invoke(_list[i]);
            return this;
        }

        public void Reverse()
        {
            var middleIdx = Count / 2;
            for (var i = 0; i < middleIdx; ++i)
                (_list[i], _list[Count - 1 - i]) = (_list[Count - 1 - i], _list[i]);
        }

        public bool TrueForAll(Predicate<T> predicate)
        {
            for (var i = 0; i < Count; ++i)
                if (!predicate.Invoke(_list[i]))
                    return false;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return _list[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        private void Resize()
        {
            if (_list.Length == 0)
            {
                _list = new T[1];
                return;
            }

            var oldList = (T[])_list.Clone();
            _list = new T[_list.Length * ResizeFactor];
            for (var i = 0; i < Count; ++i)
                _list[i] = oldList[i];
        }

        private void TryTrim()
        {
            if ((_list.Length + 1) / ResizeFactor < Count) return;

            var oldList = (T[])_list.Clone();
            _list = new T[(_list.Length + 1) / ResizeFactor];
            for (var i = 0; i < _list.Length; ++i)
                _list[i] = oldList[i];
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
                _list = Array.Empty<T>();
                Count = 0;
            }

#endif
            if (_list.Length == 0)
            {
                foreach (var value in _initialValues)
                    Add(value);
            }
        }

        #endregion
    }
}