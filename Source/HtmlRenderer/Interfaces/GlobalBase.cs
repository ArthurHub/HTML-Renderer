// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"


using System.IO;
using HtmlRenderer.Core;

namespace HtmlRenderer.Interfaces
{
    /// <summary>
    /// Optional base class for <see cref="IGlobal"/> implementers to provide base helper functionality.
    /// </summary>
    public abstract class GlobalBase
    {
        #region Fields and Consts

        /// <summary>
        /// default CSS parsed data singleton
        /// </summary>
        private CssData _defaultCssData;

        /// <summary>
        /// image used to draw loading image icon
        /// </summary>
        private static IImage _loadImage;

        /// <summary>
        /// image used to draw error image icon
        /// </summary>
        private static IImage _errorImage;

        #endregion

        public CssData GetDefaultCssData()
        {
            return _defaultCssData ?? (_defaultCssData = CreateDefaultCssData());
        }

        /// <summary>
        /// Get image to be used while HTML image is loading.
        /// </summary>
        public IImage GetLoadImage()
        {
            if( _loadImage == null )
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageLoad.png");
                if( stream != null )
                    _loadImage = ImageFromStream(stream);
            }
            return _loadImage;
        }

        /// <summary>
        /// Get image to be used if HTML image load failed.
        /// </summary>
        public IImage GetErrorImage()
        {
            if (_errorImage == null)
            {
                var stream = typeof(HtmlRendererUtils).Assembly.GetManifestResourceStream("HtmlRenderer.Core.Utils.ImageError.png");
                if (stream != null)
                    _errorImage = ImageFromStream(stream);
            }
            return _errorImage;
        }

        /// <summary>
        /// Create an <see cref="IImage"/> object from the given stream.
        /// </summary>
        /// <param name="memoryStream">the stream to create image from</param>
        /// <returns>new image instance</returns>
        public abstract IImage ImageFromStream(Stream memoryStream);

        /// <summary>
        /// Create a default CSS data object that will be cached.
        /// </summary>
        /// <returns></returns>
        protected abstract CssData CreateDefaultCssData();
    }
}
