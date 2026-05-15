using RadaeeWinUI.Controls.DOCXView;
using RadaeeWinUI.Helpers;
using RadaeeWinUI.Models;
using RadaeeWinUI.Services;
using System;
using System.Diagnostics;

namespace RadaeeWinUI.ViewModels
{
    public class DOCXViewModel : ObservableObject
    {
        private readonly IDOCXRenderService _renderService;
        private readonly ILayoutManager _layoutManager;
        private IDocument? _currentDocument;
        private DOCXView? _currentDOCXView;
        private ViewMode _viewMode = ViewMode.SinglePage;

        public event EventHandler<PageChangedEventArgs>? CurrentPageChanged;

        public DOCXViewModel(IDOCXRenderService renderService, ILayoutManager layoutManager)
        {
            _renderService = renderService;
            _layoutManager = layoutManager;
        }

        public DOCXView? CurrentDOCXView
        {
            get => _currentDOCXView;
            private set => SetProperty(ref _currentDOCXView, value);
        }

        public ViewMode ViewMode
        {
            get => _viewMode;
            set => SetProperty(ref _viewMode, value);
        }

        public void SwitchViewMode(ViewMode mode)
        {
            if (CurrentDOCXView != null)
            {
                CurrentDOCXView.CurrentPageChanged -= OnCurrentPageChanged;
                CurrentDOCXView.DOCXVClose();
            }

            DOCXView? newView = mode switch
            {
                ViewMode.SinglePage => CreateSinglePageView(),
                ViewMode.VerticalContinuous => CreateVerticalScrollView(),
                ViewMode.HorizontalContinuous => CreateHorizontalScrollView(),
                ViewMode.DualPage => CreateDualPageView(),
                ViewMode.DualPageContinuous => CreateDualPageView(),
                _ => CreateVerticalScrollView()
            };

            if (newView != null)
            {
                InitializeView(newView, mode);
            }
        }

        private DOCXSinglePageView CreateSinglePageView()
        {
            var view = new DOCXSinglePageView();
            view.SetRenderService(_renderService);
            return view;
        }

        private DOCXVerticalScrollView CreateVerticalScrollView()
        {
            var view = new DOCXVerticalScrollView();
            view.SetRenderService(_renderService);
            view.SetLayoutManager(_layoutManager);
            return view;
        }

        private DOCXHorizontalScrollView CreateHorizontalScrollView()
        {
            var view = new DOCXHorizontalScrollView();
            view.SetRenderService(_renderService);
            view.SetLayoutManager(_layoutManager);
            return view;
        }

        private DOCXDualPageView CreateDualPageView()
        {
            var view = new DOCXDualPageView();
            view.SetRenderService(_renderService);
            view.SetLayoutManager(_layoutManager);
            return view;
        }

        private void InitializeView(DOCXView view, ViewMode mode)
        {
            if (_currentDocument != null && _currentDocument.IsOpened)
            {
                view.DOCXVOpen(_currentDocument);
            }

            view.CurrentPageChanged += OnCurrentPageChanged;
            CurrentDOCXView = view;
            ViewMode = mode;
        }

        private void OnCurrentPageChanged(object? sender, PageChangedEventArgs e)
        {
            Debug.WriteLine($"DOCX page changed, old: {e.OldPageIndex}; new: {e.NewPageIndex}");
            CurrentPageChanged?.Invoke(this, e);
        }

        public void OnDocumentLoaded(IDocument doc)
        {
            _currentDocument = doc;
            if (CurrentDOCXView != null && doc != null && doc.IsOpened)
            {
                CurrentDOCXView.DOCXVOpen(doc);
            }
        }

        public void OnDocumentClosed()
        {
            if (CurrentDOCXView != null)
            {
                CurrentDOCXView.DOCXVClose();
            }
            _currentDocument = null;
            _renderService.ClearCache();
            _renderService.ClearTileCache();
        }
    }
}
