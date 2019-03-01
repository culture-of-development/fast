using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using fast.helpers;
using System.Collections.Generic;
using fast.search.problems;

namespace fast.search.tests
{
    public class OsmTests : TestsBase
    {
        public OsmTests(ITestOutputHelper output) : base(output) { }

        public const string LimaFilename = @"../../../../datasets/Lima/Lima.osm.pbf";
        public const string NewYorkFilename = @"../../../../datasets/NewYork/NewYork.osm.pbf";

        [Fact]
        public void TestReadLimaPeru()
        {
            var osmData = OpenStreetMapDataHelper.ViewAllFileData(LimaFilename);
            int i = 0;
            foreach(var item in osmData)
            {
                output.WriteLine(item);
                i++;
                if (i >= 10) break;
            }
        }

        [Fact]
        public void TestBuildGraphLimaPeru()
        {
            var lima = OpenStreetMapDataHelper.ExtractMapProblem(
                    LimaFilename, -12.073457334109781, -77.16832519640246,
                    -12.045889755060621,-77.04266126523356
                );
        }

        [Fact]
        public void TestBuildGraphNewYork()
        {
            var lima = OpenStreetMapDataHelper.ExtractMapProblem(
                    NewYorkFilename, -12.073457334109781, -77.16832519640246,
                    -12.045889755060621,-77.04266126523356
                );
        }

        [Fact]
        public void OsmEdgeDedupesCorrectly()
        {
            var from_a = new FindingDirectionsState(139489);
            var to_a = new FindingDirectionsState(198573);
            var from_b = new FindingDirectionsState(139489);
            var to_b = new FindingDirectionsState(198573);
            var from_c = new FindingDirectionsState(139489);
            var to_c = new FindingDirectionsState(298573);
            IWeightedGraphEdge<FindingDirectionsState, double> a = new OpenStreetMapDataHelper.OsmEdge(from_a, to_a, 0d);
            IWeightedGraphEdge<FindingDirectionsState, double> b = new OpenStreetMapDataHelper.OsmEdge(from_a, to_a, 1d);
            IWeightedGraphEdge<FindingDirectionsState, double> c = new OpenStreetMapDataHelper.OsmEdge(from_a, to_c, 3d);
            var hashSet = new HashSet<IWeightedGraphEdge<FindingDirectionsState, double>>();
            hashSet.Add(a);
            var actual = hashSet.Add(b);
            Assert.False(actual, "It did not dedupe correctly.");
            actual = hashSet.Add(c);
            Assert.True(actual, "Should have added that last point.");
            var graph = new AdjacencyListWeightedGraph<FindingDirectionsState, double>(hashSet);
            Assert.Equal(2, graph.EnumerateEdges().Count());
        }
    }
}