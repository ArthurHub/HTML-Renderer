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
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Parse;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    /// <summary>
    /// Utility method for handling CSS stuff.
    /// </summary>
    internal static class CssUtils
    {
        #region Fields and Consts

        /// <summary>
        /// Brush for selection background
        /// </summary>
        private static readonly RColor _defaultSelectionBackcolor = RColor.FromArgb(0xa9, 0x33, 0x99, 0xFF);

        #endregion


        /// <summary>
        /// Brush for selection background
        /// </summary>
        public static RColor DefaultSelectionBackcolor
        {
            get { return _defaultSelectionBackcolor; }
        }

        /// <summary>
        /// Gets the white space width of the specified box
        /// </summary>
        /// <param name="g"></param>
        /// <param name="box"></param>
        /// <returns></returns>
        public static double WhiteSpace(RGraphics g, CssBoxProperties box)
        {
            double w = box.ActualFont.GetWhitespaceWidth(g);
            if (!(String.IsNullOrEmpty(box.WordSpacing) || box.WordSpacing == CssConstants.Normal))
            {
                w += CssValueParser.ParseLength(box.WordSpacing, 0, box, true);
            }
            return w;
        }

        /// <summary>
        /// Get CSS box property value by the CSS name.<br/>
        /// Used as a mapping between CSS property and the class property.
        /// </summary>
        /// <param name="cssBox">the CSS box to get it's property value</param>
        /// <param name="propName">the name of the CSS property</param>
        /// <returns>the value of the property, null if no such property exists</returns>
        public static string GetPropertyValue(CssBox cssBox, string propName)
        {
            switch (propName)
            {
                case "border-bottom-width":
                    return cssBox.BorderBottomWidth;
                case "border-left-width":
                    return cssBox.BorderLeftWidth;
                case "border-right-width":
                    return cssBox.BorderRightWidth;
                case "border-top-width":
                    return cssBox.BorderTopWidth;
                case "border-bottom-style":
                    return cssBox.BorderBottomStyle;
                case "border-left-style":
                    return cssBox.BorderLeftStyle;
                case "border-right-style":
                    return cssBox.BorderRightStyle;
                case "border-top-style":
                    return cssBox.BorderTopStyle;
                case "border-bottom-color":
                    return cssBox.BorderBottomColor;
                case "border-left-color":
                    return cssBox.BorderLeftColor;
                case "border-right-color":
                    return cssBox.BorderRightColor;
                case "border-top-color":
                    return cssBox.BorderTopColor;
                case "border-spacing":
                    return cssBox.BorderSpacing;
                case "border-collapse":
                    return cssBox.BorderCollapse;
                case "corner-radius":
                    return cssBox.CornerRadius;
                case "corner-nw-radius":
                    return cssBox.CornerNwRadius;
                case "corner-ne-radius":
                    return cssBox.CornerNeRadius;
                case "corner-se-radius":
                    return cssBox.CornerSeRadius;
                case "corner-sw-radius":
                    return cssBox.CornerSwRadius;
                case "margin-bottom":
                    return cssBox.MarginBottom;
                case "margin-left":
                    return cssBox.MarginLeft;
                case "margin-right":
                    return cssBox.MarginRight;
                case "margin-top":
                    return cssBox.MarginTop;
                case "padding-bottom":
                    return cssBox.PaddingBottom;
                case "padding-left":
                    return cssBox.PaddingLeft;
                case "padding-right":
                    return cssBox.PaddingRight;
                case "padding-top":
                    return cssBox.PaddingTop;
                case "page-break-inside":
                    return cssBox.PageBreakInside;
                case "left":
                    return cssBox.Left;
                case "top":
                    return cssBox.Top;
                case "width":
                    return cssBox.Width;
                case "max-width":
                    return cssBox.MaxWidth;
                case "height":
                    return cssBox.Height;
                case "background-color":
                    return cssBox.BackgroundColor;
                case "background-image":
                    return cssBox.BackgroundImage;
                case "background-position":
                    return cssBox.BackgroundPosition;
                case "background-repeat":
                    return cssBox.BackgroundRepeat;
                case "background-gradient":
                    return cssBox.BackgroundGradient;
                case "background-gradient-angle":
                    return cssBox.BackgroundGradientAngle;
                case "content":
                    return cssBox.Content;
                case "color":
                    return cssBox.Color;
                case "display":
                    return cssBox.Display;
                case "direction":
                    return cssBox.Direction;
                case "empty-cells":
                    return cssBox.EmptyCells;
                case "float":
                    return cssBox.Float;
                case "position":
                    return cssBox.Position;
                case "line-height":
                    return cssBox.LineHeight;
                case "vertical-align":
                    return cssBox.VerticalAlign;
                case "text-indent":
                    return cssBox.TextIndent;
                case "text-align":
                    return cssBox.TextAlign;
                case "text-decoration":
                    return cssBox.TextDecoration;
                case "white-space":
                    return cssBox.WhiteSpace;
                case "word-break":
                    return cssBox.WordBreak;
                case "visibility":
                    return cssBox.Visibility;
                case "word-spacing":
                    return cssBox.WordSpacing;
                case "font-family":
                    return cssBox.FontFamily;
                case "font-size":
                    return cssBox.FontSize;
                case "font-style":
                    return cssBox.FontStyle;
                case "font-variant":
                    return cssBox.FontVariant;
                case "font-weight":
                    return cssBox.FontWeight;
                case "list-style":
                    return cssBox.ListStyle;
                case "list-style-position":
                    return cssBox.ListStylePosition;
                case "list-style-image":
                    return cssBox.ListStyleImage;
                case "list-style-type":
                    return cssBox.ListStyleType;
                case "overflow":
                    return cssBox.Overflow;
            }
            return null;
        }

        /// <summary>
        /// Set CSS box property value by the CSS name.<br/>
        /// Used as a mapping between CSS property and the class property.
        /// </summary>
        /// <param name="cssBox">the CSS box to set it's property value</param>
        /// <param name="propName">the name of the CSS property</param>
        /// <param name="value">the value to set</param>
        public static void SetPropertyValue(CssBox cssBox, string propName, string value)
        {
            switch (propName)
            {
                case "border-bottom-width":
                    cssBox.BorderBottomWidth = value;
                    break;
                case "border-left-width":
                    cssBox.BorderLeftWidth = value;
                    break;
                case "border-right-width":
                    cssBox.BorderRightWidth = value;
                    break;
                case "border-top-width":
                    cssBox.BorderTopWidth = value;
                    break;
                case "border-bottom-style":
                    cssBox.BorderBottomStyle = value;
                    break;
                case "border-left-style":
                    cssBox.BorderLeftStyle = value;
                    break;
                case "border-right-style":
                    cssBox.BorderRightStyle = value;
                    break;
                case "border-top-style":
                    cssBox.BorderTopStyle = value;
                    break;
                case "border-bottom-color":
                    cssBox.BorderBottomColor = value;
                    break;
                case "border-left-color":
                    cssBox.BorderLeftColor = value;
                    break;
                case "border-right-color":
                    cssBox.BorderRightColor = value;
                    break;
                case "border-top-color":
                    cssBox.BorderTopColor = value;
                    break;
                case "border-spacing":
                    cssBox.BorderSpacing = value;
                    break;
                case "border-collapse":
                    cssBox.BorderCollapse = value;
                    break;
                case "corner-radius":
                    cssBox.CornerRadius = value;
                    break;
                case "corner-nw-radius":
                    cssBox.CornerNwRadius = value;
                    break;
                case "corner-ne-radius":
                    cssBox.CornerNeRadius = value;
                    break;
                case "corner-se-radius":
                    cssBox.CornerSeRadius = value;
                    break;
                case "corner-sw-radius":
                    cssBox.CornerSwRadius = value;
                    break;
                case "margin-bottom":
                    cssBox.MarginBottom = value;
                    break;
                case "margin-left":
                    cssBox.MarginLeft = value;
                    break;
                case "margin-right":
                    cssBox.MarginRight = value;
                    break;
                case "margin-top":
                    cssBox.MarginTop = value;
                    break;
                case "padding-bottom":
                    cssBox.PaddingBottom = value;
                    break;
                case "padding-left":
                    cssBox.PaddingLeft = value;
                    break;
                case "padding-right":
                    cssBox.PaddingRight = value;
                    break;
                case "padding-top":
                    cssBox.PaddingTop = value;
                    break;
                case "page-break-inside":
                    cssBox.PageBreakInside = value;
                    break;
                case "left":
                    cssBox.Left = value;
                    break;
                case "top":
                    cssBox.Top = value;
                    break;
                case "width":
                    cssBox.Width = value;
                    break;
                case "max-width":
                    cssBox.MaxWidth = value;
                    break;
                case "height":
                    cssBox.Height = value;
                    break;
                case "background-color":
                    cssBox.BackgroundColor = value;
                    break;
                case "background-image":
                    cssBox.BackgroundImage = value;
                    break;
                case "background-position":
                    cssBox.BackgroundPosition = value;
                    break;
                case "background-repeat":
                    cssBox.BackgroundRepeat = value;
                    break;
                case "background-gradient":
                    cssBox.BackgroundGradient = value;
                    break;
                case "background-gradient-angle":
                    cssBox.BackgroundGradientAngle = value;
                    break;
                case "color":
                    cssBox.Color = value;
                    break;
                case "content":
                    cssBox.Content = value;
                    break;
                case "display":
                    cssBox.Display = value;
                    break;
                case "direction":
                    cssBox.Direction = value;
                    break;
                case "empty-cells":
                    cssBox.EmptyCells = value;
                    break;
                case "float":
                    cssBox.Float = value;
                    break;
                case "position":
                    cssBox.Position = value;
                    break;
                case "line-height":
                    cssBox.LineHeight = value;
                    break;
                case "vertical-align":
                    cssBox.VerticalAlign = value;
                    break;
                case "text-indent":
                    cssBox.TextIndent = value;
                    break;
                case "text-align":
                    cssBox.TextAlign = value;
                    break;
                case "text-decoration":
                    cssBox.TextDecoration = value;
                    break;
                case "white-space":
                    cssBox.WhiteSpace = value;
                    break;
                case "word-break":
                    cssBox.WordBreak = value;
                    break;
                case "visibility":
                    cssBox.Visibility = value;
                    break;
                case "word-spacing":
                    cssBox.WordSpacing = value;
                    break;
                case "font-family":
                    cssBox.FontFamily = value;
                    break;
                case "font-size":
                    cssBox.FontSize = value;
                    break;
                case "font-style":
                    cssBox.FontStyle = value;
                    break;
                case "font-variant":
                    cssBox.FontVariant = value;
                    break;
                case "font-weight":
                    cssBox.FontWeight = value;
                    break;
                case "list-style":
                    cssBox.ListStyle = value;
                    break;
                case "list-style-position":
                    cssBox.ListStylePosition = value;
                    break;
                case "list-style-image":
                    cssBox.ListStyleImage = value;
                    break;
                case "list-style-type":
                    cssBox.ListStyleType = value;
                    break;
                case "overflow":
                    cssBox.Overflow = value;
                    break;
            }
        }
    }
}