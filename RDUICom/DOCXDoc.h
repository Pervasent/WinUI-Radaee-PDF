#pragma once

#include "RDUILib.DOCXDoc.g.h"
#include "UICom.h"
#include "UIDOCX.h"
#include "DOCXPage.h"

namespace winrt::RDUILib::implementation
{
    struct DOCXDoc : DOCXDocT<DOCXDoc>
    {
        DOCXDoc()
        {
            m_doc = NULL;
        }
        ~DOCXDoc()
        {
            Close();
		}
        int32_t OpenPath(winrt::hstring path, winrt::hstring password)
        {
            PDF_ERR err;
            m_doc = DOCX_Document_openPath(path.c_str(), password, &err);
            return err;
        }
        int32_t Open(IRandomAccessStream stream, winrt::hstring password)
        {
            PDF_ERR err;
            m_doc = DOCX_Document_open(stream, password, &err);
            return err;
        }
        void Close()
        {
            if (m_doc)
            {
                DOCX_Document_close(m_doc);
                m_doc = NULL;
			}
        }
        float GetPageWidth(int32_t pageno)
        {
            return DOCX_Document_getPageWidth(m_doc, pageno);
        }
        float GetPageHeight(int32_t pageno)
        {
            return DOCX_Document_getPageHeight(m_doc, pageno);
        }
        bool ExportPDF(PDFDoc pdf)
        {
            return DOCX_Document_exportPDF(m_doc, (PDF_DOC)pdf.Handle());
        }
        winrt::RDUILib::DOCXPage GetPage(int32_t pageno)
        {
            DOCX_PAGE page = DOCX_Document_getPage(m_doc, pageno);
            if (page)
            {
                if (!page) return nullptr;
                return winrt::make<implementation::DOCXPage>((int64_t)page);
            }
            else
                return nullptr;
        }
        DOCX_DOC m_doc;
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct DOCXDoc : DOCXDocT<DOCXDoc, implementation::DOCXDoc>
    {
    };
}
