using System;
using System.Collections.Generic;

namespace fast.search
{
    public interface IProblemAction
    {
    }

    public interface IProblemState<TState> : IEquatable<TState>
    {
    }

    public interface IProblem<TState>
        where TState : IProblemState<TState>
    {
        (TState, double) ApplyAction(TState state, IProblemAction action);
        bool IsGoal(TState state);
        IEnumerable<IProblemAction> Expand(TState state);
        TState GetInitialState();
    }

    public interface ISearchAlgorithm<TState>
        where TState : IProblemState<TState>
    {
        ulong StatesEvaluated { get; }
        double MaxCostEvaulated { get; }
        IProblemAction[] Solve(IProblem<TState> initial);
    }
}