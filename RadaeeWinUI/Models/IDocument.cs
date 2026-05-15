namespace RadaeeWinUI.Models
{
    public interface IDocument
    {
        DocumentType Type { get; }
        int PageCount { get; }
        bool IsOpened { get; }
        float GetPageWidth(int pageIndex);
        float GetPageHeight(int pageIndex);
        void Close();
    }
}
