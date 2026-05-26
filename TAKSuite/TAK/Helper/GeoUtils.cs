using TAKSuite.Data.ModelsTak;

namespace TAKSuite.TAK.Helper
{
    public class GeoUtils
    {
        // ── Coordinate parsing & UTM conversion ──────────────────────────────

        /// <summary>
        /// Converts UTM WGS84 to decimal lat/lon. isNorth = true for northern hemisphere.
        /// </summary>
        public static (double lat, double lon) UtmToLatLon(int zone, bool isNorth, double easting, double northing)
        {
            const double a  = 6378137.0;
            const double e2 = 0.00669437999014;
            const double k0 = 0.9996;
            const double E0 = 500000.0;

            double N0 = isNorth ? 0.0 : 10000000.0;
            double x   = easting  - E0;
            double y   = northing - N0;
            double lon0 = ((zone - 1) * 6.0 - 180.0 + 3.0) * Math.PI / 180.0;
            double eP2  = e2 / (1.0 - e2);
            double M    = y / k0;
            double mu   = M / (a * (1.0 - e2 / 4.0 - 3.0 * e2 * e2 / 64.0 - 5.0 * e2 * e2 * e2 / 256.0));
            double e1   = (1.0 - Math.Sqrt(1.0 - e2)) / (1.0 + Math.Sqrt(1.0 - e2));

            double phi1 = mu
                + (3.0 * e1 / 2.0          - 27.0 * Math.Pow(e1, 3) / 32.0) * Math.Sin(2.0 * mu)
                + (21.0 * e1 * e1 / 16.0   - 55.0 * Math.Pow(e1, 4) / 32.0) * Math.Sin(4.0 * mu)
                + (151.0 * Math.Pow(e1, 3) / 96.0)                           * Math.Sin(6.0 * mu)
                + (1097.0 * Math.Pow(e1, 4) / 512.0)                         * Math.Sin(8.0 * mu);

            double N1  = a / Math.Sqrt(1.0 - e2 * Math.Sin(phi1) * Math.Sin(phi1));
            double T1  = Math.Tan(phi1) * Math.Tan(phi1);
            double C1  = eP2 * Math.Cos(phi1) * Math.Cos(phi1);
            double R1  = a * (1.0 - e2) / Math.Pow(1.0 - e2 * Math.Sin(phi1) * Math.Sin(phi1), 1.5);
            double D   = x / (N1 * k0);

            double lat = phi1 - (N1 * Math.Tan(phi1) / R1)
                * (Math.Pow(D, 2) / 2.0
                 - (5.0 + 3.0 * T1 + 10.0 * C1 - 4.0 * C1 * C1 - 9.0 * eP2) * Math.Pow(D, 4) / 24.0
                 + (61.0 + 90.0 * T1 + 298.0 * C1 + 45.0 * T1 * T1 - 252.0 * eP2 - 3.0 * C1 * C1) * Math.Pow(D, 6) / 720.0);

            double lon = (D
                - (1.0 + 2.0 * T1 + C1) * Math.Pow(D, 3) / 6.0
                + (5.0 - 2.0 * C1 + 28.0 * T1 - 3.0 * C1 * C1 + 8.0 * eP2 + 24.0 * T1 * T1) * Math.Pow(D, 5) / 120.0)
                / Math.Cos(phi1);

            return (lat * 180.0 / Math.PI, (lon0 + lon) * 180.0 / Math.PI);
        }

        private const string UtmBands = "CDEFGHJKLMNPQRSTUVWX";

