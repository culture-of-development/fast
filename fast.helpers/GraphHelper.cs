using System;
using System.Linq;
using fast.search;

namespace fast.helpers
{
    public static class GraphHelper
    {
        public static IWeightedGraph<TState, TWeight> MakeReverseGraph<TState, TWeight>(IWeightedGraph<TState, TWeight> graph)
            where TState : IGraphNode
        {
            var reverseNodes = graph.EnumerateEdges().Select(m => new GenericGraphEdge<TState, TWeight> { From = m.to, To = m.from, Weight = m.weight });
            return new AdjacencyListWeightedGraph<TState, TWeight>(reverseNodes);
        }
        
        private class GenericGraphEdge<TState, TWeight> : IWeightedGraphEdge<TState, TWeight>
            where TState : IGraphNode
        {
            public TState From { get; set; }
            public TState To { get; set; }
            public TWeight Weight { get; set; }
        }
    }
}