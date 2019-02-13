using System;
using System.Collections.Generic;

namespace fast.search
{
    public class BreadthFirstSearchSolver<TState> : ISearchAlgorithm<TState>
        where TState : IProblemState<TState>
    {
        public ulong StatesEvaluated { get; private set; }
        public double MaxCostEvaulated { get; private set; }

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
                var stateCost = openSet.PopMin();
                var state = stateCost.State;
                var cost = stateCost.Cost;
                closedSet.Add(state);
                StatesEvaluated++;
                MaxCostEvaulated = Math.Max(MaxCostEvaulated, cost);
                if (problem.IsGoal(state)) return RebuildSolution(cameFrom, state);
                foreach(var move in problem.Expand(state))
                {
                    var (successor, stepCost) = problem.ApplyAction(state, move);
                    if (closedSet.Contains(successor)) continue;
                    // why is this 1 and not step cost? because that's how we enforce
                    // the BFS property of exploring on level fully before starting the next
                    var wasImprovement = openSet.PushOrImprove(cost + 1, new StateCost<TState>(successor, cost + 1));
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
            return "Breadth First Search (BFS)";
        }
    }
}