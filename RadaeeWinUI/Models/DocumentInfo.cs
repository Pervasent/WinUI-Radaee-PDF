namespace RadaeeWinUI.Models
{
    public class DocumentInfo
    {
        public DocumentType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsOpened { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
    }
}
