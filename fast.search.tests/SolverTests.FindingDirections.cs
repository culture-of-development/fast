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
            //var heuristic = StraightLineToBucharestHeuristic();
            var heuristic = MakeRomaniaLandmarksHeuristic();
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            var problem = GetRomaniaProblem();
            TestSolver(solver, problem, 418);
        }
        private Func<FindingDirectionsState, double> MakeRomaniaLandmarksHeuristic()
        {
            var (graph, nodes) = GetRomaniaGraph();
            var names = nodes.ToDictionary(m => m.Value.NodeId, m => m.Value.LocationName);
            var bucharest = nodes["Bucharest"];
            var clookup = HeuristicsHelper.BuildShortestPathLookup(graph, nodes["Craiova"]);
            var olookup = HeuristicsHelper.BuildShortestPathLookup(graph, nodes["Oradea"]);
            //var dists = lookup.OrderBy(m => names[m.Key]);
            // foreach(var dist in dists)
            // {
            //     output.WriteLine(names[dist.Key] + "," + dist.Value);
            // }

            var l = new[] { nodes["Oradea"], nodes["Craiova"] };
            var h = HeuristicsHelper.MakeLandmarksHeuristic(bucharest, graph, l);
            return h;
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
            var (graph, nodes) = GetRomaniaGraph();
            var problem = new FindingDirectionsProblem(graph, nodes["Arad"], nodes["Bucharest"]);
            return problem;
        }
        private (IWeightedGraph<FindingDirectionsState, double>, Dictionary<string, FindingDirectionsState>) GetRomaniaGraph()
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
            return (graph, nodes);
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
        public void TestFindingDirectionsAStarSearchLimaLandmarksHeuristic()
        {
            var filename = @"../../../../datasets/Lima/Lima.osm.pbf";
            var (graph, nodes) = OpenStreetMapDataHelper.ExtractMapGraph(filename);
            var nodeLocator = OpenStreetMapDataHelper.MakeNodeLocator(nodes);
            double latFrom = -12.073457334109781;
            double lonFrom = -77.16832519640246;
            double latTo = -12.045889755060621;
            double lonTo = -77.04266126523356;
            var problem = OpenStreetMapDataHelper.BuildMapProblem(graph, nodeLocator, latFrom, lonFrom, latTo, lonTo);
            var goal = problem.GetGoalState();
            // TODO: find a good way to choose landmarks
            var landmarks = new List<FindingDirectionsState>();
            landmarks.Add(nodeLocator.GetNearestNeighbours(new[] { -11.9997645637738,-77.08525616778775 }, 1)[0].Value);
            landmarks.Add(nodeLocator.GetNearestNeighbours(new[] { -12.073047431238026,-77.16825444354458 }, 1)[0].Value);
            //landmarks.Add(nodeLocator.GetNearestNeighbours(new[] { -12.190050763534796,-76.97454096014161 }, 1)[0].Value);
            //landmarks[3] = nodeLocator.GetNearestNeighbours(new[] { -12.0160677109035,-76.87767227591303 }, 1)[0].Value;
            output.WriteLine("Landmarks selected: " + landmarks.Count);
            foreach(var landmark in landmarks)
            {
                output.WriteLine("    " + landmark.ToString());
            }
            var heuristic = HeuristicsHelper.MakeLandmarksHeuristic(goal, graph, landmarks);
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            // real solution Final State (293 moves, 8.98118622895219 cost)
            // test solution Final State (361 moves, 10.7577504383769 cost)
            TestSolver(solver, problem, 8.98118622895219);
        }

        [Fact]
        public void TestFindingDirectionsAStarSearchLimaStraightLineDistanceHeuristic()
        {
            var filename = @"../../../../datasets/Lima/Lima.osm.pbf";
            // var problem = OpenStreetMapDataHelper.ExtractMapProblem(
            //         filename, 
            //         -12.073457334109781, -77.16832519640246,
            //         -12.045889755060621,-77.04266126523356
            //         // -12.041839850701912,-77.06330992167841,
            //         // -12.060138589193466,-77.01043821757685
            //     );
            var (graph, nodes) = OpenStreetMapDataHelper.ExtractMapGraph(filename);
            var nodeLocator = OpenStreetMapDataHelper.MakeNodeLocator(nodes);
            double latFrom = -12.073457334109781;
            double lonFrom = -77.16832519640246;
            double latTo = -12.045889755060621;
            double lonTo = -77.04266126523356;
            var problem = OpenStreetMapDataHelper.BuildMapProblem(graph, nodeLocator, latFrom, lonFrom, latTo, lonTo);
            var goal = problem.GetGoalState();
            var heuristic = HeuristicsHelper.MakeStraightLineDistanceHeuristic(goal);
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            TestSolver(solver, problem, 8.98118622895219);
        }

        [Fact]
        public void TestRomainaBuildShortestPathLookup()
        {
            var (graph, nodes) = GetRomaniaGraph();
            var names = nodes.ToDictionary(m => m.Value.NodeId, m => m.Value.LocationName);
            var bucharest = nodes["Bucharest"];
            var clookup = HeuristicsHelper.BuildShortestPathLookup(graph, nodes["Craiova"]);
            var olookup = HeuristicsHelper.BuildShortestPathLookup(graph, nodes["Oradea"]);
            //var dists = lookup.OrderBy(m => names[m.Key]);
            // foreach(var dist in dists)
            // {
            //     output.WriteLine(names[dist.Key] + "," + dist.Value);
            // }

            var l = new[] { nodes["Oradea"], nodes["Craiova"] };
            var h = HeuristicsHelper.MakeLandmarksHeuristic(bucharest, graph, l);
            Assert.Equal(239, clookup[bucharest.NodeId]);
            Assert.Equal(429, olookup[bucharest.NodeId]);
            Assert.Equal(358, h(nodes["Zerind"]));
            Assert.Equal(278, h(nodes["Sibiu"]));
            Assert.Equal(165, h(nodes["Timisoara"]));
        }
    }
}