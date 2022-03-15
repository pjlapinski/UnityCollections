using System;
using System.Collections.Generic;

namespace Collections
{
    public class MaxPriorityQueue<TValue, TPriority> : PriorityQueue<TValue, TPriority> where TPriority : IComparable<TPriority>
    {
        public MaxPriorityQueue(int capacity) : base(capacity) { }

        public MaxPriorityQueue() : base() { }

        public MaxPriorityQueue(IEnumerable<ValueTuple<TValue, TPriority>> collection) : base(collection) { }

        protected override void HeapifyDown(int idx)
        {
            var leftIdx = GetLeftChildIndex(idx);
            var rightIdx = GetRightChildIndex(idx);

            var largestIdx = idx;

            if (leftIdx < Count && _elements[leftIdx].Value.CompareTo(_elements[idx].Value) > 0)
                largestIdx = leftIdx;

            if (rightIdx < Count && _elements[rightIdx].Value.CompareTo(_elements[largestIdx].Value) > 0)
                largestIdx = rightIdx;

            if (largestIdx != idx)
            {
                Swap(idx, largestIdx);
                HeapifyDown(largestIdx);
            }
        }

        protected override void HeapifyUp(int idx)
        {
            var parentIdx = GetParentIndex(idx);
            if (idx > 0 && _elements[parentIdx].Value.CompareTo(_elements[idx].Value) < 0)
            {
                Swap(idx, parentIdx);
                HeapifyUp(parentIdx);
            }
        }
    }
}
