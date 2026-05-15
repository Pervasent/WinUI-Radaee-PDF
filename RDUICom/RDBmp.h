#pragma once

#include "RDUILib.RDBmp.g.h"
#include "UICom.h"
#include "UIPDF.h"
struct _INNER_BMP_DATA_
{
    byte* data;
    int w;
	int h;
};

namespace winrt::RDUILib::implementation
{
    struct RDBmp : RDBmpT<RDBmp>
    {
        RDBmp(IRDDelegate del, int32_t w, int32_t h)
            : m_dib(del.OnCreateBitmap(w, h))
        {
            m_w = w;
            m_h = h;
            IBuffer buf = del.OnGetBitmapBuffer(m_dib);
            m_bmp.data = buf.data();
            m_bmp.w = w;
            m_bmp.h = h;
        }
        ~RDBmp()
        {
            m_bmp.data = NULL;
        }
        int64_t Handle()
        {
            return (int64_t)&m_bmp;
        }
        bool SaveJPG(winrt::hstring path, int32_t quality)
        {
            char stxt[512];
            WideCharToMultiByte(CP_ACP, 0, path.c_str(), -1, stxt, 511, NULL, NULL);
            return Global_saveBitmapJPG((PDF_BMP)&m_bmp, stxt, quality);
        }
        Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap Data()
        {
            return m_dib;
        }
        void Reset(uint32_t color)
        {
            Global_eraseColor((PDF_BMP)&m_bmp, color);
        }
        void DrawDIB(RDUILib::RDDIB src, int32_t x, int32_t y)
        {
            if (src) Global_drawDIB((PDF_BMP)&m_bmp, (PDF_DIB)src.Handle(), x, y);
        }
        int32_t Width()
        {
            return m_w;
        }
        int32_t Height()
        {
            return m_h;
        }
        Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap m_dib;
        _INNER_BMP_DATA_ m_bmp;
        int m_w;
        int m_h;
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct RDBmp : RDBmpT<RDBmp, implementation::RDBmp>
    {
    };
}
