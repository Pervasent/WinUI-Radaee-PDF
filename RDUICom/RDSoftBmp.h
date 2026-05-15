#pragma once

#include "RDUILib.RDSoftBmp.g.h"
#include "UICom.h"
#include "UIPDF.h"

namespace winrt::RDUILib::implementation
{
    struct RDSoftBmp : RDSoftBmpT<RDSoftBmp>
    {
        RDSoftBmp(int32_t w, int32_t h)
            : m_dib(BitmapPixelFormat::Bgra8, w, h, BitmapAlphaMode::Premultiplied)
        {
            m_w = w;
            m_h = h;
            m_bmp = Global_lockSoftBitmap(m_dib);
        }
        ~RDSoftBmp()
        {
            if (m_bmp)
            {
                Global_unlockBitmap(m_bmp);
                m_bmp = NULL;
                m_dib = nullptr;
            }
        }
        winrt::Windows::Graphics::Imaging::SoftwareBitmap Data()
        {
            return m_dib;
        }
        int64_t Handle()
        {
            return (int64_t)m_bmp;
        }
        bool SaveJPG(winrt::hstring path, int32_t quality)
        {
            char stxt[512];
            WideCharToMultiByte(CP_ACP, 0, path.c_str(), -1, stxt, 511, NULL, NULL);
            return Global_saveBitmapJPG(m_bmp, stxt, quality);
        }
        void Reset(uint32_t color)
        {
            Global_eraseColor(m_bmp, color);
        }
        void DrawDIB(RDUILib::RDDIB src, int32_t x, int32_t y)
        {
            if (src) Global_drawDIB(m_bmp, (PDF_DIB)src.Handle(), x, y);
        }
        int32_t Width()
        {
            return m_w;
        }
        int32_t Height()
        {
            return m_h;
        }
        winrt::Windows::Graphics::Imaging::SoftwareBitmap m_dib;
        PDF_BMP m_bmp;
        int m_w;
        int m_h;
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct RDSoftBmp : RDSoftBmpT<RDSoftBmp, implementation::RDSoftBmp>
    {
    };
}
