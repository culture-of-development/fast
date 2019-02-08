namespace fast.search
{
    public interface IGraphNode
    {
        ulong NodeId { get; }
    }

    public interface IWeightedGraph<TNode>
        where TNode : IGraphNode
    {
        // this is by default directional
        // if you want undirected, then initialize with edges in both directions
        TNode[] GetNeighbors(TNode node);
        double GetEdgeWeight(TNode from, TNode to);
    }
}