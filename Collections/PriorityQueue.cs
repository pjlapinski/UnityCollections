using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Collections
{
    public abstract class PriorityQueue<TValue, TPriority> : ISerializationCallbackReceiver where TPriority : IComparable<TPriority>
    {
        [SerializeField] private PriorityQueueElement[] _initialValues;

        #region Public Properties

        [field: SerializeField, HideInInspector] public int Count { get; protected set; }

        public bool Empty => Count == 0;

        #endregion

        #region Protected & Private Fields

        private const int ResizeFactor = 2;
        [Serializable]
        protected struct PriorityQueueElement
        {
            [field: SerializeField] public TValue Value { get; private set; }
            [field: SerializeField] public TPriority Priority { get; private set; }

            public PriorityQueueElement(TValue value, TPriority priority)
            {
                Value = value;
                Priority = priority;
            }

            public void Deconstruct(out TValue value, out TPriority priority)
            {
                value = Value;
                priority = Priority;
            }
        }
        [SerializeField, HideInInspector] protected PriorityQueueElement[] _elements;

        #endregion

        #region Constructors

        protected PriorityQueue(int capacity)
        {
            Count = 0;
            if (capacity < 0)
                throw new ArgumentException("Capacity cannot be lower than zero");
            _elements = new PriorityQueueElement[capacity];
        }

        protected PriorityQueue() : this(0) { }

        protected PriorityQueue(IEnumerable<ValueTuple<TValue, TPriority>> collection) : this()
        {
            EnqueueRange(collection);
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            Count = 0;
            _elements = new PriorityQueueElement[1];
        }

        public bool TryDequeue(out TValue value, out TPriority priority)
        {
            if (Empty)
            {
                value = default;
                priority = default;
                return false;
            }

            (value, priority) = _elements[0];
            _elements[0] = _elements[--Count];
            _elements[Count] = default;
            HeapifyDown(0);
            TryTrim();
            return true;
        }

        public TValue Dequeue()
        {
            if (!TryDequeue(out var value, out _))
                throw new InvalidOperationException("The queue is empty, it cannot be dequeued");
            return value;
        }

        public void Enqueue(TValue value, TPriority priority)
        {
            if (Count + 1 >= _elements.Length)
                Resize();
            _elements[Count] = new PriorityQueueElement(value, priority);
            HeapifyUp(Count++);
        }

        public TValue EnqueueDequeue(TValue value, TPriority priority)
        {
            Enqueue(value, priority);
            return Dequeue();
        }

        public void EnqueueRange(IEnumerable<TValue> values, TPriority priority)
        {
            foreach (var value in values)
                Enqueue(value, priority);
        }

        public void EnqueueRange(IEnumerable<ValueTuple<TValue, TPriority>> values)
        {
            foreach (var (value, priority) in values)
                Enqueue(value, priority);
        }

        public bool TryPeek(out TValue value, out TPriority priority)
        {
            if (Empty)
            {
                value = default;
                priority = default;
                return false;
            }

            value = _elements[0].Value;
            priority = _elements[0].Priority;
            return true;
        }

        public TValue Peek()
        {
            if (!TryPeek(out var value, out _))
                throw new InvalidOperationException("The queue is empty, it cannot be peeked");
            return value;
        }

        #endregion

        #region Protected Methods

        protected static int GetLeftChildIndex(int parentIndex) => 2 * parentIndex + 1;
        protected static int GetRightChildIndex(int parentIndex) => 2 * parentIndex + 2;
        protected static int GetParentIndex(int childIndex) => (childIndex - 1) / 2;

        protected void Swap(int first, int second) => (_elements[first], _elements[second]) = (_elements[second], _elements[first]);

        protected abstract void HeapifyDown(int index);
        protected abstract void HeapifyUp(int index);

        protected void Resize()
        {
            if (_elements.Length == 0)
            {
                _elements = new PriorityQueueElement[1];
                return;
            }

            var oldElements = (PriorityQueueElement[])_elements.Clone();
            _elements = new PriorityQueueElement[_elements.Length * ResizeFactor];
            for (var i = 0; i < Count; ++i)
                _elements[i] = oldElements[i];
        }

        protected void TryTrim()
        {
            if ((_elements.Length + 1) / ResizeFactor < Count) return;

            var oldList = (PriorityQueueElement[])_elements.Clone();
            _elements = new PriorityQueueElement[(_elements.Length + 1) / ResizeFactor];
            for (var i = 0; i < _elements.Length; ++i)
                _elements[i] = oldList[i];
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
                _elements = Array.Empty<PriorityQueueElement<TValue, TPriority>>();
                Count = 0;
            }

#endif
            if (_elements.Length == 0)
            {
                foreach (var (value, priority) in _initialValues)
                    Enqueue(value, priority);
            }
        }

        #endregion
    }
}
