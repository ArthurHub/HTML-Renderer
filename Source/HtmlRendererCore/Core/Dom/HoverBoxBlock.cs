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

using TheArtOfDev.HtmlRenderer.Core.Entities;

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
        /// the :hover style block data
        /// </summary>
        private readonly CssBlock _cssBlock;

        /// <summary>
        /// Init.
        /// </summary>
        public HoverBoxBlock(CssBox cssBox, CssBlock cssBlock)
        {
            _cssBox = cssBox;
            _cssBlock = cssBlock;
        }

        /// <summary>
        /// the box that has :hover css on
        /// </summary>
        public CssBox CssBox
        {
            get { return _cssBox; }
        }

        /// <summary>
        /// the :hover style block data
        /// </summary>
        public CssBlock CssBlock
        {
            get { return _cssBlock; }
        }
    }
}