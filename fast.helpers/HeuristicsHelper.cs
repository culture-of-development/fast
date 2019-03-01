using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using fast.search;
using fast.search.problems;

namespace fast.helpers
{
    public static class HeuristicsHelper
    {
        public static Func<FindingDirectionsState, double> MakeStraightLineDistanceHeuristic(FindingDirectionsState goal)
        {
            return from => DistanceHelper.Haversine(from.Latitude, from.Longitude, goal.Latitude, goal.Longitude);
        }

        public static Func<FindingDirectionsState, double> MakeLandmarksHeuristic(
            FindingDirectionsState goal,
            IWeightedGraph<FindingDirectionsState, double> mapGraph,
            IEnumerable<FindingDirectionsState> landmarks
        )
        {
            // https://pdfs.semanticscholar.org/8d94/9fb5f753296db787b2b2e10b86b4224545d5.pdf
            var reverseGraph = GraphHelper.MakeReverseGraph(mapGraph);
            var landmarkLookups = landmarks
                .Select(l => BuildShortestPathLookup(mapGraph, reverseGraph, l))
                .ToArray();
            return from => landmarkLookups
                .Max(lookups => {
                    double max = 0d;
                    if (lookups.to_landmark.ContainsKey(from.NodeId) && lookups.to_landmark.ContainsKey(goal.NodeId))
                    {
                        max = Math.Max(max, lookups.to_landmark[from.NodeId] - lookups.to_landmark[goal.NodeId]);
                    }
                    if (lookups.from_landmark.ContainsKey(from.NodeId) && lookups.from_landmark.ContainsKey(goal.NodeId))
                    {
                        max = Math.Max(max, lookups.from_landmark[goal.NodeId] - lookups.from_landmark[from.NodeId]);
                    }
                    return max;
                });
        }

        // TODO: clean up this copy/paste job
        public static Func<FindingDirectionsState, FindingDirectionsState, double> MakeLandmarksHeuristicDouble(
            IWeightedGraph<FindingDirectionsState, double> mapGraph,
            IEnumerable<FindingDirectionsState> landmarks
        )
        {
            // https://pdfs.semanticscholar.org/8d94/9fb5f753296db787b2b2e10b86b4224545d5.pdf
            var reverseGraph = GraphHelper.MakeReverseGraph(mapGraph);
            var landmarkLookups = landmarks
                .Select(l => BuildShortestPathLookup(mapGraph, reverseGraph, l))
                .ToArray();
            return (from, to) => landmarkLookups
                .Max(lookups => {
                    double max = 0d;
                    if (lookups.to_landmark.ContainsKey(from.NodeId) && lookups.to_landmark.ContainsKey(to.NodeId))
                    {
                        max = Math.Max(max, lookups.to_landmark[from.NodeId] - lookups.to_landmark[to.NodeId]);
                    }
                    if (lookups.from_landmark.ContainsKey(from.NodeId) && lookups.from_landmark.ContainsKey(to.NodeId))
                    {
                        max = Math.Max(max, lookups.from_landmark[to.NodeId] - lookups.from_landmark[from.NodeId]);
                    }
                    return max;
                });
        }

        public static (Dictionary<ulong, double> from_landmark, Dictionary<ulong, double> to_landmark) BuildShortestPathLookup(
            IWeightedGraph<FindingDirectionsState, double> forwardGraph,
            IWeightedGraph<FindingDirectionsState, double> reverseGraph,
            FindingDirectionsState landmark
        )
        {
            var from_landmark = BellmanFordDistances(forwardGraph, landmark);
            var to_landmark = BellmanFordDistances(reverseGraph, landmark);
            return (from_landmark, to_landmark);
        }

        public static Dictionary<ulong, double> BellmanFordDistances(
            IWeightedGraph<FindingDirectionsState, double> graph,
            FindingDirectionsState landmark
        )
        {
            // TODO: move this to fast.search
            // https://en.wikipedia.org/wiki/Bellman%E2%80%93Ford_algorithm
            // this is a version of Bellman-Ford that assumes no prior knowledge
            // of the full node set and only tracks reachable nodes, anything not
            // in the result of this function should be considered infinite
            // we also only care about the distances here and do not maintain prev
            var result = new Dictionary<ulong, double>();
            var openSet = new OpenSet<double, FindingDirectionsState>();
            openSet.PushOrImprove(0d, landmark);
            while (!openSet.IsEmpty)
            {
                var dist = openSet.MinCost;
                var node = openSet.PopMin();
                if (result.ContainsKey(node.NodeId)) continue;
                result.Add(node.NodeId, dist);
                var neighbors = graph.GetNeighbors(node);
                foreach(var neighbor in neighbors)
                {
                    openSet.PushOrImprove(dist + graph.GetEdgeWeight(node, neighbor), neighbor);
                }
            }
            return result;
        }
    }
}