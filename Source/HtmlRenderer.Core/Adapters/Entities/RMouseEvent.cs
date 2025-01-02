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

using TheArtOfDev.HtmlRenderer.Core;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities
{
    /// <summary>
    /// Even class for handling keyboard events in <see cref="HtmlContainerInt"/>.
    /// </summary>
    public sealed class RMouseEvent
    {
        /// <summary>
        /// Is the left mouse button participated in the event
        /// </summary>
        private readonly bool _leftButton;

        /// <summary>
        /// Init.
        /// </summary>
        public RMouseEvent(bool leftButton)
        {
            _leftButton = leftButton;
        }

        /// <summary>
        /// Is the left mouse button participated in the event
        /// </summary>
        public bool LeftButton
        {
            get { return _leftButton; }
        }
    }
}