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

using System.Collections.Generic;
using System.Drawing;
using HtmlRenderer.Core.Entities;
using HtmlRenderer.Core.Interfaces;
using HtmlRenderer.WinForms.Adapters;

namespace HtmlRenderer.WinForms.Utilities
{
    /// <summary>
    /// Provides some drawing functionality
    /// </summary>
    internal static class CacheUtils
    {
        #region Fields and Consts

        /// <summary>
        /// cache of brush color to brush instance
        /// </summary>
        private static readonly Dictionary<ColorInt, IBrush> _brushesCache = new Dictionary<ColorInt, IBrush>();

        /// <summary>
        /// cache of pen color to pen instance
        /// </summary>
        private static readonly Dictionary<ColorInt, IPen> _penCache = new Dictionary<ColorInt, IPen>();

        #endregion

        /// <summary>
        /// Get cached pen instance for the given color.
        /// </summary>
        /// <param name="color">the color to get pen for</param>
        /// <returns>pen instance</returns>
        public static IPen GetPen(ColorInt color)
        {
            IPen pen;
            if (!_penCache.TryGetValue(color, out pen))
            {
                var solidPen = new Pen(Utils.Convert(color));
                pen = new PenAdapter(solidPen);
                _penCache[color] = pen;
            }
            return pen;
        }
        
        /// <summary>
        /// Get cached solid brush instance for the given color.
        /// </summary>
        /// <param name="color">the color to get brush for</param>
        /// <returns>brush instance</returns>
        public static IBrush GetSolidBrush(ColorInt color)
        {
            IBrush brush;
            if( !_brushesCache.TryGetValue(color, out brush) )
            {
                Brush solidBrush;
                if( color == ColorInt.White )
                    solidBrush = Brushes.White;
                else if( color == ColorInt.Black )
                    solidBrush = Brushes.Black;
                else if (color == ColorInt.WhiteSmoke)
                    solidBrush = Brushes.WhiteSmoke;
                else if( color.A < 1 )
                    solidBrush = Brushes.Transparent;
                else
                    solidBrush = new SolidBrush(Utils.Convert(color));

                brush = new BrushAdapter(solidBrush, false);
                _brushesCache[color] = brush;
            }
            return brush;
        }
    }
}