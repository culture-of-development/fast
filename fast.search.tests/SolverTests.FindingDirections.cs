using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fast.helpers;
using fast.search.problems;
using Xunit;

namespace fast.search.tests
{
    partial class SolverTests
    {
        [Fact]
        public void TestFindingDirectionsBreadthFirstSearchRomania()
        {
            var solver = new BreadthFirstSearchSolver<FindingDirectionsState>();
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 450);
            // note this is not optimal in fewest number of edges, not cost
        }

        [Fact]
        public void TestFindingDirectionsBestFirstSearchRomania()
        {
            var solver = new AStarSearchSolver<FindingDirectionsState>(_ => default(double));
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 418);
        }

        [Fact]
        public void TestFindingDirectionsAStarSearchRomaniaStraightLineDistanceHeuristic()
        {
            var heuristic = StraightLineToBucharestHeuristic();
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 418);
        }

        [Fact]
        public void TestFindingDirectionsIDAStarSearchRomaniaStraightLineDistanceHeuristic()
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
                if (!nodes.ContainsKey(parts[0])) nodes.Add(parts[0], new FindingDirectionsState(++nodeId, name: parts[0]));
                if (!nodes.ContainsKey(parts[1])) nodes.Add(parts[1], new FindingDirectionsState(++nodeId, name: parts[1]));
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




        [Fact]
        public void TestFindingDirectionsAStarSearchLimaStraightLineDistanceHeuristic()
        {
            var filename = @"../../../../datasets/Lima/Lima.osm.pbf";
            var problem = OpenStreetMapDataHelper.ExtractMapProblem(
                    filename, 
                    -12.073457334109781, -77.16832519640246,
                    -12.045889755060621,-77.04266126523356
                    // -12.041839850701912,-77.06330992167841,
                    // -12.060138589193466,-77.01043821757685
                );
            var goal = problem.GetGoalState();
            Func<FindingDirectionsState, double> heuristic = from => 
                DistanceHelper.Haversine(from.Latitude, from.Longitude, goal.Latitude, goal.Longitude);
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            TestSolver(solver, problem, 418);
        }
    }
}