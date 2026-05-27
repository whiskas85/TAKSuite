using System.Text;

namespace TAKSuite.TAK
{
    /// <summary>
    /// Decodes ATAK TakMessage protobuf (UDP multicast 6969) to CoT XML.
    /// Field numbers from official ATAK proto files (atakmap.commoncommo.protobuf.v1).
    /// No external NuGet — implements only the varint/64-bit/length-delimited wire types needed.
    /// </summary>
    public static class TakProtoDecoder
    {
        /// <summary>Returns CoT XML if data is a valid TakMessage protobuf, null otherwise.</summary>
        public static string? TryConvertToXml(byte[] data)
        {
            try
            {
                if (data.Length < 4) return null;

                var pos = 0;

                // ATAK UDP framing: every UDP multicast packet starts with [0xBF][0x01][0xBF]
                // followed by raw TakMessage protobuf bytes (no length prefix).
                if (data[0] == 0xBF && data.Length >= 3 && data[1] == 0x01 && data[2] == 0xBF)
                    pos = 3;
                else if (data[0] != 0x0a && data[0] != 0x12)
                    return null; // not a known protobuf packet

                if (pos >= data.Length) return null;

                // Sanity check: first byte of TakMessage must be a valid field tag
                var firstTag = data[pos];
                if (firstTag != 0x0a && firstTag != 0x12 && firstTag != 0x18 && firstTag != 0x20)
                    return null;

                byte[]? cotBytes = null;

                // Parse outer TakMessage — look for field 2 (cotEvent), skip everything else
                while (pos < data.Length)
                {
                    var (field, wire) = ReadTag(data, ref pos);
                    if (field == 2 && wire == 2)
                    {
                        cotBytes = ReadBytes(data, ref pos);
                        break;
                    }
                    Skip(data, ref pos, wire);
                }

                return cotBytes != null ? ParseCotEvent(cotBytes) : null;
            }
            catch { return null; }
        }

        // ── CotEvent parser ───────────────────────────────────────────────────
        // Field map (cotevent.proto):
        //   1=type, 2=access, 3=qos, 4=opex, 5=uid,
        //   6=sendTime(u64), 7=startTime(u64), 8=staleTime(u64), 9=how,
        //   10=lat, 11=lon, 12=hae, 13=ce, 14=le, 15=detail

        private static string? ParseCotEvent(byte[] data)
        {
            var pos = 0;
            string uid = "", type = "a-f-G-U-C", how = "m-g";
            double lat = 0, lon = 0, hae = 9999999;
            string? xmlDetail = null, callsign = null, team = null, role = null;

            while (pos < data.Length)
            {
                var (f, w) = ReadTag(data, ref pos);
                switch (f)
                {
                    case 1:  type = ReadString(data, ref pos); break;  // type
                    case 5:  uid  = ReadString(data, ref pos); break;  // uid
                    case 9:  how  = ReadString(data, ref pos); break;  // how
                    case 10: lat  = ReadDouble(data, ref pos); break;  // lat
                    case 11: lon  = ReadDouble(data, ref pos); break;  // lon
                    case 12: hae  = ReadDouble(data, ref pos); break;  // hae
                    case 15:
                        var detailBytes = ReadBytes(data, ref pos);
                        ParseDetail(detailBytes, out xmlDetail, out callsign, out team, out role);
                        break;
                    default: Skip(data, ref pos, w); break;
                }
            }

            if (string.IsNullOrEmpty(uid) || (lat == 0 && lon == 0)) return null;

            return BuildXml(uid, type, how, lat, lon, hae, xmlDetail, callsign, team, role);
        }

        // Field map (detail.proto):
        //   1=xmlDetail, 2=contact, 3=group, 4=precisionLocation, 5=status, 6=takv, 7=track

        private static void ParseDetail(byte[] data,
            out string? xmlDetail, out string? callsign, out string? team, out string? role)
        {
            xmlDetail = callsign = team = role = null;
            var pos = 0;

            while (pos < data.Length)
            {
                var (f, w) = ReadTag(data, ref pos);
                switch (f)
                {
                    case 1: // xmlDetail — raw XML string (inner children of <detail>)
                        xmlDetail = ReadString(data, ref pos);
                        break;
                    case 2: // contact sub-message (contact.proto: 1=endpoint, 2=callsign)
                        ParseContact(ReadBytes(data, ref pos), out callsign);
                        break;
                    case 3: // group (__group) sub-message (group.proto: 1=name, 2=role)
                        ParseGroup(ReadBytes(data, ref pos), out team, out role);
                        break;
                    default:
                        Skip(data, ref pos, w);
                        break;
                }
            }
        }

