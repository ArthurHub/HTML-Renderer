using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Demo.Common;

namespace HtmlRenderer.Demo.Console
{
    public class SampleConverterBase
    {
        private string _sampleRunIdentifier;
        private string _thisTypeName;
        private string _basePath;

        public SampleConverterBase(string sampleRunIdentifier, string basePath) 
        {
            _sampleRunIdentifier = sampleRunIdentifier;
            _basePath = basePath;
            _thisTypeName = this.GetType().Name;

            this.OnImageLoaded += ImageLoad;
            this.OnStyleLoaded += StylesheetLoad;
        }

        public CssData CssData => null;

        protected string GetSamplePath(HtmlSample sample)
        {
            var path = Path.Combine(_basePath, _sampleRunIdentifier);
            Directory.CreateDirectory(path);
            return Path.Combine(path, sample.FullName + _thisTypeName + "_" + ".pdf");
        }

        protected EventHandler<HtmlImageLoadEventArgs> OnImageLoaded;
        protected EventHandler<HtmlStylesheetLoadEventArgs> OnStyleLoaded;

        internal void ImageLoad(object? sender, HtmlImageLoadEventArgs e)
        {
            //The samples use some well known image resources, so do that here.
            var imageStream = DemoUtils.GetImageStream(e.Src);
            if (imageStream != null)
            {
                e.Handled = true;
                e.Callback(imageStream);
            }
        }

        internal void StylesheetLoad(object? sender, HtmlStylesheetLoadEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
