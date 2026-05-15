#include "pch.h"
#include "RDGlobal.h"
#include "RDUILib.RDGlobal.g.cpp"

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winrt::RDUILib::implementation
{
    int32_t RDGlobal::Active(winrt::hstring serial)
    {
        return Global_active(serial);
    }
    winrt::hstring RDGlobal::GetVersion()
    {
        return Global_getVersion();
    }
    bool RDGlobal::SetCMYKICC(winrt::hstring path)
    {
        char cname[512];
        ::WideCharToMultiByte(CP_ACP, 0, path.c_str(), -1, cname, 511, NULL, NULL);
        return Global_setCMYKICC(cname);
    }
    void RDGlobal::SetCMapsPath(winrt::hstring cpath, winrt::hstring upath)
    {
        char cname[512];
        ::WideCharToMultiByte(CP_ACP, 0, cpath.c_str(), -1, cname, 511, NULL, NULL);
        char uname[512];
        ::WideCharToMultiByte(CP_ACP, 0, upath.c_str(), -1, uname, 511, NULL, NULL);
        Global_setCMapsPath(cname, uname);
    }
    void RDGlobal::FontFileListStart()
    {
        Global_fontfileListStart();
    }
    void RDGlobal::FontFileListAdd(winrt::hstring path)
    {
        char cname[512];
        ::WideCharToMultiByte(CP_ACP, 0, path.c_str(), -1, cname, 511, NULL, NULL);
        Global_fontfileListAdd(cname);
    }
    void RDGlobal::FontFileListEnd()
    {
        Global_fontfileListEnd();
    }
    bool RDGlobal::FontFileMapping(winrt::hstring map_name, winrt::hstring name)
    {
        char cname[256];
        ::WideCharToMultiByte(CP_ACP, 0, map_name.c_str(), -1, cname, 255, NULL, NULL);
        char uname[256];
        ::WideCharToMultiByte(CP_ACP, 0, name.c_str(), -1, uname, 255, NULL, NULL);
        return Global_fontfileMapping(cname, uname);
    }
    int32_t RDGlobal::GetFaceCount()
    {
        return Global_getFaceCount();
    }
    winrt::hstring RDGlobal::GetFaceName(int32_t index)
    {
        const char *sname = Global_getFaceName(index);
        if (sname)
        {
            wchar_t wstr[256];
            ::MultiByteToWideChar(CP_ACP, 0, sname, -1, wstr, 255);
            return wstr;
        }
        else return L"";
    }
    bool RDGlobal::SetDefaultFont(winrt::hstring collection, winrt::hstring name, bool fixed)
    {
        char cname[256];
        ::WideCharToMultiByte(CP_ACP, 0, collection.c_str(), -1, cname, 255, NULL, NULL);
        char uname[256];
        ::WideCharToMultiByte(CP_ACP, 0, name.c_str(), -1, uname, 255, NULL, NULL);
        return Global_setDefaultFont(cname, uname, fixed);
    }
    bool RDGlobal::SetAnnotFont(winrt::hstring name)
    {
        char fname[256];
        ::WideCharToMultiByte(CP_ACP, 0, name.c_str(), -1, fname, 255, NULL, NULL);
        return Global_setAnnotFont(fname);
    }
    void RDGlobal::SetAnnotTransparence(uint32_t color)
    {
        Global_setAnnotTransparency(color);
    }
    void RDGlobal::LoadStdFont(int32_t index, winrt::hstring path)
    {
        char cname[512];
        ::WideCharToMultiByte(CP_ACP, 0, path.c_str(), -1, cname, 511, NULL, NULL);
        Global_loadStdFont(index, cname);
    }
}
