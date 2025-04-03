using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text;

[XmlRoot("event")]
public class EventData
{
    [XmlAttribute("version")] public string Version { get; set; }
    [XmlAttribute("uid")] public string Uid { get; set; }
    [XmlAttribute("type")] public string Type { get; set; }
    [XmlAttribute("time")] public DateTime Time { get; set; }
    [XmlAttribute("start")] public DateTime Start { get; set; }
    [XmlAttribute("stale")] public DateTime Stale { get; set; }
    [XmlAttribute("how")] public string How { get; set; }

    [XmlElement("point")]
    public Point Point { get; set; }

    [XmlElement("detail")]
    public Detail Detail { get; set; }

    public static EventData LoadFromString(string xmlContent)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(EventData));
        using (StringReader reader = new StringReader(xmlContent))
        {
            return (EventData)serializer.Deserialize(reader);
        }
    }
}

public class Point
{
    [XmlAttribute("lat")] public double Latitude { get; set; }
    [XmlAttribute("lon")] public double Longitude { get; set; }
    [XmlAttribute("hae")] public double Hae { get; set; }
    [XmlAttribute("ce")] public double Ce { get; set; }
    [XmlAttribute("le")] public double Le { get; set; }
}

public class Detail
{
    [XmlElement("contact")]
    public Contact Contact { get; set; }

    [XmlElement("status")]
    public Status Status { get; set; }

    [XmlElement("usericon")]
    public UserIcon UserIcon { get; set; }

    [XmlElement("color")]
    public Color Color { get; set; }

    [XmlElement("link")]
    public List<Link> Links { get; set; }
}

public class Contact
{
    [XmlAttribute("callsign")] public string Callsign { get; set; }
}

public class Status
{
    [XmlAttribute("readiness")] public bool Readiness { get; set; }
}

public class UserIcon
{
    [XmlAttribute("iconsetpath")] public string IconSetPath { get; set; }
}

public class Color
{
    [XmlAttribute("argb")] public string Argb { get; set; }
}

public class Link
{
    [XmlAttribute("uid")] public string Uid { get; set; }
    [XmlAttribute("production_time")] public DateTime ProductionTime { get; set; }
    [XmlAttribute("type")] public string Type { get; set; }
    [XmlAttribute("parent_callsign")] public string ParentCallsign { get; set; }
    [XmlAttribute("relation")] public string Relation { get; set; }
}
