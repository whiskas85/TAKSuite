using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using TAKSuite.Helper;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml;

namespace TAKSuite.Data.ServicesTak
{
    public class AttachmentService
    {
        private readonly MartiApiClient _client;

        public AttachmentService(MartiApiClient client)
        {
            _client = client;
        }

        public async Task<int> GetCountAsync(string itemAttachment)
        {
            if (string.IsNullOrEmpty(itemAttachment)) return 0;
            try
            {
                var uidInfo = await _client.GetInfoAsync(itemAttachment);
                return GetAttachmentFromItem(uidInfo).Count;
            }
            catch { return 0; }
        }

        public async Task<List<AtakAttachment>> GetAllAsync(string itemAttachment)
        {
            var uidInfo = await _client.GetInfoAsync(itemAttachment);
            var hashes  = GetAttachmentFromItem(uidInfo);

            var attachments = new List<AtakAttachment>();
            foreach (var hash in hashes)
            {
                var attachment = await _client.GetFileAsync(hash);
                if (attachment != null)
                {
                    attachment.Uid  = itemAttachment;
                    attachment.Hash = hash;
                    attachments.Add(attachment);
                }
            }
            return attachments;
        }

        protected List<string> GetAttachmentFromItem(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return new();

            try
            {
                var doc             = XDocument.Parse(data);
                var attachmentEl    = doc.Descendants("attachment_list").FirstOrDefault();
                var hashesAttr      = attachmentEl?.Attribute("hashes");
                if (hashesAttr == null || string.IsNullOrWhiteSpace(hashesAttr.Value)) return new();

                // hashes attribute is a JSON array string: ["hash1","hash2"]
                return hashesAttr.Value.Trim('[', ']')
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(h => h.Trim('"'))
                    .ToList();
            }
            catch { return new(); }
        }

        public static XmlDocument AddAttachment(XmlDocument doc, string hash)
        {
            var detailNode = doc.SelectSingleNode("//detail") ?? CreateAndAppend(doc, doc.DocumentElement!, "detail");
            var listNode   = detailNode.SelectSingleNode("attachment_list") ?? CreateAndAppend(doc, detailNode, "attachment_list");

            var hashesAttr = listNode.Attributes?["hashes"];
            if (hashesAttr == null)
            {
                hashesAttr = doc.CreateAttribute("hashes");
                listNode.Attributes!.Append(hashesAttr);
                hashesAttr.Value = $"[\"{hash}\"]";
            }
            else
            {
                var set = ParseHashSet(hashesAttr.Value);
                if (set.Add(hash))
                    hashesAttr.Value = SerializeHashSet(set);
            }

            return doc;
        }

        public static XmlDocument RemoveAttachment(XmlDocument doc, string hash)
        {
            var listNode  = doc.SelectSingleNode("//detail/attachment_list");
            var hashesAttr = listNode?.Attributes?["hashes"];
            if (hashesAttr == null || string.IsNullOrWhiteSpace(hashesAttr.Value)) return doc;

            var set = ParseHashSet(hashesAttr.Value);
            if (!set.Remove(hash)) return doc;

            if (set.Count == 0)
                listNode!.Attributes!.Remove(hashesAttr);
            else
                hashesAttr.Value = SerializeHashSet(set);

            return doc;
        }

        public async Task<bool> AttachHtmlAsync(string missionUid, string poiUid, byte[] htmlBytes, string filename)
        {
            try
            {
                // Replace any existing file with the same name before re-uploading
                var existing = await GetAllAsync(poiUid);
                foreach (var att in existing.Where(a =>
                    string.Equals(Uri.UnescapeDataString(a.Name ?? ""), filename, StringComparison.OrdinalIgnoreCase)))
                {
                    await DeleteAsync(missionUid, att);
                }
            }
            catch { }

            var hash = await _client.UploadFileToMissionAsync(missionUid, htmlBytes, filename, "text/html");
            if (string.IsNullOrEmpty(hash)) return false;
            return await _client.AddFileToCotAsync(poiUid, hash, missionUid);
        }

        public async Task<bool> UploadAndAttachAsync(string missionUid, string poiUid, byte[] bytes, string filename, string contentType = "application/octet-stream")
        {
            var hash = await _client.UploadFileToMissionAsync(missionUid, bytes, filename, contentType);
            if (string.IsNullOrEmpty(hash)) return false;
            await _client.AddFileToCotAsync(poiUid, hash, missionUid);
            return true;
        }

        public async Task<bool> UploadToCotAsync(string cotUid, byte[] bytes, string filename, string contentType = "application/octet-stream")
        {
            var hash = await _client.UploadFileAsync(bytes, filename, contentType);
            if (string.IsNullOrEmpty(hash)) return false;
            return await _client.AddFileToCotAsync(cotUid, hash);
        }

        public async Task<bool> DeleteAsync(string missionUid, AtakAttachment itemAttachment)
        {
            await _client.RemoveFileFromCotAsync(itemAttachment.Uid, itemAttachment.Hash,
                string.IsNullOrEmpty(missionUid) ? null : missionUid);
            return await _client.DeleteAttachmentAsync(missionUid, itemAttachment.Hash);
        }

        private static XmlNode CreateAndAppend(XmlDocument doc, XmlNode parent, string elementName)
        {
            var node = doc.CreateElement(elementName);
            parent.AppendChild(node);
            return node;
        }

        private static HashSet<string> ParseHashSet(string raw)
            => new(raw.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim('"')));

        private static string SerializeHashSet(HashSet<string> set)
            => $"[\"{string.Join("\",\"", set)}\"]";
    }
}
