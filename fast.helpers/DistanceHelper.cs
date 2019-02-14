using System;
using System.Runtime.CompilerServices;

namespace fast.helpers
{
    public static class DistanceHelper
    {
        public const double EarthRadiusInMiles = 3963.1676;

        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            // https://www.movable-type.co.uk/scripts/latlong.html
            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);
            var latDiff = lat2 - lat1;
            var lonDiff = DegreesToRadians(lon2 - lon1);

            var h = Math.Pow(Math.Sin(latDiff / 2d), 2d) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Pow(Math.Sin(lonDiff / 2d), 2d);

            var distance = EarthRadiusInMiles * 2d * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1-h));
            return distance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180d;
        }
    }
}