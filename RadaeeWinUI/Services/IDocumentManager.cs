using System.Collections.Generic;
using System.Threading.Tasks;
using RadaeeWinUI.Models;
using Windows.Storage;

namespace RadaeeWinUI.Services
{
    public interface IDocumentManager
    {
        Task<IDocument?> OpenDocumentAsync(StorageFile file, string password = "");
        void CloseDocument(IDocument? doc);
        DocumentInfo? GetDocumentInfo(IDocument? doc);
        Task<Dictionary<string, string>> GetMetadataAsync(IDocument? doc);
    }
}
