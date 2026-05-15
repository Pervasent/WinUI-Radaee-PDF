using RDUILib;

namespace RadaeeWinUI.Models
{
    public class DOCXDocumentWrapper : IDocument
    {
        private readonly DOCXDoc _docxDoc;
        private bool _isOpened;

        public DOCXDocumentWrapper(DOCXDoc docxDoc)
        {
            _docxDoc = docxDoc;
            _isOpened = true;
        }

        public DocumentType Type => DocumentType.DOCX;

        public int PageCount => _docxDoc.PageCount;

        public bool IsOpened => _isOpened;

        public float GetPageWidth(int pageIndex)
        {
            return _docxDoc.GetPageWidth(pageIndex);
        }

        public float GetPageHeight(int pageIndex)
        {
            return _docxDoc.GetPageHeight(pageIndex);
        }

        public void Close()
        {
            _docxDoc.Close();
            _isOpened = false;
        }

        public DOCXDoc InnerDoc => _docxDoc;
    }
}
