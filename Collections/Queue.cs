using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Collections
{
    [Serializable]
    public class Queue<T> : ICollection<T>
    {
        #region Public Properties

        public int Count => _values.Count;
        public bool IsReadOnly => false;
        public bool Empty => _values.Empty;

        #endregion

        #region Private Fields

        [SerializeField] private LinkedList<T> _values;

        #endregion

        #region Constructors

        public Queue()
        {
            _values = new LinkedList<T>();
        }

        public Queue(IEnumerable<T> collection)
        {
            _values = new LinkedList<T>(collection);
        }

        #endregion

        #region Public Methods

        public void Add(T item) => Enqueue(item);

        public void Clear() => _values.Clear();

        public bool Contains(T item) => _values.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public void Enqueue(T item) => _values.AddLast(item);

        public T Dequeue()
        {
            if (!TryDequeue(out var value))
                throw new InvalidOperationException("The queue is empty, it cannot be dequeued");
            return value;
        }

        public bool TryDequeue(out T value)
        {
            if (Empty)
            {
                value = default;
                return false;
            }
            value = _values.First.Value;
            _values.RemoveFirst();
            return true;
        }

        public bool Remove(T item) => _values.Remove(item);

        public T Peek()
        {
            if (!TryPeek(out var value))
                throw new InvalidOperationException("The queue is empty, it cannot be peeked");
            return value;
        }

        public bool TryPeek(out T value)
        {
            if (Empty)
            {
                value = default;
                return false;
            }
            value = _values.First.Value;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
