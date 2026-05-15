#pragma once

#include "RDUILib.RDGlobal.g.h"
#include "UICom.h"

namespace winrt::RDUILib::implementation
{
    struct RDGlobal : RDGlobalT<RDGlobal>
    {
        static int32_t Active(winrt::hstring serial);
        static void SetCMapsPath(winrt::hstring cpath, winrt::hstring upath);
        static bool SetCMYKICC(winrt::hstring path);
        static winrt::hstring GetVersion();
        static void FontFileListStart();
        static void FontFileListAdd(winrt::hstring path);
        static void FontFileListEnd();
        static bool FontFileMapping(winrt::hstring map_name, winrt::hstring name);
        static int32_t GetFaceCount();
        static winrt::hstring GetFaceName(int32_t index);
        static bool SetDefaultFont(winrt::hstring collection, winrt::hstring name, bool fixed);
        static bool SetAnnotFont(winrt::hstring name);
        static void SetAnnotTransparence(uint32_t color);
        static void LoadStdFont(int32_t index, winrt::hstring path);
    };
}

namespace winrt::RDUILib::factory_implementation
{
    struct RDGlobal : RDGlobalT<RDGlobal, implementation::RDGlobal>
    {
    };
}
