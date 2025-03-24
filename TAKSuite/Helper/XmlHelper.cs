using System.Xml;
using System.Xml.Linq;

namespace TAKSuite.Helper
{
    public class XmlHelper
    {
        public static string FormatXml(string xml)
        {
            if (String.IsNullOrEmpty(xml)) return "";

            try
            {
                XmlDocument doc = new();
                doc.LoadXml(xml);

                return FormatXml(doc);
            }
            catch (Exception ex)
            {
                return $"Errore durante l'indentazione XML: {ex.Message}";
            }
        }
        public static string FormatXml(XmlDocument doc)
        {
            try
            {
                using StringWriter stringWriter = new();
                using XmlTextWriter xmlTextWriter = new(stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.Indentation = 4;
                doc.WriteContentTo(xmlTextWriter);
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                return $"Errore durante l'indentazione XML: {ex.Message}";
            }
        }

        public static string FormatXmlPlain(string xml)
        {
            if (String.IsNullOrEmpty(xml)) return "";

            try
            {
                XmlDocument doc = new();
                doc.LoadXml(xml);

                return FormatXmlPlain(doc);
            }
            catch (Exception ex)
            {
                return $"Errore durante l'indentazione XML: {ex.Message}";
            }
        }
        public static string FormatXmlPlain(XmlDocument doc)
        {
            try
            {
                using StringWriter stringWriter = new();
                using XmlTextWriter xmlTextWriter = new(stringWriter);
                xmlTextWriter.Formatting = Formatting.None;
                doc.WriteContentTo(xmlTextWriter);
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                return $"Errore durante l'indentazione XML: {ex.Message}";
            }
        }
    }
}
