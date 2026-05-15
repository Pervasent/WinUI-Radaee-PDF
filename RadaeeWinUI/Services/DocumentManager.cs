using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RDUILib;
using RadaeeWinUI.Models;
using Windows.Storage;
using Windows.Storage.Streams;

namespace RadaeeWinUI.Services
{
    public class DocumentManager : IDocumentManager
    {
        public async Task<IDocument?> OpenDocumentAsync(StorageFile file, string password = "")
        {
            try
            {
                string extension = file.FileType.ToLower();

                if (extension == ".pdf")
                {
                    return await OpenPDFAsync(file, password);
                }
                else if (extension == ".docx")
                {
                    return await OpenDOCXAsync(file, password);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unsupported file type: {extension}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening document: {ex.Message}");
                return null;
            }
        }

        private async Task<IDocument?> OpenPDFAsync(StorageFile file, string password)
        {
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            PDFDoc doc = new PDFDoc();
            int ret = doc.Open(stream, password);

            if (ret == 0)
            {
                return new PDFDocumentWrapper(doc);
            }
            else if (ret == -2)
            {
                System.Diagnostics.Debug.WriteLine("PDF document requires password.");
                return null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open PDF document, error code: {ret}");
                return null;
            }
        }

        private async Task<IDocument?> OpenDOCXAsync(StorageFile file, string password)
        {
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            DOCXDoc doc = new DOCXDoc();
            int ret = doc.Open(stream, password);

            if (ret == 0)
            {
                return new DOCXDocumentWrapper(doc);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open DOCX document, error code: {ret}");
                return null;
            }
        }

        public void CloseDocument(IDocument? doc)
        {
            if (doc != null && doc.IsOpened)
            {
                doc.Close();
            }
        }

        public DocumentInfo? GetDocumentInfo(IDocument? doc)
        {
            if (doc == null || !doc.IsOpened)
                return null;

            return new DocumentInfo
            {
                Type = doc.Type,
                PageCount = doc.PageCount,
                IsEncrypted = false,
                IsOpened = doc.IsOpened
            };
        }

        public Task<Dictionary<string, string>> GetMetadataAsync(IDocument? doc)
        {
            var metadata = new Dictionary<string, string>();

            if (doc == null || !doc.IsOpened)
                return Task.FromResult(metadata);

            try
            {
                if (doc is PDFDocumentWrapper pdfWrapper)
                {
                    string xmp = pdfWrapper.InnerDoc.XMP;
                    if (!string.IsNullOrEmpty(xmp))
                    {
                        metadata["XMP"] = xmp;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting metadata: {ex.Message}");
            }

            return Task.FromResult(metadata);
        }
    }
}
