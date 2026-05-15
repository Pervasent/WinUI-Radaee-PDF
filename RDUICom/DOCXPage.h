#pragma once

#include "RDUILib.DOCXPage.g.h"
#include "UICom.h"
#include "UIDOCX.h"
#include "DOCXFinder.h"

namespace winrt::RDUILib::implementation
{
    struct DOCXPage : DOCXPageT<DOCXPage>
    {
        DOCXPage(int64_t hand)
        {
            m_hand = (DOCX_PAGE)hand;
        }
		~DOCXPage()
		{
			Close();
		}
        int64_t Handle() { return (int64_t)m_hand; }
		winrt::RDUILib::DOCXFinder GetFinder(winrt::hstring key, bool match_case, bool whole_word)
		{
			DOCX_FINDER find = DOCX_Page_findOpen(m_hand, key, match_case, whole_word);
			if (find)
			{
				if (!find) return nullptr;
				return winrt::make<implementation::DOCXFinder>((int64_t)find);
			}
			else return nullptr;
		}
		winrt::RDUILib::DOCXFinder GetFinder(winrt::hstring key, bool match_case, bool whole_word, bool skip_blanks)
		{
			DOCX_FINDER find = DOCX_Page_findOpen2(m_hand, key, match_case, whole_word, skip_blanks);
			if (find)
			{
				if (!find) return nullptr;
				return winrt::make<implementation::DOCXFinder>((int64_t)find);
			}
			else return nullptr;
		}
		void Close()
		{
			if (m_hand)
				DOCX_Page_close(m_hand);
			m_hand = NULL;
		}
		winrt::hstring GetHLink(float x, float y)
		{
			return DOCX_Page_getHLink(m_hand, x, y);
		}
		int ObjsAlignWord(int index, int dir)
		{
			return DOCX_Page_objsAlignWord(m_hand, index, dir);
		}
		int ObjsGetCharCount()
		{
			return DOCX_Page_objsGetCharCount(m_hand);
		}
		int ObjsGetCharIndex(float x, float y)
		{
			PDF_POINT pt;
			pt.x = x;
			pt.y = y;
			return DOCX_Page_objsGetCharIndex(m_hand, &pt);
		}
		RDRect ObjsGetCharRect(int index)
		{
			RDRect rect;
			DOCX_Page_objsGetCharRect(m_hand, index, (PDF_RECT*)&rect);
			return rect;
		}
		winrt::hstring ObjsGetString(int from, int to)
		{
			return DOCX_Page_objsGetString(m_hand, from, to);
		}
		void ObjsStart()
		{
			DOCX_Page_objsStart(m_hand);
		}
		bool Render(RDDIB dib, float scale, int orgx, int orgy, int quality)
		{
			return DOCX_Page_render(m_hand, (PDF_DIB)dib.Handle(), scale, orgx, orgy, quality);
		}
		void RenderCancel()
		{
			DOCX_Page_renderCancel(m_hand);
		}
		bool RenderIsFinished()
		{
			return DOCX_Page_renderIsFinished(m_hand);
		}
		void RenderPrepare()
		{
			DOCX_Page_renderPrepare(m_hand, NULL);
		}
		void RenderPrepare(RDDIB dib)
		{
			if (dib)
				DOCX_Page_renderPrepare(m_hand, (PDF_DIB)dib.Handle());
			else
				DOCX_Page_renderPrepare(m_hand, NULL);
		}
		DOCX_PAGE m_hand;
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct DOCXPage : DOCXPageT<DOCXPage, implementation::DOCXPage>
    {
    };
}
