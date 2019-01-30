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
            
            var openSet = new SortedSetPriorityQueue<int, (NPuzzle state, int cost)>();
            var closedSet = new HashSet<NPuzzle>();
            var cameFrom = new Dictionary<NPuzzle, (NPuzzle parent, NPuzzle.Location move)>();
            
            openSet.Push(0, (initialState, 0));
            cameFrom.Add(initialState, (null, null)); 

            while (!openSet.IsEmpty)
            {
                var (state, cost) = openSet.Pop();
                closedSet.Add(state);
                StatesEvaluated++;
                MaxCostEvaulated = Math.Max(MaxCostEvaulated, cost);
                if (state.IsGoal()) return RebuildSolution(cameFrom, state);
                foreach(var move in state.ExpandMoves())
                {
                    var successor = state.MoveCopy(move);
                    if (closedSet.Contains(successor)) continue;
                    // TODO: check if it's better than the best in the open set
                    openSet.Push(cost + NPuzzle.StepCost + heuristic(successor), (successor, cost + NPuzzle.StepCost));
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
    }
}