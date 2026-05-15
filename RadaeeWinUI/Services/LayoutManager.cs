using System;
using System.Collections.Generic;
using RadaeeWinUI.Models;

namespace RadaeeWinUI.Services
{
    public class LayoutManager : ILayoutManager
    {
        private ViewMode _currentViewMode = ViewMode.SinglePage;
        private double _pageSpacing = 10.0;
        private int _totalPages;
        private double _containerWidth;
        private double _containerHeight;
        private Dictionary<int, (double width, double height)> _pageSizes = new();

        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set => _currentViewMode = value;
        }

        public double PageSpacing
        {
            get => _pageSpacing;
            set => _pageSpacing = value;
        }

        public void Initialize(int totalPages, double containerWidth, double containerHeight)
        {
            _totalPages = totalPages;
            _containerWidth = containerWidth;
            _containerHeight = containerHeight;
            _pageSizes.Clear();
        }

        public void UpdatePageSize(int pageIndex, double width, double height)
        {
            _pageSizes[pageIndex] = (width, height);
        }

        public List<PageLayoutInfo> CalculateLayout(double scrollOffsetX, double scrollOffsetY, double viewportWidth, double viewportHeight)
        {
            return _currentViewMode switch
            {
                ViewMode.VerticalContinuous => CalculateVerticalContinuousLayout(scrollOffsetY, viewportHeight),
                ViewMode.HorizontalContinuous => CalculateHorizontalContinuousLayout(scrollOffsetX, viewportWidth),
                ViewMode.DualPage => CalculateDualPageLayout(scrollOffsetY, viewportHeight),
                ViewMode.DualPageContinuous => CalculateDualPageContinuousLayout(scrollOffsetY, viewportHeight),
                _ => CalculateSinglePageLayout()
            };
        }

        public (double width, double height) GetTotalSize()
        {
            return _currentViewMode switch
            {
                ViewMode.VerticalContinuous => GetVerticalContinuousTotalSize(),
                ViewMode.HorizontalContinuous => GetHorizontalContinuousTotalSize(),
                ViewMode.DualPage => GetDualPageTotalSize(),
                ViewMode.DualPageContinuous => GetDualPageContinuousTotalSize(),
                _ => (0, 0)
            };
        }

        public (double x, double y) GetPagePosition(int pageIndex)
        {
            return _currentViewMode switch
            {
                ViewMode.VerticalContinuous => GetVerticalContinuousPagePosition(pageIndex),
                ViewMode.HorizontalContinuous => GetHorizontalContinuousPagePosition(pageIndex),
                ViewMode.DualPage => GetDualPagePosition(pageIndex),
                ViewMode.DualPageContinuous => GetDualPageContinuousPosition(pageIndex),
                _ => (0, 0)
            };
        }

        private List<PageLayoutInfo> CalculateSinglePageLayout()
        {
            return new List<PageLayoutInfo>();
        }

        private List<PageLayoutInfo> CalculateVerticalContinuousLayout(double scrollOffsetY, double viewportHeight)
        {
            var visiblePages = new List<PageLayoutInfo>();
            double currentY = 0;

            for (int i = 0; i < _totalPages; i++)
            {
                if (!_pageSizes.TryGetValue(i, out var size))
                {
                    size = (612, 792);
                }

                double pageHeight = size.height;
                double pageBottom = currentY + pageHeight;

                bool isVisible = pageBottom >= scrollOffsetY - viewportHeight &&
                                currentY <= scrollOffsetY + viewportHeight * 2;

                if (isVisible)
                {
                    visiblePages.Add(new PageLayoutInfo
                    {
                        PageIndex = i,
                        X = (_containerWidth - size.width) / 2,
                        Y = currentY,
                        Width = size.width,
                        Height = size.height,
                        IsVisible = true
                    });
                }

                currentY += pageHeight + _pageSpacing;
            }

            return visiblePages;
        }

        private List<PageLayoutInfo> CalculateHorizontalContinuousLayout(double scrollOffsetX, double viewportWidth)
        {
            var visiblePages = new List<PageLayoutInfo>();
            double currentX = 0;

            for (int i = 0; i < _totalPages; i++)
            {
                if (!_pageSizes.TryGetValue(i, out var size))
                {
                    size = (612, 792);
                }

                double pageWidth = size.width;
                double pageRight = currentX + pageWidth;

                bool isVisible = pageRight >= scrollOffsetX - viewportWidth &&
                                currentX <= scrollOffsetX + viewportWidth * 2;

                if (isVisible)
                {
                    visiblePages.Add(new PageLayoutInfo
                    {
                        PageIndex = i,
                        X = currentX,
                        Y = (_containerHeight - size.height) / 2,
                        Width = size.width,
                        Height = size.height,
                        IsVisible = true
                    });
                }

                currentX += pageWidth + _pageSpacing;
            }

            return visiblePages;
        }

