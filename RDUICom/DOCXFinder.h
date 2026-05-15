#pragma once

#include "RDUILib.DOCXFinder.g.h"
#include "UICom.h"
#include "UIDOCX.h"

namespace winrt::RDUILib::implementation
{
    struct DOCXFinder : DOCXFinderT<DOCXFinder>
    {
        DOCXFinder(int64_t hand)
        {
            m_hand = (DOCX_FINDER)hand;
        }
        int64_t Handle() { return (int64_t)m_hand; }
        int GetCount()
        {
            return DOCX_Page_findGetCount(m_hand);
        }
        int GetFirstChar(int index)
        {
            return DOCX_Page_findGetFirstChar(m_hand, index);
        }
        int GetLastChar(int index)
        {
            return DOCX_Page_findGetEndChar(m_hand, index);
        }
        DOCX_FINDER m_hand;
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct DOCXFinder : DOCXFinderT<DOCXFinder, implementation::DOCXFinder>
    {
    };
}
