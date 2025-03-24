using System.Net.Mail;
using System.Text.Json;

namespace TAKSuite.Data.ModelsTak
{
    public class ATAKPhoto: BaseCoT<ATAKPhoto>
    {
        public List<string> AttachmentUidList { get; set; }

        public ATAKPhoto(JsonElement uid) : base(uid)
        {
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
