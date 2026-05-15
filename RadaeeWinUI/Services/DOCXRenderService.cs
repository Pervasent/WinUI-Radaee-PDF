using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using RDUILib;
using RadaeeWinUI.Models;
using Windows.Foundation;

namespace RadaeeWinUI.Services
{
    public class DOCXRenderService : IDOCXRenderService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _pageCache = new();
        private readonly LinkedList<string> _lruList = new();
        private readonly object _lruLock = new();
        private const int MaxCacheSize = 20;
        private int _cacheAccessCounter = 0;

        private readonly ConcurrentDictionary<string, byte[]> _tileCache = new();
        private const int MaxTileCacheEntries = 200;

        // Serializes all native DOCX library calls (DOCX_Page_render, RDDIB, etc.)
        // to prevent concurrent access from multiple Task.Run threads which causes
        // memory corruption crashes in the statically linked native library.
        private readonly SemaphoreSlim _nativeRenderLock = new(1, 1);

        private class CacheEntry
        {
            public WriteableBitmap Bitmap { get; set; }
            public int LastAccessTime { get; set; }
            public LinkedListNode<string>? LruNode { get; set; }
            public string CacheKey { get; set; }
            public int PageIndex { get; set; }

            public CacheEntry(WriteableBitmap bitmap, int accessTime, string cacheKey, int pageIndex)
            {
                Bitmap = bitmap;
                LastAccessTime = accessTime;
                CacheKey = cacheKey;
                PageIndex = pageIndex;
            }
        }

        /// <summary>
        /// Maps RD_RENDER_MODE to an integer quality value for DOCX_Page_render.
        /// </summary>
        private static int MapRenderModeToQuality(RD_RENDER_MODE mode)
        {
            return mode switch
            {
                RD_RENDER_MODE.mode_poor => 0,
                RD_RENDER_MODE.mode_normal => 1,
                _ => 2  // mode_best
            };
        }

        public async Task<WriteableBitmap?> RenderPageAsync(DOCXPage page, int width, int height, RenderOptions options, CancellationToken cancellationToken = default)
        {
            if (width <= 0 || height <= 0)
                return null;

            try
            {
                byte[]? pixelData = await Task.Run(async () =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return null;

                        await _nativeRenderLock.WaitAsync(cancellationToken);
                        try
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return null;

                            RDDIB dib = new RDDIB(width, height);
                            dib.Reset(options.BackgroundColor);

                            if (cancellationToken.IsCancellationRequested)
                                return null;

                            int quality = MapRenderModeToQuality(options.RenderMode);

                            page.RenderPrepare(dib);
                            bool success = page.Render(dib, options.Scale, 0, 0, quality);

                            if (success)
                            {
                                return dib.Data;
                            }

                            return null;
                        }
                        finally
                        {
                            _nativeRenderLock.Release();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error rendering DOCX page: {ex.Message}");
                        return null;
                    }
                }, cancellationToken);

