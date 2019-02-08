using System;
using System.Collections.Generic;
using System.Linq;

namespace fast.search
{
    public class IDAStarSearchSolver<TState> : ISearchAlgorithm<TState> 
        where TState : IProblemState<TState>, IEquatable<TState>
    {
        public ulong StatesEvaluated { get; private set; }
        public double MaxCostEvaulated { get; private set; }

        private Func<TState, double> heuristic;

        public IDAStarSearchSolver(Func<TState, double> heuristic)
        {
            this.heuristic = heuristic;
        }

        const double FOUND = -1;
        private IProblem<TState> problem;
        public IProblemAction[] Solve(IProblem<TState> problem)
        {
            this.problem = problem;
            StatesEvaluated = 0UL;
            MaxCostEvaulated = 0;
            
            var initialState = problem.GetInitialState();
            double bound = heuristic(initialState);
            var path = new Stack<IProblemAction>(10000);
            while(true)
            {
                double result = Search(initialState, initialState, 0, path, bound);
                MaxCostEvaulated = Math.Max(MaxCostEvaulated, result);
                if (result == FOUND) return path.Reverse().ToArray();
                // TODO: how do we identify if we have not found a solution
                if (result == bound) return null;
                bound = result;
            }
        }
        private double Search(TState state, TState parent, double pathCost, Stack<IProblemAction> path, double bound)
        {
            var f = pathCost + heuristic(state);
            StatesEvaluated++;
            if (f > bound) return f;
            if (problem.IsGoal(state)) return FOUND;
            var min = double.MaxValue;
            var actions = problem.Expand(state);
            foreach(var action in actions)
            {
                var (successor, stepCost) = problem.ApplyAction(state, action);
                var successorPathCost = pathCost + stepCost;
                // TODO: so many assumptions here
                if (successor.Equals(parent)) continue;
                path.Push(action);
                var result = Search(successor, state, successorPathCost, path, bound);
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