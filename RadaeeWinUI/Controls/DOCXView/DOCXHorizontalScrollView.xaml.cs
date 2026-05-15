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
    public sealed partial class DOCXHorizontalScrollView : DOCXView
    {
        private IDOCXRenderService? _renderService;
        private ILayoutManager? _layoutManager;
        private Dictionary<int, PageContainer> _pageContainers = new();
        private Dictionary<int, CancellationTokenSource> _renderCancellationTokens = new();
        private List<PageLayoutInfo> _visiblePages = new();
        private DispatcherTimer? _scrollDebounceTimer;
        private DispatcherTimer? _resizeDebounceTimer;
        private double _lastScrollOffset = 0;
        private double _lastViewportHeight = 0;
        private bool _isLoaded = false;
        private bool _needsInitialization = false;
        private bool _isResizing = false;
        private GestureRecognizer? _gestureRecognizer;

        public DOCXHorizontalScrollView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            InitializeScrollDebounceTimer();
            InitializeResizeDebounceTimer();
            InitializeGestureRecognizer();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            if (_needsInitialization && mDocument != null && mDocument.IsOpened)
            {
                _needsInitialization = false;
                InitializeLayout();
                UpdateVisiblePages();
            }
        }

        private void InitializeScrollDebounceTimer()
        {
            _scrollDebounceTimer = new DispatcherTimer();
            _scrollDebounceTimer.Interval = TimeSpan.FromMilliseconds(100);
            _scrollDebounceTimer.Tick += OnScrollDebounceTimerTick;
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
                _currentPageIndex = 0;

                if (_isLoaded)
                {
                    InitializeLayout();
                    UpdateVisiblePages();
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
            _scrollDebounceTimer?.Stop();
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

            _layoutManager.CurrentViewMode = ViewMode.HorizontalContinuous;
            _layoutManager.Initialize(mDocument.PageCount, containerWidth, containerHeight);

            for (int i = 0; i < mDocument.PageCount; i++)
            {
                float pageWidth = vPageGetWidth(i);
                float pageHeight = vPageGetHeight(i);

                float scale = (float)(containerHeight / pageHeight);

                _layoutManager.UpdatePageSize(i, pageWidth * ZoomLevel * scale, pageHeight * ZoomLevel * scale);
            }

            var totalSize = _layoutManager.GetTotalSize();
            PageCanvas.Width = totalSize.width;
            PageCanvas.Height = Math.Max(totalSize.height, containerHeight);
        }

        private float GetCurrentScale()
        {
            if (mDocument == null || !mDocument.IsOpened)
                return 1.0f;

            double viewportHeight = MainScrollViewer.ViewportHeight > 0 ? MainScrollViewer.ViewportHeight : (ActualHeight > 0 ? ActualHeight : 600);
            float pageHeight = vPageGetHeight(CurrentPageIndex);

            return (float)(viewportHeight / pageHeight) * ZoomLevel;
        }

        private float GetPageScale(int pageIndex)
        {
            if (mDocument == null || !mDocument.IsOpened)
                return 1.0f;

            double viewportHeight = MainScrollViewer.ViewportHeight > 0 ? MainScrollViewer.ViewportHeight : (ActualHeight > 0 ? ActualHeight : 600);
            float pageHeight = vPageGetHeight(pageIndex);

            return (float)(viewportHeight / pageHeight) * ZoomLevel;
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
            foreach (var pageInfo in _visiblePages)
            {
                if (screenX >= pageInfo.X && screenX <= pageInfo.X + pageInfo.Width &&
                    screenY >= pageInfo.Y && screenY <= pageInfo.Y + pageInfo.Height)
                {
                    return pageInfo.PageIndex;
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
            if (mDocument == null || !mDocument.IsOpened || _layoutManager == null)
                return;

            if (pageIndex < 0 || pageIndex >= mDocument.PageCount)
                return;

            int oldIndex = _currentPageIndex;
            _currentPageIndex = pageIndex;

            var pagePos = _layoutManager.GetPagePosition(pageIndex);

            double containerHeight = MainScrollViewer.ViewportHeight > 0 ? MainScrollViewer.ViewportHeight : (ActualHeight > 0 ? ActualHeight : 600);
            float pageWidth = vPageGetWidth(pageIndex);
            float pageHeight = vPageGetHeight(pageIndex);
            float scale = (float)(containerHeight / pageHeight);
            double scaledPageWidth = pageWidth * ZoomLevel * scale;

            double targetScrollX = pagePos.x + (scaledPageWidth / 2) - (MainScrollViewer.ViewportWidth / 2);
            targetScrollX = Math.Max(0, targetScrollX);

            MainScrollViewer.ChangeView(targetScrollX, null, null, false);

            UpdateVisiblePages();
            RaiseCurrentPageChanged(oldIndex, _currentPageIndex);
        }

        public override void vRefresh()
        {
            UpdateVisiblePages();
        }

        public override void vSetZoom(float zoomLevel)
        {
            if (Math.Abs(ZoomLevel - zoomLevel) < 0.001f)
                return;

            ZoomLevel = zoomLevel;
            InitializeLayout();
            UpdateVisiblePages();
            _renderService?.ClearCache();
            _renderService?.ClearTileCache();
            InvalidatePage(CurrentPageIndex);

            RaiseCurrentPageChanged(_currentPageIndex, _currentPageIndex);
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
            UpdateVisiblePages();
        }

        private void UpdateVisiblePages()
        {
            if (mDocument == null || !mDocument.IsOpened || _layoutManager == null)
                return;

            double scrollOffsetX = MainScrollViewer.HorizontalOffset;
            double viewportWidth = MainScrollViewer.ViewportWidth;
            double viewportHeight = MainScrollViewer.ViewportHeight;

            var newVisiblePages = _layoutManager.CalculateLayout(scrollOffsetX, 0, viewportWidth, viewportHeight);

            var pagesToRemove = new List<int>();
            foreach (var pageIndex in _pageContainers.Keys)
            {
                bool isStillVisible = false;
                foreach (var pageInfo in newVisiblePages)
                {
                    if (pageInfo.PageIndex == pageIndex)
                    {
                        isStillVisible = true;
                        break;
                    }
                }

                if (!isStillVisible)
                {
                    pagesToRemove.Add(pageIndex);
                }
            }

            foreach (var pageIndex in pagesToRemove)
            {
                RemovePage(pageIndex);
            }

            _visiblePages = newVisiblePages;

            foreach (var pageInfo in _visiblePages)
            {
                if (!_pageContainers.ContainsKey(pageInfo.PageIndex))
                {
                    CreatePageContainer(pageInfo);
                }
                else
                {
                    UpdatePageContainerPosition(pageInfo);
                }
            }

            UpdateCurrentPage(scrollOffsetX);
        }

        private void CreatePageContainer(PageLayoutInfo pageInfo)
        {
            var container = new PageContainer
            {
                PageIndex = pageInfo.PageIndex
            };

            Canvas.SetLeft(container, pageInfo.X);
            Canvas.SetTop(container, pageInfo.Y);
            container.Width = pageInfo.Width;
            container.Height = pageInfo.Height;

            PageCanvas.Children.Add(container);
            _pageContainers[pageInfo.PageIndex] = container;

            _ = RenderPageAsync(pageInfo.PageIndex);
        }

        private void UpdatePageContainerPosition(PageLayoutInfo pageInfo)
        {
            if (_pageContainers.TryGetValue(pageInfo.PageIndex, out var container))
            {
                Canvas.SetLeft(container, pageInfo.X);
                Canvas.SetTop(container, pageInfo.Y);
                container.Width = pageInfo.Width;
                container.Height = pageInfo.Height;
            }
        }

        private void RemovePage(int pageIndex)
        {
            if (_pageContainers.TryGetValue(pageIndex, out var container))
            {
                PageCanvas.Children.Remove(container);
                _pageContainers.Remove(pageIndex);
            }

            if (_renderCancellationTokens.TryGetValue(pageIndex, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _renderCancellationTokens.Remove(pageIndex);
            }
        }

        private void ClearAllPages()
        {
            foreach (var container in _pageContainers.Values)
            {
                PageCanvas.Children.Remove(container);
            }
            _pageContainers.Clear();
            _visiblePages.Clear();
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

                double viewportHeight = MainScrollViewer.ViewportHeight > 0 ? MainScrollViewer.ViewportHeight : (ActualHeight > 0 ? ActualHeight : 600);

                _scale = (float)(viewportHeight / pageHeight);

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

        private void UpdateCurrentPage(double scrollOffsetX)
        {
            if (mDocument == null || !mDocument.IsOpened)
                return;

            double viewportCenter = scrollOffsetX + MainScrollViewer.ViewportWidth / 2;

            int newCurrentPage = _currentPageIndex;
            double minDistance = double.MaxValue;

            foreach (var pageInfo in _visiblePages)
            {
                double pageCenter = pageInfo.X + pageInfo.Width / 2;
                double distance = Math.Abs(pageCenter - viewportCenter);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    newCurrentPage = pageInfo.PageIndex;
                }
            }

            if (newCurrentPage != _currentPageIndex)
            {
                int oldIndex = _currentPageIndex;
                _currentPageIndex = newCurrentPage;
                RaiseCurrentPageChanged(oldIndex, _currentPageIndex);
            }
        }

        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            double currentScrollOffset = MainScrollViewer.HorizontalOffset;

            if (Math.Abs(currentScrollOffset - _lastScrollOffset) > 1.0)
            {
                _lastScrollOffset = currentScrollOffset;
                _scrollDebounceTimer?.Stop();
                _scrollDebounceTimer?.Start();
            }
        }

        private void OnScrollDebounceTimerTick(object? sender, object e)
        {
            _scrollDebounceTimer?.Stop();
            UpdateVisiblePages();
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

            double currentViewportHeight = MainScrollViewer.ViewportHeight;
            _lastViewportHeight = currentViewportHeight;

            _renderService?.ClearCache();
            _renderService?.ClearTileCache();

            InitializeLayout();

            var existingPages = new HashSet<int>(_pageContainers.Keys);

            UpdateVisiblePages();

            foreach (var pageIndex in existingPages)
            {
                if (_pageContainers.ContainsKey(pageIndex))
                {
                    _ = RenderPageAsync(pageIndex);
                }
            }
        }
    }
}
