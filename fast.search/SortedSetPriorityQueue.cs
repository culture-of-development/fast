using System;
using System.Collections.Generic;

namespace fast.search
{
    public class SortedSetPriorityQueue<TKey, TValue> : IPriorityQueueSet<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private SortedSet<SortedSetItem> items;

        public bool IsEmpty => items.Count == 0;

        public SortedSetPriorityQueue()
        {
            items = new SortedSet<SortedSetItem>(new SortedSetItemScoreComparator());
        }

        public TValue Pop()
        {
            var min = items.Min;
            items.Remove(min);
            return min.Item;
        }

        public void Push(TKey key, TValue value)
        {
            var setItem = new SortedSetItem(key, value);
            if (items.Contains(setItem)) throw new InvalidOperationException("the provided item already exists in the set");
            items.Add(setItem);
        }

        private class SortedSetItem
        {
            public TKey Score { get; }
            public TValue Item { get; }

            public SortedSetItem(TKey score, TValue item)
            {
                Score = score;
                Item = item;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;
                SortedSetItem other = (SortedSetItem)obj;
                return Item.Equals(other.Item);
            }
            
            // override object.GetHashCode
            public override int GetHashCode()
            {
                return Item.GetHashCode();
            }
        }

        private class SortedSetItemScoreComparator : IComparer<SortedSetItem>
        {
            public int Compare(SortedSetItem x, SortedSetItem y)
            {
                return x.Score.CompareTo(y.Score);
            }
        }
    }
}