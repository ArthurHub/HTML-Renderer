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

using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Holds single class selector in css block hierarchical selection (p class1 > div.class2)
    /// </summary>
    public struct CssBlockSelectorItem
    {
        #region Fields and Consts

        /// <summary>
        /// the name of the css class of the block
        /// </summary>
        private readonly string _class;

        /// <summary>
        /// is the selector item has to be direct parent
        /// </summary>
        private readonly bool _directParent;

        #endregion


        /// <summary>
        /// Creates a new block from the block's source
        /// </summary>
        /// <param name="class">the name of the css class of the block</param>
        /// <param name="directParent"> </param>
        public CssBlockSelectorItem(string @class, bool directParent)
        {
            ArgChecker.AssertArgNotNullOrEmpty(@class, "@class");

            _class = @class;
            _directParent = directParent;
        }

        /// <summary>
        /// the name of the css class of the block
        /// </summary>
        public string Class
        {
            get { return _class; }
        }

        /// <summary>
        /// is the selector item has to be direct parent
        /// </summary>
        public bool DirectParent
        {
            get { return _directParent; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        public override string ToString()
        {
            return _class + (_directParent ? " > " : string.Empty);
        }
    }
}