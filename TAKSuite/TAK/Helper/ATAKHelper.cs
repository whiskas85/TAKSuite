using System.Xml;
using TAKSuite.Helper;

namespace TAKSuite.TAK.Helper
{
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

        internal static void CleanFlowTags(XmlDocument doc)
        {
            // Seleziona il nodo '_flow-tags_'
            XmlNode? flowTagsNode = doc.SelectSingleNode("//_flow-tags_");

            // Rimuovi il nodo se trovato
            if (flowTagsNode != null)
            {
                flowTagsNode.ParentNode?.RemoveChild(flowTagsNode);
                Console.WriteLine("Elemento '_flow-tags_' rimosso.");
            }
        }
    }

}

