using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Collections
{
    [Serializable]
    public class LinkedList<T> : ICollection<T>, ISerializationCallbackReceiver
    {
        [SerializeField] private T[] _initialValues;

        #region Public Properties

        [SerializeField] public int Count { get; private set; }
        public bool IsReadOnly => false;
        public bool Empty => Count == 0;
        public LinkedListNode<T> First { get; private set; }
        public LinkedListNode<T> Last { get; private set; }

        #endregion

        #region Constructors

        public LinkedList()
        {
            Count = 0;
        }

        public LinkedList(IEnumerable<T> collection) : this()
        {
            foreach (var element in collection)
            {
                AddFirst(element);
            }
        }

        #endregion

        #region Public Methods

        public void AddFirst(T item) => AddFirst(new LinkedListNode<T> { Value = item });

        public void AddFirst(LinkedListNode<T> node)
        {
            if (First == null)
            {
                First = node;
                Last = node;
                node.Previous = null;
                node.Next = null;
            }
            else
            {
                First.Previous = node;
                node.Next = First;
                node.Previous = null;
                First = node;
            }

            ++Count;
        }

        public void AddLast(T item) => AddLast(new LinkedListNode<T> { Value = item });

        public void AddLast(LinkedListNode<T> node)
        {
            if (Last == null)
            {
                AddFirst(node);
                return;
            }
            Last.Next = node;
            node.Previous = Last;
            node.Next = null;
            Last = node;

            ++Count;
        }

        public void AddAfter(LinkedListNode<T> node, T item) =>
            AddAfter(node, new LinkedListNode<T> { Value = item });

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            if (node == Last)
            {
                AddLast(newNode);
                return;
            }
            node.Next.Previous = newNode;
            newNode.Previous = node;
            newNode.Next = node.Next;
            node.Next = newNode;

            ++Count;
        }

        public void AddBefore(LinkedListNode<T> node, T item) =>
            AddBefore(node, new LinkedListNode<T> { Value = item });

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            if (node == First)
            {
                AddFirst(newNode);
                return;
            }
            node.Previous.Next = newNode;
            newNode.Next = node;
            newNode.Previous = node.Previous;
            node.Previous = newNode;

            ++Count;
        }

        public void Add(T item) => AddLast(item);

        public void Clear()
        {
            First = null;
            Last = null;
            Count = 0;
        }

        public bool Contains(T item)
        {
            if (Count == 0) return false;
            var node = First;
            while (node != null)
            {
                if (node.Value?.Equals(item) ?? (node.Value == null && item == null))
                    return true;
                node = node.Next;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException("The destination array is not long enough to contain all the elements of this collection.");
            foreach (var value in this)
            {
                array[arrayIndex++] = value;
            }
        }

        public void RemoveFirst()
        {
            if (Count == 0) return;
            if (First.Next == null)
            {
                First = null;
                Last = null;
            }
            else
            {
                First.Next.Previous = null;
                var node = First;
                First = node.Next;
                node.Next = null;
                node.Value = default;
            }

            --Count;
        }

        public void RemoveLast()
        {
            if (Count == 0) return;
            if (Last.Previous == null)
            {
                First = null;
                Last = null;
            }
            else
            {
                Last.Previous.Next = null;
                var node = Last;
                Last = node.Previous;
                node.Previous = null;
                node.Value = default;
            }

            --Count;
        }

        public bool Remove(T item)
        {
            if (Count == 0) return false;
            var node = First;
            while (node != null)
            {
                if (node.Value?.Equals(item) ?? (node.Value == null && item == null))
                {
                    if (node.Previous == null) // node is first
                        First = node.Next;
                    else
                    {
                        node.Previous.Next = node.Next;
                        if (node.Next == null) // node is last
                            Last = node.Previous;
                        else
                            node.Next.Previous = node.Previous;
                    }
                    node.Value = default;

                    --Count;
                    return true;
                }
                node = node.Next;
            }

            return false;
        }

        public LinkedListNode<T> Find(T item)
        {
            var node = First;
            while (node != null)
            {
                if (node.Value.Equals(item) || (node.Value == null && item == null))
                    return node;
                node = node.Next;
            }
            return null;
        }

        public LinkedListNode<T> FindLast(T item)
        {
            var node = Last;
            while (node != null)
            {
                if (node.Value.Equals(item) || (node.Value == null && item == null))
                    return node;
                node = node.Previous;
            }
            return null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var node = First;
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Unity Serialization

        [SerializeField, HideInInspector] private T[] _list;

        public void OnBeforeSerialize()
        {
            _list = new T[Count];
            var idx = 0;
            foreach (var element in this)
                _list[idx++] = element;
        }

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

#endif
            if (_list == null
#if UNITY_EDITOR
                || !playMode
#endif
            )
            {
                _list = (T[])_initialValues.Clone();
            }
            Clear();
            foreach (var element in _list)
                Add(element);
            _list = Array.Empty<T>();
        }

        #endregion
    }
}
