using System.Text.Json;
using System.Xml.Linq;

namespace TAKSuite.Data.ModelsTak
{
    public abstract class BaseCoT<T> where T : BaseCoT<T>
    {
        public string Uid { get; set; }
        public string? Callsign { get; set; }
        public string? CotType { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string CreatorUid { get; set; }
        public string ReporterCallsign { get; set; }


        internal BaseCoT(JsonElement uid)
        {
            Uid = uid.GetProperty("data").GetString();
            LastUpdateTime = uid.GetProperty("timestamp").GetDateTime();
            Callsign = uid.GetProperty("details").GetProperty("callsign").GetString();
            CotType = uid.GetProperty("details").GetProperty("type").GetString();
            Lat = uid.GetProperty("details").GetProperty("location").GetProperty("lat").GetDouble();
            Lon = uid.GetProperty("details").GetProperty("location").GetProperty("lon").GetDouble();

            CreatorUid = uid.GetProperty("creatorUid").GetString();
        }

        public static T Parse(JsonElement uid)
        {
            return (T)Activator.CreateInstance(typeof(T), uid)!;

        }

        public void IntegrateUserInformationFromXml(string xmlContent)
        {
            try
            {
                XDocument xmlDoc = XDocument.Parse(xmlContent);
                XElement contactElement = xmlDoc.Descendants("contact").FirstOrDefault();

                if (contactElement != null && contactElement.Attribute("callsign") != null)
                {
                    ReporterCallsign = contactElement.Attribute("callsign").Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante la lettura del file XML: " + ex.Message);
            }

        }
    }
}
