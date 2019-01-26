namespace search_problems
{
    public interface IPriorityQueue<TKey, TValue>
    {
        bool IsEmpty { get; }
        void Push(TKey key, TValue value);
        (TKey, TValue) Pop();
    }
}