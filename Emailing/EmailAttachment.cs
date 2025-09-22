namespace Emailing
{
    public class EmailAttachment
    {
        private string _FilePath;
        public string FilePath { get { return _FilePath; } }
        private string _ContentId;
        public string ContentId { get { return _ContentId; } }
        public EmailAttachment(string filePath, string contentId = null) {
            _FilePath = filePath;
            _ContentId = contentId;
        }
    }
}