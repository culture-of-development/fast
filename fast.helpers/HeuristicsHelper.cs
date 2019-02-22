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
            var landmarkLookups = landmarks
                .Select(l => BuildShortestPathLookup(mapGraph, l))
                .ToArray();
            return from => landmarkLookups
                .Max(lookup => Math.Abs(lookup[from.NodeId] - lookup[goal.NodeId]));
        }

        public static Dictionary<ulong, double> BuildShortestPathLookup(
            IWeightedGraph<FindingDirectionsState, double> mapGraph,
            FindingDirectionsState landmark
        )
        {
            // TODO: abstract this to Djikstra's algorithm
            // https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
            // function Dijkstra(Graph, source):
            //     create vertex set Q
            //     for each vertex v in Graph:             
            //         dist[v] ← INFINITY                  
            //         prev[v] ← UNDEFINED                 
            //         add v to Q                      
            //     dist[source] ← 0                        
            //     while Q is not empty:
            //         u ← vertex in Q with min dist[u]    
            //         remove u from Q 
            //         for each neighbor v of u:           
            //             alt ← dist[u] + length(u, v)
            //             if alt < dist[v]:               
            //                 dist[v] ← alt 
            //                 prev[v] ← u 
            //     return dist[], prev[]
            var result = new Dictionary<ulong, double>();
            var openSet = new OpenSet<double, FindingDirectionsState>();
            openSet.PushOrImprove(0d, landmark);
            while (!openSet.IsEmpty)
            {
                var dist = openSet.MinCost;
                var node = openSet.PopMin();
                if (result.ContainsKey(node.NodeId)) continue;
                result.Add(node.NodeId, dist);
                var neighbors = mapGraph.GetNeighbors(node);
                foreach(var neighbor in neighbors)
                {
                    openSet.PushOrImprove(dist + mapGraph.GetEdgeWeight(node, neighbor), neighbor);
                }
            }
            return result;
        }
    }
}