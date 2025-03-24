using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Xml;

namespace TAKSuite.Data.ServicesTak
{
    public class AttachmentService
    {
        private readonly MartiApiClient _client;
        private readonly CoTManager _cotManager;
        private readonly ApplicationDbContext _context;
        public AttachmentService(ApplicationDbContext context, MartiApiClient client, CoTManager manager)
        {

            _client = client;
            _cotManager = manager;
            _context = context;
        }
        public async Task<List<AtakAttachment>> GetAllAsync(string itemAttachment)
        {
            // le missioni devono essere tutte quelle che sono assegnate al team
            var uidInfo = await _client.GetInfoAsync(itemAttachment);

            var list = GetAttachmentFromItem(uidInfo);

            List<AtakAttachment> attachments = new List<AtakAttachment>();

            foreach (var item in list)
            {
                var attachment = await _client.GetFileAsync(item);
                if (attachment != null)
                {
                    attachment.Uid = itemAttachment;
                    attachment.Hash = item;
                    attachments.Add(attachment);
                }
            }

            return await Task.Run(() => attachments);
        }


        protected List<string> GetAttachmentFromItem(string data)
        {
            List<string> attachmentIds = new List<string>();

            if (string.IsNullOrWhiteSpace(data))
            {
                Console.WriteLine("Errore: Il dato XML è nullo o vuoto.");
                return attachmentIds;
            }

            try
            {
                XDocument doc = XDocument.Parse(data);
                XElement attachmentElement = doc.Descendants("attachment_list").FirstOrDefault();

                if (attachmentElement == null)
                {
                    Console.WriteLine("Errore: Nessun elemento attachment_list trovato.");
                    return attachmentIds;
                }

                XAttribute hashesAttribute = attachmentElement.Attribute("hashes");
                if (hashesAttribute == null || string.IsNullOrWhiteSpace(hashesAttribute.Value))
                {
                    Console.WriteLine("Errore: Attributo 'hashes' non trovato o vuoto.");
                    return attachmentIds;
                }

                // Rimuove le parentesi quadre e divide gli ID
                string hashesValue = hashesAttribute.Value.Trim('[', ']');
                attachmentIds.AddRange(hashesValue.Split(',').Select(h => h.Trim('"')));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il parsing dell'XML: {ex.Message}");
            }

            return attachmentIds;
        }

        public static XmlDocument AddAttachment(XmlDocument doc, string hash)
        {
            XmlNode? detailNode = doc.SelectSingleNode("//detail");
            if (detailNode == null)
            {
                detailNode = doc.CreateElement("detail");
                doc.DocumentElement?.AppendChild(detailNode);
            }

            XmlNode? attachmentListNode = detailNode.SelectSingleNode("attachment_list");
            if (attachmentListNode == null)
            {
                attachmentListNode = doc.CreateElement("attachment_list");
                detailNode.AppendChild(attachmentListNode);
            }

            // Controlla se l'attributo "hashes" esiste
            XmlAttribute? hashesAttr = attachmentListNode.Attributes?["hashes"];
            if (hashesAttr == null)
            {
                hashesAttr = doc.CreateAttribute("hashes");
                attachmentListNode.Attributes.Append(hashesAttr);
                hashesAttr.Value = $"[\"{hash}\"]";
            }
            else
            {
                // Converte la stringa in una lista e aggiunge il nuovo hash se non è già presente
                var hashesList = hashesAttr.Value.Trim('[', ']').Split(',');
                var hashSet = new HashSet<string>(hashesList.Select(h => h.Trim('\"')));

                if (!hashSet.Contains(hash))
                {
                    hashSet.Add(hash);
                    hashesAttr.Value = $"[\"{string.Join("\",\"", hashSet)}\"]";
                }
            }

            return doc;
        }

        public static XmlDocument RemoveAttachment(XmlDocument doc, string hash)
        {
            XmlNode? attachmentListNode = doc.SelectSingleNode("//detail/attachment_list");
            if (attachmentListNode == null)
            {
                return doc; // Se il nodo non esiste, non c'è nulla da rimuovere
            }

            XmlAttribute? hashesAttr = attachmentListNode.Attributes?["hashes"];
            if (hashesAttr == null || string.IsNullOrWhiteSpace(hashesAttr.Value))
            {
                return doc; // Se non ci sono hash, non c'è nulla da rimuovere
            }

            // Converte la stringa dell'attributo in una lista di hash
            var hashesList = hashesAttr.Value.Trim('[', ']').Split(',');
            var hashSet = new HashSet<string>(hashesList.Select(h => h.Trim('\"')));

            // Rimuove l'hash specificato
            if (hashSet.Contains(hash))
            {
                hashSet.Remove(hash);

                if (hashSet.Count == 0)
                {
                    // Se non ci sono più hash, rimuove completamente l'attributo
                    attachmentListNode.Attributes.Remove(hashesAttr);
                }
                else
                {
                    // Aggiorna l'attributo con la nuova lista
                    hashesAttr.Value = $"[\"{string.Join("\",\"", hashSet)}\"]";
                }
            }

            return doc;
        }



        public async Task<bool> DeleteAsync(AtakAttachment itemAttachment)
        {
            var response = await _client.GetInfoAsync(itemAttachment.Uid);

            if (response != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);

                RemoveAttachment(doc, itemAttachment.Hash);
                await _cotManager.UpdateCoTMission("TEST FEED2", itemAttachment.Uid, doc);
            }

            // le missioni devono essere tutte quelle che sono assegnate al team
            var res = await _client.DeleteAttachmentAsync("TEST FEED2", itemAttachment.Hash);

            return await Task.Run(() => res);
        }
    }

}
