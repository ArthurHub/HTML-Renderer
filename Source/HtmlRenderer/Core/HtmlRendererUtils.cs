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
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Core
{
    /// <summary>
    /// General utilities.
    /// </summary>
    public static class HtmlRendererUtils
    {
        /// <summary>
        /// Measure the size of the html by performing layout under the given restrictions.
        /// </summary>
        /// <param name="g">the graphics to use</param>
        /// <param name="htmlContainer">the html to calculate the layout for</param>
        /// <param name="minSize">the minimal size of the rendered html (zero - not limit the width/height)</param>
        /// <param name="maxSize">the maximum size of the rendered html, if not zero and html cannot be layout within the limit it will be clipped (zero - not limit the width/height)</param>
        /// <returns>return: the size of the html to be rendered within the min/max limits</returns>
        public static RSize MeasureHtmlByRestrictions(RGraphics g, HtmlContainerInt htmlContainer, RSize minSize, RSize maxSize)
        {
            // first layout without size restriction to know html actual size
            htmlContainer.PerformLayout(g);

            if (maxSize.Width > 0 && maxSize.Width < htmlContainer.ActualSize.Width)
            {
                // to allow the actual size be smaller than max we need to set max size only if it is really larger
                htmlContainer.MaxSize = new RSize(maxSize.Width, 0);
                htmlContainer.PerformLayout(g);
            }

            // restrict the final size by min/max
            var finalWidth = Math.Max(maxSize.Width > 0 ? Math.Min(maxSize.Width, (int)htmlContainer.ActualSize.Width) : (int)htmlContainer.ActualSize.Width, minSize.Width);

            // if the final width is larger than the actual we need to re-layout so the html can take the full given width.
            if (finalWidth > htmlContainer.ActualSize.Width)
            {
                htmlContainer.MaxSize = new RSize(finalWidth, 0);
                htmlContainer.PerformLayout(g);
            }

            var finalHeight = Math.Max(maxSize.Height > 0 ? Math.Min(maxSize.Height, (int)htmlContainer.ActualSize.Height) : (int)htmlContainer.ActualSize.Height, minSize.Height);

            return new RSize(finalWidth, finalHeight);
        }


        /// <summary>
        /// Perform the layout of the html container by given size restrictions returning the final size.<br/>
        /// The layout can be effected by the HTML content in the <paramref name="htmlContainer"/> if <paramref name="autoSize"/> or
        /// <paramref name="autoSizeHeightOnly"/> is set to true.<br/>
        /// Handle minimum and maximum size restrictions.<br/>
        /// Handle auto size and auto size for height only. if <paramref name="autoSize"/> is true <paramref name="autoSizeHeightOnly"/>
        /// is ignored.<br/>
        /// </summary>
        /// <param name="g">the graphics used for layout</param>
        /// <param name="htmlContainer">the html container to layout</param>
        /// <param name="size">the current size</param>
        /// <param name="minSize">the min size restriction - can be empty for no restriction</param>
        /// <param name="maxSize">the max size restriction - can be empty for no restriction</param>
        /// <param name="autoSize">if to modify the size (width and height) by html content layout</param>
        /// <param name="autoSizeHeightOnly">if to modify the height by html content layout</param>
        public static RSize Layout(RGraphics g, HtmlContainerInt htmlContainer, RSize size, RSize minSize, RSize maxSize, bool autoSize, bool autoSizeHeightOnly)
        {
            if (autoSize)
                htmlContainer.MaxSize = new RSize(0, 0);
            else if (autoSizeHeightOnly)
                htmlContainer.MaxSize = new RSize(size.Width, 0);
            else
                htmlContainer.MaxSize = size;

            htmlContainer.PerformLayout(g);

            RSize newSize = size;
            if (autoSize || autoSizeHeightOnly)
            {
                if (autoSize)
                {
                    if (maxSize.Width > 0 && maxSize.Width < htmlContainer.ActualSize.Width)
                    {
                        // to allow the actual size be smaller than max we need to set max size only if it is really larger
                        htmlContainer.MaxSize = maxSize;
                        htmlContainer.PerformLayout(g);
                    }
                    else if (minSize.Width > 0 && minSize.Width > htmlContainer.ActualSize.Width)
                    {
                        // if min size is larger than the actual we need to re-layout so all 100% layouts will be correct
                        htmlContainer.MaxSize = new RSize(minSize.Width, 0);
                        htmlContainer.PerformLayout(g);
                    }
                    newSize = htmlContainer.ActualSize;
                }
                else if (Math.Abs(size.Height - htmlContainer.ActualSize.Height) > 0.01)
                {
                    var prevWidth = size.Width;

                    // make sure the height is not lower than min if given
                    newSize.Height = minSize.Height > 0 && minSize.Height > htmlContainer.ActualSize.Height
                        ? minSize.Height
                        : htmlContainer.ActualSize.Height;

                    // handle if changing the height of the label affects the desired width and those require re-layout
                    if (Math.Abs(prevWidth - size.Width) > 0.01)
                        return Layout(g, htmlContainer, size, minSize, maxSize, false, true);
                }
            }

            return newSize;
        }
    }
}