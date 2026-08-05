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

using TheArtOfDev.HtmlRenderer.Core.CssEngine;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// CSS boxes that have ":hover" selector on them.
    /// </summary>
    internal sealed class HoverBoxBlock
    {
        /// <summary>
        /// the box that has :hover css on
        /// </summary>
        private readonly CssBox _cssBox;

        /// <summary>
        /// the :hover style rule
        /// </summary>
        private readonly IStyleRule _styleRule;

        /// <summary>
        /// Init.
        /// </summary>
        public HoverBoxBlock(CssBox cssBox, IStyleRule styleRule)
        {
            _cssBox = cssBox;
            _styleRule = styleRule;
        }

        /// <summary>
        /// the box that has :hover css on
        /// </summary>
        public CssBox CssBox
        {
            get { return _cssBox; }
        }

        /// <summary>
        /// the :hover style rule
        /// </summary>
        public IStyleRule StyleRule
        {
            get { return _styleRule; }
        }
    }
}