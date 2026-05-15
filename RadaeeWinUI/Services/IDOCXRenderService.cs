using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using RDUILib;
using RadaeeWinUI.Models;

namespace RadaeeWinUI.Services
{
    public interface IDOCXRenderService
    {
        Task<WriteableBitmap?> RenderPageAsync(DOCXPage page, int width, int height, RenderOptions options, CancellationToken cancellationToken = default);
        Task RenderPageTiledAsync(int pageIndex, DOCXPage page, int width, int height, RenderOptions options, Action<TileRenderResult> tileCallback, CancellationToken cancellationToken = default);
        string GenerateCacheKey(int pageIndex, int width, int height, RenderOptions options);
        void CacheRenderedPage(string cacheKey, WriteableBitmap bitmap);
        WriteableBitmap? GetCachedPage(string cacheKey);
        void ClearCache();
        void ClearCache(int pageIndex);
        void ClearTileCache();
        void ClearTileCache(int pageIndex);
    }
}
