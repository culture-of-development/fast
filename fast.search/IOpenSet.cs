using System;
using System.Collections.Generic;

namespace fast.search
{
    public interface IOpenSet<TCost, TState> 
        where TCost : IComparable<TCost>
        where TState : IEquatable<TState>
    {
        bool IsEmpty { get; }
        void Improve(TCost cost, TState state);
        TState PopMin();
    }

    public class OpenSet<TCost, TState> : IOpenSet<TCost, TState>
        where TCost : IComparable<TCost>
        where TState : IEquatable<TState>
    {
        private SortedSet<(TCost Cost, TState State)> items;

        public bool IsEmpty => Size == 0;
        public int Size => items.Count;
        public TCost MinCost => items.Min.Cost;

        public OpenSet()
        {
            items = new SortedSet<(TCost, TState)>(new MagicComparer());
        }

        private class MagicComparer : IComparer<(TCost Cost, TState State)>
        {
            public int Compare((TCost Cost, TState State) x, (TCost Cost, TState State) y)
            {
                // if the state is the same, it's the same regardless of cost
                // TODO: this limits the number of states to int range
                if (x.State.GetHashCode() == y.State.GetHashCode()) return 0;
                // HACK: if it's not the same state, dont ever consider the same based on cost
                // TODO: look at actual impl and see if < or <= is more efficient
                return x.Cost.CompareTo(y.Cost) < 0 ? -1 : 1;
            }
        }

        public void Improve(TCost cost, TState state)
        {
            var possible = (Cost: cost, State: state);
            if (items.TryGetValue(possible, out var actual))
            {
                if (possible.Cost.CompareTo(actual.Cost) < 0)
                {
                    items.Remove(actual);
                    items.Add(possible);
                }
            }
            else
            {
                items.Add(possible);
            }
        }

        public TState PopMin()
        {
            if (IsEmpty) throw new InvalidOperationException("Cannot pop from an empty set.");
            var min = items.Min;
            items.Remove(min);
            return min.State;
        }
    }
}