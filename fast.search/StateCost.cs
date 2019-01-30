using System;

namespace fast.search
{
    public struct StateCost : IEquatable<StateCost>
    {
        public int Cost { get; private set; }
        public NPuzzle State { get; private set; }
        
        public StateCost(NPuzzle state, int cost)
        {
            Cost = cost;
            State = state;
        }

        // these have to be implemented so that the sets can correctly dedupe
        public override int GetHashCode()
        {
            // the only meaningful part of this object is the board representation
            // the rest is only there to help us perform other ops faster
            return State.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals((NPuzzle)obj);
        }
        public bool Equals(StateCost other)
        {
            return State.Equals(other.State);
        }
        public override string ToString()
        {
            return State.ToString();
        }
    }
}