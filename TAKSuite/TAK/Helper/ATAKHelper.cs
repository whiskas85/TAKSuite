using System.Globalization;
using System.Xml;
using TAKSuite.Helper;

namespace TAKSuite.TAK.Helper
{
    public enum PoiType
    {
        Spotmap,
        Obiettivo,
        PuntoGenerico
    }
    public class PredefinedColor
    {
        public string Name { get; set; }
        public int Argb { get; set; }

        public string ToHex()
        {
            return ColorConverterHelper.ArgbToHex(Argb);
        }
    }
    public class ATAKHelper
    {
        // Lista dei colori predefiniti con nome
        public static List<PredefinedColor> ATAKPredefinedColors = new List<PredefinedColor>
        {
            new PredefinedColor { Name = "White", Argb = -1 },                   // Transparent / None
            new PredefinedColor { Name = "Yellow", Argb = -256 },               // Yellow
            new PredefinedColor { Name = "Orange", Argb = -35072 },             // Orange
            new PredefinedColor { Name = "Magenta", Argb = -65281 },            // Magenta
            new PredefinedColor { Name = "Red", Argb = -65536 },                // Red
            new PredefinedColor { Name = "Maroon", Argb = -8454144 },            // Brown
            new PredefinedColor { Name = "Purple", Argb = -8454017 },             // Light Pink
            new PredefinedColor { Name = "Dark Blue", Argb = -16777089 },            // Cyan
            new PredefinedColor { Name = "Blue", Argb = -16776961 },            // Blue
            new PredefinedColor { Name = "Cyan", Argb = -16711681 },      // Aqua / Light Blue
            new PredefinedColor { Name = "Teal", Argb = -16744577 },       // Turquoise
            new PredefinedColor { Name = "Green", Argb = -16711936 },           // Green
            new PredefinedColor { Name = "Dark Green", Argb = -16744704 },            // Lime Green
            new PredefinedColor { Name = "Grey", Argb = -8947849 },             // Gray
            new PredefinedColor { Name = "Black", Argb = -16777216 }            // Black
        };
        public const int COLOR_WAYPOINT_COMPLETED = -8947849;
        public static void ChangeSpotmapColor(XmlDocument doc, int newArgbValue)
        {
            String _argbValue = "argb";
            String _parentElement = "color";

            XmlNode detailNode = doc.SelectSingleNode("//detail") ?? throw new Exception("Nodo <detail> non trovato nel documento XML.");
            XmlNode colorNode = detailNode.SelectSingleNode(_parentElement);

            if (colorNode == null)
            {
                // Se non esiste, creiamo il nodo "<color>" e lo aggiungiamo a <detail>
                colorNode = doc.CreateElement(_parentElement);
                XmlAttribute argbAttribute = doc.CreateAttribute(_argbValue);
                argbAttribute.Value = newArgbValue.ToString();
                colorNode.Attributes.Append(argbAttribute);
                detailNode.AppendChild(colorNode);
            }
            else
            {
                // Se esiste già, aggiorniamo il valore dell'attributo "argb"
                XmlAttribute argbAttribute = colorNode.Attributes[_argbValue];
                if (argbAttribute == null)
                {
                    // Se manca l'attributo, lo creiamo
                    argbAttribute = doc.CreateAttribute(_argbValue);
                    colorNode.Attributes.Append(argbAttribute);
                }
                argbAttribute.Value = newArgbValue.ToString();
            }
        }
        public static XmlDocument ChangeSpotmapColor(string xml, int newArgbValue)
        {
            XmlDocument doc = new();
            doc.LoadXml(xml);
            ChangeSpotmapColor(doc, newArgbValue);
            return doc;
        }

        public static void RefreshTimestamps(XmlDocument doc, TimeSpan? staleWindow = null)
        {
            var evt = doc.DocumentElement;
            if (evt == null) return;
            var now   = DateTime.UtcNow;
            var stale = now.Add(staleWindow ?? TimeSpan.FromDays(365));
            var fmt   = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
            evt.SetAttribute("time",  now.ToString(fmt));
            evt.SetAttribute("start", now.ToString(fmt));
            evt.SetAttribute("stale", stale.ToString(fmt));
        }

        public static string GetCotType(PoiType poiType) => poiType switch
        {
            PoiType.Spotmap => "b-m-p-s-m",
            PoiType.Obiettivo => "b-m-p-w-GOTO",
            PoiType.PuntoGenerico => "a-u-G",
            _ => "b-m-p-s-m"
        };

        public static XmlDocument CreatePoiCoT(string uid, string callsign, PoiType poiType,
            double lat, double lon, string? remarks = null, int? argbColor = null, double hae = 0)
        {
            var now = DateTime.UtcNow;
            var stale = now.AddDays(365);
            var cotType = GetCotType(poiType);

            var doc = new XmlDocument();
            var evt = doc.CreateElement("event");
            evt.SetAttribute("version", "2.0");
            evt.SetAttribute("uid", uid);
            evt.SetAttribute("type", cotType);
            evt.SetAttribute("time", now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
            evt.SetAttribute("start", now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
            evt.SetAttribute("stale", stale.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
            evt.SetAttribute("how", "h-g-i-g-o");
            doc.AppendChild(evt);

            var point = doc.CreateElement("point");
            point.SetAttribute("lat", lat.ToString("G", CultureInfo.InvariantCulture));
            point.SetAttribute("lon", lon.ToString("G", CultureInfo.InvariantCulture));
            point.SetAttribute("hae", hae.ToString("G", CultureInfo.InvariantCulture));
            point.SetAttribute("ce", "9999999.0");
            point.SetAttribute("le", "9999999.0");
            evt.AppendChild(point);

            var detail = doc.CreateElement("detail");
            evt.AppendChild(detail);

            var contact = doc.CreateElement("contact");
            contact.SetAttribute("callsign", callsign);
            detail.AppendChild(contact);

            var status = doc.CreateElement("status");
            status.SetAttribute("readiness", "true");
            detail.AppendChild(status);

            detail.AppendChild(doc.CreateElement("archive"));

            var colorArgb = argbColor ?? -1;
            var colorEl = doc.CreateElement("color");
            colorEl.SetAttribute("argb", colorArgb.ToString());
            detail.AppendChild(colorEl);

            if (!string.IsNullOrWhiteSpace(remarks))
            {
                var remarksEl = doc.CreateElement("remarks");
                remarksEl.InnerText = remarks;
                detail.AppendChild(remarksEl);
            }

            if (poiType == PoiType.Spotmap)
            {
                var usericon = doc.CreateElement("usericon");
                usericon.SetAttribute("iconsetpath", $"COT_MAPPING_SPOTMAP/{cotType}/{colorArgb}");
                detail.AppendChild(usericon);
            }

            return doc;
        }

        internal static void CleanFlowTags(XmlDocument doc)
        {
            // _flow-tags_ is injected by ATAK clients and must be stripped before re-sending CoT
            var node = doc.SelectSingleNode("//_flow-tags_");
            node?.ParentNode?.RemoveChild(node);
        }
    }

}