        private List<PageLayoutInfo> CalculateDualPageLayout(double scrollOffsetY, double viewportHeight)
        {
            var visiblePages = new List<PageLayoutInfo>();
            
            return visiblePages;
        }

        private List<PageLayoutInfo> CalculateDualPageContinuousLayout(double scrollOffsetY, double viewportHeight)
        {
            var visiblePages = new List<PageLayoutInfo>();
            double currentY = 0;

            for (int i = 0; i < _totalPages; i += 2)
            {
                if (!_pageSizes.TryGetValue(i, out var leftSize))
                {
                    leftSize = (612, 792);
                }

                double maxHeight = leftSize.height;
                double totalWidth = leftSize.width;

                if (i + 1 < _totalPages)
                {
                    if (!_pageSizes.TryGetValue(i + 1, out var rightSize))
                    {
                        rightSize = (612, 792);
                    }
                    maxHeight = Math.Max(maxHeight, rightSize.height);
                    totalWidth += _pageSpacing + rightSize.width;
                }

                double pageBottom = currentY + maxHeight;
                bool isVisible = pageBottom >= scrollOffsetY - viewportHeight &&
                                currentY <= scrollOffsetY + viewportHeight * 2;

                if (isVisible)
                {
                    double startX = (_containerWidth - totalWidth) / 2;

                    visiblePages.Add(new PageLayoutInfo
                    {
                        PageIndex = i,
                        X = startX,
                        Y = currentY,
                        Width = leftSize.width,
                        Height = leftSize.height,
                        IsVisible = true
                    });

                    if (i + 1 < _totalPages)
                    {
                        if (!_pageSizes.TryGetValue(i + 1, out var rightSize))
                        {
                            rightSize = (612, 792);
                        }

                        visiblePages.Add(new PageLayoutInfo
                        {
                            PageIndex = i + 1,
                            X = startX + leftSize.width + _pageSpacing,
                            Y = currentY,
                            Width = rightSize.width,
                            Height = rightSize.height,
                            IsVisible = true
                        });
                    }
                }

                currentY += maxHeight + _pageSpacing;
            }

            return visiblePages;
        }

        private (double width, double height) GetVerticalContinuousTotalSize()
        {
            double totalHeight = 0;
            double maxWidth = 0;

            for (int i = 0; i < _totalPages; i++)
            {
                if (!_pageSizes.TryGetValue(i, out var size))
                {
                    size = (612, 792);
                }

                totalHeight += size.height;
                if (i < _totalPages - 1)
                {
                    totalHeight += _pageSpacing;
                }
                maxWidth = Math.Max(maxWidth, size.width);
            }

            return (maxWidth, totalHeight);
        }

        private (double width, double height) GetHorizontalContinuousTotalSize()
        {
            double totalWidth = 0;
            double maxHeight = 0;

            for (int i = 0; i < _totalPages; i++)
            {
                if (!_pageSizes.TryGetValue(i, out var size))
                {
                    size = (612, 792);
                }

                totalWidth += size.width;
                if (i < _totalPages - 1)
                {
                    totalWidth += _pageSpacing;
                }
                maxHeight = Math.Max(maxHeight, size.height);
            }

            return (totalWidth, maxHeight);
        }

        private (double width, double height) GetDualPageTotalSize()
        {
            if (_totalPages == 0)
                return (0, 0);

            // Find the actual left page index from registered page sizes
            int leftIdx = -1;
            foreach (var key in _pageSizes.Keys)
            {
                if (key % 2 == 0)
                {
                    leftIdx = key;
                    break;
                }
            }
            if (leftIdx < 0)
            {
                // Fallback: use first registered page
                foreach (var key in _pageSizes.Keys)
                {
                    leftIdx = key;
                    break;
                }
            }
            if (leftIdx < 0)
                return (_containerWidth, _containerHeight);

            int rightIdx = leftIdx + 1;

            if (!_pageSizes.TryGetValue(leftIdx, out var leftSize))
            {
                leftSize = (612, 792);
            }

            double maxHeight = leftSize.height;
            double totalWidth = leftSize.width;

            if (rightIdx < _totalPages)
            {
                if (!_pageSizes.TryGetValue(rightIdx, out var rightSize))
                {
                    rightSize = (612, 792);
                }
                maxHeight = Math.Max(maxHeight, rightSize.height);
                totalWidth += _pageSpacing + rightSize.width;
            }

            return (totalWidth, maxHeight);
        }

