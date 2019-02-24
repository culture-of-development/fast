using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using fast.helpers;
using fast.search;
using fast.search.problems;
using System.Diagnostics;
using KdTree;

namespace fast.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private static IWeightedGraph<FindingDirectionsState, double> mapGraph;
        private static INearestNeighbor<FindingDirectionsState> locations;

        public static void InitializeMapData(string city_name)
        {
            Console.WriteLine("Loading map data");
            var filename = Path.Combine($@"../datasets/{city_name}/{city_name}.osm.pbf");
            var (graph, nodes) = OpenStreetMapDataHelper.ExtractMapGraph(filename);
            MapController.mapGraph = graph;
            MapController.locations = OpenStreetMapDataHelper.MakeNodeLocator(nodes);
            Console.WriteLine("Map data loaded");
            Console.WriteLine("    Total map nodes: " + nodes.Count);
        }

        // GET api/map/{string}
        [HttpGet()]
        public IActionResult Show()
        {
            var filename = Path.Combine(Directory.GetCurrentDirectory(), "Static", $"map.html");
            return PhysicalFile(filename, "text/html");
        }

        [HttpGet("directions")]
        public ActionResult<DirectionsResult> GetDirections(double latFrom, double lonFrom, double latTo, double lonTo)
        {
            var timer = Stopwatch.StartNew();
            var problem = OpenStreetMapDataHelper.BuildMapProblem(mapGraph, locations, latFrom, lonFrom, latTo, lonTo);
            var goal = problem.GetGoalState();
            Func<FindingDirectionsState, double> heuristic = from => 
                DistanceHelper.Haversine(from.Latitude, from.Longitude, goal.Latitude, goal.Longitude);
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            var locationFinding = timer.Elapsed.TotalMilliseconds;
            timer = Stopwatch.StartNew();
            var steps = solver.Solve(problem)
                .Cast<FindingDirectionsState>()
                .Select(m => new LatLng { lat = m.Latitude, lng = m.Longitude })
                .ToArray();
            var itemsTime = timer.Elapsed.TotalMilliseconds;
            return new DirectionsResult { points = steps, problemDefineTime = locationFinding, directionsFindTime = itemsTime };
        }

        public class LatLng
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
        public class DirectionsResult
        {
            public IEnumerable<LatLng> points { get; set; }
            public double problemDefineTime { get; set; }
            public double directionsFindTime { get; set; }
        }
    }
}
