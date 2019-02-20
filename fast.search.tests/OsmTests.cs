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
    }
}