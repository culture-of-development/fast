using System;
using System.Collections.Generic;
using System.Linq;

namespace fast.search
{
    public class AStarSearchSolver : ISearchAlgorithm
    {
        public ulong StatesEvaluated { get; private set; }
        public int MaxCostEvaulated { get; private set; }

        private Func<NPuzzle, int> heuristic;

        public AStarSearchSolver(Func<NPuzzle, int> heuristic)
        {
            this.heuristic = heuristic;
        }

        public NPuzzle.Location[] Solve(NPuzzle initialState)
        {
            StatesEvaluated = 0UL;
            MaxCostEvaulated = 0;
            
            var openSet = new OpenSet<int, AStarOpenSetItem>();
            var closedSet = new HashSet<NPuzzle>();
            var cameFrom = new Dictionary<NPuzzle, (NPuzzle parent, NPuzzle.Location move)>();
            
            openSet.PushOrImprove(0, new AStarOpenSetItem(initialState, 0));
            cameFrom.Add(initialState, (null, null)); 

            while (!openSet.IsEmpty)
            {
                var next = openSet.PopMin();
                var state = next.State;
                var cost = next.Cost;
                closedSet.Add(state);
                StatesEvaluated++;
                MaxCostEvaulated = Math.Max(MaxCostEvaulated, cost);
                if (state.IsGoal()) return RebuildSolution(cameFrom, state);
                foreach(var move in state.ExpandMoves())
                {
                    var successor = state.MoveCopy(move);
                    if (closedSet.Contains(successor)) continue;
                    openSet.PushOrImprove(
                        cost + NPuzzle.StepCost + heuristic(successor), 
                        new AStarOpenSetItem(successor, cost + NPuzzle.StepCost)
                    );
                    cameFrom[successor] = (state, move);
                }
            }
            return null;
        }

        private NPuzzle.Location[] RebuildSolution(
            Dictionary<NPuzzle, (NPuzzle parent, NPuzzle.Location move)> cameFrom,
            NPuzzle end
        )
        {
            var state = end;
            var solution = new List<NPuzzle.Location>();
            while(true)
            {
                var (parent, move) = cameFrom[state];
                if (move == null) break;
                solution.Add(move);
                state = parent;
            }
            solution.Reverse();
            return solution.ToArray();
        }

        public override string ToString()
        {
            return $"A* ({heuristic.Method.Name})";
        }

        private struct AStarOpenSetItem : IEquatable<AStarOpenSetItem>
        {
            public int Cost { get; private set; }
            public NPuzzle State { get; private set; }
            
            public AStarOpenSetItem(NPuzzle state, int cost)
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
            public bool Equals(AStarOpenSetItem other)
            {
                return State.Equals(other.State);
            }
            public override string ToString()
            {
                return State.ToString();
            }
        }
    }
}