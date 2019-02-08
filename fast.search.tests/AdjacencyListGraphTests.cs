using System;
using System.Linq;
using Xunit;

using fast.search;
using Xunit.Abstractions;
using System.Threading;

namespace fast.search.tests
{
    public class AdjacencyListGraphTests : TestsBase
    {
        public AdjacencyListGraphTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void TestSimpleGraph()
        {
            var nodes = Enumerable.Range(1, 5).Select(m => new TestNode { NodeId = (ulong)m }).ToArray();
            var edges = from x in nodes
                        from y in nodes
                        where x.NodeId > y.NodeId
                        select new TestEdge { From = x, To = y, Weight = x.NodeId * y.NodeId };
            var graph = new AdjacencyListWeightedGraph<TestNode, double>(edges);
            foreach(var node in nodes)
            {
                var neighbors = graph.GetNeighbors(node).ToList();
                Assert.Equal((int)node.NodeId - 1, neighbors.Count);
                foreach(var neighbor in neighbors)
                {
                    var edgeWeight = graph.GetEdgeWeight(node, neighbor);
                    var expected = node.NodeId * neighbor.NodeId * 1d;
                    Assert.Equal(expected, edgeWeight);
                }
            }
        }



        class TestNode : IGraphNode
        {
            public ulong NodeId { get; set; }
        }
        class TestEdge : IWeightedGraphEdge<TestNode, double>
        {
            public TestNode From { get; set; }
            public TestNode To { get; set; }
            public double Weight { get; set; }
        }
    }
}