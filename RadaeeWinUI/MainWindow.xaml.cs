using Microsoft.UI.Xaml;
using RDUILib;
using RadaeeWinUI.Models;
using RadaeeWinUI.RadaeeUtil;
using RadaeeWinUI.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using RadaeeWinUI.Controls;



namespace RadaeeWinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }



        public MainWindow()
        {
            InitializeComponent();

            if (!initLib())
            {
                Debug.WriteLine("Failed to initialize Radaee library.");
            }

            ViewModel = App.GetService<MainViewModel>();
            AnnotationToolbar.ViewModel = ViewModel.PDFViewModel;
            SearchToolbar.ViewModel = ViewModel.PDFViewModel;
            SearchToolbar.CloseRequested += (s, e) =>
            {
                SearchToolbar.Visibility = Visibility.Collapsed;
                if (ViewModel.IsDocumentLoaded && ViewModel.CurrentDocumentType == DocumentType.PDF)
                {
                    AnnotationToolbar.Visibility = Visibility.Visible;
                }
            };

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.PDFViewModel.PropertyChanged += PDFViewModel_PropertyChanged;
            ViewModel.DOCXViewModel.PropertyChanged += DOCXViewModel_PropertyChanged;
            UpdateUI();
        }

        private bool initLib()
        {
            return RadaeeUtil.RDGlobal.init();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.IsDocumentLoaded):
                        UpdateUI();
                        if (ViewModel.IsDocumentLoaded)
                        {
                            InitializeDocumentView();
                        }
                        break;
                    case nameof(ViewModel.CurrentPageNumber):
                        PageNumberText.Text = ViewModel.CurrentPageNumber.ToString();
                        break;
                    case nameof(ViewModel.TotalPages):
                        TotalPagesText.Text = ViewModel.TotalPages.ToString();
                        break;
                    case nameof(ViewModel.HasAttachments):
                        UpdateAttachmentButtonVisibility();
                        break;
                }
            });
        }

        private void UpdateUI()
        {
            if (ViewModel.IsDocumentLoaded)
            {
                EmptyMessage.Visibility = Visibility.Collapsed;
                PageNumberText.Text = ViewModel.CurrentPageNumber.ToString();
                TotalPagesText.Text = ViewModel.TotalPages.ToString();
                UpdateZoomLevel();
            }
            else
            {
                EmptyMessage.Visibility = Visibility.Visible;
                AnnotationToolbar.Visibility = Visibility.Collapsed;
                PDFViewContainer.Visibility = Visibility.Collapsed;
                DOCXViewContainer.Visibility = Visibility.Collapsed;
                PageNumberText.Text = "0";
                TotalPagesText.Text = "0";
                ZoomLevelText.Text = "100%";
            }
        }

        private void UpdateZoomLevel()
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF && ViewModel.PDFViewModel.CurrentPDFView != null)
            {
                float zoomLevel = ViewModel.PDFViewModel.CurrentPDFView.ZoomLevel;
                ZoomLevelText.Text = $"{(int)(zoomLevel * 100)}%";
            }
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX && ViewModel.DOCXViewModel.CurrentDOCXView != null)
            {
                float zoomLevel = ViewModel.DOCXViewModel.CurrentDOCXView.ZoomLevel;
                ZoomLevelText.Text = $"{(int)(zoomLevel * 100)}%";
            }
            else
            {
                ZoomLevelText.Text = "100%";
            }
        }

        private void InitializeDocumentView()
        {
            if (!ViewModel.IsDocumentLoaded)
                return;

            var doc = ViewModel.GetCurrentDocument();
            if (doc == null)
                return;

            if (doc.Type == DocumentType.PDF)
            {
                ViewModel.DOCXViewModel.OnDocumentClosed();
                ViewModel.PDFViewModel.SwitchViewMode(ViewMode.SinglePage);
                PDFViewContainer.Content = ViewModel.PDFViewModel.CurrentPDFView;
                PDFViewContainer.Visibility = Visibility.Visible;
                DOCXViewContainer.Visibility = Visibility.Collapsed;
                AnnotationToolbar.Visibility = Visibility.Visible;
                BtnSearch.Visibility = Visibility.Visible;

                if (doc is PDFDocumentWrapper pdfWrapper)
                {
                    PDFDoc pdfDoc = pdfWrapper.InnerDoc;
                    BtnOutline.Visibility = pdfDoc.GetRootOutline() != null ? Visibility.Visible : Visibility.Collapsed;
                }

                UpdateAttachmentButtonVisibility();
            }
            else if (doc.Type == DocumentType.DOCX)
            {
                ViewModel.DOCXViewModel.SwitchViewMode(ViewMode.SinglePage);
                DOCXViewContainer.Content = ViewModel.DOCXViewModel.CurrentDOCXView;
                DOCXViewContainer.Visibility = Visibility.Visible;
                PDFViewContainer.Visibility = Visibility.Collapsed;
                AnnotationToolbar.Visibility = Visibility.Collapsed;
                SearchToolbar.Visibility = Visibility.Collapsed;
                BtnOutline.Visibility = Visibility.Collapsed;
                BtnAttachment.Visibility = Visibility.Collapsed;
                BtnSearch.Visibility = Visibility.Collapsed;
            }
        }

        private async void ShowPDFOutline()
        {
            var doc = ViewModel.GetCurrentDocument();
            if (doc is PDFDocumentWrapper pdfWrapper)
            {
                var outline = pdfWrapper.InnerDoc.GetRootOutline();
                if (outline != null)
                {
                    var dialog = new OutlineDialog();
                    dialog.LoadOutline(outline);

                    await dialog.ShowAsync();

                    if (dialog.SelectedPageIndex >= 0)
                    {
                        ViewModel.GoToPage(dialog.SelectedPageIndex);
                    }
                }
            }
        }

        private void PDFViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.PropertyName == nameof(ViewModel.PDFViewModel.CurrentPDFView))
                {
                    PDFViewContainer.Content = ViewModel.PDFViewModel.CurrentPDFView;
                    UpdateZoomLevel();
                }
            });
        }

        private void DOCXViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.PropertyName == nameof(ViewModel.DOCXViewModel.CurrentDOCXView))
                {
                    DOCXViewContainer.Content = ViewModel.DOCXViewModel.CurrentDOCXView;
                    UpdateZoomLevel();
                }
            });
        }

        private async void OpenDocument_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.OpenDocumentAsync();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NextPageCommand.Execute(null);
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PreviousPageCommand.Execute(null);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF && ViewModel.PDFViewModel.CurrentPDFView != null)
            {
                float currentZoom = ViewModel.PDFViewModel.CurrentPDFView.ZoomLevel;
                float newZoom = Math.Min(currentZoom * 1.2f, 5.0f);
                ViewModel.PDFViewModel.CurrentPDFView.vSetZoom(newZoom);
                UpdateZoomLevel();
            }
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX && ViewModel.DOCXViewModel.CurrentDOCXView != null)
            {
                float currentZoom = ViewModel.DOCXViewModel.CurrentDOCXView.ZoomLevel;
                float newZoom = Math.Min(currentZoom * 1.2f, 5.0f);
                ViewModel.DOCXViewModel.CurrentDOCXView.vSetZoom(newZoom);
                UpdateZoomLevel();
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF && ViewModel.PDFViewModel.CurrentPDFView != null)
            {
                float currentZoom = ViewModel.PDFViewModel.CurrentPDFView.ZoomLevel;
                float newZoom = Math.Max(currentZoom / 1.2f, 0.5f);
                ViewModel.PDFViewModel.CurrentPDFView.vSetZoom(newZoom);
                UpdateZoomLevel();
            }
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX && ViewModel.DOCXViewModel.CurrentDOCXView != null)
            {
                float currentZoom = ViewModel.DOCXViewModel.CurrentDOCXView.ZoomLevel;
                float newZoom = Math.Max(currentZoom / 1.2f, 0.5f);
                ViewModel.DOCXViewModel.CurrentDOCXView.vSetZoom(newZoom);
                UpdateZoomLevel();
            }
        }

        private void FitWidth_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF && ViewModel.PDFViewModel.CurrentPDFView != null)
            {
                ViewModel.PDFViewModel.CurrentPDFView.vSetZoom(1.0f);
                UpdateZoomLevel();
            }
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX && ViewModel.DOCXViewModel.CurrentDOCXView != null)
            {
                ViewModel.DOCXViewModel.CurrentDOCXView.vSetZoom(1.0f);
                UpdateZoomLevel();
            }
        }

        private void ViewMode_SinglePage_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF)
                ViewModel.PDFViewModel.SwitchViewMode(ViewMode.SinglePage);
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX)
                ViewModel.DOCXViewModel.SwitchViewMode(ViewMode.SinglePage);
        }

        private void ViewMode_VerticalContinuous_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF)
                ViewModel.PDFViewModel.SwitchViewMode(ViewMode.VerticalContinuous);
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX)
                ViewModel.DOCXViewModel.SwitchViewMode(ViewMode.VerticalContinuous);
        }

        private void ViewMode_HorizontalContinuous_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF)
                ViewModel.PDFViewModel.SwitchViewMode(ViewMode.HorizontalContinuous);
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX)
                ViewModel.DOCXViewModel.SwitchViewMode(ViewMode.HorizontalContinuous);
        }

        private void ViewMode_DualPage_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType == DocumentType.PDF)
                ViewModel.PDFViewModel.SwitchViewMode(ViewMode.DualPageContinuous);
            else if (ViewModel.CurrentDocumentType == DocumentType.DOCX)
                ViewModel.DOCXViewModel.SwitchViewMode(ViewMode.DualPage);
        }

        private void Outline_Click(object sender, RoutedEventArgs e)
        {
            ShowPDFOutline();
        }

        private void UpdateAttachmentButtonVisibility()
        {
            BtnAttachment.Visibility = ViewModel.HasAttachments ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Attachment_Click(object sender, RoutedEventArgs e)
        {
            var doc = ViewModel.GetCurrentDocument();
            if (doc is PDFDocumentWrapper pdfWrapper)
            {
                var dialog = new AttachmentListDialog
                {
                    XamlRoot = this.Content.XamlRoot
                };
                dialog.LoadAttachments(pdfWrapper.InnerDoc);
                await dialog.ShowAsync();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentDocumentType != DocumentType.PDF)
                return;

            if (SearchToolbar.Visibility == Visibility.Visible)
            {
                SearchToolbar.Visibility = Visibility.Collapsed;
                AnnotationToolbar.Visibility = Visibility.Visible;
            }
            else
            {
                AnnotationToolbar.Visibility = Visibility.Collapsed;
                SearchToolbar.Visibility = Visibility.Visible;
            }
        }
    }
}



