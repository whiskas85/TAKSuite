namespace TAKSuite.Data.ModelsTak
{
    using System;
    using System.Collections.Generic;

    public class MissionsRoot
    {
        public string Version { get; set; }
        public string Type { get; set; }
        public List<MissionAtak> Data { get; set; }
        public string NodeId { get; set; }  // Aggiunto per corrispondere al JSON
    }

    public class MissionAtak
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ChatRoom { get; set; }
        public string BaseLayer { get; set; }
        public string Bbox { get; set; }
        public string Path { get; set; }
        public string Classification { get; set; }
        public string Tool { get; set; }
        public List<string> Keywords { get; set; } = new();
        public string CreatorUid { get; set; }
        public DateTime? CreateTime { get; set; }
        public List<string> Groups { get; set; } = new();
        public List<object> ExternalData { get; set; } = new();
        public List<object> Feeds { get; set; } = new();
        public List<object> MapLayers { get; set; } = new();
        public DefaultRole DefaultRole { get; set; }
        public bool InviteOnly { get; set; }
        public int? Expiration { get; set; }  // Reso nullable per sicurezza
        public string Guid { get; set; }
        public List<UidEntry> Uids { get; set; } = new();
        public List<Content> Contents { get; set; } = new();
        public bool PasswordProtected { get; set; }  // Aggiunto per corrispondenza con il JSON
    }

    public class DefaultRole
    {
        public List<string> Permissions { get; set; } = new();
        public string Type { get; set; }
    }

    public class UidEntry
    {
        public string Data { get; set; }
        public DateTime? Timestamp { get; set; }
        public string CreatorUid { get; set; }
        public UidDetails Details { get; set; }  // Aggiunto per mappare i dettagli
    }

    public class UidDetails
    {
        public string Type { get; set; }
        public string Callsign { get; set; }
        public string IconsetPath { get; set; }
        public string Color { get; set; }  // Modificato da int? a string
        public Location Location { get; set; }
        public List<string> Attachments { get; set; } = new();
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class Content
    {
        public ContentData Data { get; set; }
        public DateTime? Timestamp { get; set; }  // Aggiunto per corrispondenza con il JSON
        public string CreatorUid { get; set; }
    }

    public class ContentData
    {
        public List<string> Keywords { get; set; } = new();
        public string MimeType { get; set; }
        public string Name { get; set; }
        public DateTime? SubmissionTime { get; set; }
        public string Submitter { get; set; }
        public string Uid { get; set; }
        public string CreatorUid { get; set; }
        public string Hash { get; set; }
        public long? Size { get; set; }
        public int? Expiration { get; set; }  // Reso nullable per sicurezza
        public bool PasswordProtected { get; set; }
    }
}
