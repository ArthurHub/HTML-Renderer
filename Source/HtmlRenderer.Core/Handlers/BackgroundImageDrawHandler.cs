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
using HtmlRenderer.Core.Dom;
using HtmlRenderer.Core.SysEntities;

namespace HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Contains all the paint code to paint different background images.
    /// </summary>
    internal static class BackgroundImageDrawHandler
    {
        /// <summary>
        /// Draw the background image of the given box in the given rectangle.<br/>
        /// Handle background-repeat and background-position values.
        /// </summary>
        /// <param name="g">the device to draw into</param>
        /// <param name="box">the box to draw its background image</param>
        /// <param name="imageLoadHandler">the handler that loads image to draw</param>
        /// <param name="rectangle">the rectangle to draw image in</param>
        public static void DrawBackgroundImage(IGraphics g, CssBox box, ImageLoadHandler imageLoadHandler, RectangleInt rectangle)
        {
            // image size depends if specific rectangle given in image loader
            var imgSize = new SizeInt(imageLoadHandler.Rectangle == RectangleInt.Empty ? imageLoadHandler.Image.Width : imageLoadHandler.Rectangle.Width,
                                   imageLoadHandler.Rectangle == RectangleInt.Empty ? imageLoadHandler.Image.Height : imageLoadHandler.Rectangle.Height);

            // get the location by BackgroundPosition value
            var location = GetLocation(box.BackgroundPosition, rectangle, imgSize);

            var srcRect = imageLoadHandler.Rectangle == RectangleInt.Empty
                              ? new RectangleInt(0, 0, imgSize.Width, imgSize.Height)
                              : new RectangleInt(imageLoadHandler.Rectangle.Left, imageLoadHandler.Rectangle.Top, imgSize.Width, imgSize.Height);

            // initial image destination rectangle
            var destRect = new RectangleInt(location, imgSize);

            // need to clip so repeated image will be cut on rectangle
            var prevClip = g.GetClip();
            var lRectangle = rectangle;
            lRectangle.Intersect(prevClip);
            g.SetClipReplace(lRectangle);

            switch( box.BackgroundRepeat )
            {
                case "no-repeat":
                    g.DrawImage(imageLoadHandler.Image, destRect, srcRect);
                    break;
                case "repeat-x":
                    DrawRepeatX(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                    break;
                case "repeat-y":
                    DrawRepeatY(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                    break;
                default:
                    DrawRepeat(g, imageLoadHandler, rectangle, srcRect, destRect, imgSize);
                    break;
            }

            g.SetClipReplace(prevClip);
        }


        #region Private methods

        /// <summary>
        /// Get top-left location to start drawing the image at depending on background-position value.
        /// </summary>
        /// <param name="backgroundPosition">the background-position value</param>
        /// <param name="rectangle">the rectangle to position image in</param>
        /// <param name="imgSize">the size of the image</param>
        /// <returns>the top-left location</returns>
        private static PointInt GetLocation(string backgroundPosition, RectangleInt rectangle, SizeInt imgSize)
        {
            float left = rectangle.Left;
            if( backgroundPosition.IndexOf("left", StringComparison.OrdinalIgnoreCase) > -1 )
            {
                left = (rectangle.Left + .5f);
            }
            else if (backgroundPosition.IndexOf("right", StringComparison.OrdinalIgnoreCase) > -1)
            {
                left = rectangle.Right - imgSize.Width;                
            }
            else if (backgroundPosition.IndexOf("0", StringComparison.OrdinalIgnoreCase) < 0)
            {
                left = (rectangle.Left + (rectangle.Width - imgSize.Width) / 2 +.5f);
            }

            float top = rectangle.Top;
            if (backgroundPosition.IndexOf("top", StringComparison.OrdinalIgnoreCase) > -1)
            {
                top = rectangle.Top;
            }
            else if (backgroundPosition.IndexOf("bottom", StringComparison.OrdinalIgnoreCase) > -1)
            {
                top = rectangle.Bottom - imgSize.Height;
            }
            else if (backgroundPosition.IndexOf("0", StringComparison.OrdinalIgnoreCase) < 0)
            {
                top = (rectangle.Top + (rectangle.Height - imgSize.Height) / 2 + .5f);
            }

            return new PointInt(left, top);
        }

        /// <summary>
        /// Draw the background image at the required location repeating it over the X axis.<br/>
        /// Adjust location to left if starting location doesn't include all the range (adjusted to center or right).
        /// </summary>
        private static void DrawRepeatX(IGraphics g, ImageLoadHandler imageLoadHandler, RectangleInt rectangle, RectangleInt srcRect, RectangleInt destRect, SizeInt imgSize)
        {
            while (destRect.X > rectangle.X)
                destRect.X -= imgSize.Width;

            using (var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location))
            {
                g.FillRectangle(brush, rectangle.X, destRect.Y, rectangle.Width, srcRect.Height);
            }
        }

        /// <summary>
        /// Draw the background image at the required location repeating it over the Y axis.<br/>
        /// Adjust location to top if starting location doesn't include all the range (adjusted to center or bottom).
        /// </summary>
        private static void DrawRepeatY(IGraphics g, ImageLoadHandler imageLoadHandler, RectangleInt rectangle, RectangleInt srcRect, RectangleInt destRect, SizeInt imgSize)
        {
            while (destRect.Y > rectangle.Y)
                destRect.Y -= imgSize.Height;

            using (var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location))
            {
                g.FillRectangle(brush, destRect.X, rectangle.Y, srcRect.Width, rectangle.Height);
            }
        }

        /// <summary>
        /// Draw the background image at the required location repeating it over the X and Y axis.<br/>
        /// Adjust location to left-top if starting location doesn't include all the range (adjusted to center or bottom/right).
        /// </summary>
        private static void DrawRepeat(IGraphics g, ImageLoadHandler imageLoadHandler, RectangleInt rectangle, RectangleInt srcRect, RectangleInt destRect, SizeInt imgSize)
        {
            while (destRect.X > rectangle.X)
                destRect.X -= imgSize.Width; 
            while (destRect.Y > rectangle.Y)
                destRect.Y -= imgSize.Height;

            using (var brush = g.GetTextureBrush(imageLoadHandler.Image, srcRect, destRect.Location))
            {
                g.FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }
        }

        #endregion
    }
}