                if (pixelData != null && !cancellationToken.IsCancellationRequested)
                {
                    WriteableBitmap bitmap = new WriteableBitmap(width, height);
                    using (var stream = bitmap.PixelBuffer.AsStream())
                    {
                        stream.Write(pixelData, 0, pixelData.Length);
                    }
                    bitmap.Invalidate();
                    return bitmap;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating DOCX bitmap: {ex.Message}");
                return null;
            }
        }

        public async Task RenderPageTiledAsync(int pageIndex, DOCXPage page, int width, int height, RenderOptions options, Action<TileRenderResult> tileCallback, CancellationToken cancellationToken = default)
        {
            if (width <= 0 || height <= 0)
                return;

            try
            {
                int tileSize = options.TileSize > 0 ? options.TileSize : 512;
                var tiles = GenerateTileGrid(width, height, tileSize);
                AssignTilePriorities(tiles, options.ViewportRect);

                var sortedTiles = tiles.OrderBy(t => (int)t.Priority)
                                       .ThenBy(t => t.Row)
                                       .ThenBy(t => t.Col)
                                       .ToList();

                // Render tiles sequentially in a single Task.Run (native library thread safety).
                // After each tile, invoke tileCallback so the caller can write
                // the tile data into a WriteableBitmap on the UI thread.
                // The _nativeRenderLock is acquired per-tile so that cancellation
                // can be checked between tiles and other page renders can interleave.
                await Task.Run(async () =>
                {
                    try
                    {
                        foreach (var tile in sortedTiles)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            string tileCacheKey = tile.GetCacheKey(pageIndex, options.Scale, false);

                            // Check tile cache
                            if (_tileCache.TryGetValue(tileCacheKey, out var cachedData))
                            {
                                tileCallback(new TileRenderResult(tile, cachedData, true));
                                continue;
                            }

                            // Acquire lock before calling native render code
                            await _nativeRenderLock.WaitAsync(cancellationToken);
                            byte[]? tileData;
                            try
                            {
                                tileData = RenderSingleTile(page, tile, options, cancellationToken);
                            }
                            finally
                            {
                                _nativeRenderLock.Release();
                            }

                            if (tileData != null && !cancellationToken.IsCancellationRequested)
                            {
                                CacheTileData(tileCacheKey, tileData);
                                tileCallback(new TileRenderResult(tile, tileData, true));
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected during cancellation
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in DOCX tiled rendering: {ex.Message}");
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected during cancellation
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DOCX tiled rendering outer: {ex.Message}");
            }
        }

        public string GenerateCacheKey(int pageIndex, int width, int height, RenderOptions options)
        {
            return $"docx_{pageIndex}_{width}_{height}_{(int)options.RenderMode}";
        }

        public void CacheRenderedPage(string cacheKey, WriteableBitmap bitmap)
        {
            int pageIndex = ExtractPageIndexFromCacheKey(cacheKey);

            lock (_lruLock)
            {
                if (_pageCache.TryGetValue(cacheKey, out var existingEntry))
                {
                    if (existingEntry.LruNode != null)
                    {
                        _lruList.Remove(existingEntry.LruNode);
                    }
                    existingEntry.Bitmap = bitmap;
                    existingEntry.LastAccessTime = Interlocked.Increment(ref _cacheAccessCounter);
                    existingEntry.LruNode = _lruList.AddFirst(cacheKey);
                }
                else
                {
                    if (_pageCache.Count >= MaxCacheSize)
                    {
                        var lruCacheKey = _lruList.Last?.Value;
                        if (lruCacheKey != null)
                        {
                            _lruList.RemoveLast();
                            _pageCache.TryRemove(lruCacheKey, out _);
                        }
                    }

                    var entry = new CacheEntry(bitmap, Interlocked.Increment(ref _cacheAccessCounter), cacheKey, pageIndex);
                    entry.LruNode = _lruList.AddFirst(cacheKey);
                    _pageCache[cacheKey] = entry;
                }
            }
        }

        public WriteableBitmap? GetCachedPage(string cacheKey)
        {
            if (_pageCache.TryGetValue(cacheKey, out var entry))
            {
                lock (_lruLock)
                {
                    entry.LastAccessTime = Interlocked.Increment(ref _cacheAccessCounter);
                    if (entry.LruNode != null)
                    {
                        _lruList.Remove(entry.LruNode);
                        entry.LruNode = _lruList.AddFirst(cacheKey);
                    }
                }
                return entry.Bitmap;
            }
            return null;
        }

        public void ClearCache()
        {
            lock (_lruLock)
            {
                _pageCache.Clear();
                _lruList.Clear();
            }
        }

        public void ClearCache(int pageIndex)
        {
            lock (_lruLock)
            {
                var keysToRemove = _pageCache.Where(kvp => kvp.Value.PageIndex == pageIndex)
                                             .Select(kvp => kvp.Key)
                                             .ToList();

                foreach (var key in keysToRemove)
                {
                    if (_pageCache.TryRemove(key, out var entry))
                    {
                        if (entry.LruNode != null)
                        {
                            _lruList.Remove(entry.LruNode);
                        }
                    }
                }
            }
        }

        public void ClearTileCache()
        {
            _tileCache.Clear();
        }

        public void ClearTileCache(int pageIndex)
        {
            var keysToRemove = _tileCache.Keys
                .Where(k => k.StartsWith($"tile_{pageIndex}_"))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _tileCache.TryRemove(key, out _);
            }
        }

        private List<TileInfo> GenerateTileGrid(int pageWidth, int pageHeight, int tileSize)
        {
            var tiles = new List<TileInfo>();
            int cols = (int)Math.Ceiling((double)pageWidth / tileSize);
            int rows = (int)Math.Ceiling((double)pageHeight / tileSize);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = col * tileSize;
                    int y = row * tileSize;
                    int w = Math.Min(tileSize, pageWidth - x);
                    int h = Math.Min(tileSize, pageHeight - y);

                    tiles.Add(new TileInfo
                    {
                        Row = row,
                        Col = col,
                        X = x,
                        Y = y,
                        Width = w,
                        Height = h,
                        Priority = TilePriority.Low
                    });
                }
            }

            return tiles;
        }

        private void AssignTilePriorities(List<TileInfo> tiles, Rect? viewportRect)
        {
            if (viewportRect == null || viewportRect.Value.IsEmpty)
            {
                // No viewport info: all tiles are high priority
                foreach (var tile in tiles)
                    tile.Priority = TilePriority.High;
                return;
            }

            var vp = viewportRect.Value;

            foreach (var tile in tiles)
            {
                double overlapRatio = tile.GetViewportOverlapRatio(vp);

                if (overlapRatio >= 0.5)
                    tile.Priority = TilePriority.High;
                else if (overlapRatio > 0)
                    tile.Priority = TilePriority.Medium;
                else
                    tile.Priority = TilePriority.Low;
            }
        }

        /// <summary>
        /// Renders a single tile of a DOCX page.
        /// DOCXPage.Render uses (dib, scale, orgx, orgy, quality) where orgx/orgy
        /// specify the pixel origin offset, effectively shifting the rendered content
        /// so that only the tile region is captured in the tile-sized DIB.
        /// </summary>
        private byte[]? RenderSingleTile(DOCXPage page, TileInfo tile, RenderOptions options, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                RDDIB tileDib = new RDDIB(tile.Width, tile.Height);
                tileDib.Reset(options.BackgroundColor);

                if (cancellationToken.IsCancellationRequested)
                    return null;

                int quality = MapRenderModeToQuality(options.RenderMode);

                page.RenderPrepare(tileDib);
                bool success = page.Render(tileDib, options.Scale, tile.X, tile.Y, quality);

                if (success)
                {
                    return tileDib.Data;
                }

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rendering DOCX single tile: {ex.Message}");
                return null;
            }
        }

        private void CacheTileData(string tileCacheKey, byte[] tileData)
        {
            // Evict oldest entries if cache is full
            if (_tileCache.Count >= MaxTileCacheEntries)
            {
                // Simple eviction: remove first 10% of entries
                var keysToRemove = _tileCache.Keys.Take(MaxTileCacheEntries / 10).ToList();
                foreach (var key in keysToRemove)
                {
                    _tileCache.TryRemove(key, out _);
                }
            }

            _tileCache[tileCacheKey] = tileData;
        }

        private int ExtractPageIndexFromCacheKey(string cacheKey)
        {
            // Cache key format: docx_{pageIndex}_{width}_{height}_{renderMode}
            var parts = cacheKey.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int pageIndex))
            {
                return pageIndex;
            }
            return -1;
        }
    }
}
