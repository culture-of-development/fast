using System;

namespace fast.search
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