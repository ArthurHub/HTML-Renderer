using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Used to make space on vertical cell combination
    /// </summary>
    internal sealed class CssSpacingBox : CssBox
    {
        #region Fields and Consts

        private readonly CssBox _extendedBox;

        /// <summary>
        /// the index of the row where box starts
        /// </summary>
        private readonly int _startRow;

        /// <summary>
        /// the index of the row where box ends
        /// </summary>
        private readonly int _endRow;

        #endregion


        public CssSpacingBox(CssBox tableBox, ref CssBox extendedBox, int startRow)
            : base(tableBox, new HtmlTag("none", false, new Dictionary<string, string> { { "colspan", "1" } }))
        {
            _extendedBox = extendedBox;
            Display = CssConstants.None;

            _startRow = startRow;
            _endRow = startRow + Int32.Parse(extendedBox.GetAttribute("rowspan", "1")) - 1;
        }

        public CssBox ExtendedBox
        {
            get { return _extendedBox; }
        }

        /// <summary>
        /// Gets the index of the row where box starts
        /// </summary>
        public int StartRow
        {
            get { return _startRow; }
        }

        /// <summary>
        /// Gets the index of the row where box ends
        /// </summary>
        public int EndRow
        {
            get { return _endRow; }
        }
    }
}