        private (double width, double height) GetDualPageContinuousTotalSize()
        {
            double totalHeight = 0;
            double maxWidth = 0;

            for (int i = 0; i < _totalPages; i += 2)
            {
                if (!_pageSizes.TryGetValue(i, out var leftSize))
                {
                    leftSize = (612, 792);
                }

                double pairHeight = leftSize.height;
                double pairWidth = leftSize.width;

                if (i + 1 < _totalPages)
                {
                    if (!_pageSizes.TryGetValue(i + 1, out var rightSize))
                    {
                        rightSize = (612, 792);
                    }
                    pairHeight = Math.Max(pairHeight, rightSize.height);
                    pairWidth += _pageSpacing + rightSize.width;
                }

                totalHeight += pairHeight;
                if (i + 2 < _totalPages)
                {
                    totalHeight += _pageSpacing;
                }
                maxWidth = Math.Max(maxWidth, pairWidth);
            }

            return (maxWidth, totalHeight);
        }

        private (double x, double y) GetVerticalContinuousPagePosition(int pageIndex)
        {
            double y = 0;
            for (int i = 0; i < pageIndex && i < _totalPages; i++)
            {
                if (!_pageSizes.TryGetValue(i, out var size))
                {
                    size = (612, 792);
                }
                y += size.height + _pageSpacing;
            }

            if (_pageSizes.TryGetValue(pageIndex, out var pageSize))
            {
                return ((_containerWidth - pageSize.width) / 2, y);
            }

            return (0, y);
        }

        private (double x, double y) GetHorizontalContinuousPagePosition(int pageIndex)
        {
            double x = 0;
            for (int i = 0; i < pageIndex && i < _totalPages; i++)
            {
                if (!_pageSizes.TryGetValue(i, out var size))
                {
                    size = (612, 792);
                }
                x += size.width + _pageSpacing;
            }

            if (_pageSizes.TryGetValue(pageIndex, out var pageSize))
            {
                return (x, (_containerHeight - pageSize.height) / 2);
            }

            return (x, 0);
        }

        private (double x, double y) GetDualPagePosition(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= _totalPages)
                return (0, 0);

            int leftIdx = (pageIndex / 2) * 2;
            int rightIdx = leftIdx + 1;

            if (!_pageSizes.TryGetValue(leftIdx, out var leftSize))
            {
                leftSize = (612, 792);
            }

            double totalWidth = leftSize.width;
            if (rightIdx < _totalPages)
            {
                if (!_pageSizes.TryGetValue(rightIdx, out var rightSize))
                {
                    rightSize = (612, 792);
                }
                totalWidth += _pageSpacing + rightSize.width;
            }

            double startX = (_containerWidth - totalWidth) / 2;
            double y = 0;

            if (pageIndex % 2 == 0)
            {
                return (startX, y);
            }
            else
            {
                return (startX + leftSize.width + _pageSpacing, y);
            }
        }

        private (double x, double y) GetDualPageContinuousPosition(int pageIndex)
        {
            double y = 0;
            int pairIndex = pageIndex / 2;

            for (int i = 0; i < pairIndex; i++)
            {
                int leftIdx = i * 2;
                if (!_pageSizes.TryGetValue(leftIdx, out var leftSize))
                {
                    leftSize = (612, 792);
                }

                double pairHeight = leftSize.height;
                if (leftIdx + 1 < _totalPages)
                {
                    if (!_pageSizes.TryGetValue(leftIdx + 1, out var rightSize))
                    {
                        rightSize = (612, 792);
                    }
                    pairHeight = Math.Max(pairHeight, rightSize.height);
                }

                y += pairHeight + _pageSpacing;
            }

            int currentLeftIdx = pairIndex * 2;
            if (!_pageSizes.TryGetValue(currentLeftIdx, out var currentLeftSize))
            {
                currentLeftSize = (612, 792);
            }

            double totalWidth = currentLeftSize.width;
            if (currentLeftIdx + 1 < _totalPages)
            {
                if (!_pageSizes.TryGetValue(currentLeftIdx + 1, out var currentRightSize))
                {
                    currentRightSize = (612, 792);
                }
                totalWidth += _pageSpacing + currentRightSize.width;
            }

            double startX = (_containerWidth - totalWidth) / 2;

            if (pageIndex % 2 == 0)
            {
                return (startX, y);
            }
            else
            {
                return (startX + currentLeftSize.width + _pageSpacing, y);
            }
        }
    }
}
