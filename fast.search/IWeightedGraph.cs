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
        private Dictionary<ulong, Dictionary<ulong, (TNode node, TWeight weight)>> edges;

        public AdjacencyListWeightedGraph(IEnumerable<IWeightedGraphEdge<TNode, TWeight>> edges)
        {
            this.edges = edges
                .GroupBy(m => m.From)
                .ToDictionary(
                    m => m.Key.NodeId,
                    // TODO: remove this logic, deduping edges should throw, revert this
                    m => m.GroupBy(j => j.To.NodeId)
                        .ToDictionary(j => j.Key, j => { var a = j.OrderBy(t => t.Weight).First(); return (a.To, a.Weight); })
                );
        }
        
        public TWeight GetEdgeWeight(TNode from, TNode to)
        {
            return edges[from.NodeId][to.NodeId].weight;
        }

        public IEnumerable<TNode> GetNeighbors(TNode node)
        {
            // TODO: this should throw if the node is not in the graph at all
            if (!edges.ContainsKey(node.NodeId)) return new TNode[0];
            return edges[node.NodeId].Values.Select(m => m.node);
        }
    }
}