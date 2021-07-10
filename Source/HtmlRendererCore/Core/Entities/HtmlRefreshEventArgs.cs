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

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Raised when html renderer requires refresh of the control hosting (invalidation and re-layout).<br/>
    /// It can happen if some async event has occurred that requires re-paint and re-layout of the html.<br/>
    /// Example: async download of image is complete.
    /// </summary>
    public sealed class HtmlRefreshEventArgs : EventArgs
    {
        /// <summary>
        /// is re-layout is required for the refresh
        /// </summary>
        private readonly bool _layout;

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="layout">is re-layout is required for the refresh</param>
        public HtmlRefreshEventArgs(bool layout)
        {
            _layout = layout;
        }

        /// <summary>
        /// is re-layout is required for the refresh
        /// </summary>
        public bool Layout
        {
            get { return _layout; }
        }

        public override string ToString()
        {
            return string.Format("Layout: {0}", _layout);
        }
    }
}