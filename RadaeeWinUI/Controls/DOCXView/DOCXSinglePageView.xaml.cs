using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using RadaeeWinUI.Models;
using RadaeeWinUI.Services;
using RadaeeWinUI.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RadaeeWinUI.Controls.DOCXView
{
    public sealed partial class DOCXSinglePageView : DOCXView
    {
        private IDOCXRenderService? _renderService;
        private PageContainer? _pageContainer;
        private float _pageOffsetX;
        private float _pageOffsetY;
        private CancellationTokenSource? _renderCancellationTokenSource;
        private DispatcherTimer? _resizeDebounceTimer;
        private bool _isResizing = false;
        private GestureRecognizer? _gestureRecognizer;
        private bool _shouldRaisePageChanged = false;
        private int _oldPageIndex = 0;

        public DOCXSinglePageView()
        {
            InitializeComponent();
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

        public void SetRenderService(IDOCXRenderService renderService)
        {
            _renderService = renderService;
        }

        public override void DOCXVOpen(IDocument doc)
        {
            mDocument = doc;
            if (doc != null && doc.IsOpened)
            {
                _currentPageIndex = 0;
                _ = RenderCurrentPageAsync();
            }
        }

        public override List<int> GetVisiblePageIndices()
        {
            return new List<int> { _currentPageIndex };
        }

        public override void DOCXVClose()
        {
            CancelCurrentRender();
            _resizeDebounceTimer?.Stop();
            _gestureRecognizer?.Dispose();
            _gestureRecognizer = null;

            if (_pageContainer != null)
            {
                PageCanvas.Children.Remove(_pageContainer);
                _pageContainer = null;
            }

            mDocument = null;
            _renderService?.ClearCache();
            _renderService?.ClearTileCache();
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
            return _currentPageIndex;
        }

        public override (float x, float y) GetPagePosition(int pageIndex)
        {
            if (pageIndex != _currentPageIndex)
                return (0, 0);

            return (_pageOffsetX, _pageOffsetY);
        }

        public override void vPageGoto(int pageIndex)
        {
            if (mDocument == null || !mDocument.IsOpened)
                return;

            if (pageIndex < 0 || pageIndex >= mDocument.PageCount)
                return;

            if (pageIndex == _currentPageIndex)
                return;

            _oldPageIndex = _currentPageIndex;
            _currentPageIndex = pageIndex;
            _shouldRaisePageChanged = true;
            _ = RenderCurrentPageAsync();
        }

        public override void vRefresh()
        {
            _ = RenderCurrentPageAsync();
        }

        public override void vSetZoom(float zoomLevel)
        {
            ZoomLevel = zoomLevel;
            if (_renderService != null)
                _renderService.ClearCache();
            _ = RenderCurrentPageAsync();
        }

        public override void InvalidatePage(int pageIndex)
        {
            if (pageIndex == _currentPageIndex)
            {
                _ = RenderCurrentPageAsync();
            }
        }

        public override void InvalidateAll()
        {
            _ = RenderCurrentPageAsync();
        }

        private void CancelCurrentRender()
        {
            if (_renderCancellationTokenSource != null)
            {
                _renderCancellationTokenSource.Cancel();
                _renderCancellationTokenSource.Dispose();
                _renderCancellationTokenSource = null;
            }
        }

        private async Task RenderCurrentPageAsync()
        {
            if (_isResizing)
                return;

            if (mDocument == null || !mDocument.IsOpened || _renderService == null)
                return;

            if (mDocument is not DOCXDocumentWrapper docxWrapper)
                return;

            CancelCurrentRender();
            _renderCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _renderCancellationTokenSource.Token;

            try
            {
                var page = docxWrapper.InnerDoc.GetPage(_currentPageIndex);
                if (page == null)
                    return;

                float pageWidth = vPageGetWidth(_currentPageIndex);
                float pageHeight = vPageGetHeight(_currentPageIndex);

                double availableWidth = ActualWidth > 0 ? ActualWidth : 800;
                double availableHeight = ActualHeight > 0 ? ActualHeight : 600;

                _scale = (float)Math.Min(availableWidth / pageWidth, availableHeight / pageHeight);
                _scale *= ZoomLevel;

                int renderWidth = (int)(pageWidth * _scale);
                int renderHeight = (int)(pageHeight * _scale);

                var options = new RenderOptions
                {
                    Scale = _scale,
                    RenderMode = RDUILib.RD_RENDER_MODE.mode_best,
                    ShowAnnotations = false
                };

                // Try to get from cache first
                string cacheKey = _renderService.GenerateCacheKey(_currentPageIndex, renderWidth, renderHeight, options);
                var cachedBitmap = _renderService.GetCachedPage(cacheKey);
                WriteableBitmap? bitmap = cachedBitmap;

                // Setup container and page offset before rendering
                _pageOffsetX = (float)Math.Max(0, (availableWidth - renderWidth) / 2);
                _pageOffsetY = (float)Math.Max(0, (availableHeight - renderHeight) / 2);

                if (_pageContainer == null)
                {
                    _pageContainer = new PageContainer
                    {
                        PageIndex = _currentPageIndex
                    };
                    PageCanvas.Children.Add(_pageContainer);
                }
                else
                {
                    _pageContainer.PageIndex = _currentPageIndex;
                }

                _pageContainer.Width = renderWidth;
                _pageContainer.Height = renderHeight;
                Canvas.SetLeft(_pageContainer, _pageOffsetX);
                Canvas.SetTop(_pageContainer, _pageOffsetY);

                PageCanvas.Width = Math.Max(renderWidth, availableWidth);
                PageCanvas.Height = Math.Max(renderHeight, availableHeight);

                // Cache miss - render using tiled approach with progressive display
                if (bitmap == null && !cancellationToken.IsCancellationRequested)
                {
                    bitmap = new WriteableBitmap(renderWidth, renderHeight);
                    _pageContainer.PageImageControl.Source = bitmap;

                    var targetBitmap = bitmap;
                    int bitmapWidth = renderWidth;

                    await _renderService.RenderPageTiledAsync(
                        _currentPageIndex, page, renderWidth, renderHeight, options,
                        tileCallback: (result) =>
                        {
                            if (!result.Success || result.PixelData == null || cancellationToken.IsCancellationRequested)
                                return;
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                if (cancellationToken.IsCancellationRequested) return;
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
                        cancellationToken: cancellationToken
                    );

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _renderService.CacheRenderedPage(cacheKey, bitmap);
                    }
                }
                else if (bitmap != null && !cancellationToken.IsCancellationRequested)
                {
                    _pageContainer.PageImageControl.Source = bitmap;
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    if (_shouldRaisePageChanged)
                    {
                        _shouldRaisePageChanged = false;
                        RaiseCurrentPageChanged(_oldPageIndex, _currentPageIndex);
                    }
                    else
                    {
                        RaiseCurrentPageChanged(_currentPageIndex, _currentPageIndex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Render was cancelled, this is expected
            }
            finally
            {
                if (_renderCancellationTokenSource != null && !_renderCancellationTokenSource.IsCancellationRequested)
                {
                    _renderCancellationTokenSource.Dispose();
                    _renderCancellationTokenSource = null;
                }
            }
        }

        private void PageCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pointPressed = true;
            var point = e.GetCurrentPoint(PageCanvas);

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
                CancelCurrentRender();
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
            _ = RenderCurrentPageAsync();
        }
    }
}