        /// <summary>
        /// Converts WGS84 decimal lat/lon to UTM zone, band letter, easting, northing.
        /// </summary>
        public static (int zone, char band, double easting, double northing) LatLonToUtm(double lat, double lon)
        {
            const double a  = 6378137.0;
            const double e2 = 0.00669437999014;
            const double k0 = 0.9996;
            const double E0 = 500000.0;

            int zone   = (int)Math.Floor((lon + 180.0) / 6.0) + 1;
            double lon0 = ((zone - 1) * 6.0 - 180.0 + 3.0) * Math.PI / 180.0;
            double latR = lat * Math.PI / 180.0;
            double lonR = lon * Math.PI / 180.0;

            double eP2 = e2 / (1.0 - e2);
            double N   = a / Math.Sqrt(1.0 - e2 * Math.Sin(latR) * Math.Sin(latR));
            double T   = Math.Tan(latR) * Math.Tan(latR);
            double C   = eP2 * Math.Cos(latR) * Math.Cos(latR);
            double A   = Math.Cos(latR) * (lonR - lon0);
            double M   = a * ((1.0 - e2/4 - 3*e2*e2/64 - 5*e2*e2*e2/256) * latR
                             - (3*e2/8 + 3*e2*e2/32 + 45*e2*e2*e2/1024) * Math.Sin(2*latR)
                             + (15*e2*e2/256 + 45*e2*e2*e2/1024) * Math.Sin(4*latR)
                             - (35*e2*e2*e2/3072) * Math.Sin(6*latR));

            double easting = k0 * N * (A
                + (1 - T + C) * Math.Pow(A, 3) / 6.0
                + (5 - 18*T + T*T + 72*C - 58*eP2) * Math.Pow(A, 5) / 120.0) + E0;

            double northing = k0 * (M + N * Math.Tan(latR)
                * (A*A / 2.0
                 + (5 - T + 9*C + 4*C*C) * Math.Pow(A, 4) / 24.0
                 + (61 - 58*T + T*T + 600*C - 330*eP2) * Math.Pow(A, 6) / 720.0));
            if (lat < 0) northing += 10_000_000.0;

            int bi = Math.Clamp((int)Math.Floor((lat + 80) / 8), 0, 19);
            return (zone, UtmBands[bi], easting, northing);
        }

        /// <summary>
        /// Tries to parse a raw coordinate string (UTM or decimal lat/lon) into WGS84 decimal lat/lon.
        /// Handles: "32T 373313 4992906", "32T 452390E 4520000N", "45.123, 8.456", "45.123N 8.456E".
        /// </summary>
        public static bool TryParseCoordinate(string raw, out double lat, out double lon)
        {
            lat = 0; lon = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            if (TryParseUtm(raw.Trim(), out lat, out lon))    return true;
            if (TryParseLatLon(raw.Trim(), out lat, out lon)) return true;
            return false;
        }

        private static bool TryParseUtm(string raw, out double lat, out double lon)
        {
            lat = 0; lon = 0;
            var parts = raw.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return false;

            var zonePart = parts[0];
            if (zonePart.Length < 2) return false;
            char band = char.ToUpper(zonePart[^1]);
            if (!char.IsLetter(band) || !UtmBands.Contains(band)) return false;
            if (!int.TryParse(zonePart[..^1], out int zone) || zone < 1 || zone > 60) return false;

            // Strip optional E/N suffixes
            string eStr = parts[1].TrimEnd('E', 'e');
            string nStr = parts[2].TrimEnd('N', 'n');

            if (!double.TryParse(eStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double easting))  return false;
            if (!double.TryParse(nStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double northing)) return false;

            bool isNorth = band >= 'N';
            (lat, lon) = UtmToLatLon(zone, isNorth, easting, northing);
            return true;
        }

        private static bool TryParseLatLon(string raw, out double lat, out double lon)
        {
            lat = 0; lon = 0;
            // Split on comma/semicolon, or on space if two tokens
            var parts = raw.Split(new[] { ',', ';' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                parts = raw.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            return ParseLatLonParts(parts[0].Trim(), parts[1].Trim(), out lat, out lon);
        }

        private static bool ParseLatLonParts(string latStr, string lonStr, out double lat, out double lon)
        {
            lat = 0; lon = 0;
            bool south = latStr.EndsWith("S", StringComparison.OrdinalIgnoreCase);
            bool north = latStr.EndsWith("N", StringComparison.OrdinalIgnoreCase);
            if (south || north) latStr = latStr[..^1].TrimEnd('°', ' ');

            bool west = lonStr.EndsWith("W", StringComparison.OrdinalIgnoreCase);
            bool east = lonStr.EndsWith("E", StringComparison.OrdinalIgnoreCase);
            if (west || east) lonStr = lonStr[..^1].TrimEnd('°', ' ');

            if (!double.TryParse(latStr.TrimEnd('°'), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out lat)) return false;
            if (!double.TryParse(lonStr.TrimEnd('°'), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out lon)) return false;

            if (south) lat = -lat;
            if (west)  lon = -lon;
            return lat is >= -90 and <= 90 && lon is >= -180 and <= 180;
        }
        private const double EarthRadius = 6371000;

        // Returns true if the haversine distance between the two points is within threshold metres
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
