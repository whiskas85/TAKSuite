using TAKSuite.Data.ModelsTak;

namespace TAKSuite.TAK.Helper
{
    public class GeoUtils
    {
        private const double EarthRadius = 6371000; // Raggio della Terra in metri
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <param name="threshold">La distanza in metri</param>
        /// <returns></returns>
        public static bool ArePointsClose(double lat1, double lon1, double lat2, double lon2, double threshold = 10.0)
        {
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = EarthRadius * c;

            return distance < threshold;
        }
        public static bool ArePointsClose<X,Y>(BaseCoT<X> pt, BaseCoT<Y>pt2, double threshold = 10.0) where X : BaseCoT<X> where Y : BaseCoT<Y>
        {
            return ArePointsClose(pt.Lat.Value, pt.Lon.Value, pt2.Lat.Value, pt2.Lon.Value, threshold);
        }

        public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return EarthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
