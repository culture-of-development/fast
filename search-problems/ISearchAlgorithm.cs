namespace search_problems
{
    public interface ISearchAlgorithm
    {
        ulong StatesEvaluated { get; }
        NPuzzle.Location[] Solve(NPuzzle puzzle);
    }
}