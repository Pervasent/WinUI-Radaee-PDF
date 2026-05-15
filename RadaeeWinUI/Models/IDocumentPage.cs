using RDUILib;

namespace RadaeeWinUI.Models
{
    public interface IDocumentPage
    {
        long Handle { get; }
        void Close();
        void RenderPrepare();
        void RenderPrepare(RDDIB dib);
        void RenderCancel();
        bool RenderIsFinished();
    }
}
