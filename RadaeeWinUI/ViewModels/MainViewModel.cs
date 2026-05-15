using System;
using System.Threading.Tasks;
using RDUILib;
using RadaeeWinUI.Helpers;
using RadaeeWinUI.Models;
using RadaeeWinUI.Services;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace RadaeeWinUI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDocumentManager _documentManager;
        private readonly INavigationService _navigationService;
        private readonly PDFViewModel _pdfViewModel;
        private readonly DOCXViewModel _docxViewModel;

        private IDocument? _currentDocument;
        private DocumentType _currentDocumentType;
        private DocumentInfo? _documentInfo;
        private bool _isDocumentLoaded;
        private bool _hasAttachments;

        public MainViewModel(
            IDocumentManager documentManager,
            INavigationService navigationService,
            PDFViewModel pdfViewModel,
            DOCXViewModel docxViewModel)
        {
            _documentManager = documentManager;
            _navigationService = navigationService;
            _pdfViewModel = pdfViewModel;
            _docxViewModel = docxViewModel;

            _pdfViewModel.CurrentPageChanged += _pdfPageChanged;
            _docxViewModel.CurrentPageChanged += _docxPageChanged;

            OpenDocumentCommand = new AsyncRelayCommand(OpenDocumentAsync);
            NextPageCommand = new AsyncRelayCommand(GoToNextPageAsync, () => _navigationService.CanGoToNextPage);
            PreviousPageCommand = new AsyncRelayCommand(GoToPreviousPageAsync, () => _navigationService.CanGoToPreviousPage);
        }

        private void _pdfPageChanged(object? sender, PageChangedEventArgs e)
        {
            _navigationService.GoToPage(e.NewPageIndex);
            OnPropertyChanged(nameof(CurrentPageNumber));
        }

        private void _docxPageChanged(object? sender, PageChangedEventArgs e)
        {
            _navigationService.GoToPage(e.NewPageIndex);
            OnPropertyChanged(nameof(CurrentPageNumber));
        }

        public AsyncRelayCommand OpenDocumentCommand { get; }
        public AsyncRelayCommand NextPageCommand { get; }
        public AsyncRelayCommand PreviousPageCommand { get; }

        public PDFViewModel PDFViewModel => _pdfViewModel;
        public DOCXViewModel DOCXViewModel => _docxViewModel;

        public DocumentType CurrentDocumentType
        {
            get => _currentDocumentType;
            private set => SetProperty(ref _currentDocumentType, value);
        }

        public DocumentInfo? DocumentInfo
        {
            get => _documentInfo;
            set => SetProperty(ref _documentInfo, value);
        }

        public bool IsDocumentLoaded
        {
            get => _isDocumentLoaded;
            set => SetProperty(ref _isDocumentLoaded, value);
        }

        public bool HasAttachments
        {
            get => _hasAttachments;
            set => SetProperty(ref _hasAttachments, value);
        }

        public int CurrentPageNumber => _navigationService.CurrentPageIndex + 1;
        public int CurrentPageIndex => _navigationService.CurrentPageIndex;
        public int TotalPages => _navigationService.TotalPages;

        public PDFPage? CurrentPage
        {
            get
            {
                if (_currentDocument == null || !_currentDocument.IsOpened)
                    return null;
                if (_currentDocument is PDFDocumentWrapper pdfWrapper)
                    return pdfWrapper.InnerDoc.GetPage(_navigationService.CurrentPageIndex);
                return null;
            }
        }

        public async Task OpenDocumentAsync()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".pdf");
            picker.FileTypeFilter.Add(".docx");

            var window = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile? file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await LoadDocumentAsync(file);
            }
        }

        private async Task LoadDocumentAsync(StorageFile file, string password = "")
        {
            _pdfViewModel.OnDocumentClosed();
            _docxViewModel.OnDocumentClosed();
            _documentManager.CloseDocument(_currentDocument);

            _currentDocument = await _documentManager.OpenDocumentAsync(file, password);

            if (_currentDocument != null)
            {
                CurrentDocumentType = _currentDocument.Type;
                DocumentInfo = _documentManager.GetDocumentInfo(_currentDocument);
                _navigationService.SetTotalPages(DocumentInfo?.PageCount ?? 0);
                _navigationService.GoToPage(0);
                IsDocumentLoaded = true;

                if (_currentDocumentType == DocumentType.PDF && _currentDocument is PDFDocumentWrapper pdfWrapper)
                {
                    _pdfViewModel.OnDocumentLoaded(pdfWrapper.InnerDoc);
                    UpdateAttachmentStatus();
                }
                else if (_currentDocumentType == DocumentType.DOCX)
                {
                    _docxViewModel.OnDocumentLoaded(_currentDocument);
                    HasAttachments = false;
                }

                OnPropertyChanged(nameof(CurrentPageNumber));
                OnPropertyChanged(nameof(TotalPages));
                UpdateNavigationCommands();
            }
        }


        private ViewMode GetActiveViewMode()
        {
            if (_currentDocumentType == DocumentType.PDF)
                return _pdfViewModel.ViewMode;
            else
                return _docxViewModel.ViewMode;
        }

        private void NavigateActiveView(int pageIndex)
        {
            if (_currentDocumentType == DocumentType.PDF)
            {
                if (_pdfViewModel.CurrentPDFView != null)
                    _pdfViewModel.CurrentPDFView.vPageGoto(pageIndex);
            }
            else if (_currentDocumentType == DocumentType.DOCX)
            {
                if (_docxViewModel.CurrentDOCXView != null)
                    _docxViewModel.CurrentDOCXView.vPageGoto(pageIndex);
            }
        }

        private async Task GoToNextPageAsync()
        {
            var activeMode = GetActiveViewMode();
            int step = (activeMode == ViewMode.DualPage || activeMode == ViewMode.DualPageContinuous) ? 2 : 1;

            int targetPage = _navigationService.CurrentPageIndex + step;
            if (targetPage < _navigationService.TotalPages)
            {
                if (_navigationService.GoToPage(targetPage))
                {
                    NavigateActiveView(_navigationService.CurrentPageIndex);
                    OnPropertyChanged(nameof(CurrentPageNumber));
                    UpdateNavigationCommands();
                }
            }
            await Task.CompletedTask;
        }

        private async Task GoToPreviousPageAsync()
        {
            var activeMode = GetActiveViewMode();
            int step = (activeMode == ViewMode.DualPage || activeMode == ViewMode.DualPageContinuous) ? 2 : 1;

            int targetPage = _navigationService.CurrentPageIndex - step;
            if (targetPage >= 0)
            {
                if (_navigationService.GoToPage(targetPage))
                {
                    NavigateActiveView(_navigationService.CurrentPageIndex);
                    OnPropertyChanged(nameof(CurrentPageNumber));
                    UpdateNavigationCommands();
                }
            }
            await Task.CompletedTask;
        }

        public void GoToPage(int pageIndex)
        {
            if (_navigationService.GoToPage(pageIndex))
            {
                NavigateActiveView(_navigationService.CurrentPageIndex);
                OnPropertyChanged(nameof(CurrentPageNumber));
                UpdateNavigationCommands();
            }
        }

        private void UpdateNavigationCommands()
        {
            NextPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
        }

        public void Cleanup()
        {
            _pdfViewModel.OnDocumentClosed();
            _docxViewModel.OnDocumentClosed();
            _documentManager.CloseDocument(_currentDocument);
        }

        public IDocument? GetCurrentDocument()
        {
            return _currentDocument;
        }

        private void UpdateAttachmentStatus()
        {
            if (_currentDocument is PDFDocumentWrapper pdfWrapper && pdfWrapper.IsOpened)
            {
                int efCount = pdfWrapper.InnerDoc.EFCount;
                HasAttachments = efCount > 0;
            }
            else
            {
                HasAttachments = false;
            }
        }
    }
}
