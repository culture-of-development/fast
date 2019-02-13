using System;
using System.Collections.Generic;
using System.IO;
using OsmSharp.Streams;

namespace fast.helpers
{
    public static class OpenStreetMapDataHelper
    {
        public static IEnumerable<string> ViewAllFileData(string filename)
        {
            using(var fileStream = File.OpenRead(filename))
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source)
                {
                    yield return element.ToString();
                }
            }
        }
    }
}
