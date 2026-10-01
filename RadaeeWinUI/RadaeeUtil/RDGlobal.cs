using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace RadaeeWinUI.RadaeeUtil
{
    public sealed class RDGlobal
    {
        static private string GetInstalledPath()
        {
            try
            {
                return AppContext.BaseDirectory.TrimEnd('\\');
                //return Package.Current.InstalledLocation.Path;
            }
            catch
            {
                // Unpackaged mode: Package.Current is not available
                return AppContext.BaseDirectory.TrimEnd('\\');
            }
        }

        static private bool ms_loaded = false;
        static private void load_data()
        {
            if (ms_loaded) return;
            ms_loaded = true;
            String inst_path = GetInstalledPath();
            String cmap_path = inst_path + "\\Assets\\dat\\cmaps.dat";
            String umap_path = inst_path + "\\Assets\\dat\\umaps.dat";
            String cmyk_path = inst_path + "\\Assets\\dat\\cmyk_rgb.dat";
            if (new FileInfo(cmap_path).Exists && new FileInfo(umap_path).Exists)
                RDUILib.RDGlobal.SetCMapsPath(cmap_path, umap_path);
            if (new FileInfo(cmyk_path).Exists)
                RDUILib.RDGlobal.SetCMYKICC(cmyk_path);

            RDUILib.RDGlobal.FontFileListStart();
            //the new UWP can access font directory in system path.
            String fpath = SystemDataPaths.GetDefault().Fonts;
            DirectoryInfo finfo = new DirectoryInfo(fpath);
            FileInfo[] files = finfo.GetFiles();
            foreach (FileInfo file in files)
            {
                String ext = file.Extension.ToLower();
                if (ext.CompareTo(".ttf") == 0 || ext.CompareTo(".ttc") == 0 ||
                    ext.CompareTo(".otf") == 0 || ext.CompareTo(".otc") == 0)
                    RDUILib.RDGlobal.FontFileListAdd(file.FullName);
            }
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\argbsn00lp.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\arimo.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\arimob.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\arimobi.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\arimoi.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\texgy.otf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\texgyb.otf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\texgybi.otf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\texgyi.otf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\cousine.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\cousineb.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\cousinei.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\cousinebi.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\symbol.ttf");
            RDUILib.RDGlobal.FontFileListAdd(inst_path + "\\Assets\\font\\amiriRegular.ttf");
            RDUILib.RDGlobal.FontFileListEnd();

            RDUILib.RDGlobal.FontFileMapping("Arial", "Arimo");
            RDUILib.RDGlobal.FontFileMapping("Arial Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Arial BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Arial Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Arial,Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Arial,BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Arial,Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Arial-Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Arial-BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Arial-Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("ArialMT", "Arimo");
            RDUILib.RDGlobal.FontFileMapping("Calibri", "Arimo");
            RDUILib.RDGlobal.FontFileMapping("Calibri Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Calibri BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Calibri Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Calibri,Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Calibri,BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Calibri,Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Calibri-Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Calibri-BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Calibri-Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Helvetica", "Arimo");
            RDUILib.RDGlobal.FontFileMapping("Helvetica Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Helvetica BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Helvetica Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Helvetica,Bold", "Arimo,Bold");
            RDUILib.RDGlobal.FontFileMapping("Helvetica,BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Helvetica,Italic", "Arimo Italic");
            RDUILib.RDGlobal.FontFileMapping("Helvetica-Bold", "Arimo Bold");
            RDUILib.RDGlobal.FontFileMapping("Helvetica-BoldItalic", "Arimo Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Helvetica-Italic", "Arimo Italic");

            RDUILib.RDGlobal.FontFileMapping("Garamond", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("Garamond,Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("Garamond,BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("Garamond,Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("Garamond-Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("Garamond-BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("Garamond-Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("Times", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("Times,Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("Times,BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("Times,Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("Times-Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("Times-BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("Times-Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("Times-Roman", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman,Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman,BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman,Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman-Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman-BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("Times New Roman-Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman,Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman,BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman,Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman-Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman-BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRoman-Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS,Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS,BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS,Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS-Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS-BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPS-Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT", "TeXGyreTermes-Regular");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT,Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT,BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT,Italic", "TeXGyreTermes-Italic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT-Bold", "TeXGyreTermes-Bold");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT-BoldItalic", "TeXGyreTermes-BoldItalic");
            RDUILib.RDGlobal.FontFileMapping("TimesNewRomanPSMT-Italic", "TeXGyreTermes-Italic");

            RDUILib.RDGlobal.FontFileMapping("Courier", "Cousine");
            RDUILib.RDGlobal.FontFileMapping("Courier Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("Courier BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier,Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("Courier,BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier,Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier-Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("Courier-BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier-Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier New", "Cousine");
            RDUILib.RDGlobal.FontFileMapping("Courier New Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("Courier New BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier New Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier New,Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("Courier New,BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier New,Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier New-Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("Courier New-BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("Courier New-Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("CourierNew", "Cousine");
            RDUILib.RDGlobal.FontFileMapping("CourierNew Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("CourierNew BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("CourierNew Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("CourierNew,Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("CourierNew,BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("CourierNew,Italic", "Cousine Italic");
            RDUILib.RDGlobal.FontFileMapping("CourierNew-Bold", "Cousine Bold");
            RDUILib.RDGlobal.FontFileMapping("CourierNew-BoldItalic", "Cousine Bold Italic");
            RDUILib.RDGlobal.FontFileMapping("CourierNew-Italic", "Cousine Italic");

            RDUILib.RDGlobal.FontFileMapping("Symbol", "Symbol Neu for Powerline");
            RDUILib.RDGlobal.FontFileMapping("Symbol,Bold", "Symbol Neu for Powerline");
            RDUILib.RDGlobal.FontFileMapping("Symbol,BoldItalic", "Symbol Neu for Powerline");
            RDUILib.RDGlobal.FontFileMapping("Symbol,Italic", "Symbol Neu for Powerline");

            int face_first = 0;
            int face_count = RDUILib.RDGlobal.GetFaceCount();
            String rand_fname = null;
            uint sys_fonts = 0;
            while (face_first < face_count)
            {
                String fname = RDUILib.RDGlobal.GetFaceName(face_first);
                if (fname != null)
                {
                    if (fname.CompareTo("SimSun") == 0) sys_fonts |= 1;
                    if (fname.CompareTo("Microsoft JHengHei") == 0 || fname.CompareTo("MingLiU") == 0) sys_fonts |= 2;
                    if (fname.CompareTo("MS Gothic") == 0) sys_fonts |= 4;
                    if (fname.CompareTo("Malgun Gothic Regular") == 0) sys_fonts |= 8;
                }
                if (rand_fname == null && fname != null && fname.Length > 0)
                    rand_fname = fname;
                face_first++;
            }
            // set default fonts.
            if (sys_fonts > 0)
            {
                RDUILib.RDGlobal.SetDefaultFont("", "Calibri", true);
                RDUILib.RDGlobal.SetDefaultFont("", "Times New Roman", false);
                RDUILib.RDGlobal.SetDefaultFont("GB1", "SimSun", true);
                RDUILib.RDGlobal.SetDefaultFont("GB1", "SimSun", false);
                if (!RDUILib.RDGlobal.SetDefaultFont("CNS1", "Microsoft JHengHei", true))
                    RDUILib.RDGlobal.SetDefaultFont("CNS1", "MingLiU", true);
                if (!RDUILib.RDGlobal.SetDefaultFont("CNS1", "Microsoft JHengHei", false))
                    RDUILib.RDGlobal.SetDefaultFont("CNS1", "MingLiU", false);
                RDUILib.RDGlobal.SetDefaultFont("Japan1", "MS Gothic", true);
                RDUILib.RDGlobal.SetDefaultFont("Japan1", "MS Gothic", false);
                RDUILib.RDGlobal.SetDefaultFont("Korea1", "Malgun Gothic Regular", true);
                RDUILib.RDGlobal.SetDefaultFont("Korea1", "Malgun Gothic Regular", false);
                RDUILib.RDGlobal.SetAnnotFont("SimSun");

                RDUILib.PDFEditNode.SetDefFont("Times New Roman");
                RDUILib.PDFEditNode.SetDefCJKFont("SimSun");
            }
            else
            {
                if (!RDUILib.RDGlobal.SetDefaultFont("", "AR PL SungtiL GB", true) && rand_fname != null)
                    RDUILib.RDGlobal.SetDefaultFont("", rand_fname, true);
                if (!RDUILib.RDGlobal.SetDefaultFont("", "AR PL SungtiL GB", false) && rand_fname != null)
                    RDUILib.RDGlobal.SetDefaultFont("", rand_fname, false);
                if (!RDUILib.RDGlobal.SetAnnotFont("AR PL SungtiL GB") && rand_fname != null)
                    RDUILib.RDGlobal.SetAnnotFont(rand_fname);
                RDUILib.PDFEditNode.SetDefFont("Arial");
                RDUILib.PDFEditNode.SetDefCJKFont("AR PL SungtiL GB");
            }

            // set annotation text font.
            RDUILib.RDGlobal.LoadStdFont(13, inst_path + "\\Assets\\font\\rdf013");
        }

        static public bool init()
        {
            load_data();
            String sver = RDUILib.RDGlobal.GetVersion();//this versioin string, example "20220225".
            //the key is binding to package "com.radaee.reader", can active version before "20260814"
            int ret = RDUILib.RDGlobal.Active("755836CA098838C0986F3123ECCAB01F9E84778014E59DA080D972017D2E78BF317E448DEBED21A16608F2884E925C46");
            return ret == 3;
        }

        static public bool DrawDash(float[] dash, int dashCount, WriteableBitmap dib) {
            return true;
            //return RDUILib.RDGlobal.DrawDash(dash, dashCount, dib);
        }
    }
}
