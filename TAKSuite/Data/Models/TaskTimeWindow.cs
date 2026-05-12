using System.Text.Json.Serialization;

namespace TAKSuite.Data.Models
{
    public class TaskTimeWindow
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime   { get; set; }

        [JsonIgnore]
        public bool IsEmpty => !StartDateTime.HasValue && !EndDateTime.HasValue;
    }
}
