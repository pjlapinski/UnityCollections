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

            while (leftIdx < Count && rightIdx < Count)
            {
                var smallestIdx = idx;

                if (leftIdx < Count && _elements[leftIdx].Priority.CompareTo(_elements[idx].Priority) < 0)
                    smallestIdx = leftIdx;

                if (rightIdx < Count && _elements[rightIdx].Priority.CompareTo(_elements[smallestIdx].Priority) < 0)
                    smallestIdx = rightIdx;

                if (smallestIdx == idx) return;

                Swap(idx, smallestIdx);
                idx = smallestIdx;

                leftIdx = GetLeftChildIndex(idx);
                rightIdx = GetRightChildIndex(idx);
            }   
        }

        protected override void HeapifyUp(int idx)
        {
            while (idx > 0)
            {
                var parentIdx = GetParentIndex(idx);
                if (_elements[parentIdx].Priority.CompareTo(_elements[idx].Priority) <= 0) return;

                Swap(idx, parentIdx);
                idx = parentIdx;
            }
        }
    }
}