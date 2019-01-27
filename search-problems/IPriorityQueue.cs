namespace fast.search_problems
{
    public interface IPriorityQueue<TKey, TValue>
    {
        bool IsEmpty { get; }
        void Push(TKey key, TValue value);
        TValue Pop();
    }

    public interface IPriorityQueueSet<TKey, TValue>
    {
        bool IsEmpty { get; }
        void Push(TKey key, TValue value);
        TValue Pop();
    }
}