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
using System.Drawing;

namespace HtmlRenderer.Entities
{
    /// <summary>
    /// Raised when Html Renderer request scroll to specific location.<br/>
    /// This can occur on document anchor click.
    /// </summary>
    public sealed class HtmlScrollEventArgs : EventArgs
    {
        /// <summary>
        /// the location to scroll to
        /// </summary>
        private readonly Point _location;

        /// <summary>
        /// init.
        /// </summary>
        /// <param name="location">the location to scroll to</param>
        public HtmlScrollEventArgs(Point location)
        {
            _location = location;
        }

        /// <summary>
        /// the location to scroll to
        /// </summary>
        public Point Location
        {
            get { return _location; }
        }

        public override string ToString()
        {
            return string.Format("Location: {0}", _location);
        }
    }
}
