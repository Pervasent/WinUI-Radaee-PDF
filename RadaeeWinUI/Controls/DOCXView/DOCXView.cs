using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RadaeeWinUI.Models;
using System;
using System.Collections.Generic;

namespace RadaeeWinUI.Controls.DOCXView
{
    public abstract class DOCXView : UserControl
    {
        protected IDocument? mDocument;
        public float _zoomLevel = 1.0f;
        protected float _scale;
        private float _maxZoomLevel = 5.0f;
        private float _minZoomLevel = 0.5f;
        protected int _currentPageIndex = 0;
        protected Boolean _pointPressed = false;

        // Drag-to-scroll state
        protected bool _isDragScrolling = false;
        protected double _dragStartX;
        protected double _dragStartY;
        protected double _dragStartScrollX;
        protected double _dragStartScrollY;
        protected uint? _dragPointerId = null;

        public bool DragScrollEnabled { get; set; } = true;

        public int CurrentPageIndex => _currentPageIndex;

        public abstract List<int> GetVisiblePageIndices();

        public abstract void DOCXVOpen(IDocument doc);
        public abstract void DOCXVClose();

        public abstract float vPageGetWidth(int pageIndex);
        public abstract float vPageGetHeight(int pageIndex);
        public abstract int GetPageAtPoint(float screenX, float screenY);
        public abstract (float x, float y) GetPagePosition(int pageIndex);

        public abstract void vPageGoto(int pageIndex);
        public abstract void vRefresh();
        public abstract void vSetZoom(float zoomLevel);
        public abstract void InvalidatePage(int pageIndex);
        public abstract void InvalidateAll();
        protected abstract void OnSizeChanged(object sender, SizeChangedEventArgs e);

        public event EventHandler<PageChangedEventArgs>? CurrentPageChanged;

        public float ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (_zoomLevel != value && value <= _maxZoomLevel && value >= _minZoomLevel)
                {
                    _zoomLevel = value;
                    vSetZoom(_zoomLevel);
                }
            }
        }

        public float DOCXScale => _scale * ZoomLevel;

        protected void RaiseCurrentPageChanged(int oldIndex, int newIndex)
        {
            CurrentPageChanged?.Invoke(this, new PageChangedEventArgs
            {
                OldPageIndex = oldIndex,
                NewPageIndex = newIndex
            });
        }
    }
}