        private static void ParseContact(byte[] data, out string? callsign)
        {
            callsign = null;
            var pos = 0;
            while (pos < data.Length)
            {
                var (f, w) = ReadTag(data, ref pos);
                if (f == 2) callsign = ReadString(data, ref pos);  // callsign
                else        Skip(data, ref pos, w);
            }
        }

        private static void ParseGroup(byte[] data, out string? name, out string? role)
        {
            name = role = null;
            var pos = 0;
            while (pos < data.Length)
            {
                var (f, w) = ReadTag(data, ref pos);
                if      (f == 1) name = ReadString(data, ref pos);  // name
                else if (f == 2) role = ReadString(data, ref pos);  // role
                else             Skip(data, ref pos, w);
            }
        }

        // ── XML builder ───────────────────────────────────────────────────────

        private static string BuildXml(string uid, string type, string how,
            double lat, double lon, double hae,
            string? xmlDetail, string? callsign, string? team, string? role)
        {
            var now   = DateTime.UtcNow;
            var stale = now.AddMinutes(5);
            string Fmt(DateTime d) => d.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append($"<event version=\"2.0\" uid=\"{Esc(uid)}\" type=\"{Esc(type)}\" how=\"{Esc(how)}\"");
            sb.Append($" time=\"{Fmt(now)}\" start=\"{Fmt(now)}\" stale=\"{Fmt(stale)}\">");
            sb.Append($"<point lat=\"{lat.ToString("F6", inv)}\" lon=\"{lon.ToString("F6", inv)}\"");
            sb.Append($" hae=\"{hae.ToString("F1", inv)}\" ce=\"9999999.0\" le=\"9999999.0\"/>");
            sb.Append("<detail>");

            // xmlDetail contains inner children of <detail> (already stripped of outer tags)
            if (!string.IsNullOrEmpty(xmlDetail))
                sb.Append(xmlDetail);
            else if (!string.IsNullOrEmpty(callsign))
                sb.Append($"<contact callsign=\"{Esc(callsign)}\"/>");

            if (!string.IsNullOrEmpty(team) && (xmlDetail == null || !xmlDetail.Contains("__group")))
                sb.Append($"<__group name=\"{Esc(team)}\" role=\"{Esc(role ?? "Team Member")}\"/>");

            sb.Append("</detail></event>");
            return sb.ToString();
        }

        private static string Esc(string s) =>
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        // ── Protobuf primitives ───────────────────────────────────────────────

        private static (int field, int wireType) ReadTag(byte[] data, ref int pos)
        {
            var v = (int)ReadVarint(data, ref pos);
            return (v >> 3, v & 0x07);
        }

        private static ulong ReadVarint(byte[] data, ref int pos)
        {
            ulong result = 0;
            int shift = 0;
            while (pos < data.Length)
            {
                var b = data[pos++];
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }

        private static byte[] ReadBytes(byte[] data, ref int pos)
        {
            var len = (int)ReadVarint(data, ref pos);
            var result = new byte[len];
            Array.Copy(data, pos, result, 0, len);
            pos += len;
            return result;
        }

        private static string ReadString(byte[] data, ref int pos) =>
            Encoding.UTF8.GetString(ReadBytes(data, ref pos));

        private static double ReadDouble(byte[] data, ref int pos)
        {
            var d = BitConverter.ToDouble(data, pos);
            pos += 8;
            return d;
        }

        private static void Skip(byte[] data, ref int pos, int wireType)
        {
            switch (wireType)
            {
                case 0: ReadVarint(data, ref pos); break;   // varint
                case 1: pos += 8; break;                    // 64-bit
                case 2: ReadBytes(data, ref pos); break;    // length-delimited
                case 5: pos += 4; break;                    // 32-bit
            }
        }
    }
}
