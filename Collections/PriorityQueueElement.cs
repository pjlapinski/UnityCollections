using System;
using UnityEngine;

namespace Collections
{
    [Serializable]
    public struct PriorityQueueElement<TValue, TPriority>
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
}