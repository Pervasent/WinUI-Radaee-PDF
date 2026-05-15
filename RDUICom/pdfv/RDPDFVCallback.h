#pragma once
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;
using namespace winrt::Windows::UI::Core;
namespace RDDLib::pdfv
{
    class IVCallback
    {
    public:
        virtual void vpDrawSelRect(double left, double top, double right, double bottom) = 0;
        virtual void vpDrawMarkRect(double left, double top, double right, double bottom) = 0;
        virtual void vpOnFound(bool found) = 0;
        virtual winrt::Windows::UI::Core::CoreDispatcher vpGetDisp() = 0;
        virtual void vpShowBlock(Image img, double x, double y, double w, double h) = 0;
        virtual void vpRemoveBlock(Image img) = 0;
        virtual void vpShowPNO(TextBlock txt, double left, double top, double right, double bottom) = 0;
        virtual void vpRemovePNO(TextBlock txt) = 0;
        virtual void vpDetachBmp(winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap bmp) = 0;
        virtual void vpAttachBmp(winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap bmp, const byte *arr) = 0;
    };
}
