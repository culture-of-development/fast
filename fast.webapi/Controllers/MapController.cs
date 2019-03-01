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
        private static FindingDirectionsState[] landmarks;
        private static Func<FindingDirectionsState, FindingDirectionsState, double> double_heuristic;
        private static long nodeCount;
        private static LatLng bbNorthWest;
        private static LatLng bbSouthEast;

        public static void InitializeMapData(string city_name)
        {
            Log.WriteLine("Loading map data: " + city_name);
            var filename = Path.Combine($@"../datasets/{city_name}/{city_name}.osm.pbf");
            var (graph, nodes) = OpenStreetMapDataHelper.ExtractMapGraph(filename);
            MapController.mapGraph = graph;
            MapController.locations = OpenStreetMapDataHelper.MakeNodeLocator(nodes);
            Log.WriteLine("Calculating heuristic");
            //MapController.double_heuristic = (from, goal) => DistanceHelper.Haversine(from.Latitude, from.Longitude, goal.Latitude, goal.Longitude);
            var random = new Random();
            // TODO: find a better way to choose landmarks
            MapController.landmarks = nodes.OrderBy(m => random.Next()).Take(4).ToArray();
            MapController.double_heuristic = HeuristicsHelper.MakeLandmarksHeuristicDouble(mapGraph, MapController.landmarks);
            MapController.nodeCount = nodes.Count;
            Log.WriteLine("Calculating bounding box");
            foreach(var node in nodes)
            {
                if (bbNorthWest == null) bbNorthWest = new LatLng { lat = node.Latitude, lng = node.Longitude };
                bbNorthWest.lat = Math.Min(bbNorthWest.lat, node.Latitude);
                bbNorthWest.lng = Math.Min(bbNorthWest.lng, node.Longitude);
                if (bbSouthEast == null) bbSouthEast = new LatLng { lat = node.Latitude, lng = node.Longitude };
                bbSouthEast.lat = Math.Max(bbSouthEast.lat, node.Latitude);
                bbSouthEast.lng = Math.Max(bbSouthEast.lng, node.Longitude);
            }
            Log.WriteLine("Map data loaded: " + city_name);
            Log.WriteLine("    Total map nodes: " + nodes.Count);
        }

        // GET api/map
        [HttpGet()]
        public IActionResult Show()
        {
            var filename = Path.Combine(Directory.GetCurrentDirectory(), "Static", $"map.html");
            return PhysicalFile(filename, "text/html");
        }

        [HttpGet("get-landmarks")]
        public ActionResult<LatLng[]> GetLandmarks()
        {
            return landmarks.Select(m => new LatLng { lat = m.Latitude, lng = m.Longitude }).ToArray();
        }

        [HttpGet("info")]
        public ActionResult<object> GetMapInfo()
        {
            return new {
                nodeCount,
                bbNorthWest,
                bbSouthEast,
            };
        }

        // GET api/map/directions
        [HttpGet("directions")]
        public ActionResult<DirectionsResult> GetDirections(double latFrom, double lonFrom, double latTo, double lonTo)
        {
            var timer = Stopwatch.StartNew();
            var problem = OpenStreetMapDataHelper.BuildMapProblem(mapGraph, locations, latFrom, lonFrom, latTo, lonTo);
            var goal = problem.GetGoalState();
            Func<FindingDirectionsState, double> heuristic = from => double_heuristic(from, goal);
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
