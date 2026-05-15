using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RDUILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace RadaeeWinUI
{
    public sealed class RDBmpDelegate : IRDDelegate
    {
        public WriteableBitmap OnCreateBitmap(int width, int height)
        {                
            return new WriteableBitmap(width, height);
        }

        public IBuffer OnGetBitmapBuffer(WriteableBitmap bmp)
        {
            return bmp.PixelBuffer;
        }

        public Image OnMakeImage(WriteableBitmap bmp)
        {
            Image image = new Image();
            image.Source = bmp;
            return image;
        }
    }
}