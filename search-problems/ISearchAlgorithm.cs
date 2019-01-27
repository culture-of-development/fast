namespace fast.search_problems
{
    public interface ISearchAlgorithm
    {
        ulong StatesEvaluated { get; }
        int MaxCostEvaulated { get; }
        NPuzzle.Location[] Solve(NPuzzle puzzle);
    }
}