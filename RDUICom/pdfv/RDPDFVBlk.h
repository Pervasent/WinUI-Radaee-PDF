#pragma once
#include "UICom.h"
#include "UIPDF.h"
#include "RDUILib.RDGlobal.g.h"
#include "RDUILib.RDBmp.g.h"
#include "RDUILib.PDFPage.g.h"
#include "RDUILib.PDFDoc.g.h"
#include "RDPDFVCallback.h"
#include <ppltasks.h>
//extern int g_iblks;
using namespace Concurrency;
using namespace winrt::Microsoft::UI::Xaml::Media::Imaging;
using namespace winrt::Windows::UI::Xaml::Media;
using namespace winrt::Windows::Foundation;
namespace RDDLib::pdfv
{
	/**
	* inner class
	*/
	class CRDVBlk
	{
	public:
		int m_x;
		int m_y;
		int m_w;
		int m_h;
		double m_ph;
		int m_status;
		int m_pageno;
		double m_scale;
		PDF_DOC m_doc;
		PDF_PAGE m_page;
		PDF_DIB m_dib;
		winrt::Microsoft::UI::Xaml::Controls::Image m_img;
		winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap m_bmp;
		bool m_attached;
		static winrt::RDUILib::RDBmp m_def_bmp;
		static int m_cell_size;
		CRDVBlk(winrt::RDUILib::IRDDelegate del, winrt::RDUILib::PDFDoc doc, int pageno, double scale, int x, int y, int w, int h, double ph)
			: m_bmp(nullptr)
		{
			if (!m_def_bmp)
			{
				m_def_bmp = winrt::make<winrt::RDUILib::RDBmp>(del, 2, 2);
				m_def_bmp.Reset(-1);
			}
			m_x = x;
			m_y = y;
			m_w = w;
			m_h = h;
			m_ph = ph;
			m_scale = scale;
			m_status = 0;
			m_page = NULL;
			m_dib = NULL;
			m_img = nullptr;
			m_bmp = nullptr;
			m_doc = (PDF_DOC)doc.Handle();
			m_pageno = pageno;
			m_attached = false;
		}
		CRDVBlk(const CRDVBlk* src)
			: m_bmp(nullptr)
		{
			m_x = src->m_x;
			m_y = src->m_y;
			m_w = src->m_w;
			m_h = src->m_h;
			m_ph = src->m_ph;
			m_scale = src->m_scale;
			m_status = 0;
			m_page = NULL;
			m_dib = NULL;
			m_img = nullptr;
			m_bmp = nullptr;
			m_doc = src->m_doc;
			m_pageno = src->m_pageno;
			m_attached = false;
		}
		~CRDVBlk()
		{
			if (m_dib)
			{
				Global_dibFree(m_dib);
				m_dib = NULL;
			}
			if (m_page)
			{
				PDF_Page_close(m_page);
				m_page = NULL;
			}
			m_doc = NULL;
		}
		static long long annot_callback(void* user, PDF_ANNOT annot)
		{
			CRDVBlk* thiz = (CRDVBlk*)user;
			int type = PDF_Page_getAnnotType(thiz->m_page, annot);
			if (type == 2) return 0x100000000L;//do not display link annoataion
			if (type == 20)
			{
				int sta = PDF_Page_getAnnotCheckStatus(thiz->m_page, annot);
				if (sta <= 0) return 0;//fully transparency.
			}
			return 0x200000ff;//blue transparency.
		}
		inline void bk_render()
		{
			if (m_status == -1) return;
			m_page = PDF_Document_getPage(m_doc, m_pageno);
			PDF_PAGE page = m_page;
			PDF_DIB dib = Global_dibGet(NULL, m_w, m_h);
			PDF_Page_renderPrepare(page, dib);
			if (m_status == -1)
			{
				Global_dibFree(dib);
				return;
			}
			PDF_MATRIX mat = Matrix_createScale(m_scale, -m_scale, -m_x, m_ph - m_y);
			PDF_Page_renderWithPGEditor(page, dib, mat, true, mode_best);
			//PDF_Page_renderWithPGEditor1(page, dib, mat, annot_callback, this, mode_best);
			Matrix_destroy(mat);

			if (m_status != -1)
			{
				m_dib = dib;
				m_status = 2;
			}
			else
				Global_dibFree(dib);
		}
		inline bool ui_start()
		{
			if (m_status != 0) return false;
			m_status = 1;
			return true;
		}
		inline bool ui_end(IVCallback *canvas)
		{
			if (m_status == 0 || m_status == -1) return false;
			if (m_status == 1 && m_page) PDF_Page_renderCancel(m_page);
			m_status = -1;
			ui_cancel(canvas);
			return true;
		}
		inline void ui_cancel(IVCallback *canvas)
		{
			if (m_img != nullptr)
			{
				//m_img.Source(winrt::Microsoft::UI::Xaml::Controls::Image(nullptr));//m_def_bmp->Data;
				canvas->vpRemoveBlock(m_img);
				m_img = nullptr;
			}
			//seems no help to invoke System.GC.Collect()
			if (m_bmp)
			{
				canvas->vpDetachBmp(m_bmp);
				m_bmp = nullptr;
			}
		}
		inline bool ui_is_cancel()
		{
			return m_status < 0;
		}
		inline void ui_draw(IVCallback *canvas, double orgx, double orgy)
		{
			if (!m_img)
			{
				m_img = winrt::make<winrt::Microsoft::UI::Xaml::Controls::Image>();
				m_img.Stretch(winrt::Microsoft::UI::Xaml::Media::Stretch::Fill);
				m_img.Source(m_def_bmp.Data());
				canvas->vpShowBlock(m_img, orgx + m_x, orgy + m_y, m_w, m_h);
			}
			//else canvas->vpMoveBlock(m_img, left, top, right, bottom);
			if (m_status == 2 && !m_attached)
			{
				if (m_dib)
				{
					m_bmp = winrt::make <winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap>(m_w, m_h);
					canvas->vpAttachBmp(m_bmp, (const byte *)Global_dibGetData(m_dib));
					m_img.Source(m_bmp);

					Global_dibFree(m_dib);
					m_dib = NULL;
				}
				m_attached = true;
			}
		}
		inline void ui_draw(IVCallback *canvas, int orgx, int orgy)
		{
			if (!m_img)
			{
				m_img = winrt::make<winrt::Microsoft::UI::Xaml::Controls::Image>();
				m_img.Stretch(winrt::Microsoft::UI::Xaml::Media::Stretch::Fill);
				m_img.Source(m_def_bmp.Data());
				canvas->vpShowBlock(m_img, orgx + m_x, orgy + m_y, m_w, m_h);
			}
			//else canvas->vpMoveBlock(m_img, left, top, right, bottom);
			if (m_status == 2 && !m_attached)
			{
				if (m_dib)
				{
					m_bmp = winrt::make <winrt::Microsoft::UI::Xaml::Media::Imaging::WriteableBitmap>(m_w, m_h);
					canvas->vpAttachBmp(m_bmp, (const byte *)Global_dibGetData(m_dib));
					m_img.Source(m_bmp);
					Global_dibFree(m_dib);
					m_dib = NULL;
				}
				m_attached = true;
			}
		}
		inline bool isCross(double left, double top, double right, double bottom)
		{
			return !(left >= m_x + m_w || right < m_x || top >= m_y + m_h || bottom < m_y);
		}
		inline bool isRender()
		{
			return (m_status == 1 || m_status == 2);
		}
		inline bool isFinished()
		{
			return (m_status == 2);
		}
		inline double GetX()
		{
			return m_x;
		}
		inline double GetY()
		{
			return m_y;
		}
		inline double GetRight()
		{
			return m_x + m_w;
		}
		inline double GetBottom()
		{
			return m_y + m_h;
		}
		inline double GetW()
		{
			return m_w;
		}
		inline double GetH()
		{
			return m_h;
		}
		inline int GetPageNo() { return m_pageno; }
	};
}
