namespace Collections
{
    public sealed class LinkedListNode<T>
    {
        public T Value { get; internal set; }
        public LinkedListNode<T>? Next { get; internal set; }
        public LinkedListNode<T>? Previous { get; internal set; }
    }
}
