using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Ephemera.NBagOfTricks;


namespace Ephemera.IconicSelector
{
    /// <summary></summary>
    internal class ImageCache : IDisposable
    {
        /// <summary></summary>
        readonly Dictionary<string, Image> _images = [];

        /// <summary></summary>
        readonly Image _defaultImage;

        /// <summary></summary>
        readonly int _imageSize = 8;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageSize"></param>
        ImageCache(int imageSize)
        {
            _imageSize = imageSize;

            // Make a default image. Big X.
            Bitmap bmp = new(imageSize, imageSize);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                Pen pen = new(Color.Purple, 4);
                int pad = 2;
                int sz = imageSize - 2 * pad;
                gr.DrawLine(pen, pad, pad, sz, sz);
                gr.DrawLine(pen, pad, sz, sz, pad);
            }
            _defaultImage = bmp;
        }

        /// <summary>
        /// Add a named bitmap to images if not added already.
        /// </summary>
        /// <param name="imgName">Reference name - usually file extension</param>
        /// <param name="bmp"></param>
        public void AddImage(string imgName, Bitmap bmp)
        {
            if (_images.ContainsKey(imgName))
            {
                throw new InvalidOperationException($"Cached already contains [{imgName}]");
            }
            var img = MiscUtils.ResizeBitmap(bmp, _imageSize, _imageSize);
            _images.Add(imgName, img);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public Image GetImage(string imgName)
        {
            return _images.TryGetValue(imgName, out Image? value) ? value : _defaultImage;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _images.ForEach(img => img.Value.Dispose());
            _images.Clear();
        }
    }
}
