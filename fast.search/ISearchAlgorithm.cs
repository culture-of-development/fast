namespace fast.search
{
    public interface ISearchAlgorithm
    {
        ulong StatesEvaluated { get; }
        int MaxCostEvaulated { get; }
        NPuzzle.Location[] Solve(NPuzzle puzzle);
    }
}