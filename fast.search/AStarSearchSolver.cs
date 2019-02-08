using System;
using System.Collections.Generic;
using System.Linq;

namespace fast.search
{
    public class AStarSearchSolver<TState> : ISearchAlgorithm<TState> 
        where TState : IProblemState<TState>
    {
        public ulong StatesEvaluated { get; private set; }
        public double MaxCostEvaulated { get; private set; }

        private Func<TState, double> heuristic;

        public AStarSearchSolver(Func<TState, double> heuristic)
        {
            this.heuristic = heuristic;
        }

        public IProblemAction[] Solve(IProblem<TState> problem)
        {
            StatesEvaluated = 0UL;
            MaxCostEvaulated = 0;
            
            var openSet = new OpenSet<double, StateCost<TState>>();
            var closedSet = new HashSet<TState>();
            var cameFrom = new Dictionary<TState, (TState parent, IProblemAction move)>();
            
            var initialState = problem.GetInitialState();
            openSet.PushOrImprove(0, new StateCost<TState>(initialState, 0));
            cameFrom.Add(initialState, (default(TState), null)); 

            while (!openSet.IsEmpty)
            {
                var next = openSet.PopMin();
                var state = next.State;
                var cost = next.Cost;
                closedSet.Add(state);
                StatesEvaluated++;
                MaxCostEvaulated = Math.Max(MaxCostEvaulated, cost);
                if (problem.IsGoal(state)) return RebuildSolution(cameFrom, state);
                foreach(var move in problem.Expand(state))
                {
                    var successor = state.Copy();
                    int stepCost = problem.ApplyAction(successor, move);
                    if (closedSet.Contains(successor)) continue;
                    var wasImprovement = 
                        openSet.PushOrImprove(
                            cost + stepCost + heuristic(successor), 
                            new StateCost<TState>(successor, cost + stepCost)
                        );
                    if (wasImprovement) cameFrom[successor] = (state, move);
                }
            }
            return null;
        }

        private IProblemAction[] RebuildSolution(
            Dictionary<TState, (TState parent, IProblemAction move)> cameFrom,
            TState end
        )
        {
            var state = end;
            var solution = new List<IProblemAction>();
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