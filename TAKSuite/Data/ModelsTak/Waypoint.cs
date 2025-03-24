using System.Net.Mail;
using System.Text.Json;

namespace TAKSuite.Data.ModelsTak
{
    public class Waypoint : BaseCoT<Waypoint>, IColoredCoT
    {
        public int? Color { get; set; }

        public List<string> AttachmentUidList { get; set; }

        public Waypoint(JsonElement uid) : base(uid)
        {
            Color = int.Parse(uid.GetProperty("details").GetProperty("color").GetString());


            var attachments = new List<String>();
            if (uid.TryGetProperty("details", out JsonElement details) &&
            details.TryGetProperty("attachments", out JsonElement attachmentsElement) &&
            attachmentsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement attachment in attachmentsElement.EnumerateArray())
                {
                    attachments.Add(attachment.GetString());
                }
            }
            AttachmentUidList = attachments;
        }



    }
}
