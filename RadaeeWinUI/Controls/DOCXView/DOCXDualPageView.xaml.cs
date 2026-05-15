using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using RadaeeWinUI.Models;
using RadaeeWinUI.Services;
using RadaeeWinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RadaeeWinUI.Controls.DOCXView
{
    public sealed partial class DOCXDualPageView : DOCXView
    {
        private IDOCXRenderService? _renderService;
        private ILayoutManager? _layoutManager;
        private Dictionary<int, PageContainer> _pageContainers = new();
        private Dictionary<int, CancellationTokenSource> _renderCancellationTokens = new();
        private DispatcherTimer? _resizeDebounceTimer;
        private bool _isLoaded = false;
        private bool _needsInitialization = false;
        private bool _isResizing = false;
        private int _currentBasePage = 0;
        private GestureRecognizer? _gestureRecognizer;

        public DOCXDualPageView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            InitializeResizeDebounceTimer();
            InitializeGestureRecognizer();
        }

        private void InitializeResizeDebounceTimer()
        {
            _resizeDebounceTimer = new DispatcherTimer();
            _resizeDebounceTimer.Interval = TimeSpan.FromMilliseconds(200);
            _resizeDebounceTimer.Tick += OnResizeDebounceTimerTick;
        }

        private void InitializeGestureRecognizer()
        {
            _gestureRecognizer = new GestureRecognizer(PageCanvas);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            if (_needsInitialization && mDocument != null && mDocument.IsOpened)
            {
                _needsInitialization = false;
                InitializeLayout();
                RenderCurrentPages();
            }
        }

        public void SetRenderService(IDOCXRenderService renderService)
        {
            _renderService = renderService;
        }

        public void SetLayoutManager(ILayoutManager layoutManager)
        {
            _layoutManager = layoutManager;
        }

        public override void DOCXVOpen(IDocument doc)
        {
            mDocument = doc;
            if (doc != null && doc.IsOpened)
            {
                _currentBasePage = 0;
                _currentPageIndex = 0;

                if (_isLoaded)
                {
                    InitializeLayout();
                    RenderCurrentPages();
                }
                else
                {
                    _needsInitialization = true;
                }
            }
        }

        public override List<int> GetVisiblePageIndices()
        {
            return new List<int>(_pageContainers.Keys);
        }

        public override void DOCXVClose()
        {
            CancelAllRenders();
            _resizeDebounceTimer?.Stop();
            _isResizing = false;
            _gestureRecognizer?.Dispose();
            _gestureRecognizer = null;
            ClearAllPages();
            mDocument = null;
            _renderService?.ClearCache();
            _renderService?.ClearTileCache();
        }

        private void InitializeLayout()
        {
            if (mDocument == null || !mDocument.IsOpened || _layoutManager == null)
                return;

            double containerWidth = MainScrollViewer.ViewportWidth > 0 ? MainScrollViewer.ViewportWidth : (ActualWidth > 0 ? ActualWidth : 800);
            double containerHeight = MainScrollViewer.ViewportHeight > 0 ? MainScrollViewer.ViewportHeight : (ActualHeight > 0 ? ActualHeight : 600);

            _layoutManager.CurrentViewMode = ViewMode.DualPage;
            _layoutManager.Initialize(mDocument.PageCount, containerWidth, containerHeight);

            int leftPage = _currentBasePage;
            int rightPage = _currentBasePage + 1;

            if (leftPage < mDocument.PageCount)
            {
                float leftPageWidth = vPageGetWidth(leftPage);
                float leftPageHeight = vPageGetHeight(leftPage);

                float scale = (float)(containerWidth / (leftPageWidth * 2));

                _layoutManager.UpdatePageSize(leftPage, leftPageWidth * scale, leftPageHeight * scale);

                if (rightPage < mDocument.PageCount)
                {
                    float rightPageWidth = vPageGetWidth(rightPage);
                    float rightPageHeight = vPageGetHeight(rightPage);

                    _layoutManager.UpdatePageSize(rightPage, rightPageWidth * ZoomLevel * scale, rightPageHeight * ZoomLevel * scale);
                }
            }

            var totalSize = _layoutManager.GetTotalSize();
            PageCanvas.Width = Math.Max(totalSize.width, containerWidth);
            PageCanvas.Height = Math.Max(totalSize.height, containerHeight);
        }

        private float GetCurrentScale()
        {
            if (mDocument == null || !mDocument.IsOpened)
                return 1.0f;

            double viewportWidth = MainScrollViewer.ViewportWidth > 0 ? MainScrollViewer.ViewportWidth : (ActualWidth > 0 ? ActualWidth : 800);
            float pageWidth = vPageGetWidth(_currentBasePage);

            return (float)(viewportWidth / (pageWidth * 2)) * ZoomLevel;
        }

        public override float vPageGetWidth(int pageIndex)
        {
            if (mDocument == null || !mDocument.IsOpened)
                return 0;

            if (pageIndex < 0 || pageIndex >= mDocument.PageCount)
                return 0;

            return mDocument.GetPageWidth(pageIndex);
        }

        public override float vPageGetHeight(int pageIndex)
        {
            if (mDocument == null || !mDocument.IsOpened)
                return 0;

            if (pageIndex < 0 || pageIndex >= mDocument.PageCount)
                return 0;

            return mDocument.GetPageHeight(pageIndex);
        }

        public override int GetPageAtPoint(float screenX, float screenY)
        {
            if (mDocument == null || !mDocument.IsOpened || _layoutManager == null)
                return -1;

            int leftPage = _currentBasePage;
            int rightPage = _currentBasePage + 1;

            if (leftPage < mDocument.PageCount)
            {
                var leftPagePos = _layoutManager.GetPagePosition(leftPage);
                float leftPageWidth = vPageGetWidth(leftPage) * GetCurrentScale();
                float leftPageHeight = vPageGetHeight(leftPage) * GetCurrentScale();

                if (screenX >= leftPagePos.x && screenX <= leftPagePos.x + leftPageWidth &&
                    screenY >= leftPagePos.y && screenY <= leftPagePos.y + leftPageHeight)
                {
                    return leftPage;
                }
            }

            if (rightPage < mDocument.PageCount)
            {
                var rightPagePos = _layoutManager.GetPagePosition(rightPage);
                float rightPageWidth = vPageGetWidth(rightPage) * GetCurrentScale();
                float rightPageHeight = vPageGetHeight(rightPage) * GetCurrentScale();

                if (screenX >= rightPagePos.x && screenX <= rightPagePos.x + rightPageWidth &&
                    screenY >= rightPagePos.y && screenY <= rightPagePos.y + rightPageHeight)
                {
                    return rightPage;
                }
            }

            return -1;
        }

        public override (float x, float y) GetPagePosition(int pageIndex)
        {
            if (_layoutManager == null)
                return (0, 0);

            var pos = _layoutManager.GetPagePosition(pageIndex);
            return ((float)pos.x, (float)pos.y);
        }

        public override void vPageGoto(int pageIndex)
        {
            if (mDocument == null || !mDocument.IsOpened)
                return;

            if (pageIndex < 0 || pageIndex >= mDocument.PageCount)
                return;

            int oldIndex = _currentPageIndex;

            _currentBasePage = (pageIndex / 2) * 2;
            _currentPageIndex = pageIndex;

            InitializeLayout();
            RenderCurrentPages();

            RaiseCurrentPageChanged(oldIndex, _currentPageIndex);
        }

        public override void vRefresh()
        {
            RenderCurrentPages();
        }

        public override void vSetZoom(float zoomLevel)
        {
            if (Math.Abs(ZoomLevel - zoomLevel) < 0.001f)
                return;

            ZoomLevel = zoomLevel;
            InitializeLayout();
            RenderCurrentPages();
            InvalidatePage(CurrentPageIndex);
            InvalidatePage(CurrentPageIndex + 1);
        }

        public override void InvalidatePage(int pageIndex)
        {
            if (_pageContainers.ContainsKey(pageIndex))
            {
                _ = RenderPageAsync(pageIndex);
            }
        }

        public override void InvalidateAll()
        {
            RenderCurrentPages();
        }

        private void RenderCurrentPages()
        {
            if (mDocument == null || !mDocument.IsOpened || _layoutManager == null)
                return;

            ClearAllPages();

            int leftPage = _currentBasePage;
            int rightPage = _currentBasePage + 1;

            if (leftPage < mDocument.PageCount)
            {
                CreatePageContainer(leftPage);
            }

            if (rightPage < mDocument.PageCount)
            {
                CreatePageContainer(rightPage);
            }
        }

        private void CreatePageContainer(int pageIndex)
        {
            if (_layoutManager == null)
                return;

            var pagePos = _layoutManager.GetPagePosition(pageIndex);
            float scale = GetCurrentScale();
            float pageWidth = vPageGetWidth(pageIndex) * scale;
            float pageHeight = vPageGetHeight(pageIndex) * scale;

            var container = new PageContainer
            {
                PageIndex = pageIndex
            };

            Canvas.SetLeft(container, pagePos.x);
            Canvas.SetTop(container, pagePos.y);
            container.Width = pageWidth;
            container.Height = pageHeight;

            PageCanvas.Children.Add(container);
            _pageContainers[pageIndex] = container;

            _ = RenderPageAsync(pageIndex);
        }

        private void ClearAllPages()
        {
            foreach (var container in _pageContainers.Values)
            {
                PageCanvas.Children.Remove(container);
            }
            _pageContainers.Clear();
        }

        private void CancelAllRenders()
        {
            foreach (var cts in _renderCancellationTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _renderCancellationTokens.Clear();
        }

        private async Task RenderPageAsync(int pageIndex)
        {
            if (_isResizing)
                return;

            if (mDocument == null || !mDocument.IsOpened || _renderService == null)
                return;

            if (mDocument is not DOCXDocumentWrapper docxWrapper)
                return;

            if (!_pageContainers.ContainsKey(pageIndex))
                return;

            if (_renderCancellationTokens.TryGetValue(pageIndex, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            _renderCancellationTokens[pageIndex] = cts;

            try
            {
                var page = docxWrapper.InnerDoc.GetPage(pageIndex);
                if (page == null)
                    return;

                float pageWidth = vPageGetWidth(pageIndex);
                float pageHeight = vPageGetHeight(pageIndex);
                _scale = GetCurrentScale();

                int renderWidth = (int)(pageWidth * _scale);
                int renderHeight = (int)(pageHeight * _scale);

                var options = new RenderOptions
                {
                    Scale = _scale,
                    RenderMode = RDUILib.RD_RENDER_MODE.mode_best,
                    ShowAnnotations = false
                };

                // Try to get from page cache first
                string cacheKey = _renderService.GenerateCacheKey(pageIndex, renderWidth, renderHeight, options);
                var cachedBitmap = _renderService.GetCachedPage(cacheKey);

                WriteableBitmap? bitmap = cachedBitmap;

                // Cache miss - render using tiled approach with progressive display
                if (bitmap == null && !cts.Token.IsCancellationRequested)
                {
                    bitmap = new WriteableBitmap(renderWidth, renderHeight);
                    if (_pageContainers.TryGetValue(pageIndex, out var container))
                    {
                        container.PageImageControl.Source = bitmap;
                    }

                    var targetBitmap = bitmap;
                    int bitmapWidth = renderWidth;

                    await _renderService.RenderPageTiledAsync(
                        pageIndex, page, renderWidth, renderHeight, options,
                        tileCallback: (result) =>
                        {
                            if (!result.Success || result.PixelData == null || cts.Token.IsCancellationRequested)
                                return;
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                if (cts.Token.IsCancellationRequested) return;
                                try
                                {
                                    var tile = result.Tile;
                                    using (var stream = targetBitmap.PixelBuffer.AsStream())
                                    {
                                        int bytesPerPixel = 4;
                                        int tileStride = tile.Width * bytesPerPixel;
                                        int bmpStride = bitmapWidth * bytesPerPixel;
                                        for (int row = 0; row < tile.Height; row++)
                                        {
                                            stream.Seek((tile.Y + row) * bmpStride + tile.X * bytesPerPixel, SeekOrigin.Begin);
                                            stream.Write(result.PixelData, row * tileStride, tileStride);
                                        }
                                    }
                                    targetBitmap.Invalidate();
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error writing tile to bitmap: {ex.Message}");
                                }
                            });
                        },
                        cancellationToken: cts.Token
                    );

                    if (!cts.Token.IsCancellationRequested)
                    {
                        _renderService.CacheRenderedPage(cacheKey, bitmap);
                    }
                }
                else if (bitmap != null && !cts.Token.IsCancellationRequested && _pageContainers.TryGetValue(pageIndex, out var cachedContainer))
                {
                    cachedContainer.PageImageControl.Source = bitmap;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_renderCancellationTokens.TryGetValue(pageIndex, out var currentCts) && currentCts == cts)
                {
                    _renderCancellationTokens.Remove(pageIndex);
                }
                cts.Dispose();
            }
        }

        private void PageCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(PageCanvas);
            float screenX = (float)point.Position.X;
            float screenY = (float)point.Position.Y;

            int pageIndex = GetPageAtPoint(screenX, screenY);
            if (pageIndex < 0)
                return;

            _pointPressed = true;

            // Start drag-to-scroll
            if (DragScrollEnabled)
            {
                var svPoint = e.GetCurrentPoint(MainScrollViewer);
                _dragStartX = svPoint.Position.X;
                _dragStartY = svPoint.Position.Y;
                _dragStartScrollX = MainScrollViewer.HorizontalOffset;
                _dragStartScrollY = MainScrollViewer.VerticalOffset;
                _dragPointerId = point.PointerId;
                _isDragScrolling = false;
                PageCanvas.CapturePointer(e.Pointer);
            }
        }

        private void PageCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_pointPressed)
                return;

            var point = e.GetCurrentPoint(PageCanvas);

            // Handle drag-to-scroll
            if (DragScrollEnabled && _dragPointerId == point.PointerId && point.Properties.IsLeftButtonPressed)
            {
                var svPoint = e.GetCurrentPoint(MainScrollViewer);
                double deltaX = _dragStartX - svPoint.Position.X;
                double deltaY = _dragStartY - svPoint.Position.Y;

                if (!_isDragScrolling && (Math.Abs(deltaX) > 5 || Math.Abs(deltaY) > 5))
                {
                    _isDragScrolling = true;
                }

                if (_isDragScrolling)
                {
                    double newScrollX = _dragStartScrollX + deltaX;
                    double newScrollY = _dragStartScrollY + deltaY;
                    MainScrollViewer.ChangeView(newScrollX, newScrollY, null, true);
                    return;
                }
            }
        }

        private void PageCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _pointPressed = false;
            _isDragScrolling = false;
            _dragPointerId = null;
            if (DragScrollEnabled)
                PageCanvas.ReleasePointerCaptures();
        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                _isResizing = true;
                CancelAllRenders();
                _resizeDebounceTimer?.Stop();
                _resizeDebounceTimer?.Start();
            }
        }

        private void OnResizeDebounceTimerTick(object? sender, object e)
        {
            _resizeDebounceTimer?.Stop();
            _isResizing = false;

            _renderService?.ClearCache();
            _renderService?.ClearTileCache();

            InitializeLayout();
            RenderCurrentPages();
        }
    }
}
