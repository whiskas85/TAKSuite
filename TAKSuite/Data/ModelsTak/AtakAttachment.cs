using System.Net.Http.Headers;

namespace TAKSuite.Data.ModelsTak
{
    public class AtakAttachment
    {
        internal string? MediaType { get; set; }
        internal byte[] FileBytes { get; set; }

        public string Uid { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Hash { get; set; }

        public AtakAttachment()
        {
        }
    }
}
