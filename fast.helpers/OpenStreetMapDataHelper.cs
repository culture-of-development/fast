using System;
using System.Collections.Generic;
using System.IO;
using fast.search;
using fast.search.problems;
using OsmSharp;
using OsmSharp.Streams;
using KdTree;
using KdTree.Math;

namespace fast.helpers
{
    public static class OpenStreetMapDataHelper
    {
        public static IEnumerable<string> ViewAllFileData(string osm_pbf_filename)
        {
            using(var fileStream = File.OpenRead(osm_pbf_filename))
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source)
                {
                    yield return element.ToString();
                }
            }
        }

        public static FindingDirectionsProblem ExtractMapProblem(
            string osm_pbf_filename, 
            double latFrom, double lonFrom, 
            double latTo, double lonTo
        )
        {
            var (graph, nodes) = ExtractMapGraph(osm_pbf_filename);
            var nodeLocator = new KdTreeNearestNeighbors(nodes);
            return BuildMapProblem(graph, nodeLocator, latFrom, lonFrom, latTo, lonTo);
        }
        public static INearestNeighbor<FindingDirectionsState> MakeNodeLocator(HashSet<FindingDirectionsState> nodes)
        {
            return new KdTreeNearestNeighbors(nodes);
        }
        private class KdTreeNearestNeighbors : INearestNeighbor<FindingDirectionsState>
        {
            private KdTree<double, FindingDirectionsState> tree;

            public KdTreeNearestNeighbors(HashSet<FindingDirectionsState> nodes)
            {
                tree = new KdTree<double, FindingDirectionsState>(2, new DoubleGeoMath());
                foreach(var node in nodes)
                {
                    tree.Add(new[] { node.Latitude, node.Longitude }, node);
                }
            }
            public FindingDirectionsState FindNearestNeighbor(FindingDirectionsState source)
            {
                double lat = source.Latitude;
                double lon = source.Longitude;
                var nearest = tree.GetNearestNeighbours(new[] { lat, lon }, 1)[0].Value;
                return nearest;
            }
        }
        [Serializable]
        public class DoubleGeoMath : DoubleMath
        {
            public override double DistanceSquaredBetweenPoints(double[] a, double[] b)
            {
                double dst = DistanceHelper.Haversine(a[0], a[1], b[0], b[1]);
                return (float)(dst * dst);
            }
        }

        public static FindingDirectionsProblem BuildMapProblem(
            IWeightedGraph<FindingDirectionsState, double> graph, 
            // TODO: swap this out for something that isn't a dependency
            INearestNeighbor<FindingDirectionsState> nodeLocator,
            double latFrom, double lonFrom, 
            double latTo, double lonTo
        )
        {
            var from = nodeLocator.FindNearestNeighbor(new FindingDirectionsState(0, null, latFrom, lonFrom));
            var to = nodeLocator.FindNearestNeighbor(new FindingDirectionsState(0, null, latTo, lonTo));
            var problem = new FindingDirectionsProblem(graph, from, to);
            return problem;
        }

        // TODO: make this look like an actual function signature
        public static
            (IWeightedGraph<FindingDirectionsState, double> graph, HashSet<FindingDirectionsState> nodes)
            ExtractMapGraph(string osm_pbf_filename)
        {
            var recreationalVechicleRoadTags = new HashSet<string>(
                new[] {
                    // https://wiki.openstreetmap.org/wiki/Key:highway
                    "motorway",
                    "trunk",
                    "primary",
                    "secondary",
                    "tertiary",
                    "unclassified",
                    "residential",
                    "service",
                    // ramps and other road connectors
                    "motorway_link",
                    "trunk_link",
                    "primary_link",
                    "secondary_link",
                    "tertiary_link",
                }
            );
            var onewayIdentifiers = new HashSet<string>(new [] { "yes", "1", "true" });
            var nodes = new Dictionary<long, FindingDirectionsState>();
            var edges = new List<IWeightedGraphEdge<FindingDirectionsState, double>>();
            var graphNodes = new HashSet<FindingDirectionsState>();
            using(var fileStream = File.OpenRead(osm_pbf_filename))
            using(var source = new PBFOsmStreamSource(fileStream))
            foreach (var element in source)
            {
                switch(element.Type)
                {
                    case OsmGeoType.Node:
                        if (!element.Id.HasValue) continue;
                        var osm_node = (Node)element;
                        if (!osm_node.Latitude.HasValue || !osm_node.Longitude.HasValue) continue;
                        var graph_node = new FindingDirectionsState(
                                nodeId: (ulong)osm_node.Id.Value, 
                                latitude: osm_node.Latitude.Value,
                                longitude: osm_node.Longitude.Value
                            );
                        nodes[element.Id.Value] = graph_node;
                        break;
                    case OsmGeoType.Way:
                        if (!element.Tags.ContainsKey("highway")) continue;
                        var highwayType = element.Tags.GetValue("highway");
                        if (!recreationalVechicleRoadTags.Contains(highwayType)) continue;
                        var way = (Way)element;
                        if (way.Nodes.Length < 2) continue;
                        bool isOneWay = element.Tags.ContainsKey("oneway") && onewayIdentifiers.Contains(element.Tags["oneway"]);
                        for(int i = 1; i < way.Nodes.Length; i++)
                        {
                            var a = nodes[way.Nodes[i-1]];
                            var b = nodes[way.Nodes[i]];
                            graphNodes.Add(a);
                            graphNodes.Add(b);
                            var distance = DistanceHelper.Haversine(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
                            edges.Add(new OsmEdge(a, b, distance));
                            if (!isOneWay) edges.Add(new OsmEdge(b, a, distance));
                        }
                        break;
                    case OsmGeoType.Relation:
                        // don't need these?
                        break;
                }
            }
            var graph = new AdjacencyListWeightedGraph<FindingDirectionsState, double>(edges);
            return (graph, graphNodes);
        }

        private class OsmEdge : IWeightedGraphEdge<FindingDirectionsState, double>
        {
            public FindingDirectionsState From { get; private set; }
            public FindingDirectionsState To { get; private set; }
            public double Weight { get; private set; }

            public OsmEdge(FindingDirectionsState from, FindingDirectionsState to, double weight)
            {
                this.From = from;
                this.To = to;
                this.Weight = weight;
            }

            public override string ToString()
            {
                return $"({From.Latitude}, {From.Longitude}, {To.Latitude}, {To.Longitude})";
            }
        }
    }
}
