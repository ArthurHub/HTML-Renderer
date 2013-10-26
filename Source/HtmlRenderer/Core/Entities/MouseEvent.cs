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

namespace HtmlRenderer.Entities
{
    /// <summary>
    /// aTODO: add doc
    /// </summary>
    public sealed class MouseEvent
    {
        /// <summary>
        /// Is the left mouse button participated in the event
        /// </summary>
        private readonly bool _leftButton;

        /// <summary>
        /// Init.
        /// </summary>
        public MouseEvent(bool leftButton)
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
