using System;
using System.Collections.Generic;

namespace Collections
{
    [Serializable]
    public class MaxPriorityQueue<TValue, TPriority> : PriorityQueue<TValue, TPriority> where TPriority : IComparable<TPriority>
    {
        public MaxPriorityQueue(int capacity) : base(capacity) { }

        public MaxPriorityQueue() : base() { }

        public MaxPriorityQueue(IEnumerable<ValueTuple<TValue, TPriority>> collection) : base(collection) { }

        protected override void HeapifyDown(int idx)
        {
            var leftIdx = GetLeftChildIndex(idx);
            var rightIdx = GetRightChildIndex(idx);

            while (leftIdx < Count && rightIdx < Count)
            {
                var largestIdx = idx;

                if (leftIdx < Count && _elements[leftIdx].Priority.CompareTo(_elements[idx].Priority) > 0) 
                    largestIdx = leftIdx;

                if (rightIdx < Count && _elements[rightIdx].Priority.CompareTo(_elements[largestIdx].Priority) > 0) 
                    largestIdx = rightIdx;

                if (largestIdx == idx) return;

                Swap(idx, largestIdx);
                idx = largestIdx;

                leftIdx = GetLeftChildIndex(idx);
                rightIdx = GetRightChildIndex(idx);
            }
        }

        protected override void HeapifyUp(int idx)
        {
            while (idx > 0)
            {
                var parentIdx = GetParentIndex(idx);

                if (_elements[parentIdx].Priority.CompareTo(_elements[idx].Priority) >= 0) return;

                Swap(idx, parentIdx);
                idx = parentIdx;
            }
        }
    }
}
