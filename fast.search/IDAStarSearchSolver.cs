using System;
using System.Collections.Generic;
using System.Linq;

namespace fast.search
{
    public class IDAStarSearchSolver<TState> : ISearchAlgorithm<TState> 
        where TState : IProblemState<TState>, IEquatable<TState>
    {
        public ulong StatesEvaluated { get; private set; }
        public int MaxCostEvaulated { get; private set; }

        private Func<TState, int> heuristic;

        public IDAStarSearchSolver(Func<TState, int> heuristic)
        {
            this.heuristic = heuristic;
        }

        const int FOUND = -1;
        const int NO_SOLUTION = -2;
        private IProblem<TState> problem;
        public IProblemAction[] Solve(IProblem<TState> problem)
        {
            this.problem = problem;
            StatesEvaluated = 0UL;
            MaxCostEvaulated = 0;
            
            var initialState = problem.GetInitialState();
            int bound = heuristic(initialState);
            var path = new Stack<IProblemAction>(10000);
            while(true)
            {
                int result = Search(initialState, initialState, 0, path, bound);
                MaxCostEvaulated = Math.Max(MaxCostEvaulated, result);
                if (result == FOUND) return path.Reverse().ToArray();
                if (result == NO_SOLUTION) return null;
                bound = result;
            }
        }
        private int Search(TState state, TState parent, int pathCost, Stack<IProblemAction> path, int bound)
        {
            int f = pathCost + heuristic(state);
            StatesEvaluated++;
            if (f > bound) return f;
            if (problem.IsGoal(state)) return FOUND;
            int min = int.MaxValue;
            var actions = problem.Expand(state);
            foreach(var action in actions)
            {
                var successor = state.Copy();
                int successorPathCost = pathCost + problem.ApplyAction(successor, action);
                // TODO: so many assumptions here
                if (successor.Equals(parent)) continue;
                path.Push(action);
                int result = Search(successor, state, successorPathCost, path, bound);
                if (result == FOUND) return FOUND;
                min = Math.Min(min, result);
                path.Pop();
            }
            return min;
        }

        public override string ToString()
        {
            return $"IDA* ({heuristic.Method.Name})";
        }
    }
}