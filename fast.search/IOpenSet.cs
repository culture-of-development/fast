using System;
using System.Collections.Generic;
using System.Linq;

namespace fast.search
{
    public interface IOpenSet<TCost, TState> 
        where TCost : IComparable<TCost>
        where TState : IEquatable<TState>
    {
        bool IsEmpty { get; }
        bool PushOrImprove(TCost cost, TState state);
        TState PopMin();
    }

    public class OpenSet<TCost, TState> : IOpenSet<TCost, TState>
        where TCost : IComparable<TCost>
        where TState : IEquatable<TState>
    {
        Dictionary<TState, TCost> stateCosts;
        SortedDictionary<TCost, HashSet<TState>> costStates;

        public bool IsEmpty => Size == 0;
        public int Size => stateCosts.Count;
        public TCost MinCost => costStates.First().Key;

        public OpenSet()
        {
            stateCosts = new Dictionary<TState, TCost>();
            costStates = new SortedDictionary<TCost, HashSet<TState>>();
        }

        public bool PushOrImprove(TCost cost, TState state)
        {
            if (stateCosts.ContainsKey(state))
            {
                var existingCost = stateCosts[state];
                if (cost.CompareTo(existingCost) >= 0) return false;
                stateCosts[state] = cost;
                // solid indication of a problem if this fails
                var states = costStates[existingCost];
                states.Remove(state);
                if (states.Count == 0) 
                {
                    costStates.Remove(existingCost);
                }
            }
            else
            {
                stateCosts.Add(state, cost);
            }
            // add the state to the costStates
            if (costStates.ContainsKey(cost))
            {
                costStates[cost].Add(state);
            }
            else
            {
                var states = new HashSet<TState>();
                states.Add(state);
                costStates.Add(cost, states);
            }
            return true;
        }

        public TState PopMin()
        {
            if (IsEmpty) throw new InvalidOperationException("Cannot pop from an empty set.");
            var min = costStates.First();
            var minStates = min.Value;
            // TODO: worried about stability? probably not
            var state = minStates.First();
            stateCosts.Remove(state);
            minStates.Remove(state);
            if (minStates.Count == 0)
            {
                costStates.Remove(min.Key);
            }
            return state;
        }
    }
}