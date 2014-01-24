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

using System;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Interfaces;

namespace HtmlRenderer.Core
{
    /// <summary>
    /// General utilities.
    /// </summary>
    public static class HtmlRendererUtils
    {
        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        /// <param name="colorName">the color name</param>
        /// <returns>color value</returns>
        public delegate ColorInt ResolveColorFromNameDelegate(String colorName);

        /// <summary>
        /// The manifest resource name for embedded image used for image loading.
        /// </summary>
        public static string ManifestResourceNameForImageLoad
        {
            get { return "HtmlRenderer.Core.Utils.ImageLoad.png"; }
        }

        /// <summary>
        /// The manifest resource name for embedded image used for image loading failed.
        /// </summary>
        public static string ManifestResourceNameForImageError
        {
            get { return "HtmlRenderer.Core.Utils.ImageError.png"; }
        }

        /// <summary>
        /// Resolve color value from given color name.
        /// </summary>
        public static ResolveColorFromNameDelegate ResolveColorFromName { get; set; }

        /// <summary>
        /// Measure the size of the html by performing layout under the given restrictions.
        /// </summary>
        /// <param name="g">the graphics to use</param>
        /// <param name="htmlContainer">the html to calculate the layout for</param>
        /// <param name="minSize">the minimal size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">the maximum size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <returns>return: the size of the html to be rendered within the min/max limits</returns>
        public static SizeInt MeasureHtmlByRestrictions(IGraphics g, HtmlContainerInt htmlContainer, SizeInt minSize, SizeInt maxSize)
        {
            // first layout without size restriction to know html actual size
            htmlContainer.PerformLayout(g);

            if (maxSize.Width > 0 && maxSize.Width < htmlContainer.ActualSize.Width)
            {
                // to allow the actual size be smaller than max we need to set max size only if it is really larger
                htmlContainer.MaxSize = new SizeInt(maxSize.Width, 0);
                htmlContainer.PerformLayout(g);
            }

            // restrict the final size by min/max
            var finalWidth = Math.Max(maxSize.Width > 0 ? Math.Min(maxSize.Width, (int)htmlContainer.ActualSize.Width) : (int)htmlContainer.ActualSize.Width, minSize.Width);

            // if the final width is larger than the actual we need to re-layout so the html can take the full given width.
            if (finalWidth > htmlContainer.ActualSize.Width)
            {
                htmlContainer.MaxSize = new SizeInt(finalWidth, 0);
                htmlContainer.PerformLayout(g);
            }

            var finalHeight = Math.Max(maxSize.Height > 0 ? Math.Min(maxSize.Height, (int)htmlContainer.ActualSize.Height) : (int)htmlContainer.ActualSize.Height, minSize.Height);

            return new SizeInt(finalWidth, finalHeight);
        }
    }
}
