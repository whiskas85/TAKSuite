using System.Text.Json;
using System.Xml.Linq;

namespace TAKSuite.Data.ModelsTak
{
    public class Medevac : BaseCoT<Medevac>, IColoredCoT
    {
        public int? Color { get ; set; }
        public string Remarks { get ; set; }


        public Medevac(JsonElement uid) : base(uid)
        {
            Color = -65535;
        }



        public void IntegrateInformationFromXml(string xmlContent)
        {
            try
            {
                XDocument xmlDoc = XDocument.Parse(xmlContent);
                XElement medevacElement = xmlDoc.Descendants("_medevac_").FirstOrDefault();

                if (medevacElement != null && medevacElement.Attribute("medline_remarks") != null)
                {
                     Remarks = medevacElement.Attribute("medline_remarks").Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante la lettura del file XML: " + ex.Message);
            }
        }

    }
}
