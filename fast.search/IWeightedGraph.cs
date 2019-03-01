using System.Collections.Generic;
using System.Linq;

namespace fast.search
{
    public interface IGraphNode
    {
        ulong NodeId { get; }
    }

    public interface IWeightedGraph<TNode, TWeight>
        where TNode : IGraphNode
    {
        // this is by default directional
        // if you want undirected, then initialize with edges in both directions
        IEnumerable<TNode> GetNeighbors(TNode node);
        TWeight GetEdgeWeight(TNode from, TNode to);
        IEnumerable<(TNode from, TNode to, TWeight weight)> EnumerateEdges();
    }

    public interface IWeightedGraphEdge<TNode, TWeight>
        where TNode : IGraphNode
    {
        TNode From { get; }
        TNode To { get; }
        TWeight Weight { get; }
    }

    public class AdjacencyListWeightedGraph<TNode, TWeight> : IWeightedGraph<TNode, TWeight>
        where TNode : IGraphNode
    {
        // should these be read only dictionaries?
        private Dictionary<TNode, Dictionary<TNode, TWeight>> edges;

        public AdjacencyListWeightedGraph(IEnumerable<IWeightedGraphEdge<TNode, TWeight>> edges)
        {
            this.edges = edges
                .GroupBy(m => m.From)
                .ToDictionary(
                    m => m.Key,
                    m => m.ToDictionary(j => j.To, j => j.Weight)
                );
        }
        
        public TWeight GetEdgeWeight(TNode from, TNode to)
        {
            return edges[from][to];
        }

        public IEnumerable<TNode> GetNeighbors(TNode node)
        {
            // TODO: this should throw if the node is not in the graph at all
            if (!edges.ContainsKey(node)) return new TNode[0];
            return edges[node].Keys;
        }

        public IEnumerable<(TNode from, TNode to, TWeight weight)> EnumerateEdges()
        {
            return edges.SelectMany(from => from.Value.Select(to => (from.Key, to.Key, to.Value)));
        }
    }
}