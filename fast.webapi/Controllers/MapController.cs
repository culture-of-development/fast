using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using fast.helpers;
using fast.search;
using fast.search.problems;

namespace fast.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private static IWeightedGraph<FindingDirectionsState, double> mapGraph;
        private static HashSet<FindingDirectionsState> locations;

        public static void InitializeMapData()
        {
            Console.WriteLine("Loading map data");
            var filename = @"I:\culture-of-development\fast\datasets\lima-peru-osm\Lima.osm.pbf";
            var (graph, nodes) = OpenStreetMapDataHelper.ExtractMapGraph(filename);
            MapController.mapGraph = graph;
            MapController.locations = nodes;
            Console.WriteLine("Map data loaded");
        }

        // GET api/map/{string}
        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            var filename = Path.Combine(Directory.GetCurrentDirectory(), "Static", $"map-{name}.html");
            return PhysicalFile(filename, "text/html");
        }

        [HttpGet("directions")]
        public ActionResult<LatLng[]> GetDirections(double latFrom, double lonFrom, double latTo, double lonTo)
        {
            var problem = OpenStreetMapDataHelper.BuildMapProblem(mapGraph, locations, latFrom, lonFrom, latTo, lonTo);
            var goal = problem.GetGoalState();
            Func<FindingDirectionsState, double> heuristic = from => 
                DistanceHelper.Haversine(from.Latitude, from.Longitude, goal.Latitude, goal.Longitude);
            var solver = new AStarSearchSolver<FindingDirectionsState>(heuristic);
            var steps = solver.Solve(problem)
                .Cast<FindingDirectionsState>()
                .Select(m => new LatLng { lat = m.Latitude, lng = m.Longitude })
                .ToArray();
            return steps;
        }

        public class LatLng
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
    }
}
