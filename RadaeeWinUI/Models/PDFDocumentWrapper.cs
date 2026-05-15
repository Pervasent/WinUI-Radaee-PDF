using RDUILib;

namespace RadaeeWinUI.Models
{
    public class PDFDocumentWrapper : IDocument
    {
        private readonly PDFDoc _pdfDoc;

        public PDFDocumentWrapper(PDFDoc pdfDoc)
        {
            _pdfDoc = pdfDoc;
        }

        public DocumentType Type => DocumentType.PDF;

        public int PageCount => _pdfDoc.PageCount;

        public bool IsOpened => _pdfDoc.IsOpened;

        public float GetPageWidth(int pageIndex)
        {
            return _pdfDoc.GetPageWidth(pageIndex);
        }

        public float GetPageHeight(int pageIndex)
        {
            return _pdfDoc.GetPageHeight(pageIndex);
        }

        public void Close()
        {
            _pdfDoc.Close();
        }

        public PDFDoc InnerDoc => _pdfDoc;
    }
}
