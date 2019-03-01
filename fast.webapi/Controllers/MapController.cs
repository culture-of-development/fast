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
        private static IWeightedGraph<FindingDirectionsState, IEnumerable<FindingDirectionsState>> segmentsGraph;
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
            //var (graph, nodes) = OpenStreetMapDataHelper.ExtractMapGraph(filename);
            var (graph, segments, nodes) = OpenStreetMapDataHelper.ExtractIntersectionMapGraph(filename);
            MapController.mapGraph = graph;
            MapController.segmentsGraph = segments;
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

        [HttpGet("info")]
        public ActionResult<object> GetMapInfo()
        {
            return new {
                nodeCount,
                bbNorthWest,
                bbSouthEast,
                landmarks = landmarks?.Select(m => new LatLng { lat = m.Latitude, lng = m.Longitude }).ToArray(),
            };
        }

        // GET api/map/directions
        [HttpGet("directions")]
        public ActionResult<object> GetDirections(double latFrom, double lonFrom, double latTo, double lonTo)
        {
            var timer = Stopwatch.StartNew();
            var problem = OpenStreetMapDataHelper.BuildMapProblem(mapGraph, locations, latFrom, lonFrom, latTo, lonTo);
            var goal = problem.GetGoalState();
            Func<FindingDirectionsState, double> heuristic = from => double_heuristic(from, goal);
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            var locationFinding = timer.Elapsed.TotalMilliseconds;
            timer = Stopwatch.StartNew();
            var solution = solver.Solve(problem).Cast<FindingDirectionsState>().ToArray();
            List<LatLng> steps = null;
            if (MapController.segmentsGraph == null)
            {
                steps = new List<LatLng>();
                for(int i = 1; i < solution.Length; i++)
                {
                    var parts = MapController.segmentsGraph.GetEdgeWeight(solution[i-1], solution[i]);
                    steps.AddRange(parts.Select(m => new LatLng { lat = m.Latitude, lng = m.Longitude }));
                }
            }
            else
            {
                steps = solution
                    .Select(m => new LatLng { lat = m.Latitude, lng = m.Longitude })
                    .ToList();
            }
            var itemsTime = timer.Elapsed.TotalMilliseconds;
            return new { 
                points = steps, 
                problemDefineTime = locationFinding, 
                directionsFindTime = itemsTime,
                statesEvaluated = solver.StatesEvaluated,
            };
        }

        public class LatLng
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
    }
}
