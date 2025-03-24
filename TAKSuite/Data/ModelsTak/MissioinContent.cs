namespace TAKSuite.Data.ModelsTak
{
    using System;
    using System.Collections.Generic;
    public class MissionsRoot
    {
        public String Version { get; set; }
        public String Type { get; set; }
        public List<MissionAtak> Data { get; set; }

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
        public int Expiration { get; set; }
        public string Guid { get; set; }
        public List<UidEntry> Uids { get; set; } = new();
        public List<Content> Contents { get; set; } = new();
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
    }

    public class Content
    {
        public ContentData Data { get; set; }
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
        public int Expiration { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool PasswordProtected { get; set; }
    }

}
