#pragma once

#include "RDUILib.PDFAnnot.g.h"
#include "UICom.h"
#include "UIPDF.h"
char* cvt_str_cstr(winrt::hstring str);
winrt::hstring cvt_cstr_str(const char* str);

namespace winrt::RDUILib::implementation
{
    struct PDFAnnot : PDFAnnotT<PDFAnnot>
    {
        PDFAnnot(int64_t page, int64_t hand)
        {
            m_page = (PDF_PAGE)page;
            m_hand = (PDF_ANNOT)hand;
        }
        int64_t Handle() { return (int64_t)m_hand; }
        bool DoReset()
        {
            return PDF_Page_setAnnotReset(m_page, m_hand);
        }
        com_array<uint8_t> Export()
        {
            unsigned char* buf = (unsigned char*)malloc(8192);
            int len = PDF_Page_exportAnnot(m_page, m_hand, buf, 8192);
            com_array<uint8_t> ret(len);
            memcpy(ret.data(), buf, len);
            free(buf);
            return ret;
        }
        bool FlateFromPage()
        {
            bool ret = PDF_Page_flateAnnot(m_page, m_hand);
            if (ret)
            {
                m_page = NULL;
                m_hand = NULL;
            }
            return ret;
        }
        bool Get3DData(winrt::hstring save_path)
        {
            char* tmp = cvt_str_cstr(save_path);
            bool ret = PDF_Page_getAnnot3DData(m_page, m_hand, tmp);
            free(tmp);
            return ret;
        }
        winrt::hstring Get3DName()
        {
            return PDF_Page_getAnnot3D(m_page, m_hand);
        }
        bool GetAttachmentData(winrt::hstring save_path)
        {
            char* tmp = cvt_str_cstr(save_path);
            bool ret = PDF_Page_getAnnotAttachmentData(m_page, m_hand, tmp);
            free(tmp);
            return ret;
        }
        winrt::hstring GetAttachmentName()
        {
            return PDF_Page_getAnnotAttachment(m_page, m_hand);
        }
        int GetCheckStatus()
        {
            return PDF_Page_getAnnotCheckStatus(m_page, m_hand);
        }
        winrt::hstring GetComboItem(int item)
        {
            return PDF_Page_getAnnotComboItem(m_page, m_hand, item);
        }
        winrt::hstring GetFieldJS(int idx)
        {
            return PDF_Page_getAnnotFieldJS(m_page, m_hand, idx);
        }
        RDPoint GetLinePoint(int index)
        {
            PDF_POINT pt;
            pt.x = 0;
            pt.y = 0;
            PDF_Page_getAnnotLinePoint(m_page, m_hand, index, &pt);
            return *(RDPoint*)&pt;
        }
        winrt::hstring GetListItem(int item)
        {
            return PDF_Page_getAnnotListItem(m_page, m_hand, item);
        }
        bool GetMovieData(winrt::hstring save_path)
        {
            char* tmp = cvt_str_cstr(save_path);
            bool ret = PDF_Page_getAnnotMovieData(m_page, m_hand, tmp);
            free(tmp);
            return ret;
        }
        winrt::hstring GetMovieName()
        {
            return PDF_Page_getAnnotMovie(m_page, m_hand);
        }
        bool GetRenditionData(winrt::hstring save_path)
        {
            char* tmp = cvt_str_cstr(save_path);
            bool ret = PDF_Page_getAnnotRenditionData(m_page, m_hand, tmp);
            free(tmp);
            return ret;
        }
        winrt::hstring GetRenditionName()
        {
            return PDF_Page_getAnnotRendition(m_page, m_hand);
        }
        RDUILib::PDFAnnot GetReply(int idx)
        {
            PDF_ANNOT annot = PDF_Page_getAnnotReply(m_page, m_hand, idx);
            if (!annot) return nullptr;
            return winrt::make<implementation::PDFAnnot>((int64_t)m_page, (int64_t)m_hand);
        }
        bool GetRichMediaData(winrt::hstring name, winrt::hstring save_path)
        {
            return PDF_Page_getAnnotRichMediaData(m_page, m_hand, name, save_path);
        }
        winrt::hstring GetRichMediaItemAsset(int idx)
        {
            return PDF_Page_getAnnotRichMediaItemAsset(m_page, m_hand, idx);
        }
        winrt::hstring GetRichMediaItemPara(int idx)
        {
            return PDF_Page_getAnnotRichMediaItemPara(m_page, m_hand, idx);
        }
        winrt::hstring GetRichMediaItemSource(int idx)
        {
            return PDF_Page_getAnnotRichMediaItemSource(m_page, m_hand, idx);
        }
        bool GetRichMediaItemSourceData(int idx, winrt::hstring save_path)
        {
            return PDF_Page_getAnnotRichMediaItemSourceData(m_page, m_hand, idx, save_path);
        }
        int GetRichMediaItemType(int idx)
        {
            return PDF_Page_getAnnotRichMediaItemType(m_page, m_hand, idx);
        }
        com_array<int> GetSoundData(winrt::hstring save_path)
        {
            int paras[6];
            char* tmp = cvt_str_cstr(save_path);
            bool ret = PDF_Page_getAnnotSoundData(m_page, m_hand, paras, tmp);
            free(tmp);
            if (!ret) return com_array<int>(0);
            else
            {
                com_array<int> arr(6);
                memcpy(arr.data(), paras, sizeof(int) * 6);
                return arr;
            }
        }
        winrt::hstring GetSoundName()
        {
            return PDF_Page_getAnnotSound(m_page, m_hand);
        }
        bool IsResetButton()
        {
            return PDF_Page_getAnnotReset(m_page, m_hand);
        }
        bool MoveToPage(RDUILib::PDFPage page, RDRect rect)
        {
            if (!m_page) return false;
            return PDF_Page_moveAnnot(m_page, (PDF_PAGE)page.Handle(), m_hand, (const PDF_RECT*)&rect);
        }
        bool RemoveFromPage()
        {
            bool ret = PDF_Page_removeAnnot(m_page, m_hand);
            if (ret)
            {
                m_page = nullptr;
                m_hand = NULL;
            }
            return ret;
        }
        bool RenderToBmp(RDUILib::RDBmp bmp)
        {
            if (!bmp || !m_page) return false;
            return PDF_Page_renderAnnotToBmp(m_page, m_hand, (PDF_BMP)bmp.Handle());
        }
        bool RenderToSoftBmp(RDUILib::RDSoftBmp bmp)
        {
            if (!bmp || !m_page) return false;
            return PDF_Page_renderAnnotToBmp(m_page, m_hand, (PDF_BMP)bmp.Handle());
        }
        bool SetCheckValue(bool check)
        {
            return PDF_Page_setAnnotCheckValue(m_page, m_hand, check);
        }
        bool SetEditFont(RDUILib::PDFDocFont font)
        {
            if (!font) return false;
            return PDF_Page_setAnnotEditFont(m_page, m_hand, (PDF_DOC_FONT)font.Handle());
        }
        bool SetLinePoint(float x1, float y1, float x2, float y2)
        {
            return PDF_Page_setAnnotLinePoint(m_page, m_hand, x1, y1, x2, y2);
        }
        bool SetRadio()
        {
            return PDF_Page_setAnnotRadio(m_page, m_hand);
        }
        int SignField(RDUILib::PDFDocForm form, winrt::hstring cert_file, winrt::hstring pswd, winrt::hstring name, winrt::hstring reason, winrt::hstring location, winrt::hstring contact)
        {
            char* ccert_file = cvt_str_cstr(cert_file);
            char* cpswd = cvt_str_cstr(pswd);
            char* cname = cvt_str_cstr(name);
            char* creason = cvt_str_cstr(reason);
            char* clocation = cvt_str_cstr(location);
            char* ccontact = cvt_str_cstr(contact);
            int ret = PDF_Page_signAnnotField(m_page, m_hand, (PDF_DOC_FORM)form.Handle(), ccert_file, cpswd, cname, creason, clocation, ccontact);
            free(ccontact);
            free(clocation);
            free(creason);
            free(cname);
            free(cpswd);
            free(ccert_file);
            return ret;
        }
        int ComboItemCount()
        {
            return PDF_Page_getAnnotComboItemCount(m_page, m_hand);
        }
        int ComboItemSel()
        {
            return PDF_Page_getAnnotComboItemSel(m_page, m_hand);
        }
        void ComboItemSel(int item)
        {
            PDF_Page_setAnnotComboItem(m_page, m_hand, item);
        }
        int Dest()
        {
            return PDF_Page_getAnnotDest(m_page, m_hand);
        }
        winrt::hstring EditText()
        {
            return PDF_Page_getAnnotEditText(m_page, m_hand);
        }
        void EditText(winrt::hstring txt)
        {
            bool ret = PDF_Page_setAnnotEditTextW(m_page, m_hand, txt.c_str());
            ret = 0;
        }
        int EditTextAlign() { return PDF_Page_getAnnotEditTextAlign(m_page, m_hand); }
        void EditTextAlign(int val) { PDF_Page_setAnnotEditTextAlign(m_page, m_hand, val); }
        unsigned int EditTextColor() { return PDF_Page_getAnnotEditTextColor(m_page, m_hand); }
        void EditTextColor(unsigned int val) { PDF_Page_setAnnotEditTextColor(m_page, m_hand, val); }
        RDRect EditTextRect()
        {
            RDRect rect;
            if (!PDF_Page_getAnnotEditTextRect(m_page, m_hand, (PDF_RECT*)&rect))
            {
                rect.left = 0;
                rect.top = 0;
                rect.right = 0;
                rect.bottom = 0;
            }
            return rect;
        }
        float EditTextSize() { return PDF_Page_getAnnotEditTextSize(m_page, m_hand); }
        void EditTextSize(float val) { PDF_Page_setAnnotEditTextSize(m_page, m_hand, val); }
        int EditType() { return PDF_Page_getAnnotEditType(m_page, m_hand); }
        winrt::hstring FieldFullName()
        {
            wchar_t tmp[512] = {0};
            if (PDF_Page_getAnnotFieldFullNameW(m_page, m_hand, tmp, 511) <= 0) return L"";
            else return tmp;
        }
        winrt::hstring FieldFullName2()
        {
            wchar_t tmp[512] = {0};
            if (PDF_Page_getAnnotFieldFullName2W(m_page, m_hand, tmp, 511) <= 0) return L"";
            else return tmp;
        }
        winrt::hstring FieldName()
        {
            wchar_t tmp[512] = {0};
            if (PDF_Page_getAnnotFieldNameW(m_page, m_hand, tmp, 511) <= 0) return L"";
            else return tmp;
        }
        winrt::hstring FieldNameWithNO()
        {
            wchar_t tmp[512] = { 0 };
            if (PDF_Page_getAnnotFieldNameWithNOW(m_page, m_hand, tmp, 511) <= 0) return L"";
            else return tmp;
        }
        int FieldType() { return PDF_Page_getAnnotFieldType(m_page, m_hand); }
        winrt::hstring FileLink()
        {
            return PDF_Page_getAnnotFileLink(m_page, m_hand);
        }
        int FillColor() { return PDF_Page_getAnnotFillColor(m_page, m_hand); }
        void FillColor(int color) { PDF_Page_setAnnotFillColor(m_page, m_hand, color); }
        bool Hide() { return PDF_Page_isAnnotHide(m_page, m_hand); }
        void Hide(bool val) { PDF_Page_setAnnotHide(m_page, m_hand, val); }
        int Icon() { return PDF_Page_getAnnotIcon(m_page, m_hand); }
        void Icon(int icon) { PDF_Page_setAnnotIcon(m_page, m_hand, icon); }
        int IndexInPage()
        {
            int cur = 0;
            int cnt = PDF_Page_getAnnotCount(m_page);
            while (cur < cnt)
            {
                PDF_ANNOT tmp = PDF_Page_getAnnot(m_page, cur);
                if (tmp == m_hand) return cur;
                cur++;
            }
            return -1;
        }
        uint64_t ObjRef() { return PDF_Page_getAnnotRef(m_page, m_hand); }
        RDUILib::RDPath InkPath();
        void InkPath(RDUILib::RDPath path)
        {
            PDF_Page_setAnnotInkPath(m_page, m_hand, (PDF_PATH)path.Handle());
        }
        bool Is3D()
        {
            winrt::hstring ret = PDF_Page_getAnnot3D(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsAttachment()
        {
            winrt::hstring ret = PDF_Page_getAnnotAttachment(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsFileLink()
        {
            winrt::hstring ret = PDF_Page_getAnnotFileLink(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsMovie()
        {
            winrt::hstring ret = PDF_Page_getAnnotMovie(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsPopup()
        {
            winrt::hstring ret = PDF_Page_getAnnotPopupSubject(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsRemoteDest()
        {
            winrt::hstring ret = PDF_Page_getAnnotRemoteDest(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsSound()
        {
            winrt::hstring ret = PDF_Page_getAnnotSound(m_page, m_hand);
            return (!ret.empty());
        }
        bool IsURI()
        {
            winrt::hstring ret = PDF_Page_getAnnotURI(m_page, m_hand);
            return (!ret.empty());
        }
        winrt::hstring JS() { return PDF_Page_getAnnotJS(m_page, m_hand); }
        int LineStyle() { return PDF_Page_getAnnotLineStyle(m_page, m_hand); }
        void LineStyle(int val) { PDF_Page_setAnnotLineStyle(m_page, m_hand, val); }
        int ListItemCount() { return PDF_Page_getAnnotListItemCount(m_page, m_hand); }
        com_array<int> ListItemSel()
        {
            int sels[128];
            int cnt = PDF_Page_getAnnotListSels(m_page, m_hand, sels, 128);
            com_array<int> tmp(cnt);
            memcpy(tmp.data(), sels, cnt * sizeof(int));
            return tmp;
        }
        void ListItemSel(array_view<int const> sel)
        {
            PDF_Page_setAnnotListSels(m_page, m_hand, sel.data(), sel.size());
        }
        bool Locked() { return PDF_Page_isAnnotLocked(m_page, m_hand); }
        void Locked(bool val) { PDF_Page_setAnnotLock(m_page, m_hand, val); }
        bool LockedContent() { return PDF_Page_isAnnotLockedContent(m_page, m_hand); }
        RDUILib::RDPath PolygonPath();
        void PolygonPath(RDUILib::RDPath path)
        {
            PDF_Page_setAnnotPolygonPath(m_page, m_hand, (PDF_PATH)path.Handle());
        }
        RDUILib::RDPath PolylinePath();
        void PolylinePath(RDUILib::RDPath path)
        {
            PDF_Page_setAnnotPolylinePath(m_page, m_hand, (PDF_PATH)path.Handle());
        }
        RDUILib::PDFAnnot Popup()
        {
            PDF_ANNOT pop = PDF_Page_getAnnotPopup(m_page, m_hand);
            return (pop) ? winrt::make<implementation::PDFAnnot>((int64_t)m_page, (int64_t)pop) : nullptr;
        }
        winrt::hstring PopupLabel()
        {
            return PDF_Page_getAnnotPopupLabel(m_page, m_hand);
        }
        void PopupLabel(winrt::hstring txt)
        {
            PDF_Page_setAnnotPopupLabelW(m_page, m_hand, txt.c_str());
        }
        bool PopupOpen()
        {
            return PDF_Page_getAnnotPopupOpen(m_page, m_hand);
        }
        void PopupOpen(bool open)
        {
            PDF_Page_setAnnotPopupOpen(m_page, m_hand, open);
        }
        winrt::hstring PopupSubject()
        {
            return PDF_Page_getAnnotPopupSubject(m_page, m_hand);
        }
        void PopupSubject(winrt::hstring txt)
        {
            PDF_Page_setAnnotPopupSubjectW(m_page, m_hand, txt.c_str());
        }
        winrt::hstring PopupText()
        {
            return PDF_Page_getAnnotPopupText(m_page, m_hand);
        }
        void PopupText(winrt::hstring txt)
        {
            PDF_Page_setAnnotPopupTextW(m_page, m_hand, txt.c_str());
        }
        bool ReadOnly() { return PDF_Page_isAnnotReadOnly(m_page, m_hand); }
        void ReadOnly(bool val) { PDF_Page_setAnnotReadOnly(m_page, m_hand, val); }
        RDRect Rect() { RDRect rect; PDF_Page_getAnnotRect(m_page, m_hand, (PDF_RECT*)&rect); return rect; }
        void Rect(RDRect rect) { PDF_Page_setAnnotRect(m_page, m_hand, (const PDF_RECT*)&rect); }
        winrt::hstring RemoteDest()
        {
            return PDF_Page_getAnnotRemoteDest(m_page, m_hand);
        }
        int ReplyCount()
        {
            return PDF_Page_getAnnotReplyCount(m_page, m_hand);
        }
        int RichMediaItemActived()
        {
            return PDF_Page_getAnnotRichMediaItemActived(m_page, m_hand);
        }
        int RichMediaItemCount()
        {
            return PDF_Page_getAnnotRichMediaItemCount(m_page, m_hand);
        }
        RDUILib::PDFSign Sign();
        int SignStatus()
        {
            return PDF_Page_getAnnotSignStatus(m_page, m_hand);
        }
        int StrokeColor() { return PDF_Page_getAnnotStrokeColor(m_page, m_hand); }
        void StrokeColor(int color) { PDF_Page_setAnnotStrokeColor(m_page, m_hand, color); }
        com_array<float> StrokeDash() {
            float stmp[128];
            int cnt = PDF_Page_getAnnotStrokeDash(m_page, m_hand, stmp, 128);
            if (cnt <= 0) return com_array<float>(0);
            com_array<float> ret(cnt);
            memcpy(ret.data(), stmp, cnt * sizeof(float));
            return ret;
        }
        void StrokeDash(array_view<float const> val)
        {
            PDF_Page_setAnnotStrokeDash(m_page, m_hand, val.data(), val.size());
        }
        float StrokeWidth() { return PDF_Page_getAnnotStrokeWidth(m_page, m_hand); }
        void StrokeWidth(float val) { PDF_Page_setAnnotStrokeWidth(m_page, m_hand, val); }
        winrt::hstring SubmitPara()
        {
            wchar_t uri[512];
            if (!PDF_Page_getAnnotSubmitParaW(m_page, m_hand, uri, 511)) return L"";
            else return uri;
        }
        winrt::hstring SubmitTarget()
        {
            return PDF_Page_getAnnotSubmitTarget(m_page, m_hand);
        }
        int Type() { return PDF_Page_getAnnotType(m_page, m_hand); }
        winrt::hstring URI()
        {
            return PDF_Page_getAnnotURI(m_page, m_hand);
        }
        PDF_PAGE m_page;
        PDF_ANNOT m_hand;
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct PDFAnnot : PDFAnnotT<PDFAnnot, implementation::PDFAnnot>
    {
    };
}
