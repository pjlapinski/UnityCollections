using System;

namespace Collections
{
    public class MinPriorityQueue<TValue, TPriority> : PriorityQueue<TValue, TPriority> where TPriority : IComparable<TPriority>
    {
        protected override void HeapifyDown(int idx)
        {
            var leftIdx = GetLeftChildIndex(idx);
            var rightIdx = GetRightChildIndex(idx);

            var smallestIdx = idx;

            if (leftIdx < Count && _elements[leftIdx].Value.CompareTo(_elements[idx].Value) < 0)
                smallestIdx = leftIdx;

            if (rightIdx < Count && _elements[rightIdx].Value.CompareTo(_elements[smallestIdx].Value) < 0)
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
            if (idx > 0 && _elements[parentIdx].Value.CompareTo(_elements[idx].Value) > 0)
            {
                Swap(idx, parentIdx);
                HeapifyUp(parentIdx);
            }
        }
    }
}