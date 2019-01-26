namespace search_problems
{
    public interface ISearchAlgorithm
    {
        ulong StatesEvaluated { get; }
        int MaxDepth { get; }
        NPuzzle.Location[] Solve(NPuzzle puzzle);
    }
}