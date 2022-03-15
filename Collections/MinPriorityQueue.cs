using System;
using System.Collections.Generic;

namespace Collections
{
    [Serializable]
    public class MinPriorityQueue<TValue, TPriority> : PriorityQueue<TValue, TPriority> where TPriority : IComparable<TPriority>
    {
        public MinPriorityQueue(int capacity) : base(capacity) { }

        public MinPriorityQueue() : base() { }

        public MinPriorityQueue(IEnumerable<ValueTuple<TValue, TPriority>> collection) : base(collection) { }

        protected override void HeapifyDown(int idx)
        {
            var leftIdx = GetLeftChildIndex(idx);
            var rightIdx = GetRightChildIndex(idx);

            var smallestIdx = idx;

            if (leftIdx < Count && _elements[leftIdx].Priority.CompareTo(_elements[idx].Priority) < 0)
                smallestIdx = leftIdx;

            if (rightIdx < Count && _elements[rightIdx].Priority.CompareTo(_elements[smallestIdx].Priority) < 0)
                smallestIdx = rightIdx;

            if (smallestIdx != idx)
            {
                Swap(idx, smallestIdx);
                HeapifyDown(smallestIdx);
            }
        }

        protected override void HeapifyUp(int idx)
        {
            var parentIdx = GetParentIndex(idx);
            if (idx > 0 && _elements[parentIdx].Priority.CompareTo(_elements[idx].Priority) > 0)
            {
                Swap(idx, parentIdx);
                HeapifyUp(parentIdx);
            }
        }
    }
}