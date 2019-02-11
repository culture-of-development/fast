using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fast.search.problems;
using Xunit;

namespace fast.search.tests
{
    partial class SolverTests
    {
        [Fact]
        public void TestFindingDirectionsBreadthFirstSearch()
        {
            var solver = new BreadthFirstSearchSolver<FindingDirectionsState>();
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 450);
            // note this is not optimal in fewest number of edges, not cost
        }

        [Fact]
        public void TestFindingDirectionsBestFirstSearch()
        {
            var solver = new AStarSearchSolver<FindingDirectionsState>(_ => default(double));
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 418);
        }

        [Fact]
        public void TestFindingDirectionsAStarSearchStraightLineDistanceHeuristic()
        {
            var heuristic = StraightLineToBucharestHeuristic();
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 418);
        }

        [Fact]
        public void TestFindingDirectionsIDAStarSearchStraightLineDistanceHeuristic()
        {
            var heuristic = StraightLineToBucharestHeuristic();
            var solver = new IDAStarSearchSolver<FindingDirectionsState>(heuristic);
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 418);
        }



        private FindingDirectionsProblem GetRomaniaProblem()
        {
            ulong nodeId = 0;
            var nodes = new Dictionary<string, FindingDirectionsState>();
            var edges = new List<RomaniaEdge>();
            var romaniaLines = File.ReadAllLines("RomaniaGraph.csv");
            foreach(var line in romaniaLines.Skip(1)) // skip header
            {
                var parts = line.Split(',');
                if (!nodes.ContainsKey(parts[0])) nodes.Add(parts[0], new FindingDirectionsState(++nodeId, parts[0]));
                if (!nodes.ContainsKey(parts[1])) nodes.Add(parts[1], new FindingDirectionsState(++nodeId, parts[1]));
                edges.Add(new RomaniaEdge { From = nodes[parts[0]], To = nodes[parts[1]], Weight = double.Parse(parts[2]) });
            }
            var graph = new AdjacencyListWeightedGraph<FindingDirectionsState, double>(edges);
            var problem = new FindingDirectionsProblem(graph, nodes["Arad"], nodes["Bucharest"]);
            return problem;
        }
        class RomaniaEdge : IWeightedGraphEdge<FindingDirectionsState, double>
        {
            public FindingDirectionsState From { get; set; }
            public FindingDirectionsState To { get; set; }
            public double Weight { get; set; }
        }

        Func<FindingDirectionsState, double> StraightLineToBucharestHeuristic()
        {
            var distances = File.ReadAllLines("RomaniaGraphBucharestHeuristic.csv")
                .Skip(1) // header
                .Select(m => m.Split(','))
                .ToDictionary(m => m[0], m => double.Parse(m[1]));
            return (FindingDirectionsState location) => distances[location.LocationName];
        }
    }
}