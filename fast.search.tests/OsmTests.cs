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

        [Fact]
        public void TestReadLimaPeru()
        {
            var limaFilename = @"I:\culture-of-development\fast\datasets\lima-peru-osm\Lima.osm.pbf";
            var osmData = OpenStreetMapDataHelper.ViewAllFileData(limaFilename);
            int i = 0;
            foreach(var item in osmData)
            {
                output.WriteLine(item);
                i++;
                if (i >= 1000) break;
            }
        }

        [Fact]
        public void TestBuildGraphLimaPeru()
        {
            var limaFilename = @"I:\culture-of-development\fast\datasets\lima-peru-osm\Lima.osm.pbf";
            var lima = OpenStreetMapDataHelper.ExtractMapProblem(
                    limaFilename, -12.073457334109781, -77.16832519640246,
                    -12.045889755060621,-77.04266126523356
                );
        }
    }
}