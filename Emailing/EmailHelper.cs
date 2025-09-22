using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Web;
using Core.Exceptions;
using Core.Strings;

namespace Emailing
{
    public class EmailHelper
    {
        public static void Send(
            SmtpServerConfiguration smtpServerConfiguration,
            string fromEmailAddress, 
            string toEmailAddress, 
            string subject, 
            string message,
            Dictionary<string, string>? mapTagToCustomContent = null,
            Dictionary<string, string> mapCustomHeaderNameToCustomContent = null,
            EmailAttachment[]? emailAttachments = null)
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(fromEmailAddress, fromEmailAddress));
            mimeMessage.To.Add(new MailboxAddress(toEmailAddress, toEmailAddress));
            mimeMessage.Subject = subject;
            Multipart multipart = new Multipart();
            if (mapCustomHeaderNameToCustomContent != null)
            {
                AddCustomHeaders(mimeMessage, mapCustomHeaderNameToCustomContent, mapTagToCustomContent);
            }
            if (mapTagToCustomContent != null)
            {
                message = InsertCustomContent(message, mapTagToCustomContent);
            }
            if (emailAttachments != null)
            {
                DoFileAttachments(multipart, emailAttachments);
            }
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = PreMailer.Net.PreMailer.MoveCssInline(message).Html;
            multipart.Add(bodyBuilder.ToMessageBody());
            mimeMessage.Body = multipart;
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                if (smtpServerConfiguration.Port == null) throw new ArgumentNullException($"The {nameof(SmtpServerConfiguration)} had {nameof(smtpServerConfiguration.Port)}==null");
                client.Connect(smtpServerConfiguration.Domain, (int)smtpServerConfiguration.Port, smtpServerConfiguration.UseSsl);
                if(!string.IsNullOrEmpty(smtpServerConfiguration.Password))
                    client.Authenticate(smtpServerConfiguration.Username, smtpServerConfiguration.Password);
                client.Send(mimeMessage);
                client.Disconnect(true);
            }
        }
        private static void DoFileAttachments(Multipart multipart, EmailAttachment[] emailAttachments)
        {
            foreach (EmailAttachment emailAttachment in emailAttachments)
                DoFileAttachment(multipart, emailAttachment);
        }
        private static void DoFileAttachment(Multipart multipart, EmailAttachment emailAttachment)
        {
            var attachment = new MimePart()
            {
                Content = new MimeContent(File.OpenRead(emailAttachment.FilePath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(emailAttachment.FilePath)
            };
            if (emailAttachment.ContentId != null)
                attachment.ContentId = emailAttachment.ContentId;
            multipart.Add(attachment);
        }
        private static void AddCustomHeaders(MimeMessage mimeMessage, Dictionary<string, string> mapCustomHeaderNameToCustomContent,
            Dictionary<string, string> mapTagToCustomContent)
        {
            foreach (KeyValuePair<string, string> headerNameCustomContentKeyValuePair in mapCustomHeaderNameToCustomContent)
            {
                string headerName = headerNameCustomContentKeyValuePair.Key;
                headerName = HttpUtility.UrlEncode(headerName);
                string customContent = headerNameCustomContentKeyValuePair.Value;
                if (mapCustomHeaderNameToCustomContent != null)
                {
                    if (customContent == null)
                    {
                        continue;
                    }
                    customContent = InsertCustomContent(customContent, mapTagToCustomContent);
                }
                if (customContent == null) continue;
                bool mailMessageHasHeader = mimeMessage.Headers.Contains(headerName);
                if (mailMessageHasHeader)
                    throw new DuplicateKeyException($"Attempt to apply the header \"{headerName}\" twice");
                mimeMessage.Headers.Add(headerName, customContent);
            }
        }

        private static string InsertCustomContent(string str, Dictionary<string, string> mapTagToCustomContent)
        {
            return StringHelper.Format(str, mapTagToCustomContent);
        }
    }
}