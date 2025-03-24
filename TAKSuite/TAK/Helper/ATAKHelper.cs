using System.Xml;

namespace TAKSuite.TAK.Helper
{
    public class ATAKHelper
    {
        // Lista dei colori predefiniti
        public static List<int> ATAKPredefinedColors =
        [
            -1, -256, -35072, -65281, -65536, -8454144, -8454017, -16777089, -16776961, -16711681, -16744577, -16711936, -16744704, -8947849, -16777216
        ];
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

